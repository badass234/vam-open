namespace Leap
{
	public class DistortionEventArgs : LeapEventArgs
	{
		public DistortionData distortion { get; protected set; }

		public Image.CameraType camera { get; protected set; }

		public DistortionEventArgs(DistortionData distortion, Image.CameraType camera)
			: base(LeapEvent.EVENT_DISTORTION_CHANGE)
		{
			this.distortion = distortion;
			this.camera = camera;
		}
	}
}
