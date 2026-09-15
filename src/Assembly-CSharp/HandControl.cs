using System;
using System.Collections.Generic;
using MeshVR.Hands;
using UnityEngine;

public class HandControl : JSONStorable
{
	public enum FingerControlMode
	{
		Morphs,
		JSONParams,
		Remote
	}

	public AdjustJointSprings handJointSprings;

	public JSONStorableFloat handGraspStrengthJSON;

	protected HandOutput handOutput;

	[SerializeField]
	protected FingerControlMode _fingerControlMode;

	public JSONStorableStringChooser fingerControlModeJSON;

	protected bool _possessed;

	[SerializeField]
	protected bool _allowPossessFingerControl = true;

	public JSONStorableBool allowPossessFingerControlJSON;

	public DAZBone thumbProximalBone;

	public DAZBone thumbMiddlaBone;

	public DAZBone thumbDistalBone;

	public DAZBone indexProximalBone;

	public DAZBone indexMiddlaBone;

	public DAZBone indexDistalBone;

	public DAZBone middleProximalBone;

	public DAZBone middleMiddlaBone;

	public DAZBone middleDistalBone;

	public DAZBone ringProximalBone;

	public DAZBone ringMiddlaBone;

	public DAZBone ringDistalBone;

	public DAZBone pinkyProximalBone;

	public DAZBone pinkyMiddlaBone;

	public DAZBone pinkyDistalBone;

	public bool possessed
	{
		get
		{
			return _possessed;
		}
		set
		{
			if (_possessed != value)
			{
				_possessed = value;
				SyncFingerControl();
			}
		}
	}

	public void SyncHandGraspStrength(float f)
	{
		if (handJointSprings != null)
		{
			handJointSprings.percent = f;
		}
	}

	protected void SyncFingerControl()
	{
		if ((_possessed && _allowPossessFingerControl) || _fingerControlMode != 0)
		{
			thumbProximalBone.rotationMorphsEnabled = false;
			thumbMiddlaBone.rotationMorphsEnabled = false;
			thumbDistalBone.rotationMorphsEnabled = false;
			indexProximalBone.rotationMorphsEnabled = false;
			indexMiddlaBone.rotationMorphsEnabled = false;
			indexDistalBone.rotationMorphsEnabled = false;
			middleProximalBone.rotationMorphsEnabled = false;
			middleMiddlaBone.rotationMorphsEnabled = false;
			middleDistalBone.rotationMorphsEnabled = false;
			ringProximalBone.rotationMorphsEnabled = false;
			ringMiddlaBone.rotationMorphsEnabled = false;
			ringDistalBone.rotationMorphsEnabled = false;
			pinkyProximalBone.rotationMorphsEnabled = false;
			pinkyMiddlaBone.rotationMorphsEnabled = false;
			pinkyDistalBone.rotationMorphsEnabled = false;
			if (handOutput != null)
			{
				handOutput.outputConnected = true;
				handOutput.inputConnected = (_possessed && _allowPossessFingerControl) || _fingerControlMode == FingerControlMode.Remote;
			}
		}
		else
		{
			if (handOutput != null)
			{
				handOutput.outputConnected = false;
				handOutput.inputConnected = false;
			}
			thumbProximalBone.rotationMorphsEnabled = true;
			thumbMiddlaBone.rotationMorphsEnabled = true;
			thumbDistalBone.rotationMorphsEnabled = true;
			indexProximalBone.rotationMorphsEnabled = true;
			indexMiddlaBone.rotationMorphsEnabled = true;
			indexDistalBone.rotationMorphsEnabled = true;
			middleProximalBone.rotationMorphsEnabled = true;
			middleMiddlaBone.rotationMorphsEnabled = true;
			middleDistalBone.rotationMorphsEnabled = true;
			ringProximalBone.rotationMorphsEnabled = true;
			ringMiddlaBone.rotationMorphsEnabled = true;
			ringDistalBone.rotationMorphsEnabled = true;
			pinkyProximalBone.rotationMorphsEnabled = true;
			pinkyMiddlaBone.rotationMorphsEnabled = true;
			pinkyDistalBone.rotationMorphsEnabled = true;
		}
	}

	protected void SyncFingerControlMode(string choice)
	{
		try
		{
			_fingerControlMode = (FingerControlMode)Enum.Parse(typeof(FingerControlMode), choice);
			SyncFingerControl();
		}
		catch (ArgumentException)
		{
			Debug.LogError("Attempted to set finger control mode to " + choice + " which is not a valid finger control mode");
		}
	}

	protected void SyncAllowPossessFingerControl(bool b)
	{
		_allowPossessFingerControl = b;
		SyncFingerControl();
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (t != null)
		{
			HandControlUI componentInChildren = t.GetComponentInChildren<HandControlUI>(true);
			if (componentInChildren != null)
			{
				handGraspStrengthJSON.RegisterSlider(componentInChildren.handGraspStrengthSlider, isAlt);
				fingerControlModeJSON.RegisterPopup(componentInChildren.fingerControlModePopup, isAlt);
				allowPossessFingerControlJSON.RegisterToggle(componentInChildren.allowPossessFingerControlToggle, isAlt);
			}
		}
	}

	protected void Init()
	{
		handOutput = GetComponent<HandOutput>();
		handGraspStrengthJSON = new JSONStorableFloat("handGraspStrength", 0.2f, SyncHandGraspStrength, 0f, 1f);
		handGraspStrengthJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(handGraspStrengthJSON);
		List<string> choicesList = new List<string>(Enum.GetNames(typeof(FingerControlMode)));
		fingerControlModeJSON = new JSONStorableStringChooser("fingerControlMode", choicesList, _fingerControlMode.ToString(), "Finger Control Mode", SyncFingerControlMode);
		fingerControlModeJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterStringChooser(fingerControlModeJSON);
		allowPossessFingerControlJSON = new JSONStorableBool("allowPossessFingerControl", _allowPossessFingerControl, SyncAllowPossessFingerControl);
		allowPossessFingerControlJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(allowPossessFingerControlJSON);
		SyncFingerControl();
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
