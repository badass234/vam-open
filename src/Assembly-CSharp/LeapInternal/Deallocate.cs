using System;
using System.Runtime.InteropServices;

namespace LeapInternal
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void Deallocate(IntPtr buffer, IntPtr state);
}
