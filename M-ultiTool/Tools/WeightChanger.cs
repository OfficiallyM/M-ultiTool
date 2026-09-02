using MultiTool.Save;
using MultiTool.Services;
using MultiTool.UI;
using System;
using UnityEngine;

namespace MultiTool.Tools
{
	internal class WeightChangerTool : Tool
	{
		public override string Name => "Weight Changer";
		public override bool UsesObjectSelection => true;
		public override bool UsesDefaultObjectSelectionUI => false;

		private float _weightValue = 0.1f;
		private float[] _weightOptions = new float[] { 100f, 10f, 1f, 0.1f, 0.01f };
		private bool _weightHold = true;

		public override void ControlRender()
		{
			string name = Name.ToLowerInvariant();
			if (GUILayout.Button(Accessibility.GetAccessibleString($"Toggle {name} mode", Tools.IsActive(Id)), GUILayout.MaxWidth(200)))
				Tools.Toggle(Id);
			GUILayout.Space(10);
		}

		public override void HudRender()
		{

			// Deliberately not using the default selection UI so we
			// can show the object weight data.
			GUILayout.BeginVertical();
			GUILayout.Space(Screen.height * 0.05f);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical();
			GUILayout.Button(
				$"Selected object: {(SelectedObject != null ? SelectedObject.name : "None")}\n{Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.action1)} to {(SelectedObject != null ? "deselect" : "select")}",
				GUILayout.MinHeight(50f)
			);
			if (SelectedObject != null)
			{
				GUILayout.Space(5);
				GUILayout.Button($"Base weight: {SelectedObject.GetComponent<massScript>()?.OwnMass().ToString("F2")} kg", GUILayout.MaxHeight(30));
				GUILayout.Button($"Total weight: {SelectedObject.GetComponent<massScript>()?.Mass().ToString("F2")} kg", GUILayout.MaxHeight(30));
			}
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			if (SelectedObject == null) return;

			float fullWidth = Screen.width * 0.2f;
			float halfWidth = fullWidth / 2;

			GUILayout.BeginVertical();
			GUILayout.FlexibleSpace();

			GUILayout.BeginVertical("box", GUILayout.Width(fullWidth));
			GUILayout.BeginHorizontal();
			GUILayout.Button("Weight up", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.up), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Button($"Weight down", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.down), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Button($"Change amount: {_weightValue} kg", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.action5), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Button($"Toggle hold to change ({(_weightHold ? "Hold" : "Click")})", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.select), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Button("Reset", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.action4), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
		}

		public override void Update()
		{
			if (SelectedObject == null) return;

			// Deselect and return early if we can't change the weight.
			if (SelectedObject.GetComponent<massScript>() == null)
			{
				SelectedObject = null;
				return;
			}

			// Weight value selection control.
			if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.action5).AssignedKey))
			{
				int currentIndex = Array.FindIndex(_weightOptions, s => s == _weightValue);
				if (currentIndex == -1 || currentIndex == _weightOptions.Length - 1)
					_weightValue = _weightOptions[0];
				else
					_weightValue = _weightOptions[currentIndex + 1];
			}

			if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.select).AssignedKey))
			{
				_weightHold = !_weightHold;
			}

			massScript mass = SelectedObject.GetComponent<massScript>();
			bool update = false;

			float currentMass = mass.OwnMass();

			// Mass increase.
			bool massUp = Input.GetKey(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.up).AssignedKey);
			if (!_weightHold)
				massUp = Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.up).AssignedKey);
			if (massUp)
			{
				mass.SetMass(currentMass + _weightValue);

				update = true;
			}

			// Mass decrease.
			bool massDown = Input.GetKey(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.down).AssignedKey);
			if (!_weightHold)
				massDown = Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.down).AssignedKey);
			if (massDown)
			{
				mass.SetMass(currentMass - _weightValue);

				update = true;
			}

			// Reset weight to default.
			if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.action4).AssignedKey))
			{
				WeightData weight = SaveUtilities.GetWeight(SelectedObject.idInSave);

				if (weight == null)
				{
					Notifications.SendWarning("Weight Changer", "Unable to reset - no default available");
					return;
				}
				else
				{
					mass.SetMass(weight.DefaultMass);
					update = true;
				}
			}

			if (update)
			{
				SaveUtilities.UpdateWeight(new WeightData()
				{
					ID = SelectedObject.idInSave,
					Mass = mass.OwnMass(),
					DefaultMass = currentMass,
				});
			}
		}
	}
}
