using MultiTool.Components;
using MultiTool.Core;
using MultiTool.Modules;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		public override string Version => "3.2.0.2";
        public override bool LoadInDB => true;
		public override bool LoadInMenu => true;

        // Initialise modules.
        private readonly GUIRenderer renderer;
		private readonly Config config;
		private readonly Keybinds binds;

		private Settings settings = new Settings();

		internal static Mod mod;
        internal static string configVersion;

		private bool loaded = false;
		private bool showDebugString = false;
        private bool reset = false;

		public MultiTool()
		{
			mod = this;

			// Initialise modules.
			try
			{
				Logger.Init();
				Translator.Init();
				ThumbnailGenerator.Init();

				// We can't use GetModConfigFolder here as the mod isn't fully initialised yet.
				string configDirectory = Path.Combine(ModLoader.ModsFolder, "Config", "Mod Settings", ID);

				config = new Config();
				binds = new Keybinds(config);
				renderer = new GUIRenderer(config, binds);
			}
			catch (Exception ex)
			{
				Logger.Log($"Module initialisation failed - {ex}", Logger.LogLevel.Critical);
			}
		}

		// Override functions.
		public override void OnMenuLoad()
		{
			// Check for and delete old spawner.
			string file = Path.Combine(ModLoader.ModsFolder, "SpawnerTLD.dll");
			if (File.Exists(file))
			{
				try
				{
					File.Delete(file);
					Logger.Log("Detected and removed old SpawnerTLD.");
				}
				catch (Exception ex)
				{
					Logger.Log($"Failed to delete old SpawnerTLD, this will cause conflicts - {ex}", Logger.LogLevel.Critical);
				}
			}

			// Set the configuration path.
			config.SetConfigPath(ModLoader.GetModConfigFolder(this) + "\\Config.json");

            configVersion = config.GetVersion();
            config.UpdateVersion();

            loaded = true;

			renderer.enabled = CoreUtilities.HasPassedValidation();
		}

		public override void OnGUI()
		{
			renderer.OnGUI();
		}

        public override void DbLoad()
        {
            GameObject controller = new GameObject("M-ultiToolController");
            controller.transform.SetParent(mainscript.s.transform);
            controller.AddComponent<Controller>();
        }

        public override void OnLoad()
		{
			// Return early if M-ultiTool is disabled.
			if (!renderer.enabled)
				return;

			// Run spawner migration.
			MigrateUtilities.MigrateFromSpawner();

            // Load the GUI renderer.
            renderer.OnLoad();
		}

		public override void Update()
		{
			// Return early if M-ultiTool isn't enabled.
			if (!renderer.enabled)
            {
                if (mainscript.s == null && !reset && PlayerPrefs.GetFloat("DistanceDriven") == 0)
                {
                    CoreUtilities.HasPassedValidation();
                    reset = true;
                }
				return;
            }

			renderer.Update();

			// Delete mode.
			if (settings.deleteMode)
			{
				try
				{
					if (Input.GetKeyDown(binds.GetKeyByAction((int)Keybinds.Inputs.deleteMode).key) && mainscript.s.player.seat == null)
					{
						Physics.Raycast(mainscript.s.player.Cam.transform.position, mainscript.s.player.Cam.transform.forward, out var raycastHit, float.PositiveInfinity, mainscript.s.player.useLayer);

						// Require objects to have a tosaveitemscript in order to delete them.
						// This prevents players from deleting the world, buildings and other
						// stuff that would break the game.
						tosaveitemscript save = raycastHit.transform.gameObject.GetComponent<tosaveitemscript>();
						if (save != null)
						{
							//save.removeFromMemory = true;

							foreach (tosaveitemscript component in raycastHit.transform.root.GetComponentsInChildren<tosaveitemscript>())
							{
								//component.removeFromMemory = true;
							}
							UnityEngine.Object.Destroy(raycastHit.transform.root.gameObject);
						}
					}
				}
				catch (Exception ex)
				{
					Logger.Log($"Failed to delete entity - {ex}", Logger.LogLevel.Warning);
				}
			}

			switch (settings.mode)
			{
				case "colorPicker":
					if (Input.GetKeyDown(binds.GetKeyByAction((int)Keybinds.Inputs.action1).key) && !renderer.show)
					{
						Physics.Raycast(mainscript.s.player.Cam.transform.position, mainscript.s.player.Cam.transform.forward, out var raycastHit, float.PositiveInfinity, mainscript.s.player.useLayer);
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
							foreach (Renderer renderer in part.renderers)
							{
								if (renderer.material == null)
									continue;

								objectColor = renderer.material.color;
							}
						}

						renderer.SetColor(objectColor);
					}

					if (Input.GetKeyDown(binds.GetKeyByAction((int)Keybinds.Inputs.action2).key) && !renderer.show)
					{
						Physics.Raycast(mainscript.s.player.Cam.transform.position, mainscript.s.player.Cam.transform.forward, out var raycastHit, float.PositiveInfinity, mainscript.s.player.useLayer);
						GameObject hitGameObject = raycastHit.transform.gameObject;
						partconditionscript part = hitGameObject.transform.root.GetComponent<partconditionscript>();
						sprayscript spray = hitGameObject.transform.root.GetComponent<sprayscript>();

						// Return early if hit GameObject has no partconditionscript or sprayscript.
						if (part == null && spray == null)
							return;

						if (spray != null)
						{
							spray.color.color = renderer.GetColor();
							spray.UpdColor();
						}
						else
							GameUtilities.Paint(renderer.GetColor(), part);
					}
					break;
				case "scale":
					if (!renderer.show)
					{
                        // Select object.
                        if (Input.GetKeyDown(binds.GetKeyByAction((int)Keybinds.Inputs.action6).key))
                        {
                            Physics.Raycast(mainscript.s.player.Cam.transform.position, mainscript.s.player.Cam.transform.forward, out var raycastHit, float.PositiveInfinity, mainscript.s.player.useLayer);
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
							bool scaleUp = Input.GetKey(binds.GetKeyByAction((int)Keybinds.Inputs.up).key);
							if (!GUIRenderer.scaleHold)
								scaleUp = Input.GetKeyDown(binds.GetKeyByAction((int)Keybinds.Inputs.up).key);
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
							bool scaleDown = Input.GetKey(binds.GetKeyByAction((int)Keybinds.Inputs.down).key);
							if (!GUIRenderer.scaleHold)
								scaleDown = Input.GetKeyDown(binds.GetKeyByAction((int)Keybinds.Inputs.down).key);
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
							if (Input.GetKeyDown(binds.GetKeyByAction((int)Keybinds.Inputs.action4).key))
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
						if (Input.GetKeyDown(binds.GetKeyByAction((int)Keybinds.Inputs.action3).key))
						{
							int currentIndex = Array.FindIndex(GUIRenderer.axisOptions, a => a == GUIRenderer.axis);
							if (currentIndex == -1 || currentIndex == GUIRenderer.axisOptions.Length - 1)
								GUIRenderer.axis = GUIRenderer.axisOptions[0];
							else
								GUIRenderer.axis = GUIRenderer.axisOptions[currentIndex + 1];
						}

						// Scale value selection control.
						if (Input.GetKeyDown(binds.GetKeyByAction((int)Keybinds.Inputs.action5).key))
						{
							int currentIndex = Array.FindIndex(GUIRenderer.scaleOptions, s => s == GUIRenderer.scaleValue);
							if (currentIndex == -1 || currentIndex == GUIRenderer.scaleOptions.Length - 1)
								GUIRenderer.scaleValue = GUIRenderer.scaleOptions[0];
							else
								GUIRenderer.scaleValue = GUIRenderer.scaleOptions[currentIndex + 1];
						}

						if (Input.GetKeyDown(binds.GetKeyByAction((int)Keybinds.Inputs.select).key))
						{
							GUIRenderer.scaleHold = !GUIRenderer.scaleHold;
						}
					}
					break;
				case "objectRegenerator":
					// Select object.
					if (Input.GetKeyDown(binds.GetKeyByAction((int)Keybinds.Inputs.action1).key))
					{
						Physics.Raycast(mainscript.s.player.Cam.transform.position, mainscript.s.player.Cam.transform.forward, out var raycastHit, float.PositiveInfinity, mainscript.s.player.useLayer);
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
					if (Input.GetKeyDown(binds.GetKeyByAction((int)Keybinds.Inputs.action4).key))
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
								attached.Load(new Vector3d(attached.pointLocalPos));
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
									newAttach.Load(new Vector3d(attach.pointLocalPos));
								}
							}

							partslotscript oldSlot = gameObject.GetComponent<partscript>()?.slot;

							// Destroy the old object.
							//save.removeFromMemory = true;
							foreach (tosaveitemscript component in gameObject.GetComponentsInChildren<tosaveitemscript>())
							{
								//component.removeFromMemory = true;
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
									oldSlot.Craft(part, true);
									//part.tosaveitem.Claim(false);
								}
							}
						}
					}
					break;
			}

            // Return early if not in-game.
            if (mainscript.s.player == null) return;

			// Fake the player being on a ladder to remove the gravity during noclip.
			// This is needed because setting useGravity directly on the player RigidBody
			// gets enabled again immediately by the fpscontroller.
			if (settings.noclip)
				mainscript.s.player.ladderV = 1;

			// Apply player settings.
			if (GUIRenderer.playerData != null)
			{
				PlayerData playerData = GUIRenderer.playerData;
				fpscontroller player = mainscript.s.player;
				if (player != null)
				{
					player.FdefMaxSpeed = playerData.walkSpeed;
					player.FrunM = playerData.runSpeed;
					player.FjumpForce = playerData.jumpForce;
					mainscript.s.pushForce = playerData.pushForce;
					player.maxWeight = playerData.carryWeight;
					player.maxPickupForce = playerData.pickupForce;
					if (player.mass != null && player.mass.Mass() != playerData.mass)
						player.mass.SetMass(playerData.mass);

					if (player.inHandP != null && player.inHandP.weapon != null)
					{
						tosaveitemscript save = player.inHandP.weapon.GetComponent<tosaveitemscript>();

						player.inHandP.weapon.infinite = playerData.infiniteAmmo;
					}
				}
			}
		}

        internal static void LateUpdate()
        {
            // This was here for something that didn't work, but it's worth keeping for the future.
        }

		public override void Config()
		{
			SettingAPI setting = new SettingAPI(this);
			showDebugString = setting.GUICheckbox(showDebugString, "Show debug string", 10, 10);
			if (showDebugString && loaded)
				setting.GUIDescriptionText($"Debug string: {PlayerPrefs.GetString("Data", string.Empty)}", 60, 10, 40);
		}
	}
}