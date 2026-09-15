using Leap.Unity.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Leap.Unity
{
	public class PinchDetector : AbstractHoldDetector
	{
		protected const float MM_TO_M = 0.001f;

		[Tooltip("The distance at which to enter the pinching state.")]
		[Header("Distance Settings")]
		[MinValue(0f)]
		[Units("meters")]
		[FormerlySerializedAs("_activatePinchDist")]
		public float ActivateDistance = 0.03f;

		[Tooltip("The distance at which to leave the pinching state.")]
		[MinValue(0f)]
		[Units("meters")]
		[FormerlySerializedAs("_deactivatePinchDist")]
		public float DeactivateDistance = 0.04f;

		protected bool _isPinching;

		protected float _lastPinchTime;

		protected float _lastUnpinchTime;

		protected Vector3 _pinchPos;

		protected Quaternion _pinchRotation;

		public bool IsPinching => IsHolding;

		public bool DidStartPinch => DidStartHold;

		public bool DidEndPinch => DidRelease;

		protected virtual void OnValidate()
		{
			ActivateDistance = Mathf.Max(0f, ActivateDistance);
			DeactivateDistance = Mathf.Max(0f, DeactivateDistance);
			if (DeactivateDistance < ActivateDistance)
			{
				DeactivateDistance = ActivateDistance;
			}
		}

		private float GetPinchDistance(Hand hand)
		{
			Vector3 a = hand.GetIndex().TipPosition.ToVector3();
			Vector3 b = hand.GetThumb().TipPosition.ToVector3();
			return Vector3.Distance(a, b) / base.transform.lossyScale.x;
		}

		protected override void ensureUpToDate()
		{
			if (Time.frameCount == _lastUpdateFrame)
			{
				return;
			}
			_lastUpdateFrame = Time.frameCount;
			_didChange = false;
			Hand leapHand = _handModel.GetLeapHand();
			if (leapHand == null || !_handModel.IsTracked)
			{
				changeState(false);
				return;
			}
			_distance = GetPinchDistance(leapHand);
			_rotation = leapHand.Basis.CalculateRotation();
			_position = ((leapHand.Fingers[0].TipPosition + leapHand.Fingers[1].TipPosition) * 0.5f).ToVector3();
			if (base.IsActive)
			{
				if (_distance > DeactivateDistance)
				{
					changeState(false);
				}
			}
			else if (_distance < ActivateDistance)
			{
				changeState(true);
			}
			if (base.IsActive)
			{
				_lastPosition = _position;
				_lastRotation = _rotation;
				_lastDistance = _distance;
				_lastDirection = _direction;
				_lastNormal = _normal;
			}
			if (ControlsTransform)
			{
				base.transform.position = _position;
				base.transform.rotation = _rotation;
			}
		}
	}
}
