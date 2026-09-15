using System;
using System.Collections.Generic;
using UnityEngine;

public class EyesControl : LookAtWithLimitsControl
{
	public enum LookMode
	{
		None,
		Player,
		Target,
		Custom
	}

	protected JSONStorableStringChooser currentLookModeJSON;

	[SerializeField]
	protected LookMode _currentLookMode = LookMode.Player;

	[SerializeField]
	protected Transform _lookAt;

	protected Atom targetAtom;

	protected JSONStorableStringChooser targetAtomJSON;

	protected FreeControllerV3 targetControl;

	protected JSONStorableStringChooser targetControlJSON;

	protected float _divergeConvergeAngleAdjust;

	protected JSONStorableFloat divergeConvergeAngleAdjustJSON;

	public float blinkAdjustFactor = 0.1f;

	protected float _blinkWeight;

	public LookMode currentLookMode
	{
		get
		{
			return _currentLookMode;
		}
		set
		{
			if (currentLookModeJSON != null)
			{
				currentLookModeJSON.val = value.ToString();
			}
			else if (_currentLookMode != value)
			{
				SetLookMode(value.ToString());
			}
		}
	}

	public Transform lookAt
	{
		get
		{
			return _lookAt;
		}
		set
		{
			if (_lookAt != value)
			{
				_lookAt = value;
				SyncLookMode();
			}
		}
	}

	public float blinkWeight
	{
		get
		{
			return _blinkWeight;
		}
		set
		{
			if (_blinkWeight != value)
			{
				_blinkWeight = value;
				float moveFactor = Mathf.Clamp01(1f - _blinkWeight * blinkAdjustFactor);
				if (lookAt1 != null)
				{
					lookAt1.MoveFactor = moveFactor;
				}
				if (lookAt2 != null)
				{
					lookAt2.MoveFactor = moveFactor;
				}
			}
		}
	}

	protected void SyncLookMode()
	{
		switch (_currentLookMode)
		{
		case LookMode.None:
			if (lookAt1 != null)
			{
				lookAt1.enabled = false;
			}
			if (lookAt2 != null)
			{
				lookAt2.enabled = false;
			}
			break;
		case LookMode.Player:
			if (lookAt1 != null)
			{
				lookAt1.enabled = true;
				lookAt1.lookAtCameraLocation = CameraTarget.CameraLocation.Center;
			}
			if (lookAt2 != null)
			{
				lookAt2.enabled = true;
				lookAt2.lookAtCameraLocation = CameraTarget.CameraLocation.Center;
			}
			break;
		case LookMode.Target:
			if (lookAt1 != null)
			{
				lookAt1.enabled = true;
				lookAt1.lookAtCameraLocation = CameraTarget.CameraLocation.None;
				if (_lookAt != null)
				{
					lookAt1.target = _lookAt;
				}
				else
				{
					lookAt1.target = null;
				}
			}
			if (lookAt2 != null)
			{
				lookAt2.enabled = true;
				lookAt2.lookAtCameraLocation = CameraTarget.CameraLocation.None;
				if (_lookAt != null)
				{
					lookAt2.target = _lookAt;
				}
				else
				{
					lookAt2.target = null;
				}
			}
			break;
		case LookMode.Custom:
			if (lookAt1 != null)
			{
				lookAt1.enabled = true;
				lookAt1.lookAtCameraLocation = CameraTarget.CameraLocation.None;
				if (targetControl != null)
				{
					lookAt1.target = targetControl.control;
				}
				else
				{
					lookAt1.target = null;
				}
			}
			if (lookAt2 != null)
			{
				lookAt2.enabled = true;
				lookAt2.lookAtCameraLocation = CameraTarget.CameraLocation.None;
				if (targetControl != null)
				{
					lookAt2.target = targetControl.control;
				}
				else
				{
					lookAt2.target = null;
				}
			}
			break;
		}
		if (targetAtomJSON != null && targetAtomJSON.popup != null)
		{
			targetAtomJSON.popup.gameObject.SetActive(_currentLookMode == LookMode.Custom);
		}
		if (targetControlJSON != null && targetControlJSON.popup != null)
		{
			targetControlJSON.popup.gameObject.SetActive(_currentLookMode == LookMode.Custom);
		}
	}

	public void SetLookMode(string lookModeString)
	{
		try
		{
			LookMode lookMode = (LookMode)Enum.Parse(typeof(LookMode), lookModeString);
			_currentLookMode = lookMode;
			SyncLookMode();
		}
		catch (ArgumentException)
		{
			Debug.LogError("Attempted to set look mode type to " + lookModeString + " which is not a valid type");
		}
	}

	protected void OnAtomUIDRename(string fromid, string toid)
	{
		if (targetAtom != null && targetAtom.uid == toid)
		{
			targetAtomJSON.valNoCallback = toid;
		}
		SyncAtomChocies();
	}

