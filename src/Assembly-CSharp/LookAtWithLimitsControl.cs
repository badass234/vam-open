using UnityEngine;

public class LookAtWithLimitsControl : JSONStorable
{
	public LookAtWithLimits lookAt1;

	public LookAtWithLimits lookAt2;

	public bool lookAt2InvertRightLeft;

	protected float _maxLeft;

	protected JSONStorableFloat maxLeftJSON;

	protected float _maxRight;

	protected JSONStorableFloat maxRightJSON;

	protected float _maxUp;

	protected JSONStorableFloat maxUpJSON;

	protected float _maxDown;

	protected JSONStorableFloat maxDownJSON;

	protected float _minEngageDistance;

	protected JSONStorableFloat minEngageDistanceJSON;

	protected float _lookSpeed;

	protected JSONStorableFloat lookSpeedJSON;

	protected float _leftRightAngleAdjust;

	protected JSONStorableFloat leftRightAngleAdjustJSON;

	protected float _upDownAngleAdjust;

	protected JSONStorableFloat upDownAngleAdjustJSON;

	protected float _depthAdjust;

	protected JSONStorableFloat depthAdjustJSON;

	protected void SyncMaxLeft(float f)
	{
		_maxLeft = f;
		if (lookAt1 != null)
		{
			lookAt1.MaxLeft = _maxLeft;
		}
		if (lookAt2 != null)
		{
			if (lookAt2InvertRightLeft)
			{
				lookAt2.MaxRight = _maxLeft;
			}
			else
			{
				lookAt2.MaxLeft = _maxLeft;
			}
		}
	}

	protected void SyncMaxRight(float f)
	{
		_maxRight = f;
		if (lookAt1 != null)
		{
			lookAt1.MaxRight = _maxRight;
		}
		if (lookAt2 != null)
		{
			if (lookAt2InvertRightLeft)
			{
				lookAt2.MaxLeft = _maxRight;
			}
			else
			{
				lookAt2.MaxRight = _maxRight;
			}
		}
	}

	protected void SyncMaxUp(float f)
	{
		_maxUp = f;
		if (lookAt1 != null)
		{
			lookAt1.MaxUp = _maxUp;
		}
		if (lookAt2 != null)
		{
			lookAt2.MaxUp = _maxUp;
		}
	}

	protected void SyncMaxDown(float f)
	{
		_maxDown = f;
		if (lookAt1 != null)
		{
			lookAt1.MaxDown = _maxDown;
		}
		if (lookAt2 != null)
		{
			lookAt2.MaxDown = _maxDown;
		}
	}

	protected void SyncMinEngageDistance(float f)
	{
		_minEngageDistance = f;
		if (lookAt1 != null)
		{
			lookAt1.MinEngageDistance = _minEngageDistance;
		}
		if (lookAt2 != null)
		{
			lookAt2.MinEngageDistance = _minEngageDistance;
		}
	}

	protected void SyncLookSpeed(float f)
	{
		_lookSpeed = f;
		if (lookAt1 != null)
		{
			lookAt1.smoothFactor = _lookSpeed;
		}
		if (lookAt2 != null)
		{
			lookAt2.smoothFactor = _lookSpeed;
		}
	}

	protected virtual void SyncLookAtLeftRightAngleAdjust()
	{
		if (lookAt1 != null)
		{
			lookAt1.LeftRightAngleAdjust = _leftRightAngleAdjust;
		}
		if (lookAt2 != null)
		{
			lookAt2.LeftRightAngleAdjust = _leftRightAngleAdjust;
		}
	}

	protected void SyncLeftRightAngleAdjust(float f)
	{
		_leftRightAngleAdjust = f;
		SyncLookAtLeftRightAngleAdjust();
	}

	protected void SyncUpDownAngleAdjust(float f)
	{
		_upDownAngleAdjust = f;
		if (lookAt1 != null)
		{
			lookAt1.UpDownAngleAdjust = _upDownAngleAdjust;
		}
		if (lookAt2 != null)
		{
			lookAt2.UpDownAngleAdjust = _upDownAngleAdjust;
		}
	}

