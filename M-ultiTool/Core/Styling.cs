﻿using MultiTool.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiTool.Core
{
	[DataContract]
	internal class Theme
	{
		[DataMember] public string Name { get; set; }
		[DataMember] public bool IsCore { get; set; } = false;

		[DataMember] public Color ButtonPrimaryColour { get; set; } = Color.white;
		[DataMember] public Color ButtonPrimaryHoverColour { get; set; } = Color.grey;
		[DataMember] public Color ButtonSecondaryColour { get; set; } = Color.white;
		[DataMember] public Color ButtonSecondaryHoverColour { get; set; } = Color.grey;
		[DataMember] public Color BoxColour { get; set; } = new Color(0, 0, 0, 0.4f);
		[DataMember] public Color BoxHoverColour { get; set; } = new Color(0, 0, 0, 0.5f);

		[DataMember] public Color ButtonPrimaryTextColour { get; set; } = Color.black;
		[DataMember] public Color ButtonSecondaryTextColour { get; set; } = Color.black;
		[DataMember] public Color TextColour { get; set; } = Color.white;
		[DataMember] public Color AccessibilityOnColour { get; set; } = Color.green;
		[DataMember] public Color AccessibilityOffColour { get; set; } = Color.red;

		public Texture2D ButtonPrimary { get; set; }
		public Texture2D ButtonPrimaryHover { get; set; }
		public Texture2D ButtonSecondary { get; set; }
		public Texture2D ButtonSecondaryHover { get; set; }
		public Texture2D Box { get; set; }
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

	[DataContract]
	internal class Themes
	{
		[DataMember] public List<Theme> Data { get; set; }

		public Themes()
		{
			Data = new List<Theme>();
		}

		/// <summary>
		/// Add a new theme.
		/// </summary>
		/// <param name="theme">Theme to add</param>
		public void Add(Theme theme)
		{
			Data.Add(theme);
		}

		/// <summary>
		/// Remove a theme.
		/// </summary>
		/// <param name="theme">Theme to remove</param>
		public void Remove(Theme theme)
		{
			Data.Remove(theme);
		}

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
