using UnityEngine;

namespace MultiTool.Data
{
	internal class Poi
	{
		public GameObject Obj;
		public string Name;
		public Texture2D Thumbnail;
	}

	internal class SpawnedPOI
	{
		public int? ID;
		public GameObject PoiObject;
		public Poi Data;
	}
}
