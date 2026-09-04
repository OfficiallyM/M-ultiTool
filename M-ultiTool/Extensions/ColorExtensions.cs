using System.Collections.Generic;
using UnityEngine;

namespace MultiTool.Extensions
{
	internal static class ColorExtensions
	{
		private static Dictionary<string, Color> _namedColors;

		/// <summary>
		/// Change brightness level of a color.
		/// </summary>
		/// <param name="color">Base color</param>
		/// <param name="factor">Brightness factor between -1 and 1</param>
		/// <returns></returns>
		public static Color ChangeBrightness(this Color color, float factor)
		{
			float red = color.r * 255;
			float green = color.g * 255;
			float blue = color.b * 255;

			if (factor < 0)
			{
				factor = 1 + factor;
				red *= factor;
				green *= factor;
				blue *= factor;
			}
			else
			{
				red = (255 - red) * factor + red;
				green = (255 - green) * factor + green;
				blue = (255 - blue) * factor + blue;
			}

			return new Color(red / 255, green / 255, blue / 255);
		}

		/// <summary>
		/// Get the name of a color.
		/// </summary>
		/// <param name="color">Color to name</param>
		/// <returns>Closest color name</returns>
		public static string GetName(this Color color)
		{
			EnsureNamedColorsPopulated();

			string closestName = "Unknown";
			float closestDistance = float.MaxValue;

			foreach (var pair in _namedColors)
			{
				float distance = ColorDistance(color, pair.Value);

				if (distance < closestDistance)
				{
					closestDistance = distance;
					closestName = pair.Key;
				}
			}

			return closestName;
		}

		/// <summary>
		/// Get Unity Color from RGB 0-255 values.
		/// </summary>
		/// <param name="r">Red</param>
		/// <param name="g">Green</param>
		/// <param name="b">Blue</param>
		/// <returns>Unity Color</returns>
		public static Color FromRgb255(int r, int g, int b)
		{
			return new Color(r / 255f, g / 255f, b / 255f);
		}

		private static float ColorDistance(Color a, Color b)
		{
			float dr = a.r - b.r;
			float dg = a.g - b.g;
			float db = a.b - b.b;

			return (dr * dr) + (dg * dg) + (db * db);
		}

