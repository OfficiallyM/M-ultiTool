using MultiTool.Core;
using MultiTool.Utilities.UI;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MultiTool.Extensions;

namespace MultiTool.Tabs.VehicleConfiguration
{
    internal sealed class TransmissionTuning : Core.VehicleConfigurationTab
	{
        public override string Name => "Transmission Tuning";

		private Vector2 _position;
		private Core.TransmissionTuning _transmissionTuning = null;
		private Core.TransmissionTuning _defaultTuning = null;

		public override void OnVehicleChange()
		{
			_transmissionTuning = null;
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
					_transmissionTuning = new Core.TransmissionTuning()
					{
						gears = new List<Gear>(),
					};

					_defaultTuning = new Core.TransmissionTuning()
					{
						gears = new List<Gear>(),
					};

					// Populate gearing.
					gearIndex = 1;
					foreach (carscript.gearc gear in car.gears)
					{
						_transmissionTuning.gears.Add(new Gear(gearIndex, gear.ratio, gear.freeRun) { });
						_defaultTuning.gears.Add(new Gear(gearIndex, gear.ratio, gear.freeRun) { });
						gearIndex++;
					}
				}
			}

			GUILayout.BeginArea(dimensions);
			_position = GUILayout.BeginScrollView(_position);

			GUILayout.BeginVertical();
			GUILayout.Label("Gears and ratios", "LabelHeader");
			gearIndex = 1;
			foreach (Gear gear in _transmissionTuning.gears)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Gear");
				int.TryParse(GUILayout.TextField(gear.gear.ToString(), GUILayout.MaxWidth(200)), out gear.gear);
				string helpText = string.Empty;
				switch (gear.gear)
				{
					case 1:
						helpText = "Reverse";
						break;
					case 2:
						helpText = "Neutral";
						break;
					default:
						helpText = $"Gear {gear.gear - 2}";
						break;
				}
				GUILayout.Label(helpText != string.Empty ? $"({helpText})" : string.Empty);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.Space(5);

				GUILayout.Label("Ratio");
				gear.ratio = GUILayout.HorizontalSlider(gear.ratio, -50, 50);
				float.TryParse(GUILayout.TextField(gear.ratio.ToString("F2"), GUILayout.MaxWidth(200)), out gear.ratio);

				GUILayout.Label("Free run");
				if (GUILayout.Button(Accessibility.GetAccessibleString("Yes", "No", gear.freeRun), GUILayout.MaxWidth(200)))
					gear.freeRun = !gear.freeRun;

				GUILayout.Space(5);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Remove", GUILayout.MaxWidth(200)))
				{
					_transmissionTuning.gears.Remove(gear);
					break;
				}
				if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				{
					if (_transmissionTuning.gears.Count > gearIndex && _defaultTuning.gears[gearIndex] != null)
					{
						Gear defaultGear = _defaultTuning.gears[gearIndex];
						_transmissionTuning.gears[gearIndex] = defaultGear;
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
				_transmissionTuning.gears.Add(new Gear(_transmissionTuning.gears.Count + 1, 1, false));
			GUILayout.Space(5);
			if (GUILayout.Button("Reorder by gear", GUILayout.MaxWidth(200)))
				_transmissionTuning.gears = _transmissionTuning.gears.OrderBy(t => t.gear).ToList();
			GUILayout.Space(5);
			if (GUILayout.Button("Reset gearing to stock", GUILayout.MaxWidth(200)))
				_transmissionTuning.gears = _defaultTuning.gears.Copy();
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			GUILayout.EndScrollView();

			GUILayout.Space(10);

			GUILayout.BeginVertical("box");
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Apply", GUILayout.MaxWidth(200)))
			{
				SaveUtilities.UpdateTransmissionTuning(new TransmissionTuningData() { ID = save.idInSave, tuning = _transmissionTuning });
				GameUtilities.ApplyTransmissionTuning(car, _transmissionTuning);
			}

			if (GUILayout.Button("Reset tuning to stock", GUILayout.MaxWidth(200)))
			{
				_transmissionTuning.gears = _defaultTuning.gears.Copy();
			}

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}
}
