using System.Collections;
using Leap.Unity.Attributes;
using UnityEngine;

namespace Leap.Unity
{
	public class FingerDirectionDetector : Detector
	{
		[Units("seconds")]
		[Tooltip("The interval in seconds at which to check this detector's conditions.")]
		[MinValue(0f)]
		public float Period = 0.1f;

		[Tooltip("The hand model to watch. Set automatically if detector is on a hand.")]
		public HandModelBase HandModel;

		[Tooltip("The finger to observe.")]
		public Finger.FingerType FingerName = Finger.FingerType.TYPE_INDEX;

		[Header("Direction Settings")]
		[Tooltip("How to treat the target direction.")]
		public PointingType PointingType = PointingType.RelativeToHorizon;

		[Tooltip("The target direction.")]
		[DisableIf("PointingType", PointingType.AtTarget, null)]
		public Vector3 PointingDirection = Vector3.forward;

		[Tooltip("A target object(optional). Use PointingType.AtTarget")]
		[DisableIf("PointingType", null, PointingType.AtTarget)]
		public Transform TargetObject;

		[Tooltip("The angle in degrees from the target direction at which to turn on.")]
		[Range(0f, 180f)]
		public float OnAngle = 15f;

		[Tooltip("The angle in degrees from the target direction at which to turn off.")]
		[Range(0f, 180f)]
		public float OffAngle = 25f;

		[Header("")]
		[Tooltip("Draw this detector's Gizmos, if any. (Gizmos must be on in Unity edtor, too.)")]
		public bool ShowGizmos = true;

		private IEnumerator watcherCoroutine;

		private void OnValidate()
		{
			if (OffAngle < OnAngle)
			{
				OffAngle = OnAngle;
			}
		}

		private void Awake()
		{
			watcherCoroutine = fingerPointingWatcher();
		}

		private void OnEnable()
		{
			StartCoroutine(watcherCoroutine);
		}

		private void OnDisable()
		{
			StopCoroutine(watcherCoroutine);
			Deactivate();
		}

		private IEnumerator fingerPointingWatcher()
		{
			int selectedFinger = selectedFingerOrdinal();
			while (true)
			{
				if (HandModel != null && HandModel.IsTracked)
				{
					Hand hand = HandModel.GetLeapHand();
					if (hand != null)
					{
						Vector3 targetDirection = selectedDirection(hand.Fingers[selectedFinger].TipPosition.ToVector3());
						Vector3 fingerDirection = hand.Fingers[selectedFinger].Bone(Bone.BoneType.TYPE_DISTAL).Direction.ToVector3();
						float num = Vector3.Angle(fingerDirection, targetDirection);
						if (HandModel.IsTracked && num <= OnAngle)
						{
							Activate();
						}
						else if (!HandModel.IsTracked || num >= OffAngle)
						{
							Deactivate();
						}
					}
				}
				yield return new WaitForSeconds(Period);
			}
		}

		private Vector3 selectedDirection(Vector3 tipPosition)
		{
			switch (PointingType)
			{
			case PointingType.RelativeToHorizon:
			{
				float y = Camera.main.transform.rotation.eulerAngles.y;
				Quaternion quaternion = Quaternion.AngleAxis(y, Vector3.up);
				return quaternion * PointingDirection;
			}
			case PointingType.RelativeToCamera:
				return Camera.main.transform.TransformDirection(PointingDirection);
			case PointingType.RelativeToWorld:
				return PointingDirection;
			case PointingType.AtTarget:
				return TargetObject.position - tipPosition;
			default:
				return PointingDirection;
			}
		}

		private int selectedFingerOrdinal()
		{
			switch (FingerName)
			{
			case Finger.FingerType.TYPE_INDEX:
				return 1;
			case Finger.FingerType.TYPE_MIDDLE:
				return 2;
			case Finger.FingerType.TYPE_PINKY:
				return 4;
			case Finger.FingerType.TYPE_RING:
				return 3;
			case Finger.FingerType.TYPE_THUMB:
				return 0;
			default:
				return 1;
			}
		}
	}
}
