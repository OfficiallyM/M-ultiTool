using MultiTool.Save;
using MultiTool.Services;
using MultiTool.UI;
using System;
using UnityEngine;

namespace MultiTool.Tools
{
	internal class ScaleTool : Tool
	{
		public override string Name => "Object Scale";
		public override bool UsesObjectSelection => true;
		public override bool UsesDefaultObjectSelectionUI => true;

		private string _axis = "all";
		private string[] _axisOptions = new string[] { "all", "x", "y", "z" };
		private float _scaleValue = 0.1f;
		private float[] _scaleOptions = new float[] { 10f, 1f, 0.1f, 0.01f, 0.001f };
		private bool _scaleHold = true;

		public override void ControlRender()
		{
			string name = Name.ToLowerInvariant();
			if (GUILayout.Button(Accessibility.GetAccessibleString($"Toggle {name} mode", MultiTool.Tools.IsActive(Id)), GUILayout.MaxWidth(200)))
				MultiTool.Tools.Toggle(Id);
			GUILayout.Space(10);
		}

		public override void HudRender()
		{
			if (SelectedObject == null) return;

			float fullWidth = Screen.width * 0.2f;
			float halfWidth = fullWidth / 2;

			GUILayout.BeginVertical();
			GUILayout.FlexibleSpace();

			GUILayout.BeginVertical("box", GUILayout.Width(fullWidth));
			GUILayout.BeginHorizontal();
			GUILayout.Button("Scale up", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.up), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Button("Scale down", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.down), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Button($"Axis: {_axis}", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.action3), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Button($"Scale amount: {_scaleValue}", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.action5), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Button($"Toggle hold to scale ({(_scaleHold ? "Hold" : "Click")})", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.select), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Button("Reset", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.action4), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();

			Vector3 scale = SelectedObject.transform.localScale;
			string scaleDisplay = scale.ToString();
			switch (_axis)
			{
				case "x":
					scaleDisplay = scale.x.ToString();
					break;
				case "y":
					scaleDisplay = scale.y.ToString();
					break;
				case "z":
					scaleDisplay = scale.z.ToString();
					break;
			}
			GUILayout.Button($"Scale: {scaleDisplay}");
			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
		}

		public override void Update()
		{
			if (SelectedObject == null) return;

			bool update = false;

			Vector3 scale = SelectedObject.transform.localScale;

			// Scale up.
			bool scaleUp = Input.GetKey(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.up).AssignedKey);
			if (!_scaleHold)
				scaleUp = Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.up).AssignedKey);
			if (scaleUp)
			{
				switch (_axis)
				{
					case "all":
						SelectedObject.transform.localScale = new Vector3(scale.x + _scaleValue, scale.y + _scaleValue, scale.z + _scaleValue);
						break;
					case "x":
						scale.x += _scaleValue;
						SelectedObject.transform.localScale = scale;
						break;
					case "y":
						scale.y += _scaleValue;
						SelectedObject.transform.localScale = scale;
						break;
					case "z":
						scale.z += _scaleValue;
						SelectedObject.transform.localScale = scale;
						break;
				}
				update = true;
			}

			// Scale down.
			bool scaleDown = Input.GetKey(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.down).AssignedKey);
			if (!_scaleHold)
				scaleDown = Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.down).AssignedKey);
			if (scaleDown)
			{
				switch (_axis)
				{
					case "all":
						SelectedObject.transform.localScale = new Vector3(scale.x - _scaleValue, scale.y - _scaleValue, scale.z - _scaleValue);
						break;
					case "x":
						scale.x -= _scaleValue;
						SelectedObject.transform.localScale = scale;
						break;
					case "y":
						scale.y -= _scaleValue;
						SelectedObject.transform.localScale = scale;
						break;
					case "z":
						scale.z -= _scaleValue;
						SelectedObject.transform.localScale = scale;
						break;
				}
				update = true;
			}

			// Reset scale to default.
			if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.action4).AssignedKey))
			{
				// TODO: Store default scale after refactoring save system. For now, assume it's 1.
				switch (_axis)
				{
					case "all":
						SelectedObject.transform.localScale = new Vector3(1, 1, 1);
						break;
					case "x":
						scale.x = 1;
						SelectedObject.transform.localScale = scale;
						break;
					case "y":
						scale.y = 1;
						SelectedObject.transform.localScale = scale;
						break;
					case "z":
						scale.z = 1;
						SelectedObject.transform.localScale = scale;
						break;
				}
				update = true;
			}

			// Change axis.
			if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.action3).AssignedKey))
			{
				int currentIndex = Array.FindIndex(_axisOptions, a => a == _axis);
				if (currentIndex == -1 || currentIndex == _axisOptions.Length - 1)
					_axis = _axisOptions[0];
				else
					_axis = _axisOptions[currentIndex + 1];
			}

			// Change scale value.
			if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.action5).AssignedKey))
			{
				int currentIndex = Array.FindIndex(_scaleOptions, s => s == _scaleValue);
				if (currentIndex == -1 || currentIndex == _scaleOptions.Length - 1)
					_scaleValue = _scaleOptions[0];
				else
					_scaleValue = _scaleOptions[currentIndex + 1];
			}

			// Change scale hold.
			if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.select).AssignedKey))
			{
				_scaleHold = !_scaleHold;
			}

			// Update saved scale.
			if (update)
			{
				SaveUtilities.UpdateScale(new ScaleData()
				{
					ID = SelectedObject.idInSave,
					Scale = SelectedObject.transform.localScale
				});
			}
		}
	}
}
