using System.Collections.Generic;
using UnityEngine;

namespace MultiTool.Core
{
	internal class SceneObject
	{
		public GameObject gameObject;
		public SceneObject parent;
		public List<SceneObject> children = new List<SceneObject>();
	}

	internal class TrackedSceneObject
	{
		public int instanceId { get; private set; }
		public SceneObject sceneObject { get; private set; }

		public TrackedSceneObject(int instanceId, SceneObject sceneObject)
		{
			if (sceneObject?.gameObject == null) return;
			this.instanceId = instanceId;
			this.sceneObject = sceneObject;
		}

		public bool Refresh(List<SceneObject> objects)
		{
			sceneObject = FindByInstanceId(objects, instanceId);
			return sceneObject != null;
		}

		private SceneObject FindByInstanceId(List<SceneObject> roots, int id)
		{
			foreach (var obj in roots)
			{
				if (obj.gameObject?.GetInstanceID() == id)
					return obj;

				var found = FindByInstanceId(obj.children, id);
				if (found != null)
					return found;
			}
			return null;
		}
	}
}
