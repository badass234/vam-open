using System;
using System.Collections.Generic;
using Leap.Unity;
using UnityEngine;

namespace Leap
{
	public static class TestHandFactory
	{
		public enum UnitType
		{
			LeapUnits,
			UnityUnits
		}

		public enum TestHandPose
		{
			HeadMountedA,
			HeadMountedB,
			DesktopModeA
		}

		public static Frame MakeTestFrame(int frameId, bool includeLeftHand = true, bool includeRightHand = true, TestHandPose handPose = TestHandPose.HeadMountedA, UnitType unitType = UnitType.LeapUnits)
		{
			Frame frame = new Frame(frameId, 0L, 120f, new List<Hand>());
			if (includeLeftHand)
			{
				frame.Hands.Add(MakeTestHand(true, handPose, frameId, 10, unitType));
			}
			if (includeRightHand)
			{
				frame.Hands.Add(MakeTestHand(false, handPose, frameId, 20, unitType));
			}
			return frame;
		}

		public static Hand MakeTestHand(bool isLeft, LeapTransform leftHandTransform, int frameId = 0, int handId = 0, UnitType unitType = UnitType.LeapUnits)
		{
			if (!isLeft)
			{
				leftHandTransform.translation = new Vector(0f - leftHandTransform.translation.x, leftHandTransform.translation.y, leftHandTransform.translation.z);
				leftHandTransform.rotation = new LeapQuaternion(0f - leftHandTransform.rotation.x, leftHandTransform.rotation.y, leftHandTransform.rotation.z, 0f - leftHandTransform.rotation.w);
				leftHandTransform.MirrorX();
			}
			Hand hand = makeLeapSpaceTestHand(frameId, handId, isLeft).Transform(leftHandTransform);
			Quaternion quaternion = Quaternion.Euler(90f, 0f, 180f);
			Hand hand2 = hand.Transform(new LeapTransform(rotation: new LeapQuaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w), translation: Vector.Zero));
			if (unitType == UnitType.UnityUnits)
			{
				hand2.TransformToUnityUnits();
			}
			return hand2;
		}

		public static Hand MakeTestHand(bool isLeft, int frameId = 0, int handId = 0, UnitType unitType = UnitType.LeapUnits)
		{
			return MakeTestHand(isLeft, LeapTransform.Identity, frameId, handId, unitType);
		}

		public static Hand MakeTestHand(bool isLeft, TestHandPose pose, int frameId = 0, int handId = 0, UnitType unitType = UnitType.LeapUnits)
		{
			return MakeTestHand(isLeft, GetTestPoseLeftHandTransform(pose), frameId, handId, unitType);
		}

		public static LeapTransform GetTestPoseLeftHandTransform(TestHandPose pose)
		{
			LeapTransform identity = LeapTransform.Identity;
			switch (pose)
			{
			case TestHandPose.HeadMountedA:
				identity.rotation = angleAxis((float)Math.PI, Vector.Forward);
				identity.translation = new Vector(80f, 120f, 0f);
				break;
			case TestHandPose.HeadMountedB:
				identity.rotation = Quaternion.Euler(30f, -10f, -20f).ToLeapQuaternion();
				identity.translation = new Vector(220f, 270f, 130f);
				break;
			case TestHandPose.DesktopModeA:
				identity.rotation = angleAxis(0f, Vector.Forward).Multiply(angleAxis(-(float)Math.PI / 2f, Vector.Right)).Multiply(angleAxis((float)Math.PI, Vector.Up));
				identity.translation = new Vector(120f, 0f, -170f);
				break;
			}
			return identity;
		}

		private static Hand makeLeapSpaceTestHand(int frameId, int handId, bool isLeft)
		{
			List<Finger> list = new List<Finger>(5);
			list.Add(makeThumb(frameId, handId, isLeft));
			list.Add(makeIndexFinger(frameId, handId, isLeft));
			list.Add(makeMiddleFinger(frameId, handId, isLeft));
			list.Add(makeRingFinger(frameId, handId, isLeft));
			list.Add(makePinky(frameId, handId, isLeft));
			Vector vector = new Vector(-7.0580993f, 4f, 50f);
			Vector vector2 = vector + 250f * Vector.Backward;
			Arm arm = new Arm(vector2, vector, (vector2 + vector) / 2f, Vector.Forward, 250f, 41f, LeapQuaternion.Identity);
			return new Hand(frameId, handId, 1f, 0f, 0f, 0f, 0f, 85f, isLeft, 0f, arm, list, new Vector(0f, 0f, 0f), new Vector(0f, 0f, 0f), new Vector(0f, 0f, 0f), Vector.Down, LeapQuaternion.Identity, Vector.Forward, new Vector(-4.3638577f, 6.5f, 31.011135f));
		}

		private static LeapQuaternion angleAxis(float angle, Vector axis)
		{
			if (!axis.MagnitudeSquared.NearlyEquals(1f))
			{
				throw new ArgumentException("Axis must be a unit vector.");
			}
			float num = Mathf.Sin(angle / 2f);
			return new LeapQuaternion(num * axis.x, num * axis.y, num * axis.z, Mathf.Cos(angle / 2f)).Normalized;
		}

