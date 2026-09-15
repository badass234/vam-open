using System;

namespace Leap
{
	public class LeapEventArgs : EventArgs
	{
		public LeapEvent type { get; set; }

		public LeapEventArgs(LeapEvent type)
		{
			this.type = type;
		}
	}
}
