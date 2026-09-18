using System;
using System.Collections.Generic;
using System.IO;

namespace IKVM.Reflection.Reader
{
	internal sealed class ResourceModule : NonPEModule
	{
		private readonly ModuleReader manifest;

		private readonly int index;

		private readonly string location;

		public override int MDStreamVersion
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override Assembly Assembly => manifest.Assembly;

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

		public override string ScopeName => manifest.GetString(manifest.File.records[index].Name);

		public override Guid ModuleVersionId
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override byte[] __ModuleHash
		{
			get
			{
				int hashValue = manifest.File.records[index].HashValue;
				if (hashValue != 0)
				{
					return manifest.GetBlobCopy(hashValue);
				}
				return Empty<byte>.Array;
			}
		}

		internal ResourceModule(ModuleReader manifest, int index, string location)
			: base(manifest.universe)
		{
			this.manifest = manifest;
			this.index = index;
			this.location = location;
		}

		public override bool IsResource()
		{
			return true;
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
		}

		protected override Exception ArgumentOutOfRangeException()
		{
			return new NotSupportedException();
		}
	}
}
