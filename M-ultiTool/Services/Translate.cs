using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MultiTool.Services
{
	[DataContract]
	internal class Translatable
	{
		[DataMember] public string objectName { get; set; }
		[DataMember] public int? variant { get; set; }
		[DataMember] public string name { get; set; }
	}

	[DataContract]
	internal class Translate
	{
		[DataMember] public List<Translatable> vehicles { get; set; }
		[DataMember] public List<Translatable> POIs { get; set; }
		[DataMember] public List<Translatable> menuVehicles { get; set; }
	}
}
