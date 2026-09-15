using UnityEngine;

public class DAZSkinWrapControl : JSONStorable
{
	[SerializeField]
	protected DAZSkinWrap _wrap;

	[SerializeField]
	protected DAZSkinWrap _wrap2;

	protected JSONStorableFloat surfaceOffsetJSON;

	protected JSONStorableFloat additionalThicknessMultiplierJSON;

	protected JSONStorableBool wrapToSmoothedVertsJSON;

	protected JSONStorableFloat smoothIterationsJSON;

	public DAZSkinWrap wrap
	{
		get
		{
			return _wrap;
		}
		set
		{
			if (_wrap != value)
			{
				_wrap = value;
				if (surfaceOffsetJSON != null)
				{
					surfaceOffsetJSON.val = _wrap.surfaceOffset;
					surfaceOffsetJSON.defaultVal = _wrap.defaultSurfaceOffset;
				}
				if (additionalThicknessMultiplierJSON != null)
				{
					additionalThicknessMultiplierJSON.val = _wrap.additionalThicknessMultiplier;
					additionalThicknessMultiplierJSON.defaultVal = _wrap.defaultAdditionalThicknessMultiplier;
				}
				if (wrapToSmoothedVertsJSON != null)
				{
					wrapToSmoothedVertsJSON.val = !_wrap.forceRawSkinVerts;
					wrapToSmoothedVertsJSON.defaultVal = !_wrap.forceRawSkinVerts;
				}
				if (smoothIterationsJSON != null)
				{
					smoothIterationsJSON.val = _wrap.smoothOuterLoops;
					smoothIterationsJSON.defaultVal = _wrap.smoothOuterLoops;
				}
			}
		}
	}

	public DAZSkinWrap wrap2
	{
		get
		{
			return _wrap2;
		}
		set
		{
			if (_wrap2 != value)
			{
				_wrap2 = value;
				if (surfaceOffsetJSON != null)
				{
					_wrap2.surfaceOffset = surfaceOffsetJSON.val;
				}
				if (additionalThicknessMultiplierJSON != null)
				{
					_wrap2.additionalThicknessMultiplier = additionalThicknessMultiplierJSON.val;
				}
				if (wrapToSmoothedVertsJSON != null)
				{
					_wrap2.forceRawSkinVerts = !wrapToSmoothedVertsJSON.val;
				}
				if (smoothIterationsJSON != null)
				{
					_wrap2.smoothOuterLoops = Mathf.FloorToInt(smoothIterationsJSON.val);
				}
			}
		}
	}

	protected void SyncSurfaceOffset(float f)
	{
		if (_wrap != null)
		{
			_wrap.surfaceOffset = f;
		}
		if (_wrap2 != null)
		{
			_wrap2.surfaceOffset = f;
		}
	}

	protected void SyncAdditionalThicknessMultiplier(float f)
	{
		if (_wrap != null)
		{
			_wrap.additionalThicknessMultiplier = f;
		}
		if (_wrap2 != null)
		{
			_wrap2.additionalThicknessMultiplier = f;
		}
	}

	protected void SyncWrapToSmoothedVerts(bool b)
	{
		if (_wrap != null)
		{
			_wrap.forceRawSkinVerts = !b;
		}
		if (_wrap2 != null)
		{
			_wrap2.forceRawSkinVerts = !b;
		}
	}

	protected void SyncSmoothIterations(float f)
	{
		int smoothOuterLoops = Mathf.FloorToInt(f);
		if (_wrap != null)
		{
			_wrap.smoothOuterLoops = smoothOuterLoops;
		}
		if (_wrap2 != null)
		{
			_wrap2.smoothOuterLoops = smoothOuterLoops;
		}
	}

	protected void Init()
	{
		if (_wrap == null)
		{
			DAZSkinWrap[] components = GetComponents<DAZSkinWrap>();
			DAZSkinWrap[] array = components;
			foreach (DAZSkinWrap dAZSkinWrap in array)
			{
				if (dAZSkinWrap.enabled && dAZSkinWrap.draw)
				{
					_wrap = dAZSkinWrap;
					break;
				}
			}
		}
		float startingValue = 0f;
		float startingValue2 = 0f;
		bool startingValue3 = false;
		float startingValue4 = 0f;
		if (_wrap != null)
		{
			startingValue = _wrap.surfaceOffset;
			startingValue2 = _wrap.additionalThicknessMultiplier;
			startingValue3 = !_wrap.forceRawSkinVerts;
			startingValue4 = _wrap.smoothOuterLoops;
		}
		surfaceOffsetJSON = new JSONStorableFloat("surfaceOffset", startingValue, SyncSurfaceOffset, -0.01f, 0.01f, false);
		RegisterFloat(surfaceOffsetJSON);
		additionalThicknessMultiplierJSON = new JSONStorableFloat("additionalThicknessMultiplier", startingValue2, SyncAdditionalThicknessMultiplier, -0.1f, 0.1f, false);
		RegisterFloat(additionalThicknessMultiplierJSON);
		wrapToSmoothedVertsJSON = new JSONStorableBool("wrapToSmoothedVerts", startingValue3, SyncWrapToSmoothedVerts);
		RegisterBool(wrapToSmoothedVertsJSON);
		smoothIterationsJSON = new JSONStorableFloat("smoothIterations", startingValue4, SyncSmoothIterations, 0f, 3f);
		RegisterFloat(smoothIterationsJSON);
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (!(t != null))
		{
			return;
		}
		DAZSkinWrapControlUI componentInChildren = t.GetComponentInChildren<DAZSkinWrapControlUI>(true);
		if (componentInChildren != null)
		{
			if (surfaceOffsetJSON != null)
			{
				surfaceOffsetJSON.RegisterSlider(componentInChildren.surfaceOffsetSlider, isAlt);
			}
			if (additionalThicknessMultiplierJSON != null)
			{
				additionalThicknessMultiplierJSON.RegisterSlider(componentInChildren.additionalThicknessMultiplierSlider, isAlt);
			}
			if (wrapToSmoothedVertsJSON != null)
			{
				wrapToSmoothedVertsJSON.RegisterToggle(componentInChildren.wrapToSmoothedVertsToggle, isAlt);
			}
			if (smoothIterationsJSON != null)
			{
				smoothIterationsJSON.RegisterSlider(componentInChildren.smoothIterationsSlider, isAlt);
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
