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
		public override string ConfigTitle => _selected?.sceneObject.gameObject.name ?? "Select object to show components";
		public override bool HasCache => true;
		public override int CacheRefreshTime => 2;

		// Tab.
		private Settings _settings = new Settings();
		private Vector2 _position;
		private Vector2 _configPosition;
		private Vector2 _bookmarksPosition;
		private Vector2 _helpModalPosition;
		private float _viewportHeight;

		// Objects.
		private List<SceneObject> _cachedObjects;
		private List<SceneObject> _objects;
		private TrackedSceneObject _selected;
		
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

		// Bookmarks.
		private List<TrackedSceneObject> _bookmarks = new List<TrackedSceneObject>();
		private bool _isBookmarksExpanded = false;

		// Help modal.
		private bool _showHelpModal = false;
		private Rect _helpModalRect = new Rect(100, 100, MultiTool.Renderer.resolutionX / 3, MultiTool.Renderer.resolutionY / 3);

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
					var result = await Task.Run(() => Search(_cachedObjects, query, token), token);
					if (!token.IsCancellationRequested)
						_cachedObjects = result;
				}
				RefreshTracked();
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
		/// <param name="token">CancellationToken</param>
		/// <returns>List of SceneObject results</returns>
		private List<SceneObject> Search(List<SceneObject> roots, string query, CancellationToken token)
		{
			var results = new List<SceneObject>();
			HashSet<string> searchExpandedPaths = new HashSet<string>();
			var filter = ParseQuery(query);

			foreach (var root in roots)
			{
				token.ThrowIfCancellationRequested();

				var match = FilterAndBuildTree(root, filter, searchExpandedPaths, 0, token);
				if (match != null)
					results.Add(match);
			}

			_searchExpandedPaths = searchExpandedPaths;
			return results;
		}

		/// <summary>
		/// Parse query for tags.
		/// </summary>
		/// <param name="query">Query string</param>
		/// <returns>Built search filter</returns>
		private SearchFilter ParseQuery(string query)
		{
			var filter = new SearchFilter();
			var tokens = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var rawToken in tokens)
			{
				string token = rawToken.Trim();
				bool negate = token.StartsWith("!");
				if (negate) token = token.Substring(1);

				SearchScope scope = SearchScope.Self;
				if (token.StartsWith("parent:")) { scope = SearchScope.Parent; token = token.Substring(7); }
				else if (token.StartsWith("root:")) { scope = SearchScope.Root; token = token.Substring(5); }

				ConditionType type = ConditionType.Name;
				if (token.StartsWith("tag:")) { type = ConditionType.Tag; token = token.Substring(4); }
				else if (token.StartsWith("type:")) { type = ConditionType.Type; token = token.Substring(5); }
				else if (token.StartsWith("component:")) { type = ConditionType.Type; token = token.Substring(10); }
				else if (token.StartsWith("layer:")) { type = ConditionType.Layer; token = token.Substring(6); }

				if (token.StartsWith("\"") && token.EndsWith("\""))
					token = token.Substring(1, token.Length - 2);

				filter.Conditions[scope].Add(new SearchCondition
				{
					Type = type,
					Value = token,
					Negate = negate
				});
			}

			return filter;
		}

		/// <summary>
		/// Check if a GameObject matches a given list of query conditions.
		/// </summary>
		/// <param name="obj">GameObject</param>
		/// <param name="conditions">Query conditions</param>
		/// <returns>True if GameObject matches the query conditions, otherwise false</returns>
		private bool MatchesConditions(GameObject obj, List<SearchCondition> conditions)
		{
			if (obj == null) return false;

			foreach (var cond in conditions)
			{
				bool match = false;

				switch (cond.Type)
				{
					case ConditionType.Name:
						match = obj.name.ToLowerInvariant().Contains(cond.Value.ToLowerInvariant());
						break;
					case ConditionType.Tag:
						match = obj.CompareTag(cond.Value);
						break;
					case ConditionType.Type:
						match = obj.GetComponent(cond.Value) != null;
						break;
					case ConditionType.Layer:
						match = LayerMask.LayerToName(obj.layer).Equals(cond.Value, StringComparison.OrdinalIgnoreCase);
						break;
				}

				if (cond.Negate) match = !match;
				if (!match) return false;
			}
			return true;
		}

		/// <summary>
		/// Recursively search hierarchy by query string.
		/// </summary>
		/// <param name="obj">Root object</param>
		/// <param name="filter">Search filter</param>
		/// <param name="expandedPaths">Current paths to expand</param>
		/// <param name="depth">Current search depth</param>
		/// <param name="token">CancellationToken</param>
		/// <returns>SceneObject if found, otherwise null</returns>
		private SceneObject FilterAndBuildTree(SceneObject obj, SearchFilter filter, HashSet<string> expandedPaths, int depth, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();

			try
			{
				if (obj.gameObject == null) return null;
				if (_searchDepth > -1 && depth > _searchDepth) return null;
				if (!_searchInactive && !obj.gameObject.activeInHierarchy) return null;

				bool selfMatch = MatchesConditions(obj.gameObject, filter.Conditions[SearchScope.Self]);

				var matchedChildren = new List<SceneObject>();
				foreach (var child in obj.children)
				{
					var match = FilterAndBuildTree(child, filter, expandedPaths, depth + 1, token);
					if (match != null)
						matchedChildren.Add(match);
				}

				// Evaluate root and parent conditions.
				if (filter.Conditions[SearchScope.Parent].Count > 0 && obj.gameObject.transform.parent != null)
				{
					var parentObj = obj.gameObject.transform.parent.gameObject;
					if (!MatchesConditions(parentObj, filter.Conditions[SearchScope.Parent])) return null;
				}
				if (filter.Conditions[SearchScope.Root].Count > 0)
				{
					var rootObj = obj.gameObject.transform.root.gameObject;
					if (!MatchesConditions(rootObj, filter.Conditions[SearchScope.Root])) return null;
				}

				if (_onlyShowMatchingObjects)
				{
					if (!selfMatch && matchedChildren.Count == 0) return null;
				}
				else
				{
					if (!selfMatch) return null;
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
			catch (OperationCanceledException)
			{
				// Search was cancelled, ignore.
			}
			catch (Exception ex)
			{
				Logger.Log($"Error during component browser search. Details: {ex}", Logger.LogLevel.Error);
				Notifications.SendError("Component Browser", "An error occurred during search!");
			}
			return null;
		}

		/// <summary>
		/// Refresh any tracked scene objects from their instance IDs.
		/// </summary>
		private void RefreshTracked()
		{
			if (_selected != null && !_selected.Refresh(_cachedObjects))
				_selected = null;

			List<TrackedSceneObject> clear = new List<TrackedSceneObject>();
			foreach (TrackedSceneObject bookmark in _bookmarks)
			{
				if (!bookmark.Refresh(_cachedObjects))
					clear.Add(bookmark);
			}
			foreach (TrackedSceneObject c in clear)
			{
				_bookmarks.Remove(c);
			}
		}

		public override void RenderTab(Rect dimensions)
		{
			if (_showHelpModal)
			{
				_helpModalRect = GUILayout.Window(0, _helpModalRect, RenderHelpWindow, "<size=24>Help</size>", "box");
			}

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
				GUILayout.Space(10);
				if (GUILayout.Button("Help", GUILayout.MaxWidth(50)))
					_showHelpModal = !_showHelpModal;
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

					bool expandChildren = GUILayout.Toggle(_expandChildren, "Expand to show matching child objects");
					if (expandChildren != _expandChildren)
					{
						_expandChildren = expandChildren;
						SearchAsync(_search);
					}

					bool onlyShowMatchingObjects = GUILayout.Toggle(_onlyShowMatchingObjects, "Only show matching children");
					if (onlyShowMatchingObjects != _onlyShowMatchingObjects)
					{
						_onlyShowMatchingObjects = onlyShowMatchingObjects;
						SearchAsync(_search);
					}
					GUILayout.Label("With this unchecked, all children of a matching parent will show regardless of whether the child matches", GUILayout.MaxWidth(550));
				}
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();

				if (_selected != null || _bookmarks.Count > 0)
				{
					GUILayout.BeginHorizontal("box");
					if (_selected != null && GUILayout.Button("Scroll to selected", GUILayout.MaxWidth(200)))
						ScrollToObject();

					GUILayout.FlexibleSpace();

					if (_bookmarks.Count > 0 && GUILayout.Button($"{(_isBookmarksExpanded ? "-" : "+")} Bookmarks", GUILayout.MaxWidth(200)))
						_isBookmarksExpanded = !_isBookmarksExpanded;
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
					GUILayout.BeginHorizontal();
					GUILayout.BeginVertical();
					_position = GUILayout.BeginScrollView(_position);
					RenderHierarchy(_cachedObjects);
					GUILayout.EndScrollView();
					GUILayout.EndVertical();

					if (_isBookmarksExpanded)
					{
						GUILayout.BeginVertical(GUILayout.MaxWidth(350));
						GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						GUILayout.Label("Bookmarks", "LabelSubHeader");
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();

						_bookmarksPosition = GUILayout.BeginScrollView(_bookmarksPosition);
						foreach (TrackedSceneObject bookmark in _bookmarks)
						{
							if (bookmark.sceneObject == null || bookmark.sceneObject.gameObject == null) continue;

							GUILayout.BeginHorizontal("box");
							string name = "Unknown";
							try
							{
								name = bookmark.sceneObject.gameObject.name;
							}
							catch { }
							if (GUILayout.Button(name, "ButtonPrimaryTextLeft"))
							{
								_selected = new TrackedSceneObject(bookmark.sceneObject.gameObject.GetInstanceID(), bookmark.sceneObject);
								ScrollToObject(bookmark.sceneObject);
							}

							if (GUILayout.Button("X", GUILayout.MaxWidth(30)))
							{
								_bookmarks.Remove(bookmark);
								if (_bookmarks.Count == 0)
									_isBookmarksExpanded = false;
								break;
							}
							GUILayout.EndHorizontal();
						}
						GUILayout.EndScrollView();
						GUILayout.EndVertical();
					}
					GUILayout.EndHorizontal();
				}
			}

			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		private void RenderHelpWindow(int windowID)
		{
			if (GUI.Button(new Rect(_helpModalRect.width - 40, 10, 30, 30), "X"))
				_showHelpModal = false;

			GUILayout.BeginVertical();
			GUILayout.Space(40);
			_helpModalPosition = GUILayout.BeginScrollView(_helpModalPosition);
			GUILayout.Label("Search help", "LabelHeader");
			GUILayout.Space(10);
			GUILayout.Label("All searches are case sensitive unless otherwise specified");

			GUILayout.Label("Basic", "LabelSubHeader");
			GUILayout.Label("Name search (default):");
			GUILayout.Label("Just type part of an object's name\nExample: 'car' finds all objects with \"car\" in their name\nNOTE: This search is case insensitive");
			GUILayout.Space(20);

			GUILayout.Label("Tags (Filters)", "LabelSubHeader");
			GUILayout.Label("'type:' or 'component:'");
			GUILayout.Label("Match components attached to the object\nExample: 'type:Rigidbody' or 'component:Rigidbody' finds any objects containing a Rigidbody");
			GUILayout.Space(5);
			GUILayout.Label("'tag:'");
			GUILayout.Label("Match objects with a tag\nExample: 'tag:Player' would find any objects tagged \"Player\"");
			GUILayout.Space(5);
			GUILayout.Label("'layer:'");
			GUILayout.Label("Match objects on a layer\nExample: TODO: Add example");
			GUILayout.Space(5);
			GUILayout.Label("'name:'");
			GUILayout.Label("Same as the basic name search");
			GUILayout.Space(20);

			GUILayout.Label("Hierarchy filters", "LabelSubHeader");
			GUILayout.Label("These let you search relative to a parent/root object\nThese also support chaining with any of the above tags, but will default to name searching if a tag isn't specified");
			GUILayout.Space(5);
			GUILayout.Label("'parent:'");
			GUILayout.Label("Used to match on a parent\nExample: 'parent:car06' would find any where any parent name contains \"car06\"");
			GUILayout.Space(5);
			GUILayout.Label("'root:'");
			GUILayout.Label("Used to match on the root object\nExample: 'parent:car06' would find any where the root object name contains \"car06\"");
			GUILayout.Space(5);
			GUILayout.Label("Example chained with other filters: 'root:car06 type:enginescript' would find any objects that have an enginescript where the root object name contains car06\nOr, 'parent:component:Rigidbody component:Light' would show any objects containing a Light where a parent object contains a Rigidbody");
			GUILayout.Space(20);

			GUILayout.Label("Negation", "LabelSubHeader");
			GUILayout.Label("Prefix any filter with ! to exclude results that match\nFor example '!root:component:carscript component:enginescript' would show any objects that have an enginescript where the root object does not have a carscript");

			GUILayout.EndScrollView();
			GUILayout.EndVertical();

			GUI.DragWindow(new Rect(0, 0, _helpModalRect.width, _helpModalRect.height));
		}

		/// <summary>
		/// Recursively render hierarchy.
		/// </summary>
		/// <param name="objects">List of objects to render</param>
		/// <param name="indent">Object indentation in hierarchy</param>
		private void RenderHierarchy(List<SceneObject> objects, int indent = 0)
		{
			if (objects == null) return;
			foreach (SceneObject obj in objects)
			{
				if (obj.gameObject == null) continue;

				GUILayout.BeginHorizontal(_selected?.sceneObject == obj ? "BoxGrey" : "box");
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
					if (_selected != null && _selected.sceneObject == obj)
						_selected = null;
					else
						_selected = new TrackedSceneObject(obj.gameObject.GetInstanceID(), obj);
				}

				if (GUILayout.Button(IsInBookmarks(obj) ? "★" : "☆", "ButtonSecondary", GUILayout.Width(35)))
				{
					if (IsInBookmarks(obj))
						RemoveBookmark(obj);
					else
						_bookmarks.Add(new TrackedSceneObject(obj.gameObject.GetInstanceID(), obj));
				}

				GUILayout.EndHorizontal();

				if (expanded)
					RenderHierarchy(obj.children, indent + 1);
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
			if (obj == null) obj = _selected?.sceneObject;
			if (obj == null) return;

			// Expand parent if collapsed to ensure we can scroll to it.
			SceneObject parent = obj.parent;
			if (parent != null && !IsExpanded(parent.gameObject))
				ToggleExpand(parent.gameObject);

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

		/// <summary>
		/// Check if a given object is bookmarked.
		/// </summary>
		/// <param name="obj">Object to check</param>
		/// <returns>True if object is bookmarked, otherwise false</returns>
		private bool IsInBookmarks(SceneObject obj)
		{
			foreach (TrackedSceneObject bookmark in _bookmarks)
			{
				if (bookmark.sceneObject == obj) return true;
			}

			return false;
		}

		/// <summary>
		/// Remove an object from bookmarks.
		/// </summary>
		/// <param name="obj">Object to remove</param>
		private void RemoveBookmark(SceneObject obj)
		{
			foreach (TrackedSceneObject bookmark in _bookmarks)
			{
				if (bookmark.sceneObject == obj)
				{
					_bookmarks.Remove(bookmark);
					return;
				}
			}
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

			if (_selected.sceneObject.gameObject.activeSelf)
			{
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Teleport to", GUILayout.MaxWidth(100)))
					GameUtilities.TeleportPlayerWithParent(_selected.sceneObject.gameObject.transform.position + Vector3.up * 2f);

				GUILayout.Space(5);

				if (GUILayout.Button("Teleport here", GUILayout.MaxWidth(100)))
				{
					Vector3 position = mainscript.M.player.lookPoint + Vector3.up * 0.75f;
					Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, -mainscript.M.player.mainCam.transform.right);

					_selected.sceneObject.gameObject.transform.position = position;
					_selected.sceneObject.gameObject.transform.rotation = rotation;
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
