using MultiTool.Core;
using MultiTool.Utilities.UI;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static mainscript;
using MultiTool.Extensions;

namespace MultiTool.Tabs.VehicleConfiguration
{
    internal sealed class EngineTuning : Core.VehicleConfigurationTab
	{
        public override string Name => "Engine Tuning";

		private Vector2 _position;
		private Vector2 _tunerStatsPosition;

		private Core.EngineTuning _engineTuning = null;
		private bool _isEngineTuningStatsOpen = false;
		private Core.EngineStats _engineStats = null;
		private bool _hideLastTorquePoint = false;
		private int _maxFluidIndex = 0;

		public override void OnRegister()
		{
			_maxFluidIndex = (int)Enum.GetValues(typeof(fluidenum)).Cast<fluidenum>().Max();
		}

		public override void OnVehicleChange()
		{
			_engineTuning = null;
		}

		public override void RenderTab(Rect dimensions)
		{
			carscript car = mainscript.M.player.Car;
			enginescript engine = car.Engine;
			tosaveitemscript engineSave = engine?.GetComponent<tosaveitemscript>();

			// Disable tab if engine isn't mounted.
			if (engine == null)
			{
				GUILayout.BeginArea(dimensions);
				GUILayout.FlexibleSpace();
				GUILayout.Label("No engine installed to tune.", "LabelMessage");
				GUILayout.FlexibleSpace();
				GUILayout.EndArea();
				return;
			}

			// Populate default tuning values if missing.
			if (_engineTuning == null)
			{
				// Attempt to load data from save.
				_engineTuning = SaveUtilities.GetEngineTuning(engineSave.idInSave);

				// Save has no data for this engine, load defaults.
				if (_engineTuning == null)
				{
					_engineTuning = new Core.EngineTuning()
					{
						rpmChangeModifier = engine.rpmChangeModifier,
						defaultRpmChangeModifier = engine.rpmChangeModifier,

						startChance = engine.startChance,
						defaultStartChance = engine.startChance,

						motorBrakeModifier = engine.motorBrakeModifier,
						defaultMotorBrakeModifier = engine.motorBrakeModifier,

						minOptimalTemp2 = engine.minOptimalTemp2,
						defaultMinOptimalTemp2 = engine.minOptimalTemp2,

						maxOptimalTemp2 = engine.maxOptimalTemp2,
						defaultMaxOptimalTemp2 = engine.maxOptimalTemp2,

						engineHeatGainMin = engine.engineHeatGainMin,
						defaultEngineHeatGainMin = engine.engineHeatGainMin,

						engineHeatGainMax = engine.engineHeatGainMax,
						defaultEngineHeatGainMax = engine.engineHeatGainMax,

						consumptionModifier = engine.consumptionM,
						defaultConsumptionModifier = engine.consumptionM,

						noOverheat = engine.noOverHeat,
						defaultNoOverheat = engine.noOverHeat,

						twoStroke = engine.twostroke,
						defaultTwoStroke = engine.twostroke,

						oilFluid = engine.Oilfluid,
						defaultOilFluid = engine.Oilfluid,

						oilTolerationMin = engine.oilTolerationMin,
						defaultOilTolerationMin = engine.oilTolerationMin,

						oilTolerationMax = engine.oilTolerationMax,
						defaultOilTolerationMax = engine.oilTolerationMax,

						oilConsumptionModifier = engine.OilConsumptionModifier,
						defaultOilConsumptionModifier = engine.OilConsumptionModifier,

						consumption = new List<Fluid>(),
						defaultConsumption = new List<Fluid>(),

						torqueCurve = new List<TorqueCurve>(),
						defaultTorqueCurve = new List<TorqueCurve>(),
					};

					// Populate fuel consumption fluids.
					foreach (fluid fluid in engine.FuelConsumption.fluids)
					{
						_engineTuning.consumption.Add(new Fluid() { type = fluid.type, amount = fluid.amount });
						_engineTuning.defaultConsumption.Add(new Fluid() { type = fluid.type, amount = fluid.amount });
					}

					// Populate torque curve.
					for (int torqueKey = 0; torqueKey < engine.torqueCurve.length; torqueKey++)
					{
						Keyframe torque = engine.torqueCurve.keys[torqueKey];
						_engineTuning.torqueCurve.Add(new TorqueCurve(torque.value, torque.time));
						_engineTuning.defaultTorqueCurve.Add(new TorqueCurve(torque.value, torque.time));
					}
				}

				UpdateEngineTunerStats();
			}

			bool updateEngineStats = false;

			GUILayout.BeginArea(dimensions);
			_position = GUILayout.BeginScrollView(_position);

			GUILayout.Label("Basics", "LabelHeader");

			GUILayout.BeginVertical();
			GUILayout.Label("RPM change modifier (responsiveness)");
			_engineTuning.rpmChangeModifier = GUILayout.HorizontalSlider(_engineTuning.rpmChangeModifier, 0f, 10f);
			float.TryParse(GUILayout.TextField(_engineTuning.rpmChangeModifier.ToString("F2"), GUILayout.MaxWidth(200)), out _engineTuning.rpmChangeModifier);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.rpmChangeModifier = _engineTuning.defaultRpmChangeModifier;
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.BeginVertical();
			GUILayout.Label("Start chance");
			_engineTuning.startChance = GUILayout.HorizontalSlider(_engineTuning.startChance, 0f, 1f);
			GUILayout.BeginHorizontal();
			if (float.TryParse(GUILayout.TextField((_engineTuning.startChance * 100).ToString("F0"), GUILayout.MaxWidth(200)), out float startChance))
				_engineTuning.startChance = startChance / 100;
			GUILayout.Label("%");
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.startChance = _engineTuning.defaultStartChance;
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.BeginVertical();
			GUILayout.Label("Engine brake modifier");
			_engineTuning.motorBrakeModifier = GUILayout.HorizontalSlider(_engineTuning.motorBrakeModifier, 0f, 10f);
			float.TryParse(GUILayout.TextField(_engineTuning.motorBrakeModifier.ToString("F2"), GUILayout.MaxWidth(200)), out _engineTuning.motorBrakeModifier);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.motorBrakeModifier = _engineTuning.defaultMotorBrakeModifier;
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.Label("Temperature", "LabelHeader");

			GUILayout.BeginVertical();
			GUILayout.Label("Min optimal temp");
			_engineTuning.minOptimalTemp2 = GUILayout.HorizontalSlider(_engineTuning.minOptimalTemp2, 0f, 300f);
			float.TryParse(GUILayout.TextField(_engineTuning.minOptimalTemp2.ToString("F2"), GUILayout.MaxWidth(200)), out _engineTuning.minOptimalTemp2);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.minOptimalTemp2 = _engineTuning.defaultMinOptimalTemp2;
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.BeginVertical();
			GUILayout.Label("Max optimal temp");
			_engineTuning.maxOptimalTemp2 = GUILayout.HorizontalSlider(_engineTuning.maxOptimalTemp2, 0f, 300f);
			float.TryParse(GUILayout.TextField(_engineTuning.maxOptimalTemp2.ToString("F2"), GUILayout.MaxWidth(200)), out _engineTuning.maxOptimalTemp2);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.maxOptimalTemp2 = _engineTuning.defaultMaxOptimalTemp2;
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.BeginVertical();
			GUILayout.Label("Engine heat gain min");
			_engineTuning.engineHeatGainMin = GUILayout.HorizontalSlider(_engineTuning.engineHeatGainMin, 0f, 300f);
			float.TryParse(GUILayout.TextField(_engineTuning.engineHeatGainMin.ToString("F2"), GUILayout.MaxWidth(200)), out _engineTuning.engineHeatGainMin);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.engineHeatGainMin = _engineTuning.defaultEngineHeatGainMin;
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.BeginVertical();
			GUILayout.Label("Engine heat gain max");
			_engineTuning.engineHeatGainMax = GUILayout.HorizontalSlider(_engineTuning.engineHeatGainMax, 0f, 300f);
			float.TryParse(GUILayout.TextField(_engineTuning.engineHeatGainMax.ToString("F2"), GUILayout.MaxWidth(200)), out _engineTuning.engineHeatGainMax);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.engineHeatGainMax = _engineTuning.defaultEngineHeatGainMax;
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.BeginHorizontal();
			GUILayout.Label("No overheat");
			if (GUILayout.Button(Accessibility.GetAccessibleString("Yes", "No", _engineTuning.noOverheat), GUILayout.MaxWidth(200)))
				_engineTuning.noOverheat = !_engineTuning.noOverheat;
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.noOverheat = _engineTuning.defaultNoOverheat;
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.Space(10);

			GUILayout.Label("Oil", "LabelHeader");

			if (!_engineTuning.defaultTwoStroke)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Is two-stroke?");
				if (GUILayout.Button(Accessibility.GetAccessibleString("Yes", "No", _engineTuning.twoStroke), GUILayout.MaxWidth(200)))
					_engineTuning.twoStroke = !_engineTuning.twoStroke;
				if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
					_engineTuning.twoStroke = _engineTuning.defaultTwoStroke;
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				GUILayout.Space(10);
			}

