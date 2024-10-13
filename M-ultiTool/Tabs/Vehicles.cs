using MultiTool.Core;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MultiTool.Modules;

namespace MultiTool.Tabs
{
	internal class VehiclesTab : Tab
	{
		public override string Name => "Vehicles";
		public override bool HasConfigPane => true;

		private Vector2 vehicleScrollPosition;
		public override void RenderTab(Rect dimensions)
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
			int columnCount = (int)Math.Ceiling((double)GUIRenderer.vehicles.Count / maxRowItems);
			float scrollHeight = (itemHeight + 10f) * (columnCount + 1);

			// Search field.
			GUI.Label(new Rect(dimensions.x + 10f, dimensions.y + 10f, 60f, searchHeight), "Search:", GUIRenderer.labelStyle);
			GUIRenderer.search = GUI.TextField(new Rect(dimensions.x + 70f, dimensions.y + 10f, dimensions.width * 0.25f, searchHeight), GUIRenderer.search, GUIRenderer.labelStyle);
			if (GUI.Button(new Rect(dimensions.x + 60f + dimensions.width * 0.25f + 10f, dimensions.y + 10f, 100f, searchHeight), "Reset"))
				GUIRenderer.search = String.Empty;

			// Filter vehicle list by search term.
			List<Vehicle> searchVehicles = GUIRenderer.vehicles;
			if (GUIRenderer.search != String.Empty)
				searchVehicles = GUIRenderer.vehicles.Where(v => v.name.ToLower().Contains(GUIRenderer.search.ToLower()) || v.gameObject.name.ToLower().Contains(GUIRenderer.search.ToLower())).ToList();
			scrollHeight = (itemHeight + 10f) * (columnCount + 1);
			vehicleScrollPosition = GUI.BeginScrollView(new Rect(dimensions.x, dimensions.y + 10f + searchHeight, dimensions.width, dimensions.height - 10f - searchHeight), vehicleScrollPosition, new Rect(dimensions.x, dimensions.y, dimensions.width, scrollHeight - 10f - searchHeight), new GUIStyle(), GUI.skin.verticalScrollbar);
			for (int i = 0; i < searchVehicles.Count(); i++)
			{
				Vehicle currentVehicle = searchVehicles[i];
				itemX += itemWidth + 10f;
				if (i % maxRowItems == 0)
				{
					itemX = initialRowX;
					itemY += itemHeight + 10f;
				}
				if (GUI.Button(new Rect(itemX, itemY, itemWidth, itemHeight), String.Empty) ||
					GUI.Button(new Rect(itemX, itemY, itemWidth, thumbnailHeight), currentVehicle.thumbnail) ||
					GUI.Button(new Rect(itemX, itemY + thumbnailHeight, itemWidth, textHeight), currentVehicle.name)
				)
				{
					SpawnUtilities.Spawn(new Vehicle()
					{
						gameObject = currentVehicle.gameObject,
						variant = currentVehicle.variant,
						conditionInt = GUIRenderer.conditionInt,
						fuelMixes = GUIRenderer.fuelMixes,
						fuelValues = GUIRenderer.fuelValues,
						fuelTypeInts = GUIRenderer.fuelTypeInts,
						color = GUIRenderer.color,
						plate = GUIRenderer.plate,
                        amt = currentVehicle.amt,
					});
				}
			}
			GUI.EndScrollView();
		}
	}
}
