using UnityEngine;

namespace MultiTool.Database
{
	internal class POI
	{
		public GameObject poi;
		public string name;
		public Texture2D thumbnail;
	}

	internal class SpawnedPOI
	{
		public int? ID;
		public GameObject poiObject;
		public POI poi;
	}
}
