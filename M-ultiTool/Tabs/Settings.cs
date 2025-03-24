using MultiTool.Core;
using System;
using UnityEngine;
using MultiTool.Modules;
using Logger = MultiTool.Modules.Logger;
using MultiTool.Utilities.UI;

namespace MultiTool.Tabs
{
	internal class SettingsTab : Tab
	{
		public override string Name => "Settings";
        public override bool ShowInNavigation => false;
        internal override bool IsFullScreen => true;

		private Vector2 currentPosition;
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
            currentPosition = GUILayout.BeginScrollView(currentPosition);
            GUILayout.BeginVertical(GUILayout.MaxWidth(settingsWidth * 0.85f));

            GUILayout.Space(20);

			GUILayout.Label("Theme select");
			foreach (string theme in Styling.GetThemeNames())
			{
				if (GUILayout.Button(Accessibility.GetAccessibleString(theme, theme == Styling.GetActiveTheme().Name), GUILayout.MaxWidth(200)))
					Styling.SetActiveTheme(theme);
				GUILayout.Space(2);
			}

			GUILayout.Space(10);

			GUILayout.Label($"Scroll bar width: {GUIRenderer.settingsScrollWidth.ToString()}", GUIRenderer.labelStyle);
            float tempScrollWidth = GUILayout.HorizontalSlider(GUIRenderer.settingsScrollWidth, 5f, 30f);
            GUIRenderer.settingsScrollWidth = Mathf.Round(tempScrollWidth);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply", GUILayout.MaxWidth(200)))
            {
                GUIRenderer.scrollWidth = GUIRenderer.settingsScrollWidth;
                MultiTool.Configuration.UpdateScrollWidth(GUIRenderer.scrollWidth);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Reset", "ButtonSecondary", GUILayout.MaxWidth(200)))
            {
                GUIRenderer.scrollWidth = 10f;
                GUIRenderer.settingsScrollWidth = GUIRenderer.scrollWidth;
                MultiTool.Configuration.UpdateScrollWidth(GUIRenderer.scrollWidth);
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Noclip speed increase factor:", GUIRenderer.labelStyle);
            float factor = GUILayout.HorizontalSlider(GUIRenderer.noclipFastMoveFactor, 2f, 100f);
            GUIRenderer.noclipFastMoveFactor = Mathf.Round(factor);
            MultiTool.Configuration.UpdateNoclipFastMoveFactor(GUIRenderer.noclipFastMoveFactor);
            GUILayout.Label(GUIRenderer.noclipFastMoveFactor.ToString());

            if (GUILayout.Button("Accessibility mode", GUILayout.MaxWidth(200)))
            {
                GUIRenderer.accessibilityShow = !GUIRenderer.accessibilityShow;
            }
            if (GUIRenderer.accessibilityShow)
            {
                for (int i = 0; i <= Accessibility.GetAccessibilityModeCount(); i++)
                {
                    if (GUILayout.Button(Accessibility.GetAccessibleString(Accessibility.GetAccessibilityModeName(i), (int)Accessibility.GetAccessibilityMode() == i), GUILayout.MaxWidth(200)))
                    {
                        Accessibility.SetAccessibilityMode(i);
                        MultiTool.Configuration.UpdateAccessibilityMode(i);
                    }
                }
            }

            GUILayout.Label("Accessibility mode affects color slider labels:", GUIRenderer.labelStyle);
            bool doesAffectColors = Accessibility.GetDoesAffectColors();

            if (GUILayout.Button(Accessibility.GetAccessibleString("On", "Off", doesAffectColors), GUILayout.MaxWidth(200)))
            {
                doesAffectColors = !doesAffectColors;
                Accessibility.SetDoesAffectColors(doesAffectColors);
                MultiTool.Configuration.UpdateAccessibilityModeAffectsColor(doesAffectColors);
            }

            GUILayout.Label("Basic collider colour", GUIRenderer.labelStyle);

            Color basicCollider = MultiTool.Configuration.GetColliderColour("basic");

            basicCollider = Colour.RenderColourSliders(settingsWidth / 2, basicCollider, true);
            MultiTool.Configuration.UpdateColliderColour(basicCollider, "basic");

            if (GUILayout.Button("Reset to default", GUILayout.MaxWidth(200)))
            {
                basicCollider = new Color(1f, 0.0f, 0.0f, 0.8f);
                MultiTool.Configuration.UpdateColliderColour(basicCollider, "basic");
            }

            GUILayout.Label("Trigger collider colour", GUIRenderer.labelStyle);

            Color triggerCollider = MultiTool.Configuration.GetColliderColour("trigger");

            triggerCollider = Colour.RenderColourSliders(settingsWidth / 2, triggerCollider, true);
            MultiTool.Configuration.UpdateColliderColour(triggerCollider, "trigger");

            if (GUILayout.Button("Reset to default", GUILayout.MaxWidth(200)))
            {
                triggerCollider = new Color(0.0f, 1f, 0.0f, 0.8f);
                MultiTool.Configuration.UpdateColliderColour(triggerCollider, "trigger");
            }

            GUILayout.Label("Interior collider colour", GUIRenderer.labelStyle);

            Color interiorCollider = MultiTool.Configuration.GetColliderColour("interior");

            interiorCollider = Colour.RenderColourSliders(settingsWidth / 2, interiorCollider, true);
            MultiTool.Configuration.UpdateColliderColour(interiorCollider, "interior");

            if (GUILayout.Button("Reset to default", GUILayout.MaxWidth(200)))
            {
                interiorCollider = new Color(0f, 0f, 1f, 0.8f);
                MultiTool.Configuration.UpdateColliderColour(interiorCollider, "interior");
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
	}
}
