using MultiTool.Core;
using MultiTool.Extensions;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static mainscript;
using static settingsscript;
using UnityEngine;
using MultiTool.Modules;
using Logger = MultiTool.Modules.Logger;

namespace MultiTool.Tabs
{
	internal class VehicleConfigurationTab : Tab
	{
		public override string Name => "Vehicle Configuration";

		private Vector2 currentVehiclePosition;
		public override void RenderTab(Dimensions dimensions)
		{
			float startingCurrVehicleX = dimensions.x + 10f;
			float currVehicleX = startingCurrVehicleX;
			float currVehicleY = dimensions.y + 10f;
			float buttonWidth = 200f;
			float buttonHeight = 20f;
			float sliderWidth = 300f;
			float headerWidth = dimensions.width - 20f;
			float headerHeight = 40f;

			if (mainscript.M.player.Car == null)
			{
				GUI.Label(new Rect(currVehicleX, currVehicleY, dimensions.width - 20f, dimensions.height - 20f), "No current vehicle\nSit in a vehicle to show configuration", GUIRenderer.messageStyle);
				return;
			}

			carscript car = mainscript.M.player.Car;
			GameObject carObject = car.gameObject;
			partconditionscript partconditionscript = car.gameObject.GetComponent<partconditionscript>();
			coolantTankscript coolant = car.coolant;
			enginescript engine = car.Engine;
			tankscript fuel = car.Tank;
			Transform sunRoofSlot = carObject.transform.FindRecursive("SunRoofSlot");
			tosaveitemscript save = carObject.GetComponent<tosaveitemscript>();

			int buttonCount = 2;
			int sliderCount = 8;
			int fluidSlidersCount = 0;
			float fluidsHeight = 0;

			if (coolant != null)
				fluidSlidersCount++;
			else
				fluidsHeight += headerHeight * 2 + 30f;

			if (engine != null && engine.T != null)
				fluidSlidersCount++;
			else
				fluidsHeight += headerHeight * 2 + 30f;

			if (fuel != null)
				fluidSlidersCount++;
			else
				fluidsHeight += headerHeight * 2 + 30f;

			if (sunRoofSlot != null)
				sliderCount += 6;

			float buttonsHeight = buttonCount * (buttonHeight + 10f + headerHeight);
			float slidersHeight = sliderCount * ((buttonHeight * 3) + 10f) + headerHeight;
			fluidsHeight += fluidSlidersCount * ((buttonHeight + 10f) * Enum.GetValues(typeof(mainscript.fluidenum)).Cast<mainscript.fluidenum>().Count()) + (fluidSlidersCount * headerHeight) + (buttonHeight + 10f) + 10f;
			float currVehicleHeight = buttonsHeight + slidersHeight + fluidsHeight;

			currentVehiclePosition = GUI.BeginScrollView(new Rect(currVehicleX, currVehicleY, dimensions.width - 20f, dimensions.height - 20f), currentVehiclePosition, new Rect(currVehicleX, currVehicleY, dimensions.width - 20f, currVehicleHeight - 20f), new GUIStyle(), GUI.skin.verticalScrollbar);

			// Vehicle god mode.
			if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleString("Vehicle god mode", car.crashMultiplier <= 0.0)))
			{
				car.crashMultiplier *= -1f;
			}

			currVehicleY += buttonHeight + 10f;

