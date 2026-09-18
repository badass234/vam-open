using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Reader;

namespace IKVM.Reflection
{
	public sealed class CustomAttributeData
	{
		internal static readonly IList<CustomAttributeData> EmptyList = new List<CustomAttributeData>(0).AsReadOnly();

		private readonly Module module;

		private readonly int customAttributeIndex;

		private readonly int declSecurityIndex;

		private readonly byte[] declSecurityBlob;

		private ConstructorInfo lazyConstructor;

		private IList<CustomAttributeTypedArgument> lazyConstructorArguments;

		private IList<CustomAttributeNamedArgument> lazyNamedArguments;

		public int __Parent
		{
			get
			{
				if (customAttributeIndex < 0)
				{
					if (declSecurityIndex < 0)
					{
						return 0;
					}
					return module.DeclSecurity.records[declSecurityIndex].Parent;
				}
				return module.CustomAttribute.records[customAttributeIndex].Parent;
			}
		}

		public Type AttributeType => Constructor.DeclaringType;

		public ConstructorInfo Constructor
		{
			get
			{
				if (lazyConstructor == null)
				{
					lazyConstructor = (ConstructorInfo)module.ResolveMethod(module.CustomAttribute.records[customAttributeIndex].Type);
				}
				return lazyConstructor;
			}
		}

		public IList<CustomAttributeTypedArgument> ConstructorArguments
		{
			get
			{
				if (lazyConstructorArguments == null)
				{
					LazyParseArguments(false);
				}
				return lazyConstructorArguments;
			}
		}

		public IList<CustomAttributeNamedArgument> NamedArguments
		{
			get
			{
				if (lazyNamedArguments == null)
				{
					if (customAttributeIndex >= 0)
					{
						LazyParseArguments(true);
					}
					else
					{
						ByteReader byteReader = new ByteReader(declSecurityBlob, 0, declSecurityBlob.Length);
						lazyNamedArguments = ReadNamedArguments(module, byteReader, byteReader.ReadCompressedUInt(), Constructor.DeclaringType, true);
					}
				}
				return lazyNamedArguments;
			}
		}

		internal CustomAttributeData(Module module, int index)
		{
			this.module = module;
			customAttributeIndex = index;
			declSecurityIndex = -1;
		}

		internal CustomAttributeData(Module module, ConstructorInfo constructor, object[] args, List<CustomAttributeNamedArgument> namedArguments)
			: this(module, constructor, WrapConstructorArgs(args, constructor.MethodSignature), namedArguments)
		{
		}

		private static List<CustomAttributeTypedArgument> WrapConstructorArgs(object[] args, MethodSignature sig)
		{
			List<CustomAttributeTypedArgument> list = new List<CustomAttributeTypedArgument>();
			for (int i = 0; i < args.Length; i++)
			{
				list.Add(new CustomAttributeTypedArgument(sig.GetParameterType(i), args[i]));
			}
			return list;
		}

		internal CustomAttributeData(Module module, ConstructorInfo constructor, List<CustomAttributeTypedArgument> constructorArgs, List<CustomAttributeNamedArgument> namedArguments)
		{
			this.module = module;
			customAttributeIndex = -1;
			declSecurityIndex = -1;
			lazyConstructor = constructor;
			lazyConstructorArguments = constructorArgs.AsReadOnly();
			if (namedArguments == null)
			{
				lazyNamedArguments = Empty<CustomAttributeNamedArgument>.Array;
			}
			else
			{
				lazyNamedArguments = namedArguments.AsReadOnly();
			}
		}

