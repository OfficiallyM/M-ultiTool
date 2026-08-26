using UnityEngine;

namespace MultiTool.Save
{
	internal class SaveDataLoader : MonoBehaviour
	{
		public void Start()
		{
			tosaveitemscript save = gameObject.GetComponentInChildren<tosaveitemscript>();
			if (save == null) return;

			SaveUtilities.TriggerSaveLoad(save);
		}
	}
}
