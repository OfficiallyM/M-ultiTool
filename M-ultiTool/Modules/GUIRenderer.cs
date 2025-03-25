using MultiTool.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using TLDLoader;
using UnityEngine;
using MultiTool.Extensions;
using Settings = MultiTool.Core.Settings;
using MultiTool.Utilities;
using UnityEngine.Rendering;
using System.Text.RegularExpressions;
using MultiTool.Utilities.UI;

namespace MultiTool.Modules
{
	internal class GUIRenderer
	{
        // Modules.
        private static TabController Tabs = new TabController();

		private Settings settings = new Settings();

		// Menu control.
		internal bool enabled = false;
		internal bool show = false;
		private bool loaded = false;
		internal bool settingsShow = false;
        private string settingsTabId = null;
		internal bool creditsShow = false;
        private string creditsTabId = null;
		internal bool themeShow = false;
		private string themeTabId = null;

		internal int resolutionX;
		internal int resolutionY;

		internal float mainMenuWidth;
        internal float mainMenuHeight;
        internal float mainMenuX;
        internal float mainMenuY;

        // Styling.
		internal static GUIStyle labelStyle = new GUIStyle();
		internal static GUIStyle headerStyle = new GUIStyle()
		{
			fontSize = 24,
			alignment = TextAnchor.UpperLeft,
			wordWrap = true,
			normal = new GUIStyleState()
			{
				textColor = Color.white,
			}
		};
        internal static GUIStyle subHeaderStyle = new GUIStyle()
        {
            fontSize = 18,
            alignment = TextAnchor.UpperLeft,
            wordWrap = true,
            normal = new GUIStyleState()
            {
                textColor = Color.white,
            }
        };
		internal static float scrollWidth = 10f;
		internal static GUIStyle messageStyle = new GUIStyle()
		{
			fontSize = 40,
			alignment = TextAnchor.MiddleCenter,
			wordWrap = true,
			normal = new GUIStyleState()
			{
				textColor = Color.white,
			}
		};
		private GUIStyle hudStyle = new GUIStyle()
		{
			fontSize = 20,
			alignment = TextAnchor.MiddleLeft,
			normal = new GUIStyleState()
			{
				textColor = Color.white,
			}
		};

		// General variables.
		internal static string search = String.Empty;

		// Vehicle-related variables.
		internal static List<Vehicle> vehicles = new List<Vehicle>();
		internal static Color color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
		internal static int conditionInt = 0;
		internal static bool applyConditionToAttached = false;

		// Item menu variables.
		internal static List<Item> items = new List<Item>();
		internal static Dictionary<string, List<Type>> categories = new Dictionary<string, List<Type>>()
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

		internal static List<GameObject> spawnedObjects = new List<GameObject>();

		// POI variables.
		internal static List<POI> POIs = new List<POI>();

		// Shape variables.
		internal static Vector3 scale = Vector3.one;

		// Player variables.
		internal static Dictionary<mainscript.fluidenum, int> piss = new Dictionary<mainscript.fluidenum, int>();

		// Vehicle configuration variables.
		internal static List<FluidPercentage> fluidDefaults = new List<FluidPercentage>();
		internal static Dictionary<mainscript.fluidenum, int> coolants = new Dictionary<mainscript.fluidenum, int>();
		internal static Dictionary<mainscript.fluidenum, int> oils = new Dictionary<mainscript.fluidenum, int>();
		internal static Dictionary<mainscript.fluidenum, int> fuels = new Dictionary<mainscript.fluidenum, int>();

		// Slot mover variables.
		internal static GameObject selectedSlot;
		internal static GameObject hoveredSlot;
		private static int hoveredSlotIndex = 0;
		private static int previousHoveredSlotIndex = 0;
		private static bool slotMoverFirstRun = true;
		private static Vector3 selectedSlotResetPosition;
		private static Quaternion selectedSlotResetRotation;
		private float[] moveOptions = new float[] { 10f, 1f, 0.1f, 0.01f, 0.001f };
		private float moveValue = 0.1f;
		private static List<GameObject> slots = new List<GameObject>();

		// Object selection.
		internal static tosaveitemscript selectedObject;

		// Settings.
		internal static float settingsScrollWidth;
        internal static bool accessibilityShow = false;
        internal static float noclipFastMoveFactor = 10f;

		// HUD variables.
		private GameObject debugObject = null;
		internal static string axis = "all";
		internal static string[] axisOptions = new string[] { "all", "x", "y", "z" };
		internal static float scaleValue = 0.1f;
		internal static float[] scaleOptions = new float[] { 10f, 1f, 0.1f, 0.01f, 0.001f };
		internal static bool scaleHold = true;

		internal static float weightValue = 0.1f;
		internal static float[] weightOptions = new float[] { 100f, 10f, 1f, 0.1f, 0.01f};
		internal static bool weightHold = true;

		// Colour palettes.
		internal static List<Color> palette = new List<Color>();
		private static Dictionary<int, GUIStyle> paletteCache = new Dictionary<int, GUIStyle>();

		// Main menu variables.
		private bool mainMenuLoaded = false;
		private bool stateChanged = false;
		private Vector2 currentMainMenuPosition;
		private static string[] mainMenuStages = new string[] { "vehicle", "basics", "color" };
		private string mainMenuStage = mainMenuStages[0];
		private Color? startVehicleColor = null;
		private int startVehicleCondition = -1;
		private string startVehiclePlate = string.Empty;
		private bool appliedStartVehicleChanges = false;
		private string[] largeVehicles = new string[]
		{
			"bus01",
			"bus02",
			"bus03",
			"car07",
			"car09T",
            "car11",
		};
		private string[] bikes = new string[]
		{
			"bike01",
			"bike03",
		};

