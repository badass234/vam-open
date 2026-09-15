namespace Leap
{
	public class ConnectionLostEventArgs : LeapEventArgs
	{
		public ConnectionLostEventArgs()
			: base(LeapEvent.EVENT_CONNECTION_LOST)
		{
		}
	}
}
