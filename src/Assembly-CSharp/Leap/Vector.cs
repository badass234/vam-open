using System;

namespace Leap
{
	[Serializable]
	public struct Vector : IEquatable<Vector>
	{
		public float x;

		public float y;

		public float z;

		public static readonly Vector Zero = new Vector(0f, 0f, 0f);

		public static readonly Vector Ones = new Vector(1f, 1f, 1f);

		public static readonly Vector XAxis = new Vector(1f, 0f, 0f);

		public static readonly Vector YAxis = new Vector(0f, 1f, 0f);

		public static readonly Vector ZAxis = new Vector(0f, 0f, 1f);

		public static readonly Vector Forward = new Vector(0f, 0f, -1f);

		public static readonly Vector Backward = new Vector(0f, 0f, 1f);

		public static readonly Vector Left = new Vector(-1f, 0f, 0f);

		public static readonly Vector Right = new Vector(1f, 0f, 0f);

		public static readonly Vector Up = new Vector(0f, 1f, 0f);

		public static readonly Vector Down = new Vector(0f, -1f, 0f);

		public float this[uint index]
		{
			get
			{
				switch (index)
				{
				case 0u:
					return x;
				case 1u:
					return y;
				case 2u:
					return z;
				default:
					throw new IndexOutOfRangeException();
				}
			}
			set
			{
				if (index == 0)
				{
					x = value;
				}
				if (index == 1)
				{
					y = value;
				}
				if (index == 2)
				{
					z = value;
				}
				throw new IndexOutOfRangeException();
			}
		}

		public float Magnitude => (float)Math.Sqrt(x * x + y * y + z * z);

		public float MagnitudeSquared => x * x + y * y + z * z;

		public float Pitch => (float)Math.Atan2(y, 0f - z);

		public float Roll => (float)Math.Atan2(x, 0f - y);

		public float Yaw => (float)Math.Atan2(x, 0f - z);

		public Vector Normalized
		{
			get
			{
				float magnitudeSquared = MagnitudeSquared;
				if (magnitudeSquared <= 1.1920929E-07f)
				{
					return Zero;
				}
				magnitudeSquared = 1f / (float)Math.Sqrt(magnitudeSquared);
				return new Vector(x * magnitudeSquared, y * magnitudeSquared, z * magnitudeSquared);
			}
		}

		public Vector(float x, float y, float z)
		{
			this = default(Vector);
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public Vector(Vector vector)
		{
			this = default(Vector);
			x = vector.x;
			y = vector.y;
			z = vector.z;
		}

		public static Vector operator +(Vector v1, Vector v2)
		{
			return new Vector(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
		}

		public static Vector operator -(Vector v1, Vector v2)
		{
			return new Vector(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
		}

		public static Vector operator *(Vector v1, float scalar)
		{
			return new Vector(v1.x * scalar, v1.y * scalar, v1.z * scalar);
		}

		public static Vector operator *(float scalar, Vector v1)
		{
			return new Vector(v1.x * scalar, v1.y * scalar, v1.z * scalar);
		}

		public static Vector operator /(Vector v1, float scalar)
		{
			return new Vector(v1.x / scalar, v1.y / scalar, v1.z / scalar);
		}

		public static Vector operator -(Vector v1)
		{
			return new Vector(0f - v1.x, 0f - v1.y, 0f - v1.z);
		}

		public static bool operator ==(Vector v1, Vector v2)
		{
			return v1.Equals(v2);
		}

		public static bool operator !=(Vector v1, Vector v2)
		{
			return !v1.Equals(v2);
		}

		public float[] ToFloatArray()
		{
			return new float[3] { x, y, z };
		}

		public float DistanceTo(Vector other)
		{
			return (float)Math.Sqrt((x - other.x) * (x - other.x) + (y - other.y) * (y - other.y) + (z - other.z) * (z - other.z));
		}

		public float AngleTo(Vector other)
		{
			float num = MagnitudeSquared * other.MagnitudeSquared;
			if (num <= 1.1920929E-07f)
			{
				return 0f;
			}
			float num2 = Dot(other) / (float)Math.Sqrt(num);
			if (num2 >= 1f)
			{
				return 0f;
			}
			if (num2 <= -1f)
			{
				return (float)Math.PI;
			}
			return (float)Math.Acos(num2);
		}

		public float Dot(Vector other)
		{
			return x * other.x + y * other.y + z * other.z;
		}

		public Vector Cross(Vector other)
		{
			return new Vector(y * other.z - z * other.y, z * other.x - x * other.z, x * other.y - y * other.x);
		}

		public override string ToString()
		{
			return "(" + x + ", " + y + ", " + z + ")";
		}

		public bool Equals(Vector v)
		{
			return x.NearlyEquals(v.x) && y.NearlyEquals(v.y) && z.NearlyEquals(v.z);
		}

		public override bool Equals(object obj)
		{
			return obj is Vector && Equals((Vector)obj);
		}

		public bool IsValid()
		{
			return !float.IsNaN(x) && !float.IsInfinity(x) && !float.IsNaN(y) && !float.IsInfinity(y) && !float.IsNaN(z) && !float.IsInfinity(z);
		}

		public static Vector Lerp(Vector a, Vector b, float t)
		{
			return new Vector(a.x + t * (b.x - a.x), a.y + t * (b.y - a.y), a.z + t * (b.z - a.z));
		}

		public override int GetHashCode()
		{
			int num = 17;
			num = num * 23 + x.GetHashCode();
			num = num * 23 + y.GetHashCode();
			return num * 23 + z.GetHashCode();
		}
	}
}
