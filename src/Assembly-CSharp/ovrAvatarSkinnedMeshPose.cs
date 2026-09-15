using System;
using System.Runtime.InteropServices;

public struct ovrAvatarSkinnedMeshPose
{
	public uint jointCount;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
	public ovrAvatarTransform[] jointTransform;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
	public int[] jointParents;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
	public IntPtr[] jointNames;
}
