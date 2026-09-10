using UnityEngine;

namespace MultiTool.Data
{
	internal class Item
	{
		public GameObject GameObject { get; set; }
		public string Name { get; set; }
		public int? Category { get; set; }
		public int? Variant { get; set; }
		public Texture2D Thumbnail { get; set; }
		public AMTData Amt { get; set; } = null;
	}
}
