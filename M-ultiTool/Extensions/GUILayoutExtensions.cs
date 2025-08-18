using System;
using System.Collections.Generic;
using UnityEngine;

namespace MultiTool.Extensions
{
	internal static class GUILayoutExtensions
	{
		/// <summary>
		/// Renders a float input field.
		/// </summary>
		/// <param name="value">The current float value</param>
		/// <param name="label">Optional label to display above the field</param>
		/// <param name="maxWidth">Optional maximum width for the field</param>
		/// <returns>Float value, or the original if parsing fails</returns>
		public static float FloatField(float value, string label = null, float? maxWidth = null, string precision = "F4", float? min = null, float? max = null)
		{
			GUILayout.BeginVertical();

			if (!string.IsNullOrEmpty(label))
				GUILayout.Label(label, maxWidth != null ? GUILayout.MaxWidth(maxWidth.Value) : GUILayout.ExpandWidth(true));

			string input = GUILayout.TextField(value.ToString(precision), maxWidth != null ? GUILayout.MaxWidth(maxWidth.Value) : GUILayout.ExpandWidth(true));

			GUILayout.EndVertical();

			// Don't reset when entering minus numbers.
			if (input == "-") return value;

			if (float.TryParse(input, out float parsed))
			{
				if (min.HasValue) parsed = Mathf.Max(min.Value, parsed);
				if (max.HasValue) parsed = Mathf.Min(max.Value, parsed);
				return parsed;
			}

			return value;
		}

