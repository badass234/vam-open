using System;
using System.Runtime.InteropServices;

namespace LeapInternal
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate IntPtr Allocate(uint size, eLeapAllocatorType typeHint, IntPtr state);
}
