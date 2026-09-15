using System;

namespace Leap
{
	[Serializable]
	public class Finger
	{
		public enum FingerType
		{
			TYPE_THUMB = 0,
			TYPE_INDEX = 1,
			TYPE_MIDDLE = 2,
			TYPE_RING = 3,
			TYPE_PINKY = 4,
			TYPE_UNKNOWN = -1
		}

		public Bone[] bones = new Bone[4];

		public FingerType Type;

		public int Id;

		public int HandId;

		public Vector TipPosition;

		public Vector Direction;

		public float Width;

		public float Length;

		public bool IsExtended;

		public float TimeVisible;

		public Finger()
		{
			bones[0] = new Bone();
			bones[1] = new Bone();
			bones[2] = new Bone();
			bones[3] = new Bone();
		}

		public Finger(long frameId, int handId, int fingerId, float timeVisible, Vector tipPosition, Vector direction, float width, float length, bool isExtended, FingerType type, Bone metacarpal, Bone proximal, Bone intermediate, Bone distal)
		{
			Type = type;
			bones[0] = metacarpal;
			bones[1] = proximal;
			bones[2] = intermediate;
			bones[3] = distal;
			Id = handId * 10 + fingerId;
			HandId = handId;
			TipPosition = tipPosition;
			Direction = direction;
			Width = width;
			Length = length;
			IsExtended = isExtended;
			TimeVisible = timeVisible;
		}

		public Bone Bone(Bone.BoneType boneIx)
		{
			return bones[(int)boneIx];
		}

		public override string ToString()
		{
			return Enum.GetName(typeof(FingerType), Type) + " id:" + Id;
		}
	}
}
