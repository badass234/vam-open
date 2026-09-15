namespace Leap
{
	public class FrameEventArgs : LeapEventArgs
	{
		public Frame frame { get; set; }

		public FrameEventArgs(Frame frame)
			: base(LeapEvent.EVENT_FRAME)
		{
			this.frame = frame;
		}
	}
}
