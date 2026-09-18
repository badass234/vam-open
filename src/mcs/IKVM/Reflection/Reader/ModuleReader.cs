using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection.Reader
{
	internal sealed class ModuleReader : Module
	{
		private sealed class LazyForwardedType
		{
			private readonly int index;

			private Type type;

			internal LazyForwardedType(int index)
			{
				this.index = index;
			}

			internal Type GetType(ModuleReader module)
			{
				if (type == MarkerType.Pinned)
				{
					TypeName typeName = module.GetTypeName(module.ExportedType.records[index].TypeNamespace, module.ExportedType.records[index].TypeName);
					return module.universe.GetMissingTypeOrThrow(module, module, null, typeName).SetCyclicTypeForwarder();
				}
				if (type == null)
				{
					type = MarkerType.Pinned;
					type = module.ResolveExportedType(index);
				}
				return type;
			}
		}

		private sealed class TrackingGenericContext : IGenericContext
		{
			private readonly IGenericContext context;

			private bool used;

			internal bool IsUsed => used;

			internal TrackingGenericContext(IGenericContext context)
			{
				this.context = context;
			}

			public Type GetGenericTypeArgument(int index)
			{
				used = true;
				return context.GetGenericTypeArgument(index);
			}

			public Type GetGenericMethodArgument(int index)
			{
				used = true;
				return context.GetGenericMethodArgument(index);
			}
		}

		private readonly Stream stream;

		private readonly string location;

		private Assembly assembly;

		private readonly PEReader peFile = new PEReader();

		private readonly CliHeader cliHeader = new CliHeader();

		private string imageRuntimeVersion;

		private int metadataStreamVersion;

		private byte[] stringHeap;

		private byte[] blobHeap;

		private byte[] guidHeap;

		private uint userStringHeapOffset;

		private uint userStringHeapSize;

		private byte[] lazyUserStringHeap;

		private TypeDefImpl[] typeDefs;

		private TypeDefImpl moduleType;

		private Assembly[] assemblyRefs;

		private Type[] typeRefs;

		private Type[] typeSpecs;

		private FieldInfo[] fields;

		private MethodBase[] methods;

		private MemberInfo[] memberRefs;

		private Dictionary<int, string> strings = new Dictionary<int, string>();

		private Dictionary<TypeName, Type> types = new Dictionary<TypeName, Type>();

		private Dictionary<TypeName, LazyForwardedType> forwardedTypes = new Dictionary<TypeName, LazyForwardedType>();

		public override Guid ModuleVersionId
		{
			get
			{
				byte[] array = new byte[16];
				Buffer.BlockCopy(guidHeap, 16 * (ModuleTable.records[0].Mvid - 1), array, 0, 16);
				return new Guid(array);
			}
		}

		public override string FullyQualifiedName => location ?? "<Unknown>";

		public override string Name
		{
			get
			{
				if (location != null)
				{
					return Path.GetFileName(location);
				}
				return "<Unknown>";
			}
		}

		public override Assembly Assembly => assembly;

		public override string ScopeName => GetString(ModuleTable.records[0].Name);

		public override string __ImageRuntimeVersion => imageRuntimeVersion;

		public override int MDStreamVersion => metadataStreamVersion;

		public override int __Subsystem => peFile.OptionalHeader.Subsystem;

		public override int __EntryPointRVA
		{
			get
			{
				if ((cliHeader.Flags & 0x10) == 0)
				{
					return 0;
				}
				return (int)cliHeader.EntryPointToken;
			}
		}

		public override int __EntryPointToken
		{
			get
			{
				if ((cliHeader.Flags & 0x10) != 0)
				{
					return 0;
				}
				return (int)cliHeader.EntryPointToken;
			}
		}

		internal ModuleReader(AssemblyReader assembly, Universe universe, Stream stream, string location, bool mapped)
			: base(universe)
		{
			this.stream = ((universe != null && universe.MetadataOnly) ? null : stream);
			this.location = location;
			Read(stream, mapped);
			if (universe != null && universe.WindowsRuntimeProjection && imageRuntimeVersion.StartsWith("WindowsRuntime ", StringComparison.Ordinal))
			{
				WindowsRuntimeProjection.Patch(this, strings, ref imageRuntimeVersion, ref blobHeap);
			}
			if (assembly == null && AssemblyTable.records.Length != 0)
			{
				assembly = new AssemblyReader(location, this);
			}
			this.assembly = assembly;
		}

		private void Read(Stream stream, bool mapped)
		{
			BinaryReader br = new BinaryReader(stream);
			peFile.Read(br, mapped);
			stream.Seek(peFile.RvaToFileOffset(peFile.GetComDescriptorVirtualAddress()), SeekOrigin.Begin);
			cliHeader.Read(br);
			stream.Seek(peFile.RvaToFileOffset(cliHeader.MetaData.VirtualAddress), SeekOrigin.Begin);
			StreamHeader[] array = ReadStreamHeaders(br, out imageRuntimeVersion);
			foreach (StreamHeader streamHeader in array)
			{
				switch (streamHeader.Name)
				{
				case "#Strings":
					stringHeap = ReadHeap(stream, streamHeader.Offset, streamHeader.Size);
					break;
				case "#Blob":
					blobHeap = ReadHeap(stream, streamHeader.Offset, streamHeader.Size);
					break;
				case "#US":
					userStringHeapOffset = streamHeader.Offset;
					userStringHeapSize = streamHeader.Size;
					break;
				case "#GUID":
					guidHeap = ReadHeap(stream, streamHeader.Offset, streamHeader.Size);
					break;
				case "#~":
				case "#-":
					stream.Seek(peFile.RvaToFileOffset(cliHeader.MetaData.VirtualAddress + streamHeader.Offset), SeekOrigin.Begin);
					ReadTables(br);
					break;
				}
			}
		}

		internal void SetAssembly(Assembly assembly)
		{
			this.assembly = assembly;
		}

		private static StreamHeader[] ReadStreamHeaders(BinaryReader br, out string Version)
		{
			if (br.ReadUInt32() != 1112167234)
			{
				throw new BadImageFormatException("Invalid metadata signature");
			}
			br.ReadUInt16();
			br.ReadUInt16();
			br.ReadUInt32();
			uint count = br.ReadUInt32();
			byte[] bytes = br.ReadBytes((int)count);
			Version = Encoding.UTF8.GetString(bytes).TrimEnd(default(char));
			br.ReadUInt16();
			StreamHeader[] array = new StreamHeader[br.ReadUInt16()];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new StreamHeader();
				array[i].Read(br);
			}
			return array;
		}

		private void ReadTables(BinaryReader br)
		{
			Table[] tables = GetTables();
			br.ReadUInt32();
			byte b = br.ReadByte();
			byte b2 = br.ReadByte();
			metadataStreamVersion = (b << 16) | b2;
			byte heapSizes = br.ReadByte();
			br.ReadByte();
			ulong num = br.ReadUInt64();
			ulong num2 = br.ReadUInt64();
			for (int i = 0; i < 64; i++)
			{
				if ((num & (ulong)(1L << i)) != 0L)
				{
					tables[i].Sorted = (num2 & (ulong)(1L << i)) != 0;
					tables[i].RowCount = br.ReadInt32();
				}
			}
			MetadataReader mr = new MetadataReader(this, br.BaseStream, heapSizes);
			for (int j = 0; j < 64; j++)
			{
				if ((num & (ulong)(1L << j)) != 0L)
				{
					tables[j].Read(mr);
				}
			}
			if (ParamPtr.RowCount != 0)
			{
				throw new NotImplementedException("ParamPtr table support has not yet been implemented.");
			}
		}

		private byte[] ReadHeap(Stream stream, uint offset, uint size)
		{
			byte[] array = new byte[size];
			stream.Seek(peFile.RvaToFileOffset(cliHeader.MetaData.VirtualAddress + offset), SeekOrigin.Begin);
			int num;
			for (int i = 0; i < array.Length; i += num)
			{
				num = stream.Read(array, i, array.Length - i);
				if (num == 0)
				{
					throw new BadImageFormatException();
				}
			}
			return array;
		}

		internal void SeekRVA(int rva)
		{
			GetStream().Seek(peFile.RvaToFileOffset((uint)rva), SeekOrigin.Begin);
		}

		internal Stream GetStream()
		{
			if (stream == null)
			{
				throw new InvalidOperationException("Operation not available when UniverseOptions.MetadataOnly is enabled.");
			}
			return stream;
		}

		internal override void GetTypesImpl(List<Type> list)
		{
			PopulateTypeDef();
			TypeDefImpl[] array = typeDefs;
			foreach (TypeDefImpl typeDefImpl in array)
			{
				if (typeDefImpl != moduleType)
				{
					list.Add(typeDefImpl);
				}
			}
		}

		private void PopulateTypeDef()
		{
			if (typeDefs != null)
			{
				return;
			}
			typeDefs = new TypeDefImpl[TypeDef.records.Length];
			for (int i = 0; i < typeDefs.Length; i++)
			{
				TypeDefImpl typeDefImpl = new TypeDefImpl(this, i);
				typeDefs[i] = typeDefImpl;
				if (typeDefImpl.IsModulePseudoType)
				{
					moduleType = typeDefImpl;
				}
				else if (!typeDefImpl.IsNestedByFlags)
				{
					types.Add(typeDefImpl.TypeName, typeDefImpl);
				}
			}
			for (int j = 0; j < ExportedType.records.Length; j++)
			{
				if (ExportedType.records[j].Implementation >> 24 == 35)
				{
					TypeName typeName = GetTypeName(ExportedType.records[j].TypeNamespace, ExportedType.records[j].TypeName);
					forwardedTypes.Add(typeName, new LazyForwardedType(j));
				}
			}
		}

		internal override string GetString(int index)
		{
			if (index == 0)
			{
				return null;
			}
			string value;
			if (!strings.TryGetValue(index, out value))
			{
				int i;
				for (i = 0; stringHeap[index + i] != 0; i++)
				{
				}
				value = Encoding.UTF8.GetString(stringHeap, index, i);
				strings.Add(index, value);
			}
			return value;
		}

		private static int ReadCompressedUInt(byte[] buffer, ref int offset)
		{
			byte b = buffer[offset++];
			if (b <= 127)
			{
				return b;
			}
			if ((b & 0xC0) == 128)
			{
				byte b2 = buffer[offset++];
				return ((b & 0x3F) << 8) | b2;
			}
			byte b3 = buffer[offset++];
			byte b4 = buffer[offset++];
			byte b5 = buffer[offset++];
			return ((b & 0x3F) << 24) + (b3 << 16) + (b4 << 8) + b5;
		}

		internal byte[] GetBlobCopy(int blobIndex)
		{
			int num = ReadCompressedUInt(blobHeap, ref blobIndex);
			byte[] array = new byte[num];
			Buffer.BlockCopy(blobHeap, blobIndex, array, 0, num);
			return array;
		}

		internal override ByteReader GetBlob(int blobIndex)
		{
			return ByteReader.FromBlob(blobHeap, blobIndex);
		}

		public override string ResolveString(int metadataToken)
		{
			string value;
			if (!strings.TryGetValue(metadataToken, out value))
			{
				if (metadataToken >> 24 != 112)
				{
					throw TokenOutOfRangeException(metadataToken);
				}
				if (lazyUserStringHeap == null)
				{
					lazyUserStringHeap = ReadHeap(GetStream(), userStringHeapOffset, userStringHeapSize);
				}
				int offset = metadataToken & 0xFFFFFF;
				int num = ReadCompressedUInt(lazyUserStringHeap, ref offset) & -2;
				StringBuilder stringBuilder = new StringBuilder(num / 2);
				for (int i = 0; i < num; i += 2)
				{
					char value2 = (char)(lazyUserStringHeap[offset + i] | (lazyUserStringHeap[offset + i + 1] << 8));
					stringBuilder.Append(value2);
				}
				value = stringBuilder.ToString();
				strings.Add(metadataToken, value);
			}
			return value;
		}

		internal override Type ResolveType(int metadataToken, IGenericContext context)
		{
			int num = (metadataToken & 0xFFFFFF) - 1;
			if (num < 0)
			{
				throw TokenOutOfRangeException(metadataToken);
			}
			if (metadataToken >> 24 == 2 && num < TypeDef.RowCount)
			{
				PopulateTypeDef();
				return typeDefs[num];
			}
			if (metadataToken >> 24 == 1 && num < TypeRef.RowCount)
			{
				if (typeRefs == null)
				{
					typeRefs = new Type[TypeRef.records.Length];
				}
				if (typeRefs[num] == null)
				{
					int resolutionScope = TypeRef.records[num].ResolutionScope;
					switch (resolutionScope >> 24)
					{
					case 35:
					{
						Assembly assembly = ResolveAssemblyRef((resolutionScope & 0xFFFFFF) - 1);
						TypeName typeName3 = GetTypeName(TypeRef.records[num].TypeNamespace, TypeRef.records[num].TypeName);
						typeRefs[num] = assembly.ResolveType(this, typeName3);
						break;
					}
					case 1:
					{
						Type type = ResolveType(resolutionScope, null);
						TypeName typeName2 = GetTypeName(TypeRef.records[num].TypeNamespace, TypeRef.records[num].TypeName);
						typeRefs[num] = type.ResolveNestedType(this, typeName2);
						break;
					}
					case 0:
					case 26:
					{
						Module module;
						if (resolutionScope >> 24 == 0)
						{
							if (resolutionScope != 0 && resolutionScope != 1)
							{
								throw new NotImplementedException("self reference scope?");
							}
							module = this;
						}
						else
						{
							module = ResolveModuleRef(ModuleRef.records[(resolutionScope & 0xFFFFFF) - 1]);
						}
						TypeName typeName = GetTypeName(TypeRef.records[num].TypeNamespace, TypeRef.records[num].TypeName);
						typeRefs[num] = module.FindType(typeName) ?? module.universe.GetMissingTypeOrThrow(this, module, null, typeName);
						break;
					}
					default:
						throw new NotImplementedException("ResolutionScope = " + resolutionScope.ToString("X"));
					}
				}
				return typeRefs[num];
			}
			if (metadataToken >> 24 == 27 && num < TypeSpec.RowCount)
			{
				if (typeSpecs == null)
				{
					typeSpecs = new Type[TypeSpec.records.Length];
				}
				Type type2 = typeSpecs[num];
				if (type2 == null)
				{
					TrackingGenericContext trackingGenericContext = ((context == null) ? null : new TrackingGenericContext(context));
					type2 = Signature.ReadTypeSpec(this, ByteReader.FromBlob(blobHeap, TypeSpec.records[num]), trackingGenericContext);
					if (trackingGenericContext == null || !trackingGenericContext.IsUsed)
					{
						typeSpecs[num] = type2;
					}
				}
				return type2;
			}
			throw TokenOutOfRangeException(metadataToken);
		}

		private Module ResolveModuleRef(int moduleNameIndex)
		{
			string text = GetString(moduleNameIndex);
			Module module = assembly.GetModule(text);
			if (module == null)
			{
				throw new FileNotFoundException(text);
			}
			return module;
		}

		private TypeName GetTypeName(int typeNamespace, int typeName)
		{
			return new TypeName(GetString(typeNamespace), GetString(typeName));
		}

		internal Assembly ResolveAssemblyRef(int index)
		{
			if (assemblyRefs == null)
			{
				assemblyRefs = new Assembly[AssemblyRef.RowCount];
			}
			if (assemblyRefs[index] == null)
			{
				assemblyRefs[index] = ResolveAssemblyRefImpl(ref AssemblyRef.records[index]);
			}
			return assemblyRefs[index];
		}

		private Assembly ResolveAssemblyRefImpl(ref AssemblyRefTable.Record rec)
		{
			string fullName = AssemblyName.GetFullName(GetString(rec.Name), rec.MajorVersion, rec.MinorVersion, rec.BuildNumber, rec.RevisionNumber, (rec.Culture == 0) ? "neutral" : GetString(rec.Culture), (rec.PublicKeyOrToken == 0) ? Empty<byte>.Array : (((rec.Flags & 1) == 0) ? GetBlobCopy(rec.PublicKeyOrToken) : AssemblyName.ComputePublicKeyToken(GetBlobCopy(rec.PublicKeyOrToken))), rec.Flags);
			return universe.Load(fullName, this, true);
		}

		internal override Type FindType(TypeName typeName)
		{
			PopulateTypeDef();
			Type value;
			LazyForwardedType value2;
			if (!types.TryGetValue(typeName, out value) && forwardedTypes.TryGetValue(typeName, out value2))
			{
				return value2.GetType(this);
			}
			return value;
		}

		internal override Type FindTypeIgnoreCase(TypeName lowerCaseName)
		{
			PopulateTypeDef();
			foreach (Type value in types.Values)
			{
				if (value.TypeName.ToLowerInvariant() == lowerCaseName)
				{
					return value;
				}
			}
			foreach (TypeName key in forwardedTypes.Keys)
			{
				if (key.ToLowerInvariant() == lowerCaseName)
				{
					return forwardedTypes[key].GetType(this);
				}
			}
			return null;
		}

		private Exception TokenOutOfRangeException(int metadataToken)
		{
			return new ArgumentOutOfRangeException("metadataToken", $"Token 0x{metadataToken:x8} is not valid in the scope of module {Name}.");
		}

		public override MemberInfo ResolveMember(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			switch (metadataToken >> 24)
			{
			case 4:
				return ResolveField(metadataToken, genericTypeArguments, genericMethodArguments);
			case 10:
			{
				int num = (metadataToken & 0xFFFFFF) - 1;
				if (num >= 0 && num < MemberRef.RowCount)
				{
					return GetMemberRef(num, genericTypeArguments, genericMethodArguments);
				}
				break;
			}
			case 6:
			case 43:
				return ResolveMethod(metadataToken, genericTypeArguments, genericMethodArguments);
			case 1:
			case 2:
			case 27:
				return ResolveType(metadataToken, genericTypeArguments, genericMethodArguments);
			}
			throw TokenOutOfRangeException(metadataToken);
		}

		internal FieldInfo GetFieldAt(TypeDefImpl owner, int index)
		{
			if (fields == null)
			{
				fields = new FieldInfo[Field.records.Length];
			}
			if (fields[index] == null)
			{
				fields[index] = new FieldDefImpl(this, owner ?? FindFieldOwner(index), index);
			}
			return fields[index];
		}

		public override FieldInfo ResolveField(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			int num = (metadataToken & 0xFFFFFF) - 1;
			if (num < 0)
			{
				throw TokenOutOfRangeException(metadataToken);
			}
			if (metadataToken >> 24 == 4 && num < Field.RowCount)
			{
				return GetFieldAt(null, num);
			}
			if (metadataToken >> 24 == 10 && num < MemberRef.RowCount)
			{
				FieldInfo fieldInfo = GetMemberRef(num, genericTypeArguments, genericMethodArguments) as FieldInfo;
				if (fieldInfo != null)
				{
					return fieldInfo;
				}
				throw new ArgumentException($"Token 0x{metadataToken:x8} is not a valid FieldInfo token in the scope of module {Name}.", "metadataToken");
			}
			throw TokenOutOfRangeException(metadataToken);
		}

		private TypeDefImpl FindFieldOwner(int fieldIndex)
		{
			for (int i = 0; i < TypeDef.records.Length; i++)
			{
				int num = TypeDef.records[i].FieldList - 1;
				int num2 = ((TypeDef.records.Length > i + 1) ? (TypeDef.records[i + 1].FieldList - 1) : Field.records.Length);
				if (num <= fieldIndex && fieldIndex < num2)
				{
					PopulateTypeDef();
					return typeDefs[i];
				}
			}
			throw new InvalidOperationException();
		}

		internal MethodBase GetMethodAt(TypeDefImpl owner, int index)
		{
			if (methods == null)
			{
				methods = new MethodBase[MethodDef.records.Length];
			}
			if (methods[index] == null)
			{
				MethodDefImpl methodDefImpl = new MethodDefImpl(this, owner ?? FindMethodOwner(index), index);
				methods[index] = (methodDefImpl.IsConstructor ? ((MethodBase)new ConstructorInfoImpl(methodDefImpl)) : ((MethodBase)methodDefImpl));
			}
			return methods[index];
		}

		public override MethodBase ResolveMethod(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			int num = (metadataToken & 0xFFFFFF) - 1;
			if (num < 0)
			{
				throw TokenOutOfRangeException(metadataToken);
			}
			if (metadataToken >> 24 == 6 && num < MethodDef.RowCount)
			{
				return GetMethodAt(null, num);
			}
			if (metadataToken >> 24 == 10 && num < MemberRef.RowCount)
			{
				MethodBase methodBase = GetMemberRef(num, genericTypeArguments, genericMethodArguments) as MethodBase;
				if (methodBase != null)
				{
					return methodBase;
				}
				throw new ArgumentException($"Token 0x{metadataToken:x8} is not a valid MethodBase token in the scope of module {Name}.", "metadataToken");
			}
			if (metadataToken >> 24 == 43 && num < MethodSpec.RowCount)
			{
				MethodInfo obj = (MethodInfo)ResolveMethod(MethodSpec.records[num].Method, genericTypeArguments, genericMethodArguments);
				ByteReader br = ByteReader.FromBlob(blobHeap, MethodSpec.records[num].Instantiation);
				return obj.MakeGenericMethod(Signature.ReadMethodSpec(this, br, new GenericContext(genericTypeArguments, genericMethodArguments)));
			}
			throw TokenOutOfRangeException(metadataToken);
		}

		public override Type[] __ResolveOptionalParameterTypes(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments, out CustomModifiers[] customModifiers)
		{
			int num = (metadataToken & 0xFFFFFF) - 1;
			if (num < 0)
			{
				throw TokenOutOfRangeException(metadataToken);
			}
			if (metadataToken >> 24 == 10 && num < MemberRef.RowCount)
			{
				int signature = MemberRef.records[num].Signature;
				return Signature.ReadOptionalParameterTypes(this, GetBlob(signature), new GenericContext(genericTypeArguments, genericMethodArguments), out customModifiers);
			}
			if (metadataToken >> 24 == 6 && num < MethodDef.RowCount)
			{
				customModifiers = Empty<CustomModifiers>.Array;
				return Type.EmptyTypes;
			}
			throw TokenOutOfRangeException(metadataToken);
		}

		public override CustomModifiers __ResolveTypeSpecCustomModifiers(int typeSpecToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			int num = (typeSpecToken & 0xFFFFFF) - 1;
			if (typeSpecToken >> 24 != 27 || num < 0 || num >= TypeSpec.RowCount)
			{
				throw TokenOutOfRangeException(typeSpecToken);
			}
			return CustomModifiers.Read(this, ByteReader.FromBlob(blobHeap, TypeSpec.records[num]), new GenericContext(genericTypeArguments, genericMethodArguments));
		}

		private TypeDefImpl FindMethodOwner(int methodIndex)
		{
			for (int i = 0; i < TypeDef.records.Length; i++)
			{
				int num = TypeDef.records[i].MethodList - 1;
				int num2 = ((TypeDef.records.Length > i + 1) ? (TypeDef.records[i + 1].MethodList - 1) : MethodDef.records.Length);
				if (num <= methodIndex && methodIndex < num2)
				{
					PopulateTypeDef();
					return typeDefs[i];
				}
			}
			throw new InvalidOperationException();
		}

		private MemberInfo GetMemberRef(int index, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			if (memberRefs == null)
			{
				memberRefs = new MemberInfo[MemberRef.records.Length];
			}
			if (memberRefs[index] == null)
			{
				int num = MemberRef.records[index].Class;
				int signature = MemberRef.records[index].Signature;
				string name = GetString(MemberRef.records[index].Name);
				switch (num >> 24)
				{
				case 6:
					return GetMethodAt(null, (num & 0xFFFFFF) - 1);
				case 26:
					memberRefs[index] = ResolveTypeMemberRef(ResolveModuleType(num), name, ByteReader.FromBlob(blobHeap, signature));
					break;
				case 1:
				case 2:
					memberRefs[index] = ResolveTypeMemberRef(ResolveType(num), name, ByteReader.FromBlob(blobHeap, signature));
					break;
				case 27:
				{
					Type type = ResolveType(num, genericTypeArguments, genericMethodArguments);
					if (type.IsArray)
					{
						MethodSignature signature2 = MethodSignature.ReadSig(this, ByteReader.FromBlob(blobHeap, signature), new GenericContext(genericTypeArguments, genericMethodArguments));
						return type.FindMethod(name, signature2) ?? universe.GetMissingMethodOrThrow(this, type, name, signature2);
					}
					if (type.IsConstructedGenericType)
					{
						MemberInfo memberInfo = ResolveTypeMemberRef(type.GetGenericTypeDefinition(), name, ByteReader.FromBlob(blobHeap, signature));
						MethodBase methodBase = memberInfo as MethodBase;
						if (methodBase != null)
						{
							memberInfo = methodBase.BindTypeParameters(type);
						}
						FieldInfo fieldInfo = memberInfo as FieldInfo;
						if (fieldInfo != null)
						{
							memberInfo = fieldInfo.BindTypeParameters(type);
						}
						return memberInfo;
					}
					return ResolveTypeMemberRef(type, name, ByteReader.FromBlob(blobHeap, signature));
				}
				default:
					throw new BadImageFormatException();
				}
			}
			return memberRefs[index];
		}

		private Type ResolveModuleType(int token)
		{
			int num = (token & 0xFFFFFF) - 1;
			string name = GetString(ModuleRef.records[num]);
			Module module = assembly.GetModule(name);
			if (module == null || module.IsResource())
			{
				throw new BadImageFormatException();
			}
			return module.GetModuleType();
		}

		private MemberInfo ResolveTypeMemberRef(Type type, string name, ByteReader sig)
		{
			if (sig.PeekByte() == 6)
			{
				Type type2 = type;
				FieldSignature signature = FieldSignature.ReadSig(this, sig, type);
				FieldInfo fieldInfo = type.FindField(name, signature);
				if (fieldInfo == null && universe.MissingMemberResolution)
				{
					return universe.GetMissingFieldOrThrow(this, type, name, signature);
				}
				while (fieldInfo == null && (type = type.BaseType) != null)
				{
					fieldInfo = type.FindField(name, signature);
				}
				if (fieldInfo != null)
				{
					return fieldInfo;
				}
				throw new MissingFieldException(type2.ToString(), name);
			}
			Type type3 = type;
			MethodSignature signature2 = MethodSignature.ReadSig(this, sig, type);
			MethodBase methodBase = type.FindMethod(name, signature2);
			if (methodBase == null && universe.MissingMemberResolution)
			{
				return universe.GetMissingMethodOrThrow(this, type, name, signature2);
			}
			while (methodBase == null && (type = type.BaseType) != null)
			{
				methodBase = type.FindMethod(name, signature2);
			}
			if (methodBase != null)
			{
				return methodBase;
			}
			throw new MissingMethodException(type3.ToString(), name);
		}

		internal ByteReader GetStandAloneSig(int index)
		{
			return ByteReader.FromBlob(blobHeap, StandAloneSig.records[index]);
		}

		public override byte[] ResolveSignature(int metadataToken)
		{
			int num = (metadataToken & 0xFFFFFF) - 1;
			if (metadataToken >> 24 == 17 && num >= 0 && num < StandAloneSig.RowCount)
			{
				ByteReader standAloneSig = GetStandAloneSig(num);
				return standAloneSig.ReadBytes(standAloneSig.Length);
			}
			throw TokenOutOfRangeException(metadataToken);
		}

		public override __StandAloneMethodSig __ResolveStandAloneMethodSig(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			int num = (metadataToken & 0xFFFFFF) - 1;
			if (metadataToken >> 24 == 17 && num >= 0 && num < StandAloneSig.RowCount)
			{
				return MethodSignature.ReadStandAloneMethodSig(this, GetStandAloneSig(num), new GenericContext(genericTypeArguments, genericMethodArguments));
			}
			throw TokenOutOfRangeException(metadataToken);
		}

		internal MethodInfo GetEntryPoint()
		{
			if (cliHeader.EntryPointToken != 0 && (cliHeader.Flags & 0x10) == 0)
			{
				return (MethodInfo)ResolveMethod((int)cliHeader.EntryPointToken);
			}
			return null;
		}

		internal string[] GetManifestResourceNames()
		{
			string[] array = new string[ManifestResource.records.Length];
			for (int i = 0; i < ManifestResource.records.Length; i++)
			{
				array[i] = GetString(ManifestResource.records[i].Name);
			}
			return array;
		}

		internal ManifestResourceInfo GetManifestResourceInfo(string resourceName)
		{
			for (int i = 0; i < ManifestResource.records.Length; i++)
			{
				if (resourceName == GetString(ManifestResource.records[i].Name))
				{
					ManifestResourceInfo manifestResourceInfo = new ManifestResourceInfo(this, i);
					Assembly referencedAssembly = manifestResourceInfo.ReferencedAssembly;
					if (referencedAssembly != null && !referencedAssembly.__IsMissing && referencedAssembly.GetManifestResourceInfo(resourceName) == null)
					{
						return null;
					}
					return manifestResourceInfo;
				}
			}
			return null;
		}

		internal Stream GetManifestResourceStream(string resourceName)
		{
			for (int i = 0; i < ManifestResource.records.Length; i++)
			{
				if (!(resourceName == GetString(ManifestResource.records[i].Name)))
				{
					continue;
				}
				if (ManifestResource.records[i].Implementation != 637534208)
				{
					ManifestResourceInfo manifestResourceInfo = new ManifestResourceInfo(this, i);
					switch (ManifestResource.records[i].Implementation >> 24)
					{
					case 38:
					{
						string path = Path.Combine(Path.GetDirectoryName(location), manifestResourceInfo.FileName);
						if (System.IO.File.Exists(path))
						{
							FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
							if (fileStream.Length == 0L)
							{
								fileStream.Close();
								return null;
							}
							return fileStream;
						}
						return null;
					}
					case 35:
					{
						Assembly referencedAssembly = manifestResourceInfo.ReferencedAssembly;
						if (referencedAssembly.__IsMissing)
						{
							return null;
						}
						return referencedAssembly.GetManifestResourceStream(resourceName);
					}
					default:
						throw new BadImageFormatException();
					}
				}
				SeekRVA((int)cliHeader.Resources.VirtualAddress + ManifestResource.records[i].Offset);
				BinaryReader binaryReader = new BinaryReader(stream);
				int count = binaryReader.ReadInt32();
				return new MemoryStream(binaryReader.ReadBytes(count));
			}
			return null;
		}

		public override AssemblyName[] __GetReferencedAssemblies()
		{
			List<AssemblyName> list = new List<AssemblyName>();
			for (int i = 0; i < AssemblyRef.records.Length; i++)
			{
				AssemblyName assemblyName = new AssemblyName();
				assemblyName.Name = GetString(AssemblyRef.records[i].Name);
				assemblyName.Version = new Version(AssemblyRef.records[i].MajorVersion, AssemblyRef.records[i].MinorVersion, AssemblyRef.records[i].BuildNumber, AssemblyRef.records[i].RevisionNumber);
				if (AssemblyRef.records[i].PublicKeyOrToken != 0)
				{
					byte[] blobCopy = GetBlobCopy(AssemblyRef.records[i].PublicKeyOrToken);
					if ((AssemblyRef.records[i].Flags & 1) != 0)
					{
						assemblyName.SetPublicKey(blobCopy);
					}
					else
					{
						assemblyName.SetPublicKeyToken(blobCopy);
					}
				}
				else
				{
					assemblyName.SetPublicKeyToken(Empty<byte>.Array);
				}
				if (AssemblyRef.records[i].Culture != 0)
				{
					assemblyName.Culture = GetString(AssemblyRef.records[i].Culture);
				}
				else
				{
					assemblyName.Culture = "";
				}
				if (AssemblyRef.records[i].HashValue != 0)
				{
					assemblyName.hash = GetBlobCopy(AssemblyRef.records[i].HashValue);
				}
				assemblyName.RawFlags = (AssemblyNameFlags)AssemblyRef.records[i].Flags;
				list.Add(assemblyName);
			}
			return list.ToArray();
		}

		public override void __ResolveReferencedAssemblies(Assembly[] assemblies)
		{
			if (assemblyRefs == null)
			{
				assemblyRefs = new Assembly[AssemblyRef.RowCount];
			}
			for (int i = 0; i < assemblies.Length; i++)
			{
				if (assemblyRefs[i] == null)
				{
					assemblyRefs[i] = assemblies[i];
				}
			}
		}

		public override string[] __GetReferencedModules()
		{
			string[] array = new string[ModuleRef.RowCount];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = GetString(ModuleRef.records[i]);
			}
			return array;
		}

		public override Type[] __GetReferencedTypes()
		{
			Type[] array = new Type[TypeRef.RowCount];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = ResolveType((1 << 24) + i + 1);
			}
			return array;
		}

		public override Type[] __GetExportedTypes()
		{
			Type[] array = new Type[ExportedType.RowCount];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = ResolveExportedType(i);
			}
			return array;
		}

		private Type ResolveExportedType(int index)
		{
			TypeName typeName = GetTypeName(ExportedType.records[index].TypeNamespace, ExportedType.records[index].TypeName);
			int implementation = ExportedType.records[index].Implementation;
			int typeDefId = ExportedType.records[index].TypeDefId;
			int flags = ExportedType.records[index].Flags;
			switch (implementation >> 24)
			{
			case 35:
				return ResolveAssemblyRef((implementation & 0xFFFFFF) - 1).ResolveType(this, typeName).SetMetadataTokenForMissing(typeDefId, flags);
			case 39:
				return ResolveExportedType((implementation & 0xFFFFFF) - 1).ResolveNestedType(this, typeName).SetMetadataTokenForMissing(typeDefId, flags);
			case 38:
			{
				Module module = assembly.GetModule(GetString(File.records[(implementation & 0xFFFFFF) - 1].Name));
				return module.FindType(typeName) ?? module.universe.GetMissingTypeOrThrow(this, module, null, typeName).SetMetadataTokenForMissing(typeDefId, flags);
			}
			default:
				throw new BadImageFormatException();
			}
		}

		internal override Type GetModuleType()
		{
			PopulateTypeDef();
			return moduleType;
		}

		public override void __GetDataDirectoryEntry(int index, out int rva, out int length)
		{
			peFile.GetDataDirectoryEntry(index, out rva, out length);
		}

		public override long __RelativeVirtualAddressToFileOffset(int rva)
		{
			return peFile.RvaToFileOffset((uint)rva);
		}

		public override bool __GetSectionInfo(int rva, out string name, out int characteristics, out int virtualAddress, out int virtualSize, out int pointerToRawData, out int sizeOfRawData)
		{
			return peFile.GetSectionInfo(rva, out name, out characteristics, out virtualAddress, out virtualSize, out pointerToRawData, out sizeOfRawData);
		}

		public override int __ReadDataFromRVA(int rva, byte[] data, int offset, int length)
		{
			SeekRVA(rva);
			int num = 0;
			while (length > 0)
			{
				int num2 = stream.Read(data, offset, length);
				if (num2 == 0)
				{
					break;
				}
				offset += num2;
				length -= num2;
				num += num2;
			}
			return num;
		}

		public override void GetPEKind(out PortableExecutableKinds peKind, out ImageFileMachine machine)
		{
			peKind = PortableExecutableKinds.NotAPortableExecutableImage;
			if ((cliHeader.Flags & 1) != 0)
			{
				peKind |= PortableExecutableKinds.ILOnly;
			}
			switch (cliHeader.Flags & 0x20002)
			{
			case 2u:
				peKind |= PortableExecutableKinds.Required32Bit;
				break;
			case 131074u:
				peKind |= PortableExecutableKinds.Preferred32Bit;
				break;
			}
			if (peFile.OptionalHeader.Magic == 523)
			{
				peKind |= PortableExecutableKinds.PE32Plus;
			}
			machine = (ImageFileMachine)peFile.FileHeader.Machine;
		}

		public override IList<CustomAttributeData> __GetPlaceholderAssemblyCustomAttributes(bool multiple, bool security)
		{
			TypeName typeName;
			switch ((multiple ? 1 : 0) + (security ? 2 : 0))
			{
			case 0:
				typeName = new TypeName("System.Runtime.CompilerServices", "AssemblyAttributesGoHere");
				break;
			case 1:
				typeName = new TypeName("System.Runtime.CompilerServices", "AssemblyAttributesGoHereM");
				break;
			case 2:
				typeName = new TypeName("System.Runtime.CompilerServices", "AssemblyAttributesGoHereS");
				break;
			default:
				typeName = new TypeName("System.Runtime.CompilerServices", "AssemblyAttributesGoHereSM");
				break;
			}
			List<CustomAttributeData> list = new List<CustomAttributeData>();
			for (int i = 0; i < CustomAttribute.records.Length; i++)
			{
				if (CustomAttribute.records[i].Parent >> 24 == 1)
				{
					int num = (CustomAttribute.records[i].Parent & 0xFFFFFF) - 1;
					if (typeName == GetTypeName(TypeRef.records[num].TypeNamespace, TypeRef.records[num].TypeName))
					{
						list.Add(new CustomAttributeData(this, i));
					}
				}
			}
			return list;
		}

		internal override void Dispose()
		{
			if (stream != null)
			{
				stream.Close();
			}
		}

		internal override void ExportTypes(int fileToken, ModuleBuilder manifestModule)
		{
			PopulateTypeDef();
			manifestModule.ExportTypes(typeDefs, fileToken);
		}

		protected override long GetImageBaseImpl()
		{
			return (long)peFile.OptionalHeader.ImageBase;
		}

		protected override long GetStackReserveImpl()
		{
			return (long)peFile.OptionalHeader.SizeOfStackReserve;
		}

		protected override int GetFileAlignmentImpl()
		{
			return (int)peFile.OptionalHeader.FileAlignment;
		}

		protected override DllCharacteristics GetDllCharacteristicsImpl()
		{
			return (DllCharacteristics)peFile.OptionalHeader.DllCharacteristics;
		}
	}
}
