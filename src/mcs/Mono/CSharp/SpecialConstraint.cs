using System;

namespace Mono.CSharp
{
	[Flags]
	public enum SpecialConstraint
	{
		None = 0,
		Constructor = 4,
		Class = 8,
		Struct = 0x10
	}
}
