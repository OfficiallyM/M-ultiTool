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
        private float settingsLastHeight = 0f;
		public override void RenderTab(Rect dimensions)
		{
            // Render the setting page.
            try
            {
                MultiTool.Binds.RenderRebindMenu("Rebind keys", (int[])Enum.GetValues(typeof(Keybinds.Inputs)), dimensions.x + 10f, dimensions.y + 10f, dimensions.width * 0.25f, dimensions.height - 20f);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error building settings rebind menu - {ex}", Logger.LogLevel.Error);
            }

            // Other settings.
            float settingsX = dimensions.x + (dimensions.width * 0.25f) + 20f;
            float settingsY = dimensions.y + 10f;
            float settingsWidth = dimensions.width * 0.75f - 30f;
            float settingsHeight = dimensions.height - 20f;
            GUI.Box(new Rect(settingsX, settingsY, settingsWidth, settingsHeight), "Settings");
            currentPosition = GUI.BeginScrollView(new Rect(settingsX, settingsY, settingsWidth, settingsHeight), currentPosition, new Rect(settingsX, settingsY, settingsWidth, settingsLastHeight), new GUIStyle(), GUI.skin.verticalScrollbar);

            settingsX += 10f;
            settingsY += 50f;
            float configHeight = 20f;

            float buttonWidth = 200f;

            settingsWidth -= 20f;

            GUI.Label(new Rect(settingsX, settingsY, settingsWidth, configHeight), "Scroll bar width:", GUIRenderer.labelStyle);
            settingsY += configHeight;
            float tempScrollWidth = GUI.HorizontalSlider(new Rect(settingsX, settingsY, settingsWidth, configHeight), GUIRenderer.settingsScrollWidth, 5f, 30f);
            GUIRenderer.settingsScrollWidth = Mathf.Round(tempScrollWidth);
            settingsY += configHeight;

            // GUI.VerticalScrollbar doesn't work properly so just use a button as the preview.
            GUI.Button(new Rect(settingsX, settingsY, GUIRenderer.settingsScrollWidth, configHeight), String.Empty);
            GUI.Label(new Rect(settingsX + GUIRenderer.settingsScrollWidth + 10f, settingsY, settingsWidth - GUIRenderer.settingsScrollWidth - 10f, configHeight), GUIRenderer.settingsScrollWidth.ToString());

            settingsY += configHeight;

            if (GUI.Button(new Rect(settingsX, settingsY, buttonWidth, configHeight), "Apply"))
            {
                GUIRenderer.scrollWidth = GUIRenderer.settingsScrollWidth;
                MultiTool.Configuration.UpdateScrollWidth(GUIRenderer.scrollWidth);
            }

            if (GUI.Button(new Rect(settingsX + buttonWidth + 10f, settingsY, buttonWidth, configHeight), "Reset"))
            {
                GUIRenderer.scrollWidth = 10f;
                GUIRenderer.settingsScrollWidth = GUIRenderer.scrollWidth;
                MultiTool.Configuration.UpdateScrollWidth(GUIRenderer.scrollWidth);
            }

            settingsY += configHeight + 10f;

            GUI.Label(new Rect(settingsX, settingsY, settingsWidth, configHeight), "Noclip speed increase factor:", GUIRenderer.labelStyle);
            settingsY += configHeight;
            float factor = GUI.HorizontalSlider(new Rect(settingsX, settingsY, settingsWidth, configHeight), GUIRenderer.noclipFastMoveFactor, 2f, 100f);
            GUIRenderer.noclipFastMoveFactor = Mathf.Round(factor);
            MultiTool.Configuration.UpdateNoclipFastMoveFactor(GUIRenderer.noclipFastMoveFactor);
            settingsY += configHeight;
            GUI.Label(new Rect(settingsX, settingsY, settingsWidth, configHeight), GUIRenderer.noclipFastMoveFactor.ToString());

            settingsY += configHeight + 10f;

            if (GUI.Button(new Rect(settingsX, settingsY, buttonWidth, configHeight), "Accessibility mode"))
            {
                GUIRenderer.accessibilityShow = !GUIRenderer.accessibilityShow;
            }
            if (GUIRenderer.accessibilityShow)
            {
                settingsY += configHeight;
                for (int i = 0; i <= Accessibility.GetAccessibilityModeCount(); i++)
                {
                    if (GUI.Button(new Rect(settingsX, settingsY, buttonWidth, configHeight), Accessibility.GetAccessibleString(Accessibility.GetAccessibilityModeName(i), (int)Accessibility.GetAccessibilityMode() == i)))
                    {
                        Accessibility.SetAccessibilityMode(i);
                        MultiTool.Configuration.UpdateAccessibilityMode(i);
                    }

                    settingsY += configHeight + 2f;
                }
            }

            settingsY += configHeight + 10f;
            GUI.Label(new Rect(settingsX, settingsY, settingsWidth, configHeight), "Accessibility mode affects color slider labels:", GUIRenderer.labelStyle);
            settingsY += configHeight;
            bool doesAffectColors = Accessibility.GetDoesAffectColors();

            if (GUI.Button(new Rect(settingsX, settingsY, buttonWidth, configHeight), Accessibility.GetAccessibleString("On", "Off", doesAffectColors)))
            {
                doesAffectColors = !doesAffectColors;
                Accessibility.SetDoesAffectColors(doesAffectColors);
                MultiTool.Configuration.UpdateAccessibilityModeAffectsColor(doesAffectColors);
            }

            GUIStyle defaultStyle = GUI.skin.button;
            GUIStyle previewStyle = new GUIStyle(defaultStyle);
            Texture2D previewTexture = new Texture2D(1, 1);
            Color[] pixels = null;

            settingsY += configHeight + 10f;
            GUI.Label(new Rect(settingsX, settingsY, settingsWidth, configHeight), "Basic collider colour", GUIRenderer.labelStyle);
            settingsY += configHeight + 10f;

            Color basicCollider = MultiTool.Configuration.GetColliderColour("basic");

            // Red.
            GUI.Label(new Rect(settingsX, settingsY, settingsWidth, configHeight), Accessibility.GetAccessibleColorString("Red:", new Color(255, 0, 0)), GUIRenderer.labelStyle);
            settingsY += configHeight;
            float basicColliderRed = GUI.HorizontalSlider(new Rect(settingsX, settingsY, settingsWidth, configHeight), basicCollider.r * 255, 0, 255);
            basicColliderRed = Mathf.Round(basicColliderRed);
            settingsY += configHeight;
            bool basicColliderRedParse = float.TryParse(GUI.TextField(new Rect(settingsX, settingsY, settingsWidth, configHeight), basicColliderRed.ToString(), GUIRenderer.labelStyle), out basicColliderRed);
            if (!basicColliderRedParse)
                Logger.Log($"{basicColliderRedParse} is not a number", Logger.LogLevel.Error);
            basicColliderRed = Mathf.Clamp(basicColliderRed, 0f, 255f);
            basicCollider.r = basicColliderRed / 255f;

            // Green.
            settingsY += configHeight + 10f;
            GUI.Label(new Rect(settingsX, settingsY, settingsWidth, configHeight), Accessibility.GetAccessibleColorString("Green:", new Color(0, 255, 0)), GUIRenderer.labelStyle);
            settingsY += configHeight;
            float basicColliderGreen = GUI.HorizontalSlider(new Rect(settingsX, settingsY, settingsWidth, configHeight), basicCollider.g * 255, 0, 255);
            basicColliderGreen = Mathf.Round(basicColliderGreen);
            settingsY += configHeight;
            bool basicColliderGreenParse = float.TryParse(GUI.TextField(new Rect(settingsX, settingsY, settingsWidth, configHeight), basicColliderGreen.ToString(), GUIRenderer.labelStyle), out basicColliderGreen);
            if (!basicColliderGreenParse)
                Logger.Log($"{basicColliderGreenParse} is not a number", Logger.LogLevel.Error);
            basicColliderGreen = Mathf.Clamp(basicColliderGreen, 0f, 255f);
            basicCollider.g = basicColliderGreen / 255f;

            // Blue.
            settingsY += configHeight + 10f;
            GUI.Label(new Rect(settingsX, settingsY, settingsWidth, configHeight), Accessibility.GetAccessibleColorString("Blue:", new Color(0, 0, 255)), GUIRenderer.labelStyle);
            settingsY += configHeight;
            float basicColliderBlue = GUI.HorizontalSlider(new Rect(settingsX, settingsY, settingsWidth, configHeight), basicCollider.b * 255, 0, 255);
            basicColliderBlue = Mathf.Round(basicColliderBlue);
            settingsY += configHeight;
            bool basicColliderBlueParse = float.TryParse(GUI.TextField(new Rect(settingsX, settingsY, settingsWidth, configHeight), basicColliderBlue.ToString(), GUIRenderer.labelStyle), out basicColliderBlue);
            if (!basicColliderBlueParse)
                Logger.Log($"{basicColliderBlueParse.ToString()} is not a number", Logger.LogLevel.Error);
            basicColliderBlue = Mathf.Clamp(basicColliderBlue, 0f, 255f);
            basicCollider.b = basicColliderBlue / 255f;

            // Alpha.
            settingsY += configHeight + 10f;
            GUI.Label(new Rect(settingsX, settingsY, settingsWidth, configHeight), "Alpha:", GUIRenderer.labelStyle);
            settingsY += configHeight;
            float basicColliderAlpha = GUI.HorizontalSlider(new Rect(settingsX, settingsY, settingsWidth, configHeight), basicCollider.a * 255, 0, 255);
            basicColliderAlpha = Mathf.Round(basicColliderAlpha);
            settingsY += configHeight;
            bool basicColliderAlphaParse = float.TryParse(GUI.TextField(new Rect(settingsX, settingsY, settingsWidth, configHeight), basicColliderAlpha.ToString(), GUIRenderer.labelStyle), out basicColliderAlpha);
            if (!basicColliderAlphaParse)
                Logger.Log($"{basicColliderAlphaParse.ToString()} is not a number", Logger.LogLevel.Error);
            basicColliderAlpha = Mathf.Clamp(basicColliderAlpha, 0f, 255f);
            basicCollider.a = basicColliderAlpha / 255f;

            settingsY += configHeight + 10f;

            // Colour preview.
            // Override alpha for colour preview.
            Color basicColliderPreview = basicCollider;
            basicColliderPreview.a = 1;
            pixels = new Color[] { basicColliderPreview };
            previewTexture.SetPixels(pixels);
            previewTexture.Apply();
            previewStyle.normal.background = previewTexture;
            previewStyle.active.background = previewTexture;
            previewStyle.hover.background = previewTexture;
            previewStyle.margin = new RectOffset(0, 0, 0, 0);
            GUI.skin.button = previewStyle;
            GUI.Button(new Rect(settingsX, settingsY, settingsWidth / 2, configHeight * 2), "");
            GUI.skin.button = defaultStyle;

            settingsY += configHeight * 2 + 10f;

            basicCollider = GUIRenderer.RenderColourPalette(settingsX, settingsY, settingsWidth / 2, basicCollider);
            settingsY += GUIRenderer.GetPaletteHeight(settingsWidth / 2) + 10f;
            MultiTool.Configuration.UpdateColliderColour(basicCollider, "basic");

            if (GUI.Button(new Rect(settingsX, settingsY, buttonWidth, configHeight), "Reset to default"))
            {
                basicCollider = new Color(1f, 0.0f, 0.0f, 0.8f);
                MultiTool.Configuration.UpdateColliderColour(basicCollider, "basic");
            }

            settingsY += configHeight + 10f;
            GUI.Label(new Rect(settingsX, settingsY, settingsWidth, configHeight), "Trigger collider colour", GUIRenderer.labelStyle);
            settingsY += configHeight + 10f;

            Color triggerCollider = MultiTool.Configuration.GetColliderColour("trigger");

            // Red.
            GUI.Label(new Rect(settingsX, settingsY, settingsWidth, configHeight), Accessibility.GetAccessibleColorString("Red:", new Color(255, 0, 0)), GUIRenderer.labelStyle);
            settingsY += configHeight;
            float triggerColliderRed = GUI.HorizontalSlider(new Rect(settingsX, settingsY, settingsWidth, configHeight), triggerCollider.r * 255, 0, 255);
            triggerColliderRed = Mathf.Round(triggerColliderRed);
            settingsY += configHeight;
            bool triggerColliderRedParse = float.TryParse(GUI.TextField(new Rect(settingsX, settingsY, settingsWidth, configHeight), triggerColliderRed.ToString(), GUIRenderer.labelStyle), out triggerColliderRed);
            if (!triggerColliderRedParse)
                Logger.Log($"{triggerColliderRedParse} is not a number", Logger.LogLevel.Error);
            triggerColliderRed = Mathf.Clamp(triggerColliderRed, 0f, 255f);
            triggerCollider.r = triggerColliderRed / 255f;

            // Green.
            settingsY += configHeight + 10f;
            GUI.Label(new Rect(settingsX, settingsY, settingsWidth, configHeight), Accessibility.GetAccessibleColorString("Green:", new Color(0, 255, 0)), GUIRenderer.labelStyle);
            settingsY += configHeight;
            float triggerColliderGreen = GUI.HorizontalSlider(new Rect(settingsX, settingsY, settingsWidth, configHeight), triggerCollider.g * 255, 0, 255);
            triggerColliderGreen = Mathf.Round(triggerColliderGreen);
            settingsY += configHeight;
            bool triggerColliderGreenParse = float.TryParse(GUI.TextField(new Rect(settingsX, settingsY, settingsWidth, configHeight), triggerColliderGreen.ToString(), GUIRenderer.labelStyle), out triggerColliderGreen);
            if (!triggerColliderGreenParse)
                Logger.Log($"{triggerColliderGreenParse} is not a number", Logger.LogLevel.Error);
            triggerColliderGreen = Mathf.Clamp(triggerColliderGreen, 0f, 255f);
            triggerCollider.g = triggerColliderGreen / 255f;

            // Blue.
            settingsY += configHeight + 10f;
            GUI.Label(new Rect(settingsX, settingsY, settingsWidth, configHeight), Accessibility.GetAccessibleColorString("Blue:", new Color(0, 0, 255)), GUIRenderer.labelStyle);
            settingsY += configHeight;
            float triggerColliderBlue = GUI.HorizontalSlider(new Rect(settingsX, settingsY, settingsWidth, configHeight), triggerCollider.b * 255, 0, 255);
            triggerColliderBlue = Mathf.Round(triggerColliderBlue);
            settingsY += configHeight;
            bool triggerColliderBlueParse = float.TryParse(GUI.TextField(new Rect(settingsX, settingsY, settingsWidth, configHeight), triggerColliderBlue.ToString(), GUIRenderer.labelStyle), out triggerColliderBlue);
            if (!triggerColliderBlueParse)
                Logger.Log($"{triggerColliderBlueParse.ToString()} is not a number", Logger.LogLevel.Error);
            triggerColliderBlue = Mathf.Clamp(triggerColliderBlue, 0f, 255f);
            triggerCollider.b = triggerColliderBlue / 255f;

            // Alpha.
            settingsY += configHeight + 10f;
            GUI.Label(new Rect(settingsX, settingsY, settingsWidth, configHeight), "Alpha:", GUIRenderer.labelStyle);
            settingsY += configHeight;
            float triggerColliderAlpha = GUI.HorizontalSlider(new Rect(settingsX, settingsY, settingsWidth, configHeight), triggerCollider.a * 255, 0, 255);
            triggerColliderAlpha = Mathf.Round(triggerColliderAlpha);
            settingsY += configHeight;
            bool triggerColliderAlphaParse = float.TryParse(GUI.TextField(new Rect(settingsX, settingsY, settingsWidth, configHeight), triggerColliderAlpha.ToString(), GUIRenderer.labelStyle), out triggerColliderAlpha);
            if (!triggerColliderAlphaParse)
                Logger.Log($"{triggerColliderAlphaParse.ToString()} is not a number", Logger.LogLevel.Error);
            triggerColliderAlpha = Mathf.Clamp(triggerColliderAlpha, 0f, 255f);
            triggerCollider.a = triggerColliderAlpha / 255f;

            settingsY += configHeight + 10f;

            // Colour preview.
            // Override alpha for colour preview.
            Color triggerColliderPreview = triggerCollider;
            triggerColliderPreview.a = 1;
            pixels = new Color[] { triggerColliderPreview };
            previewTexture.SetPixels(pixels);
            previewTexture.Apply();
            previewStyle.normal.background = previewTexture;
            previewStyle.active.background = previewTexture;
            previewStyle.hover.background = previewTexture;
            previewStyle.margin = new RectOffset(0, 0, 0, 0);
            GUI.skin.button = previewStyle;
            GUI.Button(new Rect(settingsX, settingsY, settingsWidth / 2, configHeight * 2), "");
            GUI.skin.button = defaultStyle;

            settingsY += configHeight * 2 + 10f;

            triggerCollider = GUIRenderer.RenderColourPalette(settingsX, settingsY, settingsWidth / 2, triggerCollider);
            settingsY += GUIRenderer.GetPaletteHeight(settingsWidth / 2) + 10f;
            MultiTool.Configuration.UpdateColliderColour(triggerCollider, "trigger");

            if (GUI.Button(new Rect(settingsX, settingsY, buttonWidth, configHeight), "Reset to default"))
            {
                triggerCollider = new Color(0.0f, 1f, 0.0f, 0.8f);
                MultiTool.Configuration.UpdateColliderColour(triggerCollider, "trigger");
            }

            settingsY += configHeight + 10f;
            GUI.Label(new Rect(settingsX, settingsY, settingsWidth, configHeight), "Interior collider colour", GUIRenderer.labelStyle);
            settingsY += configHeight + 10f;

            Color interiorCollider = MultiTool.Configuration.GetColliderColour("interior");

            // Red.
            GUI.Label(new Rect(settingsX, settingsY, settingsWidth, configHeight), Accessibility.GetAccessibleColorString("Red:", new Color(255, 0, 0)), GUIRenderer.labelStyle);
            settingsY += configHeight;
            float interiorColliderRed = GUI.HorizontalSlider(new Rect(settingsX, settingsY, settingsWidth, configHeight), interiorCollider.r * 255, 0, 255);
            interiorColliderRed = Mathf.Round(interiorColliderRed);
            settingsY += configHeight;
            bool interiorColliderRedParse = float.TryParse(GUI.TextField(new Rect(settingsX, settingsY, settingsWidth, configHeight), interiorColliderRed.ToString(), GUIRenderer.labelStyle), out interiorColliderRed);
            if (!interiorColliderRedParse)
                Logger.Log($"{interiorColliderRedParse} is not a number", Logger.LogLevel.Error);
            interiorColliderRed = Mathf.Clamp(interiorColliderRed, 0f, 255f);
            interiorCollider.r = interiorColliderRed / 255f;

            // Green.
            settingsY += configHeight + 10f;
            GUI.Label(new Rect(settingsX, settingsY, settingsWidth, configHeight), Accessibility.GetAccessibleColorString("Green:", new Color(0, 255, 0)), GUIRenderer.labelStyle);
            settingsY += configHeight;
            float interiorColliderGreen = GUI.HorizontalSlider(new Rect(settingsX, settingsY, settingsWidth, configHeight), interiorCollider.g * 255, 0, 255);
            interiorColliderGreen = Mathf.Round(interiorColliderGreen);
            settingsY += configHeight;
            bool interiorColliderGreenParse = float.TryParse(GUI.TextField(new Rect(settingsX, settingsY, settingsWidth, configHeight), interiorColliderGreen.ToString(), GUIRenderer.labelStyle), out interiorColliderGreen);
            if (!interiorColliderGreenParse)
                Logger.Log($"{interiorColliderGreenParse} is not a number", Logger.LogLevel.Error);
            interiorColliderGreen = Mathf.Clamp(interiorColliderGreen, 0f, 255f);
            interiorCollider.g = interiorColliderGreen / 255f;

            // Blue.
            settingsY += configHeight + 10f;
            GUI.Label(new Rect(settingsX, settingsY, settingsWidth, configHeight), Accessibility.GetAccessibleColorString("Blue:", new Color(0, 0, 255)), GUIRenderer.labelStyle);
            settingsY += configHeight;
            float interiorColliderBlue = GUI.HorizontalSlider(new Rect(settingsX, settingsY, settingsWidth, configHeight), interiorCollider.b * 255, 0, 255);
            interiorColliderBlue = Mathf.Round(interiorColliderBlue);
            settingsY += configHeight;
            bool interiorColliderBlueParse = float.TryParse(GUI.TextField(new Rect(settingsX, settingsY, settingsWidth, configHeight), interiorColliderBlue.ToString(), GUIRenderer.labelStyle), out interiorColliderBlue);
            if (!interiorColliderBlueParse)
                Logger.Log($"{interiorColliderBlueParse.ToString()} is not a number", Logger.LogLevel.Error);
            interiorColliderBlue = Mathf.Clamp(interiorColliderBlue, 0f, 255f);
            interiorCollider.b = interiorColliderBlue / 255f;

            // Alpha.
            settingsY += configHeight + 10f;
            GUI.Label(new Rect(settingsX, settingsY, settingsWidth, configHeight), "Alpha:", GUIRenderer.labelStyle);
            settingsY += configHeight;
            float interiorColliderAlpha = GUI.HorizontalSlider(new Rect(settingsX, settingsY, settingsWidth, configHeight), interiorCollider.a * 255, 0, 255);
            interiorColliderAlpha = Mathf.Round(interiorColliderAlpha);
            settingsY += configHeight;
            bool interiorColliderAlphaParse = float.TryParse(GUI.TextField(new Rect(settingsX, settingsY, settingsWidth, configHeight), interiorColliderAlpha.ToString(), GUIRenderer.labelStyle), out interiorColliderAlpha);
            if (!interiorColliderAlphaParse)
                Logger.Log($"{interiorColliderAlphaParse.ToString()} is not a number", Logger.LogLevel.Error);
            interiorColliderAlpha = Mathf.Clamp(interiorColliderAlpha, 0f, 255f);
            interiorCollider.a = interiorColliderAlpha / 255f;

            settingsY += configHeight + 10f;

            // Colour preview.
            // Override alpha for colour preview.
            Color interiorColliderPreview = interiorCollider;
            interiorColliderPreview.a = 1;
            pixels = new Color[] { interiorColliderPreview };
            previewTexture.SetPixels(pixels);
            previewTexture.Apply();
            previewStyle.normal.background = previewTexture;
            previewStyle.active.background = previewTexture;
            previewStyle.hover.background = previewTexture;
            previewStyle.margin = new RectOffset(0, 0, 0, 0);
            GUI.skin.button = previewStyle;
            GUI.Button(new Rect(settingsX, settingsY, settingsWidth / 2, configHeight * 2), "");
            GUI.skin.button = defaultStyle;

            settingsY += configHeight * 2 + 10f;

            interiorCollider = GUIRenderer.RenderColourPalette(settingsX, settingsY, settingsWidth / 2, interiorCollider);
            settingsY += GUIRenderer.GetPaletteHeight(settingsWidth / 2) + 10f;
            MultiTool.Configuration.UpdateColliderColour(interiorCollider, "interior");

            if (GUI.Button(new Rect(settingsX, settingsY, buttonWidth, configHeight), "Reset to default"))
            {
                interiorCollider = new Color(0f, 0f, 1f, 0.8f);
                MultiTool.Configuration.UpdateColliderColour(interiorCollider, "interior");
            }

            settingsLastHeight = settingsY;

            GUI.EndScrollView();
        }
	}
}