	protected void SyncDepthAdjust(float f)
	{
		_depthAdjust = f;
		if (lookAt1 != null)
		{
			lookAt1.DepthAdjust = _depthAdjust;
		}
		if (lookAt2 != null)
		{
			lookAt2.DepthAdjust = _depthAdjust;
		}
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (t != null)
		{
			LookAtWithLimitsControlUI componentInChildren = t.GetComponentInChildren<LookAtWithLimitsControlUI>(true);
			if (componentInChildren != null)
			{
				maxDownJSON.RegisterSlider(componentInChildren.maxDownSlider);
				maxUpJSON.RegisterSlider(componentInChildren.maxUpSlider);
				maxLeftJSON.RegisterSlider(componentInChildren.maxLeftSlider);
				maxRightJSON.RegisterSlider(componentInChildren.maxRightSlider);
				minEngageDistanceJSON.RegisterSlider(componentInChildren.minEngageDistanceSlider);
				lookSpeedJSON.RegisterSlider(componentInChildren.lookSpeedSlider);
				leftRightAngleAdjustJSON.RegisterSlider(componentInChildren.leftRightAngleAdjustSlider);
				upDownAngleAdjustJSON.RegisterSlider(componentInChildren.upDownAngleAdjustSlider);
				depthAdjustJSON.RegisterSlider(componentInChildren.depthAdjustSlider);
			}
		}
	}

	protected virtual void Init()
	{
		LookAtWithLimits lookAtWithLimits = ((!(lookAt1 != null)) ? lookAt2 : lookAt1);
		if (lookAtWithLimits != null)
		{
			_maxDown = lookAtWithLimits.MaxDown;
			_maxUp = lookAtWithLimits.MaxUp;
			_maxLeft = lookAtWithLimits.MaxLeft;
			_maxRight = lookAtWithLimits.MaxRight;
			_minEngageDistance = lookAtWithLimits.MinEngageDistance;
			_lookSpeed = lookAtWithLimits.smoothFactor;
			SyncMaxUp(_maxUp);
			SyncMaxLeft(_maxLeft);
			SyncMaxRight(_maxRight);
			SyncMinEngageDistance(_minEngageDistance);
			SyncLookSpeed(_lookSpeed);
		}
		maxDownJSON = new JSONStorableFloat("maxDown", _maxDown, SyncMaxDown, 0f, 89f);
		maxDownJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(maxDownJSON);
		maxUpJSON = new JSONStorableFloat("maxUp", _maxUp, SyncMaxUp, 0f, 89f);
		maxUpJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(maxUpJSON);
		maxLeftJSON = new JSONStorableFloat("maxLeft", _maxLeft, SyncMaxLeft, 0f, 89f);
		maxLeftJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(maxLeftJSON);
		maxRightJSON = new JSONStorableFloat("maxRight", _maxRight, SyncMaxRight, 0f, 89f);
		maxRightJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(maxRightJSON);
		minEngageDistanceJSON = new JSONStorableFloat("minEngageDistance", _minEngageDistance, SyncMinEngageDistance, 0f, 1f, false);
		minEngageDistanceJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(minEngageDistanceJSON);
		lookSpeedJSON = new JSONStorableFloat("lookSpeed", _lookSpeed, SyncLookSpeed, 0f, 50f);
		lookSpeedJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(lookSpeedJSON);
		leftRightAngleAdjustJSON = new JSONStorableFloat("leftRightAngleAdjust", _leftRightAngleAdjust, SyncLeftRightAngleAdjust, -60f, 60f);
		leftRightAngleAdjustJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(leftRightAngleAdjustJSON);
		upDownAngleAdjustJSON = new JSONStorableFloat("upDownAngleAdjust", _upDownAngleAdjust, SyncUpDownAngleAdjust, -60f, 60f);
		upDownAngleAdjustJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(upDownAngleAdjustJSON);
		depthAdjustJSON = new JSONStorableFloat("depthAdjust", _depthAdjust, SyncDepthAdjust, -0.5f, 0.5f);
		depthAdjustJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(depthAdjustJSON);
	}
}
