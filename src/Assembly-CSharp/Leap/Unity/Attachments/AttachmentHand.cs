using System;
using Leap.Unity.Attributes;
using Leap.Unity.Query;
using UnityEngine;

namespace Leap.Unity.Attachments
{
	[AddComponentMenu("")]
	[ExecuteInEditMode]
	public class AttachmentHand : MonoBehaviour
	{
		public struct AttachmentPointsEnumerator
		{
			private int _curIdx;

			private AttachmentHand _hand;

			private int _flagsCount;

			public AttachmentPointBehaviour Current
			{
				get
				{
					if (_hand == null)
					{
						return null;
					}
					return _hand.GetBehaviourForPoint(GetFlagFromFlagIdx(_curIdx));
				}
			}

			public AttachmentPointsEnumerator(AttachmentHand hand)
			{
				if (hand != null && hand._attachmentPointFlagConstants != null)
				{
					_curIdx = -1;
					_hand = hand;
					_flagsCount = hand._attachmentPointFlagConstants.Length;
				}
				else
				{
					_curIdx = -1;
					_hand = null;
					_flagsCount = 0;
				}
			}

			public AttachmentPointsEnumerator GetEnumerator()
			{
				return this;
			}

			public bool MoveNext()
			{
				do
				{
					_curIdx++;
				}
				while (_curIdx < _flagsCount && _hand.GetBehaviourForPoint(GetFlagFromFlagIdx(_curIdx)) == null);
				return _curIdx < _flagsCount;
			}
		}

		public Action OnAttachmentPointsModified = delegate
		{
		};

		[HideInInspector]
		public AttachmentPointBehaviour wrist;

		[HideInInspector]
		public AttachmentPointBehaviour palm;

		[HideInInspector]
		public AttachmentPointBehaviour thumbProximalJoint;

		[HideInInspector]
		public AttachmentPointBehaviour thumbDistalJoint;

		[HideInInspector]
		public AttachmentPointBehaviour thumbTip;

		[HideInInspector]
		public AttachmentPointBehaviour indexKnuckle;

		[HideInInspector]
		public AttachmentPointBehaviour indexMiddleJoint;

		[HideInInspector]
		public AttachmentPointBehaviour indexDistalJoint;

		[HideInInspector]
		public AttachmentPointBehaviour indexTip;

		[HideInInspector]
		public AttachmentPointBehaviour middleKnuckle;

		[HideInInspector]
		public AttachmentPointBehaviour middleMiddleJoint;

		[HideInInspector]
		public AttachmentPointBehaviour middleDistalJoint;

		[HideInInspector]
		public AttachmentPointBehaviour middleTip;

		[HideInInspector]
		public AttachmentPointBehaviour ringKnuckle;

		[HideInInspector]
		public AttachmentPointBehaviour ringMiddleJoint;

		[HideInInspector]
		public AttachmentPointBehaviour ringDistalJoint;

		[HideInInspector]
		public AttachmentPointBehaviour ringTip;

		[HideInInspector]
		public AttachmentPointBehaviour pinkyKnuckle;

		[HideInInspector]
		public AttachmentPointBehaviour pinkyMiddleJoint;

		[HideInInspector]
		public AttachmentPointBehaviour pinkyDistalJoint;

		[HideInInspector]
		public AttachmentPointBehaviour pinkyTip;

		private bool _attachmentPointsDirty;

		[SerializeField]
		[Disable]
		private Chirality _chirality;

		[SerializeField]
		[Disable]
		private bool _isTracked;

		private bool _isBeingDestroyed;

		private AttachmentPointFlags[] _attachmentPointFlagConstants;

		private static Transform[] s_hierarchyTransformsBuffer = new Transform[4];

		private static Transform[] s_transformsBuffer = new Transform[4];

		public AttachmentPointsEnumerator points => new AttachmentPointsEnumerator(this);

		public Chirality chirality
		{
			get
			{
				return _chirality;
			}
			set
			{
				_chirality = value;
			}
		}

		public bool isTracked
		{
			get
			{
				return _isTracked;
			}
			set
			{
				_isTracked = value;
			}
		}

