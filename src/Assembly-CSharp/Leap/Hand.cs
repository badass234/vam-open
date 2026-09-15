using System;
using System.Collections.Generic;

namespace Leap
{
	[Serializable]
	public class Hand : IEquatable<Hand>
	{
		public long FrameId;

		public int Id;

		public List<Finger> Fingers;

		public Vector PalmPosition;

		public Vector PalmVelocity;

		public Vector PalmNormal;

		public Vector Direction;

		public LeapQuaternion Rotation;

		public float GrabStrength;

		public float GrabAngle;

		public float PinchStrength;

		public float PinchDistance;

		public float PalmWidth;

		public Vector StabilizedPalmPosition;

		public Vector WristPosition;

		public float TimeVisible;

		public float Confidence;

		public bool IsLeft;

		public Arm Arm;

		public LeapTransform Basis => new LeapTransform(PalmPosition, Rotation);

		public bool IsRight => !IsLeft;

		public Hand()
		{
			Arm = new Arm();
			Fingers = new List<Finger>(5);
			Fingers.Add(new Finger());
			Fingers.Add(new Finger());
			Fingers.Add(new Finger());
			Fingers.Add(new Finger());
			Fingers.Add(new Finger());
		}

		public Hand(long frameID, int id, float confidence, float grabStrength, float grabAngle, float pinchStrength, float pinchDistance, float palmWidth, bool isLeft, float timeVisible, Arm arm, List<Finger> fingers, Vector palmPosition, Vector stabilizedPalmPosition, Vector palmVelocity, Vector palmNormal, LeapQuaternion palmOrientation, Vector direction, Vector wristPosition)
		{
			FrameId = frameID;
			Id = id;
			Confidence = confidence;
			GrabStrength = grabStrength;
			GrabAngle = grabAngle;
			PinchStrength = pinchStrength;
			PinchDistance = pinchDistance;
			PalmWidth = palmWidth;
			IsLeft = isLeft;
			TimeVisible = timeVisible;
			Arm = arm;
			Fingers = fingers;
			PalmPosition = palmPosition;
			StabilizedPalmPosition = stabilizedPalmPosition;
			PalmVelocity = palmVelocity;
			PalmNormal = palmNormal;
			Rotation = palmOrientation;
			Direction = direction;
			WristPosition = wristPosition;
		}

		public Finger Finger(int id)
		{
			int count = Fingers.Count;
			while (count-- != 0)
			{
				if (Fingers[count].Id == id)
				{
					return Fingers[count];
				}
			}
			return null;
		}

		public bool Equals(Hand other)
		{
			return Id == other.Id && FrameId == other.FrameId;
		}

		public override string ToString()
		{
			return string.Format("Hand {0} {1}.", Id, (!IsLeft) ? "right" : "left");
		}
	}
}
