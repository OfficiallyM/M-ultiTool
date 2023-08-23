using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace SpawnerTLD.Core
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
	internal class Save
	{
		[DataMember] public List<POIData> pois { get; set; }
	}
}
