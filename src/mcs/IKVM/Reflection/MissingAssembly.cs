using System.Collections.Generic;
using System.IO;

namespace IKVM.Reflection
{
	internal sealed class MissingAssembly : Assembly
	{
		private readonly MissingModule module;

		public override string ImageRuntimeVersion
		{
			get
			{
				throw new MissingAssemblyException(this);
			}
		}

		public override Module ManifestModule => module;

		public override MethodInfo EntryPoint
		{
			get
			{
				throw new MissingAssemblyException(this);
			}
		}

		public override string Location
		{
			get
			{
				throw new MissingAssemblyException(this);
			}
		}

		public override bool __IsMissing => true;

		internal MissingAssembly(Universe universe, string name)
			: base(universe)
		{
			module = new MissingModule(this, -1);
			fullName = name;
		}

		public override Type[] GetTypes()
		{
			throw new MissingAssemblyException(this);
		}

		public override AssemblyName GetName()
		{
			return new AssemblyName(fullName);
		}

		public override AssemblyName[] GetReferencedAssemblies()
		{
			throw new MissingAssemblyException(this);
		}

		public override Module[] GetModules(bool getResourceModules)
		{
			throw new MissingAssemblyException(this);
		}

		public override Module[] GetLoadedModules(bool getResourceModules)
		{
			throw new MissingAssemblyException(this);
		}

		public override Module GetModule(string name)
		{
			throw new MissingAssemblyException(this);
		}

		public override string[] GetManifestResourceNames()
		{
			throw new MissingAssemblyException(this);
		}

		public override ManifestResourceInfo GetManifestResourceInfo(string resourceName)
		{
			throw new MissingAssemblyException(this);
		}

		public override Stream GetManifestResourceStream(string resourceName)
		{
			throw new MissingAssemblyException(this);
		}

		internal override Type FindType(TypeName typeName)
		{
			return null;
		}

		internal override Type FindTypeIgnoreCase(TypeName lowerCaseName)
		{
			return null;
		}

		internal override IList<CustomAttributeData> GetCustomAttributesData(Type attributeType)
		{
			throw new MissingAssemblyException(this);
		}
	}
}
