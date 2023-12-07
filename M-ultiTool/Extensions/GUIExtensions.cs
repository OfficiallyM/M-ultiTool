using UnityEngine;

namespace MultiTool.Extensions
{
	internal static class GUIExtensions
	{
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
	}
}
