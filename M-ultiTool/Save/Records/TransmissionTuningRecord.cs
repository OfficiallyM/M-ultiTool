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
				GameUtilities.ApplyTransmissionTuning(save.GetComponent<carscript>(), Tuning);
			}
			catch (Exception ex)
			{
				Logger.Log($"Transmission tuning data load error - {ex}", Logger.LogLevel.Error);
			}
		}
	}
}
