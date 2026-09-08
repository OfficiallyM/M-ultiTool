using MultiTool.Services;
using System;

namespace MultiTool.Save.Records
{
	internal class WeightRecord : SaveRecord, ISaveApplier
	{
		public float Mass { get; set; }
		public float DefaultMass { get; set; }

		public void Apply(tosaveitemscript save)
		{
			try
			{
				massScript mass = save.GetComponent<massScript>();
				mass.SetMass(Mass);
			}
			catch (Exception ex)
			{
				Logger.Log($"Weight data load error - {ex}", Logger.LogLevel.Error);
			}
		}
	}
}
