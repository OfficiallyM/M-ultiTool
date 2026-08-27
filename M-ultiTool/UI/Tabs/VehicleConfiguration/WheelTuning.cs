using MultiTool.Extensions;
using MultiTool.Save;
using MultiTool.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiTool.UI.Tabs.VehicleConfiguration
{
	internal sealed class WheelTuningTab : UI.VehicleConfigurationTab
	{
		public override string Name => "Wheel Tuning";
		public override bool HasCache => true;

		private Vector2 _position;
		private Vector2 _footerPosition;
		private WheelTuning _tuning = null;
		private WheelTuning _defaultTuning = null;
		private WheelTuning _lastSavedTuning = null;
		private bool _sharingOpen = false;
		private string _export;
		private string _import;
		private TuningSave _saved;

		public override void OnVehicleChange()
		{
			carscript car = mainscript.M.player.Car;
			_tuning = null;
			LoadData(car);
			_lastSavedTuning = null;
			_export = null;
		}

		public override void OnCacheRefresh()
		{
			if (mainscript.M.player == null || mainscript.M.player.Car == null) return;

			carscript car = mainscript.M.player.Car;

			if (_tuning == null)
				LoadData(car);

			// Check for tire mounting if required.
			foreach (Wheel wheel in _tuning.Wheels)
			{
				if (wheel.ForwardSlip == null || wheel.SideSlip == null)
				{
					wheelgraphicsscript wheelgraphic = wheel.Save.GetComponentInChildren<wheelgraphicsscript>();
					gumiscript tire = wheelgraphic?.slot?.part?.p?.wheel?.gumi?.part?.p?.gumi;
					wheel.ForwardSlip = tire?.slip1;
					wheel.SideSlip = tire?.slip2;
				}
			}
		}

		private void LoadData(carscript car)
		{
			tosaveitemscript save = car.GetComponent<tosaveitemscript>();

			// Populate default tuning values if missing.
			if (_tuning == null || _defaultTuning == null)
			{
				// Attempt to load data from save.
				_tuning = SaveUtilities.GetWheelTuning(save);
				_defaultTuning = SaveUtilities.GetDefaultWheelTuning(save);

				// Reset any invalid tuning data.
				if (_tuning != null && _tuning.Wheels[0].Slot == null)
					_tuning = null;

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
							Save = wheelSave,
							Graphics = wheelgraphic,
							Slot = wheelgraphic.name,
							ForwardSlip = tire?.slip1,
							SideSlip = tire?.slip2,
							WheelDamping = collider.wheelDampingRate,
							Distance = collider.suspensionDistance,
							Stiffness = collider.suspensionSpring.spring,
							Damper = collider.suspensionSpring.damper,
							TargetPosition = collider.suspensionSpring.targetPosition,
							Position = collider.transform.localPosition,
						});

						defaultWheels.Add(new Wheel()
						{
							Slot = wheelgraphic.name,
							ForwardSlip = tire?.slip1,
							SideSlip = tire?.slip2,
							WheelDamping = collider.wheelDampingRate,
							Distance = collider.suspensionDistance,
							Stiffness = collider.suspensionSpring.spring,
							Damper = collider.suspensionSpring.damper,
							TargetPosition = collider.suspensionSpring.targetPosition,
							Position = collider.transform.localPosition,
						});
					}

					_tuning = new WheelTuning()
					{
						Wheels = wheels,
					};

					_defaultTuning = new WheelTuning()
					{
						Wheels = defaultWheels,
					};
				}

				// Reorder wheel list by slot name.
				_tuning.Wheels = _tuning.Wheels.OrderBy(w => w.Slot).ToList();
				_defaultTuning.Wheels = _defaultTuning.Wheels.OrderBy(w => w.Slot).ToList();
			}
		}

		public override void RenderTab(Rect dimensions)
		{
			carscript car = mainscript.M.player.Car;
			tosaveitemscript save = car.GetComponent<tosaveitemscript>();
			LoadData(car);

			GUILayout.BeginArea(dimensions);
			_position = GUILayout.BeginScrollView(_position);
			GUILayout.BeginVertical();

			if (GUILayout.Button(Accessibility.GetAccessibleString("Apply to all wheels", _tuning.ApplyToAll), GUILayout.MaxWidth(200)))
				_tuning.ApplyToAll = !_tuning.ApplyToAll;
			GUILayout.Space(10);

			if (_tuning.ApplyToAll)
			{
				GUILayout.BeginVertical("box");
				GUILayout.Label($"All wheels", "LabelHeader");
				GUILayout.Space(5);

				if (_tuning.Wheels.Count == 0)
				{
					GUILayout.Label("Vehicle has no wheels");
				}
				else
				{
					// Just grab the settings for the first wheel.
					Wheel wheel = _tuning.Wheels[0];

					RenderWheelSliders(wheel, _defaultTuning.Wheels[0]);

					// Update all other wheels.
					foreach (Wheel updateWheel in _tuning.Wheels)
					{
						if (updateWheel == wheel) continue;

						updateWheel.ForwardSlip = wheel.ForwardSlip;
						updateWheel.SideSlip = wheel.SideSlip;
						updateWheel.WheelDamping = wheel.WheelDamping;
						updateWheel.Distance = wheel.Distance;
						updateWheel.Stiffness = wheel.Stiffness;
						updateWheel.Damper = wheel.Damper;
						updateWheel.TargetPosition = wheel.TargetPosition;
					}
				}
				GUILayout.Space(5);
				GUILayout.EndVertical();
			}
			else
			{
				int index = 0;
				foreach (Wheel wheel in _tuning.Wheels)
				{
					GUILayout.BeginVertical("box");
					GUILayout.Label($"Wheel {wheel.Slot}", "LabelHeader");
					GUILayout.Space(5);

					RenderWheelSliders(wheel, _defaultTuning.Wheels[index], true);
					GUILayout.Space(5);
					GUILayout.EndVertical();

					GUILayout.Space(20);
					index++;
				}
			}
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
						Type = "wheel",
						Car = car.name,
						Tuning = _tuning,
					}
					.ToExportString();
				if (!string.IsNullOrEmpty(_export))
				{
					GUILayout.Label("Exported tuning:");
					GUILayout.Label("Copy and paste the below to someone to share the wheel tuning with them.");
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
					if (_saved.Type != "wheel")
					{
						Notifications.SendError("Import failed", "Not a valid wheel tune.");
						_saved = null;
					}
					else if (_saved.Part != car.name)
					{
						GUILayout.Label("This tune is not designed for this vehicle, import anyway?");
						if (GUILayout.Button("Import anyway", GUILayout.MaxWidth(200)))
						{
							_tuning = _saved.Tuning as WheelTuning;
							GameUtilities.RemapWheelTuning(save, _tuning);
							_saved = null;
							_import = null;
							Notifications.SendSuccess("Wheel tuning", "Tuning imported");
						}
					}
					else
					{
						_tuning = _saved.Tuning as WheelTuning;
						GameUtilities.RemapWheelTuning(save, _tuning);
						_saved = null;
						_import = null;
						Notifications.SendSuccess("Wheel tuning", "Tuning imported");
					}
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
				GUILayout.EndScrollView();
			}
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Apply", GUILayout.MaxWidth(200)))
			{
				SaveUtilities.UpdateWheelTuning(new WheelTuningData() { ID = save.idInSave, Tuning = _tuning, DefaultTuning = _defaultTuning });
				GameUtilities.ApplyWheelTuning(_tuning);
				_lastSavedTuning = _tuning.DeepCopy();
			}

			if (!ObjectExtensions.AreDataMembersEqual(_tuning, _lastSavedTuning))
			{
				GUILayout.Label("Unapplied changes detected!", GUILayout.ExpandWidth(false));
				GUILayout.Space(2);
			}

			if (GUILayout.Button("Reset tuning to stock", GUILayout.MaxWidth(200)))
			{
				for (int i = 0; i < _tuning.Wheels.Count; i++)
				{
					Wheel wheel = _tuning.Wheels[i];
					Wheel defaultWheel = _defaultTuning.Wheels[i];

					wheel.ForwardSlip = defaultWheel.ForwardSlip;
					wheel.SideSlip = defaultWheel.SideSlip;
					wheel.WheelDamping = defaultWheel.WheelDamping;
					wheel.Distance = defaultWheel.Distance;
					wheel.Stiffness = defaultWheel.Stiffness;
					wheel.Damper = defaultWheel.Damper;
					wheel.TargetPosition = defaultWheel.TargetPosition;
					wheel.Position = defaultWheel.Position;
					wheel.OutwardOffset = 0;
					wheel.ForwardOffset = 0;
					wheel.VerticalOffset = 0;
				}
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
				_lastSavedTuning = _tuning.DeepCopy();
		}

		private void RenderWheelSliders(Wheel wheel, Wheel defaultWheel, bool perWheel = false)
		{
			if (wheel.ForwardSlip != null && wheel.SideSlip != null)
			{
				GUILayout.Label("Grip", "LabelSubHeader");
				GUILayout.BeginVertical();
				GUILayout.Label("Forward slip");
				wheel.ForwardSlip = GUILayout.HorizontalSlider(wheel.ForwardSlip.Value, 0f, 10f);
				float.TryParse(GUILayout.TextField(wheel.ForwardSlip.Value.ToString("F2"), GUILayout.MaxWidth(200)), out float forwardSlip);
				wheel.ForwardSlip = forwardSlip;
				if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
					wheel.ForwardSlip = defaultWheel.ForwardSlip;
				GUILayout.EndVertical();
				GUILayout.Space(5);

				GUILayout.BeginVertical();
				GUILayout.Label("Side slip");
				wheel.SideSlip = GUILayout.HorizontalSlider(wheel.SideSlip.Value, 0f, 10f);
				float.TryParse(GUILayout.TextField(wheel.SideSlip.Value.ToString("F2"), GUILayout.MaxWidth(200)), out float sideSlip);
				wheel.SideSlip = sideSlip;
				if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
					wheel.SideSlip = defaultWheel.SideSlip;
				GUILayout.EndVertical();
				GUILayout.Space(5);
			}
			else
				GUILayout.Label("Wheel has no tire.");

			GUILayout.BeginVertical();
			GUILayout.Label("Wheel damping rate");
			GUILayout.Label("Wheel slow down rate");
			GUILayout.Label("High values = wheel slows down more quickly");
			wheel.WheelDamping = GUILayout.HorizontalSlider(wheel.WheelDamping * 100, 0, 100f) / 100;
			float.TryParse(GUILayout.TextField((wheel.WheelDamping * 100).ToString("F2"), GUILayout.MaxWidth(200)), out float wheelDamping);
			wheel.WheelDamping = wheelDamping / 100;
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				wheel.WheelDamping = defaultWheel.WheelDamping;
			GUILayout.EndVertical();
			GUILayout.Space(5);

			GUILayout.Label("Suspension", "LabelSubHeader");
			GUILayout.BeginVertical();
			GUILayout.Label("Spring distance (Ride height)");
			wheel.Distance = GUILayout.HorizontalSlider(wheel.Distance, 0, 5f);
			float.TryParse(GUILayout.TextField(wheel.Distance.ToString("F2"), GUILayout.MaxWidth(200)), out float distance);
			wheel.Distance = distance;
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				wheel.Distance = defaultWheel.Distance;
			GUILayout.EndVertical();
			GUILayout.Space(5);

			GUILayout.BeginVertical();
			GUILayout.Label("Spring stiffness");
			wheel.Stiffness = GUILayout.HorizontalSlider(wheel.Stiffness / 1000, 0f, 100f) * 1000;
			float.TryParse(GUILayout.TextField((wheel.Stiffness / 1000).ToString("F2"), GUILayout.MaxWidth(200)), out float stiffness);
			wheel.Stiffness = stiffness * 1000;
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				wheel.Stiffness = defaultWheel.Stiffness;
			GUILayout.EndVertical();
			GUILayout.Space(5);

			GUILayout.BeginVertical();
			GUILayout.Label("Spring damper (Shock absorber strength)");
			wheel.Damper = GUILayout.HorizontalSlider(wheel.Damper / 100, 0f, 100f) * 100;
			float.TryParse(GUILayout.TextField((wheel.Damper / 100).ToString("F2"), GUILayout.MaxWidth(200)), out float damper);
			wheel.Damper = damper * 100;
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				wheel.Damper = defaultWheel.Damper;
			GUILayout.EndVertical();
			GUILayout.Space(5);

			GUILayout.BeginVertical();
			GUILayout.Label("Target suspension position");
			GUILayout.Label("0 = fully extended suspension, 100 = fully compressed suspension");
			wheel.TargetPosition = GUILayout.HorizontalSlider(wheel.TargetPosition * 100, 0f, 100f) / 100;
			float.TryParse(GUILayout.TextField((wheel.TargetPosition * 100).ToString("F2"), GUILayout.MaxWidth(200)), out float targetPosition);
			wheel.TargetPosition = targetPosition / 100;
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				wheel.TargetPosition = defaultWheel.TargetPosition;
			GUILayout.EndVertical();
			GUILayout.Space(5);

			GUILayout.Label("Wheel position", "LabelSubHeader");
			string wheelPlural = perWheel ? "wheel" : "wheels";
			GUILayout.BeginVertical();

			GUILayout.Label($"Widen/thin {wheelPlural}");
			wheel.OutwardOffset = GUILayout.HorizontalSlider(wheel.OutwardOffset, -10f, 10f);
			float.TryParse(GUILayout.TextField(wheel.OutwardOffset.ToString("F3"), GUILayout.MaxWidth(200)), out wheel.OutwardOffset);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				wheel.OutwardOffset = 0;
			GUILayout.EndVertical();
			GUILayout.Space(5);

			GUILayout.BeginVertical();
			GUILayout.Label($"Lengthen/shorten {wheelPlural}");
			wheel.ForwardOffset = GUILayout.HorizontalSlider(wheel.ForwardOffset, -10f, 10f);
			float.TryParse(GUILayout.TextField(wheel.ForwardOffset.ToString("F3"), GUILayout.MaxWidth(200)), out wheel.ForwardOffset);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				wheel.ForwardOffset = 0;
			GUILayout.EndVertical();

			GUILayout.BeginVertical();
			GUILayout.Label($"Raise/lower {wheelPlural}");
			wheel.VerticalOffset = GUILayout.HorizontalSlider(wheel.VerticalOffset, -10f, 10f);
			float.TryParse(GUILayout.TextField(wheel.VerticalOffset.ToString("F3"), GUILayout.MaxWidth(200)), out wheel.VerticalOffset);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(200)))
				wheel.VerticalOffset = 0;
			GUILayout.EndVertical();

			// Apply wheel positioning offsets.
			if (perWheel)
			{
				wheel.Position = defaultWheel.Position;
				wheel.Position.x += wheel.OutwardOffset * (IsRightSide(wheel) ? 1f : -1f);
				wheel.Position.y += wheel.VerticalOffset;
				wheel.Position.z += wheel.ForwardOffset * (IsFront(wheel) ? 1f : -1f);
			}
			else
			{
				int index = 0;
				foreach (Wheel updateWheel in _tuning.Wheels)
				{
					updateWheel.Position = _defaultTuning.Wheels[index].Position;
					updateWheel.Position.x += wheel.OutwardOffset * (IsRightSide(updateWheel) ? 1f : -1f);
					updateWheel.Position.y += wheel.VerticalOffset;
					updateWheel.Position.z += wheel.ForwardOffset * (IsFront(updateWheel) ? 1f : -1f);
					index++;
				}
			}
		}

		private bool IsRightSide(Wheel wheel)
		{
			return wheel.Graphics?.W?.transform.localPosition.x > 0;
		}

		private bool IsFront(Wheel wheel)
		{
			return wheel.Graphics?.W?.transform.localPosition.z > 0;
		}
	}
}
