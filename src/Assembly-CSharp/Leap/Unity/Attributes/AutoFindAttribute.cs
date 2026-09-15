using System;

namespace Leap.Unity.Attributes
{
	[Obsolete]
	public class AutoFindAttribute : Attribute
	{
		public readonly AutoFindLocations searchLocations;

		public AutoFindAttribute(AutoFindLocations searchLocations = AutoFindLocations.All)
		{
			this.searchLocations = searchLocations;
		}
	}
}
