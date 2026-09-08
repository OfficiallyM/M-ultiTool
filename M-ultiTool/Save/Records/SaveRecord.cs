using UnityEngine;

namespace MultiTool.Save.Records
{
	internal abstract class SaveRecord
	{
		// Matches tosaveitemscript.idInSave for records that attach to an existing scene object.
		public int ID { get; set; }

		// Stable identity for records that spawn their own object instead of matching onto
		// one that already exists (e.g. POIs). Left null for every other record type.
		// Assigned automatically by SaveRepository.Upsert - never set this by hand.
		public string InstanceID { get; set; }

		public bool RequiresInstantiation { get; set; } = false;

		// Only meaningful when RequiresInstantiation is true.
		public Vector3? SpawnPosition { get; set; }
		public Quaternion? SpawnRotation { get; set; }
	}
}
