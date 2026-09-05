using MultiTool.Services;
using MultiTool.UI;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.Tools
{
	internal class ObjectDebugTool : Tool
	{
		public override string Name => "Object debug";

		private Vector2 _scroll;
		private GameObject _selected;
		private string _selectedName;
		private bool _enableAdvancedDebug = false;
		private bool _showGameComponents = true;
		private bool _showUnityComponents = true;
		private bool _showChildren = false;

		private Dictionary<Component, (string assembly, string type)> _components = new Dictionary<Component, (string, string)>();

		public override void ControlRender()
		{
			string name = Name.ToLowerInvariant();
			if (GUILayout.Button(Accessibility.GetAccessibleString($"Toggle {name} mode", Tools.IsActive(Id)), GUILayout.MaxWidth(200)))
				Tools.Toggle(Id);
			GUILayout.Space(10);
		}

		public override void Update()
		{
			try
			{
				GameObject foundObject = null;
				// Find object the player is looking at.
				var obj = Raycast();

				tosaveitemscript save = obj?.GetComponentInParent<tosaveitemscript>();
				if (save != null)
					foundObject = save.gameObject;

				// Debug picked up if player is holding something.
				if (mainscript.M.player.pickedUp != null)
					foundObject = mainscript.M.player.pickedUp.gameObject;

				// Debug held item if something is equipped.
				if (mainscript.M.player.inHandP != null)
					foundObject = mainscript.M.player.inHandP.gameObject;

				if (foundObject != _selected)
				{
					_selected = foundObject;
					if (_selected != null)
					{
						_selectedName = _selected.name.Replace("(Clone)", string.Empty);
						GetComponents();
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Log($"Error determining debug object. Details: {ex}", Logger.LogLevel.Error);
			}

			if (_selected == null) return;

			bool newAdvancedDebug = Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.action1).AssignedKey);
			bool newShowGameComponents = Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.action3).AssignedKey);
			bool newShowUnityComponents = Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.action4).AssignedKey);
			bool newShowChildren = Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.action6).AssignedKey);
			bool scrollUp = Input.GetKey(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.up).AssignedKey);
			bool scrollDown = Input.GetKey(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.down).AssignedKey);

			if (newAdvancedDebug)
			{
				_enableAdvancedDebug = !_enableAdvancedDebug;
				GetComponents();
			}

			if (newShowGameComponents)
			{
				_showGameComponents = !_showGameComponents;
				GetComponents();
			}

			if (newShowUnityComponents)
			{
				_showUnityComponents = !_showUnityComponents;
				GetComponents();
			}

			if (newShowChildren)
			{
				_showChildren = !_showChildren;
				GetComponents();
			}

			if (scrollUp)
			{
				_scroll = _scroll -= new Vector2(4f, 4f);
				if (Mathf.Approximately(_scroll.sqrMagnitude, 0))
					_scroll = Vector2.zero;
			}

			if (scrollDown)
			{
				_scroll = _scroll += new Vector2(4f, 4f);
			}
		}

		public override void HudRender()
		{
			if (_selected == null) return;

			float fullWidth = Screen.width * 0.25f;
			float halfWidth = fullWidth / 2;

			// Left side settings UI.
			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();
			GUILayout.FlexibleSpace();

			GUILayout.BeginVertical("box", GUILayout.Width(fullWidth));
			GUILayout.BeginHorizontal();
			GUILayout.Button($"{(_enableAdvancedDebug ? "Disable" : "Enable")} advanced debug", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.action1), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Button($"{(_showGameComponents ? "Disable" : "Enable")} show game components", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.action3), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Button($"{(_showUnityComponents ? "Disable" : "Enable")} show Unity components", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.action4), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Button($"{(_showChildren ? "Disable" : "Enable")} show child object components", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.action6), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Button("Scroll display up", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.up), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Button("Scroll display down", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.down), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();

			// Right side display UI.
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical();
			GUILayout.FlexibleSpace();

			GUILayout.BeginVertical("box");
			_scroll = GUILayout.BeginScrollView(_scroll);
			GUILayout.Label(_selectedName, "LabelHeader");
			GUILayout.Space(10);
			// Basic object information.
			GUILayout.Label($"Save ID: {_selected.GetComponent<tosaveitemscript>()?.idInSave}");
			GUILayout.Label($"Local position: {_selected.transform.position}");
			GUILayout.Label($"Global position: {GameUtilities.GetGlobalObjectPosition(_selected.transform.position)}");
			GUILayout.Label($"Rotation (Euler angles): {_selected.transform.rotation.eulerAngles}");
			GUILayout.Label($"Rotation (Quaternion): {_selected.transform.rotation}");
			
			if (_enableAdvancedDebug)
			{
				GUILayout.Space(10);
				GUILayout.Label("Components", "LabelSubHeader");
				GUILayout.Label("Assembly - Class");
				GUILayout.Space(10);

				foreach (var componentData in _components)
				{
					var component = componentData.Key;
					var assembly = componentData.Value.assembly;
					var type = componentData.Value.type;

					if (!_showGameComponents && assembly == "Assembly-CSharp")
						continue;

					if (!_showUnityComponents && assembly.Contains("UnityEngine"))
						continue;

					GUILayout.Label($"{assembly} - {type}");
					if (component.transform.parent != null)
						GUILayout.Label("(Child of: " + component.transform.parent.name + ")");
					GUILayout.Space(2);
				}
			}

			GUILayout.EndScrollView();
			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
			GUILayout.Space(10);
			GUILayout.EndHorizontal();
		}

		private void GetComponents()
		{
			_components.Clear();
			if (_selected == null) return;

			var components = _showChildren
				? _selected.GetComponentsInChildren(typeof(Component))
				: _selected.GetComponents(typeof(Component));
			components = components.Distinct().ToArray();

			foreach (var component in components)
			{
				Type type = component.GetType();
				string assembly = type.Assembly.GetName().Name;

				_components.Add(component, (assembly, type.Name));
			}
		}
	}
}
