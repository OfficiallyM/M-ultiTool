using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiTool.Core
{
	internal class Item
	{
		public GameObject item;
		public Texture2D thumbnail;
		public int category;
		public int conditionInt = 0;
		public int fuelMixes = 1;
		public List<float> fuelValues = new List<float> { -1f };
		public List<int> fuelTypeInts = new List<int> { -1 };
		public Color color = new Color(255f / 255f, 255f / 255f, 255f / 255f);

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
