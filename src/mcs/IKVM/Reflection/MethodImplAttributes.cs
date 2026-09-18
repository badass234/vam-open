using System;

namespace IKVM.Reflection
{
	[Flags]
	public enum MethodImplAttributes
	{
		CodeTypeMask = 3,
		IL = 0,
		Native = 1,
		OPTIL = 2,
		Runtime = CodeTypeMask,
		ManagedMask = 4,
		Unmanaged = ManagedMask,
		Managed = 0,
		ForwardRef = 0x10,
		PreserveSig = 0x80,
		InternalCall = 0x1000,
		Synchronized = 0x20,
		NoInlining = 8,
		NoOptimization = 0x40,
		AggressiveInlining = 0x100,
		MaxMethodImplVal = 0xFFFF
	}
}
