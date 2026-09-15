using Leap.Unity.Query;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Leap.Unity
{
	public static class Hands
	{
		private static LeapProvider s_provider;

		private static GameObject s_leapRig;

		public static GameObject CameraRig
		{
			get
			{
				if (s_leapRig == null)
				{
					InitStatic();
				}
				return s_leapRig;
			}
		}

		public static LeapProvider Provider
		{
			get
			{
				if (s_provider == null)
				{
					InitStatic();
				}
				return s_provider;
			}
		}

		public static Hand Left
		{
			get
			{
				if (Provider == null)
				{
					return null;
				}
				if (Provider.CurrentFrame == null)
				{
					return null;
				}
				return Provider.CurrentFrame.Hands.Query().FirstOrDefault((Hand hand) => hand.IsLeft);
			}
		}

		public static Hand Right
		{
			get
			{
				if (Provider == null)
				{
					return null;
				}
				if (Provider.CurrentFrame == null)
				{
					return null;
				}
				return Provider.CurrentFrame.Hands.Query().FirstOrDefault((Hand hand) => hand.IsRight);
			}
		}

		public static Hand FixedLeft
		{
			get
			{
				if (Provider == null)
				{
					return null;
				}
				if (Provider.CurrentFixedFrame == null)
				{
					return null;
				}
				return Provider.CurrentFixedFrame.Hands.Query().FirstOrDefault((Hand hand) => hand.IsLeft);
			}
		}

		public static Hand FixedRight
		{
			get
			{
				if (Provider == null)
				{
					return null;
				}
				if (Provider.CurrentFixedFrame == null)
				{
					return null;
				}
				return Provider.CurrentFixedFrame.Hands.Query().FirstOrDefault((Hand hand) => hand.IsRight);
			}
		}

		static Hands()
		{
			InitStatic();
			SceneManager.activeSceneChanged += InitStaticOnNewScene;
		}

		private static void InitStaticOnNewScene(Scene unused, Scene unused2)
		{
			InitStatic();
		}

		private static void InitStatic()
		{
			s_provider = Object.FindObjectOfType<LeapServiceProvider>();
			if (s_provider == null)
			{
				s_provider = Object.FindObjectOfType<LeapProvider>();
				if (s_provider == null)
				{
					return;
				}
			}
			Camera componentInParent = s_provider.GetComponentInParent<Camera>();
			if (!(componentInParent == null) && !(componentInParent.transform.parent == null))
			{
				s_leapRig = componentInParent.transform.parent.gameObject;
			}
		}

		public static Hand Get(Chirality chirality)
		{
			if (chirality == Chirality.Left)
			{
				return Left;
			}
			return Right;
		}

		public static Hand GetFixed(Chirality chirality)
		{
			if (chirality == Chirality.Left)
			{
				return FixedLeft;
			}
			return FixedRight;
		}

		public static Finger GetThumb(this Hand hand)
		{
			return hand.Fingers[0];
		}

		public static Finger GetIndex(this Hand hand)
		{
			return hand.Fingers[1];
		}

		public static Finger GetMiddle(this Hand hand)
		{
			return hand.Fingers[2];
		}

		public static Finger GetRing(this Hand hand)
		{
			return hand.Fingers[3];
		}

		public static Finger GetPinky(this Hand hand)
		{
			return hand.Fingers[4];
		}

		public static Pose GetPalmPose(this Hand hand)
		{
			return new Pose(hand.PalmPosition.ToVector3(), hand.Rotation.ToQuaternion());
		}

		public static void SetPalmPose(this Hand hand, Pose newPalmPose)
		{
			hand.SetTransform(newPalmPose.position, newPalmPose.rotation);
		}

		public static Vector3 PalmarAxis(this Hand hand)
		{
			return -hand.Basis.yBasis.ToVector3();
		}

		public static Vector3 RadialAxis(this Hand hand)
		{
			if (hand.IsRight)
			{
				return -hand.Basis.xBasis.ToVector3();
			}
			return hand.Basis.xBasis.ToVector3();
		}

		public static Vector3 DistalAxis(this Hand hand)
		{
			return hand.Basis.zBasis.ToVector3();
		}

		public static bool IsPinching(this Hand hand)
		{
			return hand.PinchStrength > 0.8f;
		}

		public static Vector3 GetPinchPosition(this Hand hand)
		{
			Vector tipPosition = hand.Fingers[1].TipPosition;
			Vector tipPosition2 = hand.Fingers[0].TipPosition;
			return (2f * tipPosition2 + tipPosition).ToVector3() * 0.333333f;
		}

		public static Vector3 GetPredictedPinchPosition(this Hand hand)
		{
			Vector3 b = hand.GetIndex().TipPosition.ToVector3();
			Vector3 vector = hand.GetThumb().TipPosition.ToVector3();
			Vector3 vector2 = hand.Fingers[1].bones[1].PrevJoint.ToVector3();
			float length = hand.Fingers[1].Length;
			Vector3 vector3 = hand.RadialAxis();
			float t = Vector3.Dot((vector - vector2).normalized, vector3).Map(0f, 1f, 0.5f, 0f);
			Vector3 a = vector2 + hand.PalmarAxis() * length * 0.85f + hand.DistalAxis() * length * 0.2f + vector3 * length * 0.2f;
			a = Vector3.Lerp(a, vector, t);
			return Vector3.Lerp(a, b, 0.15f);
		}

		public static bool IsFacing(this Vector3 facingVector, Vector3 fromWorldPosition, Vector3 towardsWorldPosition, float maxOffsetAngleAllowed)
		{
			Vector3 normalized = (towardsWorldPosition - fromWorldPosition).normalized;
			return Vector3.Angle(facingVector, normalized) <= maxOffsetAngleAllowed;
		}

		public static float GetFistStrength(this Hand hand)
		{
			return (Vector3.Dot(hand.Fingers[1].Direction.ToVector3(), -hand.DistalAxis()) + Vector3.Dot(hand.Fingers[2].Direction.ToVector3(), -hand.DistalAxis()) + Vector3.Dot(hand.Fingers[3].Direction.ToVector3(), -hand.DistalAxis()) + Vector3.Dot(hand.Fingers[4].Direction.ToVector3(), -hand.DistalAxis()) + Vector3.Dot(hand.Fingers[0].Direction.ToVector3(), -hand.RadialAxis())).Map(-5f, 5f, 0f, 1f);
		}

		public static void Transform(this Bone bone, Vector3 position, Quaternion rotation)
		{
			bone.Transform(new LeapTransform(position.ToVector(), rotation.ToLeapQuaternion()));
		}

		public static void Transform(this Finger finger, Vector3 position, Quaternion rotation)
		{
			finger.Transform(new LeapTransform(position.ToVector(), rotation.ToLeapQuaternion()));
		}

		public static void Transform(this Hand hand, Vector3 position, Quaternion rotation)
		{
			hand.Transform(new LeapTransform(position.ToVector(), rotation.ToLeapQuaternion()));
		}

		public static void Transform(this Frame frame, Vector3 position, Quaternion rotation)
		{
			frame.Transform(new LeapTransform(position.ToVector(), rotation.ToLeapQuaternion()));
		}

		public static void SetTransform(this Bone bone, Vector3 position, Quaternion rotation)
		{
			bone.Transform(Vector3.zero, rotation * Quaternion.Inverse(bone.Rotation.ToQuaternion()));
			bone.Transform(position - bone.PrevJoint.ToVector3(), Quaternion.identity);
		}

		public static void SetTipTransform(this Finger finger, Vector3 position, Quaternion rotation)
		{
			finger.Transform(Vector3.zero, rotation * Quaternion.Inverse(finger.bones[3].Rotation.ToQuaternion()));
			finger.Transform(position - finger.bones[3].NextJoint.ToVector3(), Quaternion.identity);
		}

		public static void SetTransform(this Hand hand, Vector3 position, Quaternion rotation)
		{
			hand.Transform(Vector3.zero, Quaternion.Slerp(rotation * Quaternion.Inverse(hand.Rotation.ToQuaternion()), Quaternion.identity, 0f));
			hand.Transform(position - hand.PalmPosition.ToVector3(), Quaternion.identity);
		}
	}
}
