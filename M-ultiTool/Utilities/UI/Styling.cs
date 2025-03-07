using MultiTool.Extensions;
using MultiTool.Modules;
using UnityEngine;

namespace MultiTool.Utilities.UI
{
    internal static class Styling
    {
        private static bool _hasInitialised = false;

        public static GUISkin Skin;

        // GUI styles.
        private static GUIStyle _buttonStyle;

        // Textures.
        private static Texture2D _black;
        private static Texture2D _blackHover;
        private static Texture2D _darkGrey;
        private static Texture2D _darkGreyHover;
        private static Texture2D _grey;
        private static Texture2D _greyHover;
        private static Texture2D _lightGrey;
        private static Texture2D _lightGreyHover;
        private static Texture2D _white;
        private static Texture2D _whiteHover;
        private static Texture2D _blackTranslucent;
        private static Texture2D _blackTranslucentHover;
        private static Texture2D _transparent;

        public static void CreateStyling()
        {
            if (!_hasInitialised)
            {
                // Create skin off default to save setting all GUIStyles individually.
                // Unity doesn't offer a way of doing this so build it manually.
                Skin = ScriptableObject.CreateInstance<GUISkin>();
                Skin.name = "M-ultiTool";
                Skin.box = new GUIStyle(GUI.skin.box);
                Skin.button = new GUIStyle(GUI.skin.button);
                Skin.horizontalScrollbar = new GUIStyle(GUI.skin.horizontalScrollbar);
                Skin.horizontalScrollbarLeftButton = new GUIStyle(GUI.skin.horizontalScrollbarLeftButton);
                Skin.horizontalScrollbarRightButton = new GUIStyle(GUI.skin.horizontalScrollbarRightButton);
                Skin.horizontalScrollbarThumb = new GUIStyle(GUI.skin.horizontalScrollbarThumb);
                Skin.horizontalSlider = new GUIStyle(GUI.skin.horizontalSlider);
                Skin.horizontalSliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);
                Skin.label = new GUIStyle(GUI.skin.label);
                Skin.scrollView = new GUIStyle(GUI.skin.scrollView);
                Skin.textArea = new GUIStyle(GUI.skin.textArea);
                Skin.textField = new GUIStyle(GUI.skin.textField);
                Skin.toggle = new GUIStyle(GUI.skin.toggle);
                Skin.verticalScrollbar = new GUIStyle(GUI.skin.verticalScrollbar);
                Skin.verticalScrollbarDownButton = new GUIStyle(GUI.skin.verticalScrollbarDownButton);
                Skin.verticalScrollbarThumb = new GUIStyle(GUI.skin.verticalScrollbarThumb);
                Skin.verticalScrollbarUpButton = new GUIStyle(GUI.skin.verticalScrollbarUpButton);
                Skin.verticalSlider = new GUIStyle(GUI.skin.verticalSlider);
                Skin.verticalSliderThumb = new GUIStyle(GUI.skin.verticalSliderThumb);
                Skin.window = new GUIStyle(GUI.skin.window);
                Skin.font = GUI.skin.font;

                // Create any required textures.
                _black = GUIExtensions.ColorTexture(1, 1, new Color(0f, 0f, 0f));
                _blackHover = GUIExtensions.ColorTexture(1, 1, new Color(0.1f, 0.1f, 0.1f));
                _darkGrey = GUIExtensions.ColorTexture(1, 1, new Color(0.15f, 0.15f, 0.15f));
                _darkGreyHover = GUIExtensions.ColorTexture(1, 1, new Color(0.25f, 0.25f, 0.25f));
                _grey = GUIExtensions.ColorTexture(1, 1, new Color(0.4f, 0.4f, 0.4f));
                _greyHover = GUIExtensions.ColorTexture(1, 1, new Color(0.5f, 0.5f, 0.5f));
                _lightGrey = GUIExtensions.ColorTexture(1, 1, new Color(0.7f, 0.7f, 0.7f));
                _lightGreyHover = GUIExtensions.ColorTexture(1, 1, new Color(0.8f, 0.8f, 0.8f));
                _white = GUIExtensions.ColorTexture(1, 1, new Color(1, 1, 1));
                _whiteHover = GUIExtensions.ColorTexture(1, 1, new Color(0.9f, 0.9f, 0.9f));
                _blackTranslucent = GUIExtensions.ColorTexture(1, 1, new Color(0, 0, 0, 0.4f));
                _blackTranslucentHover = GUIExtensions.ColorTexture(1, 1, new Color(0, 0, 0, 0.5f));
                _transparent = GUIExtensions.ColorTexture(1, 1, new Color(0, 0, 0, 0));

                // Override scrollbar width and height.
                Skin.verticalScrollbar.fixedWidth = GUIRenderer.scrollWidth;
                Skin.verticalScrollbarThumb.fixedWidth = GUIRenderer.scrollWidth;
                Skin.horizontalScrollbar.fixedHeight = GUIRenderer.scrollWidth;
                Skin.horizontalScrollbarThumb.fixedHeight = GUIRenderer.scrollWidth;

                // Button styling.
                _buttonStyle = new GUIStyle(Skin.button);
                _buttonStyle.padding = new RectOffset(10, 10, 5, 5);

                GUIStyle buttonPrimary = new GUIStyle(_buttonStyle);
                buttonPrimary.name = "ButtonPrimary";
                buttonPrimary.normal.background = _grey;
                buttonPrimary.hover.background = _greyHover;
                buttonPrimary.active.background = _greyHover;
                buttonPrimary.focused.background = _greyHover;

                // Default to use primary button.
                Skin.button = buttonPrimary;

				GUIStyle buttonPrimaryWrap = new GUIStyle(buttonPrimary);
				buttonPrimaryWrap.name = "ButtonPrimaryWrap";
				buttonPrimaryWrap.wordWrap = true;

				GUIStyle buttonSecondary = new GUIStyle(_buttonStyle);
                buttonSecondary.name = "ButtonSecondary";
                buttonSecondary.normal.background = _darkGrey;
                buttonSecondary.hover.background = _darkGreyHover;
                buttonSecondary.active.background = _darkGreyHover;
                buttonSecondary.focused.background = _darkGreyHover;

                GUIStyle buttonBlack = new GUIStyle(_buttonStyle);
                buttonBlack.name = "ButtonBlack";
                buttonBlack.normal.background = _black;
                buttonBlack.hover.background = _blackHover;
                buttonBlack.active.background = _blackHover;
                buttonBlack.focused.background = _blackHover;

                GUIStyle buttonBlackTranslucent = new GUIStyle(_buttonStyle);
                buttonBlackTranslucent.name = "ButtonBlackTranslucent";
                buttonBlackTranslucent.normal.background = _blackTranslucent;
                buttonBlackTranslucent.hover.background = _blackTranslucentHover;
                buttonBlackTranslucent.active.background = _blackTranslucentHover;
                buttonBlackTranslucent.focused.background = _blackTranslucentHover;
                buttonBlackTranslucent.normal.textColor = Color.white;
                buttonBlackTranslucent.hover.textColor = Color.white;
                buttonBlackTranslucent.active.textColor = Color.white;
                buttonBlackTranslucent.focused.textColor = Color.white;

                GUIStyle buttonLightGrey = new GUIStyle(_buttonStyle);
                buttonLightGrey.name = "ButtonLightGrey";
                buttonLightGrey.normal.background = _lightGrey;
                buttonLightGrey.hover.background = _lightGreyHover;
                buttonLightGrey.active.background = _lightGreyHover;
                buttonLightGrey.focused.background = _lightGreyHover;
                buttonLightGrey.normal.textColor = Color.black;
                buttonLightGrey.hover.textColor = Color.black;
                buttonLightGrey.active.textColor = Color.black;
                buttonLightGrey.focused.textColor = Color.black;

                GUIStyle buttonWhite = new GUIStyle(_buttonStyle);
                buttonWhite.name = "ButtonWhite";
                buttonWhite.normal.background = _white;
                buttonWhite.hover.background = _whiteHover;
                buttonWhite.active.background = _whiteHover;
                buttonWhite.focused.background = _whiteHover;
                buttonWhite.normal.textColor = Color.black;
                buttonWhite.hover.textColor = Color.black;
                buttonWhite.active.textColor = Color.black;
                buttonWhite.focused.textColor = Color.black;

                GUIStyle buttonTransparent = new GUIStyle(_buttonStyle);
                buttonTransparent.name = "ButtonTransparent";
                buttonTransparent.normal.background = null;
                buttonTransparent.hover.background = _transparent;
                buttonTransparent.active.background = _transparent;
                buttonTransparent.focused.background = _transparent;
                buttonTransparent.normal.textColor = Color.white;
                buttonTransparent.hover.textColor = Color.white;
                buttonTransparent.active.textColor = Color.white;
                buttonTransparent.focused.textColor = Color.white;
                buttonTransparent.wordWrap = true;
                buttonTransparent.alignment = TextAnchor.LowerCenter;

                // Box styling.
                Skin.box.normal.background = _blackTranslucent;

                // Label styling.
                GUIStyle labelHeader = new GUIStyle(Skin.label);
                labelHeader.name = "LabelHeader";
                labelHeader.alignment = TextAnchor.MiddleLeft;
                labelHeader.fontSize = 24;
                labelHeader.fontStyle = FontStyle.Bold;
                labelHeader.normal.textColor = Color.white;
                labelHeader.hover.textColor = Color.white;
                labelHeader.active.textColor = Color.white;
                labelHeader.focused.textColor = Color.white;
                labelHeader.wordWrap = true;

				GUIStyle labelMessage = new GUIStyle(Skin.label);
				labelMessage.name = "LabelMessage";
				labelMessage.alignment = TextAnchor.MiddleCenter;
				labelMessage.fontSize = 40;
				labelMessage.fontStyle = FontStyle.Bold;
				labelMessage.normal.textColor = Color.white;
				labelMessage.hover.textColor = Color.white;
				labelMessage.active.textColor = Color.white;
				labelMessage.focused.textColor = Color.white;
				labelMessage.wordWrap = true;

				// Add custom styles.
				Skin.customStyles = new GUIStyle[]
                {
                    // Buttons.
                    buttonPrimary,
					buttonPrimaryWrap,
					buttonSecondary,
                    buttonBlack,
                    buttonBlackTranslucent,
                    buttonLightGrey,
                    buttonWhite,
                    buttonTransparent,

                    // Labels.
                    labelHeader,
					labelMessage,

                    // These are just here to prevent log errors, idk where they're coming from.
                    new GUIStyle() { name = "thumb" },
                    new GUIStyle() { name = "upbutton" },
                    new GUIStyle() { name = "downbutton" },
                };

                _hasInitialised = true;
            }
        }
    }
}
