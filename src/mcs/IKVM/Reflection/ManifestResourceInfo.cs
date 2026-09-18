using IKVM.Reflection.Reader;

namespace IKVM.Reflection
{
	public sealed class ManifestResourceInfo
	{
		private readonly ModuleReader module;

		private readonly int index;

		public ResourceAttributes __ResourceAttributes => (ResourceAttributes)module.ManifestResource.records[index].Flags;

		public int __Offset => module.ManifestResource.records[index].Offset;

		public ResourceLocation ResourceLocation
		{
			get
			{
				int implementation = module.ManifestResource.records[index].Implementation;
				if (implementation >> 24 == 35)
				{
					Assembly referencedAssembly = ReferencedAssembly;
					if (referencedAssembly == null || referencedAssembly.__IsMissing)
					{
						return ResourceLocation.ContainedInAnotherAssembly;
					}
					return referencedAssembly.GetManifestResourceInfo(module.GetString(module.ManifestResource.records[index].Name)).ResourceLocation | ResourceLocation.ContainedInAnotherAssembly;
				}
				if (implementation >> 24 == 38)
				{
					if ((implementation & 0xFFFFFF) == 0)
					{
						return ResourceLocation.Embedded | ResourceLocation.ContainedInManifestFile;
					}
					return (ResourceLocation)0;
				}
				throw new BadImageFormatException();
			}
		}

		public Assembly ReferencedAssembly
		{
			get
			{
				int implementation = module.ManifestResource.records[index].Implementation;
				if (implementation >> 24 == 35)
				{
					return module.ResolveAssemblyRef((implementation & 0xFFFFFF) - 1);
				}
				return null;
			}
		}

		public string FileName
		{
			get
			{
				int implementation = module.ManifestResource.records[index].Implementation;
				if (implementation >> 24 == 38)
				{
					if ((implementation & 0xFFFFFF) == 0)
					{
						return null;
					}
					return module.GetString(module.File.records[(implementation & 0xFFFFFF) - 1].Name);
				}
				return null;
			}
		}

		internal ManifestResourceInfo(ModuleReader module, int index)
		{
			this.module = module;
			this.index = index;
		}
	}
}
