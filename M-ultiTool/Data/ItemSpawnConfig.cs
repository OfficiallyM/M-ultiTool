using MultiTool.UI;
using System.Collections.Generic;
using UnityEngine;
using static MultiTool.Data.Item;

namespace MultiTool.Data
{
	internal class ItemSpawnConfig : ISpawnConfig
	{
		public int Condition { get; set; }
		public bool SpawnWithFuel { get; set; } = true;
		public int FuelMixCount { get; set; } = 1;
		public List<float> FuelValues { get; set; } = new List<float>() { -1 };
		public List<int> FuelTypes { get; set; } = new List<int>() { -1 };
		public string Plate { get; set; } = string.Empty;

		public int MaxFuelType { get; set; }
		public int MaxCondition { get; set; }

		public void Render(Rect dimensions)
		{
			// Condition.
			GUILayout.Label($"Condition: {(Condition)Condition}");
			Condition = Mathf.RoundToInt(GUILayout.HorizontalSlider(Condition, -1, MaxCondition));
			GUILayout.Space(10);

			// Plate.
			GUILayout.Label("Plate (blank for random):");
			Plate = GUILayout.TextField(Plate);
			GUILayout.Space(10);

			// Spawn with fuel.
			if (GUILayout.Button(Accessibility.GetAccessibleString("Spawn with fuel", SpawnWithFuel)))
				SpawnWithFuel = !SpawnWithFuel;
			GUILayout.Space(10);

			// Fuel mixes.
			for (int i = 0; i < FuelMixCount; i++)
			{
				GUILayout.BeginVertical($"Fluid {i + 1}", "box");
				GUILayout.Space(10);

				// Fluid type.
				string fuelType = ((mainscript.fluidenum)FuelTypes[i]).ToString();
				if (FuelTypes[i] == -1)
					fuelType = "Default";
				else
					fuelType = fuelType[0].ToString().ToUpper() + fuelType.Substring(1);
				GUILayout.Label($"Fluid type: {fuelType}");
				FuelTypes[i] = Mathf.RoundToInt(GUILayout.HorizontalSlider(FuelTypes[i], -1, MaxFuelType));

				GUILayout.Space(10);

				// Fluid amount.
				GUILayout.Label($"Fuel amount: {FuelValues[i]}");
				FuelValues[i] = GUILayout.HorizontalSlider(FuelValues[i], -1f, 1000f);

				bool fuelValueParse = float.TryParse(GUILayout.TextField(FuelValues[i].ToString()), out float tempFuelValue);
				if (fuelValueParse)
					FuelValues[i] = tempFuelValue;

				GUILayout.EndVertical();
				GUILayout.Space(5);
			}
			GUILayout.Space(5);

			GUILayout.BeginHorizontal();
			if (FuelMixCount <= MaxFuelType && GUILayout.Button("Add fluid"))
			{
				FuelMixCount++;
				FuelTypes.Add(0);
				FuelValues.Add(0);
			}
			GUILayout.Space(10);

			if (FuelMixCount > 1 && GUILayout.Button("Remove last fluid"))
			{
				FuelMixCount--;
				FuelTypes.RemoveAt(FuelTypes.Count - 1);
				FuelValues.RemoveAt(FuelValues.Count - 1);
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(10);

			Colour.RenderColourSliders(dimensions.width);
			if (GUILayout.Button("Randomise colour", GUILayout.MaxWidth(200)))
			{
				Color color = new Color
				{
					r = Random.Range(0f, 255f) / 255f,
					g = Random.Range(0f, 255f) / 255f,
					b = Random.Range(0f, 255f) / 255f
				};
				Colour.SetColour(color);
			}
		}
	}
}
