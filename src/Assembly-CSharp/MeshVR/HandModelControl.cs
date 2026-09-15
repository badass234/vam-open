using System;
using System.Collections.Generic;
using GPUTools.Physics.Scripts.Behaviours;
using UnityEngine;

namespace MeshVR
{
	public class HandModelControl : JSONStorable
	{
		[Serializable]
		public class Hand
		{
			public string name;

			public Transform transform;
		}

		public enum Axis
		{
			X,
			Y,
			Z
		}

		public Hand[] leftHands;

		public Hand[] rightHands;

		public Hand disabledLeftHand;

		public Hand disabledRightHand;

		protected JSONStorableStringChooser leftHandChooser;

		[SerializeField]
		protected string _leftHandChoice = "None";

		protected JSONStorableBool leftHandEnabledJSON;

		[SerializeField]
		protected bool _leftHandEnabled;

		protected JSONStorableStringChooser rightHandChooser;

		[SerializeField]
		protected string _rightHandChoice = "None";

		protected JSONStorableBool rightHandEnabledJSON;

		[SerializeField]
		protected bool _rightHandEnabled;

		[SerializeField]
		protected bool _linkHands = true;

		protected JSONStorableBool linkHandsJSON;

		public bool alwaysLinkHands;

		protected List<ConfigurableJointReconnector> leftJointReconnectors;

		protected List<ConfigurableJointReconnector> rightJointReconnectors;

		protected Vector3 _leftPosition;

		protected Vector3 _rightPosition;

		public Axis forwardBackAxis;

		public Axis rightLeftAxis;

		public Axis upDownAxis;

		protected JSONStorableFloat xPositionJSON;

		protected float _xPosition;

		protected JSONStorableFloat yPositionJSON;

		protected float _yPosition;

		protected JSONStorableFloat zPositionJSON;

		protected float _zPosition;

		protected Vector3 _leftRotation;

		protected Vector3 _rightRotation;

		protected Quaternion _qLeftRotation;

		protected Quaternion _qRightRotation;

		protected JSONStorableFloat xRotationJSON;

		protected float _xRotation;

		protected JSONStorableFloat yRotationJSON;

		protected float _yRotation;

		protected JSONStorableFloat zRotationJSON;

		protected float _zRotation;

		protected bool _ignorePositionRotationLeft;

		protected bool _ignorePositionRotationRight;

		public int collisionLayer;

		public int noCollisionLayer;

		protected List<Collider> colliders;

		protected List<CapsuleLineSphereCollider> gpuLineSphereColliders;

		protected List<GpuSphereCollider> gpuSphereColliders;

		[SerializeField]
		protected bool _useCollision;

		protected JSONStorableBool useCollisionJSON;

		public string leftHandChoice
		{
			get
			{
				if (leftHandChooser != null)
				{
					return leftHandChooser.val;
				}
				return _leftHandChoice;
			}
			set
			{
				if (leftHandChooser != null)
				{
					leftHandChooser.valNoCallback = value;
				}
				SetLeftHand(value);
			}
		}

		public bool leftHandEnabled
		{
			get
			{
				if (leftHandEnabledJSON != null)
				{
					return leftHandEnabledJSON.val;
				}
				return _leftHandEnabled;
			}
			set
			{
				if (leftHandEnabledJSON != null)
				{
					leftHandEnabledJSON.val = value;
				}
				else
				{
					SyncLeftHandEnabled(value);
				}
			}
		}

		public string rightHandChoice
		{
			get
			{
				if (rightHandChooser != null)
				{
					return rightHandChooser.val;
				}
				return _rightHandChoice;
			}
			set
			{
				if (rightHandChooser != null)
				{
					rightHandChooser.valNoCallback = value;
				}
				SetRightHand(value);
			}
		}

		public bool rightHandEnabled
		{
			get
			{
				if (rightHandEnabledJSON != null)
				{
					return rightHandEnabledJSON.val;
				}
				return _rightHandEnabled;
			}
			set
			{
				if (rightHandEnabledJSON != null)
				{
					rightHandEnabledJSON.val = value;
				}
				else
				{
					SyncRightHandEnabled(value);
				}
			}
		}

		public bool linkHands
		{
			get
			{
				if (linkHandsJSON != null)
				{
					return linkHandsJSON.val;
				}
				return _linkHands;
			}
			set
			{
				if (linkHandsJSON != null)
				{
					linkHandsJSON.valNoCallback = value;
				}
				else
				{
					_linkHands = value;
				}
			}
		}

