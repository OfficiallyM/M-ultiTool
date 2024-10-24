using MultiTool.Core;
using MultiTool.Modules;
using MultiTool.Utilities;
using System;
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
		public override string Version => "4.0.0-dev";
		public override bool LoadInMenu => true;

        // Modules.
        internal static GUIRenderer Renderer;
		internal static Config Configuration;
		internal static Keybinds Binds;

		private Settings settings = new Settings();

		internal static Mod mod;
        internal static string configVersion;

		public MultiTool()
		{
			mod = this;

			// Initialise modules.
			try
			{
				Logger.Init();
				Translator.Init();
				ThumbnailGenerator.Init();

                Renderer = new GUIRenderer();
                Configuration = new Config();
                Binds = new Keybinds();
			}
			catch (Exception ex)
			{
				Logger.Log($"Module initialisation failed - {ex}", Logger.LogLevel.Critical);
			}
		}

		// Override functions.
		public override void OnMenuLoad()
		{
            // Set the configuration path.
            Configuration.SetConfigPath(Path.Combine(ModLoader.GetModConfigFolder(this), "Config.json"));

            configVersion = Configuration.GetVersion();
            Configuration.UpdateVersion();
        }

		public override void OnGUI()
		{
            Renderer.OnGUI();
		}

        public override void OnLoad()
		{
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
					if (Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.deleteMode).key) && mainscript.s.player.seat == null)
					{
						Physics.Raycast(mainscript.s.player.Cam.transform.position, mainscript.s.player.Cam.transform.forward, out var raycastHit, float.PositiveInfinity, mainscript.s.player.useLayer);

						// Require objects to have a tosaveitemscript in order to delete them.
						// This prevents players from deleting the world, buildings and other
						// stuff that would break the game.
						tosaveitemscript save = raycastHit.transform.gameObject.GetComponent<tosaveitemscript>();
						if (save != null)
						{
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
					if (Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.action1).key) && !Renderer.show)
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
							foreach (Renderer Renderer in part.renderers)
							{
								if (Renderer.material == null)
									continue;

								objectColor = Renderer.material.color;
							}
						}

						Renderer.SetColor(objectColor);
					}

					if (Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.action2).key) && !Renderer.show)
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
							spray.color.color = Renderer.GetColor();
							spray.UpdColor();
						}
						else
							GameUtilities.Paint(Renderer.GetColor(), part);
					}
					break;
				case "scale":
					if (!Renderer.show)
					{
                        // Select object.
                        if (Input.GetKeyDown(Binds.GetKeyByAction((int)Keybinds.Inputs.action6).key))
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
								if (attached.tosave == null || attached.tosave.gameObject != gameObject) continue;

								attached.Detach();
								attached.tosave = spawned.GetComponent<tosaveitemscript>();
							}

							// Re-Set object parent if required.
							attachablescript attach = gameObject.GetComponent<attachablescript>();
							if (attach != null && attach.tosave != null)
							{
								attachablescript newAttach = spawned.GetComponent<attachablescript>();
								if (newAttach != null)
								{
									tosaveitemscript attachSave = attach.tosave;
									attach.Detach();
									newAttach.tosave = attachSave;
									newAttach.Attach(attach.point);
								}
							}

							partslotscript oldSlot = gameObject.GetComponent<attachablescript>()?.slot;

							// Destroy the old object.
							UnityEngine.Object.Destroy(gameObject);

							// Mount the new part if it was previously mounted.
							// TODO: Doesn't actually mount.
							// Also, anything mounted to something you're regenerating gets destroyed.
							if (oldSlot != null)
							{
								attachablescript part = spawned.GetComponent<attachablescript>();
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
		}
	}
}