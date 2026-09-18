using System;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.IO;
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection.Reader
{
	internal sealed class AssemblyReader : Assembly
	{
		private const int ContainsNoMetaData = 1;

		private readonly string location;

		private readonly ModuleReader manifestModule;

		private readonly Module[] externalModules;

		public override string Location => location ?? "";

		public override string ImageRuntimeVersion => manifestModule.__ImageRuntimeVersion;

		public override Module ManifestModule => manifestModule;

		public override MethodInfo EntryPoint => manifestModule.GetEntryPoint();

		internal string Name => manifestModule.GetString(manifestModule.AssemblyTable.records[0].Name);

		internal AssemblyReader(string location, ModuleReader manifestModule)
			: base(manifestModule.universe)
		{
			this.location = location;
			this.manifestModule = manifestModule;
			externalModules = new Module[manifestModule.File.records.Length];
		}

		public override AssemblyName GetName()
		{
			return GetNameImpl(ref manifestModule.AssemblyTable.records[0]);
		}

		private AssemblyName GetNameImpl(ref AssemblyTable.Record rec)
		{
			AssemblyName assemblyName = new AssemblyName();
			assemblyName.Name = manifestModule.GetString(rec.Name);
			assemblyName.Version = new Version(rec.MajorVersion, rec.MinorVersion, rec.BuildNumber, rec.RevisionNumber);
			if (rec.PublicKey != 0)
			{
				assemblyName.SetPublicKey(manifestModule.GetBlobCopy(rec.PublicKey));
			}
			else
			{
				assemblyName.SetPublicKey(Empty<byte>.Array);
			}
			if (rec.Culture != 0)
			{
				assemblyName.Culture = manifestModule.GetString(rec.Culture);
			}
			else
			{
				assemblyName.Culture = "";
			}
			assemblyName.HashAlgorithm = (AssemblyHashAlgorithm)rec.HashAlgId;
			assemblyName.CodeBase = base.CodeBase;
			PortableExecutableKinds peKind;
			ImageFileMachine machine;
			manifestModule.GetPEKind(out peKind, out machine);
			switch (machine)
			{
			case ImageFileMachine.I386:
				if ((peKind & (PortableExecutableKinds.Required32Bit | PortableExecutableKinds.Preferred32Bit)) != PortableExecutableKinds.NotAPortableExecutableImage)
				{
					assemblyName.ProcessorArchitecture = ProcessorArchitecture.X86;
				}
				else if ((rec.Flags & 0x70) == 112)
				{
					assemblyName.ProcessorArchitecture = ProcessorArchitecture.None;
				}
				else
				{
					assemblyName.ProcessorArchitecture = ProcessorArchitecture.MSIL;
				}
				break;
			case ImageFileMachine.IA64:
				assemblyName.ProcessorArchitecture = ProcessorArchitecture.IA64;
				break;
			case ImageFileMachine.AMD64:
				assemblyName.ProcessorArchitecture = ProcessorArchitecture.Amd64;
				break;
			case ImageFileMachine.ARM:
				assemblyName.ProcessorArchitecture = ProcessorArchitecture.Arm;
				break;
			}
			assemblyName.RawFlags = (AssemblyNameFlags)rec.Flags;
			return assemblyName;
		}

		public override Type[] GetTypes()
		{
			if (externalModules.Length == 0)
			{
				return manifestModule.GetTypes();
			}
			List<Type> list = new List<Type>();
			Module[] modules = GetModules(false);
			foreach (Module module in modules)
			{
				list.AddRange(module.GetTypes());
			}
			return list.ToArray();
		}

		internal override Type FindType(TypeName typeName)
		{
			Type type = manifestModule.FindType(typeName);
			int num = 0;
			while (type == null && num < externalModules.Length)
			{
				if ((manifestModule.File.records[num].Flags & 1) == 0)
				{
					type = GetModule(num).FindType(typeName);
				}
				num++;
			}
			return type;
		}

		internal override Type FindTypeIgnoreCase(TypeName lowerCaseName)
		{
			Type type = manifestModule.FindTypeIgnoreCase(lowerCaseName);
			int num = 0;
			while (type == null && num < externalModules.Length)
			{
				if ((manifestModule.File.records[num].Flags & 1) == 0)
				{
					type = GetModule(num).FindTypeIgnoreCase(lowerCaseName);
				}
				num++;
			}
			return type;
		}

		public override Module[] GetLoadedModules(bool getResourceModules)
		{
			List<Module> list = new List<Module>();
			list.Add(manifestModule);
			Module[] array = externalModules;
			foreach (Module module in array)
			{
				if (module != null)
				{
					list.Add(module);
				}
			}
			return list.ToArray();
		}

		public override Module[] GetModules(bool getResourceModules)
		{
			if (externalModules.Length == 0)
			{
				return new Module[1] { manifestModule };
			}
			List<Module> list = new List<Module>();
			list.Add(manifestModule);
			for (int i = 0; i < manifestModule.File.records.Length; i++)
			{
				if (getResourceModules || (manifestModule.File.records[i].Flags & 1) == 0)
				{
					list.Add(GetModule(i));
				}
			}
			return list.ToArray();
		}

		public override Module GetModule(string name)
		{
			if (name.Equals(manifestModule.ScopeName, StringComparison.OrdinalIgnoreCase))
			{
				return manifestModule;
			}
			int moduleIndex = GetModuleIndex(name);
			if (moduleIndex != -1)
			{
				return GetModule(moduleIndex);
			}
			return null;
		}

		private int GetModuleIndex(string name)
		{
			for (int i = 0; i < manifestModule.File.records.Length; i++)
			{
				if (name.Equals(manifestModule.GetString(manifestModule.File.records[i].Name), StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
			}
			return -1;
		}

		private Module GetModule(int index)
		{
			if (externalModules[index] != null)
			{
				return externalModules[index];
			}
			return LoadModule(index, null, manifestModule.GetString(manifestModule.File.records[index].Name));
		}

		private Module LoadModule(int index, byte[] rawModule, string name)
		{
			string path = ((name == null) ? null : Path.Combine(Path.GetDirectoryName(location), name));
			if ((manifestModule.File.records[index].Flags & 1) != 0)
			{
				return externalModules[index] = new ResourceModule(manifestModule, index, path);
			}
			if (rawModule == null)
			{
				try
				{
					rawModule = File.ReadAllBytes(path);
				}
				catch (FileNotFoundException)
				{
					if (resolvers != null)
					{
						ResolveEventArgs e = new ResolveEventArgs(name, this);
						foreach (ModuleResolveEventHandler resolver in resolvers)
						{
							Module module = resolver(this, e);
							if (module != null)
							{
								return module;
							}
						}
					}
					if (universe.MissingMemberResolution)
					{
						return externalModules[index] = new MissingModule(this, index);
					}
					throw;
				}
			}
			return externalModules[index] = new ModuleReader(this, manifestModule.universe, new MemoryStream(rawModule), path, false);
		}

		public override Module LoadModule(string moduleName, byte[] rawModule)
		{
			int moduleIndex = GetModuleIndex(moduleName);
			if (moduleIndex == -1)
			{
				throw new ArgumentException();
			}
			if (externalModules[moduleIndex] != null)
			{
				return externalModules[moduleIndex];
			}
			return LoadModule(moduleIndex, rawModule, null);
		}

		public override string[] GetManifestResourceNames()
		{
			return manifestModule.GetManifestResourceNames();
		}

		public override ManifestResourceInfo GetManifestResourceInfo(string resourceName)
		{
			return manifestModule.GetManifestResourceInfo(resourceName);
		}

		public override Stream GetManifestResourceStream(string resourceName)
		{
			return manifestModule.GetManifestResourceStream(resourceName);
		}

		public override AssemblyName[] GetReferencedAssemblies()
		{
			return manifestModule.__GetReferencedAssemblies();
		}

		protected override AssemblyNameFlags GetAssemblyFlags()
		{
			return (AssemblyNameFlags)manifestModule.AssemblyTable.records[0].Flags;
		}

		internal override IList<CustomAttributeData> GetCustomAttributesData(Type attributeType)
		{
			IList<CustomAttributeData> customAttributesImpl = CustomAttributeData.GetCustomAttributesImpl(null, manifestModule, 536870913, attributeType);
			return customAttributesImpl ?? CustomAttributeData.EmptyList;
		}
	}
}
