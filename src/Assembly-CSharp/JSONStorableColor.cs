using System;
using SimpleJSON;
using UnityEngine;

public class JSONStorableColor : JSONStorableParam
{
	public delegate void SetHSVColorCallback(float h, float s, float v);

	public delegate void SetJSONColorCallback(JSONStorableColor jc);

	protected HSVColor _defaultVal;

	protected HSVColor _val;

	public SetHSVColorCallback setCallbackFunction;

	public SetJSONColorCallback setJSONCallbackFunction;

	protected HSVColorPicker _colorPicker;

	protected HSVColorPicker _colorPickerAlt;

	public HSVColor defaultVal
	{
		get
		{
			return _defaultVal;
		}
		set
		{
			HSVColor hSVColor = value;
			if (_defaultVal.H != hSVColor.H || _defaultVal.S != hSVColor.S || _defaultVal.V != hSVColor.V)
			{
				_defaultVal.H = hSVColor.H;
				_defaultVal.S = hSVColor.S;
				_defaultVal.V = hSVColor.V;
				if (_colorPicker != null)
				{
					_colorPicker.defaultHue = _defaultVal.H;
					_colorPicker.defaultSaturation = _defaultVal.S;
					_colorPicker.defaultCvalue = _defaultVal.V;
				}
				if (_colorPickerAlt != null)
				{
					_colorPickerAlt.defaultHue = _defaultVal.H;
					_colorPickerAlt.defaultSaturation = _defaultVal.S;
					_colorPickerAlt.defaultCvalue = _defaultVal.V;
				}
			}
		}
	}

	public HSVColor val
	{
		get
		{
			return _val;
		}
		set
		{
			InternalSetVal(value.H, value.S, value.V, true);
		}
	}

	public HSVColor valNoCallback
	{
		get
		{
			return _val;
		}
		set
		{
			InternalSetVal(value.H, value.S, value.V, false);
		}
	}

	public HSVColorPicker colorPicker
	{
		get
		{
			return _colorPicker;
		}
		set
		{
			if (_colorPicker != value)
			{
				if (_colorPicker != null)
				{
					HSVColorPicker hSVColorPicker = _colorPicker;
					hSVColorPicker.onHSVColorChangedHandlers = (HSVColorPicker.OnHSVColorChanged)Delegate.Remove(hSVColorPicker.onHSVColorChangedHandlers, new HSVColorPicker.OnHSVColorChanged(SetVal));
				}
				_colorPicker = value;
				if (_colorPicker != null)
				{
					_colorPicker.hue = _val.H;
					_colorPicker.saturation = _val.S;
					_colorPicker.cvalue = _val.V;
					_colorPicker.defaultHue = _defaultVal.H;
					_colorPicker.defaultSaturation = _defaultVal.S;
					_colorPicker.defaultCvalue = _defaultVal.V;
					HSVColorPicker hSVColorPicker2 = _colorPicker;
					hSVColorPicker2.onHSVColorChangedHandlers = (HSVColorPicker.OnHSVColorChanged)Delegate.Combine(hSVColorPicker2.onHSVColorChangedHandlers, new HSVColorPicker.OnHSVColorChanged(SetVal));
				}
			}
		}
	}

	public HSVColorPicker colorPickerAlt
	{
		get
		{
			return _colorPickerAlt;
		}
		set
		{
			if (_colorPickerAlt != value)
			{
				if (_colorPickerAlt != null)
				{
					HSVColorPicker hSVColorPicker = _colorPickerAlt;
					hSVColorPicker.onHSVColorChangedHandlers = (HSVColorPicker.OnHSVColorChanged)Delegate.Remove(hSVColorPicker.onHSVColorChangedHandlers, new HSVColorPicker.OnHSVColorChanged(SetVal));
				}
				_colorPickerAlt = value;
				if (_colorPickerAlt != null)
				{
					_colorPickerAlt.hue = _val.H;
					_colorPickerAlt.saturation = _val.S;
					_colorPickerAlt.cvalue = _val.V;
					_colorPickerAlt.defaultHue = _defaultVal.H;
					_colorPickerAlt.defaultSaturation = _defaultVal.S;
					_colorPickerAlt.defaultCvalue = _defaultVal.V;
					HSVColorPicker hSVColorPicker2 = _colorPickerAlt;
					hSVColorPicker2.onHSVColorChangedHandlers = (HSVColorPicker.OnHSVColorChanged)Delegate.Combine(hSVColorPicker2.onHSVColorChangedHandlers, new HSVColorPicker.OnHSVColorChanged(SetVal));
				}
			}
		}
	}

