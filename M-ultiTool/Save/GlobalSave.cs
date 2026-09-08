using MultiTool.UI.Tabs.VehicleConfiguration;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MultiTool.Save
{
	// Deliberately not touched by this rewrite - still DataContractJsonSerializer, same as
	// before. TuningSave.Tuning is the one remaining polymorphic ITuning case, still relying
	// on the [DataContract(Namespace = "...")] pins added in stage 1. Worth migrating onto the
	// same SaveRepository/SaveSerializationBinder pattern later, but that's a separate,
	// lower-risk pass once the per-save Records system above has bedded in.
	[DataContract]
	internal class GlobalSave
	{
		[DataMember(Name = "playerData")] public PlayerData PlayerData { get; set; }
		[DataMember(Name = "tunes")] public List<TuningSave> Tunes { get; set; }
	}
}
