using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace IKVM.Reflection
{
	public abstract class Type : MemberInfo, IGenericContext, IGenericBinder
	{
		[Flags]
		protected enum TypeFlags : ushort
		{
			IsGenericTypeDefinition = 1,
			HasNestedTypes = 2,
			Baked = 4,
			ValueType = 8,
			NotValueType = 0x10,
			PotentialEnumOrValueType = 0x20,
			EnumOrValueType = 0x40,
			NotGenericTypeDefinition = 0x80,
			ContainsMissingType_Unknown = 0,
			ContainsMissingType_Pending = 0x100,
			ContainsMissingType_Yes = 0x200,
			ContainsMissingType_No = ContainsMissingType_Pending | ContainsMissingType_Yes,
			ContainsMissingType_Mask = ContainsMissingType_No,
			PotentialBuiltIn = 0x400,
			BuiltIn = 0x800
		}

		public static readonly Type[] EmptyTypes = Empty<Type>.Array;

		protected readonly Type underlyingType;

		protected TypeFlags typeFlags;

		private byte sigElementType;

		public static Binder DefaultBinder => new DefaultBinder();

		public sealed override MemberTypes MemberType
		{
			get
			{
				if (!IsNested)
				{
					return MemberTypes.TypeInfo;
				}
				return MemberTypes.NestedType;
			}
		}

		public virtual string AssemblyQualifiedName => FullName + ", " + Assembly.FullName;

		public abstract Type BaseType { get; }

		public abstract TypeAttributes Attributes { get; }

		public virtual __StandAloneMethodSig __MethodSignature
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		public bool HasElementType
		{
			get
			{
				if (!IsArray && !IsByRef)
				{
					return IsPointer;
				}
				return true;
			}
		}

		public bool IsArray
		{
			get
			{
				if (sigElementType != 20)
				{
					return sigElementType == 29;
				}
				return true;
			}
		}

		public bool __IsVector => sigElementType == 29;

		public bool IsByRef => sigElementType == 16;

		public bool IsPointer => sigElementType == 15;

		public bool __IsFunctionPointer => sigElementType == 27;

		public virtual bool IsValueType
		{
			get
			{
				Type baseType = BaseType;
				if (baseType != null && baseType.IsEnumOrValueType)
				{
					return !IsEnumOrValueType;
				}
				return false;
			}
		}

		public bool IsGenericParameter
		{
			get
			{
				if (sigElementType != 19)
				{
					return sigElementType == 30;
				}
				return true;
			}
		}

		public virtual int GenericParameterPosition
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public virtual MethodBase DeclaringMethod => null;

		public Type UnderlyingSystemType => underlyingType;

		public override Type DeclaringType => null;

		internal virtual TypeName TypeName
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		public string __Name => TypeName.Name;

		public string __Namespace => TypeName.Namespace;

		public abstract override string Name { get; }

		public virtual string Namespace
		{
			get
			{
				if (IsNested)
				{
					return DeclaringType.Namespace;
				}
				return __Namespace;
			}
		}

		public Type[] GenericTypeArguments
		{
			get
			{
				if (!IsConstructedGenericType)
				{
					return EmptyTypes;
				}
				return GetGenericArguments();
			}
		}

		public StructLayoutAttribute StructLayoutAttribute
		{
			get
			{
				StructLayoutAttribute structLayoutAttribute;
				switch (Attributes & TypeAttributes.LayoutMask)
				{
				case TypeAttributes.AnsiClass:
					structLayoutAttribute = new StructLayoutAttribute(LayoutKind.Auto);
					break;
				case TypeAttributes.SequentialLayout:
					structLayoutAttribute = new StructLayoutAttribute(LayoutKind.Sequential);
					break;
				case TypeAttributes.ExplicitLayout:
					structLayoutAttribute = new StructLayoutAttribute(LayoutKind.Explicit);
					break;
				default:
					throw new BadImageFormatException();
				}
				switch (Attributes & TypeAttributes.CustomFormatClass)
				{
				case TypeAttributes.AnsiClass:
					structLayoutAttribute.CharSet = CharSet.Ansi;
					break;
				case TypeAttributes.UnicodeClass:
					structLayoutAttribute.CharSet = CharSet.Unicode;
					break;
				case TypeAttributes.AutoClass:
					structLayoutAttribute.CharSet = CharSet.Auto;
					break;
				default:
					structLayoutAttribute.CharSet = CharSet.None;
					break;
				}
				if (!__GetLayout(out structLayoutAttribute.Pack, out structLayoutAttribute.Size))
				{
					structLayoutAttribute.Pack = 8;
				}
				return structLayoutAttribute;
			}
		}

		public virtual bool IsGenericType => false;

		public virtual bool IsGenericTypeDefinition => false;

		public virtual bool IsConstructedGenericType => false;

		public virtual bool ContainsGenericParameters
		{
			get
			{
				if (IsGenericParameter)
				{
					return true;
				}
				Type[] genericArguments = GetGenericArguments();
				for (int i = 0; i < genericArguments.Length; i++)
				{
					if (genericArguments[i].ContainsGenericParameters)
					{
						return true;
					}
				}
				return false;
			}
		}

		public virtual GenericParameterAttributes GenericParameterAttributes
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		public abstract string FullName { get; }

		internal virtual bool IsModulePseudoType => false;

		public ConstructorInfo TypeInitializer => GetConstructor(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, EmptyTypes, null);

		public bool IsPrimitive
		{
			get
			{
				if (__IsBuiltIn)
				{
					if ((sigElementType < 2 || sigElementType > 13) && sigElementType != 24)
					{
						return sigElementType == 25;
					}
					return true;
				}
				return false;
			}
		}

		public bool __IsBuiltIn
		{
			get
			{
				if ((typeFlags & (TypeFlags.PotentialBuiltIn | TypeFlags.BuiltIn)) != TypeFlags.ContainsMissingType_Unknown)
				{
					if ((typeFlags & TypeFlags.BuiltIn) == 0)
					{
						return ResolvePotentialBuiltInType();
					}
					return true;
				}
				return false;
			}
		}

		internal byte SigElementType => sigElementType;

		public bool IsEnum
		{
			get
			{
				Type baseType = BaseType;
				if (baseType != null && baseType.IsEnumOrValueType)
				{
					return baseType.__Name[0] == 'E';
				}
				return false;
			}
		}

		public bool IsSealed => (Attributes & TypeAttributes.Sealed) != 0;

		public bool IsAbstract => (Attributes & TypeAttributes.Abstract) != 0;

		public bool IsPublic => CheckVisibility(TypeAttributes.Public);

		public bool IsNestedPublic => CheckVisibility(TypeAttributes.NestedPublic);

		public bool IsNestedPrivate => CheckVisibility(TypeAttributes.NestedPrivate);

		public bool IsNestedFamily => CheckVisibility(TypeAttributes.NestedFamily);

		public bool IsNestedAssembly => CheckVisibility(TypeAttributes.NestedAssembly);

		public bool IsNestedFamANDAssem => CheckVisibility(TypeAttributes.NestedFamANDAssem);

		public bool IsNestedFamORAssem => CheckVisibility(TypeAttributes.VisibilityMask);

		public bool IsNotPublic => CheckVisibility(TypeAttributes.AnsiClass);

		public bool IsImport => (Attributes & TypeAttributes.Import) != 0;

		public bool IsCOMObject
		{
			get
			{
				if (IsClass)
				{
					return IsImport;
				}
				return false;
			}
		}

		public bool IsContextful => IsSubclassOf(Module.universe.Import(typeof(ContextBoundObject)));

		public bool IsMarshalByRef => IsSubclassOf(Module.universe.Import(typeof(MarshalByRefObject)));

		public virtual bool IsVisible
		{
			get
			{
				if (!IsPublic)
				{
					if (IsNestedPublic)
					{
						return DeclaringType.IsVisible;
					}
					return false;
				}
				return true;
			}
		}

		public bool IsAnsiClass => (Attributes & TypeAttributes.CustomFormatClass) == 0;

		public bool IsUnicodeClass => (Attributes & TypeAttributes.CustomFormatClass) == TypeAttributes.UnicodeClass;

		public bool IsAutoClass => (Attributes & TypeAttributes.CustomFormatClass) == TypeAttributes.AutoClass;

		public bool IsAutoLayout => (Attributes & TypeAttributes.LayoutMask) == 0;

		public bool IsLayoutSequential => (Attributes & TypeAttributes.LayoutMask) == TypeAttributes.SequentialLayout;

		public bool IsExplicitLayout => (Attributes & TypeAttributes.LayoutMask) == TypeAttributes.ExplicitLayout;

		public bool IsSpecialName => (Attributes & TypeAttributes.SpecialName) != 0;

		public bool IsSerializable => (Attributes & TypeAttributes.Serializable) != 0;

		public bool IsClass
		{
			get
			{
				if (!IsInterface)
				{
					return !IsValueType;
				}
				return false;
			}
		}

		public bool IsInterface => (Attributes & TypeAttributes.ClassSemanticsMask) != 0;

		public bool IsNested => DeclaringType != null;

		public bool __ContainsMissingType
		{
			get
			{
				if ((typeFlags & TypeFlags.ContainsMissingType_No) == 0)
				{
					typeFlags |= TypeFlags.ContainsMissingType_Pending;
					typeFlags = (TypeFlags)((uint)(typeFlags & ~TypeFlags.ContainsMissingType_No) | (uint)(ContainsMissingTypeImpl ? 512 : 768));
				}
				return (typeFlags & TypeFlags.ContainsMissingType_No) == TypeFlags.ContainsMissingType_Yes;
			}
		}

		protected virtual bool ContainsMissingTypeImpl
		{
			get
			{
				if (!__IsMissing && !ContainsMissingType(GetGenericArguments()))
				{
					return __GetCustomModifiers().ContainsMissingType;
				}
				return true;
			}
		}

		public Assembly Assembly => Module.Assembly;

		internal bool IsAllowMultipleCustomAttribute
		{
			get
			{
				IList<CustomAttributeData> list = CustomAttributeData.__GetCustomAttributes(this, Module.universe.System_AttributeUsageAttribute, false);
				if (list.Count == 1)
				{
					foreach (CustomAttributeNamedArgument namedArgument in list[0].NamedArguments)
					{
						if (namedArgument.MemberInfo.Name == "AllowMultiple")
						{
							return (bool)namedArgument.TypedValue.Value;
						}
					}
				}
				return false;
			}
		}

		private bool IsEnumOrValueType
		{
			get
			{
				if ((typeFlags & (TypeFlags.PotentialEnumOrValueType | TypeFlags.EnumOrValueType)) != TypeFlags.ContainsMissingType_Unknown)
				{
					if ((typeFlags & TypeFlags.EnumOrValueType) == 0)
					{
						return ResolvePotentialEnumOrValueType();
					}
					return true;
				}
				return false;
			}
		}

		internal virtual Universe Universe => Module.universe;

		public virtual bool __IsTypeForwarder => false;

		public virtual bool __IsCyclicTypeForwarder => false;

		internal Type()
		{
			underlyingType = this;
		}

		internal Type(Type underlyingType)
		{
			this.underlyingType = underlyingType;
			typeFlags = underlyingType.typeFlags;
		}

		internal Type(byte sigElementType)
			: this()
		{
			this.sigElementType = sigElementType;
		}

		public virtual Type GetElementType()
		{
			return null;
		}

		internal virtual void CheckBaked()
		{
		}

		public virtual Type[] __GetDeclaredTypes()
		{
			return EmptyTypes;
		}

		public virtual Type[] __GetDeclaredInterfaces()
		{
			return EmptyTypes;
		}

		public virtual MethodBase[] __GetDeclaredMethods()
		{
			return Empty<MethodBase>.Array;
		}

		public virtual __MethodImplMap __GetMethodImplMap()
		{
			throw new NotSupportedException();
		}

		public virtual FieldInfo[] __GetDeclaredFields()
		{
			return Empty<FieldInfo>.Array;
		}

		public virtual EventInfo[] __GetDeclaredEvents()
		{
			return Empty<EventInfo>.Array;
		}

		public virtual PropertyInfo[] __GetDeclaredProperties()
		{
			return Empty<PropertyInfo>.Array;
		}

		public virtual CustomModifiers __GetCustomModifiers()
		{
			return default(CustomModifiers);
		}

		[Obsolete("Please use __GetCustomModifiers() instead.")]
		public Type[] __GetRequiredCustomModifiers()
		{
			return __GetCustomModifiers().GetRequired();
		}

		[Obsolete("Please use __GetCustomModifiers() instead.")]
		public Type[] __GetOptionalCustomModifiers()
		{
			return __GetCustomModifiers().GetOptional();
		}

		internal virtual int GetModuleBuilderToken()
		{
			throw new InvalidOperationException();
		}

		public static bool operator ==(Type t1, Type t2)
		{
			if ((object)t1 != t2)
			{
				if ((object)t1 != null && (object)t2 != null)
				{
					return (object)t1.underlyingType == t2.underlyingType;
				}
				return false;
			}
			return true;
		}

		public static bool operator !=(Type t1, Type t2)
		{
			return !(t1 == t2);
		}

		public bool Equals(Type type)
		{
			return this == type;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Type);
		}

		public override int GetHashCode()
		{
			Type underlyingSystemType = UnderlyingSystemType;
			if ((object)underlyingSystemType != this)
			{
				return underlyingSystemType.GetHashCode();
			}
			return base.GetHashCode();
		}

		public virtual Type[] GetGenericArguments()
		{
			return EmptyTypes;
		}

		public virtual CustomModifiers[] __GetGenericArgumentsCustomModifiers()
		{
			return Empty<CustomModifiers>.Array;
		}

		[Obsolete("Please use __GetGenericArgumentsCustomModifiers() instead")]
		public Type[][] __GetGenericArgumentsRequiredCustomModifiers()
		{
			CustomModifiers[] array = __GetGenericArgumentsCustomModifiers();
			Type[][] array2 = new Type[array.Length][];
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = array[i].GetRequired();
			}
			return array2;
		}

		[Obsolete("Please use __GetGenericArgumentsCustomModifiers() instead")]
		public Type[][] __GetGenericArgumentsOptionalCustomModifiers()
		{
			CustomModifiers[] array = __GetGenericArgumentsCustomModifiers();
			Type[][] array2 = new Type[array.Length][];
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = array[i].GetOptional();
			}
			return array2;
		}

		public virtual Type GetGenericTypeDefinition()
		{
			throw new InvalidOperationException();
		}

		public virtual bool __GetLayout(out int packingSize, out int typeSize)
		{
			packingSize = 0;
			typeSize = 0;
			return false;
		}

		public virtual Type[] GetGenericParameterConstraints()
		{
			throw new InvalidOperationException();
		}

		public virtual CustomModifiers[] __GetGenericParameterConstraintCustomModifiers()
		{
			throw new InvalidOperationException();
		}

		public virtual int GetArrayRank()
		{
			throw new NotSupportedException();
		}

		public virtual int[] __GetArraySizes()
		{
			throw new NotSupportedException();
		}

		public virtual int[] __GetArrayLowerBounds()
		{
			throw new NotSupportedException();
		}

		public virtual Type GetEnumUnderlyingType()
		{
			if (!IsEnum)
			{
				throw new ArgumentException();
			}
			CheckBaked();
			return GetEnumUnderlyingTypeImpl();
		}

		internal Type GetEnumUnderlyingTypeImpl()
		{
			FieldInfo[] array = __GetDeclaredFields();
			foreach (FieldInfo fieldInfo in array)
			{
				if (!fieldInfo.IsStatic)
				{
					return fieldInfo.FieldType;
				}
			}
			throw new InvalidOperationException();
		}

		public string[] GetEnumNames()
		{
			if (!IsEnum)
			{
				throw new ArgumentException();
			}
			List<string> list = new List<string>();
			FieldInfo[] array = __GetDeclaredFields();
			foreach (FieldInfo fieldInfo in array)
			{
				if (fieldInfo.IsLiteral)
				{
					list.Add(fieldInfo.Name);
				}
			}
			return list.ToArray();
		}

		public string GetEnumName(object value)
		{
			if (!IsEnum)
			{
				throw new ArgumentException();
			}
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			try
			{
				value = Convert.ChangeType(value, GetTypeCode(GetEnumUnderlyingType()));
			}
			catch (FormatException)
			{
				throw new ArgumentException();
			}
			catch (OverflowException)
			{
				return null;
			}
			catch (InvalidCastException)
			{
				return null;
			}
			FieldInfo[] array = __GetDeclaredFields();
			foreach (FieldInfo fieldInfo in array)
			{
				if (fieldInfo.IsLiteral && fieldInfo.GetRawConstantValue().Equals(value))
				{
					return fieldInfo.Name;
				}
			}
			return null;
		}

		public bool IsEnumDefined(object value)
		{
			if (value is string)
			{
				return Array.IndexOf(GetEnumNames(), value) != -1;
			}
			if (!IsEnum)
			{
				throw new ArgumentException();
			}
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			if (System.Type.GetTypeCode(value.GetType()) != GetTypeCode(GetEnumUnderlyingType()))
			{
				throw new ArgumentException();
			}
			FieldInfo[] array = __GetDeclaredFields();
			foreach (FieldInfo fieldInfo in array)
			{
				if (fieldInfo.IsLiteral && fieldInfo.GetRawConstantValue().Equals(value))
				{
					return true;
				}
			}
			return false;
		}

		public override string ToString()
		{
			return FullName;
		}

		protected string GetFullName()
		{
			string text = TypeNameParser.Escape(__Namespace);
			Type declaringType = DeclaringType;
			if (declaringType == null)
			{
				if (text == null)
				{
					return Name;
				}
				return text + "." + Name;
			}
			if (text == null)
			{
				return declaringType.FullName + "+" + Name;
			}
			return declaringType.FullName + "+" + text + "." + Name;
		}

		internal virtual Type GetGenericTypeArgument(int index)
		{
			throw new InvalidOperationException();
		}

		public MemberInfo[] GetDefaultMembers()
		{
			Type type = Module.universe.Import(typeof(DefaultMemberAttribute));
			foreach (CustomAttributeData customAttribute in CustomAttributeData.GetCustomAttributes(this))
			{
				if (customAttribute.Constructor.DeclaringType.Equals(type))
				{
					return GetMember((string)customAttribute.ConstructorArguments[0].Value);
				}
			}
			return Empty<MemberInfo>.Array;
		}

		public MemberInfo[] GetMember(string name)
		{
			return GetMember(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
		{
			return GetMember(name, MemberTypes.All, bindingAttr);
		}

		public MemberInfo[] GetMembers()
		{
			return GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public MemberInfo[] GetMembers(BindingFlags bindingAttr)
		{
			List<MemberInfo> list = new List<MemberInfo>();
			list.AddRange(GetConstructors(bindingAttr));
			list.AddRange(GetMethods(bindingAttr));
			list.AddRange(GetFields(bindingAttr));
			list.AddRange(GetProperties(bindingAttr));
			list.AddRange(GetEvents(bindingAttr));
			list.AddRange(GetNestedTypes(bindingAttr));
			return list.ToArray();
		}

		public MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
		{
			MemberFilter filter;
			if ((bindingAttr & BindingFlags.IgnoreCase) != BindingFlags.Default)
			{
				name = name.ToLowerInvariant();
				filter = (MemberInfo member, object filterCriteria) => member.Name.ToLowerInvariant().Equals(filterCriteria);
			}
			else
			{
				filter = (MemberInfo member, object filterCriteria) => member.Name.Equals(filterCriteria);
			}
			return FindMembers(type, bindingAttr, filter, name);
		}

		private static void AddMembers(List<MemberInfo> list, MemberFilter filter, object filterCriteria, MemberInfo[] members)
		{
			foreach (MemberInfo memberInfo in members)
			{
				if (filter == null || filter(memberInfo, filterCriteria))
				{
					list.Add(memberInfo);
				}
			}
		}

		public MemberInfo[] FindMembers(MemberTypes memberType, BindingFlags bindingAttr, MemberFilter filter, object filterCriteria)
		{
			List<MemberInfo> list = new List<MemberInfo>();
			if ((memberType & MemberTypes.Constructor) != 0)
			{
				AddMembers(list, filter, filterCriteria, GetConstructors(bindingAttr));
			}
			if ((memberType & MemberTypes.Method) != 0)
			{
				AddMembers(list, filter, filterCriteria, GetMethods(bindingAttr));
			}
			if ((memberType & MemberTypes.Field) != 0)
			{
				AddMembers(list, filter, filterCriteria, GetFields(bindingAttr));
			}
			if ((memberType & MemberTypes.Property) != 0)
			{
				AddMembers(list, filter, filterCriteria, GetProperties(bindingAttr));
			}
			if ((memberType & MemberTypes.Event) != 0)
			{
				AddMembers(list, filter, filterCriteria, GetEvents(bindingAttr));
			}
			if ((memberType & MemberTypes.NestedType) != 0)
			{
				AddMembers(list, filter, filterCriteria, GetNestedTypes(bindingAttr));
			}
			return list.ToArray();
		}

		private MemberInfo[] GetMembers<T>()
		{
			if (typeof(T) == typeof(ConstructorInfo) || typeof(T) == typeof(MethodInfo))
			{
				return __GetDeclaredMethods();
			}
			if (typeof(T) == typeof(FieldInfo))
			{
				return __GetDeclaredFields();
			}
			if (typeof(T) == typeof(PropertyInfo))
			{
				return __GetDeclaredProperties();
			}
			if (typeof(T) == typeof(EventInfo))
			{
				return __GetDeclaredEvents();
			}
			if (typeof(T) == typeof(Type))
			{
				return __GetDeclaredTypes();
			}
			throw new InvalidOperationException();
		}

		private T[] GetMembers<T>(BindingFlags flags) where T : MemberInfo
		{
			CheckBaked();
			List<T> list = new List<T>();
			MemberInfo[] members = GetMembers<T>();
			foreach (MemberInfo memberInfo in members)
			{
				if (memberInfo is T && memberInfo.BindingFlagsMatch(flags))
				{
					list.Add((T)memberInfo);
				}
			}
			if ((flags & BindingFlags.DeclaredOnly) == 0)
			{
				Type baseType = BaseType;
				while (baseType != null)
				{
					baseType.CheckBaked();
					members = baseType.GetMembers<T>();
					foreach (MemberInfo memberInfo2 in members)
					{
						if (memberInfo2 is T && memberInfo2.BindingFlagsMatchInherited(flags))
						{
							list.Add((T)memberInfo2.SetReflectedType(this));
						}
					}
					baseType = baseType.BaseType;
				}
			}
			return list.ToArray();
		}

		private T GetMemberByName<T>(string name, BindingFlags flags, Predicate<T> filter) where T : MemberInfo
		{
			CheckBaked();
			if ((flags & BindingFlags.IgnoreCase) != BindingFlags.Default)
			{
				name = name.ToLowerInvariant();
			}
			T val = null;
			MemberInfo[] members = GetMembers<T>();
			foreach (MemberInfo memberInfo in members)
			{
				if (!(memberInfo is T) || !memberInfo.BindingFlagsMatch(flags))
				{
					continue;
				}
				string text = memberInfo.Name;
				if ((flags & BindingFlags.IgnoreCase) != BindingFlags.Default)
				{
					text = text.ToLowerInvariant();
				}
				if (text == name && (filter == null || filter((T)memberInfo)))
				{
					if (val != null)
					{
						throw new AmbiguousMatchException();
					}
					val = (T)memberInfo;
				}
			}
			if ((flags & BindingFlags.DeclaredOnly) == 0)
			{
				Type baseType = BaseType;
				while ((val == null || typeof(T) == typeof(MethodInfo)) && baseType != null)
				{
					baseType.CheckBaked();
					members = baseType.GetMembers<T>();
					foreach (MemberInfo memberInfo2 in members)
					{
						if (!(memberInfo2 is T) || !memberInfo2.BindingFlagsMatchInherited(flags))
						{
							continue;
						}
						string text2 = memberInfo2.Name;
						if ((flags & BindingFlags.IgnoreCase) != BindingFlags.Default)
						{
							text2 = text2.ToLowerInvariant();
						}
						if (!(text2 == name) || (filter != null && !filter((T)memberInfo2)))
						{
							continue;
						}
						if (val != null)
						{
							MethodInfo methodInfo;
							if (!((methodInfo = val as MethodInfo) != null) || !methodInfo.MethodSignature.MatchParameterTypes(((MethodBase)memberInfo2).MethodSignature))
							{
								throw new AmbiguousMatchException();
							}
						}
						else
						{
							val = (T)memberInfo2.SetReflectedType(this);
						}
					}
					baseType = baseType.BaseType;
				}
			}
			return val;
		}

		private T GetMemberByName<T>(string name, BindingFlags flags) where T : MemberInfo
		{
			return GetMemberByName<T>(name, flags, null);
		}

		public EventInfo GetEvent(string name)
		{
			return GetEvent(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public EventInfo GetEvent(string name, BindingFlags bindingAttr)
		{
			return GetMemberByName<EventInfo>(name, bindingAttr);
		}

		public EventInfo[] GetEvents()
		{
			return GetEvents(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public EventInfo[] GetEvents(BindingFlags bindingAttr)
		{
			return GetMembers<EventInfo>(bindingAttr);
		}

		public FieldInfo GetField(string name)
		{
			return GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public FieldInfo GetField(string name, BindingFlags bindingAttr)
		{
			return GetMemberByName<FieldInfo>(name, bindingAttr);
		}

		public FieldInfo[] GetFields()
		{
			return GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public FieldInfo[] GetFields(BindingFlags bindingAttr)
		{
			return GetMembers<FieldInfo>(bindingAttr);
		}

		public Type[] GetInterfaces()
		{
			List<Type> list = new List<Type>();
			Type type = this;
			while (type != null)
			{
				AddInterfaces(list, type);
				type = type.BaseType;
			}
			return list.ToArray();
		}

		private static void AddInterfaces(List<Type> list, Type type)
		{
			Type[] array = type.__GetDeclaredInterfaces();
			foreach (Type type2 in array)
			{
				if (!list.Contains(type2))
				{
					list.Add(type2);
					AddInterfaces(list, type2);
				}
			}
		}

		public MethodInfo[] GetMethods(BindingFlags bindingAttr)
		{
			CheckBaked();
			List<MethodInfo> list = new List<MethodInfo>();
			MethodBase[] array = __GetDeclaredMethods();
			for (int i = 0; i < array.Length; i++)
			{
				MethodInfo methodInfo = array[i] as MethodInfo;
				if (methodInfo != null && methodInfo.BindingFlagsMatch(bindingAttr))
				{
					list.Add(methodInfo);
				}
			}
			if ((bindingAttr & BindingFlags.DeclaredOnly) == 0)
			{
				List<MethodInfo> list2 = new List<MethodInfo>();
				foreach (MethodInfo item in list)
				{
					if (item.IsVirtual)
					{
						list2.Add(item.GetBaseDefinition());
					}
				}
				Type baseType = BaseType;
				while (baseType != null)
				{
					baseType.CheckBaked();
					array = baseType.__GetDeclaredMethods();
					for (int i = 0; i < array.Length; i++)
					{
						MethodInfo methodInfo2 = array[i] as MethodInfo;
						if (!(methodInfo2 != null) || !methodInfo2.BindingFlagsMatchInherited(bindingAttr))
						{
							continue;
						}
						if (methodInfo2.IsVirtual)
						{
							if (list2 == null)
							{
								list2 = new List<MethodInfo>();
							}
							else if (list2.Contains(methodInfo2.GetBaseDefinition()))
							{
								continue;
							}
							list2.Add(methodInfo2.GetBaseDefinition());
						}
						list.Add((MethodInfo)methodInfo2.SetReflectedType(this));
					}
					baseType = baseType.BaseType;
				}
			}
			return list.ToArray();
		}

		public MethodInfo[] GetMethods()
		{
			return GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public MethodInfo GetMethod(string name)
		{
			return GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public MethodInfo GetMethod(string name, BindingFlags bindingAttr)
		{
			return GetMemberByName<MethodInfo>(name, bindingAttr);
		}

		public MethodInfo GetMethod(string name, Type[] types)
		{
			return GetMethod(name, types, null);
		}

		public MethodInfo GetMethod(string name, Type[] types, ParameterModifier[] modifiers)
		{
			return GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, types, modifiers);
		}

		public MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
		{
			return GetMemberByName(name, bindingAttr, (MethodInfo method) => method.MethodSignature.MatchParameterTypes(types)) ?? GetMethodWithBinder<MethodInfo>(name, bindingAttr, binder ?? DefaultBinder, types, modifiers);
		}

		private T GetMethodWithBinder<T>(string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers) where T : MethodBase
		{
			List<MethodBase> list = new List<MethodBase>();
			GetMemberByName(name, bindingAttr, (T method) =>
			{
				list.Add(method);
				return false;
			});
			return (T)binder.SelectMethod(bindingAttr, list.ToArray(), types, modifiers);
		}

		public MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			return GetMethod(name, bindingAttr, binder, types, modifiers);
		}

		public ConstructorInfo[] GetConstructors()
		{
			return GetConstructors(BindingFlags.Instance | BindingFlags.Public);
		}

		public ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
		{
			return GetMembers<ConstructorInfo>(bindingAttr | BindingFlags.DeclaredOnly);
		}

		public ConstructorInfo GetConstructor(Type[] types)
		{
			return GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.Standard, types, null);
		}

		public ConstructorInfo GetConstructor(BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
		{
			ConstructorInfo constructorInfo = null;
			if ((bindingAttr & BindingFlags.Instance) != BindingFlags.Default)
			{
				constructorInfo = GetConstructorImpl(ConstructorInfo.ConstructorName, bindingAttr, binder, types, modifiers);
			}
			if ((bindingAttr & BindingFlags.Static) != BindingFlags.Default)
			{
				ConstructorInfo constructorImpl = GetConstructorImpl(ConstructorInfo.TypeConstructorName, bindingAttr, binder, types, modifiers);
				if (constructorImpl != null)
				{
					if (constructorInfo != null)
					{
						throw new AmbiguousMatchException();
					}
					return constructorImpl;
				}
			}
			return constructorInfo;
		}

		private ConstructorInfo GetConstructorImpl(string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
		{
			return GetMemberByName(name, bindingAttr | BindingFlags.DeclaredOnly, (ConstructorInfo ctor) => ctor.MethodSignature.MatchParameterTypes(types)) ?? GetMethodWithBinder<ConstructorInfo>(name, bindingAttr, binder ?? DefaultBinder, types, modifiers);
		}

		public ConstructorInfo GetConstructor(BindingFlags bindingAttr, Binder binder, CallingConventions callingConvention, Type[] types, ParameterModifier[] modifiers)
		{
			return GetConstructor(bindingAttr, binder, types, modifiers);
		}

		internal Type ResolveNestedType(Module requester, TypeName typeName)
		{
			return FindNestedType(typeName) ?? Module.universe.GetMissingTypeOrThrow(requester, Module, this, typeName);
		}

		internal virtual Type FindNestedType(TypeName name)
		{
			Type[] array = __GetDeclaredTypes();
			foreach (Type type in array)
			{
				if (type.TypeName == name)
				{
					return type;
				}
			}
			return null;
		}

		internal virtual Type FindNestedTypeIgnoreCase(TypeName lowerCaseName)
		{
			Type[] array = __GetDeclaredTypes();
			foreach (Type type in array)
			{
				if (type.TypeName.ToLowerInvariant() == lowerCaseName)
				{
					return type;
				}
			}
			return null;
		}

		public Type GetNestedType(string name)
		{
			return GetNestedType(name, BindingFlags.Public);
		}

		public Type GetNestedType(string name, BindingFlags bindingAttr)
		{
			return GetMemberByName<Type>(name, bindingAttr | BindingFlags.DeclaredOnly);
		}

		public Type[] GetNestedTypes()
		{
			return GetNestedTypes(BindingFlags.Public);
		}

		public Type[] GetNestedTypes(BindingFlags bindingAttr)
		{
			return GetMembers<Type>(bindingAttr | BindingFlags.DeclaredOnly);
		}

		public PropertyInfo[] GetProperties()
		{
			return GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public PropertyInfo[] GetProperties(BindingFlags bindingAttr)
		{
			return GetMembers<PropertyInfo>(bindingAttr);
		}

		public PropertyInfo GetProperty(string name)
		{
			return GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public PropertyInfo GetProperty(string name, BindingFlags bindingAttr)
		{
			return GetMemberByName<PropertyInfo>(name, bindingAttr);
		}

		public PropertyInfo GetProperty(string name, Type returnType)
		{
			return GetMemberByName(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, (PropertyInfo prop) => prop.PropertyType.Equals(returnType)) ?? GetPropertyWithBinder(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, DefaultBinder, returnType, null, null);
		}

		public PropertyInfo GetProperty(string name, Type[] types)
		{
			return GetMemberByName(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, (PropertyInfo prop) => prop.PropertySignature.MatchParameterTypes(types)) ?? GetPropertyWithBinder(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, DefaultBinder, null, types, null);
		}

		public PropertyInfo GetProperty(string name, Type returnType, Type[] types)
		{
			return GetProperty(name, returnType, types, null);
		}

		public PropertyInfo GetProperty(string name, Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			return GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, returnType, types, modifiers);
		}

		public PropertyInfo GetProperty(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			return GetMemberByName(name, bindingAttr, (PropertyInfo prop) => prop.PropertyType.Equals(returnType) && prop.PropertySignature.MatchParameterTypes(types)) ?? GetPropertyWithBinder(name, bindingAttr, binder ?? DefaultBinder, returnType, types, modifiers);
		}

		private PropertyInfo GetPropertyWithBinder(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			List<PropertyInfo> list = new List<PropertyInfo>();
			GetMemberByName(name, bindingAttr, (PropertyInfo property) =>
			{
				list.Add(property);
				return false;
			});
			return binder.SelectProperty(bindingAttr, list.ToArray(), returnType, types, modifiers);
		}

		public Type GetInterface(string name)
		{
			return GetInterface(name, false);
		}

		public Type GetInterface(string name, bool ignoreCase)
		{
			if (ignoreCase)
			{
				name = name.ToLowerInvariant();
			}
			Type type = null;
			Type[] interfaces = GetInterfaces();
			foreach (Type type2 in interfaces)
			{
				string text = type2.FullName;
				if (ignoreCase)
				{
					text = text.ToLowerInvariant();
				}
				if (text == name)
				{
					if (type != null)
					{
						throw new AmbiguousMatchException();
					}
					type = type2;
				}
			}
			return type;
		}

		public Type[] FindInterfaces(TypeFilter filter, object filterCriteria)
		{
			List<Type> list = new List<Type>();
			Type[] interfaces = GetInterfaces();
			foreach (Type type in interfaces)
			{
				if (filter(type, filterCriteria))
				{
					list.Add(type);
				}
			}
			return list.ToArray();
		}

		private bool ResolvePotentialBuiltInType()
		{
			typeFlags &= ~TypeFlags.PotentialBuiltIn;
			Universe universe = Universe;
			switch (__Name)
			{
			case "Boolean":
				return ResolvePotentialBuiltInType(universe.System_Boolean, 2);
			case "Char":
				return ResolvePotentialBuiltInType(universe.System_Char, 3);
			case "Object":
				return ResolvePotentialBuiltInType(universe.System_Object, 28);
			case "String":
				return ResolvePotentialBuiltInType(universe.System_String, 14);
			case "Single":
				return ResolvePotentialBuiltInType(universe.System_Single, 12);
			case "Double":
				return ResolvePotentialBuiltInType(universe.System_Double, 13);
			case "SByte":
				return ResolvePotentialBuiltInType(universe.System_SByte, 4);
			case "Int16":
				return ResolvePotentialBuiltInType(universe.System_Int16, 6);
			case "Int32":
				return ResolvePotentialBuiltInType(universe.System_Int32, 8);
			case "Int64":
				return ResolvePotentialBuiltInType(universe.System_Int64, 10);
			case "IntPtr":
				return ResolvePotentialBuiltInType(universe.System_IntPtr, 24);
			case "UIntPtr":
				return ResolvePotentialBuiltInType(universe.System_UIntPtr, 25);
			case "TypedReference":
				return ResolvePotentialBuiltInType(universe.System_TypedReference, 22);
			case "Byte":
				return ResolvePotentialBuiltInType(universe.System_Byte, 5);
			case "UInt16":
				return ResolvePotentialBuiltInType(universe.System_UInt16, 7);
			case "UInt32":
				return ResolvePotentialBuiltInType(universe.System_UInt32, 9);
			case "UInt64":
				return ResolvePotentialBuiltInType(universe.System_UInt64, 11);
			case "Void":
				return ResolvePotentialBuiltInType(universe.System_Void, 1);
			default:
				throw new InvalidOperationException();
			}
		}

		private bool ResolvePotentialBuiltInType(Type builtIn, byte elementType)
		{
			if (this == builtIn)
			{
				typeFlags |= TypeFlags.BuiltIn;
				sigElementType = elementType;
				return true;
			}
			return false;
		}

		private bool CheckVisibility(TypeAttributes access)
		{
			return (Attributes & TypeAttributes.VisibilityMask) == access;
		}

		internal static bool ContainsMissingType(Type[] types)
		{
			if (types == null)
			{
				return false;
			}
			for (int i = 0; i < types.Length; i++)
			{
				if (types[i].__ContainsMissingType)
				{
					return true;
				}
			}
			return false;
		}

		public Type MakeArrayType()
		{
			return ArrayType.Make(this, default(CustomModifiers));
		}

		public Type __MakeArrayType(CustomModifiers customModifiers)
		{
			return ArrayType.Make(this, customModifiers);
		}

		[Obsolete("Please use __MakeArrayType(CustomModifiers) instead.")]
		public Type __MakeArrayType(Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			return __MakeArrayType(CustomModifiers.FromReqOpt(requiredCustomModifiers, optionalCustomModifiers));
		}

		public Type MakeArrayType(int rank)
		{
			return __MakeArrayType(rank, default(CustomModifiers));
		}

		public Type __MakeArrayType(int rank, CustomModifiers customModifiers)
		{
			return MultiArrayType.Make(this, rank, Empty<int>.Array, new int[rank], customModifiers);
		}

		[Obsolete("Please use __MakeArrayType(int, CustomModifiers) instead.")]
		public Type __MakeArrayType(int rank, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			return __MakeArrayType(rank, CustomModifiers.FromReqOpt(requiredCustomModifiers, optionalCustomModifiers));
		}

		public Type __MakeArrayType(int rank, int[] sizes, int[] lobounds, CustomModifiers customModifiers)
		{
			return MultiArrayType.Make(this, rank, sizes ?? Empty<int>.Array, lobounds ?? Empty<int>.Array, customModifiers);
		}

		[Obsolete("Please use __MakeArrayType(int, int[], int[], CustomModifiers) instead.")]
		public Type __MakeArrayType(int rank, int[] sizes, int[] lobounds, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			return __MakeArrayType(rank, sizes, lobounds, CustomModifiers.FromReqOpt(requiredCustomModifiers, optionalCustomModifiers));
		}

		public Type MakeByRefType()
		{
			return ByRefType.Make(this, default(CustomModifiers));
		}

		public Type __MakeByRefType(CustomModifiers customModifiers)
		{
			return ByRefType.Make(this, customModifiers);
		}

		[Obsolete("Please use __MakeByRefType(CustomModifiers) instead.")]
		public Type __MakeByRefType(Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			return __MakeByRefType(CustomModifiers.FromReqOpt(requiredCustomModifiers, optionalCustomModifiers));
		}

		public Type MakePointerType()
		{
			return PointerType.Make(this, default(CustomModifiers));
		}

		public Type __MakePointerType(CustomModifiers customModifiers)
		{
			return PointerType.Make(this, customModifiers);
		}

		[Obsolete("Please use __MakeByRefType(CustomModifiers) instead.")]
		public Type __MakePointerType(Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			return __MakePointerType(CustomModifiers.FromReqOpt(requiredCustomModifiers, optionalCustomModifiers));
		}

		public Type MakeGenericType(params Type[] typeArguments)
		{
			return __MakeGenericType(typeArguments, null);
		}

		public Type __MakeGenericType(Type[] typeArguments, CustomModifiers[] customModifiers)
		{
			if (!__IsMissing && !IsGenericTypeDefinition)
			{
				throw new InvalidOperationException();
			}
			return GenericTypeInstance.Make(this, Util.Copy(typeArguments), (customModifiers == null) ? null : ((CustomModifiers[])customModifiers.Clone()));
		}

		[Obsolete("Please use __MakeGenericType(Type[], CustomModifiers[]) instead.")]
		public Type __MakeGenericType(Type[] typeArguments, Type[][] requiredCustomModifiers, Type[][] optionalCustomModifiers)
		{
			if (!__IsMissing && !IsGenericTypeDefinition)
			{
				throw new InvalidOperationException();
			}
			CustomModifiers[] array = null;
			if (requiredCustomModifiers != null || optionalCustomModifiers != null)
			{
				array = new CustomModifiers[typeArguments.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = CustomModifiers.FromReqOpt(Util.NullSafeElementAt(requiredCustomModifiers, i), Util.NullSafeElementAt(optionalCustomModifiers, i));
				}
			}
			return GenericTypeInstance.Make(this, Util.Copy(typeArguments), array);
		}

		public static System.Type __GetSystemType(TypeCode typeCode)
		{
			switch (typeCode)
			{
			case TypeCode.Boolean:
				return typeof(bool);
			case TypeCode.Byte:
				return typeof(byte);
			case TypeCode.Char:
				return typeof(char);
			case TypeCode.DBNull:
				return typeof(DBNull);
			case TypeCode.DateTime:
				return typeof(DateTime);
			case TypeCode.Decimal:
				return typeof(decimal);
			case TypeCode.Double:
				return typeof(double);
			case TypeCode.Empty:
				return null;
			case TypeCode.Int16:
				return typeof(short);
			case TypeCode.Int32:
				return typeof(int);
			case TypeCode.Int64:
				return typeof(long);
			case TypeCode.Object:
				return typeof(object);
			case TypeCode.SByte:
				return typeof(sbyte);
			case TypeCode.Single:
				return typeof(float);
			case TypeCode.String:
				return typeof(string);
			case TypeCode.UInt16:
				return typeof(ushort);
			case TypeCode.UInt32:
				return typeof(uint);
			case TypeCode.UInt64:
				return typeof(ulong);
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		public static TypeCode GetTypeCode(Type type)
		{
			if (type == null)
			{
				return TypeCode.Empty;
			}
			if (!type.__IsMissing && type.IsEnum)
			{
				type = type.GetEnumUnderlyingType();
			}
			Universe universe = type.Module.universe;
			if (type == universe.System_Boolean)
			{
				return TypeCode.Boolean;
			}
			if (type == universe.System_Char)
			{
				return TypeCode.Char;
			}
			if (type == universe.System_SByte)
			{
				return TypeCode.SByte;
			}
			if (type == universe.System_Byte)
			{
				return TypeCode.Byte;
			}
			if (type == universe.System_Int16)
			{
				return TypeCode.Int16;
			}
			if (type == universe.System_UInt16)
			{
				return TypeCode.UInt16;
			}
			if (type == universe.System_Int32)
			{
				return TypeCode.Int32;
			}
			if (type == universe.System_UInt32)
			{
				return TypeCode.UInt32;
			}
			if (type == universe.System_Int64)
			{
				return TypeCode.Int64;
			}
			if (type == universe.System_UInt64)
			{
				return TypeCode.UInt64;
			}
			if (type == universe.System_Single)
			{
				return TypeCode.Single;
			}
			if (type == universe.System_Double)
			{
				return TypeCode.Double;
			}
			if (type == universe.System_DateTime)
			{
				return TypeCode.DateTime;
			}
			if (type == universe.System_DBNull)
			{
				return TypeCode.DBNull;
			}
			if (type == universe.System_Decimal)
			{
				return TypeCode.Decimal;
			}
			if (type == universe.System_String)
			{
				return TypeCode.String;
			}
			if (type.__IsMissing)
			{
				throw new MissingMemberException(type);
			}
			return TypeCode.Object;
		}

		public bool IsAssignableFrom(Type type)
		{
			if (Equals(type))
			{
				return true;
			}
			if (type == null)
			{
				return false;
			}
			if (IsArray && type.IsArray)
			{
				if (GetArrayRank() != type.GetArrayRank())
				{
					return false;
				}
				if (__IsVector && !type.__IsVector)
				{
					return false;
				}
				Type elementType = GetElementType();
				Type elementType2 = type.GetElementType();
				if (elementType.IsValueType == elementType2.IsValueType)
				{
					return elementType.IsAssignableFrom(elementType2);
				}
				return false;
			}
			if (IsCovariant(type))
			{
				return true;
			}
			if (IsSealed)
			{
				return false;
			}
			if (IsInterface)
			{
				Type[] interfaces = type.GetInterfaces();
				foreach (Type type2 in interfaces)
				{
					if (Equals(type2) || IsCovariant(type2))
					{
						return true;
					}
				}
				return false;
			}
			if (type.IsInterface)
			{
				return this == Module.universe.System_Object;
			}
			if (type.IsPointer)
			{
				if (!(this == Module.universe.System_Object))
				{
					return this == Module.universe.System_ValueType;
				}
				return true;
			}
			return type.IsSubclassOf(this);
		}

		private bool IsCovariant(Type other)
		{
			if (IsConstructedGenericType && other.IsConstructedGenericType && GetGenericTypeDefinition() == other.GetGenericTypeDefinition())
			{
				Type[] genericArguments = GetGenericTypeDefinition().GetGenericArguments();
				for (int i = 0; i < genericArguments.Length; i++)
				{
					Type genericTypeArgument = GetGenericTypeArgument(i);
					Type genericTypeArgument2 = other.GetGenericTypeArgument(i);
					if (genericTypeArgument.IsValueType != genericTypeArgument2.IsValueType)
					{
						return false;
					}
					switch (genericArguments[i].GenericParameterAttributes & GenericParameterAttributes.VarianceMask)
					{
					case GenericParameterAttributes.Covariant:
						if (!genericTypeArgument.IsAssignableFrom(genericTypeArgument2))
						{
							return false;
						}
						break;
					case GenericParameterAttributes.Contravariant:
						if (!genericTypeArgument2.IsAssignableFrom(genericTypeArgument))
						{
							return false;
						}
						break;
					case GenericParameterAttributes.None:
						if (genericTypeArgument != genericTypeArgument2)
						{
							return false;
						}
						break;
					}
				}
				return true;
			}
			return false;
		}

		public bool IsSubclassOf(Type type)
		{
			Type baseType = BaseType;
			while (baseType != null)
			{
				if (baseType.Equals(type))
				{
					return true;
				}
				baseType = baseType.BaseType;
			}
			return false;
		}

		private bool IsDirectlyImplementedInterface(Type interfaceType)
		{
			Type[] array = __GetDeclaredInterfaces();
			foreach (Type type in array)
			{
				if (interfaceType.IsAssignableFrom(type))
				{
					return true;
				}
			}
			return false;
		}

		public InterfaceMapping GetInterfaceMap(Type interfaceType)
		{
			CheckBaked();
			InterfaceMapping result = default(InterfaceMapping);
			result.InterfaceMethods = interfaceType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
			result.InterfaceType = interfaceType;
			result.TargetMethods = new MethodInfo[result.InterfaceMethods.Length];
			result.TargetType = this;
			FillInInterfaceMethods(interfaceType, result.InterfaceMethods, result.TargetMethods);
			return result;
		}

		private void FillInInterfaceMethods(Type interfaceType, MethodInfo[] interfaceMethods, MethodInfo[] targetMethods)
		{
			FillInExplicitInterfaceMethods(interfaceMethods, targetMethods);
			bool num = IsDirectlyImplementedInterface(interfaceType);
			if (num)
			{
				FillInImplicitInterfaceMethods(interfaceMethods, targetMethods);
			}
			Type baseType = BaseType;
			if (baseType != null)
			{
				baseType.FillInInterfaceMethods(interfaceType, interfaceMethods, targetMethods);
				ReplaceOverriddenMethods(targetMethods);
			}
			if (num)
			{
				Type baseType2 = BaseType;
				while (baseType2 != null && baseType2.Module == Module)
				{
					baseType2.FillInImplicitInterfaceMethods(interfaceMethods, targetMethods);
					baseType2 = baseType2.BaseType;
				}
			}
		}

		private void FillInImplicitInterfaceMethods(MethodInfo[] interfaceMethods, MethodInfo[] targetMethods)
		{
			MethodBase[] array = null;
			for (int i = 0; i < targetMethods.Length; i++)
			{
				if (!(targetMethods[i] == null))
				{
					continue;
				}
				if (array == null)
				{
					array = __GetDeclaredMethods();
				}
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j].IsVirtual && array[j].Name == interfaceMethods[i].Name && array[j].MethodSignature.Equals(interfaceMethods[i].MethodSignature))
					{
						targetMethods[i] = (MethodInfo)array[j];
						break;
					}
				}
			}
		}

		private void ReplaceOverriddenMethods(MethodInfo[] baseMethods)
		{
			__MethodImplMap _MethodImplMap = __GetMethodImplMap();
			for (int i = 0; i < baseMethods.Length; i++)
			{
				if (!(baseMethods[i] != null) || baseMethods[i].IsFinal)
				{
					continue;
				}
				MethodInfo baseDefinition = baseMethods[i].GetBaseDefinition();
				int num = 0;
				while (true)
				{
					if (num < _MethodImplMap.MethodDeclarations.Length)
					{
						int num2 = 0;
						while (num2 < _MethodImplMap.MethodDeclarations[num].Length)
						{
							if (!(_MethodImplMap.MethodDeclarations[num][num2].GetBaseDefinition() == baseDefinition))
							{
								num2++;
								continue;
							}
							goto IL_0055;
						}
						num++;
						continue;
					}
					MethodInfo methodInfo = FindMethod(baseDefinition.Name, baseDefinition.MethodSignature) as MethodInfo;
					if (methodInfo != null && methodInfo.IsVirtual && !methodInfo.IsNewSlot)
					{
						baseMethods[i] = methodInfo;
					}
					break;
					IL_0055:
					baseMethods[i] = _MethodImplMap.MethodBodies[num];
					break;
				}
			}
		}

		internal void FillInExplicitInterfaceMethods(MethodInfo[] interfaceMethods, MethodInfo[] targetMethods)
		{
			__MethodImplMap _MethodImplMap = __GetMethodImplMap();
			for (int i = 0; i < _MethodImplMap.MethodDeclarations.Length; i++)
			{
				for (int j = 0; j < _MethodImplMap.MethodDeclarations[i].Length; j++)
				{
					int num = Array.IndexOf(interfaceMethods, _MethodImplMap.MethodDeclarations[i][j]);
					if (num != -1 && targetMethods[num] == null)
					{
						targetMethods[num] = _MethodImplMap.MethodBodies[i];
					}
				}
			}
		}

		Type IGenericContext.GetGenericTypeArgument(int index)
		{
			return GetGenericTypeArgument(index);
		}

		Type IGenericContext.GetGenericMethodArgument(int index)
		{
			throw new BadImageFormatException();
		}

		Type IGenericBinder.BindTypeParameter(Type type)
		{
			return GetGenericTypeArgument(type.GenericParameterPosition);
		}

		Type IGenericBinder.BindMethodParameter(Type type)
		{
			throw new BadImageFormatException();
		}

		internal virtual Type BindTypeParameters(IGenericBinder binder)
		{
			if (IsGenericTypeDefinition)
			{
				Type[] genericArguments = GetGenericArguments();
				InplaceBindTypeParameters(binder, genericArguments);
				return GenericTypeInstance.Make(this, genericArguments, null);
			}
			return this;
		}

		private static void InplaceBindTypeParameters(IGenericBinder binder, Type[] types)
		{
			for (int i = 0; i < types.Length; i++)
			{
				types[i] = types[i].BindTypeParameters(binder);
			}
		}

		internal virtual MethodBase FindMethod(string name, MethodSignature signature)
		{
			MethodBase[] array = __GetDeclaredMethods();
			foreach (MethodBase methodBase in array)
			{
				if (methodBase.Name == name && methodBase.MethodSignature.Equals(signature))
				{
					return methodBase;
				}
			}
			return null;
		}

		internal virtual FieldInfo FindField(string name, FieldSignature signature)
		{
			FieldInfo[] array = __GetDeclaredFields();
			foreach (FieldInfo fieldInfo in array)
			{
				if (fieldInfo.Name == name && fieldInfo.FieldSignature.Equals(signature))
				{
					return fieldInfo;
				}
			}
			return null;
		}

		internal Type MarkNotValueType()
		{
			typeFlags |= TypeFlags.NotValueType;
			return this;
		}

		internal Type MarkValueType()
		{
			typeFlags |= TypeFlags.ValueType;
			return this;
		}

		internal ConstructorInfo GetPseudoCustomAttributeConstructor(params Type[] parameterTypes)
		{
			Universe universe = Module.universe;
			MethodSignature signature = MethodSignature.MakeFromBuilder(universe.System_Void, parameterTypes, default(PackedCustomModifiers), CallingConventions.Standard | CallingConventions.HasThis, 0);
			return (ConstructorInfo)(FindMethod(".ctor", signature) ?? universe.GetMissingMethodOrThrow(null, this, ".ctor", signature));
		}

		public MethodBase __CreateMissingMethod(string name, CallingConventions callingConvention, Type returnType, CustomModifiers returnTypeCustomModifiers, Type[] parameterTypes, CustomModifiers[] parameterTypeCustomModifiers)
		{
			return CreateMissingMethod(name, callingConvention, returnType, parameterTypes, PackedCustomModifiers.CreateFromExternal(returnTypeCustomModifiers, parameterTypeCustomModifiers, parameterTypes.Length));
		}

		private MethodBase CreateMissingMethod(string name, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, PackedCustomModifiers customModifiers)
		{
			MethodSignature signature = new MethodSignature(returnType ?? Module.universe.System_Void, Util.Copy(parameterTypes), customModifiers, callingConvention, 0);
			MethodInfo methodInfo = new MissingMethod(this, name, signature);
			if (name == ".ctor" || name == ".cctor")
			{
				return new ConstructorInfoImpl(methodInfo);
			}
			return methodInfo;
		}

		[Obsolete("Please use __CreateMissingMethod(string, CallingConventions, Type, CustomModifiers, Type[], CustomModifiers[]) instead")]
		public MethodBase __CreateMissingMethod(string name, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
		{
			return CreateMissingMethod(name, callingConvention, returnType, parameterTypes, PackedCustomModifiers.CreateFromExternal(returnTypeOptionalCustomModifiers, returnTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers, parameterTypeRequiredCustomModifiers, parameterTypes.Length));
		}

		public FieldInfo __CreateMissingField(string name, Type fieldType, CustomModifiers customModifiers)
		{
			return new MissingField(this, name, FieldSignature.Create(fieldType, customModifiers));
		}

		[Obsolete("Please use __CreateMissingField(string, Type, CustomModifiers) instead")]
		public FieldInfo __CreateMissingField(string name, Type fieldType, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			return __CreateMissingField(name, fieldType, CustomModifiers.FromReqOpt(requiredCustomModifiers, optionalCustomModifiers));
		}

		public PropertyInfo __CreateMissingProperty(string name, CallingConventions callingConvention, Type propertyType, CustomModifiers propertyTypeCustomModifiers, Type[] parameterTypes, CustomModifiers[] parameterTypeCustomModifiers)
		{
			PropertySignature signature = PropertySignature.Create(callingConvention, propertyType, parameterTypes, PackedCustomModifiers.CreateFromExternal(propertyTypeCustomModifiers, parameterTypeCustomModifiers, Util.NullSafeLength(parameterTypes)));
			return new MissingProperty(this, name, signature);
		}

		internal virtual Type SetMetadataTokenForMissing(int token, int flags)
		{
			return this;
		}

		internal virtual Type SetCyclicTypeForwarder()
		{
			return this;
		}

		protected void MarkKnownType(string typeNamespace, string typeName)
		{
			if (typeNamespace == "System")
			{
				switch (typeName)
				{
				case "Boolean":
				case "Char":
				case "Object":
				case "String":
				case "Single":
				case "Double":
				case "SByte":
				case "Int16":
				case "Int32":
				case "Int64":
				case "IntPtr":
				case "UIntPtr":
				case "TypedReference":
				case "Byte":
				case "UInt16":
				case "UInt32":
				case "UInt64":
				case "Void":
					typeFlags |= TypeFlags.PotentialBuiltIn;
					break;
				case "Enum":
				case "ValueType":
					typeFlags |= TypeFlags.PotentialEnumOrValueType;
					break;
				}
			}
		}

		private bool ResolvePotentialEnumOrValueType()
		{
			if (Assembly == Universe.Mscorlib || Assembly.GetName().Name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase) || Universe.Mscorlib.FindType(TypeName) == this)
			{
				typeFlags = (typeFlags & ~TypeFlags.PotentialEnumOrValueType) | TypeFlags.EnumOrValueType;
				return true;
			}
			typeFlags &= ~TypeFlags.PotentialEnumOrValueType;
			return false;
		}

		internal sealed override bool BindingFlagsMatch(BindingFlags flags)
		{
			return MemberInfo.BindingFlagsMatch(IsNestedPublic, flags, BindingFlags.Public, BindingFlags.NonPublic);
		}

		internal sealed override MemberInfo SetReflectedType(Type type)
		{
			throw new InvalidOperationException();
		}

		internal override int GetCurrentToken()
		{
			return MetadataToken;
		}

		internal sealed override List<CustomAttributeData> GetPseudoCustomAttributes(Type attributeType)
		{
			return null;
		}

		public TypeInfo GetTypeInfo()
		{
			TypeInfo obj = this as TypeInfo;
			if (obj == null)
			{
				throw new MissingMemberException(this);
			}
			return obj;
		}
	}
}
