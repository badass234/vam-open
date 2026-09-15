using UnityEngine;

public class CycleForceProducerV2 : ForceProducerV2
{
	protected JSONStorableFloat periodJSON;

	[SerializeField]
	protected float _period = 1f;

	protected JSONStorableFloat periodRatioJSON;

	[SerializeField]
	protected float _periodRatio = 0.5f;

	protected JSONStorableFloat forceDurationJSON;

	[SerializeField]
	protected float _forceDuration = 1f;

	protected JSONStorableBool applyForceOnReturnJSON;

	[SerializeField]
	protected bool _applyForceOnReturn = true;

	protected float timer;

	protected float forceTimer;

	protected float flip;

	public float period
	{
		get
		{
			return _period;
		}
		set
		{
			if (periodJSON != null)
			{
				periodJSON.val = value;
			}
			else if (_period != value)
			{
				SyncPeriod(value);
			}
		}
	}

	public float periodRatio
	{
		get
		{
			return _periodRatio;
		}
		set
		{
			if (periodRatioJSON != null)
			{
				periodRatioJSON.val = value;
			}
			else if (_periodRatio != value)
			{
				SyncPeriodRatio(value);
			}
		}
	}

	public float forceDuration
	{
		get
		{
			return _forceDuration;
		}
		set
		{
			if (forceDurationJSON != null)
			{
				forceDurationJSON.val = value;
			}
			else if (_forceDuration != value)
			{
				SyncForceDuration(value);
			}
		}
	}

	public bool applyForceOnReturn
	{
		get
		{
			return _applyForceOnReturn;
		}
		set
		{
			if (applyForceOnReturnJSON != null)
			{
				applyForceOnReturnJSON.val = value;
			}
			else if (_applyForceOnReturn != value)
			{
				SyncApplyForceOnReturn(value);
			}
		}
	}

	protected override void SyncForceFactor(float f)
	{
		base.SyncForceFactor(f);
		maxForce = f;
	}

	protected override void SyncTorqueFactor(float f)
	{
		base.SyncTorqueFactor(f);
		maxTorque = f;
	}

	protected void SyncPeriod(float f)
	{
		_period = f;
	}

	protected void SyncPeriodRatio(float f)
	{
		_periodRatio = f;
	}

	protected void SyncForceDuration(float f)
	{
		_forceDuration = f;
	}

	protected void SyncApplyForceOnReturn(bool b)
	{
		_applyForceOnReturn = b;
	}

	protected override void Init()
	{
		base.Init();
		SyncForceFactor(_forceFactor);
		SyncTorqueFactor(_torqueFactor);
		periodJSON = new JSONStorableFloat("period", _period, SyncPeriod, 0f, 10f, false);
		periodJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(periodJSON);
		periodRatioJSON = new JSONStorableFloat("periodRatio", _periodRatio, SyncPeriodRatio, 0f, 1f);
		periodRatioJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(periodRatioJSON);
		forceDurationJSON = new JSONStorableFloat("forceDuration", _forceDuration, SyncForceDuration, 0f, 1f);
		forceDurationJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(forceDurationJSON);
		applyForceOnReturnJSON = new JSONStorableBool("applyForceOnReturn", _applyForceOnReturn, SyncApplyForceOnReturn);
		applyForceOnReturnJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(applyForceOnReturnJSON);
	}

	public override void InitUI()
	{
		base.InitUI();
		if (UITransform != null)
		{
			CycleForceProducerV2UI componentInChildren = UITransform.GetComponentInChildren<CycleForceProducerV2UI>(true);
			if (componentInChildren != null)
			{
				applyForceOnReturnJSON.toggle = componentInChildren.applyForceOnReturnToggle;
				forceDurationJSON.slider = componentInChildren.forceDurationSlider;
				periodRatioJSON.slider = componentInChildren.periodRatioSlider;
				periodJSON.slider = componentInChildren.periodSlider;
			}
		}
	}

	public override void InitUIAlt()
	{
		base.InitUIAlt();
		if (UITransformAlt != null)
		{
			CycleForceProducerV2UI componentInChildren = UITransformAlt.GetComponentInChildren<CycleForceProducerV2UI>(true);
			if (componentInChildren != null)
			{
				applyForceOnReturnJSON.toggleAlt = componentInChildren.applyForceOnReturnToggle;
				forceDurationJSON.sliderAlt = componentInChildren.forceDurationSlider;
				periodRatioJSON.sliderAlt = componentInChildren.periodRatioSlider;
				periodJSON.sliderAlt = componentInChildren.periodSlider;
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		flip = 1f;
	}

	protected override void Update()
	{
		base.Update();
		timer -= Time.deltaTime;
		forceTimer -= Time.deltaTime;
		if (timer < 0f)
		{
			if ((flip > 0f && _periodRatio != 1f) || _periodRatio == 0f)
			{
				if (_applyForceOnReturn)
				{
					flip = -1f;
				}
				else
				{
					flip = 0f;
				}
				timer = _period * (1f - _periodRatio);
				forceTimer = _forceDuration * _period;
			}
			else
			{
				flip = 1f;
				timer = _period * periodRatio;
				forceTimer = _forceDuration * _period;
			}
			SetTargetForcePercent(flip);
		}
		else if (forceTimer < 0f)
		{
			SetTargetForcePercent(0f);
		}
	}
}
