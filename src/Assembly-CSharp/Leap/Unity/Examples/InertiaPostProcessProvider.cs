using Leap.Unity.Query;
using UnityEngine;

namespace Leap.Unity.Examples
{
	public class InertiaPostProcessProvider : PostProcessProvider
	{
		[Header("Inertia")]
		[Tooltip("Higher stiffness will keep the bouncy hand closer to the tracked hand data.")]
		[Range(0f, 10f)]
		public float stiffness = 2f;

		[Tooltip("Higher damping will suppress more motion and reduce oscillation.")]
		[Range(0f, 10f)]
		public float damping = 2f;

		private Pose? _leftPose;

		private Pose? _previousLeftPose;

		private float _leftAge;

		private Pose? _rightPose;

		private Pose? _previousRightPose;

		private float _rightAge;

		private Pose? _fixedLeftPose;

		private Pose? _fixedPreviousLeftPose;

		private float _fixedLeftAge;

		private Pose? _fixedRightPose;

		private Pose? _fixedPreviousRightPose;

		private float _fixedRightAge;

		public override void ProcessFrame(ref Frame inputFrame)
		{
			Hand hand = inputFrame.Hands.Query().FirstOrDefault((Hand h) => h.IsLeft);
			Hand hand2 = inputFrame.Hands.Query().FirstOrDefault((Hand h) => !h.IsLeft);
			if (Time.inFixedTimeStep)
			{
				processHand(hand, ref _fixedLeftPose, ref _fixedPreviousLeftPose, ref _fixedLeftAge);
				processHand(hand2, ref _fixedRightPose, ref _fixedPreviousRightPose, ref _fixedRightAge);
			}
			else
			{
				processHand(hand, ref _leftPose, ref _previousLeftPose, ref _leftAge);
				processHand(hand2, ref _rightPose, ref _previousRightPose, ref _rightAge);
			}
		}

		private void processHand(Hand hand, ref Pose? maybeCurPose, ref Pose? maybePrevPose, ref float handAge)
		{
			if (hand == null)
			{
				maybeCurPose = null;
				maybePrevPose = null;
				handAge = 0f;
				return;
			}
			Pose palmPose = hand.GetPalmPose();
			if (!maybeCurPose.HasValue)
			{
				maybePrevPose = null;
				maybeCurPose = palmPose;
				return;
			}
			if (!maybePrevPose.HasValue)
			{
				maybePrevPose = maybeCurPose;
				maybeCurPose = palmPose;
				return;
			}
			float num = hand.TimeVisible - handAge;
			if (num > 0f)
			{
				handAge = hand.TimeVisible;
				Pose curPose = maybeCurPose.Value;
				Pose prevPose = maybePrevPose.Value;
				integratePose(ref curPose, ref prevPose, palmPose, num);
				hand.SetPalmPose(curPose);
				maybeCurPose = curPose;
				maybePrevPose = prevPose;
			}
		}

		private void integratePose(ref Pose curPose, ref Pose prevPose, Pose targetPose, float deltaTime)
		{
			Pose pose = curPose.inverse * prevPose;
			pose = new Pose(-pose.position, Quaternion.Inverse(pose.rotation));
			pose = Pose.Lerp(pose, Pose.identity, damping * deltaTime);
			Pose pose2 = curPose;
			curPose *= pose;
			prevPose = pose2;
			curPose = Pose.Lerp(curPose, targetPose, stiffness * deltaTime);
		}
	}
}
