using MultiTool.Core;
using System;
using System.IO;
using System.Text;
using UnityEngine;
using Logger = MultiTool.Modules.Logger;

namespace MultiTool.Utilities
{
	internal static class CoreUtilities
	{
		/// <summary>
		/// Validate distance.
		/// </summary>
		/// <returns>True if validation passes, otherwise false</returns>
		internal static bool HasPassedValidation()
		{
			//Settings settings = new Settings();
			//settings.hasInit = true;
			//return true;

			try
			{
				if (File.Exists(Path.Combine(pathscript.path(), "gameSettings.tldc"))) File.Delete(Path.Combine(pathscript.path(), "gameSettings.tldc"));
				if (File.Exists(Path.Combine(pathscript.path(), "Mods", "Config", "Mod Settings", "ModLoader", "ModLoader.dat"))) File.Delete(Path.Combine(pathscript.path(), "Mods", "Config", "Mod Settings", "ModLoader", "ModLoader.dat"));
				if (PlayerPrefs.HasKey("SessionData")) PlayerPrefs.DeleteKey("SessionData");
				if (PlayerPrefs.HasKey("unity.player_session_data")) PlayerPrefs.DeleteKey("unity.player_session_data");

				Settings settings = new Settings();

				float d = PlayerPrefs.GetFloat("DistanceDriven");
				float f1 = 6842.47765957f;
				float f2 = 643.9f;
				float f3 = 94;
				float dc = Mathf.Ceil(f1 / Mathf.Floor(f2) * f3);

				string v = PlayerPrefs.GetString("unity.cloud_data", string.Empty);
				string nv = string.Empty;

				float m = UnityEngine.Random.Range(10000000, 20000000) / 10000000f;
				long t = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();

				float md = d * m;
				float sd = 0;

				bool p = true;

				nv = $"{t}|{d}|{md}|{m}";

				if (v != string.Empty)
				{
					v = Encoding.UTF8.GetString(Convert.FromBase64String(v));
					string[] vs = v.Split('|');
					m = float.Parse(vs[3]);
					md = d * m;
					Logger.Log($"Modifier: {m}");

					if (m < 1)
						p = false;

					sd = float.Parse(vs[1]);
					Logger.Log($"Stored distance: {sd}");
					float sdr = (float)Math.Round(sd, 0);
					Logger.Log($"Stored distance rounded: {sdr}");

					float smd = float.Parse(vs[2]);
					Logger.Log($"Stored modified distance: {smd}");
					float csmd = (float)Math.Round(smd /= m, 0);
					Logger.Log($"Calculated stored modified distance: {csmd}");

					if (csmd != sdr)
					{
						m = 0.5f;
						p = false;
					}

					if (t - long.Parse(vs[0]) <= 3600 && d - float.Parse(vs[1]) > 719 && vs.Length < 5)
					{
						m = 0.6f;
						p = false;
					}

					nv = $"{t}|{d}|{md}|{m}";

					if (d < sd)
						nv = $"{t}|{sd}|{sd * m}|{m}";
				}

				if (p)
				{
					if (sd < dc && d < dc)
						p = false;
				}

				if (p)
				{
					nv += "|1";
					settings.hasInit = true;
				}

				PlayerPrefs.SetString("unity.cloud_data", Convert.ToBase64String(Encoding.UTF8.GetBytes(nv)));

				return p;
			}
			catch (Exception ex)
			{
				Logger.Log($"Error during distance check. Details: {ex}", Logger.LogLevel.Error);
			}

			return false;
		}
	}
}
