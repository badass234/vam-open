using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using IKVM.Reflection.Impl;
using IKVM.Reflection.Metadata;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Emit
{
	public sealed class ModuleBuilder : Module, ITypeOwner
	{
		private struct ResourceWriterRecord
		{
			private readonly string name;

			private readonly ResourceWriter rw;

			private readonly Stream stream;

			private readonly ResourceAttributes attributes;

			internal ResourceWriterRecord(string name, Stream stream, ResourceAttributes attributes)
				: this(name, null, stream, attributes)
			{
			}

			internal ResourceWriterRecord(string name, ResourceWriter rw, Stream stream, ResourceAttributes attributes)
			{
				this.name = name;
				this.rw = rw;
				this.stream = stream;
				this.attributes = attributes;
			}

			internal void Emit(ModuleBuilder mb, int offset)
			{
				if (rw != null)
				{
					rw.Generate();
				}
				ManifestResourceTable.Record newRecord = new ManifestResourceTable.Record
				{
					Offset = offset,
					Flags = (int)attributes,
					Name = mb.Strings.Add(name),
					Implementation = 0
				};
				mb.ManifestResource.AddRecord(newRecord);
			}

			internal int GetLength()
			{
				return 4 + (int)stream.Length;
			}

			internal void Write(MetadataWriter mw)
			{
				mw.Write((int)stream.Length);
				stream.Position = 0L;
				byte[] array = new byte[8192];
				int count;
				while ((count = stream.Read(array, 0, array.Length)) != 0)
				{
					mw.Write(array, 0, count);
				}
			}

			internal void Close()
			{
				if (rw != null)
				{
					rw.Close();
				}
			}
		}

		internal struct VTableFixups
		{
			internal uint initializedDataOffset;

			internal ushort count;

			internal ushort type;

			internal int SlotWidth
			{
				get
				{
					if ((type & 2) != 0)
					{
						return 8;
					}
					return 4;
				}
			}
		}

		private struct InterfaceImplCustomAttribute
		{
			internal int type;

			internal int interfaceType;

			internal int pseudoToken;
		}

		private struct MemberRefKey : IEquatable<MemberRefKey>
		{
			private readonly Type type;

			private readonly string name;

			private readonly Signature signature;

			internal MemberRefKey(Type type, string name, Signature signature)
			{
				this.type = type;
				this.name = name;
				this.signature = signature;
			}

			public bool Equals(MemberRefKey other)
			{
				if (other.type.Equals(type) && other.name == name)
				{
					return other.signature.Equals(signature);
				}
				return false;
			}

			public override bool Equals(object obj)
			{
				MemberRefKey? memberRefKey = obj as MemberRefKey?;
				if (memberRefKey.HasValue)
				{
					return Equals(memberRefKey.Value);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return type.GetHashCode() + name.GetHashCode() + signature.GetHashCode();
			}

			internal MethodBase LookupMethod()
			{
				return type.FindMethod(name, (MethodSignature)signature);
			}
		}

		private struct MethodSpecKey : IEquatable<MethodSpecKey>
		{
			private readonly Type type;

			private readonly string name;

			private readonly MethodSignature signature;

			private readonly Type[] genericParameters;

			internal MethodSpecKey(Type type, string name, MethodSignature signature, Type[] genericParameters)
			{
				this.type = type;
				this.name = name;
				this.signature = signature;
				this.genericParameters = genericParameters;
			}

			public bool Equals(MethodSpecKey other)
			{
				if (other.type.Equals(type) && other.name == name && other.signature.Equals(signature))
				{
					return Util.ArrayEquals(other.genericParameters, genericParameters);
				}
				return false;
			}

			public override bool Equals(object obj)
			{
				MethodSpecKey? methodSpecKey = obj as MethodSpecKey?;
				if (methodSpecKey.HasValue)
				{
					return Equals(methodSpecKey.Value);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return type.GetHashCode() + name.GetHashCode() + signature.GetHashCode() + Util.GetHashCode(genericParameters);
			}
		}

		private static readonly bool usePublicKeyAssemblyReference;

		private Guid mvid;

		private uint timestamp;

		private long imageBaseAddress = 4194304L;

		private long stackReserve = -1L;

		private int fileAlignment = 512;

		private DllCharacteristics dllCharacteristics = DllCharacteristics.DynamicBase | DllCharacteristics.NoSEH | DllCharacteristics.NXCompat | DllCharacteristics.TerminalServerAware;

		private readonly AssemblyBuilder asm;

		internal readonly string moduleName;

		internal readonly string fileName;

		internal readonly ISymbolWriterImpl symbolWriter;

		private readonly TypeBuilder moduleType;

		private readonly List<TypeBuilder> types = new List<TypeBuilder>();

		private readonly Dictionary<Type, int> typeTokens = new Dictionary<Type, int>();

		private readonly Dictionary<Type, int> memberRefTypeTokens = new Dictionary<Type, int>();

		internal readonly ByteBuffer methodBodies = new ByteBuffer(131072);

		internal readonly List<int> tokenFixupOffsets = new List<int>();

		internal readonly ByteBuffer initializedData = new ByteBuffer(512);

		internal ResourceSection unmanagedResources;

		private readonly Dictionary<MemberRefKey, int> importedMemberRefs = new Dictionary<MemberRefKey, int>();

		private readonly Dictionary<MethodSpecKey, int> importedMethodSpecs = new Dictionary<MethodSpecKey, int>();

		private readonly Dictionary<Assembly, int> referencedAssemblies = new Dictionary<Assembly, int>();

		private List<AssemblyName> referencedAssemblyNames;

		private int nextPseudoToken = -1;

		private readonly List<int> resolvedTokens = new List<int>();

		internal readonly TableHeap Tables = new TableHeap();

		internal readonly StringHeap Strings = new StringHeap();

		internal readonly UserStringHeap UserStrings = new UserStringHeap();

		internal readonly GuidHeap Guids = new GuidHeap();

		internal readonly BlobHeap Blobs = new BlobHeap();

		internal readonly List<VTableFixups> vtablefixups = new List<VTableFixups>();

		internal readonly List<UnmanagedExport> unmanagedExports = new List<UnmanagedExport>();

		private List<InterfaceImplCustomAttribute> interfaceImplCustomAttributes;

		private readonly List<ResourceWriterRecord> resourceWriters = new List<ResourceWriterRecord>();

		private bool saved;

		public override Assembly Assembly => asm;

		internal int MetadataLength => GetHeaderLength() + ((!Blobs.IsEmpty) ? Blobs.Length : 0) + Tables.Length + Strings.Length + UserStrings.Length + Guids.Length;

		ModuleBuilder ITypeOwner.ModuleBuilder => this;

		public override string FullyQualifiedName => Path.GetFullPath(Path.Combine(asm.dir, fileName));

		public override string Name => fileName;

		public override Guid ModuleVersionId
		{
			get
			{
				if (mvid == Guid.Empty && universe.Deterministic)
				{
					throw new InvalidOperationException();
				}
				return mvid;
			}
		}

		public DateTime __PEHeaderTimeDateStamp
		{
			get
			{
				return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
			}
			set
			{
				if (value < new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) || value > new DateTime(2106, 2, 7, 6, 28, 15, DateTimeKind.Utc))
				{
					throw new ArgumentOutOfRangeException();
				}
				timestamp = (uint)(value - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
			}
		}

		public override string ScopeName => moduleName;

		public new long __ImageBase
		{
			get
			{
				return imageBaseAddress;
			}
			set
			{
				imageBaseAddress = value;
			}
		}

		public new long __StackReserve
		{
			get
			{
				return stackReserve;
			}
			set
			{
				stackReserve = value;
			}
		}

		public new int __FileAlignment
		{
			get
			{
				return fileAlignment;
			}
			set
			{
				fileAlignment = value;
			}
		}

		public new DllCharacteristics __DllCharacteristics
		{
			get
			{
				return dllCharacteristics;
			}
			set
			{
				dllCharacteristics = value;
			}
		}

		public override int MDStreamVersion => asm.mdStreamVersion;

		internal bool IsSaved => saved;

		internal ModuleBuilder(AssemblyBuilder asm, string moduleName, string fileName, bool emitSymbolInfo)
			: base(asm.universe)
		{
			this.asm = asm;
			this.moduleName = moduleName;
			this.fileName = fileName;
			if (emitSymbolInfo)
			{
				symbolWriter = SymbolSupport.CreateSymbolWriterFor(this);
				if (universe.Deterministic && !symbolWriter.IsDeterministic)
				{
					throw new NotSupportedException();
				}
			}
			if (!universe.Deterministic)
			{
				__PEHeaderTimeDateStamp = DateTime.UtcNow;
				mvid = Guid.NewGuid();
			}
			moduleType = new TypeBuilder(this, null, "<Module>");
			types.Add(moduleType);
		}

		internal void PopulatePropertyAndEventTables()
		{
			foreach (TypeBuilder type in types)
			{
				type.PopulatePropertyAndEventTables();
			}
		}

		internal void WriteTypeDefTable(MetadataWriter mw)
		{
			int fieldList = 1;
			int methodList = 1;
			foreach (TypeBuilder type in types)
			{
				type.WriteTypeDefRecord(mw, ref fieldList, ref methodList);
			}
		}

		internal void WriteMethodDefTable(int baseRVA, MetadataWriter mw)
		{
			int paramList = 1;
			foreach (TypeBuilder type in types)
			{
				type.WriteMethodDefRecords(baseRVA, mw, ref paramList);
			}
		}

		internal void WriteParamTable(MetadataWriter mw)
		{
			foreach (TypeBuilder type in types)
			{
				type.WriteParamRecords(mw);
			}
		}

		internal void WriteFieldTable(MetadataWriter mw)
		{
			foreach (TypeBuilder type in types)
			{
				type.WriteFieldRecords(mw);
			}
		}

		internal int AllocPseudoToken()
		{
			return nextPseudoToken--;
		}

		public TypeBuilder DefineType(string name)
		{
			return DefineType(name, TypeAttributes.AnsiClass);
		}

		public TypeBuilder DefineType(string name, TypeAttributes attr)
		{
			return DefineType(name, attr, null);
		}

		public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent)
		{
			return DefineType(name, attr, parent, PackingSize.Unspecified, 0);
		}

		public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, int typesize)
		{
			return DefineType(name, attr, parent, PackingSize.Unspecified, typesize);
		}

		public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, PackingSize packsize)
		{
			return DefineType(name, attr, parent, packsize, 0);
		}

		public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, Type[] interfaces)
		{
			TypeBuilder typeBuilder = DefineType(name, attr, parent);
			foreach (Type interfaceType in interfaces)
			{
				typeBuilder.AddInterfaceImplementation(interfaceType);
			}
			return typeBuilder;
		}

		public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, PackingSize packingSize, int typesize)
		{
			string ns = null;
			int num = name.LastIndexOf('.');
			if (num > 0)
			{
				ns = name.Substring(0, num);
				name = name.Substring(num + 1);
			}
			TypeBuilder typeBuilder = __DefineType(ns, name);
			typeBuilder.__SetAttributes(attr);
			typeBuilder.SetParent(parent);
			if (packingSize != PackingSize.Unspecified || typesize != 0)
			{
				typeBuilder.__SetLayout((int)packingSize, typesize);
			}
			return typeBuilder;
		}

		public TypeBuilder __DefineType(string ns, string name)
		{
			return DefineType(this, ns, name);
		}

		internal TypeBuilder DefineType(ITypeOwner owner, string ns, string name)
		{
			TypeBuilder typeBuilder = new TypeBuilder(owner, ns, name);
			types.Add(typeBuilder);
			return typeBuilder;
		}

		public EnumBuilder DefineEnum(string name, TypeAttributes visibility, Type underlyingType)
		{
			TypeBuilder typeBuilder = DefineType(name, (visibility & TypeAttributes.VisibilityMask) | TypeAttributes.Sealed, universe.System_Enum);
			FieldBuilder fieldBuilder = typeBuilder.DefineField("value__", underlyingType, FieldAttributes.Public | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName);
			return new EnumBuilder(typeBuilder, fieldBuilder);
		}

		public FieldBuilder __DefineField(string name, Type type, CustomModifiers customModifiers, FieldAttributes attributes)
		{
			return moduleType.__DefineField(name, type, customModifiers, attributes);
		}

		[Obsolete("Please use __DefineField(string, Type, CustomModifiers, FieldAttributes) instead.")]
		public FieldBuilder __DefineField(string name, Type type, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers, FieldAttributes attributes)
		{
			return moduleType.DefineField(name, type, requiredCustomModifiers, optionalCustomModifiers, attributes);
		}

		public ConstructorBuilder __DefineModuleInitializer(MethodAttributes visibility)
		{
			return moduleType.DefineConstructor(visibility | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, Type.EmptyTypes);
		}

		public FieldBuilder DefineUninitializedData(string name, int size, FieldAttributes attributes)
		{
			return moduleType.DefineUninitializedData(name, size, attributes);
		}

		public FieldBuilder DefineInitializedData(string name, byte[] data, FieldAttributes attributes)
		{
			return moduleType.DefineInitializedData(name, data, attributes);
		}

		public MethodBuilder DefineGlobalMethod(string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes)
		{
			return moduleType.DefineMethod(name, attributes, returnType, parameterTypes);
		}

		public MethodBuilder DefineGlobalMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
		{
			return moduleType.DefineMethod(name, attributes, callingConvention, returnType, parameterTypes);
		}

		public MethodBuilder DefineGlobalMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
		{
			return moduleType.DefineMethod(name, attributes, callingConvention, returnType, requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers, parameterTypes, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers);
		}

		public MethodBuilder DefinePInvokeMethod(string name, string dllName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
		{
			return moduleType.DefinePInvokeMethod(name, dllName, attributes, callingConvention, returnType, parameterTypes, nativeCallConv, nativeCharSet);
		}

		public MethodBuilder DefinePInvokeMethod(string name, string dllName, string entryName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
		{
			return moduleType.DefinePInvokeMethod(name, dllName, entryName, attributes, callingConvention, returnType, parameterTypes, nativeCallConv, nativeCharSet);
		}

		public void CreateGlobalFunctions()
		{
			moduleType.CreateType();
		}

		internal void AddTypeForwarder(Type type, bool includeNested)
		{
			ExportType(type);
			if (includeNested && !type.__IsMissing)
			{
				Type[] nestedTypes = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
				foreach (Type type2 in nestedTypes)
				{
					AddTypeForwarder(type2, true);
				}
			}
		}

		private int ExportType(Type type)
		{
			ExportedTypeTable.Record rec = default(ExportedTypeTable.Record);
			if (asm.ImageRuntimeVersion == "v2.0.50727")
			{
				rec.TypeDefId = type.MetadataToken;
			}
			SetTypeNameAndTypeNamespace(type.TypeName, out rec.TypeName, out rec.TypeNamespace);
			if (type.IsNested)
			{
				rec.Flags = 0;
				rec.Implementation = ExportType(type.DeclaringType);
			}
			else
			{
				rec.Flags = 2097152;
				rec.Implementation = ImportAssemblyRef(type.Assembly);
			}
			return 0x27000000 | ExportedType.FindOrAddRecord(rec);
		}

		private void SetTypeNameAndTypeNamespace(TypeName name, out int typeName, out int typeNamespace)
		{
			typeName = Strings.Add(name.Name);
			typeNamespace = ((name.Namespace != null) ? Strings.Add(name.Namespace) : 0);
		}

		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			SetCustomAttribute(new CustomAttributeBuilder(con, binaryAttribute));
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			SetCustomAttribute(1, customBuilder);
		}

		internal void SetCustomAttribute(int token, CustomAttributeBuilder customBuilder)
		{
			CustomAttributeTable.Record newRecord = new CustomAttributeTable.Record
			{
				Parent = token,
				Type = (asm.IsWindowsRuntime ? customBuilder.Constructor.ImportTo(this) : GetConstructorToken(customBuilder.Constructor).Token),
				Value = customBuilder.WriteBlob(this)
			};
			CustomAttribute.AddRecord(newRecord);
		}

		private void AddDeclSecurityRecord(int token, int action, int blob)
		{
			DeclSecurityTable.Record newRecord = new DeclSecurityTable.Record
			{
				Action = (short)action,
				Parent = token,
				PermissionSet = blob
			};
			DeclSecurity.AddRecord(newRecord);
		}

		internal void AddDeclarativeSecurity(int token, SecurityAction securityAction, PermissionSet permissionSet)
		{
			AddDeclSecurityRecord(token, (int)securityAction, Blobs.Add(ByteBuffer.Wrap(Encoding.Unicode.GetBytes(permissionSet.ToXml().ToString()))));
		}

		internal void AddDeclarativeSecurity(int token, List<CustomAttributeBuilder> declarativeSecurity)
		{
			Dictionary<int, List<CustomAttributeBuilder>> dictionary = new Dictionary<int, List<CustomAttributeBuilder>>();
			foreach (CustomAttributeBuilder item in declarativeSecurity)
			{
				int num = ((item.ConstructorArgumentCount != 0) ? ((int)item.GetConstructorArgument(0)) : 6);
				if (item.IsLegacyDeclSecurity)
				{
					AddDeclSecurityRecord(token, num, item.WriteLegacyDeclSecurityBlob(this));
					continue;
				}
				List<CustomAttributeBuilder> value;
				if (!dictionary.TryGetValue(num, out value))
				{
					value = new List<CustomAttributeBuilder>();
					dictionary.Add(num, value);
				}
				value.Add(item);
			}
			foreach (KeyValuePair<int, List<CustomAttributeBuilder>> item2 in dictionary)
			{
				AddDeclSecurityRecord(token, item2.Key, WriteDeclSecurityBlob(item2.Value));
			}
		}

		private int WriteDeclSecurityBlob(List<CustomAttributeBuilder> list)
		{
			ByteBuffer byteBuffer = new ByteBuffer(100);
			ByteBuffer byteBuffer2 = new ByteBuffer(list.Count * 100);
			byteBuffer2.Write((byte)46);
			byteBuffer2.WriteCompressedUInt(list.Count);
			foreach (CustomAttributeBuilder item in list)
			{
				byteBuffer2.Write(item.Constructor.DeclaringType.AssemblyQualifiedName);
				byteBuffer.Clear();
				item.WriteNamedArgumentsForDeclSecurity(this, byteBuffer);
				byteBuffer2.WriteCompressedUInt(byteBuffer.Length);
				byteBuffer2.Write(byteBuffer);
			}
			return Blobs.Add(byteBuffer2);
		}

		public void DefineManifestResource(string name, Stream stream, ResourceAttributes attribute)
		{
			resourceWriters.Add(new ResourceWriterRecord(name, stream, attribute));
		}

		public IResourceWriter DefineResource(string name, string description)
		{
			return DefineResource(name, description, ResourceAttributes.Public);
		}

		public IResourceWriter DefineResource(string name, string description, ResourceAttributes attribute)
		{
			MemoryStream stream = new MemoryStream();
			ResourceWriter resourceWriter = new ResourceWriter(stream);
			resourceWriters.Add(new ResourceWriterRecord(name, resourceWriter, stream, attribute));
			return resourceWriter;
		}

		internal void EmitResources()
		{
			int num = 0;
			foreach (ResourceWriterRecord resourceWriter in resourceWriters)
			{
				num = (num + 7) & -8;
				resourceWriter.Emit(this, num);
				num += resourceWriter.GetLength();
			}
		}

		internal void WriteResources(MetadataWriter mw)
		{
			int num = 0;
			foreach (ResourceWriterRecord resourceWriter in resourceWriters)
			{
				int num2 = ((num + 7) & -8) - num;
				for (int i = 0; i < num2; i++)
				{
					mw.Write((byte)0);
				}
				resourceWriter.Write(mw);
				num += resourceWriter.GetLength() + num2;
			}
		}

		internal void CloseResources()
		{
			foreach (ResourceWriterRecord resourceWriter in resourceWriters)
			{
				resourceWriter.Close();
			}
		}

		internal int GetManifestResourcesLength()
		{
			int num = 0;
			foreach (ResourceWriterRecord resourceWriter in resourceWriters)
			{
				num = (num + 7) & -8;
				num += resourceWriter.GetLength();
			}
			return num;
		}

		internal override Type FindType(TypeName name)
		{
			foreach (TypeBuilder type in types)
			{
				if (type.TypeName == name)
				{
					return type;
				}
			}
			return null;
		}

		internal override Type FindTypeIgnoreCase(TypeName lowerCaseName)
		{
			foreach (TypeBuilder type in types)
			{
				if (type.TypeName.ToLowerInvariant() == lowerCaseName)
				{
					return type;
				}
			}
			return null;
		}

		internal override void GetTypesImpl(List<Type> list)
		{
			foreach (TypeBuilder type in types)
			{
				if (type != moduleType)
				{
					list.Add(type);
				}
			}
		}

		public int __GetAssemblyToken(Assembly assembly)
		{
			return ImportAssemblyRef(assembly);
		}

		public TypeToken GetTypeToken(string name)
		{
			return new TypeToken(GetType(name, true, false).MetadataToken);
		}

		public TypeToken GetTypeToken(Type type)
		{
			if (type.Module == this && !asm.IsWindowsRuntime)
			{
				return new TypeToken(type.GetModuleBuilderToken());
			}
			return new TypeToken(ImportType(type));
		}

		internal int GetTypeTokenForMemberRef(Type type)
		{
			if (type.__IsMissing)
			{
				return ImportType(type);
			}
			if (type.IsGenericTypeDefinition)
			{
				int value;
				if (!memberRefTypeTokens.TryGetValue(type, out value))
				{
					ByteBuffer bb = new ByteBuffer(5);
					Signature.WriteTypeSpec(this, bb, type);
					value = 0x1B000000 | TypeSpec.AddRecord(Blobs.Add(bb));
					memberRefTypeTokens.Add(type, value);
				}
				return value;
			}
			if (type.IsModulePseudoType)
			{
				return 0x1A000000 | ModuleRef.FindOrAddRecord(Strings.Add(type.Module.ScopeName));
			}
			return GetTypeToken(type).Token;
		}

		private static bool IsFromGenericTypeDefinition(MemberInfo member)
		{
			Type declaringType = member.DeclaringType;
			if (declaringType != null && !declaringType.__IsMissing)
			{
				return declaringType.IsGenericTypeDefinition;
			}
			return false;
		}

		public FieldToken GetFieldToken(FieldInfo field)
		{
			FieldBuilder fieldBuilder = field as FieldBuilder;
			if (fieldBuilder != null && fieldBuilder.Module == this && !IsFromGenericTypeDefinition(fieldBuilder))
			{
				return new FieldToken(fieldBuilder.MetadataToken);
			}
			return new FieldToken(field.ImportTo(this));
		}

		public MethodToken GetMethodToken(MethodInfo method)
		{
			MethodBuilder methodBuilder = method as MethodBuilder;
			if (methodBuilder != null && methodBuilder.ModuleBuilder == this)
			{
				return new MethodToken(methodBuilder.MetadataToken);
			}
			return new MethodToken(method.ImportTo(this));
		}

		public MethodToken GetMethodToken(MethodInfo method, IEnumerable<Type> optionalParameterTypes)
		{
			return __GetMethodToken(method, Util.ToArray(optionalParameterTypes), null);
		}

		public MethodToken __GetMethodToken(MethodInfo method, Type[] optionalParameterTypes, CustomModifiers[] customModifiers)
		{
			ByteBuffer bb = new ByteBuffer(16);
			method.MethodSignature.WriteMethodRefSig(this, bb, optionalParameterTypes, customModifiers);
			MemberRefTable.Record record = default(MemberRefTable.Record);
			if (method.Module == this)
			{
				record.Class = method.MetadataToken;
			}
			else
			{
				record.Class = GetTypeTokenForMemberRef(method.DeclaringType ?? method.Module.GetModuleType());
			}
			record.Name = Strings.Add(method.Name);
			record.Signature = Blobs.Add(bb);
			return new MethodToken(0xA000000 | MemberRef.FindOrAddRecord(record));
		}

		internal MethodToken GetMethodTokenForIL(MethodInfo method)
		{
			if (method.IsGenericMethodDefinition)
			{
				MethodInfo methodInfo = method;
				method = methodInfo.MakeGenericMethod(methodInfo.GetGenericArguments());
			}
			if (IsFromGenericTypeDefinition(method))
			{
				return new MethodToken(method.ImportTo(this));
			}
			return GetMethodToken(method);
		}

		internal int GetMethodTokenWinRT(MethodInfo method)
		{
			if (!asm.IsWindowsRuntime)
			{
				return GetMethodToken(method).Token;
			}
			return method.ImportTo(this);
		}

		public MethodToken GetConstructorToken(ConstructorInfo constructor)
		{
			return GetMethodToken(constructor.GetMethodInfo());
		}

		public MethodToken GetConstructorToken(ConstructorInfo constructor, IEnumerable<Type> optionalParameterTypes)
		{
			return GetMethodToken(constructor.GetMethodInfo(), optionalParameterTypes);
		}

		public MethodToken __GetConstructorToken(ConstructorInfo constructor, Type[] optionalParameterTypes, CustomModifiers[] customModifiers)
		{
			return __GetMethodToken(constructor.GetMethodInfo(), optionalParameterTypes, customModifiers);
		}

		internal int ImportMethodOrField(Type declaringType, string name, Signature sig)
		{
			MemberRefKey key = new MemberRefKey(declaringType, name, sig);
			int value;
			if (!importedMemberRefs.TryGetValue(key, out value))
			{
				MemberRefTable.Record newRecord = new MemberRefTable.Record
				{
					Class = GetTypeTokenForMemberRef(declaringType),
					Name = Strings.Add(name)
				};
				ByteBuffer bb = new ByteBuffer(16);
				sig.WriteSig(this, bb);
				newRecord.Signature = Blobs.Add(bb);
				value = 0xA000000 | MemberRef.AddRecord(newRecord);
				importedMemberRefs.Add(key, value);
			}
			return value;
		}

		internal int ImportMethodSpec(Type declaringType, MethodInfo method, Type[] genericParameters)
		{
			MethodSpecKey key = new MethodSpecKey(declaringType, method.Name, method.MethodSignature, genericParameters);
			int value;
			if (!importedMethodSpecs.TryGetValue(key, out value))
			{
				MethodSpecTable.Record record = default(MethodSpecTable.Record);
				MethodBuilder methodBuilder = method as MethodBuilder;
				if (methodBuilder != null && methodBuilder.ModuleBuilder == this && !declaringType.IsGenericType)
				{
					record.Method = methodBuilder.MetadataToken;
				}
				else
				{
					record.Method = ImportMethodOrField(declaringType, method.Name, method.MethodSignature);
				}
				ByteBuffer bb = new ByteBuffer(10);
				Signature.WriteMethodSpec(this, bb, genericParameters);
				record.Instantiation = Blobs.Add(bb);
				value = 0x2B000000 | MethodSpec.FindOrAddRecord(record);
				importedMethodSpecs.Add(key, value);
			}
			return value;
		}

		internal int ImportType(Type type)
		{
			int value;
			if (!typeTokens.TryGetValue(type, out value))
			{
				if (type.HasElementType || type.IsConstructedGenericType || type.__IsFunctionPointer)
				{
					ByteBuffer bb = new ByteBuffer(5);
					Signature.WriteTypeSpec(this, bb, type);
					value = 0x1B000000 | TypeSpec.AddRecord(Blobs.Add(bb));
				}
				else
				{
					TypeRefTable.Record newRecord = default(TypeRefTable.Record);
					if (type.IsNested)
					{
						newRecord.ResolutionScope = GetTypeToken(type.DeclaringType).Token;
					}
					else if (type.Module == this)
					{
						newRecord.ResolutionScope = 1;
					}
					else
					{
						newRecord.ResolutionScope = ImportAssemblyRef(type.Assembly);
					}
					SetTypeNameAndTypeNamespace(type.TypeName, out newRecord.TypeName, out newRecord.TypeNamespace);
					value = 0x1000000 | TypeRef.AddRecord(newRecord);
				}
				typeTokens.Add(type, value);
			}
			return value;
		}

		private int ImportAssemblyRef(Assembly asm)
		{
			int value;
			if (!referencedAssemblies.TryGetValue(asm, out value))
			{
				value = AllocPseudoToken();
				referencedAssemblies.Add(asm, value);
			}
			return value;
		}

		internal void FillAssemblyRefTable()
		{
			foreach (KeyValuePair<Assembly, int> referencedAssembly in referencedAssemblies)
			{
				if (IsPseudoToken(referencedAssembly.Value))
				{
					RegisterTokenFixup(referencedAssembly.Value, FindOrAddAssemblyRef(referencedAssembly.Key.GetName(), false));
				}
			}
		}

		private int FindOrAddAssemblyRef(AssemblyName name, bool alwaysAdd)
		{
			AssemblyRefTable.Record record = default(AssemblyRefTable.Record);
			Version version = name.Version ?? new Version(0, 0, 0, 0);
			record.MajorVersion = (ushort)version.Major;
			record.MinorVersion = (ushort)version.Minor;
			record.BuildNumber = (ushort)version.Build;
			record.RevisionNumber = (ushort)version.Revision;
			record.Flags = (int)(name.Flags & ~AssemblyNameFlags.PublicKey);
			if ((name.RawFlags & (AssemblyNameFlags)0x80) != AssemblyNameFlags.None)
			{
				record.Flags |= (int)(name.RawFlags & (AssemblyNameFlags)0x70);
			}
			if (name.ContentType == AssemblyContentType.WindowsRuntime)
			{
				record.Flags |= 512;
			}
			byte[] array = null;
			if (usePublicKeyAssemblyReference)
			{
				array = name.GetPublicKey();
			}
			if (array == null || array.Length == 0)
			{
				array = name.GetPublicKeyToken() ?? Empty<byte>.Array;
			}
			else
			{
				record.Flags |= 1;
			}
			record.PublicKeyOrToken = Blobs.Add(ByteBuffer.Wrap(array));
			record.Name = Strings.Add(name.Name);
			record.Culture = ((name.Culture != null) ? Strings.Add(name.Culture) : 0);
			if (name.hash != null)
			{
				record.HashValue = Blobs.Add(ByteBuffer.Wrap(name.hash));
			}
			else
			{
				record.HashValue = 0;
			}
			return 0x23000000 | (alwaysAdd ? AssemblyRef.AddRecord(record) : AssemblyRef.FindOrAddRecord(record));
		}

		internal void WriteSymbolTokenMap()
		{
			for (int i = 0; i < resolvedTokens.Count; i++)
			{
				int num = resolvedTokens[i];
				int oldToken = (i + 1) | (num & -16777216);
				SymbolSupport.RemapToken(symbolWriter, oldToken, num);
			}
		}

		internal void RegisterTokenFixup(int pseudoToken, int realToken)
		{
			int num = -(pseudoToken + 1);
			while (resolvedTokens.Count <= num)
			{
				resolvedTokens.Add(0);
			}
			resolvedTokens[num] = realToken;
		}

		internal static bool IsPseudoToken(int token)
		{
			return token < 0;
		}

		internal int ResolvePseudoToken(int pseudoToken)
		{
			int index = -(pseudoToken + 1);
			return resolvedTokens[index];
		}

		internal void ApplyUnmanagedExports(ImageFileMachine imageFileMachine)
		{
			if (unmanagedExports.Count == 0)
			{
				return;
			}
			int type;
			int num;
			switch (imageFileMachine)
			{
			case ImageFileMachine.I386:
			case ImageFileMachine.ARM:
				type = 5;
				num = 4;
				break;
			case ImageFileMachine.AMD64:
				type = 6;
				num = 8;
				break;
			default:
				throw new NotSupportedException();
			}
			List<MethodBuilder> list = new List<MethodBuilder>();
			for (int i = 0; i < unmanagedExports.Count; i++)
			{
				if (unmanagedExports[i].mb != null)
				{
					list.Add(unmanagedExports[i].mb);
				}
			}
			if (list.Count == 0)
			{
				return;
			}
			RelativeVirtualAddress relativeVirtualAddress = __AddVTableFixups(list.ToArray(), type);
			for (int j = 0; j < unmanagedExports.Count; j++)
			{
				if (unmanagedExports[j].mb != null)
				{
					UnmanagedExport value = unmanagedExports[j];
					value.rva = new RelativeVirtualAddress(relativeVirtualAddress.initializedDataOffset + (uint)(list.IndexOf(unmanagedExports[j].mb) * num));
					unmanagedExports[j] = value;
				}
			}
		}

		internal void FixupMethodBodyTokens()
		{
			int methodToken = 100663297;
			int fieldToken = 67108865;
			int parameterToken = 134217729;
			foreach (TypeBuilder type in types)
			{
				type.ResolveMethodAndFieldTokens(ref methodToken, ref fieldToken, ref parameterToken);
			}
			foreach (int tokenFixupOffset in tokenFixupOffsets)
			{
				methodBodies.Position = tokenFixupOffset;
				int int32AtCurrentPosition = methodBodies.GetInt32AtCurrentPosition();
				methodBodies.Write(ResolvePseudoToken(int32AtCurrentPosition));
			}
			foreach (VTableFixups vtablefixup in vtablefixups)
			{
				for (int i = 0; i < vtablefixup.count; i++)
				{
					initializedData.Position = (int)vtablefixup.initializedDataOffset + i * vtablefixup.SlotWidth;
					initializedData.Write(ResolvePseudoToken(initializedData.GetInt32AtCurrentPosition()));
				}
			}
		}

		private int GetHeaderLength()
		{
			return 16 + StringToPaddedUTF8Length(asm.ImageRuntimeVersion) + 2 + 2 + 4 + 4 + 4 + 4 + 4 + 12 + 4 + 4 + 4 + 4 + 4 + 8 + ((!Blobs.IsEmpty) ? 16 : 0);
		}

		internal void WriteMetadata(MetadataWriter mw, out int guidHeapOffset)
		{
			mw.Write(1112167234);
			mw.Write((ushort)1);
			mw.Write((ushort)1);
			mw.Write(0);
			byte[] array = StringToPaddedUTF8(asm.ImageRuntimeVersion);
			mw.Write(array.Length);
			mw.Write(array);
			mw.Write((ushort)0);
			if (Blobs.IsEmpty)
			{
				mw.Write((ushort)4);
			}
			else
			{
				mw.Write((ushort)5);
			}
			int headerLength = GetHeaderLength();
			mw.Write(headerLength);
			mw.Write(Tables.Length);
			mw.Write(StringToPaddedUTF8("#~"));
			headerLength += Tables.Length;
			mw.Write(headerLength);
			mw.Write(Strings.Length);
			mw.Write(StringToPaddedUTF8("#Strings"));
			headerLength += Strings.Length;
			mw.Write(headerLength);
			mw.Write(UserStrings.Length);
			mw.Write(StringToPaddedUTF8("#US"));
			headerLength += UserStrings.Length;
			mw.Write(headerLength);
			mw.Write(Guids.Length);
			mw.Write(StringToPaddedUTF8("#GUID"));
			headerLength += Guids.Length;
			if (!Blobs.IsEmpty)
			{
				mw.Write(headerLength);
				mw.Write(Blobs.Length);
				mw.Write(StringToPaddedUTF8("#Blob"));
			}
			Tables.Write(mw);
			Strings.Write(mw);
			UserStrings.Write(mw);
			guidHeapOffset = mw.Position;
			Guids.Write(mw);
			if (!Blobs.IsEmpty)
			{
				Blobs.Write(mw);
			}
		}

		private static int StringToPaddedUTF8Length(string str)
		{
			return (Encoding.UTF8.GetByteCount(str) + 4) & -4;
		}

		private static byte[] StringToPaddedUTF8(string str)
		{
			byte[] array = new byte[(Encoding.UTF8.GetByteCount(str) + 4) & -4];
			Encoding.UTF8.GetBytes(str, 0, str.Length, array, 0);
			return array;
		}

		internal override void ExportTypes(int fileToken, ModuleBuilder manifestModule)
		{
			manifestModule.ExportTypes(types.ToArray(), fileToken);
		}

		internal void ExportTypes(Type[] types, int fileToken)
		{
			Dictionary<Type, int> dictionary = new Dictionary<Type, int>();
			foreach (Type type in types)
			{
				if (!type.IsModulePseudoType && IsVisible(type))
				{
					ExportedTypeTable.Record newRecord = new ExportedTypeTable.Record
					{
						Flags = (int)type.Attributes,
						TypeDefId = type.MetadataToken
					};
					SetTypeNameAndTypeNamespace(type.TypeName, out newRecord.TypeName, out newRecord.TypeNamespace);
					if (type.IsNested)
					{
						newRecord.Implementation = dictionary[type.DeclaringType];
					}
					else
					{
						newRecord.Implementation = fileToken;
					}
					int value = 0x27000000 | ExportedType.AddRecord(newRecord);
					dictionary.Add(type, value);
				}
			}
		}

		private static bool IsVisible(Type type)
		{
			if (!type.IsPublic)
			{
				if (type.IsNestedFamily || type.IsNestedFamORAssem || type.IsNestedPublic)
				{
					return IsVisible(type.DeclaringType);
				}
				return false;
			}
			return true;
		}

		internal void AddConstant(int parentToken, object defaultValue)
		{
			ConstantTable.Record newRecord = new ConstantTable.Record
			{
				Parent = parentToken
			};
			ByteBuffer byteBuffer = new ByteBuffer(16);
			if (defaultValue == null)
			{
				newRecord.Type = 18;
				byteBuffer.Write(0);
			}
			else if (defaultValue is bool)
			{
				newRecord.Type = 2;
				byteBuffer.Write((byte)(((bool)defaultValue) ? 1 : 0));
			}
			else if (defaultValue is char)
			{
				newRecord.Type = 3;
				byteBuffer.Write((char)defaultValue);
			}
			else if (defaultValue is sbyte)
			{
				newRecord.Type = 4;
				byteBuffer.Write((sbyte)defaultValue);
			}
			else if (defaultValue is byte)
			{
				newRecord.Type = 5;
				byteBuffer.Write((byte)defaultValue);
			}
			else if (defaultValue is short)
			{
				newRecord.Type = 6;
				byteBuffer.Write((short)defaultValue);
			}
			else if (defaultValue is ushort)
			{
				newRecord.Type = 7;
				byteBuffer.Write((ushort)defaultValue);
			}
			else if (defaultValue is int)
			{
				newRecord.Type = 8;
				byteBuffer.Write((int)defaultValue);
			}
			else if (defaultValue is uint)
			{
				newRecord.Type = 9;
				byteBuffer.Write((uint)defaultValue);
			}
			else if (defaultValue is long)
			{
				newRecord.Type = 10;
				byteBuffer.Write((long)defaultValue);
			}
			else if (defaultValue is ulong)
			{
				newRecord.Type = 11;
				byteBuffer.Write((ulong)defaultValue);
			}
			else if (defaultValue is float)
			{
				newRecord.Type = 12;
				byteBuffer.Write((float)defaultValue);
			}
			else if (defaultValue is double)
			{
				newRecord.Type = 13;
				byteBuffer.Write((double)defaultValue);
			}
			else if (defaultValue is string)
			{
				newRecord.Type = 14;
				string text = (string)defaultValue;
				foreach (char value in text)
				{
					byteBuffer.Write(value);
				}
			}
			else
			{
				if (!(defaultValue is DateTime))
				{
					throw new ArgumentException();
				}
				newRecord.Type = 10;
				byteBuffer.Write(((DateTime)defaultValue).Ticks);
			}
			newRecord.Value = Blobs.Add(byteBuffer);
			Constant.AddRecord(newRecord);
		}

		internal override Type ResolveType(int metadataToken, IGenericContext context)
		{
			if (metadataToken >> 24 != 2)
			{
				throw new NotImplementedException();
			}
			return types[(metadataToken & 0xFFFFFF) - 1];
		}

		public override MethodBase ResolveMethod(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			if (genericTypeArguments != null || genericMethodArguments != null)
			{
				throw new NotImplementedException();
			}
			if (metadataToken >> 24 == 10)
			{
				foreach (KeyValuePair<MemberRefKey, int> importedMemberRef in importedMemberRefs)
				{
					if (importedMemberRef.Value == metadataToken)
					{
						return importedMemberRef.Key.LookupMethod();
					}
				}
			}
			if ((metadataToken & 0xFF000000u) == 100663296)
			{
				metadataToken = -(metadataToken & 0xFFFFFF);
			}
			foreach (TypeBuilder type in types)
			{
				MethodBase methodBase = type.LookupMethod(metadataToken);
				if (methodBase != null)
				{
					return methodBase;
				}
			}
			return moduleType.LookupMethod(metadataToken);
		}

		public override FieldInfo ResolveField(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw new NotImplementedException();
		}

		public override MemberInfo ResolveMember(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw new NotImplementedException();
		}

		public override string ResolveString(int metadataToken)
		{
			throw new NotImplementedException();
		}

		internal Guid GetModuleVersionIdOrEmpty()
		{
			return mvid;
		}

		public void __SetModuleVersionId(Guid guid)
		{
			if (guid == Guid.Empty && universe.Deterministic)
			{
				throw new ArgumentOutOfRangeException();
			}
			mvid = guid;
		}

		internal uint GetTimeDateStamp()
		{
			return timestamp;
		}

		public override Type[] __ResolveOptionalParameterTypes(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments, out CustomModifiers[] customModifiers)
		{
			throw new NotImplementedException();
		}

		public void DefineUnmanagedResource(string resourceFileName)
		{
			unmanagedResources = new ResourceSection();
			unmanagedResources.ExtractResources(System.IO.File.ReadAllBytes(resourceFileName));
		}

		public bool IsTransient()
		{
			return false;
		}

		public void SetUserEntryPoint(MethodInfo entryPoint)
		{
			int metadataToken = entryPoint.MetadataToken;
			if (metadataToken < 0)
			{
				metadataToken = -metadataToken | 0x6000000;
			}
		}

		public StringToken GetStringConstant(string str)
		{
			return new StringToken(UserStrings.Add(str) | 0x70000000);
		}

		public SignatureToken GetSignatureToken(SignatureHelper sigHelper)
		{
			return new SignatureToken(StandAloneSig.FindOrAddRecord(Blobs.Add(sigHelper.GetSignature(this))) | 0x11000000);
		}

		public SignatureToken GetSignatureToken(byte[] sigBytes, int sigLength)
		{
			return new SignatureToken(StandAloneSig.FindOrAddRecord(Blobs.Add(ByteBuffer.Wrap(sigBytes, sigLength))) | 0x11000000);
		}

		public MethodInfo GetArrayMethod(Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
		{
			return new ArrayMethod(this, arrayClass, methodName, callingConvention, returnType, parameterTypes);
		}

		public MethodToken GetArrayMethodToken(Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
		{
			return GetMethodToken(GetArrayMethod(arrayClass, methodName, callingConvention, returnType, parameterTypes));
		}

		internal override Type GetModuleType()
		{
			return moduleType;
		}

		internal override ByteReader GetBlob(int blobIndex)
		{
			return Blobs.GetBlob(blobIndex);
		}

		internal int GetSignatureBlobIndex(Signature sig)
		{
			ByteBuffer bb = new ByteBuffer(16);
			sig.WriteSig(this, bb);
			return Blobs.Add(bb);
		}

		protected override long GetImageBaseImpl()
		{
			return imageBaseAddress;
		}

		protected override long GetStackReserveImpl()
		{
			return stackReserve;
		}

		[Obsolete("Use __StackReserve property.")]
		public void __SetStackReserve(long stackReserve)
		{
			__StackReserve = stackReserve;
		}

		internal ulong GetStackReserve(ulong defaultValue)
		{
			if (stackReserve != -1)
			{
				return (ulong)stackReserve;
			}
			return defaultValue;
		}

		protected override int GetFileAlignmentImpl()
		{
			return fileAlignment;
		}

		protected override DllCharacteristics GetDllCharacteristicsImpl()
		{
			return dllCharacteristics;
		}

		private int AddTypeRefByName(int resolutionScope, string ns, string name)
		{
			TypeRefTable.Record newRecord = new TypeRefTable.Record
			{
				ResolutionScope = resolutionScope
			};
			SetTypeNameAndTypeNamespace(new TypeName(ns, name), out newRecord.TypeName, out newRecord.TypeNamespace);
			return 0x1000000 | TypeRef.AddRecord(newRecord);
		}

		public void __Save(PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
		{
			SaveImpl(null, portableExecutableKind, imageFileMachine);
		}

		public void __Save(Stream stream, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
		{
			if (!stream.CanRead || !stream.CanWrite || !stream.CanSeek || stream.Position != 0L)
			{
				throw new ArgumentException("Stream must support read/write/seek and current position must be zero.", "stream");
			}
			SaveImpl(stream, portableExecutableKind, imageFileMachine);
		}

		private void SaveImpl(Stream streamOrNull, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
		{
			SetIsSaved();
			PopulatePropertyAndEventTables();
			IList<CustomAttributeData> customAttributesData = asm.GetCustomAttributesData(null);
			if (customAttributesData.Count > 0)
			{
				int resolutionScope = ImportAssemblyRef(universe.Mscorlib);
				int[] array = new int[4];
				string[] array2 = new string[4] { "AssemblyAttributesGoHere", "AssemblyAttributesGoHereM", "AssemblyAttributesGoHereS", "AssemblyAttributesGoHereSM" };
				foreach (CustomAttributeData item in customAttributesData)
				{
					int num = ((item.Constructor.DeclaringType.BaseType == universe.System_Security_Permissions_CodeAccessSecurityAttribute) ? ((!item.Constructor.DeclaringType.IsAllowMultipleCustomAttribute) ? 2 : 3) : (item.Constructor.DeclaringType.IsAllowMultipleCustomAttribute ? 1 : 0));
					if (array[num] == 0)
					{
						array[num] = AddTypeRefByName(resolutionScope, "System.Runtime.CompilerServices", array2[num]);
					}
					SetCustomAttribute(array[num], item.__ToBuilder());
				}
			}
			FillAssemblyRefTable();
			EmitResources();
			ModuleWriter.WriteModule(null, null, this, PEFileKinds.Dll, portableExecutableKind, imageFileMachine, unmanagedResources, 0, streamOrNull);
			CloseResources();
		}

		public void __AddAssemblyReference(AssemblyName assemblyName)
		{
			__AddAssemblyReference(assemblyName, null);
		}

		public void __AddAssemblyReference(AssemblyName assemblyName, Assembly assembly)
		{
			if (referencedAssemblyNames == null)
			{
				referencedAssemblyNames = new List<AssemblyName>();
			}
			referencedAssemblyNames.Add((AssemblyName)assemblyName.Clone());
			int value = FindOrAddAssemblyRef(assemblyName, true);
			if (assembly != null)
			{
				referencedAssemblies.Add(assembly, value);
			}
		}

		public override AssemblyName[] __GetReferencedAssemblies()
		{
			List<AssemblyName> list = new List<AssemblyName>();
			if (referencedAssemblyNames != null)
			{
				foreach (AssemblyName referencedAssemblyName in referencedAssemblyNames)
				{
					if (!list.Contains(referencedAssemblyName))
					{
						list.Add(referencedAssemblyName);
					}
				}
			}
			foreach (Assembly key in referencedAssemblies.Keys)
			{
				AssemblyName name = key.GetName();
				if (!list.Contains(name))
				{
					list.Add(name);
				}
			}
			return list.ToArray();
		}

		public void __AddModuleReference(string module)
		{
			ModuleRef.FindOrAddRecord((module != null) ? Strings.Add(module) : 0);
		}

		public override string[] __GetReferencedModules()
		{
			string[] array = new string[ModuleRef.RowCount];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = Strings.Find(ModuleRef.records[i]);
			}
			return array;
		}

		public override Type[] __GetReferencedTypes()
		{
			List<Type> list = new List<Type>();
			foreach (KeyValuePair<Type, int> typeToken in typeTokens)
			{
				if (typeToken.Value >> 24 == 1)
				{
					list.Add(typeToken.Key);
				}
			}
			return list.ToArray();
		}

		public override Type[] __GetExportedTypes()
		{
			throw new NotImplementedException();
		}

		public int __AddModule(int flags, string name, byte[] hash)
		{
			FileTable.Record newRecord = new FileTable.Record
			{
				Flags = flags,
				Name = Strings.Add(name),
				HashValue = Blobs.Add(ByteBuffer.Wrap(hash))
			};
			return 637534208 + File.AddRecord(newRecord);
		}

		public int __AddManifestResource(int offset, ResourceAttributes flags, string name, int implementation)
		{
			ManifestResourceTable.Record newRecord = new ManifestResourceTable.Record
			{
				Offset = offset,
				Flags = (int)flags,
				Name = Strings.Add(name),
				Implementation = implementation
			};
			return 671088640 + ManifestResource.AddRecord(newRecord);
		}

		public void __SetCustomAttributeFor(int token, CustomAttributeBuilder customBuilder)
		{
			SetCustomAttribute(token, customBuilder);
		}

		public RelativeVirtualAddress __AddVTableFixups(MethodBuilder[] methods, int type)
		{
			initializedData.Align(8);
			VTableFixups item = default(VTableFixups);
			item.initializedDataOffset = (uint)initializedData.Position;
			item.count = (ushort)methods.Length;
			item.type = (ushort)type;
			foreach (MethodBuilder methodBuilder in methods)
			{
				initializedData.Write(methodBuilder.MetadataToken);
				if (item.SlotWidth == 8)
				{
					initializedData.Write(0);
				}
			}
			vtablefixups.Add(item);
			return new RelativeVirtualAddress(item.initializedDataOffset);
		}

		public void __AddUnmanagedExportStub(string name, int ordinal, RelativeVirtualAddress rva)
		{
			AddUnmanagedExport(name, ordinal, null, rva);
		}

		internal void AddUnmanagedExport(string name, int ordinal, MethodBuilder methodBuilder, RelativeVirtualAddress rva)
		{
			UnmanagedExport item = default(UnmanagedExport);
			item.name = name;
			item.ordinal = ordinal;
			item.mb = methodBuilder;
			item.rva = rva;
			unmanagedExports.Add(item);
		}

		internal void SetInterfaceImplementationCustomAttribute(TypeBuilder typeBuilder, Type interfaceType, CustomAttributeBuilder cab)
		{
			if (interfaceImplCustomAttributes == null)
			{
				interfaceImplCustomAttributes = new List<InterfaceImplCustomAttribute>();
			}
			InterfaceImplCustomAttribute item = default(InterfaceImplCustomAttribute);
			item.type = typeBuilder.MetadataToken;
			int token = GetTypeToken(interfaceType).Token;
			switch (token >> 24)
			{
			case 2:
				token = ((token & 0xFFFFFF) << 2) | 0;
				break;
			case 1:
				token = ((token & 0xFFFFFF) << 2) | 1;
				break;
			case 27:
				token = ((token & 0xFFFFFF) << 2) | 2;
				break;
			default:
				throw new InvalidOperationException();
			}
			item.interfaceType = token;
			item.pseudoToken = AllocPseudoToken();
			interfaceImplCustomAttributes.Add(item);
			SetCustomAttribute(item.pseudoToken, cab);
		}

		internal void ResolveInterfaceImplPseudoTokens()
		{
			if (interfaceImplCustomAttributes == null)
			{
				return;
			}
			foreach (InterfaceImplCustomAttribute interfaceImplCustomAttribute in interfaceImplCustomAttributes)
			{
				for (int i = 0; i < InterfaceImpl.records.Length; i++)
				{
					if (InterfaceImpl.records[i].Class == interfaceImplCustomAttribute.type && InterfaceImpl.records[i].Interface == interfaceImplCustomAttribute.interfaceType)
					{
						RegisterTokenFixup(interfaceImplCustomAttribute.pseudoToken, 0x9000000 | (i + 1));
						break;
					}
				}
			}
		}

		internal void FixupPseudoToken(ref int token)
		{
			if (IsPseudoToken(token))
			{
				token = ResolvePseudoToken(token);
			}
		}

		internal void SetIsSaved()
		{
			if (saved)
			{
				throw new InvalidOperationException();
			}
			saved = true;
		}

		internal override string GetString(int index)
		{
			return Strings.Find(index);
		}
	}
}
