using MultiTool.Services;
using MultiTool.UI.Tabs.VehicleConfiguration;
using MultiTool.Utilities;
using System;

namespace MultiTool.Save.Records
{
	internal class WheelTuningRecord : SaveRecord, ISaveApplier
	{
		public WheelTuning Tuning { get; set; }
		public WheelTuning DefaultTuning { get; set; }

		public void Apply(tosaveitemscript save)
		{
			if (Tuning == null) return;

			try
			{
				if (Tuning.Wheels == null || Tuning.Wheels.Count == 0) return;

				GameUtilities.RemapWheelTuning(save, Tuning);
				GameUtilities.ApplyWheelTuning(Tuning);
			}
			catch (Exception ex)
			{
				Logger.Log($"Wheel tuning data load error - {ex}", Logger.LogLevel.Error);
			}
		}
	}
}
