namespace Leap
{
	public class ImageEventArgs : LeapEventArgs
	{
		public Image image { get; set; }

		public ImageEventArgs(Image image)
			: base(LeapEvent.EVENT_IMAGE)
		{
			this.image = image;
		}
	}
}
