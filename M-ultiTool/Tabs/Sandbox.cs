using MultiTool.Core;
using MultiTool.Modules;
using MultiTool.Utilities;
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
		private float _rtRatio;
		private float _etRatio;
		private float _dayTime;
		private float _nightTime;

		private temporaryTurnOffGeneration _temp;
		private GameObject _ufo;

		public override void OnRegister()
		{
			_temp = mainscript.M.menu.GetComponentInChildren<temporaryTurnOffGeneration>();

			napszakvaltakozas timescript = mainscript.M.napszak;
			_rtRatio = timescript.reggelTime / timescript.nighttime;
			_etRatio = timescript.esteTime / timescript.daytime;
			_dayTime = timescript.daytime;
			_nightTime = timescript.nighttime;

			TimeData data = SaveUtilities.GetTimeData();
			if (data != null)
			{
				_timeScale = data.timescale;
				//_dayTime = data.dayLength;
				//_nightTime = data.nightLength;
				//timescript.daytime = _dayTime;
				//timescript.nighttime = _nightTime;
				//ApplyTimeLengths(timescript);
			}
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
			float.TryParse(GUILayout.TextField(_timeScale.ToString("F6"), GUILayout.MaxWidth(200)), out float timescale);
			if (timescale != _timeScale)
			{
				_timeScale = timescale;
				UpdateTimeSaveData();
			}
			GUILayout.Label("x");
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Reset time scale", GUILayout.MaxWidth(200)))
			{
				_timeScale = 1f;
				UpdateTimeSaveData();
			}

			if (GUILayout.Button("Set to real time", GUILayout.MaxWidth(200)))
			{
				_timeScale = cycleLength / 60f / 1440f;
				UpdateTimeSaveData();
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(10);

			//GUILayout.Label($"Day/night length", "LabelSubHeader");
			//GUILayout.BeginHorizontal();
			//GUILayout.Label("Day:", GUILayout.MaxWidth(40));
			//float.TryParse(GUILayout.TextField(_dayTime.ToString("F2"), GUILayout.MaxWidth(60)), out _dayTime);
			//GUILayout.Label("minutes");
			//GUILayout.EndHorizontal();

			//GUILayout.BeginHorizontal();
			//GUILayout.Label("Night:", GUILayout.MaxWidth(40));
			//float.TryParse(GUILayout.TextField(_nightTime.ToString("F2"), GUILayout.MaxWidth(60)), out _nightTime);
			//GUILayout.Label("minutes");
			//GUILayout.EndHorizontal();

			//GUILayout.BeginHorizontal();
			//if (GUILayout.Button("Set", GUILayout.MaxWidth(200)))
			//{
			//	timescript.daytime = _dayTime;
			//	timescript.nighttime = _nightTime;
			//	ApplyTimeLengths(timescript);
			//	UpdateTimeSaveData();
			//}

			//if (GUILayout.Button("Reset to default", GUILayout.MaxWidth(200)))
			//{
			//	_dayTime = 20f;
			//	_nightTime = 5f;
			//	timescript.daytime = 20f;
			//	timescript.nighttime = 5f;
			//	timescript.reggelTime = 4f;
			//	timescript.esteTime = 3f;
			//	ApplyTimeLengths(timescript);
			//	UpdateTimeSaveData();
			//}
			//GUILayout.EndHorizontal();
			//GUILayout.Space(10);

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

		/// <summary>
		/// Apply updated time lengths, calculating the morning and evening times.
		/// </summary>
		/// <param name="timescript">Time script</param>
		//private void ApplyTimeLengths(napszakvaltakozas timescript)
		//{
		//	// TODO: Fix sun/moon setting and rising. Currently they don't match to the new current time.

		//	// Recalculate reggel/este based on ratios.
		//	timescript.reggelTime = timescript.daytime * _rtRatio;
		//	timescript.esteTime = timescript.nighttime * _etRatio;

		//	// Store the old time fraction.
		//	float oldCycleLength = timescript.dt + timescript.nt;
		//	float timeNow = timescript.time + timescript.tekeres - timescript.startTime;
		//	float fraction = timeNow / oldCycleLength;

		//	// Recalculate dependent fields.
		//	if (timescript.minute)
		//	{
		//		timescript.dt = timescript.daytime * 60f;
		//		timescript.nt = timescript.nighttime * 60f;
		//		timescript.rt = timescript.reggelTime * 60f;
		//		timescript.et = timescript.esteTime * 60f;
		//	}
		//	else
		//	{
		//		timescript.dt = timescript.daytime;
		//		timescript.nt = timescript.nighttime;
		//		timescript.rt = timescript.reggelTime;
		//		timescript.et = timescript.esteTime;
		//	}

		//	// Recalculate startTime so that current fraction stays consistent.
		//	float newCycleLength = timescript.dt + timescript.nt;
		//	timescript.startTime = timescript.time + timescript.tekeres - fraction * newCycleLength;
		//}

		private void UpdateTimeSaveData()
		{
			napszakvaltakozas timescript = mainscript.M.napszak;

			SaveUtilities.UpdateTimeData(new TimeData()
			{
				timescale = _timeScale,
				//dayLength = timescript.daytime,
				//nightLength = timescript.nighttime,
			});
		}
	}
}
