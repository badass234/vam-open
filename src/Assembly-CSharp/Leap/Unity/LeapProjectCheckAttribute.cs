using System;

namespace Leap.Unity
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class LeapProjectCheckAttribute : Attribute
	{
		public string header;

		public int order;

		public LeapProjectCheckAttribute(string header, int order)
		{
			this.header = header;
			this.order = order;
		}
	}
}
