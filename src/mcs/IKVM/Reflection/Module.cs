using System;
using System.Collections.Generic;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Metadata;
using IKVM.Reflection.Reader;

namespace IKVM.Reflection
{
	public abstract class Module : ICustomAttributeProvider
	{
		internal sealed class GenericContext : IGenericContext
		{
			private readonly Type[] genericTypeArguments;

			private readonly Type[] genericMethodArguments;

			internal GenericContext(Type[] genericTypeArguments, Type[] genericMethodArguments)
			{
				this.genericTypeArguments = genericTypeArguments;
				this.genericMethodArguments = genericMethodArguments;
			}

			public Type GetGenericTypeArgument(int index)
			{
				return genericTypeArguments[index];
			}

			public Type GetGenericMethodArgument(int index)
			{
				return genericMethodArguments[index];
			}
		}

		internal readonly Universe universe;

		internal readonly ModuleTable ModuleTable = new ModuleTable();

		internal readonly TypeRefTable TypeRef = new TypeRefTable();

		internal readonly TypeDefTable TypeDef = new TypeDefTable();

		internal readonly FieldPtrTable FieldPtr = new FieldPtrTable();

		internal readonly FieldTable Field = new FieldTable();

		internal readonly MemberRefTable MemberRef = new MemberRefTable();

		internal readonly ConstantTable Constant = new ConstantTable();

		internal readonly CustomAttributeTable CustomAttribute = new CustomAttributeTable();

		internal readonly FieldMarshalTable FieldMarshal = new FieldMarshalTable();

		internal readonly DeclSecurityTable DeclSecurity = new DeclSecurityTable();

		internal readonly ClassLayoutTable ClassLayout = new ClassLayoutTable();

		internal readonly FieldLayoutTable FieldLayout = new FieldLayoutTable();

		internal readonly ParamPtrTable ParamPtr = new ParamPtrTable();

		internal readonly ParamTable Param = new ParamTable();

		internal readonly InterfaceImplTable InterfaceImpl = new InterfaceImplTable();

		internal readonly StandAloneSigTable StandAloneSig = new StandAloneSigTable();

		internal readonly EventMapTable EventMap = new EventMapTable();

		internal readonly EventPtrTable EventPtr = new EventPtrTable();

		internal readonly EventTable Event = new EventTable();

		internal readonly PropertyMapTable PropertyMap = new PropertyMapTable();

		internal readonly PropertyPtrTable PropertyPtr = new PropertyPtrTable();

		internal readonly PropertyTable Property = new PropertyTable();

		internal readonly MethodSemanticsTable MethodSemantics = new MethodSemanticsTable();

		internal readonly MethodImplTable MethodImpl = new MethodImplTable();

		internal readonly ModuleRefTable ModuleRef = new ModuleRefTable();

		internal readonly TypeSpecTable TypeSpec = new TypeSpecTable();

		internal readonly ImplMapTable ImplMap = new ImplMapTable();

		internal readonly FieldRVATable FieldRVA = new FieldRVATable();

		internal readonly AssemblyTable AssemblyTable = new AssemblyTable();

		internal readonly AssemblyRefTable AssemblyRef = new AssemblyRefTable();

		internal readonly MethodPtrTable MethodPtr = new MethodPtrTable();

		internal readonly MethodDefTable MethodDef = new MethodDefTable();

		internal readonly NestedClassTable NestedClass = new NestedClassTable();

		internal readonly FileTable File = new FileTable();

		internal readonly ExportedTypeTable ExportedType = new ExportedTypeTable();

		internal readonly ManifestResourceTable ManifestResource = new ManifestResourceTable();

		internal readonly GenericParamTable GenericParam = new GenericParamTable();

		internal readonly MethodSpecTable MethodSpec = new MethodSpecTable();

		internal readonly GenericParamConstraintTable GenericParamConstraint = new GenericParamConstraintTable();

		public virtual int __Subsystem
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public ConstructorInfo __ModuleInitializer
		{
			get
			{
				if (!IsResource())
				{
					return GetModuleType().TypeInitializer;
				}
				return null;
			}
		}

