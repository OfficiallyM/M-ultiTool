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
		private int fuelMixes = 1;
		private List<float> fuelValues = new List<float> { -1f };
		private List<int> fuelTypeInts = new List<int> { -1 };
		private Vector2 scrollPosition;
		private string plate = String.Empty;
		// TODO: Find a way of loading trailers dynamically.
		private string[] trailers = new string[] {
			"Bus02UtanfutoFull",
			"UtanFutoFull"
		};

		// Item menu variables.
		private Vector2 itemsScrollPosition;
		private List<GameObject> items = new List<GameObject>();

		// Settings.
		private bool deleteMode = false;
		private List<QuickSpawn> quickSpawns = new List<QuickSpawn>();
		private float selectedTime;
		private bool isTimeLocked;
		GameObject ufo;

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

			if (itemsMenu)
			{
				ItemsMenu();
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
			vehicleMenuY = mainMenuY;
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

			// Parse items.
			foreach (GameObject item in itemdatabase.d.items)
			{
				// Remove vehicles and trailers from items array.
				if (!IsVehicleOrTrailer(item) && item.name != "ErrorPrefab")
				{
					items.Add(item);
				}
			}

			// Prepopulate fuel types and fuel values as all default.
			int maxFuelType = (int)Enum.GetValues(typeof(mainscript.fluidenum)).Cast<mainscript.fluidenum>().Max();
			for (int i = 0; i < maxFuelType; i++)
			{
				fuelValues.Add(-1f);
				fuelTypeInts.Add(-1);
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
					if (IsVehicleOrTrailer(gameObject))
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
		/// Check if an object is a vehicle.
		/// </summary>
		/// <param name="gameObject">The object to check</param>
		/// <returns>true if the object is a vehicle or trailer; otherwise, false</returns>
		private bool IsVehicleOrTrailer(GameObject gameObject)
		{
			if ((gameObject.name.ToLower().Contains("full") && gameObject.GetComponentsInChildren<carscript>().Length > 0) || trailers.Contains(gameObject.name))
				return true;
			return false;
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
			// Fallback to English if the current language isn't supported.
			if (!translations.ContainsKey(language))
			{
				language = "English";
			}

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

			// Set vehicle license plate text.
			if (IsVehicleOrTrailer(gameObject) && plate != String.Empty)
			{
				rendszamscript[] plateScripts = gameObject.GetComponentsInChildren<rendszamscript>();
				foreach (rendszamscript plateScript in plateScripts)
				{
					if (plateScript == null)
						continue;

					plateScript.Same(plate);
				}
			}

			if (!IsVehicleOrTrailer(gameObject) && !fluidOverride)
			{
				Color objectColor = new Color(255f / 255f, 255f / 255f, 255f / 255f);
				mainscript.M.Spawn(gameObject, objectColor, selectedCondition, variant);
				return;
			}

			tankscript fuelTank = gameObject.GetComponent<tankscript>();

			// Find fuel tank objects.
			if (fuelTank == null)
			{
				fuelTank = gameObject.GetComponentInChildren<tankscript>();
			}

			if (fuelTank == null)
			{
				// Vehicle doesn't have a fuel tank, just spawn the vehicle and return.
				mainscript.M.Spawn(gameObject, color, selectedCondition, variant);
				return;
			}

			// Fuel type and value are default, just spawn the vehicle.
			if (fuelMixes == 1)
			{
				if (fuelTypeInts[0] == -1 && fuelValues[0] == -1f)
				{
					mainscript.M.Spawn(gameObject, color, selectedCondition, variant);
					return;
				}
			}
			
			// Store the current fuel types and amounts to return either to default.
			List<mainscript.fluidenum> currentFuelTypes = new List<mainscript.fluidenum>();
			List<float> currentFuelAmounts = new List<float>();
			foreach (mainscript.fluid fluid in fuelTank.F.fluids)
			{
				currentFuelTypes.Add(fluid.type);
				currentFuelAmounts.Add(fluid.amount);
			}

			fuelTank.F.fluids.Clear();

			for (int i = 0; i < fuelMixes; i++)
			{
				if (fuelTypeInts[i] == -1 && fuelValues[i] > -1)
				{
					fuelTank.F.ChangeOne(fuelValues[i], currentFuelTypes[i]);
				}
				else if (fuelTypeInts[i] > -1 && fuelValues[i] == -1)
				{
					fuelTank.F.ChangeOne(currentFuelAmounts[i], (mainscript.fluidenum)fuelTypeInts[i]);
				}
				else
				{
					fuelTank.F.ChangeOne(fuelValues[i], (mainscript.fluidenum)fuelTypeInts[i]);
				}
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

				if (vehicleMenu)
					itemsMenu = false;
			}

			// Developer settings menu.
			float developerMenuY = vehicleMenuY + 25f;
			if (GUI.Button(new Rect(x, developerMenuY, width, buttonHeight), developerMenu ? "<color=#0F0>Developer menu</color>" : "<color=#F00>Developer menu</color>"))
			{
				developerMenu = !developerMenu;

				if (developerMenu)
					itemsMenu = false;
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

			height += (fuelMixes * 40f);

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

			// Fuel mixes.
			int maxFuelType = (int)Enum.GetValues(typeof(mainscript.fluidenum)).Cast<mainscript.fluidenum>().Max();
			GUI.Label(new Rect(x + 10f, sliderY - 2.5f, textWidth, sliderHeight), "Number of fuels:", labelStyle);
			float rawFuelMixes = GUI.HorizontalSlider(new Rect(sliderX, sliderY, sliderWidth, sliderHeight), fuelMixes, 1, maxFuelType + 1);
			fuelMixes = Mathf.RoundToInt(rawFuelMixes);
			GUI.Label(new Rect(textX, sliderY - 2.5f, textWidth, sliderHeight), fuelMixes.ToString(), labelStyle);

			sliderY += 20f;

			for (int i = 0; i < fuelMixes; i++)
			{
				if (i > 0)
					sliderY += 20f;

				// Fuel type.
				GUI.Label(new Rect(x + 10f, sliderY - 2.5f, textWidth, sliderHeight), $"Fuel type {i + 1}:", labelStyle);
				float rawFuelType = GUI.HorizontalSlider(new Rect(sliderX, sliderY, sliderWidth, sliderHeight), fuelTypeInts[i], -1, maxFuelType);
				fuelTypeInts[i] = Mathf.RoundToInt(rawFuelType);

				string fuelType = ((mainscript.fluidenum)fuelTypeInts[i]).ToString();
				if (fuelTypeInts[i] == -1)
					fuelType = "Default";
				else
					fuelType = fuelType[0].ToString().ToUpper() + fuelType.Substring(1);

				GUI.Label(new Rect(textX, sliderY - 2.5f, textWidth, sliderHeight), fuelType, labelStyle);

				sliderY += 20f;

				// Fuel amount.
				GUI.Label(new Rect(x + 10f, sliderY - 2.5f, textWidth, sliderHeight), $"Fuel amount {i + 1}:", labelStyle);
				float rawFuelValue = GUI.HorizontalSlider(new Rect(sliderX, sliderY, sliderWidth, sliderHeight), fuelValues[i], -1f, 1000f);
				fuelValues[i] = Mathf.Round(rawFuelValue);

				bool fuelValueParse = float.TryParse(GUI.TextField(new Rect(textX, sliderY - 2.5f, textWidth, sliderHeight), fuelValues[i].ToString(), labelStyle), out float tempFuelValue);
				if (!fuelValueParse)
					Log($"{tempFuelValue} is not a number", LogLevel.Error);
				else
					fuelValues[i] = tempFuelValue;
			}

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

			// License plate.
			sliderY += 30f;
			GUI.Label(new Rect(x + 10f, sliderY - 2.5f, textWidth, sliderHeight), "Plate (blank for random):", labelStyle);
			plate = GUI.TextField(new Rect(sliderX, sliderY, sliderWidth, sliderHeight), plate, 7, labelStyle);
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

			y += (fuelMixes * 40f);

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
			if (GUI.Button(new Rect(textX, sliderY - 15f, textWidth, sliderHeight), "Set"))
			{
				mainscript.M.napszak.tekeres = selectedTime;
			}

			if (GUI.Button(new Rect(textX, sliderY + 5f, textWidth, sliderHeight), isTimeLocked ? "<color=#0F0>Unlock</color>" : "<color=#F00>Lock</color>"))
			{
				isTimeLocked = !isTimeLocked;

				mainscript.M.napszak.enabled = !isTimeLocked;
			}

			sliderY += 25f;

			GUI.Label(new Rect(x + 10f, sliderY - 2.5f, textWidth, sliderHeight), "UFO control (Doesn't save):", labelStyle);

			if (GUI.Button(new Rect(sliderX, sliderY - 2.5f, textWidth * 2f, sliderHeight), "Spawn UFO"))
			{
				temporaryTurnOffGeneration temp = mainscript.M.menu.Kaposztaleves.GetComponentInParent<temporaryTurnOffGeneration>();
				if (temp != null)
				{
					// Destory existing UFO.
					if (ufo != null)
						UnityEngine.Object.Destroy(ufo);

					ufo = UnityEngine.Object.Instantiate(temp.FEDOSPAWN.prefab, mainscript.M.player.lookPoint + Vector3.up * 0.75f, Quaternion.FromToRotation(Vector3.forward, -mainscript.M.player.transform.right));
					fedoscript ufoScript = ufo.GetComponent<fedoscript>();
					ufoScript.ai = false;
					ufoScript.followRoad = false;
				}
			}

			if (GUI.Button(new Rect(sliderX + textWidth * 2 + 5f, sliderY - 2.5f, textWidth * 2f, sliderHeight), "Remove UFO"))
			{
				if (ufo != null)
					UnityEngine.Object.Destroy(ufo);
			}
		}

		private void ItemsMenu()
		{
			float x = mainMenuX + mainMenuWidth + 15f;
			float y = mainMenuY;
			float width = Screen.width / 1.75f;
			float height = mainMenuHeight;

			GUI.Box(new Rect(x, y, width, height), "<color=#FFF><size=16><b>Items</b></size></color>");

			float itemWidth = 140f;
			float itemHeight = 30f;
			float initialRowX = x + 10f;
			float itemX = initialRowX;
			float itemY = 70f;

			int maxRowItems = Mathf.FloorToInt(width / (itemWidth + 10f));
			int drawnRows = 0;

			float scrollHeight = itemHeight * (10f * maxRowItems) + itemY;
			itemsScrollPosition = GUI.BeginScrollView(new Rect(x, y + 30f, width - 10f, height - 40f), itemsScrollPosition, new Rect(x, y + 30f, width - 10f, scrollHeight), new GUIStyle(), new GUIStyle());

			for (int i = 0; i < items.Count(); i++)
			{
				GameObject item = items[i];

				itemX += itemWidth + 10f;

				if (i % maxRowItems == 0)
				{
					drawnRows++;
					itemX = initialRowX;
					itemY += itemHeight + 10f;
				}

				if (GUI.Button(new Rect(itemX, itemY, itemWidth, itemHeight), item.name))
				{
					Spawn(item);
				}
			}

			GUI.EndScrollView();
		}
	}
}