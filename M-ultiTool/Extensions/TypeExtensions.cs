using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiTool.Extensions
{
	public static class TypeExtensions
	{
		private static readonly Dictionary<Type, string> Aliases = new Dictionary<Type, string>
		{
			{ typeof(void), "void" }, { typeof(bool), "bool" }, { typeof(byte), "byte" },
			{ typeof(sbyte), "sbyte" }, { typeof(char), "char" }, { typeof(decimal), "decimal" },
			{ typeof(double), "double" }, { typeof(float), "float" }, { typeof(int), "int" },
			{ typeof(uint), "uint" }, { typeof(long), "long" }, { typeof(ulong), "ulong" },
			{ typeof(short), "short" }, { typeof(ushort), "ushort" }, { typeof(string), "string" },
			{ typeof(object), "object" }
		};

		public static string GetFriendlyName(this Type type)
		{
			if (type == null) return "null";

			// Handle nullable types.
			if (Nullable.GetUnderlyingType(type) is Type underlying)
			{
				return $"{underlying.GetFriendlyName()}?";
			}

			// Handle arrays.
			if (type.IsArray)
			{
				return $"{type.GetElementType().GetFriendlyName()}[]";
			}

			// Handle generic types like List<T>, Dictionary<TKey, TValue>, etc.
			if (type.IsGenericType)
			{
				var genericTypeDefName = type.Name;
				var backtickIndex = genericTypeDefName.IndexOf('`');
				if (backtickIndex > 0)
					genericTypeDefName = genericTypeDefName.Substring(0, backtickIndex);

				var genericArgs = type.GetGenericArguments();
				string genericArgsNames = string.Join(", ", genericArgs.Select(t => t.GetFriendlyName()));
				return $"{genericTypeDefName}<{genericArgsNames}>";
			}

			// Check for alias (int, float, etc.).
			if (Aliases.TryGetValue(type, out var alias))
				return alias;

			// Default to just the type name.
			return type.Name;
		}
	}
}
