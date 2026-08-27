using MultiTool.Extensions;
using MultiTool.Save;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static mainscript;

namespace MultiTool.UI.Tabs.VehicleConfiguration
{
	internal sealed class EngineTuningTab : UI.VehicleConfigurationTab
	{
		public override string Name => "Engine Tuning";

		private Vector2 _position;
		private Vector2 _footerPosition;
		private Vector3 _sidebarPosition;

		private enum FooterPane
		{
			None,
			EngineStats,
			Share,
		}

		private EngineTuning _engineTuning = null;
		private EngineTuning _defaultTuning = null;
		private EngineTuning _lastSavedTuning = null;
		private FooterPane _footerPane = FooterPane.None;
		private bool _sidebarOpen = false;
		private EngineStats _engineStats = null;
		private bool _hideLastTorquePoint = false;
		private int _maxFluidIndex = 0;
		private string _export;
		private string _import;
		private TuningSave _saved;
		private string _saveTuneName;

		public override void OnRegister()
		{
			_maxFluidIndex = (int)Enum.GetValues(typeof(fluidenum)).Cast<fluidenum>().Max();
		}

		public override void OnVehicleChange()
		{
			_engineTuning = null;
			_lastSavedTuning = null;
			_export = null;
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
			if (_engineTuning == null || _defaultTuning == null)
			{
				// Attempt to load data from save.
				_engineTuning = SaveUtilities.GetEngineTuning(engineSave.idInSave);
				_defaultTuning = SaveUtilities.GetDefaultEngineTuning(engineSave.idInSave);

				// Save has no data for this engine, load defaults.
				if (_engineTuning == null || _defaultTuning == null)
				{
					_engineTuning = new EngineTuning()
					{
						RpmChangeModifier = engine.rpmChangeModifier,
						StartChance = engine.startChance,
						MotorBrakeModifier = engine.motorBrakeModifier,
						MinOptimalTemp2 = engine.minOptimalTemp2,
						MaxOptimalTemp2 = engine.maxOptimalTemp2,
						EngineHeatGainMin = engine.engineHeatGainMin,
						EngineHeatGainMax = engine.engineHeatGainMax,
						ConsumptionModifier = engine.consumptionM,
						NoOverheat = engine.noOverHeat,
						TwoStroke = engine.twostroke,
						OilFluid = engine.Oilfluid,
						OilTolerationMin = engine.oilTolerationMin,
						OilTolerationMax = engine.oilTolerationMax,
						OilConsumptionModifier = engine.OilConsumptionModifier,
						Consumption = new List<Fluid>(),
						TorqueCurve = new List<TorqueCurve>(),
					};

					_defaultTuning = new EngineTuning()
					{
						RpmChangeModifier = engine.rpmChangeModifier,
						StartChance = engine.startChance,
						MotorBrakeModifier = engine.motorBrakeModifier,
						MinOptimalTemp2 = engine.minOptimalTemp2,
						MaxOptimalTemp2 = engine.maxOptimalTemp2,
						EngineHeatGainMin = engine.engineHeatGainMin,
						EngineHeatGainMax = engine.engineHeatGainMax,
						ConsumptionModifier = engine.consumptionM,
						NoOverheat = engine.noOverHeat,
						TwoStroke = engine.twostroke,
						OilFluid = engine.Oilfluid,
						OilTolerationMin = engine.oilTolerationMin,
						OilTolerationMax = engine.oilTolerationMax,
						OilConsumptionModifier = engine.OilConsumptionModifier,
						Consumption = new List<Fluid>(),
						TorqueCurve = new List<TorqueCurve>(),
					};

					// Populate fuel consumption fluids.
					foreach (fluid fluid in engine.FuelConsumption.fluids)
					{
						_engineTuning.Consumption.Add(new Fluid() { Type = fluid.type, Amount = fluid.amount });
						_defaultTuning.Consumption.Add(new Fluid() { Type = fluid.type, Amount = fluid.amount });
					}

					// Populate torque curve.
					for (int torqueKey = 0; torqueKey < engine.torqueCurve.length; torqueKey++)
					{
						Keyframe torque = engine.torqueCurve.keys[torqueKey];
						_engineTuning.TorqueCurve.Add(new TorqueCurve(torque.value, torque.time));
						_defaultTuning.TorqueCurve.Add(new TorqueCurve(torque.value, torque.time));
					}
				}

				UpdateEngineTunerStats();
			}

			bool updateEngineStats = false;

			GUILayout.BeginArea(dimensions);

			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();
			_position = GUILayout.BeginScrollView(_position);

			GUILayout.Label("Basics", "LabelHeader");

			GUILayout.BeginVertical();
			GUILayout.Label("RPM change modifier (responsiveness)");
			_engineTuning.RpmChangeModifier = GUILayout.HorizontalSlider(_engineTuning.RpmChangeModifier, 0f, 10f);
			float.TryParse(GUILayout.TextField(_engineTuning.RpmChangeModifier.ToString("F2"), GUILayout.MaxWidth(200)), out _engineTuning.RpmChangeModifier);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.RpmChangeModifier = _defaultTuning.RpmChangeModifier;
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.BeginVertical();
			GUILayout.Label("Start chance");
			_engineTuning.StartChance = GUILayout.HorizontalSlider(_engineTuning.StartChance, 0f, 1f);
			GUILayout.BeginHorizontal();
			if (float.TryParse(GUILayout.TextField((_engineTuning.StartChance * 100).ToString("F0"), GUILayout.MaxWidth(200)), out float startChance))
				_engineTuning.StartChance = startChance / 100;
			GUILayout.Label("%");
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.StartChance = _defaultTuning.StartChance;
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.BeginVertical();
			GUILayout.Label("Engine brake modifier");
			_engineTuning.MotorBrakeModifier = GUILayout.HorizontalSlider(_engineTuning.MotorBrakeModifier, 0f, 10f);
			float.TryParse(GUILayout.TextField(_engineTuning.MotorBrakeModifier.ToString("F2"), GUILayout.MaxWidth(200)), out _engineTuning.MotorBrakeModifier);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.MotorBrakeModifier = _defaultTuning.MotorBrakeModifier;
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.Label("Temperature", "LabelHeader");

			GUILayout.BeginVertical();
			GUILayout.Label("Min optimal temp");
			_engineTuning.MinOptimalTemp2 = GUILayout.HorizontalSlider(_engineTuning.MinOptimalTemp2, 0f, 300f);
			float.TryParse(GUILayout.TextField(_engineTuning.MinOptimalTemp2.ToString("F2"), GUILayout.MaxWidth(200)), out _engineTuning.MinOptimalTemp2);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.MinOptimalTemp2 = _defaultTuning.MinOptimalTemp2;
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.BeginVertical();
			GUILayout.Label("Max optimal temp");
			_engineTuning.MaxOptimalTemp2 = GUILayout.HorizontalSlider(_engineTuning.MaxOptimalTemp2, 0f, 300f);
			float.TryParse(GUILayout.TextField(_engineTuning.MaxOptimalTemp2.ToString("F2"), GUILayout.MaxWidth(200)), out _engineTuning.MaxOptimalTemp2);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.MaxOptimalTemp2 = _defaultTuning.MaxOptimalTemp2;
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.BeginVertical();
			GUILayout.Label("Engine heat gain min");
			_engineTuning.EngineHeatGainMin = GUILayout.HorizontalSlider(_engineTuning.EngineHeatGainMin, 0f, 300f);
			float.TryParse(GUILayout.TextField(_engineTuning.EngineHeatGainMin.ToString("F2"), GUILayout.MaxWidth(200)), out _engineTuning.EngineHeatGainMin);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.EngineHeatGainMin = _defaultTuning.EngineHeatGainMin;
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.BeginVertical();
			GUILayout.Label("Engine heat gain max");
			_engineTuning.EngineHeatGainMax = GUILayout.HorizontalSlider(_engineTuning.EngineHeatGainMax, 0f, 300f);
			float.TryParse(GUILayout.TextField(_engineTuning.EngineHeatGainMax.ToString("F2"), GUILayout.MaxWidth(200)), out _engineTuning.EngineHeatGainMax);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.EngineHeatGainMax = _defaultTuning.EngineHeatGainMax;
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.BeginHorizontal();
			GUILayout.Label("No overheat");
			if (GUILayout.Button(Accessibility.GetAccessibleString("Yes", "No", _engineTuning.NoOverheat), GUILayout.MaxWidth(200)))
				_engineTuning.NoOverheat = !_engineTuning.NoOverheat;
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.NoOverheat = _defaultTuning.NoOverheat;
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.Space(10);

			GUILayout.Label("Oil", "LabelHeader");

			if (!_defaultTuning.TwoStroke)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Is two-stroke?");
				if (GUILayout.Button(Accessibility.GetAccessibleString("Yes", "No", _engineTuning.TwoStroke), GUILayout.MaxWidth(200)))
					_engineTuning.TwoStroke = !_engineTuning.TwoStroke;
				if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
					_engineTuning.TwoStroke = _defaultTuning.TwoStroke;
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				GUILayout.Space(10);
			}

