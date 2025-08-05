using MultiTool.Components;
using MultiTool.Core;
using MultiTool.Extensions;
using MultiTool.Modules;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Logger = MultiTool.Modules.Logger;

namespace MultiTool.Tabs
{
	internal class BrowserTab : Tab
	{
		public override string Name => "Component Browser (Alpha)";
		public override bool HasConfigPane => true;
		public override string ConfigTitle => _selected?.gameObject.name ?? "Select object to show components";
		public override bool HasCache => true;
		public override int CacheRefreshTime => 2;

		// Tab.
		private Settings _settings = new Settings();
		private Vector2 _position;
		private Vector2 _configPosition;
		private float _viewportHeight;

		// Objects.
		private List<SceneObject> _cachedObjects;
		private List<SceneObject> _objects;
		private SceneObject _selected;
		private int? _selectedInstanceId;
		
		// Pathing.
		private HashSet<string> _expandedPaths = new HashSet<string>();
		private HashSet<string> _searchExpandedPaths = new HashSet<string>();

		// Searching.
		private bool _showSearchSettings = false;
		private string _search = "";
		private int _searchDepth = -1;
		private bool _searchInactive = false;
		private bool _expandChildren = true;
		private bool _onlyShowMatchingObjects = true;
		private CancellationTokenSource _searchCts;
		private bool _isSearching = false;

		public override void OnRegister()
		{
			DataFetcher.I.StartScan();
		}

		public override void OnCacheRefresh()
		{
		}

		public override void Update()
		{
			FetchData();
		}

		/// <summary>
		/// Check for and refresh hierarchy data.
		/// </summary>
		private void FetchData()
		{
			if (_objects != null) return;
			_objects = DataFetcher.I.GetObjects();
			// Reverse to set the list to oldest first.
			_objects.Reverse();
			_cachedObjects = _objects.ToList();
		}

		/// <summary>
		/// Trigger a search, cancelling any existing searches.
		/// </summary>
		/// <param name="query">Query string</param>
		private async void SearchAsync(string query)
		{
			_searchCts?.Cancel();
			_searchCts = new CancellationTokenSource();
			var token = _searchCts.Token;
			_isSearching = true;

			try
			{
				_searchExpandedPaths.Clear();
				_cachedObjects = _objects.ToList();
				if (query != string.Empty)
				{
					var result = await Task.Run(() => Search(_cachedObjects, query), token);
					_cachedObjects = result;
				}
				RefreshSelected();
				_position = Vector2.zero;
			}
			catch (OperationCanceledException)
			{
				// Search was cancelled, ignore.
			}
			_isSearching = false;
		}

		/// <summary>
		/// Search hierarchy for a query string.
		/// </summary>
		/// <param name="roots">Hierarchy</param>
		/// <param name="query">Query string</param>
		/// <returns>List of SceneObject results</returns>
		private List<SceneObject> Search(List<SceneObject> roots, string query)
		{
			var results = new List<SceneObject>();
			HashSet<string> searchExpandedPaths = new HashSet<string>();
			query = query.ToLowerInvariant();

			foreach (var root in roots)
			{
				var match = FilterAndBuildTree(root, query, searchExpandedPaths, 0);
				if (match != null)
					results.Add(match);
			}

			_searchExpandedPaths = searchExpandedPaths;
			return results;
		}

		/// <summary>
		/// Recursively search hierarchy by query string.
		/// </summary>
		/// <param name="obj">Root object</param>
		/// <param name="query">Query string</param>
		/// <param name="expandedPaths">Current paths to expand</param>
		/// <param name="depth">Current search depth</param>
		/// <returns>SceneObject if found, otherwise null</returns>
		private SceneObject FilterAndBuildTree(SceneObject obj, string query, HashSet<string> expandedPaths, int depth)
		{
			try
			{
				if (obj.gameObject == null)
				return null;

				if (_searchDepth > -1 && depth > _searchDepth)
					return null;

				if (!_searchInactive && !obj.gameObject.activeInHierarchy)
					return null;

				bool selfMatch = false;
				try
				{
					selfMatch = obj.gameObject.name.ToLowerInvariant().Contains(query);
				}
				catch { }

				var matchedChildren = new List<SceneObject>();
				foreach (var child in obj.children)
				{
					var match = FilterAndBuildTree(child, query, expandedPaths, depth + 1);
					if (match != null)
						matchedChildren.Add(match);
				}

				if (_onlyShowMatchingObjects)
				{
					if (!selfMatch && matchedChildren.Count == 0)
						return null;
				}
				else
				{
					if (!selfMatch)
						return null;
					matchedChildren = obj.children;
				}

				if (_expandChildren && matchedChildren.Count > 0 && obj.gameObject?.transform != null)
					expandedPaths.Add(obj.gameObject.transform.GetPath());

				return new SceneObject
				{
					gameObject = obj.gameObject,
					children = matchedChildren
				};
			}
			catch (Exception ex)
			{
				Logger.Log($"Error during component browser search. Details: {ex}", Logger.LogLevel.Error);
				Notifications.SendError("Component Browser", "An error occurred during search!");
			}
			return null;
		}

