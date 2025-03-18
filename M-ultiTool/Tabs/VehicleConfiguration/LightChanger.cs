using MultiTool.Core;
using MultiTool.Modules;
using MultiTool.Utilities.UI;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiTool.Tabs.VehicleConfiguration
{
    internal sealed class LightChanger : Core.VehicleConfigurationTab
	{
        public override string Name => "Light Changer";

        private Vector2 _position;

		private bool _lightSelectorOpen = false;
		private List<LightGroup> _selectedLights = new List<LightGroup>();
		private List<LightGroup> lights = new List<LightGroup>();

		public override void OnCacheRefresh()
		{
			lights.Clear();
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
					lights.Add(LightGroup.Create(name, headlight, isInterior));
				}
			}
		}

		public override void RenderTab(Rect dimensions)
        {
			dimensions.width /= 2;
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
				foreach (LightGroup light in lights)
				{
					// Remove selected lights from selectable.
					if (_selectedLights.Where(l => l.name == light.name).FirstOrDefault() != null) continue;

					if (GUILayout.Button(PrettifyName(light.name), GUILayout.MaxWidth(200)))
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
					if (GUILayout.Button(PrettifyName(light.name), GUILayout.MaxWidth(200)))
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
					if (light.headlights != null && light.headlights.Count > 0)
					{
						foreach (headlightscript headlight in light.headlights)
						{
							GameUtilities.SetHeadlightColor(headlight, Colour.GetColour(), light.isInteriorLight);
							int? id = save.idInSave;
							if (!light.isInteriorLight)
								id = headlight.GetComponent<tosaveitemscript>()?.idInSave;

							string name = null;
							if (light.isInteriorLight)
								name = "interior";

							if (id.HasValue)
								SaveUtilities.UpdateLight(new LightData() { ID = id.Value, name = name, color = Colour.GetColour() });
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
