using MultiTool.Extensions;
using MultiTool.Save;
using MultiTool.Save.Records;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.UI.Tabs.VehicleConfiguration
{
	internal sealed class GlassTab : UI.VehicleConfigurationTab
	{
		public override string Name => "Glass";

		private Vector2 _position;
		private Color _color;
		private float _innerGlassAlpha = 0.2f;
		private Color _sunroofColor;

		public override void RenderTab(Rect dimensions)
		{
			GUILayout.BeginArea(dimensions);
			GUILayout.BeginVertical();
			_position = GUILayout.BeginScrollView(_position);

			carscript car = mainscript.M.player.Car;
			tosaveitemscript save = car.GetComponent<tosaveitemscript>();
			Transform sunRoofSlot = car.transform.FindRecursive("SunRoofSlot");

			GUILayout.Label("Window settings", "LabelHeader");

			Color newColor = Colour.RenderColourSliders(dimensions.width / 2, _color, true);
			if (newColor.a != _color.a)
			{
				_innerGlassAlpha = newColor.a;
				if (newColor.a > 0.2f)
					_innerGlassAlpha = 0.2f;
			}
			_color = newColor;

			// Alpha.
			GUILayout.Label("Inner glass alpha", "LabelSubHeader");
			GUILayout.Label("Alpha:");
			float alpha = GUILayout.HorizontalSlider(_innerGlassAlpha * 255, 0, 255);
			alpha = Mathf.Round(alpha);
			bool alphaParse = float.TryParse(GUILayout.TextField(alpha.ToString()), out alpha);
			if (!alphaParse)
				Logger.Log($"{alphaParse} is not a number", Logger.LogLevel.Error);
			alpha = Mathf.Clamp(alpha, 0f, 255f);
			_innerGlassAlpha = alpha / 255f;

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Randomise colour", GUILayout.MaxWidth(200)))
			{
				_color.r = UnityEngine.Random.Range(0f, 255f) / 255f;
				_color.g = UnityEngine.Random.Range(0f, 255f) / 255f;
				_color.b = UnityEngine.Random.Range(0f, 255f) / 255f;
			}

			GUILayout.Space(10);

			if (GUILayout.Button("Apply", GUILayout.MaxWidth(200)))
			{
				List<MeshRenderer> renderers = car.GetComponentsInChildren<MeshRenderer>().ToList();
				foreach (MeshRenderer meshRenderer in renderers)
				{
					string materialName = meshRenderer.material.name.Replace(" (Instance)", "");
					switch (materialName)
					{
						// Outer glass.
						case "Glass":
							// Use selected colour.
							meshRenderer.material.color = _color;
							break;

						// Inner glass.
						case "GlassNoReflection":
							Color innerColor = _color;
							innerColor.a = _innerGlassAlpha;
							meshRenderer.material.color = innerColor;
							break;
					}
				}

				GlassRecord record = new GlassRecord { ID = save.idInSave, Color = _color, InnerAlpha = _innerGlassAlpha, Type = "windows" };
				SaveRepository.Upsert(record, r => r.ID == record.ID && r.Type == record.Type);
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(10);

			// Sunroof settings.
			if (sunRoofSlot != null)
			{
				GUILayout.Label("Sunroof settings", "LabelHeader");

				Transform outerGlass = sunRoofSlot.FindRecursive("sunroof outer glass", exact: false);
				if (outerGlass != null)
				{
					MeshRenderer meshRenderer = outerGlass.GetComponent<MeshRenderer>();

					_sunroofColor = Colour.RenderColourSliders(dimensions.width / 2, _sunroofColor, true);

					GUILayout.BeginHorizontal();
					if (GUILayout.Button("Randomise colour", GUILayout.MaxWidth(200)))
					{
						_sunroofColor.r = UnityEngine.Random.Range(0f, 255f) / 255f;
						_sunroofColor.g = UnityEngine.Random.Range(0f, 255f) / 255f;
						_sunroofColor.b = UnityEngine.Random.Range(0f, 255f) / 255f;
					}

					GUILayout.Space(10);

					if (GUILayout.Button("Apply", GUILayout.MaxWidth(200)))
					{
						meshRenderer.material.color = _sunroofColor;

						GlassRecord record = new GlassRecord { ID = save.idInSave, Color = _sunroofColor, Type = "sunroof" };
						SaveRepository.Upsert(record, r => r.ID == record.ID && r.Type == record.Type);
					}
					GUILayout.EndHorizontal();
				}
				else
					GUILayout.Label("No sunroof mounted.");
			}

			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}
}
