using Leap.Unity;
using UnityEngine;

namespace MeshVR
{
	public class LeapHandModelControl : HandModelControl
	{
		public HandModelManager handModelManager;

		protected RiggedHand currentLeftHand;

		protected RiggedHand currentRightHand;

		protected LeapMotionGrabber[] grabbers;

		[SerializeField]
		protected bool _allowPinchGrab;

		protected JSONStorableBool allowPinchGrabJSON;

		public bool allowPinchGrab
		{
			get
			{
				if (allowPinchGrabJSON != null)
				{
					return allowPinchGrabJSON.val;
				}
				return _allowPinchGrab;
			}
			set
			{
				if (allowPinchGrabJSON != null)
				{
					allowPinchGrabJSON.valNoCallback = value;
				}
				SetPinchGrab(value);
			}
		}

		protected void SyncHands()
		{
			if (handModelManager != null)
			{
				handModelManager.DisableGroup("Main");
				handModelManager.RemoveGroupWait("Main");
				handModelManager.AddNewGroupWait("Main", currentLeftHand, currentRightHand);
				handModelManager.EnableGroup("Main");
			}
		}

		protected override void SetLeftHand(string choice)
		{
			bool flag = false;
			for (int i = 0; i < leftHands.Length; i++)
			{
				if (!(leftHands[i].name == choice))
				{
					continue;
				}
				_leftHandChoice = choice;
				if (leftHands[i].transform != null)
				{
					if (_leftHandEnabled)
					{
						RiggedHand component = leftHands[i].transform.GetComponent<RiggedHand>();
						currentLeftHand = component;
					}
					else
					{
						currentLeftHand = null;
					}
				}
				else
				{
					currentLeftHand = null;
				}
				flag = true;
				break;
			}
			if (!flag)
			{
				_leftHandChoice = "None";
				currentLeftHand = null;
			}
			SyncHands();
		}

		protected override void SetRightHand(string choice)
		{
			bool flag = false;
			for (int i = 0; i < rightHands.Length; i++)
			{
				if (!(rightHands[i].name == choice))
				{
					continue;
				}
				_rightHandChoice = choice;
				if (rightHands[i].transform != null)
				{
					if (_rightHandEnabled)
					{
						RiggedHand component = rightHands[i].transform.GetComponent<RiggedHand>();
						currentRightHand = component;
					}
					else
					{
						currentRightHand = null;
					}
				}
				else
				{
					currentRightHand = null;
				}
				flag = true;
				break;
			}
			if (!flag)
			{
				_rightHandChoice = "None";
				currentRightHand = null;
			}
			SyncHands();
		}

		protected void FindGrabbers()
		{
			grabbers = GetComponentsInChildren<LeapMotionGrabber>(true);
		}

		protected void SetPinchGrab(bool b)
		{
			_allowPinchGrab = b;
			LeapMotionGrabber[] array = grabbers;
			foreach (LeapMotionGrabber leapMotionGrabber in array)
			{
				leapMotionGrabber.controllerGrabOn = b;
			}
		}

		protected void SyncAllowPinchGrab(bool b)
		{
			SetPinchGrab(b);
			UserPreferences.singleton.SavePreferences();
		}

		protected override void Init()
		{
			base.Init();
			allowPinchGrabJSON = new JSONStorableBool("allowPinchGrab", _allowPinchGrab, SyncAllowPinchGrab);
			FindGrabbers();
			SetPinchGrab(_allowPinchGrab);
		}

		protected override void InitUI(Transform t, bool isAlt)
		{
			base.InitUI(t, isAlt);
			if (t != null)
			{
				LeapHandModelControlUI componentInChildren = t.GetComponentInChildren<LeapHandModelControlUI>(true);
				if (componentInChildren != null)
				{
					allowPinchGrabJSON.RegisterToggle(componentInChildren.allowPinchGrabToggle, isAlt);
				}
			}
		}
	}
}
