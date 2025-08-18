using MultiTool.Components;
using MultiTool.Core;
using MultiTool.Extensions;
using MultiTool.Modules;
using MultiTool.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
		public override string ConfigTitle => GetConfigTitle();
		public override bool HasCache => true;
		public override int CacheRefreshTime => _componentRefreshInterval;

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
		private Tracked _selected = new Tracked();

		// Hierarchy interaction.
		private enum HierarchySelectionMode
		{
			Normal,
			ChangeParent,
		}
		private HierarchySelectionMode _selectionMode = HierarchySelectionMode.Normal;
		private GameObject _parentChangingObject;
		private GameObject _createdGameObject;

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

		// Configuration.
		private Dictionary<int, string> _layers = new Dictionary<int, string>();
		private bool _layersExpanded = false;
		private float _transformStepSize = 0.1f;
		private List<MemberInfo> _componentMembers = new List<MemberInfo>();
		private List<MemberInfo> _cachedComponentMembers = new List<MemberInfo>();
		private static List<MemberInfo> _expandedMembers = new List<MemberInfo>();
		private Dictionary<MemberInfo, (object, Type)> _memberValuesCache = new Dictionary<MemberInfo, (object, Type)>();
		private DateTime _lastComponentRefresh = DateTime.Now;
		private bool _automaticComponentRefresh = false;
		private int _componentRefreshInterval = 2;
		private string _componentSearch = "";
		private bool _isComponentSearching = false;
		private CancellationTokenSource _componentSearchCts;
		private enum SortMode
		{
			FieldOrder,
			NameAscending,
			NameDescending
		}
		private SortMode _componentSortMode = SortMode.FieldOrder;
		private string _addComponentSearch;
		private List<Type> _addComponentSuggestions = new List<Type>();
		private static readonly HashSet<string> _excludedMembers = new HashSet<string>
		{
			"useGUILayout",
			"enabled",
			"isActiveAndEnabled",
			"transform",
			"gameObject",
			"tag",
			"name",
			"hideFlags"
		};
		private readonly List<(Type type, Func<MemberInfo, object, object> renderer)> _renderers = new List<(Type, Func<MemberInfo, object, object>)>
		{
			(typeof(Transform), (member, obj) => {
				RenderMemberExpandButton(member);
				if (IsMemberExpanded(member))
					return GUILayoutExtensions.RenderTransform((Transform)obj, 100);
				return obj;
			}),
			(typeof(Color), (member, obj) => {
				RenderMemberExpandButton(member);
				if (IsMemberExpanded(member))
					return GUILayoutExtensions.ColorField((Color)obj);
				return obj;
			}),
			(typeof(Vector3), (member, obj) => {
				RenderMemberExpandButton(member);
				if (IsMemberExpanded(member))
					return GUILayoutExtensions.Vector3Field((Vector3)obj);
				return obj;
			}),
			(typeof(string), (member, obj) => GUILayout.TextField((string)obj)),
			(typeof(float), (member, obj) => {
				if (float.TryParse(GUILayout.TextField(((float)obj).ToString("F4")), out float result))
					return result;
				return obj;
			}),
			(typeof(int), (member, obj) => {
				if (int.TryParse(GUILayout.TextField(((int)obj).ToString()), out int result))
					return result;
				return obj;
			}),
			(typeof(bool), (member, obj) => GUILayout.Toggle((bool)obj, member.Name)),
		};

		public override void OnRegister()
		{
			DataFetcher.I.StartScan();

			// Get available layer names.
			_layers = Enumerable.Range(0, 32)
				.Where(i => !string.IsNullOrEmpty(LayerMask.LayerToName(i)))
				.ToDictionary(i => i, i => LayerMask.LayerToName(i));
		}

		public override void OnCacheRefresh()
		{
			if (!_automaticComponentRefresh) return;
			UpdateMemberValues();
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
			SearchAsync(_search);

			// Support for scrolling to newly created objects.
			if (_createdGameObject != null)
			{
				SceneObject createdSceneObject = GetSceneObjectFromGameObject(_createdGameObject);
				_selected.trackedSceneObject = new TrackedSceneObject(_createdGameObject.GetInstanceID(), createdSceneObject);
				_selected.trackedComponent = null;
				ScrollToObject();
			}
		}

		private string GetConfigTitle()
		{
			string title = "Select object to edit";
			if (_selected.trackedComponent != null)
				title = _selected.trackedComponent.component.GetType().Name;
			else if (_selected.trackedSceneObject?.sceneObject?.gameObject != null)
				title = _selected.trackedSceneObject?.sceneObject?.gameObject?.name;
			else if (_parentChangingObject != null)
				title = _parentChangingObject.name;
			return title;
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
			if (_selected.trackedSceneObject != null && !_selected.trackedSceneObject.Refresh(_cachedObjects) && _selectionMode == HierarchySelectionMode.Normal)
				_selected.trackedSceneObject = null;

			if (_selected.trackedComponent != null && !_selected.trackedComponent.Refresh(_cachedObjects))
				_selected.trackedComponent = null;

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
			// Wrap the entire tab rendering in a try-catch to prevent
			// tab crashing due to unstable data.
			try
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
						RefreshData();
						return;
					}
					GUILayout.Space(10);
					DateTime lastScan = DataFetcher.I.GetLastScanTime();
					TimeSpan elapsed = DateTime.Now - lastScan;
					if (elapsed.TotalMinutes < 1)
						GUILayout.Label($"Last refreshed {(int)elapsed.TotalSeconds} second(s) ago");
					else
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

					GUILayout.BeginHorizontal("box");
					if (GUILayout.Button("Create new object", GUILayout.ExpandWidth(false)))
					{
						_createdGameObject = new GameObject("New GameObject");
						RefreshData();
					}
					GUILayout.Space(5);

					if (_selected.trackedSceneObject != null && GUILayout.Button("Scroll to selected", GUILayout.ExpandWidth(false)))
						ScrollToObject();

					GUILayout.FlexibleSpace();

					if (_bookmarks.Count > 0 && GUILayout.Button($"{(_isBookmarksExpanded ? "-" : "+")} Bookmarks", GUILayout.ExpandWidth(false)))
						_isBookmarksExpanded = !_isBookmarksExpanded;
					GUILayout.EndHorizontal();

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
						if (_selectionMode != HierarchySelectionMode.Normal)
						{
							GUILayout.BeginHorizontal();
							string mode = "parent change mode";
							GUILayout.Label($"Hierachy in {mode}", "LabelSubHeader", GUILayout.ExpandWidth(false));
							if (GUILayout.Button("Cancel", GUILayout.ExpandWidth(false)))
								_selectionMode = HierarchySelectionMode.Normal;
							GUILayout.EndHorizontal();
						}
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
									_selected.trackedSceneObject = new TrackedSceneObject(bookmark.sceneObject.gameObject.GetInstanceID(), bookmark.sceneObject);
									_selected.trackedComponent = null;
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
			catch (Exception ex)
			{
				Logger.Log($"Error in component browser tab render. Details: {ex}", Logger.LogLevel.Error);
			}
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
			GUILayout.Label("Match objects on a layer\nExample: 'layer:player' would find any objects on the \"player\" layer");
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

				GUILayout.BeginHorizontal(_selected?.trackedSceneObject?.sceneObject == obj ? "BoxGrey" : "box");
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
					switch (_selectionMode)
					{
						case HierarchySelectionMode.Normal:
							if (_selected.trackedSceneObject != null && _selected.trackedSceneObject?.sceneObject == obj)
								_selected.trackedSceneObject = null;
							else
								_selected.trackedSceneObject = new TrackedSceneObject(obj.gameObject.GetInstanceID(), obj);
							_selected.trackedComponent = null;
							break;
						case HierarchySelectionMode.ChangeParent:
							ChangeParent(obj);
							break;
					}
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
		/// Refresh hierarchy data.
		/// </summary>
		private void RefreshData()
		{
			_objects = null;
			_cachedObjects = null;
			DataFetcher.I.StartScan();
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
			if (obj == null) obj = _selected?.trackedSceneObject?.sceneObject;
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
		/// Get a SceneObject from a given GameObject.
		/// </summary>
		/// <param name="obj">GameObject to find</param>
		/// <returns>SceneObject if found, otherwise null</returns>
		private SceneObject GetSceneObjectFromGameObject(GameObject obj)
		{
			if (obj == null) return null;

			foreach (var root in _cachedObjects)
			{
				var found = FindSceneObjectRecursive(root, obj);
				if (found != null)
					return found;
			}

			return null;
		}

		/// <summary>
		/// Recursively search for gameObject within a scene.
		/// </summary>
		/// <param name="current">Current SceneObject</param>
		/// <param name="target">The target GameObject</param>
		/// <returns>SceneObject if found, otherwise null</returns>
		private SceneObject FindSceneObjectRecursive(SceneObject current, GameObject target)
		{
			if (current == null) return null;

			if (current.gameObject == target)
				return current;

			foreach (var child in current.children)
			{
				var found = FindSceneObjectRecursive(child, target);
				if (found != null)
					return found;
			}

			return null;
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

		/// <summary>
		/// Change selected object parent.
		/// </summary>
		/// <param name="targetObj">New parent object</param>
		private void ChangeParent(SceneObject targetObj)
		{
			GameObject targetGameObject = targetObj.gameObject;

			if (_parentChangingObject == null) return;

			// Can't parent to self.
			if (targetGameObject == _parentChangingObject)
			{
				Notifications.SendWarning("Parent change", "You cannot set an object as its own parent.");
				return;
			}

			// Can't parent to a descendant.
			if (IsDescendantOf(targetGameObject.transform, _parentChangingObject.transform))
			{
				Notifications.SendWarning("Parent change", "You cannot make an object a child of its own descendant.");
				return;
			}

			// Already the parent.
			if (_parentChangingObject.transform.parent == targetGameObject.transform)
			{
				Notifications.SendWarning("Parent change", "This object is already a child of the selected parent.");
				return;
			}

			_parentChangingObject.transform.SetParent(targetGameObject.transform);
			RefreshData();
			Notifications.SendSuccess("Parent change", $"'{_parentChangingObject.name}' is now a child of '{targetGameObject.name}'.");
			_parentChangingObject = null;
			_selectionMode = HierarchySelectionMode.Normal;
		}

		/// <summary>
		/// Check if transform is a descendant of another transform.
		/// </summary>
		/// <param name="potentialParent">Parent transform</param>
		/// <param name="potentialChild">Child transform</param>
		/// <returns>True if child transform is a child of parent, otherwise false</returns>
		private bool IsDescendantOf(Transform potentialParent, Transform potentialChild)
		{
			foreach (Transform child in potentialParent)
			{
				if (child == potentialChild)
					return true;
				if (IsDescendantOf(child, potentialChild))
					return true;
			}
			return false;
		}

		public override void RenderConfigPane(Rect dimensions)
		{
			// Wrap the entire config rendering in a try-catch to prevent
			// tab crashing due to unstable data.
			try
			{
				if (_selected?.trackedSceneObject == null && _selected?.trackedComponent == null) return;
			
				GUILayout.BeginArea(dimensions);
				GUILayout.BeginHorizontal();
				GUILayout.Space(5);
				GUILayout.BeginVertical();
				GUILayout.Space(10);

				if (_selected.trackedComponent != null)
					RenderComponentConfig();
				else if (_selectionMode == HierarchySelectionMode.ChangeParent || _parentChangingObject != null)
				{
					GUILayout.Label("Changing parent", "LabelSubHeader");
					if (GUILayout.Button("Cancel", GUILayout.ExpandWidth(false)))
						_selectionMode = HierarchySelectionMode.Normal;
				}
				else
					RenderObjectConfig();

				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
				GUILayout.EndArea();
			}
			catch (Exception ex)
			{
				Logger.Log($"Error in component browser config render. Details: {ex}", Logger.LogLevel.Error);
			}
		}

		/// <summary>
		/// Render object configuration for the selected object.
		/// </summary>
		private void RenderObjectConfig()
		{
			SceneObject sceneObject = _selected.trackedSceneObject.sceneObject;
			GameObject gameObject = sceneObject.gameObject;
			if (gameObject == null) return;
			Transform transform = gameObject.transform;

			_configPosition = GUILayout.BeginScrollView(_configPosition);

			GUILayout.Label("Actions", "LabelHeader");
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Teleport to", GUILayout.ExpandWidth(false)))
				GameUtilities.TeleportPlayerWithParent(gameObject.transform.position + Vector3.up * 2f);
			GUILayout.Space(5);

			if (GUILayout.Button("Teleport here", GUILayout.ExpandWidth(false)))
			{
				Vector3 position = mainscript.M.player.lookPoint + Vector3.up * 0.75f;
				Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, -mainscript.M.player.mainCam.transform.right);

				gameObject.transform.position = position;
				gameObject.transform.rotation = rotation;
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();

			if (GUILayout.Button("Change parent", GUILayout.ExpandWidth(false)))
			{
				_selectionMode = HierarchySelectionMode.ChangeParent;
				_parentChangingObject = gameObject;
			}
			GUILayout.Space(5);

			if (GUILayout.Button("Duplicate", GUILayout.ExpandWidth(false)))
			{
				GameObject clone = GameObject.Instantiate(gameObject);
				clone.transform.parent = gameObject.transform.parent;
				RefreshData();
			}
			GUILayout.Space(5);

			if (GUILayout.Button("Delete", "ButtonSecondary", GUILayout.ExpandWidth(false)))
			{
				tosaveitemscript save = gameObject.GetComponent<tosaveitemscript>();
				if (save != null)
				{
					save.removeFromMemory = true;

					foreach (tosaveitemscript childSave in gameObject.GetComponentsInChildren<tosaveitemscript>())
					{
						childSave.removeFromMemory = true;
					}
				}
				UnityEngine.Object.Destroy(gameObject);
				_selected.trackedSceneObject = null;
				_selected.trackedComponent = null;
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(20);

			GUILayout.Label("Details", "LabelHeader");
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("", gameObject.activeSelf ? "BoxGreen" : "BoxGrey", GUILayout.Width(25)))
			{
				gameObject.SetActive(!gameObject.activeSelf);
			}
			GUILayout.Label("activeSelf");
			gameObject.name = GUILayout.TextField(gameObject.name, GUILayout.MaxWidth(300));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("", gameObject.isStatic ? "BoxGreen" : "BoxGrey", GUILayout.Width(25)))
			{
				gameObject.isStatic = !gameObject.isStatic;
			}
			GUILayout.Label("isStatic");
			GUILayout.Label($"Instance ID: {gameObject.GetInstanceID()}");
			GUILayout.Label($"Tag: {gameObject.tag}");
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Layer:", GUILayout.ExpandWidth(false));
			GUILayout.BeginVertical();
			if (GUILayout.Button(_layers[gameObject.layer] ?? "Unknown", GUILayout.MaxWidth(150)))
				_layersExpanded = !_layersExpanded;
			if (_layersExpanded)
			{
				foreach (var layer in _layers)
				{
					if (layer.Key == gameObject.layer) continue;

					if (GUILayout.Button(layer.Value, GUILayout.MaxWidth(150)))
					{
						gameObject.layer = layer.Key;
						_layersExpanded = false;
					}
				}
			}
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			GUILayout.Space(20);

			GUILayout.Label("Transform", "LabelHeader");
			GUILayout.Label("Step size (For -/+ buttons)");
			float.TryParse(GUILayout.TextField(Mathf.Abs(_transformStepSize).ToString("F2"), GUILayout.MaxWidth(100)), out _transformStepSize);
			GUILayout.Space(5);
			GUILayoutExtensions.RenderTransform(transform, 100, _transformStepSize);
			GUILayout.Space(20);

			GUILayout.Label("Components", "LabelHeader");

			GUILayout.BeginVertical("box");
			GUILayout.Label("Add new component", "LabelSubHeader");
			string newSearch = GUILayout.TextField(_addComponentSearch, GUILayout.MaxWidth(300));
			if (newSearch != _addComponentSearch)
			{
				_addComponentSearch = newSearch;
				UpdateComponentSuggestions();
			}

			if (_addComponentSuggestions.Count > 0)
			{
				foreach (var type in _addComponentSuggestions.Take(10))
				{
					if (GUILayout.Button(type.FullName))
					{
						try
						{
							gameObject.AddComponent(type);
							Notifications.SendSuccess("Component added", $"{type.Name} added successfully.");
							_addComponentSearch = "";
							_addComponentSuggestions.Clear();
						}
						catch (Exception ex)
						{
							Notifications.SendError("Error", $"Failed to add component {type.Name}");
							Logger.Log($"Failed to add component {type.Name}. Details: {ex}", Logger.LogLevel.Error);
						}
					}
				}
			}
			GUILayout.EndVertical();

			GUILayout.Label("Current components", "LabelSubHeader");
			Component[] components = gameObject.GetComponents<Component>();
			int rendered = 0;
			foreach (Component component in components)
			{
				string componentName = component.GetType().Name;
				string[] ignoredComponents = new string[] { "Transform" };
				if (ignoredComponents.Contains(componentName))
					continue;

				GUILayout.BeginHorizontal("box");
				if (GUILayout.Button(componentName))
				{
					_selected.trackedComponent = new TrackedComponent(component, gameObject.GetInstanceID(), sceneObject);
					_configPosition = Vector2.zero;
					UpdateMembers();
					_expandedMembers.Clear();
				}

				if (GUILayout.Button("X", GUILayout.MaxWidth(30)))
				{
					UnityEngine.Object.Destroy(component);
					break;
				}
				GUILayout.EndHorizontal();
				rendered++;
			}
			if (rendered == 0)
			{
				GUILayout.Label("No components");
			}

			GUILayout.EndScrollView();
		}

		/// <summary>
		/// Render component configuration for the selected object and component.
		/// </summary>
		private void RenderComponentConfig()
		{
			Component component = _selected.trackedComponent.component;

			GUILayout.BeginVertical("box");
			GUILayout.BeginHorizontal();
			GUILayout.Label("Search", GUILayout.ExpandWidth(false));
			string newSearch = GUILayout.TextField(_componentSearch, GUILayout.MaxWidth(300));
			if (newSearch != _componentSearch)
			{
				_componentSearch = newSearch;
				ComponentSearchAsync(_componentSearch);
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button($"Sort by {GetSortName(_componentSortMode)}", GUILayout.ExpandWidth(false)))
			{
				_componentSortMode = (SortMode)(((int)_componentSortMode + 1) % Enum.GetValues(typeof(SortMode)).Length);
				ComponentSearchAsync(_componentSearch);
			}
			GUILayout.Space(5);
			if (GUILayout.Button("Copy as JSON", GUILayout.ExpandWidth(false)))
			{
				CopyComponentToJson();
				Notifications.SendSuccess("Success", "Copied component to clipboard");
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUILayout.Space(5);

			if (_isComponentSearching)
			{
				GUILayout.FlexibleSpace();
				GUILayout.Label("Searching...", "LabelMessage");
				GUILayout.FlexibleSpace();
			}
			else
			{
				_configPosition = GUILayout.BeginScrollView(_configPosition);
				foreach (MemberInfo member in _cachedComponentMembers)
				{
					GUILayout.BeginVertical("box");
					GUILayout.BeginHorizontal();
					GUILayout.Label($"{member.Name}", "LabelSubHeader", GUILayout.ExpandWidth(false));
					(object current, Type type) = _memberValuesCache[member];
					if (type == null)
						GUILayout.Label("NULL", "LabelLeft");
					else
						GUILayout.Label($"{GetAccessLevel(member)} {type.GetFriendlyName()}", "LabelLeft");
					GUILayout.EndHorizontal();
					if (current != null)
					{
						object next = null;
						bool handled = false;

						foreach (var (rendererType, renderer) in _renderers)
						{
							if (rendererType.IsInstanceOfType(current))
							{
								next = renderer(member, current);
								if (next != null && !Equals(current, next))
									SetValue(member, component, next);
								handled = true;
								break;
							}
						}

						if (!handled && current is Component memberComponent)
						{
							if (GUILayout.Button($"Select", "ButtonSecondary", GUILayout.MaxWidth(100)))
							{
								try
								{
									GameObject gameObject = memberComponent.gameObject;
									SceneObject sceneObj = GetSceneObjectFromGameObject(gameObject);
									_selected.trackedSceneObject = new TrackedSceneObject(gameObject.GetInstanceID(), sceneObj);
									_selected.trackedComponent = new TrackedComponent(memberComponent, gameObject.GetInstanceID(), sceneObj);
									ScrollToObject();
								}
								catch
								{
									_selected.trackedSceneObject = null;
									_selected.trackedComponent = new TrackedComponent(memberComponent);
								}
								_configPosition = Vector2.zero;
								UpdateMembers();
								_expandedMembers.Clear();
							}
							handled = true;
						}

						if (!handled)
						{
							GUILayout.Label($"Value: {current}");
							GUILayout.Label("NOTE: This type doesn't currently support editing");
						}
					}
					GUILayout.EndVertical();
					GUILayout.Space(10);
				}
				GUILayout.EndScrollView();
			}

			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal("box");
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			if (_selected.trackedSceneObject != null)
			{
				if (GUILayout.Button("< Back to object", GUILayout.ExpandWidth(false)))
				{
					_selected.trackedComponent = null;
					_configPosition = Vector2.zero;
					_componentMembers.Clear();
					_cachedComponentMembers.Clear();
					_expandedMembers.Clear();
				}
			}
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Refresh", GUILayout.ExpandWidth(false)))
			{
				UpdateMemberValues();
			}
			GUILayout.EndHorizontal();
			_automaticComponentRefresh = GUILayout.Toggle(_automaticComponentRefresh, "Automatically refresh");
			if (_automaticComponentRefresh)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Refresh interval: ", GUILayout.ExpandWidth(false));
				int.TryParse(GUILayout.TextField(_componentRefreshInterval.ToString(), GUILayout.ExpandWidth(false)), out int newInterval);
				if (newInterval != _componentRefreshInterval)
				{
					_componentRefreshInterval = newInterval;
					NextCacheUpdate = _componentRefreshInterval;
				}
				GUILayout.Label("s");
				GUILayout.EndHorizontal();
			}
			TimeSpan elapsed = DateTime.Now - _lastComponentRefresh;
			if (elapsed.TotalMinutes < 1)
				GUILayout.Label($"Last refreshed {(int)elapsed.TotalSeconds} second(s) ago");
			else
				GUILayout.Label($"Last refreshed {(int)elapsed.TotalMinutes} minute(s) ago");
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			GUILayout.Space(5);
		}

		/// <summary>
		/// Get all valid members for a given component.
		/// </summary>
		/// <param name="component">The component</param>
		/// <returns>List of MemberInfo</returns>
		private List<MemberInfo> GetMembers(Component component)
		{
			var members = new List<MemberInfo>();
			var type = component.GetType();

			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
										BindingFlags.Public | BindingFlags.NonPublic;

			foreach (var field in type.GetFields(flags))
			{
				if (!_excludedMembers.Contains(field.Name))
					members.Add(field);
			}

			foreach (var prop in type.GetProperties(flags))
			{
				if (!_excludedMembers.Contains(prop.Name))
					members.Add(prop);
			}

			return members;
		}

		/// <summary>
		/// Get members for a tracked component.
		/// </summary>
		private void UpdateMembers()
		{
			if (_selected.trackedComponent == null) return;

			_componentMembers = GetMembers(_selected.trackedComponent.component);
			_cachedComponentMembers = _componentMembers.ToList();
			UpdateMemberValues();
		}

		/// <summary>
		/// Update member values cache.
		/// </summary>
		private void UpdateMemberValues()
		{
			if (_selected.trackedComponent == null || _componentMembers == null || _componentMembers.Count == 0) return;

			_memberValuesCache.Clear();
			foreach (var member in _componentMembers)
			{
				try
				{
					object value = GetValue(member, _selected.trackedComponent.component);
					Type type = value?.GetType();
					_memberValuesCache[member] = (value, type);
				}
				catch (Exception ex)
				{
					Logger.Log($"Failed to get value for {member.Name}. Details: {ex}", Logger.LogLevel.Error);
				}
			}
			_lastComponentRefresh = DateTime.Now;
		}

		/// <summary>
		/// Trigger a component search, cancelling any existing searches.
		/// </summary>
		/// <param name="query">Query string</param>
		private async void ComponentSearchAsync(string query)
		{
			_componentSearchCts?.Cancel();
			_componentSearchCts = new CancellationTokenSource();
			var token = _componentSearchCts.Token;
			_isComponentSearching = true;

			try
			{
				_cachedComponentMembers = _componentMembers.ToList();
				if (query != string.Empty)
				{
					var result = await Task.Run(() => ComponentSearch(_cachedComponentMembers, query, token), token);
					if (!token.IsCancellationRequested)
					{
						_cachedComponentMembers = result;
					}
				}
				SortMembers();
				_position = Vector2.zero;
			}
			catch (OperationCanceledException)
			{
				// Search was cancelled, ignore.
			}
			_isComponentSearching = false;
		}

		/// <summary>
		/// Search components for a query string.
		/// </summary>
		/// <param name="members">Component members</param>
		/// <param name="query">Query string</param>
		/// <param name="token">CancellationToken</param>
		/// <returns>List of MemberInfo results</returns>
		private List<MemberInfo> ComponentSearch(List<MemberInfo> members, string query, CancellationToken token)
		{
			var results = new List<MemberInfo>();
			query = query.ToLowerInvariant();

			foreach (var member in members)
			{
				token.ThrowIfCancellationRequested();

				string memberName = member.Name.ToLowerInvariant();
				if (memberName.Contains(query))
					results.Add(member);
			}

			return results;
		}

		/// <summary>
		/// Sort component members cache.
		/// </summary>
		private void SortMembers()
		{
			switch (_componentSortMode)
			{
				case SortMode.NameAscending:
					_cachedComponentMembers = _cachedComponentMembers
						.OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase).ToList();
					break;
				case SortMode.NameDescending:
					_cachedComponentMembers = _cachedComponentMembers
						.OrderByDescending(m => m.Name, StringComparer.OrdinalIgnoreCase).ToList();
					break;
				case SortMode.FieldOrder:
				default:
					// No sort needed, result already matches field order from _componentMembers.
					break;
			}
		}

		/// <summary>
		/// Get pretty name for sort mode.
		/// </summary>
		/// <param name="mode">Sort mode</param>
		/// <returns>Prettified sort mode</returns>
		private string GetSortName(SortMode mode)
		{
			switch (_componentSortMode)
			{
				case SortMode.NameAscending:
					return "Name ascending";
				case SortMode.NameDescending:
					return "Name descending";
			}

			return "Field order";
		}

		/// <summary>
		/// Wrapper around field/property GetValue().
		/// </summary>
		/// <param name="member">MemberInfo</param>
		/// <param name="target">Object whose value will be returned</param>
		/// <returns>Object value or null if member is invalid</returns>
		private object GetValue(MemberInfo member, object target)
		{
			var field = member as FieldInfo;
			if (field != null)
				return field.GetValue(target);

			var prop = member as PropertyInfo;
			if (prop != null)
				return prop.GetValue(target, null);

			return null;
		}

		/// <summary>
		/// Wrapper around field/property SetValue().
		/// </summary>
		/// <param name="member">MemberInfo</param>
		/// <param name="target">Object whose value will be set</param>
		/// <param name="value">Value to set</param>
		private void SetValue(MemberInfo member, object target, object value)
		{
			try
			{
				var field = member as FieldInfo;
				if (field != null)
				{
					field.SetValue(target, value);
				}
				else
				{
					var prop = member as PropertyInfo;
					if (prop != null)
					{
						prop.SetValue(target, value, null);
					}
				}

				UpdateMemberValues();
			}
			catch (Exception ex)
			{
				Logger.Log($"Failed to set {member.Name}. Details: {ex}", Logger.LogLevel.Warning);
				Notifications.SendError("Save failed", $"Failed to set value for {member.Name}.");
			}
		}

		/// <summary>
		/// Check if member fields are expanded.
		/// </summary>
		/// <param name="member">MemberInfo to check</param>
		/// <returns>True if member is expanded, otherwise false</returns>
		private static bool IsMemberExpanded(MemberInfo member)
		{
			return _expandedMembers.Contains(member);
		}

		/// <summary>
		/// Render button to expand member fields.
		/// </summary>
		/// <param name="member">MemberInfo to render expand button for</param>
		private static void RenderMemberExpandButton(MemberInfo member)
		{
			bool expanded = IsMemberExpanded(member);
			if (GUILayout.Button($"{(expanded ? "- Hide" : "+ Show")} fields", GUILayout.MaxWidth(100)))
			{
				if (expanded)
					_expandedMembers.Remove(member);
				else
					_expandedMembers.Add(member);
			}
		}

		/// <summary>
		/// Get the access level for a given member.
		/// </summary>
		/// <param name="member">Member to get access level for</param>
		/// <returns>Access level as a string or "unknown"</returns>
		private string GetAccessLevel(MemberInfo member)
		{
			if (member is FieldInfo field)
			{
				if (field.IsPublic) return "public";
				if (field.IsPrivate) return "private";
				if (field.IsFamily) return "protected";
				if (field.IsAssembly) return "internal";
				if (field.IsFamilyOrAssembly) return "protected internal";
				if (field.IsFamilyAndAssembly) return "private protected";
			}
			else if (member is PropertyInfo prop)
			{
				var getter = prop.GetGetMethod(true);
				if (getter != null)
				{
					if (getter.IsPublic) return "public";
					if (getter.IsPrivate) return "private";
					if (getter.IsFamily) return "protected";
					if (getter.IsAssembly) return "internal";
					if (getter.IsFamilyOrAssembly) return "protected internal";
					if (getter.IsFamilyAndAssembly) return "private protected";
				}
			}
			return "unknown";
		}

		/// <summary>
		/// Export a component as JSON to clipboard.
		/// </summary>
		private void CopyComponentToJson()
		{
			var exportDict = _memberValuesCache.ToDictionary(
				kvp => kvp.Key.Name,
				kvp => FormatValue(kvp.Value.Item1)
			);

			string json = JsonConvert.SerializeObject(exportDict, Formatting.Indented);
			GUIUtility.systemCopyBuffer = json;
		}

		/// <summary>
		/// Format component values to be JSON safe.
		/// </summary>
		/// <param name="value">Value to format</param>
		/// <returns>JSON safe formatted value</returns>
		private object FormatValue(object value)
		{
			if (value == null) return null;
			try
			{
				if (value is UnityEngine.Object uo) return uo.name;
			}
			catch
			{
				return "unknown";
			}

			if (value is Vector3 v3)
				return new { v3.x, v3.y, v3.z };

			if (value is System.Collections.IEnumerable enumerable && !(value is string))
				return enumerable.Cast<object>().Select(FormatValue).ToList();

			if (value.GetType().IsPrimitive || value is string)
				return value;

			return value.ToString();
		}

		/// <summary>
		/// Update the component autocomplete from search query.
		/// </summary>
		private void UpdateComponentSuggestions()
		{
			if (string.IsNullOrWhiteSpace(_addComponentSearch))
			{
				_addComponentSuggestions.Clear();
				return;
			}

			string query = _addComponentSearch.ToLowerInvariant();
			_addComponentSuggestions = DataUtilities.AllComponentTypes
				.Where(t => t.Name.ToLowerInvariant().Contains(query))
				.OrderBy(t => t.Name)
				.ToList();
		}
	}
}
