using MultiTool.Services;
using MultiTool.UI;
using System;
using UnityEngine;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.Tools
{
	internal class DeleteModeTool : Tool
	{
		public override string Name => "Delete mode";
		public override bool IsExclusive => false;

		public override void ControlRender()
		{
			if (GUILayout.Button(Accessibility.GetAccessibleString(Name, MultiTool.Tools.IsActive(Id)) + $" (Press {Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.deleteMode).AssignedKey})", GUILayout.MaxWidth(250)))
				Tools.Toggle(Id);
		}

		public override void Update()
		{
			try
			{
				if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.deleteMode).AssignedKey) && mainscript.M.player.seat == null)
				{
					var obj = Raycast();

					// Prevent players from deleting world objects like buildings.
					tosaveitemscript save = obj?.GetComponentInParent<tosaveitemscript>();
					var isBuilding = save?.GetComponent<buildingscript>() != null;
					if (save != null && !isBuilding)
					{
						save.removeFromMemory = true;

						foreach (tosaveitemscript component in save.transform.root.GetComponentsInChildren<tosaveitemscript>())
							component.removeFromMemory = true;
						UnityEngine.Object.Destroy(save.transform.root.gameObject);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Log($"Failed to delete entity. Details: {ex}", Logger.LogLevel.Warning);
			}
		}
	}
}
