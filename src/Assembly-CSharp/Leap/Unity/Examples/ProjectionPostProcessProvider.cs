using UnityEngine;

namespace Leap.Unity.Examples
{
	public class ProjectionPostProcessProvider : PostProcessProvider
	{
		[Header("Projection")]
		[Tooltip("The exponent of the projection of any hand distance from the approximated shoulder beyond the handMergeDistance.")]
		[Range(0f, 5f)]
		public float projectionExponent = 3.5f;

		[Tooltip("The distance from the approximated shoulder beyond which any additional distance is exponentiated by the projectionExponent.")]
		[Range(0f, 1f)]
		public float handMergeDistance = 0.3f;

		public override void ProcessFrame(ref Frame inputFrame)
		{
			Vector3 position = Camera.main.transform.position;
			Quaternion quaternion = Quaternion.LookRotation(Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up), Vector3.up);
			foreach (Hand hand in inputFrame.Hands)
			{
				Vector3 vector = position + quaternion * (new Vector3(0f, -0.2f, -0.1f) + Vector3.left * 0.1f * ((!hand.IsLeft) ? (-1f) : 1f));
				Vector3 vector2 = hand.PalmPosition.ToVector3() - vector;
				float magnitude = vector2.magnitude;
				float num = Mathf.Max(0f, magnitude - handMergeDistance);
				float num2 = Mathf.Pow(1f + num, projectionExponent);
				hand.SetTransform(vector + vector2 * num2, hand.Rotation.ToQuaternion());
			}
		}
	}
}
