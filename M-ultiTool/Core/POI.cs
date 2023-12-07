using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiTool.Core
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
		public GameObject poi;
	}
}
