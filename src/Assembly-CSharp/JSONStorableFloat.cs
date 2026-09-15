using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class JSONStorableFloat : JSONStorableParam
{
	public delegate void SetJSONFloatCallback(JSONStorableFloat jf);

	public delegate void SetFloatCallback(float val);

	protected bool _interactable;

	protected float _defaultVal;

	protected float _val;

	protected bool _constrained;

	protected float _min;

	protected float _max;

	public SetFloatCallback setCallbackFunction;

	public SetJSONFloatCallback setJSONCallbackFunction;

	protected SliderControl _sliderControl;

	protected Slider _slider;

	protected SliderControl _sliderControlAlt;

	protected Slider _sliderAlt;

	public float defaultVal
	{
		get
		{
			return _defaultVal;
		}
		set
		{
			if (_defaultVal == value)
			{
				return;
			}
			_defaultVal = value;
			if (slider != null)
			{
				SliderControl component = slider.GetComponent<SliderControl>();
				if (component != null)
				{
					component.defaultValue = _defaultVal;
				}
			}
			if (sliderAlt != null)
			{
				SliderControl component2 = sliderAlt.GetComponent<SliderControl>();
				if (component2 != null)
				{
					component2.defaultValue = _defaultVal;
				}
			}
		}
	}

	public virtual float val
	{
		get
		{
			return _val;
		}
		set
		{
			InternalSetVal(value);
		}
	}

	public virtual float valNoCallback
	{
		get
		{
			return _val;
		}
		set
		{
			InternalSetVal(value, false);
		}
	}

	public bool constrained
	{
		get
		{
			return _constrained;
		}
		set
		{
			if (_constrained == value)
			{
				return;
			}
			_constrained = value;
			if (_slider != null)
			{
				SliderControl component = _slider.GetComponent<SliderControl>();
				if (component != null)
				{
					component.clamp = _constrained;
				}
			}
			if (_sliderAlt != null)
			{
				SliderControl component2 = _sliderAlt.GetComponent<SliderControl>();
				if (component2 != null)
				{
					component2.clamp = _constrained;
				}
			}
		}
	}

	public float min
	{
		get
		{
			return _min;
		}
		set
		{
			if (_min != value)
			{
				_min = value;
				if (_slider != null)
				{
					_slider.minValue = _min;
				}
				if (_sliderAlt != null)
				{
					_sliderAlt.minValue = _min;
				}
			}
		}
	}

	public float max
	{
		get
		{
			return _max;
		}
		set
		{
			if (_max != value)
			{
				_max = value;
				if (_slider != null)
				{
					_slider.maxValue = _max;
				}
				if (_sliderAlt != null)
				{
					_sliderAlt.maxValue = _max;
				}
			}
		}
	}

	public Slider slider
	{
		get
		{
			return _slider;
		}
		set
		{
			if (!(_slider != value))
			{
				return;
			}
			if (_slider != null)
			{
				_slider.interactable = true;
				_slider.onValueChanged.RemoveListener(SetVal);
			}
			_slider = value;
			_sliderControl = null;
			if (_slider != null)
			{
				_slider.interactable = _interactable;
				_sliderControl = _slider.GetComponent<SliderControl>();
				if (_sliderControl != null)
				{
					_sliderControl.defaultValue = _defaultVal;
					_sliderControl.clamp = _constrained;
				}
				_slider.minValue = _min;
				_slider.maxValue = _max;
				_slider.value = _val;
				_slider.onValueChanged.AddListener(SetVal);
			}
		}
	}

	public Slider sliderAlt
	{
		get
		{
			return _sliderAlt;
		}
		set
		{
			if (!(_sliderAlt != value))
			{
				return;
			}
			if (_sliderAlt != null)
			{
				_sliderAlt.interactable = true;
				_sliderAlt.onValueChanged.RemoveListener(SetVal);
			}
			_sliderAlt = value;
			_sliderControlAlt = null;
			if (_sliderAlt != null)
			{
				_sliderAlt.interactable = _interactable;
				_sliderControlAlt = _sliderAlt.GetComponent<SliderControl>();
				if (_sliderControlAlt != null)
				{
					_sliderControlAlt.defaultValue = _defaultVal;
					_sliderControlAlt.clamp = _constrained;
				}
				_sliderAlt.minValue = _min;
				_sliderAlt.maxValue = _max;
				_sliderAlt.value = _val;
				_sliderAlt.onValueChanged.AddListener(SetVal);
			}
		}
	}

	public JSONStorableFloat(string paramName, float startingValue, float minValue, float maxValue, bool constrain = true, bool interactable = true)
	{
		type = JSONStorable.Type.Float;
		name = paramName;
		min = minValue;
		max = maxValue;
		defaultVal = startingValue;
		val = startingValue;
		setCallbackFunction = null;
		setJSONCallbackFunction = null;
		constrained = constrain;
		_interactable = interactable;
	}

	public JSONStorableFloat(string paramName, float startingValue, SetFloatCallback callback, float minValue, float maxValue, bool constrain = true, bool interactable = true)
	{
		type = JSONStorable.Type.Float;
		name = paramName;
		min = minValue;
		max = maxValue;
		defaultVal = startingValue;
		val = startingValue;
		setCallbackFunction = callback;
		setJSONCallbackFunction = null;
		constrained = constrain;
		_interactable = interactable;
	}

	public JSONStorableFloat(string paramName, float startingValue, SetJSONFloatCallback callback, float minValue, float maxValue, bool constrain = true, bool interactable = true)
	{
		type = JSONStorable.Type.Float;
		name = paramName;
		min = minValue;
		max = maxValue;
		defaultVal = startingValue;
		val = startingValue;
		setCallbackFunction = null;
		setJSONCallbackFunction = callback;
		constrained = constrain;
		_interactable = interactable;
	}

	public override bool StoreJSON(JSONClass jc, bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		bool flag = NeedsStore(jc, includePhysical, includeAppearance) && (forceStore || _val != _defaultVal);
		if (flag)
		{
			if (name.IndexOf(':') != -1)
			{
				string[] array = name.Split(':');
				jc[array[0]][array[1]].AsFloat = _val;
			}
			else
			{
				jc[name].AsFloat = _val;
			}
		}
		return flag;
	}

	public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
	{
		if (!NeedsRestore(jc, restorePhysical, restoreAppearance))
		{
			return;
		}
		if (name.IndexOf(':') != -1)
		{
			string[] array = name.Split(':');
			if (jc[array[0]][array[1]] != null)
			{
				val = jc[array[0]][array[1]].AsFloat;
			}
			else if (setMissingToDefault)
			{
				val = _defaultVal;
			}
		}
		else if (jc[name] != null)
		{
			if (jc[name].Value == string.Empty)
			{
				val = _defaultVal;
			}
			else
			{
				val = jc[name].AsFloat;
			}
		}
		else if (setMissingToDefault)
		{
			val = _defaultVal;
		}
	}

	public override void LateRestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
	{
		if (!NeedsLateRestore(jc, restorePhysical, restoreAppearance))
		{
			return;
		}
		if (name.IndexOf(':') != -1)
		{
			string[] array = name.Split(':');
			if (jc[array[0]][array[1]] != null)
			{
				val = jc[array[0]][array[1]].AsFloat;
			}
			else if (setMissingToDefault)
			{
				val = _defaultVal;
			}
		}
		else if (jc[name] != null)
		{
			if (jc[name].Value == string.Empty)
			{
				val = _defaultVal;
			}
			else
			{
				val = jc[name].AsFloat;
			}
		}
		else if (setMissingToDefault)
		{
			val = _defaultVal;
		}
	}

	public override void SetDefaultFromCurrent()
	{
		defaultVal = val;
	}

	public override void SetValToDefault()
	{
		val = defaultVal;
	}

	public void SetInteractble(bool b)
	{
		_interactable = b;
		if (slider != null)
		{
			slider.interactable = _interactable;
		}
		if (sliderAlt != null)
		{
			sliderAlt.interactable = _interactable;
		}
	}

	protected void InternalSetVal(float f, bool doCallback = true)
	{
		float num = f;
		if (_constrained)
		{
			num = Mathf.Clamp(num, _min, _max);
		}
		if (_val == num)
		{
			return;
		}
		_val = num;
		if (min > _val)
		{
			float minValue = _val;
			if (_slider != null && minValue > _slider.minValue)
			{
				minValue = _slider.minValue;
			}
			if (_sliderAlt != null && minValue > _sliderAlt.minValue)
			{
				minValue = _sliderAlt.minValue;
			}
			min = minValue;
		}
		if (max < _val)
		{
			float maxValue = _val;
			if (_slider != null && maxValue < _slider.maxValue)
			{
				maxValue = _slider.maxValue;
			}
			if (_sliderAlt != null && maxValue < _sliderAlt.maxValue)
			{
				maxValue = _sliderAlt.maxValue;
			}
			max = maxValue;
		}
		if (_slider != null)
		{
			_slider.value = _val;
		}
		if (_sliderAlt != null)
		{
			_sliderAlt.value = _val;
		}
		if (doCallback)
		{
			if (setCallbackFunction != null)
			{
				setCallbackFunction(_val);
			}
			if (setJSONCallbackFunction != null)
			{
				setJSONCallbackFunction(this);
			}
		}
	}

	public void SetVal(float value)
	{
		val = value;
	}

	public void RegisterSlider(Slider s, bool isAlt = false)
	{
		if (isAlt)
		{
			sliderAlt = s;
		}
		else
		{
			slider = s;
		}
	}
}