		internal void OnGUI()
		{
            Styling.Bootstrap();
            GUI.skin = Styling.GetActiveSkin();

            // Find screen resolution.
            resolutionX = Screen.width;
			resolutionY = Screen.height;
            int resX = settingsscript.s.S.IResolutionX;
			int resY = settingsscript.s.S.IResolutionY;
			if (resX != resolutionX)
			{
				resolutionX = resX;
				resolutionY = resY;

				mainMenuWidth = resolutionX - 80f;
				mainMenuHeight = resolutionY - 80f;
				mainMenuX = 40f;
				mainMenuY = 40f;
			}

			// In game.
			if (mainscript.M != null)
			{
				if (loaded)
                {
				    if (!show && !mainscript.M.menu.Menu.activeSelf)
					    RenderHUD();

				    else if (!show && mainscript.M.menu.Menu.activeSelf)
					    RenderPauseMenu();

				    else if (show)
				        MainMenu();
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
				loaded = false;

                // Unregister tabs so they re-register correctly if already loaded.
                if (Tabs.GetCount() > 0)
                    Tabs.UnregisterAll();

                // Ensure UI loads hidden.
                show = false;

				resolutionX = settingsscript.s.S.IResolutionX;
				resolutionY = settingsscript.s.S.IResolutionY;

				mainMenuWidth = resolutionX - 80f;
				mainMenuHeight = resolutionY - 80f;
				mainMenuX = 40f;
				mainMenuY = 40f;

				// Add default navigation tabs.
				Tabs.AddTab(new Tabs.VehiclesTab());
				Tabs.AddTab(new Tabs.ItemsTab());
				Tabs.AddTab(new Tabs.POIsTab());
				Tabs.AddTab(new Tabs.ShapesTab());
                Tabs.AddTab(new Tabs.PlayerTab());
				Tabs.AddTab(new Tabs.VehicleConfigurationTab());
				Tabs.AddTab(new Tabs.SandboxTab());
				Tabs.AddTab(new Tabs.DeveloperTab());

                // Add default hidden tabs.
                settingsTabId = Tabs.AddTab(new Tabs.SettingsTab());
                creditsTabId = Tabs.AddTab(new Tabs.CreditsTab());
				themeTabId = Tabs.AddTab(new Tabs.ThemeTab());

                // Load data from database.
                vehicles = DatabaseUtilities.LoadVehicles();
				items = DatabaseUtilities.LoadItems();
				POIs = DatabaseUtilities.LoadPOIs();

				// Load save data.
				SaveUtilities.LoadSaveData();

				// Clear any existing static values.
				fluidDefaults.Clear();
				coolants.Clear();
				oils.Clear();
				fuels.Clear();
				piss.Clear();

				// Prepopulate any variables that use the fluidenum.
				int maxFuelType = (int)Enum.GetValues(typeof(mainscript.fluidenum)).Cast<mainscript.fluidenum>().Max();
				for (int i = 0; i <= maxFuelType; i++)
				{
					fluidDefaults.Add(new FluidPercentage() { type = (mainscript.fluidenum)i, percentage = 0 });
					coolants.Add((mainscript.fluidenum)i, 0);
					oils.Add((mainscript.fluidenum)i, 0);
					fuels.Add((mainscript.fluidenum)i, 0);
					piss.Add((mainscript.fluidenum)i, 0);
				}

				// Load any configs not loaded on the main menu.
				try
				{
					settingsScrollWidth = scrollWidth;
					noclipFastMoveFactor = MultiTool.Configuration.GetNoclipFastMoveFactor(noclipFastMoveFactor);
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
				Notifications.Send(MultiTool.mod.Name, "Critical error occurred. Please report to M-.");
			}

			loaded = true;
		}

		internal void OnMenuLoad()
		{
			show = false;
			loaded = false;
			mainMenuLoaded = false;
		}

		internal void Update()
		{
			Notifications.Update();

			if (MultiTool.isOnMainMenu)
			{
				MainMenuUpdate();
				return;
			}

			if (!loaded) return;

            // Trigger update for tabs and notifications.
            Tabs.Update();

			if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.menu).key) && !mainscript.M.menu.Menu.activeSelf && !mainscript.M.settingsOpen && !mainscript.M.menu.saveScreen.gameObject.activeSelf)
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

			// Detect item when item debugging is enabled.
			if (settings.objectDebug)
			{
				try
				{
					GameObject foundObject = null;
					// Find object the player is looking at.
					Physics.Raycast(mainscript.M.player.Cam.transform.position, mainscript.M.player.Cam.transform.forward, out var raycastHit, float.PositiveInfinity, mainscript.M.player.useLayer);

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

					debugObject = foundObject;
				}
				catch (Exception ex)
				{
					Logger.Log($"Error determining debug object - {ex}", Logger.LogLevel.Error);
				}
			}

			if (settings.mode == "slotControl")
			{
				try
				{
					// Unset slotControl mode when exiting a vehicle.
					if (mainscript.M.player.Car == null)
					{
						SlotMoverDispose();
					}
					else if (slots.Count == 0)
					{
						partslotscript[] partSlots = settings.car.GetComponentsInChildren<partslotscript>();
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

							slots.Add(obj);
						}

						// Find anything that isn't an actual part.
						foreach (MeshRenderer child in settings.car.GetComponentsInChildren<MeshRenderer>())
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
                                    slots.Add(child.gameObject);
                                }
                            }

