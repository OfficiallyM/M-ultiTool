using System.Runtime.Serialization;

namespace MultiTool.Save
{
	// [DataContract]/[DataMember] attributes are kept here even though the new Save system uses
	// Newtonsoft (which ignores them) - GlobalSave still uses DataContractJsonSerializer and
	// needs them to keep reading/writing GlobalData.json in its existing format. See the note
	// above GlobalSave in GlobalSave.cs.
	[DataContract]
	internal class PlayerData
	{
		[DataMember(Name = "walkSpeed")] public float WalkSpeed { get; set; }
		[DataMember(Name = "runSpeed")] public float RunSpeed { get; set; }
		[DataMember(Name = "jumpForce")] public float JumpForce { get; set; }
		[DataMember(Name = "pushForce")] public float PushForce { get; set; }
		[DataMember(Name = "carryWeight")] public float CarryWeight { get; set; }
		[DataMember(Name = "pickupForce")] public float PickupForce { get; set; }
		[DataMember(Name = "throwForce")] public float ThrowForce { get; set; }
		[DataMember(Name = "pedalSpeed")] public float PedalSpeed { get; set; }
		[DataMember(Name = "infiniteAmmo")] public bool InfiniteAmmo { get; set; }
		[DataMember(Name = "mass")] public float Mass { get; set; }
		[DataMember(Name = "clickTeleport")] public bool ClickTeleport { get; set; }
	}
}