			GUILayout.BeginVertical();
			GUILayout.Label($"Oil fluid - {_engineTuning.oilFluid.ToString().ToSentenceCase()}");
			for (int oilFluidIndex = 0; oilFluidIndex <= _maxFluidIndex; oilFluidIndex++)
			{
				fluidenum oilFluid = (fluidenum)oilFluidIndex;
				// Skip currently set fluid.
				if (oilFluid == _engineTuning.oilFluid) continue;
				if (GUILayout.Button(oilFluid.ToString().ToSentenceCase(), GUILayout.MaxWidth(200)))
					_engineTuning.oilFluid = oilFluid;
			}
			GUILayout.Space(5);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.oilFluid = _engineTuning.defaultOilFluid;
			GUILayout.EndVertical();

			if (_engineTuning.twoStroke)
			{
				GUILayout.Space(10);

				GUILayout.BeginVertical();
				GUILayout.Label("Two-stroke oil toleration min");
				_engineTuning.oilTolerationMin = GUILayout.HorizontalSlider(_engineTuning.oilTolerationMin, 0f, 1f);
				GUILayout.BeginHorizontal();
				if (float.TryParse(GUILayout.TextField((_engineTuning.oilTolerationMin * 100).ToString("F2"), GUILayout.MaxWidth(200)), out float oilTolerationMin))
					_engineTuning.oilTolerationMin = oilTolerationMin / 100;
				GUILayout.Label("%");
				GUILayout.EndHorizontal();
				if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
					_engineTuning.oilTolerationMin = _engineTuning.defaultOilTolerationMin;
				GUILayout.EndVertical();

				GUILayout.Space(10);

				GUILayout.BeginVertical();
				GUILayout.Label("Two-stroke oil toleration max");
				_engineTuning.oilTolerationMax = GUILayout.HorizontalSlider(_engineTuning.oilTolerationMax, 0f, 1f);
				GUILayout.BeginHorizontal();
				if (float.TryParse(GUILayout.TextField((_engineTuning.oilTolerationMax * 100).ToString("F2"), GUILayout.MaxWidth(200)), out float oilTolerationMax))
					_engineTuning.oilTolerationMax = oilTolerationMax / 100;
				GUILayout.Label("%");
				GUILayout.EndHorizontal();
				if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
					_engineTuning.oilTolerationMax = _engineTuning.defaultOilTolerationMax;
				GUILayout.EndVertical();
			}