		internal CustomAttributeData(Assembly asm, ConstructorInfo constructor, ByteReader br)
		{
			module = asm.ManifestModule;
			customAttributeIndex = -1;
			declSecurityIndex = -1;
			lazyConstructor = constructor;
			if (br.Length == 0)
			{
				lazyConstructorArguments = Empty<CustomAttributeTypedArgument>.Array;
				lazyNamedArguments = Empty<CustomAttributeNamedArgument>.Array;
				return;
			}
			if (br.ReadUInt16() != 1)
			{
				throw new BadImageFormatException();
			}
			lazyConstructorArguments = ReadConstructorArguments(module, br, constructor);
			lazyNamedArguments = ReadNamedArguments(module, br, br.ReadUInt16(), constructor.DeclaringType, true);
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append('[');
			stringBuilder.Append(Constructor.DeclaringType.FullName);
			stringBuilder.Append('(');
			string value = "";
			ParameterInfo[] parameters = Constructor.GetParameters();
			IList<CustomAttributeTypedArgument> constructorArguments = ConstructorArguments;
			for (int i = 0; i < parameters.Length; i++)
			{
				stringBuilder.Append(value);
				value = ", ";
				AppendValue(stringBuilder, parameters[i].ParameterType, constructorArguments[i]);
			}
			foreach (CustomAttributeNamedArgument namedArgument in NamedArguments)
			{
				stringBuilder.Append(value);
				value = ", ";
				stringBuilder.Append(namedArgument.MemberInfo.Name);
				stringBuilder.Append(" = ");
				FieldInfo fieldInfo = namedArgument.MemberInfo as FieldInfo;
				Type type = ((fieldInfo != null) ? fieldInfo.FieldType : ((PropertyInfo)namedArgument.MemberInfo).PropertyType);
				AppendValue(stringBuilder, type, namedArgument.TypedValue);
			}
			stringBuilder.Append(')');
			stringBuilder.Append(']');
			return stringBuilder.ToString();
		}

		private static void AppendValue(StringBuilder sb, Type type, CustomAttributeTypedArgument arg)
		{
			if (arg.ArgumentType == arg.ArgumentType.Module.universe.System_String)
			{
				sb.Append('"').Append(arg.Value).Append('"');
			}
			else if (arg.ArgumentType.IsArray)
			{
				Type elementType = arg.ArgumentType.GetElementType();
				string value = ((!elementType.IsPrimitive && !(elementType == type.Module.universe.System_Object) && !(elementType == type.Module.universe.System_String) && !(elementType == type.Module.universe.System_Type)) ? elementType.FullName : elementType.Name);
				sb.Append("new ").Append(value).Append("[")
					.Append(((Array)arg.Value).Length)
					.Append("] { ");
				string value2 = "";
				CustomAttributeTypedArgument[] array = (CustomAttributeTypedArgument[])arg.Value;
				foreach (CustomAttributeTypedArgument arg2 in array)
				{
					sb.Append(value2);
					value2 = ", ";
					AppendValue(sb, elementType, arg2);
				}
				sb.Append(" }");
			}
			else
			{
				if (arg.ArgumentType != type || (type.IsEnum && !arg.Value.Equals(0)))
				{
					sb.Append('(');
					sb.Append(arg.ArgumentType.FullName);
					sb.Append(')');
				}
				sb.Append(arg.Value);
			}
		}

		internal static void ReadDeclarativeSecurity(Module module, int index, List<CustomAttributeData> list)
		{
			Universe universe = module.universe;
			Assembly assembly = module.Assembly;
			int action = module.DeclSecurity.records[index].Action;
			ByteReader blob = module.GetBlob(module.DeclSecurity.records[index].PermissionSet);
			if (blob.PeekByte() == 46)
			{
				blob.ReadByte();
				int num = blob.ReadCompressedUInt();
				for (int i = 0; i < num; i++)
				{
					ConstructorInfo pseudoCustomAttributeConstructor = ReadType(module, blob).GetPseudoCustomAttributeConstructor(universe.System_Security_Permissions_SecurityAction);
					byte[] blob2 = blob.ReadBytes(blob.ReadCompressedUInt());
					list.Add(new CustomAttributeData(assembly, pseudoCustomAttributeConstructor, action, blob2, index));
				}
				return;
			}
			char[] array = new char[blob.Length / 2];
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = blob.ReadChar();
			}
			string value = new string(array);
			ConstructorInfo pseudoCustomAttributeConstructor2 = universe.System_Security_Permissions_PermissionSetAttribute.GetPseudoCustomAttributeConstructor(universe.System_Security_Permissions_SecurityAction);
			List<CustomAttributeNamedArgument> list2 = new List<CustomAttributeNamedArgument>();
			list2.Add(new CustomAttributeNamedArgument(GetProperty(null, universe.System_Security_Permissions_PermissionSetAttribute, "XML", universe.System_String), new CustomAttributeTypedArgument(universe.System_String, value)));
			list.Add(new CustomAttributeData(assembly.ManifestModule, pseudoCustomAttributeConstructor2, new object[1] { action }, list2));
		}