		/// <summary>
		/// Refresh selected object from instance ID.
		/// </summary>
		private void RefreshSelected()
		{
			_selected = null;

			if (_selectedInstanceId == null)
				return;

			foreach (var root in _cachedObjects)
			{
				var found = FindByInstanceId(root, _selectedInstanceId.Value);
				if (found != null)
				{
					_selected = found;
					break;
				}
			}
		}

		/// <summary>
		/// Recursively attempt to find object in hierarchy by instance ID.
		/// </summary>
		/// <param name="obj">Root object</param>
		/// <param name="instanceId">Instance ID</param>
		/// <returns>SceneObject if exists, otherwise null</returns>
		private SceneObject FindByInstanceId(SceneObject obj, int instanceId)
		{
			if (obj?.gameObject == null)
				return null;

			if (obj.gameObject.GetInstanceID() == instanceId)
				return obj;

			foreach (var child in obj.children)
			{
				var found = FindByInstanceId(child, instanceId);
				if (found != null)
					return found;
			}

			return null;
		}

		public override void RenderTab(Rect dimensions)
		{
			_viewportHeight = dimensions.height - 20f;
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
					_cachedObjects = null;
					DataFetcher.I.StartScan();
					return;
				}
				GUILayout.Space(10);
				DateTime lastScan = DataFetcher.I.GetLastScanTime();
				TimeSpan elapsed = DateTime.Now - lastScan;
				GUILayout.Label($"Last refreshed {(int)elapsed.TotalMinutes} minute(s) ago");
				GUILayout.FlexibleSpace();
				GUILayout.BeginVertical();
				GUILayout.BeginHorizontal();
				GUILayout.Label("Search", GUILayout.MaxWidth(50));
				string newSearch = GUILayout.TextField(_search, GUILayout.MaxWidth(300));
				if (newSearch != _search)
				{
					_search = newSearch;
					SearchAsync(_search);
				}
				GUILayout.Space(5);
				if (GUILayout.Button($"{(_showSearchSettings ? "-" : "+")} Search settings", GUILayout.MaxWidth(200)))
					_showSearchSettings = !_showSearchSettings;
				GUILayout.EndHorizontal();
				if (_showSearchSettings)
				{
					GUILayout.Label("Search settings", "LabelSubHeader");

					GUILayout.BeginHorizontal();
					GUILayout.Label("Search depth", GUILayout.MaxWidth(100));
					int.TryParse(GUILayout.TextField(_searchDepth.ToString(), GUILayout.MaxWidth(100)), out int searchDepth);
					if (searchDepth != _searchDepth)
					{
						_searchDepth = searchDepth;
						SearchAsync(_search);
					}
					if (GUILayout.Button("Set to infinite", GUILayout.MaxWidth(100)))
					{
						_searchDepth = -1;
						SearchAsync(_search);
					}
					GUILayout.EndHorizontal();
					GUILayout.Label("Depth of child objects to search, -1 for unlimited depth", GUILayout.MaxWidth(500));

					bool shouldSearchInactive = GUILayout.Toggle(_searchInactive, "Search inactive objects");
					if (shouldSearchInactive != _searchInactive)
					{
						_searchInactive = shouldSearchInactive;
						SearchAsync(_search);
					}

					bool expandChildren = GUILayout.Toggle(_expandChildren, "Automatically expand to show matching child objects");
					if (expandChildren != _expandChildren)
					{
						_expandChildren = expandChildren;
						SearchAsync(_search);
					}

					bool onlyShowMatchingObjects = GUILayout.Toggle(_onlyShowMatchingObjects, "Only show matching objects");
					if (onlyShowMatchingObjects != _onlyShowMatchingObjects)
					{
						_onlyShowMatchingObjects = onlyShowMatchingObjects;
						SearchAsync(_search);
					}
					GUILayout.Label("With this unchecked, all children of a matching parent will show regardless of whether the child matches", GUILayout.MaxWidth(550));
				}
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();

