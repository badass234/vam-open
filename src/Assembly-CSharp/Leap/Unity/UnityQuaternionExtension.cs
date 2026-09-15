using LeapInternal;
using UnityEngine;

namespace Leap.Unity
{
	public static class UnityQuaternionExtension
	{
		public static Quaternion ToQuaternion(this LeapQuaternion q)
		{
			return new Quaternion(q.x, q.y, q.z, q.w);
		}

		public static Quaternion ToQuaternion(this LEAP_QUATERNION q)
		{
			return new Quaternion(q.x, q.y, q.z, q.w);
		}

		public static LeapQuaternion ToLeapQuaternion(this Quaternion q)
		{
			return new LeapQuaternion(q.x, q.y, q.z, q.w);
		}

		public static LEAP_QUATERNION ToCQuaternion(this Quaternion q)
		{
			LEAP_QUATERNION result = default(LEAP_QUATERNION);
			result.x = q.x;
			result.y = q.y;
			result.z = q.z;
			result.w = q.w;
			return result;
		}
	}
}
