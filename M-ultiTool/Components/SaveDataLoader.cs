using MultiTool.Utilities;
using UnityEngine;
using Logger = MultiTool.Modules.Logger;

namespace MultiTool.Components
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
