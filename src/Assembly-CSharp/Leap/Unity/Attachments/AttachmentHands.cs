using System;
using UnityEngine;

namespace Leap.Unity.Attachments
{
	[ExecuteInEditMode]
	public class AttachmentHands : MonoBehaviour
	{
		[SerializeField]
		private AttachmentPointFlags _attachmentPoints = AttachmentPointFlags.Wrist | AttachmentPointFlags.Palm;

		private Func<Hand>[] _handAccessors;

		private AttachmentHand[] _attachmentHands;

		public AttachmentPointFlags attachmentPoints
		{
			get
			{
				return _attachmentPoints;
			}
			set
			{
				if (_attachmentPoints != value)
				{
					_attachmentPoints = value;
					refreshAttachmentHandTransforms();
				}
			}
		}

		public Func<Hand>[] handAccessors
		{
			get
			{
				return _handAccessors;
			}
			set
			{
				_handAccessors = value;
			}
		}

		public AttachmentHand[] attachmentHands
		{
			get
			{
				return _attachmentHands;
			}
			set
			{
				_attachmentHands = value;
			}
		}

		private void Awake()
		{
			reinitialize();
		}

		private void reinitialize()
		{
			refreshHandAccessors();
			refreshAttachmentHands();
			refreshAttachmentHandTransforms();
		}

		private void Update()
		{
			bool flag = false;
			using (new ProfilerSample("Attachment Hands Update", base.gameObject))
			{
				for (int i = 0; i < _attachmentHands.Length; i++)
				{
					AttachmentHand attachmentHand = attachmentHands[i];
					if (attachmentHand == null)
					{
						flag = true;
						break;
					}
					Hand hand = handAccessors[i]();
					attachmentHand.isTracked = hand != null;
					using (new ProfilerSample(attachmentHand.gameObject.name + " Update Points"))
					{
						AttachmentHand.AttachmentPointsEnumerator enumerator = attachmentHand.points.GetEnumerator();
						while (enumerator.MoveNext())
						{
							AttachmentPointBehaviour current = enumerator.Current;
							current.SetTransformUsingHand(hand);
						}
					}
				}
				if (flag)
				{
					reinitialize();
				}
			}
		}

		private void refreshHandAccessors()
		{
			if (_handAccessors == null || _handAccessors.Length == 0)
			{
				_handAccessors = new Func<Hand>[2];
				_handAccessors[0] = () => Hands.Left;
				_handAccessors[1] = () => Hands.Right;
			}
		}

		private void refreshAttachmentHands()
		{
			bool flag = false;
			if (_attachmentHands != null && _attachmentHands.Length != 0 && !(_attachmentHands[0] == null) && !(_attachmentHands[1] == null))
			{
				return;
			}
			_attachmentHands = new AttachmentHand[2];
			foreach (Transform child in base.transform.GetChildren())
			{
				AttachmentHand component = child.GetComponent<AttachmentHand>();
				if (component != null)
				{
					_attachmentHands[(component.chirality != 0) ? 1 : 0] = component;
				}
			}
			if (!flag || (!(_attachmentHands[0] == null) && !(_attachmentHands[0].transform.parent != base.transform) && !(_attachmentHands[1] == null) && !(_attachmentHands[1].transform.parent != base.transform)))
			{
				if (_attachmentHands[0] == null)
				{
					GameObject gameObject = new GameObject();
					_attachmentHands[0] = gameObject.AddComponent<AttachmentHand>();
					_attachmentHands[0].chirality = Chirality.Left;
				}
				_attachmentHands[0].gameObject.name = "Attachment Hand (Left)";
				if (_attachmentHands[0].transform.parent != base.transform)
				{
					_attachmentHands[0].transform.parent = base.transform;
				}
				if (_attachmentHands[1] == null)
				{
					GameObject gameObject2 = new GameObject();
					_attachmentHands[1] = gameObject2.AddComponent<AttachmentHand>();
					_attachmentHands[1].chirality = Chirality.Right;
				}
				_attachmentHands[1].gameObject.name = "Attachment Hand (Right)";
				if (_attachmentHands[1].transform.parent != base.transform)
				{
					_attachmentHands[1].transform.parent = base.transform;
				}
				_attachmentHands[0].transform.SetSiblingIndex(0);
				_attachmentHands[1].transform.SetSiblingIndex(1);
			}
		}

		private void refreshAttachmentHandTransforms()
		{
			if (this == null)
			{
				return;
			}
			bool flag = false;
			if (_attachmentHands == null)
			{
				flag = true;
			}
			else
			{
				AttachmentHand[] array = _attachmentHands;
				foreach (AttachmentHand attachmentHand in array)
				{
					if (attachmentHand == null)
					{
						flag = true;
						break;
					}
					attachmentHand.refreshAttachmentTransforms(_attachmentPoints);
				}
			}
			if (flag)
			{
				reinitialize();
			}
		}
	}
}
