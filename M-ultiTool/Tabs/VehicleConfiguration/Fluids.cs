using MultiTool.Core;
using MultiTool.Modules;
using MultiTool.Utilities.UI;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MultiTool.Extensions;
using ScottPlot.Plottable.AxisManagers;
using static mainscript;

namespace MultiTool.Tabs.VehicleConfiguration
{
    internal sealed class Fluids : Core.VehicleConfigurationTab
	{
        public override string Name => "Fluids";

		private Vector2 _position;

		public override void RenderTab(Rect dimensions)
		{
			dimensions.width /= 2;
			GUILayout.BeginArea(dimensions);
			GUILayout.BeginVertical();
			_position = GUILayout.BeginScrollView(_position);

			carscript car = mainscript.M.player.Car;
			coolantTankscript coolant = car.coolant;
			enginescript engine = car.Engine;
			tankscript fuel = car.Tank;

			// Coolant settings.
			GUILayout.Label("Radiator settings", "LabelHeader");
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
					GUILayout.BeginHorizontal();
					GUILayout.Label(fluid.Key.ToString().ToSentenceCase(), GUILayout.MaxWidth(100));
					int percentage = Mathf.RoundToInt(GUILayout.HorizontalSlider(fluid.Value, 0, 100));
					if (percentage + (coolantPercentage - fluid.Value) <= 100)
					{
						tempCoolants[fluid.Key] = percentage;
						changed = true;
					}
					GUILayout.Label($"{percentage}%");
					GUILayout.EndHorizontal();
				}

				if (changed)
					GUIRenderer.coolants = tempCoolants;

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Get current", GUILayout.MaxWidth(200)))
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

				if (GUILayout.Button("Apply", GUILayout.MaxWidth(200)))
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
				GUILayout.EndHorizontal();
			}
			else
				GUILayout.Label("No radiator found.");

			GUILayout.Space(10);

			// Engine settings.
			GUILayout.Label("Engine settings", "LabelHeader");
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
						GUILayout.BeginHorizontal();
						GUILayout.Label(fluid.Key.ToString().ToSentenceCase(), GUILayout.MaxWidth(100));
						int percentage = Mathf.RoundToInt(GUILayout.HorizontalSlider(fluid.Value, 0, 100));
						if (percentage + (oilPercentage - fluid.Value) <= 100)
						{
							tempOils[fluid.Key] = percentage;
							changed = true;
						}
						GUILayout.Label($"{percentage}%");
						GUILayout.EndHorizontal();
					}

					if (changed)
						GUIRenderer.oils = tempOils;

					GUILayout.BeginHorizontal();
					if (GUILayout.Button("Get current", GUILayout.MaxWidth(200)))
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

					if (GUILayout.Button("Apply", GUILayout.MaxWidth(200)))
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
					GUILayout.EndHorizontal();
				}
				else
					GUILayout.Label("Engine has no oil tank.");
			}
			else
				GUILayout.Label("No engine found.");

			GUILayout.Space(10);

			// Fuel settings.
			GUILayout.Label("Fuel settings", "LabelHeader");
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
					GUILayout.BeginHorizontal();
					GUILayout.Label(fluid.Key.ToString().ToSentenceCase(), GUILayout.MaxWidth(100));
					int percentage = Mathf.RoundToInt(GUILayout.HorizontalSlider(fluid.Value, 0, 100));
					if (percentage + (fuelPercentage - fluid.Value) <= 100)
					{
						tempFuels[fluid.Key] = percentage;
						changed = true;
					}
					GUILayout.Label($"{percentage}%");
					GUILayout.EndHorizontal();
				}

				if (changed)
					GUIRenderer.fuels = tempFuels;

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Get current", GUILayout.MaxWidth(200)))
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

				if (GUILayout.Button("Apply", GUILayout.MaxWidth(200)))
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
					if (GUILayout.Button("Fill with correct fuel", GUILayout.MaxWidth(200)))
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
				GUILayout.EndHorizontal();
			}
			else
				GUILayout.Label("No fuel tank found.");

			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}
}