		/// <summary>
		/// Renders two float input fields.
		/// </summary>
		/// <param name="value">The current Vector2 value</param>
		/// <param name="label">Optional label to display above the fields</param>
		/// <param name="maxWidth">Optional maximum width for each axis field</param>
		/// <param name="stepSize">Optional step size for -/+ buttons. If null, those buttons won't render</param>
		/// <returns>Vector2</returns>
		public static Vector2 Vector2Field(Vector2 value, string label = null, float? maxWidth = null, float? stepSize = null)
		{
			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();
			if (!string.IsNullOrEmpty(label))
			{
				GUILayout.Label(label, GUILayout.ExpandWidth(false));
				GUILayout.Space(5);
			}

			if (GUILayout.Button("Copy", GUILayout.ExpandWidth(false)))
				GUIUtility.systemCopyBuffer = $"{value.x}, {value.y}";

			if (GUILayout.Button("Paste", GUILayout.ExpandWidth(false)))
			{
				var parts = GUIUtility.systemCopyBuffer.Split(',');
				if (parts.Length == 2 && float.TryParse(parts[0], out var x) && float.TryParse(parts[1], out var y))
					value = new Vector2(x, y);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();

			GUILayout.BeginVertical();
			value.x = FloatField(value.x, "X", maxWidth);
			if (stepSize != null)
			{
				GUILayout.BeginHorizontal(maxWidth != null ? GUILayout.MaxWidth(maxWidth.Value) : GUILayout.ExpandWidth(true));
				if (GUILayout.Button("-", GUILayout.MaxWidth(30)))
					value.x -= stepSize.Value;
				if (GUILayout.Button("+", GUILayout.MaxWidth(30)))
					value.x += stepSize.Value;
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

			GUILayout.BeginVertical();
			value.y = FloatField(value.y, "Y", maxWidth);
			if (stepSize != null)
			{
				GUILayout.BeginHorizontal(maxWidth != null ? GUILayout.MaxWidth(maxWidth.Value) : GUILayout.ExpandWidth(true));
				if (GUILayout.Button("-", GUILayout.MaxWidth(30)))
					value.y -= stepSize.Value;
				if (GUILayout.Button("+", GUILayout.MaxWidth(30)))
					value.y += stepSize.Value;
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			return value;
		}

		/// <summary>
		/// Renders three float input fields.
		/// </summary>
		/// <param name="value">The current Vector3 value</param>
		/// <param name="label">Optional label to display above the fields</param>
		/// <param name="maxWidth">Optional maximum width for each axis field</param>
		/// <param name="stepSize">Optional step size for -/+ buttons. If null, those buttons won't render</param>
		/// <returns>Vector3</returns>
		public static Vector3 Vector3Field(Vector3 value, string label = null, float? maxWidth = null, float? stepSize = null)
		{
			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();
			if (!string.IsNullOrEmpty(label))
			{
				GUILayout.Label(label, GUILayout.ExpandWidth(false));
				GUILayout.Space(5);
			}

			if (GUILayout.Button("Copy", GUILayout.ExpandWidth(false)))
				GUIUtility.systemCopyBuffer = $"{value.x}, {value.y}, {value.z}";

			if (GUILayout.Button("Paste", GUILayout.ExpandWidth(false)))
			{
				var parts = GUIUtility.systemCopyBuffer.Split(',');
				if (parts.Length == 3 && float.TryParse(parts[0], out var x) && float.TryParse(parts[1], out var y) && float.TryParse(parts[2], out var z))
					value = new Vector3(x, y, z);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();

			GUILayout.BeginVertical();
			value.x = FloatField(value.x, "X", maxWidth);
			if (stepSize != null)
			{
				GUILayout.BeginHorizontal(maxWidth != null ? GUILayout.MaxWidth(maxWidth.Value) : GUILayout.ExpandWidth(true));
				if (GUILayout.Button("-", GUILayout.MaxWidth(30)))
					value.x -= stepSize.Value;
				if (GUILayout.Button("+", GUILayout.MaxWidth(30)))
					value.x += stepSize.Value;
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
			GUILayout.BeginVertical();
			value.y = FloatField(value.y, "Y", maxWidth);
			if (stepSize != null)
			{
				GUILayout.BeginHorizontal(maxWidth != null ? GUILayout.MaxWidth(maxWidth.Value) : GUILayout.ExpandWidth(true));
				if (GUILayout.Button("-", GUILayout.MaxWidth(30)))
					value.y -= stepSize.Value;
				if (GUILayout.Button("+", GUILayout.MaxWidth(30)))
					value.y += stepSize.Value;
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
			GUILayout.BeginVertical();
			value.z = FloatField(value.z, "Z", maxWidth);
			if (stepSize != null)
			{
				GUILayout.BeginHorizontal(maxWidth != null ? GUILayout.MaxWidth(maxWidth.Value) : GUILayout.ExpandWidth(true));
				if (GUILayout.Button("-", GUILayout.MaxWidth(30)))
					value.z -= stepSize.Value;
				if (GUILayout.Button("+", GUILayout.MaxWidth(30)))
					value.z += stepSize.Value;
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			return value;
		}

		/// <summary>
		/// Renders four float input fields.
		/// </summary>
		/// <param name="value">The current Vector4 value</param>
		/// <param name="label">Optional label to display above the fields</param>
		/// <param name="maxWidth">Optional maximum width for each axis field</param>
		/// <param name="stepSize">Optional step size for -/+ buttons. If null, those buttons won't render</param>
		/// <returns>Vector4</returns>
		public static Vector4 Vector4Field(Vector4 value, string label = null, float? maxWidth = null, float? stepSize = null)
		{
			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();
			if (!string.IsNullOrEmpty(label))
			{
				GUILayout.Label(label, GUILayout.ExpandWidth(false));
				GUILayout.Space(5);
			}

			if (GUILayout.Button("Copy", GUILayout.ExpandWidth(false)))
				GUIUtility.systemCopyBuffer = $"{value.x}, {value.y}, {value.z}, {value.w}";

			if (GUILayout.Button("Paste", GUILayout.ExpandWidth(false)))
			{
				var parts = GUIUtility.systemCopyBuffer.Split(',');
				if (parts.Length == 4 && float.TryParse(parts[0], out var x) && float.TryParse(parts[1], out var y) && float.TryParse(parts[2], out var z) && float.TryParse(parts[3], out var w))
					value = new Vector4(x, y, z, w);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();

			GUILayout.BeginVertical();
			value.x = FloatField(value.x, "X", maxWidth);
			if (stepSize != null)
			{
				GUILayout.BeginHorizontal(maxWidth != null ? GUILayout.MaxWidth(maxWidth.Value) : GUILayout.ExpandWidth(true));
				if (GUILayout.Button("-", GUILayout.MaxWidth(30)))
					value.x -= stepSize.Value;
				if (GUILayout.Button("+", GUILayout.MaxWidth(30)))
					value.x += stepSize.Value;
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

			GUILayout.BeginVertical();
			value.y = FloatField(value.y, "Y", maxWidth);
			if (stepSize != null)
			{
				GUILayout.BeginHorizontal(maxWidth != null ? GUILayout.MaxWidth(maxWidth.Value) : GUILayout.ExpandWidth(true));
				if (GUILayout.Button("-", GUILayout.MaxWidth(30)))
					value.y -= stepSize.Value;
				if (GUILayout.Button("+", GUILayout.MaxWidth(30)))
					value.y += stepSize.Value;
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

			GUILayout.BeginVertical();
			value.z = FloatField(value.z, "Z", maxWidth);
			if (stepSize != null)
			{
				GUILayout.BeginHorizontal(maxWidth != null ? GUILayout.MaxWidth(maxWidth.Value) : GUILayout.ExpandWidth(true));
				if (GUILayout.Button("-", GUILayout.MaxWidth(30)))
					value.z -= stepSize.Value;
				if (GUILayout.Button("+", GUILayout.MaxWidth(30)))
					value.z += stepSize.Value;
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

			GUILayout.BeginVertical();
			value.w = FloatField(value.w, "W", maxWidth);
			if (stepSize != null)
			{
				GUILayout.BeginHorizontal(maxWidth != null ? GUILayout.MaxWidth(maxWidth.Value) : GUILayout.ExpandWidth(true));
				if (GUILayout.Button("-", GUILayout.MaxWidth(30)))
					value.w -= stepSize.Value;
				if (GUILayout.Button("+", GUILayout.MaxWidth(30)))
					value.w += stepSize.Value;
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			return value;
		}

		/// <summary>
		/// Renders four float input fields for a Quaternion.
		/// </summary>
		/// <param name="value">The current Quaternion value</param>
		/// <param name="label">Optional label to display above the fields</param>
		/// <param name="maxWidth">Optional maximum width for each axis field</param>
		/// <param name="stepSize">Optional step size for -/+ buttons. If null, those buttons won't render</param>
		/// <returns>Quaternion</returns>
		public static Quaternion QuaternionField(Quaternion value, string label = null, float? maxWidth = null, float? stepSize = null)
		{
			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();
			if (!string.IsNullOrEmpty(label))
			{
				GUILayout.Label(label, GUILayout.ExpandWidth(false));
				GUILayout.Space(5);
			}

			if (GUILayout.Button("Copy", GUILayout.ExpandWidth(false)))
				GUIUtility.systemCopyBuffer = $"{value.x}, {value.y}, {value.z}, {value.w}";

			if (GUILayout.Button("Paste", GUILayout.ExpandWidth(false)))
			{
				var parts = GUIUtility.systemCopyBuffer.Split(',');
				if (parts.Length == 4 && float.TryParse(parts[0], out var x) && float.TryParse(parts[1], out var y) && float.TryParse(parts[2], out var z) && float.TryParse(parts[3], out var w))
					value = new Quaternion(x, y, z, w);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();

			GUILayout.BeginVertical();
			value.x = FloatField(value.x, "X", maxWidth);
			if (stepSize != null)
			{
				GUILayout.BeginHorizontal(maxWidth != null ? GUILayout.MaxWidth(maxWidth.Value) : GUILayout.ExpandWidth(true));
				if (GUILayout.Button("-", GUILayout.MaxWidth(30)))
					value.x -= stepSize.Value;
				if (GUILayout.Button("+", GUILayout.MaxWidth(30)))
					value.x += stepSize.Value;
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

			GUILayout.BeginVertical();
			value.y = FloatField(value.y, "Y", maxWidth);
			if (stepSize != null)
			{
				GUILayout.BeginHorizontal(maxWidth != null ? GUILayout.MaxWidth(maxWidth.Value) : GUILayout.ExpandWidth(true));
				if (GUILayout.Button("-", GUILayout.MaxWidth(30)))
					value.y -= stepSize.Value;
				if (GUILayout.Button("+", GUILayout.MaxWidth(30)))
					value.y += stepSize.Value;
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

			GUILayout.BeginVertical();
			value.z = FloatField(value.z, "Z", maxWidth);
			if (stepSize != null)
			{
				GUILayout.BeginHorizontal(maxWidth != null ? GUILayout.MaxWidth(maxWidth.Value) : GUILayout.ExpandWidth(true));
				if (GUILayout.Button("-", GUILayout.MaxWidth(30)))
					value.z -= stepSize.Value;
				if (GUILayout.Button("+", GUILayout.MaxWidth(30)))
					value.z += stepSize.Value;
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

			GUILayout.BeginVertical();
			value.w = FloatField(value.w, "W", maxWidth);
			if (stepSize != null)
			{
				GUILayout.BeginHorizontal(maxWidth != null ? GUILayout.MaxWidth(maxWidth.Value) : GUILayout.ExpandWidth(true));
				if (GUILayout.Button("-", GUILayout.MaxWidth(30)))
					value.w -= stepSize.Value;
				if (GUILayout.Button("+", GUILayout.MaxWidth(30)))
					value.w += stepSize.Value;
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			return value;
		}

		/// <summary>
		/// Renders four float input fields.
		/// </summary>
		/// <param name="value">The current color value</param>
		/// <param name="label">Optional label to display above the fields</param>
		/// <param name="maxWidth">Optional maximum width for each axis field</param>
		/// <returns>Color</returns>
		public static Color ColorField(Color value, string label = null, float? maxWidth = null)
		{
			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();
			if (!string.IsNullOrEmpty(label))
			{
				GUILayout.Label(label, GUILayout.ExpandWidth(false));
				GUILayout.Space(5);
			}

			if (GUILayout.Button("Copy", GUILayout.ExpandWidth(false)))
				GUIUtility.systemCopyBuffer = $"{value.r}, {value.g}, {value.b}, {value.a}";

			if (GUILayout.Button("Paste", GUILayout.ExpandWidth(false)))
			{
				var parts = GUIUtility.systemCopyBuffer.Split(',');
				if (parts.Length == 4 && float.TryParse(parts[0], out var r) && float.TryParse(parts[1], out var g) && float.TryParse(parts[2], out var b) && float.TryParse(parts[3], out var a))
					value = new Color(r, g, b, a);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();

			value.r = FloatField(value.r * 255f, "R", maxWidth, "F2", 0, 255) / 255f;
			value.g = FloatField(value.g * 255f, "G", maxWidth, "F2", 0, 255) / 255f;
			value.b = FloatField(value.b * 255f, "B", maxWidth, "F2", 0, 255) / 255f;
			value.a = FloatField(value.a * 255f, "A", maxWidth, "F2", 0, 255) / 255f;

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			return value;
		}

		/// <summary>
		/// Render inputs for a transform.
		/// </summary>
		/// <param name="transform">The transform</param>
		/// <param name="maxWidth">Optional maximum width for each field</param>
		/// <param name="stepSize">Optional step size for -/+ buttons. If null, those buttons won't render</param>
		/// <returns></returns>
		public static Transform RenderTransform(Transform transform, float? maxWidth = null, float? stepSize = null)
		{
			transform.position = Vector3Field(transform.position, "World position", maxWidth, stepSize);
			GUILayout.Space(5);
			transform.localPosition = Vector3Field(transform.localPosition, "Local position", maxWidth, stepSize);
			GUILayout.Space(5);
			transform.localEulerAngles = Vector3Field(transform.localEulerAngles, "Local rotation", maxWidth, stepSize);
			GUILayout.Space(5);
			transform.localScale = Vector3Field(transform.localScale, "Scale", maxWidth, stepSize);
			return transform;
		}

		/// <summary>
		/// Renders an enum field as a set of buttons.
		/// </summary>
		/// <param name="value">Current enum value</param>
		/// <param name="label">Optional label above the buttons</param>
		/// <returns>New enum value</returns>
		public static Enum EnumField(Enum value, string label = null)
		{
			GUILayout.BeginVertical();

			if (!string.IsNullOrEmpty(label))
				GUILayout.Label(label, GUILayout.ExpandWidth(false));

			var values = Enum.GetValues(value.GetType());

			foreach (Enum option in values)
			{
				bool isSelected = value.Equals(option);
				if (GUILayout.Toggle(isSelected, option.ToString()))
				{
					value = option;
				}
			}

			GUILayout.EndVertical();

			return value;
		}
	}
}
