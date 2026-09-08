using MultiTool.UI.Tabs.VehicleConfiguration;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace MultiTool.Save
{
	// Everything in this file is the pre-rewrite save shape, read via DataContractJsonSerializer
	// exactly as the old Save.cs did. Only SaveMigration should ever touch these - once a save
	// has been migrated it's written back out in the new format and these classes are never
	// consulted for it again.

	[DataContract]
	internal class POIDataLegacy
	{
		[DataMember] public int ID { get; set; }
		[DataMember(Name = "poi")] public string Poi { get; set; }
		[DataMember(Name = "position")] public Vector3 Position { get; set; }
		[DataMember(Name = "rotation")] public Quaternion Rotation { get; set; }
	}

	[DataContract]
	internal class GlassDataLegacy
	{
		[DataMember] public int ID { get; set; }
		[DataMember(Name = "color")] public Color Color { get; set; }
		[DataMember(Name = "type")] public string Type { get; set; }
	}

	[DataContract]
	internal class MaterialDataLegacy
	{
		[DataMember] public int ID { get; set; }
		[DataMember(Name = "part")] public string Part { get; set; }
		[DataMember(Name = "parent")] public string Parent { get; set; }
		[DataMember(Name = "isConditionless")] public bool? IsConditionless { get; set; } = false;
		[DataMember(Name = "exact")] public bool Exact { get; set; }
		[DataMember(Name = "type")] public string Type { get; set; }
		[DataMember(Name = "color")] public Color? Color { get; set; }
	}

	[DataContract]
	internal class ScaleDataLegacy
	{
		[DataMember] public int ID { get; set; }
		[DataMember(Name = "scale")] public Vector3 Scale { get; set; }
	}

	[DataContract]
	internal class SlotDataLegacy
	{
		[DataMember] public int ID { get; set; }
		[DataMember(Name = "slot")] public string Slot { get; set; }
		[DataMember(Name = "position")] public Vector3 Position { get; set; }
		[DataMember(Name = "resetPosition")] public Vector3 ResetPosition { get; set; }
		[DataMember(Name = "rotation")] public Quaternion Rotation { get; set; }
		[DataMember(Name = "resetRotation")] public Quaternion ResetRotation { get; set; }
	}

	[DataContract]
	internal class LightDataLegacy
	{
		[DataMember] public int ID { get; set; }
		[DataMember(Name = "name")] public string Name { get; set; }
		[DataMember(Name = "color")] public Color Color { get; set; }
	}

	[DataContract]
	internal class EngineTuningDataLegacy
	{
		[DataMember] public int ID { get; set; }
		[DataMember(Name = "tuning")] public EngineTuning Tuning { get; set; }
		[DataMember(Name = "defaultTuning")] public EngineTuning DefaultTuning { get; set; }
	}

	[DataContract]
	internal class TransmissionTuningDataLegacy
	{
		[DataMember] public int ID { get; set; }
		[DataMember(Name = "tuning")] public TransmissionTuning Tuning { get; set; }
		[DataMember(Name = "defaultTuning")] public TransmissionTuning DefaultTuning { get; set; }
	}

	[DataContract]
	internal class VehicleTuningDataLegacy
	{
		[DataMember] public int ID { get; set; }
		[DataMember(Name = "tuning")] public VehicleTuning Tuning { get; set; }
		[DataMember(Name = "defaultTuning")] public VehicleTuning DefaultTuning { get; set; }
	}

	[DataContract]
	internal class WheelTuningDataLegacy
	{
		[DataMember] public int ID { get; set; }
		[DataMember(Name = "tuning")] public WheelTuning Tuning { get; set; }
		[DataMember(Name = "defaultTuning")] public WheelTuning DefaultTuning { get; set; }
	}

	[DataContract]
	internal class WeightDataLegacy
	{
		[DataMember] public int ID { get; set; }
		[DataMember(Name = "mass")] public float Mass { get; set; }
		[DataMember(Name = "defaultMass")] public float DefaultMass { get; set; }
	}

	[DataContract]
	internal class TankDataLegacy
	{
		[DataMember] public int ID { get; set; }
		[DataMember(Name = "capacity")] public float Capacity { get; set; }
		[DataMember(Name = "defaultCapacity")] public float DefaultCapacity { get; set; }
	}

	[DataContract]
	internal class PlayerDataLegacy
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

	[DataContract]
	internal class TimeDataLegacy
	{
		[DataMember(Name = "timescale")] public float Timescale { get; set; }
		[DataMember(Name = "dayLength")] public float DayLength { get; set; }
		[DataMember(Name = "nightLength")] public float NightLength { get; set; }
	}

	[DataContract]
	internal class SaveLegacy
	{
		[DataMember(Name = "pois")] public List<POIDataLegacy> Pois { get; set; }
		[DataMember(Name = "glass")] public List<GlassDataLegacy> Glass { get; set; }
		[DataMember(Name = "materials")] public List<MaterialDataLegacy> Materials { get; set; }
		[DataMember(Name = "scale")] public List<ScaleDataLegacy> Scale { get; set; }
		[DataMember(Name = "slots")] public List<SlotDataLegacy> Slots { get; set; }
		[DataMember(Name = "lights")] public List<LightDataLegacy> Lights { get; set; }
		[DataMember(Name = "engineTuning")] public List<EngineTuningDataLegacy> EngineTuning { get; set; }
		[DataMember(Name = "transmissionTuning")] public List<TransmissionTuningDataLegacy> TransmissionTuning { get; set; }
		[DataMember(Name = "vehicleTuning")] public List<VehicleTuningDataLegacy> VehicleTuning { get; set; }
		[DataMember(Name = "wheelTuning")] public List<WheelTuningDataLegacy> WheelTuning { get; set; }
		[DataMember(Name = "weight")] public List<WeightDataLegacy> Weight { get; set; }
		[DataMember(Name = "tank")] public List<TankDataLegacy> Tank { get; set; }

		[DataMember(Name = "playerData")] public PlayerDataLegacy PlayerData { get; set; }
		[DataMember(Name = "isPlayerDataPerSave")] public bool IsPlayerDataPerSave { get; set; } = false;

		[DataMember(Name = "timeData")] public TimeDataLegacy TimeData { get; set; }
	}
}
