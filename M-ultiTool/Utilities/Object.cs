using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using Logger = MultiTool.Modules.Logger;

namespace MultiTool.Utilities
{
	/// <summary>
	/// Object interaction utilities.
	/// </summary>
	public static class ObjectUtilities
	{
		/// <summary>
		/// Show colliders for a given GameObject.
		/// </summary>
		/// <param name="obj">Object to display colliders of</param>
		public static void ShowColliders(GameObject obj, Color colliderColor)
		{
			// Create material based off Standard shader.
			Material source;
			source = new Material(Shader.Find("Standard"));
			source.SetOverrideTag("RenderType", "Transparent");
			source.SetFloat("_SrcBlend", (float)BlendMode.One);
			source.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
			source.SetFloat("_ZWrite", 0.0f);
			source.DisableKeyword("_ALPHATEST_ON");
			source.DisableKeyword("_ALPHABLEND_ON");
			source.EnableKeyword("_ALPHAPREMULTIPLY_ON");

			foreach (Collider collider in obj.GetComponentsInChildren<Collider>())
			{
				string name = $"COLLIDER CUBE {collider.GetInstanceID()}";

				// Create collider.
				GameObject gameObject = new GameObject(name);
				gameObject.transform.SetParent(collider.transform, false);
				if (collider.GetType() == typeof(BoxCollider))
				{
					gameObject.transform.localPosition = ((BoxCollider)collider).center;
					gameObject.transform.localScale = ((BoxCollider)collider).size;
					gameObject.transform.localRotation = Quaternion.identity;
					// Get the mesh based on the cube primitive mesh.
					gameObject.AddComponent<MeshFilter>().mesh = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshFilter>().mesh;
				}
				else if (collider.GetType() == typeof(MeshCollider))
				{
					gameObject.transform.localEulerAngles = gameObject.transform.localPosition = Vector3.zero;
					gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
					gameObject.AddComponent<MeshFilter>().mesh = ((MeshCollider)collider).sharedMesh;
				}

				try
				{
					// Set collider color.
					source = new Material(source);
					source.SetColor("_Color", colliderColor);
				}
				catch
				{
				}
				gameObject.AddComponent<MeshRenderer>().material = source;
			}
		}

		/// <summary>
		/// Destroy visible colliders for a given GameObject.
		/// </summary>
		/// <param name="obj">GameObject to destroy visible colliders of</param>
		public static void DestroyColliders(GameObject obj)
		{
			foreach (Collider collider in obj.GetComponentsInChildren<Collider>())
			{
				string name = $"COLLIDER CUBE {collider.GetInstanceID()}";

				// Delete collider.
				UnityEngine.Object.DestroyImmediate(collider.transform.Find(name).gameObject);
			}
		}
	}
}
