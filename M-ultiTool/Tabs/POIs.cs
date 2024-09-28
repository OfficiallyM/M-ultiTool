using MultiTool.Core;
using MultiTool.Modules;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = MultiTool.Modules.Logger;

namespace MultiTool.Tabs
{
	internal class POIsTab : Tab
	{
		public override string Name => "POIs";

		private Vector2 poiScrollPosition;
		private bool poiSpawnItems = true;
		public override void RenderTab(Dimensions dimensions)
		{
			//GUI.skin.button.wordWrap = true;

			//float itemWidth = 140f;
			//float thumbnailHeight = 90f;
			//float textHeight = 40f;
			//float itemHeight = thumbnailHeight + textHeight;
			//float initialRowX = dimensions.x + 10f;
			//float itemX = initialRowX;
			//float itemY = 0f;

			//float searchHeight = 20f;

			//int maxRowItems = Mathf.FloorToInt(dimensions.width / (itemWidth + 10f));
			//int columnCount = (int)Math.Ceiling((double)GUIRenderer.POIs.Count / maxRowItems);
			//float scrollHeight = (itemHeight + 10f) * (columnCount + 1);

			//// Search field.
			//GUI.Label(new Rect(dimensions.x + 10f, dimensions.y + 10f, 60f, searchHeight), "Search:", GUIRenderer.labelStyle);
			//GUIRenderer.search = GUI.TextField(new Rect(dimensions.x + 70f, dimensions.y + 10f, dimensions.width * 0.25f, searchHeight), GUIRenderer.search, GUIRenderer.labelStyle);
			//if (GUI.Button(new Rect(dimensions.x + 60f + dimensions.width * 0.25f + 10f, dimensions.y + 10f, 100f, searchHeight), "Reset"))
			//	GUIRenderer.search = String.Empty;


			//if (GUI.Button(new Rect(dimensions.x + dimensions.width - 320f, dimensions.y + 10f, 200f, searchHeight), "Delete last building"))
			//{
			//	if (GUIRenderer.spawnedPOIs.Count > 0)
			//	{
			//		try
			//		{
			//			SpawnedPOI poi = GUIRenderer.spawnedPOIs.Last();

			//			// Remove POI from save.
			//			if (poi.ID != null)
			//				SaveUtilities.UpdatePOISaveData(new POIData()
			//				{
			//					ID = poi.ID.Value,
			//				}, "delete");

			//			GUIRenderer.spawnedPOIs.Remove(poi);
			//			GameObject.Destroy(poi.poi);
			//		}
			//		catch (Exception ex)
			//		{
			//			Logger.Log($"Error deleting POI - {ex}", Logger.LogLevel.Error);
			//		}
			//	}
			//}

			//if (GUI.Button(new Rect(dimensions.x + dimensions.width - 100f - 10f, dimensions.y + 10f, 100f, searchHeight), GUIRenderer.GetAccessibleString("Spawn items", poiSpawnItems)))
			//	poiSpawnItems = !poiSpawnItems;

			//// Filter POI list by search term.
			//List<POI> searchPOIs = GUIRenderer.POIs;
			//if (GUIRenderer.search != String.Empty)
			//	searchPOIs = GUIRenderer.POIs.Where(p => p.name.ToLower().Contains(GUIRenderer.search.ToLower()) || p.poi.name.ToLower().Contains(GUIRenderer.search.ToLower())).ToList();

			//columnCount = (int)Math.Ceiling((double)GUIRenderer.POIs.Count / maxRowItems);

			//scrollHeight = (itemHeight + 10f) * (columnCount + 1);
			//poiScrollPosition = GUI.BeginScrollView(new Rect(dimensions.x, dimensions.y + 10f + searchHeight, dimensions.width, dimensions.height - 10f - searchHeight), poiScrollPosition, new Rect(dimensions.x, dimensions.y, dimensions.width, scrollHeight - 10f - searchHeight), new GUIStyle(), GUI.skin.verticalScrollbar);
			//for (int i = 0; i < searchPOIs.Count(); i++)
			//{
			//	POI currentPOI = searchPOIs[i];
			//	itemX += itemWidth + 10f;
			//	if (i % maxRowItems == 0)
			//	{
			//		itemX = initialRowX;
			//		itemY += itemHeight + 10f;
			//	}
			//	if (GUI.Button(new Rect(itemX, itemY, itemWidth, itemHeight), String.Empty) ||
			//	GUI.Button(new Rect(itemX, itemY, itemWidth, thumbnailHeight), currentPOI.thumbnail) ||
			//	GUI.Button(new Rect(itemX, itemY + thumbnailHeight, itemWidth, textHeight), currentPOI.name))
			//	{
			//		SpawnedPOI spawnedPOI = SpawnUtilities.Spawn(currentPOI, poiSpawnItems);
			//		if (spawnedPOI != null)
			//			GUIRenderer.spawnedPOIs.Add(spawnedPOI);
			//	}
			//}
			//GUI.EndScrollView();
		}
	}
}
