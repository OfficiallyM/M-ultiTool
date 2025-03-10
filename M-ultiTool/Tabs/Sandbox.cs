using MultiTool.Core;
using MultiTool.Modules;
using MultiTool.Utilities;
using System;
using UnityEngine;
using Settings = MultiTool.Core.Settings;
using Logger = MultiTool.Modules.Logger;
using MultiTool.Utilities.UI;

namespace MultiTool.Tabs
{
	internal class SandboxTab : Tab
	{
		public override string Name => "Sandbox";

		private Settings _settings = new Settings();
		private Vector2 _position;

		private float _selectedTime;
		private bool _isTimeLocked = false;

		private temporaryTurnOffGeneration _temp;
		private GameObject _ufo;

		public override void OnRegister()
		{
			_temp = mainscript.M.menu.GetComponentInChildren<temporaryTurnOffGeneration>();
		}

		public override void RenderTab(Rect dimensions)
		{
			GUILayout.BeginArea(dimensions);
			GUILayout.BeginVertical();
			_position = GUILayout.BeginScrollView(_position);

			// Time setting.
			napszakvaltakozas timescript = mainscript.M.napszak;
			float currentTime = Mathf.InverseLerp(0f, timescript.dt + timescript.nt, timescript.time + _selectedTime - timescript.startTime);
			int totalSeconds = (int)(currentTime * 24 * 60 * 60);
			int hours = totalSeconds / 3600;
			int minutes = (totalSeconds % 3600) / 60;
			int seconds = totalSeconds % 60;
			GUILayout.Label($"Time: {hours}:{minutes}:{seconds}");
			float time = GUILayout.HorizontalSlider(_selectedTime, 0f, timescript.dt + timescript.nt, GUILayout.MaxWidth(dimensions.width / 2));
			_selectedTime = Mathf.Round(time);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Set", GUILayout.MaxWidth(250)))
			{
				timescript.tekeres = _selectedTime;
			}

			if (GUILayout.Button(Accessibility.GetAccessibleString("Unlock", "Lock", _isTimeLocked), GUILayout.MaxWidth(250)))
			{
				_isTimeLocked = !_isTimeLocked;

				timescript.enabled = !_isTimeLocked;
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(10);

			GUILayout.Label("UFO spawning (doesn't save):");
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Spawn", GUILayout.MaxWidth(250)))
			{
				try
				{
					// Destory existing UFO.
					if (_ufo != null)
						UnityEngine.Object.Destroy(_ufo);

					_ufo = UnityEngine.Object.Instantiate(_temp.FEDOSPAWN.prefab, mainscript.M.player.transform.position + (mainscript.M.player.transform.forward * 5f) + (Vector3.up * 2f), Quaternion.FromToRotation(Vector3.forward, -mainscript.M.player.transform.right));
					fedoscript ufoScript = _ufo.GetComponent<fedoscript>();
					ufoScript.ai = false;
					ufoScript.followRoad = false;
				}
				catch (Exception ex)
				{
					Logger.Log($"Failed to spawn UFO - {ex}", Logger.LogLevel.Error);
				}
			}

			if (GUILayout.Button("Delete", GUILayout.MaxWidth(200)))
			{
				if (_ufo != null)
				{
					fedoscript ufoScript = _ufo.GetComponent<fedoscript>();
					if (!ufoScript.seat.inUse)
						UnityEngine.Object.Destroy(_ufo);
				}
			}
			GUILayout.EndHorizontal();
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

			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}
}
