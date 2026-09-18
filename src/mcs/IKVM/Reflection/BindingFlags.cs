using System;

namespace IKVM.Reflection
{
	[Flags]
	public enum BindingFlags
	{
		Default = 0,
		IgnoreCase = 1,
		DeclaredOnly = 2,
		Instance = 4,
		Static = 8,
		Public = 0x10,
		NonPublic = 0x20,
		FlattenHierarchy = 0x40
	}
}
