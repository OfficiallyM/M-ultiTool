using MultiTool.Extensions;
using MultiTool.Save;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiTool.UI.Tabs.VehicleConfiguration
{
	internal sealed class GlassTab : UI.VehicleConfigurationTab
	{
		public override string Name => "Glass";

		private Vector2 _position;
		private Color _color;

		public override void RenderTab(Rect dimensions)
		{
			GUILayout.BeginArea(dimensions);
			GUILayout.BeginVertical();
			_position = GUILayout.BeginScrollView(_position);

			carscript car = mainscript.M.player.Car;
			tosaveitemscript save = car.GetComponent<tosaveitemscript>();
			Transform sunRoofSlot = car.transform.FindRecursive("SunRoofSlot");

			GUILayout.Label("Window settings", "LabelHeader");

			_color = Colour.RenderColourSliders(dimensions.width / 2, _color, true);

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
						// TODO: Add setting for inner glass alpha.
						case "GlassNoReflection":
							// Use a more transparent version of the selected colour
							// for the inner glass to ensure it's still see-through.
							Color innerColor = _color;
							if (innerColor.a > 0.2f)
								innerColor.a = 0.2f;
							meshRenderer.material.color = innerColor;
							break;
					}
				}

				SaveUtilities.UpdateGlass(new GlassData() { ID = save.idInSave, Color = _color, Type = "windows" });
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

					_color = Colour.RenderColourSliders(dimensions.width / 2, _color, true);

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
						meshRenderer.material.color = _color;

						SaveUtilities.UpdateGlass(new GlassData() { ID = save.idInSave, Color = _color, Type = "sunroof" });
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
