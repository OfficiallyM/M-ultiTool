using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TLDLoader;
using UnityEngine;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

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

		private float vehicleMenuWidth;
		private float vehicleMenuHeight;
		private float vehicleMenuX;
		private float vehicleMenuY;

		private bool vehicleMenu = false;
		private bool developerMenu = false;
		private bool itemsMenu = false;

		// Styling.
		private GUIStyle labelStyle = new GUIStyle();
		private GUIStyle headerStyle = new GUIStyle();

		// Vehicle-related variables.
		private List<Vehicle> vehicles = new List<Vehicle>();
		private Color color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
		private int conditionInt = 0;
		private enum condition
		{
			Random = -1,
			Pristine,
			Dull,
			Rough,
			Crusty,
			Rusty
		}
		private float fuelValue = 5f;
		private int fuelTypeInt = -1;
		private Vector2 scrollPosition;

		// Settings.
		private bool deleteMode = false;
		private List<QuickSpawn> quickSpawns = new List<QuickSpawn>();
		private float selectedTime;

		// Translation-related variables.
		private string language;
		private Dictionary<string, List<ConfigVehicle>> translations = new Dictionary<string, List<ConfigVehicle>>();

		// Vehicle class to track variants.
		private class Vehicle
		{
			public GameObject vehicle;
			public int variant;
		}

		// Objects available for quickspawn.
		private class QuickSpawn
		{
			public GameObject gameObject;
			public string name;
			public bool fluidOverride = false;
		}

		// Serializable vehicle wrapper for translation config.
		[DataContract]
		private class ConfigVehicle
		{
			[DataMember] public string objectName { get; set; }
			[DataMember] public int? variant { get; set; }
			[DataMember] public string name { get; set; }
		}

		[DataContract]
		private class ConfigWrapper
		{
			[DataMember] public List<ConfigVehicle> vehicles { get; set; }
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

			if (developerMenu)
			{
				DeveloperMenu();
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

			// Set label styling.
			labelStyle.alignment = TextAnchor.UpperLeft;
			labelStyle.normal.textColor = Color.white;

			// Set header styling.
			headerStyle.alignment = TextAnchor.MiddleCenter;
			headerStyle.fontSize = 16;
			headerStyle.normal.textColor = Color.white;

			// Set main menu position here so other menus can be based around it.
			mainMenuWidth = Screen.width / 7.5f;
			mainMenuHeight = Screen.height / 1.2f;
			mainMenuX = Screen.width / 2.5f - mainMenuWidth;
			mainMenuY = 75f;

			// Also store the vehicle menu so the developer menu can be
			// placed under it.
			vehicleMenuX = mainMenuX + mainMenuWidth + 15f;
			vehicleMenuY = 75f;
			vehicleMenuWidth = Screen.width / 3.5f;
			vehicleMenuHeight = Screen.height / 5f;

			LoadVehicles();
			LoadTranslationFiles();
			language = mainscript.M.menu.language.languageNames[mainscript.M.menu.language.selectedLanguage];

			// Add available quickspawn items.
			// TODO: Allow these to be user-selected?
			quickSpawns.Add(new QuickSpawn() { gameObject = itemdatabase.d.goilcan, name = "Oil can",	fluidOverride = true });
			quickSpawns.Add(new QuickSpawn() { gameObject = itemdatabase.d.ggascan, name = "Jerry can", fluidOverride = true });
			quickSpawns.Add(new QuickSpawn() { gameObject = itemdatabase.d.gbarrel, name = "Barrel",	fluidOverride = true });
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
			
			// TODO: Find a way of loading trailers dynamically.
			string[] trailers = new string[] {
				"Bus02UtanfutoFull",
				"UtanFutoFull"
			};
			foreach (GameObject gameObject in itemdatabase.d.items)
			{
				try
				{
					if ((gameObject.name.ToLower().Contains("full") && gameObject.GetComponentsInChildren<carscript>().Length > 0) || trailers.Contains(gameObject.name))
					{
						// Check for variants.
						tosaveitemscript save = gameObject.GetComponent<tosaveitemscript>();
						if (save != null && save.randoms.Length > 0)
						{
							int variants = save.randoms.Length;

							// TODO: Look into why IFA trailer (bed) is missing from randoms?
							// For now, manually include it.
							if (gameObject.name == "Bus02UtanfutoFull")
								variants += 1;

							for (int i = 0; i <= variants; i++)
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
		/// Load translation JSON files from mod config folder.
		/// </summary>
		private void LoadTranslationFiles()
		{
			// Return early if the config directory doesn't exist.
			if (!Directory.Exists(ModLoader.GetModConfigFolder(this))) {
				Log("Config folder is missing, nothing will be translated", LogLevel.Error);
				return;
			}

			string[] files = Directory.GetFiles(ModLoader.GetModConfigFolder(this), "*.json");
			foreach (string file in files)
			{
				if (!File.Exists(file))
				{
					continue;
				}

				try
				{
					string json = File.ReadAllText(file);
					MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
					DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(ConfigWrapper));
					var config = jsonSerializer.ReadObject(ms) as ConfigWrapper;
					ms.Close();

					translations.Add(Path.GetFileNameWithoutExtension(file), config.vehicles);
				}
				catch (Exception ex)
				{
					Log($"Failed loading translation file {Path.GetFileNameWithoutExtension(file)} - error:\n{ex}", LogLevel.Error);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="objectName">The object name to translate</param>
		/// <param name="variant">The vehicle variant (optional)</param>
		/// <returns>Translated object name or untranslated name if no translation is found</returns>
		private string T(string objectName, int? variant = null)
		{
			if (translations.ContainsKey(language))
			{
				List<ConfigVehicle> vehicles = translations[language];
				foreach (ConfigVehicle vehicle in vehicles)
				{
					if (vehicle.objectName == objectName)
					{
						if (variant != null && variant != -1)
						{
							if (vehicle.variant == variant)
								return vehicle.name;
						}
						else
							return vehicle.name;
					}
				}
			}

			if (variant != null && variant != -1)
			{
				objectName += $" (Variant {variant.GetValueOrDefault()})";
			}
			return objectName;
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
		/// <param name="fluidOverride">Allow the fluid to be overriden using the vehicle fuel selector</param>
		private void Spawn(GameObject gameObject, int variant = -1, bool fluidOverride = false)
		{
			int selectedCondition = conditionInt;
			if (selectedCondition == -1)
			{
				// Randomise vehicle condition.
				int maxCondition = (int)Enum.GetValues(typeof(condition)).Cast<condition>().Max();
				gameObject.GetComponent<partconditionscript>().StartFullRandom(0, maxCondition);
				selectedCondition = UnityEngine.Random.Range(0, maxCondition);
			}
			if (!IsVehicle(gameObject) && !fluidOverride)
			{
				Color objectColor = new Color(255f / 255f, 255f / 255f, 255f / 255f);
				mainscript.M.Spawn(gameObject, objectColor, selectedCondition, variant);
				return;
			}

			tankscript fuelTank = gameObject.GetComponent<tankscript>();
			if (fuelTank == null)
			{
				// Vehicle doesn't have a fuel tank, log a warning and return.
				mainscript.M.Spawn(gameObject, color, selectedCondition, variant);
				Log($"Vehicle {gameObject.name} has no fuel tank.", LogLevel.Warning);
				return;
			}


			// Fuel type and value are default, just spawn the vehicle.
			if (fuelTypeInt == -1 && fuelValue == -1f)
			{
				mainscript.M.Spawn(gameObject, color, selectedCondition, variant);
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
			mainscript.M.Spawn(gameObject, color, selectedCondition, variant);
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

			GUI.Box(new Rect(x, y, width, height), $"<color=#ac78ad><size=16><b>{Name}</b></size>\n<size=14>v{Version} - made with ❤️ by {Author}</size></color>");

			float buttonHeight = 20f;
			width -= 10f;
			x += 5f;

			// Delete mode.
			float deleteY = y + 50f;
			if (GUI.Button(new Rect(x, deleteY, width, buttonHeight), (deleteMode ? "<color=#0F0>Delete mode</color>" : "<color=#F00>Delete mode</color>") + " (Press del)"))
			{
				deleteMode = !deleteMode;
			}

			// Vehicle settings menu.
			float vehicleMenuY = deleteY + 25f;
			if (GUI.Button(new Rect(x, vehicleMenuY, width, buttonHeight), vehicleMenu ? "<color=#0F0>Vehicle menu</color>" : "<color=#F00>Vehicle menu</color>"))
			{
				vehicleMenu = !vehicleMenu;
			}

			// Developer settings menu.
			float developerMenuY = vehicleMenuY + 25f;
			if (GUI.Button(new Rect(x, developerMenuY, width, buttonHeight), developerMenu ? "<color=#0F0>Developer menu</color>" : "<color=#F00>Developer menu</color>"))
			{
				developerMenu = !developerMenu;
			}

			// Items menu.
			float itemsMenuY = developerMenuY + 25f;
			if (GUI.Button(new Rect(x, itemsMenuY, width, buttonHeight), itemsMenu ? "<color=#0F0>Items menu</color>" : "<color=#F00>Items menu</color>"))
			{
				itemsMenu = !itemsMenu;

				// Close all other menus when the items menu opens.
				if (itemsMenu)
				{
					vehicleMenu = false;
					developerMenu = false;
				}
			}

			// Quick spawns.
			float quickSpawnY = itemsMenuY + 40f;
			GUI.Label(new Rect(x, quickSpawnY, width, buttonHeight * 2), "<color=#FFF><size=14>Quick spawns</size>\n<size=12>Container fluids can be changed\n using vehicle menu</size></color>", headerStyle);
			quickSpawnY += 50f;
			foreach (QuickSpawn spawn in quickSpawns)
			{
				if (GUI.Button(new Rect(x, quickSpawnY, width, buttonHeight), spawn.name))
				{
					Spawn(spawn.gameObject, fluidOverride: spawn.fluidOverride);
				}
				quickSpawnY += 25f;
			}

			// Vehicle spawner.
			float scrollHeight = (buttonHeight + 5f) * vehicles.Count;
			float scrollY = y + height / 2;
			GUI.Label(new Rect(x, scrollY - 40f, width, buttonHeight * 2), "<color=#FFF><size=14>Vehicles</size>\n<size=12>Scroll for the full list</size></color>", headerStyle);
			scrollPosition = GUI.BeginScrollView(new Rect(x, scrollY, width, height / 2), scrollPosition, new Rect(x, scrollY, width, scrollHeight), GUIStyle.none, GUIStyle.none);
			foreach (Vehicle vehicle in vehicles)
			{
				GameObject gameObject = vehicle.vehicle;
				string name = T(gameObject.name, vehicle.variant);

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
			float x = vehicleMenuX;
			float y = vehicleMenuY;
			float width = vehicleMenuWidth;
			float height = vehicleMenuHeight;

			GUI.Box(new Rect(x, y, width, height), "<color=#FFF><size=16><b>Vehicle settings</b></size></color>");

			float sliderX = x + 175f;
			float sliderY = y + 30f;
			float sliderWidth = width / 1.75f;
			float sliderHeight = 20f;

			float textX = sliderX + sliderWidth + 10f;
			float textWidth = 50f;

			// Condition.
			GUI.Label(new Rect(x + 10f, sliderY - 2.5f, textWidth, sliderHeight), "Condition:", labelStyle);
			int maxCondition = (int)Enum.GetValues(typeof(condition)).Cast<condition>().Max();
			float rawCondition = GUI.HorizontalSlider(new Rect(sliderX, sliderY, sliderWidth, sliderHeight), conditionInt, -1, maxCondition);
			conditionInt = Mathf.RoundToInt(rawCondition);

			string conditionName = ((condition)conditionInt).ToString();

			GUI.Label(new Rect(textX, sliderY - 2.5f, textWidth, sliderHeight), conditionName, labelStyle);

			sliderY += 20f;

			// TODO: Support multiple fuel types and amount, allowing for spawning with mixed fuel tanks.

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

			// Vehicle colour sliders.
			// Red.
			sliderY += 20f;
			GUI.Label(new Rect(x + 10f, sliderY - 2.5f, textWidth, sliderHeight), "<color=#F00>Red:</color>", labelStyle);
			float red = GUI.HorizontalSlider(new Rect(sliderX, sliderY, sliderWidth, sliderHeight), color.r * 255, 0, 255);
			red = Mathf.Round(red);
			bool redParse = float.TryParse(GUI.TextField(new Rect(textX, sliderY - 2.5f, textWidth, sliderHeight), red.ToString(), labelStyle), out red);
			if (!redParse)
				Log($"{redParse.ToString()} is not a number", LogLevel.Error);
			red = Mathf.Clamp(red, 0f, 255f);
			color.r = red / 255f;
			GUI.Label(new Rect(textX, sliderY - 2.5f, textWidth, sliderHeight), red.ToString(), labelStyle);

			// Green.
			sliderY += 20f;
			GUI.Label(new Rect(x + 10f, sliderY - 2.5f, textWidth, sliderHeight), "<color=#0F0>Green:</color>", labelStyle);
			float green = GUI.HorizontalSlider(new Rect(sliderX, sliderY, sliderWidth, sliderHeight), color.g * 255, 0, 255);
			green = Mathf.Round(green);
			bool greenParse = float.TryParse(GUI.TextField(new Rect(textX, sliderY - 2.5f, textWidth, sliderHeight), green.ToString(), labelStyle), out green);
			if (!greenParse)
				Log($"{greenParse.ToString()} is not a number", LogLevel.Error);
			green = Mathf.Clamp(green, 0f, 255f);
			color.g = green / 255f;
			GUI.Label(new Rect(textX, sliderY - 2.5f, textWidth, sliderHeight), green.ToString(), labelStyle);

			// Blue.
			sliderY += 20f;
			GUI.Label(new Rect(x + 10f, sliderY - 2.5f, textWidth, sliderHeight), "<color=#00F>Blue:</color>", labelStyle);
			float blue = GUI.HorizontalSlider(new Rect(sliderX, sliderY, sliderWidth, sliderHeight), color.b * 255, 0, 255);
			blue = Mathf.Round(blue);
			bool blueParse = float.TryParse(GUI.TextField(new Rect(textX, sliderY - 2.5f, textWidth, sliderHeight), blue.ToString(), labelStyle), out blue);
			if (!blueParse)
				Log($"{blueParse.ToString()} is not a number", LogLevel.Error);
			blue = Mathf.Clamp(blue, 0f, 255f);
			color.b = blue / 255f;
			GUI.Label(new Rect(textX, sliderY - 2.5f, textWidth, sliderHeight), blue.ToString(), labelStyle);

			sliderY += 20f;

			// Colour preview.
			GUIStyle defaultStyle = GUI.skin.button;
			GUIStyle previewStyle = new GUIStyle(defaultStyle);
			Texture2D previewTexture = new Texture2D(1, 1);
			Color[] pixels = new Color[] { color };
			previewTexture.SetPixels(pixels);
			previewTexture.Apply();
			previewStyle.normal.background = previewTexture;
			previewStyle.active.background = previewTexture;
			previewStyle.hover.background = previewTexture;
			previewStyle.margin = new RectOffset(0, 0, 0, 0);
			GUI.skin.button = previewStyle;
			GUI.Button(new Rect(x + 10f, sliderY, width - 20f, 20f), "");
			GUI.skin.button = defaultStyle;
		}

		/// <summary>
		/// Developer menu config GUI.
		/// </summary>
		private void DeveloperMenu()
		{
			float x = vehicleMenuX;
			float y = vehicleMenuY + vehicleMenuHeight + 25f;
			float width = vehicleMenuWidth;
			float height = vehicleMenuHeight;

			GUI.Box(new Rect(x, y, width, height), "<color=#FFF><size=16><b>Developer settings</b></size></color>");

			float sliderX = x + 175f;
			float sliderY = y + 30f;
			float sliderWidth = width / 1.75f;
			float sliderHeight = 20f;

			float textX = sliderX + sliderWidth + 10f;
			float textWidth = 50f;

			// Time setting.
			// TODO: Work out what the time actually is.
			GUI.Label(new Rect(x + 10f, sliderY - 2.5f, textWidth, sliderHeight), "Time:", labelStyle);
			float time = GUI.HorizontalSlider(new Rect(sliderX, sliderY, sliderWidth, sliderHeight), selectedTime, 0f, 360f);
			selectedTime = Mathf.Round(time);
			if (GUI.Button(new Rect(textX, sliderY - 2.5f, textWidth, sliderHeight), "Set"))
			{
				mainscript.M.napszak.tekeres = selectedTime;
			}
		}
	}
}