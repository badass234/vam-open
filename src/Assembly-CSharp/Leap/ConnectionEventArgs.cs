namespace Leap
{
	public class ConnectionEventArgs : LeapEventArgs
	{
		public ConnectionEventArgs()
			: base(LeapEvent.EVENT_CONNECTION)
		{
		}
	}
}
