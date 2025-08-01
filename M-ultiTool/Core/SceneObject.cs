using System.Collections.Generic;
using UnityEngine;

namespace MultiTool.Core
{
	internal class SceneObject
	{
		public GameObject gameObject;
		public List<SceneObject> children = new List<SceneObject>();
	}
}
