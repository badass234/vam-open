using Leap.Unity;
using UnityEngine;

namespace Leap
{
	public static class LeapTestProviderExtensions
	{
		public static readonly float MM_TO_M = 0.001f;

		public static LeapTransform GetLeapTransform(Vector3 position, Quaternion rotation)
		{
			LeapTransform result = new LeapTransform(scale: new Vector(MM_TO_M, MM_TO_M, MM_TO_M), translation: position.ToVector(), rotation: rotation.ToLeapQuaternion());
			result.MirrorZ();
			return result;
		}

		public static void TransformToUnityUnits(this Hand hand)
		{
			hand.Transform(GetLeapTransform(Vector3.zero, Quaternion.identity));
		}
	}
}
