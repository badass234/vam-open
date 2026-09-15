using UnityEngine;

public class AllJointsController : JSONStorable
{
	public FreeControllerV3[] freeControllers;

	public FreeControllerV3[] keyControllers;

	public FreeControllerV3[] keyControllersAlt;

	protected JSONStorableAction SetOnlyKeyJointsOnAction;

	protected JSONStorableAction SetOnlyKeyJointsComplyAction;

	protected JSONStorableAction SetOnlyKeyAltJointsOnAction;

	protected JSONStorableAction SetOnlyKeyAltJointsComplyAction;

	protected JSONStorableAction SetAllJointsOnAction;

	protected JSONStorableAction SetAllJointsComplyAction;

	protected JSONStorableAction SetAllJointsControlPositionAction;

	protected JSONStorableAction SetAllJointsControlRotationAction;

	protected JSONStorableAction SetAllJointsOffAction;

	protected JSONStorableAction SetAllJointsMaxHoldSpringAction;

	protected float _springPercent = 0.2f;

	protected JSONStorableFloat springPercentJSON;

	protected JSONStorableAction SetAllJointsPercentHoldSpringAction;

	protected JSONStorableAction SetAllJointsMinHoldSpringAction;

	protected JSONStorableAction SetAllJointsMaxHoldDamperAction;

	protected float _damperPercent = 0.2f;

	protected JSONStorableFloat damperPercentJSON;

	protected JSONStorableAction SetAllJointsPercentHoldDamperAction;

	protected JSONStorableAction SetAllJointsMinHoldDamperAction;

	protected float _maxVelocity = 1f;

	protected JSONStorableFloat maxVelocityJSON;

	protected JSONStorableAction SetAllJointsMaxVelocityAction;

	protected JSONStorableAction SetAllJointsDisableMaxVelocityAction;

	protected float _maxVelocitySuperSoft = 0.1f;

	protected JSONStorableAction SetAllJointsMaxVelocitySuperSoftAction;

	protected float _maxVelocitySoft = 0.2f;

	protected JSONStorableAction SetAllJointsMaxVelocitySoftAction;

	protected float _maxVelocityNormal = 10f;

	protected JSONStorableAction SetAllJointsMaxVelocityNormalAction;

