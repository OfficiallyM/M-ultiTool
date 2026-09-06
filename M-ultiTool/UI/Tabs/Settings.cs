using MultiTool.Services;
using System;
using UnityEngine;
using static MultiTool.Services.Keybinds;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.UI.Tabs
{
	internal class SettingsTab : Tab
	{
		public override string Name => "Settings";
		public override bool ShowInNavigation => false;
		internal override bool IsFullScreen => true;

		private Vector2 _position;
		private string _themeImport;
		private string _themeExport;
		private float _scrollWidth;
		private float _noclipSpeedFactor;
		private bool _accessibilityShow = false;

		public override void OnRegister()
		{
			_scrollWidth = Services.Configuration.Config.ScrollWidth;
			_noclipSpeedFactor = Services.Configuration.Config.NoclipFastMoveFactor;
		}

		public override void RenderTab(Rect dimensions)
		{
			// Render the keybind pane.
			try
			{
				MultiTool.Binds.RenderRebindMenu("Rebind keys", (int[])Enum.GetValues(typeof(Keybinds.Inputs)), dimensions.x + 10f, dimensions.y + 10f, dimensions.width * 0.25f, dimensions.height - 20f);
			}
			catch (Exception ex)
			{
				Logger.Log($"Error building settings rebind menu - {ex}", Logger.LogLevel.Error);
			}

			// Render settings pane.
			float settingsX = dimensions.x + (dimensions.width * 0.25f) + 20f;
			float settingsY = dimensions.y + 10f;
			float settingsWidth = dimensions.width * 0.75f - 30f;
			float settingsHeight = dimensions.height - 20f;
			GUILayout.BeginArea(new Rect(settingsX, settingsY, settingsWidth, settingsHeight), "<size=16><b>Settings</b></size>", "box");
			_position = GUILayout.BeginScrollView(_position);
			GUILayout.BeginVertical(GUILayout.MaxWidth(settingsWidth * 0.85f));

			GUILayout.Space(20);

			GUILayout.Label("Theme select");
			foreach (string themeName in Styling.GetThemeNames())
			{
				Theme theme = Styling.GetThemeByName(themeName);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(Accessibility.GetAccessibleString(themeName, themeName == Styling.GetActiveTheme().Name), GUILayout.MaxWidth(200)))
					Styling.SetActiveTheme(themeName);

				GUILayout.Space(5);

				if (!theme.IsCore)
				{
					if (GUILayout.Button("Edit theme", GUILayout.MaxWidth(200)))
					{
						Styling.SetEditingTheme(theme);
						GUIRenderer.Tabs.SetActive(MultiTool.Renderer.ThemeTabId, false);
					}
					GUILayout.Space(5);

					if (GUILayout.Button("Export theme", GUILayout.MaxWidth(200)))
					{
						_themeExport = Styling.Export(theme);
					}
					GUILayout.Space(5);

					if (GUILayout.Button("Delete theme", "ButtonSecondary", GUILayout.MaxWidth(200)))
					{
						Styling.DeleteTheme(theme);
					}
				}

				GUILayout.EndHorizontal();
				GUILayout.Space(2);
			}

			GUILayout.Space(5);

			if (GUILayout.Button("Create new theme", "ButtonSecondary", GUILayout.MaxWidth(200)))
			{
				GUIRenderer.Tabs.SetActive(MultiTool.Renderer.ThemeTabId, false);
			}

			GUILayout.Space(10);

			if (_themeExport != null && _themeExport != string.Empty)
			{
				GUILayout.Label("Exported theme:");
				GUILayout.Label("Copy and paste the below to someone to share the theme with them.");
				GUILayout.Label("To import a theme, use the \"Theme import\" section below");
				GUILayout.TextArea(_themeExport);
				GUILayout.Space(10);
			}

			GUILayout.Label("Theme import:");
			GUILayout.Label("Paste an exported theme here");
			_themeImport = GUILayout.TextArea(_themeImport);
			if (GUILayout.Button("Import", GUILayout.MaxWidth(200)))
			{
				Styling.Import(_themeImport);
				_themeImport = null;
			}
			GUILayout.Space(10);

			GUILayout.Label($"Scroll bar width: {_scrollWidth.ToString()}");
			_scrollWidth = GUILayout.HorizontalSlider(_scrollWidth, 5f, 30f);

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Apply", GUILayout.MaxWidth(200)))
			{
				MultiTool.Configuration.Update(c => { c.ScrollWidth = _scrollWidth; });
			}

			GUILayout.Space(10);

			if (GUILayout.Button("Reset", "ButtonSecondary", GUILayout.MaxWidth(200)))
			{
				_scrollWidth = 10f;
				MultiTool.Configuration.Update(c => { c.ScrollWidth = _scrollWidth; });
			}
			GUILayout.EndHorizontal();

			GUILayout.Label("Noclip speed increase factor:");
			float factor = GUILayout.HorizontalSlider(_noclipSpeedFactor, 2f, 100f);
			factor = Mathf.Round(factor);
			if (factor != _noclipSpeedFactor)
			{
				_noclipSpeedFactor = factor;
				MultiTool.Configuration.Update(c => { c.NoclipFastMoveFactor = _noclipSpeedFactor; });
			}
			GUILayout.Label(_noclipSpeedFactor.ToString());

			if (GUILayout.Button("Accessibility mode", GUILayout.MaxWidth(200)))
				_accessibilityShow = !_accessibilityShow;

			if (_accessibilityShow)
			{
				for (int i = 0; i <= Accessibility.GetAccessibilityModeCount(); i++)
				{
					if (GUILayout.Button(Accessibility.GetAccessibleString(Accessibility.GetAccessibilityModeName(i), (int)Accessibility.GetAccessibilityMode() == i), GUILayout.MaxWidth(200)))
					{
						Accessibility.SetAccessibilityMode(i);
						MultiTool.Configuration.Update(c => { c.Accessibility = i; });
					}
				}
			}

			GUILayout.Label("Accessibility mode affects color slider labels:");
			bool doesAffectColors = Accessibility.GetDoesAffectColors();

			if (GUILayout.Button(Accessibility.GetAccessibleString("On", "Off", doesAffectColors), GUILayout.MaxWidth(200)))
			{
				doesAffectColors = !doesAffectColors;
				Accessibility.SetDoesAffectColors(doesAffectColors);
				MultiTool.Configuration.Update(c => { c.AccessibilityModeAffectsColor = doesAffectColors; });
			}

			GUILayout.Label("Basic collider colour");

			Color basicCollider = MultiTool.Configuration.Config.BasicColliderColor;

			basicCollider = Colour.RenderColourSliders(settingsWidth / 2, basicCollider, true);
			MultiTool.Configuration.Update(c => { c.BasicColliderColor = basicCollider; });

			if (GUILayout.Button("Reset to default", GUILayout.MaxWidth(200)))
			{
				basicCollider = new Color(1f, 0.0f, 0.0f, 0.8f);
				MultiTool.Configuration.Update(c => { c.BasicColliderColor = basicCollider; });
			}

			GUILayout.Label("Trigger collider colour");

			Color triggerCollider = MultiTool.Configuration.Config.TriggerColliderColor;

			triggerCollider = Colour.RenderColourSliders(settingsWidth / 2, triggerCollider, true);
			MultiTool.Configuration.Update(c => { c.TriggerColliderColor = triggerCollider; });

			if (GUILayout.Button("Reset to default", GUILayout.MaxWidth(200)))
			{
				triggerCollider = new Color(0.0f, 1f, 0.0f, 0.8f);
				MultiTool.Configuration.Update(c => { c.TriggerColliderColor = triggerCollider; });
			}

			GUILayout.Label("Interior collider colour");

			Color interiorCollider = MultiTool.Configuration.Config.InteriorColliderColor;

			interiorCollider = Colour.RenderColourSliders(settingsWidth / 2, interiorCollider, true);
			MultiTool.Configuration.Update(c => { c.InteriorColliderColor = interiorCollider; });

			if (GUILayout.Button("Reset to default", GUILayout.MaxWidth(200)))
			{
				interiorCollider = new Color(0f, 0f, 1f, 0.8f);
				MultiTool.Configuration.Update(c => { c.InteriorColliderColor = interiorCollider; });
			}
			GUILayout.Space(10);

			if (GUILayout.Button("Go to mod debug", GUILayout.MaxWidth(200)))
				GUIRenderer.Tabs.SetActive(MultiTool.Renderer.DebugTabId, false);

			GUILayout.EndVertical();
			GUILayout.EndScrollView();
			GUILayout.EndArea();
		}
	}
}
