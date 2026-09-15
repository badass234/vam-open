namespace Leap
{
	public class DeviceEventArgs : LeapEventArgs
	{
		public Device Device { get; set; }

		public DeviceEventArgs(Device device)
			: base(LeapEvent.EVENT_DEVICE)
		{
			Device = device;
		}
	}
}
