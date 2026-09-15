using System;
using System.Runtime.InteropServices;

namespace LeapInternal
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAP_ALLOCATOR
	{
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public Allocate allocate;

		[MarshalAs(UnmanagedType.FunctionPtr)]
		public Deallocate deallocate;

		public IntPtr state;
	}
}
