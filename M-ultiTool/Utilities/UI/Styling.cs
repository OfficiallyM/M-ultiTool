using MultiTool.Core;
using MultiTool.Extensions;
using MultiTool.Modules;
using System.Collections.Generic;
using UnityEngine;

namespace MultiTool.Utilities.UI
{
    internal static class Styling
    {
        private static bool _hasInitialised = false;
		private static GUISkin _baseSkin;
        private static GUISkin _skin;
		private static Themes _themes = new Themes();
		private static Theme _activeTheme;

        // GUI styles.
        private static GUIStyle _buttonStyle;

        // Core textures.
        private static Texture2D _black;
        private static Texture2D _blackHover;
        private static Texture2D _grey;
        private static Texture2D _transparent;
		private static Texture2D _green;
		private static Texture2D _orange;
		private static Texture2D _red;
		private static Texture2D _blue;

		public static GUISkin GetActiveSkin() => _skin;
		public static Theme GetActiveTheme() => _activeTheme;

		public static void Bootstrap()
        {
            if (!_hasInitialised)
            {
				_baseSkin = CreateSkin(GUI.skin, null);

				_themes.Data = new List<Theme>();

				// Add default themes.
				_themes.Add(new Theme()
				{
					Name = "Greyscale",

					ButtonPrimaryColour = new Color(0.4f, 0.4f, 0.4f),
					ButtonPrimaryHoverColour = new Color(0.5f, 0.5f, 0.5f),
					ButtonSecondaryColour = new Color(0.15f, 0.15f, 0.15f),
					ButtonSecondaryHoverColour = new Color(0.25f, 0.25f, 0.25f),
					BoxColour = new Color(0, 0, 0, 0.4f),
					BoxHoverColour = new Color(0, 0, 0, 0.5f),

					ButtonPrimaryTextColour = Color.white,
					ButtonSecondaryTextColour = Color.white,
					TextColour = Color.white,
					AccessibilityOnColour = Color.green,
					AccessibilityOffColour = Color.red,
				});
				_themes.Add(new Theme()
				{
					Name = "M- Purple",

					ButtonPrimaryColour = new Color(68 / 255f, 0 / 255f, 132 / 255f),
					ButtonPrimaryHoverColour = new Color(80 / 255f, 0 / 255f, 155 / 255f),
					ButtonSecondaryColour = new Color(132 / 255f, 0 / 255f, 130 / 255f),
					ButtonSecondaryHoverColour = new Color(158 / 255f, 0 / 255f, 155 / 255f),
					BoxColour = new Color(0, 0, 0, 0.8f),
					BoxHoverColour = new Color(0, 0, 0, 0.6f),

					ButtonPrimaryTextColour = Color.white,
					ButtonSecondaryTextColour = Color.white,
					TextColour = Color.white,
					AccessibilityOnColour = Color.green,
					AccessibilityOffColour = Color.white,
				});

				// TODO: Load any custom themes.
				// Also requires the UI for building and the ability to save.
				// Thinking to store similar to config but in their own file.

				CreateThemeTextures();
				LoadSelectedTheme();
				_hasInitialised = true;
            }
		}

		/// <summary>
		/// Set the active theme by name.
		/// </summary>
		/// <param name="name">Theme name</param>
		public static void SetActiveTheme(string name)
		{
			Theme theme = _themes.GetByName(name);
			if (theme == null) return;

			_activeTheme = theme;

			// Save in config.
			MultiTool.Configuration.UpdateTheme(_activeTheme.Name);

			_skin = CreateSkinForTheme(_activeTheme);
		}

		/// <summary>
		/// Get all available theme names.
		/// </summary>
		/// <returns>Array of theme names</returns>
		public static string[] GetThemeNames()
		{
			return _themes.GetThemeNames();
		}

		private static GUISkin CreateSkin(GUISkin original, string name)
		{
			// Create skin off default to save setting all GUIStyles individually.
			// Unity doesn't offer a way of doing this so build it manually.
			GUISkin skin = ScriptableObject.CreateInstance<GUISkin>();
			skin.name = $"M-ultiTool";
			if (name != null)
			{
				skin.name += $" - {name}";
			}
			skin.box = new GUIStyle(original.box);
			skin.button = new GUIStyle(original.button);
			skin.horizontalScrollbar = new GUIStyle(original.horizontalScrollbar);
			skin.horizontalScrollbarLeftButton = new GUIStyle(original.horizontalScrollbarLeftButton);
			skin.horizontalScrollbarRightButton = new GUIStyle(original.horizontalScrollbarRightButton);
			skin.horizontalScrollbarThumb = new GUIStyle(original.horizontalScrollbarThumb);
			skin.horizontalSlider = new GUIStyle(original.horizontalSlider);
			skin.horizontalSliderThumb = new GUIStyle(original.horizontalSliderThumb);
			skin.label = new GUIStyle(original.label);
			skin.scrollView = new GUIStyle(original.scrollView);
			skin.textArea = new GUIStyle(original.textArea);
			skin.textField = new GUIStyle(original.textField);
			skin.toggle = new GUIStyle(original.toggle);
			skin.verticalScrollbar = new GUIStyle(original.verticalScrollbar);
			skin.verticalScrollbarDownButton = new GUIStyle(original.verticalScrollbarDownButton);
			skin.verticalScrollbarThumb = new GUIStyle(original.verticalScrollbarThumb);
			skin.verticalScrollbarUpButton = new GUIStyle(original.verticalScrollbarUpButton);
			skin.verticalSlider = new GUIStyle(original.verticalSlider);
			skin.verticalSliderThumb = new GUIStyle(original.verticalSliderThumb);
			skin.window = new GUIStyle(original.window);
			skin.font = original.font;

			return skin;
		}