		public float xPosition
		{
			get
			{
				if (xPositionJSON != null)
				{
					return xPositionJSON.val;
				}
				return _xPosition;
			}
			set
			{
				if (xPositionJSON != null)
				{
					xPositionJSON.valNoCallback = value;
				}
				SetXPosition(value);
			}
		}

		public float yPosition
		{
			get
			{
				if (yPositionJSON != null)
				{
					return yPositionJSON.val;
				}
				return _yPosition;
			}
			set
			{
				if (yPositionJSON != null)
				{
					yPositionJSON.valNoCallback = value;
				}
				SetYPosition(value);
			}
		}

		public float zPosition
		{
			get
			{
				if (zPositionJSON != null)
				{
					return zPositionJSON.val;
				}
				return _zPosition;
			}
			set
			{
				if (zPositionJSON != null)
				{
					zPositionJSON.valNoCallback = value;
				}
				SetZPosition(value);
			}
		}

		public float xRotation
		{
			get
			{
				if (xRotationJSON != null)
				{
					return xRotationJSON.val;
				}
				return _xRotation;
			}
			set
			{
				if (xRotationJSON != null)
				{
					xRotationJSON.valNoCallback = value;
				}
				SetXRotation(value);
			}
		}

		public float yRotation
		{
			get
			{
				if (yRotationJSON != null)
				{
					return yRotationJSON.val;
				}
				return _yRotation;
			}
			set
			{
				if (yRotationJSON != null)
				{
					yRotationJSON.valNoCallback = value;
				}
				SetYRotation(value);
			}
		}

		public float zRotation
		{
			get
			{
				if (zRotationJSON != null)
				{
					return zRotationJSON.val;
				}
				return _zRotation;
			}
			set
			{
				if (zRotationJSON != null)
				{
					zRotationJSON.valNoCallback = value;
				}
				SetZRotation(value);
			}
		}

		public bool ignorePositionRotationLeft
		{
			get
			{
				return _ignorePositionRotationLeft;
			}
			set
			{
				if (_ignorePositionRotationLeft != value)
				{
					_ignorePositionRotationLeft = value;
					SyncPosition();
					SyncRotation();
				}
			}
		}

		public bool ignorePositionRotationRight
		{
			get
			{
				return _ignorePositionRotationRight;
			}
			set
			{
				if (_ignorePositionRotationRight != value)
				{
					_ignorePositionRotationRight = value;
					SyncPosition();
					SyncRotation();
				}
			}
		}

		public bool useCollision
		{
			get
			{
				if (useCollisionJSON != null)
				{
					return useCollisionJSON.val;
				}
				return _useCollision;
			}
			set
			{
				if (useCollisionJSON != null)
				{
					useCollisionJSON.valNoCallback = value;
				}
				SetUseCollision(value);
			}
		}

		protected virtual void SetLeftHand(string choice)
		{
			bool flag = false;
			for (int i = 0; i < leftHands.Length; i++)
			{
				if (leftHands[i].name == choice)
				{
					_leftHandChoice = choice;
					if (leftHands[i].transform != null)
					{
						if (_leftHandEnabled)
						{
							leftHands[i].transform.gameObject.SetActive(true);
						}
						else
						{
							leftHands[i].transform.gameObject.SetActive(false);
						}
					}
					flag = true;
				}
				else if (leftHands[i].transform != null)
				{
					leftHands[i].transform.gameObject.SetActive(false);
				}
			}
			if (disabledLeftHand != null && disabledLeftHand.transform != null)
			{
				if (!_leftHandEnabled || !flag)
				{
					disabledLeftHand.transform.gameObject.SetActive(true);
				}
				else
				{
					disabledLeftHand.transform.gameObject.SetActive(false);
				}
			}
		}

		protected void SyncLeftHand(string choice)
		{
			SetLeftHand(choice);
			if (_linkHands || alwaysLinkHands)
			{
				rightHandChooser.val = _leftHandChoice;
			}
			UserPreferences.singleton.SavePreferences();
		}

		protected void SyncLeftHandEnabled(bool b)
		{
			_leftHandEnabled = b;
			SetLeftHand(_leftHandChoice);
		}

		public void ToggleLeftHandEnabled()
		{
			leftHandEnabled = !_leftHandEnabled;
		}