		private void OnValidate()
		{
			initializeAttachmentPointFlagConstants();
		}

		private void Awake()
		{
			initializeAttachmentPointFlagConstants();
		}

		private void OnDestroy()
		{
			_isBeingDestroyed = true;
		}

		public AttachmentPointBehaviour GetBehaviourForPoint(AttachmentPointFlags singlePoint)
		{
			AttachmentPointBehaviour result = null;
			switch (singlePoint)
			{
			case AttachmentPointFlags.Wrist:
				result = wrist;
				break;
			case AttachmentPointFlags.Palm:
				result = palm;
				break;
			case AttachmentPointFlags.ThumbProximalJoint:
				result = thumbProximalJoint;
				break;
			case AttachmentPointFlags.ThumbDistalJoint:
				result = thumbDistalJoint;
				break;
			case AttachmentPointFlags.ThumbTip:
				result = thumbTip;
				break;
			case AttachmentPointFlags.IndexKnuckle:
				result = indexKnuckle;
				break;
			case AttachmentPointFlags.IndexMiddleJoint:
				result = indexMiddleJoint;
				break;
			case AttachmentPointFlags.IndexDistalJoint:
				result = indexDistalJoint;
				break;
			case AttachmentPointFlags.IndexTip:
				result = indexTip;
				break;
			case AttachmentPointFlags.MiddleKnuckle:
				result = middleKnuckle;
				break;
			case AttachmentPointFlags.MiddleMiddleJoint:
				result = middleMiddleJoint;
				break;
			case AttachmentPointFlags.MiddleDistalJoint:
				result = middleDistalJoint;
				break;
			case AttachmentPointFlags.MiddleTip:
				result = middleTip;
				break;
			case AttachmentPointFlags.RingKnuckle:
				result = ringKnuckle;
				break;
			case AttachmentPointFlags.RingMiddleJoint:
				result = ringMiddleJoint;
				break;
			case AttachmentPointFlags.RingDistalJoint:
				result = ringDistalJoint;
				break;
			case AttachmentPointFlags.RingTip:
				result = ringTip;
				break;
			case AttachmentPointFlags.PinkyKnuckle:
				result = pinkyKnuckle;
				break;
			case AttachmentPointFlags.PinkyMiddleJoint:
				result = pinkyMiddleJoint;
				break;
			case AttachmentPointFlags.PinkyDistalJoint:
				result = pinkyDistalJoint;
				break;
			case AttachmentPointFlags.PinkyTip:
				result = pinkyTip;
				break;
			}
			return result;
		}

		public void refreshAttachmentTransforms(AttachmentPointFlags points)
		{
			if (_attachmentPointFlagConstants == null || _attachmentPointFlagConstants.Length == 0)
			{
				initializeAttachmentPointFlagConstants();
			}
			bool flag = false;
			AttachmentPointFlags[] attachmentPointFlagConstants = _attachmentPointFlagConstants;
			foreach (AttachmentPointFlags attachmentPointFlags in attachmentPointFlagConstants)
			{
				if (attachmentPointFlags != 0 && ((!points.Contains(attachmentPointFlags) && GetBehaviourForPoint(attachmentPointFlags) != null) || (points.Contains(attachmentPointFlags) && GetBehaviourForPoint(attachmentPointFlags) == null)))
				{
					flag = true;
				}
			}
			if (flag)
			{
				flattenAttachmentTransformHierarchy();
				AttachmentPointFlags[] attachmentPointFlagConstants2 = _attachmentPointFlagConstants;
				foreach (AttachmentPointFlags attachmentPointFlags2 in attachmentPointFlagConstants2)
				{
					if (attachmentPointFlags2 != 0)
					{
						if (points.Contains(attachmentPointFlags2))
						{
							ensureTransformExists(attachmentPointFlags2);
						}
						else
						{
							ensureTransformDoesNotExist(attachmentPointFlags2);
						}
					}
				}
				organizeAttachmentTransforms();
			}
			if (_attachmentPointsDirty)
			{
				OnAttachmentPointsModified();
				_attachmentPointsDirty = false;
			}
		}