				if (_selected != null)
				{
					GUILayout.BeginHorizontal("box");
					if (GUILayout.Button("Scroll to selected", GUILayout.MaxWidth(200)))
						ScrollToObject();
					GUILayout.EndHorizontal();
				}

				if (_isSearching)
				{
					GUILayout.FlexibleSpace();
					GUILayout.Label("Searching...", "LabelMessage");
					GUILayout.FlexibleSpace();
				}
				else
				{
					_position = GUILayout.BeginScrollView(_position);
					DrawHierarchy(_cachedObjects);
					GUILayout.EndScrollView();
				}
			}

			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		/// <summary>
		/// Recursively render hierarchy.
		/// </summary>
		/// <param name="objects">List of objects to render</param>
		/// <param name="indent">Object indentation in hierarchy</param>
		private void DrawHierarchy(List<SceneObject> objects, int indent = 0)
		{
			if (objects == null) return;
			foreach (SceneObject obj in objects)
			{
				if (obj.gameObject == null) continue;

				GUILayout.BeginHorizontal(_selected == obj ? "BoxGrey" : "box");
				GUILayout.Space(indent * 34f);

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
					{
						_selected = null;
						_selectedInstanceId = null;
					}
					else
					{
						_selected = obj;
						_selectedInstanceId = obj.gameObject.GetInstanceID();
					}
				}
				GUILayout.EndHorizontal();

				if (expanded)
					DrawHierarchy(obj.children, indent + 1);
			}
		}

		/// <summary>
		/// Expand a given object in the hierarchy.
		/// </summary>
		/// <param name="obj">Object to expand</param>
		private void ToggleExpand(GameObject obj)
		{
			if (obj?.transform == null) return;

			string path = obj.transform.GetPath();

			if (_searchExpandedPaths.Contains(path))
			{
				_searchExpandedPaths.Remove(path);
				return;
			}

			if (_expandedPaths.Contains(path))
				_expandedPaths.Remove(path);
			else
				_expandedPaths.Add(path);
		}

		/// <summary>
		/// Check if object is expanded in hierarchy.
		/// </summary>
		/// <param name="obj">GameObject to check</param>
		/// <returns>True if object is expanded, otherwise false</returns>
		private bool IsExpanded(GameObject obj)
		{
			if (obj?.transform == null) return false;
			string path = obj.transform.GetPath();
			return _expandedPaths.Contains(path) || _searchExpandedPaths.Contains(path);
		}

		/// <summary>
		/// Scroll viewport to object.
		/// </summary>
		/// <param name="obj">Object to scroll to, or selected object if not set</param>
		private void ScrollToObject(SceneObject obj = null)
		{
			if (obj == null) obj = _selected;
			if (obj == null) return;

			float offset = 0f;
			if (TryGetScrollOffset(_cachedObjects, obj, ref offset))
			{
				_position.y = Mathf.Max(0, offset - _viewportHeight / 2);
			}
		}

		/// <summary>
		/// Recursively loop through objects to calculate height to scroll to.
		/// </summary>
		/// <param name="objects">Objects list</param>
		/// <param name="target">Target object to scroll to</param>
		/// <param name="offset">Current offset</param>
		/// <returns></returns>
		private bool TryGetScrollOffset(List<SceneObject> objects, SceneObject target, ref float offset)
		{
			foreach (var obj in objects)
			{
				if (obj.gameObject == null) continue;
				bool isExpanded = _expandedPaths.Contains(obj.gameObject.transform.GetPath());

				if (obj == target)
					return true;

				// Offset by row height.
				offset += 37.5f;

				if (isExpanded && obj.children.Count > 0)
				{
					if (TryGetScrollOffset(obj.children, target, ref offset))
						return true;
				}
			}

			return false;
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