		internal CustomAttributeData(Assembly asm, ConstructorInfo constructor, int securityAction, byte[] blob, int index)
		{
			module = asm.ManifestModule;
			customAttributeIndex = -1;
			declSecurityIndex = index;
			Universe universe = constructor.Module.universe;
			lazyConstructor = constructor;
			lazyConstructorArguments = new List<CustomAttributeTypedArgument>
			{
				new CustomAttributeTypedArgument(universe.System_Security_Permissions_SecurityAction, securityAction)
			}.AsReadOnly();
			declSecurityBlob = blob;
		}

		private static Type ReadFieldOrPropType(Module context, ByteReader br)
		{
			Universe universe = context.universe;
			switch (br.ReadByte())
			{
			case 2:
				return universe.System_Boolean;
			case 3:
				return universe.System_Char;
			case 4:
				return universe.System_SByte;
			case 5:
				return universe.System_Byte;
			case 6:
				return universe.System_Int16;
			case 7:
				return universe.System_UInt16;
			case 8:
				return universe.System_Int32;
			case 9:
				return universe.System_UInt32;
			case 10:
				return universe.System_Int64;
			case 11:
				return universe.System_UInt64;
			case 12:
				return universe.System_Single;
			case 13:
				return universe.System_Double;
			case 14:
				return universe.System_String;
			case 29:
				return ReadFieldOrPropType(context, br).MakeArrayType();
			case 85:
				return ReadType(context, br);
			case 80:
				return universe.System_Type;
			case 81:
				return universe.System_Object;
			default:
				throw new BadImageFormatException();
			}
		}

		private static CustomAttributeTypedArgument ReadFixedArg(Module context, ByteReader br, Type type)
		{
			Universe universe = context.universe;
			if (type == universe.System_String)
			{
				return new CustomAttributeTypedArgument(type, br.ReadString());
			}
			if (type == universe.System_Boolean)
			{
				return new CustomAttributeTypedArgument(type, br.ReadByte() != 0);
			}
			if (type == universe.System_Char)
			{
				return new CustomAttributeTypedArgument(type, br.ReadChar());
			}
			if (type == universe.System_Single)
			{
				return new CustomAttributeTypedArgument(type, br.ReadSingle());
			}
			if (type == universe.System_Double)
			{
				return new CustomAttributeTypedArgument(type, br.ReadDouble());
			}
			if (type == universe.System_SByte)
			{
				return new CustomAttributeTypedArgument(type, br.ReadSByte());
			}
			if (type == universe.System_Int16)
			{
				return new CustomAttributeTypedArgument(type, br.ReadInt16());
			}
			if (type == universe.System_Int32)
			{
				return new CustomAttributeTypedArgument(type, br.ReadInt32());
			}
			if (type == universe.System_Int64)
			{
				return new CustomAttributeTypedArgument(type, br.ReadInt64());
			}
			if (type == universe.System_Byte)
			{
				return new CustomAttributeTypedArgument(type, br.ReadByte());
			}
			if (type == universe.System_UInt16)
			{
				return new CustomAttributeTypedArgument(type, br.ReadUInt16());
			}
			if (type == universe.System_UInt32)
			{
				return new CustomAttributeTypedArgument(type, br.ReadUInt32());
			}
			if (type == universe.System_UInt64)
			{
				return new CustomAttributeTypedArgument(type, br.ReadUInt64());
			}
			if (type == universe.System_Type)
			{
				return new CustomAttributeTypedArgument(type, ReadType(context, br));
			}
			if (type == universe.System_Object)
			{
				return ReadFixedArg(context, br, ReadFieldOrPropType(context, br));
			}
			if (type.IsArray)
			{
				int num = br.ReadInt32();
				if (num == -1)
				{
					return new CustomAttributeTypedArgument(type, null);
				}
				Type elementType = type.GetElementType();
				CustomAttributeTypedArgument[] array = new CustomAttributeTypedArgument[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = ReadFixedArg(context, br, elementType);
				}
				return new CustomAttributeTypedArgument(type, array);
			}
			if (type.IsEnum)
			{
				return new CustomAttributeTypedArgument(type, ReadFixedArg(context, br, type.GetEnumUnderlyingTypeImpl()).Value);
			}
			throw new InvalidOperationException();
		}

