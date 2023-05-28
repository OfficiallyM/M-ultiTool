using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TLDLoader;
using UnityEngine;

namespace MSpawner
{
	public class MSpawner : Mod
	{
		// Mod meta stuff.
		public override string ID => "MSpawner";
		public override string Name => "Spawner";
		public override string Author => "M-";
		public override string Version => "1.0.0";

		// Variables.
		private string logFile = "";
		private bool show = false;
		private bool enabled = false;
		private GUIStyle style = new GUIStyle();
		private GUIStyle smallStyle = new GUIStyle();
		private Dictionary<string, GameObject> items = new Dictionary<string, GameObject>();
		private Dictionary<string, GameObject> vehicles = new Dictionary<string, GameObject>();
		private Color color = new Color(255f, 255f, 255f);
		private int condition = 0;
		private enum LogLevel
		{
			Debug,
			Info,
			Warning,
			Error,
			Critical
		}

		// Override functions.
		public override void OnGUI()
		{
			// Return early if spawner is disabled.
			if (!enabled)
				return;

			// Return early if pause menu isn't active.
			if (!mainscript.M.menu.Menu.activeSelf)
				return;

			ToggleVisibility();

			// Return early if the UI isn't supposed to be visible.
			if (!show)
				return;

			// Main menu always shows.
			MainMenu();
		}

		public override void OnLoad()
		{
			// Distance check.
			float minDistance = 1000f;
			float distance = mainscript.DistanceRead();
			if (distance >= minDistance)
				enabled = true;

			// Return early if spawner is disabled.
			if (!enabled)
			{
				Log("Distance requirement not met, spawner disabled.", LogLevel.Warning);
				return;
			}

			style.fontSize = 14;
			style.font = Font.CreateDynamicFontFromOSFont("Consolas", 14);
			smallStyle.fontSize = 10;
			smallStyle.font = Font.CreateDynamicFontFromOSFont("Consolas", 10);

			// Load items and vehicles now to avoid lagging the menu on first open.
			LoadItems();
			LoadVehicles();
			Log("Post OnLoad", LogLevel.Debug);
		}

		public override void Update()
		{
			// Return early if spawner isn't enabled.
			if (!enabled)
				return;
		}

		// Mod-specific functions.
		public MSpawner()
		{
			if (Directory.Exists(ModLoader.ModsFolder))
			{
				Directory.CreateDirectory(Path.Combine(ModLoader.ModsFolder, "Logs"));
				logFile = ModLoader.ModsFolder + "\\Logs\\MSpawner.log";
				File.WriteAllText(logFile, $"MSpawner v{Version} initialised\r\n");
			}
		}

		// Logging functions.

		/// <summary>
		/// Log messages to a file.
		/// </summary>
		/// <param name="msg">The message to log</param>
		private void Log(string msg, LogLevel logLevel)
		{
			if (logFile != string.Empty)
				File.AppendAllText(logFile, $"[{logLevel}] {msg}\r\n");
		}

		/// <summary>
		/// Show menu toggle button.
		/// </summary>
		private void ToggleVisibility()
		{
			if (GUI.Button(new Rect(230f, 30f, 200f, 50f), show ? "<size=28><color=#0F0>Spawner</color></size>" : "<size=28><color=#F00>Spawner</color></size>"))
				show = !show;
		}

		/// <summary>
		/// Load all items.
		/// </summary>
		private void LoadItems()
		{
			items = new Dictionary<string, GameObject>();
			foreach (GameObject gameObject in itemdatabase.d.items)
			{
				items.Add(gameObject.name, gameObject);
			}
		}

		/// <summary>
		/// Load all vehicles.
		/// </summary>
		private void LoadVehicles()
		{
			vehicles = new Dictionary<string, GameObject>();
			foreach (GameObject gameObject in itemdatabase.d.items)
			{
				//foreach (Transform child in gameObject.transform)
				//{
				//	foreach (Component component in child.gameObject.GetComponents(typeof(Component)))
				//	{
				//		Debug($"Vehicle: {gameObject.name} - {child.name} - {component}");
				//	}
				//}

				if (gameObject.name.ToLower().Contains("full"))
				{
					//vehicles.Add(gameObject.name, gameObject);
					//Log("Vehicle: " + gameObject.name, LogLevel.Debug);
				}
			}
		}

		/// <summary>
		/// Wrapper around the default spawn function to handle fuel, condition, etc.
		/// </summary>
		/// <param name="vehicle">The vehicle to spawn</param>
		/// <param name="fuel">The amount of fuel to spawn with</param>
		/// <param name="fluid">The fluid the vehicle should spawn with in the fuel tank</param>
		private void Spawn(GameObject vehicle, float fuel, int fluid)
		{
			// I've got no idea what param 4 does.
			mainscript.M.Spawn(vehicle, color, condition, 1);
		}

		// Menus.
		private void MainMenu()
		{
			float width = Screen.width / 8f;
			float height = Screen.height / 1.2f;
			float x = Screen.width / 2.5f - width;
			float y = 75f;
			GUI.Box(new Rect(x, y, width, height), $"<color=#ac78ad><size=16><b>{Name}</b></size>\n<size=14>v{Version} - made with ❤️ by {Author}</size></color>");
			height = 20f;
			width -= 5f;
			x += 20f;
			y += 2.5f;
			//for (var i = 0; i <= vehicles.Count; i++)
			//{
			//	string vehicleName = vehicles.ElementAt(i).Key;
			//	Debug("Loop: " + vehicleName);
			//	GameObject vehicle = vehicles.ElementAt(i).Value;
			//	if (GUI.Button(new Rect(x, y, width, height), vehicleName))
			//	{
			//		// Spawn vehicle.
			//	}

			//	x += 5f;
			//}
		}
	}
}