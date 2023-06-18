using SpawnerTLD.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLDLoader;
using UnityEngine;
using Settings = SpawnerTLD.Core.Settings;

namespace SpawnerTLD.Modules
{
	internal class GUIRenderer
	{
		private Mod mod;
		private Settings settings = new Core.Settings();

		// Initialise required modules.
		private Translator translator;
		private Logger logger = new Logger();

		// Menu control.
		public bool enabled = false;
		private bool show = false;

		private float mainMenuWidth;
		private float mainMenuHeight;
		private float mainMenuX;
		private float mainMenuY;

		private float vehicleMenuWidth;
		private float vehicleMenuHeight;
		private float vehicleMenuX;
		private float vehicleMenuY;

		private bool vehicleMenu = false;
		private bool miscMenu = false;
		private bool itemsMenu = false;

		// Styling.
		private GUIStyle labelStyle = new GUIStyle();
		private GUIStyle headerStyle = new GUIStyle();

		// Vehicle-related variables.
		private List<Vehicle> vehicles = new List<Vehicle>();
		private Color color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
		private int conditionInt = 0;
		private int fuelMixes = 1;
		private List<float> fuelValues = new List<float> { -1f };
		private List<int> fuelTypeInts = new List<int> { -1 };
		private Vector2 scrollPosition;
		private string plate = String.Empty;

		// Item menu variables.
		private Vector2 itemsScrollPosition;
		private List<Item> items = new List<Item>();

		// Settings.
		private List<QuickSpawn> quickSpawns = new List<QuickSpawn>();
		private float selectedTime;
		private bool isTimeLocked;
		GameObject ufo;

		public GUIRenderer(Mod _mod)
		{
			mod = _mod;
		}

		public void OnGUI()
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

			if (miscMenu)
			{
				MiscMenu();
			}

			if (itemsMenu)
			{
				ItemsMenu();
			}
		}

