using MultiTool.Core;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MultiTool.Modules;
using MultiTool.Utilities.UI;
using MultiTool.Extensions;
using Logger = MultiTool.Modules.Logger;

namespace MultiTool.Tabs
{
	internal class ItemsTab : Tab
	{
		public override string Name => "Items";
		public override bool HasConfigPane => true;
		private string _configTitle = "Configuration";
		public override string ConfigTitle => _configTitle;

		private Settings _settings = new Settings();

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

        // Config variables.
        private int _maxFuelType = 0;
        private int _maxCondition = 0;
        private int _condition = 0;
        private int _fuelMixes = 1;
        private List<float> _fuelValues = new List<float> { -1f };
        private List<int> _fuelTypes = new List<int> { -1 };
        private string _plate = string.Empty;

		private bool _showSpawnHistory = false;

		public override void OnRegister()
        {
            _maxFuelType = (int)Enum.GetValues(typeof(mainscript.fluidenum)).Cast<mainscript.fluidenum>().Max();
            _maxCondition = (int)Enum.GetValues(typeof(Item.Condition)).Cast<Item.Condition>().Max();
        }

        public override void OnUnregister()
        {
            _fuelValues.Clear();
            _fuelTypes.Clear();
        }

        public override void Update()
        {
            List<Item> items = GUIRenderer.items;
            if (_search != _lastSearch)
            {
                items = GUIRenderer.items.Where(i => i.gameObject.name.ToLower().Contains(_search.ToLower())).ToList();
                _rechunk = true;
				_lastSearch = _search;
				_itemScrollPosition = new Vector2(0, 0);
            }

            if (_filters.Count > 0 && _rechunk)
            {
                items = items.Where(v => _filters.Contains(v.category)).ToList();
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

			GUILayout.FlexibleSpace();

			// Delete mode.
			if (GUILayout.Button(Accessibility.GetAccessibleString("Delete mode", _settings.deleteMode) + $" (Press {MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.deleteMode).key})", GUILayout.MaxWidth(250)))
			{
				_settings.deleteMode = !_settings.deleteMode;
			}

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
					if (item.gameObject == null)
					{
						GUIRenderer.items.Remove(item);
						_rechunk = true;
						break;
					}

                    GUILayout.Box("", "button", GUILayout.Width(140), GUILayout.Height(140));
                    Rect boxRect = GUILayoutUtility.GetLastRect();
                    bool buttonImage = GUI.Button(new Rect(boxRect.x + 10f, boxRect.y - 10f, boxRect.width - 20f, boxRect.height - 20f), item.thumbnail, "ButtonTransparent");
                    bool buttonText = GUI.Button(new Rect(boxRect.x, boxRect.y + (boxRect.height / 2), boxRect.width, boxRect.height / 2), item.gameObject?.name ?? "Unknown", "ButtonTransparent");
                    if (buttonImage || buttonText)
                    {
						GameObject spawned = SpawnUtilities.Spawn(new Item()
						{
							gameObject = item.gameObject,
							conditionInt = _condition,
							fuelMixes = _fuelMixes,
							fuelValues = _fuelValues,
							fuelTypeInts = _fuelTypes,
							color = Colour.GetColour(),
							plate = _plate,
							amt = item.amt,
						});

						if (spawned != null)
							GUIRenderer.spawnedObjects.Add(spawned);
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
                for (int i = 0; i < GUIRenderer.categories.Count; i++)
                {
                    string name = GUIRenderer.categories.ElementAt(i).Key;
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

				if (GUIRenderer.spawnedObjects.Count == 0)
				{
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUILayout.Label("Nothing has been spawned yet");
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}

				foreach (GameObject obj in GUIRenderer.spawnedObjects)
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
								GUIRenderer.spawnedObjects.Remove(obj);
								break;
							}
						}
						GUILayout.EndHorizontal();
						GUILayout.Space(10);
					}
					catch (Exception ex)
					{
						Logger.Log($"Spawn history error for item {obj.name ?? "Unknown"}. Details: {ex}");
						GUIRenderer.spawnedObjects.Remove(obj);
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

				// Condition.
				GUILayout.Label($"Condition: {(Item.Condition)_condition}");
				_condition = Mathf.RoundToInt(GUILayout.HorizontalSlider(_condition, -1, _maxCondition));
				GUILayout.Space(10);

				// Plate.
				GUILayout.Label("Plate (blank for random):");
				_plate = GUILayout.TextField(_plate);
				GUILayout.Space(10);

				// Spawn with fuel.
				if (GUILayout.Button(Accessibility.GetAccessibleString("Spawn with fuel", _settings.spawnWithFuel)))
					_settings.spawnWithFuel = !_settings.spawnWithFuel;
				GUILayout.Space(10);

				// Fuel mixes.
				for (int i = 0; i < _fuelMixes; i++)
				{
					GUILayout.BeginVertical($"Fluid {i + 1}", "box");
					GUILayout.Space(10);

					// Fluid type.
					string fuelType = ((mainscript.fluidenum)_fuelTypes[i]).ToString();
					if (_fuelTypes[i] == -1)
						fuelType = "Default";
					else
						fuelType = fuelType[0].ToString().ToUpper() + fuelType.Substring(1);
					GUILayout.Label($"Fluid type: {fuelType}");
					_fuelTypes[i] = Mathf.RoundToInt(GUILayout.HorizontalSlider(_fuelTypes[i], -1, _maxFuelType));

					GUILayout.Space(10);

					// Fluid amount.
					GUILayout.Label($"Fuel amount: {_fuelValues[i]}");
					_fuelValues[i] = GUILayout.HorizontalSlider(_fuelValues[i], -1f, 1000f);

					bool fuelValueParse = float.TryParse(GUILayout.TextField(_fuelValues[i].ToString()), out float tempFuelValue);
					if (fuelValueParse)
						_fuelValues[i] = tempFuelValue;

					GUILayout.EndVertical();
					GUILayout.Space(5);
				}
				GUILayout.Space(5);

				GUILayout.BeginHorizontal();
				if (_fuelMixes <= _maxFuelType && GUILayout.Button("Add fluid"))
				{
					_fuelMixes++;
					_fuelTypes.Add(0);
					_fuelValues.Add(0);
				}
				GUILayout.Space(10);

				if (_fuelMixes > 1 && GUILayout.Button("Remove last fluid"))
				{
					_fuelMixes--;
					_fuelTypes.RemoveAt(_fuelTypes.Count - 1);
					_fuelValues.RemoveAt(_fuelValues.Count - 1);
				}
				GUILayout.EndHorizontal();
				GUILayout.Space(10);

				Colour.RenderColourSliders(dimensions.width);
				if (GUILayout.Button("Randomise colour", GUILayout.MaxWidth(200)))
				{
					Color color = Colour.GetColour();
					color.r = UnityEngine.Random.Range(0f, 255f) / 255f;
					color.g = UnityEngine.Random.Range(0f, 255f) / 255f;
					color.b = UnityEngine.Random.Range(0f, 255f) / 255f;
					Colour.SetColour(color);
				}
				GUILayout.Space(10);
			}

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
