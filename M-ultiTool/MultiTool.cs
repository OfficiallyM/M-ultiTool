using MultiTool.Config;
using MultiTool.Database;
using MultiTool.Save;
using MultiTool.Services;
using MultiTool.Tools;
using MultiTool.UI;
using MultiTool.UI.Tabs.ComponentBrowser;
using MultiTool.Utilities;
using System;
using System.IO;
using System.Linq;
using TLDLoader;
using UnityEngine;

namespace MultiTool
{
	public class MultiTool : Mod
	{
		// Mod meta stuff.
		public override string ID => "M-ultiTool";
		public override string Name => "M-ultiTool";
		public override string Author => "M-";
		public override string Version => "5.0.0-DEV";
		public override bool LoadInMenu => true;

		internal static GUIRenderer Renderer;
		internal static ToolController Tools;

		// Named Context, not Services - "Services" already resolves to the MultiTool.Services
		// namespace from inside this class (see the Services.Logger.* calls below), so a field
		// with that name would shadow it and silently break every one of those call sites.
		internal static ServiceContext Context;

		// Shorthand access for widely used services.
		// TODO: Remove these once the stuff in this file is ported to tools.
		internal static Keybinds Binds => Context.Keybinds;
		internal static Configuration Configuration => Context.Configuration;

		internal static Mod ModInstance;
		internal static bool IsOnMainMenu = true;

		public MultiTool()
		{
			ModInstance = this;

			try
			{
				Services.Logger.Init();
				Translator.Init();
				ThumbnailGenerator.Init();

				Context = new ServiceContext(new Configuration(), new Keybinds(), new ModState());
				// Bootstrap the configuration with a manually constructed 
				// path because the mod hasn't fully initialised yet.
				Configuration.Bootstrap(Path.Combine(ModLoader.ModsFolder, "Config", "Mod Settings", ID, "Config.json"));
				Renderer = new GUIRenderer(Context);
				Tools = new ToolController(Context);
			}
			catch (Exception ex)
			{
				Services.Logger.Log($"Bootstrap failed - {ex}", Services.Logger.LogLevel.Critical);
			}
		}

		// Override functions.
		public override void OnMenuLoad()
		{
			Configuration.Update(c => { c.Version = Version; });
			IsOnMainMenu = true;

			// Register tools.
			Tools.Register(new NoclipTool());
			Tools.Register(new ScaleTool());
			Tools.Register(new ObjectRegeneratorTool());
			Tools.Register(new WeightChangerTool());

			Renderer.OnMenuLoad();
		}

		public override void OnGUI()
		{
			Renderer.OnGUI();
		}

		public override void OnLoad()
		{
			Translator.SetLanguage(mainscript.M.menu.language.languageNames[mainscript.M.menu.language.selectedLanguage]);
			IsOnMainMenu = false;

			GameObject controller = new GameObject("M-ultiTool");
			controller.AddComponent<DataFetcher>();

			// Load the GUI Renderer.
			Renderer.OnLoad();
		}

		public override void Update()
		{
			Renderer.Update();
			Tools.Update();

			// Delete mode.
			if (Context.State.DeleteMode)
			{
				try
				{
					if (Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.deleteMode).AssignedKey) && mainscript.M.player.seat == null)
					{
						Physics.Raycast(mainscript.M.player.Cam.transform.position, mainscript.M.player.Cam.transform.forward, out RaycastHit raycastHit, float.PositiveInfinity, mainscript.M.player.useLayer);

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
					Services.Logger.Log($"Failed to delete entity - {ex}", Services.Logger.LogLevel.Warning);
				}
			}

			switch (Context.State.Mode)
			{
				case "colorPicker":
					if (Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.action1).AssignedKey) && !Renderer.Show)
					{
						Physics.Raycast(mainscript.M.player.Cam.transform.position, mainscript.M.player.Cam.transform.forward, out RaycastHit raycastHit, float.PositiveInfinity, mainscript.M.player.useLayer);
						GameObject hitGameObject = raycastHit.transform.gameObject;
						partconditionscript part = hitGameObject.GetComponent<partconditionscript>();
						sprayscript spray = hitGameObject.GetComponent<sprayscript>();

						// Return early if hit GameObject has no partconditionscript or sprayscript.
						if (part == null && spray == null)
							return;

						Color objectColor = new Color();
						if (spray != null)
						{
							objectColor = spray.color.color;
						}
						else
						{
							foreach (Renderer Renderer in part.renderers)
							{
								if (Renderer.material == null)
									continue;

								objectColor = Renderer.material.color;
							}
						}

						objectColor.a = 1;
						Colour.SetColour(objectColor);
					}

					if (Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.action2).AssignedKey) && !Renderer.Show)
					{
						Physics.Raycast(mainscript.M.player.Cam.transform.position, mainscript.M.player.Cam.transform.forward, out RaycastHit raycastHit, float.PositiveInfinity, mainscript.M.player.useLayer);
						GameObject hitGameObject = raycastHit.transform.gameObject;
						partconditionscript part = hitGameObject.transform.root.GetComponent<partconditionscript>();
						sprayscript spray = hitGameObject.transform.root.GetComponent<sprayscript>();

						// Return early if hit GameObject has no partconditionscript or sprayscript.
						if (part == null && spray == null)
							return;

						if (spray != null)
						{
							spray.color.color = Colour.GetColour();
						}
						else
							GameUtilities.Paint(Colour.GetColour(), part);
					}
					break;
			}
		}

		public override void FixedUpdate()
		{
			Renderer.FixedUpdate();
			Tools.FixedUpdate();
		}
	}
}