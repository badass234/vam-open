using System;

namespace Leap.Unity.Attributes
{
	[Obsolete]
	public enum AutoFindLocations
	{
		Object = 1,
		Children = 2,
		Parents = 4,
		Scene = 8,
		All = 65535
	}
}
