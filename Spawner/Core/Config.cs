using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static SpawnerTLD.Modules.Keybinds;

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

		public static bool s_godMode = false;
		public bool godMode
		{
			get { return s_godMode; }
			set { s_godMode = value; }
		}

		public static bool s_noclip = false;
		public bool noclip
		{
			get { return s_noclip; }
			set { s_noclip = value; }
		}

		public static bool s_duplicateMode = false;
		public bool duplicateMode
		{
			get { return s_duplicateMode; }
			set { s_duplicateMode = value; }
		}
	}

	[DataContract]
	internal class ConfigSerializable
	{
		[DataMember] public List<Key> keybinds { get; set; }
		[DataMember] public bool? legacyUI { get; set; }
		[DataMember] public float scrollWidth { get; set; }
		[DataMember] public bool? noclipGodmodeDisable { get; set; }
	}
}
