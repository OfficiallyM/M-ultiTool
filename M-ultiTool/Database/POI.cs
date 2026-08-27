using UnityEngine;

namespace MultiTool.Database
{
	internal class POI
	{
		public GameObject Poi;
		public string Name;
		public Texture2D Thumbnail;
	}

	internal class SpawnedPOI
	{
		public int? ID;
		public GameObject PoiObject;
		public POI Poi;
	}
}
