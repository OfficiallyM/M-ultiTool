using MultiTool.Save;
using MultiTool.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiTool.UI.Tabs.VehicleConfiguration
{
	internal sealed class LightChangerTab : UI.VehicleConfigurationTab
	{
		public override string Name => "Light Changer";
		public override bool HasCache => true;

		private Vector2 _position;

		private bool _lightSelectorOpen = false;
		private List<LightGroup> _selectedLights = new List<LightGroup>();
		private List<LightGroup> _lights = new List<LightGroup>();

		public override void OnCacheRefresh()
		{
			if (mainscript.M.player == null || mainscript.M.player.Car == null) return;

			_lights.Clear();
			GameObject carObject = mainscript.M.player.Car.gameObject;

			headlightscript[] headlights = carObject.GetComponentsInChildren<headlightscript>();
			if (headlights.Length > 0)
			{
				for (int i = 0; i < headlights.Length; i++)
				{
					headlightscript headlight = headlights[i];
					string name = $"{i + 1} - Headlight";
					bool isInterior = false;
					if (headlight.name.ToLower().Contains("interior") || headlight.transform.parent.name.ToLower().Contains("interior"))
					{
						name = $"{i + 1} - Interior light";
						isInterior = true;
					}
					_lights.Add(LightGroup.Create(name, headlight, isInterior));
				}
			}
		}

		public override void RenderTab(Rect dimensions)
		{
			GUILayout.BeginArea(dimensions);
			GUILayout.BeginVertical();
			_position = GUILayout.BeginScrollView(_position);

			carscript car = mainscript.M.player.Car;
			tosaveitemscript save = car.GetComponent<tosaveitemscript>();

			GUILayout.Label("Light changer", "LabelHeader");

			GUILayout.Label("Choose lights to alter", "LabelSubHeader");

			if (GUILayout.Button("Select", GUILayout.MaxWidth(200)))
				_lightSelectorOpen = !_lightSelectorOpen;

			GUILayout.Space(10);

			if (_lightSelectorOpen)
			{
				foreach (LightGroup light in _lights)
				{
					// Remove selected lights from selectable.
					if (_selectedLights.Where(l => l.Name == light.Name).FirstOrDefault() != null) continue;

					if (GUILayout.Button(PrettifyName(light.Name), GUILayout.MaxWidth(200)))
						_selectedLights.Add(light);
					GUILayout.Space(2);
				}
				GUILayout.Space(10);
			}

			GUILayout.Label("Selected lights", "LabelSubHeader");

			if (_selectedLights.Count == 0)
			{
				GUILayout.Button("Nothing selected", GUILayout.MaxWidth(200));
			}
			else
			{
				foreach (LightGroup light in _selectedLights)
				{
					if (GUILayout.Button(PrettifyName(light.Name), GUILayout.MaxWidth(200)))
					{
						_selectedLights.Remove(light);
						break;
					}
					GUILayout.Space(2);
				}
			}
			GUILayout.Space(10);

			Colour.RenderColourSliders(dimensions.width / 2);

			if (GUILayout.Button("Apply to selected", GUILayout.MaxWidth(200)))
			{
				foreach (LightGroup light in _selectedLights)
				{
					if (light.Headlights != null && light.Headlights.Count > 0)
					{
						foreach (headlightscript headlight in light.Headlights)
						{
							GameUtilities.SetHeadlightColor(headlight, Colour.GetColour(), light.IsInteriorLight);
							int? id = save.idInSave;
							if (!light.IsInteriorLight)
								id = headlight.GetComponent<tosaveitemscript>()?.idInSave;

							string name = null;
							if (light.IsInteriorLight)
								name = "interior";

							if (id.HasValue)
								SaveUtilities.UpdateLight(new LightData() { ID = id.Value, Name = name, Color = Colour.GetColour() });
						}
					}
				}
			}

			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		/// <summary>
		/// Make part name more user friendly.
		/// </summary>
		/// <param name="random">Part name to prettify</param>
		/// <returns>Prettified part name</returns>
		private string PrettifyName(string name)
		{
			return name.Replace("(Clone)", string.Empty);
		}
	}
}
