using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiTool.Extensions
{
	public static class TransformExtensions
	{
		/// <summary>
		/// Find by name recursively.
		/// </summary>
		/// <param name="parent">Parent transform</param>
		/// <param name="n">Name to search by</param>
		/// <returns>Transform if found, otherwise null</returns>
		public static Transform FindRecursive(this Transform parent, string n, bool exact = true, bool active = true)
		{
			foreach (Transform child in parent)
			{
				bool check = child.name == n;
				if (!exact)
					check = child.name.Contains(n);
				if (active)
					check = check && child.gameObject.activeSelf;
				if (check)
					return child;
				else
				{
					Transform found = FindRecursive(child, n, exact, active);
					if (found != null)
						return found;
				}
			}
			return null;
		}

		/// <summary>
		/// Get the full path for a transform.
		/// </summary>
		/// <param name="current">The current transform</param>
		/// <returns>Transform path</returns>
		public static string GetPath(this Transform current)
		{
			if (current.parent == null)
				return "/" + current.GetInstanceID();
			return current.parent.GetPath() + "/" + current.GetInstanceID();
		}
	}
}
