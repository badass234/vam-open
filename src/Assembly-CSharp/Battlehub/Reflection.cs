using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Battlehub
{
	public static class Reflection
	{
		public static object GetDefault(Type type)
		{
			if (type == typeof(string))
			{
				return string.Empty;
			}
			if (type.IsValueType())
			{
				return Activator.CreateInstance(type);
			}
			return null;
		}

		public static bool IsScript(this Type type)
		{
			return type.IsSubclassOf(typeof(MonoBehaviour));
		}

		public static PropertyInfo[] GetSerializableProperties(this Type type)
		{
			return type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(IsSerializable).ToArray();
		}

		public static FieldInfo[] GetSerializableFields(this Type type)
		{
			return type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(IsSerializable).ToArray();
		}

		private static bool IsSerializable(this FieldInfo field)
		{
			return (field.IsPublic || field.IsDefined(typeof(SerializeField), false)) && !field.IsDefined(typeof(SerializeIgnore), false);
		}

		private static bool IsSerializable(this PropertyInfo property)
		{
			return ((property.CanWrite && property.GetSetMethod(true).IsPublic && property.CanRead && property.GetGetMethod(true).IsPublic) || property.IsDefined(typeof(SerializeField), false)) && !property.IsDefined(typeof(SerializeIgnore), false);
		}

		public static Type[] GetAllFromCurrentAssembly()
		{
			Type[] types = typeof(Reflection).Assembly.GetTypes();
			return types.ToArray();
		}

		public static Type[] GetAssignableFromTypes(Type type)
		{
			IEnumerable<Type> source = from p in AppDomain.CurrentDomain.GetAssemblies().SelectMany((Assembly s) => s.GetTypes())
				where type.IsAssignableFrom(p) && p.IsClass
				select p;
			return source.ToArray();
		}

		public static Type BaseType(this Type type)
		{
			return type.BaseType;
		}

		public static bool IsValueType(this Type type)
		{
			return type.IsValueType;
		}

		public static bool IsPrimitive(this Type type)
		{
			return type.IsPrimitive;
		}

		public static bool IsArray(this Type type)
		{
			return type.IsArray;
		}

		public static bool IsGenericType(this Type type)
		{
			return type.IsGenericType;
		}

		public static bool IsEnum(this Type type)
		{
			return type.IsEnum;
		}

		public static bool IsClass(this Type type)
		{
			return type.IsClass;
		}
	}
}
