using LeapInternal;

namespace Leap
{
	public class HeadPoseEventArgs : LeapEventArgs
	{
		public LEAP_VECTOR headPosition { get; set; }

		public LEAP_QUATERNION headOrientation { get; set; }

		public HeadPoseEventArgs(LEAP_VECTOR head_position, LEAP_QUATERNION head_orientation)
			: base(LeapEvent.EVENT_POINT_MAPPING_CHANGE)
		{
			headPosition = head_position;
			headOrientation = head_orientation;
		}
	}
}
