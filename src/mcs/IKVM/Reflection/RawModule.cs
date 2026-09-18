using System;
using IKVM.Reflection.Reader;

namespace IKVM.Reflection
{
	public sealed class RawModule : IDisposable
	{
		private readonly ModuleReader module;

		private readonly bool isManifestModule;

		private bool imported;

		public string Location => module.FullyQualifiedName;

		public bool IsManifestModule => isManifestModule;

		public Guid ModuleVersionId => module.ModuleVersionId;

		public string ImageRuntimeVersion => module.__ImageRuntimeVersion;

		public int MDStreamVersion => module.MDStreamVersion;

		internal RawModule(ModuleReader module)
		{
			this.module = module;
			isManifestModule = module.Assembly != null;
		}

		private void CheckManifestModule()
		{
			if (!IsManifestModule)
			{
				throw new BadImageFormatException("Module does not contain a manifest");
			}
		}

		public AssemblyName GetAssemblyName()
		{
			CheckManifestModule();
			return module.Assembly.GetName();
		}

		public AssemblyName[] GetReferencedAssemblies()
		{
			return module.__GetReferencedAssemblies();
		}

		public void Dispose()
		{
			if (!imported)
			{
				module.Dispose();
			}
		}

		internal AssemblyReader ToAssembly()
		{
			if (imported)
			{
				throw new InvalidOperationException();
			}
			imported = true;
			return (AssemblyReader)module.Assembly;
		}

		internal Module ToModule(Assembly assembly)
		{
			if (module.Assembly != null)
			{
				throw new InvalidOperationException();
			}
			imported = true;
			module.SetAssembly(assembly);
			return module;
		}
	}
}
