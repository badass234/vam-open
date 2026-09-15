using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class JSONStorableVector3 : JSONStorableParam
{
	public delegate void SetJSONVector3Callback(JSONStorableVector3 jf);

	public delegate void SetVector3Callback(Vector3 v);

	protected bool _interactable;

	protected Vector3 _defaultVal;

	protected Vector3 _val;

	protected bool _constrained;

	protected Vector3 _min;

	protected Vector3 _max;

	public SetVector3Callback setCallbackFunction;

	public SetJSONVector3Callback setJSONCallbackFunction;

	protected SliderControl _sliderXControl;

	protected Slider _sliderX;

	protected SliderControl _sliderXControlAlt;

	protected Slider _sliderXAlt;

	protected SliderControl _sliderYControl;

	protected Slider _sliderY;

	protected SliderControl _sliderYControlAlt;

	protected Slider _sliderYAlt;

	protected SliderControl _sliderZControl;

	protected Slider _sliderZ;

	protected SliderControl _sliderZControlAlt;

	protected Slider _sliderZAlt;

	public Vector3 defaultVal
	{
		get
		{
			return _defaultVal;
		}
		set
		{
			if (!(_defaultVal != value))
			{
				return;
			}
			_defaultVal = value;
			if (sliderX != null)
			{
				SliderControl component = sliderX.GetComponent<SliderControl>();
				if (component != null)
				{
					component.defaultValue = _defaultVal.x;
				}
			}
			if (sliderXAlt != null)
			{
				SliderControl component2 = sliderXAlt.GetComponent<SliderControl>();
				if (component2 != null)
				{
					component2.defaultValue = _defaultVal.x;
				}
			}
			if (sliderY != null)
			{
				SliderControl component3 = sliderY.GetComponent<SliderControl>();
				if (component3 != null)
				{
					component3.defaultValue = _defaultVal.y;
				}
			}
			if (sliderYAlt != null)
			{
				SliderControl component4 = sliderYAlt.GetComponent<SliderControl>();
				if (component4 != null)
				{
					component4.defaultValue = _defaultVal.y;
				}
			}
			if (sliderZ != null)
			{
				SliderControl component5 = sliderZ.GetComponent<SliderControl>();
				if (component5 != null)
				{
					component5.defaultValue = _defaultVal.z;
				}
			}
			if (sliderZAlt != null)
			{
				SliderControl component6 = sliderZAlt.GetComponent<SliderControl>();
				if (component6 != null)
				{
					component6.defaultValue = _defaultVal.z;
				}
			}
		}
	}

	public virtual Vector3 val
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

	public virtual Vector3 valNoCallback
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
			if (_constrained != value)
			{
				_constrained = value;
			}
		}
	}

	public Vector3 min
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
				if (_sliderX != null)
				{
					_sliderX.minValue = _min.x;
				}
				if (_sliderXAlt != null)
				{
					_sliderXAlt.minValue = _min.x;
				}
				if (_sliderY != null)
				{
					_sliderY.minValue = _min.y;
				}
				if (_sliderYAlt != null)
				{
					_sliderYAlt.minValue = _min.y;
				}
				if (_sliderZ != null)
				{
					_sliderZ.minValue = _min.z;
				}
				if (_sliderZAlt != null)
				{
					_sliderZAlt.minValue = _min.z;
				}
			}
		}
	}

	public Vector3 max
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
				if (_sliderX != null)
				{
					_sliderX.maxValue = _max.x;
				}
				if (_sliderXAlt != null)
				{
					_sliderXAlt.maxValue = _max.x;
				}
				if (_sliderY != null)
				{
					_sliderY.maxValue = _max.y;
				}
				if (_sliderYAlt != null)
				{
					_sliderYAlt.maxValue = _max.y;
				}
				if (_sliderZ != null)
				{
					_sliderZ.maxValue = _max.z;
				}
				if (_sliderZAlt != null)
				{
					_sliderZAlt.maxValue = _max.z;
				}
			}
		}
	}

	public Slider sliderX
	{
		get
		{
			return _sliderX;
		}
		set
		{
			if (!(_sliderX != value))
			{
				return;
			}
			if (_sliderX != null)
			{
				_sliderX.interactable = true;
				_sliderX.onValueChanged.RemoveListener(SetXVal);
			}
			_sliderX = value;
			_sliderXControl = null;
			if (_sliderX != null)
			{
				_sliderX.interactable = _interactable;
				_sliderXControl = _sliderX.GetComponent<SliderControl>();
				if (_sliderXControl != null)
				{
					_sliderXControl.defaultValue = _defaultVal.x;
				}
				_sliderX.minValue = _min.x;
				_sliderX.maxValue = _max.x;
				_sliderX.value = _val.x;
				_sliderX.onValueChanged.AddListener(SetXVal);
			}
		}
	}

	public Slider sliderXAlt
	{
		get
		{
			return _sliderXAlt;
		}
		set
		{
			if (!(_sliderXAlt != value))
			{
				return;
			}
			if (_sliderXAlt != null)
			{
				_sliderXAlt.interactable = true;
				_sliderXAlt.onValueChanged.RemoveListener(SetXVal);
			}
			_sliderXAlt = value;
			_sliderXControlAlt = null;
			if (_sliderXAlt != null)
			{
				_sliderXAlt.interactable = _interactable;
				_sliderXControlAlt = _sliderXAlt.GetComponent<SliderControl>();
				if (_sliderXControlAlt != null)
				{
					_sliderXControlAlt.defaultValue = _defaultVal.x;
				}
				_sliderXAlt.minValue = _min.x;
				_sliderXAlt.maxValue = _max.x;
				_sliderXAlt.value = _val.x;
				_sliderXAlt.onValueChanged.AddListener(SetXVal);
			}
		}
	}

	public Slider sliderY
	{
		get
		{
			return _sliderY;
		}
		set
		{
			if (!(_sliderY != value))
			{
				return;
			}
			if (_sliderY != null)
			{
				_sliderY.interactable = true;
				_sliderY.onValueChanged.RemoveListener(SetXVal);
			}
			_sliderY = value;
			_sliderYControl = null;
			if (_sliderY != null)
			{
				_sliderY.interactable = _interactable;
				_sliderYControl = _sliderY.GetComponent<SliderControl>();
				if (_sliderYControl != null)
				{
					_sliderYControl.defaultValue = _defaultVal.y;
				}
				_sliderY.minValue = _min.y;
				_sliderY.maxValue = _max.y;
				_sliderY.value = _val.y;
				_sliderY.onValueChanged.AddListener(SetXVal);
			}
		}
	}

	public Slider sliderYAlt
	{
		get
		{
			return _sliderYAlt;
		}
		set
		{
			if (!(_sliderYAlt != value))
			{
				return;
			}
			if (_sliderYAlt != null)
			{
				_sliderYAlt.interactable = true;
				_sliderYAlt.onValueChanged.RemoveListener(SetXVal);
			}
			_sliderYAlt = value;
			_sliderYControlAlt = null;
			if (_sliderYAlt != null)
			{
				_sliderYAlt.interactable = _interactable;
				_sliderYControlAlt = _sliderYAlt.GetComponent<SliderControl>();
				if (_sliderYControlAlt != null)
				{
					_sliderYControlAlt.defaultValue = _defaultVal.y;
				}
				_sliderYAlt.minValue = _min.y;
				_sliderYAlt.maxValue = _max.y;
				_sliderYAlt.value = _val.y;
				_sliderYAlt.onValueChanged.AddListener(SetXVal);
			}
		}
	}

	public Slider sliderZ
	{
		get
		{
			return _sliderZ;
		}
		set
		{
			if (!(_sliderZ != value))
			{
				return;
			}
			if (_sliderZ != null)
			{
				_sliderZ.interactable = true;
				_sliderZ.onValueChanged.RemoveListener(SetXVal);
			}
			_sliderZ = value;
			_sliderZControl = null;
			if (_sliderZ != null)
			{
				_sliderZ.interactable = _interactable;
				_sliderZControl = _sliderZ.GetComponent<SliderControl>();
				if (_sliderZControl != null)
				{
					_sliderZControl.defaultValue = _defaultVal.z;
				}
				_sliderZ.minValue = _min.z;
				_sliderZ.maxValue = _max.z;
				_sliderZ.value = _val.z;
				_sliderZ.onValueChanged.AddListener(SetXVal);
			}
		}
	}

	public Slider sliderZAlt
	{
		get
		{
			return _sliderZAlt;
		}
		set
		{
			if (!(_sliderZAlt != value))
			{
				return;
			}
			if (_sliderZAlt != null)
			{
				_sliderZAlt.interactable = true;
				_sliderZAlt.onValueChanged.RemoveListener(SetXVal);
			}
			_sliderZAlt = value;
			_sliderZControlAlt = null;
			if (_sliderZAlt != null)
			{
				_sliderZAlt.interactable = _interactable;
				_sliderZControlAlt = _sliderZAlt.GetComponent<SliderControl>();
				if (_sliderZControlAlt != null)
				{
					_sliderZControlAlt.defaultValue = _defaultVal.z;
				}
				_sliderZAlt.minValue = _min.z;
				_sliderZAlt.maxValue = _max.z;
				_sliderZAlt.value = _val.z;
				_sliderZAlt.onValueChanged.AddListener(SetXVal);
			}
		}
	}

	public JSONStorableVector3(string paramName, Vector3 startingValue, Vector3 minValue, Vector3 maxValue, bool constrain = true, bool interactable = true)
	{
		type = JSONStorable.Type.Vector3;
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

	public JSONStorableVector3(string paramName, Vector3 startingValue, SetVector3Callback callback, Vector3 minValue, Vector3 maxValue, bool constrain = true, bool interactable = true)
	{
		type = JSONStorable.Type.Vector3;
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

	public JSONStorableVector3(string paramName, Vector3 startingValue, SetJSONVector3Callback callback, Vector3 minValue, Vector3 maxValue, bool constrain = true, bool interactable = true)
	{
		type = JSONStorable.Type.Vector3;
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
			jc[name][120].AsFloat = _val.x;
			jc[name][121].AsFloat = _val.y;
			jc[name][122].AsFloat = _val.z;
		}
		return flag;
	}

	public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
	{
		if (!NeedsRestore(jc, restorePhysical, restoreAppearance))
		{
			return;
		}
		if (jc[name] != null)
		{
			Vector3 vector = default(Vector3);
			vector.x = 0f;
			vector.y = 0f;
			vector.z = 0f;
			if (jc[name][120] != null)
			{
				vector.x = jc[name][120].AsFloat;
			}
			if (jc[name][121] != null)
			{
				vector.y = jc[name][121].AsFloat;
			}
			if (jc[name][122] != null)
			{
				vector.z = jc[name][122].AsFloat;
			}
			val = vector;
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
		if (jc[name] != null)
		{
			Vector3 vector = default(Vector3);
			vector.x = 0f;
			vector.y = 0f;
			vector.z = 0f;
			if (jc[name][120] != null)
			{
				vector.x = jc[name][120].AsFloat;
			}
			if (jc[name][121] != null)
			{
				vector.y = jc[name][121].AsFloat;
			}
			if (jc[name][122] != null)
			{
				vector.z = jc[name][122].AsFloat;
			}
			val = vector;
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

	protected void InternalSetVal(Vector3 v, bool doCallback = true)
	{
		Vector3 vector = v;
		if (_constrained)
		{
			vector.x = Mathf.Clamp(vector.x, _min.x, _max.x);
			vector.y = Mathf.Clamp(vector.y, _min.x, _max.x);
			vector.z = Mathf.Clamp(vector.z, _min.x, _max.x);
		}
		if (!(_val != vector))
		{
			return;
		}
		_val = vector;
		bool flag = false;
		Vector3 vector2 = min;
		if (min.x > _val.x)
		{
			vector2.x = _val.x;
			flag = true;
			if (_sliderX != null && vector2.x > _sliderX.minValue)
			{
				vector2.x = _sliderX.minValue;
			}
			if (_sliderXAlt != null && vector2.x > _sliderXAlt.minValue)
			{
				vector2.x = _sliderXAlt.minValue;
			}
		}
		if (min.y > _val.y)
		{
			vector2.y = _val.y;
			flag = true;
			if (_sliderY != null && vector2.y > _sliderY.minValue)
			{
				vector2.y = _sliderY.minValue;
			}
			if (_sliderYAlt != null && vector2.y > _sliderYAlt.minValue)
			{
				vector2.y = _sliderYAlt.minValue;
			}
		}
		if (min.z > _val.z)
		{
			vector2.z = _val.z;
			flag = true;
			if (_sliderZ != null && vector2.z > _sliderZ.minValue)
			{
				vector2.z = _sliderZ.minValue;
			}
			if (_sliderZAlt != null && vector2.z > _sliderZAlt.minValue)
			{
				vector2.z = _sliderZAlt.minValue;
			}
		}
		if (flag)
		{
			min = vector2;
		}
		bool flag2 = false;
		Vector3 vector3 = max;
		if (max.x < _val.x)
		{
			vector3.x = _val.x;
			flag2 = true;
			if (_sliderX != null && vector3.x < _sliderX.maxValue)
			{
				vector3.x = _sliderX.maxValue;
			}
			if (_sliderXAlt != null && vector3.x < _sliderXAlt.maxValue)
			{
				vector3.x = _sliderXAlt.maxValue;
			}
		}
		if (max.y < _val.y)
		{
			vector3.y = _val.y;
			flag2 = true;
			if (_sliderY != null && vector3.y < _sliderY.maxValue)
			{
				vector3.y = _sliderY.maxValue;
			}
			if (_sliderYAlt != null && vector3.y < _sliderYAlt.maxValue)
			{
				vector3.y = _sliderYAlt.maxValue;
			}
		}
		if (max.z < _val.z)
		{
			vector3.z = _val.z;
			flag2 = true;
			if (_sliderZ != null && vector3.z < _sliderZ.maxValue)
			{
				vector3.z = _sliderZ.maxValue;
			}
			if (_sliderZAlt != null && vector3.z < _sliderZAlt.maxValue)
			{
				vector3.z = _sliderZAlt.maxValue;
			}
		}
		if (flag2)
		{
			max = vector3;
		}
		if (_sliderX != null)
		{
			_sliderX.value = _val.x;
		}
		if (_sliderXAlt != null)
		{
			_sliderXAlt.value = _val.x;
		}
		if (_sliderY != null)
		{
			_sliderY.value = _val.y;
		}
		if (_sliderYAlt != null)
		{
			_sliderYAlt.value = _val.y;
		}
		if (_sliderZ != null)
		{
			_sliderZ.value = _val.z;
		}
		if (_sliderZAlt != null)
		{
			_sliderZAlt.value = _val.z;
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

	public void SetVal(Vector3 value)
	{
		val = value;
	}

	public void SetXVal(float f)
	{
		Vector3 vector = _val;
		vector.x = f;
		val = vector;
	}

	public void SetYVal(float f)
	{
		Vector3 vector = _val;
		vector.y = f;
		val = vector;
	}

	public void SetZVal(float f)
	{
		Vector3 vector = _val;
		vector.z = f;
		val = vector;
	}

	public void RegisterSliderX(Slider s, bool isAlt = false)
	{
		if (isAlt)
		{
			sliderXAlt = s;
		}
		else
		{
			sliderX = s;
		}
	}

	public void RegisterSliderY(Slider s, bool isAlt = false)
	{
		if (isAlt)
		{
			sliderYAlt = s;
		}
		else
		{
			sliderY = s;
		}
	}

	public void RegisterSliderZ(Slider s, bool isAlt = false)
	{
		if (isAlt)
		{
			sliderZAlt = s;
		}
		else
		{
			sliderZ = s;
		}
	}
}
