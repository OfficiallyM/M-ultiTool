using System.Collections.Generic;
using UnityEngine;

namespace MultiTool.Database
{
	internal class Vehicle
	{
		public GameObject GameObject;
		public string Name;
		public Texture2D Thumbnail;
		public int Variant;
		public int ConditionInt = 0;
		public int FuelMixes = 1;
		public List<float> FuelValues = new List<float> { -1f };
		public List<int> FuelTypeInts = new List<int> { -1 };
		public Color Color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
		public string Plate = string.Empty;
		public AMTData Amt = null;
	}
}