	public JSONStorableColor(string paramName, HSVColor startingColor)
	{
		type = JSONStorable.Type.Color;
		name = paramName;
		_defaultVal = default(HSVColor);
		_defaultVal.H = startingColor.H;
		_defaultVal.S = startingColor.S;
		_defaultVal.V = startingColor.V;
		_val = default(HSVColor);
		_val.H = startingColor.H;
		_val.S = startingColor.S;
		_val.V = startingColor.V;
		setCallbackFunction = null;
		setJSONCallbackFunction = null;
	}

	public JSONStorableColor(string paramName, HSVColor startingColor, SetHSVColorCallback callback)
	{
		type = JSONStorable.Type.Color;
		name = paramName;
		_defaultVal = default(HSVColor);
		_defaultVal.H = startingColor.H;
		_defaultVal.S = startingColor.S;
		_defaultVal.V = startingColor.V;
		_val = default(HSVColor);
		_val.H = startingColor.H;
		_val.S = startingColor.S;
		_val.V = startingColor.V;
		setCallbackFunction = callback;
		setJSONCallbackFunction = null;
	}

	public JSONStorableColor(string paramName, HSVColor startingColor, SetJSONColorCallback callback)
	{
		type = JSONStorable.Type.Color;
		name = paramName;
		_defaultVal = default(HSVColor);
		_defaultVal.H = startingColor.H;
		_defaultVal.S = startingColor.S;
		_defaultVal.V = startingColor.V;
		_val = default(HSVColor);
		_val.H = startingColor.H;
		_val.S = startingColor.S;
		_val.V = startingColor.V;
		setCallbackFunction = null;
		setJSONCallbackFunction = callback;
	}

	public override bool StoreJSON(JSONClass jc, bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		bool flag = NeedsStore(jc, includePhysical, includeAppearance) && (forceStore || _val.H != _defaultVal.H || _val.S != _defaultVal.S || _val.V != _defaultVal.V);
		if (flag)
		{
			jc[name]["h"].AsFloat = _val.H;
			jc[name]["s"].AsFloat = _val.S;
			jc[name]["v"].AsFloat = _val.V;
		}
		return flag;
	}

	protected void Restore(JSONClass jc, bool setMissingToDefault)
	{
		float h = _val.H;
		float s = _val.S;
		float v = _val.V;
		if (jc[name] != null)
		{
			if (jc[name]["h"] != null)
			{
				h = jc[name]["h"].AsFloat;
			}
			else if (setMissingToDefault)
			{
				h = _defaultVal.H;
			}
			if (jc[name]["s"] != null)
			{
				s = jc[name]["s"].AsFloat;
			}
			else if (setMissingToDefault)
			{
				s = _defaultVal.S;
			}
			if (jc[name]["v"] != null)
			{
				v = jc[name]["v"].AsFloat;
			}
			else if (setMissingToDefault)
			{
				v = _defaultVal.V;
			}
		}
		else if (setMissingToDefault)
		{
			h = _defaultVal.H;
			s = _defaultVal.S;
			v = _defaultVal.V;
		}
		InternalSetVal(h, s, v, true);
	}

	public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
	{
		if (NeedsRestore(jc, restorePhysical, restoreAppearance))
		{
			Restore(jc, setMissingToDefault);
		}
	}

	public override void LateRestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
	{
		if (NeedsLateRestore(jc, restorePhysical, restoreAppearance))
		{
			Restore(jc, setMissingToDefault);
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

	public void SetVal(float h, float s, float v)
	{
		InternalSetVal(h, s, v, true);
	}

	protected void InternalSetVal(float h, float s, float v, bool doCallback)
	{
		if (_val.H == h && _val.S == s && _val.V == v)
		{
			return;
		}
		_val.H = h;
		_val.S = s;
		_val.V = v;
		if (_colorPicker != null)
		{
			_colorPicker.SetHSV(_val, true);
		}
		if (_colorPickerAlt != null)
		{
			_colorPickerAlt.SetHSV(_val, true);
		}
		if (doCallback)
		{
			if (setCallbackFunction != null)
			{
				setCallbackFunction(_val.H, _val.S, _val.V);
			}
			if (setJSONCallbackFunction != null)
			{
				setJSONCallbackFunction(this);
			}
		}
	}

	public void SetColor(Color c)
	{
		HSVColor hSVColor = HSVColorPicker.RGBToHSV(c.r, c.g, c.b);
		InternalSetVal(hSVColor.H, hSVColor.S, hSVColor.V, true);
	}

	public void RegisterColorPicker(HSVColorPicker cp, bool isAlt = false)
	{
		if (isAlt)
		{
			colorPickerAlt = cp;
		}
		else
		{
			colorPicker = cp;
		}
	}
}
