using MultiTool.Database;
using MultiTool.Extensions;
using MultiTool.Save;
using MultiTool.Services;
using MultiTool.UI.Tabs.VehicleConfiguration;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TLDLoader;
using UnityEngine;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.UI
{
	internal class GUIRenderer
	{
		// Modules.
		private static ServiceContext _services;
		internal static TabController Tabs;

		public GUIRenderer(ServiceContext services)
		{
			_services = services;
			Tabs = new TabController(services);
		}

		// Menu control.
		internal bool Show = false;
		private bool _menuKeyConsumed = false;
		public static event Action<bool> OnMenuToggle;
		private bool _loaded = false;
		internal string SettingsTabId = null;
		internal string CreditsTabId = null;
		internal string ThemeTabId = null;
		internal string DebugTabId = null;

		internal int ResolutionX;
		internal int ResolutionY;

		internal float MainMenuWidth;
		internal float MainMenuHeight;
		internal float MainMenuX;
		internal float MainMenuY;

		// Styling.
		internal static GUIStyle LabelStyle = new GUIStyle();
		internal static float ScrollWidth = 10f;

		// Vehicle-related variables.
		internal static List<Vehicle> Vehicles = new List<Vehicle>();
		internal static int ConditionInt = 0;
		internal static bool ApplyConditionToAttached = false;

		// Item menu variables.
		internal static List<Database.Item> Items = new List<Database.Item>();
		internal static Dictionary<string, List<Type>> Categories = new Dictionary<string, List<Type>>()
		{
			{ "Vehicle chassis", new List<Type>() { typeof(carscript) } },
			{ "Trailers", new List<Type>() { typeof(utanfutoscript) } },
			{ "Tanks", new List<Type>() { typeof(tankscript) } },
			{ "Lights", new List<Type>() { typeof(headlightscript) } },
			{ "Engines", new List<Type>() { typeof(enginescript) } },
			{ "Wheels", new List<Type>() { typeof(wheelscript) } },
			{ "Tires", new List<Type>() { typeof(gumiscript) } },
			{ "Dials", new List<Type>() { typeof(meterscript) } },
			{ "Attachables", new List<Type>() { typeof(attachablescript) } },
			{ "Other vehicle parts", new List<Type>() { typeof(attachablescript) } },
			{ "Guns", new List<Type>() { typeof(weaponscript) } },
			{ "Melee weapons", new List<Type>() { typeof(meleeweaponscript) } },
			{ "Cleaning", new List<Type>() { typeof(drotkefescript), typeof(spricniscript) } },
			{ "Refillables", new List<Type>() { typeof(ammoscript) } },
			{ "Food", new List<Type>() { typeof(ediblescript) } },
			{ "Wearables", new List<Type>() { typeof(wearable) } },
			{ "Usables", new List<Type>() { typeof(pickupable) } },
			{ "Mod items", new List<Type>() { typeof(tosaveitemscript) } },
			{ "Other", new List<Type>() { typeof(MonoBehaviour) } },
		};

		internal static List<GameObject> SpawnedObjects = new List<GameObject>();

		// POI variables.
		internal static List<POI> POIs = new List<POI>();

		// Player variables.
		internal static Dictionary<mainscript.fluidenum, int> Piss = new Dictionary<mainscript.fluidenum, int>();

		// Vehicle configuration variables.
		internal static List<FluidPercentage> FluidDefaults = new List<FluidPercentage>();
		internal static Dictionary<mainscript.fluidenum, int> Coolants = new Dictionary<mainscript.fluidenum, int>();
		internal static Dictionary<mainscript.fluidenum, int> Oils = new Dictionary<mainscript.fluidenum, int>();
		internal static Dictionary<mainscript.fluidenum, int> Fuels = new Dictionary<mainscript.fluidenum, int>();

		// Settings.
		internal static float SettingsScrollWidth;
		internal static bool AccessibilityShow = false;
		internal static float NoclipFastMoveFactor = 10f;

		// Colour palettes.
		internal static List<Color> Palette = new List<Color>();
		private static Dictionary<int, GUIStyle> _paletteCache = new Dictionary<int, GUIStyle>();

		// Main menu variables.
		private bool _mainMenuLoaded = false;
		private bool _stateChanged = false;
		private Vector2 _currentMainMenuPosition;
		private static string[] _mainMenuStages = new string[] { "distance", "vehicle", "basics", "color" };
		private string _mainMenuStage = _mainMenuStages[1];
		private Color? _startVehicleColor = null;
		private int _startVehicleCondition = -1;
		private string _startVehiclePlate = string.Empty;
		private bool _appliedStartVehicleChanges = false;
		private string[] _largeVehicles = new string[]
		{
			"bus01",
			"bus02",
			"bus03",
			"car07",
			"car09T",
			"car11",
		};
		private string[] _bikes = new string[]
		{
			"bike01",
			"bike03",
		};
		private float _distanceDriven;

		internal void OnGUI()
		{
			Styling.Bootstrap();
			GUI.skin = Styling.GetActiveSkin();

			// Find screen resolution.
			ResolutionX = Screen.width;
			ResolutionY = Screen.height;
			int resX = settingsscript.s.S.IResolutionX;
			int resY = settingsscript.s.S.IResolutionY;
			if (resX != ResolutionX)
			{
				ResolutionX = resX;
				ResolutionY = resY;

				MainMenuWidth = ResolutionX - 80f;
				MainMenuHeight = ResolutionY - 80f;
				MainMenuX = 40f;
				MainMenuY = 40f;
			}

			// In game.
			if (mainscript.M != null)
			{
				if (_loaded)
				{
					if (!Show && !mainscript.M.menu.Menu.activeSelf)
						MultiTool.Tools.RenderHud();

					else if (!Show && mainscript.M.menu.Menu.activeSelf)
						RenderPauseMenu();

					else if (Show)
					{
						// Override to allow menu to close with text input focused.
						Event e = Event.current;
						if (e.type == EventType.KeyDown && e.keyCode == MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.menu).AssignedKey)
						{
							ToggleMenu();
							e.Use();
							_menuKeyConsumed = true;
						}

						MainMenu();
					}
				}
			}
			// Main menu.
			else
			{
				GameMainMenuUI();
			}

			// Render notifications last to ensure they show above anything else.
			Notifications.Render();

			// Reset back to default Unity skin to avoid styling bleeding to other UI mods.
			GUI.skin = null;
		}

		internal void OnLoad()
		{
			try
			{
				_loaded = false;

				// Unregister tabs so they re-register correctly if already loaded.
				if (Tabs.GetCount() > 0)
					Tabs.UnregisterAll();

				// Ensure UI loads hidden.
				Show = false;

				ResolutionX = settingsscript.s.S.IResolutionX;
				ResolutionY = settingsscript.s.S.IResolutionY;

				MainMenuWidth = ResolutionX - 80f;
				MainMenuHeight = ResolutionY - 80f;
				MainMenuX = 40f;
				MainMenuY = 40f;

				// Add default navigation tabs.
				Tabs.AddTab(new Tabs.VehiclesTab());
				Tabs.AddTab(new Tabs.ItemsTab());
				Tabs.AddTab(new Tabs.POIsTab());
				Tabs.AddTab(new Tabs.ShapesTab());
				Tabs.AddTab(new Tabs.PlayerTab());
				Tabs.AddTab(new Tabs.VehicleConfiguration.VehicleConfigurationTab());
				Tabs.AddTab(new Tabs.SandboxTab());
				Tabs.AddTab(new Tabs.DeveloperTab());
				Tabs.AddTab(new Tabs.ComponentBrowser.BrowserTab());

				// Add default hidden tabs.
				SettingsTabId = Tabs.AddTab(new Tabs.SettingsTab());
				CreditsTabId = Tabs.AddTab(new Tabs.CreditsTab());
				ThemeTabId = Tabs.AddTab(new Tabs.ThemeTab());
				DebugTabId = Tabs.AddTab(new Tabs.DebugTab());

				// Load data from database.
				DatabaseUtilities.ClearCaches();
				Vehicles = DatabaseUtilities.LoadVehicles();
				Items = DatabaseUtilities.LoadItems();
				POIs = DatabaseUtilities.LoadPOIs();

				// Load save data.
				SaveUtilities.LoadSaveData();

				// Attach any components to database objects.
				foreach (GameObject obj in itemdatabase.d.items)
				{
					if (obj.GetComponent<SaveDataLoader>() == null)
						obj.AddComponent<SaveDataLoader>();
				}

				// Clear any existing static values.
				FluidDefaults.Clear();
				Coolants.Clear();
				Oils.Clear();
				Fuels.Clear();
				Piss.Clear();

				// Prepopulate any variables that use the fluidenum.
				int maxFuelType = (int)Enum.GetValues(typeof(mainscript.fluidenum)).Cast<mainscript.fluidenum>().Max();
				for (int i = 0; i <= maxFuelType; i++)
				{
					FluidDefaults.Add(new FluidPercentage() { Type = (mainscript.fluidenum)i, Percentage = 0 });
					Coolants.Add((mainscript.fluidenum)i, 0);
					Oils.Add((mainscript.fluidenum)i, 0);
					Fuels.Add((mainscript.fluidenum)i, 0);
					Piss.Add((mainscript.fluidenum)i, 0);
				}

				// Load any configs not loaded on the main menu.
				try
				{
					SettingsScrollWidth = ScrollWidth;
					NoclipFastMoveFactor = MultiTool.Configuration.Config.NoclipFastMoveFactor;
				}
				catch (Exception ex)
				{
					Logger.Log($"Config load error - {ex}", Logger.LogLevel.Error);
				}

				// Load keybinds.
				MultiTool.Binds.OnLoad();
			}
			catch (Exception ex)
			{
				Logger.Log($"Error during OnLoad() - {ex}", Logger.LogLevel.Critical);
				Notifications.Send(MultiTool.ModInstance.Name, "Critical error occurred. Please report to M-.");
			}

			_loaded = true;
		}

		internal void OnMenuLoad()
		{
			Show = false;
			_loaded = false;
			_mainMenuLoaded = false;
		}

		internal void Update()
		{
			Notifications.Update();

			if (MultiTool.IsOnMainMenu)
			{
				MainMenuUpdate();
				return;
			}

			if (!_loaded) return;

			// Trigger update for tabs and notifications.
			Tabs.Update();

			// Remove any null objects from the spawn history.
			foreach (GameObject spawned in SpawnedObjects)
			{
				if (spawned == null)
				{
					SpawnedObjects.Remove(spawned);
					break;
				}
			}

			if (!_menuKeyConsumed && Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.menu).AssignedKey) && !mainscript.M.menu.Menu.activeSelf && !mainscript.M.settingsOpen && !mainscript.M.menu.saveScreen.gameObject.activeSelf)
				ToggleMenu();

			if (Show && !mainscript.M.menu.Menu.activeSelf && Input.GetButtonDown("Cancel"))
				ToggleMenu(false);

			_menuKeyConsumed = false;

			// Apply starter vehicle customisation here as OnLoad() is too early.
			// Other mods that potentially modify paintable parts won't have loaded yet.
			if (!_appliedStartVehicleChanges && _loaded && !ModLoader.loading.activeSelf)
			{
				try
				{
					// Don't apply any new game changes when loading a save.
					if (mainscript.M.menu.DFMS.load)
					{
						_appliedStartVehicleChanges = true;
						return;
					}

					GameObject starterVehicle = null;
					string starterVehicleName = mainscript.M.StartCar.ToString();
					bool isLargeVehicle = _largeVehicles.Contains(starterVehicleName);
					bool isBike = _bikes.Contains(starterVehicleName);
					foreach (carscript car in mainscript.M.Cars)
					{
						if ((isLargeVehicle || isBike) && !car.name.ToLower().Contains("bike"))
						{
							// Attempt to find the position of the starter car to override the vehicle.
							starterVehicle = car.gameObject;
						}
						else if (car.name.ToLower().Contains(starterVehicleName.ToLower()))
						{
							// Find the selected starter car object.
							starterVehicle = car.gameObject;
						}
					}

					if (starterVehicle == null) return;

					GameObject finalStarterVehicle = null;

					if (isBike || isLargeVehicle)
					{
						// Store the position and rotation to keep the bikes at the original spawn position.
						Vector3 position = starterVehicle.transform.position;
						Quaternion rotation = starterVehicle.transform.rotation;

						if (isLargeVehicle)
						{
							position = starterVehicle.transform.position + (Vector3.left * 15f) + (Vector3.up * 5f);
							rotation = starterVehicle.transform.rotation * Quaternion.AngleAxis(-90, Vector3.up);
						}

						Color color = starterVehicle.GetComponent<partconditionscript>().color;
						if (_startVehicleColor.HasValue)
							color = _startVehicleColor.Value;

						// Destroying the actual starter car doesn't want to cooperate
						// so drop it out the map instead.
						UnityEngine.Object.Destroy(starterVehicle.gameObject);
						starterVehicle.transform.position += Vector3.down * 15f;

						Vehicle vehicle = Vehicles.Where(v => v.GameObject.name.ToLower().Contains(starterVehicleName.ToLower())).FirstOrDefault();
						if (vehicle != null)
						{
							finalStarterVehicle = SpawnUtilities.Spawn(vehicle.GameObject, color, _startVehicleCondition, -1, position, rotation);
						}
					}
					else
					{
						finalStarterVehicle = starterVehicle;

						// Set starter vehicle colour.
						if (_startVehicleColor.HasValue)
						{
							partconditionscript partconditionscript = finalStarterVehicle.GetComponent<partconditionscript>();
							GameUtilities.Paint(_startVehicleColor.Value, partconditionscript);
						}

						// Set starter vehicle condition.
						if (_startVehicleCondition != -1)
						{
							partconditionscript partconditionscript = finalStarterVehicle.GetComponent<partconditionscript>();
							List<partconditionscript> children = GameUtilities.FindPartChildren(partconditionscript);

							foreach (partconditionscript child in children)
							{
								child.state = _startVehicleCondition;
								child.Refresh();
							}
						}
					}

					// Set starter vehicle plate.
					if (_startVehiclePlate != string.Empty)
					{
						rendszamscript[] plateScripts = finalStarterVehicle.GetComponentsInChildren<rendszamscript>();
						foreach (rendszamscript plateScript in plateScripts)
						{
							if (plateScript == null)
								continue;

							plateScript.Same(_startVehiclePlate);
						}
					}

					_appliedStartVehicleChanges = true;
				}
				catch (Exception ex)
				{
					Logger.Log($"Error occurred during starter vehicle configuration - {ex}");
					_appliedStartVehicleChanges = true;
				}
			}
		}

		internal void FixedUpdate()
		{
			Tabs.FixedUpdate();
		}

		private void ToggleMenu(bool? force = null)
		{
			if (force.HasValue)
				Show = force.Value;
			else
				Show = !Show;

			OnMenuToggle?.Invoke(Show);

			mainscript.M.crsrLocked = !Show;
			mainscript.M.SetCursorVisible(Show);
			mainscript.M.menu.gameObject.SetActive(!Show);
			GUI.FocusControl(null);
			_menuKeyConsumed = false;
		}

		/// <summary>
		/// Separate update function for the main menu.
		/// </summary>
		private void MainMenuUpdate()
		{
			// Use the first run of Update() to get any variables we need
			// as OnMenuLoad() is called before anything is started.
			if (!_mainMenuLoaded)
			{
				ResolutionX = settingsscript.s.S.IResolutionX;
				ResolutionY = settingsscript.s.S.IResolutionY;

				// Default language to English until we can pull it from mainscript.
				Translator.SetLanguage("English");

				// Set label styling.
				LabelStyle.alignment = TextAnchor.UpperLeft;
				LabelStyle.normal.textColor = Color.white;

				// Set default palette to all white.
				Palette.Clear();
				Palette = Enumerable.Repeat(Color.white, 60).ToList();
				_paletteCache.Clear();

				// Load any configs needed for the main menu UI.
				try
				{
					ScrollWidth = MultiTool.Configuration.Config.ScrollWidth;
					Palette = MultiTool.Configuration.Config.Palette;
				}
				catch (Exception ex)
				{
					Logger.Log($"Config load error - {ex}", Logger.LogLevel.Error);
				}

				_mainMenuLoaded = true;
			}

			if (_stateChanged)
			{
				string[] toggles = new string[] { "ButtonLoad", "ButtonSettings", "ButtonExit", "ButtonDiscord", "ButtonNews" };
				foreach (string toggle in toggles)
				{
					mainmenuscript.mainmenu.Canvas.Find($"GameObject/MainStuff/{toggle}").gameObject.SetActive(!Show);
				}

				_stateChanged = false;
			}
		}

		/// <summary>
		/// Show menu toggle button.
		/// </summary>
		private void RenderPauseMenu()
		{
			MultiTool.Binds.RenderRebindMenu("M-ultiTool menu key", new int[] { (int)Keybinds.Inputs.menu }, ResolutionX - 350f, 50f, 300f, 100f);
		}

		/// <summary>
		/// Main menu GUI.
		/// </summary>
		private void MainMenu()
		{
			float x = MainMenuX;
			float y = MainMenuY;
			float width = MainMenuWidth;
			float height = MainMenuHeight;

			GUILayout.BeginArea(new Rect(x, y, width, height), $"<color=#f87ffa><size=18><b>{MultiTool.ModInstance.Name}</b></size>\n<size=16>v{MultiTool.ModInstance.Version} - made with ❤️ by M-</size></color>", "box");
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button(Accessibility.GetAccessibleString("Settings", Tabs.GetActive() == SettingsTabId), "ButtonSecondary", GUILayout.MinHeight(30)))
			{
				Tabs.ToggleActive(SettingsTabId);
			}

			GUILayout.Space(10);

			if (GUILayout.Button(Accessibility.GetAccessibleString("Credits", Tabs.GetActive() == CreditsTabId), "ButtonSecondary", GUILayout.MinHeight(30)))
			{
				Tabs.ToggleActive(CreditsTabId);
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(20);

			// Render navigation bar.
			if (Tabs.GetActive() == null || !Tabs.GetById(Tabs.GetActive()).IsFullScreen)
			{
				GUILayout.BeginHorizontal();
				for (int tabIndex = 0; tabIndex < Tabs.GetCount(); tabIndex++)
				{
					Tab tab = Tabs.GetByIndex(tabIndex);

					// Ignore any tabs excluded from navigation.
					if (!tab.ShowInNavigation) continue;

					// Render disabled tabs as unclickable.
					if (tab.IsDisabled)
						GUI.enabled = false;

					if (GUILayout.Button(Accessibility.GetAccessibleString(tab.Name, Tabs.GetActive() == tab.Id, true), GUILayout.MinWidth(60), GUILayout.MaxHeight(30)))
					{
						Tabs.SetActive(tab.Id);
					}

					GUI.enabled = true;
				}
				GUILayout.EndHorizontal();
			}

			// Render the active tab.
			Tabs.RenderTab();

			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		/// <summary>
		/// Render game main menu UI.
		/// </summary>
		private void GameMainMenuUI()
		{
			float width = ResolutionX / 3;
			float height = ResolutionY - 200f;
			float x = ResolutionX - ResolutionX / 3;
			float y = 100f;

			// Don't render the UI if any game menus are open.
			if (mainmenuscript.mainmenu.SettingsScreenObj.activeSelf || mainmenuscript.mainmenu.SaveScreenObj.activeSelf) return;

			if (!Show)
			{
				GUILayout.BeginArea(new Rect(ResolutionX - 200f, ResolutionY / 3 - 10f, 200f, 60f));
				if (GUILayout.Button("M-ultiTool", "ButtonBlackTranslucent", GUILayout.MinHeight(60)))
				{
					Show = true;
					_stateChanged = true;
				}
				GUILayout.EndArea();
			}

			if (!Show)
				return;

			GUILayout.BeginArea(new Rect(x, y, width, height), $"<color=#f87ffa><size=18><b>{(_mainMenuStage == "distance" ? "Change distance driven" : "New game settings")}</b></size></color>", "box");
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("<size=30><color=#F00>X</color></size>", "ButtonBlack", GUILayout.MinWidth(40f)))
			{
				Show = false;
				_stateChanged = true;
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginVertical(GUILayout.MaxWidth(width - 15f));
			_currentMainMenuPosition = GUILayout.BeginScrollView(_currentMainMenuPosition);
			switch (_mainMenuStage)
			{
				case "distance":
					float.TryParse(GUILayout.TextField(_distanceDriven.ToString()), out _distanceDriven);
					GUILayout.BeginHorizontal();
					if (GUILayout.Button("Get distance", GUILayout.MaxWidth(200)))
						_distanceDriven = mainscript.DistanceRead();

					GUILayout.Space(10);

					if (GUILayout.Button("<color=#F00>Set distance</color>", "ButtonBlack", GUILayout.MaxWidth(200)))
					{
						PlayerPrefs.SetFloat("DistanceDriven", _distanceDriven);
						mainmenuscript.mainmenu.Refresh();
					}
					GUILayout.EndHorizontal();
					break;

				case "vehicle":
					int optionCount = (int)Enum.GetValues(typeof(itemdatabase.CarType)).Cast<itemdatabase.CarType>().Max();
					foreach (object car in Enum.GetValues(typeof(itemdatabase.CarType)))
					{
						string name = Translator.T(car.ToString(), "menuVehicles");

						if (GUILayout.Button(Accessibility.GetAccessibleString(name, mainmenuscript.mainmenu.DFMS.startcar == (itemdatabase.CarType)car)))
							mainmenuscript.mainmenu.DFMS.startcar = (itemdatabase.CarType)car;
					}
					break;

				case "basics":
					// Condition.
					GUILayout.Label($"Condition: {(Database.Item.Condition)_startVehicleCondition}");
					int maxCondition = (int)Enum.GetValues(typeof(Database.Item.Condition)).Cast<Database.Item.Condition>().Max();
					float rawCondition = GUILayout.HorizontalSlider(_startVehicleCondition, -1, maxCondition);
					_startVehicleCondition = Mathf.RoundToInt(rawCondition);

					GUILayout.Space(10);

					// License plate.
					GUILayout.Label("Plate (blank for random):");
					_startVehiclePlate = GUILayout.TextField(_startVehiclePlate, 7);
					break;

				case "color":
					if (GUILayout.Button($"Using {(_startVehicleColor.HasValue ? "custom" : "random")} colour"))
					{
						if (_startVehicleColor.HasValue)
							_startVehicleColor = null;
						else
							_startVehicleColor = Color.white;
					}
					GUILayout.Space(10);

					if (_startVehicleColor.HasValue)
						_startVehicleColor = Colour.RenderColourSliders(width - 20f, _startVehicleColor.Value);
					break;
			}
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			// Back button.
			if (_mainMenuStage != _mainMenuStages.First())
			{
				string previousStage = _mainMenuStages[Array.FindIndex(_mainMenuStages, s => s == _mainMenuStage) - 1];
				if (GUILayout.Button($"To {previousStage}", GUILayout.MinWidth(200), GUILayout.Height(20)))
				{
					_mainMenuStage = previousStage;
					_currentMainMenuPosition = Vector2.zero;
				}
			}

			GUILayout.FlexibleSpace();

			// Next button.
			if (_mainMenuStage != _mainMenuStages.Last())
			{
				string nextStage = _mainMenuStages[Array.FindIndex(_mainMenuStages, s => s == _mainMenuStage) + 1];
				if (GUILayout.Button($"To {nextStage}", GUILayout.MinWidth(200), GUILayout.Height(20)))
				{
					_mainMenuStage = nextStage;
					_currentMainMenuPosition = Vector2.zero;
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}
	}
}
