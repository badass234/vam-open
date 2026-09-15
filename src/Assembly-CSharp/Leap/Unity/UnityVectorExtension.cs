using LeapInternal;
using UnityEngine;

namespace Leap.Unity
{
	public static class UnityVectorExtension
	{
		public static Vector3 ToVector3(this Vector vector)
		{
			return new Vector3(vector.x, vector.y, vector.z);
		}

		public static Vector3 ToVector3(this LEAP_VECTOR vector)
		{
			return new Vector3(vector.x, vector.y, vector.z);
		}

		public static Vector4 ToVector4(this Vector vector)
		{
			return new Vector4(vector.x, vector.y, vector.z, 0f);
		}

		public static Vector ToVector(this Vector3 vector)
		{
			return new Vector(vector.x, vector.y, vector.z);
		}

		public static LEAP_VECTOR ToCVector(this Vector3 vector)
		{
			LEAP_VECTOR result = default(LEAP_VECTOR);
			result.x = vector.x;
			result.y = vector.y;
			result.z = vector.z;
			return result;
		}
	}
}
