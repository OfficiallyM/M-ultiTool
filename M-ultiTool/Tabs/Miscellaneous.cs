using MultiTool.Core;
using MultiTool.Modules;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Settings = MultiTool.Core.Settings;
using Logger = MultiTool.Modules.Logger;
using MultiTool.Utilities.UI;
using static ScottPlot.Plottable.PopulationPlot;
using ScottPlot.Palettes;

namespace MultiTool.Tabs
{
	internal class MiscellaneousTab : Tab
	{
		public override string Name => "Miscellaneous";

		private Settings _settings = new Settings();
		private Vector2 _position;

		public override void RenderTab(Rect dimensions)
		{
			GUILayout.BeginArea(dimensions);
			GUILayout.BeginVertical();
			_position = GUILayout.BeginScrollView(_position);

			// Delete mode.
			if (GUILayout.Button(Accessibility.GetAccessibleString("Delete mode", _settings.deleteMode) + $" (Press {MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.deleteMode).key})", GUILayout.MaxWidth(250)))
			{
				_settings.deleteMode = !_settings.deleteMode;
			}
			GUILayout.Space(10);

			// Time setting.
			// TODO: Work out what the time actually is.
			GUILayout.Label("Time:");
			float time = GUILayout.HorizontalSlider(GUIRenderer.selectedTime, 0f, 360f, GUILayout.MaxWidth(dimensions.width / 2));
			GUIRenderer.selectedTime = Mathf.Round(time);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Set", GUILayout.MaxWidth(250)))
			{
				napszakvaltakozas.s.tekeres = GUIRenderer.selectedTime;
			}

			if (GUILayout.Button(Accessibility.GetAccessibleString("Unlock", "Lock", GUIRenderer.isTimeLocked), GUILayout.MaxWidth(250)))
			{
				GUIRenderer.isTimeLocked = !GUIRenderer.isTimeLocked;

				napszakvaltakozas.s.enabled = !GUIRenderer.isTimeLocked;
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(10);

			//GUILayout.Label("UFO spawning (doesn't save):");
			//if (GUILayout.Button("Spawn", GUILayout.MaxWidth(250)))
			//{
			//	try
			//	{
			//		// Destory existing UFO.
			//		if (GUIRenderer.ufo != null)
			//			UnityEngine.Object.Destroy(GUIRenderer.ufo);

			//		GUIRenderer.ufo = UnityEngine.Object.Instantiate(GUIRenderer.temp.FEDOSPAWN.prefab, mainscript.s.player.transform.position + (mainscript.s.player.transform.forward * 5f) + (Vector3.up * 2f), Quaternion.FromToRotation(Vector3.forward, -mainscript.s.player.transform.right));
			//		fedoscript ufoScript = GUIRenderer.ufo.GetComponent<fedoscript>();
			//		ufoScript.ai = false;
			//		ufoScript.followRoad = false;
			//	}
			//	catch (Exception ex)
			//	{
			//		Logger.Log($"Failed to spawn UFO - {ex}", Logger.LogLevel.Error);
			//	}
			//}

			//if (GUILayout.Button("Delete", GUILayout.MaxWidth(200)))
			//{
			//	if (GUIRenderer.ufo != null)
			//	{
			//		fedoscript ufoScript = GUIRenderer.ufo.GetComponent<fedoscript>();
			//		if (!ufoScript.seat.inUse)
			//			UnityEngine.Object.Destroy(GUIRenderer.ufo);
			//	}
			//}
			//GUILayout.Space(10);

			if (GUILayout.Button("Respawn nearest building items", GUILayout.MaxWidth(250)))
			{
				Vector3 playerPosition = mainscript.s.player.transform.position;

				// Find all generated buildings.
				List<poiGenScript.poiClass> buildings = new List<poiGenScript.poiClass>();

				for (int index = 0; index < menuhandler.s.currentMainMap.poiGens.Count; index++)
				{
					foreach (KeyValuePair<Vector3d, poiGenScript.chunkClass> chunk in menuhandler.s.currentMainMap.poiGens[index].chunks)
					{
						foreach (KeyValuePair<Vector3d, poiGenScript.poiClass> poi in chunk.Value.pois)
						{
							buildings.Add(poi.Value);
						}
					}
				}

				// Have 100 attempts to find the closest valid building.
				poiGenScript.poiClass closestBuilding = null;
				for (int attempt = 0; attempt < 100; attempt++)
				{
					closestBuilding = poiGenScript.NearestPoi(mainscript.GlobalFromUnityPos(playerPosition), buildings);
					if (closestBuilding != null && closestBuilding.pobj != null && !closestBuilding.poiName.ToLower().Contains("haz02"))
					{
						// Found a valid building, break.
						break;
					}
					else
					{
						// Continue the loop but remove the invalid building we've just checked.
						buildings.Remove(closestBuilding);
					}
				}

				if (closestBuilding != null)
				{
					// Trigger item respawn.
					closestBuilding.spawnedItems = false;
					closestBuilding.pobj.SpawnStuff(closestBuilding);
				}
			}
			GUILayout.Space(10);

			if (GUILayout.Button(Accessibility.GetAccessibleString("Toggle color picker", _settings.mode == "colorPicker"), GUILayout.MaxWidth(250)))
			{
				if (_settings.mode == "colorPicker")
					_settings.mode = null;
				else
					_settings.mode = "colorPicker";
			}
			GUILayout.Space(10);

			if (GUILayout.Button(Accessibility.GetAccessibleString("Toggle object scale mode", _settings.mode == "scale"), GUILayout.MaxWidth(250)))
			{
				if (_settings.mode == "scale")
					_settings.mode = null;
				else
					_settings.mode = "scale";
			}
			GUILayout.Space(10);

			if (GUILayout.Button(Accessibility.GetAccessibleString("Toggle object regenerator", _settings.mode == "objectRegenerator"), GUILayout.MaxWidth(250)))
			{
				if (_settings.mode == "objectRegenerator")
				{
					_settings.mode = null;
					GUIRenderer.selectedObject = null;
				}
				else
					_settings.mode = "objectRegenerator";
			}
			GUILayout.Space(10);

			if (GUILayout.Button("Rebuild thumbnail cache (this will lag)", "ButtonPrimaryWrap", GUILayout.MaxWidth(250)))
				ThumbnailGenerator.RebuildCache();

			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}
}
