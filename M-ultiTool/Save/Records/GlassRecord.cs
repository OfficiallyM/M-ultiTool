using MultiTool.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.Save.Records
{
	internal class GlassRecord : SaveRecord, ISaveApplier
	{
		public Color Color { get; set; }
		public string Type { get; set; }

		public void Apply(tosaveitemscript save)
		{
			try
			{
				switch (Type)
				{
					case "windows":
						// Set window colour.
						List<MeshRenderer> renderers = save.gameObject.GetComponentsInChildren<MeshRenderer>().ToList();
						foreach (MeshRenderer meshRenderer in renderers)
						{
							string materialName = meshRenderer.material.name.Replace(" (Instance)", "");
							switch (materialName)
							{
								// Outer glass.
								case "Glass":
									// Use selected colour.
									meshRenderer.material.color = Color;
									break;

								// Inner glass.
								case "GlassNoReflection":
									// Use a more transparent version of the selected colour
									// for the inner glass to ensure it's still see-through.
									Color innerColor = Color;
									if (innerColor.a > 0.2f)
										innerColor.a = 0.2f;
									meshRenderer.material.color = innerColor;
									break;
							}
						}
						break;
					case "sunroof":
						// Set sunroof colour.
						GameObject car = save.gameObject;
						Transform sunRoofSlot = car.transform.FindRecursive("SunRoofSlot");
						Transform outerGlass = sunRoofSlot.FindRecursive("sunroof outer glass", exact: false);
						if (outerGlass != null)
						{
							MeshRenderer meshRenderer = outerGlass.GetComponent<MeshRenderer>();
							meshRenderer.material.color = Color;
						}
						break;
				}
			}
			catch (Exception ex)
			{
				Logger.Log($"Glass load error - {ex}", Logger.LogLevel.Error);
			}
		}
	}
}
