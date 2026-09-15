using UnityEngine;

namespace Leap.Unity
{
	public static class PoseExtensions
	{
		public const float EPSILON = 0.0001f;

		public static Pose ToLocalPose(this Transform t)
		{
			return new Pose(t.localPosition, t.localRotation);
		}

		public static Pose ToPose(this Transform t)
		{
			return new Pose(t.position, t.rotation);
		}

		public static Pose ToWorldPose(this Transform t)
		{
			return t.ToPose();
		}

		public static void SetLocalPose(this Transform t, Pose localPose)
		{
			t.localPosition = localPose.position;
			t.localRotation = localPose.rotation;
		}

		public static void SetPose(this Transform t, Pose worldPose)
		{
			t.position = worldPose.position;
			t.rotation = worldPose.rotation;
		}

		public static void SetWorldPose(this Transform t, Pose worldPose)
		{
			t.SetPose(worldPose);
		}

		public static Pose GetPose(this Matrix4x4 m)
		{
			return new Pose(m.GetColumn(3), (!(m.GetColumn(2) == m.GetColumn(1))) ? Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1)) : Quaternion.identity);
		}

		public static Pose WithRotation(this Pose pose, Quaternion newRotation)
		{
			return new Pose(pose.position, newRotation);
		}

		public static Pose WithPosition(this Pose pose, Vector3 newPosition)
		{
			return new Pose(newPosition, pose.rotation);
		}

		public static Vector3 GetVector3(this Matrix4x4 m)
		{
			return m.GetColumn(3);
		}

		public static Quaternion GetQuaternion(this Matrix4x4 m)
		{
			if (m.GetColumn(2) == m.GetColumn(1))
			{
				return Quaternion.identity;
			}
			return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
		}

		public static bool ApproxEquals(this Vector3 v0, Vector3 v1)
		{
			return (v0 - v1).magnitude < 0.0001f;
		}

		public static bool ApproxEquals(this Quaternion q0, Quaternion q1)
		{
			return (q0.ToAngleAxisVector() - q1.ToAngleAxisVector()).magnitude < 0.0001f;
		}
	}
}
