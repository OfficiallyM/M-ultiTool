using MultiTool.Core;
using MultiTool.Modules;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Settings = MultiTool.Core.Settings;
using Logger = MultiTool.Modules.Logger;

namespace MultiTool.Tabs
{
	internal class MiscellaneousTab : Tab
	{
		public override string Name => "Miscellaneous";

		private Settings settings = new Settings();
		public override void RenderTab(Dimensions dimensions)
		{
			float miscX = dimensions.x + 10f;
			float miscY = dimensions.y + 10f;
			float buttonWidth = 200f;
			float buttonHeight = 20f;

			float miscWidth = 250f;
			float labelWidth = dimensions.width - 20f;

			// Delete mode.
			if (GUI.Button(new Rect(miscX, miscY, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleString("Delete mode", settings.deleteMode) + $" (Press {GUIRenderer.binds.GetKeyByAction((int)Keybinds.Inputs.deleteMode).key})"))
			{
				settings.deleteMode = !settings.deleteMode;
			}

			miscY += buttonHeight + 20f;

			// Time setting.
			// TODO: Work out what the time actually is.
			GUI.Label(new Rect(miscX, miscY, labelWidth, buttonHeight), "Time:", GUIRenderer.labelStyle);
			miscY += buttonHeight;
			float time = GUI.HorizontalSlider(new Rect(miscX, miscY, miscWidth, buttonHeight), GUIRenderer.selectedTime, 0f, 360f);
			GUIRenderer.selectedTime = Mathf.Round(time);
			if (GUI.Button(new Rect(miscX + miscWidth + 10f, miscY, buttonWidth, buttonHeight), "Set"))
			{
                napszakvaltakozas.s.tekeres = GUIRenderer.selectedTime;
			}

			if (GUI.Button(new Rect(miscX + miscWidth + buttonWidth + 20f, miscY, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleString("Unlock", "Lock", GUIRenderer.isTimeLocked)))
			{
				GUIRenderer.isTimeLocked = !GUIRenderer.isTimeLocked;

                napszakvaltakozas.s.enabled = !GUIRenderer.isTimeLocked;
			}

			miscY += buttonHeight + 10f;

			GUI.Label(new Rect(miscX, miscY, labelWidth, buttonHeight), "UFO spawning (doesn't save):", GUIRenderer.labelStyle);

			if (GUI.Button(new Rect(miscX + miscWidth + 10f, miscY, buttonWidth, buttonHeight), "Spawn"))
			{
				try
				{
					// Destory existing UFO.
					if (GUIRenderer.ufo != null)
						UnityEngine.Object.Destroy(GUIRenderer.ufo);

					GUIRenderer.ufo = UnityEngine.Object.Instantiate(GUIRenderer.temp.FEDOSPAWN.prefab, mainscript.s.player.transform.position + (mainscript.s.player.transform.forward * 5f) + (Vector3.up * 2f), Quaternion.FromToRotation(Vector3.forward, -mainscript.s.player.transform.right));
					fedoscript ufoScript = GUIRenderer.ufo.GetComponent<fedoscript>();
					ufoScript.ai = false;
					ufoScript.followRoad = false;
				}
				catch (Exception ex)
				{
					Logger.Log($"Failed to spawn UFO - {ex}", Logger.LogLevel.Error);
				}
			}

			if (GUI.Button(new Rect(miscX + miscWidth + buttonWidth + 20f, miscY, buttonWidth, buttonHeight), "Delete"))
			{
				if (GUIRenderer.ufo != null)
				{
					fedoscript ufoScript = GUIRenderer.ufo.GetComponent<fedoscript>();
					if (!ufoScript.seat.inUse)
						UnityEngine.Object.Destroy(GUIRenderer.ufo);
				}
			}

			miscY += buttonHeight + 10f;

			//if (GUI.Button(new Rect(miscX, miscY, buttonWidth, buttonHeight), "Respawn nearest building items"))
			//{
			//	Vector3 playerPosition = mainscript.s.player.transform.position;

			//	// Find closest building.
			//	float distance = float.MaxValue;
			//	GameObject closestBuilding = null;

			//	List<GameObject> buildings = new List<GameObject>();

			//	foreach (KeyValuePair<int, GameObject> building in mainscript.s.terrainGenerationSettings.roadBuildingGeneration.placedBuildings)
			//	{
			//		buildings.Add(building.Value);
			//	}

			//	foreach (SpawnedPOI spawnedPOI in GUIRenderer.spawnedPOIs)
			//	{
			//		buildings.Add(spawnedPOI.poi);
			//	}

			//	foreach (GameObject building in buildings)
			//	{
			//		Vector3 position = building.transform.position;
			//		float buildingDistance = Vector3.Distance(position, playerPosition);
			//		if (buildingDistance < distance)
			//		{
			//			distance = buildingDistance;
			//			closestBuilding = building;
			//		}
			//	}

			//	// Trigger item respawn.
			//	buildingscript buildingscript = closestBuilding.GetComponent<buildingscript>();
			//	buildingscript.itemsSpawned = false;
			//	buildingscript.SpawnStuff(0);
			//}
			//miscY += buttonHeight + 10f;

			if (GUI.Button(new Rect(miscX, miscY, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleString("Toggle color picker", settings.mode == "colorPicker")))
			{
				if (settings.mode == "colorPicker")
					settings.mode = null;
				else
					settings.mode = "colorPicker";
			}
			miscY += buttonHeight + 10f;

			if (GUI.Button(new Rect(miscX, miscY, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleString("Toggle object scale mode", settings.mode == "scale")))
			{
				if (settings.mode == "scale")
					settings.mode = null;
				else
					settings.mode = "scale";
			}

			miscY += buttonHeight + 10f;

			if (GUI.Button(new Rect(miscX, miscY, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleString("Toggle object regenerator", settings.mode == "objectRegenerator")))
			{
				if (settings.mode == "objectRegenerator")
				{
					settings.mode = null;
					GUIRenderer.selectedObject = null;
				}
				else
					settings.mode = "objectRegenerator";
			}

			miscY += buttonHeight + 10f;

			if (GUI.Button(new Rect(miscX, miscY, buttonWidth * 2, buttonHeight), "Rebuild thumbnail cache (this will lag)"))
				ThumbnailGenerator.RebuildCache();
		}
	}
}
