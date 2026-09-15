namespace Leap
{
	public class PointMappingChangeEventArgs : LeapEventArgs
	{
		public long frameID { get; set; }

		public long timestamp { get; set; }

		public uint nPoints { get; set; }

		public PointMappingChangeEventArgs(long frame_id, long timestamp, uint nPoints)
			: base(LeapEvent.EVENT_POINT_MAPPING_CHANGE)
		{
			frameID = frame_id;
			this.timestamp = timestamp;
			this.nPoints = nPoints;
		}
	}
}
