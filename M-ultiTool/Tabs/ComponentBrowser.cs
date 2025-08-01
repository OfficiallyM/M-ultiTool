using MultiTool.Components;
using MultiTool.Core;
using MultiTool.Extensions;
using MultiTool.Modules;
using MultiTool.Utilities;
using MultiTool.Utilities.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using static instancedRenderScript;
using Logger = MultiTool.Modules.Logger;
using Settings = MultiTool.Core.Settings;

namespace MultiTool.Tabs
{
	internal class BrowserTab : Tab
	{
		public override string Name => "Component Browser (Alpha)";
		public override bool HasConfigPane => true;
		public override string ConfigTitle => _selected?.gameObject.name ?? "Select object to show components";
		public override bool HasCache => true;
		public override int CacheRefreshTime => 2;

		private Settings _settings = new Settings();
		private Vector2 _position;
		private Vector2 _configPosition;

		private List<SceneObject> _objects;
		private SceneObject _selected;
		private HashSet<string> _expandedPaths = new HashSet<string>();

		public override void OnRegister()
		{
			DataFetcher.I.StartScan();
		}

		public override void OnCacheRefresh()
		{
		}

		public override void Update()
		{
			if (_objects != null) return;
			_objects = DataFetcher.I.GetObjects();
			// Reverse to set the list to oldest first.
			_objects.Reverse();
		}

		public override void RenderTab(Rect dimensions)
		{
			GUILayout.BeginArea(dimensions);
			GUILayout.BeginVertical();

			if (_objects == null)
			{
				GUILayout.FlexibleSpace();
				GUILayout.Label("Loading...", "LabelMessage");
				GUILayout.FlexibleSpace();
			}
			else
			{
				GUILayout.BeginHorizontal("box");
				if (GUILayout.Button("Refresh", GUILayout.MaxWidth(200)))
				{
					_objects = null;
					DataFetcher.I.StartScan();
					return;
				}
				GUILayout.Space(10);
				DateTime lastScan = DataFetcher.I.GetLastScanTime();
				TimeSpan elapsed = DateTime.Now - lastScan;
				GUILayout.Label($"Last refreshed {(int)elapsed.TotalMinutes} minute(s) ago");
				GUILayout.EndHorizontal();

				_position = GUILayout.BeginScrollView(_position);

				DrawHierarchy(_objects);

				GUILayout.EndScrollView();
			}

			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		private void DrawHierarchy(List<SceneObject> objects, int indent = 0)
		{
			foreach (SceneObject obj in objects)
			{
				if (obj.gameObject == null) continue;

				GUILayout.BeginHorizontal(_selected == obj ? "BoxGrey" : "box");
				GUILayout.Space(indent * 20f);

				bool expanded = IsExpanded(obj.gameObject);

				if (obj.children.Count > 0)
				{
					if (GUILayout.Button(expanded ? "-" : "+", GUILayout.Width(30)))
					{
						ToggleExpand(obj.gameObject);
					}
				}
				else
				{
					// Placeholder to ensure alignment.
					GUILayout.Space(34);
				}

				if (GUILayout.Button("", obj.gameObject.activeSelf ? "BoxGreen" : "BoxGrey", GUILayout.Width(25)))
				{
					obj.gameObject.SetActive(!obj.gameObject.activeSelf);
				}

				string name = "Unknown";
				try
				{
					name = obj.gameObject.name;
				}
				catch { }
				if (GUILayout.Button(name, "ButtonPrimaryTextLeft"))
				{
					if (_selected == obj)
						_selected = null;
					else
						_selected = obj;
				}
				GUILayout.EndHorizontal();

				if (expanded)
					DrawHierarchy(obj.children, indent + 1);
			}
		}

		private void ToggleExpand(GameObject obj)
		{
			if (obj?.transform == null) return;

			string path = obj.transform.GetPath();

			if (_expandedPaths.Contains(path))
				_expandedPaths.Remove(path);
			else
				_expandedPaths.Add(path);
		}

		private bool IsExpanded(GameObject obj)
		{
			if (obj?.transform == null) return false;
			return _expandedPaths.Contains(obj.transform.GetPath());
		}

		public override void RenderConfigPane(Rect dimensions)
		{
			if (_selected == null) return;

			GUILayout.BeginArea(dimensions);
			GUILayout.BeginHorizontal();
			GUILayout.Space(5);
			GUILayout.BeginVertical();
			GUILayout.Space(10);
			_configPosition = GUILayout.BeginScrollView(_configPosition);

			if (_selected.gameObject.activeSelf)
			{
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Teleport to", GUILayout.MaxWidth(100)))
					GameUtilities.TeleportPlayerWithParent(_selected.gameObject.transform.position + Vector3.up * 2f);

				GUILayout.Space(5);

				if (GUILayout.Button("Teleport here", GUILayout.MaxWidth(100)))
				{
					Vector3 position = mainscript.M.player.lookPoint + Vector3.up * 0.75f;
					Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, -mainscript.M.player.mainCam.transform.right);

					_selected.gameObject.transform.position = position;
					_selected.gameObject.transform.rotation = rotation;
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}

			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}
	}
}
