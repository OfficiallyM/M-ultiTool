using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SpawnerTLD.Core
{
	// Serializable vehicle wrapper for translation config.
	[DataContract]
	internal class ConfigVehicle
	{
		[DataMember] public string objectName { get; set; }
		[DataMember] public int? variant { get; set; }
		[DataMember] public string name { get; set; }
	}

	[DataContract]
	internal class ConfigWrapper
	{
		[DataMember] public List<ConfigVehicle> vehicles { get; set; }
	}

	internal class Settings
	{
		public static bool s_deleteMode = false;
		public bool deleteMode
		{
			get { return s_deleteMode; }
			set { s_deleteMode = value; }
		}
	}
}
