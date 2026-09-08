using MultiTool.Save.Records;
using System.Collections.Generic;

namespace MultiTool.Save
{
	internal class Save
	{
		public List<SaveRecord> Records { get; set; } = new List<SaveRecord>();

		public PlayerData PlayerData { get; set; }
		public bool IsPlayerDataPerSave { get; set; } = false;

		public TimeData TimeData { get; set; }
	}
}
