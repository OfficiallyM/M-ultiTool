using MultiTool.Utilities;
using System;
using System.Linq;
using UnityEngine;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.Save.Records
{
	internal class LightRecord : SaveRecord, ISaveApplier
	{
		public string Name { get; set; }
		public Color Color { get; set; }

		public void Apply(tosaveitemscript save)
		{
			try
			{
				headlightscript headlight = null;
				bool isInteriorLight = false;
				if (Name != null && Name != string.Empty)
				{
					headlightscript[] lights = save.GetComponentsInChildren<headlightscript>();
					foreach (headlightscript childLight in lights)
					{
						if (childLight.name.ToLower().Contains(Name.ToLower()))
							headlight = childLight;
					}
					isInteriorLight = true;
				}
				else
				{
					headlight = save.GetComponent<headlightscript>();
				}

				// Unable to find headlight, return early.
				if (headlight == null) return;

				GameUtilities.SetHeadlightColor(headlight, Color, isInteriorLight);
			}
			catch (Exception ex)
			{
				Logger.Log($"Light data load error - {ex}", Logger.LogLevel.Error);
			}
		}
	}
}
