using MultiTool.Core;
using MultiTool.Extensions;
using MultiTool.Utilities;
using MultiTool.Utilities.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = MultiTool.Modules.Logger;

namespace MultiTool.Tabs.VehicleConfiguration
{
    internal sealed class WheelTuning : Core.VehicleConfigurationTab
	{
        public override string Name => "Wheel Tuning";

		private Vector2 _position;
		private Core.WheelTuning _tuning = null;
		private Core.WheelTuning _defaultTuning = null;

		public override void OnVehicleChange()
		{
			_tuning = null;
		}

		public override void OnCacheRefresh()
		{
			if (_tuning == null) return;

			// Check for tire mounting if required.
			foreach (Wheel wheel in _tuning.wheels)
			{
				if (wheel.forwardSlip == null || wheel.sideSlip == null)
				{
					wheelgraphicsscript wheelgraphic = wheel.save.GetComponentInChildren<wheelgraphicsscript>();
					gumiscript tire = wheelgraphic?.slot?.part?.p?.wheel?.gumi?.part?.p?.gumi;
					wheel.forwardSlip = tire?.slip1;
					wheel.sideSlip = tire?.slip2;
				}
			}
		}

		public override void RenderTab(Rect dimensions)
		{
			carscript car = mainscript.M.player.Car;
			tosaveitemscript save = car.GetComponent<tosaveitemscript>();

			// Populate default tuning values if missing.
			if (_tuning == null || _defaultTuning == null)
			{
				// Attempt to load data from save.
				_tuning = SaveUtilities.GetWheelTuning(save);
				_defaultTuning = SaveUtilities.GetDefaultWheelTuning(save);

				// Save has no data for wheels, load defaults.
				if (_tuning == null || _defaultTuning == null)
				{
					List<Wheel> wheels = new List<Wheel>();
					List<Wheel> defaultWheels = new List<Wheel>();
					wheelgraphicsscript[] wheelGraphics = car.GetComponentsInChildren<wheelgraphicsscript>(true);

					foreach (wheelgraphicsscript wheelgraphic in wheelGraphics)
					{
						// Ignore non mounted wheels.
                        if (wheelgraphic?.slot?.part?.p?.wheel == null)
                        {
							continue;
                        }

						gumiscript tire = wheelgraphic?.slot?.part?.p?.wheel?.gumi?.part?.p?.gumi;
						WheelCollider collider = wheelgraphic.W;
						tosaveitemscript wheelSave = wheelgraphic.slot.part.tosaveitem;
						wheels.Add(new Wheel()
						{
							save = wheelSave,
							graphics = wheelgraphic,
							name = PrettifyWheelName(wheelSave.transform.parent.name),
							ID = wheelSave.idInSave,
							forwardSlip = tire?.slip1,
							sideSlip = tire?.slip2,
							wheelDamping = collider.wheelDampingRate,
							distance = collider.suspensionDistance,
							stiffness = collider.suspensionSpring.spring,
							damper = collider.suspensionSpring.damper,
							targetPosition = collider.suspensionSpring.targetPosition,
							position = collider.transform.localPosition,
						});

						defaultWheels.Add(new Wheel()
						{
							name = PrettifyWheelName(wheelSave.transform.parent.name),
							ID = wheelSave.idInSave,
							forwardSlip = tire?.slip1,
							sideSlip = tire?.slip2,
							wheelDamping = collider.wheelDampingRate,
							distance = collider.suspensionDistance,
							stiffness = collider.suspensionSpring.spring,
							damper = collider.suspensionSpring.damper,
							targetPosition = collider.suspensionSpring.targetPosition,
							position = collider.transform.localPosition,
						});
					}

					_tuning = new Core.WheelTuning()
					{
						wheels = wheels,
					};

					_defaultTuning = new Core.WheelTuning()
					{
						wheels = defaultWheels,
					};
				}

				// Reorder wheel list by name.
				_tuning.wheels = _tuning.wheels.OrderBy(w => w.name).ToList();
				_defaultTuning.wheels = _defaultTuning.wheels.OrderBy(w => w.name).ToList();
			}

			GUILayout.BeginArea(dimensions);
			_position = GUILayout.BeginScrollView(_position);
			GUILayout.BeginVertical();

			if (GUILayout.Button(Accessibility.GetAccessibleString("Apply to all wheels", _tuning.applyToAll), GUILayout.MaxWidth(200)))
				_tuning.applyToAll = !_tuning.applyToAll;
			GUILayout.Space(10);

			// TODO: Track if changed, alert user for unapplied changes. Do for all tuning tabs.

			if (_tuning.applyToAll)
			{
				GUILayout.BeginVertical("box");
				GUILayout.Label($"All wheels", "LabelHeader");
				GUILayout.Space(5);

				if (_tuning.wheels.Count == 0)
				{
					GUILayout.Label("Vehicle has no wheels");
				}
				else
				{
					// Just grab the settings for the first wheel.
					Wheel wheel = _tuning.wheels[0];

					RenderWheelSliders(wheel, _defaultTuning.wheels[0]);

					// Update all other wheels.
					foreach (Wheel updateWheel in _tuning.wheels)
					{
						if (updateWheel == wheel) continue;

						updateWheel.forwardSlip = wheel.forwardSlip;
						updateWheel.sideSlip = wheel.sideSlip;
						updateWheel.wheelDamping = wheel.wheelDamping;
						updateWheel.distance = wheel.distance;
						updateWheel.stiffness = wheel.stiffness;
						updateWheel.damper = wheel.damper;
						updateWheel.targetPosition = wheel.targetPosition;
					}
				}
				GUILayout.Space(5);
				GUILayout.EndVertical();
			}
			else
			{
				int index = 0;
				foreach (Wheel wheel in _tuning.wheels)
				{
					GUILayout.BeginVertical("box");
					GUILayout.Label($"Wheel {wheel.name}", "LabelHeader");
					GUILayout.Space(5);

					RenderWheelSliders(wheel, _defaultTuning.wheels[index] ,true);
					GUILayout.Space(5);
					GUILayout.EndVertical();

					GUILayout.Space(20);
					index++;
				}
			}
			GUILayout.EndVertical();
			GUILayout.EndScrollView();

			GUILayout.Space(10);

			GUILayout.BeginVertical("box");
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Apply", GUILayout.MaxWidth(200)))
			{
				SaveUtilities.UpdateWheelTuning(new WheelTuningData() { ID = save.idInSave, tuning = _tuning, defaultTuning = _defaultTuning });
				GameUtilities.ApplyWheelTuning(_tuning);
			}

			if (GUILayout.Button("Reset tuning to stock", GUILayout.MaxWidth(200)))
			{
				for (int i = 0; i < _tuning.wheels.Count; i++)
				{
					Wheel wheel = _tuning.wheels[i];
					Wheel defaultWheel = _defaultTuning.wheels[i];

					wheel.forwardSlip = defaultWheel.forwardSlip;
					wheel.sideSlip = defaultWheel.sideSlip;
					wheel.wheelDamping = defaultWheel.wheelDamping;
					wheel.distance = defaultWheel.distance;
					wheel.stiffness = defaultWheel.stiffness;
					wheel.damper = defaultWheel.damper;
					wheel.targetPosition = defaultWheel.targetPosition;
					wheel.position = defaultWheel.position;
					wheel.outwardOffset = 0;
					wheel.forwardOffset = 0;
					wheel.verticalOffset = 0;
				}
			}

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			GUILayout.EndArea();
		}

