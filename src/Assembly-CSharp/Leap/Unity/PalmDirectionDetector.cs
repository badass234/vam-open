using System.Collections;
using Leap.Unity.Attributes;
using UnityEngine;

namespace Leap.Unity
{
	public class PalmDirectionDetector : Detector
	{
		[Units("seconds")]
		[Tooltip("The interval in seconds at which to check this detector's conditions.")]
		[MinValue(0f)]
		public float Period = 0.1f;

		[Tooltip("The hand model to watch. Set automatically if detector is on a hand.")]
		public HandModelBase HandModel;

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
		public float OnAngle = 45f;

		[Tooltip("The angle in degrees from the target direction at which to turn off.")]
		[Range(0f, 180f)]
		public float OffAngle = 65f;

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
			watcherCoroutine = palmWatcher();
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

		private IEnumerator palmWatcher()
		{
			while (true)
			{
				if (HandModel != null)
				{
					Hand hand = HandModel.GetLeapHand();
					if (hand != null)
					{
						Vector3 normal = hand.PalmNormal.ToVector3();
						float num = Vector3.Angle(normal, selectedDirection(hand.PalmPosition.ToVector3()));
						if (num <= OnAngle)
						{
							Activate();
						}
						else if (num > OffAngle)
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
				if (TargetObject != null)
				{
					return TargetObject.position - tipPosition;
				}
				return Vector3.zero;
			default:
				return PointingDirection;
			}
		}
	}
}