		// CSS/X11 named colors, converted to Unity's 0-1 float range.
		private static void EnsureNamedColorsPopulated()
		{
			if (_namedColors != null)
			{
				return;
			}

			_namedColors = new Dictionary<string, Color>
			{
				{ "AliceBlue", FromRgb255(240, 248, 255) },
				{ "AntiqueWhite", FromRgb255(250, 235, 215) },
				{ "Aqua", FromRgb255(0, 255, 255) },
				{ "Aquamarine", FromRgb255(127, 255, 212) },
				{ "Azure", FromRgb255(240, 255, 255) },
				{ "Beige", FromRgb255(245, 245, 220) },
				{ "Bisque", FromRgb255(255, 228, 196) },
				{ "Black", FromRgb255(0, 0, 0) },
				{ "BlanchedAlmond", FromRgb255(255, 235, 205) },
				{ "Blue", FromRgb255(0, 0, 255) },
				{ "BlueViolet", FromRgb255(138, 43, 226) },
				{ "Brown", FromRgb255(165, 42, 42) },
				{ "BurlyWood", FromRgb255(222, 184, 135) },
				{ "CadetBlue", FromRgb255(95, 158, 160) },
				{ "Chartreuse", FromRgb255(127, 255, 0) },
				{ "Chocolate", FromRgb255(210, 105, 30) },
				{ "Coral", FromRgb255(255, 127, 80) },
				{ "CornflowerBlue", FromRgb255(100, 149, 237) },
				{ "Cornsilk", FromRgb255(255, 248, 220) },
				{ "Crimson", FromRgb255(220, 20, 60) },
				{ "Cyan", FromRgb255(0, 255, 255) },
				{ "DarkBlue", FromRgb255(0, 0, 139) },
				{ "DarkCyan", FromRgb255(0, 139, 139) },
				{ "DarkGoldenrod", FromRgb255(184, 134, 11) },
				{ "DarkGray", FromRgb255(169, 169, 169) },
				{ "DarkGreen", FromRgb255(0, 100, 0) },
				{ "DarkKhaki", FromRgb255(189, 183, 107) },
				{ "DarkMagenta", FromRgb255(139, 0, 139) },
				{ "DarkOliveGreen", FromRgb255(85, 107, 47) },
				{ "DarkOrange", FromRgb255(255, 140, 0) },
				{ "DarkOrchid", FromRgb255(153, 50, 204) },
				{ "DarkRed", FromRgb255(139, 0, 0) },
				{ "DarkSalmon", FromRgb255(233, 150, 122) },
				{ "DarkSeaGreen", FromRgb255(143, 188, 143) },
				{ "DarkSlateBlue", FromRgb255(72, 61, 139) },
				{ "DarkSlateGray", FromRgb255(47, 79, 79) },
				{ "DarkTurquoise", FromRgb255(0, 206, 209) },
				{ "DarkViolet", FromRgb255(148, 0, 211) },
				{ "DeepPink", FromRgb255(255, 20, 147) },
				{ "DeepSkyBlue", FromRgb255(0, 191, 255) },
				{ "DimGray", FromRgb255(105, 105, 105) },
				{ "DodgerBlue", FromRgb255(30, 144, 255) },
				{ "Firebrick", FromRgb255(178, 34, 34) },
				{ "FloralWhite", FromRgb255(255, 250, 240) },
				{ "ForestGreen", FromRgb255(34, 139, 34) },
				{ "Fuchsia", FromRgb255(255, 0, 255) },
				{ "Gainsboro", FromRgb255(220, 220, 220) },
				{ "GhostWhite", FromRgb255(248, 248, 255) },
				{ "Gold", FromRgb255(255, 215, 0) },
				{ "Goldenrod", FromRgb255(218, 165, 32) },
				{ "Gray", FromRgb255(128, 128, 128) },
				{ "Green", FromRgb255(0, 128, 0) },
				{ "GreenYellow", FromRgb255(173, 255, 47) },
				{ "Honeydew", FromRgb255(240, 255, 240) },
				{ "HotPink", FromRgb255(255, 105, 180) },
				{ "IndianRed", FromRgb255(205, 92, 92) },
				{ "Indigo", FromRgb255(75, 0, 130) },
				{ "Ivory", FromRgb255(255, 255, 240) },
				{ "Khaki", FromRgb255(240, 230, 140) },
				{ "Lavender", FromRgb255(230, 230, 250) },
				{ "LavenderBlush", FromRgb255(255, 240, 245) },
				{ "LawnGreen", FromRgb255(124, 252, 0) },
				{ "LemonChiffon", FromRgb255(255, 250, 205) },
				{ "LightBlue", FromRgb255(173, 216, 230) },
				{ "LightCoral", FromRgb255(240, 128, 128) },
				{ "LightCyan", FromRgb255(224, 255, 255) },
				{ "LightGoldenrodYellow", FromRgb255(250, 250, 210) },
				{ "LightGray", FromRgb255(211, 211, 211) },
				{ "LightGreen", FromRgb255(144, 238, 144) },
				{ "LightPink", FromRgb255(255, 182, 193) },
				{ "LightSalmon", FromRgb255(255, 160, 122) },
				{ "LightSeaGreen", FromRgb255(32, 178, 170) },
				{ "LightSkyBlue", FromRgb255(135, 206, 250) },
				{ "LightSlateGray", FromRgb255(119, 136, 153) },
				{ "LightSteelBlue", FromRgb255(176, 196, 222) },
				{ "LightYellow", FromRgb255(255, 255, 224) },
				{ "Lime", FromRgb255(0, 255, 0) },
				{ "LimeGreen", FromRgb255(50, 205, 50) },
				{ "Linen", FromRgb255(250, 240, 230) },
				{ "Magenta", FromRgb255(255, 0, 255) },
				{ "Maroon", FromRgb255(128, 0, 0) },
				{ "MediumAquamarine", FromRgb255(102, 205, 170) },
				{ "MediumBlue", FromRgb255(0, 0, 205) },
				{ "MediumOrchid", FromRgb255(186, 85, 211) },
				{ "MediumPurple", FromRgb255(147, 112, 219) },
				{ "MediumSeaGreen", FromRgb255(60, 179, 113) },
				{ "MediumSlateBlue", FromRgb255(123, 104, 238) },
				{ "MediumSpringGreen", FromRgb255(0, 250, 154) },
				{ "MediumTurquoise", FromRgb255(72, 209, 204) },
				{ "MediumVioletRed", FromRgb255(199, 21, 133) },
				{ "MidnightBlue", FromRgb255(25, 25, 112) },
				{ "MintCream", FromRgb255(245, 255, 250) },
				{ "MistyRose", FromRgb255(255, 228, 225) },
				{ "Moccasin", FromRgb255(255, 228, 181) },
				{ "NavajoWhite", FromRgb255(255, 222, 173) },
				{ "Navy", FromRgb255(0, 0, 128) },
				{ "OldLace", FromRgb255(253, 245, 230) },
				{ "Olive", FromRgb255(128, 128, 0) },
				{ "OliveDrab", FromRgb255(107, 142, 35) },
				{ "Orange", FromRgb255(255, 165, 0) },
				{ "OrangeRed", FromRgb255(255, 69, 0) },
				{ "Orchid", FromRgb255(218, 112, 214) },
				{ "PaleGoldenrod", FromRgb255(238, 232, 170) },
				{ "PaleGreen", FromRgb255(152, 251, 152) },
				{ "PaleTurquoise", FromRgb255(175, 238, 238) },
				{ "PaleVioletRed", FromRgb255(219, 112, 147) },
				{ "PapayaWhip", FromRgb255(255, 239, 213) },
				{ "PeachPuff", FromRgb255(255, 218, 185) },
				{ "Peru", FromRgb255(205, 133, 63) },
				{ "Pink", FromRgb255(255, 192, 203) },
				{ "Plum", FromRgb255(221, 160, 221) },
				{ "PowderBlue", FromRgb255(176, 224, 230) },
				{ "Purple", FromRgb255(128, 0, 128) },
				{ "Red", FromRgb255(255, 0, 0) },
				{ "RosyBrown", FromRgb255(188, 143, 143) },
				{ "RoyalBlue", FromRgb255(65, 105, 225) },
				{ "SaddleBrown", FromRgb255(139, 69, 19) },
				{ "Salmon", FromRgb255(250, 128, 114) },
				{ "SandyBrown", FromRgb255(244, 164, 96) },
				{ "SeaGreen", FromRgb255(46, 139, 87) },
				{ "SeaShell", FromRgb255(255, 245, 238) },
				{ "Sienna", FromRgb255(160, 82, 45) },
				{ "Silver", FromRgb255(192, 192, 192) },
				{ "SkyBlue", FromRgb255(135, 206, 235) },
				{ "SlateBlue", FromRgb255(106, 90, 205) },
				{ "SlateGray", FromRgb255(112, 128, 144) },
				{ "Snow", FromRgb255(255, 250, 250) },
				{ "SpringGreen", FromRgb255(0, 255, 127) },
				{ "SteelBlue", FromRgb255(70, 130, 180) },
				{ "Tan", FromRgb255(210, 180, 140) },
				{ "Teal", FromRgb255(0, 128, 128) },
				{ "Thistle", FromRgb255(216, 191, 216) },
				{ "Tomato", FromRgb255(255, 99, 71) },
				{ "Turquoise", FromRgb255(64, 224, 208) },
				{ "Violet", FromRgb255(238, 130, 238) },
				{ "Wheat", FromRgb255(245, 222, 179) },
				{ "White", FromRgb255(255, 255, 255) },
				{ "WhiteSmoke", FromRgb255(245, 245, 245) },
				{ "Yellow", FromRgb255(255, 255, 0) },
				{ "YellowGreen", FromRgb255(154, 205, 50) },
			};
		}
	}
}
