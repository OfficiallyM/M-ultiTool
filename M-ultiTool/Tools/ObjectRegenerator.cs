using MultiTool.Database;
using MultiTool.Services;
using MultiTool.UI;
using System.Linq;
using UnityEngine;

namespace MultiTool.Tools
{
	internal class ObjectRegeneratorTool : Tool
	{
		public override string Name => "Object Regenerator";
		public override bool UsesObjectSelection => true;
		public override bool UsesDefaultObjectSelectionUI => false;

		public override void ControlRender()
		{
			string name = Name.ToLowerInvariant();
			if (GUILayout.Button(Accessibility.GetAccessibleString($"Toggle {name} mode", Tools.IsActive(Id)), GUILayout.MaxWidth(200)))
				Tools.Toggle(Id);
			GUILayout.Space(10);
		}

		public override void HudRender()
		{
			// Deliberately not using the default selection UI so we
			// can show the object ID.
			GUILayout.BeginVertical();
			GUILayout.Space(Screen.height * 0.05f);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Button(
				$"Selected object: {(SelectedObject != null ? SelectedObject.name + $" ({SelectedObject.idInSave})" : "None")}\n{Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.action1)} to {(SelectedObject != null ? "deselect" : "select")}",
				GUILayout.MinHeight(50f)
			);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			if (SelectedObject == null) return;

			float fullWidth = Screen.width * 0.2f;
			float halfWidth = fullWidth / 2;

			GUILayout.BeginVertical();
			GUILayout.FlexibleSpace();

			GUILayout.BeginVertical("box", GUILayout.Width(fullWidth));
			GUILayout.BeginHorizontal();
			GUILayout.Button("Select object", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.action1), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Button("Regenerate object", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.action4), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
		}

		public override void Update()
		{
			if (SelectedObject == null) return;

			if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.action4).AssignedKey))
			{
				GameObject gameObject = SelectedObject.gameObject;
				Item prefab = GUIRenderer.Items.FirstOrDefault(i => i.GameObject.name == gameObject.name.Replace("(Clone)", ""));
				if (prefab == null)
					return;

				Vector3 position = gameObject.transform.position;
				Quaternion rotation = gameObject.transform.rotation;

				// Recreate object.
				GameObject spawned = SpawnUtilities.Spawn(prefab, position, rotation, Services.State.SpawnWithFuel);
				SelectedObject = spawned.GetComponent<tosaveitemscript>();

				// Handle attached children.
				foreach (attachablescript attached in gameObject.GetComponentsInChildren<attachablescript>())
				{
					if (attached.targetTosave == null || attached.targetTosave.gameObject != gameObject) continue;

					attached.Detach();
					attached.targetTosave = spawned.GetComponent<tosaveitemscript>();
					attached.Load(attached.pointLocalPos);
				}

				// Re-Set object parent if required.
				attachablescript attach = gameObject.GetComponent<attachablescript>();
				if (attach != null && attach.targetTosave != null)
				{
					attachablescript newAttach = spawned.GetComponent<attachablescript>();
					if (newAttach != null)
					{
						tosaveitemscript attachSave = attach.targetTosave;
						attach.Detach();
						newAttach.targetTosave = attachSave;
						newAttach.Load(attach.pointLocalPos);
					}
				}

				partslotscript oldSlot = gameObject.GetComponent<partscript>()?.slot;

				// Destroy the old object.
				SelectedObject.removeFromMemory = true;
				foreach (tosaveitemscript component in gameObject.GetComponentsInChildren<tosaveitemscript>())
				{
					component.removeFromMemory = true;
				}
				UnityEngine.Object.Destroy(gameObject);

				// Mount the new part if it was previously mounted.
				// TODO: Doesn't actually mount.
				// Also, anything mounted to something you're regenerating gets destroyed.
				if (oldSlot != null)
				{
					partscript part = spawned.GetComponent<partscript>();
					if (oldSlot != null)
					{
						oldSlot.Craft(part);
						part.tosaveitem.Claim(false);
					}
				}
			}
		}
	}
}
