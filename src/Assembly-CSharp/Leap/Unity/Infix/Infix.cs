using UnityEngine;

namespace Leap.Unity.Infix
{
	public static class Infix
	{
		public static float Clamped01(this float f)
		{
			return Mathf.Clamp01(f);
		}

		public static float Clamped(this float f, float min, float max)
		{
			return Mathf.Clamp(f, min, max);
		}

		public static Vector3 RotatedBy(this Vector3 thisVector, Quaternion byQuaternion)
		{
			return byQuaternion * thisVector;
		}

		public static Vector3 MovedTowards(this Vector3 thisPosition, Vector3 otherPosition, float maxDistanceDelta)
		{
			return Vector3.MoveTowards(thisPosition, otherPosition, maxDistanceDelta);
		}

		public static float Dot(this Vector3 a, Vector3 b)
		{
			return Vector3.Dot(a, b);
		}

		public static Vector3 Cross(this Vector3 a, Vector3 b)
		{
			return Vector3.Cross(a, b);
		}

		public static float Angle(this Vector3 a, Vector3 b)
		{
			return Vector3.Angle(a, b);
		}

		public static float SignedAngle(this Vector3 a, Vector3 b, Vector3 axis)
		{
			float num = ((!(Vector3.Dot(Vector3.Cross(a, b), axis) < 0f)) ? 1f : (-1f));
			return num * Vector3.Angle(a, b);
		}

		public static Vector3 GetRight(this Quaternion q)
		{
			return q * Vector3.right;
		}

		public static Vector3 GetUp(this Quaternion q)
		{
			return q * Vector3.up;
		}

		public static Vector3 GetForward(this Quaternion q)
		{
			return q * Vector3.forward;
		}
	}
}