	protected void OnAtomRemoved(Atom a)
	{
		if (targetAtom == a)
		{
			targetAtomJSON.val = "None";
		}
	}

	protected void SyncAtomChocies()
	{
		List<string> list = new List<string>();
		list.Add("None");
		foreach (string atomUIDsWithFreeController in SuperController.singleton.GetAtomUIDsWithFreeControllers())
		{
			list.Add(atomUIDsWithFreeController);
		}
		targetAtomJSON.choices = list;
	}

	protected void SyncTargetAtom(string atomUID)
	{
		List<string> list = new List<string>();
		list.Add("None");
		if (atomUID != null)
		{
			targetAtom = SuperController.singleton.GetAtomByUid(atomUID);
			if (targetAtom != null)
			{
				FreeControllerV3[] freeControllers = targetAtom.freeControllers;
				foreach (FreeControllerV3 freeControllerV in freeControllers)
				{
					list.Add(freeControllerV.name);
				}
			}
		}
		else
		{
			targetAtom = null;
		}
		targetControlJSON.choices = list;
		targetControlJSON.val = "None";
	}

	protected void SyncTargetControl(string targetID)
	{
		if (targetAtom != null && targetID != null && targetID != "None")
		{
			targetControl = SuperController.singleton.FreeControllerNameToFreeController(targetAtom.uid + ":" + targetID);
		}
		else
		{
			targetControl = null;
		}
		SyncLookMode();
	}

	protected override void SyncLookAtLeftRightAngleAdjust()
	{
		if (lookAt1 != null)
		{
			lookAt1.LeftRightAngleAdjust = _leftRightAngleAdjust - _divergeConvergeAngleAdjust;
		}
		if (lookAt2 != null)
		{
			lookAt2.LeftRightAngleAdjust = _leftRightAngleAdjust + _divergeConvergeAngleAdjust;
		}
	}

	protected void SyncDivergeConvergeAngleAdjust(float f)
	{
		_divergeConvergeAngleAdjust = f;
		SyncLookAtLeftRightAngleAdjust();
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		base.InitUI(t, isAlt);
		if (t != null)
		{
			EyesControlUI componentInChildren = t.GetComponentInChildren<EyesControlUI>(true);
			if (componentInChildren != null)
			{
				currentLookModeJSON.RegisterPopup(componentInChildren.lookModePopup, isAlt);
				targetAtomJSON.RegisterPopup(componentInChildren.targetAtomPopup, isAlt);
				targetControlJSON.RegisterPopup(componentInChildren.targetControlPopup, isAlt);
				divergeConvergeAngleAdjustJSON.RegisterSlider(componentInChildren.divergeConvergeAngleAdjustSlider, isAlt);
				SyncLookMode();
			}
		}
	}

	protected override void Init()
	{
		lookAt2InvertRightLeft = true;
		base.Init();
		List<string> choicesList = new List<string>(Enum.GetNames(typeof(LookMode)));
		currentLookModeJSON = new JSONStorableStringChooser("lookMode", choicesList, _currentLookMode.ToString(), "Eyes Look At", SetLookMode);
		currentLookModeJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterStringChooser(currentLookModeJSON);
		targetAtomJSON = new JSONStorableStringChooser("targetAtom", null, "None", "Target Atom", SyncTargetAtom);
		targetAtomJSON.storeType = JSONStorableParam.StoreType.Physical;
		targetAtomJSON.popupOpenCallback = SyncAtomChocies;
		RegisterStringChooser(targetAtomJSON);
		targetControlJSON = new JSONStorableStringChooser("targetControl", null, "None", "Target Control", SyncTargetControl);
		targetControlJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterStringChooser(targetControlJSON);
		divergeConvergeAngleAdjustJSON = new JSONStorableFloat("divergeConvergeAngleAdjust", _divergeConvergeAngleAdjust, SyncDivergeConvergeAngleAdjust, -30f, 30f);
		divergeConvergeAngleAdjustJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(divergeConvergeAngleAdjustJSON);
		SyncTargetAtom(null);
		SyncLookMode();
		if (SuperController.singleton != null)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Combine(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomUIDRename));
			SuperController singleton2 = SuperController.singleton;
			singleton2.onAtomRemovedHandlers = (SuperController.OnAtomRemoved)Delegate.Combine(singleton2.onAtomRemovedHandlers, new SuperController.OnAtomRemoved(OnAtomRemoved));
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

	protected void OnDestroy()
	{
		if (SuperController.singleton != null)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Remove(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomUIDRename));
			SuperController singleton2 = SuperController.singleton;
			singleton2.onAtomRemovedHandlers = (SuperController.OnAtomRemoved)Delegate.Remove(singleton2.onAtomRemovedHandlers, new SuperController.OnAtomRemoved(OnAtomRemoved));
		}
	}
}