			// Condition.
			GUI.Label(new Rect(currVehicleX, currVehicleY, headerWidth, headerHeight), "Condition", GUIRenderer.headerStyle);
			currVehicleY += headerHeight;
			int maxCondition = (int)Enum.GetValues(typeof(Item.Condition)).Cast<Item.Condition>().Max();
			float rawCondition = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.conditionInt, 0, maxCondition);
			GUIRenderer.conditionInt = Mathf.RoundToInt(rawCondition);
			currVehicleX += sliderWidth + 10f;
			if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Apply"))
			{
				List<partconditionscript> children = new List<partconditionscript>();
				GameUtilities.FindPartChildren(partconditionscript, ref children);

				partconditionscript.state = GUIRenderer.conditionInt;
				partconditionscript.Refresh();
				foreach (partconditionscript child in children)
				{
					child.state = GUIRenderer.conditionInt;
					child.Refresh();
				}
			}
			currVehicleX = startingCurrVehicleX;
			currVehicleY += buttonHeight;
			string conditionName = ((Item.Condition)GUIRenderer.conditionInt).ToString();
			GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), conditionName, GUIRenderer.labelStyle);

			currVehicleY += buttonHeight + 10f;

			// Vehicle colour sliders.
			GUI.Label(new Rect(currVehicleX, currVehicleY, headerWidth, headerHeight), "Color", GUIRenderer.headerStyle);
			currVehicleY += headerHeight;
			// Red.
			GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "<color=#F00>Red:</color>", GUIRenderer.labelStyle);
			currVehicleY += buttonHeight;
			float red = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.color.r * 255, 0, 255);
			red = Mathf.Round(red);
			currVehicleY += buttonHeight;
			bool redParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), red.ToString(), GUIRenderer.labelStyle), out red);
			if (!redParse)
				Logger.Log($"{redParse} is not a number", Logger.LogLevel.Error);
			red = Mathf.Clamp(red, 0f, 255f);
			GUIRenderer.color.r = red / 255f;

			// Green.
			currVehicleY += buttonHeight + 10f;
			GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "<color=#0F0>Green:</color>", GUIRenderer.labelStyle);
			currVehicleY += buttonHeight;
			float green = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.color.g * 255, 0, 255);
			green = Mathf.Round(green);
			currVehicleY += buttonHeight;
			bool greenParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), green.ToString(), GUIRenderer.labelStyle), out green);
			if (!greenParse)
				Logger.Log($"{greenParse} is not a number", Logger.LogLevel.Error);
			green = Mathf.Clamp(green, 0f, 255f);
			GUIRenderer.color.g = green / 255f;

			// Blue.
			currVehicleY += buttonHeight + 10f;
			GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "<color=#00F>Blue:</color>", GUIRenderer.labelStyle);
			currVehicleY += buttonHeight;
			float blue = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.color.b * 255, 0, 255);
			blue = Mathf.Round(blue);
			currVehicleY += buttonHeight;
			bool blueParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), blue.ToString(), GUIRenderer.labelStyle), out blue);
			if (!blueParse)
				Logger.Log($"{blueParse.ToString()} is not a number", Logger.LogLevel.Error);
			blue = Mathf.Clamp(blue, 0f, 255f);
			GUIRenderer.color.b = blue / 255f;

			currVehicleY += buttonHeight + 10f;

			// Colour preview.
			GUIStyle defaultStyle = GUI.skin.button;
			GUIStyle previewStyle = new GUIStyle(defaultStyle);
			Texture2D previewTexture = new Texture2D(1, 1);
			Color[] pixels = new Color[] { GUIRenderer.color };
			previewTexture.SetPixels(pixels);
			previewTexture.Apply();
			previewStyle.normal.background = previewTexture;
			previewStyle.active.background = previewTexture;
			previewStyle.hover.background = previewTexture;
			previewStyle.margin = new RectOffset(0, 0, 0, 0);
			GUI.skin.button = previewStyle;
			GUI.Button(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), "");
			GUI.skin.button = defaultStyle;

			currVehicleY += buttonHeight + 10f;

			if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Randomise colour"))
			{
				GUIRenderer.color.r = UnityEngine.Random.Range(0f, 255f) / 255f;
				GUIRenderer.color.g = UnityEngine.Random.Range(0f, 255f) / 255f;
				GUIRenderer.color.b = UnityEngine.Random.Range(0f, 255f) / 255f;
			}

			currVehicleX += buttonWidth + 10f;

			if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Apply"))
			{
				partconditionscript[] partconditionscripts = car.GetComponentsInChildren<partconditionscript>();
				foreach (partconditionscript part in partconditionscripts)
				{
					part.Paint(GUIRenderer.color);
				}
			}

			currVehicleX = startingCurrVehicleX;
			currVehicleY += buttonHeight + 10f;

			// Coolant settings.
			GUI.Label(new Rect(currVehicleX, currVehicleY, headerWidth, headerHeight), "Radiator settings", GUIRenderer.headerStyle);
			currVehicleY += headerHeight;
			if (coolant != null)
			{
				float coolantMax = coolant.coolant.F.maxC;
				int coolantPercentage = 0;

				foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.coolants)
				{
					coolantPercentage += fluid.Value;
				}

				if (coolantPercentage > 100)
					coolantPercentage = 100;

				bool changed = false;

				// Deep copy coolants dictionary.
				Dictionary<mainscript.fluidenum, int> tempCoolants = GUIRenderer.coolants.ToDictionary(fluid => fluid.Key, fluid => fluid.Value);

				foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.coolants)
				{
					GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth / 3, buttonHeight), fluid.Key.ToString().ToSentenceCase(), GUIRenderer.labelStyle);
					currVehicleX += buttonWidth / 3;
					int percentage = Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), fluid.Value, 0, 100));
					if (percentage + (coolantPercentage - fluid.Value) <= 100)
					{
						tempCoolants[fluid.Key] = percentage;
						changed = true;
					}
					currVehicleX += sliderWidth + 5f;
					GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), $"{percentage}%", GUIRenderer.labelStyle);
					currVehicleX = startingCurrVehicleX;
					currVehicleY += buttonHeight;
				}

				if (changed)
					GUIRenderer.coolants = tempCoolants;

				if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Get current"))
				{
					tankscript tank = coolant.coolant;

					tempCoolants = new Dictionary<fluidenum, int>();
					foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.coolants)
					{
						tempCoolants[fluid.Key] = 0;
					}

					GUIRenderer.coolants = tempCoolants;

					foreach (fluid fluid in tank.F.fluids)
					{
						int percentage = (int)(fluid.amount / tank.F.maxC * 100);
						GUIRenderer.coolants[fluid.type] = percentage;
					}
				}

				currVehicleX += buttonWidth + 10f;

				if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Apply"))
				{
					tankscript tank = coolant.coolant;
					tank.F.fluids.Clear();
					foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.coolants)
					{
						if (fluid.Value > 0)
						{
							tank.F.ChangeOne((coolantMax / 100) * fluid.Value, fluid.Key);
						}
					}
				}

				currVehicleX = startingCurrVehicleX;
			}
			else
				GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "No radiator found.", GUIRenderer.labelStyle);

			currVehicleY += buttonHeight + 20f;

			// Engine settings.
			GUI.Label(new Rect(currVehicleX, currVehicleY, headerWidth, headerHeight), "Engine settings", GUIRenderer.headerStyle);
			currVehicleY += headerHeight;
			if (engine != null)
			{
				if (engine.T != null)
				{
					float oilMax = engine.T.F.maxC;
					int oilPercentage = 0;

					foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.oils)
					{
						oilPercentage += fluid.Value;
					}

					if (oilPercentage > 100)
						oilPercentage = 100;

					bool changed = false;

					// Deep copy oils dictionary.
					Dictionary<mainscript.fluidenum, int> tempOils = GUIRenderer.oils.ToDictionary(fluid => fluid.Key, fluid => fluid.Value);

					foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.oils)
					{
						GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth / 3, buttonHeight), fluid.Key.ToString().ToSentenceCase(), GUIRenderer.labelStyle);
						currVehicleX += buttonWidth / 3;
						int percentage = Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), fluid.Value, 0, 100));
						if (percentage + (oilPercentage - fluid.Value) <= 100)
						{
							tempOils[fluid.Key] = percentage;
							changed = true;
						}
						currVehicleX += sliderWidth + 5f;
						GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), $"{percentage}%", GUIRenderer.labelStyle);
						currVehicleX = startingCurrVehicleX;
						currVehicleY += buttonHeight;
					}

					if (changed)
						GUIRenderer.oils = tempOils;

					if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Get current"))
					{
						tankscript tank = engine.T;

						tempOils = new Dictionary<fluidenum, int>();
						foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.oils)
						{
							tempOils[fluid.Key] = 0;
						}

						GUIRenderer.oils = tempOils;

						foreach (fluid fluid in tank.F.fluids)
						{
							int percentage = (int)(fluid.amount / tank.F.maxC * 100);
							GUIRenderer.oils[fluid.type] = percentage;
						}
					}

					currVehicleX += buttonWidth + 10f;

					if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Apply"))
					{
						tankscript tank = engine.T;
						tank.F.fluids.Clear();
						foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.oils)
						{
							if (fluid.Value > 0)
							{
								tank.F.ChangeOne((oilMax / 100) * fluid.Value, fluid.Key);
							}
						}
					}

					currVehicleX = startingCurrVehicleX;
				}
				else
					GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Engine has no oil tank.", GUIRenderer.labelStyle);
			}
			else
				GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "No engine found.", GUIRenderer.labelStyle);

			currVehicleY += buttonHeight + 20f;

			// Fuel settings.
			GUI.Label(new Rect(currVehicleX, currVehicleY, headerWidth, headerHeight), "Fuel settings", GUIRenderer.headerStyle);
			currVehicleY += headerHeight;
			if (fuel != null)
			{
				float fuelMax = fuel.F.maxC;
				int fuelPercentage = 0;

				foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.fuels)
				{
					fuelPercentage += fluid.Value;
				}

				if (fuelPercentage > 100)
					fuelPercentage = 100;

				bool changed = false;

				// Deep copy fuels dictionary.
				Dictionary<mainscript.fluidenum, int> tempFuels = GUIRenderer.fuels.ToDictionary(fluid => fluid.Key, fluid => fluid.Value);

				foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.fuels)
				{
					GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth / 3, buttonHeight), fluid.Key.ToString().ToSentenceCase(), GUIRenderer.labelStyle);
					currVehicleX += buttonWidth / 3;
					int percentage = Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), fluid.Value, 0, 100));
					if (percentage + (fuelPercentage - fluid.Value) <= 100)
					{
						tempFuels[fluid.Key] = percentage;
						changed = true;
					}
					currVehicleX += sliderWidth + 5f;
					GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), $"{percentage}%", GUIRenderer.labelStyle);
					currVehicleX = startingCurrVehicleX;
					currVehicleY += buttonHeight;
				}

				if (changed)
					GUIRenderer.fuels = tempFuels;

				if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Get current"))
				{
					tankscript tank = fuel;

					tempFuels = new Dictionary<fluidenum, int>();
					foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.fuels)
					{
						tempFuels[fluid.Key] = 0;
					}

					GUIRenderer.fuels = tempFuels;

					foreach (fluid fluid in tank.F.fluids)
					{
						int percentage = (int)(fluid.amount / tank.F.maxC * 100);
						GUIRenderer.fuels[fluid.type] = percentage;
					}
				}

				currVehicleX += buttonWidth + 10f;

				if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Apply"))
				{
					tankscript tank = fuel;
					tank.F.fluids.Clear();
					foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.fuels)
					{
						if (fluid.Value > 0)
						{
							tank.F.ChangeOne((fuelMax / 100) * fluid.Value, fluid.Key);
						}
					}
				}

				if (engine != null)
				{
					currVehicleX += buttonWidth + 10f;

					if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Fill with correct fuel"))
					{
						fuel.F.fluids.Clear();

						// Find the correct fluid(s) from the engine.
						List<mainscript.fluidenum> fluids = new List<mainscript.fluidenum>();
						foreach (fluid fluid in engine.FuelConsumption.fluids)
						{
							fluids.Add(fluid.type);
						}

						if (fluids.Count > 0)
						{
							// Two stoke.
							if (fluids.Contains(mainscript.fluidenum.oil) && fluids.Contains(mainscript.fluidenum.gas))
							{
								fuel.F.ChangeOne(fuelMax / 100 * 97, mainscript.fluidenum.gas);
								fuel.F.ChangeOne(fuelMax / 100 * 3, mainscript.fluidenum.oil);
							}
							else
							{
								// Just use the first fluid found by default.
								// Only mixed fuel currently is two-stroke which we're
								// accounting for already.
								fuel.F.ChangeOne(fuelMax, fluids[0]);
							}
						}

						// Update UI.
						tempFuels = new Dictionary<fluidenum, int>();
						foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.fuels)
						{
							tempFuels[fluid.Key] = 0;
						}

						GUIRenderer.fuels = tempFuels;

						foreach (fluid fluid in fuel.F.fluids)
						{
							int percentage = (int)(fluid.amount / fuel.F.maxC * 100);
							GUIRenderer.fuels[fluid.type] = percentage;
						}
					}
				}

				currVehicleX = startingCurrVehicleX;
			}
			else
				GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "No fuel tank found.", GUIRenderer.labelStyle);

			currVehicleY += buttonHeight + 20f;
			GUI.Label(new Rect(currVehicleX, currVehicleY, headerWidth, headerHeight), "Window settings", GUIRenderer.headerStyle);
			currVehicleY += headerHeight;

			// Window colour sliders.
			// Red.
			GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "<color=#F00>Red:</color>", GUIRenderer.labelStyle);
			currVehicleY += buttonHeight;
			float windowRed = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.windowColor.r * 255, 0, 255);
			windowRed = Mathf.Round(windowRed);
			currVehicleY += buttonHeight;
			bool windowRedParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), windowRed.ToString(), GUIRenderer.labelStyle), out windowRed);
			if (!windowRedParse)
				Logger.Log($"{windowRedParse} is not a number", Logger.LogLevel.Error);
			windowRed = Mathf.Clamp(windowRed, 0f, 255f);
			GUIRenderer.windowColor.r = windowRed / 255f;

			// Green.
			currVehicleY += buttonHeight + 10f;
			GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "<color=#0F0>Green:</color>", GUIRenderer.labelStyle);
			currVehicleY += buttonHeight;
			float windowGreen = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.windowColor.g * 255, 0, 255);
			windowGreen = Mathf.Round(windowGreen);
			currVehicleY += buttonHeight;
			bool windowGreenParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), windowGreen.ToString(), GUIRenderer.labelStyle), out windowGreen);
			if (!windowGreenParse)
				Logger.Log($"{windowGreenParse} is not a number", Logger.LogLevel.Error);
			windowGreen = Mathf.Clamp(windowGreen, 0f, 255f);
			GUIRenderer.windowColor.g = windowGreen / 255f;

			// Blue.
			currVehicleY += buttonHeight + 10f;
			GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "<color=#00F>Blue:</color>", GUIRenderer.labelStyle);
			currVehicleY += buttonHeight;
			float windowBlue = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.windowColor.b * 255, 0, 255);
			windowBlue = Mathf.Round(windowBlue);
			currVehicleY += buttonHeight;
			bool windowBlueParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), windowBlue.ToString(), GUIRenderer.labelStyle), out windowBlue);
			if (!windowBlueParse)
				Logger.Log($"{windowBlueParse.ToString()} is not a number", Logger.LogLevel.Error);
			windowBlue = Mathf.Clamp(windowBlue, 0f, 255f);
			GUIRenderer.windowColor.b = windowBlue / 255f;

			// Alpha.
			currVehicleY += buttonHeight + 10f;
			GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Alpha:", GUIRenderer.labelStyle);
			currVehicleY += buttonHeight;
			float windowAlpha = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.windowColor.a * 255, 0, 255);
			windowAlpha = Mathf.Round(windowAlpha);
			currVehicleY += buttonHeight;
			bool windowAlphaParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), windowAlpha.ToString(), GUIRenderer.labelStyle), out windowAlpha);
			if (!windowAlphaParse)
				Logger.Log($"{windowAlphaParse.ToString()} is not a number", Logger.LogLevel.Error);
			windowAlpha = Mathf.Clamp(windowAlpha, 0f, 255f);
			GUIRenderer.windowColor.a = windowAlpha / 255f;

			currVehicleY += buttonHeight + 10f;

			// Colour preview.
			// Override alpha for colour preview.
			Color windowPreview = GUIRenderer.windowColor;
			windowPreview.a = 1;
			pixels = new Color[] { windowPreview };
			previewTexture.SetPixels(pixels);
			previewTexture.Apply();
			previewStyle.normal.background = previewTexture;
			previewStyle.active.background = previewTexture;
			previewStyle.hover.background = previewTexture;
			previewStyle.margin = new RectOffset(0, 0, 0, 0);
			GUI.skin.button = previewStyle;
			GUI.Button(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), "");
			GUI.skin.button = defaultStyle;

			currVehicleY += buttonHeight + 10f;

			if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Randomise colour"))
			{
				GUIRenderer.windowColor.r = UnityEngine.Random.Range(0f, 255f) / 255f;
				GUIRenderer.windowColor.g = UnityEngine.Random.Range(0f, 255f) / 255f;
				GUIRenderer.windowColor.b = UnityEngine.Random.Range(0f, 255f) / 255f;
			}

			currVehicleX += buttonWidth + 10f;

			if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Apply"))
			{
				List<MeshRenderer> renderers = carObject.GetComponentsInChildren<MeshRenderer>().ToList();
				foreach (MeshRenderer meshRenderer in renderers)
				{
					string materialName = meshRenderer.material.name.Replace(" (Instance)", "");
					switch (materialName)
					{
						// Outer glass.
						case "Glass":
							// Use selected colour.
							meshRenderer.material.color = GUIRenderer.windowColor;
							break;

						// Inner glass.
						case "GlassNoReflection":
							// Use a more transparent version of the selected colour
							// for the inner glass to ensure it's still see-through.
							Color innerColor = GUIRenderer.windowColor;
							if (innerColor.a > 0.2f)
								innerColor.a = 0.2f;
							meshRenderer.material.color = innerColor;
							break;
					}
				}

				SaveUtilities.UpdateGlass(new GlassData() { ID = save.idInSave, color = GUIRenderer.windowColor, type = "windows" });
			}

			currVehicleX = startingCurrVehicleX;
			currVehicleY += buttonHeight + 10f;

			// Sunroof settings.
			if (sunRoofSlot != null)
			{
				currVehicleY += buttonHeight + 20f;
				GUI.Label(new Rect(currVehicleX, currVehicleY, headerWidth, headerHeight), "Sunroof settings", GUIRenderer.headerStyle);
				currVehicleY += headerHeight;

				Transform outerGlass = sunRoofSlot.FindRecursive("sunroof outer glass", exact: false);
				if (outerGlass != null)
				{
					MeshRenderer meshRenderer = outerGlass.GetComponent<MeshRenderer>();

					// Sunroof colour sliders.
					// Red.
					GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "<color=#F00>Red:</color>", GUIRenderer.labelStyle);
					currVehicleY += buttonHeight;
					float sunroofRed = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.sunRoofColor.r * 255, 0, 255);
					sunroofRed = Mathf.Round(sunroofRed);
					currVehicleY += buttonHeight;
					bool sunroofRedParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), sunroofRed.ToString(), GUIRenderer.labelStyle), out sunroofRed);
					if (!sunroofRedParse)
						Logger.Log($"{sunroofRedParse} is not a number", Logger.LogLevel.Error);
					sunroofRed = Mathf.Clamp(sunroofRed, 0f, 255f);
					GUIRenderer.sunRoofColor.r = sunroofRed / 255f;

					// Green.
					currVehicleY += buttonHeight + 10f;
					GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "<color=#0F0>Green:</color>", GUIRenderer.labelStyle);
					currVehicleY += buttonHeight;
					float sunroofGreen = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.sunRoofColor.g * 255, 0, 255);
					sunroofGreen = Mathf.Round(sunroofGreen);
					currVehicleY += buttonHeight;
					bool sunroofGreenParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), sunroofGreen.ToString(), GUIRenderer.labelStyle), out sunroofGreen);
					if (!sunroofGreenParse)
						Logger.Log($"{sunroofGreenParse} is not a number", Logger.LogLevel.Error);
					sunroofGreen = Mathf.Clamp(sunroofGreen, 0f, 255f);
					GUIRenderer.sunRoofColor.g = sunroofGreen / 255f;

					// Blue.
					currVehicleY += buttonHeight + 10f;
					GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "<color=#00F>Blue:</color>", GUIRenderer.labelStyle);
					currVehicleY += buttonHeight;
					float sunroofBlue = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.sunRoofColor.b * 255, 0, 255);
					sunroofBlue = Mathf.Round(sunroofBlue);
					currVehicleY += buttonHeight;
					bool sunroofBlueParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), sunroofBlue.ToString(), GUIRenderer.labelStyle), out sunroofBlue);
					if (!sunroofBlueParse)
						Logger.Log($"{sunroofBlueParse.ToString()} is not a number", Logger.LogLevel.Error);
					sunroofBlue = Mathf.Clamp(sunroofBlue, 0f, 255f);
					GUIRenderer.sunRoofColor.b = sunroofBlue / 255f;

					// Alpha.
					currVehicleY += buttonHeight + 10f;
					GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Alpha:", GUIRenderer.labelStyle);
					currVehicleY += buttonHeight;
					float sunroofAlpha = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.sunRoofColor.a * 255, 0, 255);
					sunroofAlpha = Mathf.Round(sunroofAlpha);
					currVehicleY += buttonHeight;
					bool sunroofAlphaParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), sunroofAlpha.ToString(), GUIRenderer.labelStyle), out sunroofAlpha);
					if (!sunroofAlphaParse)
						Logger.Log($"{sunroofAlphaParse.ToString()} is not a number", Logger.LogLevel.Error);
					sunroofAlpha = Mathf.Clamp(sunroofAlpha, 0f, 255f);
					GUIRenderer.sunRoofColor.a = sunroofAlpha / 255f;

					currVehicleY += buttonHeight + 10f;

					// Colour preview.
					// Override alpha for colour preview.
					Color sunroofPreview = GUIRenderer.sunRoofColor;
					sunroofPreview.a = 1;
					pixels = new Color[] { sunroofPreview };
					previewTexture.SetPixels(pixels);
					previewTexture.Apply();
					previewStyle.normal.background = previewTexture;
					previewStyle.active.background = previewTexture;
					previewStyle.hover.background = previewTexture;
					previewStyle.margin = new RectOffset(0, 0, 0, 0);
					GUI.skin.button = previewStyle;
					GUI.Button(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), "");
					GUI.skin.button = defaultStyle;

					currVehicleY += buttonHeight + 10f;

					if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Randomise colour"))
					{
						GUIRenderer.sunRoofColor.r = UnityEngine.Random.Range(0f, 255f) / 255f;
						GUIRenderer.sunRoofColor.g = UnityEngine.Random.Range(0f, 255f) / 255f;
						GUIRenderer.sunRoofColor.b = UnityEngine.Random.Range(0f, 255f) / 255f;
					}

					currVehicleX += buttonWidth + 10f;

					if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Apply"))
					{
						meshRenderer.material.color = GUIRenderer.sunRoofColor;

						SaveUtilities.UpdateGlass(new GlassData() { ID = save.idInSave, color = GUIRenderer.sunRoofColor, type = "sunroof" });
					}

					currVehicleX = startingCurrVehicleX;
					currVehicleY += buttonHeight + 10f;
				}
				else
					GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "No sunroof mounted.", GUIRenderer.labelStyle);
			}

			GUI.EndScrollView();
		}
	}
}
