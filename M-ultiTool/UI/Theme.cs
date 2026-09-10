using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace MultiTool.UI
{
	internal class Theme
	{
		public string Name { get; set; }
		public bool IsCore { get; set; } = false;

		public Color ButtonPrimaryColour { get; set; } = Color.white;
		public Color ButtonPrimaryHoverColour { get; set; } = Color.grey;
		public Color ButtonSecondaryColour { get; set; } = Color.white;
		public Color ButtonSecondaryHoverColour { get; set; } = Color.grey;
		public Color BoxColour { get; set; } = new Color(0, 0, 0, 0.4f);
		public Color BoxHoverColour { get; set; } = new Color(0, 0, 0, 0.5f);

		public Color ButtonPrimaryTextColour { get; set; } = Color.black;
		public Color ButtonSecondaryTextColour { get; set; } = Color.black;
		public Color TextColour { get; set; } = Color.white;
		public Color AccessibilityOnColour { get; set; } = Color.green;
		public Color AccessibilityOffColour { get; set; } = Color.red;

		[JsonIgnore]
		public Texture2D ButtonPrimary { get; set; }
		[JsonIgnore]
		public Texture2D ButtonPrimaryHover { get; set; }
		[JsonIgnore]
		public Texture2D ButtonSecondary { get; set; }
		[JsonIgnore]
		public Texture2D ButtonSecondaryHover { get; set; }
		[JsonIgnore]
		public Texture2D Box { get; set; }
		[JsonIgnore]
		public Texture2D BoxHover { get; set; }

		public void CreateTextures()
		{
			ButtonPrimary = GUIExtensions.ColorTexture(1, 1, ButtonPrimaryColour);
			ButtonPrimaryHover = GUIExtensions.ColorTexture(1, 1, ButtonPrimaryHoverColour);
			ButtonSecondary = GUIExtensions.ColorTexture(1, 1, ButtonSecondaryColour);
			ButtonSecondaryHover = GUIExtensions.ColorTexture(1, 1, ButtonSecondaryHoverColour);
			Box = GUIExtensions.ColorTexture(1, 1, BoxColour);
			BoxHover = GUIExtensions.ColorTexture(1, 1, BoxHoverColour);
		}
	}

	internal class Themes
	{
		public List<Theme> Data { get; set; }

		public Themes()
		{
			Data = new List<Theme>();
		}

		/// <summary>
		/// Add a new theme.
		/// </summary>
		/// <param name="theme">Theme to add</param>
		public void Add(Theme theme)
			=> Data.Add(theme);

		/// <summary>
		/// Remove a theme.
		/// </summary>
		/// <param name="theme">Theme to remove</param>
		public void Remove(Theme theme)
			=> Data.Remove(theme);

		/// <summary>
		/// Get a theme by name.
		/// </summary>
		/// <param name="name">Theme name</param>
		/// <returns>Theme if found, otherwise null</returns>
		public Theme GetByName(string name)
		{
			foreach (Theme theme in Data)
				if (theme.Name == name) return theme;

			return null;
		}

		/// <summary>
		/// Get an array of all available theme names.
		/// </summary>
		/// <returns>Array of all theme names</returns>
		public string[] GetThemeNames()
		{
			List<string> names = new List<string>();
			foreach (Theme theme in Data)
				names.Add(theme.Name);

			return names.ToArray();
		}
	}
}
