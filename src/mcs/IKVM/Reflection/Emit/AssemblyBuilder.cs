using System;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.IO;
using System.Resources;
using System.Security.Cryptography;
using IKVM.Reflection.Metadata;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Emit
{
	public sealed class AssemblyBuilder : Assembly
	{
		private struct TypeForwarder
		{
			internal readonly Type Type;

			internal readonly bool IncludeNested;

			internal TypeForwarder(Type type, bool includeNested)
			{
				Type = type;
				IncludeNested = includeNested;
			}
		}

		private struct ResourceFile
		{
			internal string Name;

			internal string FileName;

			internal ResourceAttributes Attributes;

			internal ResourceWriter Writer;
		}

		private readonly string name;

		private ushort majorVersion;

		private ushort minorVersion;

		private ushort buildVersion;

		private ushort revisionVersion;

		private string culture;

		private AssemblyNameFlags flags;

		private AssemblyHashAlgorithm hashAlgorithm;

		private StrongNameKeyPair keyPair;

		private byte[] publicKey;

		internal readonly string dir;

		private PEFileKinds fileKind = PEFileKinds.Dll;

		private MethodInfo entryPoint;

		private VersionInfo versionInfo;

		private byte[] win32icon;

		private byte[] win32manifest;

		private byte[] win32resources;

		private string imageRuntimeVersion;

		internal int mdStreamVersion = 131072;

		private Module pseudoManifestModule;

		private readonly List<ResourceFile> resourceFiles = new List<ResourceFile>();

		private readonly List<ModuleBuilder> modules = new List<ModuleBuilder>();

		private readonly List<Module> addedModules = new List<Module>();

		private readonly List<CustomAttributeBuilder> customAttributes = new List<CustomAttributeBuilder>();

		private readonly List<CustomAttributeBuilder> declarativeSecurity = new List<CustomAttributeBuilder>();

		private readonly List<TypeForwarder> typeForwarders = new List<TypeForwarder>();

		public new AssemblyNameFlags __AssemblyFlags
		{
			get
			{
				return flags;
			}
			set
			{
				AssemblyName oldName = GetName();
				flags = value;
				Rename(oldName);
			}
		}

		internal string Name => name;

		public override string Location
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override string ImageRuntimeVersion => imageRuntimeVersion;

		public override Module ManifestModule
		{
			get
			{
				if (pseudoManifestModule == null)
				{
					pseudoManifestModule = new ManifestModule(this);
				}
				return pseudoManifestModule;
			}
		}

		public override MethodInfo EntryPoint => entryPoint;

		public override bool IsDynamic => true;

		internal bool IsWindowsRuntime => (flags & (AssemblyNameFlags)0x200) != 0;

		internal AssemblyBuilder(Universe universe, AssemblyName name, string dir, IEnumerable<CustomAttributeBuilder> customAttributes)
			: base(universe)
		{
			this.name = name.Name;
			SetVersionHelper(name.Version);
			if (!string.IsNullOrEmpty(name.Culture))
			{
				culture = name.Culture;
			}
			flags = name.RawFlags;
			hashAlgorithm = name.HashAlgorithm;
			if (hashAlgorithm == AssemblyHashAlgorithm.None)
			{
				hashAlgorithm = AssemblyHashAlgorithm.SHA1;
			}
			keyPair = name.KeyPair;
			if (keyPair != null)
			{
				publicKey = keyPair.PublicKey;
			}
			else
			{
				byte[] array = name.GetPublicKey();
				if (array != null && array.Length != 0)
				{
					publicKey = (byte[])array.Clone();
				}
			}
			this.dir = dir ?? ".";
			if (customAttributes != null)
			{
				this.customAttributes.AddRange(customAttributes);
			}
			if (universe.HasMscorlib && !universe.Mscorlib.__IsMissing && universe.Mscorlib.ImageRuntimeVersion != null)
			{
				imageRuntimeVersion = universe.Mscorlib.ImageRuntimeVersion;
			}
			else
			{
				imageRuntimeVersion = typeof(object).Assembly.ImageRuntimeVersion;
			}
			universe.RegisterDynamicAssembly(this);
		}

		private void SetVersionHelper(Version version)
		{
			if (version == null)
			{
				majorVersion = 0;
				minorVersion = 0;
				buildVersion = 0;
				revisionVersion = 0;
			}
			else
			{
				majorVersion = (ushort)version.Major;
				minorVersion = (ushort)version.Minor;
				buildVersion = (ushort)((version.Build != -1) ? ((ushort)version.Build) : 0);
				revisionVersion = (ushort)((version.Revision != -1) ? ((ushort)version.Revision) : 0);
			}
		}

		private void Rename(AssemblyName oldName)
		{
			fullName = null;
			universe.RenameAssembly(this, oldName);
		}

		public void __SetAssemblyVersion(Version version)
		{
			AssemblyName oldName = GetName();
			SetVersionHelper(version);
			Rename(oldName);
		}

		public void __SetAssemblyCulture(string cultureName)
		{
			AssemblyName oldName = GetName();
			culture = cultureName;
			Rename(oldName);
		}

		public void __SetAssemblyKeyPair(StrongNameKeyPair keyPair)
		{
			AssemblyName oldName = GetName();
			this.keyPair = keyPair;
			if (keyPair != null)
			{
				publicKey = keyPair.PublicKey;
			}
			Rename(oldName);
		}

		public void __SetAssemblyPublicKey(byte[] publicKey)
		{
			AssemblyName oldName = GetName();
			this.publicKey = ((publicKey == null) ? null : ((byte[])publicKey.Clone()));
			Rename(oldName);
		}

		public void __SetAssemblyAlgorithmId(AssemblyHashAlgorithm hashAlgorithm)
		{
			this.hashAlgorithm = hashAlgorithm;
		}

		[Obsolete("Use __AssemblyFlags property instead.")]
		public void __SetAssemblyFlags(AssemblyNameFlags flags)
		{
			__AssemblyFlags = flags;
		}

		protected override AssemblyNameFlags GetAssemblyFlags()
		{
			return flags;
		}

		public override AssemblyName GetName()
		{
			AssemblyName assemblyName = new AssemblyName();
			assemblyName.Name = name;
			assemblyName.Version = new Version(majorVersion, minorVersion, buildVersion, revisionVersion);
			assemblyName.Culture = culture ?? "";
			assemblyName.HashAlgorithm = hashAlgorithm;
			assemblyName.RawFlags = flags;
			assemblyName.SetPublicKey((publicKey != null) ? ((byte[])publicKey.Clone()) : Empty<byte>.Array);
			assemblyName.KeyPair = keyPair;
			return assemblyName;
		}

		public ModuleBuilder DefineDynamicModule(string name, string fileName)
		{
			return DefineDynamicModule(name, fileName, false);
		}

		public ModuleBuilder DefineDynamicModule(string name, string fileName, bool emitSymbolInfo)
		{
			ModuleBuilder moduleBuilder = new ModuleBuilder(this, name, fileName, emitSymbolInfo);
			modules.Add(moduleBuilder);
			return moduleBuilder;
		}

		public ModuleBuilder GetDynamicModule(string name)
		{
			foreach (ModuleBuilder module in modules)
			{
				if (module.Name == name)
				{
					return module;
				}
			}
			return null;
		}

		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			SetCustomAttribute(new CustomAttributeBuilder(con, binaryAttribute));
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			customAttributes.Add(customBuilder);
		}

		public void __AddDeclarativeSecurity(CustomAttributeBuilder customBuilder)
		{
			declarativeSecurity.Add(customBuilder);
		}

		public void __AddTypeForwarder(Type type)
		{
			__AddTypeForwarder(type, true);
		}

		public void __AddTypeForwarder(Type type, bool includeNested)
		{
			typeForwarders.Add(new TypeForwarder(type, includeNested));
		}

		public void SetEntryPoint(MethodInfo entryMethod)
		{
			SetEntryPoint(entryMethod, PEFileKinds.ConsoleApplication);
		}

		public void SetEntryPoint(MethodInfo entryMethod, PEFileKinds fileKind)
		{
			entryPoint = entryMethod;
			this.fileKind = fileKind;
		}

		public void __Save(Stream stream, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
		{
			if (!stream.CanRead || !stream.CanWrite || !stream.CanSeek || stream.Position != 0L)
			{
				throw new ArgumentException("Stream must support read/write/seek and current position must be zero.", "stream");
			}
			if (modules.Count != 1)
			{
				throw new NotSupportedException("Saving to a stream is only supported for single module assemblies.");
			}
			SaveImpl(modules[0].fileName, stream, portableExecutableKind, imageFileMachine);
		}

		public void Save(string assemblyFileName)
		{
			Save(assemblyFileName, PortableExecutableKinds.ILOnly, ImageFileMachine.I386);
		}

		public void Save(string assemblyFileName, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
		{
			SaveImpl(assemblyFileName, null, portableExecutableKind, imageFileMachine);
		}

		private void SaveImpl(string assemblyFileName, Stream streamOrNull, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
		{
			ModuleBuilder moduleBuilder = null;
			foreach (ModuleBuilder module in modules)
			{
				module.SetIsSaved();
				module.PopulatePropertyAndEventTables();
				if (moduleBuilder == null && string.Compare(module.fileName, assemblyFileName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					moduleBuilder = module;
				}
			}
			if (moduleBuilder == null)
			{
				moduleBuilder = DefineDynamicModule("RefEmit_OnDiskManifestModule", assemblyFileName, false);
			}
			AssemblyTable.Record newRecord = new AssemblyTable.Record
			{
				HashAlgId = (int)hashAlgorithm,
				Name = moduleBuilder.Strings.Add(name),
				MajorVersion = majorVersion,
				MinorVersion = minorVersion,
				BuildNumber = buildVersion,
				RevisionNumber = revisionVersion
			};
			if (publicKey != null)
			{
				newRecord.PublicKey = moduleBuilder.Blobs.Add(ByteBuffer.Wrap(publicKey));
				newRecord.Flags = (int)(flags | AssemblyNameFlags.PublicKey);
			}
			else
			{
				newRecord.Flags = (int)(flags & ~AssemblyNameFlags.PublicKey);
			}
			if (culture != null)
			{
				newRecord.Culture = moduleBuilder.Strings.Add(culture);
			}
			moduleBuilder.AssemblyTable.AddRecord(newRecord);
			ResourceSection resourceSection = ((versionInfo != null || win32icon != null || win32manifest != null || win32resources != null) ? new ResourceSection() : null);
			if (versionInfo != null)
			{
				versionInfo.SetName(GetName());
				versionInfo.SetFileName(assemblyFileName);
				foreach (CustomAttributeBuilder customAttribute in customAttributes)
				{
					if (!customAttribute.HasBlob || universe.DecodeVersionInfoAttributeBlobs)
					{
						versionInfo.SetAttribute(this, customAttribute);
					}
				}
				ByteBuffer bb = new ByteBuffer(512);
				versionInfo.Write(bb);
				resourceSection.AddVersionInfo(bb);
			}
			if (win32icon != null)
			{
				resourceSection.AddIcon(win32icon);
			}
			if (win32manifest != null)
			{
				resourceSection.AddManifest(win32manifest, (ushort)((fileKind != PEFileKinds.Dll) ? 1 : 2));
			}
			if (win32resources != null)
			{
				resourceSection.ExtractResources(win32resources);
			}
			foreach (CustomAttributeBuilder customAttribute2 in customAttributes)
			{
				moduleBuilder.SetCustomAttribute(536870913, customAttribute2);
			}
			moduleBuilder.AddDeclarativeSecurity(536870913, declarativeSecurity);
			foreach (TypeForwarder typeForwarder in typeForwarders)
			{
				moduleBuilder.AddTypeForwarder(typeForwarder.Type, typeForwarder.IncludeNested);
			}
			foreach (ResourceFile resourceFile in resourceFiles)
			{
				if (resourceFile.Writer != null)
				{
					resourceFile.Writer.Generate();
					resourceFile.Writer.Close();
				}
				int implementation = AddFile(moduleBuilder, resourceFile.FileName, 1);
				ManifestResourceTable.Record newRecord2 = new ManifestResourceTable.Record
				{
					Offset = 0,
					Flags = (int)resourceFile.Attributes,
					Name = moduleBuilder.Strings.Add(resourceFile.Name),
					Implementation = implementation
				};
				moduleBuilder.ManifestResource.AddRecord(newRecord2);
			}
			int num = 0;
			foreach (ModuleBuilder module2 in modules)
			{
				module2.FillAssemblyRefTable();
				module2.EmitResources();
				if (module2 != moduleBuilder)
				{
					int fileToken;
					if (entryPoint != null && entryPoint.Module == module2)
					{
						ModuleWriter.WriteModule(null, null, module2, fileKind, portableExecutableKind, imageFileMachine, module2.unmanagedResources, entryPoint.MetadataToken);
						num = (fileToken = AddFile(moduleBuilder, module2.fileName, 0));
					}
					else
					{
						ModuleWriter.WriteModule(null, null, module2, fileKind, portableExecutableKind, imageFileMachine, module2.unmanagedResources, 0);
						fileToken = AddFile(moduleBuilder, module2.fileName, 0);
					}
					module2.ExportTypes(fileToken, moduleBuilder);
				}
				module2.CloseResources();
			}
			foreach (Module addedModule in addedModules)
			{
				int fileToken2 = AddFile(moduleBuilder, addedModule.FullyQualifiedName, 0);
				addedModule.ExportTypes(fileToken2, moduleBuilder);
			}
			if (num == 0 && entryPoint != null)
			{
				num = entryPoint.MetadataToken;
			}
			ModuleWriter.WriteModule(keyPair, publicKey, moduleBuilder, fileKind, portableExecutableKind, imageFileMachine, resourceSection ?? moduleBuilder.unmanagedResources, num, streamOrNull);
		}

		private int AddFile(ModuleBuilder manifestModule, string fileName, int flags)
		{
			SHA1Managed sHA1Managed = new SHA1Managed();
			string path = fileName;
			if (dir != null)
			{
				path = Path.Combine(dir, fileName);
			}
			using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				using (CryptoStream cs = new CryptoStream(Stream.Null, sHA1Managed, CryptoStreamMode.Write))
				{
					byte[] buf = new byte[8192];
					ModuleWriter.HashChunk(fileStream, cs, buf, (int)fileStream.Length);
				}
			}
			return manifestModule.__AddModule(flags, Path.GetFileName(fileName), sHA1Managed.Hash);
		}

		public void AddResourceFile(string name, string fileName)
		{
			AddResourceFile(name, fileName, ResourceAttributes.Public);
		}

		public void AddResourceFile(string name, string fileName, ResourceAttributes attribs)
		{
			ResourceFile item = new ResourceFile
			{
				Name = name,
				FileName = fileName,
				Attributes = attribs
			};
			resourceFiles.Add(item);
		}

		public IResourceWriter DefineResource(string name, string description, string fileName)
		{
			return DefineResource(name, description, fileName, ResourceAttributes.Public);
		}

		public IResourceWriter DefineResource(string name, string description, string fileName, ResourceAttributes attribute)
		{
			string fileName2 = fileName;
			if (dir != null)
			{
				fileName2 = Path.Combine(dir, fileName);
			}
			ResourceWriter resourceWriter = new ResourceWriter(fileName2);
			ResourceFile item = default(ResourceFile);
			item.Name = name;
			item.FileName = fileName;
			item.Attributes = attribute;
			item.Writer = resourceWriter;
			resourceFiles.Add(item);
			return resourceWriter;
		}

		public void DefineVersionInfoResource()
		{
			if (versionInfo != null || win32resources != null)
			{
				throw new ArgumentException("Native resource has already been defined.");
			}
			versionInfo = new VersionInfo();
		}

		public void DefineVersionInfoResource(string product, string productVersion, string company, string copyright, string trademark)
		{
			if (versionInfo != null || win32resources != null)
			{
				throw new ArgumentException("Native resource has already been defined.");
			}
			versionInfo = new VersionInfo();
			versionInfo.product = product;
			versionInfo.informationalVersion = productVersion;
			versionInfo.company = company;
			versionInfo.copyright = copyright;
			versionInfo.trademark = trademark;
		}

		public void __DefineIconResource(byte[] iconFile)
		{
			if (win32icon != null || win32resources != null)
			{
				throw new ArgumentException("Native resource has already been defined.");
			}
			win32icon = (byte[])iconFile.Clone();
		}

		public void __DefineManifestResource(byte[] manifest)
		{
			if (win32manifest != null || win32resources != null)
			{
				throw new ArgumentException("Native resource has already been defined.");
			}
			win32manifest = (byte[])manifest.Clone();
		}

		public void __DefineUnmanagedResource(byte[] resource)
		{
			if (versionInfo != null || win32icon != null || win32manifest != null || win32resources != null)
			{
				throw new ArgumentException("Native resource has already been defined.");
			}
			win32resources = (byte[])resource.Clone();
		}

		public void DefineUnmanagedResource(string resourceFileName)
		{
			__DefineUnmanagedResource(File.ReadAllBytes(resourceFileName));
		}

		public override Type[] GetTypes()
		{
			List<Type> list = new List<Type>();
			foreach (ModuleBuilder module in modules)
			{
				module.GetTypesImpl(list);
			}
			foreach (Module addedModule in addedModules)
			{
				addedModule.GetTypesImpl(list);
			}
			return list.ToArray();
		}

		internal override Type FindType(TypeName typeName)
		{
			foreach (ModuleBuilder module in modules)
			{
				Type type = module.FindType(typeName);
				if (type != null)
				{
					return type;
				}
			}
			foreach (Module addedModule in addedModules)
			{
				Type type2 = addedModule.FindType(typeName);
				if (type2 != null)
				{
					return type2;
				}
			}
			return null;
		}

		internal override Type FindTypeIgnoreCase(TypeName lowerCaseName)
		{
			foreach (ModuleBuilder module in modules)
			{
				Type type = module.FindTypeIgnoreCase(lowerCaseName);
				if (type != null)
				{
					return type;
				}
			}
			foreach (Module addedModule in addedModules)
			{
				Type type2 = addedModule.FindTypeIgnoreCase(lowerCaseName);
				if (type2 != null)
				{
					return type2;
				}
			}
			return null;
		}

		public void __SetImageRuntimeVersion(string imageRuntimeVersion, int mdStreamVersion)
		{
			this.imageRuntimeVersion = imageRuntimeVersion;
			this.mdStreamVersion = mdStreamVersion;
		}

		public override AssemblyName[] GetReferencedAssemblies()
		{
			return Empty<AssemblyName>.Array;
		}

		public override Module[] GetLoadedModules(bool getResourceModules)
		{
			return GetModules(getResourceModules);
		}

		public override Module[] GetModules(bool getResourceModules)
		{
			List<Module> list = new List<Module>();
			foreach (ModuleBuilder module in modules)
			{
				if (getResourceModules || !module.IsResource())
				{
					list.Add(module);
				}
			}
			foreach (Module addedModule in addedModules)
			{
				if (getResourceModules || !addedModule.IsResource())
				{
					list.Add(addedModule);
				}
			}
			return list.ToArray();
		}

		public override Module GetModule(string name)
		{
			foreach (ModuleBuilder module in modules)
			{
				if (module.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					return module;
				}
			}
			foreach (Module addedModule in addedModules)
			{
				if (addedModule.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					return addedModule;
				}
			}
			return null;
		}

		public Module __AddModule(RawModule module)
		{
			Module module2 = module.ToModule(this);
			addedModules.Add(module2);
			return module2;
		}

		public override ManifestResourceInfo GetManifestResourceInfo(string resourceName)
		{
			throw new NotSupportedException();
		}

		public override string[] GetManifestResourceNames()
		{
			throw new NotSupportedException();
		}

		public override Stream GetManifestResourceStream(string resourceName)
		{
			throw new NotSupportedException();
		}

		public static AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access)
		{
			return new Universe().DefineDynamicAssembly(name, access);
		}

		public static AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, IEnumerable<CustomAttributeBuilder> assemblyAttributes)
		{
			return new Universe().DefineDynamicAssembly(name, access, assemblyAttributes);
		}

		internal override IList<CustomAttributeData> GetCustomAttributesData(Type attributeType)
		{
			List<CustomAttributeData> list = new List<CustomAttributeData>();
			foreach (CustomAttributeBuilder customAttribute in customAttributes)
			{
				if (attributeType == null || attributeType.IsAssignableFrom(customAttribute.Constructor.DeclaringType))
				{
					list.Add(customAttribute.ToData(this));
				}
			}
			return list;
		}
	}
}
