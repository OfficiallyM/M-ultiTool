using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace MultiTool.Core
{
	[DataContract]
	internal class POIData
	{
		[DataMember] public int ID { get; set; }
		[DataMember] public string poi { get; set; }
		[DataMember] public Vector3 position { get; set; }
		[DataMember] public Quaternion rotation { get; set; }
	}

	[DataContract]
	internal class GlassData
	{
		[DataMember] public int ID { get; set; }
		[DataMember] public Color color { get; set; }
		[DataMember] public string type { get; set; }
	}

	[DataContract]
	internal class MaterialData
	{
		[DataMember] public int ID { get; set; }
		[DataMember] public string part { get; set; }
		[DataMember] public string parent { get; set; }
        [DataMember] public bool? isConditionless { get; set; } = false;
		[DataMember] public bool exact { get; set; }
		[DataMember] public string type { get; set; }
		[DataMember] public Color? color { get; set; }
	}

	[DataContract]
	internal class ScaleData
	{
		[DataMember] public int ID { get; set; }
		[DataMember] public Vector3 scale { get; set; }
	}

	[DataContract]
	internal class SlotData
	{
		[DataMember] public int ID { get; set; }
		[DataMember] public string slot { get; set; }
		[DataMember] public Vector3 position { get; set; }
		[DataMember] public Vector3 resetPosition { get; set; }
		[DataMember] public Quaternion rotation { get; set; }
		[DataMember] public Quaternion resetRotation { get; set; }
	}

    [DataContract]
    internal class LightData
    {
        [DataMember] public int ID { get; set; }
        [DataMember] public string name { get; set; }
        [DataMember] public Color color { get; set; }
    }

    [DataContract]
    internal class EngineTuningData
    {
        [DataMember] public int ID { get; set; }
        [DataMember] public EngineTuning tuning { get; set; }
		[DataMember] public EngineTuning defaultTuning { get; set; }
    }

    [DataContract]
    internal class TransmissionTuningData
    {
        [DataMember] public int ID { get; set; }
        [DataMember] public TransmissionTuning tuning { get; set; }
		[DataMember] public TransmissionTuning defaultTuning { get; set; }
	}

    [DataContract]
    internal class VehicleTuningData
    {
        [DataMember] public int ID { get; set; }
        [DataMember] public VehicleTuning tuning { get; set; }
		[DataMember] public VehicleTuning defaultTuning { get; set; }
	}

	[DataContract]
	internal class WheelTuningData
	{
		[DataMember] public int ID { get; set; }
		[DataMember] public WheelTuning tuning { get; set; }
		[DataMember] public WheelTuning defaultTuning { get; set; }
	}

	[DataContract]
	internal class WeightData
	{
		[DataMember] public int ID { get; set; }
		[DataMember] public float mass { get; set; }
		[DataMember] public float defaultMass { get; set; }
	}

    [DataContract]
    internal class PlayerData
    {
        [DataMember] public float walkSpeed { get; set; }
        [DataMember] public float runSpeed { get; set; }
        [DataMember] public float jumpForce { get; set; }
        [DataMember] public float pushForce { get; set; }
        [DataMember] public float carryWeight { get; set; }
        [DataMember] public float pickupForce { get; set; }
		[DataMember] public float throwForce { get; set; }
		[DataMember] public float pedalSpeed { get; set; }
        [DataMember] public bool infiniteAmmo { get; set; }
        [DataMember] public float mass { get; set; }
		[DataMember] public bool clickTeleport { get; set; }
    }

    [DataContract]
	internal class Save
	{
		[DataMember] public List<POIData> pois { get; set; }
		[DataMember] public List<GlassData> glass { get; set; }
		[DataMember] public List<MaterialData> materials { get; set; }
		[DataMember] public List<ScaleData> scale { get; set; }
		[DataMember] public List<SlotData> slots { get; set; }
        [DataMember] public List<LightData> lights { get; set; }
        [DataMember] public List<EngineTuningData> engineTuning { get; set; }
        [DataMember] public List<TransmissionTuningData> transmissionTuning { get; set; }
        [DataMember] public List<VehicleTuningData> vehicleTuning { get; set; }
		[DataMember] public List<WheelTuningData> wheelTuning { get; set; }
		[DataMember] public List<WeightData> weight { get; set; }

        [DataMember] public PlayerData playerData { get; set; }
        [DataMember] public bool isPlayerDataPerSave { get; set; } = false;
    }

    [DataContract]
    internal class GlobalSave
    {
        [DataMember] public PlayerData playerData { get; set; }
    }
}
