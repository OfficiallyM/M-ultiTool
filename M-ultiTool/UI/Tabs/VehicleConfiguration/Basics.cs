using MultiTool.Data;
using MultiTool.Utilities;
using System;
using System.Linq;
using UnityEngine;

namespace MultiTool.UI.Tabs.VehicleConfiguration
{
	internal sealed class BasicsTab : UI.VehicleConfigurationTab
	{
		public override string Name => "Basics";

		private Vector2 _position;
		private int _conditionInt = 0;
		private bool _applyConditionToAttached = false;

		public override void RenderTab(Rect dimensions)
		{
			GUILayout.BeginArea(dimensions);
			GUILayout.BeginVertical();
			_position = GUILayout.BeginScrollView(_position);

			carscript car = mainscript.M.player.Car;
			partconditionscript partconditionscript = car.gameObject.GetComponent<partconditionscript>();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button(Accessibility.GetAccessibleString("Vehicle god mode", car.crashMultiplier <= 0.0), GUILayout.MaxWidth(200)))
			{
				car.crashMultiplier *= -1f;
			}
			GUILayout.Space(10);

			if (GUILayout.Button("Flip vehicle", GUILayout.MaxWidth(200)))
			{
				car.transform.rotation = Quaternion.Euler(0, car.transform.rotation.eulerAngles.y, 0);
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(10);

			// Toggle slot mover.
			MultiTool.Tools.RenderControl("slot_mover");

			// Condition.
			GUILayout.Label("Condition", "LabelHeader");
			int maxCondition = (int)Enum.GetValues(typeof(Condition)).Cast<Condition>().Max();
			float rawCondition = GUILayout.HorizontalSlider(_conditionInt, 0, maxCondition);
			_conditionInt = Mathf.RoundToInt(rawCondition);
			GUILayout.Label(((Condition)_conditionInt).ToString());

			_applyConditionToAttached = GUILayout.Toggle(_applyConditionToAttached, "Apply to attached");

			if (GUILayout.Button("Apply", GUILayout.MaxWidth(200)))
			{
				GameUtilities.SetCondition(_conditionInt, _applyConditionToAttached, partconditionscript);
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