		private static Type ReadType(Module context, ByteReader br)
		{
			string text = br.ReadString();
			if (text == null)
			{
				return null;
			}
			if (text.Length > 0)
			{
				string text2 = text;
				if (text2[text2.Length - 1] == '\0')
				{
					text = text.Substring(0, text.Length - 1);
				}
			}
			return TypeNameParser.Parse(text, true).GetType(context.universe, context, true, text, true, false);
		}

		private static IList<CustomAttributeTypedArgument> ReadConstructorArguments(Module context, ByteReader br, ConstructorInfo constructor)
		{
			MethodSignature methodSignature = constructor.MethodSignature;
			int parameterCount = methodSignature.GetParameterCount();
			List<CustomAttributeTypedArgument> list = new List<CustomAttributeTypedArgument>(parameterCount);
			for (int i = 0; i < parameterCount; i++)
			{
				list.Add(ReadFixedArg(context, br, methodSignature.GetParameterType(i)));
			}
			return list.AsReadOnly();
		}

		private static IList<CustomAttributeNamedArgument> ReadNamedArguments(Module context, ByteReader br, int named, Type type, bool required)
		{
			List<CustomAttributeNamedArgument> list = new List<CustomAttributeNamedArgument>(named);
			for (int i = 0; i < named; i++)
			{
				byte b = br.ReadByte();
				Type type2 = ReadFieldOrPropType(context, br);
				if (type2.__IsMissing && !required)
				{
					return null;
				}
				string name = br.ReadString();
				CustomAttributeTypedArgument value = ReadFixedArg(context, br, type2);
				MemberInfo member;
				switch (b)
				{
				case 83:
					member = GetField(context, type, name, type2);
					break;
				case 84:
					member = GetProperty(context, type, name, type2);
					break;
				default:
					throw new BadImageFormatException();
				}
				list.Add(new CustomAttributeNamedArgument(member, value));
			}
			return list.AsReadOnly();
		}

		private static FieldInfo GetField(Module context, Type type, string name, Type fieldType)
		{
			Type type2 = type;
			while (type != null && !type.__IsMissing)
			{
				FieldInfo[] array = type.__GetDeclaredFields();
				foreach (FieldInfo fieldInfo in array)
				{
					if (fieldInfo.IsPublic && !fieldInfo.IsStatic && fieldInfo.Name == name)
					{
						return fieldInfo;
					}
				}
				type = type.BaseType;
			}
			if (type == null)
			{
				type = type2;
			}
			FieldSignature signature = FieldSignature.Create(fieldType, default(CustomModifiers));
			return type.FindField(name, signature) ?? type.Module.universe.GetMissingFieldOrThrow(context, type, name, signature);
		}

		private static PropertyInfo GetProperty(Module context, Type type, string name, Type propertyType)
		{
			Type type2 = type;
			while (type != null && !type.__IsMissing)
			{
				PropertyInfo[] array = type.__GetDeclaredProperties();
				foreach (PropertyInfo propertyInfo in array)
				{
					if (propertyInfo.IsPublic && !propertyInfo.IsStatic && propertyInfo.Name == name)
					{
						return propertyInfo;
					}
				}
				type = type.BaseType;
			}
			if (type == null)
			{
				type = type2;
			}
			return type.Module.universe.GetMissingPropertyOrThrow(context, type, name, PropertySignature.Create(CallingConventions.Standard | CallingConventions.HasThis, propertyType, null, default(PackedCustomModifiers)));
		}

		[Obsolete("Use AttributeType property instead.")]
		internal bool __TryReadTypeName(out string ns, out string name)
		{
			if (Constructor.DeclaringType.IsNested)
			{
				ns = null;
				name = null;
				return false;
			}
			TypeName typeName = AttributeType.TypeName;
			ns = typeName.Namespace;
			name = typeName.Name;
			return true;
		}

		public byte[] __GetBlob()
		{
			if (declSecurityBlob != null)
			{
				return (byte[])declSecurityBlob.Clone();
			}
			if (customAttributeIndex == -1)
			{
				return __ToBuilder().GetBlob(module.Assembly);
			}
			return ((ModuleReader)module).GetBlobCopy(module.CustomAttribute.records[customAttributeIndex].Value);
		}