		public int MetadataToken
		{
			get
			{
				if (!IsResource())
				{
					return 1;
				}
				return 0;
			}
		}

		public abstract int MDStreamVersion { get; }

		public abstract Assembly Assembly { get; }

		public abstract string FullyQualifiedName { get; }

		public abstract string Name { get; }

		public abstract Guid ModuleVersionId { get; }

		public abstract string ScopeName { get; }

		public IEnumerable<CustomAttributeData> CustomAttributes => GetCustomAttributesData();

		public virtual bool __IsMissing => false;

		public long __ImageBase => GetImageBaseImpl();

		public long __StackReserve => GetStackReserveImpl();

		public int __FileAlignment => GetFileAlignmentImpl();

		public DllCharacteristics __DllCharacteristics => GetDllCharacteristicsImpl();

		public virtual byte[] __ModuleHash
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public virtual int __EntryPointRVA
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public virtual int __EntryPointToken
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public virtual string __ImageRuntimeVersion
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		protected Module(Universe universe)
		{
			this.universe = universe;
		}

		internal Table[] GetTables()
		{
			Table[] array = new Table[64];
			array[0] = ModuleTable;
			array[1] = TypeRef;
			array[2] = TypeDef;
			array[3] = FieldPtr;
			array[4] = Field;
			array[10] = MemberRef;
			array[11] = Constant;
			array[12] = CustomAttribute;
			array[13] = FieldMarshal;
			array[14] = DeclSecurity;
			array[15] = ClassLayout;
			array[16] = FieldLayout;
			array[7] = ParamPtr;
			array[8] = Param;
			array[9] = InterfaceImpl;
			array[17] = StandAloneSig;
			array[18] = EventMap;
			array[19] = EventPtr;
			array[20] = Event;
			array[21] = PropertyMap;
			array[22] = PropertyPtr;
			array[23] = Property;
			array[24] = MethodSemantics;
			array[25] = MethodImpl;
			array[26] = ModuleRef;
			array[27] = TypeSpec;
			array[28] = ImplMap;
			array[29] = FieldRVA;
			array[32] = AssemblyTable;
			array[35] = AssemblyRef;
			array[5] = MethodPtr;
			array[6] = MethodDef;
			array[41] = NestedClass;
			array[38] = File;
			array[39] = ExportedType;
			array[40] = ManifestResource;
			array[42] = GenericParam;
			array[43] = MethodSpec;
			array[44] = GenericParamConstraint;
			return array;
		}

		public virtual void __GetDataDirectoryEntry(int index, out int rva, out int length)
		{
			throw new NotSupportedException();
		}

		public virtual long __RelativeVirtualAddressToFileOffset(int rva)
		{
			throw new NotSupportedException();
		}

		public bool __GetSectionInfo(int rva, out string name, out int characteristics)
		{
			int virtualAddress;
			int virtualSize;
			int pointerToRawData;
			int sizeOfRawData;
			return __GetSectionInfo(rva, out name, out characteristics, out virtualAddress, out virtualSize, out pointerToRawData, out sizeOfRawData);
		}

		public virtual bool __GetSectionInfo(int rva, out string name, out int characteristics, out int virtualAddress, out int virtualSize, out int pointerToRawData, out int sizeOfRawData)
		{
			throw new NotSupportedException();
		}

		public virtual int __ReadDataFromRVA(int rva, byte[] data, int offset, int length)
		{
			throw new NotSupportedException();
		}

		public virtual void GetPEKind(out PortableExecutableKinds peKind, out ImageFileMachine machine)
		{
			throw new NotSupportedException();
		}

