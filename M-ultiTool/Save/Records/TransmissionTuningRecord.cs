using MultiTool.UI.Tabs.VehicleConfiguration;
using MultiTool.Utilities;
using System;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.Save.Records
{
	internal class TransmissionTuningRecord : SaveRecord, ISaveApplier
	{
		public TransmissionTuning Tuning { get; set; }
		public TransmissionTuning DefaultTuning { get; set; }

		public void Apply(tosaveitemscript save)
		{
			if (Tuning == null) return;

			try
			{
				var car = save.GetComponent<carscript>();
				GameUtilities.ApplyTransmissionTuning(car, Tuning);
				// Reset gear to neutral when loading to avoid the car being stuck in an invalid gear.
				car.gear = 0;
			}
			catch (Exception ex)
			{
				Logger.Log($"Transmission tuning data load error - {ex}", Logger.LogLevel.Error);
			}
		}
	}
}
