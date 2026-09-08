using System;
using UnityEngine;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.Save.Records
{
	internal class SlotRecord : SaveRecord, ISaveApplier
	{
		public string Slot { get; set; }
		public Vector3 Position { get; set; }
		public Vector3 ResetPosition { get; set; }
		public Quaternion Rotation { get; set; }
		public Quaternion ResetRotation { get; set; }

		public void Apply(tosaveitemscript save)
		{
			try
			{
				// Find the child part.
				foreach (Transform transform in save.GetComponentsInChildren<Transform>())
				{
					// Apply position and rotation changes.
					if (transform.name == Slot)
					{
						transform.localPosition = Position;
						transform.localRotation = Rotation;
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Log($"Slot data load error - {ex}", Logger.LogLevel.Error);
			}
		}
	}
}
