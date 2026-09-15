using LeapInternal;

namespace Leap
{
	public class InternalFrameEventArgs : LeapEventArgs
	{
		public LEAP_TRACKING_EVENT frame { get; set; }

		public InternalFrameEventArgs(ref LEAP_TRACKING_EVENT frame)
			: base(LeapEvent.EVENT_INTERNAL_FRAME)
		{
			this.frame = frame;
		}
	}
}
