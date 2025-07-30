using MultiTool.Core;
using MultiTool.Extensions;
using MultiTool.Modules;
using MultiTool.Utilities;
using System.Collections.Generic;
using UnityEngine;
using static mainscript;
using Logger = MultiTool.Modules.Logger;

namespace MultiTool.Tabs.VehicleConfiguration
{
    internal sealed class Fluids : Core.VehicleConfigurationTab
	{
        public override string Name => "Fluids";

		private Vector2 _position;
		private List<FluidMix> _fluids = new List<FluidMix>();
		private List<TankCapacity> _tanks = new List<TankCapacity>();
		private carscript _lastVehicle = null;

		public override void OnCacheRefresh()
		{
			carscript car = mainscript.M.player.Car;

			if (_lastVehicle != car)
				_fluids.Clear();
			_lastVehicle = car;

			foreach (tankscript tank in car.GetComponentsInChildren<tankscript>())
			{
				// Ignore any player tanks.
				if (tank.gameObject.name.ToLower() == "player" || tank.transform.parent?.name.ToLower() == "head") continue;

				tosaveitemscript save = tank.GetComponentInParent<tosaveitemscript>();

				bool createMix = true;
				foreach (FluidMix mix in _fluids)
				{
					if (mix.tank == tank)
					{
						createMix = false;
						break;
					}
				}

                if (createMix)
                {
					List<FluidPercentage> defaults = new List<FluidPercentage>();
					foreach (FluidPercentage fluidDefault in GUIRenderer.fluidDefaults) 
					{
						defaults.Add(fluidDefault.Clone());
					}

					FluidMix newMix = new FluidMix()
					{
						tank = tank,
						fluids = defaults,
					};

					// Copy amounts from tank as default value.
					ResetToTank(newMix);

					_fluids.Add(newMix);
                }

				bool createTank = true;
				foreach (TankCapacity tankCapacity in _tanks)
				{
					if (tankCapacity.tank == tank)
					{
						createTank = false;
						break;
					}
				}

				if (createTank)
				{
					TankData tankData = SaveUtilities.GetTank(save.idInSave);
					_tanks.Add(new TankCapacity()
					{
						tank = tank,
						max = tank.F.maxC,
						defaultMax = tankData?.defaultCapacity ?? tank.F.maxC,
					});
				}
			}
		}

		public override void RenderTab(Rect dimensions)
		{
			GUILayout.BeginArea(dimensions);
			GUILayout.BeginVertical();
			_position = GUILayout.BeginScrollView(_position);

			carscript car = mainscript.M.player.Car;
			enginescript engine = car.Engine;

			FluidMix coolantMix = FindMixByTank(car.coolant?.coolant);
			FluidMix engineMix = FindMixByTank(car.Engine?.T);
			FluidMix fuelMix = FindMixByTank(car.Tank);

			TankCapacity coolantCapacity = FindCapacityByTank(car.coolant?.coolant);
			TankCapacity engineCapacity = FindCapacityByTank(car.Engine?.T);
			TankCapacity fuelCapacity = FindCapacityByTank(car.Tank);

			GUILayout.Label("Fluid settings", "LabelHeader");

			GUILayout.Space(10);

			GUILayout.Label("Fuel settings", "LabelSubHeader");
			if (fuelMix != null)
			{
				// Can't use RenderMixSliders() for fuel as it needs the extra
				// 'Fill with correct fuel' button.
				tankscript fuelTank = fuelMix.tank;
				float fuelMax = fuelTank.F.maxC;
				float fuelPercentage = 0;

				foreach (FluidPercentage fluid in fuelMix.fluids)
				{
					fuelPercentage += fluid.percentage;
				}

				if (fuelPercentage > 100)
					fuelPercentage = 100;

				foreach (FluidPercentage fluid in fuelMix.fluids)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label(fluid.type.ToString().ToSentenceCase(), GUILayout.MaxWidth(100));
					int percentage = Mathf.RoundToInt(GUILayout.HorizontalSlider(fluid.percentage, 0, 100));
					if (percentage + (fuelPercentage - fluid.percentage) <= 100)
						fluid.percentage = percentage;
					GUILayout.Label($"{percentage}%");
					GUILayout.EndHorizontal();
				}

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Get current", GUILayout.MaxWidth(200)))
				{
					ResetToTank(fuelMix);
				}

				if (GUILayout.Button("Apply", GUILayout.MaxWidth(200)))
				{
					tankscript tank = fuelMix.tank;
					tank.F.fluids.Clear();
					foreach (FluidPercentage fluid in fuelMix.fluids)
					{
						if (fluid.percentage > 0)
						{
							tank.F.ChangeOne((fluid.percentage / 100) * tank.F.maxC, fluid.type);
						}
					}
				}

				if (engine != null)
				{
					if (GUILayout.Button("Fill with correct fuel", GUILayout.MaxWidth(200)))
					{
						// Find the correct fluid(s) from the engine.
						List<mainscript.fluidenum> fluids = new List<mainscript.fluidenum>();
						foreach (fluid fluid in engine.FuelConsumption.fluids)
						{
							fluids.Add(fluid.type);
						}

						if (fluids.Count > 0)
						{
							fuelTank.F.fluids.Clear();
							// Two stroke.
							if (fluids.Contains(mainscript.fluidenum.oil) && fluids.Contains(mainscript.fluidenum.gas))
							{
								fuelTank.F.ChangeOne(fuelMax * 0.97f, mainscript.fluidenum.gas);
								fuelTank.F.ChangeOne(fuelMax, mainscript.fluidenum.oil);
							}
							else
							{
								// Just use the first fluid found by default.
								// Only mixed fuel currently is two-stroke which we're
								// accounting for already.
								fuelTank.F.ChangeOne(fuelMax, fluids[0]);
							}
						}

						// Update UI.
						foreach (fluid fluid in fuelTank.F.fluids)
						{
							foreach (FluidPercentage fuelFluid in fuelMix.fluids)
							{
								if (fluid.type == fuelFluid.type)
								{
									int percentage = (int)(fluid.amount / fuelTank.F.maxC * 100);
									fuelFluid.percentage = percentage;
									break;
								}
							}
						}
					}
				}
				GUILayout.EndHorizontal();

				if (fuelCapacity != null)
					RenderCapcity(fuelCapacity);
			}
			else
				GUILayout.Label("No fuel tank found.");


