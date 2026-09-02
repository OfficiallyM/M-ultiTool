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
					Physics.Raycast(mainscript.M.player.Cam.transform.position, mainscript.M.player.Cam.transform.forward, out RaycastHit raycastHit, float.PositiveInfinity, mainscript.M.player.useLayer);

					// Require objects to have a tosaveitemscript in order to delete them.
					// This prevents players from deleting the world, buildings and other
					// stuff that would break the game.
					tosaveitemscript save = raycastHit.transform.gameObject.GetComponent<tosaveitemscript>();
					if (save != null)
					{
						save.removeFromMemory = true;

						foreach (tosaveitemscript component in raycastHit.transform.root.GetComponentsInChildren<tosaveitemscript>())
							component.removeFromMemory = true;
						UnityEngine.Object.Destroy(raycastHit.transform.root.gameObject);
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
