using System;

namespace Mono.CSharp
{
	[Flags]
	public enum BindingRestriction
	{
		None = 0,
		DeclaredOnly = 2,
		InstanceOnly = 4,
		NoAccessors = 8,
		OverrideOnly = 0x10
	}
}