			GUILayout.Space(10);

			GUILayout.BeginVertical();
			GUILayout.Label("Oil consumption modifier");
			_engineTuning.oilConsumptionModifier = GUILayout.HorizontalSlider(_engineTuning.oilConsumptionModifier, 0f, 10f);
			float.TryParse(GUILayout.TextField(_engineTuning.oilConsumptionModifier.ToString("F2"), GUILayout.MaxWidth(200)), out _engineTuning.oilConsumptionModifier);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.oilConsumptionModifier = _engineTuning.defaultOilConsumptionModifier;
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.Label("Fuel", "LabelHeader");

			GUILayout.BeginVertical();
			GUILayout.Label("Fuel consumption modifier");
			_engineTuning.consumptionModifier = GUILayout.HorizontalSlider(_engineTuning.consumptionModifier, 0f, 10f);
			float.TryParse(GUILayout.TextField(_engineTuning.consumptionModifier.ToString("F2"), GUILayout.MaxWidth(200)), out _engineTuning.consumptionModifier);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.consumptionModifier = _engineTuning.defaultConsumptionModifier;
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.BeginVertical();
			GUILayout.Label("Fuel consumption", "LabelHeader");
			foreach (Fluid fluid in _engineTuning.consumption)
			{
				for (int fuelFluidIndex = 0; fuelFluidIndex <= _maxFluidIndex; fuelFluidIndex++)
				{
					// Skip fluids already selected.
					if (_engineTuning.consumption.Where(f => (int)f.type == fuelFluidIndex && f.type != fluid.type).FirstOrDefault() != null)
						continue;

					fluidenum fuelFluid = (fluidenum)fuelFluidIndex;
					if (GUILayout.Button(Accessibility.GetAccessibleString(fuelFluid.ToString().ToSentenceCase(), fuelFluid == fluid.type), GUILayout.MaxWidth(200)))
						fluid.type = fuelFluid;
				}
				fluid.amount = GUILayout.HorizontalSlider(fluid.amount, 0f, 500f);
				float.TryParse(GUILayout.TextField(fluid.amount.ToString("F2"), GUILayout.MaxWidth(200)), out fluid.amount);
				GUILayout.Space(5);
				if (GUILayout.Button("Remove fluid", GUILayout.MaxWidth(200)))
				{
					_engineTuning.consumption.Remove(fluid);
					break;
				}
				GUILayout.Space(10);
			}
			if (_engineTuning.consumption.Count <= _maxFluidIndex)
			{
				if (GUILayout.Button("Add another fluid", GUILayout.MaxWidth(200)))
				{
					// Find the next unused fluid index.
					List<int> existingIndexes = new List<int>();
					foreach (Fluid existing in _engineTuning.consumption)
					{
						existingIndexes.Add((int)existing.type);
					}
					existingIndexes.Sort();
					int index = existingIndexes.Last() + 1;
					_engineTuning.consumption.Add(new Fluid() { type = (fluidenum)index, amount = 0 });
				}
			}
			GUILayout.Space(5);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.consumption = _engineTuning.defaultConsumption;
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.BeginVertical();
			GUILayout.Label("Torque curve", "LabelHeader");
			int torqueIndex = 0;
			foreach (TorqueCurve torque in _engineTuning.torqueCurve)
			{
				float originalTorque = torque.torque;
				float originalRpm = torque.rpm;

				bool lastIndex = torqueIndex == _engineTuning.torqueCurve.Count - 1;
				bool firstIndex = torqueIndex == 0;

				GUILayout.Label($"Torque {(firstIndex || lastIndex ? "(Should be zero)" : string.Empty)}");
				// Lock first or last curve point to zero.
				torque.torque = GUILayout.HorizontalSlider(torque.torque, 0, firstIndex || lastIndex ? 0 : 1000);
				float.TryParse(GUILayout.TextField(torque.torque.ToString("F2"), GUILayout.MaxWidth(200)), out torque.torque);

				GUILayout.Label($"RPM {(firstIndex ? "(Should be zero)" : string.Empty)}");
				torque.rpm = GUILayout.HorizontalSlider(torque.rpm, 0, firstIndex ? 0 : 20000);
				float.TryParse(GUILayout.TextField(torque.rpm.ToString("F2"), GUILayout.MaxWidth(200)), out torque.rpm);

				GUILayout.Space(5);
				GUILayout.BeginHorizontal();
				if (_engineTuning.torqueCurve.Count > 3 && GUILayout.Button("Remove", GUILayout.MaxWidth(200)))
				{
					_engineTuning.torqueCurve.Remove(torque);
					break;
				}
				if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				{
					int key = _engineTuning.torqueCurve.IndexOf(torque);
					if (_engineTuning.defaultTorqueCurve.Count > key && _engineTuning.defaultTorqueCurve[key] != null)
					{
						TorqueCurve defaultTorque = _engineTuning.defaultTorqueCurve[key];
						_engineTuning.torqueCurve[key] = defaultTorque;
						updateEngineStats = true;
						break;
					}
				}
				GUILayout.EndHorizontal();

				// Check for any changes and update engine stats.
				if (originalTorque != torque.torque || originalRpm != torque.rpm)
					updateEngineStats = true;

				GUILayout.Space(10);
				torqueIndex++;
			}
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add new", GUILayout.MaxWidth(200)))
			{
				_engineTuning.torqueCurve.Add(new TorqueCurve(0, _engineTuning.torqueCurve[_engineTuning.torqueCurve.Count - 1].rpm));
				updateEngineStats = true;
			}
			GUILayout.Space(5);
			if (GUILayout.Button("Reorder by RPM", GUILayout.MaxWidth(200)))
			{
				_engineTuning.torqueCurve = _engineTuning.torqueCurve.OrderBy(t => t.rpm).ToList();
				updateEngineStats = true;
			}
			GUILayout.Space(5);
			if (GUILayout.Button("Reset torque curve to stock", GUILayout.MaxWidth(200)))
			{
				_engineTuning.torqueCurve = _engineTuning.defaultTorqueCurve.Copy();
				updateEngineStats = true;
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			GUILayout.EndScrollView();

			GUILayout.Space(10);

			if (updateEngineStats)
				UpdateEngineTunerStats();

			GUILayout.BeginVertical("box", _isEngineTuningStatsOpen ? GUILayout.MinHeight(dimensions.height / 1.25f) : GUILayout.MinHeight(20));
			if (_isEngineTuningStatsOpen)
			{
				_tunerStatsPosition = GUILayout.BeginScrollView(_tunerStatsPosition);
				GUILayout.BeginVertical(GUILayout.MinHeight(dimensions.height / 2f), GUILayout.MaxHeight(dimensions.height - 20f));
				GUILayout.Label("Engine statistics", "LabelHeader");
				GUILayout.Label($"Max torque: {_engineStats.maxTorque.ToString("F2")}Nm");
				GUILayout.Label($"Max RPM: {_engineStats.maxRPM.ToString("F2")}");
				GUILayout.Label($"Max horsepower: {_engineStats.maxHp.ToString("F2")}");
				if (GUILayout.Button(Accessibility.GetAccessibleString("Hide last graph point", _hideLastTorquePoint), GUILayout.MaxWidth(200)))
				{
					_hideLastTorquePoint = !_hideLastTorquePoint;
					UpdateEngineTunerStats();
				}
				GUILayout.Label(_engineStats.torqueGraph);
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
				GUILayout.EndScrollView();
			}

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Apply", GUILayout.MaxWidth(200)))
			{
				SaveUtilities.UpdateEngineTuning(new EngineTuningData() { ID = engineSave.idInSave, tuning = _engineTuning });
				GameUtilities.ApplyEngineTuning(engine, _engineTuning);
			}

			if (GUILayout.Button("Reset tuning to stock", GUILayout.MaxWidth(200)))
			{
				_engineTuning.rpmChangeModifier = _engineTuning.defaultRpmChangeModifier;
				_engineTuning.startChance = _engineTuning.defaultStartChance;
				_engineTuning.motorBrakeModifier = _engineTuning.defaultMotorBrakeModifier;
				_engineTuning.minOptimalTemp2 = _engineTuning.defaultMinOptimalTemp2;
				_engineTuning.maxOptimalTemp2 = _engineTuning.defaultMaxOptimalTemp2;
				_engineTuning.engineHeatGainMin = _engineTuning.defaultEngineHeatGainMin;
				_engineTuning.engineHeatGainMax = _engineTuning.defaultEngineHeatGainMax;
				_engineTuning.noOverheat = _engineTuning.defaultNoOverheat;
				_engineTuning.twoStroke = _engineTuning.defaultTwoStroke;
				_engineTuning.oilFluid = _engineTuning.defaultOilFluid;
				_engineTuning.oilTolerationMin = _engineTuning.defaultOilTolerationMin;
				_engineTuning.oilTolerationMax = _engineTuning.defaultOilTolerationMax;
				_engineTuning.oilConsumptionModifier = _engineTuning.defaultOilConsumptionModifier;
				_engineTuning.consumption = _engineTuning.defaultConsumption.Copy();
				_engineTuning.torqueCurve = _engineTuning.defaultTorqueCurve.Copy();
				UpdateEngineTunerStats();
			}

			GUILayout.FlexibleSpace();
			if (GUILayout.Button(Accessibility.GetAccessibleString("Toggle stats", _isEngineTuningStatsOpen), GUILayout.MaxWidth(200)))
				_isEngineTuningStatsOpen = !_isEngineTuningStatsOpen;
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			GUILayout.EndArea();
		}