			GUILayout.BeginVertical();
			GUILayout.Label($"Oil fluid - {_engineTuning.OilFluid.ToString().ToSentenceCase()}");
			for (int oilFluidIndex = 0; oilFluidIndex <= _maxFluidIndex; oilFluidIndex++)
			{
				fluidenum oilFluid = (fluidenum)oilFluidIndex;
				// Skip currently set fluid.
				if (oilFluid == _engineTuning.OilFluid) continue;
				if (GUILayout.Button(oilFluid.ToString().ToSentenceCase(), GUILayout.MaxWidth(200)))
					_engineTuning.OilFluid = oilFluid;
			}
			GUILayout.Space(5);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.OilFluid = _defaultTuning.OilFluid;
			GUILayout.EndVertical();

			if (_engineTuning.TwoStroke)
			{
				GUILayout.Space(10);

				GUILayout.BeginVertical();
				GUILayout.Label("Two-stroke oil toleration min");
				_engineTuning.OilTolerationMin = GUILayout.HorizontalSlider(_engineTuning.OilTolerationMin, 0f, 1f);
				GUILayout.BeginHorizontal();
				if (float.TryParse(GUILayout.TextField((_engineTuning.OilTolerationMin * 100).ToString("F2"), GUILayout.MaxWidth(200)), out float oilTolerationMin))
					_engineTuning.OilTolerationMin = oilTolerationMin / 100;
				GUILayout.Label("%");
				GUILayout.EndHorizontal();
				if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
					_engineTuning.OilTolerationMin = _defaultTuning.OilTolerationMin;
				GUILayout.EndVertical();

				GUILayout.Space(10);

				GUILayout.BeginVertical();
				GUILayout.Label("Two-stroke oil toleration max");
				_engineTuning.OilTolerationMax = GUILayout.HorizontalSlider(_engineTuning.OilTolerationMax, 0f, 1f);
				GUILayout.BeginHorizontal();
				if (float.TryParse(GUILayout.TextField((_engineTuning.OilTolerationMax * 100).ToString("F2"), GUILayout.MaxWidth(200)), out float oilTolerationMax))
					_engineTuning.OilTolerationMax = oilTolerationMax / 100;
				GUILayout.Label("%");
				GUILayout.EndHorizontal();
				if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
					_engineTuning.OilTolerationMax = _defaultTuning.OilTolerationMax;
				GUILayout.EndVertical();
			}

