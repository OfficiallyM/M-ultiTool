using MultiTool.Extensions;
using MultiTool.Services;
using MultiTool.UI;
using UnityEngine;
using UnityEngine.Rendering;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.Tools
{
	internal class ShowCollidersTool : Tool
	{
		public override string Name => "Show colliders";

		public override void ControlRender()
		{
			string name = Name.ToLowerInvariant();
			if (GUILayout.Button(Accessibility.GetAccessibleString($"Toggle {name} mode", Tools.IsActive(Id)), GUILayout.MaxWidth(200)))
				Tools.Toggle(Id);
			GUILayout.Space(10);
		}

		public override void Update()
		{
			if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.select).AssignedKey))
			{
				var obj = Raycast();
				Mesh mesh = itemdatabase.d.gerror.GetComponentInChildren<MeshFilter>().mesh;
				Material source;
				try
				{
					source = new Material(Shader.Find("Standard"));
					source.SetOverrideTag("RenderType", "Transparent");
					source.SetFloat("_SrcBlend", (float)BlendMode.One);
					source.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
					source.SetFloat("_ZWrite", 0.0f);
					source.DisableKeyword("_ALPHATEST_ON");
					source.DisableKeyword("_ALPHABLEND_ON");
					source.EnableKeyword("_ALPHAPREMULTIPLY_ON");
				}
				catch
				{
					source = new Material(mainscript.M.conditionmaterials[0].New);
				}
				foreach (Collider componentsInChild in obj.transform.root.GetComponentsInChildren<Collider>())
				{
					string str = "TEMPORARY DISPLAY CUBE " + componentsInChild.GetInstanceID();
					if (componentsInChild.transform.Find(str) != null)
					{
						UnityEngine.Object.DestroyImmediate(componentsInChild.transform.Find(str).gameObject);
					}
					else
					{
						GameObject gameObject = new GameObject(str);
						gameObject.transform.SetParent(componentsInChild.transform, false);
						if (componentsInChild.GetType() == typeof(BoxCollider))
						{
							gameObject.transform.localPosition = ((BoxCollider)componentsInChild).center;
							gameObject.transform.localScale = ((BoxCollider)componentsInChild).size;
							gameObject.transform.localRotation = Quaternion.identity;
							// Get the mesh based on the cube primitive mesh.
							gameObject.AddComponent<MeshFilter>().mesh = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshFilter>().mesh;
						}
						else if (componentsInChild.GetType() == typeof(CapsuleCollider))
						{
							CapsuleCollider collider = (CapsuleCollider)componentsInChild;
							gameObject.transform.localPosition = collider.center;
							// There's fuck all logic here, it was entirely trial and error.
							gameObject.transform.localScale = new Vector3(collider.radius * 2, collider.height / 2, collider.radius * 2);
							Vector3 axis = Vector3.up;
							float angle = 0;
							switch (collider.direction)
							{
								case 1:
									axis = Vector3.forward;
									break;
								case 2:
									axis = Vector3.right;
									angle = 90;
									break;
							}
							gameObject.transform.localRotation = Quaternion.AngleAxis(angle, axis);
							// Get the mesh based on the capsule primitive mesh.
							gameObject.AddComponent<MeshFilter>().mesh = GameObject.CreatePrimitive(PrimitiveType.Capsule).GetComponent<MeshFilter>().mesh;
						}
						else if (componentsInChild.GetType() == typeof(MeshCollider))
						{
							gameObject.transform.localEulerAngles = gameObject.transform.localPosition = Vector3.zero;
							gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
							gameObject.AddComponent<MeshFilter>().mesh = ((MeshCollider)componentsInChild).sharedMesh;
						}
						try
						{
							source = new Material(source);
							Color color = Services.Configuration.Config.BasicColliderColor;
							if (componentsInChild.isTrigger)
								color = Services.Configuration.Config.TriggerColliderColor;
							if (componentsInChild.gameObject.GetComponent<interiorscript>() != null)
								color = Services.Configuration.Config.InteriorColliderColor;
							source.SetColor("_Color", color);
						}
						catch
						{
						}
						gameObject.AddComponent<MeshRenderer>().material = source;
					}
				}
			}
		}

		public override void HudRender()
		{
			float fullWidth = Screen.width * 0.25f;
			float halfWidth = fullWidth / 2;

			GUILayout.BeginVertical();
			GUILayout.FlexibleSpace();

			GUILayout.BeginVertical("box", GUILayout.Width(fullWidth));
			GUILayout.BeginHorizontal();
			GUILayout.Button("Toggle colliders on looking object", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.select), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Button(Services.Configuration.Config.BasicColliderColor.GetName(), GUILayout.Width(halfWidth));
			GUILayout.Button("Standard collider", GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Button(Services.Configuration.Config.TriggerColliderColor.GetName(), GUILayout.Width(halfWidth));
			GUILayout.Button("Trigger collider", GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Button(Services.Configuration.Config.InteriorColliderColor.GetName(), GUILayout.Width(halfWidth));
			GUILayout.Button("Interior zone collider", GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
		}
	}
}
