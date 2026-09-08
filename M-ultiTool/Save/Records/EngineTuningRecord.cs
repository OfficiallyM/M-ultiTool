using MultiTool.UI.Tabs.VehicleConfiguration;
using MultiTool.Utilities;
using System;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.Save.Records
{
	internal class EngineTuningRecord : SaveRecord, ISaveApplier
	{
		public EngineTuning Tuning { get; set; }
		public EngineTuning DefaultTuning { get; set; }

		public void Apply(tosaveitemscript save)
		{
			if (Tuning == null) return;

			try
			{
				GameUtilities.ApplyEngineTuning(save.GetComponent<enginescript>(), Tuning);
			}
			catch (Exception ex)
			{
				Logger.Log($"Engine tuning data load error - {ex}", Logger.LogLevel.Error);
			}
		}
	}
}
