using System;
using UnityEngine;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.Save.Records
{
	internal class ScaleRecord : SaveRecord, ISaveApplier
	{
		public Vector3 Scale { get; set; }
		public Vector3 DefaultScale { get; set; }

		public void Apply(tosaveitemscript save)
		{
			try
			{
				save.gameObject.transform.localScale = Scale;
			}
			catch (Exception ex)
			{
				Logger.Log($"Scale data load error - {ex}", Logger.LogLevel.Error);
			}
		}
	}
}