		private static LeapQuaternion rotationBetween(Vector fromDirection, Vector toDirection)
		{
			float num = Mathf.Sqrt(2f + 2f * fromDirection.Dot(toDirection));
			Vector vector = 1f / num * fromDirection.Cross(toDirection);
			return new LeapQuaternion(vector.x, vector.y, vector.z, 0.5f * num);
		}

		private static Finger makeThumb(int frameId, int handId, bool isLeft)
		{
			Vector position = new Vector(19.33826f, -6f, 53.168484f);
			Vector forward = new Vector(0.6363291f, -0.5f, -0.8997871f);
			Vector up = new Vector(0.80479395f, 0.44721392f, 0.39026454f);
			float[] jointLengths = new float[4] { 0f, 46.22f, 31.57f, 21.67f };
			return makeFinger(Finger.FingerType.TYPE_THUMB, position, forward, up, jointLengths, frameId, handId, handId, isLeft);
		}

		private static Finger makeIndexFinger(int frameId, int handId, bool isLeft)
		{
			Vector position = new Vector(23.181286f, 2f, -23.149345f);
			Vector forward = new Vector(0.16604431f, -0.14834045f, -0.97489715f);
			Vector up = new Vector(0.024906646f, 0.98893636f, -0.14623457f);
			float[] jointLengths = new float[4] { 68.12f, 39.78f, 22.38f, 15.82f };
			return makeFinger(Finger.FingerType.TYPE_INDEX, position, forward, up, jointLengths, frameId, handId, handId + 1, isLeft);
		}

		private static Finger makeMiddleFinger(int frameId, int handId, bool isLeft)
		{
			Vector position = new Vector(2.7887783f, 4f, -23.252106f);
			Vector forward = new Vector(0.029520785f, -0.14834045f, -0.98849565f);
			Vector up = new Vector(-0.14576527f, 0.97771597f, -0.15107597f);
			float[] jointLengths = new float[4] { 64.6f, 44.63f, 26.33f, 17.4f };
			return makeFinger(Finger.FingerType.TYPE_MIDDLE, position, forward, up, jointLengths, frameId, handId, handId + 2, isLeft);
		}

		private static Finger makeRingFinger(int frameId, int handId, bool isLeft)
		{
			Vector position = new Vector(-17.447168f, 4f, -17.279144f);
			Vector forward = new Vector(-0.12131794f, -0.14834034f, -0.9814668f);
			Vector up = new Vector(-0.21691047f, 0.96883494f, -0.1196191f);
			float[] jointLengths = new float[4] { 58f, 41.37f, 25.65f, 17.3f };
			return makeFinger(Finger.FingerType.TYPE_RING, position, forward, up, jointLengths, frameId, handId, handId + 3, isLeft);
		}

		private static Finger makePinky(int frameId, int handId, bool isLeft)
		{
			Vector position = new Vector(-35.33744f, 0f, -9.728714f);
			Vector forward = new Vector(-277f / (340f * (float)Math.PI), -0.105851226f, -0.95997083f);
			Vector up = new Vector(-0.35335022f, 0.9354595f, -0.007693566f);
			float[] jointLengths = new float[4] { 53.69f, 32.74f, 18.11f, 15.96f };
			return makeFinger(Finger.FingerType.TYPE_PINKY, position, forward, up, jointLengths, frameId, handId, handId + 4, isLeft);
		}

		private static Finger makeFinger(Finger.FingerType name, Vector position, Vector forward, Vector up, float[] jointLengths, int frameId, int handId, int fingerId, bool isLeft)
		{
			forward = forward.Normalized;
			up = up.Normalized;
			Bone[] array = new Bone[5];
			float num = 0f - jointLengths[0];
			Bone bone = makeBone(Bone.BoneType.TYPE_METACARPAL, position + forward * num, jointLengths[0], 8f, forward, up, isLeft);
			num += jointLengths[0];
			array[0] = bone;
			Bone bone2 = makeBone(Bone.BoneType.TYPE_PROXIMAL, position + forward * num, jointLengths[1], 8f, forward, up, isLeft);
			num += jointLengths[1];
			array[1] = bone2;
			Bone bone3 = makeBone(Bone.BoneType.TYPE_INTERMEDIATE, position + forward * num, jointLengths[2], 8f, forward, up, isLeft);
			num += jointLengths[2];
			array[2] = bone3;
			return new Finger(frameId, handId, fingerId, 0f, (array[3] = makeBone(Bone.BoneType.TYPE_DISTAL, position + forward * num, jointLengths[3], 8f, forward, up, isLeft)).NextJoint, forward, 8f, jointLengths[1] + jointLengths[2] + jointLengths[3], true, name, array[0], array[1], array[2], array[3]);
		}

		private static Bone makeBone(Bone.BoneType name, Vector proximalPosition, float length, float width, Vector direction, Vector up, bool isLeft)
		{
			LeapQuaternion rotation = Quaternion.LookRotation(-direction.ToVector3(), up.ToVector3()).ToLeapQuaternion();
			return new Bone(proximalPosition, proximalPosition + direction * length, Vector.Lerp(proximalPosition, proximalPosition + direction * length, 0.5f), direction, length, width, name, rotation);
		}
	}
}