			GUILayout.Space(10);

			GUILayout.BeginVertical();
			GUILayout.Label("Oil consumption modifier");
			_engineTuning.OilConsumptionModifier = GUILayout.HorizontalSlider(_engineTuning.OilConsumptionModifier, 0f, 10f);
			float.TryParse(GUILayout.TextField(_engineTuning.OilConsumptionModifier.ToString("F2"), GUILayout.MaxWidth(200)), out _engineTuning.OilConsumptionModifier);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.OilConsumptionModifier = _defaultTuning.OilConsumptionModifier;
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.Label("Fuel", "LabelHeader");

			GUILayout.BeginVertical();
			GUILayout.Label("Fuel consumption modifier");
			_engineTuning.ConsumptionModifier = GUILayout.HorizontalSlider(_engineTuning.ConsumptionModifier, 0f, 10f);
			float.TryParse(GUILayout.TextField(_engineTuning.ConsumptionModifier.ToString("F2"), GUILayout.MaxWidth(200)), out _engineTuning.ConsumptionModifier);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.ConsumptionModifier = _defaultTuning.ConsumptionModifier;
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.BeginVertical();
			GUILayout.Label("Fuel consumption", "LabelHeader");
			foreach (Fluid fluid in _engineTuning.Consumption)
			{
				for (int fuelFluidIndex = 0; fuelFluidIndex <= _maxFluidIndex; fuelFluidIndex++)
				{
					// Skip fluids already selected.
					if (_engineTuning.Consumption.Where(f => (int)f.Type == fuelFluidIndex && f.Type != fluid.Type).FirstOrDefault() != null)
						continue;

					fluidenum fuelFluid = (fluidenum)fuelFluidIndex;
					if (GUILayout.Button(Accessibility.GetAccessibleString(fuelFluid.ToString().ToSentenceCase(), fuelFluid == fluid.Type), GUILayout.MaxWidth(200)))
						fluid.Type = fuelFluid;
				}
				fluid.Amount = GUILayout.HorizontalSlider(fluid.Amount, 0f, 500f);
				float.TryParse(GUILayout.TextField(fluid.Amount.ToString("F2"), GUILayout.MaxWidth(200)), out fluid.Amount);
				GUILayout.Space(5);
				if (GUILayout.Button("Remove fluid", GUILayout.MaxWidth(200)))
				{
					_engineTuning.Consumption.Remove(fluid);
					break;
				}
				GUILayout.Space(10);
			}
			if (_engineTuning.Consumption.Count <= _maxFluidIndex)
			{
				if (GUILayout.Button("Add another fluid", GUILayout.MaxWidth(200)))
				{
					// Find the next unused fluid index.
					List<int> existingIndexes = new List<int>();
					foreach (Fluid existing in _engineTuning.Consumption)
					{
						existingIndexes.Add((int)existing.Type);
					}
					existingIndexes.Sort();
					int index = existingIndexes.Last() + 1;
					_engineTuning.Consumption.Add(new Fluid() { Type = (fluidenum)index, Amount = 0 });
				}
			}
			GUILayout.Space(5);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_engineTuning.Consumption = _defaultTuning.Consumption;
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.BeginVertical();
			GUILayout.Label("Torque curve", "LabelHeader");
			int torqueIndex = 0;
			foreach (TorqueCurve torque in _engineTuning.TorqueCurve)
			{
				float originalTorque = torque.Torque;
				float originalRpm = torque.Rpm;

				bool lastIndex = torqueIndex == _engineTuning.TorqueCurve.Count - 1;
				bool firstIndex = torqueIndex == 0;

				GUILayout.Label($"Torque {(firstIndex || lastIndex ? "(Should be zero)" : string.Empty)}");
				// Lock first or last curve point to zero.
				torque.Torque = GUILayout.HorizontalSlider(torque.Torque, 0, firstIndex || lastIndex ? 0 : 1000);
				float.TryParse(GUILayout.TextField(torque.Torque.ToString("F2"), GUILayout.MaxWidth(200)), out torque.Torque);

				GUILayout.Label($"RPM {(firstIndex ? "(Should be zero)" : string.Empty)}");
				torque.Rpm = GUILayout.HorizontalSlider(torque.Rpm, 0, firstIndex ? 0 : 20000);
				float.TryParse(GUILayout.TextField(torque.Rpm.ToString("F2"), GUILayout.MaxWidth(200)), out torque.Rpm);

				GUILayout.Space(5);
				GUILayout.BeginHorizontal();
				if (_engineTuning.TorqueCurve.Count > 3 && GUILayout.Button("Remove", GUILayout.MaxWidth(200)))
				{
					_engineTuning.TorqueCurve.Remove(torque);
					break;
				}
				if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				{
					int key = _engineTuning.TorqueCurve.IndexOf(torque);
					if (_defaultTuning.TorqueCurve.Count > key && _defaultTuning.TorqueCurve[key] != null)
					{
						TorqueCurve defaultTorque = _defaultTuning.TorqueCurve[key];
						_engineTuning.TorqueCurve[key] = defaultTorque;
						updateEngineStats = true;
						break;
					}
				}
				GUILayout.EndHorizontal();

				// Check for any changes and update engine stats.
				if (originalTorque != torque.Torque || originalRpm != torque.Rpm)
					updateEngineStats = true;

				GUILayout.Space(10);
				torqueIndex++;
			}
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add new", GUILayout.MaxWidth(200)))
			{
				_engineTuning.TorqueCurve.Add(new TorqueCurve(0, _engineTuning.TorqueCurve[_engineTuning.TorqueCurve.Count - 1].Rpm));
				updateEngineStats = true;
			}
			GUILayout.Space(5);
			if (GUILayout.Button("Reorder by RPM", GUILayout.MaxWidth(200)))
			{
				_engineTuning.TorqueCurve = _engineTuning.TorqueCurve.OrderBy(t => t.Rpm).ToList();
				updateEngineStats = true;
			}
			GUILayout.Space(5);
			if (GUILayout.Button("Reset torque curve to stock", GUILayout.MaxWidth(200)))
			{
				_engineTuning.TorqueCurve = _defaultTuning.TorqueCurve.Copy();
				updateEngineStats = true;
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			GUILayout.EndScrollView();

			GUILayout.Space(10);

			if (updateEngineStats)
				UpdateEngineTunerStats();
			GUILayout.EndVertical();

			if (_sidebarOpen)
			{
				GUILayout.Space(10);
				GUILayout.BeginVertical(GUILayout.MinWidth(dimensions.width * 0.3f));
				GUILayout.Label("Saved engine tunes", "LabelHeader");
				GUILayout.Space(5);
				GUILayout.BeginHorizontal();
				_saveTuneName = GUILayout.TextField(_saveTuneName);
				if (GUILayout.Button("Save current tune", GUILayout.ExpandWidth(false)))
				{
					if (string.IsNullOrEmpty(_saveTuneName))
					{
						Notifications.SendError("Engine tuning", "Please provide a tune name.");
						return;
					}

					SaveUtilities.AddTune(new TuningSave()
					{
						Name = _saveTuneName,
						Part = engine.name,
						Type = "engine",
						Car = car.name,
						Tuning = _engineTuning,
					});
					_saveTuneName = null;
				}
				GUILayout.EndHorizontal();

				_sidebarPosition = GUILayout.BeginScrollView(_sidebarPosition);

				foreach (TuningSave tune in SaveUtilities.GetTunesByType("engine"))
				{
					GUILayout.BeginVertical("box");
					GUILayout.Label(tune.Name, "LabelSubHeader");
					GUILayout.Label($"For engine: {tune.Part}");
					if (tune.Part != engine.name)
						GUILayout.Label("Warning: Not designed for current engine");
					GUILayout.Label($"Built using vehicle: {tune.Car}");
					GUILayout.BeginHorizontal();
					if (GUILayout.Button("Use tune", GUILayout.ExpandWidth(false)))
					{
						_engineTuning = tune.Tuning as EngineTuning;
						Notifications.SendSuccess("Engine tuning", "Tune applied to current settings.");
					}
					GUILayout.Space(10);
					if (GUILayout.Button("Remove tune", "ButtonSecondary", GUILayout.ExpandWidth(false)))
					{
						SaveUtilities.RemoveTune(tune);
						Notifications.SendSuccess("Engine tuning", "Tune has been deleted.");
						break;
					}
					GUILayout.EndHorizontal();
					GUILayout.Space(10);
					GUILayout.EndVertical();
				}

				GUILayout.EndScrollView();

				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginVertical("box", _footerPane != FooterPane.None ? GUILayout.MinHeight(dimensions.height / 1.25f) : GUILayout.MinHeight(20));
			switch (_footerPane)
			{
				case FooterPane.EngineStats:
					_footerPosition = GUILayout.BeginScrollView(_footerPosition);
					GUILayout.BeginVertical(GUILayout.MinHeight(dimensions.height / 2f), GUILayout.MaxHeight(dimensions.height - 20f));
					GUILayout.Label("Engine statistics", "LabelHeader");
					GUILayout.Label($"Max torque: {_engineStats.MaxTorque.ToString("F2")}Nm");
					GUILayout.Label($"Max RPM: {_engineStats.MaxRPM.ToString("F2")}");
					GUILayout.Label($"Max horsepower: {_engineStats.MaxHp.ToString("F2")}");
					if (GUILayout.Button(Accessibility.GetAccessibleString("Hide last graph point", _hideLastTorquePoint), GUILayout.MaxWidth(200)))
					{
						_hideLastTorquePoint = !_hideLastTorquePoint;
						UpdateEngineTunerStats();
					}
					GUILayout.Label(_engineStats.TorqueGraph);
					GUILayout.FlexibleSpace();
					GUILayout.EndVertical();
					GUILayout.EndScrollView();
					break;
				case FooterPane.Share:
					_footerPosition = GUILayout.BeginScrollView(_footerPosition);
					GUILayout.BeginVertical(GUILayout.MinHeight(dimensions.height / 2f), GUILayout.MaxHeight(dimensions.height - 20f));
					GUILayout.Label("Exporting", "LabelSubHeader");
					if (GUILayout.Button("Export current tuning", GUILayout.MaxWidth(200)))
						_export = new TuningSave()
						{
							Part = engine.name,
							Type = "engine",
							Car = car.name,
							Tuning = _engineTuning,
						}
						.ToExportString();
					if (!string.IsNullOrEmpty(_export))
					{
						GUILayout.Label("Exported tuning:");
						GUILayout.Label("Copy and paste the below to someone to share the engine tuning with them.");
						GUILayout.TextArea(_export);
						GUILayout.Space(10);
					}

					GUILayout.Label("Importing", "LabelSubHeader");
					_import = GUILayout.TextArea(_import);
					if (GUILayout.Button("Import", GUILayout.MaxWidth(200)))
					{
						_saved = _import.ToObjectImport<TuningSave>();
					}
					if (_saved != null)
					{
						if (_saved.Type != "engine")
						{
							Notifications.SendError("Import failed", "Not a valid engine tune.");
							_saved = null;
						}
						else if (_saved.Part != engine.name)
						{
							GUILayout.Label("This tune is not designed for this engine, import anyway?");
							if (GUILayout.Button("Import anyway", GUILayout.MaxWidth(200)))
							{
								_engineTuning = _saved.Tuning as EngineTuning;
								_saved = null;
								_import = null;
								Notifications.SendSuccess("Engine tuning", "Tuning imported");
							}
						}
						else
						{
							_engineTuning = _saved.Tuning as EngineTuning;
							_saved = null;
							_import = null;
							Notifications.SendSuccess("Engine tuning", "Tuning imported");
						}
					}
					GUILayout.FlexibleSpace();
					GUILayout.EndVertical();
					GUILayout.EndScrollView();
					break;
			}

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Apply", GUILayout.MaxWidth(200)))
			{
				SaveUtilities.UpdateEngineTuning(new EngineTuningData() { ID = engineSave.idInSave, Tuning = _engineTuning, DefaultTuning = _defaultTuning });
				GameUtilities.ApplyEngineTuning(engine, _engineTuning);
				_lastSavedTuning = _engineTuning.DeepCopy();
			}

			if (!ObjectExtensions.AreDataMembersEqual(_engineTuning, _lastSavedTuning))
			{
				GUILayout.Label("Unapplied changes detected!", GUILayout.ExpandWidth(false));
				GUILayout.Space(2);
			}

			if (GUILayout.Button("Reset tuning to stock", GUILayout.MaxWidth(200)))
			{
				_engineTuning.RpmChangeModifier = _defaultTuning.RpmChangeModifier;
				_engineTuning.StartChance = _defaultTuning.StartChance;
				_engineTuning.MotorBrakeModifier = _defaultTuning.MotorBrakeModifier;
				_engineTuning.MinOptimalTemp2 = _defaultTuning.MinOptimalTemp2;
				_engineTuning.MaxOptimalTemp2 = _defaultTuning.MaxOptimalTemp2;
				_engineTuning.EngineHeatGainMin = _defaultTuning.EngineHeatGainMin;
				_engineTuning.EngineHeatGainMax = _defaultTuning.EngineHeatGainMax;
				_engineTuning.NoOverheat = _defaultTuning.NoOverheat;
				_engineTuning.TwoStroke = _defaultTuning.TwoStroke;
				_engineTuning.OilFluid = _defaultTuning.OilFluid;
				_engineTuning.OilTolerationMin = _defaultTuning.OilTolerationMin;
				_engineTuning.OilTolerationMax = _defaultTuning.OilTolerationMax;
				_engineTuning.OilConsumptionModifier = _defaultTuning.OilConsumptionModifier;
				_engineTuning.Consumption = _defaultTuning.Consumption.Copy();
				_engineTuning.TorqueCurve = _defaultTuning.TorqueCurve.Copy();
				UpdateEngineTunerStats();
			}

			GUILayout.FlexibleSpace();
			if (GUILayout.Button(Accessibility.GetAccessibleString("Tuning sharing", _footerPane == FooterPane.Share), GUILayout.MaxWidth(200)))
			{
				_footerPane = _footerPane == FooterPane.None ? FooterPane.Share : FooterPane.None;
				_footerPosition = Vector2.zero;
			}

			if (GUILayout.Button(Accessibility.GetAccessibleString("Toggle stats", _footerPane == FooterPane.EngineStats), GUILayout.MaxWidth(200)))
			{
				_footerPane = _footerPane == FooterPane.None ? FooterPane.EngineStats : FooterPane.None;
				_footerPosition = Vector2.zero;
			}
			GUILayout.Space(10);

			if (GUILayout.Button(Accessibility.GetAccessibleString("Saved tunes", _sidebarOpen), "ButtonSecondary"))
			{
				_sidebarOpen = !_sidebarOpen;
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			GUILayout.EndArea();

			// Perform the deep copy last to ensure any defaults are set correctly first.
			if (_lastSavedTuning == null)
				_lastSavedTuning = _engineTuning.DeepCopy();
		}

		/// <summary>
		/// Trigger engine statistics update.
		/// </summary>
		private void UpdateEngineTunerStats()
		{
			float maxRPM = _engineTuning.TorqueCurve.Last().Rpm;
			float maxTorqueRPM = 0;
			float maxTorque = 0;

			List<double> graphX = new List<double>();
			List<double> torqueGraphY = new List<double>();
			List<double> hpGraphY = new List<double>();

			foreach (TorqueCurve torque in _engineTuning.TorqueCurve)
			{
				if (torque.Torque > maxTorque)
				{
					maxTorque = torque.Torque;
					maxTorqueRPM = torque.Rpm;
				}

				if (_hideLastTorquePoint && torque == _engineTuning.TorqueCurve.Last())
					break;

				graphX.Add((double)new decimal(torque.Rpm));
				torqueGraphY.Add((double)new decimal(torque.Torque));
				hpGraphY.Add((double)new decimal(0.0001403f * torque.Torque * torque.Rpm));
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
				MaxTorque = maxTorque,
				MaxRPM = maxRPM,
				MaxHp = maxHp,
				TorqueGraph = graphTexture,
			};
		}
	}
}
