using MultiTool.Core;
using MultiTool.Extensions;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MultiTool.Modules;
using Logger = MultiTool.Modules.Logger;
using Settings = MultiTool.Core.Settings;
using MultiTool.Utilities.UI;

namespace MultiTool.Tabs
{
	internal class PlayerTab : Tab
	{
		public override string Name => "Player";

		private Vector2 _currentPosition;
		private Settings _settings = new Settings();

        private PlayerData _playerData;
        private PlayerData _globalPlayerData;
        private PlayerData _defaultPlayerData;

        private bool _isPerSave = false;

		private List<buildingscript> _previousBuildingTeleports = new List<buildingscript>();

        public override void OnRegister()
        {
            // Get default player data values.
            if (_defaultPlayerData == null)
            {
                fpscontroller player = mainscript.M.player;
				_defaultPlayerData = new PlayerData()
				{
					walkSpeed = player.FdefMaxSpeed,
					runSpeed = player.FrunM,
					jumpForce = player.FjumpForce,
					pushForce = mainscript.M.pushForce,
					carryWeight = player.maxWeight,
					pickupForce = player.maxPickupForce,
					throwForce = player.throwForceM,
					pedalSpeed = player.PedalingRpm,
					mass = player != null && player.mass != null ? player.mass.Mass() : 0,
                    infiniteAmmo = false,
                };
            }
            _playerData = SaveUtilities.LoadPlayerData(_defaultPlayerData);
            _globalPlayerData = SaveUtilities.LoadGlobalPlayerData(_defaultPlayerData);
            _isPerSave = SaveUtilities.LoadIsPlayerDataPerSave();
            ApplyPlayerData();
        }

        public override void RenderTab(Rect dimensions)
		{
			bool update = false;

            PlayerData activePlayerData = _isPerSave ? _playerData : _globalPlayerData;

            GUILayout.BeginArea(dimensions);
            GUILayout.BeginVertical();
            _currentPosition = GUILayout.BeginScrollView(_currentPosition);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Accessibility.GetAccessibleString("God mode", _settings.godMode), GUILayout.MaxWidth(200)))
            {
                _settings.godMode = !_settings.godMode;
				mainscript.M.ChGodMode(_settings.godMode);
            }
            GUILayout.Space(10);