                            foreach (string parentSlotName in parentNames)
                            {
							    if (parentName.Contains(parentSlotName) && parent.activeSelf)
							    {
								    slots.Add(parent);
							    }
                            }
						}
					}

					tosaveitemscript carSave = settings.car.GetComponent<tosaveitemscript>();

					switch (settings.slotStage)
					{
						case "slotSelect":
							bool slotChanged = false;

							// Render collider on first load.
							if (slotMoverFirstRun)
							{
								slotChanged = true;
								hoveredSlot = slots[hoveredSlotIndex];
							}

							// Move selector left.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.left).key))
							{
								previousHoveredSlotIndex = hoveredSlotIndex;
								hoveredSlotIndex--;
								if (hoveredSlotIndex < 0)
									hoveredSlotIndex = slots.Count - 1;

								hoveredSlot = slots[hoveredSlotIndex];
								slotChanged = true;
							}

							// Move selector right.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.right).key))
							{
								previousHoveredSlotIndex = hoveredSlotIndex;
								hoveredSlotIndex++;
								if (hoveredSlotIndex >= slots.Count)
									hoveredSlotIndex = 0;

								hoveredSlot = slots[hoveredSlotIndex];
								slotChanged = true;
							}

							// Select the hovered slot.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.select).key))
							{
								settings.slotStage = "move";
								selectedSlot = hoveredSlot;

								selectedSlotResetPosition = selectedSlot.transform.localPosition;
								selectedSlotResetRotation = selectedSlot.transform.localRotation;

								// Get reset positions from save data.
								SlotData slotData = SaveUtilities.GetSlotData(carSave.idInSave, selectedSlot.name);
								if (slotData != null)
								{
									selectedSlotResetPosition = slotData.resetPosition;
									selectedSlotResetRotation = slotData.resetRotation;
								}

								SlotMoverSelectDispose();

								ObjectUtilities.ShowColliders(selectedSlot, Color.blue);
							}

							if (slotChanged)
							{
								ObjectUtilities.ShowColliders(hoveredSlot, Color.red);

								if (!slotMoverFirstRun)
								{
									GameObject previousSlot = slots[previousHoveredSlotIndex];

									ObjectUtilities.DestroyColliders(previousSlot);
								}
								slotMoverFirstRun = false;
							}
							break;
						case "move":
							// Deselect slot.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.select).key))
							{
								settings.slotStage = "slotSelect";
								hoveredSlotIndex = Array.FindIndex(slots.ToArray(), s => s.name == selectedSlot.name);
								SlotMoverMoveDispose();
								return;
							}

							// Switch to rotate mode.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.action3).key))
							{
								settings.slotStage = "rotate";
							}

							// Change move amount.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.action5).key))
							{
								int currentIndex = Array.FindIndex(moveOptions, s => s == moveValue);
								if (currentIndex == -1 || currentIndex == moveOptions.Length - 1)
									moveValue = moveOptions[0];
								else
									moveValue = moveOptions[currentIndex + 1];
							}

							Transform partTransform = selectedSlot.transform;
							Vector3 oldPos = partTransform.localPosition;

							// Move forward.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.up).key))
							{
								partTransform.localPosition += Vector3.forward * moveValue;
							}

							// Move backwards.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.down).key))
							{
								partTransform.localPosition += Vector3.back * moveValue;
							}

							// Move left.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.left).key))
							{
								partTransform.localPosition += Vector3.left * moveValue;
							}

							// Move right.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.right).key))
							{
								partTransform.localPosition += Vector3.right * moveValue;
							}

							// Move up.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.noclipSpeedUp).key))
							{
								partTransform.localPosition += Vector3.up * moveValue;
							}

							// Move down.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.noclipDown).key))
							{
								partTransform.localPosition += Vector3.down * moveValue;
							}

							// Reset position.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.action4).key))
							{
								partTransform.localPosition = selectedSlotResetPosition;
							}

							// Check if position has changed.
							if (oldPos != partTransform.localPosition)
							{
								SlotData slotData = new SlotData()
								{
									ID = carSave.idInSave,
									slot = selectedSlot.name,
									position = partTransform.localPosition,
									resetPosition = selectedSlotResetPosition,
									rotation = partTransform.localRotation,
									resetRotation = selectedSlotResetRotation,
								};
								SaveUtilities.UpdateSlot(slotData);
							}

							break;
						case "rotate":
							// Deselect slot.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.select).key))
							{
								settings.slotStage = "slotSelect";
								hoveredSlotIndex = Array.FindIndex(slots.ToArray(), s => s.name == selectedSlot.name);
								SlotMoverMoveDispose();
								return;
							}

							// Switch to move mode.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.action3).key))
							{
								settings.slotStage = "move";
							}

							// Change move amount.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.action5).key))
							{
								int currentIndex = Array.FindIndex(moveOptions, s => s == moveValue);
								if (currentIndex == -1 || currentIndex == moveOptions.Length - 1)
									moveValue = moveOptions[0];
								else
									moveValue = moveOptions[currentIndex + 1];
							}

							Transform rotatePartTransform = selectedSlot.transform;
							Quaternion oldRot = rotatePartTransform.localRotation;

							// Rotate forward.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.up).key))
							{
								rotatePartTransform.Rotate(Vector3.right, moveValue);
							}

							// Rotate backwards.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.down).key))
							{
								rotatePartTransform.Rotate(-Vector3.right, moveValue);
							}

							// Rotate left.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.left).key))
							{
								rotatePartTransform.Rotate(-Vector3.forward, moveValue);
							}

							// Rotate right.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.right).key))
							{
								rotatePartTransform.Rotate(Vector3.forward, moveValue);
							}

							// Rotate anticlockwise.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.noclipSpeedUp).key))
							{
								rotatePartTransform.Rotate(Vector3.up, moveValue);
							}

							// Rotate clockwise.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.noclipDown).key))
							{
								rotatePartTransform.Rotate(-Vector3.up, moveValue);
							}

							// Reset position.
							if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.action4).key))
							{
								rotatePartTransform.localRotation = selectedSlotResetRotation;
							}

							// Check if rotation has changed.
							if (oldRot != rotatePartTransform.localRotation)
							{
								SlotData slotData = new SlotData()
								{
									ID = carSave.idInSave,
									slot = selectedSlot.name,
									position = rotatePartTransform.localPosition,
									resetPosition = selectedSlotResetPosition,
									rotation = rotatePartTransform.localRotation,
									resetRotation = selectedSlotResetRotation,
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
			if (settings.showColliders)
			{
				RaycastHit hitInfo;
				if (Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.select).key) && Physics.Raycast(mainscript.M.player.Cam.transform.position, mainscript.M.player.Cam.transform.forward, out hitInfo, float.PositiveInfinity, (int)mainscript.M.player.useLayer))
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
								Color color = MultiTool.Configuration.GetColliderColour("basic");
								if (componentsInChild.isTrigger)
									color = MultiTool.Configuration.GetColliderColour("trigger");
                                if (componentsInChild.gameObject.GetComponent<interiorscript>() != null)
									color = MultiTool.Configuration.GetColliderColour("interior");
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
			if (!appliedStartVehicleChanges && loaded && !ModLoader.loading.activeSelf)
			{
				try
				{
					// Don't apply any new game changes when loading a save.
					if (mainscript.M.menu.DFMS.load)
					{
						appliedStartVehicleChanges = true;
						return;
					}

					GameObject starterVehicle = null;
					string starterVehicleName = mainscript.M.StartCar.ToString();
					bool isLargeVehicle = largeVehicles.Contains(starterVehicleName);
					bool isBike = bikes.Contains(starterVehicleName);
					foreach (var car in mainscript.M.Cars)
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
						if (startVehicleColor.HasValue)
							color = startVehicleColor.Value;

						// Destroying the actual starter car doesn't want to cooperate
						// so drop it out the map instead.
						UnityEngine.Object.Destroy(starterVehicle.gameObject);
						starterVehicle.transform.position += Vector3.down * 15f;

						Vehicle vehicle = vehicles.Where(v => v.gameObject.name.ToLower().Contains(starterVehicleName.ToLower())).FirstOrDefault();
						if (vehicle != null)
						{
							finalStarterVehicle = SpawnUtilities.Spawn(vehicle.gameObject, color, startVehicleCondition, -1, position, rotation);
						}
					}
					else
					{
						finalStarterVehicle = starterVehicle;

						// Set starter vehicle colour.
						if (startVehicleColor.HasValue)
						{
							partconditionscript partconditionscript = finalStarterVehicle.GetComponent<partconditionscript>();
							GameUtilities.Paint(startVehicleColor.Value, partconditionscript);
						}

						// Set starter vehicle condition.
						if (startVehicleCondition != -1)
						{
							partconditionscript partconditionscript = finalStarterVehicle.GetComponent<partconditionscript>();
							List<partconditionscript> children = GameUtilities.FindPartChildren(partconditionscript);

							foreach (partconditionscript child in children)
							{
								child.state = startVehicleCondition;
								child.Refresh();
							}
						}
					}

					// Set starter vehicle plate.
					if (startVehiclePlate != string.Empty)
					{
						rendszamscript[] plateScripts = finalStarterVehicle.GetComponentsInChildren<rendszamscript>();
						foreach (rendszamscript plateScript in plateScripts)
						{
							if (plateScript == null)
								continue;

							plateScript.Same(startVehiclePlate);
						}
					}

					appliedStartVehicleChanges = true;
				}
				catch (Exception ex)
				{
					Logger.Log($"Error occurred during starter vehicle configuration - {ex}");
					appliedStartVehicleChanges = true;
				}
			}
		}

		/// <summary>
		/// Separate update function for the main menu.
		/// </summary>
		private void MainMenuUpdate()
		{
			// Use the first run of Update() to get any variables we need
			// as OnMenuLoad() is called before anything is started.
			if (!mainMenuLoaded)
			{
				resolutionX = settingsscript.s.S.IResolutionX;
				resolutionY = settingsscript.s.S.IResolutionY;

				// Default language to English until we can pull it from mainscript.
				Translator.SetLanguage("English");

				// Set label styling.
				labelStyle.alignment = TextAnchor.UpperLeft;
				labelStyle.normal.textColor = Color.white;

				// Set default palette to all white.
				palette.Clear();
				palette = Enumerable.Repeat(Color.white, 60).ToList();
				paletteCache.Clear();

				// Load any configs needed for the main menu UI.
				try
				{
					scrollWidth = MultiTool.Configuration.GetScrollWidth(scrollWidth);
					palette = MultiTool.Configuration.GetPalette(palette);
				}
				catch (Exception ex)
				{
					Logger.Log($"Config load error - {ex}", Logger.LogLevel.Error);
				}

				mainMenuLoaded = true;
			}

			if (stateChanged)
			{
				string[] toggles = new string[] { "ButtonLoad", "ButtonSettings", "ButtonExit", "ButtonDiscord", "ButtonNews" };
				foreach (string toggle in toggles)
				{
					mainmenuscript.mainmenu.Canvas.Find($"GameObject/MainStuff/{toggle}").gameObject.SetActive(!show);
				}

				stateChanged = false;
			}
		}

		/// <summary>
		/// Show menu toggle button.
		/// </summary>
		private void RenderPauseMenu()
		{
			MultiTool.Binds.RenderRebindMenu("M-ultiTool menu key", new int[] { (int)Keybinds.Inputs.menu }, resolutionX - 350f, 50f, 300f, 100f);
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

            GUILayout.BeginArea(new Rect(x, y, width, height), $"<color=#f87ffa><size=18><b>{MultiTool.mod.Name}</b></size>\n<size=16>v{MultiTool.mod.Version} - made with ❤️ by {MultiTool.mod.Author}</size></color>", "box");
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(Accessibility.GetAccessibleString("Settings", settingsShow), "ButtonSecondary", GUILayout.MinHeight(30)))
            {
                settingsShow = !settingsShow;
                creditsShow = false;
            }

            GUILayout.Space(10);

            if (GUILayout.Button(Accessibility.GetAccessibleString("Credits", creditsShow), "ButtonSecondary", GUILayout.MinHeight(30)))
            {
                creditsShow = !creditsShow;
                settingsShow = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(20);

            if (settingsShow)
            {
				Tabs.RenderTab(settingsTabId);
            }
            else if (creditsShow)
            {
                Tabs.RenderTab(creditsTabId);
            }
			else if (themeShow)
			{
				Tabs.RenderTab(themeTabId);
			}
            else
            {
                // Render navigation bar.
                GUILayout.BeginHorizontal();

                for (int tabIndex = 0; tabIndex < Tabs.GetCount(); tabIndex++)
                {
                    Tab tab = Tabs.GetByIndex(tabIndex);

                    // Ignore any tabs excluded from navigation.
                    if (!tab.ShowInNavigation) continue;

                    // Render disabled tabs as unclickable.
                    if (tab.IsDisabled)
                        GUI.enabled = false;

                    if (GUILayout.Button(Tabs.GetActive() == tab.Id ? $"<color=#0F0>{tab.Name}</color>" : tab.Name, GUILayout.MinWidth(60), GUILayout.MaxHeight(30)))
                    {
                        Tabs.SetActive(tab.Id);
                    }

                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();

                // Render the active tab.
                Tabs.RenderTab();
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
		}

		/// <summary>
		/// Render game main menu UI.
		/// </summary>
		private void GameMainMenuUI()
		{
			float width = resolutionX / 3;
			float height = resolutionY - 200f;
			float x = resolutionX - resolutionX / 3;
			float y = 100f;

			// Don't render the UI if any game menus are open.
			if (mainmenuscript.mainmenu.SettingsScreenObj.activeSelf || mainmenuscript.mainmenu.SaveScreenObj.activeSelf) return;

			if (!show) {
                GUILayout.BeginArea(new Rect(resolutionX - 200f, resolutionY / 3 - 10f, 200f, 60f));
                if (GUILayout.Button("New game settings", "ButtonBlackTranslucent", GUILayout.MinHeight(60)))
                {
                    show = true;
                    stateChanged = true;
                }
                GUILayout.EndArea();
			}

			if (!show)
				return;

			GUILayout.BeginArea(new Rect(x, y, width, height), "<color=#f87ffa><size=18><b>New game settings</b></size></color>", "box");
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
			if (GUILayout.Button("<size=30><color=#F00>X</color></size>", "ButtonBlack", GUILayout.MinWidth(40f)))
			{
				show = false;
				stateChanged = true;
			}
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(GUILayout.MaxWidth(width - 15f));
            currentMainMenuPosition = GUILayout.BeginScrollView(currentMainMenuPosition);
            switch (mainMenuStage)
            {
                case "vehicle":
                    int optionCount = (int)Enum.GetValues(typeof(itemdatabase.CarType)).Cast<itemdatabase.CarType>().Max();
                    foreach (var car in Enum.GetValues(typeof(itemdatabase.CarType)))
                    {
                        string name = Translator.T(car.ToString(), "menuVehicles");

                        if (GUILayout.Button(Accessibility.GetAccessibleString(name, mainmenuscript.mainmenu.DFMS.startcar == (itemdatabase.CarType)car)))
							mainmenuscript.mainmenu.DFMS.startcar = (itemdatabase.CarType)car;
					}
                    break;

                case "basics":
                    // Condition.
                    GUILayout.Label($"Condition: {(Item.Condition)startVehicleCondition}");
                    int maxCondition = (int)Enum.GetValues(typeof(Item.Condition)).Cast<Item.Condition>().Max();
                    float rawCondition = GUILayout.HorizontalSlider(startVehicleCondition, -1, maxCondition);
                    startVehicleCondition = Mathf.RoundToInt(rawCondition);

                    GUILayout.Space(10);

                    // License plate.
                    GUILayout.Label("Plate (blank for random):");
                    startVehiclePlate = GUILayout.TextField(startVehiclePlate, 7);
                    break;

                case "color":
                    if (GUILayout.Button($"Using {(startVehicleColor.HasValue ? "custom" : "random")} colour"))
                    {
                        if (startVehicleColor.HasValue)
                            startVehicleColor = null;
                        else
                            startVehicleColor = Color.white;
                    }
                    GUILayout.Space(10);

                    if (startVehicleColor.HasValue)
                        startVehicleColor = Colour.RenderColourSliders(width - 20f, startVehicleColor.Value);
                    break;
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            // Back button.
            if (mainMenuStage != mainMenuStages.First())
            {
                string previousStage = mainMenuStages[Array.FindIndex(mainMenuStages, s => s == mainMenuStage) - 1];
                if (GUILayout.Button($"To {previousStage}", GUILayout.MinWidth(200), GUILayout.Height(20)))
                {
                    mainMenuStage = previousStage;
                    currentMainMenuPosition = Vector2.zero;
                }
            }

            GUILayout.FlexibleSpace();

            // Next button.
            if (mainMenuStage != mainMenuStages.Last())
            {
                string nextStage = mainMenuStages[Array.FindIndex(mainMenuStages, s => s == mainMenuStage) + 1];
                if (GUILayout.Button($"To {nextStage}", GUILayout.MinWidth(200), GUILayout.Height(20)))
                {
                    mainMenuStage = nextStage;
                    currentMainMenuPosition = Vector2.zero;
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
			GUILayout.BeginArea(new Rect(0, 0, resolutionX, resolutionY));

			// TODO: Convert to GUILayout.
			float width = 400f;
			float height = 40f;
			float x = resolutionX / 2 - 200f;
			float y = resolutionY * 0.90f;
			switch (settings.mode)
			{
				case "colorPicker":
					GUI.Box(new Rect(x, y, width, height), string.Empty);
					GUI.Button(new Rect(x, y, width / 2, height / 2), "Copy");
					GUI.Button(new Rect(x + width / 2, y, width / 2, height / 2), "Paste");

					GUI.Button(new Rect(x, y + height / 2, width / 2, height / 2), MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.action1));
					GUI.Button(new Rect(x + width / 2, y + height / 2, width / 2, height / 2), MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.action2));

					// Colour preview.
					GUIStyle defaultStyle = GUI.skin.button;
					GUIStyle previewStyle = new GUIStyle(defaultStyle);
					Texture2D previewTexture = new Texture2D(1, 1);
					Color[] pixels = new Color[] { Colour.GetColour() };
					previewTexture.SetPixels(pixels);
					previewTexture.Apply();
					previewStyle.normal.background = previewTexture;
					previewStyle.active.background = previewTexture;
					previewStyle.hover.background = previewTexture;
					previewStyle.margin = new RectOffset(0, 0, 0, 0);
					GUI.skin.button = previewStyle;
					GUI.Button(new Rect(x, y + height, width, height / 2), "");
					GUI.skin.button = defaultStyle;
					break;
				case "scale":
                    GUI.Button(new Rect(resolutionX / 2 - 250f, 10f, 500f, 30f), $"Selected object: {(selectedObject != null ? selectedObject.name: "None")} ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.action1)} to {(selectedObject != null ? "deselect" : "select")})");
                    if (selectedObject != null)
                    {
                        width = 400f;
                        height = 120f;
                        x = 0;
                        y = resolutionY / 2 - (height + 20f) / 2;

                        GUI.Box(new Rect(x, y, width, height + 20f), string.Empty);
                        int rows = 6;
                        GUI.Button(new Rect(x, y, width / 2, height / rows), "Scale up");
                        GUI.Button(new Rect(x, y + height / rows, width / 2, height / rows), "Scale down");
                        GUI.Button(new Rect(x, y + height / rows * 2, width / 2, height / rows), $"Axis: {axis}");
                        GUI.Button(new Rect(x, y + height / rows * 3, width / 2, height / rows), $"Scale amount: {scaleValue}");
                        GUI.Button(new Rect(x, y + height / rows * 4, width / 2, height / rows), $"Toggle hold to scale ({(scaleHold ? "Hold" : "Click")})");
                        GUI.Button(new Rect(x, y + height / rows * 5, width / 2, height / rows), "Reset");

                        GUI.Button(new Rect(x + width / 2, y, width / 2, height / rows), MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.up));
                        GUI.Button(new Rect(x + width / 2, y + height / rows, width / 2, height / rows), MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.down));
                        GUI.Button(new Rect(x + width / 2, y + height / rows * 2, width / 2, height / rows), MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.action3));
                        GUI.Button(new Rect(x + width / 2, y + height / rows * 3, width / 2, height / rows), MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.action5));
                        GUI.Button(new Rect(x + width / 2, y + height / rows * 4, width / 2, height / rows), MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.select));
                        GUI.Button(new Rect(x + width / 2, y + height / rows * 5, width / 2, height / rows), MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.action4));

                        Vector3 scale = selectedObject.transform.localScale;
                        string scaleDisplay = scale.ToString();
                        switch (axis)
                        {
                            case "x":
                                scaleDisplay = scale.x.ToString();
                                break;
                            case "y":
                                scaleDisplay = scale.y.ToString();
                                break;
                            case "z":
                                scaleDisplay = scale.z.ToString();
                                break;
                        }
                        GUI.Button(new Rect(x, y + height, width, 20f), $"Scale: {scaleDisplay}");
                    }
					break;
				case "slotControl":
					width = resolutionX;
					x = 0;
					y = resolutionY - 30f;
					switch (settings.slotStage)
					{
						case "slotSelect":
							int displayedSlots = 7;

							// Possibly over-complicated method to show selected slot in the middle.
							int lowerHalf = Mathf.FloorToInt((displayedSlots - 1) / 2);
							int upperHalf = Mathf.CeilToInt(displayedSlots / 2) + 1;

							List<int> displayedIndexes = new List<int>();
							int countFrom = hoveredSlotIndex - lowerHalf - 1;
							if (countFrom < 0)
								countFrom = slots.Count - 1 - displayedSlots + upperHalf + hoveredSlotIndex;
							else if (countFrom > slots.Count - 1)
								countFrom = 0;
							for (int i = 1; i <= displayedSlots; i++)
							{
								int nextIndex;

								if (i <= lowerHalf || i >= upperHalf)
								{
									nextIndex = countFrom + 1;
									if (nextIndex > slots.Count - 1)
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
									displayedIndexes.Add(hoveredSlotIndex);
									countFrom = hoveredSlotIndex;
								}
							}

							for (int index = 0; index < displayedIndexes.Count; index++)
							{
								int slotIndex = displayedIndexes[index];
								GameObject slot = slots[slotIndex];
								string name = $"{slotIndex + 1} - {PrettifySlotName(slot.name)}";

                                if (slotIndex == hoveredSlotIndex)
								{
									name = $"<b>{name}</b>";
								}
								GUI.Button(new Rect(x + width / displayedIndexes.Count * index, y, width / displayedIndexes.Count, 30f), name);
							}
							GUI.Button(new Rect(x, y - 30f, width / displayedIndexes.Count, 30f), $"< ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.left)})");
							GUI.Button(new Rect(resolutionX / 2 - (width / displayedIndexes.Count) / 2, y - 30f, width / displayedIndexes.Count, 30f), $"Select ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.select)})");
							GUI.Button(new Rect(resolutionX - width / displayedIndexes.Count, y - 30f, width / displayedIndexes.Count, 30f), $"({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.right)}) >");
							break;
						case "move":
							GUI.Button(new Rect(resolutionX / 2 - 100f, 10f, 300f, 30f), $"Moving: {PrettifySlotName(selectedSlot.name)}");

							int moveControls = 4;
							GUI.Button(new Rect(x, y, width / moveControls, 30f), $"Back to slot select ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.select)})");
							GUI.Button(new Rect(x + width / moveControls * 3, y, width / moveControls, 30f), $"Switch to rotate ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.action3)})");

							// Movement control UI.
							// Column 2.
							GUI.Button(new Rect(x + width / moveControls, y, width / moveControls, 30f), $"Move by: {moveValue} ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.action5)})");
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
							GUI.Button(new Rect(resolutionX / 2 - 100f, 10f, 300f, 30f), $"Rotating: {PrettifySlotName(selectedSlot.name)}");

							int rotateControls = 4;
							GUI.Button(new Rect(x, y, width / rotateControls, 30f), $"Back to slot select ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.select)})");
							GUI.Button(new Rect(x + width / rotateControls * 3, y, width / rotateControls, 30f), $"Switch to move ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.action3)})");

							// Rotate control UI.
							// Column 2.
							GUI.Button(new Rect(x + width / rotateControls, y, width / rotateControls, 30f), $"Rotate by: {moveValue} ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.action5)})");
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
				case "objectRegenerator":
					width = 400f;
					height = 40f;
					x = resolutionX / 2 - 200f;
					y = resolutionY * 0.90f;
					GUI.Box(new Rect(x, y, width, height), string.Empty);
					GUI.Button(new Rect(x, y, width / 2, height / 2), "Select object");
					GUI.Button(new Rect(x + width / 2, y, width / 2, height / 2), "Regenerate object");

					GUI.Button(new Rect(x, y + height / 2, width / 2, height / 2), MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.action1));
					GUI.Button(new Rect(x + width / 2, y + height / 2, width / 2, height / 2), MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.action4));

					if (selectedObject != null)
						GUI.Button(new Rect(resolutionX / 2 - 250f, 10f, 500f, 30f), $"Selected object: {selectedObject.name} (ID: {selectedObject.idInSave})");
					break;
				case "weightChanger":
					GUILayout.BeginVertical();
					GUILayout.Space(10);
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUILayout.BeginVertical("box");
					GUILayout.Button($"Selected object: {(selectedObject != null ? selectedObject.name : "None")} ({MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.action1)} to {(selectedObject != null ? "deselect" : "select")})", GUILayout.MaxHeight(30));
					if (selectedObject != null)
					{
						GUILayout.Space(5);
						GUILayout.Button($"Base weight: {selectedObject.GetComponent<massScript>()?.OwnMass().ToString("F2")} kg", GUILayout.MaxHeight(30));
						GUILayout.Button($"Total weight: {selectedObject.GetComponent<massScript>()?.Mass().ToString("F2")} kg", GUILayout.MaxHeight(30));
					}
					GUILayout.EndVertical();
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
					GUILayout.EndVertical();

					if (selectedObject != null)
					{
						float boxWidth = resolutionX / 5;
						float buttonWidth = boxWidth / 2f;
						GUILayout.BeginVertical(GUILayout.MinWidth(boxWidth));
						GUILayout.FlexibleSpace();
						GUILayout.BeginVertical("box");
						GUILayout.BeginHorizontal();
						GUILayout.Button("Weight up", "ButtonPrimaryWrap", GUILayout.Width(buttonWidth));
						GUILayout.Button(MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.up), "ButtonPrimaryWrap", GUILayout.Width(buttonWidth));
						GUILayout.EndHorizontal();
						GUILayout.Space(5);

						GUILayout.BeginHorizontal();
						GUILayout.Button($"Weight down", "ButtonPrimaryWrap", GUILayout.Width(buttonWidth));
						GUILayout.Button(MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.down), "ButtonPrimaryWrap", GUILayout.Width(buttonWidth));
						GUILayout.EndHorizontal();
						GUILayout.Space(5);

						GUILayout.BeginHorizontal();
						GUILayout.Button($"Change amount: {weightValue} kg", "ButtonPrimaryWrap", GUILayout.Width(buttonWidth));
						GUILayout.Button(MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.action5), "ButtonPrimaryWrap", GUILayout.Width(buttonWidth));
						GUILayout.EndHorizontal();
						GUILayout.Space(5);

						GUILayout.BeginHorizontal();
						GUILayout.Button($"Toggle hold to change ({(weightHold ? "Hold" : "Click")})", "ButtonPrimaryWrap", GUILayout.Width(buttonWidth));
						GUILayout.Button(MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.select), "ButtonPrimaryWrap", GUILayout.Width(buttonWidth));
						GUILayout.EndHorizontal();
						GUILayout.Space(5);

						GUILayout.BeginHorizontal();
						GUILayout.Button("Reset", "ButtonPrimaryWrap", GUILayout.Width(buttonWidth));
						GUILayout.Button(MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.action4), "ButtonPrimaryWrap", GUILayout.Width(buttonWidth));
						GUILayout.EndHorizontal();
						GUILayout.EndVertical();
						GUILayout.FlexibleSpace();
						GUILayout.EndVertical();
					}
					break;
			}

			if (settings.showCoords)
			{
				GUIExtensions.DrawOutline(new Rect(20f, 20f, 600f, 30f), $"Local position: {mainscript.M.player.transform.position}", hudStyle, Color.black);
                GUIExtensions.DrawOutline(new Rect(20f, 50f, 600f, 30f), $"Global position: {GameUtilities.GetGlobalObjectPosition(mainscript.M.player.transform.position)}", hudStyle, Color.black);
            }

			width = resolutionX / 4f;
			height = resolutionY / 4;
			if (settings.advancedObjectDebug)
				height = resolutionY;
			x = resolutionX - width;
			y = 0;
			float contentWidth = width - 20f;

			if (settings.objectDebug && debugObject != null)
			{
				GUI.Box(new Rect(x, y, width, height), $"<color=#fff><size=18>Object: {debugObject.name.Replace("(Clone)", string.Empty)}</size></color>");

				x += 10f;
				y += 30f;

				// Basic object information.
				GUI.Label(new Rect(x, y, contentWidth, 20f), $"Save ID: {debugObject.GetComponent<tosaveitemscript>()?.idInSave}", labelStyle);
				y += 22f;
				GUI.Label(new Rect(x, y, contentWidth, 20f), $"Local position: {debugObject.transform.position}", labelStyle);
				y += 22f;
                GUI.Label(new Rect(x, y, contentWidth, 20f), $"Global position: {GameUtilities.GetGlobalObjectPosition(debugObject.transform.position)}", labelStyle);
                y += 22f;
                GUI.Label(new Rect(x, y, contentWidth, 20f), $"Rotation (Euler angles): {debugObject.transform.rotation.eulerAngles}", labelStyle);
                y += 22f;
                GUI.Label(new Rect(x, y, contentWidth, 20f), $"Rotation (Quaternion): {debugObject.transform.rotation}", labelStyle);

				if (settings.advancedObjectDebug)
				{
					y += 35f;
					GUI.Label(new Rect(x, y, contentWidth, 60f), "<color=#fff><size=18>Components</size>\nAssembly - Class</color>");
                    y += 65f;

					Component[] components = debugObject.GetComponents(typeof(Component));
					if (settings.objectDebugShowChildren)
						components = debugObject.GetComponentsInChildren(typeof(Component));
					components = components.Distinct().ToArray();

					foreach (Component component in components)
					{
						Type type = component.GetType();
						string assembly = type.Assembly.GetName().Name;

						// Skip core components if hidden.
						if (!settings.objectDebugShowCore && assembly == "Assembly-CSharp")
							continue;

						// Skip Unity components if hidden.
						if (!settings.objectDebugShowUnity && assembly.Contains("UnityEngine"))
							continue;

						GUI.Label(new Rect(x, y, contentWidth, 20f), $"{assembly} - {type.Name} {(settings.objectDebugShowChildren && component.transform.parent != null ? "(Child of" + component.transform.parent.name + ")" : "")}");
						y += 22f;
					}
                }
            }
			
			if (settings.showColliders && settings.showColliderHelp)
			{
				width = resolutionX / 6;
				height = 160f;
				x = 0;
				y = resolutionY / 2 - height;

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
			Settings settings = new Settings();
			settings.mode = null;
			settings.car = null;
			settings.slotStage = null;

			slots.Clear();

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
				if (hoveredSlot != null)
					ObjectUtilities.DestroyColliders(hoveredSlot);

				hoveredSlot = null;
				hoveredSlotIndex = 0;
				previousHoveredSlotIndex = 0;
				slotMoverFirstRun = true;
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
				if (selectedSlot != null)
					ObjectUtilities.DestroyColliders(selectedSlot);

				selectedSlot = null;
				selectedSlotResetPosition = Vector3.zero;
				selectedSlotResetRotation.Set(0, 0, 0, 0);
			}
			catch (Exception ex)
			{
				Logger.Log($"Error occurred during slot mover move stage dispose - {ex}", Logger.LogLevel.Warning);
			}
		}
	}
}
