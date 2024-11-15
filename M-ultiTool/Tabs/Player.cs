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

		private Vector2 currentPosition;
		private Settings settings = new Settings();

        private PlayerData playerData;
        private PlayerData globalPlayerData;
        private PlayerData defaultPlayerData;

        private bool isPerSave = false;

        public override void OnRegister()
        {
            // Get default player data values.
            if (defaultPlayerData == null)
            {
                fpscontroller player = mainscript.s.player;
                defaultPlayerData = new PlayerData()
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
            playerData = SaveUtilities.LoadPlayerData(defaultPlayerData);
            globalPlayerData = SaveUtilities.LoadGlobalPlayerData(defaultPlayerData);
            isPerSave = SaveUtilities.LoadIsPlayerDataPerSave();
            ApplyPlayerData();
        }

        public override void RenderTab(Rect dimensions)
		{
			bool update = false;

            PlayerData activePlayerData = isPerSave ? playerData : globalPlayerData;

            GUILayout.BeginArea(dimensions);
            GUILayout.BeginVertical();
            currentPosition = GUILayout.BeginScrollView(currentPosition);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Accessibility.GetAccessibleString("God mode", settings.godMode), GUILayout.MaxWidth(200)))
            {
                settings.godMode = !settings.godMode;
                kaposztaleves.s.settings.god = settings.godMode;
            }
            GUILayout.Space(10);

            if (GUILayout.Button(Accessibility.GetAccessibleString("Noclip", settings.noclip), GUILayout.MaxWidth(200)))
            {
                settings.noclip = !settings.noclip;

                if (settings.noclip)
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
            if (GUILayout.Button(Accessibility.GetAccessibleString("Set to global player settings", "Set to per-save player settings", isPerSave), GUILayout.MaxWidth(200)))
            {
                isPerSave = !isPerSave;
                SaveUtilities.UpdateIsPlayerDataPerSave(isPerSave);
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
            GUILayout.Label($"Walk speed: {activePlayerData.walkSpeed} (Default: {defaultPlayerData.walkSpeed})");
            GUILayout.BeginHorizontal();
			float walkSpeed = GUILayout.HorizontalSlider(activePlayerData.walkSpeed, 1f, 10f);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
			{
                activePlayerData.walkSpeed = defaultPlayerData.walkSpeed;
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
            GUILayout.Label($"Run speed: {activePlayerData.runSpeed} (Default: {defaultPlayerData.runSpeed})");
            GUILayout.BeginHorizontal();
            float runSpeed = GUILayout.HorizontalSlider(activePlayerData.runSpeed, 1f, 10f);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
			{
                activePlayerData.runSpeed = defaultPlayerData.runSpeed;
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
            GUILayout.Label($"Jump force: {activePlayerData.jumpForce / 100} (Default: {defaultPlayerData.jumpForce / 100})");
            GUILayout.BeginHorizontal();
            float jumpForce = GUILayout.HorizontalSlider(activePlayerData.jumpForce / 100, 1f, 2000f);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
			{
                activePlayerData.jumpForce = defaultPlayerData.jumpForce;
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
            GUILayout.Label($"Push force: {activePlayerData.pushForce / 10} (Default: {defaultPlayerData.pushForce / 10})");
            GUILayout.BeginHorizontal();
            float pushForce = GUILayout.HorizontalSlider(activePlayerData.pushForce / 10, 1f, 2000f);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
			{
                activePlayerData.pushForce = defaultPlayerData.pushForce;
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
            GUILayout.Label($"Carry weight: {activePlayerData.carryWeight} (Default: {defaultPlayerData.carryWeight})");
            GUILayout.BeginHorizontal();
            float carryWeight = GUILayout.HorizontalSlider(activePlayerData.carryWeight, 1f, 1000f);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
			{
                activePlayerData.carryWeight = defaultPlayerData.carryWeight;
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
            GUILayout.Label($"Pickup force: {activePlayerData.pickupForce} (Default: {defaultPlayerData.pickupForce})");
            GUILayout.BeginHorizontal();
            float pickupForce = GUILayout.HorizontalSlider(activePlayerData.pickupForce, 1f, 1000f);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
			{
                activePlayerData.pickupForce = defaultPlayerData.pickupForce;
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
            GUILayout.Label($"Mass: {activePlayerData.mass} (Default: {defaultPlayerData.mass})");
            GUILayout.BeginHorizontal();
            float mass = GUILayout.HorizontalSlider(activePlayerData.mass, 1f, 1000f);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
			{
                activePlayerData.mass = defaultPlayerData.mass;
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
            GUILayout.Label($"Bladder control:");

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
			
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndArea();

			// Trigger update if values have changed.
			if (update)
            {
                if (isPerSave)
                {
                    SaveUtilities.UpdatePlayerData(activePlayerData);
                    playerData = activePlayerData;
                }
                else
                {
                    SaveUtilities.UpdateGlobalPlayerData(activePlayerData);
                    globalPlayerData = activePlayerData;
                }
                ApplyPlayerData();
            }
		}

        public override void Update()
        {
            // Apply infinite ammo.
            fpscontroller player = mainscript.s.player;
            if (player == null) return;
            if (player.inHandP != null && player.inHandP.weapon != null)
            {
                if (isPerSave)
                    player.inHandP.weapon.infinite = playerData.infiniteAmmo;
                else
                    player.inHandP.weapon.infinite = globalPlayerData.infiniteAmmo;
            }
        }

        private void ApplyPlayerData()
        {
            if (isPerSave)
            {
                // Apply player settings.
                if (playerData != null)
                {
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
                    }
                }
                return;
            }

            // Apply global player settings.
            if (globalPlayerData != null)
            {
                fpscontroller player = mainscript.s.player;
                if (player != null)
                {
                    player.FdefMaxSpeed = globalPlayerData.walkSpeed;
                    player.FrunM = globalPlayerData.runSpeed;
                    player.FjumpForce = globalPlayerData.jumpForce;
                    mainscript.s.pushForce = globalPlayerData.pushForce;
                    player.maxWeight = globalPlayerData.carryWeight;
                    player.maxPickupForce = globalPlayerData.pickupForce;
                    if (player.mass != null && player.mass.Mass() != globalPlayerData.mass)
                        player.mass.SetMass(globalPlayerData.mass);
                }
            }
        }
	}
}
