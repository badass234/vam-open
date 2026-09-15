using System;

namespace Battlehub.RTSaveLoad
{
	[Flags]
	public enum AssetTypeHint
	{
		Prefab = 1,
		Material = 2,
		ProceduralMaterial = 4,
		Mesh = 8,
		Texture = 0x10,
		All = 0x1F
	}
}
