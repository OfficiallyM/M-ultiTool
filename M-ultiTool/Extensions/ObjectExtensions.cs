using MultiTool.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MultiTool.Extensions
{
	public static class ObjectExtensions
	{
		private static readonly MethodInfo CloneMethod = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

		public static bool IsPrimitive(this Type type)
		{
			if (type == typeof(String)) return true;
			return (type.IsValueType & type.IsPrimitive);
		}

		public static Object Copy(this Object originalObject)
		{
			return InternalCopy(originalObject, new Dictionary<Object, Object>(new ReferenceEqualityComparer()));
		}
		private static Object InternalCopy(Object originalObject, IDictionary<Object, Object> visited)
		{
			if (originalObject == null) return null;
			var typeToReflect = originalObject.GetType();
			if (IsPrimitive(typeToReflect)) return originalObject;
			if (visited.ContainsKey(originalObject)) return visited[originalObject];
			if (typeof(Delegate).IsAssignableFrom(typeToReflect)) return null;
			var cloneObject = CloneMethod.Invoke(originalObject, null);
			if (typeToReflect.IsArray)
			{
				var arrayType = typeToReflect.GetElementType();
				if (IsPrimitive(arrayType) == false)
				{
					Array clonedArray = (Array)cloneObject;
					clonedArray.ForEach((array, indices) => array.SetValue(InternalCopy(clonedArray.GetValue(indices), visited), indices));
				}

			}
			visited.Add(originalObject, cloneObject);
			CopyFields(originalObject, visited, cloneObject, typeToReflect);
			RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
			return cloneObject;
		}

		private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
		{
			if (typeToReflect.BaseType != null)
			{
				RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
				CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
			}
		}

		private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
		{
			foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags))
			{
				if (filter != null && filter(fieldInfo) == false) continue;
				if (IsPrimitive(fieldInfo.FieldType)) continue;
				var originalFieldValue = fieldInfo.GetValue(originalObject);
				var clonedFieldValue = InternalCopy(originalFieldValue, visited);
				fieldInfo.SetValue(cloneObject, clonedFieldValue);
			}
		}
		public static T Copy<T>(this T original)
		{
			return (T)Copy((Object)original);
		}

		/// <summary>
		/// Deep copy a DataContract object.
		/// </summary>
		/// <typeparam name="T">Object type</typeparam>
		/// <param name="obj">Object to copy</param>
		/// <returns>Deep copy of object</returns>
		public static T DeepCopy<T>(this T obj)
		{
			if (obj == null) return default;

			var serializer = new DataContractSerializer(typeof(T));

			using (var memoryStream = new MemoryStream())
			{
				serializer.WriteObject(memoryStream, obj);
				memoryStream.Seek(0, SeekOrigin.Begin);
				return (T)serializer.ReadObject(memoryStream);
			}
		}

		public static bool AreDataMembersEqual<T>(T obj1, T obj2)
		{
			if (obj1 == null || obj2 == null)
				return obj1 == null && obj2 == null;

			var type = obj1.GetType();
			if (type != obj2.GetType()) return false;

			var dataMembers = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
								  .Where(p => p.GetCustomAttribute<DataMemberAttribute>() != null);

			foreach (var field in dataMembers)
			{
				var value1 = field.GetValue(obj1);
				var value2 = field.GetValue(obj2);

				// Handle nulls.
				if (value1 == null || value2 == null)
				{
					if (value1 != value2)
						return false;
					continue;
				}

				// Check if it's a list or collection.
				else if (value1 is IEnumerable enumerable1 && value2 is IEnumerable enumerable2 && value1.GetType() != typeof(string))
				{
					var enum1 = enumerable1.Cast<object>().ToList();
					var enum2 = enumerable2.Cast<object>().ToList();

					if (enum1.Count != enum2.Count)
						return false;

					for (int i = 0; i < enum1.Count; i++)
					{
						var item1 = enum1[i];
						var item2 = enum2[i];

						if (!AreDataMembersEqual(item1, item2))
							return false;
					}

					continue;
				}

				// Basic equality check for primitives, strings, etc.
				else if (!Equals(value1, value2))
					return false;
			}

			return true;
		}
	}

	public class ReferenceEqualityComparer : EqualityComparer<Object>
	{
		public override bool Equals(object x, object y)
		{
			return ReferenceEquals(x, y);
		}
		public override int GetHashCode(object obj)
		{
			if (obj == null) return 0;
			return obj.GetHashCode();
		}
	}
}
