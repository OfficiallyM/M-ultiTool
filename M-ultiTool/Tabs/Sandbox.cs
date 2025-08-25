using MultiTool.Core;
using MultiTool.Modules;
using MultiTool.Utilities.UI;
using System;
using UnityEngine;
using Logger = MultiTool.Modules.Logger;

namespace MultiTool.Tabs
{
	internal class SandboxTab : Tab
	{
		public override string Name => "Sandbox";

		private Settings _settings = new Settings();
		private Vector2 _position;

		private string _hour = "0";
		private string _minute = "0";
		private string _second = "0";
		private float _timeScale = 1f;

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

			GUILayout.Label("Time manipulation", "LabelHeader");
			GUILayout.Label($"Time setting", "LabelSubHeader");
			napszakvaltakozas timescript = mainscript.M.napszak;
			float cycleLength = timescript.dt + timescript.nt;
			float clockOffset = 1f / 16f;

			GUILayout.Label($"Current world time: {ToTimestring(timescript.currentTime + clockOffset)}");

			GUILayout.BeginHorizontal();
			_hour = GUILayout.TextField(_hour, 2, GUILayout.Width(40));
			GUILayout.Label(":", GUILayout.Width(8));
			_minute = GUILayout.TextField(_minute, 2, GUILayout.Width(40));
			GUILayout.Label(":", GUILayout.Width(8));
			_second = GUILayout.TextField(_second, 2, GUILayout.Width(40));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Set", GUILayout.MaxWidth(200)))
			{
				if (!int.TryParse(_hour, out int h)) h = 0;
				if (!int.TryParse(_minute, out int m)) m = 0;
				if (!int.TryParse(_second, out int s)) s = 0;

				h = Mathf.Clamp(h, 0, 23);
				m = Mathf.Clamp(m, 0, 59);
				s = Mathf.Clamp(s, 0, 59);
				float clockSeconds = (h * 3600) + (m * 60) + s;
				float selectedTime = Mathf.Round(((clockSeconds / 86400f) * cycleLength) - clockOffset * cycleLength);
				selectedTime = Mathf.Clamp(selectedTime, 0, cycleLength);

				timescript.startTime = timescript.time + timescript.tekeres - selectedTime;
			}

			if (GUILayout.Button(Accessibility.GetAccessibleString("Unlock", "Lock", !timescript.enabled), GUILayout.MaxWidth(200)))
				timescript.enabled = !timescript.enabled;

			if (GUILayout.Button("Sync to real time", GUILayout.MaxWidth(200)))
			{
				DateTime now = DateTime.Now;
				int h = now.Hour;
				int m = now.Minute;
				int s = now.Second;
				_hour = h.ToString("F2");
				_minute = m.ToString("F2");
				_second = s.ToString("F2");

				float clockSeconds = (h * 3600) + (m * 60) + s;
				float selectedTime = Mathf.Round(((clockSeconds / 86400f) * cycleLength) - clockOffset * cycleLength);
				selectedTime = Mathf.Clamp(selectedTime, 0, cycleLength);

				timescript.startTime = timescript.time + timescript.tekeres - selectedTime;
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(10);

			GUILayout.Label($"Time Scale", "LabelSubHeader");
			float gameMinPerRealMin = (86400f / cycleLength) * Mathf.Abs(_timeScale);
			float gameHrPerRealMin = gameMinPerRealMin / 60f;
			float realMinPerGameDay = (cycleLength / 60f) / Mathf.Max(Mathf.Abs(_timeScale), 1e-6f);
			float realHrPerGameDay = realMinPerGameDay / 60f;
			GUILayout.Label($"1 real minute = {gameMinPerRealMin:F1} game minutes ({gameHrPerRealMin:F2} hours)");
			GUILayout.Label($"1 game day = {realMinPerGameDay:F1} real minutes ({realHrPerGameDay:F2} hours)");
			GUILayout.BeginHorizontal();
			float.TryParse(GUILayout.TextField(_timeScale.ToString("F6"), GUILayout.MaxWidth(200)), out _timeScale);
			GUILayout.Label("x");
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Reset time scale", GUILayout.MaxWidth(200)))
			{
				_timeScale = 1f;
			}

			if (GUILayout.Button("Set to real time", GUILayout.MaxWidth(200)))
			{
				_timeScale = cycleLength / 60f / 1440f;
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(10);

			GUILayout.Label("UFO", "LabelHeader");
			GUILayout.Label("UFO spawning (doesn't save):");
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Spawn", GUILayout.MaxWidth(200)))
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

			GUILayout.Label("Tools", "LabelHeader");
			if (GUILayout.Button(Accessibility.GetAccessibleString("Toggle color picker", _settings.mode == "colorPicker"), GUILayout.MaxWidth(200)))
			{
				if (_settings.mode == "colorPicker")
					_settings.mode = null;
				else
					_settings.mode = "colorPicker";
			}
			GUILayout.Space(10);

			if (GUILayout.Button(Accessibility.GetAccessibleString("Toggle object scale mode", _settings.mode == "scale"), GUILayout.MaxWidth(200)))
			{
				if (_settings.mode == "scale")
					_settings.mode = null;
				else
					_settings.mode = "scale";
			}
			GUILayout.Space(10);

			if (GUILayout.Button(Accessibility.GetAccessibleString("Toggle object regenerator", _settings.mode == "objectRegenerator"), GUILayout.MaxWidth(200)))
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

			if (GUILayout.Button(Accessibility.GetAccessibleString("Toggle weight changer", _settings.mode == "weightChanger"), GUILayout.MaxWidth(200)))
			{
				if (_settings.mode == "weightChanger")
				{
					_settings.mode = null;
					GUIRenderer.selectedObject = null;
				}
				else
					_settings.mode = "weightChanger";
			}

			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		public override void FixedUpdate()
		{
			napszakvaltakozas timescript = mainscript.M.napszak;
			if (timescript.enabled)
				timescript.tekeres += Time.fixedDeltaTime * (_timeScale - 1f);
		}

		/// <summary>
		/// Convert a float to a time string.
		/// </summary>
		/// <param name="time">Current time</param>
		/// <returns>Time formatted as a string</returns>
		private string ToTimestring(float time)
		{
			int totalSeconds = (int)(time * 24 * 60 * 60);
			int hours = totalSeconds / 3600;
			int minutes = (totalSeconds % 3600) / 60;
			int seconds = totalSeconds % 60;
			return $"{hours}:{minutes}:{seconds}";
		}
	}
}
