using MultiTool.Core;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiTool.Tabs.VehicleConfiguration
{
    internal sealed class VehicleTuning : Core.VehicleConfigurationTab
	{
        public override string Name => "Vehicle Tuning";

		private Vector2 _position;
		private Core.VehicleTuning _vehicleTuning = null;

		public override void OnVehicleChange()
		{
			_vehicleTuning = null;
		}

		public override void RenderTab(Rect dimensions)
		{
			carscript car = mainscript.s.player.Car;
			tosaveitemscript save = car.GetComponent<tosaveitemscript>();

			// Populate default tuning values if missing.
			if (_vehicleTuning == null)
			{
				// Attempt to load data from save.
				//_vehicleTuning = SaveUtilities.GetVehicleTuning(save.idInSave);

				// Save has no data for this transmission, load defaults.
				if (_vehicleTuning == null)
				{
					_vehicleTuning = new Core.VehicleTuning()
					{
						steerAngle = car.steerAngle,
						defaultSteerAngle = car.steerAngle,

						brakePower = car.brakePower,
						defaultBrakePower = car.brakePower,

						differentialRatio = car.differentialRatio,
						defaultDifferentialRatio = car.differentialRatio,
					};
				}
			}

			GUILayout.BeginArea(dimensions);
			_position = GUILayout.BeginScrollView(_position);

			GUILayout.Label("Steering", "LabelHeader");
			GUILayout.BeginVertical();
			GUILayout.Label("Steering angle");
			_vehicleTuning.steerAngle = GUILayout.HorizontalSlider(_vehicleTuning.steerAngle, 0f, 90f);
			float.TryParse(GUILayout.TextField(_vehicleTuning.steerAngle.ToString("F2"), GUILayout.MaxWidth(200)), out _vehicleTuning.steerAngle);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_vehicleTuning.steerAngle = _vehicleTuning.defaultSteerAngle;
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.Label("Braking", "LabelHeader");
			GUILayout.BeginVertical();
			GUILayout.Label("Brake power");
			_vehicleTuning.brakePower = GUILayout.HorizontalSlider(_vehicleTuning.brakePower, 0f, 10000f);
			float.TryParse(GUILayout.TextField(_vehicleTuning.brakePower.ToString("F2"), GUILayout.MaxWidth(200)), out _vehicleTuning.brakePower);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_vehicleTuning.brakePower = _vehicleTuning.defaultBrakePower;
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.Label("Differential", "LabelHeader");
			GUILayout.BeginVertical();
			GUILayout.Label("Differential ratio");
			GUILayout.Label("Smaller number: less acceleration, higher top speed (Taller gearing)");
			GUILayout.Label("Bigger number: more acceleration, lower top speed (Shorter gearing)");
			_vehicleTuning.differentialRatio = GUILayout.HorizontalSlider(_vehicleTuning.differentialRatio, 0f, 20f);
			float.TryParse(GUILayout.TextField(_vehicleTuning.differentialRatio.ToString("F2"), GUILayout.MaxWidth(200)), out _vehicleTuning.differentialRatio);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_vehicleTuning.differentialRatio = _vehicleTuning.defaultDifferentialRatio;
			GUILayout.EndVertical();

			GUILayout.EndScrollView();

			GUILayout.Space(10);

			GUILayout.BeginVertical("box");
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Apply", GUILayout.MaxWidth(200)))
			{
				SaveUtilities.UpdateVehicleTuning(new VehicleTuningData() { ID = save.idInSave, tuning = _vehicleTuning });
				GameUtilities.ApplyVehicleTuning(car, _vehicleTuning);
			}

			if (GUILayout.Button("Reset tuning to stock", GUILayout.MaxWidth(200)))
			{
				_vehicleTuning.steerAngle = _vehicleTuning.defaultSteerAngle;
				_vehicleTuning.brakePower = _vehicleTuning.defaultBrakePower;
				_vehicleTuning.differentialRatio = _vehicleTuning.defaultDifferentialRatio;
			}

			GUILayout.EndVertical();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}
}