		private void LazyParseArguments(bool requireNameArguments)
		{
			ByteReader blob = module.GetBlob(module.CustomAttribute.records[customAttributeIndex].Value);
			if (blob.Length == 0)
			{
				lazyConstructorArguments = Empty<CustomAttributeTypedArgument>.Array;
				lazyNamedArguments = Empty<CustomAttributeNamedArgument>.Array;
				return;
			}
			if (blob.ReadUInt16() != 1)
			{
				throw new BadImageFormatException();
			}
			lazyConstructorArguments = ReadConstructorArguments(module, blob, Constructor);
			lazyNamedArguments = ReadNamedArguments(module, blob, blob.ReadUInt16(), Constructor.DeclaringType, requireNameArguments);
		}

		public CustomAttributeBuilder __ToBuilder()
		{
			ParameterInfo[] parameters = Constructor.GetParameters();
			object[] array = new object[ConstructorArguments.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = RewrapArray(parameters[i].ParameterType, ConstructorArguments[i]);
			}
			List<PropertyInfo> list = new List<PropertyInfo>();
			List<object> list2 = new List<object>();
			List<FieldInfo> list3 = new List<FieldInfo>();
			List<object> list4 = new List<object>();
			foreach (CustomAttributeNamedArgument namedArgument in NamedArguments)
			{
				PropertyInfo propertyInfo = namedArgument.MemberInfo as PropertyInfo;
				if (propertyInfo != null)
				{
					list.Add(propertyInfo);
					list2.Add(RewrapArray(propertyInfo.PropertyType, namedArgument.TypedValue));
				}
				else
				{
					FieldInfo fieldInfo = (FieldInfo)namedArgument.MemberInfo;
					list3.Add(fieldInfo);
					list4.Add(RewrapArray(fieldInfo.FieldType, namedArgument.TypedValue));
				}
			}
			return new CustomAttributeBuilder(Constructor, array, list.ToArray(), list2.ToArray(), list3.ToArray(), list4.ToArray());
		}

