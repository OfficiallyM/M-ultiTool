using MultiTool.UI.Tabs.VehicleConfiguration;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MultiTool.Save
{
	[DataContract]
	internal class GlobalSave
	{
		[DataMember(Name = "playerData")] public PlayerData PlayerData { get; set; }
		[DataMember(Name = "tunes")] public List<TuningSave> Tunes { get; set; }
	}
}
