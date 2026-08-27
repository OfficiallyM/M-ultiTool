using System.Collections.Generic;
using UnityEngine;

namespace MultiTool.Database
{
	internal class Vehicle
	{
		public GameObject gameObject;
		public string name;
		public Texture2D thumbnail;
		public int variant;
		public int conditionInt = 0;
		public int fuelMixes = 1;
		public List<float> fuelValues = new List<float> { -1f };
		public List<int> fuelTypeInts = new List<int> { -1 };
		public Color color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
		public string plate = string.Empty;
		public AMTData amt = null;
	}
}
