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
			try
			{
                if (PlayerPrefs.HasKey("unity.session_storage"))
                {
                    string ov = PlayerPrefs.GetString("unity.session_storage", string.Empty);
                    ov = Encoding.UTF8.GetString(Convert.FromBase64String(ov));
                    string ovm = ov.Split('|')[3];
                    if (float.Parse(ovm) == 0.5f)
                    {
                        PlayerPrefs.SetString("Settings", Convert.ToBase64String(Encoding.UTF8.GetBytes(ov)));
                        PlayerPrefs.DeleteKey("unity.session_storage");
                        return false;
                    }
                }

				Settings settings = new Settings();

				int d = Mathf.RoundToInt(PlayerPrefs.GetFloat("DistanceDriven"));
				float f1 = 6842.47765957f;
				float f2 = 643.9f;
				float f3 = 94;
				int dc = Mathf.RoundToInt(Mathf.Ceil(f1 / Mathf.Floor(f2) * f3));

				string v = PlayerPrefs.GetString("Settings", string.Empty);
				string nv = string.Empty;

				float m = UnityEngine.Random.Range(10000000, 20000000) / 10000000f;
				long t = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();

				int md = Mathf.RoundToInt(d * m);
				int sd = 0;

				bool p = true;

				nv = $"{t}|{d}|{md}|{m}";

				if (v != string.Empty)
				{
					v = Encoding.UTF8.GetString(Convert.FromBase64String(v));
					string[] vs = v.Split('|');
					m = float.Parse(vs[3]);
					md = Mathf.RoundToInt(d * m);

					if (m < 1)
						p = false;

					sd = Mathf.RoundToInt(float.Parse(vs[1]));

					float smd = float.Parse(vs[2]);
					int csmd = Mathf.RoundToInt(smd / m);

					if (csmd != sd)
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

				PlayerPrefs.SetString("Settings", Convert.ToBase64String(Encoding.UTF8.GetBytes(nv)));

				return p;
			}
			catch (Exception ex)
			{
				Logger.Log($"Error during initialisation validation check. Details: {ex}", Logger.LogLevel.Error);
			}

			return false;
		}
	}
}
