using MultiTool.Core;
using MultiTool.Extensions;
using MultiTool.Modules;
using MultiTool.Utilities;
using MultiTool.Utilities.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MultiTool.Core.Item;
using Logger = MultiTool.Modules.Logger;

namespace MultiTool.Tabs
{
	internal class POIsTab : Tab
	{
		public override string Name => "POIs";
		public override bool HasConfigPane => true;
		public override string ConfigTitle => "POI spawn history";

		private Vector2 _poiScrollPosition;
		private Vector2 _configScrollPosition;
		private bool _poiSpawnItems = true;

		private List<SpawnedPOI> _spawnedPOIs = new List<SpawnedPOI>();

		// Main tab variables.
		private Rect _dimensions;
		private string _search = string.Empty;
		private string _lastSearch = string.Empty;
		private float _lastWidth = 0;
		private List<List<POI>> _poisChunked = new List<List<POI>>();
		private bool _rechunk = false;

		public override void OnRegister()
		{
			_spawnedPOIs = SaveUtilities.LoadPOIs();
		}

		public override void Update()
		{
			List<POI> pois = GUIRenderer.POIs;
			if (_search != _lastSearch)
			{
				pois = GUIRenderer.POIs.Where(v => v.name.ToLower().Contains(_search.ToLower()) || v.poi.name.ToLower().Contains(_search.ToLower())).ToList();
				_rechunk = true;
				_lastSearch = _search;
				_poiScrollPosition = new Vector2(0, 0);
			}

			if (_lastWidth != _dimensions.width || _rechunk)
			{
				int rowLength = Mathf.FloorToInt(_dimensions.width / 150f);
				_poisChunked = pois.ChunkBy(rowLength);
				_lastWidth = _dimensions.width;

				_rechunk = false;
			}
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

			if (GUILayout.Button("Respawn nearest building items", GUILayout.MaxWidth(250)))
			{
				buildingscript closestBuilding = GameUtilities.FindNearestBuilding(mainscript.M.player.transform.position);

				if (closestBuilding != null)
				{
					// Trigger item respawn.
					closestBuilding.itemsSpawned = false;
					closestBuilding.SpawnStuff(0);
				}
			}
			GUILayout.Space(10);

			if (GUILayout.Button(Accessibility.GetAccessibleString("Spawn items", _poiSpawnItems), GUILayout.MaxWidth(100)))
				_poiSpawnItems = !_poiSpawnItems;
			GUILayout.EndHorizontal();
			GUILayout.Space(10);

			_poiScrollPosition = GUILayout.BeginScrollView(_poiScrollPosition);
			foreach (List<POI> poisRow in _poisChunked)
			{
				GUILayout.BeginHorizontal();
				foreach (POI poi in poisRow)
				{
					GUILayout.Box("", "button", GUILayout.Width(140), GUILayout.Height(140));
					Rect boxRect = GUILayoutUtility.GetLastRect();
					bool buttonImage = GUI.Button(new Rect(boxRect.x + 10f, boxRect.y - 10f, boxRect.width - 20f, boxRect.height - 20f), poi.thumbnail, "ButtonTransparent");
					bool buttonText = GUI.Button(new Rect(boxRect.x, boxRect.y + (boxRect.height / 2), boxRect.width, boxRect.height / 2), poi.name, "ButtonTransparent");
					if (buttonImage || buttonText)
					{
						SpawnedPOI spawnedPOI = SpawnUtilities.Spawn(poi, _poiSpawnItems);
						if (spawnedPOI != null)
							_spawnedPOIs.Add(spawnedPOI);
					}
					GUILayout.Space(5);
				}
				GUILayout.EndHorizontal();
				GUILayout.Space(5);
			}
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		public override void RenderConfigPane(Rect dimensions)
		{
			GUILayout.BeginArea(dimensions);
			GUILayout.BeginVertical();
			_configScrollPosition = GUILayout.BeginScrollView(_configScrollPosition);

			if (_spawnedPOIs.Count == 0)
			{
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Label("Nothing has been spawned yet");
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}

			foreach (SpawnedPOI poi in _spawnedPOIs)
			{
				GUILayout.Label(poi.poi.name);
				GUILayout.Space(2);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Teleport to", GUILayout.MaxWidth(100)))
					GameUtilities.TeleportPlayerWithParent(poi.poiObject.transform.position + Vector3.up * 2f);

				GUILayout.Space(5);

				if (GUILayout.Button("Delete POI", GUILayout.MaxWidth(100)))
				{
					// Remove POI from save.
					if (poi.ID != null)
						SaveUtilities.UpdatePOISaveData(new POIData()
						{
							ID = poi.ID.Value,
						}, "delete");

					_spawnedPOIs.Remove(poi);
					GameObject.Destroy(poi.poiObject);
					break;
				}

				GUILayout.EndHorizontal();
				GUILayout.Space(10);
			}

			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}
}
