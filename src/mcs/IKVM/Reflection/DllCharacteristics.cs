using System;

namespace IKVM.Reflection
{
	[Flags]
	public enum DllCharacteristics
	{
		HighEntropyVA = 0x20,
		DynamicBase = 0x40,
		NoSEH = 0x400,
		NXCompat = 0x100,
		AppContainer = 0x1000,
		TerminalServerAware = 0x8000
	}
}
