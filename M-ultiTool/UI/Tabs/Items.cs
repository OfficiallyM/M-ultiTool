using MultiTool.Data;
using MultiTool.Extensions;
using MultiTool.Services;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.UI.Tabs
{
	internal class ItemsTab : Tab
	{
		public override string Name => "Items";
		public override bool HasConfigPane => true;
		private string _configTitle = "Configuration";
		public override string ConfigTitle => _configTitle;
		private ItemSpawnConfig _spawnConfig = new ItemSpawnConfig();
		public override ISpawnConfig SpawnConfig => _spawnConfig;


		// Scroll vectors.
		private Vector2 _itemScrollPosition;
		private Vector2 _configScrollPosition;
		private Vector2 _filterScrollPosition;

		// Main tab variables.
		private Rect _dimensions;
		private bool _filterShow = false;
		private List<int> _filters = new List<int>();
		private string _search = string.Empty;
		private string _lastSearch = string.Empty;
		private float _lastWidth = 0;
		private int _lastRowLength = 0;
		private List<List<Item>> _itemsChunked = new List<List<Item>>();
		private bool _rechunk = false;

		private bool _showSpawnHistory = false;
		private List<GameObject> _spawnedObjects = new List<GameObject>();

		public override void OnRegister()
		{
			_spawnConfig.MaxFuelType = (int)Enum.GetValues(typeof(mainscript.fluidenum)).Cast<mainscript.fluidenum>().Max();
			_spawnConfig.MaxCondition = (int)Enum.GetValues(typeof(Condition)).Cast<Condition>().Max();
		}

		public override void OnUnregister()
		{
			_spawnConfig.FuelValues.Clear();
			_spawnConfig.FuelTypes.Clear();
		}

		public override void Update()
		{
			List<Item> items = Services.Database.Items;
			if (_search != _lastSearch)
			{
				items = Services.Database.Items.Where(i => i.GameObject.name.ToLower().Contains(_search.ToLower())).ToList();
				_rechunk = true;
				_lastSearch = _search;
				_itemScrollPosition = new Vector2(0, 0);
			}

			if (_filters.Count > 0 && _rechunk)
			{
				items = items.Where(v => _filters.Contains(v.Category.Value)).ToList();
				_rechunk = true;
				_itemScrollPosition = new Vector2(0, 0);
			}

			float width = _dimensions.width;
			if (_filterShow)
				width -= 200f;

			int rowLength = Mathf.FloorToInt(width / 150f);
			if (_lastRowLength != rowLength || _rechunk)
			{
				_itemsChunked = items.ChunkBy(rowLength);
				_lastRowLength = rowLength;
				_lastWidth = rowLength * 150f;

				_rechunk = false;
			}

			_configTitle = _showSpawnHistory ? "Spawn history" : "Configuration";
		}

		public override void RenderTab(Rect dimensions)
		{
			_dimensions = dimensions;

			GUILayout.BeginArea(dimensions);
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Search:", GUILayout.MaxWidth(50));
			GUILayout.Space(5);
			_search = GUILayout.TextField(_search, GUILayout.MaxWidth(500));
			GUILayout.Space(5);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(70)))
			{
				_search = string.Empty;
				_lastSearch = string.Empty;
				_rechunk = true;
			}
			GUILayout.FlexibleSpace();

			MultiTool.Tools.RenderControl("delete_mode");

			GUILayout.Space(10);

			if (GUILayout.Button("Filters", GUILayout.Width(200)))
			{
				_filterShow = !_filterShow;
				_rechunk = true;
			}

			GUILayout.EndHorizontal();
			GUILayout.Space(10);

			GUILayout.BeginHorizontal();
			_itemScrollPosition = GUILayout.BeginScrollView(_itemScrollPosition);
			GUILayout.BeginVertical(GUILayout.MaxWidth(_lastWidth));
			foreach (List<Item> itemsRow in _itemsChunked)
			{
				GUILayout.BeginHorizontal();
				foreach (Item item in itemsRow)
				{
					// An item is broken, remove it from the list and trigger a rechunk
					// to avoid gaps in the layout.
					if (item.GameObject == null)
					{
						Services.Database.Items.Remove(item);
						_rechunk = true;
						break;
					}

					GUILayout.Box("", "button", GUILayout.Width(140), GUILayout.Height(140));
					Rect boxRect = GUILayoutUtility.GetLastRect();
					bool buttonImage = GUI.Button(new Rect(boxRect.x + 10f, boxRect.y - 10f, boxRect.width - 20f, boxRect.height - 20f), item.Thumbnail, "ButtonTransparent");
					bool buttonText = GUI.Button(new Rect(boxRect.x, boxRect.y + (boxRect.height / 2), boxRect.width, boxRect.height / 2), item.GameObject?.name ?? "Unknown", "ButtonTransparent");
					if (buttonImage || buttonText)
					{
						GameObject spawned = SpawnUtilities.Spawn(item, _spawnConfig);

						if (spawned != null)
							_spawnedObjects.Add(spawned);
					}
					GUILayout.Space(5);
				}
				GUILayout.EndHorizontal();
				GUILayout.Space(5);
			}
			GUILayout.EndVertical();
			GUILayout.EndScrollView();
			if (_filterShow)
			{
				GUILayout.FlexibleSpace();
				GUILayout.BeginVertical(GUILayout.MaxWidth(205));
				_filterScrollPosition = GUILayout.BeginScrollView(_filterScrollPosition);
				for (int i = 0; i < Services.Database.GetCategories().Count; i++)
				{
					string name = Services.Database.GetCategories()[i];
					if (GUILayout.Button(Accessibility.GetAccessibleString(name, _filters.Contains(i))))
					{
						if (_filters.Contains(i))
							_filters.Remove(i);
						else
							_filters.Add(i);
						_rechunk = true;

						_itemScrollPosition = new Vector2(0, 0);
					}
				}
				GUILayout.EndScrollView();
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		public override void RenderConfigPane(Rect dimensions)
		{
			GUILayout.BeginArea(dimensions);
			GUILayout.BeginVertical();
			GUILayout.Space(10);
			_configScrollPosition = GUILayout.BeginScrollView(_configScrollPosition);

			if (_showSpawnHistory)
			{
				if (GUILayout.Button("Switch to configuration"))
				{
					_showSpawnHistory = !_showSpawnHistory;
					_configScrollPosition = Vector2.zero;
				}
				GUILayout.Space(10);

				if (_spawnedObjects.Count == 0)
				{
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUILayout.Label("Nothing has been spawned yet");
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}

				foreach (GameObject obj in _spawnedObjects)
				{
					try
					{
						if (obj == null)
						{
							continue;
						}

						bool isVehicle = GameUtilities.IsVehicleOrTrailer(obj);

						if (isVehicle) continue;

						string name = obj.name ?? "Unknown";
						name = name.Replace("(Clone)", string.Empty);

						GUILayout.Label(name);
						GUILayout.BeginHorizontal();
						if (GUILayout.Button("Teleport to", GUILayout.MaxWidth(100)))
							GameUtilities.TeleportPlayerWithParent(obj.transform.position + Vector3.up * 2f);

						GUILayout.Space(5);

						if (GUILayout.Button("Teleport here", GUILayout.MaxWidth(100)))
						{
							Vector3 position = mainscript.M.player.lookPoint + Vector3.up * 0.75f;
							Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, -mainscript.M.player.mainCam.transform.right);

							obj.transform.position = position;
							obj.transform.rotation = rotation;
						}

						GUILayout.Space(5);

						if (GUILayout.Button("Delete", GUILayout.MaxWidth(100)))
						{
							tosaveitemscript save = obj.GetComponent<tosaveitemscript>();
							if (save != null)
							{
								save.removeFromMemory = true;

								foreach (tosaveitemscript component in obj.transform.root.GetComponentsInChildren<tosaveitemscript>())
								{
									component.removeFromMemory = true;
								}
								UnityEngine.Object.Destroy(obj);
								_spawnedObjects.Remove(obj);
								break;
							}
						}
						GUILayout.EndHorizontal();
						GUILayout.Space(10);
					}
					catch (Exception ex)
					{
						Logger.Log($"Spawn history error for item {obj.name ?? "Unknown"}. Details: {ex}");
						_spawnedObjects.Remove(obj);
						break;
					}
				}
			}
			else
			{
				if (GUILayout.Button("Switch to spawn history"))
				{
					_showSpawnHistory = !_showSpawnHistory;
					_configScrollPosition = Vector2.zero;
				}
				GUILayout.Space(10);

				SpawnConfig?.Render(dimensions);
			}

			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}
}
