using MultiTool.Core;
using MultiTool.Extensions;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MultiTool.Modules;
using Logger = MultiTool.Modules.Logger;
using Settings = MultiTool.Core.Settings;
using TLDLoader;
using System.Runtime.CompilerServices;
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
			float startingX = dimensions.x + 10f;
			float x = startingX;
			float y = dimensions.y + 10f;
			float buttonWidth = 200f;
			float buttonHeight = 20f;
			float sliderWidth = 300f;
			float headerWidth = dimensions.width - 20f;
			float headerHeight = 40f;

			float scrollHeight = 580f;

			bool update = false;

            PlayerData activePlayerData = isPerSave ? playerData : globalPlayerData;

			currentPosition = GUI.BeginScrollView(new Rect(x, y, dimensions.width - 20f, dimensions.height - 20f), currentPosition, new Rect(x, y, dimensions.width - 20f, scrollHeight), new GUIStyle(), GUI.skin.verticalScrollbar);

			// God toggle.
			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), Accessibility.GetAccessibleString("God mode", settings.godMode)))
			{
				settings.godMode = !settings.godMode;
                kaposztaleves.s.settings.god = settings.godMode;
			}
			x += buttonWidth + 10f;

			// Noclip toggle.
			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), Accessibility.GetAccessibleString("Noclip", settings.noclip)))
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

			x += buttonWidth + 10f;

			// Infinite ammo toggle.
			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), Accessibility.GetAccessibleString("Infinite ammo", activePlayerData.infiniteAmmo)))
			{
                activePlayerData.infiniteAmmo = !activePlayerData.infiniteAmmo;
				update = true;
			}

			x = startingX;
			y += buttonHeight + 10f;

            // Per save/global toggle.
            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), Accessibility.GetAccessibleString("Set to global player settings", "Set to per-save player settings", isPerSave)))
            {
                isPerSave = !isPerSave;
                SaveUtilities.UpdateIsPlayerDataPerSave(isPerSave);
                ApplyPlayerData();
            }

            y += buttonHeight + 10f;

            // Walk speed.
            GUI.Label(new Rect(x, y, headerWidth, buttonHeight), $"Walk speed: {activePlayerData.walkSpeed} (Default: {defaultPlayerData.walkSpeed})", GUIRenderer.labelStyle);
			y += buttonHeight;
			float walkSpeed = GUI.HorizontalSlider(new Rect(x, y, sliderWidth, buttonHeight), activePlayerData.walkSpeed, 1f, 10f);
			x += sliderWidth + 10f;
			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Reset"))
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
			x = startingX;
			y += buttonHeight + 10f;

			// Run speed.
			GUI.Label(new Rect(x, y, headerWidth, buttonHeight), $"Run speed: {activePlayerData.runSpeed} (Default: {defaultPlayerData.runSpeed})", GUIRenderer.labelStyle);
			y += buttonHeight;
			float runSpeed = GUI.HorizontalSlider(new Rect(x, y, sliderWidth, buttonHeight), activePlayerData.runSpeed, 1f, 10f);
			x += sliderWidth + 10f;
			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Reset"))
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
			x = startingX;
			y += buttonHeight + 10f;

			// Jump force.
			GUI.Label(new Rect(x, y, headerWidth, buttonHeight), $"Jump force: {activePlayerData.jumpForce / 100} (Default: {defaultPlayerData.jumpForce / 100})", GUIRenderer.labelStyle);
			y += buttonHeight;
			float jumpForce = GUI.HorizontalSlider(new Rect(x, y, sliderWidth, buttonHeight), activePlayerData.jumpForce / 100, 1f, 2000f);
			x += sliderWidth + 10f;
			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Reset"))
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
			x = startingX;
			y += buttonHeight + 10f;

			// Push force.
			GUI.Label(new Rect(x, y, headerWidth, buttonHeight), $"Push force: {activePlayerData.pushForce / 10} (Default: {defaultPlayerData.pushForce / 10})", GUIRenderer.labelStyle);
			y += buttonHeight;
			float pushForce = GUI.HorizontalSlider(new Rect(x, y, sliderWidth, buttonHeight), activePlayerData.pushForce / 10, 1f, 2000f);
			x += sliderWidth + 10f;
			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Reset"))
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
			x = startingX;
			y += buttonHeight + 10f;

			// Carry weight.
			GUI.Label(new Rect(x, y, headerWidth, buttonHeight), $"Carry weight: {activePlayerData.carryWeight} (Default: {defaultPlayerData.carryWeight})", GUIRenderer.labelStyle);
			y += buttonHeight;
			float carryWeight = GUI.HorizontalSlider(new Rect(x, y, sliderWidth, buttonHeight), activePlayerData.carryWeight, 1f, 1000f);
			x += sliderWidth + 10f;
			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Reset"))
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
			x = startingX;
			y += buttonHeight + 10f;

			// Pickup force.
			GUI.Label(new Rect(x, y, headerWidth, buttonHeight), $"Pickup force: {activePlayerData.pickupForce} (Default: {defaultPlayerData.pickupForce})", GUIRenderer.labelStyle);
			y += buttonHeight;
			float pickupForce = GUI.HorizontalSlider(new Rect(x, y, sliderWidth, buttonHeight), activePlayerData.pickupForce, 1f, 1000f);
			x += sliderWidth + 10f;
			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Reset"))
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
			x = startingX;
			y += buttonHeight + 10f;

			// Fire speed.
            // TODO: Rewrite saving.
			//if (mainscript.s.player.inHandP != null && mainscript.s.player.inHandP.weapon != null)
			//{
			//	tosaveitemscript save = mainscript.s.player.inHandP.weapon.GetComponent<tosaveitemscript>();

			//	if (weaponData != null)
			//	{
			//		GUI.Label(new Rect(x, y, headerWidth, buttonHeight), $"Fire rate: {weaponData.fireRate} (Default: {weaponData.defaultFireRate})", GUIRenderer.labelStyle);
			//		y += buttonHeight;
			//		float fireRate = GUI.HorizontalSlider(new Rect(x, y, sliderWidth, buttonHeight), weaponData.fireRate, 0.001f, 0.5f);
			//		x += sliderWidth + 10f;
			//		if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Reset"))
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

			//		x = startingX;
			//		y += buttonHeight + 10f;
			//	}
			//}

			// Mass.
			GUI.Label(new Rect(x, y, headerWidth, buttonHeight), $"Mass: {activePlayerData.mass} (Default: {defaultPlayerData.mass})", GUIRenderer.labelStyle);
			y += buttonHeight;
			float mass = GUI.HorizontalSlider(new Rect(x, y, sliderWidth, buttonHeight), activePlayerData.mass, 1f, 1000f);
			x += sliderWidth + 10f;
			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Reset"))
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
			x = startingX;
			y += buttonHeight + 10f;

			// Bladder control.
			GUI.Label(new Rect(x, y, headerWidth, buttonHeight), $"Bladder control:", GUIRenderer.labelStyle);
			y += buttonHeight + 10f;

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
				GUI.Label(new Rect(x, y, buttonWidth / 3, buttonHeight), fluid.Key.ToString().ToSentenceCase(), GUIRenderer.labelStyle);
				x += buttonWidth / 3;
				int percentage = Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(x, y, sliderWidth, buttonHeight), fluid.Value, 0, 100));
				if (percentage + (pissPercentage - fluid.Value) <= 100)
				{
					tempPiss[fluid.Key] = percentage;
					changed = true;
				}
				x += sliderWidth + 5f;
				GUI.Label(new Rect(x, y, buttonWidth, buttonHeight), $"{percentage}%", GUIRenderer.labelStyle);
				x = startingX;
				y += buttonHeight;
			}

			if (changed)
				GUIRenderer.piss = tempPiss;

			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Get current"))
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

			x += buttonWidth + 10f;

			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Apply piss"))
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
			
			x = startingX;
			y += buttonHeight + 10f;

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

			GUI.EndScrollView();
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
