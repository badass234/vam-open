using System;

namespace IKVM.Reflection
{
	[Flags]
	public enum ImplMapFlags
	{
		NoMangle = 1,
		CharSetMask = 6,
		CharSetNotSpec = 0,
		CharSetAnsi = 2,
		CharSetUnicode = 4,
		CharSetAuto = CharSetMask,
		SupportsLastError = 0x40,
		CallConvMask = 0x700,
		CallConvWinapi = 0x100,
		CallConvCdecl = 0x200,
		CallConvStdcall = 0x300,
		CallConvThiscall = 0x400,
		CallConvFastcall = 0x500,
		BestFitOn = 0x10,
		BestFitOff = 0x20,
		CharMapErrorOn = 0x1000,
		CharMapErrorOff = 0x2000
	}
}