		protected virtual void SetRightHand(string choice)
		{
			bool flag = false;
			for (int i = 0; i < rightHands.Length; i++)
			{
				if (rightHands[i].name == choice)
				{
					_rightHandChoice = choice;
					if (rightHands[i].transform != null)
					{
						if (_rightHandEnabled)
						{
							rightHands[i].transform.gameObject.SetActive(true);
						}
						else
						{
							rightHands[i].transform.gameObject.SetActive(false);
						}
					}
					flag = true;
				}
				else if (rightHands[i].transform != null)
				{
					rightHands[i].transform.gameObject.SetActive(false);
				}
			}
			if (disabledRightHand != null && disabledRightHand.transform != null)
			{
				if (!_rightHandEnabled || !flag)
				{
					disabledRightHand.transform.gameObject.SetActive(true);
				}
				else
				{
					disabledRightHand.transform.gameObject.SetActive(false);
				}
			}
		}

		protected void SyncRightHand(string choice)
		{
			SetRightHand(choice);
			if (_linkHands || alwaysLinkHands)
			{
				leftHandChooser.val = _rightHandChoice;
			}
			UserPreferences.singleton.SavePreferences();
		}

		protected void SyncRightHandEnabled(bool b)
		{
			_rightHandEnabled = b;
			SetRightHand(_rightHandChoice);
		}

		public void ToggleRightHandEnabled()
		{
			rightHandEnabled = !_rightHandEnabled;
		}

		public void ToggleBothHandsEnabled()
		{
			if (_rightHandEnabled || _leftHandEnabled)
			{
				rightHandEnabled = false;
				leftHandEnabled = false;
			}
			else
			{
				rightHandEnabled = true;
				leftHandEnabled = true;
			}
		}

		protected void SyncLinkHands(bool b)
		{
			_linkHands = b;
			if (_linkHands || alwaysLinkHands)
			{
				rightHandChoice = leftHandChoice;
			}
			UserPreferences.singleton.SavePreferences();
		}

		protected void FindJointReconnectors()
		{
			leftJointReconnectors = new List<ConfigurableJointReconnector>();
			Hand[] array = leftHands;
			foreach (Hand hand in array)
			{
				if (!(hand.transform != null))
				{
					continue;
				}
				ConfigurableJointReconnector[] componentsInChildren = hand.transform.GetComponentsInChildren<ConfigurableJointReconnector>();
				ConfigurableJointReconnector[] array2 = componentsInChildren;
				foreach (ConfigurableJointReconnector configurableJointReconnector in array2)
				{
					if (configurableJointReconnector.controlRelativePositionAndRotation)
					{
						leftJointReconnectors.Add(configurableJointReconnector);
					}
				}
			}
			rightJointReconnectors = new List<ConfigurableJointReconnector>();
			Hand[] array3 = rightHands;
			foreach (Hand hand2 in array3)
			{
				if (!(hand2.transform != null))
				{
					continue;
				}
				ConfigurableJointReconnector[] componentsInChildren2 = hand2.transform.GetComponentsInChildren<ConfigurableJointReconnector>();
				ConfigurableJointReconnector[] array4 = componentsInChildren2;
				foreach (ConfigurableJointReconnector configurableJointReconnector2 in array4)
				{
					if (configurableJointReconnector2.controlRelativePositionAndRotation)
					{
						rightJointReconnectors.Add(configurableJointReconnector2);
					}
				}
			}
		}

		protected void SyncPosition()
		{
			_rightPosition.x = _xPosition;
			_rightPosition.y = _yPosition;
			_rightPosition.z = _zPosition;
			_leftPosition = _rightPosition;
			switch (rightLeftAxis)
			{
			case Axis.X:
				_leftPosition.x = 0f - _xPosition;
				break;
			case Axis.Y:
				_leftPosition.y = 0f - _yPosition;
				break;
			case Axis.Z:
				_leftPosition.z = 0f - _zPosition;
				break;
			}
			foreach (ConfigurableJointReconnector leftJointReconnector in leftJointReconnectors)
			{
				if (_ignorePositionRotationLeft)
				{
					leftJointReconnector.controlledRelativePosition = Vector3.zero;
				}
				else
				{
					leftJointReconnector.controlledRelativePosition = _leftPosition;
				}
			}
			foreach (ConfigurableJointReconnector rightJointReconnector in rightJointReconnectors)
			{
				if (_ignorePositionRotationRight)
				{
					rightJointReconnector.controlledRelativePosition = Vector3.zero;
				}
				else
				{
					rightJointReconnector.controlledRelativePosition = _rightPosition;
				}
			}
		}

		protected void SetXPosition(float f)
		{
			_xPosition = f;
			SyncPosition();
		}

		protected void SyncXPosition(float f)
		{
			SetXPosition(f);
			UserPreferences.singleton.SavePreferences();
		}

		protected void SetYPosition(float f)
		{
			_yPosition = f;
			SyncPosition();
		}

		protected void SyncYPosition(float f)
		{
			SetYPosition(f);
			UserPreferences.singleton.SavePreferences();
		}

