using System;

namespace Leap
{
	[Serializable]
	public class Bone : IEquatable<Bone>
	{
		public enum BoneType
		{
			TYPE_INVALID = -1,
			TYPE_METACARPAL,
			TYPE_PROXIMAL,
			TYPE_INTERMEDIATE,
			TYPE_DISTAL
		}

		public Vector PrevJoint;

		public Vector NextJoint;

		public Vector Center;

		public Vector Direction;

		public float Length;

		public float Width;

		public BoneType Type;

		public LeapQuaternion Rotation;

		public LeapTransform Basis => new LeapTransform(PrevJoint, Rotation);

		public Bone()
		{
			Type = BoneType.TYPE_INVALID;
		}

		public Bone(Vector prevJoint, Vector nextJoint, Vector center, Vector direction, float length, float width, BoneType type, LeapQuaternion rotation)
		{
			PrevJoint = prevJoint;
			NextJoint = nextJoint;
			Center = center;
			Direction = direction;
			Rotation = rotation;
			Length = length;
			Width = width;
			Type = type;
		}

		public bool Equals(Bone other)
		{
			return Center == other.Center && Direction == other.Direction && Length == other.Length;
		}

		public override string ToString()
		{
			return Enum.GetName(typeof(BoneType), Type) + " bone";
		}
	}
}
