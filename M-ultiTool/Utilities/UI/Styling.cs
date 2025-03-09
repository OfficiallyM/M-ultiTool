using MultiTool.Extensions;
using MultiTool.Modules;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

namespace MultiTool.Utilities.UI
{
    internal static class Styling
    {
        private static bool _hasInitialised = false;
        private static GUISkin _skin;

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
		private static Texture2D _green;
		private static Texture2D _orange;
		private static Texture2D _red;
		private static Texture2D _blue;

        public static void Bootstrap()
        {
            if (!_hasInitialised)
            {
				// Create skin off default to save setting all GUIStyles individually.
				// Unity doesn't offer a way of doing this so build it manually.
				_skin = ScriptableObject.CreateInstance<GUISkin>();
				_skin.name = "M-ultiTool";
				_skin.box = new GUIStyle(GUI.skin.box);
				_skin.button = new GUIStyle(GUI.skin.button);
				_skin.horizontalScrollbar = new GUIStyle(GUI.skin.horizontalScrollbar);
                _skin.horizontalScrollbarLeftButton = new GUIStyle(GUI.skin.horizontalScrollbarLeftButton);
                _skin.horizontalScrollbarRightButton = new GUIStyle(GUI.skin.horizontalScrollbarRightButton);
                _skin.horizontalScrollbarThumb = new GUIStyle(GUI.skin.horizontalScrollbarThumb);
                _skin.horizontalSlider = new GUIStyle(GUI.skin.horizontalSlider);
                _skin.horizontalSliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);
                _skin.label = new GUIStyle(GUI.skin.label);
                _skin.scrollView = new GUIStyle(GUI.skin.scrollView);
                _skin.textArea = new GUIStyle(GUI.skin.textArea);
                _skin.textField = new GUIStyle(GUI.skin.textField);
                _skin.toggle = new GUIStyle(GUI.skin.toggle);
                _skin.verticalScrollbar = new GUIStyle(GUI.skin.verticalScrollbar);
                _skin.verticalScrollbarDownButton = new GUIStyle(GUI.skin.verticalScrollbarDownButton);
                _skin.verticalScrollbarThumb = new GUIStyle(GUI.skin.verticalScrollbarThumb);
                _skin.verticalScrollbarUpButton = new GUIStyle(GUI.skin.verticalScrollbarUpButton);
                _skin.verticalSlider = new GUIStyle(GUI.skin.verticalSlider);
                _skin.verticalSliderThumb = new GUIStyle(GUI.skin.verticalSliderThumb);
                _skin.window = new GUIStyle(GUI.skin.window);
                _skin.font = GUI.skin.font;

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
				_green = GUIExtensions.ColorTexture(1, 1, new Color(0.16f, 0.61f, 0));
				_orange = GUIExtensions.ColorTexture(1, 1, new Color(0.84f, 0.52f, 0));
				_red = GUIExtensions.ColorTexture(1, 1, new Color(1, 0, 0));
				_blue = GUIExtensions.ColorTexture(1, 1, new Color(0, 0.38f, 0.77f));

				// Override scrollbar width and height.
				_skin.verticalScrollbar.fixedWidth = GUIRenderer.scrollWidth;
                _skin.verticalScrollbarThumb.fixedWidth = GUIRenderer.scrollWidth;
                _skin.horizontalScrollbar.fixedHeight = GUIRenderer.scrollWidth;
                _skin.horizontalScrollbarThumb.fixedHeight = GUIRenderer.scrollWidth;

                // Button styling.
                _buttonStyle = new GUIStyle(_skin.button);
                _buttonStyle.padding = new RectOffset(10, 10, 5, 5);

                GUIStyle buttonPrimary = new GUIStyle(_buttonStyle);
                buttonPrimary.name = "ButtonPrimary";
                buttonPrimary.normal.background = _grey;
                buttonPrimary.hover.background = _greyHover;
                buttonPrimary.active.background = _greyHover;
                buttonPrimary.focused.background = _greyHover;

                // Default to use primary button.
                _skin.button = buttonPrimary;

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
				_skin.box.normal.background = _blackTranslucent;

                // Label styling.
                GUIStyle labelHeader = new GUIStyle(_skin.label);
                labelHeader.name = "LabelHeader";
                labelHeader.alignment = TextAnchor.MiddleLeft;
                labelHeader.fontSize = 24;
                labelHeader.fontStyle = FontStyle.Bold;
                labelHeader.normal.textColor = Color.white;
                labelHeader.hover.textColor = Color.white;
                labelHeader.active.textColor = Color.white;
                labelHeader.focused.textColor = Color.white;
                labelHeader.wordWrap = true;

				GUIStyle labelMessage = new GUIStyle(_skin.label);
				labelMessage.name = "LabelMessage";
				labelMessage.alignment = TextAnchor.MiddleCenter;
				labelMessage.fontSize = 40;
				labelMessage.fontStyle = FontStyle.Bold;
				labelMessage.normal.textColor = Color.white;
				labelMessage.hover.textColor = Color.white;
				labelMessage.active.textColor = Color.white;
				labelMessage.focused.textColor = Color.white;
				labelMessage.wordWrap = true;

				GUIStyle labelCenter = new GUIStyle(_skin.label);
				labelCenter.name = "LabelCenter";
				labelCenter.alignment = TextAnchor.MiddleCenter;
				labelMessage.normal.textColor = Color.white;
				labelMessage.hover.textColor = Color.white;
				labelMessage.active.textColor = Color.white;
				labelMessage.focused.textColor = Color.white;
				labelMessage.wordWrap = true;

				// Notification box styling.
				GUIStyle boxGrey = new GUIStyle(_skin.box);
				boxGrey.name = "BoxGrey";
				boxGrey.normal.background = _grey;

				GUIStyle boxGreen = new GUIStyle(_skin.box);
				boxGreen.name = "BoxGreen";
				boxGreen.normal.background = _green;

				GUIStyle boxOrange = new GUIStyle(_skin.box);
				boxOrange.name = "BoxOrange";
				boxOrange.normal.background = _orange;

				GUIStyle boxRed = new GUIStyle(_skin.box);
				boxRed.name = "BoxRed";
				boxRed.normal.background = _red;

				GUIStyle boxBlue = new GUIStyle(_skin.box);
				boxBlue.name = "BoxBlue";
				boxBlue.normal.background = _blue;

				// Add custom styles.
				_skin.customStyles = new GUIStyle[]
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
					labelCenter,

					// Notification boxes.
					boxGrey,
					boxGreen,
					boxOrange,
					boxRed,
					boxBlue,

                    // These are just here to prevent log errors, idk where they're coming from.
                    new GUIStyle() { name = "thumb" },
                    new GUIStyle() { name = "upbutton" },
                    new GUIStyle() { name = "downbutton" },
                };

                _hasInitialised = true;
            }
		}

		public static GUISkin GetActiveSkin() => _skin;
	}
}
