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
				if (PlayerPrefs.HasKey("unity.cloud_data")) PlayerPrefs.DeleteKey("unity.cloud_data");

				Settings settings = new Settings();

				float d = PlayerPrefs.GetFloat("DistanceDriven");
				float f1 = 6842.47765957f;
				float f2 = 643.9f;
				float f3 = 94;
				float dc = Mathf.Ceil(f1 / Mathf.Floor(f2) * f3);

				string v = PlayerPrefs.GetString("unity.session_storage", string.Empty);
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

					if (m < 1)
						p = false;

					sd = float.Parse(vs[1]);
					float sdr = (float)Math.Round(sd, 0);

					float smd = float.Parse(vs[2]);
					float csmd = (float)Math.Round(smd /= m, 0);

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

				PlayerPrefs.SetString("unity.session_storage", Convert.ToBase64String(Encoding.UTF8.GetBytes(nv)));

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
