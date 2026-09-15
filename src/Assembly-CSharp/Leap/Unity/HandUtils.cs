using System.Collections.Generic;
using Leap.Unity.Query;
using UnityEngine;

namespace Leap.Unity
{
	public static class HandUtils
	{
		public static void Fill(this Hand toFill, long frameID, int id, float confidence, float grabStrength, float grabAngle, float pinchStrength, float pinchDistance, float palmWidth, bool isLeft, float timeVisible, List<Finger> fingers, Vector palmPosition, Vector stabilizedPalmPosition, Vector palmVelocity, Vector palmNormal, LeapQuaternion rotation, Vector direction, Vector wristPosition)
		{
			toFill.FrameId = frameID;
			toFill.Id = id;
			toFill.Confidence = confidence;
			toFill.GrabStrength = grabStrength;
			toFill.GrabAngle = grabAngle;
			toFill.PinchStrength = pinchStrength;
			toFill.PinchDistance = pinchDistance;
			toFill.PalmWidth = palmWidth;
			toFill.IsLeft = isLeft;
			toFill.TimeVisible = timeVisible;
			if (fingers != null)
			{
				toFill.Fingers = fingers;
			}
			toFill.PalmPosition = palmPosition;
			toFill.StabilizedPalmPosition = stabilizedPalmPosition;
			toFill.PalmVelocity = palmVelocity;
			toFill.PalmNormal = palmNormal;
			toFill.Rotation = rotation;
			toFill.Direction = direction;
			toFill.WristPosition = wristPosition;
		}

		public static void Fill(this Bone toFill, Vector prevJoint, Vector nextJoint, Vector center, Vector direction, float length, float width, Bone.BoneType type, LeapQuaternion rotation)
		{
			toFill.PrevJoint = prevJoint;
			toFill.NextJoint = nextJoint;
			toFill.Center = center;
			toFill.Direction = direction;
			toFill.Length = length;
			toFill.Width = width;
			toFill.Type = type;
			toFill.Rotation = rotation;
		}

		public static void Fill(this Finger toFill, long frameId, int handId, int fingerId, float timeVisible, Vector tipPosition, Vector direction, float width, float length, bool isExtended, Finger.FingerType type, Bone metacarpal = null, Bone proximal = null, Bone intermediate = null, Bone distal = null)
		{
			toFill.Id = handId;
			toFill.HandId = handId;
			toFill.TimeVisible = timeVisible;
			toFill.TipPosition = tipPosition;
			toFill.Direction = direction;
			toFill.Width = width;
			toFill.Length = length;
			toFill.IsExtended = isExtended;
			toFill.Type = type;
			if (metacarpal != null)
			{
				toFill.bones[0] = metacarpal;
			}
			if (proximal != null)
			{
				toFill.bones[1] = proximal;
			}
			if (intermediate != null)
			{
				toFill.bones[2] = intermediate;
			}
			if (distal != null)
			{
				toFill.bones[3] = distal;
			}
		}

		public static void Fill(this Arm toFill, Vector elbow, Vector wrist, Vector center, Vector direction, float length, float width, LeapQuaternion rotation)
		{
			toFill.PrevJoint = elbow;
			toFill.NextJoint = wrist;
			toFill.Center = center;
			toFill.Direction = direction;
			toFill.Length = length;
			toFill.Width = width;
			toFill.Rotation = rotation;
		}

		public static void FillTemporalData(this Hand toFill, Hand previousHand, float deltaTime)
		{
			toFill.PalmVelocity = (toFill.PalmPosition - previousHand.PalmPosition) / deltaTime;
		}

		public static Hand Get(this Frame frame, Chirality whichHand)
		{
			return frame.Hands.Query().FirstOrDefault((Hand h) => h.IsLeft == (whichHand == Chirality.Left));
		}

		public static Hand Get(this LeapProvider provider, Chirality whichHand)
		{
			Frame frame = ((!Time.inFixedTimeStep) ? provider.CurrentFrame : provider.CurrentFixedFrame);
			return frame.Get(whichHand);
		}
	}
}
