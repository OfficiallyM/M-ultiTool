using SpawnerTLD.Core;
using SpawnerTLD.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using TLDLoader;
using UnityEngine;
using Logger = SpawnerTLD.Modules.Logger;
using Settings = SpawnerTLD.Core.Settings;

namespace SpawnerTLD
{
	public class SpawnerTLD : Mod
	{
		// Mod meta stuff.
		public override string ID => Meta.ID;
		public override string Name => Meta.Name;
		public override string Author => Meta.Author;
		public override string Version => Meta.Version;

		// Initialise modules.
		private readonly Logger logger = new Logger();
		private readonly GUIRenderer renderer;
		private readonly Config config;
		private readonly Translator translator;
		private readonly ThumbnailGenerator thumbnailGenerator;
		private readonly Keybinds binds;
		private readonly Utility utility;

		private Settings settings = new Settings();

		public SpawnerTLD()
		{
			// Initialise modules.
			try
			{
				// We can't use GetModConfigFolder here as the mod isn't fully initialised yet.
				string configDirectory = Path.Combine(ModLoader.ModsFolder, "Config", "Mod Settings", ID);

				config = new Config(logger);
				utility = new Utility(logger);
				translator = new Translator(logger, configDirectory);
				thumbnailGenerator = new ThumbnailGenerator(logger, utility, configDirectory);
				binds = new Keybinds(logger, config);
				renderer = new GUIRenderer(logger, config, translator, thumbnailGenerator, binds, utility);
			}
			catch (Exception ex)
			{
				logger.Log($"Module initialisation failed - {ex}", Logger.LogLevel.Critical);
			}
		}

		// Override functions.
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

			// Return early if spawner is disabled.
			if (!renderer.enabled)
			{
				logger.Log("Distance requirement not met, spawner disabled.", Logger.LogLevel.Warning);
				return;
			}

			// Set the configuration path.
			config.SetConfigPath(ModLoader.GetModConfigFolder(this) + "\\Config.json");

			translator.SetLanguage(mainscript.M.menu.language.languageNames[mainscript.M.menu.language.selectedLanguage]);

			// Load the GUI renderer.
			renderer.OnLoad();
		}

		public override void Update()
		{
			// Return early if spawner isn't enabled.
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
					logger.Log($"Failed to delete entity - {ex}", Logger.LogLevel.Warning);
				}
			}

			// Fake the player being on a ladder to remove the gravity during noclip.
			// This is needed because setting useGravity directly on the player RigidBody
			// gets enabled again immediately by the fpscontroller.
			if (settings.noclip)
				mainscript.M.player.ladderV = 1;
		}
	}
}