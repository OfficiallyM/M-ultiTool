using MultiTool.Core;
using MultiTool.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using TLDLoader;
using UnityEngine;
using Logger = MultiTool.Modules.Logger;
using Settings = MultiTool.Core.Settings;

namespace MultiTool
{
	public class MultiTool : Mod
	{
		// Mod meta stuff.
		public override string ID => Meta.ID;
		public override string Name => Meta.Name;
		public override string Author => Meta.Author;
		public override string Version => Meta.Version;
		public override bool LoadInMenu => true;

		// Initialise modules.
		private readonly GUIRenderer renderer;
		private readonly Config config;
		private readonly Translator translator;
		private readonly ThumbnailGenerator thumbnailGenerator;
		private readonly Keybinds binds;
		private readonly Utility utility;

		private Settings settings = new Settings();

		internal static Mod mod;

		public MultiTool()
		{
			// Initialise modules.
			try
			{
				Logger.Init();

				// We can't use GetModConfigFolder here as the mod isn't fully initialised yet.
				string configDirectory = Path.Combine(ModLoader.ModsFolder, "Config", "Mod Settings", ID);

				config = new Config();
				utility = new Utility();
				translator = new Translator(configDirectory);
				thumbnailGenerator = new ThumbnailGenerator(utility, configDirectory);
				binds = new Keybinds(config);
				renderer = new GUIRenderer(config, translator, thumbnailGenerator, binds, utility);
			}
			catch (Exception ex)
			{
				Logger.Log($"Module initialisation failed - {ex}", Logger.LogLevel.Critical);
			}

			mod = this;
		}

		// Override functions.
		public override void OnMenuLoad()
		{
			// Check for and delete old spawner.
			string file = Path.Combine(ModLoader.ModsFolder, "SpawnerTLD.dll");
			if (File.Exists(file))
			{
				try
				{
					File.Delete(file);
					Logger.Log("Detected and removed old SpawnerTLD.");
				}
				catch (Exception ex)
				{
					Logger.Log($"Failed to delete old SpawnerTLD, this will cause conflicts - {ex}", Logger.LogLevel.Critical);
				}
			}
		}

		public override void OnGUI()
		{
			renderer.OnGUI();
		}

		public override void OnLoad()
		{
			// Distance check.
			float minDistance = 1000f;
			float distance = mainscript.DistanceRead();
			if (distance >= minDistance)
				renderer.enabled = true;

			// Return early if M-ultiTool is disabled.
			if (!renderer.enabled)
			{
				Logger.Log("Distance requirement not met, M-ultiTool disabled.", Logger.LogLevel.Warning);
				return;
			}

			// Run spawner migration.
			utility.MigrateFromSpawner();

			// Set the configuration path.
			config.SetConfigPath(ModLoader.GetModConfigFolder(this) + "\\Config.json");

			translator.SetLanguage(mainscript.M.menu.language.languageNames[mainscript.M.menu.language.selectedLanguage]);

			// Load the GUI renderer.
			renderer.OnLoad();
		}

		public override void Update()
		{
			// Return early if M-ultiTool isn't enabled.
			if (!renderer.enabled)
				return;

			renderer.Update();

			// Delete mode.
			if (settings.deleteMode)
			{
				try
				{
					if (Input.GetKeyDown(binds.GetKeyByAction((int)Keybinds.Inputs.deleteMode).key) && mainscript.M.player.seat == null)
					{
						Physics.Raycast(mainscript.M.player.Cam.transform.position, mainscript.M.player.Cam.transform.forward, out var raycastHit, float.PositiveInfinity, mainscript.M.player.useLayer);

						// Require objects to have a tosaveitemscript in order to delete them.
						// This prevents players from deleting the world, buildings and other
						// stuff that would break the game.
						tosaveitemscript save = raycastHit.transform.gameObject.GetComponent<tosaveitemscript>();
						if (save != null)
						{
							save.removeFromMemory = true;

							foreach (tosaveitemscript component in raycastHit.transform.root.GetComponentsInChildren<tosaveitemscript>())
							{
								component.removeFromMemory = true;
							}
							UnityEngine.Object.Destroy(raycastHit.transform.root.gameObject);
						}
					}
				}
				catch (Exception ex)
				{
					Logger.Log($"Failed to delete entity - {ex}", Logger.LogLevel.Warning);
				}
			}

			switch (settings.mode)
			{
				case "colorPicker":
					if (Input.GetKeyDown(binds.GetKeyByAction((int)Keybinds.Inputs.action1).key) && !renderer.show)
					{
						Physics.Raycast(mainscript.M.player.Cam.transform.position, mainscript.M.player.Cam.transform.forward, out var raycastHit, float.PositiveInfinity, mainscript.M.player.useLayer);
						GameObject hitGameObject = raycastHit.transform.gameObject;
						partconditionscript part = hitGameObject.GetComponent<partconditionscript>();

						// Return early if hit GameObject has no partconditionscript.
						if (part == null)
							return;

						Color objectColor = new Color();
						foreach (Renderer renderer in part.renderers)
						{
							if (renderer.material == null)
								continue;

							objectColor = renderer.material.color;
						}

						renderer.SetColor(objectColor);
					}

					if (Input.GetKeyDown(binds.GetKeyByAction((int)Keybinds.Inputs.action2).key) && !renderer.show)
					{
						Physics.Raycast(mainscript.M.player.Cam.transform.position, mainscript.M.player.Cam.transform.forward, out var raycastHit, float.PositiveInfinity, mainscript.M.player.useLayer);
						GameObject hitGameObject = raycastHit.transform.gameObject;
						partconditionscript part = hitGameObject.transform.root.GetComponent<partconditionscript>();

						// Return early if hit GameObject has no partconditionscript.
						if (part == null)
							return;

						utility.Paint(renderer.GetColor(), part);
					}
					break;
				case "scale":
					if (!renderer.show)
					{
						Physics.Raycast(mainscript.M.player.Cam.transform.position, mainscript.M.player.Cam.transform.forward, out var raycastHit, float.PositiveInfinity, mainscript.M.player.useLayer);
						if (raycastHit.transform != null)
						{
							GameObject hitGameObject = raycastHit.transform.gameObject;

							// Return early if looking at terrain.
							if (hitGameObject.GetComponent<terrainscript>() != null)
								return;

							Vector3 scale = hitGameObject.transform.localScale;
							float scaleValue = 0.1f;
							if (Input.GetKey(binds.GetKeyByAction((int)Keybinds.Inputs.action1).key))
							{
								hitGameObject.transform.localScale = new Vector3(scale.x + scaleValue, scale.y + scaleValue, scale.z + scaleValue);
							}

							if (Input.GetKey(binds.GetKeyByAction((int)Keybinds.Inputs.action2).key))
							{
								hitGameObject.transform.localScale = new Vector3(scale.x - scaleValue, scale.y - scaleValue, scale.z - scaleValue);
							}
						}
					}
					break;
			}

			// Fake the player being on a ladder to remove the gravity during noclip.
			// This is needed because setting useGravity directly on the player RigidBody
			// gets enabled again immediately by the fpscontroller.
			if (settings.noclip)
				mainscript.M.player.ladderV = 1;
		}
	}
}