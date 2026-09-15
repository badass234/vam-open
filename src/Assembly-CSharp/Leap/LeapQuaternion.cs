using System;
using LeapInternal;

namespace Leap
{
	[Serializable]
	public struct LeapQuaternion : IEquatable<LeapQuaternion>
	{
		public float x;

		public float y;

		public float z;

		public float w;

		public static readonly LeapQuaternion Identity = new LeapQuaternion(0f, 0f, 0f, 1f);

		public float Magnitude => (float)Math.Sqrt(x * x + y * y + z * z + w * w);

		public float MagnitudeSquared => x * x + y * y + z * z + w * w;

		public LeapQuaternion Normalized
		{
			get
			{
				float magnitudeSquared = MagnitudeSquared;
				if (magnitudeSquared <= 1.1920929E-07f)
				{
					return Identity;
				}
				magnitudeSquared = 1f / (float)Math.Sqrt(magnitudeSquared);
				return new LeapQuaternion(x * magnitudeSquared, y * magnitudeSquared, z * magnitudeSquared, w * magnitudeSquared);
			}
		}

		public LeapQuaternion(float x, float y, float z, float w)
		{
			this = default(LeapQuaternion);
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		public LeapQuaternion(LeapQuaternion quaternion)
		{
			this = default(LeapQuaternion);
			x = quaternion.x;
			y = quaternion.y;
			z = quaternion.z;
			w = quaternion.w;
		}

		public LeapQuaternion(LEAP_QUATERNION quaternion)
		{
			this = default(LeapQuaternion);
			x = quaternion.x;
			y = quaternion.y;
			z = quaternion.z;
			w = quaternion.w;
		}

		public override string ToString()
		{
			return "(" + x + ", " + y + ", " + z + ", " + w + ")";
		}

		public bool Equals(LeapQuaternion v)
		{
			return x.NearlyEquals(v.x) && y.NearlyEquals(v.y) && z.NearlyEquals(v.z) && w.NearlyEquals(v.w);
		}

		public override bool Equals(object obj)
		{
			return obj is LeapQuaternion && Equals((LeapQuaternion)obj);
		}

		public bool IsValid()
		{
			return !float.IsNaN(x) && !float.IsInfinity(x) && !float.IsNaN(y) && !float.IsInfinity(y) && !float.IsNaN(z) && !float.IsInfinity(z) && !float.IsNaN(w) && !float.IsInfinity(w);
		}

		public LeapQuaternion Multiply(LeapQuaternion rhs)
		{
			return new LeapQuaternion(w * rhs.x + x * rhs.w + y * rhs.z - z * rhs.y, w * rhs.y + y * rhs.w + z * rhs.x - x * rhs.z, w * rhs.z + z * rhs.w + x * rhs.y - y * rhs.x, w * rhs.w - x * rhs.x - y * rhs.y - z * rhs.z);
		}

		public override int GetHashCode()
		{
			int num = 17;
			num = num * 23 + x.GetHashCode();
			num = num * 23 + y.GetHashCode();
			num = num * 23 + z.GetHashCode();
			return num * 23 + w.GetHashCode();
		}
	}
}
