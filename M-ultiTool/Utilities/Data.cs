using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MultiTool.Utilities
{
	internal static class DataUtilities
	{
		private static List<Type> _allComponentTypes;

		public static IReadOnlyList<Type> AllComponentTypes
		{
			get
			{
				if (_allComponentTypes == null)
				{
					_allComponentTypes = AppDomain.CurrentDomain.GetAssemblies()
						.SelectMany(assembly =>
						{
							try { 
								return assembly.GetTypes();
							}
							catch (ReflectionTypeLoadException e) { 
								return e.Types.Where(t => t != null);
							}
						})
						.Where(t =>
							t != null &&
							typeof(Component).IsAssignableFrom(t) &&
							!t.IsAbstract)
						.OrderBy(t => t.Name)
						.ToList();
				}
				return _allComponentTypes;
			}
		}
	}
}
