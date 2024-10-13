using MultiTool.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = MultiTool.Modules.Logger;

namespace MultiTool.Utilities.UI
{
    internal static class Colour
    {
        private static Color _colour = Color.white;
        private static List<Color> _palette = new List<Color>();
        private static Dictionary<int, GUIStyle> _paletteCache = new Dictionary<int, GUIStyle>();
        private static bool _hasInitialised = false;
        private static float _lastPaletteWidth = 0;
        private static List<List<Color>> _paletteChunked = new List<List<Color>>();

        /// <summary>
        /// Build palette and cache.
        /// </summary>
        private static void Initialise()
        {
            if (_hasInitialised) return;

            // Set default palette to all white.
            _palette.Clear();
            _palette = Enumerable.Repeat(Color.white, 60).ToList();
            _paletteCache.Clear();

            try
            {
                _palette = MultiTool.Configuration.GetPalette(_palette);
                PopulatePaletteCache();
            }
            catch (Exception ex)
            {
                Logger.Log($"Palette config load error. Details: {ex}", Logger.LogLevel.Error);
            }

            _hasInitialised = true;
        }

        /// <summary>
        /// Build palette cache from current palette.
        /// </summary>
        private static void PopulatePaletteCache()
        {
            for (int i = 0; i < _palette.Count; i++)
            {
                Color paletteColour = _palette[i];

                // Build cache if empty.
                if (!_paletteCache.ContainsKey(i))
                {
                    GUIStyle swatchStyle = new GUIStyle(GUI.skin.button);
                    Texture2D swatchTexture = GUIExtensions.ColorTexture(1, 1, paletteColour);
                    Color hoverColour = paletteColour.ChangeBrightness(0.1f);
                    Texture2D swatchHoverTexture = GUIExtensions.ColorTexture(1, 1, hoverColour);
                    swatchStyle.normal.background = swatchTexture;
                    swatchStyle.active.background = swatchTexture;
                    swatchStyle.hover.background = swatchHoverTexture;
                    swatchStyle.margin = new RectOffset(0, 0, 0, 0);
                    _paletteCache.Add(i, swatchStyle);
                }
            }
        }

        /// <summary>
        /// Update single index of palette cache.
        /// </summary>
        /// <param name="index">Index to update</param>
        /// <param name="newColour">Colour to set to</param>
        private static void UpdateCacheIndex(int index, Color newColour)
        {
            if (!_paletteCache.ContainsKey(index))
                throw new KeyNotFoundException();

            GUIStyle swatchStyle = new GUIStyle(GUI.skin.button);
            Texture2D swatchTexture = GUIExtensions.ColorTexture(1, 1, newColour);
            Color hoverColour = newColour.ChangeBrightness(0.1f);
            Texture2D swatchHoverTexture = GUIExtensions.ColorTexture(1, 1, hoverColour);
            swatchStyle.normal.background = swatchTexture;
            swatchStyle.active.background = swatchTexture;
            swatchStyle.hover.background = swatchHoverTexture;
            swatchStyle.margin = new RectOffset(0, 0, 0, 0);
            _paletteCache[index] = swatchStyle;
        }

        /// <summary>
        /// Getter for colour.
        /// </summary>
        /// <returns>Current colour</returns>
        public static Color GetColour()
        {
            return _colour;
        }

        /// <summary>
        /// Setter for colour.
        /// </summary>
        /// <param name="colour">New colour to set</param>
        public static void SetColour(Color colour)
        {
            _colour = colour;
        }

