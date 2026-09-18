using System;
using System.Collections.Generic;
using System.IO;

namespace IKVM.Reflection.Emit
{
	internal sealed class ManifestModule : NonPEModule
	{
		private readonly AssemblyBuilder assembly;

		private readonly Guid guid = Guid.NewGuid();

		public override int MDStreamVersion => assembly.mdStreamVersion;

		public override Assembly Assembly => assembly;

		public override string FullyQualifiedName => Path.Combine(assembly.dir, "RefEmit_InMemoryManifestModule");

		public override string Name => "<In Memory Module>";

		public override Guid ModuleVersionId => guid;

		public override string ScopeName => "RefEmit_InMemoryManifestModule";

		internal ManifestModule(AssemblyBuilder assembly)
			: base(assembly.universe)
		{
			this.assembly = assembly;
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

		protected override Exception NotSupportedException()
		{
			return new InvalidOperationException();
		}
	}
}