		public void notifyPointBehaviourDeleted(AttachmentPointBehaviour point)
		{
		}

		private void initializeAttachmentPointFlagConstants()
		{
			Array values = Enum.GetValues(typeof(AttachmentPointFlags));
			if (_attachmentPointFlagConstants == null || _attachmentPointFlagConstants.Length == 0)
			{
				_attachmentPointFlagConstants = new AttachmentPointFlags[values.Length];
			}
			int num = 0;
			foreach (int item in values)
			{
				_attachmentPointFlagConstants[num++] = (AttachmentPointFlags)item;
			}
		}

		private void setBehaviourForPoint(AttachmentPointFlags singlePoint, AttachmentPointBehaviour behaviour)
		{
			switch (singlePoint)
			{
			case AttachmentPointFlags.Wrist:
				wrist = behaviour;
				break;
			case AttachmentPointFlags.Palm:
				palm = behaviour;
				break;
			case AttachmentPointFlags.ThumbProximalJoint:
				thumbProximalJoint = behaviour;
				break;
			case AttachmentPointFlags.ThumbDistalJoint:
				thumbDistalJoint = behaviour;
				break;
			case AttachmentPointFlags.ThumbTip:
				thumbTip = behaviour;
				break;
			case AttachmentPointFlags.IndexKnuckle:
				indexKnuckle = behaviour;
				break;
			case AttachmentPointFlags.IndexMiddleJoint:
				indexMiddleJoint = behaviour;
				break;
			case AttachmentPointFlags.IndexDistalJoint:
				indexDistalJoint = behaviour;
				break;
			case AttachmentPointFlags.IndexTip:
				indexTip = behaviour;
				break;
			case AttachmentPointFlags.MiddleKnuckle:
				middleKnuckle = behaviour;
				break;
			case AttachmentPointFlags.MiddleMiddleJoint:
				middleMiddleJoint = behaviour;
				break;
			case AttachmentPointFlags.MiddleDistalJoint:
				middleDistalJoint = behaviour;
				break;
			case AttachmentPointFlags.MiddleTip:
				middleTip = behaviour;
				break;
			case AttachmentPointFlags.RingKnuckle:
				ringKnuckle = behaviour;
				break;
			case AttachmentPointFlags.RingMiddleJoint:
				ringMiddleJoint = behaviour;
				break;
			case AttachmentPointFlags.RingDistalJoint:
				ringDistalJoint = behaviour;
				break;
			case AttachmentPointFlags.RingTip:
				ringTip = behaviour;
				break;
			case AttachmentPointFlags.PinkyKnuckle:
				pinkyKnuckle = behaviour;
				break;
			case AttachmentPointFlags.PinkyMiddleJoint:
				pinkyMiddleJoint = behaviour;
				break;
			case AttachmentPointFlags.PinkyDistalJoint:
				pinkyDistalJoint = behaviour;
				break;
			case AttachmentPointFlags.PinkyTip:
				pinkyTip = behaviour;
				break;
			}
		}

		private void ensureTransformExists(AttachmentPointFlags singlePoint)
		{
			if (!singlePoint.IsSinglePoint())
			{
				Debug.LogError("Tried to ensure transform exists for singlePoint, but it contains more than one set flag.");
				return;
			}
			AttachmentPointBehaviour behaviourForPoint = GetBehaviourForPoint(singlePoint);
			if (behaviourForPoint == null)
			{
				AttachmentPointBehaviour attachmentPointBehaviour = base.gameObject.GetComponentsInChildren<AttachmentPointBehaviour>().Query().FirstOrDefault((AttachmentPointBehaviour p) => p.attachmentPoint == singlePoint);
				if ((AttachmentPointFlags)attachmentPointBehaviour == AttachmentPointFlags.None)
				{
					GameObject gameObject = new GameObject(Enum.GetName(typeof(AttachmentPointFlags), singlePoint));
					behaviourForPoint = gameObject.AddComponent<AttachmentPointBehaviour>();
				}
				else
				{
					behaviourForPoint = attachmentPointBehaviour;
				}
				behaviourForPoint.attachmentPoint = singlePoint;
				behaviourForPoint.attachmentHand = this;
				setBehaviourForPoint(singlePoint, behaviourForPoint);
				SetTransformParent(behaviourForPoint.transform, base.transform);
				_attachmentPointsDirty = true;
			}
		}

