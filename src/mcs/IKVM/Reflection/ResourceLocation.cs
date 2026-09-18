using System;

namespace IKVM.Reflection
{
	[Flags]
	public enum ResourceLocation
	{
		Embedded = 1,
		ContainedInAnotherAssembly = 2,
		ContainedInManifestFile = 4
	}
}
