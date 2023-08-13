using SpawnerTLD.Core;
using System;
using System.Collections;
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
		private Settings settings = new Settings();

		// Modules.
		private Logger logger;
		private Config config;
		private Translator translator;
		private ThumbnailGenerator thumbnailGenerator;
		private Keybinds binds;
		private Utility utility;

		// Menu control.
		public bool enabled = false;
		private bool show = false;
		private bool legacyUI = false;
		private bool settingsShow = false;
		private bool creditsShow = false;

		private int resolutionX;
		private int resolutionY;

		private float mainMenuWidth;
		private float mainMenuHeight;
		private float mainMenuX;
		private float mainMenuY;

		private float legacyMainMenuWidth;
		private float legacyMainMenuHeight;
		private float legacyMainMenuX;
		private float legacyMainMenuY;

		private float legacyVehicleMenuWidth;
		private float legacyVehicleMenuHeight;
		private float legacyVehicleMenuX;
		private float legacyVehicleMenuY;

		private bool vehicleMenu = false;
		private bool miscMenu = false;
		private bool itemsMenu = false;

		private int tab = 0;
		private readonly List<string> tabs = new List<string>()
		{
			"Vehicles",
			"Items",
			"POIs",
			"Shapes",
			"Miscellaneous"
		};

		private Vector2 tabScrollPosition;
		private Vector2 vehicleScrollPosition;
		private Vector2 itemScrollPosition;
		private Vector2 toggleScrollPosition;
		private Vector2 configScrollPosition;
		private Vector2 creditScrollPosition;

		// Tab indexes which render the config pane.
		private List<int> configTabs = new List<int>() { 0, 1, 3 };

		// Styling.
		private GUIStyle labelStyle = new GUIStyle();
		private GUIStyle headerStyle = new GUIStyle();
		private float scrollWidth = 10f;

		// General variables.
		private string search = String.Empty;

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

		// Filtering.
		private Dictionary<string, List<Type>> categories = new Dictionary<string, List<Type>>()
		{
			{ "Vehicles", new List<Type>() { typeof(carscript) } },
			{ "Tanks", new List<Type>() { typeof(tankscript) } },
			{ "Vehicle parts", new List<Type>() { typeof(partscript) } },
			{ "Guns", new List<Type>() { typeof(weaponscript) } },
			{ "Melee weapons", new List<Type>() { typeof(meleeweaponscript) } },
			{ "Cleaning", new List<Type>() { typeof(drotkefescript), typeof(spricniscript) } },
			{ "Refillables", new List<Type>() { typeof(ammoscript) } },
			{ "Food", new List<Type>() { typeof(ediblescript) } },
			{ "Wearables", new List<Type>() { typeof(wearable) } },
			{ "Lights", new List<Type>() { typeof(flashlightscript) } },
			{ "Usables", new List<Type>() { typeof(pickupable) } },
			{ "Other", new List<Type>() { typeof(MonoBehaviour) } },
		};
		private bool filterShow = false;
		private List<int> filters = new List<int>();

		// POI variables.
		private Vector2 poiScrollPosition;
		private List<POI> POIs = new List<POI>();
		private List<GameObject> spawnedPOIs = new List<GameObject>();
		private bool poiSpawnItems = true;

		// Shape variables.
		private Vector3 scale = Vector3.one;
		private bool linkScale = false;

		// Settings.
		private List<QuickSpawn> quickSpawns = new List<QuickSpawn>();
		private float selectedTime;
		private bool isTimeLocked;
		GameObject ufo;
		private Quaternion localRotation;
		private float settingsScrollWidth;
		private bool noclipGodmodeDisable = true;
		private Dictionary<string, string> accessibilityModes = new Dictionary<string, string>()
		{
			{ "none", "None" },
			{ "contrast", "Improved contrast" },
			{ "colourless", "Colourless" }
		};
		private bool accessibilityShow = false;
		private string accessibilityMode = "none";
		private float noclipFastMoveFactor = 10f;

		private temporaryTurnOffGeneration temp;

		public GUIRenderer(Logger _logger, Config _config, Translator _translator, ThumbnailGenerator _thumbnailGenerator, Keybinds _binds, Utility _utility)
		{
			logger = _logger;
			config = _config;
			translator = _translator;
			thumbnailGenerator = _thumbnailGenerator;
			binds = _binds;
			utility = _utility;
		}

		public void OnGUI()
		{
			// Return early if spawner is disabled.
			if (!enabled)
				return;

			// Override scrollbar width;
			GUI.skin.verticalScrollbar.fixedWidth = scrollWidth;
			GUI.skin.verticalScrollbarThumb.fixedWidth = scrollWidth;

			// Render the legacy UI if enabled.
			if (legacyUI)
			{
				RenderLegacyUI();
				return;
			}

			// Only show visibility menu on pause menu.
			if (mainscript.M.menu.Menu.activeSelf)
			{
				ToggleVisibility();
			}

			// Return early if the UI isn't supposed to be visible.
			if (!show)
				return;

			// Main menu always shows.
			MainMenu();
		}

		/// <summary>
		/// Render the legacy version of the UI
		/// </summary>
		private void RenderLegacyUI()
		{
			// Return early if pause menu isn't active.
			if (!mainscript.M.menu.Menu.activeSelf)
				return;

			ToggleVisibilityLegacy();

			// Return early if the UI isn't supposed to be visible.
			if (!show)
				return;

			// Main menu always shows.
			MainMenuLegacy();

			if (vehicleMenu)
			{
				VehicleMenuLegacy();
			}

			if (miscMenu) {
				MiscMenuLegacy();
			}
			
			if (itemsMenu)
			{
				ItemsMenuLegacy();
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

			resolutionX = mainscript.M.SettingObj.S.IResolutionX;
			resolutionY = mainscript.M.SettingObj.S.IResolutionY;

			// Set main menu position here so other menus can be based around it.
			legacyMainMenuWidth = resolutionX / 7f;
			legacyMainMenuHeight = resolutionY / 1.2f;
			legacyMainMenuX = resolutionX / 2.5f - legacyMainMenuWidth;
			legacyMainMenuY = 75f;

			mainMenuWidth = resolutionX - 80f;
			mainMenuHeight = resolutionY - 80f;
			mainMenuX = 40f;
			mainMenuY = 40f;

			// Also store the vehicle menu so the misc menu can be
			// placed under it.
			legacyVehicleMenuX = legacyMainMenuX + legacyMainMenuWidth + 15f;
			legacyVehicleMenuY = legacyMainMenuY;
			legacyVehicleMenuWidth = resolutionX / 2f;
			legacyVehicleMenuHeight = resolutionY / 4.25f;

			// Add available quickspawn items.
			// TODO: Allow these to be user-selected?
			quickSpawns.Add(new QuickSpawn() { gameObject = itemdatabase.d.goilcan, name = "Oil can" });
			quickSpawns.Add(new QuickSpawn() { gameObject = itemdatabase.d.ggascan, name = "Jerry can" });
			quickSpawns.Add(new QuickSpawn() { gameObject = itemdatabase.d.gbarrel, name = "Barrel" });

			// Prepare items list.
			thumbnailGenerator.PrepareCache();
			foreach (GameObject item in itemdatabase.d.items)
			{
				try
				{
					// Remove vehicles and trailers from items array.
					if (!utility.IsVehicleOrTrailer(item) && item.name != "ErrorPrefab")
					{
						items.Add(new Item() { item = item, thumbnail = thumbnailGenerator.GetThumbnail(item), category = utility.GetCategory(item, categories) });
					}
				}
				catch (Exception ex)
				{
					logger.Log($"Failed to load item {item.name} - {ex}", Logger.LogLevel.Error);
				}
			}

			vehicles = LoadVehicles();
			POIs = LoadPOIs();

			// Prepopulate fuel types and fuel values as all default.
			int maxFuelType = (int)Enum.GetValues(typeof(mainscript.fluidenum)).Cast<mainscript.fluidenum>().Max();
			for (int i = 0; i < maxFuelType; i++)
			{
				fuelValues.Add(-1f);
				fuelTypeInts.Add(-1);
			}

			// Load configs.
			legacyUI = config.GetLegacyMode(legacyUI);
			scrollWidth = config.GetScrollWidth(scrollWidth);
			settingsScrollWidth = scrollWidth;
			noclipGodmodeDisable = config.GetNoclipGodmodeDisable(noclipGodmodeDisable);
			accessibilityMode = config.GetAccessibilityMode(accessibilityMode);
			noclipFastMoveFactor = config.GetNoclipFastMoveFactor(noclipFastMoveFactor);

			// Load keybinds.
			binds.OnLoad();

			// Find instance of temporaryTurnOffGeneration to spawn UFO.
			temp = mainscript.M.menu.GetComponentInChildren<temporaryTurnOffGeneration>();
		}

		public void Update()
		{
			// Return early if the legacy UI is enabled.
			if (legacyUI)
				return;

			if (Input.GetKeyDown(binds.GetKeyByAction((int)Keybinds.Inputs.menu).key) && !mainscript.M.menu.Menu.activeSelf && !mainscript.M.settingsOpen && !mainscript.M.menu.saveScreen.gameObject.activeSelf)
			{
				show = !show;
				mainscript.M.crsrLocked = !show;
				mainscript.M.SetCursorVisible(show);
				mainscript.M.menu.gameObject.SetActive(!show);
			}

			if (show && !mainscript.M.menu.Menu.activeSelf && Input.GetButtonDown("Cancel"))
			{
				show = false;
				mainscript.M.crsrLocked = !show;
				mainscript.M.SetCursorVisible(show);
				mainscript.M.menu.gameObject.SetActive(!show);
			}
		}

		/// <summary>
		/// Show menu toggle button.
		/// </summary>
		private void ToggleVisibility()
		{
			if (GUI.Button(new Rect(resolutionX - 350f, 30f, 300f, 20f), "Switch to Legacy UI"))
			{
				legacyUI = true;
				config.UpdateLegacyMode(legacyUI);
			}
			binds.RenderRebindMenu("Spawner menu key", new int[] { (int)Keybinds.Inputs.menu }, resolutionX - 350f, 50f);
		}

		/// <summary>
		/// Show menu toggle button.
		/// </summary>
		private void ToggleVisibilityLegacy()
		{
			if (GUI.Button(new Rect(230f, 10f, 200f, 20f), "Switch to New UI"))
			{
				legacyUI = false;
				config.UpdateLegacyMode(legacyUI);

				// Hide the menu in case it's currently visible.
				show = false;
			}
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

			GUI.Box(new Rect(x, y, width, height), $"<color=#f87ffa><size=18><b>{Meta.Name}</b></size>\n<size=16>v{Meta.Version} - made with ❤️ by {Meta.Author}</size></color>");

			// Settings button.
			if (GUI.Button(new Rect(x + 5f, y + 5f, 150f, 25f), GetAccessibleString("Show settings", settingsShow)))
			{
				settingsShow = !settingsShow;
				creditsShow = false;
			}

			if (GUI.Button(new Rect(x + mainMenuWidth - 110f, y + 5f, 100f, 25f), GetAccessibleString("Credits", creditsShow)))
			{
				creditsShow = !creditsShow;
				settingsShow = false;
			}

			if (settingsShow)
			{
				// Render the setting page.
				try
				{
					binds.RenderRebindMenu("Rebind keys", (int[])Enum.GetValues(typeof(Keybinds.Inputs)), x + 10f, y + 50f, width * 0.25f, height - 65f);
				}
				catch (Exception ex)
				{
					logger.Log($"Error building settings rebind menu - {ex}", Logger.LogLevel.Error);
				}

				// Other settings.
				float settingsX = x + (width * 0.25f) + 20f;
				float settingsY = y + 50f;
				float settingsWidth = width * 0.75f - 30f;
				float settingsHeight = height - 65f;
				GUI.Box(new Rect(settingsX, settingsY, settingsWidth, settingsHeight), "Settings");

				settingsX += 10f;
				settingsY += 50f;
				float configHeight = 20f;

				float buttonWidth = 200f;

				settingsWidth -= 20f;

				GUI.Label(new Rect(settingsX, settingsY, settingsWidth, configHeight), "Scroll bar width:", labelStyle);
				settingsY += configHeight;
				float tempScrollWidth = GUI.HorizontalSlider(new Rect(settingsX, settingsY, settingsWidth, configHeight), settingsScrollWidth, 5f, 30f);
				settingsScrollWidth = Mathf.Round(tempScrollWidth);
				settingsY += configHeight;

				// GUI.VerticalScrollbar doesn't work properly so just use a button as the preview.
				GUI.Button(new Rect(settingsX, settingsY, settingsScrollWidth, configHeight), String.Empty);
				GUI.Label(new Rect(settingsX + settingsScrollWidth + 10f, settingsY, settingsWidth - settingsScrollWidth - 10f, configHeight), settingsScrollWidth.ToString());

				settingsY += configHeight;

				if (GUI.Button(new Rect(settingsX, settingsY, buttonWidth, configHeight), "Apply"))
				{
					scrollWidth = settingsScrollWidth;
					config.UpdateScrollWidth(scrollWidth);
				}

				if (GUI.Button(new Rect(settingsX + buttonWidth + 10f, settingsY, buttonWidth, configHeight), "Reset"))
				{
					scrollWidth = 10f;
					settingsScrollWidth = scrollWidth;
					config.UpdateScrollWidth(scrollWidth);
				}

				settingsY += configHeight + 10f;

				GUI.Label(new Rect(settingsX, settingsY, settingsWidth, configHeight), "Disabling noclip disables godmode:", labelStyle);
				settingsY += configHeight;
				if (GUI.Button(new Rect(settingsX, settingsY, buttonWidth, configHeight), GetAccessibleString("On", "Off", noclipGodmodeDisable)))
				{
					noclipGodmodeDisable = !noclipGodmodeDisable;
					config.UpdateNoclipGodmodeDisable(noclipGodmodeDisable);
				}

				settingsY += configHeight + 10f;

				GUI.Label(new Rect(settingsX, settingsY, settingsWidth, configHeight), "Noclip speed increase factor:", labelStyle);
				settingsY += configHeight;
				float factor = GUI.HorizontalSlider(new Rect(settingsX, settingsY, settingsWidth, configHeight), noclipFastMoveFactor, 2f, 100f);
				noclipFastMoveFactor = Mathf.Round(factor);
				settingsY += configHeight;
				GUI.Label(new Rect(settingsX, settingsY, settingsWidth, configHeight), noclipFastMoveFactor.ToString());

				settingsY += configHeight + 10f;

				if (GUI.Button(new Rect(settingsX, settingsY, buttonWidth, configHeight), "Accessibility mode"))
				{
					accessibilityShow = !accessibilityShow;
				}
				if (accessibilityShow)
				{
					settingsY += configHeight;
					GUI.Box(new Rect(settingsX, settingsY, buttonWidth, (configHeight + 2f) * accessibilityModes.Count), String.Empty);
					for (int i = 0; i < accessibilityModes.Count; i++)
					{
						KeyValuePair<string, string> mode = accessibilityModes.ElementAt(i);
						if (GUI.Button(new Rect(settingsX, settingsY, buttonWidth, configHeight), GetAccessibleString(mode.Value, accessibilityMode == mode.Key)))
						{
							accessibilityMode = mode.Key;
							config.UpdateAccessibilityMode(accessibilityMode);
						}

						settingsY += configHeight + 2f;
					}
				}
			}
			else if (creditsShow)
			{
				float creditsX = x + 10f;
				float creditsY = y + 50f;
				float creditsWidth = width - 20f;
				float creditsHeight = height - 65f;
				GUI.Box(new Rect(creditsX, creditsY, creditsWidth, creditsHeight), "<size=18>Credits</size>");

				creditsX += 10f;
				creditsY += 50f;


				List<string> credits = new List<string>()
				{
					"M- - The rewrite",
					"RUNDEN - Thumbnail generator",
					"FreakShow - Original spawner",
					"_RainBowSheep_ - Original spawner"
				};

				List<string> other = new List<string>()
				{
					"copperboltwire",
					"SgtJoe",
					"Tumpy_Noodles",
					"SubG",
					"_Starixx",
					"the Sabii",
					"Jessica",
					"Doubt"
				};

				float totalCreditsHeight = (credits.Count + other.Count) * 20f;

				creditScrollPosition = GUI.BeginScrollView(new Rect(creditsX, creditsY, creditsWidth, creditsHeight), creditScrollPosition, new Rect(creditsX, creditsY, creditsWidth, totalCreditsHeight));

				foreach (string credit in credits)
				{
					GUI.Label(new Rect(creditsX, creditsY, creditsWidth, 20f), $"<size=16>{credit}</size>");
					creditsY += 30f;
				}

				GUI.Label(new Rect(creditsX, creditsY, creditsWidth, 20f), $"<b><size=16>With special thanks to the following for the bug reports/feature suggestions:</size></b>");
				creditsY += 30f;
				foreach (string name in other)
				{
					GUI.Label(new Rect(creditsX, creditsY, creditsWidth, 20f), $"<size=16>{name}</size>");
					creditsY += 25f;
				}
			}
			else
			{
				// Render the menu.

				// Navigation tabs.
				float tabHeight = 25f;
				float tabButtonWidth = 150f;
				float tabWidth = (tabButtonWidth + 30f) * tabs.Count;
				float tabX = x + 20f;
				tabScrollPosition = GUI.BeginScrollView(new Rect(tabX, y + 50f, tabWidth, tabHeight), tabScrollPosition, new Rect(tabX, y + 50f, tabWidth, tabHeight));
				for (int tabIndex = 0; tabIndex < tabs.Count; tabIndex++)
				{
					if (GUI.Button(new Rect(tabX, y + 50f, tabButtonWidth, tabHeight), tab == tabIndex ? $"<color=#0F0>{tabs[tabIndex]}</color>" : tabs[tabIndex]))
					{
						tab = tabIndex;

						// Reset config scroll position when changing tabs.
						configScrollPosition = Vector2.zero;
					}

					tabX += tabButtonWidth + 30f;
				}
				GUI.EndScrollView();

				RenderTab(tab);
			}
		}

		private class ConfigDimensions
		{
			public float x;
			public float y;
			public float width;
			public float height;
		}

		/// <summary>
		/// Render a given tab
		/// </summary>
		/// <param name="tab">The tab index to render</param>
		private void RenderTab(int tab)
		{
			float tabX = mainMenuX + 10f;
			float tabY = mainMenuX + 90f;
			float tabWidth = mainMenuWidth - 20f;
			float tabHeight = mainMenuHeight - 105f;

			float configWidth = (mainMenuWidth * 0.25f);
			float configX = mainMenuX + mainMenuWidth - configWidth - 10f;

			// Config pane.
			if (configTabs.Contains(tab))
			{
				// Decrease tab width to account for content pane.
				tabWidth -= configWidth + 5f;

				GUI.Box(new Rect(configX, tabY, configWidth, tabHeight), "<size=16>Configuration</size>");

				ConfigDimensions configDimensions = new ConfigDimensions()
				{
					x = configX,
					y = tabY,
					width = configWidth,
					height = tabHeight,	
				};

				RenderConfig(tab, configDimensions);
			}

			GUI.Box(new Rect(tabX, tabY, tabWidth, tabHeight), String.Empty);

			float itemWidth = 140f;
			float thumbnailHeight = 90f;
			float textHeight = 40f;
			float itemHeight = thumbnailHeight + textHeight;
			float initialRowX = tabX + 10f;
			float itemX = initialRowX;
			float itemY = 0f;

			float searchHeight = 20f;

			int maxRowItems = Mathf.FloorToInt(tabWidth / (itemWidth + 10f));
			int columnCount = (int)Math.Ceiling((double)vehicles.Count / maxRowItems);
			float scrollHeight = (itemHeight + 10f) * (columnCount + 1);

			switch (tab)
			{
				// Vehicles tab.
				case 0:
					GUI.skin.button.wordWrap = true;

					itemWidth = 140f;
					thumbnailHeight = 90f;
					textHeight = 40f;
					itemHeight = thumbnailHeight + textHeight;
					initialRowX = tabX + 10f;
					itemX = initialRowX;
					itemY = 0f;

					// Search field.
					GUI.Label(new Rect(tabX + 10f, tabY + 10f, 60f, searchHeight), "Search:", labelStyle);
					search = GUI.TextField(new Rect(tabX + 70f, tabY + 10f, tabWidth * 0.25f, searchHeight), search, labelStyle);
					if (GUI.Button(new Rect(tabX + 60f + tabWidth * 0.25f + 10f, tabY + 10f, 100f, searchHeight), "Reset"))
						search = String.Empty;

					// Filter vehicle list by search term.
					List<Vehicle> searchVehicles = vehicles;
					if (search != String.Empty)
						searchVehicles = vehicles.Where(v => v.name.ToLower().Contains(search.ToLower()) || v.vehicle.name.ToLower().Contains(search.ToLower())).ToList();

					scrollHeight = (itemHeight + 10f) * (columnCount + 1);
					vehicleScrollPosition = GUI.BeginScrollView(new Rect(tabX, tabY + 10f + searchHeight, tabWidth, tabHeight - 10f - searchHeight), vehicleScrollPosition, new Rect(tabX, tabY, tabWidth, scrollHeight - 10f - searchHeight), new GUIStyle(), GUI.skin.verticalScrollbar);

					for (int i = 0; i < searchVehicles.Count(); i++)
					{
						Vehicle currentVehicle = searchVehicles[i];

						itemX += itemWidth + 10f;

						if (i % maxRowItems == 0)
						{
							itemX = initialRowX;
							itemY += itemHeight + 10f;
						}

						if (GUI.Button(new Rect(itemX, itemY, itemWidth, itemHeight), String.Empty) ||
							GUI.Button(new Rect(itemX, itemY, itemWidth, thumbnailHeight), currentVehicle.thumbnail) ||
							GUI.Button(new Rect(itemX, itemY + thumbnailHeight, itemWidth, textHeight), currentVehicle.name))
						{
							utility.Spawn(new Vehicle()
							{
								vehicle = currentVehicle.vehicle,
								variant = currentVehicle.variant,
								conditionInt = conditionInt,
								fuelMixes = fuelMixes,
								fuelValues = fuelValues,
								fuelTypeInts = fuelTypeInts,
								color = color,
								plate = plate
							});
						}
					}
					GUI.EndScrollView();
					break;

				// Items tab.
				case 1:
					GUI.skin.button.wordWrap = true;
					itemWidth = 140f;
					thumbnailHeight = 90f;
					textHeight = 40f;
					itemHeight = thumbnailHeight + textHeight;
					initialRowX = tabX + 10f;
					itemX = initialRowX;
					itemY = 0f;

					// Search field.
					GUI.Label(new Rect(tabX + 10f, tabY + 10f, 60f, searchHeight), "Search:", labelStyle);
					search = GUI.TextField(new Rect(tabX + 70f, tabY + 10f, tabWidth * 0.25f, searchHeight), search, labelStyle);
					if (GUI.Button(new Rect(tabX + 60f + tabWidth * 0.25f + 10f, tabY + 10f, 100f, searchHeight), "Reset"))
						search = String.Empty;

					// Filter item list by search term.
					List<Item> searchItems = items;
					if (search != String.Empty)
						searchItems = items.Where(v => v.item.name.ToLower().Contains(search.ToLower())).ToList();

					if (filters.Count > 0)
						searchItems = searchItems.Where(v => filters.Contains(v.category)).ToList();

					maxRowItems = Mathf.FloorToInt(tabWidth / (itemWidth + 10f));

					columnCount = (int)Math.Ceiling((double)items.Count / maxRowItems);

					scrollHeight = (itemHeight + 10f) * (columnCount + 1);
					itemScrollPosition = GUI.BeginScrollView(new Rect(tabX, tabY + 10f + searchHeight, tabWidth, tabHeight - 10f - searchHeight), itemScrollPosition, new Rect(tabX, tabY, tabWidth, scrollHeight - 10f - searchHeight), new GUIStyle(), GUI.skin.verticalScrollbar);

					GUI.enabled = !filterShow;
					for (int i = 0; i < searchItems.Count(); i++)
					{
						Item currentItem = searchItems[i];
						GameObject item = searchItems[i].item;

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
							utility.Spawn(new Item()
							{
								item = item,
								conditionInt = conditionInt,
								fuelMixes = fuelMixes,
								fuelValues = fuelValues,
								fuelTypeInts = fuelTypeInts,
								color = color,
							});
						}
					}

					GUI.enabled = true;

					GUI.EndScrollView();

					// Filters need rendering last to ensure they show on top of the item grid.
					float filterWidth = 200f;
					float filterY = tabY + 10f;
					float filterX = tabX + tabWidth - filterWidth - 10f;
					if (GUI.Button(new Rect(filterX, filterY, filterWidth, searchHeight), "Filters"))
					{
						filterShow = !filterShow;
					}

					if (filterShow)
					{
						filterY += searchHeight;
						GUI.Box(new Rect(filterX, filterY, filterWidth, (searchHeight + 2f ) * categories.Count), String.Empty);
						for (int i = 0; i < categories.Count; i++)
						{
							string name = categories.ElementAt(i).Key;
							if (GUI.Button(new Rect(filterX, filterY, filterWidth, searchHeight), filters.Contains(i) ? $"<color=#0F0>{name}</color>" : name))
							{
								if (filters.Contains(i))
									filters.Remove(i);
								else
									filters.Add(i);

								// Reset scroll position to avoid the items menu looking empty
								// but actually being scrolled past the end of the list.
								itemScrollPosition = new Vector2(0, 0);
							}

							filterY += searchHeight + 2f;
						}
					}
					break;

				// POIs tab.
				case 2:
					GUI.skin.button.wordWrap = true;

					itemWidth = 140f;
					thumbnailHeight = 90f;
					textHeight = 40f;
					itemHeight = thumbnailHeight + textHeight;
					initialRowX = tabX + 10f;
					itemX = initialRowX;
					itemY = 0f;

					// Search field.
					GUI.Label(new Rect(tabX + 10f, tabY + 10f, 60f, searchHeight), "Search:", labelStyle);
					search = GUI.TextField(new Rect(tabX + 70f, tabY + 10f, tabWidth * 0.25f, searchHeight), search, labelStyle);
					if (GUI.Button(new Rect(tabX + 60f + tabWidth * 0.25f + 10f, tabY + 10f, 100f, searchHeight), "Reset"))
						search = String.Empty;

					if (GUI.Button(new Rect(tabX + tabWidth - 100f - 10f, tabY + 10f, 100f, searchHeight), GetAccessibleString("Spawn items", poiSpawnItems)))
						poiSpawnItems = !poiSpawnItems;

					// Filter POI list by search term.
					List<POI> searchPOIs = POIs;
					if (search != String.Empty)
						searchPOIs = POIs.Where(p => p.name.ToLower().Contains(search.ToLower()) || p.poi.name.ToLower().Contains(search.ToLower())).ToList();

					columnCount = (int)Math.Ceiling((double)POIs.Count / maxRowItems);

					scrollHeight = (itemHeight + 10f) * (columnCount + 1);
					poiScrollPosition = GUI.BeginScrollView(new Rect(tabX, tabY + 10f + searchHeight, tabWidth, tabHeight - 10f - searchHeight), poiScrollPosition, new Rect(tabX, tabY, tabWidth, scrollHeight - 10f - searchHeight), new GUIStyle(), GUI.skin.verticalScrollbar);

					for (int i = 0; i < searchPOIs.Count(); i++)
					{
						POI currentPOI = searchPOIs[i];

						itemX += itemWidth + 10f;

						if (i % maxRowItems == 0)
						{
							itemX = initialRowX;
							itemY += itemHeight + 10f;
						}

						if (GUI.Button(new Rect(itemX, itemY, itemWidth, itemHeight), String.Empty) ||
							GUI.Button(new Rect(itemX, itemY, itemWidth, thumbnailHeight), currentPOI.thumbnail) ||
							GUI.Button(new Rect(itemX, itemY + thumbnailHeight, itemWidth, textHeight), currentPOI.name))
						{
							GameObject spawnedPOI = utility.Spawn(currentPOI, poiSpawnItems);
							if (spawnedPOI != null)
								spawnedPOIs.Add(spawnedPOI);
						}
					}
					GUI.EndScrollView();
					break;

				// Shapes tab.
				case 3:
					GUI.skin.button.wordWrap = true;

					itemWidth = 140f;
					itemHeight = 40f;
					initialRowX = tabX + 10f;
					itemX = initialRowX;
					itemY = tabY - 40f;

					Dictionary<string, string> shapes = new Dictionary<string, string>()
					{
						{ "cube", "Cube" },
						{ "sphere", "Sphere" },
						{ "cylinder", "Cylinder" }
					};

					for (int i = 0; i < shapes.Count(); i++)
					{
						KeyValuePair<string, string> shape = shapes.ElementAt(i);

						itemX += itemWidth + 10f;

						if (i % maxRowItems == 0)
						{
							itemX = initialRowX;
							itemY += itemHeight + 10f;
						}

						if (GUI.Button(new Rect(itemX, itemY, itemWidth, itemHeight), shape.Value))
						{
							GameObject primitive = null;

							switch (shape.Key)
							{
								case "cube":
									primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
									break;
								case "sphere":
									primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
									//primitive.AddComponent<childmoverscript>();
									break;
								case "cylinder":
									primitive = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
									//primitive.AddComponent<childmoverscript>();
									break;
							}


							if (primitive != null)
							{
								pickupable pickupable = primitive.AddComponent<pickupable>();
								massScript mass = primitive.AddComponent<massScript>();
								mass.SetMass(20f);
								mass.P = pickupable;
								mass.AddRB();

								primitive.GetComponent<Renderer>().material.color = color;
								primitive.transform.localScale = scale;

								mainscript.M.Spawn(primitive, -1);
							}
						}
					}
					break;

				// Miscellaneous tab.
				case 4:
					float miscX = tabX + 10f;
					float miscY = tabY + 10f;
					float buttonWidth = 200f;
					float buttonHeight = 20f;

					float miscWidth = 250f;
					float labelWidth = tabWidth - 20f;

					int toggleCount = 3;
					float toggleWidth = (buttonWidth + 10f) * toggleCount;

					float toggleX = miscX;

					toggleScrollPosition = GUI.BeginScrollView(new Rect(miscX, miscY, toggleWidth, buttonHeight), toggleScrollPosition, new Rect(miscX, miscY, toggleWidth, buttonHeight));

					// Delete mode.
					if (GUI.Button(new Rect(toggleX, miscY, buttonWidth, buttonHeight), GetAccessibleString("Delete mode", settings.deleteMode) + $" (Press {binds.GetKeyByAction((int)Keybinds.Inputs.deleteMode).key})"))
					{
						settings.deleteMode = !settings.deleteMode;
					}

					toggleX += buttonWidth + 10f;

					// God toggle.
					if (GUI.Button(new Rect(toggleX, miscY, buttonWidth, buttonHeight), GetAccessibleString("God mode", settings.godMode)))
					{
						settings.godMode = !settings.godMode;
						mainscript.M.ChGodMode(settings.godMode);
					}
					toggleX += buttonWidth + 10f;

					// Noclip toggle.
					if (GUI.Button(new Rect(toggleX, miscY, buttonWidth, buttonHeight), GetAccessibleString("Noclip", settings.noclip)))
					{
						settings.noclip = !settings.noclip;

						if (settings.noclip)
						{
							Noclip noclip = mainscript.M.player.gameObject.AddComponent<Noclip>();
							noclip.constructor(binds, logger, noclipFastMoveFactor);
							localRotation = mainscript.M.player.transform.localRotation;
							mainscript.M.player.Th.localEulerAngles = new Vector3(0f, 0f, 0f);
							settings.godMode = true;

							// Disable colliders.
							foreach (Collider collider in mainscript.M.player.C)
							{
								collider.enabled = false;
							}
						}
						else
						{
							Noclip noclip = mainscript.M.player.gameObject.GetComponent<Noclip>();
							if (noclip != null)
							{
								UnityEngine.Object.Destroy(noclip);

								// Resetting localRotation stops the player from flying infinitely
								// upwards when coming out of noclip.
								// I have no idea why, it just works.
								mainscript.M.player.transform.localRotation = localRotation;

								// Re-enable colliders.
								foreach (Collider collider in mainscript.M.player.C)
								{
									collider.enabled = true;
								}
							}

							if (noclipGodmodeDisable)
								settings.godMode = false;
						}
						mainscript.M.ChGodMode(settings.godMode);
					}
					toggleX += buttonWidth + 10f;

					GUI.EndScrollView();

					miscY += buttonHeight + 20f;

					// Time setting.
					// TODO: Work out what the time actually is.
					GUI.Label(new Rect(miscX, miscY, labelWidth, buttonHeight), "Time:", labelStyle);
					miscY += buttonHeight;
					float time = GUI.HorizontalSlider(new Rect(miscX, miscY, miscWidth, buttonHeight), selectedTime, 0f, 360f);
					selectedTime = Mathf.Round(time);
					if (GUI.Button(new Rect(miscX + miscWidth + 10f, miscY, buttonWidth, buttonHeight), "Set"))
					{
						mainscript.M.napszak.tekeres = selectedTime;
					}

					if (GUI.Button(new Rect(miscX + miscWidth + buttonWidth + 20f, miscY, buttonWidth, buttonHeight), GetAccessibleString("Unlock", "Lock", isTimeLocked)))
					{
						isTimeLocked = !isTimeLocked;

						mainscript.M.napszak.enabled = !isTimeLocked;
					}

					miscY += buttonHeight + 10f;

					GUI.Label(new Rect(miscX, miscY, labelWidth, buttonHeight), "UFO spawning (doesn't save):", labelStyle);

					if (GUI.Button(new Rect(miscX + miscWidth + 10f, miscY, buttonWidth, buttonHeight), "Spawn"))
					{
						try
						{
							// Destory existing UFO.
							if (ufo != null)
								UnityEngine.Object.Destroy(ufo);

							ufo = UnityEngine.Object.Instantiate(temp.FEDOSPAWN.prefab, mainscript.M.player.lookPoint + Vector3.up * 0.75f, Quaternion.FromToRotation(Vector3.forward, -mainscript.M.player.transform.right));
							fedoscript ufoScript = ufo.GetComponent<fedoscript>();
							ufoScript.ai = false;
							ufoScript.followRoad = false;
						}
						catch (Exception ex)
						{
							logger.Log($"Failed to spawn UFO - {ex}", Logger.LogLevel.Error);
						}
					}

					if (GUI.Button(new Rect(miscX + miscWidth + buttonWidth + 20f, miscY, buttonWidth, buttonHeight), "Delete"))
					{
						if (ufo != null)
						{
							fedoscript ufoScript = ufo.GetComponent<fedoscript>();
							if (!ufoScript.seat.inUse)
								UnityEngine.Object.Destroy(ufo);
						}
					}

					miscY += buttonHeight + 10f;

					if (GUI.Button(new Rect(miscX, miscY, buttonWidth, buttonHeight), "Delete last building"))
					{
						if (spawnedPOIs.Count > 0)
						{
							try
							{
								GameObject poi = spawnedPOIs.Last();
								spawnedPOIs.Remove(poi);
								GameObject.Destroy(poi);
							}
							catch (Exception ex)
							{
								logger.Log($"Error deleting POI - {ex}", Logger.LogLevel.Error);
							}
						}
					}

					miscY += buttonHeight + 10f;

					if (GUI.Button(new Rect(miscX, miscY, buttonWidth, buttonHeight), "Respawn nearest building items"))
					{
						Vector3 playerPosition = mainscript.M.player.transform.position;

						// Find closest building.
						float distance = float.MaxValue;
						GameObject closestBuilding = null;
						foreach (KeyValuePair<int, GameObject> building in mainscript.M.terrainGenerationSettings.roadBuildingGeneration.placedBuildings)
						{
							Vector3 position = building.Value.transform.position;
							float buildingDistance = Vector3.Distance(position, playerPosition);
							if (buildingDistance < distance)
							{
								distance = buildingDistance;
								closestBuilding = building.Value;
							}
						}

						// Trigger item respawn.
						buildingscript buildingscript = closestBuilding.GetComponent<buildingscript>();
						buildingscript.itemsSpawned = false;
						buildingscript.SpawnStuff(0);
					}

					break;
			}
		}

		/// <summary>
		/// Render the config pane
		/// </summary>
		/// <param name="tab">The tab index to render the config pane for</param>
		private void RenderConfig(int tab, ConfigDimensions configDimensions)
		{
			float configX = configDimensions.x + 5f;
			float configY = configDimensions.y + 30f;
			float configWidth = configDimensions.width - 10f;
			float configHeight = 20f;

			float red;
			float green;
			float blue;
			bool redParse;
			bool greenParse;
			bool blueParse;

			GUIStyle defaultStyle = GUI.skin.button;
			GUIStyle previewStyle = new GUIStyle(defaultStyle);
			Texture2D previewTexture = new Texture2D(1, 1);
			Color[] pixels = new Color[] { color };

			switch (tab)
			{
				case 0:
				case 1:
					// Fuel mixes needs multiplying by two as it has two fields per mix.
					int configItems = 7 + (fuelMixes * 2);
					float configScrollHeight = configItems * ((configHeight * 3) + 10f);
					configScrollPosition = GUI.BeginScrollView(new Rect(configX, configY, configWidth, configDimensions.height - 40f), configScrollPosition, new Rect(configX, configY, configWidth, configScrollHeight), new GUIStyle(), new GUIStyle());

					// Condition.
					GUI.Label(new Rect(configX, configY, configWidth, configHeight), $"Condition:", labelStyle);
					configY += configHeight;
					int maxCondition = (int)Enum.GetValues(typeof(Item.Condition)).Cast<Item.Condition>().Max();
					float rawCondition = GUI.HorizontalSlider(new Rect(configX, configY, configWidth, configHeight), conditionInt, -1, maxCondition);
					conditionInt = Mathf.RoundToInt(rawCondition);
					configY += configHeight;
					string conditionName = ((Item.Condition)conditionInt).ToString();
					GUI.Label(new Rect(configX, configY, configWidth, configHeight), conditionName, labelStyle);

					configY += configHeight + 10f;

					if (GUI.Button(new Rect(configX, configY, 200f, configHeight), GetAccessibleString("Spawn with fuel", settings.spawnWithFuel)))
						settings.spawnWithFuel = !settings.spawnWithFuel;

					configY += configHeight + 10f;

					// Fuel mixes.
					int maxFuelType = (int)Enum.GetValues(typeof(mainscript.fluidenum)).Cast<mainscript.fluidenum>().Max();
					GUI.Label(new Rect(configX, configY, configWidth, configHeight), "Number of fuels:", labelStyle);
					configY += configHeight;
					float rawFuelMixes = GUI.HorizontalSlider(new Rect(configX, configY, configWidth, configHeight), fuelMixes, 1, maxFuelType + 1);
					fuelMixes = Mathf.RoundToInt(rawFuelMixes);
					configY += configHeight;
					GUI.Label(new Rect(configX, configY, configWidth, configHeight), fuelMixes.ToString(), labelStyle);

					for (int i = 0; i < fuelMixes; i++)
					{
						configY += configHeight + 10f;

						// Fuel type.
						GUI.Label(new Rect(configX, configY, configWidth, configHeight), $"Fuel type {i + 1}:", labelStyle);
						configY += configHeight;
						float rawFuelType = GUI.HorizontalSlider(new Rect(configX, configY, configWidth, configHeight), fuelTypeInts[i], -1, maxFuelType);
						fuelTypeInts[i] = Mathf.RoundToInt(rawFuelType);
						configY += configHeight;

						string fuelType = ((mainscript.fluidenum)fuelTypeInts[i]).ToString();
						if (fuelTypeInts[i] == -1)
							fuelType = "Default";
						else
							fuelType = fuelType[0].ToString().ToUpper() + fuelType.Substring(1);

						GUI.Label(new Rect(configX, configY, configWidth, configHeight), fuelType, labelStyle);

						configY += configHeight + 10f;

						// Fuel amount.
						GUI.Label(new Rect(configX, configY, configWidth, configHeight), $"Fuel amount {i + 1}:", labelStyle);
						configY += configHeight;
						float rawFuelValue = GUI.HorizontalSlider(new Rect(configX, configY, configWidth, configHeight), fuelValues[i], -1f, 1000f);
						fuelValues[i] = Mathf.Round(rawFuelValue);
						configY += configHeight;

						bool fuelValueParse = float.TryParse(GUI.TextField(new Rect(configX, configY, configWidth, configHeight), fuelValues[i].ToString(), labelStyle), out float tempFuelValue);
						if (!fuelValueParse)
							logger.Log($"{tempFuelValue} is not a number", Logger.LogLevel.Error);
						else
							fuelValues[i] = tempFuelValue;
					}

					configY += configHeight + 10f;

					// Vehicle colour sliders.
					// Red.
					GUI.Label(new Rect(configX, configY, configWidth, configHeight), "<color=#F00>Red:</color>", labelStyle);
					configY += configHeight;
					red = GUI.HorizontalSlider(new Rect(configX, configY, configWidth, configHeight), color.r * 255, 0, 255);
					red = Mathf.Round(red);
					configY += configHeight;
					redParse = float.TryParse(GUI.TextField(new Rect(configX, configY, configWidth, configHeight), red.ToString(), labelStyle), out red);
					if (!redParse)
						logger.Log($"{redParse} is not a number", Logger.LogLevel.Error);
					red = Mathf.Clamp(red, 0f, 255f);
					color.r = red / 255f;
					//GUI.Label(new Rect(configX, configY, configWidth, configHeight), red.ToString(), labelStyle);

					// Green.
					configY += configHeight + 10f;
					GUI.Label(new Rect(configX, configY, configWidth, configHeight), "<color=#0F0>Green:</color>", labelStyle);
					configY += configHeight;
					green = GUI.HorizontalSlider(new Rect(configX, configY, configWidth, configHeight), color.g * 255, 0, 255);
					green = Mathf.Round(green);
					configY += configHeight;
					greenParse = float.TryParse(GUI.TextField(new Rect(configX, configY, configWidth, configHeight), green.ToString(), labelStyle), out green);
					if (!greenParse)
						logger.Log($"{greenParse} is not a number", Logger.LogLevel.Error);
					green = Mathf.Clamp(green, 0f, 255f);
					color.g = green / 255f;
					//GUI.Label(new Rect(configX, configY, configWidth, configHeight), green.ToString(), labelStyle);

					// Blue.
					configY += configHeight + 10f;
					GUI.Label(new Rect(configX, configY, configWidth, configHeight), "<color=#00F>Blue:</color>", labelStyle);
					configY += configHeight;
					blue = GUI.HorizontalSlider(new Rect(configX, configY, configWidth, configHeight), color.b * 255, 0, 255);
					blue = Mathf.Round(blue);
					configY += configHeight;
					blueParse = float.TryParse(GUI.TextField(new Rect(configX, configY, configWidth, configHeight), blue.ToString(), labelStyle), out blue);
					if (!blueParse)
						logger.Log($"{blueParse.ToString()} is not a number", Logger.LogLevel.Error);
					blue = Mathf.Clamp(blue, 0f, 255f);
					color.b = blue / 255f;
					//GUI.Label(new Rect(configX, configY, configWidth, configHeight), blue.ToString(), labelStyle);

					configY += configHeight + 10f;

					if (GUI.Button(new Rect(configX, configY, 200f, configHeight), "Randomise colour"))
					{
						color.r = UnityEngine.Random.Range(0f, 255f) / 255f;
						color.g = UnityEngine.Random.Range(0f, 255f) / 255f;
						color.b = UnityEngine.Random.Range(0f, 255f) / 255f;
					}

					configY += configHeight + 10f;

					// Colour preview.
					pixels = new Color[] { color };
					previewTexture.SetPixels(pixels);
					previewTexture.Apply();
					previewStyle.normal.background = previewTexture;
					previewStyle.active.background = previewTexture;
					previewStyle.hover.background = previewTexture;
					previewStyle.margin = new RectOffset(0, 0, 0, 0);
					GUI.skin.button = previewStyle;
					GUI.Button(new Rect(configX, configY, configWidth, configHeight), "");
					GUI.skin.button = defaultStyle;

					// License plate only renders for vehicle tab.
					if (tab == 0)
					{
						configY += configHeight + 10f;
						GUI.Label(new Rect(configX, configY, configWidth, configHeight), "Plate (blank for random):", labelStyle);
						configY += configHeight;
						plate = GUI.TextField(new Rect(configX, configY, configWidth, configHeight), plate, 7, labelStyle);
					}

					GUI.EndScrollView();
					break;
				case 3:
					int shapeConfigItems = 8;
					float shapeConfigScrollHeight = shapeConfigItems * ((configHeight * 3) + 10f);

					configScrollPosition = GUI.BeginScrollView(new Rect(configX, configY, configWidth, configDimensions.height - 40f), configScrollPosition, new Rect(configX, configY, configWidth, shapeConfigScrollHeight), new GUIStyle(), new GUIStyle());
					// Red.
					GUI.Label(new Rect(configX, configY, configWidth, configHeight), "<color=#F00>Red:</color>", labelStyle);
					configY += configHeight;
					red = GUI.HorizontalSlider(new Rect(configX, configY, configWidth, configHeight), color.r * 255, 0, 255);
					red = Mathf.Round(red);
					configY += configHeight;
					redParse = float.TryParse(GUI.TextField(new Rect(configX, configY, configWidth, configHeight), red.ToString(), labelStyle), out red);
					if (!redParse)
						logger.Log($"{redParse} is not a number", Logger.LogLevel.Error);
					red = Mathf.Clamp(red, 0f, 255f);
					color.r = red / 255f;

					// Green.
					configY += configHeight + 10f;
					GUI.Label(new Rect(configX, configY, configWidth, configHeight), "<color=#0F0>Green:</color>", labelStyle);
					configY += configHeight;
					green = GUI.HorizontalSlider(new Rect(configX, configY, configWidth, configHeight), color.g * 255, 0, 255);
					green = Mathf.Round(green);
					configY += configHeight;
					greenParse = float.TryParse(GUI.TextField(new Rect(configX, configY, configWidth, configHeight), green.ToString(), labelStyle), out green);
					if (!greenParse)
						logger.Log($"{greenParse} is not a number", Logger.LogLevel.Error);
					green = Mathf.Clamp(green, 0f, 255f);
					color.g = green / 255f;

					// Blue.
					configY += configHeight + 10f;
					GUI.Label(new Rect(configX, configY, configWidth, configHeight), "<color=#00F>Blue:</color>", labelStyle);
					configY += configHeight;
					blue = GUI.HorizontalSlider(new Rect(configX, configY, configWidth, configHeight), color.b * 255, 0, 255);
					blue = Mathf.Round(blue);
					configY += configHeight;
					blueParse = float.TryParse(GUI.TextField(new Rect(configX, configY, configWidth, configHeight), blue.ToString(), labelStyle), out blue);
					if (!blueParse)
						logger.Log($"{blueParse.ToString()} is not a number", Logger.LogLevel.Error);
					blue = Mathf.Clamp(blue, 0f, 255f);
					color.b = blue / 255f;

					configY += configHeight + 10f;

					if (GUI.Button(new Rect(configX, configY, 200f, configHeight), "Randomise colour"))
					{
						color.r = UnityEngine.Random.Range(0f, 255f) / 255f;
						color.g = UnityEngine.Random.Range(0f, 255f) / 255f;
						color.b = UnityEngine.Random.Range(0f, 255f) / 255f;
					}

					configY += configHeight + 10f;

					// Colour preview.
					pixels = new Color[] { color };
					previewTexture.SetPixels(pixels);
					previewTexture.Apply();
					previewStyle.normal.background = previewTexture;
					previewStyle.active.background = previewTexture;
					previewStyle.hover.background = previewTexture;
					previewStyle.margin = new RectOffset(0, 0, 0, 0);
					GUI.skin.button = previewStyle;
					GUI.Button(new Rect(configX, configY, configWidth, configHeight), "");
					GUI.skin.button = defaultStyle;

					configY += configHeight + 10f;
					if (GUI.Button(new Rect(configX, configY, configWidth, configHeight), GetAccessibleString("Link scale axis", linkScale)))
						linkScale = !linkScale;

					if (linkScale)
					{
						// Scale.
						configY += configHeight + 10f;
						GUI.Label(new Rect(configX, configY, configWidth, configHeight), "Scale:", labelStyle);
						configY += configHeight;
						float allScale = GUI.HorizontalSlider(new Rect(configX, configY, configWidth, configHeight), scale.x, 0.1f, 10f);
						allScale = (float)Math.Round(allScale, 2);
						configY += configHeight;
						allScale = (float)Math.Round(double.Parse(GUI.TextField(new Rect(configX, configY, configWidth, configHeight), allScale.ToString(), labelStyle)), 2);
						scale = new Vector3(allScale, allScale, allScale);
					}
					else
					{
						// X.
						configY += configHeight + 10f;
						GUI.Label(new Rect(configX, configY, configWidth, configHeight), "Scale X:", labelStyle);
						configY += configHeight;
						float x = GUI.HorizontalSlider(new Rect(configX, configY, configWidth, configHeight), scale.x, 0.1f, 10f);
						x = (float)Math.Round(x, 2);
						configY += configHeight;
						x = (float)Math.Round(double.Parse(GUI.TextField(new Rect(configX, configY, configWidth, configHeight), x.ToString(), labelStyle)), 2);
						scale.x = x;

						// Y.
						configY += configHeight + 10f;
						GUI.Label(new Rect(configX, configY, configWidth, configHeight), "Scale Y:", labelStyle);
						configY += configHeight;
						float y = GUI.HorizontalSlider(new Rect(configX, configY, configWidth, configHeight), scale.y, 0.1f, 10f);
						y = (float)Math.Round(y, 2);
						configY += configHeight;
						y = (float)Math.Round(double.Parse(GUI.TextField(new Rect(configX, configY, configWidth, configHeight), y.ToString(), labelStyle)), 2);
						scale.y = y;

						// Z.
						configY += configHeight + 10f;
						GUI.Label(new Rect(configX, configY, configWidth, configHeight), "Scale Z:", labelStyle);
						configY += configHeight;
						float z = GUI.HorizontalSlider(new Rect(configX, configY, configWidth, configHeight), scale.z, 0.1f, 10f);
						z = (float)Math.Round(z, 2);
						configY += configHeight;
						z = (float)Math.Round(double.Parse(GUI.TextField(new Rect(configX, configY, configWidth, configHeight), z.ToString(), labelStyle)), 2);
						scale.z = z;
					}

					GUI.EndScrollView();
					break;
			}
		}

		/// <summary>
		/// Load all vehicles and generate thumbnails
		/// </summary>
		/// <returns>List of vehicles</returns>
		public List<Vehicle> LoadVehicles()
		{
			List<Vehicle> vehicles = new List<Vehicle>();
			foreach (GameObject gameObject in itemdatabase.d.items)
			{
				try
				{
					if (utility.IsVehicleOrTrailer(gameObject))
					{
						// Check for variants.
						randomTypeSelector randoms = gameObject.GetComponent<randomTypeSelector>();
						if (randoms != null && randoms.tipusok.Length > 0)
						{
							int variants = randoms.tipusok.Length;

							for (int i = 0; i < variants; i++)
							{
								Vehicle vehicle = new Vehicle()
								{
									vehicle = gameObject,
									variant = i + 1,
									thumbnail = thumbnailGenerator.GetThumbnail(gameObject, i + 2), // I have no idea why +1 produces the wrong variant in the thumbnail.
									name = translator.T(gameObject.name, "vehicle", i + 1),
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
								thumbnail = thumbnailGenerator.GetThumbnail(gameObject),
								name = translator.T(gameObject.name, "vehicle", -1),
							};
							vehicles.Add(vehicle);
						}
					}
				}
				catch
				{
					logger.Log($"Something went wrong loading vehicle {gameObject.name}", Logger.LogLevel.Error);
				}
			}

			return vehicles;
		}

		public List<POI> LoadPOIs()
		{
			List<POI> POIs = new List<POI>();

			try
			{
				foreach (GameObject POI in itemdatabase.d.buildings)
				{
					if (POI.name == "ErrorPrefab" || POI.name == "Falu01") continue;
					
					// TODO: Some building thumbnails are a bit fucked.
					POIs.Add(new POI()
					{
						poi = POI,
						thumbnail = thumbnailGenerator.GetThumbnail(POI, POI: true),
						name = translator.T(POI.name, "POI"),
					});
				}
			}
			catch (Exception ex)
			{
				logger.Log($"Error loading POIs - {ex}", Logger.LogLevel.Error);
			}

			return POIs;
		}

		/// <summary>
		/// Translate a string appropriate for the selected accessibility mode
		/// </summary>
		/// <param name="str">The string to translate</param>
		/// <param name="state">The button state</param>
		/// <returns>Accessibility mode translated string</returns>
		private string GetAccessibleString(string str, bool state)
		{
			switch (accessibilityMode)
			{
				case "contrast":
					return state ? $"<color=#0F0>{str}</color>" : $"<color=#FFF>{str}</color>";
				case "colourless":
					return state ? $"{str} ✔" : $"{str} ✖";
			}

			return state ? $"<color=#0F0>{str}</color>" : $"<color=#F00>{str}</color>";
		}

		/// <summary>
		/// Translate a string appropriate for the selected accessibility mode
		/// </summary>
		/// <param name="trueStr">The string to translate for true</param>
		/// <param name="falseStr">The string to translate for false</param>
		/// <param name="state">The button state</param>
		/// <returns>Accessibility mode translated string</returns>
		private string GetAccessibleString(string trueStr, string falseStr, bool state)
		{
			switch (accessibilityMode)
			{
				case "contrast":
					return state ? $"<color=#0F0>{trueStr}</color>" : $"<color=#FFF>{falseStr}</color>";
				case "colourless":
					return state ? $"{trueStr} ✔" : $"{falseStr} ✖";
			}

			return state ? $"<color=#0F0>{trueStr}</color>" : $"<color=#F00>{falseStr}</color>";
		}

		/// <summary> 
		/// Legacy main menu GUI.
		/// </summary>
		private void MainMenuLegacy()
		{
			float x = legacyMainMenuX;
			float y = legacyMainMenuY;
			float width = legacyMainMenuWidth;
			float height = legacyMainMenuHeight;

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
					utility.Spawn(new Item()
					{
						item = spawn.gameObject,
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
				string name = translator.T(gameObject.name, "vehicle", vehicle.variant);

				if (GUI.Button(new Rect(x, scrollY, width, buttonHeight), name))
				{
					utility.Spawn(new Vehicle()
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
		private void VehicleMenuLegacy()
		{

			float x = legacyVehicleMenuX;
			float y = legacyVehicleMenuY;
			float width = legacyVehicleMenuWidth;
			float height = legacyVehicleMenuHeight;

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
			int maxCondition = (int)Enum.GetValues(typeof(Item.Condition)).Cast<Item.Condition>().Max();
			float rawCondition = GUI.HorizontalSlider(new Rect(sliderX, sliderY, sliderWidth, sliderHeight), conditionInt, -1, maxCondition);
			conditionInt = Mathf.RoundToInt(rawCondition);

			string conditionName = ((Item.Condition)conditionInt).ToString();

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
		private void MiscMenuLegacy()
		{
			float x = legacyVehicleMenuX;
			float y = legacyVehicleMenuY + legacyVehicleMenuHeight + 25f;
			float width = legacyVehicleMenuWidth;
			float height = legacyVehicleMenuHeight;

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

			// TODO: UFO doesn't spawn on main branch, FEDOSPAWN.prefab isn't set.
			GUI.Label(new Rect(x + 10f, sliderY - 2.5f, textWidth, sliderHeight), "UFO (Beta branch only):", labelStyle);

			if (GUI.Button(new Rect(sliderX, sliderY - 2.5f, textWidth * 2f, sliderHeight), "Spawn UFO"))
			{
				try
				{
					// Destroy existing UFO.
					if (ufo != null)
						UnityEngine.Object.Destroy(ufo);

					ufo = UnityEngine.Object.Instantiate(temp.FEDOSPAWN.prefab, mainscript.M.player.lookPoint + Vector3.up * 0.75f, Quaternion.FromToRotation(Vector3.forward, -mainscript.M.player.transform.right));
					fedoscript ufoScript = ufo.GetComponent<fedoscript>();
					ufoScript.ai = false;
					ufoScript.followRoad = false;
				}
				catch (Exception ex)
				{
					logger.Log($"Failed to spawn UFO - {ex}", Logger.LogLevel.Error);
				}
			}

			if (GUI.Button(new Rect(sliderX + textWidth * 2 + 5f, sliderY - 2.5f, textWidth * 2f, sliderHeight), "Remove UFO"))
			{
				if (ufo != null)
					UnityEngine.Object.Destroy(ufo);
			}
		}

		private void ItemsMenuLegacy()
		{
			float x = legacyMainMenuX + legacyMainMenuWidth + 15f;
			float y = legacyMainMenuY;
			float width = resolutionX / 1.75f;
			float height = legacyMainMenuHeight;

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
					utility.Spawn(currentItem.Clone());
				}
			}

			GUI.EndScrollView();
		}
	}
}
