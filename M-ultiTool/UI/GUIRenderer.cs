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
using UnityEngine.Rendering;
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
		private GUIStyle _hudStyle = new GUIStyle()
		{
			fontSize = 20,
			alignment = TextAnchor.MiddleLeft,
			normal = new GUIStyleState()
			{
				textColor = Color.white,
			}
		};

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

		// Slot mover variables.
		internal static GameObject SelectedSlot;
		internal static GameObject HoveredSlot;
		private static int _hoveredSlotIndex = 0;
		private static int _previousHoveredSlotIndex = 0;
		private static bool _slotMoverFirstRun = true;
		private static Vector3 _selectedSlotResetPosition;
		private static Quaternion _selectedSlotResetRotation;
		private float[] _moveOptions = new float[] { 10f, 1f, 0.1f, 0.01f, 0.001f };
		private float _moveValue = 0.1f;
		private static List<GameObject> _slots = new List<GameObject>();

		// Settings.
		internal static float SettingsScrollWidth;
		internal static bool AccessibilityShow = false;
		internal static float NoclipFastMoveFactor = 10f;

		// HUD variables.
		private GameObject _debugObject = null;

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
					{
						RenderHUD();
						MultiTool.Tools.RenderHud();
					}

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

			// Detect item when item debugging is enabled.
			if (_services.State.ObjectDebug)
			{
				try
				{
					GameObject foundObject = null;
					// Find object the player is looking at.
					Physics.Raycast(mainscript.M.player.Cam.transform.position, mainscript.M.player.Cam.transform.forward, out RaycastHit raycastHit, float.PositiveInfinity, mainscript.M.player.useLayer);

					tosaveitemscript save = raycastHit.transform.gameObject.GetComponent<tosaveitemscript>();
					if (save != null)
					{
						foundObject = raycastHit.transform.gameObject;
					}

					// Debug picked up if player is holding something.
					if (mainscript.M.player.pickedUp != null)
						foundObject = mainscript.M.player.pickedUp.gameObject;

					// Debug held item if something is equipped.
					if (mainscript.M.player.inHandP != null)
						foundObject = mainscript.M.player.inHandP.gameObject;

					_debugObject = foundObject;
				}
				catch (Exception ex)
				{
					Logger.Log($"Error determining debug object - {ex}", Logger.LogLevel.Error);
				}
			}

			if (_services.State.Mode == "slotControl")
			{
				try
				{
					// Unset slotControl mode when exiting a vehicle.
					if (mainscript.M.player.Car == null)
					{
						SlotMoverDispose();
					}
					else if (_slots.Count == 0)
					{
						partslotscript[] partSlots = _services.State.Car.GetComponentsInChildren<partslotscript>();
						foreach (partslotscript slot in partSlots)
						{
							GameObject obj = slot.gameObject;

							// Required as some slots don't have the actual part as a
							// child of the slot. These parts instead use a collider
							// which will either contain Col or Collider, so look for
							// either and use the parent instead.
							if (slot.name.Contains("Col"))
							{
								obj = slot.transform.parent.gameObject;
							}

							_slots.Add(obj);
						}

						// Find anything that isn't an actual part.
						foreach (MeshRenderer child in _services.State.Car.GetComponentsInChildren<MeshRenderer>())
						{
							string name = PrettifySlotName(child.name).ToLower();
							GameObject parent = child.transform.parent.gameObject;
							string parentName = PrettifySlotName(parent.name).ToLower();
							string[] mufflers = new string[]
							{
								"muffler",
								"exhaust",
							};

							string[] parentNames = new string[]
							{
								"interiorlight",
								"plate",
							};

							foreach (string muffler in mufflers)
							{
								if ((name.Contains(muffler) || parentName.Contains(muffler)) && child.gameObject.activeSelf)
								{
									_slots.Add(child.gameObject);
								}
							}

							foreach (string parentSlotName in parentNames)
							{
								if (parentName.Contains(parentSlotName) && parent.activeSelf)
								{
									_slots.Add(parent);
								}
							}
						}

						// Add seat positions.
						foreach (seatscript seat in _services.State.Car.GetComponentsInChildren<seatscript>())
						{
							if (seat.GetComponent<BoxCollider>() == null || seat.name.ToLower().Contains("col")) continue;
							_slots.Add(seat.gameObject);
						}
					}

					tosaveitemscript carSave = _services.State.Car.GetComponent<tosaveitemscript>();

					switch (_services.State.SlotStage)
					{
						case "slotSelect":
							bool slotChanged = false;

							// Render collider on first load.
							if (_slotMoverFirstRun)
							{
								slotChanged = true;
								HoveredSlot = _slots[_hoveredSlotIndex];
							}

							// Move selector left.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.left).AssignedKey))
							{
								_previousHoveredSlotIndex = _hoveredSlotIndex;
								_hoveredSlotIndex--;
								if (_hoveredSlotIndex < 0)
									_hoveredSlotIndex = _slots.Count - 1;

								HoveredSlot = _slots[_hoveredSlotIndex];
								slotChanged = true;
							}

							// Move selector right.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.right).AssignedKey))
							{
								_previousHoveredSlotIndex = _hoveredSlotIndex;
								_hoveredSlotIndex++;
								if (_hoveredSlotIndex >= _slots.Count)
									_hoveredSlotIndex = 0;

								HoveredSlot = _slots[_hoveredSlotIndex];
								slotChanged = true;
							}

							// Select the hovered slot.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.select).AssignedKey))
							{
								_services.State.SlotStage = "move";
								SelectedSlot = HoveredSlot;

								_selectedSlotResetPosition = SelectedSlot.transform.localPosition;
								_selectedSlotResetRotation = SelectedSlot.transform.localRotation;

								// Get reset positions from save data.
								SlotData slotData = SaveUtilities.GetSlotData(carSave.idInSave, SelectedSlot.name);
								if (slotData != null)
								{
									_selectedSlotResetPosition = slotData.ResetPosition;
									_selectedSlotResetRotation = slotData.ResetRotation;
								}

								SlotMoverSelectDispose();

								ObjectUtilities.ShowColliders(SelectedSlot, Color.blue);
							}

							if (slotChanged)
							{
								ObjectUtilities.ShowColliders(HoveredSlot, Color.red);

								if (!_slotMoverFirstRun)
								{
									GameObject previousSlot = _slots[_previousHoveredSlotIndex];

									ObjectUtilities.DestroyColliders(previousSlot);
								}
								_slotMoverFirstRun = false;
							}
							break;
						case "move":
							// Deselect slot.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.select).AssignedKey))
							{
								_services.State.SlotStage = "slotSelect";
								_hoveredSlotIndex = Array.FindIndex(_slots.ToArray(), s => s.name == SelectedSlot.name);
								SlotMoverMoveDispose();
								return;
							}

							// Switch to rotate mode.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.action3).AssignedKey))
							{
								_services.State.SlotStage = "rotate";
							}

							// Change move amount.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.action5).AssignedKey))
							{
								int currentIndex = Array.FindIndex(_moveOptions, s => s == _moveValue);
								if (currentIndex == -1 || currentIndex == _moveOptions.Length - 1)
									_moveValue = _moveOptions[0];
								else
									_moveValue = _moveOptions[currentIndex + 1];
							}

							Transform partTransform = SelectedSlot.transform;
							Vector3 oldPos = partTransform.localPosition;

							// Move forward.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.up).AssignedKey))
							{
								partTransform.localPosition += Vector3.forward * _moveValue;
							}

							// Move backwards.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.down).AssignedKey))
							{
								partTransform.localPosition += Vector3.back * _moveValue;
							}

							// Move left.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.left).AssignedKey))
							{
								partTransform.localPosition += Vector3.left * _moveValue;
							}

							// Move right.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.right).AssignedKey))
							{
								partTransform.localPosition += Vector3.right * _moveValue;
							}

							// Move up.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.noclipSpeedUp).AssignedKey))
							{
								partTransform.localPosition += Vector3.up * _moveValue;
							}

							// Move down.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.noclipDown).AssignedKey))
							{
								partTransform.localPosition += Vector3.down * _moveValue;
							}

							// Reset position.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.action4).AssignedKey))
							{
								partTransform.localPosition = _selectedSlotResetPosition;
							}

							// Check if position has changed.
							if (oldPos != partTransform.localPosition)
							{
								SlotData slotData = new SlotData()
								{
									ID = carSave.idInSave,
									Slot = SelectedSlot.name,
									Position = partTransform.localPosition,
									ResetPosition = _selectedSlotResetPosition,
									Rotation = partTransform.localRotation,
									ResetRotation = _selectedSlotResetRotation,
								};
								SaveUtilities.UpdateSlot(slotData);
							}

							break;
						case "rotate":
							// Deselect slot.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.select).AssignedKey))
							{
								_services.State.SlotStage = "slotSelect";
								_hoveredSlotIndex = Array.FindIndex(_slots.ToArray(), s => s.name == SelectedSlot.name);
								SlotMoverMoveDispose();
								return;
							}

							// Switch to move mode.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.action3).AssignedKey))
							{
								_services.State.SlotStage = "move";
							}

							// Change move amount.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.action5).AssignedKey))
							{
								int currentIndex = Array.FindIndex(_moveOptions, s => s == _moveValue);
								if (currentIndex == -1 || currentIndex == _moveOptions.Length - 1)
									_moveValue = _moveOptions[0];
								else
									_moveValue = _moveOptions[currentIndex + 1];
							}

							Transform rotatePartTransform = SelectedSlot.transform;
							Quaternion oldRot = rotatePartTransform.localRotation;

							// Rotate forward.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.up).AssignedKey))
							{
								rotatePartTransform.Rotate(Vector3.right, _moveValue);
							}

							// Rotate backwards.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.down).AssignedKey))
							{
								rotatePartTransform.Rotate(-Vector3.right, _moveValue);
							}

							// Rotate left.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.left).AssignedKey))
							{
								rotatePartTransform.Rotate(-Vector3.forward, _moveValue);
							}

							// Rotate right.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.right).AssignedKey))
							{
								rotatePartTransform.Rotate(Vector3.forward, _moveValue);
							}

							// Rotate anticlockwise.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.noclipSpeedUp).AssignedKey))
							{
								rotatePartTransform.Rotate(Vector3.up, _moveValue);
							}

							// Rotate clockwise.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.noclipDown).AssignedKey))
							{
								rotatePartTransform.Rotate(-Vector3.up, _moveValue);
							}

							// Reset position.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.action4).AssignedKey))
							{
								rotatePartTransform.localRotation = _selectedSlotResetRotation;
							}

							// Check if rotation has changed.
							if (oldRot != rotatePartTransform.localRotation)
							{
								SlotData slotData = new SlotData()
								{
									ID = carSave.idInSave,
									Slot = SelectedSlot.name,
									Position = rotatePartTransform.localPosition,
									ResetPosition = _selectedSlotResetPosition,
									Rotation = rotatePartTransform.localRotation,
									ResetRotation = _selectedSlotResetRotation,
								};
								SaveUtilities.UpdateSlot(slotData);
							}
							break;
					}
				}
				catch (Exception ex)
				{
					Logger.Log($"Error during slotControl - {ex}");
				}
			}

			// Logic for showing colliders.
			if (_services.State.ShowColliders)
			{
				RaycastHit hitInfo;
				if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.select).AssignedKey) && Physics.Raycast(mainscript.M.player.Cam.transform.position, mainscript.M.player.Cam.transform.forward, out hitInfo, float.PositiveInfinity, (int)mainscript.M.player.useLayer))
				{
					Mesh mesh = itemdatabase.d.gerror.GetComponentInChildren<MeshFilter>().mesh;
					Material source;
					try
					{
						source = new Material(Shader.Find("Standard"));
						source.SetOverrideTag("RenderType", "Transparent");
						source.SetFloat("_SrcBlend", (float)BlendMode.One);
						source.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
						source.SetFloat("_ZWrite", 0.0f);
						source.DisableKeyword("_ALPHATEST_ON");
						source.DisableKeyword("_ALPHABLEND_ON");
						source.EnableKeyword("_ALPHAPREMULTIPLY_ON");
					}
					catch
					{
						source = new Material(mainscript.M.conditionmaterials[0].New);
					}
					foreach (Collider componentsInChild in hitInfo.collider.transform.root.GetComponentsInChildren<Collider>())
					{
						string str = "TEMPORARY DISPLAY CUBE " + componentsInChild.GetInstanceID();
						if (componentsInChild.transform.Find(str) != null)
						{
							UnityEngine.Object.DestroyImmediate(componentsInChild.transform.Find(str).gameObject);
						}
						else
						{
							GameObject gameObject = new GameObject(str);
							gameObject.transform.SetParent(componentsInChild.transform, false);
							if (componentsInChild.GetType() == typeof(BoxCollider))
							{
								gameObject.transform.localPosition = ((BoxCollider)componentsInChild).center;
								gameObject.transform.localScale = ((BoxCollider)componentsInChild).size;
								gameObject.transform.localRotation = Quaternion.identity;
								// Get the mesh based on the cube primitive mesh.
								gameObject.AddComponent<MeshFilter>().mesh = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshFilter>().mesh;
							}
							else if (componentsInChild.GetType() == typeof(CapsuleCollider))
							{
								CapsuleCollider collider = (CapsuleCollider)componentsInChild;
								gameObject.transform.localPosition = collider.center;
								// I've got no idea if this is correct, sources for collider sizes are non existent.
								gameObject.transform.localScale = new Vector3(collider.radius * 2, collider.height / 2, collider.radius * 2);
								// There's fuck all logic here, it was entirely trial and error.
								Vector3 axis = Vector3.up;
								float angle = 0;
								switch (collider.direction)
								{
									case 1:
										axis = Vector3.forward;
										break;
									case 2:
										axis = Vector3.right;
										angle = 90;
										break;
								}
								gameObject.transform.localRotation = Quaternion.AngleAxis(angle, axis);
								// Get the mesh based on the capsule primitive mesh.
								gameObject.AddComponent<MeshFilter>().mesh = GameObject.CreatePrimitive(PrimitiveType.Capsule).GetComponent<MeshFilter>().mesh;
							}
							else if (componentsInChild.GetType() == typeof(MeshCollider))
							{
								gameObject.transform.localEulerAngles = gameObject.transform.localPosition = Vector3.zero;
								gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
								gameObject.AddComponent<MeshFilter>().mesh = ((MeshCollider)componentsInChild).sharedMesh;
							}
							try
							{
								source = new Material(source);
								Color color = MultiTool.Configuration.Config.BasicColliderColor;
								if (componentsInChild.isTrigger)
									color = MultiTool.Configuration.Config.TriggerColliderColor;
								if (componentsInChild.gameObject.GetComponent<interiorscript>() != null)
									color = MultiTool.Configuration.Config.InteriorColliderColor;
								source.SetColor("_Color", color);
							}
							catch
							{
							}
							gameObject.AddComponent<MeshRenderer>().material = source;
						}
					}
				}
			}

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

		/// <summary>
		/// Render any HUD elements.
		/// </summary>
		private void RenderHUD()
		{
			GUILayout.BeginArea(new Rect(0, 0, ResolutionX, ResolutionY));

			// TODO: Convert to GUILayout.
			float width = 400f;
			float height = 40f;
			float x = ResolutionX / 2 - 200f;
			float y = ResolutionY * 0.90f;
			switch (_services.State.Mode)
			{
				case "slotControl":
					width = ResolutionX;
					x = 0;
					y = ResolutionY - 30f;
					switch (_services.State.SlotStage)
					{
						case "slotSelect":
							int displayedSlots = 7;

							// Possibly over-complicated method to show selected slot in the middle.
							int lowerHalf = Mathf.FloorToInt((displayedSlots - 1) / 2);
							int upperHalf = Mathf.CeilToInt(displayedSlots / 2) + 1;

							List<int> displayedIndexes = new List<int>();
							int countFrom = _hoveredSlotIndex - lowerHalf - 1;
							if (countFrom < 0)
								countFrom = _slots.Count - 1 - displayedSlots + upperHalf + _hoveredSlotIndex;
							else if (countFrom > _slots.Count - 1)
								countFrom = 0;
							for (int i = 1; i <= displayedSlots; i++)
							{
								int nextIndex;

								if (i <= lowerHalf || i >= upperHalf)
								{
									nextIndex = countFrom + 1;
									if (nextIndex > _slots.Count - 1)
									{
										nextIndex = 0;
										countFrom = 0;
									}
									else
									{
										countFrom = nextIndex;
									}

									displayedIndexes.Add(nextIndex);
								}
								else
								{
									displayedIndexes.Add(_hoveredSlotIndex);
									countFrom = _hoveredSlotIndex;
								}
							}

							for (int index = 0; index < displayedIndexes.Count; index++)
							{
								int slotIndex = displayedIndexes[index];
								GameObject slot = _slots[slotIndex];
								string name = $"{slotIndex + 1} - {PrettifySlotName(slot.name)}";

								if (slotIndex == _hoveredSlotIndex)
								{
									name = $"<b>{name}</b>";
								}
								GUI.Button(new Rect(x + width / displayedIndexes.Count * index, y, width / displayedIndexes.Count, 30f), name);
							}
							GUI.Button(new Rect(x, y - 30f, width / displayedIndexes.Count, 30f), $"< ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.left)})");
							GUI.Button(new Rect(ResolutionX / 2 - (width / displayedIndexes.Count) / 2, y - 30f, width / displayedIndexes.Count, 30f), $"Select ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.select)})");
							GUI.Button(new Rect(ResolutionX - width / displayedIndexes.Count, y - 30f, width / displayedIndexes.Count, 30f), $"({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.right)}) >");
							break;
						case "move":
							GUI.Button(new Rect(ResolutionX / 2 - 100f, 10f, 300f, 30f), $"Moving: {PrettifySlotName(SelectedSlot.name)}");

							int moveControls = 4;
							GUI.Button(new Rect(x, y, width / moveControls, 30f), $"Back to slot select ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.select)})");
							GUI.Button(new Rect(x + width / moveControls * 3, y, width / moveControls, 30f), $"Switch to rotate ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.action3)})");

							// Movement control UI.
							// Column 2.
							GUI.Button(new Rect(x + width / moveControls, y, width / moveControls, 30f), $"Move by: {_moveValue} ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.action5)})");
							GUI.Button(new Rect(x + width / moveControls, y - 30f, width / moveControls, 30f), $"Left ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.left)})");
							GUI.Button(new Rect(x + width / moveControls, y - 60f, width / moveControls, 30f), $"Up ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.noclipSpeedUp)})");

							// Column 3.
							GUI.Button(new Rect(x + width / moveControls * 2, y, width / moveControls, 30f), $"Back ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.down)})");
							GUI.Button(new Rect(x + width / moveControls * 2, y - 30f, width / moveControls, 30f), $"Reset ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.action4)})");
							GUI.Button(new Rect(x + width / moveControls * 2, y - 60f, width / moveControls, 30f), $"Forward ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.up)})");

							// Column 4.
							GUI.Button(new Rect(x + width / moveControls * 3, y - 30f, width / moveControls, 30f), $"Right ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.right)})");
							GUI.Button(new Rect(x + width / moveControls * 3, y - 60f, width / moveControls, 30f), $"Down ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.noclipDown)})");
							break;
						case "rotate":
							GUI.Button(new Rect(ResolutionX / 2 - 100f, 10f, 300f, 30f), $"Rotating: {PrettifySlotName(SelectedSlot.name)}");

							int rotateControls = 4;
							GUI.Button(new Rect(x, y, width / rotateControls, 30f), $"Back to slot select ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.select)})");
							GUI.Button(new Rect(x + width / rotateControls * 3, y, width / rotateControls, 30f), $"Switch to move ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.action3)})");

							// Rotate control UI.
							// Column 2.
							GUI.Button(new Rect(x + width / rotateControls, y, width / rotateControls, 30f), $"Rotate by: {_moveValue} ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.action5)})");
							GUI.Button(new Rect(x + width / rotateControls, y - 30f, width / rotateControls, 30f), $"Left ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.left)})");
							GUI.Button(new Rect(x + width / rotateControls, y - 60f, width / rotateControls, 30f), $"Anticlockwise ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.noclipSpeedUp)})");

							// Column 3.
							GUI.Button(new Rect(x + width / rotateControls * 2, y, width / rotateControls, 30f), $"Back ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.down)})");
							GUI.Button(new Rect(x + width / rotateControls * 2, y - 30f, width / rotateControls, 30f), $"Reset ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.action4)})");
							GUI.Button(new Rect(x + width / rotateControls * 2, y - 60f, width / rotateControls, 30f), $"Forward ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.up)})");

							// Column 4.
							GUI.Button(new Rect(x + width / rotateControls * 3, y - 30f, width / rotateControls, 30f), $"Right ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.right)})");
							GUI.Button(new Rect(x + width / rotateControls * 3, y - 60f, width / rotateControls, 30f), $"Clockwise ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.noclipDown)})");
							break;
					}
					break;
			}

			if (_services.State.ShowCoords)
			{
				GUIExtensions.DrawOutline(new Rect(20f, 20f, 600f, 30f), $"Local position: {mainscript.M.player.transform.position}", _hudStyle, Color.black);
				GUIExtensions.DrawOutline(new Rect(20f, 50f, 600f, 30f), $"Global position: {GameUtilities.GetGlobalObjectPosition(mainscript.M.player.transform.position)}", _hudStyle, Color.black);
			}

			width = ResolutionX / 4f;
			height = ResolutionY / 4;
			if (_services.State.AdvancedObjectDebug)
				height = ResolutionY;
			x = ResolutionX - width;
			y = 0;
			float contentWidth = width - 20f;

			if (_services.State.ObjectDebug && _debugObject != null)
			{
				GUI.Box(new Rect(x, y, width, height), $"<color=#fff><size=18>Object: {_debugObject.name.Replace("(Clone)", string.Empty)}</size></color>");

				x += 10f;
				y += 30f;

				// Basic object information.
				GUI.Label(new Rect(x, y, contentWidth, 20f), $"Save ID: {_debugObject.GetComponent<tosaveitemscript>()?.idInSave}", LabelStyle);
				y += 22f;
				GUI.Label(new Rect(x, y, contentWidth, 20f), $"Local position: {_debugObject.transform.position}", LabelStyle);
				y += 22f;
				GUI.Label(new Rect(x, y, contentWidth, 20f), $"Global position: {GameUtilities.GetGlobalObjectPosition(_debugObject.transform.position)}", LabelStyle);
				y += 22f;
				GUI.Label(new Rect(x, y, contentWidth, 20f), $"Rotation (Euler angles): {_debugObject.transform.rotation.eulerAngles}", LabelStyle);
				y += 22f;
				GUI.Label(new Rect(x, y, contentWidth, 20f), $"Rotation (Quaternion): {_debugObject.transform.rotation}", LabelStyle);

				if (_services.State.AdvancedObjectDebug)
				{
					y += 35f;
					GUI.Label(new Rect(x, y, contentWidth, 60f), "<color=#fff><size=18>Components</size>\nAssembly - Class</color>");
					y += 65f;

					Component[] components = _debugObject.GetComponents(typeof(Component));
					if (_services.State.ObjectDebugShowChildren)
						components = _debugObject.GetComponentsInChildren(typeof(Component));
					components = components.Distinct().ToArray();

					foreach (Component component in components)
					{
						Type type = component.GetType();
						string assembly = type.Assembly.GetName().Name;

						// Skip core components if hidden.
						if (!_services.State.ObjectDebugShowCore && assembly == "Assembly-CSharp")
							continue;

						// Skip Unity components if hidden.
						if (!_services.State.ObjectDebugShowUnity && assembly.Contains("UnityEngine"))
							continue;

						GUI.Label(new Rect(x, y, contentWidth, 20f), $"{assembly} - {type.Name} {(_services.State.ObjectDebugShowChildren && component.transform.parent != null ? "(Child of" + component.transform.parent.name + ")" : "")}");
						y += 22f;
					}
				}
			}

			if (_services.State.ShowColliders && _services.State.ShowColliderHelp)
			{
				width = ResolutionX / 6;
				height = 160f;
				x = 0;
				y = ResolutionY / 2 - height;

				GUI.Box(new Rect(x, y, width, height), "Show colliders");

				y += 30f;
				x += 10f;
				GUI.Label(new Rect(x, y, contentWidth, 20f), $"Look at an object");
				y += 25f;
				GUI.Label(new Rect(x, y, contentWidth, 20f), $"Press '{MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.select)}' to toggle colliders");
				y += 25f;
				GUI.Label(new Rect(x, y, contentWidth, 20f), "Red: Standard collider");
				y += 25f;
				GUI.Label(new Rect(x, y, contentWidth, 20f), "Green: Trigger");
				y += 25f;
				GUI.Label(new Rect(x, y, contentWidth, 20f), "Blue: Interior zone");
			}

			GUILayout.EndArea();
		}

		/// <summary>
		/// Make the vehicle slot name look prettier.
		/// </summary>
		/// <param name="name">Slot name</param>
		/// <returns>Prettified slot name</returns>
		private string PrettifySlotName(string name)
		{
			name = name.Replace("(Clone)", "");
			name = Regex.Replace(name, "\\((.*?)\\)", "");
			name = name.Trim();
			return name.IsAllLower() ? name.ToSentenceCase() : name;
		}

		/// <summary>
		/// Dispose of anything pertaining to slot mover.
		/// </summary>
		internal static void SlotMoverDispose()
		{
			_services.State.Mode = null;
			_services.State.Car = null;
			_services.State.SlotStage = null;

			_slots.Clear();

			SlotMoverSelectDispose();
			SlotMoverMoveDispose();
		}

		/// <summary>
		/// Dispose of slot mover select stage.
		/// </summary>
		internal static void SlotMoverSelectDispose()
		{
			try
			{
				if (HoveredSlot != null)
					ObjectUtilities.DestroyColliders(HoveredSlot);

				HoveredSlot = null;
				_hoveredSlotIndex = 0;
				_previousHoveredSlotIndex = 0;
				_slotMoverFirstRun = true;
			}
			catch (Exception ex)
			{
				Logger.Log($"Error occurred during slot mover select stage dispose - {ex}", Logger.LogLevel.Warning);
			}
		}

		/// <summary>
		/// Dispose of slot mover move stage.
		/// </summary>
		internal static void SlotMoverMoveDispose()
		{
			try
			{
				if (SelectedSlot != null)
					ObjectUtilities.DestroyColliders(SelectedSlot);

				SelectedSlot = null;
				_selectedSlotResetPosition = Vector3.zero;
				_selectedSlotResetRotation.Set(0, 0, 0, 0);
			}
			catch (Exception ex)
			{
				Logger.Log($"Error occurred during slot mover move stage dispose - {ex}", Logger.LogLevel.Warning);
			}
		}
	}
}
