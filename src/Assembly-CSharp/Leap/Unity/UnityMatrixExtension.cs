using UnityEngine;

namespace Leap.Unity
{
	public static class UnityMatrixExtension
	{
		public static readonly Vector LEAP_UP = new Vector(0f, 1f, 0f);

		public static readonly Vector LEAP_FORWARD = new Vector(0f, 0f, -1f);

		public static readonly Vector LEAP_ORIGIN = new Vector(0f, 0f, 0f);

		public static readonly float MM_TO_M = 0.001f;

		public static Quaternion CalculateRotation(this LeapTransform trs)
		{
			Vector3 upwards = trs.yBasis.ToVector3();
			Vector3 forward = -trs.zBasis.ToVector3();
			return Quaternion.LookRotation(forward, upwards);
		}

		public static LeapTransform GetLeapMatrix(this Transform t)
		{
			Vector scale = new Vector(t.lossyScale.x * MM_TO_M, t.lossyScale.y * MM_TO_M, t.lossyScale.z * MM_TO_M);
			LeapTransform result = new LeapTransform(t.position.ToVector(), t.rotation.ToLeapQuaternion(), scale);
			result.MirrorZ();
			return result;
		}
	}
}
