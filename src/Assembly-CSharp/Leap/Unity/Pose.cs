using System;
using UnityEngine;

namespace Leap.Unity
{
	[Serializable]
	public struct Pose : IEquatable<Pose>
	{
		public Vector3 position;

		public Quaternion rotation;

		public static readonly Pose identity = new Pose(Vector3.zero, Quaternion.identity);

		public Pose inverse
		{
			get
			{
				Quaternion quaternion = Quaternion.Inverse(rotation);
				return new Pose(quaternion * -position, quaternion);
			}
		}

		public Pose(Vector3 position)
			: this(position, Quaternion.identity)
		{
		}

		public Pose(Quaternion rotation)
			: this(Vector3.zero, rotation)
		{
		}

		public Pose(Vector3 position, Quaternion rotation)
		{
			this.position = position;
			this.rotation = rotation;
		}

		public static Pose operator *(Pose A, Pose B)
		{
			return new Pose(A.position + A.rotation * B.position, A.rotation * B.rotation);
		}

		public static Pose operator +(Pose A, Pose B)
		{
			return new Pose(A.position + B.position, A.rotation * B.rotation);
		}

		public static Pose operator *(Pose pose, Vector3 localPosition)
		{
			return new Pose(pose.position + pose.rotation * localPosition, pose.rotation);
		}

		public bool ApproxEquals(Pose other)
		{
			return position.ApproxEquals(other.position) && rotation.ApproxEquals(other.rotation);
		}

		public static Pose Lerp(Pose a, Pose b, float t)
		{
			if (t >= 1f)
			{
				return b;
			}
			if (t <= 0f)
			{
				return a;
			}
			return new Pose(Vector3.Lerp(a.position, b.position, t), Quaternion.Lerp(Quaternion.Slerp(a.rotation, b.rotation, t), Quaternion.identity, 0f));
		}

		public static Pose LerpUnclamped(Pose a, Pose b, float t)
		{
			return new Pose(Vector3.LerpUnclamped(a.position, b.position, t), Quaternion.SlerpUnclamped(a.rotation, b.rotation, t));
		}

		public static Pose LerpUnclampedTimed(Pose a, float aTime, Pose b, float bTime, float extrapolateTime)
		{
			return LerpUnclamped(a, b, extrapolateTime.MapUnclamped(aTime, bTime, 0f, 1f));
		}

		public override string ToString()
		{
			return "[Pose | Position: " + position.ToString() + ", Rotation: " + rotation.ToString() + "]";
		}

		public string ToString(string format)
		{
			return "[Pose | Position: " + position.ToString(format) + ", Rotation: " + rotation.ToString(format) + "]";
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Pose))
			{
				return false;
			}
			return Equals((Pose)obj);
		}

		public bool Equals(Pose other)
		{
			return other.position == position && other.rotation == rotation;
		}

		public override int GetHashCode()
		{
			Hash hash = default(Hash);
			hash.Add(position);
			hash.Add(rotation);
			return hash;
		}

		public static bool operator ==(Pose a, Pose b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Pose a, Pose b)
		{
			return !a.Equals(b);
		}
	}
}
