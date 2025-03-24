using MultiTool.Core;
using System;
using System.Linq;
using UnityEngine;

namespace MultiTool.Utilities.UI
{
    internal class Accessibility
    {
        public enum AccessibilityMode
        {
            None,
            Contrast,
            Colourless,
        }

        private static AccessibilityMode _accessibilityMode = AccessibilityMode.Contrast;
        private static bool _doesAffectColors = true;
        private static bool _hasLoadedFromConfig = false;

        /// <summary>
        /// Get pretty name for given accessibility mode.
        /// </summary>
        /// <param name="mode">Mode ot get name for</param>
        /// <returns>Prettified name for accessibility mode</returns>
        public static string GetAccessibilityModeName(int mode)
        {
            switch (mode)
            {
                case 1:
                    return "Improved contrast";
                default:
                    return ((AccessibilityMode)mode).ToString();
            }
        }

        public static int GetAccessibilityModeCount()
        {
            return (int)Enum.GetValues(typeof(AccessibilityMode)).Cast<AccessibilityMode>().Max();
        }

        /// <summary>
        /// Set the current accessibility mode.
        /// </summary>
        /// <param name="mode">New mode</param>
        public static void SetAccessibilityMode(int mode)
        {
            _accessibilityMode = (AccessibilityMode)mode;
        }

        /// <summary>
        /// Set the current accessibility mode.
        /// </summary>
        /// <param name="mode">New mode</param>
        public static void SetAccessibilityMode(AccessibilityMode mode)
        {
            _accessibilityMode = mode;
        }

        /// <summary>
        /// Get the current accessibility mode.
        /// </summary>
        /// <returns>Current accessibility mode</returns>
        public static AccessibilityMode GetAccessibilityMode()
        {
            return _accessibilityMode;
        }

        /// <summary>
        /// Set whether accessibility mode affects color slider labels.
        /// </summary>
        /// <param name="doesAffectColors">True it should, otherwise false</param>
        public static void SetDoesAffectColors(bool doesAffectColors)
        {
            _doesAffectColors = doesAffectColors;
        }

        /// <summary>
        /// Set whether accessibility mode affects color slider labels.
        /// </summary>
        /// <param name="doesAffectColors">True it should, otherwise false</param>
        public static bool GetDoesAffectColors()
        {
            return _doesAffectColors;
        }

        /// <summary>
		/// Translate a string appropriate for the selected accessibility mode
		/// </summary>
		/// <param name="str">The string to translate</param>
		/// <param name="state">The button state</param>
		/// <returns>Accessibility mode translated string</returns>
		public static string GetAccessibleString(string str, bool state)
        {
            LoadFromConfig();

			Theme theme = Styling.GetActiveTheme();
			string onColour = ColorUtility.ToHtmlStringRGB(theme.AccessibilityOnColour);
			string offColour = ColorUtility.ToHtmlStringRGB(theme.AccessibilityOffColour);

			switch (_accessibilityMode)
            {
                case AccessibilityMode.Contrast:
                    return state ? $"<color=#{onColour}>{str}</color>" : str;
                case AccessibilityMode.Colourless:
                    return state ? $"{str} ✔" : $"{str} ✖";
            }

            return state ? $"<color=#{onColour}>{str}</color>" : $"<color=#{offColour}>{str}</color>";
        }

        /// <summary>
        /// Translate a string appropriate for the selected accessibility mode
        /// </summary>
        /// <param name="trueStr">The string to translate for true</param>
        /// <param name="falseStr">The string to translate for false</param>
        /// <param name="state">The button state</param>
        /// <returns>Accessibility mode translated string</returns>
        public static string GetAccessibleString(string trueStr, string falseStr, bool state)
        {
            LoadFromConfig();

			Theme theme = Styling.GetActiveTheme();
			string onColour = ColorUtility.ToHtmlStringRGB(theme.AccessibilityOnColour);
			string offColour = ColorUtility.ToHtmlStringRGB(theme.AccessibilityOffColour);

			switch (_accessibilityMode)
            {
                case AccessibilityMode.Contrast:
                    return state ? $"<color=#{onColour}>{trueStr}</color>" : falseStr;
                case AccessibilityMode.Colourless:
                    return state ? $"{trueStr} ✔" : $"{falseStr} ✖";
            }

            return state ? $"<color=#{onColour}>{trueStr}</color>" : $"<color=#{offColour}>{falseStr}</color>";
        }

        /// <summary>
        /// Get accessible version of a colour label string.
        /// </summary>
        /// <param name="str">The string to translate</param>
        /// <param name="color">The associated color</param>
        /// <returns>Accessibility mode translated string</returns>
        public static string GetAccessibleColorString(string str, Color color)
        {
            LoadFromConfig();

            if (_doesAffectColors)
            {
                switch (_accessibilityMode)
                {
                    case AccessibilityMode.Contrast:
                    case AccessibilityMode.Colourless:
                        return str;
                }
            }

            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{str}</color>";
        }

        /// <summary>
        /// Get accessibility settings from configuration.
        /// </summary>
        private static void LoadFromConfig()
        {
            if (_hasLoadedFromConfig) return;
            _accessibilityMode = (AccessibilityMode)MultiTool.Configuration.GetAccessibilityMode();
            _doesAffectColors = MultiTool.Configuration.GetAccessibilityModeAffectsColor(true);
            _hasLoadedFromConfig = true;
        }
    }
}
