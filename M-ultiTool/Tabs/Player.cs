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

		private List<Vector3d> _previousBuildingTeleports = new List<Vector3d>();

        public override void OnRegister()
        {
            // Get default player data values.
            if (_defaultPlayerData == null)
            {
                fpscontroller player = mainscript.s.player;
                _defaultPlayerData = new PlayerData()
                {
                    walkSpeed = player.FdefMaxSpeed,
                    runSpeed = player.FrunM,
                    jumpForce = player.FjumpForce,
                    pushForce = mainscript.s.pushForce,
                    carryWeight = player.maxWeight,
                    pickupForce = player.maxPickupForce,
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
                kaposztaleves.s.settings.god = _settings.godMode;
            }
            GUILayout.Space(10);

            if (GUILayout.Button(Accessibility.GetAccessibleString("Noclip", _settings.noclip), GUILayout.MaxWidth(200)))
            {
                _settings.noclip = !_settings.noclip;

                if (_settings.noclip)
                {
                    Noclip noclip = mainscript.s.player.gameObject.AddComponent<Noclip>();

                    // Disable colliders.
                    foreach (Collider collider in mainscript.s.player.C)
                    {
                        collider.enabled = false;
                    }
                }
                else
                {
                    Noclip noclip = mainscript.s.player.gameObject.GetComponent<Noclip>();
                    if (noclip != null)
                    {
                        UnityEngine.Object.Destroy(noclip);

                        // Re-enable colliders.
                        foreach (Collider collider in mainscript.s.player.C)
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
            GUILayout.BeginHorizontal();
			float walkSpeed = GUILayout.HorizontalSlider(activePlayerData.walkSpeed, 1f, 10f);
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
            GUILayout.BeginHorizontal();
            float runSpeed = GUILayout.HorizontalSlider(activePlayerData.runSpeed, 1f, 10f);
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
            GUILayout.BeginHorizontal();
            float jumpForce = GUILayout.HorizontalSlider(activePlayerData.jumpForce / 100, 1f, 2000f);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
			{
                activePlayerData.jumpForce = _defaultPlayerData.jumpForce;
				update = true;
			}
			else
			{
				jumpForce = Mathf.Round(jumpForce * 100);
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
            GUILayout.BeginHorizontal();
            float pushForce = GUILayout.HorizontalSlider(activePlayerData.pushForce / 10, 1f, 2000f);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
			{
                activePlayerData.pushForce = _defaultPlayerData.pushForce;
				update = true;
			}
			else
			{
				pushForce = Mathf.Round(pushForce * 10);
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
            GUILayout.BeginHorizontal();
            float carryWeight = GUILayout.HorizontalSlider(activePlayerData.carryWeight, 1f, 1000f);
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
            GUILayout.BeginHorizontal();
            float pickupForce = GUILayout.HorizontalSlider(activePlayerData.pickupForce, 1f, 1000f);
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

            // Fire speed.
            // TODO: Rewrite saving.
            //if (mainscript.s.player.inHandP != null && mainscript.s.player.inHandP.weapon != null)
            //{
            //	tosaveitemscript save = mainscript.s.player.inHandP.weapon.GetComponent<tosaveitemscript>();

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
            GUILayout.BeginHorizontal();
            float mass = GUILayout.HorizontalSlider(activePlayerData.mass, 1f, 1000f);
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

			float pissMax = mainscript.s.player.piss.Tank.F.maxC;
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
				tankscript tank = mainscript.s.player.piss.Tank;

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
				tankscript tank = mainscript.s.player.piss.Tank;
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
				poiGenScript.poiClass closestBuilding = GameUtilities.FindNearestBuilding(mainscript.s.player.transform.position, _previousBuildingTeleports.Count > 0 ? 1 : 0);

				if (closestBuilding != null)
				{
					mainscript.s.player.TeleportWithParent(mainscript.UnityPosFromGlobal(closestBuilding.pos) + Vector3.up * 2f);
					_previousBuildingTeleports.Add(closestBuilding.pos);
				}
			}
			GUILayout.Space(10);

			if (_previousBuildingTeleports.Count > 1)
			{
				if (GUILayout.Button("Teleport to previous building", GUILayout.MaxWidth(200)))
				{
					int highestIndex = _previousBuildingTeleports.Count - 1;
					Vector3d previousBuilding = _previousBuildingTeleports[highestIndex - 1];
					mainscript.s.player.TeleportWithParent(mainscript.UnityPosFromGlobal(previousBuilding) + Vector3.up * 2f);
					_previousBuildingTeleports.RemoveAt(highestIndex);
					_previousBuildingTeleports.Remove(previousBuilding);
				}
				GUILayout.Space(10);
			}

			if (GUILayout.Button("Teleport to starter house", GUILayout.MaxWidth(200)))
			{
				// Find all generated buildings.
				List<poiGenScript.poiClass> buildings = new List<poiGenScript.poiClass>();

				for (int index = 0; index < menuhandler.s.currentMainMap.poiGens.Count; index++)
				{
					foreach (KeyValuePair<Vector3d, poiGenScript.chunkClass> chunk in menuhandler.s.currentMainMap.poiGens[index].chunks)
					{
						foreach (KeyValuePair<Vector3d, poiGenScript.poiClass> poi in chunk.Value.pois)
						{
							buildings.Add(poi.Value);
						}
					}
				}

				foreach (poiGenScript.poiClass building in buildings)
				{
					if (building.poiName.ToLower().Contains("haz02"))
					{
						Transform transform = building.pobj.transform.Find("PlayerStart");
						if (transform != null)
							mainscript.s.player.Teleport(transform.position, transform.eulerAngles);
						break;
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
            fpscontroller player = mainscript.s.player;
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
				player.TeleportWithParent(hitInfo.point + Vector3.up * 2f);
        }

        private void ApplyPlayerData()
        {
            if (_isPerSave)
            {
                // Apply player _settings.
                if (_playerData != null)
                {
                    fpscontroller player = mainscript.s.player;
                    if (player != null)
                    {
                        player.FdefMaxSpeed = _playerData.walkSpeed;
                        player.FrunM = _playerData.runSpeed;
                        player.FjumpForce = _playerData.jumpForce;
                        mainscript.s.pushForce = _playerData.pushForce;
                        player.maxWeight = _playerData.carryWeight;
                        player.maxPickupForce = _playerData.pickupForce;
                        if (player.mass != null && player.mass.Mass() != _playerData.mass)
                            player.mass.SetMass(_playerData.mass);
                    }
                }
                return;
            }

            // Apply global player _settings.
            if (_globalPlayerData != null)
            {
                fpscontroller player = mainscript.s.player;
                if (player != null)
                {
                    player.FdefMaxSpeed = _globalPlayerData.walkSpeed;
                    player.FrunM = _globalPlayerData.runSpeed;
                    player.FjumpForce = _globalPlayerData.jumpForce;
                    mainscript.s.pushForce = _globalPlayerData.pushForce;
                    player.maxWeight = _globalPlayerData.carryWeight;
                    player.maxPickupForce = _globalPlayerData.pickupForce;
                    if (player.mass != null && player.mass.Mass() != _globalPlayerData.mass)
                        player.mass.SetMass(_globalPlayerData.mass);
                }
            }
        }
	}
}
