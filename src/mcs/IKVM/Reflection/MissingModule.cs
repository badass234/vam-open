using System;
using System.Collections.Generic;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Reader;

namespace IKVM.Reflection
{
	internal sealed class MissingModule : NonPEModule
	{
		private readonly Assembly assembly;

		private readonly int index;

		public override int MDStreamVersion
		{
			get
			{
				throw new MissingModuleException(this);
			}
		}

		public override Assembly Assembly => assembly;

		public override string FullyQualifiedName
		{
			get
			{
				throw new MissingModuleException(this);
			}
		}

		public override string Name
		{
			get
			{
				if (index == -1)
				{
					throw new MissingModuleException(this);
				}
				return assembly.ManifestModule.GetString(assembly.ManifestModule.File.records[index].Name);
			}
		}

		public override Guid ModuleVersionId
		{
			get
			{
				throw new MissingModuleException(this);
			}
		}

		public override string ScopeName
		{
			get
			{
				throw new MissingModuleException(this);
			}
		}

		public override int __Subsystem
		{
			get
			{
				throw new MissingModuleException(this);
			}
		}

		public override bool __IsMissing => true;

		public override byte[] __ModuleHash
		{
			get
			{
				if (index == -1)
				{
					throw new MissingModuleException(this);
				}
				if (assembly.ManifestModule.File.records[index].HashValue == 0)
				{
					return null;
				}
				ByteReader blob = assembly.ManifestModule.GetBlob(assembly.ManifestModule.File.records[index].HashValue);
				return blob.ReadBytes(blob.Length);
			}
		}

		internal MissingModule(Assembly assembly, int index)
			: base(assembly.universe)
		{
			this.assembly = assembly;
			this.index = index;
		}

		internal override Type FindType(TypeName typeName)
		{
			return null;
		}

		internal override Type FindTypeIgnoreCase(TypeName lowerCaseName)
		{
			return null;
		}

		internal override void GetTypesImpl(List<Type> list)
		{
			throw new MissingModuleException(this);
		}

		public override void __GetDataDirectoryEntry(int index, out int rva, out int length)
		{
			throw new MissingModuleException(this);
		}

		public override IList<CustomAttributeData> __GetPlaceholderAssemblyCustomAttributes(bool multiple, bool security)
		{
			throw new MissingModuleException(this);
		}

		public override long __RelativeVirtualAddressToFileOffset(int rva)
		{
			throw new MissingModuleException(this);
		}

		public override __StandAloneMethodSig __ResolveStandAloneMethodSig(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw new MissingModuleException(this);
		}

		internal override void ExportTypes(int fileToken, ModuleBuilder manifestModule)
		{
			throw new MissingModuleException(this);
		}

		public override void GetPEKind(out PortableExecutableKinds peKind, out ImageFileMachine machine)
		{
			throw new MissingModuleException(this);
		}

		protected override Exception InvalidOperationException()
		{
			return new MissingModuleException(this);
		}

		protected override Exception NotSupportedException()
		{
			return new MissingModuleException(this);
		}

		protected override Exception ArgumentOutOfRangeException()
		{
			return new MissingModuleException(this);
		}
	}
}
