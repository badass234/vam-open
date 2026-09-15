using UnityEngine;

namespace Leap.Unity
{
	public abstract class AbstractHoldDetector : Detector
	{
		[SerializeField]
		protected HandModelBase _handModel;

		[Tooltip("Whether to change the transform of the parent object.")]
		public bool ControlsTransform = true;

		[Tooltip("Draw this detector's Gizmos, if any. (Gizmos must be on in Unity edtor, too.)")]
		public bool ShowGizmos = true;

		protected int _lastUpdateFrame = -1;

		protected bool _didChange;

		protected Vector3 _position;

		protected Quaternion _rotation;

		protected Vector3 _direction = Vector3.forward;

		protected Vector3 _normal = Vector3.up;

		protected float _distance;

		protected float _lastHoldTime;

		protected float _lastReleaseTime;

		protected Vector3 _lastPosition = Vector3.zero;

		protected Quaternion _lastRotation = Quaternion.identity;

		protected Vector3 _lastDirection = Vector3.forward;

		protected Vector3 _lastNormal = Vector3.up;

		protected float _lastDistance = 1f;

		public HandModelBase HandModel
		{
			get
			{
				return _handModel;
			}
			set
			{
				_handModel = value;
			}
		}

		public virtual bool IsHolding
		{
			get
			{
				ensureUpToDate();
				return base.IsActive;
			}
		}

		public virtual bool DidChangeFromLastFrame
		{
			get
			{
				ensureUpToDate();
				return _didChange;
			}
		}

		public virtual bool DidStartHold
		{
			get
			{
				ensureUpToDate();
				return DidChangeFromLastFrame && IsHolding;
			}
		}

		public virtual bool DidRelease
		{
			get
			{
				ensureUpToDate();
				return DidChangeFromLastFrame && !IsHolding;
			}
		}

		public float LastHoldTime
		{
			get
			{
				ensureUpToDate();
				return _lastHoldTime;
			}
		}

		public float LastReleaseTime
		{
			get
			{
				ensureUpToDate();
				return _lastReleaseTime;
			}
		}

		public Vector3 Position
		{
			get
			{
				ensureUpToDate();
				return _position;
			}
		}

		public Vector3 LastActivePosition => _lastPosition;

		public Quaternion Rotation
		{
			get
			{
				ensureUpToDate();
				return _rotation;
			}
		}

		public Quaternion LastActiveRotation => _lastRotation;

		public Vector3 Direction => _direction;

		public Vector3 LastActiveDirection => _lastDirection;

		public Vector3 Normal => _normal;

		public Vector3 LastActiveNormal => _lastNormal;

		public float Distance => _distance;

		public float LastActiveDistance => _lastDistance;

		protected abstract void ensureUpToDate();

		protected virtual void Awake()
		{
			if (GetComponent<HandModelBase>() != null && ControlsTransform)
			{
				Debug.LogWarning("Detector should not be control the HandModelBase's transform. Either attach it to its own transform or set ControlsTransform to false.");
			}
			if (_handModel == null)
			{
				_handModel = GetComponentInParent<HandModelBase>();
				if (_handModel == null)
				{
					Debug.LogWarning("The HandModel field of Detector was unassigned and the detector has been disabled.");
					base.enabled = false;
				}
			}
		}

		protected virtual void Update()
		{
			ensureUpToDate();
		}

		protected virtual void changeState(bool shouldBeActive)
		{
			bool isActive = base.IsActive;
			if (shouldBeActive)
			{
				_lastHoldTime = Time.time;
				Activate();
			}
			else
			{
				_lastReleaseTime = Time.time;
				Deactivate();
			}
			if (isActive != base.IsActive)
			{
				_didChange = true;
			}
		}
	}
}