	public void SetOnlyKeyJointsOn()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.currentPositionState = FreeControllerV3.PositionState.Off;
				freeControllerV.currentRotationState = FreeControllerV3.RotationState.Off;
			}
		}
		if (keyControllers != null)
		{
			FreeControllerV3[] array2 = keyControllers;
			foreach (FreeControllerV3 freeControllerV2 in array2)
			{
				freeControllerV2.currentPositionState = FreeControllerV3.PositionState.On;
				freeControllerV2.currentRotationState = FreeControllerV3.RotationState.On;
			}
		}
	}

	public void SetOnlyKeyJointsComply()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.currentPositionState = FreeControllerV3.PositionState.Off;
				freeControllerV.currentRotationState = FreeControllerV3.RotationState.Off;
			}
		}
		if (keyControllers != null)
		{
			FreeControllerV3[] array2 = keyControllers;
			foreach (FreeControllerV3 freeControllerV2 in array2)
			{
				freeControllerV2.currentPositionState = FreeControllerV3.PositionState.Comply;
				freeControllerV2.currentRotationState = FreeControllerV3.RotationState.Comply;
			}
		}
	}

	public void SetOnlyKeyAltJointsOn()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.currentPositionState = FreeControllerV3.PositionState.Off;
				freeControllerV.currentRotationState = FreeControllerV3.RotationState.Off;
			}
		}
		if (keyControllers != null)
		{
			FreeControllerV3[] array2 = keyControllersAlt;
			foreach (FreeControllerV3 freeControllerV2 in array2)
			{
				freeControllerV2.currentPositionState = FreeControllerV3.PositionState.On;
				freeControllerV2.currentRotationState = FreeControllerV3.RotationState.On;
			}
		}
	}

	public void SetOnlyKeyAltJointsComply()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.currentPositionState = FreeControllerV3.PositionState.Off;
				freeControllerV.currentRotationState = FreeControllerV3.RotationState.Off;
			}
		}
		if (keyControllers != null)
		{
			FreeControllerV3[] array2 = keyControllersAlt;
			foreach (FreeControllerV3 freeControllerV2 in array2)
			{
				freeControllerV2.currentPositionState = FreeControllerV3.PositionState.Comply;
				freeControllerV2.currentRotationState = FreeControllerV3.RotationState.Comply;
			}
		}
	}

	public void SetAllJointsOn()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.currentPositionState = FreeControllerV3.PositionState.On;
				freeControllerV.currentRotationState = FreeControllerV3.RotationState.On;
			}
		}
	}

	public void SetAllJointsComply()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.currentPositionState = FreeControllerV3.PositionState.Comply;
				freeControllerV.currentRotationState = FreeControllerV3.RotationState.Comply;
			}
		}
	}

	public void SetAllJointsControlPosition()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.currentPositionState = FreeControllerV3.PositionState.On;
				freeControllerV.currentRotationState = FreeControllerV3.RotationState.Off;
			}
		}
	}

	public void SetAllJointsControlRotation()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.currentPositionState = FreeControllerV3.PositionState.Off;
				freeControllerV.currentRotationState = FreeControllerV3.RotationState.On;
			}
		}
	}

	public void SetAllJointsOff()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.currentPositionState = FreeControllerV3.PositionState.Off;
				freeControllerV.currentRotationState = FreeControllerV3.RotationState.Off;
			}
		}
	}

	public void SetAllJointsMaxHoldSpring()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.SetHoldPositionSpringMax();
				freeControllerV.SetHoldRotationSpringMax();
			}
		}
	}

	protected void SyncSpringPercent(float f)
	{
		_springPercent = f;
	}

	public void SetAllJointsPercentHoldSpring()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.SetHoldPositionSpringPercent(_springPercent);
				freeControllerV.SetHoldRotationSpringPercent(_springPercent);
			}
		}
	}

	public void SetAllJointsMinHoldSpring()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.SetHoldPositionSpringMin();
				freeControllerV.SetHoldRotationSpringMin();
			}
		}
	}

	public void SetAllJointsMaxHoldDamper()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.SetHoldPositionDamperMax();
				freeControllerV.SetHoldRotationDamperMax();
			}
		}
	}

	protected void SyncDamperPercent(float f)
	{
		_damperPercent = f;
	}

	public void SetAllJointsPercentHoldDamper()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.SetHoldPositionDamperPercent(_damperPercent);
				freeControllerV.SetHoldRotationDamperPercent(_damperPercent);
			}
		}
	}

	public void SetAllJointsMinHoldDamper()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.SetHoldPositionDamperMin();
				freeControllerV.SetHoldRotationDamperMin();
			}
		}
	}

	protected void SyncMaxVelocity(float f)
	{
		_maxVelocity = f;
	}

	public void SetAllJointsMaxVelocity()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.RBMaxVelocityEnable = true;
				freeControllerV.RBMaxVelocity = _maxVelocity;
			}
		}
	}

	public void SetAllJointsDisableMaxVelocity()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.RBMaxVelocityEnable = false;
			}
		}
	}

	public void SetAllJointsMaxVelocity(float f)
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.RBMaxVelocityEnable = true;
				freeControllerV.RBMaxVelocity = f;
			}
		}
	}

	public void SetAllJointsMaxVelocitySuperSoft()
	{
		SetAllJointsMaxVelocity(_maxVelocitySuperSoft);
	}

	public void SetAllJointsMaxVelocitySoft()
	{
		SetAllJointsMaxVelocity(_maxVelocitySoft);
	}

	public void SetAllJointsMaxVelocityNormal()
	{
		SetAllJointsMaxVelocity(_maxVelocityNormal);
	}

	protected void Init()
	{
		SetOnlyKeyJointsOnAction = new JSONStorableAction("SetOnlyKeyJointsOn", SetOnlyKeyJointsOn);
		RegisterAction(SetOnlyKeyJointsOnAction);
		SetOnlyKeyJointsComplyAction = new JSONStorableAction("SetOnlyKeyJointsComply", SetOnlyKeyJointsComply);
		RegisterAction(SetOnlyKeyJointsComplyAction);
		SetOnlyKeyAltJointsOnAction = new JSONStorableAction("SetOnlyKeyAltJointsOn", SetOnlyKeyAltJointsOn);
		RegisterAction(SetOnlyKeyAltJointsOnAction);
		SetOnlyKeyAltJointsComplyAction = new JSONStorableAction("SetOnlyKeyAltJointsComply", SetOnlyKeyAltJointsComply);
		RegisterAction(SetOnlyKeyAltJointsComplyAction);
		SetAllJointsOnAction = new JSONStorableAction("SetAllJointsOn", SetAllJointsOn);
		RegisterAction(SetAllJointsOnAction);
		SetAllJointsComplyAction = new JSONStorableAction("SetAllJointsComply", SetAllJointsComply);
		RegisterAction(SetAllJointsComplyAction);
		SetAllJointsControlPositionAction = new JSONStorableAction("SetAllJointsControlPosition", SetAllJointsControlPosition);
		RegisterAction(SetAllJointsControlPositionAction);
		SetAllJointsControlRotationAction = new JSONStorableAction("SetAllJointsControlRotation", SetAllJointsControlRotation);
		RegisterAction(SetAllJointsControlRotationAction);
		SetAllJointsOffAction = new JSONStorableAction("SetAllJointsOff", SetAllJointsOff);
		RegisterAction(SetAllJointsOffAction);
		SetAllJointsMaxHoldSpringAction = new JSONStorableAction("SetAllJointsMaxHoldSpring", SetAllJointsMaxHoldSpring);
		RegisterAction(SetAllJointsMaxHoldSpringAction);
		springPercentJSON = new JSONStorableFloat("springPercent", _springPercent, SyncSpringPercent, 0f, 1f);
		springPercentJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(springPercentJSON);
		SetAllJointsPercentHoldSpringAction = new JSONStorableAction("SetAllJointsPercentHoldSpring", SetAllJointsPercentHoldSpring);
		RegisterAction(SetAllJointsPercentHoldSpringAction);
		SetAllJointsMinHoldSpringAction = new JSONStorableAction("SetAllJointsMinHoldSpring", SetAllJointsMinHoldSpring);
		RegisterAction(SetAllJointsMinHoldSpringAction);
		SetAllJointsMaxHoldDamperAction = new JSONStorableAction("SetAllJointsMaxHoldDamper", SetAllJointsMaxHoldDamper);
		RegisterAction(SetAllJointsMaxHoldDamperAction);
		damperPercentJSON = new JSONStorableFloat("damperPercent", _damperPercent, SyncDamperPercent, 0f, 1f);
		damperPercentJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(damperPercentJSON);
		SetAllJointsPercentHoldDamperAction = new JSONStorableAction("SetAllJointsPercentHoldDamper", SetAllJointsPercentHoldDamper);
		RegisterAction(SetAllJointsPercentHoldDamperAction);
		SetAllJointsMinHoldDamperAction = new JSONStorableAction("SetAllJointsMinHoldDamper", SetAllJointsMinHoldDamper);
		RegisterAction(SetAllJointsMinHoldDamperAction);
		maxVelocityJSON = new JSONStorableFloat("maxVeloctiy", _maxVelocity, SyncMaxVelocity, 0f, 100f);
		maxVelocityJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(maxVelocityJSON);
		SetAllJointsMaxVelocityAction = new JSONStorableAction("SetAllJointsMaxVelocity", SetAllJointsMaxVelocity);
		RegisterAction(SetAllJointsMaxVelocityAction);
		SetAllJointsDisableMaxVelocityAction = new JSONStorableAction("SetAllJointsDisableMaxVelocity", SetAllJointsDisableMaxVelocity);
		RegisterAction(SetAllJointsDisableMaxVelocityAction);
		SetAllJointsMaxVelocitySuperSoftAction = new JSONStorableAction("SetAllJointsMaxVelocitySuperSoft", SetAllJointsMaxVelocitySuperSoft);
		RegisterAction(SetAllJointsMaxVelocitySuperSoftAction);
		SetAllJointsMaxVelocitySoftAction = new JSONStorableAction("SetAllJointsMaxVelocitySoft", SetAllJointsMaxVelocitySoft);
		RegisterAction(SetAllJointsMaxVelocitySoftAction);
		SetAllJointsMaxVelocityNormalAction = new JSONStorableAction("SetAllJointsMaxVelocityNormal", SetAllJointsMaxVelocityNormal);
		RegisterAction(SetAllJointsMaxVelocityNormalAction);
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (t != null)
		{
			AllJointsControllerUI componentInChildren = t.GetComponentInChildren<AllJointsControllerUI>(true);
			if (componentInChildren != null)
			{
				SetOnlyKeyJointsOnAction.RegisterButton(componentInChildren.SetOnlyKeyJointsOnButton, isAlt);
				SetOnlyKeyJointsComplyAction.RegisterButton(componentInChildren.SetOnlyKeyJointsComplyButton, isAlt);
				SetOnlyKeyAltJointsOnAction.RegisterButton(componentInChildren.SetOnlyKeyAltJointsOnButton, isAlt);
				SetOnlyKeyAltJointsComplyAction.RegisterButton(componentInChildren.SetOnlyKeyAltJointsComplyButton, isAlt);
				SetAllJointsOnAction.RegisterButton(componentInChildren.SetAllJointsOnButton);
				SetAllJointsOnAction.RegisterButton(componentInChildren.SetAllJointsOnButtonAlt, true);
				SetAllJointsComplyAction.RegisterButton(componentInChildren.SetAllJointsComplyButton, isAlt);
				SetAllJointsControlPositionAction.RegisterButton(componentInChildren.SetAllJointsControlPositionButton, isAlt);
				SetAllJointsControlRotationAction.RegisterButton(componentInChildren.SetAllJointsControlRotationButton, isAlt);
				SetAllJointsOffAction.RegisterButton(componentInChildren.SetAllJointsOffButton, isAlt);
				SetAllJointsMaxHoldSpringAction.RegisterButton(componentInChildren.SetAllJointsMaxHoldSpringButton, isAlt);
				springPercentJSON.RegisterSlider(componentInChildren.springPercentSlider, isAlt);
				SetAllJointsPercentHoldSpringAction.RegisterButton(componentInChildren.SetAllJointsPercentHoldSpringButton, isAlt);
				SetAllJointsMinHoldSpringAction.RegisterButton(componentInChildren.SetAllJointsMinHoldSpringButton, isAlt);
				SetAllJointsMaxHoldDamperAction.RegisterButton(componentInChildren.SetAllJointsMaxHoldDamperButton, isAlt);
				damperPercentJSON.RegisterSlider(componentInChildren.damperPercentSlider, isAlt);
				SetAllJointsPercentHoldDamperAction.RegisterButton(componentInChildren.SetAllJointsPercentHoldDamperButton, isAlt);
				SetAllJointsMinHoldDamperAction.RegisterButton(componentInChildren.SetAllJointsMinHoldDamperButton, isAlt);
				maxVelocityJSON.RegisterSlider(componentInChildren.maxVelocitySlider, isAlt);
				SetAllJointsMaxVelocityAction.RegisterButton(componentInChildren.SetAllJointsMaxVelocityButton, isAlt);
				SetAllJointsDisableMaxVelocityAction.RegisterButton(componentInChildren.SetAllJointsDisableMaxVelocityButton, isAlt);
				SetAllJointsMaxVelocitySuperSoftAction.RegisterButton(componentInChildren.SetAllJointsMaxVelocitySuperSoftButton, isAlt);
				SetAllJointsMaxVelocitySoftAction.RegisterButton(componentInChildren.SetAllJointsMaxVelocitySoftButton, isAlt);
				SetAllJointsMaxVelocityNormalAction.RegisterButton(componentInChildren.SetAllJointsMaxVelocityNormalButton, isAlt);
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
