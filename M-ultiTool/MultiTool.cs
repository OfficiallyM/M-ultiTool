using MultiTool.Components;
using MultiTool.Core;
using MultiTool.Modules;
using MultiTool.Utilities;
using MultiTool.Utilities.UI;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TLDLoader;
using UnityEngine;
using Logger = MultiTool.Modules.Logger;
using Settings = MultiTool.Core.Settings;

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

        // Modules.
        internal static GUIRenderer Renderer;
		internal static Config Configuration;
		internal static Keybinds Binds;

		private Settings settings = new Settings();

		internal static Mod mod;
        internal static string configVersion;
		internal static bool isOnMainMenu = true;

		public MultiTool()
		{
			mod = this;

			// Initialise modules.
			try
			{
				Modules.Logger.Init();
				Translator.Init();
				ThumbnailGenerator.Init();

                Renderer = new GUIRenderer();
                Configuration = new Config();
                Binds = new Keybinds();
			}
			catch (Exception ex)
			{
				Modules.Logger.Log($"Module initialisation failed - {ex}", Modules.Logger.LogLevel.Critical);
			}
		}

		// Override functions.
		public override void OnMenuLoad()
		{
            // Set the configuration path.
            Configuration.SetConfigPath(Path.Combine(ModLoader.GetModConfigFolder(this), "Config.json"));

            configVersion = Configuration.GetVersion();
            Configuration.UpdateVersion();
			isOnMainMenu = true;

			Renderer.OnMenuLoad();
        }

		public override void OnGUI()
		{
			Renderer.OnGUI();
		}

        public override void OnLoad()
		{
			Translator.SetLanguage(mainscript.M.menu.language.languageNames[mainscript.M.menu.language.selectedLanguage]);
			isOnMainMenu = false;

			GameObject controller = new GameObject("M-ultiTool");
			controller.AddComponent<DataFetcher>();

			// Load the GUI Renderer.
			Renderer.OnLoad();
		}

		public override void Update()
		{
            Renderer.Update();

			// Delete mode.
			if (settings.deleteMode)
			{
				try
				{
					if (Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.deleteMode).key) && mainscript.M.player.seat == null)
					{
						Physics.Raycast(mainscript.M.player.Cam.transform.position, mainscript.M.player.Cam.transform.forward, out var raycastHit, float.PositiveInfinity, mainscript.M.player.useLayer);

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
					Modules.Logger.Log($"Failed to delete entity - {ex}", Modules.Logger.LogLevel.Warning);
				}
			}

			switch (settings.mode)
			{
				case "colorPicker":
					if (Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.action1).key) && !Renderer.show)
					{
						Physics.Raycast(mainscript.M.player.Cam.transform.position, mainscript.M.player.Cam.transform.forward, out var raycastHit, float.PositiveInfinity, mainscript.M.player.useLayer);
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

					if (Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.action2).key) && !Renderer.show)
					{
						Physics.Raycast(mainscript.M.player.Cam.transform.position, mainscript.M.player.Cam.transform.forward, out var raycastHit, float.PositiveInfinity, mainscript.M.player.useLayer);
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
				case "scale":
					if (!Renderer.show)
					{
                        // Select object.
                        if (Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.action1).key))
                        {
                            Physics.Raycast(mainscript.M.player.Cam.transform.position, mainscript.M.player.Cam.transform.forward, out var raycastHit, float.PositiveInfinity, mainscript.M.player.useLayer);
                            if (raycastHit.collider != null && raycastHit.collider.gameObject != null)
                            {
                                GameObject hitGameObject = raycastHit.collider.transform.gameObject;

                                // Recurse upwards to find a tosaveitemscript.
                                tosaveitemscript save = hitGameObject.GetComponentInParent<tosaveitemscript>();

                                // Can't find the tosaveitemscript, return early.
                                if (save == null)
                                {
                                    GUIRenderer.selectedObject = null;
                                    return;
                                }

                                GUIRenderer.selectedObject = save;
                                return;
                            }
                            GUIRenderer.selectedObject = null;
                        }

						if (GUIRenderer.selectedObject != null)
						{
							// Return early if looking at terrain.
							if (GUIRenderer.selectedObject.GetComponent<terrainscript>() != null)
								return;

							tosaveitemscript save = GUIRenderer.selectedObject.GetComponent<tosaveitemscript>();
							bool update = false;

							Vector3 scale = GUIRenderer.selectedObject.transform.localScale;
							float scaleValue = GUIRenderer.scaleValue;
							// Scale up.
							bool scaleUp = Input.GetKey(Binds.GetKeyByAction((int)Keybinds.Inputs.up).key);
							if (!GUIRenderer.scaleHold)
								scaleUp = Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.up).key);
							if (scaleUp)
							{
								switch (GUIRenderer.axis)
								{
									case "all":
                                        GUIRenderer.selectedObject.transform.localScale = new Vector3(scale.x + scaleValue, scale.y + scaleValue, scale.z + scaleValue);
										break;
									case "x":
										scale.x += scaleValue;
                                        GUIRenderer.selectedObject.transform.localScale = scale;
										break;
									case "y":
										scale.y += scaleValue;
                                        GUIRenderer.selectedObject.transform.localScale = scale;
										break;
									case "z":
										scale.z += scaleValue;
                                        GUIRenderer.selectedObject.transform.localScale = scale;
										break;
								}
								update = true;
							}

							// Scale down.
							bool scaleDown = Input.GetKey(Binds.GetKeyByAction((int)Keybinds.Inputs.down).key);
							if (!GUIRenderer.scaleHold)
								scaleDown = Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.down).key);
							if (scaleDown)
							{
								switch (GUIRenderer.axis)
								{
									case "all":
                                        GUIRenderer.selectedObject.transform.localScale = new Vector3(scale.x - scaleValue, scale.y - scaleValue, scale.z - scaleValue);
										break;
									case "x":
										scale.x -= scaleValue;
                                        GUIRenderer.selectedObject.transform.localScale = scale;
										break;
									case "y":
										scale.y -= scaleValue;
                                        GUIRenderer.selectedObject.transform.localScale = scale;
										break;
									case "z":
										scale.z -= scaleValue;
                                        GUIRenderer.selectedObject.transform.localScale = scale;
										break;
								}
								update = true;
							}

							// Reset scale to default.
							if (Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.action4).key))
							{
								// No easy way to store default, just assume it's 1.
								switch (GUIRenderer.axis)
								{
									case "all":
                                        GUIRenderer.selectedObject.transform.localScale = new Vector3(1, 1, 1);
										break;
									case "x":
										scale.x = 1;
                                        GUIRenderer.selectedObject.transform.localScale = scale;
										break;
									case "y":
										scale.y = 1;
                                        GUIRenderer.selectedObject.transform.localScale = scale;
										break;
									case "z":
										scale.z = 1;
                                        GUIRenderer.selectedObject.transform.localScale = scale;
										break;
								}
								update = true;
							}


							// Trigger scale save if available.
							if (save != null && update)
							{
								SaveUtilities.UpdateScale(new ScaleData()
								{
									ID = save.idInSave,
									scale = GUIRenderer.selectedObject.transform.localScale
								});
							}
						}

						// Axis selection control.
						if (Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.action3).key))
						{
							int currentIndex = Array.FindIndex(GUIRenderer.axisOptions, a => a == GUIRenderer.axis);
							if (currentIndex == -1 || currentIndex == GUIRenderer.axisOptions.Length - 1)
								GUIRenderer.axis = GUIRenderer.axisOptions[0];
							else
								GUIRenderer.axis = GUIRenderer.axisOptions[currentIndex + 1];
						}

						// Scale value selection control.
						if (Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.action5).key))
						{
							int currentIndex = Array.FindIndex(GUIRenderer.scaleOptions, s => s == GUIRenderer.scaleValue);
							if (currentIndex == -1 || currentIndex == GUIRenderer.scaleOptions.Length - 1)
								GUIRenderer.scaleValue = GUIRenderer.scaleOptions[0];
							else
								GUIRenderer.scaleValue = GUIRenderer.scaleOptions[currentIndex + 1];
						}

						if (Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.select).key))
						{
							GUIRenderer.scaleHold = !GUIRenderer.scaleHold;
						}
					}
					break;
				case "objectRegenerator":
					// Select object.
					if (Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.action1).key))
					{
						Physics.Raycast(mainscript.M.player.Cam.transform.position, mainscript.M.player.Cam.transform.forward, out var raycastHit, float.PositiveInfinity, mainscript.M.player.useLayer);
						if (raycastHit.collider != null && raycastHit.collider.gameObject != null)
						{
							GameObject hitGameObject = raycastHit.collider.transform.gameObject;

							// Recurse upwards to find a tosaveitemscript.
							tosaveitemscript save = hitGameObject.GetComponentInParent<tosaveitemscript>();

							// Can't find the tosaveitemscript, return early.
							if (save == null) return;

							GUIRenderer.selectedObject = save;
							return;
						}
						GUIRenderer.selectedObject = null;
					}

					// Regenerate object.
					if (Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.action4).key))
					{
						if (GUIRenderer.selectedObject != null)
						{
							tosaveitemscript save = GUIRenderer.selectedObject;
							GameObject gameObject = save.gameObject;
							Item prefab = GUIRenderer.items.Where(i => i.gameObject.name == gameObject.name.Replace("(Clone)", "")).FirstOrDefault();
							if (prefab == null)
								return;

							Vector3 position = gameObject.transform.position;
							Quaternion rotation = gameObject.transform.rotation;

							// Recreate object.
							GameObject spawned = SpawnUtilities.Spawn(prefab, position, rotation);
							GUIRenderer.selectedObject = spawned.GetComponent<tosaveitemscript>();

							// Handle attached children.
							foreach (attachablescript attached in gameObject.GetComponentsInChildren<attachablescript>())
							{
								if (attached.targetTosave == null || attached.targetTosave.gameObject != gameObject) continue;

								attached.Detach();
								attached.targetTosave = spawned.GetComponent<tosaveitemscript>();
								attached.Load(attached.pointLocalPos);
							}

							// Re-Set object parent if required.
							attachablescript attach = gameObject.GetComponent<attachablescript>();
							if (attach != null && attach.targetTosave != null)
							{
								attachablescript newAttach = spawned.GetComponent<attachablescript>();
								if (newAttach != null)
								{
									tosaveitemscript attachSave = attach.targetTosave;
									attach.Detach();
									newAttach.targetTosave = attachSave;
									newAttach.Load(attach.pointLocalPos);
								}
							}

							partslotscript oldSlot = gameObject.GetComponent<partscript>()?.slot;

							// Destroy the old object.
							save.removeFromMemory = true;
							foreach (tosaveitemscript component in gameObject.GetComponentsInChildren<tosaveitemscript>())
							{
								component.removeFromMemory = true;
							}
							UnityEngine.Object.Destroy(gameObject);

							// Mount the new part if it was previously mounted.
							// TODO: Doesn't actually mount.
							// Also, anything mounted to something you're regenerating gets destroyed.
							if (oldSlot != null)
							{
								partscript part = spawned.GetComponent<partscript>();
								if (oldSlot != null)
								{
									oldSlot.Craft(part);
									part.tosaveitem.Claim(false);
								}
							}
						}
					}
					break;
				case "weightChanger":
					// Select object.
					if (Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.action1).key))
					{
						Physics.Raycast(mainscript.M.player.Cam.transform.position, mainscript.M.player.Cam.transform.forward, out var raycastHit, float.PositiveInfinity, mainscript.M.player.useLayer);
						if (raycastHit.collider != null && raycastHit.collider.gameObject != null)
						{
							GameObject hitGameObject = raycastHit.collider.transform.gameObject;

							// Recurse upwards to find a tosaveitemscript.
							tosaveitemscript save = hitGameObject.GetComponentInParent<tosaveitemscript>();

							// Can't find the tosaveitemscript, return early.
							if (save == null)
							{
								GUIRenderer.selectedObject = null;
								return;
							}

							// Object doesn't have mass, return early.
							if (save.GetComponent<massScript>() == null)
							{
								GUIRenderer.selectedObject = null;
								return;
							}

							GUIRenderer.selectedObject = save;
							return;
						}
						GUIRenderer.selectedObject = null;
					}

					// Weight value selection control.
					if (Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.action5).key))
					{
						int currentIndex = Array.FindIndex(GUIRenderer.weightOptions, s => s == GUIRenderer.weightValue);
						if (currentIndex == -1 || currentIndex == GUIRenderer.weightOptions.Length - 1)
							GUIRenderer.weightValue = GUIRenderer.weightOptions[0];
						else
							GUIRenderer.weightValue = GUIRenderer.weightOptions[currentIndex + 1];
					}

					if (Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.select).key))
					{
						GUIRenderer.weightHold = !GUIRenderer.weightHold;
					}

					if (GUIRenderer.selectedObject != null)
					{
						tosaveitemscript save = GUIRenderer.selectedObject.GetComponent<tosaveitemscript>();
						massScript mass = GUIRenderer.selectedObject.GetComponent<massScript>();
						bool update = false;

						float currentMass = mass.OwnMass();

						// Mass increase.
						bool massUp = Input.GetKey(Binds.GetKeyByAction((int)Keybinds.Inputs.up).key);
						if (!GUIRenderer.weightHold)
							massUp = Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.up).key);
						if (massUp)
						{
							mass.SetMass(currentMass + GUIRenderer.weightValue);

							update = true;
						}

						// Mass decrease.
						bool massDown = Input.GetKey(Binds.GetKeyByAction((int)Keybinds.Inputs.down).key);
						if (!GUIRenderer.weightHold)
							massDown = Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.down).key);
						if (massDown)
						{
							mass.SetMass(currentMass - GUIRenderer.weightValue);

							update = true;
						}

						// Reset weight to default.
						if (Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.action4).key))
						{
							WeightData weight = SaveUtilities.GetWeight(save.idInSave);

							if (weight == null)
							{
								Notifications.SendWarning("Weight Changer", "Unable to reset - no default available");
								return;
							}
							else
							{
								mass.SetMass(weight.defaultMass);
								update = true;
							}
						}

						// Trigger mass save if available.
						if (save != null && update)
						{
							SaveUtilities.UpdateWeight(new WeightData()
							{
								ID = save.idInSave,
								mass = mass.OwnMass(),
								defaultMass = currentMass,
							});
						}
					}
					break;
			}
		}

		public override void FixedUpdate()
		{
			Renderer.FixedUpdate();
		}
	}
}