			GUILayout.Space(10);

			GUILayout.Label("Engine oil settings", "LabelSubHeader");
			if (engineMix != null)
			{
				RenderMixSliders(engineMix);
				if (engineCapacity != null)
					RenderCapcity(engineCapacity);
			}
			else
				GUILayout.Label("No engine mounted.");


			GUILayout.Space(10);

			GUILayout.Label("Coolant settings", "LabelSubHeader");
			if (coolantMix != null)
			{
				RenderMixSliders(coolantMix);
				if (coolantCapacity != null)
					RenderCapcity(coolantCapacity);
			}
			else
				GUILayout.Label("No radiator mounted.");


			GUILayout.Space(10);

			foreach (FluidMix mix in _fluids)
			{
				// Skip any mixes we've already rendered.
				if (mix == fuelMix || mix == engineMix || mix == coolantMix) continue;

				// Tank no longer exists, remove it and skip rendering this frame.
				if (mix.tank == null)
				{
					_fluids.Remove(mix);
					break;
				}

				GUILayout.Label($"{mix.tank.name.ToSentenceCase()} settings", "LabelSubHeader");
				RenderMixSliders(mix);
				TankCapacity fluidTankCapacity = FindCapacityByTank(mix.tank);
				if (fluidTankCapacity != null)
					RenderCapcity(fluidTankCapacity);
				GUILayout.Space(10);
			}

			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		private FluidMix FindMixByTank(tankscript tank)
		{
			if (tank != null)
				foreach (FluidMix mix in _fluids)
					if (mix.tank == tank) return mix;
			return null;
		}

		private void ResetToTank(FluidMix mix)
		{
			// Copy amounts from tank as default value.
			foreach (var tankFluid in mix.tank.F.fluids)
			{
				foreach (FluidPercentage mixFluid in mix.fluids)
				{
					foreach (fluid fluid in mix.tank.F.fluids)
					{
						if (fluid.type == mixFluid.type)
							mixFluid.percentage = fluid.amount / mix.tank.F.maxC * 100;
					}
				}
			}
		}

		private void RenderMixSliders(FluidMix mix)
		{
			if (mix.tank == null) return;

			float fluidPercentage = 0;

			foreach (FluidPercentage fluid in mix.fluids)
			{
				fluidPercentage += fluid.percentage;
			}

			if (fluidPercentage > 100)
				fluidPercentage = 100;

			foreach (FluidPercentage fluid in mix.fluids)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(fluid.type.ToString().ToSentenceCase(), GUILayout.MaxWidth(100));
				int percentage = Mathf.RoundToInt(GUILayout.HorizontalSlider(fluid.percentage, 0, 100));
				if (percentage + (fluidPercentage - fluid.percentage) <= 100)
					fluid.percentage = percentage;
				GUILayout.Label($"{percentage}%");
				GUILayout.EndHorizontal();
			}

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Get current", GUILayout.MaxWidth(200)))
			{
				ResetToTank(mix);
			}

			if (GUILayout.Button("Apply", GUILayout.MaxWidth(200)))
			{
				tankscript tank = mix.tank;
				tank.F.fluids.Clear();
				foreach (FluidPercentage fluid in mix.fluids)
				{
					if (fluid.percentage > 0)
					{
						tank.F.ChangeOne((fluid.percentage / 100) * tank.F.maxC, fluid.type);
					}
				}
			}
			GUILayout.EndHorizontal();
		}

		private TankCapacity FindCapacityByTank(tankscript tank)
		{
			if (tank != null)
				foreach (TankCapacity capacity in _tanks)
					if (capacity.tank == tank) return capacity;
			return null;
		}

		private void RenderCapcity(TankCapacity capacity)
		{
			if (capacity.tank == null) return;

			GUILayout.BeginVertical();
			GUILayout.Label("Capacity", GUILayout.MaxWidth(100));
			capacity.max = Mathf.RoundToInt(GUILayout.HorizontalSlider(capacity.max, 1, 1000));
			float.TryParse(GUILayout.TextField(capacity.max.ToString("F0"), GUILayout.MaxWidth(200)), out capacity.max);
			GUILayout.EndVertical();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Apply", GUILayout.MaxWidth(200)))
			{
				tankscript tank = capacity.tank;
				ApplyCapacity(tank, capacity);
			}

			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
			{
				tankscript tank = capacity.tank;
				capacity.max = capacity.defaultMax;
				ApplyCapacity(tank, capacity);
			}

			GUILayout.EndHorizontal();
		}

		private void ApplyCapacity(tankscript tank, TankCapacity capacity)
		{
			tank.F.maxC = capacity.max;
			tosaveitemscript save = tank.GetComponentInParent<tosaveitemscript>();
			if (save == null) return;
			SaveUtilities.UpdateTank(new TankData()
			{
				ID = save.idInSave,
				capacity = capacity.max,
				defaultCapacity = capacity.defaultMax,
			});
		}
	}
}