		public void OnLoad()
		{
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

			// Also store the vehicle menu so the misc menu can be
			// placed under it.
			vehicleMenuX = mainMenuX + mainMenuWidth + 15f;
			vehicleMenuY = mainMenuY;
			vehicleMenuWidth = Screen.width / 3.5f;
			vehicleMenuHeight = Screen.height / 5f;

			vehicles = Utility.LoadVehicles();

			// Add available quickspawn items.
			// TODO: Allow these to be user-selected?
			quickSpawns.Add(new QuickSpawn() { gameObject = itemdatabase.d.goilcan, name = "Oil can", fluidOverride = true });
			quickSpawns.Add(new QuickSpawn() { gameObject = itemdatabase.d.ggascan, name = "Jerry can", fluidOverride = true });
			quickSpawns.Add(new QuickSpawn() { gameObject = itemdatabase.d.gbarrel, name = "Barrel", fluidOverride = true });

			translator = new Translator(mainscript.M.menu.language.languageNames[mainscript.M.menu.language.selectedLanguage], mod);

			ThumbnailGenerator generator = new ThumbnailGenerator(mod);
			generator.PrepareCache();
			foreach (GameObject item in itemdatabase.d.items)
			{
				// Remove vehicles and trailers from items array.
				if (!Utility.IsVehicleOrTrailer(item) && item.name != "ErrorPrefab")
				{
					items.Add(new Item() { item = item, thumbnail = generator.GetThumbnail(item) });
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

		/// <summary>
		/// Show menu toggle button.
		/// </summary>
		private void ToggleVisibility()
		{
			if (GUI.Button(new Rect(230f, 30f, 200f, 50f), show ? "<size=28><color=#0F0>Spawner</color></size>" : "<size=28><color=#F00>Spawner</color></size>"))
				show = !show;
		}

		/// <summary>
		/// Main menu GUI.
		/// </summary>
		private void MainMenu()
		{
			float x = mainMenuX;
			float y = mainMenuY;
			float width = mainMenuWidth;
			float height = mainMenuHeight;

			GUI.Box(new Rect(x, y, width, height), $"<color=#ac78ad><size=16><b>{Meta.Name}</b></size>\n<size=14>v{Meta.Version} - made with ❤️ by {Meta.Author}</size></color>");

			float buttonHeight = 20f;
			width -= 10f;
			x += 5f;

			// Delete mode.
			float deleteY = y + 50f;
			if (GUI.Button(new Rect(x, deleteY, width, buttonHeight), (settings.deleteMode ? "<color=#0F0>Delete mode</color>" : "<color=#F00>Delete mode</color>") + " (Press del)"))
			{
				settings.deleteMode = !settings.deleteMode;
			}

			// Vehicle settings menu.
			float vehicleMenuY = deleteY + 25f;
			if (GUI.Button(new Rect(x, vehicleMenuY, width, buttonHeight), vehicleMenu ? "<color=#0F0>Vehicle menu</color>" : "<color=#F00>Vehicle menu</color>"))
			{
				vehicleMenu = !vehicleMenu;

				if (vehicleMenu)
					itemsMenu = false;
			}

			// Misc settings menu.
			float miscMenuY = vehicleMenuY + 25f;
			if (GUI.Button(new Rect(x, miscMenuY, width, buttonHeight), miscMenu ? "<color=#0F0>Miscellaneous menu</color>" : "<color=#F00>Miscellaneous menu</color>"))
			{
				miscMenu = !miscMenu;

				if (miscMenu)
					itemsMenu = false;
			}

			// Items menu.
			float itemsMenuY = miscMenuY + 25f;
			if (GUI.Button(new Rect(x, itemsMenuY, width, buttonHeight), itemsMenu ? "<color=#0F0>Items menu</color>" : "<color=#F00>Items menu</color>"))
			{
				itemsMenu = !itemsMenu;

				// Close all other menus when the items menu opens.
				if (itemsMenu)
				{
					vehicleMenu = false;
					miscMenu = false;
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
					Utility.Spawn(new Item()
					{
						item = spawn.gameObject,
						fluidOverride = spawn.fluidOverride,
						conditionInt = conditionInt,
						fuelMixes = fuelMixes,
						fuelValues = fuelValues,
						fuelTypeInts = fuelTypeInts,
						color = color
					});
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
				string name = translator.T(gameObject.name, vehicle.variant);

				if (GUI.Button(new Rect(x, scrollY, width, buttonHeight), name))
				{
					Utility.Spawn(new Vehicle()
					{
						vehicle = vehicle.vehicle,
						variant = vehicle.variant,
						conditionInt = conditionInt,
						fuelMixes = fuelMixes,
						fuelValues = fuelValues,
						fuelTypeInts = fuelTypeInts,
						color = color,
						plate = plate
					});
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
			int maxCondition = (int)Enum.GetValues(typeof(Item.condition)).Cast<Item.condition>().Max();
			float rawCondition = GUI.HorizontalSlider(new Rect(sliderX, sliderY, sliderWidth, sliderHeight), conditionInt, -1, maxCondition);
			conditionInt = Mathf.RoundToInt(rawCondition);

			string conditionName = ((Item.condition)conditionInt).ToString();

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
					logger.Log($"{tempFuelValue} is not a number", Logger.LogLevel.Error);
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
				logger.Log($"{redParse.ToString()} is not a number", Logger.LogLevel.Error);
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
				logger.Log($"{greenParse.ToString()} is not a number", Logger.LogLevel.Error);
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
				logger.Log($"{blueParse.ToString()} is not a number", Logger.LogLevel.Error);
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
		/// Misc menu config GUI.
		/// </summary>
		private void MiscMenu()
		{
			float x = vehicleMenuX;
			float y = vehicleMenuY + vehicleMenuHeight + 25f;
			float width = vehicleMenuWidth;
			float height = vehicleMenuHeight;

			y += (fuelMixes * 40f);

			GUI.Box(new Rect(x, y, width, height), "<color=#FFF><size=16><b>Miscellaneous settings</b></size></color>");

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
			float itemHeight = 120f;
			float thumbnailHeight = 90f;
			float textHeight = 30f;
			float initialRowX = x + 10f;
			float itemX = initialRowX;
			float itemY = 0f;

			int maxRowItems = Mathf.FloorToInt(width / (itemWidth + 10f));

			int columnCount = (int)Math.Ceiling((double)items.Count() / maxRowItems);

			float scrollHeight = (itemHeight + 10f) * (columnCount + 1);
			itemsScrollPosition = GUI.BeginScrollView(new Rect(x, y + 30f, width - 10f, height - 40f), itemsScrollPosition, new Rect(x, y + 30f, width - 10f, scrollHeight), new GUIStyle(), new GUIStyle());

			for (int i = 0; i < items.Count(); i++)
			{
				Item currentItem = items[i];
				GameObject item = items[i].item;

				itemX += itemWidth + 10f;

				if (i % maxRowItems == 0)
				{
					itemX = initialRowX;
					itemY += itemHeight + 10f;
				}

				if (GUI.Button(new Rect(itemX, itemY, itemWidth, itemHeight), String.Empty) ||
					GUI.Button(new Rect(itemX, itemY, itemWidth, thumbnailHeight), currentItem.thumbnail) ||
					GUI.Button(new Rect(itemX, itemY + thumbnailHeight, itemWidth, textHeight), item.name))
				{
					Utility.Spawn(currentItem.Clone());
				}
			}

			GUI.EndScrollView();
		}
	}
}
