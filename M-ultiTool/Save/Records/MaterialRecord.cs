using MultiTool.Extensions;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.Save.Records
{
	internal class MaterialRecord : SaveRecord, ISaveApplier
	{
		public string Part { get; set; }
		public string Parent { get; set; }
		public bool? IsConditionless { get; set; } = false;
		public bool Exact { get; set; }
		public string Type { get; set; }
		public Color? Color { get; set; }

		public void Apply(tosaveitemscript save)
		{
			try
			{
				if (IsConditionless.HasValue && IsConditionless.Value)
				{
					// Conditionless parts are always matched by exact name.
					MeshRenderer mesh = GameUtilities.GetConditionlessVehiclePartByName(save.gameObject, Part);
					GameUtilities.SetConditionlessPartMaterial(mesh, Type, Color);
				}
				else
				{
					// Standard part.
					List<partconditionscript> parts = new List<partconditionscript>();

					if (Exact)
					{
						partconditionscript part = GameUtilities.GetVehiclePartByName(save.gameObject, Part, false);
						if (part != null)
							parts.Add(part);
						// Match by partial name as a failover.
						else
						{
							List<partconditionscript> matchedParts = GameUtilities.GetVehiclePartsByPartialName(save.gameObject, Part, false);
							if (matchedParts.Count > 0)
								parts.AddRange(matchedParts);
						}
					}
					else
					{
						List<partconditionscript> matchedParts = GameUtilities.GetVehiclePartsByPartialName(save.gameObject, Part, false);
						if (matchedParts.Count > 0)
							parts.AddRange(matchedParts);
					}

					foreach (partconditionscript part in parts)
					{
						// Skip any parts where the parent doesn't match.
						if (Parent != null)
							if (Parent != (part.transform.parent?.name ?? part.name).SanitiseName()) continue;

						GameUtilities.SetPartMaterial(part, Type, Color);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Log($"Material data load error - {ex}", Logger.LogLevel.Error);
			}
		}
	}
}
