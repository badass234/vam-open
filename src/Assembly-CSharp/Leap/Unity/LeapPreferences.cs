using System;

namespace Leap.Unity
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class LeapPreferences : Attribute
	{
		public readonly string header;

		public readonly int order;

		public LeapPreferences(string header, int order)
		{
			this.header = header;
			this.order = order;
		}
	}
}
