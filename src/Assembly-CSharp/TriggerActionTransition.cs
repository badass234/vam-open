using System;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class TriggerActionTransition : TriggerAction
{
	public static JSONClass copyOfData;

	protected Button copyButton;

	protected Button pasteButton;

	protected Toggle startWithCurrentValToggle;

	protected bool _startWithCurrentVal;

	protected UIDynamicSlider startValDynamicSlider;

	protected Slider startValSlider;

	protected float _startVal;

	protected UIDynamicSlider endValDynamicSlider;

	protected Slider endValSlider;

	protected float _endVal;

	protected RectTransform startColorPickerContainer;

	protected HSVColorPicker startColorPicker;

	protected float _startHSVColorH;

	protected float _startHSVColorS;

	protected float _startHSVColorV;

	protected RectTransform endColorPickerContainer;

	protected HSVColorPicker endColorPicker;

	protected float _endHSVColorH;

	protected float _endHSVColorS;

	protected float _endHSVColorV;

	protected JSONStorable.Type _actionType;

	protected HSVColor interpColor;

	protected Slider testTransitionSlider;

	public bool active;

	protected float _currentInterp;

	protected bool disableTriggerInterp;

	public bool startWithCurrentVal
	{
		get
		{
			return _startWithCurrentVal;
		}
		set
		{
			if (_startWithCurrentVal != value)
			{
				_startWithCurrentVal = value;
				if (startWithCurrentValToggle != null)
				{
					startWithCurrentValToggle.isOn = _startWithCurrentVal;
				}
			}
		}
	}

	public float startVal
	{
		get
		{
			return _startVal;
		}
		set
		{
			if (_startVal != value)
			{
				_startVal = value;
				CheckStartValConstraint();
				if (startValSlider != null)
				{
					startValSlider.value = _startVal;
				}
				AutoSetPreviewTextAndName();
			}
		}
	}

	public float endVal
	{
		get
		{
			return _endVal;
		}
		set
		{
			if (_endVal != value)
			{
				_endVal = value;
				CheckEndValConstraint();
				if (endValSlider != null)
				{
					endValSlider.value = _endVal;
				}
				AutoSetPreviewTextAndName();
			}
		}
	}

	public JSONStorable.Type actionType
	{
		get
		{
			return _actionType;
		}
		set
		{
			if (_actionType != value)
			{
				_actionType = value;
				SyncType();
			}
		}
	}

	public void CopyData()
	{
		copyOfData = GetJSON();
	}

	public void PasteData()
	{
		if (copyOfData != null)
		{
			RestoreFromJSON(copyOfData);
		}
	}

	public override JSONClass GetJSON(string subScenePrefix = null)
	{
		CheckMissingReceiver();
		JSONClass jSON = base.GetJSON(subScenePrefix);
		if (jSON != null)
		{
			switch (actionType)
			{
			case JSONStorable.Type.Float:
				jSON["startValue"].AsFloat = _startVal;
				jSON["endValue"].AsFloat = _endVal;
				break;
			case JSONStorable.Type.Color:
				jSON["startColor"]["h"].AsFloat = _startHSVColorH;
				jSON["startColor"]["s"].AsFloat = _startHSVColorS;
				jSON["startColor"]["v"].AsFloat = _startHSVColorV;
				jSON["endColor"]["h"].AsFloat = _endHSVColorH;
				jSON["endColor"]["s"].AsFloat = _endHSVColorS;
				jSON["endColor"]["v"].AsFloat = _endHSVColorV;
				break;
			}
			jSON["startWithCurrentVal"].AsBool = _startWithCurrentVal;
		}
		return jSON;
	}

	public override void RestoreFromJSON(JSONClass jc, string subScenePrefix = null)
	{
		base.RestoreFromJSON(jc, subScenePrefix);
		if (jc["startValue"] != null)
		{
			startVal = jc["startValue"].AsFloat;
		}
		if (jc["endValue"] != null)
		{
			endVal = jc["endValue"].AsFloat;
		}
		if (jc["startColor"] != null)
		{
			float h = _startHSVColorH;
			float s = _startHSVColorS;
			float v = _startHSVColorV;
			if (jc["startColor"]["h"] != null)
			{
				h = jc["startColor"]["h"].AsFloat;
			}
			if (jc["startColor"]["s"] != null)
			{
				s = jc["startColor"]["s"].AsFloat;
			}
			if (jc["startColor"]["v"] != null)
			{
				v = jc["startColor"]["v"].AsFloat;
			}
			SetStartColorFromHSV(h, s, v);
		}
		if (jc["endColor"] != null)
		{
			float h2 = _startHSVColorH;
			float s2 = _startHSVColorS;
			float v2 = _startHSVColorV;
			if (jc["endColor"]["h"] != null)
			{
				h2 = jc["endColor"]["h"].AsFloat;
			}
			if (jc["endColor"]["s"] != null)
			{
				s2 = jc["endColor"]["s"].AsFloat;
			}
			if (jc["endColor"]["v"] != null)
			{
				v2 = jc["endColor"]["v"].AsFloat;
			}
			SetEndColorFromHSV(h2, s2, v2);
		}
		if (jc["startWithCurrentVal"] != null)
		{
			startWithCurrentVal = jc["startWithCurrentVal"].AsBool;
		}
		AutoSetPreviewTextAndName();
	}

	protected override void CreateTriggerActionPanel()
	{
		if (handler != null)
		{
			triggerActionPanel = handler.CreateTriggerActionTransitionUI();
			InitTriggerActionPanelUI();
		}
		else
		{
			Debug.LogError("Attempted to CreateTriggerActionPanel when handler was null");
		}
	}

	protected override void AutoSetPreviewText()
	{
		if (_receiverAtom != null && !string.IsNullOrEmpty(_receiverStoreId) && !string.IsNullOrEmpty(_receiverTargetName))
		{
			switch (_actionType)
			{
			case JSONStorable.Type.Float:
				AutoSetPreviewTextFromFloatParam();
				break;
			case JSONStorable.Type.Color:
				AutoSetPreviewTextFromColorParam();
				break;
			default:
				base.AutoSetPreviewText();
				break;
			}
		}
	}

	protected void AutoSetPreviewTextFromFloatParam()
	{
		base.previewText = _receiverAtom.uid + ":" + _receiverStoreId + ":" + _receiverTargetName + ":" + startVal.ToString("F2") + "_" + endVal.ToString("F2");
	}

	protected void AutoSetPreviewTextFromColorParam()
	{
		if (startColorPicker != null && endColorPicker != null)
		{
			base.previewText = _receiverAtom.uid + ":" + _receiverStoreId + ":" + _receiverTargetName + ":" + startColorPicker.red255string + "_" + startColorPicker.green255string + "_" + startColorPicker.blue255string + "__" + endColorPicker.red255string + "_" + endColorPicker.green255string + "_" + endColorPicker.blue255string;
		}
	}

	protected override void AutoSetName()
	{
		if (base.receiver != null && !string.IsNullOrEmpty(_receiverTargetName))
		{
			switch (_actionType)
			{
			case JSONStorable.Type.Float:
				AutoSetNameFromFloatParam();
				break;
			case JSONStorable.Type.Color:
				AutoSetNameFromColorParam();
				break;
			default:
				base.AutoSetName();
				break;
			}
		}
	}

	protected void AutoSetNameFromFloatParam()
	{
		base.name = "A_" + _receiverTargetName + ":" + startVal.ToString("F2") + "_" + endVal.ToString("F2");
	}

	protected void AutoSetNameFromColorParam()
	{
		if (startColorPicker != null && endColorPicker != null)
		{
			base.name = "A_" + _receiverTargetName + ":" + startColorPicker.red255string + "_" + startColorPicker.green255string + "_" + startColorPicker.blue255string + "__" + endColorPicker.red255string + "_" + endColorPicker.green255string + "_" + endColorPicker.blue255string;
		}
	}

	public void SetStartWithCurrentVal(bool b)
	{
		_startWithCurrentVal = b;
	}

	protected void SyncStartValDynamicSlider()
	{
		if (startValSlider != null)
		{
			startValSlider.minValue = paramMin;
			startValSlider.maxValue = paramMax;
			SliderControl component = startValSlider.GetComponent<SliderControl>();
			if (receiverTargetFloat != null && component != null)
			{
				component.defaultValue = receiverTargetFloat.defaultVal;
				component.clamp = paramContrained;
			}
		}
		if (startValDynamicSlider != null)
		{
			startValDynamicSlider.rangeAdjustEnabled = !paramContrained;
			if (paramMax <= 2f)
			{
				startValDynamicSlider.valueFormat = "F3";
			}
			else if (paramMax <= 20f)
			{
				startValDynamicSlider.valueFormat = "F2";
			}
			else if (paramMax <= 200f)
			{
				startValDynamicSlider.valueFormat = "F1";
			}
			else
			{
				startValDynamicSlider.valueFormat = "F0";
			}
			if (receiverTargetFloat != null)
			{
				startValDynamicSlider.label = "(Start) " + receiverTargetFloat.name;
			}
		}
	}

	public void SetStartVal(float f)
	{
		_startVal = f;
		CheckStartValConstraint();
		TriggerInterp(_currentInterp);
		AutoSetPreviewTextAndName();
	}

	protected void CheckStartValConstraint(bool skipSync = false)
	{
		if (!paramContrained)
		{
			bool flag = false;
			if (_startVal < paramMin)
			{
				paramMin = _startVal;
				flag = true;
			}
			if (_startVal > paramMax)
			{
				paramMax = _startVal;
				flag = true;
			}
			if (flag && !skipSync)
			{
				SyncStartValDynamicSlider();
				SyncEndValDynamicSlider();
			}
		}
	}

	protected void SyncEndValDynamicSlider()
	{
		if (endValSlider != null)
		{
			endValSlider.minValue = paramMin;
			endValSlider.maxValue = paramMax;
			SliderControl component = endValSlider.GetComponent<SliderControl>();
			if (receiverTargetFloat != null && component != null)
			{
				component.defaultValue = receiverTargetFloat.defaultVal;
				component.clamp = paramContrained;
			}
		}
		if (endValDynamicSlider != null)
		{
			endValDynamicSlider.rangeAdjustEnabled = !paramContrained;
			if (paramMax <= 2f)
			{
				endValDynamicSlider.valueFormat = "F3";
			}
			else if (paramMax <= 20f)
			{
				endValDynamicSlider.valueFormat = "F2";
			}
			else if (paramMax <= 200f)
			{
				endValDynamicSlider.valueFormat = "F1";
			}
			else
			{
				endValDynamicSlider.valueFormat = "F0";
			}
			if (receiverTargetFloat != null)
			{
				endValDynamicSlider.label = "(End) " + receiverTargetFloat.name;
			}
		}
	}

	public void SetEndVal(float f)
	{
		_endVal = f;
		CheckEndValConstraint();
		TriggerInterp(_currentInterp);
		AutoSetPreviewTextAndName();
	}

	protected void CheckEndValConstraint(bool skipSync = false)
	{
		if (!paramContrained)
		{
			bool flag = false;
			if (_endVal < paramMin)
			{
				paramMin = _endVal;
				flag = true;
			}
			if (_endVal > paramMax)
			{
				paramMax = _endVal;
				flag = true;
			}
			if (flag && !skipSync)
			{
				SyncStartValDynamicSlider();
				SyncEndValDynamicSlider();
			}
		}
	}

	protected virtual void SetStartColorFromHSV(float h, float s, float v)
	{
		if (_startHSVColorH != h || _startHSVColorS != s || _startHSVColorV != v)
		{
			_startHSVColorH = h;
			_startHSVColorS = s;
			_startHSVColorV = v;
			if (startColorPicker != null)
			{
				startColorPicker.SetHSV(h, s, v, true);
			}
			TriggerInterp(_currentInterp);
			AutoSetPreviewTextAndName();
		}
	}

	protected virtual void SetEndColorFromHSV(float h, float s, float v)
	{
		SetEndColorFromHSV(h, s, v, false);
	}

	protected virtual void SetEndColorFromHSV(float h, float s, float v, bool force)
	{
		if (force || _endHSVColorH != h || _endHSVColorS != s || _endHSVColorV != v)
		{
			_endHSVColorH = h;
			_endHSVColorS = s;
			_endHSVColorV = v;
			if (endColorPicker != null)
			{
				endColorPicker.SetHSV(h, s, v, true);
			}
			TriggerInterp(_currentInterp);
			AutoSetPreviewTextAndName();
		}
	}

	protected override void SetReceiverTargetPopupNames()
	{
		receiverTargetNames = null;
		if (_receiver != null)
		{
			receiverTargetNames = _receiver.GetAllFloatAndColorParamNames();
		}
		SyncTargetPopupNames();
	}

	public override void SetReceiverTargetNameAndSetInitialParams(string targetName)
	{
		disableTriggerInterp = true;
		base.SetReceiverTargetNameAndSetInitialParams(targetName);
		disableTriggerInterp = false;
	}

	protected override void SetInitialParamsFromReceiverTarget()
	{
		base.SetInitialParamsFromReceiverTarget();
		if (_receiver != null && base.receiverTargetName != null)
		{
			switch (_actionType)
			{
			case JSONStorable.Type.Float:
				startVal = _receiver.GetFloatParamValue(_receiverTargetName);
				endVal = _receiver.GetFloatParamValue(_receiverTargetName);
				break;
			case JSONStorable.Type.Color:
			{
				HSVColor colorParamValue = _receiver.GetColorParamValue(_receiverTargetName);
				SetStartColorFromHSV(colorParamValue.H, colorParamValue.S, colorParamValue.V);
				SetEndColorFromHSV(colorParamValue.H, colorParamValue.S, colorParamValue.V);
				break;
			}
			}
		}
	}

	protected override void SyncFromReceiverTarget()
	{
		base.SyncFromReceiverTarget();
		if (!receiverSetFromPopup)
		{
			CheckStartValConstraint(true);
			CheckEndValConstraint(true);
		}
		SyncStartValDynamicSlider();
		SyncEndValDynamicSlider();
		if (_receiver != null && _receiverTargetName != null)
		{
			actionType = _receiver.GetParamOrActionType(_receiverTargetName);
			AutoSetPreviewTextAndName();
		}
		else
		{
			actionType = JSONStorable.Type.None;
		}
	}

	protected override void CheckMissingReceiver()
	{
		base.CheckMissingReceiver();
		if (_receiver != null && _receiverTargetName != null && _actionType == JSONStorable.Type.None)
		{
			SyncFromReceiverTarget();
		}
	}

	protected void SyncType()
	{
		if (startValSlider != null)
		{
			if (_actionType == JSONStorable.Type.Float)
			{
				startValSlider.transform.parent.gameObject.SetActive(true);
			}
			else
			{
				startValSlider.transform.parent.gameObject.SetActive(false);
			}
		}
		if (endValSlider != null)
		{
			if (_actionType == JSONStorable.Type.Float)
			{
				endValSlider.transform.parent.gameObject.SetActive(true);
			}
			else
			{
				endValSlider.transform.parent.gameObject.SetActive(false);
			}
		}
		if (startColorPickerContainer != null)
		{
			if (_actionType == JSONStorable.Type.Color)
			{
				startColorPickerContainer.gameObject.SetActive(true);
			}
			else
			{
				startColorPickerContainer.gameObject.SetActive(false);
			}
		}
		if (endColorPickerContainer != null)
		{
			if (_actionType == JSONStorable.Type.Color)
			{
				endColorPickerContainer.gameObject.SetActive(true);
			}
			else
			{
				endColorPickerContainer.gameObject.SetActive(false);
			}
		}
	}

	public void InitTriggerStart()
	{
		if (!_startWithCurrentVal)
		{
			return;
		}
		CheckMissingReceiver();
		if (base.receiver != null && base.receiverTargetName != null)
		{
			if (_actionType == JSONStorable.Type.Float)
			{
				startVal = base.receiver.GetFloatParamValue(base.receiverTargetName);
			}
			else if (_actionType == JSONStorable.Type.Color)
			{
				HSVColor colorParamValue = base.receiver.GetColorParamValue(base.receiverTargetName);
				SetStartColorFromHSV(colorParamValue.H, colorParamValue.S, colorParamValue.V);
			}
		}
	}

	public override void Validate()
	{
		CheckMissingReceiver();
		if (base.receiver != null && base.receiverTargetName != null)
		{
			switch (_actionType)
			{
			case JSONStorable.Type.Float:
				if (!base.receiver.IsFloatJSONParam(base.receiverTargetName))
				{
					base.receiverTargetName = null;
				}
				break;
			case JSONStorable.Type.Color:
				if (!base.receiver.IsColorJSONParam(base.receiverTargetName))
				{
					base.receiverTargetName = null;
				}
				break;
			}
		}
		base.Validate();
	}

	protected void SetTestTransition(float f)
	{
		if (_currentInterp != f)
		{
			TriggerInterp(f, true);
		}
	}

	public void TriggerInterp(float interp, bool force = false)
	{
		if (!base.enabled && !force)
		{
			return;
		}
		_currentInterp = interp;
		if (testTransitionSlider != null)
		{
			testTransitionSlider.value = _currentInterp;
		}
		if ((!active && !force) || disableTriggerInterp)
		{
			return;
		}
		CheckMissingReceiver();
		if (base.receiver != null && base.receiverTargetName != null)
		{
			if (_actionType == JSONStorable.Type.Float)
			{
				float value = Mathf.Lerp(startVal, endVal, interp);
				base.receiver.SetFloatParamValue(base.receiverTargetName, value);
			}
			else if (_actionType == JSONStorable.Type.Color)
			{
				interpColor.H = Mathf.Lerp(_startHSVColorH, _endHSVColorH, interp);
				interpColor.S = Mathf.Lerp(_startHSVColorS, _endHSVColorS, interp);
				interpColor.V = Mathf.Lerp(_startHSVColorV, _endHSVColorV, interp);
				base.receiver.SetColorParamValue(_receiverTargetName, interpColor);
			}
		}
	}

	public override void InitTriggerActionPanelUI()
	{
		CheckMissingReceiver();
		base.InitTriggerActionPanelUI();
		if (triggerActionPanel != null)
		{
			TriggerActionTransitionUI component = triggerActionPanel.GetComponent<TriggerActionTransitionUI>();
			if (component != null)
			{
				copyButton = component.copyButton;
				pasteButton = component.pasteButton;
				testTransitionSlider = component.testTransitionSlider;
				startWithCurrentValToggle = component.startWithCurrentValToggle;
				startValSlider = component.startValSlider;
				startValDynamicSlider = component.startValDynamicSlider;
				endValSlider = component.endValSlider;
				endValDynamicSlider = component.endValDynamicSlider;
				startColorPickerContainer = component.startColorPickerContainer;
				startColorPicker = component.startColorPicker;
				endColorPickerContainer = component.endColorPickerContainer;
				endColorPicker = component.endColorPicker;
			}
		}
		if (copyButton != null)
		{
			copyButton.onClick.AddListener(CopyData);
		}
		if (pasteButton != null)
		{
			pasteButton.onClick.AddListener(PasteData);
		}
		if (testTransitionSlider != null)
		{
			testTransitionSlider.onValueChanged.AddListener(SetTestTransition);
		}
		if (startWithCurrentValToggle != null)
		{
			startWithCurrentValToggle.isOn = _startWithCurrentVal;
			startWithCurrentValToggle.onValueChanged.AddListener(SetStartWithCurrentVal);
		}
		if (startValSlider != null)
		{
			startValSlider.minValue = paramMin;
			if (startValSlider.minValue > _startVal)
			{
				startValSlider.minValue = _startVal;
			}
			startValSlider.maxValue = paramMax;
			if (startValSlider.maxValue < _startVal)
			{
				startValSlider.maxValue = _startVal;
			}
			startValSlider.value = _startVal;
			startValSlider.onValueChanged.AddListener(SetStartVal);
		}
		if (endValSlider != null)
		{
			endValSlider.minValue = paramMin;
			if (endValSlider.minValue > _endVal)
			{
				endValSlider.minValue = _endVal;
			}
			endValSlider.maxValue = paramMax;
			if (endValSlider.maxValue < _endVal)
			{
				endValSlider.maxValue = _endVal;
			}
			endValSlider.value = _endVal;
			endValSlider.onValueChanged.AddListener(SetEndVal);
		}
		if (startColorPicker != null)
		{
			startColorPicker.hue = _startHSVColorH;
			startColorPicker.saturation = _startHSVColorS;
			startColorPicker.cvalue = _startHSVColorV;
			HSVColorPicker hSVColorPicker = startColorPicker;
			hSVColorPicker.onHSVColorChangedHandlers = (HSVColorPicker.OnHSVColorChanged)Delegate.Combine(hSVColorPicker.onHSVColorChangedHandlers, new HSVColorPicker.OnHSVColorChanged(SetStartColorFromHSV));
		}
		if (endColorPicker != null)
		{
			endColorPicker.hue = _endHSVColorH;
			endColorPicker.saturation = _endHSVColorS;
			endColorPicker.cvalue = _endHSVColorV;
			HSVColorPicker hSVColorPicker2 = endColorPicker;
			hSVColorPicker2.onHSVColorChangedHandlers = (HSVColorPicker.OnHSVColorChanged)Delegate.Combine(hSVColorPicker2.onHSVColorChangedHandlers, new HSVColorPicker.OnHSVColorChanged(SetEndColorFromHSV));
		}
		SyncType();
		SyncStartValDynamicSlider();
		SyncEndValDynamicSlider();
	}

	public override void DeregisterUI()
	{
		base.DeregisterUI();
		if (copyButton != null)
		{
			copyButton.onClick.RemoveListener(CopyData);
		}
		if (pasteButton != null)
		{
			pasteButton.onClick.RemoveListener(PasteData);
		}
		if (testTransitionSlider != null)
		{
			testTransitionSlider.onValueChanged.RemoveListener(SetTestTransition);
		}
		if (startWithCurrentValToggle != null)
		{
			startWithCurrentValToggle.onValueChanged.RemoveListener(SetStartWithCurrentVal);
		}
		if (startValSlider != null)
		{
			startValSlider.onValueChanged.RemoveListener(SetStartVal);
		}
		if (endValSlider != null)
		{
			endValSlider.onValueChanged.RemoveListener(SetEndVal);
		}
		if (startColorPicker != null)
		{
			HSVColorPicker hSVColorPicker = startColorPicker;
			hSVColorPicker.onHSVColorChangedHandlers = (HSVColorPicker.OnHSVColorChanged)Delegate.Remove(hSVColorPicker.onHSVColorChangedHandlers, new HSVColorPicker.OnHSVColorChanged(SetStartColorFromHSV));
		}
		if (endColorPicker != null)
		{
			HSVColorPicker hSVColorPicker2 = endColorPicker;
			hSVColorPicker2.onHSVColorChangedHandlers = (HSVColorPicker.OnHSVColorChanged)Delegate.Remove(hSVColorPicker2.onHSVColorChangedHandlers, new HSVColorPicker.OnHSVColorChanged(SetEndColorFromHSV));
		}
	}
}