		private static object RewrapArray(Type type, CustomAttributeTypedArgument arg)
		{
			IList<CustomAttributeTypedArgument> list = arg.Value as IList<CustomAttributeTypedArgument>;
			if (list != null)
			{
				Type elementType = arg.ArgumentType.GetElementType();
				object[] array = new object[list.Count];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = RewrapArray(elementType, list[i]);
				}
				if (type == type.Module.universe.System_Object)
				{
					return CustomAttributeBuilder.__MakeTypedArgument(arg.ArgumentType, array);
				}
				return array;
			}
			return arg.Value;
		}

		public static IList<CustomAttributeData> GetCustomAttributes(MemberInfo member)
		{
			return __GetCustomAttributes(member, null, false);
		}

		public static IList<CustomAttributeData> GetCustomAttributes(Assembly assembly)
		{
			return assembly.GetCustomAttributesData(null);
		}

		public static IList<CustomAttributeData> GetCustomAttributes(Module module)
		{
			return __GetCustomAttributes(module, null, false);
		}

		public static IList<CustomAttributeData> GetCustomAttributes(ParameterInfo parameter)
		{
			return __GetCustomAttributes(parameter, null, false);
		}

		public static IList<CustomAttributeData> __GetCustomAttributes(Assembly assembly, Type attributeType, bool inherit)
		{
			return assembly.GetCustomAttributesData(attributeType);
		}

		public static IList<CustomAttributeData> __GetCustomAttributes(Module module, Type attributeType, bool inherit)
		{
			if (module.__IsMissing)
			{
				throw new MissingModuleException((MissingModule)module);
			}
			IList<CustomAttributeData> customAttributesImpl = GetCustomAttributesImpl(null, module, 1, attributeType);
			return customAttributesImpl ?? EmptyList;
		}

		public static IList<CustomAttributeData> __GetCustomAttributes(ParameterInfo parameter, Type attributeType, bool inherit)
		{
			Module module = parameter.Module;
			List<CustomAttributeData> list = null;
			FieldMarshal fieldMarshal;
			if (module.universe.ReturnPseudoCustomAttributes && (attributeType == null || attributeType.IsAssignableFrom(parameter.Module.universe.System_Runtime_InteropServices_MarshalAsAttribute)) && parameter.__TryGetFieldMarshal(out fieldMarshal))
			{
				if (list == null)
				{
					list = new List<CustomAttributeData>();
				}
				list.Add(CreateMarshalAsPseudoCustomAttribute(parameter.Module, fieldMarshal));
			}
			ModuleBuilder moduleBuilder = module as ModuleBuilder;
			int num = parameter.MetadataToken;
			if (moduleBuilder != null && moduleBuilder.IsSaved && ModuleBuilder.IsPseudoToken(num))
			{
				num = moduleBuilder.ResolvePseudoToken(num);
			}
			IList<CustomAttributeData> customAttributesImpl = GetCustomAttributesImpl(list, module, num, attributeType);
			return customAttributesImpl ?? EmptyList;
		}

		public static IList<CustomAttributeData> __GetCustomAttributes(MemberInfo member, Type attributeType, bool inherit)
		{
			if (!member.IsBaked)
			{
				throw new NotImplementedException();
			}
			if (!inherit || !IsInheritableAttribute(attributeType))
			{
				IList<CustomAttributeData> customAttributesImpl = GetCustomAttributesImpl(null, member, attributeType);
				return customAttributesImpl ?? EmptyList;
			}
			List<CustomAttributeData> list = new List<CustomAttributeData>();
			while (true)
			{
				GetCustomAttributesImpl(list, member, attributeType);
				Type type = member as Type;
				if (type != null)
				{
					type = type.BaseType;
					if (type == null)
					{
						return list;
					}
					member = type;
					continue;
				}
				MethodInfo methodInfo = member as MethodInfo;
				if (!(methodInfo != null))
				{
					break;
				}
				MemberInfo memberInfo = member;
				methodInfo = methodInfo.GetBaseDefinition();
				if (methodInfo == null || methodInfo == memberInfo)
				{
					return list;
				}
				member = methodInfo;
			}
			return list;
		}

		private static List<CustomAttributeData> GetCustomAttributesImpl(List<CustomAttributeData> list, MemberInfo member, Type attributeType)
		{
			if (member.Module.universe.ReturnPseudoCustomAttributes)
			{
				List<CustomAttributeData> pseudoCustomAttributes = member.GetPseudoCustomAttributes(attributeType);
				if (list == null)
				{
					list = pseudoCustomAttributes;
				}
				else if (pseudoCustomAttributes != null)
				{
					list.AddRange(pseudoCustomAttributes);
				}
			}
			return GetCustomAttributesImpl(list, member.Module, member.GetCurrentToken(), attributeType);
		}

		internal static List<CustomAttributeData> GetCustomAttributesImpl(List<CustomAttributeData> list, Module module, int token, Type attributeType)
		{
			foreach (int item in module.CustomAttribute.Filter(token))
			{
				if (attributeType == null)
				{
					if (list == null)
					{
						list = new List<CustomAttributeData>();
					}
					list.Add(new CustomAttributeData(module, item));
				}
				else if (attributeType.IsAssignableFrom(module.ResolveMethod(module.CustomAttribute.records[item].Type).DeclaringType))
				{
					if (list == null)
					{
						list = new List<CustomAttributeData>();
					}
					list.Add(new CustomAttributeData(module, item));
				}
			}
			return list;
		}

		public static IList<CustomAttributeData> __GetCustomAttributes(Type type, Type interfaceType, Type attributeType, bool inherit)
		{
			Module module = type.Module;
			foreach (int item in module.InterfaceImpl.Filter(type.MetadataToken))
			{
				if (module.ResolveType(module.InterfaceImpl.records[item].Interface, type) == interfaceType)
				{
					IList<CustomAttributeData> customAttributesImpl = GetCustomAttributesImpl(null, module, 0x9000000 | (item + 1), attributeType);
					return customAttributesImpl ?? EmptyList;
				}
			}
			return EmptyList;
		}

		public static IList<CustomAttributeData> __GetDeclarativeSecurity(Assembly assembly)
		{
			if (assembly.__IsMissing)
			{
				throw new MissingAssemblyException((MissingAssembly)assembly);
			}
			return assembly.ManifestModule.GetDeclarativeSecurity(536870913);
		}

		public static IList<CustomAttributeData> __GetDeclarativeSecurity(Type type)
		{
			if ((type.Attributes & TypeAttributes.HasSecurity) != TypeAttributes.AnsiClass)
			{
				return type.Module.GetDeclarativeSecurity(type.MetadataToken);
			}
			return EmptyList;
		}

		public static IList<CustomAttributeData> __GetDeclarativeSecurity(MethodBase method)
		{
			if ((method.Attributes & MethodAttributes.HasSecurity) != MethodAttributes.PrivateScope)
			{
				return method.Module.GetDeclarativeSecurity(method.MetadataToken);
			}
			return EmptyList;
		}

		private static bool IsInheritableAttribute(Type attribute)
		{
			Type system_AttributeUsageAttribute = attribute.Module.universe.System_AttributeUsageAttribute;
			IList<CustomAttributeData> list = __GetCustomAttributes(attribute, system_AttributeUsageAttribute, false);
			if (list.Count != 0)
			{
				foreach (CustomAttributeNamedArgument namedArgument in list[0].NamedArguments)
				{
					if (namedArgument.MemberInfo.Name == "Inherited")
					{
						return (bool)namedArgument.TypedValue.Value;
					}
				}
			}
			return true;
		}

		internal static CustomAttributeData CreateDllImportPseudoCustomAttribute(Module module, ImplMapFlags flags, string entryPoint, string dllName, MethodImplAttributes attr)
		{
			Type system_Runtime_InteropServices_DllImportAttribute = module.universe.System_Runtime_InteropServices_DllImportAttribute;
			ConstructorInfo pseudoCustomAttributeConstructor = system_Runtime_InteropServices_DllImportAttribute.GetPseudoCustomAttributeConstructor(module.universe.System_String);
			List<CustomAttributeNamedArgument> list = new List<CustomAttributeNamedArgument>();
			CharSet charSet;
			switch (flags & ImplMapFlags.CharSetMask)
			{
			case ImplMapFlags.CharSetAnsi:
				charSet = CharSet.Ansi;
				break;
			case ImplMapFlags.CharSetUnicode:
				charSet = CharSet.Unicode;
				break;
			case ImplMapFlags.CharSetMask:
				charSet = CharSet.Auto;
				break;
			default:
				charSet = CharSet.None;
				break;
			}
			CallingConvention callingConvention;
			switch (flags & ImplMapFlags.CallConvMask)
			{
			case ImplMapFlags.CallConvCdecl:
				callingConvention = CallingConvention.Cdecl;
				break;
			case ImplMapFlags.CallConvFastcall:
				callingConvention = CallingConvention.FastCall;
				break;
			case ImplMapFlags.CallConvStdcall:
				callingConvention = CallingConvention.StdCall;
				break;
			case ImplMapFlags.CallConvThiscall:
				callingConvention = CallingConvention.ThisCall;
				break;
			case ImplMapFlags.CallConvWinapi:
				callingConvention = CallingConvention.Winapi;
				break;
			default:
				callingConvention = (CallingConvention)0;
				break;
			}
			AddNamedArgument(list, system_Runtime_InteropServices_DllImportAttribute, "EntryPoint", entryPoint);
			AddNamedArgument(list, system_Runtime_InteropServices_DllImportAttribute, "CharSet", module.universe.System_Runtime_InteropServices_CharSet, (int)charSet);
			AddNamedArgument(list, system_Runtime_InteropServices_DllImportAttribute, "ExactSpelling", (int)flags, 1);
			AddNamedArgument(list, system_Runtime_InteropServices_DllImportAttribute, "SetLastError", (int)flags, 64);
			AddNamedArgument(list, system_Runtime_InteropServices_DllImportAttribute, "PreserveSig", (int)attr, 128);
			AddNamedArgument(list, system_Runtime_InteropServices_DllImportAttribute, "CallingConvention", module.universe.System_Runtime_InteropServices_CallingConvention, (int)callingConvention);
			AddNamedArgument(list, system_Runtime_InteropServices_DllImportAttribute, "BestFitMapping", (int)flags, 16);
			AddNamedArgument(list, system_Runtime_InteropServices_DllImportAttribute, "ThrowOnUnmappableChar", (int)flags, 4096);
			return new CustomAttributeData(module, pseudoCustomAttributeConstructor, new object[1] { dllName }, list);
		}

		internal static CustomAttributeData CreateMarshalAsPseudoCustomAttribute(Module module, FieldMarshal fm)
		{
			Type system_Runtime_InteropServices_MarshalAsAttribute = module.universe.System_Runtime_InteropServices_MarshalAsAttribute;
			Type system_Runtime_InteropServices_UnmanagedType = module.universe.System_Runtime_InteropServices_UnmanagedType;
			Type system_Runtime_InteropServices_VarEnum = module.universe.System_Runtime_InteropServices_VarEnum;
			Type system_Type = module.universe.System_Type;
			List<CustomAttributeNamedArgument> list = new List<CustomAttributeNamedArgument>();
			AddNamedArgument(list, system_Runtime_InteropServices_MarshalAsAttribute, "ArraySubType", system_Runtime_InteropServices_UnmanagedType, (int)(fm.ArraySubType ?? ((UnmanagedType)0)));
			AddNamedArgument(list, system_Runtime_InteropServices_MarshalAsAttribute, "SizeParamIndex", module.universe.System_Int16, fm.SizeParamIndex ?? 0);
			AddNamedArgument(list, system_Runtime_InteropServices_MarshalAsAttribute, "SizeConst", module.universe.System_Int32, fm.SizeConst ?? 0);
			AddNamedArgument(list, system_Runtime_InteropServices_MarshalAsAttribute, "IidParameterIndex", module.universe.System_Int32, fm.IidParameterIndex ?? 0);
			AddNamedArgument(list, system_Runtime_InteropServices_MarshalAsAttribute, "SafeArraySubType", system_Runtime_InteropServices_VarEnum, (int)(fm.SafeArraySubType ?? VarEnum.VT_EMPTY));
			if (fm.SafeArrayUserDefinedSubType != null)
			{
				AddNamedArgument(list, system_Runtime_InteropServices_MarshalAsAttribute, "SafeArrayUserDefinedSubType", system_Type, fm.SafeArrayUserDefinedSubType);
			}
			if (fm.MarshalType != null)
			{
				AddNamedArgument(list, system_Runtime_InteropServices_MarshalAsAttribute, "MarshalType", module.universe.System_String, fm.MarshalType);
			}
			if (fm.MarshalTypeRef != null)
			{
				AddNamedArgument(list, system_Runtime_InteropServices_MarshalAsAttribute, "MarshalTypeRef", module.universe.System_Type, fm.MarshalTypeRef);
			}
			if (fm.MarshalCookie != null)
			{
				AddNamedArgument(list, system_Runtime_InteropServices_MarshalAsAttribute, "MarshalCookie", module.universe.System_String, fm.MarshalCookie);
			}
			ConstructorInfo pseudoCustomAttributeConstructor = system_Runtime_InteropServices_MarshalAsAttribute.GetPseudoCustomAttributeConstructor(system_Runtime_InteropServices_UnmanagedType);
			return new CustomAttributeData(module, pseudoCustomAttributeConstructor, new object[1] { (int)fm.UnmanagedType }, list);
		}

		private static void AddNamedArgument(List<CustomAttributeNamedArgument> list, Type type, string fieldName, string value)
		{
			AddNamedArgument(list, type, fieldName, type.Module.universe.System_String, value);
		}

		private static void AddNamedArgument(List<CustomAttributeNamedArgument> list, Type type, string fieldName, int flags, int flagMask)
		{
			AddNamedArgument(list, type, fieldName, type.Module.universe.System_Boolean, (flags & flagMask) != 0);
		}

		private static void AddNamedArgument(List<CustomAttributeNamedArgument> list, Type attributeType, string fieldName, Type valueType, object value)
		{
			FieldInfo fieldInfo = attributeType.FindField(fieldName, FieldSignature.Create(valueType, default(CustomModifiers)));
			if (fieldInfo != null)
			{
				list.Add(new CustomAttributeNamedArgument(fieldInfo, new CustomAttributeTypedArgument(valueType, value)));
			}
		}

		internal static CustomAttributeData CreateFieldOffsetPseudoCustomAttribute(Module module, int offset)
		{
			ConstructorInfo pseudoCustomAttributeConstructor = module.universe.System_Runtime_InteropServices_FieldOffsetAttribute.GetPseudoCustomAttributeConstructor(module.universe.System_Int32);
			return new CustomAttributeData(module, pseudoCustomAttributeConstructor, new object[1] { offset }, null);
		}

		internal static CustomAttributeData CreatePreserveSigPseudoCustomAttribute(Module module)
		{
			ConstructorInfo pseudoCustomAttributeConstructor = module.universe.System_Runtime_InteropServices_PreserveSigAttribute.GetPseudoCustomAttributeConstructor();
			return new CustomAttributeData(module, pseudoCustomAttributeConstructor, Empty<object>.Array, null);
		}
	}
}
