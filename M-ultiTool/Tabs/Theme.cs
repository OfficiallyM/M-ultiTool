using MultiTool.Core;
using System;
using UnityEngine;
using MultiTool.Modules;
using Logger = MultiTool.Modules.Logger;
using MultiTool.Utilities.UI;

namespace MultiTool.Tabs
{
	internal class ThemeTab : Tab
	{
		public override string Name => "Theme";
        public override bool ShowInNavigation => false;
        internal override bool IsFullScreen => true;

		private Vector2 _position;
		private Theme _theme;
		private bool _isEditing = false;

		public override void Update()
		{
			if (Styling.GetEditingTheme() != null)
			{
				_theme = Styling.GetEditingTheme();
				_isEditing = true;
			}
			else
			{
				_isEditing = false;

				if (_theme == null)
					_theme = new Theme();
			}
		}

		public override void RenderTab(Rect dimensions)
		{
            GUILayout.BeginArea(dimensions);
			GUILayout.Label(_isEditing ? $"Editing theme {_theme.Name}" : "Creating new theme", "LabelHeader");

			GUILayout.BeginHorizontal();
			_position = GUILayout.BeginScrollView(_position, GUILayout.MaxWidth(dimensions.width / 2));
			float colourWidth = (dimensions.width / 2) - 20f;
            GUILayout.BeginVertical();

			bool nameExists = false;
			if (!_isEditing)
				foreach (string name in Styling.GetThemeNames())
				if (name == _theme.Name)
					nameExists = true;

			GUILayout.Label("Theme name", "LabelSubHeader");
			_theme.Name = GUILayout.TextField(_theme.Name, GUILayout.MaxWidth(200));
			if (nameExists)
				GUILayout.Label("<color=#F00>Theme name already taken!</color>");
			GUILayout.Space(10);

			GUILayout.Label("Primary button colour", "LabelSubHeader");
			_theme.ButtonPrimaryColour = Colour.RenderColourSliders(colourWidth, _theme.ButtonPrimaryColour, true);
			GUILayout.Space(10);

			GUILayout.Label("Primary button mouse over colour", "LabelSubHeader");
			_theme.ButtonPrimaryHoverColour = Colour.RenderColourSliders(colourWidth, _theme.ButtonPrimaryHoverColour, true);
			GUILayout.Space(10);

			GUILayout.Label("Primary button text colour", "LabelSubHeader");
			_theme.ButtonPrimaryTextColour = Colour.RenderColourSliders(colourWidth, _theme.ButtonPrimaryTextColour, true);
			GUILayout.Space(10);

			GUILayout.Label("Secondary button colour", "LabelSubHeader");
			_theme.ButtonSecondaryColour = Colour.RenderColourSliders(colourWidth, _theme.ButtonSecondaryColour, true);
			GUILayout.Space(10);

			GUILayout.Label("Secondary button mouse over colour", "LabelSubHeader");
			_theme.ButtonSecondaryHoverColour = Colour.RenderColourSliders(colourWidth, _theme.ButtonSecondaryHoverColour, true);
			GUILayout.Space(10);

			GUILayout.Label("Secondary button text colour", "LabelSubHeader");
			_theme.ButtonSecondaryTextColour = Colour.RenderColourSliders(colourWidth, _theme.ButtonSecondaryTextColour, true);
			GUILayout.Space(10);

			GUILayout.Label("Menu background colour", "LabelSubHeader");
			_theme.BoxColour = Colour.RenderColourSliders(colourWidth, _theme.BoxColour, true);
			GUILayout.Space(10);

			GUILayout.Label("Menu background mouse over colour", "LabelSubHeader");
			GUILayout.Label("Used on main menu new game settings button");
			_theme.BoxHoverColour = Colour.RenderColourSliders(colourWidth, _theme.BoxHoverColour, true);
			GUILayout.Space(10);

			GUILayout.Label("Main text colour", "LabelSubHeader");
			_theme.TextColour = Colour.RenderColourSliders(colourWidth, _theme.TextColour, true);
			GUILayout.Space(10);

			GUILayout.Label("Accessibility ON colour", "LabelSubHeader");
			_theme.AccessibilityOnColour = Colour.RenderColourSliders(colourWidth, _theme.AccessibilityOnColour, true);
			GUILayout.Space(10);

			GUILayout.Label("Accessibility OFF colour", "LabelSubHeader");
			_theme.AccessibilityOffColour = Colour.RenderColourSliders(colourWidth, _theme.AccessibilityOffColour, true);
			GUILayout.Space(10);

			GUILayout.BeginHorizontal();
			if (_theme.Name != null)
			{
				if (GUILayout.Button("Save and exit", GUILayout.MaxWidth(200)))
				{
					if (nameExists)
					{
						Notifications.SendError("Theme creation", "Unable to save theme, name already taken.");
					}
					else
					{
						Styling.SetEditingTheme(_theme);
						Styling.SaveEditingTheme();
						Exit();
					}
				}
				GUILayout.Space(5);
			}

			if (GUILayout.Button("Exit without saving", "ButtonSecondary", GUILayout.MaxWidth(200)))
			{
				Exit();
			}

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
            GUILayout.EndScrollView();

			// Theme preview.
			if (_theme != null)
			{
				GUI.skin = Styling.CreatePreviewForTheme(_theme);

				GUILayout.BeginVertical("ButtonWhite");
				GUILayout.Label("<color=#000>Theme preview:</color>", "LabelHeader");
				GUILayout.BeginVertical("box");
				GUILayout.Button("Primary button", GUILayout.MaxWidth(200));
				GUILayout.Button("Secondary button", "ButtonSecondary", GUILayout.MaxWidth(200));
				GUILayout.Label("This is some text");
				GUILayout.EndVertical();
				GUILayout.EndVertical();

				GUI.skin = Styling.GetActiveSkin();
			}

			GUILayout.EndHorizontal();
			GUILayout.EndArea();
        }

		private void Exit()
		{
			_theme = null;
			GUIRenderer.Tabs.SetActive(MultiTool.Renderer.settingsTabId, false);
			Styling.SetEditingTheme(null);
			_position = Vector2.zero;
		}
	}
}