		public FieldInfo GetField(string name)
		{
			return GetField(name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public FieldInfo GetField(string name, BindingFlags bindingFlags)
		{
			if (!IsResource())
			{
				return GetModuleType().GetField(name, bindingFlags | BindingFlags.DeclaredOnly);
			}
			return null;
		}

		public FieldInfo[] GetFields()
		{
			return GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public FieldInfo[] GetFields(BindingFlags bindingFlags)
		{
			if (!IsResource())
			{
				return GetModuleType().GetFields(bindingFlags | BindingFlags.DeclaredOnly);
			}
			return Empty<FieldInfo>.Array;
		}

		public MethodInfo GetMethod(string name)
		{
			if (!IsResource())
			{
				return GetModuleType().GetMethod(name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
			}
			return null;
		}

		public MethodInfo GetMethod(string name, Type[] types)
		{
			if (!IsResource())
			{
				return GetModuleType().GetMethod(name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, types, null);
			}
			return null;
		}

		public MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConv, Type[] types, ParameterModifier[] modifiers)
		{
			if (!IsResource())
			{
				return GetModuleType().GetMethod(name, bindingAttr | BindingFlags.DeclaredOnly, binder, callConv, types, modifiers);
			}
			return null;
		}

		public MethodInfo[] GetMethods()
		{
			return GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public MethodInfo[] GetMethods(BindingFlags bindingFlags)
		{
			if (!IsResource())
			{
				return GetModuleType().GetMethods(bindingFlags | BindingFlags.DeclaredOnly);
			}
			return Empty<MethodInfo>.Array;
		}

		public virtual byte[] ResolveSignature(int metadataToken)
		{
			throw new NotSupportedException();
		}

		public virtual __StandAloneMethodSig __ResolveStandAloneMethodSig(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw new NotSupportedException();
		}

		public virtual CustomModifiers __ResolveTypeSpecCustomModifiers(int typeSpecToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw new NotSupportedException();
		}

		public abstract MethodBase ResolveMethod(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments);

		public abstract FieldInfo ResolveField(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments);

		public abstract MemberInfo ResolveMember(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments);

		public abstract string ResolveString(int metadataToken);

		public abstract Type[] __ResolveOptionalParameterTypes(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments, out CustomModifiers[] customModifiers);

		internal abstract void GetTypesImpl(List<Type> list);

		internal abstract Type FindType(TypeName name);

		internal abstract Type FindTypeIgnoreCase(TypeName lowerCaseName);

		[Obsolete("Please use __ResolveOptionalParameterTypes(int, Type[], Type[], out CustomModifiers[]) instead.")]
		public Type[] __ResolveOptionalParameterTypes(int metadataToken)
		{
			CustomModifiers[] customModifiers;
			return __ResolveOptionalParameterTypes(metadataToken, null, null, out customModifiers);
		}

		public Type GetType(string className)
		{
			return GetType(className, false, false);
		}

		public Type GetType(string className, bool ignoreCase)
		{
			return GetType(className, false, ignoreCase);
		}

		public Type GetType(string className, bool throwOnError, bool ignoreCase)
		{
			TypeNameParser typeNameParser = TypeNameParser.Parse(className, throwOnError);
			if (typeNameParser.Error)
			{
				return null;
			}
			if (typeNameParser.AssemblyName != null)
			{
				if (throwOnError)
				{
					throw new ArgumentException("Type names passed to Module.GetType() must not specify an assembly.");
				}
				return null;
			}
			TypeName name = TypeName.Split(TypeNameParser.Unescape(typeNameParser.FirstNamePart));
			Type type = (ignoreCase ? FindTypeIgnoreCase(name.ToLowerInvariant()) : FindType(name));
			if (type == null && __IsMissing)
			{
				throw new MissingModuleException((MissingModule)this);
			}
			return typeNameParser.Expand(type, this, throwOnError, className, false, ignoreCase);
		}

		public Type[] GetTypes()
		{
			List<Type> list = new List<Type>();
			GetTypesImpl(list);
			return list.ToArray();
		}

		public Type[] FindTypes(TypeFilter filter, object filterCriteria)
		{
			List<Type> list = new List<Type>();
			Type[] types = GetTypes();
			foreach (Type type in types)
			{
				if (filter(type, filterCriteria))
				{
					list.Add(type);
				}
			}
			return list.ToArray();
		}

		public virtual bool IsResource()
		{
			return false;
		}

		public Type ResolveType(int metadataToken)
		{
			return ResolveType(metadataToken, null, null);
		}

		public Type ResolveType(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			if (metadataToken >> 24 == 27)
			{
				return ResolveType(metadataToken, new GenericContext(genericTypeArguments, genericMethodArguments));
			}
			return ResolveType(metadataToken, null);
		}

		internal abstract Type ResolveType(int metadataToken, IGenericContext context);

		public MethodBase ResolveMethod(int metadataToken)
		{
			return ResolveMethod(metadataToken, null, null);
		}

		public FieldInfo ResolveField(int metadataToken)
		{
			return ResolveField(metadataToken, null, null);
		}

		public MemberInfo ResolveMember(int metadataToken)
		{
			return ResolveMember(metadataToken, null, null);
		}

		public bool IsDefined(Type attributeType, bool inherit)
		{
			return CustomAttributeData.__GetCustomAttributes(this, attributeType, inherit).Count != 0;
		}

		public IList<CustomAttributeData> __GetCustomAttributes(Type attributeType, bool inherit)
		{
			return CustomAttributeData.__GetCustomAttributes(this, attributeType, inherit);
		}

		public IList<CustomAttributeData> GetCustomAttributesData()
		{
			return CustomAttributeData.GetCustomAttributes(this);
		}

		public virtual IList<CustomAttributeData> __GetPlaceholderAssemblyCustomAttributes(bool multiple, bool security)
		{
			return Empty<CustomAttributeData>.Array;
		}

		public abstract AssemblyName[] __GetReferencedAssemblies();

		public virtual void __ResolveReferencedAssemblies(Assembly[] assemblies)
		{
			throw new NotSupportedException();
		}

		public abstract string[] __GetReferencedModules();

		public abstract Type[] __GetReferencedTypes();

		public abstract Type[] __GetExportedTypes();

		protected abstract long GetImageBaseImpl();

		protected abstract long GetStackReserveImpl();

		protected abstract int GetFileAlignmentImpl();

		protected abstract DllCharacteristics GetDllCharacteristicsImpl();

		public IEnumerable<CustomAttributeData> __EnumerateCustomAttributeTable()
		{
			List<CustomAttributeData> list = new List<CustomAttributeData>(CustomAttribute.RowCount);
			for (int i = 0; i < CustomAttribute.RowCount; i++)
			{
				list.Add(new CustomAttributeData(this, i));
			}
			return list;
		}

		[Obsolete]
		public List<CustomAttributeData> __GetCustomAttributesFor(int token)
		{
			return CustomAttributeData.GetCustomAttributesImpl(new List<CustomAttributeData>(), this, token, null);
		}

		public bool __TryGetImplMap(int token, out ImplMapFlags mappingFlags, out string importName, out string importScope)
		{
			SortedTable<ImplMapTable.Record>.Enumerator enumerator = ImplMap.Filter(token).GetEnumerator();
			if (enumerator.MoveNext())
			{
				int current = enumerator.Current;
				mappingFlags = (ImplMapFlags)(ushort)ImplMap.records[current].MappingFlags;
				importName = GetString(ImplMap.records[current].ImportName);
				importScope = GetString(ModuleRef.records[(ImplMap.records[current].ImportScope & 0xFFFFFF) - 1]);
				return true;
			}
			mappingFlags = ImplMapFlags.CharSetNotSpec;
			importName = null;
			importScope = null;
			return false;
		}

		internal abstract Type GetModuleType();

		internal abstract ByteReader GetBlob(int blobIndex);

		internal IList<CustomAttributeData> GetDeclarativeSecurity(int metadataToken)
		{
			List<CustomAttributeData> list = new List<CustomAttributeData>();
			foreach (int item in DeclSecurity.Filter(metadataToken))
			{
				CustomAttributeData.ReadDeclarativeSecurity(this, item, list);
			}
			return list;
		}

		internal virtual void Dispose()
		{
		}

		internal virtual void ExportTypes(int fileToken, ModuleBuilder manifestModule)
		{
		}

		internal virtual string GetString(int index)
		{
			throw new NotSupportedException();
		}
	}
}