		/// <summary>
		/// Trigger engine statistics update.
		/// </summary>
		private void UpdateEngineTunerStats()
		{
			float maxRPM = _engineTuning.torqueCurve.Last().rpm;
			float maxTorqueRPM = 0;
			float maxTorque = 0;

			List<double> graphX = new List<double>();
			List<double> torqueGraphY = new List<double>();
			List<double> hpGraphY = new List<double>();

			foreach (TorqueCurve torque in _engineTuning.torqueCurve)
			{
				if (torque.torque > maxTorque)
				{
					maxTorque = torque.torque;
					maxTorqueRPM = torque.rpm;
				}

				if (_hideLastTorquePoint && torque == _engineTuning.torqueCurve.Last())
					break;

				graphX.Add((double)new decimal(torque.rpm));
				torqueGraphY.Add((double)new decimal(torque.torque));
				hpGraphY.Add((double)new decimal(0.0001403f * torque.torque * torque.rpm));
			}
			float maxHp = 0.0001403f * maxTorque * maxTorqueRPM;

			ScottPlot.Plot graph = new ScottPlot.Plot();
			graph.AddScatter(graphX.ToArray(), torqueGraphY.ToArray(), label: "Torque (Nm)");
			graph.AddScatter(graphX.ToArray(), hpGraphY.ToArray(), label: "Horsepower");

			graph.XLabel("RPM");
			graph.YLabel("Torque(Nm)/Horsepower");
			graph.Legend(true, ScottPlot.Alignment.LowerCenter);

			byte[] graphBytes = graph.GetImageBytes();
			Texture2D graphTexture = new Texture2D(1, 1);
			graphTexture.LoadImage(graphBytes);
			_engineStats = new EngineStats()
			{
				maxTorque = maxTorque,
				maxRPM = maxRPM,
				maxHp = maxHp,
				torqueGraph = graphTexture,
			};
		}
	}
}