		/// <summary>
		/// Trigger theme texture creation.
		/// </summary>
		private static void CreateThemeTextures()
		{
			foreach (Theme theme in _themes.Data)
				theme.CreateTextures();
		}

		private static GUISkin CreateSkinForTheme(Theme theme)
		{
			GUISkin skin = CreateSkin(_baseSkin, theme.Name);

			// Create any required core textures.
			_black = GUIExtensions.ColorTexture(1, 1, new Color(0f, 0f, 0f));
			_blackHover = GUIExtensions.ColorTexture(1, 1, new Color(0.1f, 0.1f, 0.1f));;
			_grey = GUIExtensions.ColorTexture(1, 1, new Color(0.4f, 0.4f, 0.4f));
			_transparent = GUIExtensions.ColorTexture(1, 1, new Color(0, 0, 0, 0));
			_green = GUIExtensions.ColorTexture(1, 1, new Color(0.16f, 0.61f, 0));
			_orange = GUIExtensions.ColorTexture(1, 1, new Color(0.84f, 0.52f, 0));
			_red = GUIExtensions.ColorTexture(1, 1, new Color(1, 0, 0));
			_blue = GUIExtensions.ColorTexture(1, 1, new Color(0, 0.38f, 0.77f));

			// Override scrollbar width and height.
			skin.verticalScrollbar.fixedWidth = GUIRenderer.scrollWidth;
			skin.verticalScrollbarThumb.fixedWidth = GUIRenderer.scrollWidth;
			skin.horizontalScrollbar.fixedHeight = GUIRenderer.scrollWidth;
			skin.horizontalScrollbarThumb.fixedHeight = GUIRenderer.scrollWidth;

			// Button styling.
			_buttonStyle = new GUIStyle(skin.button);
			_buttonStyle.padding = new RectOffset(10, 10, 5, 5);

			GUIStyle buttonPrimary = new GUIStyle(_buttonStyle);
			buttonPrimary.name = "ButtonPrimary";
			buttonPrimary.normal.background = theme.ButtonPrimary;
			buttonPrimary.hover.background = theme.ButtonPrimaryHover;
			buttonPrimary.active.background = theme.ButtonPrimaryHover;
			buttonPrimary.focused.background = theme.ButtonPrimaryHover;
			buttonPrimary.normal.textColor = theme.ButtonPrimaryTextColour;
			buttonPrimary.hover.textColor = theme.ButtonPrimaryTextColour;
			buttonPrimary.active.textColor = theme.ButtonPrimaryTextColour;
			buttonPrimary.focused.textColor = theme.ButtonPrimaryTextColour;

			// Default to use primary button.
			skin.button = buttonPrimary;

			GUIStyle buttonPrimaryWrap = new GUIStyle(buttonPrimary);
			buttonPrimaryWrap.name = "ButtonPrimaryWrap";
			buttonPrimaryWrap.wordWrap = true;

			GUIStyle buttonPrimaryTextLeft = new GUIStyle(buttonPrimary);
			buttonPrimaryTextLeft.name = "ButtonPrimaryTextLeft";
			buttonPrimaryTextLeft.alignment = TextAnchor.MiddleLeft;

			GUIStyle buttonSecondary = new GUIStyle(_buttonStyle);
			buttonSecondary.name = "ButtonSecondary";
			buttonSecondary.normal.background = theme.ButtonSecondary;
			buttonSecondary.hover.background = theme.ButtonSecondaryHover;
			buttonSecondary.active.background = theme.ButtonSecondaryHover;
			buttonSecondary.focused.background = theme.ButtonSecondaryHover;
			buttonSecondary.normal.textColor = theme.ButtonSecondaryTextColour;
			buttonSecondary.hover.textColor = theme.ButtonSecondaryTextColour;
			buttonSecondary.active.textColor = theme.ButtonSecondaryTextColour;
			buttonSecondary.focused.textColor = theme.ButtonSecondaryTextColour;

			GUIStyle buttonBlack = new GUIStyle(_buttonStyle);
			buttonBlack.name = "ButtonBlack";
			buttonBlack.normal.background = _black;
			buttonBlack.hover.background = _blackHover;
			buttonBlack.active.background = _blackHover;
			buttonBlack.focused.background = _blackHover;

			GUIStyle buttonBlackTranslucent = new GUIStyle(_buttonStyle);
			buttonBlackTranslucent.name = "ButtonBlackTranslucent";
			buttonBlackTranslucent.normal.background = theme.Box;
			buttonBlackTranslucent.hover.background = theme.BoxHover;
			buttonBlackTranslucent.active.background = theme.BoxHover;
			buttonBlackTranslucent.focused.background = theme.BoxHover;
			buttonBlackTranslucent.normal.textColor = theme.TextColour;
			buttonBlackTranslucent.hover.textColor = theme.TextColour;
			buttonBlackTranslucent.active.textColor = theme.TextColour;
			buttonBlackTranslucent.focused.textColor = theme.TextColour;

			GUIStyle buttonTransparent = new GUIStyle(_buttonStyle);
			buttonTransparent.name = "ButtonTransparent";
			buttonTransparent.normal.background = null;
			buttonTransparent.hover.background = _transparent;
			buttonTransparent.active.background = _transparent;
			buttonTransparent.focused.background = _transparent;
			buttonTransparent.normal.textColor = theme.TextColour;
			buttonTransparent.hover.textColor = theme.TextColour;
			buttonTransparent.active.textColor = theme.TextColour;
			buttonTransparent.focused.textColor = theme.TextColour;
			buttonTransparent.wordWrap = true;
			buttonTransparent.alignment = TextAnchor.LowerCenter;

			// Box styling.
			skin.box.normal.background = theme.Box;

			// Label styling.
			GUIStyle labelHeader = new GUIStyle(skin.label);
			labelHeader.name = "LabelHeader";
			labelHeader.alignment = TextAnchor.MiddleLeft;
			labelHeader.fontSize = 24;
			labelHeader.fontStyle = FontStyle.Bold;
			labelHeader.normal.textColor = theme.TextColour;
			labelHeader.hover.textColor = theme.TextColour;
			labelHeader.active.textColor = theme.TextColour;
			labelHeader.focused.textColor = theme.TextColour;
			labelHeader.wordWrap = true;

			GUIStyle labelSubHeader = new GUIStyle(labelHeader);
			labelSubHeader.name = "LabelSubHeader";
			labelSubHeader.fontSize = 18;

			GUIStyle labelMessage = new GUIStyle(skin.label);
			labelMessage.name = "LabelMessage";
			labelMessage.alignment = TextAnchor.MiddleCenter;
			labelMessage.fontSize = 40;
			labelMessage.fontStyle = FontStyle.Bold;
			labelMessage.normal.textColor = theme.TextColour;
			labelMessage.hover.textColor = theme.TextColour;
			labelMessage.active.textColor = theme.TextColour;
			labelMessage.focused.textColor = theme.TextColour;
			labelMessage.wordWrap = true;

			GUIStyle labelCenter = new GUIStyle(skin.label);
			labelCenter.name = "LabelCenter";
			labelCenter.alignment = TextAnchor.MiddleCenter;
			labelMessage.normal.textColor = theme.TextColour;
			labelMessage.hover.textColor = theme.TextColour;
			labelMessage.active.textColor = theme.TextColour;
			labelMessage.focused.textColor = theme.TextColour;
			labelMessage.wordWrap = true;

			// Notification box styling.
			GUIStyle boxGrey = new GUIStyle(skin.box);
			boxGrey.name = "BoxGrey";
			boxGrey.normal.background = _grey;

			GUIStyle boxGreen = new GUIStyle(skin.box);
			boxGreen.name = "BoxGreen";
			boxGreen.normal.background = _green;

			GUIStyle boxOrange = new GUIStyle(skin.box);
			boxOrange.name = "BoxOrange";
			boxOrange.normal.background = _orange;

			GUIStyle boxRed = new GUIStyle(skin.box);
			boxRed.name = "BoxRed";
			boxRed.normal.background = _red;

			GUIStyle boxBlue = new GUIStyle(skin.box);
			boxBlue.name = "BoxBlue";
			boxBlue.normal.background = _blue;

			// Add custom styles.
			skin.customStyles = new GUIStyle[]
			{
				// Buttons.
				buttonPrimary,
				buttonPrimaryWrap,
				buttonPrimaryTextLeft,
				buttonSecondary,
				buttonBlack,
				buttonBlackTranslucent,
				buttonTransparent,

				// Labels.
				labelHeader,
				labelSubHeader,
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

			return skin;
		}

		/// <summary>
		/// Load the saved selected theme.
		/// </summary>
		private static void LoadSelectedTheme()
		{
			string name = MultiTool.Configuration.GetTheme("Greyscale");
			Theme theme = _themes.GetByName(name);
			// Selected theme doesn't exist, fallback to greyscale.
			if (theme == null)
			{
				MultiTool.Configuration.UpdateTheme("Greyscale");
				theme = _themes.GetByName("Greyscale");
			}

			SetActiveTheme(theme.Name);
		}
	}
}