		protected void SetZPosition(float f)
		{
			_zPosition = f;
			SyncPosition();
		}

		protected void SyncZPosition(float f)
		{
			SetZPosition(f);
			UserPreferences.singleton.SavePreferences();
		}

		protected void SyncRotation()
		{
			_rightRotation.x = _xRotation;
			_rightRotation.y = _yRotation;
			_rightRotation.z = _zRotation;
			_leftRotation = _rightRotation;
			switch (upDownAxis)
			{
			case Axis.X:
				_leftRotation.x = 0f - _xRotation;
				break;
			case Axis.Y:
				_leftRotation.y = 0f - _yRotation;
				break;
			case Axis.Z:
				_leftRotation.z = 0f - _zRotation;
				break;
			}
			switch (forwardBackAxis)
			{
			case Axis.X:
				_leftRotation.x = 0f - _xRotation;
				break;
			case Axis.Y:
				_leftRotation.y = 0f - _yRotation;
				break;
			case Axis.Z:
				_leftRotation.z = 0f - _zRotation;
				break;
			}
			_qRightRotation = Quaternion.Euler(_rightRotation);
			_qLeftRotation = Quaternion.Euler(_leftRotation);
			foreach (ConfigurableJointReconnector leftJointReconnector in leftJointReconnectors)
			{
				if (_ignorePositionRotationLeft)
				{
					leftJointReconnector.controlledRelativeRotation = Quaternion.identity;
				}
				else
				{
					leftJointReconnector.controlledRelativeRotation = _qLeftRotation;
				}
			}
			foreach (ConfigurableJointReconnector rightJointReconnector in rightJointReconnectors)
			{
				if (_ignorePositionRotationRight)
				{
					rightJointReconnector.controlledRelativeRotation = Quaternion.identity;
				}
				else
				{
					rightJointReconnector.controlledRelativeRotation = _qRightRotation;
				}
			}
		}

		protected void SetXRotation(float f)
		{
			_xRotation = f;
			SyncRotation();
		}

		protected void SyncXRotation(float f)
		{
			SetXRotation(f);
			UserPreferences.singleton.SavePreferences();
		}

		protected void SetYRotation(float f)
		{
			_yRotation = f;
			SyncRotation();
		}

		protected void SyncYRotation(float f)
		{
			SetYRotation(f);
			UserPreferences.singleton.SavePreferences();
		}

		protected void SetZRotation(float f)
		{
			_zRotation = f;
			SyncRotation();
		}

		protected void SyncZRotation(float f)
		{
			SetZRotation(f);
			UserPreferences.singleton.SavePreferences();
		}

		public void ToggleCollision()
		{
			useCollision = !_useCollision;
		}

		protected void SetUseCollision(bool b)
		{
			_useCollision = b;
			if (colliders != null)
			{
				foreach (Collider collider in colliders)
				{
					if (collider != null)
					{
						if (b)
						{
							collider.gameObject.layer = collisionLayer;
						}
						else
						{
							collider.gameObject.layer = noCollisionLayer;
						}
					}
				}
			}
			if (gpuLineSphereColliders != null)
			{
				foreach (CapsuleLineSphereCollider gpuLineSphereCollider in gpuLineSphereColliders)
				{
					if (gpuLineSphereCollider != null)
					{
						gpuLineSphereCollider.enabled = b;
					}
				}
			}
			if (gpuSphereColliders == null)
			{
				return;
			}
			foreach (GpuSphereCollider gpuSphereCollider in gpuSphereColliders)
			{
				if (gpuSphereCollider != null)
				{
					gpuSphereCollider.enabled = b;
				}
			}
		}

		protected void SyncUseCollision(bool b)
		{
			_useCollision = b;
			SetUseCollision(b);
			UserPreferences.singleton.SavePreferences();
		}

		protected void FindCollidersInHandArray(Hand[] hands)
		{
			foreach (Hand hand in hands)
			{
				if (hand.transform != null)
				{
					Collider[] componentsInChildren = hand.transform.GetComponentsInChildren<Collider>(true);
					Collider[] array = componentsInChildren;
					foreach (Collider item in array)
					{
						colliders.Add(item);
					}
					CapsuleLineSphereCollider[] componentsInChildren2 = hand.transform.GetComponentsInChildren<CapsuleLineSphereCollider>(true);
					CapsuleLineSphereCollider[] array2 = componentsInChildren2;
					foreach (CapsuleLineSphereCollider item2 in array2)
					{
						gpuLineSphereColliders.Add(item2);
					}
					GpuSphereCollider[] componentsInChildren3 = hand.transform.GetComponentsInChildren<GpuSphereCollider>(true);
					GpuSphereCollider[] array3 = componentsInChildren3;
					foreach (GpuSphereCollider item3 in array3)
					{
						gpuSphereColliders.Add(item3);
					}
				}
			}
		}

