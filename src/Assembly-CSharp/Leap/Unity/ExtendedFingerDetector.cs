using System;
using System.Collections;
using Leap.Unity.Attributes;
using UnityEngine;

namespace Leap.Unity
{
	public class ExtendedFingerDetector : Detector
	{
		[Tooltip("The interval in seconds at which to check this detector's conditions.")]
		[Units("seconds")]
		[MinValue(0f)]
		public float Period = 0.1f;

		[Tooltip("The hand model to watch. Set automatically if detector is on a hand.")]
		public HandModelBase HandModel;

		[Header("Finger States")]
		[Tooltip("Required state of the thumb.")]
		public PointingState Thumb = PointingState.Either;

		[Tooltip("Required state of the index finger.")]
		public PointingState Index = PointingState.Either;

		[Tooltip("Required state of the middle finger.")]
		public PointingState Middle = PointingState.Either;

		[Tooltip("Required state of the ring finger.")]
		public PointingState Ring = PointingState.Either;

		[Tooltip("Required state of the little finger.")]
		public PointingState Pinky = PointingState.Either;

		[Header("Min and Max Finger Counts")]
		[Range(0f, 5f)]
		[Tooltip("The minimum number of fingers extended.")]
		public int MinimumExtendedCount;

		[Range(0f, 5f)]
		[Tooltip("The maximum number of fingers extended.")]
		public int MaximumExtendedCount = 5;

		[Header("")]
		[Tooltip("Draw this detector's Gizmos, if any. (Gizmos must be on in Unity edtor, too.)")]
		public bool ShowGizmos = true;

		private IEnumerator watcherCoroutine;

		private void OnValidate()
		{
			int num = 0;
			int num2 = 0;
			PointingState[] array = new PointingState[5] { Thumb, Index, Middle, Ring, Pinky };
			for (int i = 0; i < array.Length; i++)
			{
				switch (array[i])
				{
				case PointingState.Extended:
					num++;
					break;
				case PointingState.NotExtended:
					num2++;
					break;
				}
				MinimumExtendedCount = Math.Max(num, MinimumExtendedCount);
				MaximumExtendedCount = Math.Min(5 - num2, MaximumExtendedCount);
				MaximumExtendedCount = Math.Max(num, MaximumExtendedCount);
			}
		}

		private void Awake()
		{
			watcherCoroutine = extendedFingerWatcher();
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

		private IEnumerator extendedFingerWatcher()
		{
			while (true)
			{
				bool fingerState = false;
				if (HandModel != null && HandModel.IsTracked)
				{
					Hand hand = HandModel.GetLeapHand();
					if (hand != null)
					{
						fingerState = matchFingerState(hand.Fingers[0], Thumb) && matchFingerState(hand.Fingers[1], Index) && matchFingerState(hand.Fingers[2], Middle) && matchFingerState(hand.Fingers[3], Ring) && matchFingerState(hand.Fingers[4], Pinky);
						int num = 0;
						for (int i = 0; i < 5; i++)
						{
							if (hand.Fingers[i].IsExtended)
							{
								num++;
							}
						}
						fingerState = fingerState && num <= MaximumExtendedCount && num >= MinimumExtendedCount;
						if (HandModel.IsTracked && fingerState)
						{
							Activate();
						}
						else if (!HandModel.IsTracked || !fingerState)
						{
							Deactivate();
						}
					}
				}
				else if (base.IsActive)
				{
					Deactivate();
				}
				yield return new WaitForSeconds(Period);
			}
		}

		private bool matchFingerState(Finger finger, PointingState requiredState)
		{
			return requiredState == PointingState.Either || (requiredState == PointingState.Extended && finger.IsExtended) || (requiredState == PointingState.NotExtended && !finger.IsExtended);
		}
	}
}