            if (GUILayout.Button(Accessibility.GetAccessibleString("Noclip", _settings.noclip), GUILayout.MaxWidth(200)))
            {
                _settings.noclip = !_settings.noclip;

                if (_settings.noclip)
                {
                    Noclip noclip = mainscript.M.player.gameObject.AddComponent<Noclip>();

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

                        // Re-enable colliders.
                        foreach (Collider collider in mainscript.M.player.C)
                        {
                            collider.enabled = true;
                        }
                    }
                }
            }
			GUILayout.EndHorizontal();
            GUILayout.Space(10);

            GUILayout.BeginVertical(GUILayout.MaxWidth(dimensions.width / 2));
            if (GUILayout.Button(Accessibility.GetAccessibleString("Set to global player settings", "Set to per-save player settings", _isPerSave), GUILayout.MaxWidth(200)))
            {
                _isPerSave = !_isPerSave;
                SaveUtilities.UpdateIsPlayerDataPerSave(_isPerSave);
                ApplyPlayerData();
            }
            GUILayout.Space(20);

            if (GUILayout.Button(Accessibility.GetAccessibleString("Infinite ammo", activePlayerData.infiniteAmmo), GUILayout.MaxWidth(200)))
            {
                activePlayerData.infiniteAmmo = !activePlayerData.infiniteAmmo;
                update = true;
            }
            GUILayout.Space(10);

            // Walk speed.
            GUILayout.Label($"Walk speed: {activePlayerData.walkSpeed} (Default: {_defaultPlayerData.walkSpeed})");
			float.TryParse(GUILayout.TextField(activePlayerData.walkSpeed.ToString(), GUILayout.MaxWidth(200)), out float walkSpeed);
			GUILayout.Space(2);
            GUILayout.BeginHorizontal();
			walkSpeed = GUILayout.HorizontalSlider(walkSpeed, 1f, 10f);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
			{
                activePlayerData.walkSpeed = _defaultPlayerData.walkSpeed;
				update = true;
			}
			else
			{
				walkSpeed = (float)Math.Round(walkSpeed, 2);
				if (walkSpeed != activePlayerData.walkSpeed)
				{
                    activePlayerData.walkSpeed = walkSpeed;
					update = true;
				}
			}
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            // Run speed.
            GUILayout.Label($"Run speed: {activePlayerData.runSpeed} (Default: {_defaultPlayerData.runSpeed})");
			float.TryParse(GUILayout.TextField(activePlayerData.runSpeed.ToString(), GUILayout.MaxWidth(200)), out float runSpeed);
			GUILayout.Space(2);
			GUILayout.BeginHorizontal();
            runSpeed = GUILayout.HorizontalSlider(runSpeed, 1f, 10f);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
			{
                activePlayerData.runSpeed = _defaultPlayerData.runSpeed;
				update = true;
			}
			else
			{
				runSpeed = (float)Math.Round(runSpeed, 2);
				if (runSpeed != activePlayerData.runSpeed)
				{
                    activePlayerData.runSpeed = runSpeed;
					update = true;
				}
			}
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            // Jump force.
            GUILayout.Label($"Jump force: {activePlayerData.jumpForce / 100} (Default: {_defaultPlayerData.jumpForce / 100})");
			float.TryParse(GUILayout.TextField((activePlayerData.jumpForce / 100).ToString(), GUILayout.MaxWidth(200)), out float jumpForce);
			GUILayout.Space(2);
			GUILayout.BeginHorizontal();
            jumpForce = GUILayout.HorizontalSlider(jumpForce, 1f, 2000f);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
			{
                activePlayerData.jumpForce = _defaultPlayerData.jumpForce;
				update = true;
			}
			else
			{
				jumpForce = Mathf.Round(jumpForce) * 100;
				if (jumpForce != activePlayerData.jumpForce)
				{
                    activePlayerData.jumpForce = jumpForce;
					update = true;
				}
			}
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            // Push force.
            GUILayout.Label($"Push force: {activePlayerData.pushForce / 10} (Default: {_defaultPlayerData.pushForce / 10})");
			float.TryParse(GUILayout.TextField((activePlayerData.pushForce / 10).ToString(), GUILayout.MaxWidth(200)), out float pushForce);
			GUILayout.Space(2);
			GUILayout.BeginHorizontal();
            pushForce = GUILayout.HorizontalSlider(pushForce, 1f, 2000f);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
			{
                activePlayerData.pushForce = _defaultPlayerData.pushForce;
				update = true;
			}
			else
			{
				pushForce = Mathf.Round(pushForce) * 10;
				if (pushForce != activePlayerData.pushForce)
				{
                    activePlayerData.pushForce = pushForce;
					update = true;
				}
			}
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            // Carry weight.
            GUILayout.Label($"Carry weight: {activePlayerData.carryWeight} (Default: {_defaultPlayerData.carryWeight})");
			float.TryParse(GUILayout.TextField(activePlayerData.carryWeight.ToString(), GUILayout.MaxWidth(200)), out float carryWeight);
			GUILayout.Space(2);
			GUILayout.BeginHorizontal();
            carryWeight = GUILayout.HorizontalSlider(carryWeight, 1f, 1000f);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
			{
                activePlayerData.carryWeight = _defaultPlayerData.carryWeight;
				update = true;
			}
			else 
			{
				carryWeight = Mathf.Round(carryWeight);
				if (carryWeight != activePlayerData.carryWeight)
				{
                    activePlayerData.carryWeight = carryWeight;
					update = true;
				}
			}
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            // Pickup force.
            GUILayout.Label($"Pickup force: {activePlayerData.pickupForce} (Default: {_defaultPlayerData.pickupForce})");
			float.TryParse(GUILayout.TextField(activePlayerData.pickupForce.ToString(), GUILayout.MaxWidth(200)), out float pickupForce);
			GUILayout.Space(2);
			GUILayout.BeginHorizontal();
            pickupForce = GUILayout.HorizontalSlider(pickupForce, 1f, 1000f);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
			{
                activePlayerData.pickupForce = _defaultPlayerData.pickupForce;
				update = true;
			}
			else
			{
				pickupForce = Mathf.Round(pickupForce);
				if (pickupForce != activePlayerData.pickupForce)
				{
                    activePlayerData.pickupForce = pickupForce;
					update = true;
				}
			}
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

			// Throw force.
			GUILayout.Label($"Throw force: {activePlayerData.throwForce / 1000} (Default: {_defaultPlayerData.throwForce / 1000})");
			float.TryParse(GUILayout.TextField((activePlayerData.throwForce / 1000).ToString(), GUILayout.MaxWidth(200)), out float throwForce);
			GUILayout.Space(2);
			GUILayout.BeginHorizontal();
			throwForce = GUILayout.HorizontalSlider(throwForce, 1f, 1000f);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
			{
				activePlayerData.throwForce = _defaultPlayerData.throwForce;
				update = true;
			}
			else
			{
				throwForce = Mathf.Round(throwForce) * 1000;
				if (throwForce != activePlayerData.throwForce)
				{
					activePlayerData.throwForce = throwForce;
					update = true;
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(10);

			// Pedal speed.
			GUILayout.Label($"Bike pedal speed: {activePlayerData.pedalSpeed} (Default: {_defaultPlayerData.pedalSpeed})");
			float.TryParse(GUILayout.TextField(activePlayerData.pedalSpeed.ToString(), GUILayout.MaxWidth(200)), out float pedalSpeed);
			GUILayout.Space(2);
			GUILayout.BeginHorizontal();
			pedalSpeed = GUILayout.HorizontalSlider(pedalSpeed, 1f, 10000f);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
			{
				activePlayerData.pedalSpeed = _defaultPlayerData.pedalSpeed;
				update = true;
			}
			else
			{
				pedalSpeed = Mathf.Round(pedalSpeed);
				if (pedalSpeed != activePlayerData.pedalSpeed)
				{
					activePlayerData.pedalSpeed = pedalSpeed;
					update = true;
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(10);

			// Fire speed.
			// TODO: Rewrite saving.
			//if (mainscript.M.player.inHandP != null && mainscript.M.player.inHandP.weapon != null)
			//{
			//	tosaveitemscript save = mainscript.M.player.inHandP.weapon.GetComponent<tosaveitemscript>();

			//	if (weaponData != null)
			//	{
			//		GUILayout.Label($"Fire rate: {weaponData.fireRate} (Default: {weaponData.defaultFireRate})");
			//		GUILayout.BeginHorizontal();
			//		float fireRate = GUILayout.HorizontalSlider(weaponData.fireRate, 0.001f, 0.5f);
			//		if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
			//		{
			//			weaponData.fireRate = weaponData.defaultFireRate;
			//			update = true;
			//		}
			//		else
			//		{
			//			fireRate = (float)Math.Round(fireRate, 3);
			//			if (fireRate != weaponData.fireRate)
			//			{
			//				weaponData.fireRate = fireRate;
			//				update = true;
			//			}
			//		}
			//      GUILayout.EndHorizontal();
			//	}
			//}

			// Mass.
			GUILayout.Label($"Mass: {activePlayerData.mass} (Default: {_defaultPlayerData.mass})");
			float.TryParse(GUILayout.TextField(activePlayerData.mass.ToString(), GUILayout.MaxWidth(200)), out float mass);
			GUILayout.Space(2);
			GUILayout.BeginHorizontal();
            mass = GUILayout.HorizontalSlider(mass, 1f, 1000f);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
			{
                activePlayerData.mass = _defaultPlayerData.mass;
				update = true;
			}
			else
			{
				mass = (float)Math.Round(mass, 2);
				if (mass != activePlayerData.mass)
				{
                    activePlayerData.mass = mass;
					update = true;
				}
			}
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            // Bladder control.
            GUILayout.Label($"Bladder control", "LabelHeader");

			float pissMax = mainscript.M.player.piss.Tank.F.maxC;
			int pissPercentage = 0;

			foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.piss)
			{
				pissPercentage += fluid.Value;
			}

			if (pissPercentage > 100)
				pissPercentage = 100;

			bool changed = false;

			// Deep copy piss dictionary.
			Dictionary<mainscript.fluidenum, int> tempPiss = GUIRenderer.piss.ToDictionary(fluid => fluid.Key, fluid => fluid.Value);

			foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.piss)
			{
                GUILayout.BeginHorizontal();
                GUILayout.Label(fluid.Key.ToString().ToSentenceCase(), GUILayout.MaxWidth(100));
				int percentage = Mathf.RoundToInt(GUILayout.HorizontalSlider(fluid.Value, 0, 100));
				if (percentage + (pissPercentage - fluid.Value) <= 100)
				{
					tempPiss[fluid.Key] = percentage;
					changed = true;
				}
                GUILayout.Space(5);
				GUILayout.Label($"{percentage}%");
                GUILayout.EndHorizontal();
			}

			if (changed)
				GUIRenderer.piss = tempPiss;

            GUILayout.BeginHorizontal();
			if (GUILayout.Button("Get current", GUILayout.MaxWidth(200)))
			{
				tankscript tank = mainscript.M.player.piss.Tank;

				tempPiss = new Dictionary<mainscript.fluidenum, int>();
				foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.piss)
				{
					tempPiss[fluid.Key] = 0;
				}

				GUIRenderer.piss = tempPiss;

				foreach (mainscript.fluid fluid in tank.F.fluids)
				{
					int percentage = (int)(fluid.amount / tank.F.maxC * 100);
					GUIRenderer.piss[fluid.type] = percentage;
				}
			}

			if (GUILayout.Button("Apply piss", GUILayout.MaxWidth(200)))
			{
				tankscript tank = mainscript.M.player.piss.Tank;
				tank.F.fluids.Clear();
				foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.piss)
				{
					if (fluid.Value > 0)
					{
						tank.F.ChangeOne((pissMax / 100) * fluid.Value, fluid.Key);
					}
				}
			}
            GUILayout.EndHorizontal();

			GUILayout.Label("Teleporting", "LabelHeader");

			if (GUILayout.Button(Accessibility.GetAccessibleString("Click to teleport", activePlayerData.clickTeleport) + $"\n(Press {MultiTool.Binds.GetPrettyName((int)Keybinds.Inputs.action1)})", "ButtonPrimaryWrap", GUILayout.MaxWidth(200)))
			{
				activePlayerData.clickTeleport = !activePlayerData.clickTeleport;
				update = true;
			}
			GUILayout.Space(10);

			if (GUILayout.Button("Teleport to next building", GUILayout.MaxWidth(200)))
			{
				buildingscript closestBuilding = GameUtilities.FindNearestBuilding(mainscript.M.player.transform.position, _previousBuildingTeleports);

				if (closestBuilding != null)
				{
					GameUtilities.TeleportPlayerWithParent(closestBuilding.transform.position + Vector3.up * 2f);
					_previousBuildingTeleports.Add(closestBuilding);
					Notifications.Send("Teleport", $"Teleported to {Translator.T(closestBuilding.name.Replace("(Clone)", string.Empty), "POI")}");
				}
				else
					Notifications.Send("Teleport", "No valid building found to teleport to", Notification.NotificationType.Warning);
			}
			GUILayout.Space(10);

			if (_previousBuildingTeleports.Count > 1)
			{
				if (GUILayout.Button("Return to previous teleport", GUILayout.MaxWidth(200)))
				{
					int highestIndex = _previousBuildingTeleports.Count - 1;
					buildingscript previousBuilding = _previousBuildingTeleports[highestIndex - 1];
					GameUtilities.TeleportPlayerWithParent(previousBuilding.transform.position + Vector3.up * 2f);
					_previousBuildingTeleports.RemoveAt(highestIndex);
					_previousBuildingTeleports.Remove(previousBuilding);
				}
				GUILayout.Space(10);
			}

			if (GUILayout.Button("Teleport to starter house", GUILayout.MaxWidth(200)))
			{
				foreach (buildingscript building in savedatascript.d.buildings)
				{
					if (building.name.ToLower().Contains("haz02"))
					{
						Transform transform = building.transform.Find("interiorKitchen");
						if (transform != null)
							GameUtilities.TeleportPlayer(transform.position + Vector3.down * 1f, transform.eulerAngles);
					}
				}
			}

			GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndArea();

			// Trigger update if values have changed.
			if (update)
            {
                if (_isPerSave)
                {
                    SaveUtilities.UpdatePlayerData(activePlayerData);
                    _playerData = activePlayerData;
                }
                else
                {
                    SaveUtilities.UpdateGlobalPlayerData(activePlayerData);
                    _globalPlayerData = activePlayerData;
                }
                ApplyPlayerData();
            }
		}

        public override void Update()
        {
            fpscontroller player = mainscript.M.player;
            if (player == null) return;

			PlayerData activePlayerData = _isPerSave ? _playerData : _globalPlayerData;

			// Apply infinite ammo.
			if (player.inHandP != null && player.inHandP.weapon != null)
				player.inHandP.weapon.infinite = activePlayerData.infiniteAmmo;

			// Click to teleport.
			if (activePlayerData.clickTeleport && 
				!MultiTool.Renderer.show &&
				Input.GetKeyDown(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.action1).key) && 
				Physics.Raycast(player.Cam.transform.position + player.Cam.transform.forward,player.Cam.transform.forward, out RaycastHit hitInfo, float.PositiveInfinity)
			)
				GameUtilities.TeleportPlayerWithParent(hitInfo.point + Vector3.up * 2f);
        }

        private void ApplyPlayerData()
        {
			PlayerData data = _isPerSave ? _playerData : _globalPlayerData;
            fpscontroller player = mainscript.M.player;
			if (player != null)
			{
				player.FdefMaxSpeed = data.walkSpeed;
				player.FrunM = data.runSpeed;
				player.FjumpForce = data.jumpForce;
				mainscript.M.pushForce = data.pushForce;
				player.maxWeight = data.carryWeight;
				player.maxPickupForce = data.pickupForce;
				player.throwForceM = data.throwForce;
				player.PedalingRpm = data.pedalSpeed;
				if (player.mass != null && player.mass.Mass() != data.mass)
					player.mass.SetMass(data.mass);
			}
        }
	}
}
