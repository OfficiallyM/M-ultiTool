﻿using System.Collections.Generic;
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
	internal class Save
	{
		[DataMember] public List<POIData> pois { get; set; }
		[DataMember] public List<GlassData> glass { get; set; }
		[DataMember] public List<MaterialData> materials { get; set; }
		[DataMember] public List<ScaleData> scale { get; set; }
		[DataMember] public List<SlotData> slots { get; set; }
	}
}