		protected void FindColliders()
		{
			colliders = new List<Collider>();
			gpuLineSphereColliders = new List<CapsuleLineSphereCollider>();
			gpuSphereColliders = new List<GpuSphereCollider>();
			FindCollidersInHandArray(leftHands);
			FindCollidersInHandArray(rightHands);
		}

		protected virtual void Init()
		{
			List<string> list = new List<string>();
			Hand[] array = leftHands;
			foreach (Hand hand in array)
			{
				list.Add(hand.name);
			}
			List<string> list2 = new List<string>();
			Hand[] array2 = rightHands;
			foreach (Hand hand2 in array2)
			{
				list2.Add(hand2.name);
			}
			if (alwaysLinkHands)
			{
				leftHandChooser = new JSONStorableStringChooser("leftHandChoice", list, _leftHandChoice, "Hand Choice", SyncLeftHand);
			}
			else
			{
				leftHandChooser = new JSONStorableStringChooser("leftHandChoice", list, _leftHandChoice, "Left Hand Choice", SyncLeftHand);
			}
			rightHandChooser = new JSONStorableStringChooser("rightHandChoice", list2, _rightHandChoice, "Right Hand Choice", SyncRightHand);
			linkHandsJSON = new JSONStorableBool("linkHands", _linkHands, SyncLinkHands);
			leftHandEnabledJSON = new JSONStorableBool("leftHandEnabled", _leftHandEnabled, SyncLeftHandEnabled);
			rightHandEnabledJSON = new JSONStorableBool("rightHandEnabled", _rightHandEnabled, SyncRightHandEnabled);
			SetLeftHand(_leftHandChoice);
			SetRightHand(_rightHandChoice);
			useCollisionJSON = new JSONStorableBool("useCollision", _useCollision, SyncUseCollision);
			xPositionJSON = new JSONStorableFloat("xPosition", _xPosition, SyncXPosition, -0.2f, 0.2f);
			yPositionJSON = new JSONStorableFloat("yPosition", _yPosition, SyncYPosition, -0.2f, 0.2f);
			zPositionJSON = new JSONStorableFloat("zPosition", _zPosition, SyncZPosition, -0.2f, 0.2f);
			xRotationJSON = new JSONStorableFloat("xRotation", _xRotation, SyncXRotation, -90f, 90f);
			yRotationJSON = new JSONStorableFloat("yRotation", _yRotation, SyncYRotation, -90f, 90f);
			zRotationJSON = new JSONStorableFloat("zRotation", _zRotation, SyncZRotation, -90f, 90f);
			FindColliders();
			SetUseCollision(_useCollision);
			FindJointReconnectors();
			SyncPosition();
			SyncRotation();
		}

		protected override void InitUI(Transform t, bool isAlt)
		{
			if (t != null)
			{
				HandModelControlUI componentInChildren = t.GetComponentInChildren<HandModelControlUI>(true);
				if (componentInChildren != null)
				{
					leftHandChooser.RegisterPopup(componentInChildren.leftHandChooserPopup, isAlt);
					rightHandChooser.RegisterPopup(componentInChildren.rightHandChooserPopup, isAlt);
					leftHandEnabledJSON.RegisterToggle(componentInChildren.leftHandEnabledToggle, isAlt);
					rightHandEnabledJSON.RegisterToggle(componentInChildren.rightHandEnabledToggle, isAlt);
					linkHandsJSON.RegisterToggle(componentInChildren.linkHandsToggle, isAlt);
					useCollisionJSON.RegisterToggle(componentInChildren.useCollisionToggle, isAlt);
					xPositionJSON.RegisterSlider(componentInChildren.xPositionSlider, isAlt);
					yPositionJSON.RegisterSlider(componentInChildren.yPositionSlider, isAlt);
					zPositionJSON.RegisterSlider(componentInChildren.zPositionSlider, isAlt);
					xRotationJSON.RegisterSlider(componentInChildren.xRotationSlider, isAlt);
					yRotationJSON.RegisterSlider(componentInChildren.yRotationSlider, isAlt);
					zRotationJSON.RegisterSlider(componentInChildren.zRotationSlider, isAlt);
				}
			}
		}

		protected override void Awake()
		{
			if (!awakecalled)
			{
				base.Awake();
				Init();
				InitUI();
				InitUIAlt();
			}
		}
	}
}
