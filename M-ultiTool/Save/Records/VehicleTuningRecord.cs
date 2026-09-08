using MultiTool.UI.Tabs.VehicleConfiguration;
using MultiTool.Utilities;
using System;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.Save.Records
{
	internal class VehicleTuningRecord : SaveRecord, ISaveApplier
	{
		public VehicleTuning Tuning { get; set; }
		public VehicleTuning DefaultTuning { get; set; }

		public void Apply(tosaveitemscript save)
		{
			if (Tuning == null) return;

			try
			{
				GameUtilities.ApplyVehicleTuning(save.GetComponent<carscript>(), Tuning);
			}
			catch (Exception ex)
			{
				Logger.Log($"Vehicle tuning data load error - {ex}", Logger.LogLevel.Error);
			}
		}
	}
}
