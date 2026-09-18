using System;

namespace IKVM.Reflection
{
	[Flags]
	public enum PropertyAttributes
	{
		None = 0,
		SpecialName = 0x200,
		RTSpecialName = 0x400,
		HasDefault = 0x1000
	}
}
