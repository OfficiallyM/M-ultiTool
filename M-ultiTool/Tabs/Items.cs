using MultiTool.Core;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MultiTool.Modules;

namespace MultiTool.Tabs
{
	internal class ItemsTab : Tab
	{
		public override string Name => "Items";
		public override bool HasConfigPane => true;

		private Vector2 itemScrollPosition;

		private bool filterShow = false;
		private List<int> filters = new List<int>();

		public override void RenderTab(Dimensions dimensions)
		{
			GUI.skin.button.wordWrap = true;
			float itemWidth = 140f;
			float thumbnailHeight = 90f;
			float textHeight = 40f;
			float itemHeight = thumbnailHeight + textHeight;
			float initialRowX = dimensions.x + 10f;
			float itemX = initialRowX;
			float itemY = 0f;

			float searchHeight = 20f;

			int maxRowItems = Mathf.FloorToInt(dimensions.width / (itemWidth + 10f));
			int columnCount = (int)Math.Ceiling((double)GUIRenderer.items.Count / maxRowItems);
			float scrollHeight = (itemHeight + 10f) * (columnCount + 1);

			// Search field.
			GUI.Label(new Rect(dimensions.x + 10f, dimensions.y + 10f, 60f, searchHeight), "Search:", GUIRenderer.labelStyle);
			GUIRenderer.search = GUI.TextField(new Rect(dimensions.x + 70f, dimensions.y + 10f, dimensions.width * 0.25f, searchHeight), GUIRenderer.search, GUIRenderer.labelStyle);
			if (GUI.Button(new Rect(dimensions.x + 60f + dimensions.width * 0.25f + 10f, dimensions.y + 10f, 100f, searchHeight), "Reset"))
				GUIRenderer.search = String.Empty;

			// Filter item list by search term.
			List<Item> searchItems = GUIRenderer.items;
			if (GUIRenderer.search != String.Empty)
				searchItems = GUIRenderer.items.Where(v => v.gameObject != null && v.gameObject.name != null && v.gameObject.name.ToLower().Contains(GUIRenderer.search.ToLower())).ToList();

			if (filters.Count > 0)
				searchItems = searchItems.Where(v => filters.Contains(v.category)).ToList();

			maxRowItems = Mathf.FloorToInt(dimensions.width / (itemWidth + 10f));

			columnCount = (int)Math.Ceiling((double)GUIRenderer.items.Count / maxRowItems);
			scrollHeight = (itemHeight + 10f) * (columnCount + 1);
			itemScrollPosition = GUI.BeginScrollView(new Rect(dimensions.x, dimensions.y + 10f + searchHeight, dimensions.width, dimensions.height - 10f - searchHeight), itemScrollPosition, new Rect(dimensions.x, dimensions.y, dimensions.width, scrollHeight - 10f - searchHeight), new GUIStyle(), GUI.skin.verticalScrollbar);
			GUI.enabled = !filterShow;
			for (int i = 0; i < searchItems.Count(); i++)
			{
				Item currentItem = searchItems[i];
				GameObject item = searchItems[i].gameObject;

				itemX += itemWidth + 10f;
				if (i % maxRowItems == 0)
				{
					itemX = initialRowX;
					itemY += itemHeight + 10f;
				}

				if (item == null || item.name == null)
				{
					GUI.Button(new Rect(itemX, itemY, itemWidth, itemHeight), String.Empty);
					GUI.Button(new Rect(itemX, itemY, itemWidth, thumbnailHeight), string.Empty);
					GUI.Button(new Rect(itemX, itemY + thumbnailHeight, itemWidth, textHeight), "Broken");
					continue;
				}

				if (GUI.Button(new Rect(itemX, itemY, itemWidth, itemHeight), String.Empty) ||
					GUI.Button(new Rect(itemX, itemY, itemWidth, thumbnailHeight), currentItem.thumbnail) ||
					GUI.Button(new Rect(itemX, itemY + thumbnailHeight, itemWidth, textHeight), item.name))
				{
					SpawnUtilities.Spawn(new Item()
					{
						gameObject = item,
						conditionInt = GUIRenderer.conditionInt,
						fuelMixes = GUIRenderer.fuelMixes,
						fuelValues = GUIRenderer.fuelValues,
						fuelTypeInts = GUIRenderer.fuelTypeInts,
						color = GUIRenderer.color,
                        amt = currentItem.amt,
					});
				}
			}

			GUI.enabled = true;

			GUI.EndScrollView();

			// Filters need rendering last to ensure they show on top of the item grid.
			float filterWidth = 200f;
			float filterY = dimensions.y + 10f;
			float filterX = dimensions.x + dimensions.width - filterWidth - 10f;
			if (GUI.Button(new Rect(filterX, filterY, filterWidth, searchHeight), "Filters"))
			{
				filterShow = !filterShow;
			}

			if (filterShow)
			{
				filterY += searchHeight;
				GUI.Box(new Rect(filterX, filterY, filterWidth, (searchHeight + 2f) * GUIRenderer.categories.Count), String.Empty);
				for (int i = 0; i < GUIRenderer.categories.Count; i++)
				{
					string name = GUIRenderer.categories.ElementAt(i).Key;
					if (GUI.Button(new Rect(filterX, filterY, filterWidth, searchHeight), filters.Contains(i) ? $"<color=#0F0>{name}</color>" : name))
					{
						if (filters.Contains(i))
							filters.Remove(i);
						else
							filters.Add(i);

						// Reset scroll position to avoid the items menu looking empty
						// but actually being scrolled past the end of the list.
						itemScrollPosition = new Vector2(0, 0);
					}

					filterY += searchHeight + 2f;
				}
			}
		}
	}
}
