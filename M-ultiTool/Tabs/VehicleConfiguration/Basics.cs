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
    internal sealed class Basics : Core.VehicleConfigurationTab
	{
        public override string Name => "Basics";

        private Settings _settings = new Settings();
        private Vector2 _position;

		public override void RenderTab(Rect dimensions)
        {
            GUILayout.BeginArea(dimensions);
            GUILayout.BeginVertical();
            _position = GUILayout.BeginScrollView(_position);

			carscript car = mainscript.M.player.Car;
			partconditionscript partconditionscript = car.gameObject.GetComponent<partconditionscript>();

			// Vehicle god mode.
			if (GUILayout.Button(Accessibility.GetAccessibleString("Vehicle god mode", car.crashMultiplier <= 0.0), GUILayout.MaxWidth(200)))
			{
				car.crashMultiplier *= -1f;
			}

			// Toggle slot mover.
			if (GUILayout.Button(Accessibility.GetAccessibleString("Toggle slot mover", _settings.mode == "slotControl"), GUILayout.MaxWidth(200)))
			{
				if (_settings.mode == "slotControl")
				{
					GUIRenderer.SlotMoverDispose();
				}
				else
				{
					_settings.mode = "slotControl";
					_settings.car = car;
					_settings.slotStage = "slotSelect";
				}
			}

			GUILayout.Space(10);

			// Condition.
			GUILayout.Label("Condition", "LabelHeader");
			int maxCondition = (int)Enum.GetValues(typeof(Item.Condition)).Cast<Item.Condition>().Max();
			float rawCondition = GUILayout.HorizontalSlider(GUIRenderer.conditionInt, 0, maxCondition);
			GUIRenderer.conditionInt = Mathf.RoundToInt(rawCondition);
			GUILayout.Label(((Item.Condition)GUIRenderer.conditionInt).ToString(), GUIRenderer.labelStyle);

			GUIRenderer.applyConditionToAttached = GUILayout.Toggle(GUIRenderer.applyConditionToAttached, "Apply to attached");

			if (GUILayout.Button("Apply", GUILayout.MaxWidth(200)))
			{
				GameUtilities.SetCondition(GUIRenderer.conditionInt, GUIRenderer.applyConditionToAttached, partconditionscript);
			}

			GUILayout.Space(10);

			GUILayout.Label("Vehicle Colour", "LabelHeader");
			Colour.RenderColourSliders(dimensions.width / 2);

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Randomise colour", GUILayout.MaxWidth(200)))
			{
				Color color = Colour.GetColour();
				color.r = UnityEngine.Random.Range(0f, 255f) / 255f;
				color.g = UnityEngine.Random.Range(0f, 255f) / 255f;
				color.b = UnityEngine.Random.Range(0f, 255f) / 255f;
				Colour.SetColour(color);
			}

			GUILayout.Space(10);

			if (GUILayout.Button("Apply", GUILayout.MaxWidth(200)))
			{
				GameUtilities.Paint(Colour.GetColour(), partconditionscript);
			}
			GUILayout.EndHorizontal();


			GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
