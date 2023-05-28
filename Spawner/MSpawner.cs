using System;
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

		// Logging variables.
		private string logFile = "";
		private enum LogLevel
		{
			Debug,
			Info,
			Warning,
			Error,
			Critical
		}

		// Menu control.
		private bool show = false;
		private bool enabled = false;

		private float mainMenuWidth;
		private float mainMenuHeight;
		private float mainMenuX;
		private float mainMenuY;

		private bool vehicleMenu = false;

		// Styling.
		private GUIStyle labelStyle = new GUIStyle();

		// Vehicle-related variables.
		private List<Vehicle> vehicles = new List<Vehicle>();
		private Color color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
		private int condition = 0;
		private float fuelValue = 5f;
		private int fuelTypeInt = -1;
		private Vector2 scrollPosition;

		// Settings.
		private bool deleteMode = false;

		// Vehicle class to track variants.
		[System.Serializable]
		private class Vehicle
		{
			public GameObject vehicle;
			public int variant;
		}

		// Serializable vehicle wrapper for translation config.
		[System.Serializable]
		private class ConfigVehicle
		{
			public string objectName;
			public int variant;
			public string name;

			public static ConfigVehicle CreateFromJSON(string json)
			{
				return JsonUtility.FromJson<ConfigVehicle>(json);
			}
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

			if (vehicleMenu)
			{
				VehicleMenu();
			}
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

			labelStyle.alignment = TextAnchor.UpperLeft;
			labelStyle.normal.textColor = Color.white;

			mainMenuWidth = Screen.width / 8f;
			mainMenuHeight = Screen.height / 1.2f;
			mainMenuX = Screen.width / 2.5f - mainMenuWidth;
			mainMenuY = 75f;

		LoadVehicles();

			foreach (int i in Enum.GetValues(typeof(mainscript.fluidenum)))
			{
				Log($"{i}: {Enum.GetName(typeof(mainscript.fluidenum), i)}", LogLevel.Debug);
			}
		}

		public override void Update()
		{
			// Return early if spawner isn't enabled.
			if (!enabled)
				return;

			if (deleteMode)
			{
				if (Input.GetKeyDown(KeyCode.Delete) && mainscript.M.player.seat == null)
				{
					Physics.Raycast(mainscript.M.player.Cam.transform.position, mainscript.M.player.Cam.transform.forward, out var raycastHit, float.PositiveInfinity, mainscript.M.player.useLayer);
					raycastHit.transform.gameObject.GetComponent<tosaveitemscript>().removeFromMemory = true;	
					foreach (tosaveitemscript component in raycastHit.transform.root.GetComponentsInChildren<tosaveitemscript>())
					{
						component.removeFromMemory = true;
					}
					UnityEngine.Object.Destroy(raycastHit.transform.root.gameObject);
				}
			}
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
		/// Load all vehicles.
		/// </summary>
		private void LoadVehicles()
		{
			vehicles = new List<Vehicle>();
			foreach (GameObject gameObject in itemdatabase.d.items)
			{
				try
				{
					if (gameObject.name.ToLower().Contains("full") && gameObject.GetComponentsInChildren<carscript>().Length > 0)
					{
						// Check for variants.
						tosaveitemscript save = gameObject.GetComponent<tosaveitemscript>();
						if (save != null && save.randoms.Length > 0)
						{
							for (int i = 0; i <= save.randoms.Length; i++)
							{
								Vehicle vehicle = new Vehicle()
								{
									vehicle = gameObject,
									variant = i + 1,
								};
								vehicles.Add(vehicle);
							}
						}
						else
						{
							Vehicle vehicle = new Vehicle()
							{
								vehicle = gameObject,
								variant = -1,
							};
							vehicles.Add(vehicle);
						}
					}
				}
				catch
				{
					Log($"Something went wrong loading vehicle {gameObject.name}", LogLevel.Error);
				}
			}
		}

		/// <summary>
		/// Check if an object is a vehicle.
		/// </summary>
		/// <param name="gameObject">The object to check</param>
		/// <returns>true if the object is a vehicle; otherwise, false</returns>
		private bool IsVehicle(GameObject gameObject)
		{
			if (gameObject.GetComponentsInChildren<carscript>().Length > 0)
				return true;
			return false;
		}

		/// <summary>
		/// Wrapper around the default spawn function to handle vehicle fuel and variants etc.
		/// </summary>
		/// <param name="gameObject">The object to spawn</param>
		/// <param name="variant">The object variant to spawn</param>
		private void Spawn(GameObject gameObject, int variant = -1)
		{
			if (!IsVehicle(gameObject))
			{
				Color objectColor = new Color(255f / 255f, 255f / 255f, 255f / 255f);
				mainscript.M.Spawn(gameObject, objectColor, condition, variant);
				return;
			}

			tankscript fuelTank = gameObject.GetComponent<tankscript>();
			if (fuelTank == null)
			{
				// Vehicle doesn't have a fuel tank, log a warning and return.
				mainscript.M.Spawn(gameObject, color, condition, variant);
				Log($"Vehicle {gameObject.name} has no fuel tank.", LogLevel.Warning);
				return;
			}


			// Fuel type and value are default, just spawn the vehicle.
			if (fuelTypeInt == -1 && fuelValue == -1f)
			{
				mainscript.M.Spawn(gameObject, color, condition, variant);
				return;
			}
			
			// Store the current fuel type and amount to return either to default.
			// TODO: Store all default fuel types in case a vehicle spawns with a mix.
			mainscript.fluidenum currentFuelType = fuelTank.F.fluids.FirstOrDefault().type;
			float currentFuelAmount = fuelTank.F.fluids.FirstOrDefault().amount;

			gameObject.GetComponent<tankscript>().F.fluids.Clear();

			if (fuelTypeInt == -1 && fuelValue > -1)
			{
				gameObject.GetComponent<tankscript>().F.ChangeOne(fuelValue, currentFuelType);
			}
			else if (fuelTypeInt > -1 && fuelValue == -1)
			{
				gameObject.GetComponent<tankscript>().F.ChangeOne(currentFuelAmount, (mainscript.fluidenum)fuelTypeInt);
			}
			else
			{
				gameObject.GetComponent<tankscript>().F.ChangeOne(fuelValue, (mainscript.fluidenum)fuelTypeInt);
			}
			mainscript.M.Spawn(gameObject, color, condition, variant);
		}

		// Menus.

		/// <summary>
		/// Main menu GUI.
		/// </summary>
		private void MainMenu()
		{
			float x = mainMenuX;
			float y = mainMenuY;
			float width = mainMenuWidth;
			float height = mainMenuHeight;

			float buttonHeight = 20f;
			width -= 10f;
			x += 2.5f;

			GUI.Box(new Rect(x, y, width, height), $"<color=#ac78ad><size=16><b>{Name}</b></size>\n<size=14>v{Version} - made with ❤️ by {Author}</size></color>");

			float deleteY = y + 50f;
			if (GUI.Button(new Rect(x, deleteY, width, buttonHeight), (deleteMode ? "<color=#0F0>Delete mode</color>" : "<color=#F00>Delete mode</color>") + " (Press del)"))
			{
				deleteMode = !deleteMode;
			}

			float vehicleMenuY = deleteY + 25f;
			if (GUI.Button(new Rect(x, vehicleMenuY, width, buttonHeight), vehicleMenu ? "<color=#0F0>Vehicle menu</color>" : "<color=#F00>Vehicle menu</color>"))
			{
				vehicleMenu = !vehicleMenu;
			}

			float scrollHeight = (buttonHeight + 5f) * vehicles.Count;
			float scrollY = y + height / 2;
			scrollPosition = GUI.BeginScrollView(new Rect(x, scrollY, width, height / 2), scrollPosition, new Rect(x, scrollY, width, scrollHeight), GUIStyle.none, GUIStyle.none);
			foreach (Vehicle vehicle in vehicles)
			{
				GameObject gameObject = vehicle.vehicle;
				string name = gameObject.name;
				
				// Vehicle has variants, add to name.
				if (vehicle.variant != -1)
				{
					name += $" (Variant {vehicle.variant})";
				}

				if (GUI.Button(new Rect(x, scrollY, width, buttonHeight), name))
				{
					Spawn(gameObject, vehicle.variant);
				}

				scrollY += 25f;
			}
			GUI.EndScrollView();
		}

		/// <summary>
		/// Vehicle config menu GUI.
		/// </summary>
		private void VehicleMenu()
		{
			float x = mainMenuX + mainMenuWidth + 15f;
			float y = 75f;
			float width = Screen.width / 3.5f;
			float height = Screen.height / 5f;

			GUI.Box(new Rect(x, y, width, height), "<color=#FFF><size=16><b>Vehicle settings</b></size></color>");

			float sliderX = x + 175f;
			float sliderY = y + 30f;
			float sliderWidth = width / 1.75f;
			float sliderHeight = 20f;

			float textX = sliderX + sliderWidth + 10f;
			float textWidth = 50f;

			// Fuel type.
			GUI.Label(new Rect(x + 10f, sliderY - 2.5f, textWidth, sliderHeight), "Fuel type:", labelStyle);
			int maxFuelType = (int)Enum.GetValues(typeof(mainscript.fluidenum)).Cast<mainscript.fluidenum>().Max();
			float rawFuelType = GUI.HorizontalSlider(new Rect(sliderX, sliderY, sliderWidth, sliderHeight), fuelTypeInt, -1, maxFuelType);
			fuelTypeInt = Mathf.RoundToInt(rawFuelType);

			string fuelType = ((mainscript.fluidenum)fuelTypeInt).ToString();
			if (fuelTypeInt == -1)
				fuelType = "Default";
			else
				fuelType = fuelType[0].ToString().ToUpper() + fuelType.Substring(1);

			GUI.Label(new Rect(textX, sliderY - 2.5f, textWidth, sliderHeight), fuelType, labelStyle);

			sliderY += 20f;

			// Fuel amount.
			GUI.Label(new Rect(x + 10f, sliderY - 2.5f, textWidth, sliderHeight), "Fuel amount (-1 for default):", labelStyle);
			float rawFuelValue = GUI.HorizontalSlider(new Rect(sliderX, sliderY, sliderWidth, sliderHeight), fuelValue, -1f, 1000f);
			fuelValue = Mathf.Round(rawFuelValue);

			bool fuelValueParse = float.TryParse(GUI.TextField(new Rect(textX, sliderY - 2.5f, textWidth, sliderHeight), fuelValue.ToString(), labelStyle), out fuelValue);
			if (!fuelValueParse)
				Log($"{fuelValue.ToString()} is not a number", LogLevel.Error);
		}
	}
}