using System;

public struct ovrAvatarMeshAssetData
{
	public uint vertexCount;

	public IntPtr vertexBuffer;

	public uint indexCount;

	public IntPtr indexBuffer;

	public ovrAvatarSkinnedMeshPose skinnedBindPose;
}