        /// <summary>
		/// Render colour palette.
		/// </summary>
		/// <param name="maxWidth">Maximum palette width</param>
		/// <param name="inputColour">Starting colour</param>
		/// <returns>Selected colour</returns>
		public static Color RenderColourPalette(float maxWidth, Color? inputColour = null)
        {
            Initialise();

            Color selectedColour = _colour;
            if (inputColour != null)
                selectedColour = inputColour.Value;

            // Width changed or chunked palette empty, chunk the palette into rows for rendering.
            if (_lastPaletteWidth != maxWidth || _paletteChunked.Count == 0)
            {
                int rowLength = Mathf.FloorToInt(maxWidth / 32f);
                _paletteChunked = _palette.ChunkBy(rowLength);
                _lastPaletteWidth = maxWidth;
            }

            GUILayout.BeginVertical("box");
            int index = 0;
            foreach (List<Color> palette in _paletteChunked)
            {
                GUILayout.BeginHorizontal();
                foreach (Color colour in palette)
                {
                    if (GUILayout.Button(string.Empty, _paletteCache[index], GUILayout.Width(30), GUILayout.Height(30)))
                    {
                        switch (Event.current.button)
                        {
                            // Left click, apply colour.
                            case 0:
                                selectedColour = colour;
                                break;

                            // Right click, set new colour.
                            case 1:
                                // Override alpha.
                                selectedColour.a = 1;

                                // Update palette index colour.
                                _palette[index] = selectedColour;
                                MultiTool.Configuration.UpdatePalette(_palette);

                                // Update texture cache.
                                UpdateCacheIndex(index, selectedColour);

                                // Re-chunk palette.
                                _paletteChunked.Clear();
                                break;
                        }
                    }
                    GUILayout.Space(2);
                    index++;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(2);
            }
            GUILayout.EndVertical();

            return selectedColour;
        }

        /// <summary>
        /// Render colour selection sliders.
        /// </summary>
        /// <param name="maxWidth">Maximum slider width</param>
        /// <param name="inputColour">Starting colour</param>
        /// <param name="hasAlpha">True if alpha slider should be rendered, otherwise false</param>
        public static Color RenderColourSliders(float maxWidth, Color? inputColour = null, bool hasAlpha = false)
        {
            Color sliderColour = _colour;
            if (inputColour != null)
                sliderColour = inputColour.Value;

            // Vehicle colour sliders.
            // Red.
            GUILayout.BeginVertical();
            GUILayout.Label(Accessibility.GetAccessibleColorString("Red:", new Color(255, 0, 0)));
            float red = GUILayout.HorizontalSlider(sliderColour.r * 255, 0, 255);
            red = Mathf.Round(red);
            bool redParse = float.TryParse(GUILayout.TextField(red.ToString()), out red);
            if (!redParse)
                Logger.Log($"{redParse} is not a number", Logger.LogLevel.Error);
            red = Mathf.Clamp(red, 0f, 255f);
            sliderColour.r = red / 255f;

            GUILayout.Space(10);

            // Green.
            GUILayout.Label(Accessibility.GetAccessibleColorString("Green:", new Color(0, 255, 0)));
            float green = GUILayout.HorizontalSlider(sliderColour.g * 255, 0, 255);
            green = Mathf.Round(green);
            bool greenParse = float.TryParse(GUILayout.TextField(green.ToString()), out green);
            if (!greenParse)
                Logger.Log($"{greenParse} is not a number", Logger.LogLevel.Error);
            green = Mathf.Clamp(green, 0f, 255f);
            sliderColour.g = green / 255f;

            GUILayout.Space(10);

            // Blue.
            GUILayout.Label(Accessibility.GetAccessibleColorString("Blue:", new Color(0, 0, 255)));
            float blue = GUILayout.HorizontalSlider(sliderColour.b * 255, 0, 255);
            blue = Mathf.Round(blue);
            bool blueParse = float.TryParse(GUILayout.TextField(blue.ToString()), out blue);
            if (!blueParse)
                Logger.Log($"{blueParse} is not a number", Logger.LogLevel.Error);
            blue = Mathf.Clamp(blue, 0f, 255f);
            sliderColour.b = blue / 255f;

            GUILayout.Space(10);

            if (hasAlpha)
            {
                // Alpha.
                GUILayout.Label(Accessibility.GetAccessibleColorString("Alpha:", new Color(0, 0, 255)));
                float alpha = GUILayout.HorizontalSlider(sliderColour.a * 255, 0, 255);
                alpha = Mathf.Round(alpha);
                bool alphaParse = float.TryParse(GUILayout.TextField(alpha.ToString()), out alpha);
                if (!alphaParse)
                    Logger.Log($"{alphaParse} is not a number", Logger.LogLevel.Error);
                alpha = Mathf.Clamp(alpha, 0f, 255f);
                sliderColour.a = alpha / 255f;
            }

            GUILayout.Space(5);

            // Colour preview.
            Color previewColour = _colour;
            previewColour.a = 1;
            GUIStyle previewStyle = new GUIStyle(GUI.skin.button);
            Texture2D previewTexture = new Texture2D(1, 1);
            Color[] pixels = new Color[] { previewColour };
            previewTexture.SetPixels(pixels);
            previewTexture.Apply();
            previewStyle.normal.background = previewTexture;
            previewStyle.active.background = previewTexture;
            previewStyle.hover.background = previewTexture;
            previewStyle.margin = new RectOffset(0, 0, 0, 0);
            GUILayout.Button(string.Empty, previewStyle, GUILayout.Height(30));

            GUILayout.Space(10);
            sliderColour = RenderColourPalette(maxWidth, sliderColour);
            GUILayout.EndVertical();

            SetColour(sliderColour);

            return sliderColour;
        }
    }
}