		private static void SetTransformParent(Transform t, Transform parent)
		{
			t.parent = parent;
		}

		private void ensureTransformDoesNotExist(AttachmentPointFlags singlePoint)
		{
			if (!singlePoint.IsSinglePoint())
			{
				Debug.LogError("Tried to ensure transform exists for singlePoint, but it contains more than one set flag");
				return;
			}
			AttachmentPointBehaviour behaviourForPoint = GetBehaviourForPoint(singlePoint);
			if (behaviourForPoint != null)
			{
				InternalUtility.Destroy(behaviourForPoint.gameObject);
				setBehaviourForPoint(singlePoint, null);
				behaviourForPoint = null;
				_attachmentPointsDirty = true;
			}
		}

		private void flattenAttachmentTransformHierarchy()
		{
			AttachmentPointsEnumerator enumerator = points.GetEnumerator();
			while (enumerator.MoveNext())
			{
				AttachmentPointBehaviour current = enumerator.Current;
				SetTransformParent(current.transform, base.transform);
			}
		}

		private void organizeAttachmentTransforms()
		{
			int num = 0;
			if (wrist != null)
			{
				wrist.transform.SetSiblingIndex(num++);
			}
			if (palm != null)
			{
				palm.transform.SetSiblingIndex(num++);
			}
			Transform transform = tryStackTransformHierarchy(thumbProximalJoint, thumbDistalJoint, thumbTip);
			if (transform != null)
			{
				transform.SetSiblingIndex(num++);
			}
			transform = tryStackTransformHierarchy(indexKnuckle, indexMiddleJoint, indexDistalJoint, indexTip);
			if (transform != null)
			{
				transform.SetSiblingIndex(num++);
			}
			transform = tryStackTransformHierarchy(middleKnuckle, middleMiddleJoint, middleDistalJoint, middleTip);
			if (transform != null)
			{
				transform.SetSiblingIndex(num++);
			}
			transform = tryStackTransformHierarchy(ringKnuckle, ringMiddleJoint, ringDistalJoint, ringTip);
			if (transform != null)
			{
				transform.SetSiblingIndex(num++);
			}
			transform = tryStackTransformHierarchy(pinkyKnuckle, pinkyMiddleJoint, pinkyDistalJoint, pinkyTip);
			if (transform != null)
			{
				transform.SetSiblingIndex(num++);
			}
		}

		private Transform tryStackTransformHierarchy(params Transform[] transforms)
		{
			for (int i = 0; i < s_hierarchyTransformsBuffer.Length; i++)
			{
				s_hierarchyTransformsBuffer[i] = null;
			}
			int num = 0;
			foreach (Transform item in from t in transforms.Query()
				where t != null
				select t)
			{
				s_hierarchyTransformsBuffer[num++] = item;
			}
			for (int num2 = num - 1; num2 > 0; num2--)
			{
				SetTransformParent(s_hierarchyTransformsBuffer[num2], s_hierarchyTransformsBuffer[num2 - 1]);
			}
			if (num > 0)
			{
				return s_hierarchyTransformsBuffer[0];
			}
			return null;
		}

		private Transform tryStackTransformHierarchy(params MonoBehaviour[] monoBehaviours)
		{
			for (int i = 0; i < s_transformsBuffer.Length; i++)
			{
				s_transformsBuffer[i] = null;
			}
			int num = 0;
			foreach (MonoBehaviour item in from b in monoBehaviours.Query()
				where b != null
				select b)
			{
				s_transformsBuffer[num++] = item.transform;
			}
			return tryStackTransformHierarchy(s_transformsBuffer);
		}

		private static AttachmentPointFlags GetFlagFromFlagIdx(int pointIdx)
		{
			return (AttachmentPointFlags)(1 << pointIdx + 1);
		}
	}
}
