using System.Collections.Generic;

namespace IKVM.Reflection
{
	public abstract class TypeInfo : Type, IReflectableType
	{
		private const BindingFlags Flags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		public IEnumerable<ConstructorInfo> DeclaredConstructors => GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

		public IEnumerable<EventInfo> DeclaredEvents => GetEvents(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

		public IEnumerable<FieldInfo> DeclaredFields => GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

		public IEnumerable<MemberInfo> DeclaredMembers => GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

		public IEnumerable<MethodInfo> DeclaredMethods => GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

		public IEnumerable<TypeInfo> DeclaredNestedTypes
		{
			get
			{
				Type[] nestedTypes = GetNestedTypes(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				TypeInfo[] array = new TypeInfo[nestedTypes.Length];
				for (int i = 0; i < nestedTypes.Length; i++)
				{
					array[i] = nestedTypes[i].GetTypeInfo();
				}
				return array;
			}
		}

		public IEnumerable<PropertyInfo> DeclaredProperties => GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

		public Type[] GenericTypeParameters
		{
			get
			{
				if (!IsGenericTypeDefinition)
				{
					return Type.EmptyTypes;
				}
				return GetGenericArguments();
			}
		}

		public IEnumerable<Type> ImplementedInterfaces => __GetDeclaredInterfaces();

		internal TypeInfo()
		{
		}

		internal TypeInfo(Type underlyingType)
			: base(underlyingType)
		{
		}

		internal TypeInfo(byte sigElementType)
			: base(sigElementType)
		{
		}

		public Type AsType()
		{
			return this;
		}

		public EventInfo GetDeclaredEvent(string name)
		{
			return GetEvent(name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public FieldInfo GetDeclaredField(string name)
		{
			return GetField(name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public MethodInfo GetDeclaredMethod(string name)
		{
			return GetMethod(name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public IEnumerable<MethodInfo> GetDeclaredMethods(string name)
		{
			List<MethodInfo> list = new List<MethodInfo>();
			MethodInfo[] methods = GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods)
			{
				if (methodInfo.Name == name)
				{
					list.Add(methodInfo);
				}
			}
			return list;
		}

		public TypeInfo GetDeclaredNestedType(string name)
		{
			return GetNestedType(name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetTypeInfo();
		}

		public PropertyInfo GetDeclaredProperty(string name)
		{
			return GetProperty(name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public bool IsAssignableFrom(TypeInfo typeInfo)
		{
			return IsAssignableFrom((Type)typeInfo);
		}
	}
}
