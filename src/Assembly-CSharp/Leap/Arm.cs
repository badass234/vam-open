using System;

namespace Leap
{
	[Serializable]
	public class Arm : Bone, IEquatable<Arm>
	{
		public Vector ElbowPosition => PrevJoint;

		public Vector WristPosition => NextJoint;

		public Arm()
		{
		}

		public Arm(Vector elbow, Vector wrist, Vector center, Vector direction, float length, float width, LeapQuaternion rotation)
			: base(elbow, wrist, center, direction, length, width, BoneType.TYPE_METACARPAL, rotation)
		{
		}

		public bool Equals(Arm other)
		{
			return Equals((Bone)other);
		}

		public override string ToString()
		{
			return "Arm";
		}
	}
}
