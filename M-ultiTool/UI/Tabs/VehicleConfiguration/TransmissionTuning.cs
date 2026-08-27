using MultiTool.Extensions;
using MultiTool.Save;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiTool.UI.Tabs.VehicleConfiguration
{
	internal sealed class TransmissionTuningTab : UI.VehicleConfigurationTab
	{
		public override string Name => "Transmission Tuning";

		private Vector2 _position;
		private Vector2 _footerPosition;
		private TransmissionTuning _transmissionTuning = null;
		private TransmissionTuning _defaultTuning = null;
		private TransmissionTuning _lastSavedTuning = null;
		private bool _sharingOpen = false;
		private string _export;
		private string _import;
		private TuningSave _saved;

		public override void OnVehicleChange()
		{
			_transmissionTuning = null;
			_lastSavedTuning = null;
			_export = null;
		}

		public override void RenderTab(Rect dimensions)
		{
			carscript car = mainscript.M.player.Car;
			tosaveitemscript save = car.GetComponent<tosaveitemscript>();

			int gearIndex = 1;
			// Populate default tuning values if missing.
			if (_transmissionTuning == null || _defaultTuning == null)
			{
				// Attempt to load data from save.
				_transmissionTuning = SaveUtilities.GetTransmissionTuning(save.idInSave);
				_defaultTuning = SaveUtilities.GetDefaultTransmissionTuning(save.idInSave);

				// Save has no data for this transmission, load defaults.
				if (_transmissionTuning == null || _defaultTuning == null)
				{
					_transmissionTuning = new TransmissionTuning()
					{
						Gears = new List<Gear>(),
						DifferentialRatio = car.differentialRatio,
						DriveTrain = GameUtilities.GetDrivetrain(car),
					};

					_defaultTuning = new TransmissionTuning()
					{
						Gears = new List<Gear>(),
						DifferentialRatio = car.differentialRatio,
						DriveTrain = GameUtilities.GetDrivetrain(car),
					};

					// Populate gearing.
					gearIndex = 1;
					foreach (carscript.gearc gear in car.gears)
					{
						_transmissionTuning.Gears.Add(new Gear(gearIndex, gear.ratio, gear.freeRun) { });
						_defaultTuning.Gears.Add(new Gear(gearIndex, gear.ratio, gear.freeRun) { });
						gearIndex++;
					}
				}
			}

			GUILayout.BeginArea(dimensions);
			_position = GUILayout.BeginScrollView(_position);

			GUILayout.BeginVertical();

			GUILayout.Label("Differential", "LabelHeader");
			GUILayout.BeginVertical();
			GUILayout.Label("Differential ratio");
			GUILayout.Label("Smaller number: less acceleration, higher top speed (Taller gearing)");
			GUILayout.Label("Bigger number: more acceleration, lower top speed (Shorter gearing)");
			_transmissionTuning.DifferentialRatio = GUILayout.HorizontalSlider(_transmissionTuning.DifferentialRatio, 0f, 20f);
			float.TryParse(GUILayout.TextField(_transmissionTuning.DifferentialRatio.ToString("F2"), GUILayout.MaxWidth(200)), out _transmissionTuning.DifferentialRatio);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_transmissionTuning.DifferentialRatio = _defaultTuning.DifferentialRatio;
			GUILayout.EndVertical();
			GUILayout.Space(10);

			GUILayout.Label($"Drivetrain", "LabelHeader");
			GUILayout.BeginVertical();
			foreach (int drivetrain in Enum.GetValues(typeof(Drivetrain)))
			{
				if (GUILayout.Button(Accessibility.GetAccessibleString(Enum.GetName(typeof(Drivetrain), drivetrain), drivetrain == (int)_transmissionTuning.DriveTrain), GUILayout.MaxWidth(200)))
					_transmissionTuning.DriveTrain = (Drivetrain)drivetrain;
			}
			GUILayout.EndVertical();
			GUILayout.Space(10);

			GUILayout.Label("Gears and ratios", "LabelHeader");
			gearIndex = 1;
			foreach (Gear gear in _transmissionTuning.Gears)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Gear");
				int.TryParse(GUILayout.TextField(gear.GearNumber.ToString(), GUILayout.MaxWidth(200)), out gear.GearNumber);
				string helpText = string.Empty;
				switch (gear.GearNumber)
				{
					case 1:
						helpText = "Reverse";
						break;
					case 2:
						helpText = "Neutral";
						break;
					default:
						helpText = $"Gear {gear.GearNumber - 2}";
						break;
				}
				GUILayout.Label(helpText != string.Empty ? $"({helpText})" : string.Empty);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.Space(5);

				GUILayout.Label("Ratio");
				gear.Ratio = GUILayout.HorizontalSlider(gear.Ratio, -50, 50);
				float.TryParse(GUILayout.TextField(gear.Ratio.ToString("F2"), GUILayout.MaxWidth(200)), out gear.Ratio);

				GUILayout.Label("Free run");
				if (GUILayout.Button(Accessibility.GetAccessibleString("Yes", "No", gear.FreeRun), GUILayout.MaxWidth(200)))
					gear.FreeRun = !gear.FreeRun;

				GUILayout.Space(5);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Remove", GUILayout.MaxWidth(200)))
				{
					_transmissionTuning.Gears.Remove(gear);
					break;
				}
				if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				{
					if (_transmissionTuning.Gears.Count > gearIndex && _defaultTuning.Gears[gearIndex] != null)
					{
						Gear defaultGear = _defaultTuning.Gears[gearIndex];
						_transmissionTuning.Gears[gearIndex] = defaultGear;
						break;
					}
				}
				GUILayout.EndHorizontal();

				GUILayout.Space(20);
				gearIndex++;
			}
			GUILayout.Space(10);

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add new", GUILayout.MaxWidth(200)))
				_transmissionTuning.Gears.Add(new Gear(_transmissionTuning.Gears.Count + 1, 1, false));
			GUILayout.Space(5);
			if (GUILayout.Button("Reorder by gear", GUILayout.MaxWidth(200)))
				_transmissionTuning.Gears = _transmissionTuning.Gears.OrderBy(t => t.GearNumber).ToList();
			GUILayout.Space(5);
			if (GUILayout.Button("Reset gearing to stock", GUILayout.MaxWidth(200)))
				_transmissionTuning.Gears = _defaultTuning.Gears.Copy();
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			GUILayout.EndScrollView();

			GUILayout.Space(10);

			GUILayout.BeginVertical("box", _sharingOpen ? GUILayout.MinHeight(dimensions.height / 1.25f) : GUILayout.MinHeight(20));
			if (_sharingOpen)
			{
				_footerPosition = GUILayout.BeginScrollView(_footerPosition);
				GUILayout.BeginVertical(GUILayout.MinHeight(dimensions.height / 2f), GUILayout.MaxHeight(dimensions.height - 20f));
				GUILayout.Label("Exporting", "LabelSubHeader");
				if (GUILayout.Button("Export current tuning", GUILayout.MaxWidth(200)))
					_export = new TuningSave()
					{
						Part = car.name,
						Type = "transmission",
						Car = car.name,
						Tuning = _transmissionTuning,
					}
					.ToExportString();
				if (!string.IsNullOrEmpty(_export))
				{
					GUILayout.Label("Exported tuning:");
					GUILayout.Label("Copy and paste the below to someone to share the transmission tuning with them.");
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
					if (_saved.Type != "transmission")
					{
						Notifications.SendError("Import failed", "Not a valid transmission tune.");
						_saved = null;
					}
					else if (_saved.Part != car.name)
					{
						GUILayout.Label("This tune is not designed for this vehicle, import anyway?");
						if (GUILayout.Button("Import anyway", GUILayout.MaxWidth(200)))
						{
							_transmissionTuning = _saved.Tuning as TransmissionTuning;
							_saved = null;
							_import = null;
							Notifications.SendSuccess("Transmission tuning", "Tuning imported");
						}
					}
					else
					{
						_transmissionTuning = _saved.Tuning as TransmissionTuning;
						_saved = null;
						_import = null;
						Notifications.SendSuccess("Transmission tuning", "Tuning imported");
					}
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
				GUILayout.EndScrollView();
			}
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Apply", GUILayout.MaxWidth(200)))
			{
				SaveUtilities.UpdateTransmissionTuning(new TransmissionTuningData() { ID = save.idInSave, Tuning = _transmissionTuning, DefaultTuning = _defaultTuning });
				GameUtilities.ApplyTransmissionTuning(car, _transmissionTuning);
				_lastSavedTuning = _transmissionTuning.DeepCopy();
			}

			if (!ObjectExtensions.AreDataMembersEqual(_transmissionTuning, _lastSavedTuning))
			{
				GUILayout.Label("Unapplied changes detected!", GUILayout.ExpandWidth(false));
				GUILayout.Space(2);
			}

			if (GUILayout.Button("Reset tuning to stock", GUILayout.MaxWidth(200)))
			{
				_transmissionTuning.Gears = _defaultTuning.Gears.Copy();
				_transmissionTuning.DifferentialRatio = _defaultTuning.DifferentialRatio;
			}

			GUILayout.FlexibleSpace();
			if (GUILayout.Button(Accessibility.GetAccessibleString("Tuning sharing", _sharingOpen), GUILayout.MaxWidth(200)))
			{
				_sharingOpen = !_sharingOpen;
				_footerPosition = Vector2.zero;
			}

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUILayout.EndArea();

			// Perform the deep copy last to ensure any defaults are set correctly first.
			if (_lastSavedTuning == null)
				_lastSavedTuning = _transmissionTuning.DeepCopy();
		}
	}
}
