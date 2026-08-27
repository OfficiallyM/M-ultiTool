using System.Collections.Generic;
using UnityEngine;

namespace MultiTool.Database
{
	internal class Item
	{
		public GameObject GameObject;
		public Texture2D Thumbnail;
		public int Category;
		public int ConditionInt = 0;
		public int FuelMixes = 1;
		public List<float> FuelValues = new List<float> { -1f };
		public List<int> FuelTypeInts = new List<int> { -1 };
		public Color Color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
		public string Plate = string.Empty;
		public AMTData Amt = null;

		public enum Condition
		{
			Random = -1,
			Pristine,
			Dull,
			Rough,
			Crusty,
			Rusty
		}

		public Item Clone()
		{
			return (Item)MemberwiseClone();
		}
	}
}
