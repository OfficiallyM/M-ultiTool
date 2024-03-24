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
using System.Data.Odbc;

namespace MultiTool.Tabs
{
	internal class PlayerTab : Tab
	{
		public override string Name => "Player";

		private Vector2 currentPosition;
		private Settings settings = new Settings();
		public override void RenderTab(Dimensions dimensions)
		{
			float startingX = dimensions.x + 10f;
			float x = startingX;
			float y = dimensions.y + 10f;
			float buttonWidth = 200f;
			float buttonHeight = 20f;
			float sliderWidth = 300f;
			float headerWidth = dimensions.width - 20f;
			float headerHeight = 40f;

			float scrollHeight = 280f;

			bool update = false;

			currentPosition = GUI.BeginScrollView(new Rect(x, y, dimensions.width - 20f, dimensions.height - 20f), currentPosition, new Rect(x, y, dimensions.width - 20f, scrollHeight), new GUIStyle(), GUI.skin.verticalScrollbar);

			// God toggle.
			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleString("God mode", settings.godMode)))
			{
				settings.godMode = !settings.godMode;
				mainscript.M.ChGodMode(settings.godMode);
			}
			x += buttonWidth + 10f;

			// Noclip toggle.
			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleString("Noclip", settings.noclip)))
			{
				settings.noclip = !settings.noclip;

				if (settings.noclip)
				{
					Noclip noclip = mainscript.M.player.gameObject.AddComponent<Noclip>();
					noclip.constructor(GUIRenderer.binds, GUIRenderer.config);
					GUIRenderer.localRotation = mainscript.M.player.transform.localRotation;
					mainscript.M.player.Th.localEulerAngles = new Vector3(0f, 0f, 0f);
					settings.godMode = true;

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

						// Resetting localRotation stops the player from flying infinitely
						// upwards when coming out of noclip.
						// I have no idea why, it just works.
						mainscript.M.player.transform.localRotation = GUIRenderer.localRotation;

						// Re-enable colliders.
						foreach (Collider collider in mainscript.M.player.C)
						{
							collider.enabled = true;
						}
					}

					if (GUIRenderer.noclipGodmodeDisable)
						settings.godMode = false;
				}
				mainscript.M.ChGodMode(settings.godMode);
			}

			x += buttonWidth + 10f;

			// Infinite ammo toggle.
			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleString("Infinite ammo", GUIRenderer.playerData.infiniteAmmo)))
			{
				GUIRenderer.playerData.infiniteAmmo = !GUIRenderer.playerData.infiniteAmmo;
				update = true;
			}

			x = startingX;
			y += buttonHeight + 10f;


			// Walk speed.
			GUI.Label(new Rect(x, y, headerWidth, buttonHeight), $"Walk speed: {GUIRenderer.playerData.walkSpeed} (Default: {GUIRenderer.defaultPlayerData.walkSpeed})", GUIRenderer.labelStyle);
			y += buttonHeight;
			float walkSpeed = GUI.HorizontalSlider(new Rect(x, y, sliderWidth, buttonHeight), GUIRenderer.playerData.walkSpeed, 1f, 10f);
			x += sliderWidth + 10f;
			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Reset"))
			{
				GUIRenderer.playerData.walkSpeed = GUIRenderer.defaultPlayerData.walkSpeed;
				update = true;
			}
			else
			{
				walkSpeed = (float)Math.Round(walkSpeed, 2);
				if (walkSpeed != GUIRenderer.playerData.walkSpeed)
				{
					GUIRenderer.playerData.walkSpeed = walkSpeed;
					update = true;
				}
			}
			x = startingX;
			y += buttonHeight + 10f;

			// Run speed.
			GUI.Label(new Rect(x, y, headerWidth, buttonHeight), $"Run speed: {GUIRenderer.playerData.runSpeed} (Default: {GUIRenderer.defaultPlayerData.runSpeed})", GUIRenderer.labelStyle);
			y += buttonHeight;
			float runSpeed = GUI.HorizontalSlider(new Rect(x, y, sliderWidth, buttonHeight), GUIRenderer.playerData.runSpeed, 1f, 10f);
			x += sliderWidth + 10f;
			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Reset"))
			{
				GUIRenderer.playerData.runSpeed = GUIRenderer.defaultPlayerData.runSpeed;
				update = true;
			}
			else
			{
				runSpeed = (float)Math.Round(runSpeed, 2);
				if (runSpeed != GUIRenderer.playerData.runSpeed)
				{
					GUIRenderer.playerData.runSpeed = runSpeed;
					update = true;
				}
			}
			x = startingX;
			y += buttonHeight + 10f;

			// Jump force.
			GUI.Label(new Rect(x, y, headerWidth, buttonHeight), $"Jump force: {GUIRenderer.playerData.jumpForce / 100} (Default: {GUIRenderer.defaultPlayerData.jumpForce / 100})", GUIRenderer.labelStyle);
			y += buttonHeight;
			float jumpForce = GUI.HorizontalSlider(new Rect(x, y, sliderWidth, buttonHeight), GUIRenderer.playerData.jumpForce / 100, 1f, 2000f);
			x += sliderWidth + 10f;
			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Reset"))
			{
				GUIRenderer.playerData.jumpForce = GUIRenderer.defaultPlayerData.jumpForce;
				update = true;
			}
			else
			{
				jumpForce = Mathf.Round(jumpForce * 100);
				if (jumpForce != GUIRenderer.playerData.jumpForce)
				{
					GUIRenderer.playerData.jumpForce = jumpForce;
					update = true;
				}
			}
			x = startingX;
			y += buttonHeight + 10f;

			// Push force.
			GUI.Label(new Rect(x, y, headerWidth, buttonHeight), $"Push force: {GUIRenderer.playerData.pushForce / 10} (Default: {GUIRenderer.defaultPlayerData.pushForce / 10})", GUIRenderer.labelStyle);
			y += buttonHeight;
			float pushForce = GUI.HorizontalSlider(new Rect(x, y, sliderWidth, buttonHeight), GUIRenderer.playerData.pushForce / 10, 1f, 2000f);
			x += sliderWidth + 10f;
			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Reset"))
			{
				GUIRenderer.playerData.pushForce = GUIRenderer.defaultPlayerData.pushForce;
				update = true;
			}
			else
			{
				pushForce = Mathf.Round(pushForce * 10);
				if (pushForce != GUIRenderer.playerData.pushForce)
				{
					GUIRenderer.playerData.pushForce = pushForce;
					update = true;
				}
			}
			x = startingX;
			y += buttonHeight + 10f;

			// Carry weight.
			GUI.Label(new Rect(x, y, headerWidth, buttonHeight), $"Carry weight: {GUIRenderer.playerData.carryWeight} (Default: {GUIRenderer.defaultPlayerData.carryWeight})", GUIRenderer.labelStyle);
			y += buttonHeight;
			float carryWeight = GUI.HorizontalSlider(new Rect(x, y, sliderWidth, buttonHeight), GUIRenderer.playerData.carryWeight, 1f, 1000f);
			x += sliderWidth + 10f;
			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Reset"))
			{
				GUIRenderer.playerData.carryWeight = GUIRenderer.defaultPlayerData.carryWeight;
				update = true;
			}
			else 
			{
				carryWeight = Mathf.Round(carryWeight);
				if (carryWeight != GUIRenderer.playerData.carryWeight)
				{
					GUIRenderer.playerData.carryWeight = carryWeight;
					update = true;
				}
			}
			x = startingX;
			y += buttonHeight + 10f;

			// Pickup force.
			GUI.Label(new Rect(x, y, headerWidth, buttonHeight), $"Pickup force: {GUIRenderer.playerData.pickupForce} (Default: {GUIRenderer.defaultPlayerData.pickupForce})", GUIRenderer.labelStyle);
			y += buttonHeight;
			float pickupForce = GUI.HorizontalSlider(new Rect(x, y, sliderWidth, buttonHeight), GUIRenderer.playerData.pickupForce, 1f, 1000f);
			x += sliderWidth + 10f;
			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Reset"))
			{
				GUIRenderer.playerData.pickupForce = GUIRenderer.defaultPlayerData.pickupForce;
				update = true;
			}
			else
			{
				pickupForce = Mathf.Round(pickupForce);
				if (pickupForce != GUIRenderer.playerData.pickupForce)
				{
					GUIRenderer.playerData.pickupForce = pickupForce;
					update = true;
				}
			}
			x = startingX;
			y += buttonHeight + 10f;

			// Fire speed.
			if (mainscript.M.player.inHandP != null && mainscript.M.player.inHandP.weapon != null)
			{
				GUI.Label(new Rect(x, y, headerWidth, buttonHeight), $"Fire speed: {GUIRenderer.playerData.fireSpeed} (Default: {GUIRenderer.defaultFireSpeed})", GUIRenderer.labelStyle);
				y += buttonHeight;
				float fireSpeed = GUI.HorizontalSlider(new Rect(x, y, sliderWidth, buttonHeight), GUIRenderer.playerData.fireSpeed, 0.001f, 0.5f);
				x += sliderWidth + 10f;
				if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Reset") && GUIRenderer.defaultFireSpeed != null)
				{
					GUIRenderer.playerData.fireSpeed = GUIRenderer.defaultFireSpeed.GetValueOrDefault();
					update = true;
				}
				else
				{
					fireSpeed = (float)Math.Round(fireSpeed, 3);
					if (fireSpeed != GUIRenderer.playerData.fireSpeed)
					{
						GUIRenderer.playerData.fireSpeed = fireSpeed;
						update = true;
					}
				}
				x = startingX;
				y += buttonHeight + 10f;
			}

			// Trigger update if values have changed.
			if (update)
				GUIRenderer.config.UpdatePlayerData(GUIRenderer.playerData);

			GUI.EndScrollView();
		}
	}
}
