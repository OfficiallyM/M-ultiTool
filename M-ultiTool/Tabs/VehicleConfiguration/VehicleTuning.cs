using MultiTool.Core;
using MultiTool.Extensions;
using MultiTool.Modules;
using MultiTool.Utilities;
using MultiTool.Utilities.UI;
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
		private Vector2 _footerPosition;
		private Core.VehicleTuning _vehicleTuning = null;
		private Core.VehicleTuning _defaultTuning = null;
		private Core.VehicleTuning _lastSavedTuning = null;
		private bool _sharingOpen = false;
		private string _export;
		private string _import;
		private TuningSave _saved;

		public override void OnVehicleChange()
		{
			_vehicleTuning = null;
			_lastSavedTuning = null;
			_export = null;
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

				// Save has no data for this vehicle, load defaults.
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

			GUILayout.BeginVertical("box", _sharingOpen ? GUILayout.MinHeight(dimensions.height / 1.25f) : GUILayout.MinHeight(20));
			if (_sharingOpen)
			{
				_footerPosition = GUILayout.BeginScrollView(_footerPosition);
				GUILayout.BeginVertical(GUILayout.MinHeight(dimensions.height / 2f), GUILayout.MaxHeight(dimensions.height - 20f));
				GUILayout.Label("Exporting", "LabelSubHeader");
				if (GUILayout.Button("Export current tuning", GUILayout.MaxWidth(200)))
					_export = new TuningSave()
					{
						part = car.name,
						type = "vehicle",
						car = car.name,
						tuning = _vehicleTuning,
					}
					.ToExportString();
				if (!string.IsNullOrEmpty(_export))
				{
					GUILayout.Label("Exported tuning:");
					GUILayout.Label("Copy and paste the below to someone to share the vehicle tuning with them.");
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
					if (_saved.type != "vehicle")
					{
						Notifications.SendError("Import failed", "Not a valid vehicle tune.");
						_saved = null;
					}
					else if (_saved.part != car.name)
					{
						GUILayout.Label("This tune is not designed for this vehicle, import anyway?");
						if (GUILayout.Button("Import anyway", GUILayout.MaxWidth(200)))
						{
							_vehicleTuning = _saved.tuning as Core.VehicleTuning;
							_saved = null;
							_import = null;
							Notifications.SendSuccess("Vehicle tuning", "Tuning imported");
						}
					}
					else
					{
						_vehicleTuning = _saved.tuning as Core.VehicleTuning;
						_saved = null;
						_import = null;
						Notifications.SendSuccess("Vehicle tuning", "Tuning imported");
					}
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
				GUILayout.EndScrollView();
			}
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Apply", GUILayout.MaxWidth(200)))
			{
				SaveUtilities.UpdateVehicleTuning(new VehicleTuningData() { ID = save.idInSave, tuning = _vehicleTuning, defaultTuning = _defaultTuning });
				GameUtilities.ApplyVehicleTuning(car, _vehicleTuning);
				_lastSavedTuning = _vehicleTuning.DeepCopy();
			}

			if (!ObjectExtensions.AreDataMembersEqual(_vehicleTuning, _lastSavedTuning))
			{
				GUILayout.Label("Unapplied changes detected!", GUILayout.ExpandWidth(false));
				GUILayout.Space(2);
			}

			if (GUILayout.Button("Reset tuning to stock", GUILayout.MaxWidth(200)))
			{
				_vehicleTuning.steerAngle = _defaultTuning.steerAngle;
				_vehicleTuning.brakePower = _defaultTuning.brakePower;
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
				_lastSavedTuning = _vehicleTuning.DeepCopy();
		}
	}
}
