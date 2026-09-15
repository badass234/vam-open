using System.Runtime.InteropServices;

public struct ovrAvatarMeshVertex
{
	public float x;

	public float y;

	public float z;

	public float nx;

	public float ny;

	public float nz;

	public float tx;

	public float ty;

	public float tz;

	public float tw;

	public float u;

	public float v;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
	public byte[] blendIndices;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
	public float[] blendWeights;
}
