using System.Collections.Generic;
using UnityEngine;

namespace MultiTool.UI.Tabs.ComponentBrowser
{
	internal class SceneObject
	{
		public GameObject GameObject;
		public SceneObject Parent;
		public List<SceneObject> Children = new List<SceneObject>();
	}

	internal class TrackedSceneObject
	{
		public int instanceId { get; private set; }
		public SceneObject sceneObject { get; private set; }

		public TrackedSceneObject(int instanceId, SceneObject sceneObject)
		{
			if (sceneObject?.GameObject == null) return;
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
			foreach (SceneObject obj in roots)
			{
				if (obj.GameObject?.GetInstanceID() == id)
					return obj;

				SceneObject found = FindByInstanceId(obj.Children, id);
				if (found != null)
					return found;
			}
			return null;
		}
	}

	internal class TrackedComponent
	{
		public Component component { get; private set; }
		public int? instanceId { get; private set; }
		public SceneObject sceneObject { get; private set; }

		public TrackedComponent(Component component)
		{
			this.component = component;
		}

		public TrackedComponent(Component component, int instanceId, SceneObject sceneObject)
		{
			this.component = component;
			this.instanceId = instanceId;
			this.sceneObject = sceneObject;
		}

		public bool Refresh(List<SceneObject> objects)
		{
			if (instanceId == null) return false;
			sceneObject = FindByInstanceId(objects, instanceId.Value);
			return sceneObject != null;
		}

		private SceneObject FindByInstanceId(List<SceneObject> roots, int id)
		{
			foreach (SceneObject obj in roots)
			{
				if (obj.GameObject?.GetInstanceID() == id)
					return obj;

				SceneObject found = FindByInstanceId(obj.Children, id);
				if (found != null)
					return found;
			}
			return null;
		}
	}

	internal class Tracked
	{
		public TrackedSceneObject trackedSceneObject { get; set; }
		public TrackedComponent trackedComponent { get; set; }
	}

	internal enum SearchScope
	{
		Self,
		Parent,
		Root,
	}

	internal enum ConditionType
	{
		Name,
		Tag,
		Type,
		Layer
	}

	internal class SearchCondition
	{
		public ConditionType Type;
		public string Value;
		public bool Negate;
	}

	internal class SearchFilter
	{
		public Dictionary<SearchScope, List<SearchCondition>> Conditions = new Dictionary<SearchScope, List<SearchCondition>>
		{
			{ SearchScope.Self, new List<SearchCondition>() },
			{ SearchScope.Parent, new List<SearchCondition>() },
			{ SearchScope.Root, new List<SearchCondition>() },
		};
	}
}