		private void RenderWheelSliders(Wheel wheel, Wheel defaultWheel, bool perWheel = false)
		{
			if (wheel.forwardSlip != null && wheel.sideSlip != null)
			{
				GUILayout.Label("Grip", "LabelSubHeader");
				GUILayout.BeginVertical();
				GUILayout.Label("Forward slip");
				wheel.forwardSlip = GUILayout.HorizontalSlider(wheel.forwardSlip.Value, 0f, 10f);
				float.TryParse(GUILayout.TextField(wheel.forwardSlip.Value.ToString("F2"), GUILayout.MaxWidth(200)), out float forwardSlip);
				wheel.forwardSlip = forwardSlip;
				if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
					wheel.forwardSlip = defaultWheel.forwardSlip;
				GUILayout.EndVertical();
				GUILayout.Space(5);

				GUILayout.BeginVertical();
				GUILayout.Label("Side slip");
				wheel.sideSlip = GUILayout.HorizontalSlider(wheel.sideSlip.Value, 0f, 10f);
				float.TryParse(GUILayout.TextField(wheel.sideSlip.Value.ToString("F2"), GUILayout.MaxWidth(200)), out float sideSlip);
				wheel.sideSlip = sideSlip;
				if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
					wheel.sideSlip = defaultWheel.sideSlip;
				GUILayout.EndVertical();
				GUILayout.Space(5);
			}
			else
				GUILayout.Label("Wheel has no tire.");

			GUILayout.BeginVertical();
			GUILayout.Label("Wheel damping rate");
			GUILayout.Label("Wheel slow down rate");
			GUILayout.Label("High values = wheel slows down more quickly");
			wheel.wheelDamping = GUILayout.HorizontalSlider(wheel.wheelDamping * 100, 0, 100f) / 100;
			float.TryParse(GUILayout.TextField((wheel.wheelDamping * 100).ToString("F2"), GUILayout.MaxWidth(200)), out float wheelDamping);
			wheel.wheelDamping = wheelDamping / 100;
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				wheel.wheelDamping = defaultWheel.wheelDamping;
			GUILayout.EndVertical();
			GUILayout.Space(5);

			GUILayout.Label("Suspension", "LabelSubHeader");
			GUILayout.BeginVertical();
			GUILayout.Label("Spring distance (Ride height)");
			wheel.distance = GUILayout.HorizontalSlider(wheel.distance, 0, 5f);
			float.TryParse(GUILayout.TextField(wheel.distance.ToString("F2"), GUILayout.MaxWidth(200)), out float distance);
			wheel.distance = distance;
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				wheel.distance = defaultWheel.distance;
			GUILayout.EndVertical();
			GUILayout.Space(5);

			GUILayout.BeginVertical();
			GUILayout.Label("Spring stiffness");
			wheel.stiffness = GUILayout.HorizontalSlider(wheel.stiffness / 1000, 0f, 100f) * 1000;
			float.TryParse(GUILayout.TextField((wheel.stiffness / 1000).ToString("F2"), GUILayout.MaxWidth(200)), out float stiffness);
			wheel.stiffness = stiffness * 1000;
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				wheel.stiffness = defaultWheel.stiffness;
			GUILayout.EndVertical();
			GUILayout.Space(5);

			GUILayout.BeginVertical();
			GUILayout.Label("Spring damper (Shock absorber strength)");
			wheel.damper = GUILayout.HorizontalSlider(wheel.damper / 100, 0f, 100f) * 100;
			float.TryParse(GUILayout.TextField((wheel.damper / 100).ToString("F2"), GUILayout.MaxWidth(200)), out float damper);
			wheel.damper = damper * 100;
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				wheel.damper = defaultWheel.damper;
			GUILayout.EndVertical();
			GUILayout.Space(5);

			GUILayout.BeginVertical();
			GUILayout.Label("Target suspension position");
			GUILayout.Label("0 = fully extended suspension, 100 = fully compressed suspension");
			wheel.targetPosition = GUILayout.HorizontalSlider(wheel.targetPosition * 100, 0f, 100f) / 100;
			float.TryParse(GUILayout.TextField((wheel.targetPosition * 100).ToString("F2"), GUILayout.MaxWidth(200)), out float targetPosition);
			wheel.targetPosition = targetPosition / 100;
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				wheel.targetPosition = defaultWheel.targetPosition;
			GUILayout.EndVertical();
			GUILayout.Space(5);

			GUILayout.Label("Wheel position", "LabelSubHeader");
			string wheelPlural = perWheel ? "wheel" : "wheels";
			GUILayout.BeginVertical();

			GUILayout.Label($"Widen/thin {wheelPlural}");
			wheel.outwardOffset = GUILayout.HorizontalSlider(wheel.outwardOffset, -10f, 10f);
			float.TryParse(GUILayout.TextField(wheel.outwardOffset.ToString("F3"), GUILayout.MaxWidth(200)), out wheel.outwardOffset);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				wheel.outwardOffset = 0;
			GUILayout.EndVertical();
			GUILayout.Space(5);

			GUILayout.BeginVertical();
			GUILayout.Label($"Lengthen/shorten {wheelPlural}");
			wheel.forwardOffset = GUILayout.HorizontalSlider(wheel.forwardOffset, -10f, 10f);
			float.TryParse(GUILayout.TextField(wheel.forwardOffset.ToString("F3"), GUILayout.MaxWidth(200)), out wheel.forwardOffset);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				wheel.forwardOffset = 0;
			GUILayout.EndVertical();

			GUILayout.BeginVertical();
			GUILayout.Label($"Raise/lower {wheelPlural}");
			wheel.verticalOffset = GUILayout.HorizontalSlider(wheel.verticalOffset, -10f, 10f);
			float.TryParse(GUILayout.TextField(wheel.verticalOffset.ToString("F3"), GUILayout.MaxWidth(200)), out wheel.verticalOffset);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				wheel.verticalOffset = 0;
			GUILayout.EndVertical();

			// Apply wheel positioning offsets.
			if (perWheel)
			{
				wheel.position = defaultWheel.position;
				wheel.position.x += wheel.outwardOffset * (IsRightSide(wheel) ? 1f : -1f);
				wheel.position.y += wheel.verticalOffset;
				wheel.position.z += wheel.forwardOffset * (IsFront(wheel) ? 1f : -1f);
			}
			else
			{
				int index = 0;
				foreach (Wheel updateWheel in _tuning.wheels)
				{
					updateWheel.position = _defaultTuning.wheels[index].position;
					updateWheel.position.x += wheel.outwardOffset * (IsRightSide(updateWheel) ? 1f : -1f);
					updateWheel.position.y += wheel.verticalOffset;
					updateWheel.position.z += wheel.forwardOffset * (IsFront(updateWheel) ? 1f : -1f);
					index++;
				}
			}
		}

		private string PrettifyWheelName(string name)
		{
			if (name == null || name == string.Empty) return "Unknown";

			if (name[0] == 'T')
				name = name.Substring(1);

			return name;
		}

		private bool IsRightSide(Wheel wheel)
		{
			return wheel.graphics.W.transform.localPosition.x > 0;
		}

		private bool IsFront(Wheel wheel)
		{
			return wheel.graphics.W.transform.localPosition.z > 0;
		}
	}
}
 