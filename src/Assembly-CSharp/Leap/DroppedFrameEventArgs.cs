using LeapInternal;

namespace Leap
{
	public class DroppedFrameEventArgs : LeapEventArgs
	{
		public long frameID { get; set; }

		public eLeapDroppedFrameType reason { get; set; }

		public DroppedFrameEventArgs(long frame_id, eLeapDroppedFrameType type)
			: base(LeapEvent.EVENT_DROPPED_FRAME)
		{
			frameID = frame_id;
			reason = type;
		}
	}
}
