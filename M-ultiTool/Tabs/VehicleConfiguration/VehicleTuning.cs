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
		private Core.VehicleTuning _defaultTuning = null;

		public override void OnVehicleChange()
		{
			_vehicleTuning = null;
		}

		public override void RenderTab(Rect dimensions)
		{
			carscript car = mainscript.M.player.Car;
			tosaveitemscript save = car.GetComponent<tosaveitemscript>();

			// Populate default tuning values if missing.
			if (_vehicleTuning == null || _defaultTuning == null)
			{
				// Attempt to load data from save.
				_vehicleTuning = SaveUtilities.GetVehicleTuning(save.idInSave);
				_defaultTuning = SaveUtilities.GetDefaultVehicleTuning(save.idInSave);

				// Save has no data for this transmission, load defaults.
				if (_vehicleTuning == null || _defaultTuning == null)
				{
					_vehicleTuning = new Core.VehicleTuning()
					{
						steerAngle = car.steerAngle,
						brakePower = car.brakePower,
					};

					_defaultTuning = new Core.VehicleTuning()
					{
						steerAngle = car.steerAngle,
						brakePower = car.brakePower,
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
				_vehicleTuning.steerAngle = _defaultTuning.steerAngle;
			GUILayout.EndVertical();

			GUILayout.Space(10);

			GUILayout.Label("Braking", "LabelHeader");
			GUILayout.BeginVertical();
			GUILayout.Label("Brake power");
			_vehicleTuning.brakePower = GUILayout.HorizontalSlider(_vehicleTuning.brakePower, 0f, 10000f);
			float.TryParse(GUILayout.TextField(_vehicleTuning.brakePower.ToString("F2"), GUILayout.MaxWidth(200)), out _vehicleTuning.brakePower);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				_vehicleTuning.brakePower = _defaultTuning.brakePower;
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
				_vehicleTuning.steerAngle = _defaultTuning.steerAngle;
				_vehicleTuning.brakePower = _defaultTuning.brakePower;
			}

			GUILayout.EndVertical();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}
}
