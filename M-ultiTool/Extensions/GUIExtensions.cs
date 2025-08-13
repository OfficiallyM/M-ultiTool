using UnityEngine;

namespace MultiTool.Extensions
{
	internal static class GUIExtensions
	{
        /// <summary>
        /// Draw GUI text with an outline.
        /// </summary>
        /// <param name="position">Text position</param>
        /// <param name="text">Text to draw</param>
        /// <param name="style">GUIStyle for text</param>
        /// <param name="outColor">Text outline color</param>
        public static void DrawOutline(Rect position, string text, GUIStyle style, Color outColor)
		{
			var backupStyle = style;
			var oldColor = style.normal.textColor;
			style.normal.textColor = outColor;
			position.x--;
			GUI.Label(position, text, style);
			position.x += 2;
			GUI.Label(position, text, style);
			position.x--;
			position.y--;
			GUI.Label(position, text, style);
			position.y += 2;
			GUI.Label(position, text, style);
			position.y--;
			style.normal.textColor = oldColor;
			GUI.Label(position, text, style);
			style = backupStyle;
		}

        /// <summary>
        /// Create a texture of a given color.
        /// </summary>
        /// <param name="width">Texture width</param>
        /// <param name="height">Texture height</param>
        /// <param name="color">Color to create texture of</param>
        /// <returns>A texture of provided dimensions and color</returns>
        public static Texture2D ColorTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
	}
}
