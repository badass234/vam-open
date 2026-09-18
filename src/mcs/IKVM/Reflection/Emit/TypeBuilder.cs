using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using IKVM.Reflection.Impl;
using IKVM.Reflection.Metadata;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Emit
{
	public sealed class TypeBuilder : TypeInfo, ITypeOwner
	{
		public const int UnspecifiedTypeSize = 0;

		private readonly ITypeOwner owner;

		private readonly int token;

		private int extends;

		private Type lazyBaseType;

		private readonly int typeName;

		private readonly int typeNameSpace;

		private readonly string ns;

		private readonly string name;

		private readonly List<MethodBuilder> methods = new List<MethodBuilder>();

		private readonly List<FieldBuilder> fields = new List<FieldBuilder>();

		private List<PropertyBuilder> properties;

		private List<EventBuilder> events;

		private TypeAttributes attribs;

		private GenericTypeParameterBuilder[] gtpb;

		private List<CustomAttributeBuilder> declarativeSecurity;

		private List<Type> interfaces;

		private int size;

		private short pack;

		private bool hasLayout;

		public int Size => size;

		public PackingSize PackingSize => (PackingSize)pack;

		public override bool ContainsGenericParameters => gtpb != null;

		public override Type BaseType
		{
			get
			{
				if (lazyBaseType == null && !base.IsInterface)
				{
					Type system_Object = Module.universe.System_Object;
					if (this != system_Object)
					{
						lazyBaseType = system_Object;
					}
				}
				return lazyBaseType;
			}
		}

		public override string FullName
		{
			get
			{
				if (base.IsNested)
				{
					return DeclaringType.FullName + "+" + TypeNameParser.Escape(name);
				}
				if (ns == null)
				{
					return TypeNameParser.Escape(name);
				}
				return TypeNameParser.Escape(ns) + "." + TypeNameParser.Escape(name);
			}
		}

		internal override TypeName TypeName => new TypeName(ns, name);

		public override string Name => name;

		public override string Namespace => ns ?? "";

		public override TypeAttributes Attributes => attribs;

		public override Type DeclaringType => owner as TypeBuilder;

		public override bool IsGenericType => IsGenericTypeDefinition;

		public override bool IsGenericTypeDefinition => (typeFlags & TypeFlags.IsGenericTypeDefinition) != 0;

		public override int MetadataToken => token;

		public override Module Module => owner.ModuleBuilder;

		public TypeToken TypeToken => new TypeToken(token);

		internal ModuleBuilder ModuleBuilder => owner.ModuleBuilder;

		ModuleBuilder ITypeOwner.ModuleBuilder => owner.ModuleBuilder;

		internal bool HasNestedTypes => (typeFlags & TypeFlags.HasNestedTypes) != 0;

		internal override bool IsModulePseudoType => token == 33554433;

		internal override bool IsBaked => IsCreated();

		internal TypeBuilder(ITypeOwner owner, string ns, string name)
		{
			this.owner = owner;
			token = ModuleBuilder.TypeDef.AllocToken();
			this.ns = ns;
			this.name = name;
			typeNameSpace = ((ns != null) ? ModuleBuilder.Strings.Add(ns) : 0);
			typeName = ModuleBuilder.Strings.Add(name);
			MarkKnownType(ns, name);
		}

		public ConstructorBuilder DefineDefaultConstructor(MethodAttributes attributes)
		{
			ConstructorBuilder constructorBuilder = DefineConstructor(attributes, CallingConventions.Standard, Type.EmptyTypes);
			ILGenerator iLGenerator = constructorBuilder.GetILGenerator();
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Call, BaseType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null));
			iLGenerator.Emit(OpCodes.Ret);
			return constructorBuilder;
		}

		public ConstructorBuilder DefineConstructor(MethodAttributes attribs, CallingConventions callConv, Type[] parameterTypes)
		{
			return DefineConstructor(attribs, callConv, parameterTypes, null, null);
		}

		public ConstructorBuilder DefineConstructor(MethodAttributes attribs, CallingConventions callingConvention, Type[] parameterTypes, Type[][] requiredCustomModifiers, Type[][] optionalCustomModifiers)
		{
			attribs |= MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
			string text = (((attribs & MethodAttributes.Static) == 0) ? ConstructorInfo.ConstructorName : ConstructorInfo.TypeConstructorName);
			return new ConstructorBuilder(DefineMethod(text, attribs, callingConvention, null, null, null, parameterTypes, requiredCustomModifiers, optionalCustomModifiers));
		}

		public ConstructorBuilder DefineTypeInitializer()
		{
			return new ConstructorBuilder(DefineMethod(ConstructorInfo.TypeConstructorName, MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, null, Type.EmptyTypes));
		}

		private MethodBuilder CreateMethodBuilder(string name, MethodAttributes attributes, CallingConventions callingConvention)
		{
			ModuleBuilder.MethodDef.AddVirtualRecord();
			MethodBuilder methodBuilder = new MethodBuilder(this, name, attributes, callingConvention);
			methods.Add(methodBuilder);
			return methodBuilder;
		}

		public MethodBuilder DefineMethod(string name, MethodAttributes attribs)
		{
			return DefineMethod(name, attribs, CallingConventions.Standard);
		}

		public MethodBuilder DefineMethod(string name, MethodAttributes attribs, CallingConventions callingConvention)
		{
			return CreateMethodBuilder(name, attribs, callingConvention);
		}

		public MethodBuilder DefineMethod(string name, MethodAttributes attribs, Type returnType, Type[] parameterTypes)
		{
			return DefineMethod(name, attribs, CallingConventions.Standard, returnType, null, null, parameterTypes, null, null);
		}

		public MethodBuilder DefineMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
		{
			return DefineMethod(name, attributes, callingConvention, returnType, null, null, parameterTypes, null, null);
		}

		public MethodBuilder DefineMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
		{
			MethodBuilder methodBuilder = CreateMethodBuilder(name, attributes, callingConvention);
			methodBuilder.SetSignature(returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers);
			return methodBuilder;
		}

		public MethodBuilder DefinePInvokeMethod(string name, string dllName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
		{
			return DefinePInvokeMethod(name, dllName, null, attributes, callingConvention, returnType, null, null, parameterTypes, null, null, nativeCallConv, nativeCharSet);
		}

		public MethodBuilder DefinePInvokeMethod(string name, string dllName, string entryName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
		{
			return DefinePInvokeMethod(name, dllName, entryName, attributes, callingConvention, returnType, null, null, parameterTypes, null, null, nativeCallConv, nativeCharSet);
		}

		public MethodBuilder DefinePInvokeMethod(string name, string dllName, string entryName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers, CallingConvention nativeCallConv, CharSet nativeCharSet)
		{
			MethodBuilder methodBuilder = DefineMethod(name, attributes | MethodAttributes.PinvokeImpl, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers);
			methodBuilder.SetDllImportPseudoCustomAttribute(dllName, entryName, nativeCallConv, nativeCharSet, null, null, null, null, null);
			return methodBuilder;
		}

		public void DefineMethodOverride(MethodInfo methodInfoBody, MethodInfo methodInfoDeclaration)
		{
			MethodImplTable.Record newRecord = new MethodImplTable.Record
			{
				Class = token,
				MethodBody = ModuleBuilder.GetMethodToken(methodInfoBody).Token,
				MethodDeclaration = ModuleBuilder.GetMethodTokenWinRT(methodInfoDeclaration)
			};
			ModuleBuilder.MethodImpl.AddRecord(newRecord);
		}

		public FieldBuilder DefineField(string name, Type fieldType, FieldAttributes attribs)
		{
			return DefineField(name, fieldType, null, null, attribs);
		}

		public FieldBuilder DefineField(string fieldName, Type type, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers, FieldAttributes attributes)
		{
			return __DefineField(fieldName, type, CustomModifiers.FromReqOpt(requiredCustomModifiers, optionalCustomModifiers), attributes);
		}

		public FieldBuilder __DefineField(string fieldName, Type type, CustomModifiers customModifiers, FieldAttributes attributes)
		{
			FieldBuilder fieldBuilder = new FieldBuilder(this, fieldName, type, customModifiers, attributes);
			fields.Add(fieldBuilder);
			return fieldBuilder;
		}

		public PropertyBuilder DefineProperty(string name, PropertyAttributes attributes, Type returnType, Type[] parameterTypes)
		{
			return DefineProperty(name, attributes, returnType, null, null, parameterTypes, null, null);
		}

		public PropertyBuilder DefineProperty(string name, PropertyAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
		{
			return DefineProperty(name, attributes, callingConvention, returnType, null, null, parameterTypes, null, null);
		}

		public PropertyBuilder DefineProperty(string name, PropertyAttributes attributes, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
		{
			return DefinePropertyImpl(name, attributes, CallingConventions.Standard, true, returnType, parameterTypes, PackedCustomModifiers.CreateFromExternal(returnTypeOptionalCustomModifiers, returnTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers, parameterTypeRequiredCustomModifiers, Util.NullSafeLength(parameterTypes)));
		}

		public PropertyBuilder DefineProperty(string name, PropertyAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
		{
			return DefinePropertyImpl(name, attributes, callingConvention, false, returnType, parameterTypes, PackedCustomModifiers.CreateFromExternal(returnTypeOptionalCustomModifiers, returnTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers, parameterTypeRequiredCustomModifiers, Util.NullSafeLength(parameterTypes)));
		}

		public PropertyBuilder __DefineProperty(string name, PropertyAttributes attributes, CallingConventions callingConvention, Type returnType, CustomModifiers returnTypeCustomModifiers, Type[] parameterTypes, CustomModifiers[] parameterTypeCustomModifiers)
		{
			return DefinePropertyImpl(name, attributes, callingConvention, false, returnType, parameterTypes, PackedCustomModifiers.CreateFromExternal(returnTypeCustomModifiers, parameterTypeCustomModifiers, Util.NullSafeLength(parameterTypes)));
		}

		private PropertyBuilder DefinePropertyImpl(string name, PropertyAttributes attributes, CallingConventions callingConvention, bool patchCallingConvention, Type returnType, Type[] parameterTypes, PackedCustomModifiers customModifiers)
		{
			if (properties == null)
			{
				properties = new List<PropertyBuilder>();
			}
			PropertySignature sig = PropertySignature.Create(callingConvention, returnType, parameterTypes, customModifiers);
			PropertyBuilder propertyBuilder = new PropertyBuilder(this, name, attributes, sig, patchCallingConvention);
			properties.Add(propertyBuilder);
			return propertyBuilder;
		}

		public EventBuilder DefineEvent(string name, EventAttributes attributes, Type eventtype)
		{
			if (events == null)
			{
				events = new List<EventBuilder>();
			}
			EventBuilder eventBuilder = new EventBuilder(this, name, attributes, eventtype);
			events.Add(eventBuilder);
			return eventBuilder;
		}

		public TypeBuilder DefineNestedType(string name)
		{
			return DefineNestedType(name, TypeAttributes.NestedPrivate);
		}

		public TypeBuilder DefineNestedType(string name, TypeAttributes attribs)
		{
			return DefineNestedType(name, attribs, null);
		}

		public TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent, Type[] interfaces)
		{
			TypeBuilder typeBuilder = DefineNestedType(name, attr, parent);
			if (interfaces != null)
			{
				foreach (Type interfaceType in interfaces)
				{
					typeBuilder.AddInterfaceImplementation(interfaceType);
				}
			}
			return typeBuilder;
		}

		public TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent)
		{
			return DefineNestedType(name, attr, parent, 0);
		}

		public TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent, int typeSize)
		{
			return DefineNestedType(name, attr, parent, PackingSize.Unspecified, typeSize);
		}

		public TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent, PackingSize packSize)
		{
			return DefineNestedType(name, attr, parent, packSize, 0);
		}

		public TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent, PackingSize packSize, int typeSize)
		{
			string text = null;
			int num = name.LastIndexOf('.');
			if (num > 0)
			{
				text = name.Substring(0, num);
				name = name.Substring(num + 1);
			}
			TypeBuilder typeBuilder = __DefineNestedType(text, name);
			typeBuilder.__SetAttributes(attr);
			typeBuilder.SetParent(parent);
			if (packSize != PackingSize.Unspecified || typeSize != 0)
			{
				typeBuilder.__SetLayout((int)packSize, typeSize);
			}
			return typeBuilder;
		}

		public TypeBuilder __DefineNestedType(string ns, string name)
		{
			typeFlags |= TypeFlags.HasNestedTypes;
			TypeBuilder typeBuilder = ModuleBuilder.DefineType(this, ns, name);
			NestedClassTable.Record newRecord = new NestedClassTable.Record
			{
				NestedClass = typeBuilder.MetadataToken,
				EnclosingClass = MetadataToken
			};
			ModuleBuilder.NestedClass.AddRecord(newRecord);
			return typeBuilder;
		}

		public void SetParent(Type parent)
		{
			lazyBaseType = parent;
		}

		public void AddInterfaceImplementation(Type interfaceType)
		{
			if (interfaces == null)
			{
				interfaces = new List<Type>();
			}
			interfaces.Add(interfaceType);
		}

		public void __SetInterfaceImplementationCustomAttribute(Type interfaceType, CustomAttributeBuilder cab)
		{
			ModuleBuilder.SetInterfaceImplementationCustomAttribute(this, interfaceType, cab);
		}

		public override bool __GetLayout(out int packingSize, out int typeSize)
		{
			packingSize = pack;
			typeSize = size;
			return hasLayout;
		}

		public void __SetLayout(int packingSize, int typesize)
		{
			pack = (short)packingSize;
			size = typesize;
			hasLayout = true;
		}

		private void SetStructLayoutPseudoCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			object constructorArgument = customBuilder.GetConstructorArgument(0);
			LayoutKind layoutKind = ((!(constructorArgument is short)) ? ((LayoutKind)constructorArgument) : ((LayoutKind)(short)constructorArgument));
			pack = (short)(((int?)customBuilder.GetFieldValue("Pack")) ?? 0);
			size = ((int?)customBuilder.GetFieldValue("Size")) ?? 0;
			CharSet charSet = customBuilder.GetFieldValue<CharSet>("CharSet") ?? CharSet.None;
			attribs &= ~TypeAttributes.LayoutMask;
			switch (layoutKind)
			{
			case LayoutKind.Auto:
				attribs |= TypeAttributes.AnsiClass;
				break;
			case LayoutKind.Explicit:
				attribs |= TypeAttributes.ExplicitLayout;
				break;
			case LayoutKind.Sequential:
				attribs |= TypeAttributes.SequentialLayout;
				break;
			}
			attribs &= ~TypeAttributes.CustomFormatClass;
			switch (charSet)
			{
			case CharSet.None:
			case CharSet.Ansi:
				attribs |= TypeAttributes.AnsiClass;
				break;
			case CharSet.Auto:
				attribs |= TypeAttributes.AutoClass;
				break;
			case CharSet.Unicode:
				attribs |= TypeAttributes.UnicodeClass;
				break;
			}
			hasLayout = pack != 0 || size != 0;
		}

		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			SetCustomAttribute(new CustomAttributeBuilder(con, binaryAttribute));
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			switch (customBuilder.KnownCA)
			{
			case KnownCA.StructLayoutAttribute:
				SetStructLayoutPseudoCustomAttribute(customBuilder.DecodeBlob(base.Assembly));
				return;
			case KnownCA.SerializableAttribute:
				attribs |= TypeAttributes.Serializable;
				return;
			case KnownCA.ComImportAttribute:
				attribs |= TypeAttributes.Import;
				return;
			case KnownCA.SpecialNameAttribute:
				attribs |= TypeAttributes.SpecialName;
				return;
			case KnownCA.SuppressUnmanagedCodeSecurityAttribute:
				attribs |= TypeAttributes.HasSecurity;
				break;
			}
			ModuleBuilder.SetCustomAttribute(token, customBuilder);
		}

		public void __AddDeclarativeSecurity(CustomAttributeBuilder customBuilder)
		{
			attribs |= TypeAttributes.HasSecurity;
			if (declarativeSecurity == null)
			{
				declarativeSecurity = new List<CustomAttributeBuilder>();
			}
			declarativeSecurity.Add(customBuilder);
		}

		public void AddDeclarativeSecurity(SecurityAction securityAction, PermissionSet permissionSet)
		{
			ModuleBuilder.AddDeclarativeSecurity(token, securityAction, permissionSet);
			attribs |= TypeAttributes.HasSecurity;
		}

		public GenericTypeParameterBuilder[] DefineGenericParameters(params string[] names)
		{
			typeFlags |= TypeFlags.IsGenericTypeDefinition;
			gtpb = new GenericTypeParameterBuilder[names.Length];
			for (int i = 0; i < names.Length; i++)
			{
				gtpb[i] = new GenericTypeParameterBuilder(names[i], this, i);
			}
			return (GenericTypeParameterBuilder[])gtpb.Clone();
		}

		public override Type[] GetGenericArguments()
		{
			return Util.Copy(gtpb);
		}

		public override CustomModifiers[] __GetGenericArgumentsCustomModifiers()
		{
			if (gtpb != null)
			{
				return new CustomModifiers[gtpb.Length];
			}
			return Empty<CustomModifiers>.Array;
		}

		internal override Type GetGenericTypeArgument(int index)
		{
			return gtpb[index];
		}

		public override Type GetGenericTypeDefinition()
		{
			return this;
		}

		public TypeInfo CreateTypeInfo()
		{
			if ((typeFlags & TypeFlags.Baked) != TypeFlags.ContainsMissingType_Unknown)
			{
				throw new NotImplementedException();
			}
			typeFlags |= TypeFlags.Baked;
			if (hasLayout)
			{
				ClassLayoutTable.Record newRecord = new ClassLayoutTable.Record
				{
					PackingSize = pack,
					ClassSize = size,
					Parent = token
				};
				ModuleBuilder.ClassLayout.AddRecord(newRecord);
			}
			bool flag = false;
			foreach (MethodBuilder method in methods)
			{
				flag |= method.IsSpecialName && method.Name == ConstructorInfo.ConstructorName;
				method.Bake();
			}
			if (!flag && !IsModulePseudoType && !base.IsInterface && !IsValueType && (!base.IsAbstract || !base.IsSealed) && Universe.AutomaticallyProvideDefaultConstructor)
			{
				((MethodBuilder)DefineDefaultConstructor(MethodAttributes.Public).GetMethodInfo()).Bake();
			}
			if (declarativeSecurity != null)
			{
				ModuleBuilder.AddDeclarativeSecurity(token, declarativeSecurity);
			}
			if (!IsModulePseudoType)
			{
				Type baseType = BaseType;
				if (baseType != null)
				{
					extends = ModuleBuilder.GetTypeToken(baseType).Token;
				}
			}
			if (interfaces != null)
			{
				foreach (Type @interface in interfaces)
				{
					InterfaceImplTable.Record newRecord2 = new InterfaceImplTable.Record
					{
						Class = token,
						Interface = ModuleBuilder.GetTypeToken(@interface).Token
					};
					ModuleBuilder.InterfaceImpl.AddRecord(newRecord2);
				}
			}
			return new BakedType(this);
		}

		public Type CreateType()
		{
			return CreateTypeInfo();
		}

		internal void PopulatePropertyAndEventTables()
		{
			if (properties != null)
			{
				PropertyMapTable.Record newRecord = new PropertyMapTable.Record
				{
					Parent = token,
					PropertyList = ModuleBuilder.Property.RowCount + 1
				};
				ModuleBuilder.PropertyMap.AddRecord(newRecord);
				foreach (PropertyBuilder property in properties)
				{
					property.Bake();
				}
			}
			if (events == null)
			{
				return;
			}
			EventMapTable.Record newRecord2 = new EventMapTable.Record
			{
				Parent = token,
				EventList = ModuleBuilder.Event.RowCount + 1
			};
			ModuleBuilder.EventMap.AddRecord(newRecord2);
			foreach (EventBuilder @event in events)
			{
				@event.Bake();
			}
		}

		public void __SetAttributes(TypeAttributes attributes)
		{
			attribs = attributes;
		}

		public override Type[] __GetDeclaredInterfaces()
		{
			return Util.ToArray(interfaces, Type.EmptyTypes);
		}

		public override MethodBase[] __GetDeclaredMethods()
		{
			MethodBase[] array = new MethodBase[methods.Count];
			for (int i = 0; i < array.Length; i++)
			{
				MethodBuilder methodBuilder = methods[i];
				if (methodBuilder.IsConstructor)
				{
					array[i] = new ConstructorInfoImpl(methodBuilder);
				}
				else
				{
					array[i] = methodBuilder;
				}
			}
			return array;
		}

		public FieldBuilder DefineUninitializedData(string name, int size, FieldAttributes attributes)
		{
			return DefineInitializedData(name, new byte[size], attributes);
		}

		public FieldBuilder DefineInitializedData(string name, byte[] data, FieldAttributes attributes)
		{
			Type type = ModuleBuilder.GetType("$ArrayType$" + data.Length);
			if (type == null)
			{
				TypeBuilder typeBuilder = ModuleBuilder.DefineType("$ArrayType$" + data.Length, TypeAttributes.Public | TypeAttributes.ExplicitLayout | TypeAttributes.Sealed, Module.universe.System_ValueType, PackingSize.Size1, data.Length);
				typeBuilder.CreateType();
				type = typeBuilder;
			}
			FieldBuilder fieldBuilder = DefineField(name, type, attributes | FieldAttributes.Static);
			fieldBuilder.__SetDataAndRVA(data);
			return fieldBuilder;
		}

		public static MethodInfo GetMethod(Type type, MethodInfo method)
		{
			return new GenericMethodInstance(type, method, null);
		}

		public static ConstructorInfo GetConstructor(Type type, ConstructorInfo constructor)
		{
			return new ConstructorInfoImpl(GetMethod(type, constructor.GetMethodInfo()));
		}

		public static FieldInfo GetField(Type type, FieldInfo field)
		{
			return new GenericFieldInstance(type, field);
		}

		internal void WriteTypeDefRecord(MetadataWriter mw, ref int fieldList, ref int methodList)
		{
			mw.Write((int)attribs);
			mw.WriteStringIndex(typeName);
			mw.WriteStringIndex(typeNameSpace);
			mw.WriteTypeDefOrRef(extends);
			mw.WriteField(fieldList);
			mw.WriteMethodDef(methodList);
			methodList += methods.Count;
			fieldList += fields.Count;
		}

		internal void WriteMethodDefRecords(int baseRVA, MetadataWriter mw, ref int paramList)
		{
			foreach (MethodBuilder method in methods)
			{
				method.WriteMethodDefRecord(baseRVA, mw, ref paramList);
			}
		}

		internal void ResolveMethodAndFieldTokens(ref int methodToken, ref int fieldToken, ref int parameterToken)
		{
			foreach (MethodBuilder method in methods)
			{
				method.FixupToken(methodToken++, ref parameterToken);
			}
			foreach (FieldBuilder field in fields)
			{
				field.FixupToken(fieldToken++);
			}
		}

		internal void WriteParamRecords(MetadataWriter mw)
		{
			foreach (MethodBuilder method in methods)
			{
				method.WriteParamRecords(mw);
			}
		}

		internal void WriteFieldRecords(MetadataWriter mw)
		{
			foreach (FieldBuilder field in fields)
			{
				field.WriteFieldRecords(mw);
			}
		}

		internal override int GetModuleBuilderToken()
		{
			return token;
		}

		internal MethodBase LookupMethod(int token)
		{
			foreach (MethodBuilder method in methods)
			{
				if (method.MetadataToken == token)
				{
					return method;
				}
			}
			return null;
		}

		public bool IsCreated()
		{
			return (typeFlags & TypeFlags.Baked) != 0;
		}

		internal override void CheckBaked()
		{
			if ((typeFlags & TypeFlags.Baked) == 0)
			{
				throw new NotSupportedException();
			}
		}

		public override Type[] __GetDeclaredTypes()
		{
			if (HasNestedTypes)
			{
				List<Type> list = new List<Type>();
				foreach (int nestedClass in ModuleBuilder.NestedClass.GetNestedClasses(token))
				{
					list.Add(ModuleBuilder.ResolveType(nestedClass));
				}
				return list.ToArray();
			}
			return Type.EmptyTypes;
		}

		public override FieldInfo[] __GetDeclaredFields()
		{
			return Util.ToArray(fields, Empty<FieldInfo>.Array);
		}

		public override EventInfo[] __GetDeclaredEvents()
		{
			return Util.ToArray(events, Empty<EventInfo>.Array);
		}

		public override PropertyInfo[] __GetDeclaredProperties()
		{
			return Util.ToArray(properties, Empty<PropertyInfo>.Array);
		}
	}
}
