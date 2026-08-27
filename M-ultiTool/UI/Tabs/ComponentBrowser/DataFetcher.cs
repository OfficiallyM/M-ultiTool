using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiTool.UI.Tabs.ComponentBrowser
{
	internal class DataFetcher : MonoBehaviour
	{
		public static DataFetcher I;

		private List<SceneObject> _sceneObjects = new List<SceneObject>();
		private Dictionary<Transform, SceneObject> _transformToObject = new Dictionary<Transform, SceneObject>();
		private bool _isScanning = false;
		private DateTime _lastScan = DateTime.Now;

		public DataFetcher()
		{
			I = this;
		}

		public void StartScan()
		{
			if (_isScanning) return;
			StartCoroutine(ScanScene());
		}

		public List<SceneObject> GetObjects()
		{
			if (_isScanning) return null;
			return _sceneObjects;
		}

		public DateTime GetLastScanTime() => _lastScan;

		private IEnumerator ScanScene()
		{
			_isScanning = true;
			_sceneObjects.Clear();

			GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>()
				.Where(go => string.IsNullOrEmpty(go.scene.name) == false)
				.ToArray();

			foreach (GameObject obj in objects)
			{
				SceneObject sceneObject = new SceneObject() { gameObject = obj };
				_transformToObject[obj.transform] = sceneObject;
			}

			foreach (GameObject obj in objects)
			{
				Transform parent = obj.transform.parent;
				SceneObject node = _transformToObject[obj.transform];

				if (parent == null)
				{
					// Root-level object.
					_sceneObjects.Add(node);
				}
				else if (_transformToObject.TryGetValue(parent, out SceneObject parentNode))
				{
					node.parent = parentNode;
					parentNode.children.Add(node);
				}
			}

			_lastScan = DateTime.Now;
			_isScanning = false;
			yield return null;
		}
	}
}
