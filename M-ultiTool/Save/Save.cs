using MultiTool.UI.Tabs.VehicleConfiguration;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace MultiTool.Save
{
	[DataContract]
	internal class POIData
	{
		[DataMember] public int ID { get; set; }
		[DataMember(Name = "poi")] public string Poi { get; set; }
		[DataMember(Name = "position")] public Vector3 Position { get; set; }
		[DataMember(Name = "rotation")] public Quaternion Rotation { get; set; }
	}

	[DataContract]
	internal class GlassData
	{
		[DataMember] public int ID { get; set; }
		[DataMember(Name = "color")] public Color Color { get; set; }
		[DataMember(Name = "type")] public string Type { get; set; }
	}

	[DataContract]
	internal class MaterialData
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
	internal class ScaleData
	{
		[DataMember] public int ID { get; set; }
		[DataMember(Name = "scale")] public Vector3 Scale { get; set; }
	}

	[DataContract]
	internal class SlotData
	{
		[DataMember] public int ID { get; set; }
		[DataMember(Name = "slot")] public string Slot { get; set; }
		[DataMember(Name = "position")] public Vector3 Position { get; set; }
		[DataMember(Name = "resetPosition")] public Vector3 ResetPosition { get; set; }
		[DataMember(Name = "rotation")] public Quaternion Rotation { get; set; }
		[DataMember(Name = "resetRotation")] public Quaternion ResetRotation { get; set; }
	}

	[DataContract]
	internal class LightData
	{
		[DataMember] public int ID { get; set; }
		[DataMember(Name = "name")] public string Name { get; set; }
		[DataMember(Name = "color")] public Color Color { get; set; }
	}

	[DataContract]
	internal class EngineTuningData
	{
		[DataMember] public int ID { get; set; }
		[DataMember(Name = "tuning")] public EngineTuning Tuning { get; set; }
		[DataMember(Name = "defaultTuning")] public EngineTuning DefaultTuning { get; set; }
	}

	[DataContract]
	internal class TransmissionTuningData
	{
		[DataMember] public int ID { get; set; }
		[DataMember(Name = "tuning")] public TransmissionTuning Tuning { get; set; }
		[DataMember(Name = "defaultTuning")] public TransmissionTuning DefaultTuning { get; set; }
	}

	[DataContract]
	internal class VehicleTuningData
	{
		[DataMember] public int ID { get; set; }
		[DataMember(Name = "tuning")] public VehicleTuning Tuning { get; set; }
		[DataMember(Name = "defaultTuning")] public VehicleTuning DefaultTuning { get; set; }
	}

	[DataContract]
	internal class WheelTuningData
	{
		[DataMember] public int ID { get; set; }
		[DataMember(Name = "tuning")] public WheelTuning Tuning { get; set; }
		[DataMember(Name = "defaultTuning")] public WheelTuning DefaultTuning { get; set; }
	}

	[DataContract]
	internal class WeightData
	{
		[DataMember] public int ID { get; set; }
		[DataMember(Name = "mass")] public float Mass { get; set; }
		[DataMember(Name = "defaultMass")] public float DefaultMass { get; set; }
	}

	[DataContract]
	internal class TankData
	{
		[DataMember] public int ID { get; set; }
		[DataMember(Name = "capacity")] public float Capacity { get; set; }
		[DataMember(Name = "defaultCapacity")] public float DefaultCapacity { get; set; }
	}

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

	[DataContract]
	internal class TimeData
	{
		[DataMember(Name = "timescale")] public float Timescale { get; set; }
		[DataMember(Name = "dayLength")] public float DayLength { get; set; }
		[DataMember(Name = "nightLength")] public float NightLength { get; set; }
	}

	[DataContract]
	internal class Save
	{
		[DataMember(Name = "pois")] public List<POIData> Pois { get; set; }
		[DataMember(Name = "glass")] public List<GlassData> Glass { get; set; }
		[DataMember(Name = "materials")] public List<MaterialData> Materials { get; set; }
		[DataMember(Name = "scale")] public List<ScaleData> Scale { get; set; }
		[DataMember(Name = "slots")] public List<SlotData> Slots { get; set; }
		[DataMember(Name = "lights")] public List<LightData> Lights { get; set; }
		[DataMember(Name = "engineTuning")] public List<EngineTuningData> EngineTuning { get; set; }
		[DataMember(Name = "transmissionTuning")] public List<TransmissionTuningData> TransmissionTuning { get; set; }
		[DataMember(Name = "vehicleTuning")] public List<VehicleTuningData> VehicleTuning { get; set; }
		[DataMember(Name = "wheelTuning")] public List<WheelTuningData> WheelTuning { get; set; }
		[DataMember(Name = "weight")] public List<WeightData> Weight { get; set; }
		[DataMember(Name = "tank")] public List<TankData> Tank { get; set; }

		[DataMember(Name = "playerData")] public PlayerData PlayerData { get; set; }
		[DataMember(Name = "isPlayerDataPerSave")] public bool IsPlayerDataPerSave { get; set; } = false;

		[DataMember(Name = "timeData")] public TimeData TimeData { get; set; }
	}

	[DataContract]
	internal class GlobalSave
	{
		[DataMember(Name = "playerData")] public PlayerData PlayerData { get; set; }
		[DataMember(Name = "tunes")] public List<TuningSave> Tunes { get; set; }
	}
}