using System;
using System.Collections.Generic;
using MVR.FileManagement;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class TriggerActionDiscrete : TriggerAction
{
	public enum AudioClipType
	{
		Embedded,
		URL
	}

	public enum TimerType
	{
		EaseInOut,
		Linear
	}

	public static JSONClass copyOfData;

	protected Button copyButton;

	protected Button pasteButton;

	protected Button testButton;

	protected Button resetTestButton;

	protected bool beforeTestStateCaptured;

	protected JSONStorable.Type _actionType;

	protected RectTransform audioClipPopupsContainer;

	protected UIPopup audioClipTypePopup;

	protected AudioClipType _audioClipType;

	protected UIPopup audioClipCategoryPopup;

	protected string _audioClipCategory;

	protected UIPopup audioClipPopup;

	protected NamedAudioClip _audioClip;

	protected string _stringChooserActionChoice;

	protected UIPopup stringChooserActionPopup;

	protected Button chooseSceneFilePathButton;

	protected Text sceneFilePathText;

	protected string _sceneFilePath;

	protected Button choosePresetFilePathButton;

	protected Text presetFilePathText;

	protected string _presetFilePath;

	protected Toggle boolValueToggle;

	protected bool beforeTestBoolValue;

	protected bool _boolValue;

	protected UIDynamicSlider floatValueDynamicSlider;

	protected Slider floatValueSlider;

	protected float beforeTestFloatValue;

	protected float _floatValue;

	protected TimerType _curveTimerType;

	protected float _curveTimerLength;

	protected float _curveValueStart;

	protected float _curveValueEnd;

	protected bool _curveUseSecondPoint;

	protected float _curveSecondPointValue;

	protected float _curveSecondPointLocation;

	protected RectTransform curveContainer;

	protected UILineRenderer curveLineRenderer;

	protected Toggle useTimerToggle;

	protected bool _useTimer;

	protected Slider timerLengthSlider;

	protected float _timerLength = 0.5f;

	protected float _oneOverTimerLength = 2f;

	protected UIPopup timerTypePopup;

	protected TimerType _timerType;

	protected Toggle useSecondTimerPointToggle;

	protected bool _useSecondTimerPoint;

	protected UIDynamicSlider secondTimerPointValueDynamicSlider;

	protected Slider secondTimerPointValueSlider;

	protected float _secondTimerPointValue;

	protected Slider secondTimerPointCurveLocationSlider;

	protected float _secondTimerPointCurveLocation = 0.5f;

	protected InputField stringValueField;

	protected InputFieldAction stringValueFieldAction;

	protected string beforeTestStringValue;

	protected string _stringValue;

	protected RectTransform colorPickerContainer;

	protected HSVColorPicker colorPicker;

	protected float _HSVColorH;

	protected float _HSVColorS;

	protected float _HSVColorV;

	protected UIPopup stringChooserValuePopup;

	protected string beforeTestStringChooserValue;

	protected string _stringChooserValue;

	protected HSVColor beforeTestTriggerColor;

	protected HSVColor triggerColor;

	public bool doActionsInReverse = true;

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

	public AudioClipType audioClipType
	{
		get
		{
			return _audioClipType;
		}
		set
		{
			if (_audioClipType != value)
			{
				_audioClipType = value;
				if (audioClipTypePopup != null)
				{
					audioClipTypePopup.currentValue = _audioClipType.ToString();
				}
				audioClipCategory = null;
				SetClipCategoryPopupValues();
			}
		}
	}

	public string audioClipCategory
	{
		get
		{
			return _audioClipCategory;
		}
		set
		{
			if (_audioClipCategory != value)
			{
				_audioClipCategory = value;
				if (audioClipCategoryPopup != null)
				{
					audioClipCategoryPopup.currentValue = value;
				}
				audioClip = null;
				SetClipPopupValues();
			}
		}
	}

	public NamedAudioClip audioClip
	{
		get
		{
			return _audioClip;
		}
		set
		{
			if (_audioClip == value)
			{
				return;
			}
			_audioClip = value;
			if (audioClipPopup != null)
			{
				if (_audioClip == null)
				{
					audioClipPopup.currentValue = "None";
				}
				else
				{
					audioClipPopup.currentValue = _audioClip.uid;
				}
			}
			AutoSetPreviewTextAndName();
		}
	}

	public string stringChooserActionChoice
	{
		get
		{
			return _stringChooserActionChoice;
		}
		set
		{
			if (_stringChooserActionChoice != value)
			{
				_stringChooserActionChoice = value;
				if (stringChooserActionPopup != null)
				{
					stringChooserActionPopup.currentValue = _stringChooserActionChoice;
				}
				AutoSetPreviewTextAndName();
			}
		}
	}

	public string sceneFilePath
	{
		get
		{
			return _sceneFilePath;
		}
		set
		{
			if (_sceneFilePath != value)
			{
				_sceneFilePath = value;
				if (sceneFilePathText != null)
				{
					sceneFilePathText.text = _sceneFilePath;
				}
				AutoSetPreviewTextAndName();
			}
		}
	}

	public string presetFilePath
	{
		get
		{
			return _presetFilePath;
		}
		set
		{
			if (_presetFilePath != value)
			{
				_presetFilePath = value;
				if (presetFilePathText != null)
				{
					presetFilePathText.text = _presetFilePath;
				}
				AutoSetPreviewTextAndName();
			}
		}
	}

	public bool boolValue
	{
		get
		{
			return _boolValue;
		}
		set
		{
			if (_boolValue != value)
			{
				_boolValue = value;
				if (boolValueToggle != null)
				{
					boolValueToggle.isOn = _boolValue;
				}
				AutoSetPreviewTextAndName();
			}
		}
	}

	public float floatValue
	{
		get
		{
			return _floatValue;
		}
		set
		{
			if (_floatValue != value)
			{
				_floatValue = value;
				SyncFloatCurve(true);
				if (floatValueSlider != null)
				{
					floatValueSlider.value = _floatValue;
				}
				AutoSetPreviewTextAndName();
			}
		}
	}

	public bool timerActive { get; protected set; }

	public float currentTimerTime { get; protected set; }

	public AnimationCurve floatCurve { get; protected set; }

	public bool useTimer
	{
		get
		{
			return _useTimer;
		}
		set
		{
			if (_useTimer != value)
			{
				_useTimer = value;
				if (useTimerToggle != null)
				{
					useTimerToggle.isOn = _useTimer;
				}
			}
		}
	}

	public float timerLength
	{
		get
		{
			return _timerLength;
		}
		set
		{
			if (_timerLength != value)
			{
				SetTimerLength(value);
				if (timerLengthSlider != null)
				{
					timerLengthSlider.value = _timerLength;
				}
			}
		}
	}

	public TimerType timerType
	{
		get
		{
			return _timerType;
		}
		set
		{
			if (_timerType != value)
			{
				_timerType = value;
				SyncFloatCurve(true);
				SyncTimerTypePopup();
			}
		}
	}

	public bool useSecondTimerPoint
	{
		get
		{
			return _useSecondTimerPoint;
		}
		set
		{
			if (_useSecondTimerPoint != value)
			{
				_useSecondTimerPoint = value;
				SyncFloatCurve(true);
				if (useSecondTimerPointToggle != null)
				{
					useSecondTimerPointToggle.isOn = _useSecondTimerPoint;
				}
			}
		}
	}

	public float secondTimerPointValue
	{
		get
		{
			return _secondTimerPointValue;
		}
		set
		{
			if (_secondTimerPointValue != value)
			{
				_secondTimerPointValue = value;
				SyncFloatCurve(true);
				if (secondTimerPointValueSlider != null)
				{
					secondTimerPointValueSlider.value = _secondTimerPointValue;
				}
			}
		}
	}

	public float secondTimerPointCurveLocation
	{
		get
		{
			return _secondTimerPointCurveLocation;
		}
		set
		{
			if (_secondTimerPointCurveLocation != value)
			{
				_secondTimerPointCurveLocation = value;
				SyncFloatCurve(true);
				if (secondTimerPointCurveLocationSlider != null)
				{
					secondTimerPointCurveLocationSlider.value = _secondTimerPointCurveLocation;
				}
			}
		}
	}

	public string stringValue
	{
		get
		{
			return _stringValue;
		}
		set
		{
			if (_stringValue != value)
			{
				_stringValue = value;
				if (stringValueField != null)
				{
					stringValueField.text = _stringValue;
				}
				AutoSetPreviewTextAndName();
			}
		}
	}

	public string stringChooserValue
	{
		get
		{
			return _stringChooserValue;
		}
		set
		{
			if (_stringChooserValue != value)
			{
				_stringChooserValue = value;
				if (stringChooserValuePopup != null)
				{
					stringChooserValuePopup.currentValueNoCallback = _stringChooserValue;
				}
				AutoSetPreviewTextAndName();
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
			case JSONStorable.Type.Bool:
				jSON["boolValue"].AsBool = _boolValue;
				break;
			case JSONStorable.Type.Float:
				jSON["floatValue"].AsFloat = _floatValue;
				if (_useTimer)
				{
					jSON["useTimer"].AsBool = _useTimer;
					jSON["timerLength"].AsFloat = _timerLength;
					jSON["timerType"] = _timerType.ToString();
					if (_useSecondTimerPoint)
					{
						jSON["useSecondTimerPoint"].AsBool = _useSecondTimerPoint;
						jSON["secondTimerPointValue"].AsFloat = _secondTimerPointValue;
						jSON["secondTimerPointCurveLocation"].AsFloat = _secondTimerPointCurveLocation;
					}
				}
				break;
			case JSONStorable.Type.String:
				if (_stringValue != null)
				{
					jSON["stringValue"] = _stringValue;
				}
				break;
			case JSONStorable.Type.Url:
				if (_stringValue != null)
				{
					jSON["urlValue"] = _stringValue;
				}
				break;
			case JSONStorable.Type.StringChooser:
				if (_stringChooserValue != null)
				{
					jSON["stringChooserValue"] = _stringChooserValue;
				}
				break;
			case JSONStorable.Type.StringChooserAction:
				if (_stringChooserActionChoice != null)
				{
					jSON["stringChooserActionChoice"] = _stringChooserActionChoice;
				}
				break;
			case JSONStorable.Type.Color:
				jSON["color"]["h"].AsFloat = _HSVColorH;
				jSON["color"]["s"].AsFloat = _HSVColorS;
				jSON["color"]["v"].AsFloat = _HSVColorV;
				break;
			case JSONStorable.Type.AudioClipAction:
				jSON["audioClipType"] = _audioClipType.ToString();
				if (_audioClipCategory != null)
				{
					jSON["audioClipCategory"] = _audioClipCategory;
				}
				if (_audioClip != null)
				{
					jSON["audioClip"] = _audioClip.uid;
				}
				break;
			case JSONStorable.Type.SceneFilePathAction:
				if (_sceneFilePath != null && _sceneFilePath != string.Empty)
				{
					string text2 = _sceneFilePath;
					if (SuperController.singleton != null)
					{
						text2 = SuperController.singleton.NormalizeSavePath(text2);
					}
					jSON["sceneFilePath"] = text2;
				}
				break;
			case JSONStorable.Type.PresetFilePathAction:
				if (_presetFilePath != null && _presetFilePath != string.Empty)
				{
					string text = _presetFilePath;
					if (SuperController.singleton != null)
					{
						text = SuperController.singleton.NormalizeSavePath(text);
					}
					jSON["presetFilePath"] = text;
				}
				break;
			}
		}
		return jSON;
	}

	public override void RestoreFromJSON(JSONClass jc, string subScenePrefix = null)
	{
		base.RestoreFromJSON(jc, subScenePrefix);
		if (jc["boolValue"] != null)
		{
			boolValue = jc["boolValue"].AsBool;
		}
		if (jc["floatValue"] != null)
		{
			floatValue = jc["floatValue"].AsFloat;
		}
		if (jc["useTimer"] != null)
		{
			useTimer = jc["useTimer"].AsBool;
		}
		else
		{
			useTimer = false;
		}
		if (jc["timerLength"] != null)
		{
			timerLength = jc["timerLength"].AsFloat;
		}
		else
		{
			timerLength = 0.5f;
		}
		if (jc["timerType"] != null)
		{
			SetTimerType(jc["timerType"]);
			SyncTimerTypePopup();
		}
		else
		{
			timerType = TimerType.EaseInOut;
		}
		if (jc["useSecondTimerPoint"] != null)
		{
			useSecondTimerPoint = jc["useSecondTimerPoint"].AsBool;
		}
		else
		{
			useSecondTimerPoint = false;
		}
		if (jc["secondTimerPointValue"] != null)
		{
			secondTimerPointValue = jc["secondTimerPointValue"].AsFloat;
		}
		else
		{
			secondTimerPointValue = _floatValue;
		}
		if (jc["secondTimerPointCurveLocation"] != null)
		{
			secondTimerPointCurveLocation = jc["secondTimerPointCurveLocation"].AsFloat;
		}
		else
		{
			secondTimerPointCurveLocation = 0.5f;
		}
		if (jc["stringValue"] != null)
		{
			stringValue = jc["stringValue"];
		}
		if (jc["urlValue"] != null)
		{
			stringValue = FileManager.NormalizeLoadPath(jc["urlValue"]);
		}
		if (jc["stringChooserValue"] != null)
		{
			stringChooserValue = jc["stringChooserValue"];
		}
		if (jc["stringChooserActionChoice"] != null)
		{
			stringChooserActionChoice = jc["stringChooserActionChoice"];
		}
		if (jc["color"] != null)
		{
			float h = _HSVColorH;
			float s = _HSVColorS;
			float v = _HSVColorV;
			if (jc["color"]["h"] != null)
			{
				h = jc["color"]["h"].AsFloat;
			}
			if (jc["color"]["s"] != null)
			{
				s = jc["color"]["s"].AsFloat;
			}
			if (jc["color"]["v"] != null)
			{
				v = jc["color"]["v"].AsFloat;
			}
			SetColorFromHSV(h, s, v);
		}
		if (jc["audioClipType"] != null)
		{
			SetAudioClipType(jc["audioClipType"]);
		}
		if (jc["audioClipCategory"] != null)
		{
			SetAudioClipCategory(jc["audioClipCategory"]);
		}
		if (jc["audioClip"] != null)
		{
			SetAudioClip(jc["audioClip"]);
		}
		if (jc["sceneFilePath"] != null)
		{
			string path = jc["sceneFilePath"];
			if (SuperController.singleton != null)
			{
				path = SuperController.singleton.NormalizeLoadPath(path);
			}
			SetSceneFilePath(path);
		}
		if (jc["presetFilePath"] != null)
		{
			string path2 = jc["presetFilePath"];
			if (SuperController.singleton != null)
			{
				path2 = SuperController.singleton.NormalizeLoadPath(path2);
			}
			SetPresetFilePath(path2);
		}
		AutoSetPreviewTextAndName();
	}

	protected override void CreateTriggerActionPanel()
	{
		if (handler != null)
		{
			triggerActionPanel = handler.CreateTriggerActionDiscreteUI();
			InitTriggerActionPanelUI();
		}
		else
		{
			Debug.LogError("Attempt to CreateTriggerActionPanel when handler is null");
		}
	}

	public override void OpenDetailPanel()
	{
		base.OpenDetailPanel();
		SyncFloatCurveRender();
	}

	public void Test()
	{
		ResetTest();
		if (resetTestButton != null)
		{
			resetTestButton.interactable = true;
		}
		if (!beforeTestStateCaptured && base.receiver != null && base.receiverTargetName != null)
		{
			switch (_actionType)
			{
			case JSONStorable.Type.Bool:
				beforeTestBoolValue = base.receiver.GetBoolParamValue(base.receiverTargetName);
				break;
			case JSONStorable.Type.Float:
				beforeTestFloatValue = base.receiver.GetFloatParamValue(base.receiverTargetName);
				break;
			case JSONStorable.Type.Color:
				beforeTestTriggerColor = base.receiver.GetColorParamValue(base.receiverTargetName);
				break;
			case JSONStorable.Type.String:
				beforeTestStringValue = base.receiver.GetStringParamValue(base.receiverTargetName);
				break;
			case JSONStorable.Type.Url:
				beforeTestStringValue = base.receiver.GetUrlParamValue(base.receiverTargetName);
				break;
			case JSONStorable.Type.StringChooser:
				beforeTestStringChooserValue = base.receiver.GetStringChooserParamValue(base.receiverTargetName);
				break;
			}
			beforeTestStateCaptured = true;
		}
		Trigger(false, true);
	}

	public void ResetTest()
	{
		if (resetTestButton != null)
		{
			resetTestButton.interactable = false;
		}
		if (beforeTestStateCaptured && base.receiver != null && base.receiverTargetName != null)
		{
			switch (_actionType)
			{
			case JSONStorable.Type.Bool:
				base.receiver.SetBoolParamValue(base.receiverTargetName, beforeTestBoolValue);
				break;
			case JSONStorable.Type.Float:
				base.receiver.SetFloatParamValue(base.receiverTargetName, beforeTestFloatValue);
				break;
			case JSONStorable.Type.Color:
				base.receiver.SetColorParamValue(base.receiverTargetName, beforeTestTriggerColor);
				break;
			case JSONStorable.Type.String:
				base.receiver.SetStringParamValue(base.receiverTargetName, beforeTestStringValue);
				break;
			case JSONStorable.Type.Url:
				base.receiver.SetUrlParamValue(base.receiverTargetName, beforeTestStringValue);
				break;
			case JSONStorable.Type.StringChooser:
				base.receiver.SetStringChooserParamValue(base.receiverTargetName, beforeTestStringChooserValue);
				break;
			case JSONStorable.Type.AudioClipAction:
				base.receiver.CallAction("Stop");
				break;
			}
			beforeTestStateCaptured = false;
			timerActive = false;
		}
	}

	protected override void SetReceiverTargetPopupNames()
	{
		receiverTargetNames = null;
		if (_receiver != null)
		{
			receiverTargetNames = _receiver.GetAllParamAndActionNames();
		}
		SyncTargetPopupNames();
	}

	protected override void AutoSetPreviewText()
	{
		if (_receiverAtom != null && !string.IsNullOrEmpty(_receiverStoreId) && !string.IsNullOrEmpty(_receiverTargetName))
		{
			switch (_actionType)
			{
			case JSONStorable.Type.Bool:
				AutoSetPreviewTextFromBoolParam();
				break;
			case JSONStorable.Type.Float:
				AutoSetPreviewTextFromFloatParam();
				break;
			case JSONStorable.Type.String:
			case JSONStorable.Type.Url:
				AutoSetPreviewTextFromStringOrUrlParam();
				break;
			case JSONStorable.Type.StringChooser:
				AutoSetPreviewTextFromStringChooserParam();
				break;
			case JSONStorable.Type.AudioClipAction:
				AutoSetPreviewTextFromAudioClipParam();
				break;
			default:
				base.AutoSetPreviewText();
				break;
			}
		}
	}

	protected void AutoSetPreviewTextFromBoolParam()
	{
		base.previewText = _receiverAtom.uid + ":" + _receiverStoreId + ":" + _receiverTargetName + ":" + boolValue;
	}

	protected void AutoSetPreviewTextFromFloatParam()
	{
		base.previewText = _receiverAtom.uid + ":" + _receiverStoreId + ":" + _receiverTargetName + ":" + floatValue.ToString("F2");
	}

	protected void AutoSetPreviewTextFromStringOrUrlParam()
	{
		base.previewText = _receiverAtom.uid + ":" + _receiverStoreId + ":" + _receiverTargetName + ":" + stringValue;
	}

	protected void AutoSetPreviewTextFromStringChooserParam()
	{
		base.previewText = _receiverAtom.uid + ":" + _receiverStoreId + ":" + _receiverTargetName + ":" + stringChooserValue;
	}

	protected void AutoSetPreviewTextFromColorParam()
	{
		if (colorPicker != null)
		{
			base.previewText = _receiverAtom.uid + ":" + _receiverStoreId + ":" + _receiverTargetName + ":" + colorPicker.red255string + "_" + colorPicker.green255string + "_" + colorPicker.blue255string;
		}
	}

	protected void AutoSetPreviewTextFromAudioClipParam()
	{
		if (_audioClip != null)
		{
			base.previewText = _receiverAtom.uid + ":" + _receiverStoreId + ":" + _receiverTargetName + ":" + _audioClip.uid;
		}
	}

	protected override void AutoSetName()
	{
		if (base.receiver != null && !string.IsNullOrEmpty(_receiverTargetName))
		{
			switch (_actionType)
			{
			case JSONStorable.Type.Bool:
				AutoSetNameFromBoolParam();
				break;
			case JSONStorable.Type.Float:
				AutoSetNameFromFloatParam();
				break;
			case JSONStorable.Type.String:
			case JSONStorable.Type.Url:
				AutoSetNameFromStringOrUrlParam();
				break;
			case JSONStorable.Type.StringChooser:
				AutoSetNameFromStringChooserParam();
				break;
			case JSONStorable.Type.AudioClipAction:
				AutoSetNameFromAudioClipParam();
				break;
			default:
				base.AutoSetName();
				break;
			}
		}
	}

	protected void AutoSetNameFromBoolParam()
	{
		base.name = "A_" + _receiverTargetName + ":" + boolValue;
	}

	protected void AutoSetNameFromFloatParam()
	{
		base.name = "A_" + _receiverTargetName + ":" + floatValue.ToString("F2");
	}

	protected void AutoSetNameFromStringOrUrlParam()
	{
		base.name = "A_" + _receiverTargetName + ":" + stringValue;
	}

	protected void AutoSetNameFromStringChooserParam()
	{
		base.name = "A_" + _receiverTargetName + ":" + stringChooserValue;
	}

	protected void AutoSetNameFromColorParam()
	{
		if (colorPicker != null)
		{
			base.name = "A_" + _receiverTargetName + ":" + colorPicker.red255string + "_" + colorPicker.green255string + "_" + colorPicker.blue255string;
		}
	}

	protected void AutoSetNameFromAudioClipParam()
	{
		if (_audioClip != null)
		{
			base.name = "A_" + _receiverTargetName + ":" + _audioClip.uid;
		}
	}

	protected override void SetInitialParamsFromReceiverTarget()
	{
		base.SetInitialParamsFromReceiverTarget();
		if (_receiver != null && base.receiverTargetName != null)
		{
			switch (actionType)
			{
			case JSONStorable.Type.Bool:
				boolValue = _receiver.GetBoolParamValue(_receiverTargetName);
				break;
			case JSONStorable.Type.Float:
				floatValue = _receiver.GetFloatParamValue(_receiverTargetName);
				secondTimerPointValue = _floatValue;
				break;
			case JSONStorable.Type.String:
				stringValue = _receiver.GetStringParamValue(_receiverTargetName);
				break;
			case JSONStorable.Type.Url:
				stringValue = _receiver.GetUrlParamValue(_receiverTargetName);
				break;
			case JSONStorable.Type.StringChooser:
				stringChooserValue = _receiver.GetStringChooserParamValue(_receiverTargetName);
				break;
			case JSONStorable.Type.Color:
			{
				HSVColor colorParamValue = _receiver.GetColorParamValue(_receiverTargetName);
				SetColorFromHSV(colorParamValue.H, colorParamValue.S, colorParamValue.V);
				break;
			}
			case JSONStorable.Type.Vector3:
				break;
			}
		}
	}

	protected void SyncStringChooserPopup()
	{
		if (!(stringChooserValuePopup != null) || !(_receiver != null) || _receiverTargetName == null)
		{
			return;
		}
		stringChooserValuePopup.label = _receiverTargetName;
		if (_receiver.IsStringChooserJSONParam(_receiverTargetName))
		{
			List<string> stringChooserJSONParamChoices = _receiver.GetStringChooserJSONParamChoices(_receiverTargetName);
			stringChooserValuePopup.numPopupValues = stringChooserJSONParamChoices.Count;
			for (int i = 0; i < stringChooserJSONParamChoices.Count; i++)
			{
				stringChooserValuePopup.setPopupValue(i, stringChooserJSONParamChoices[i]);
			}
		}
	}

	protected override void SyncFromReceiverTarget()
	{
		base.SyncFromReceiverTarget();
		if (_receiver != null && _receiverTargetName != null)
		{
			actionType = _receiver.GetParamOrActionType(_receiverTargetName);
			switch (actionType)
			{
			case JSONStorable.Type.Float:
				if (floatValueSlider != null)
				{
					floatValueSlider.minValue = paramMin;
					floatValueSlider.maxValue = paramMax;
				}
				if (floatValueDynamicSlider != null)
				{
					floatValueDynamicSlider.rangeAdjustEnabled = !paramContrained;
					floatValueDynamicSlider.defaultValue = paramDefault;
					if (receiverTargetFloat != null)
					{
						floatValueDynamicSlider.label = receiverTargetFloat.name;
					}
				}
				if (secondTimerPointValueSlider != null)
				{
					secondTimerPointValueSlider.minValue = paramMin;
					secondTimerPointValueSlider.maxValue = paramMax;
				}
				if (secondTimerPointValueDynamicSlider != null)
				{
					secondTimerPointValueDynamicSlider.rangeAdjustEnabled = !paramContrained;
					secondTimerPointValueDynamicSlider.defaultValue = paramDefault;
				}
				break;
			case JSONStorable.Type.StringChooser:
				SyncStringChooserPopup();
				break;
			}
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
		if (boolValueToggle != null)
		{
			if (_actionType == JSONStorable.Type.Bool)
			{
				boolValueToggle.gameObject.SetActive(true);
			}
			else
			{
				boolValueToggle.gameObject.SetActive(false);
			}
		}
		if (floatValueSlider != null)
		{
			if (_actionType == JSONStorable.Type.Float)
			{
				floatValueSlider.transform.parent.gameObject.SetActive(true);
			}
			else
			{
				floatValueSlider.transform.parent.gameObject.SetActive(false);
			}
		}
		if (useTimerToggle != null)
		{
			useTimerToggle.gameObject.SetActive(_actionType == JSONStorable.Type.Float);
		}
		if (timerLengthSlider != null)
		{
			timerLengthSlider.transform.parent.gameObject.SetActive(_actionType == JSONStorable.Type.Float);
		}
		if (timerTypePopup != null)
		{
			timerTypePopup.gameObject.SetActive(_actionType == JSONStorable.Type.Float);
		}
		if (curveContainer != null)
		{
			curveContainer.gameObject.SetActive(_actionType == JSONStorable.Type.Float);
		}
		if (useSecondTimerPointToggle != null)
		{
			useSecondTimerPointToggle.gameObject.SetActive(_actionType == JSONStorable.Type.Float);
		}
		if (secondTimerPointValueSlider != null)
		{
			secondTimerPointValueSlider.transform.parent.gameObject.SetActive(_actionType == JSONStorable.Type.Float);
		}
		if (secondTimerPointCurveLocationSlider != null)
		{
			secondTimerPointCurveLocationSlider.transform.parent.gameObject.SetActive(_actionType == JSONStorable.Type.Float);
		}
		if (stringValueField != null)
		{
			if (_actionType == JSONStorable.Type.String || _actionType == JSONStorable.Type.Url)
			{
				stringValueField.gameObject.SetActive(true);
			}
			else
			{
				stringValueField.gameObject.SetActive(false);
			}
		}
		if (stringChooserValuePopup != null)
		{
			if (_actionType == JSONStorable.Type.StringChooser)
			{
				stringChooserValuePopup.gameObject.SetActive(true);
			}
			else
			{
				stringChooserValuePopup.gameObject.SetActive(false);
			}
		}
		if (colorPickerContainer != null)
		{
			if (_actionType == JSONStorable.Type.Color)
			{
				colorPickerContainer.gameObject.SetActive(true);
			}
			else
			{
				colorPickerContainer.gameObject.SetActive(false);
			}
		}
		if (audioClipPopupsContainer != null)
		{
			if (_actionType == JSONStorable.Type.AudioClipAction)
			{
				audioClipPopupsContainer.gameObject.SetActive(true);
			}
			else
			{
				audioClipPopupsContainer.gameObject.SetActive(false);
			}
		}
		if (stringChooserActionPopup != null)
		{
			stringChooserActionPopup.gameObject.SetActive(_actionType == JSONStorable.Type.StringChooserAction);
		}
		if (sceneFilePathText != null)
		{
			if (_actionType == JSONStorable.Type.SceneFilePathAction)
			{
				sceneFilePathText.gameObject.SetActive(true);
			}
			else
			{
				sceneFilePathText.gameObject.SetActive(false);
			}
		}
		if (presetFilePathText != null)
		{
			if (_actionType == JSONStorable.Type.PresetFilePathAction)
			{
				presetFilePathText.gameObject.SetActive(true);
			}
			else
			{
				presetFilePathText.gameObject.SetActive(false);
			}
		}
		if (chooseSceneFilePathButton != null)
		{
			if (_actionType == JSONStorable.Type.SceneFilePathAction)
			{
				chooseSceneFilePathButton.gameObject.SetActive(true);
			}
			else
			{
				chooseSceneFilePathButton.gameObject.SetActive(false);
			}
		}
		if (choosePresetFilePathButton != null)
		{
			if (_actionType == JSONStorable.Type.PresetFilePathAction)
			{
				choosePresetFilePathButton.gameObject.SetActive(true);
			}
			else
			{
				choosePresetFilePathButton.gameObject.SetActive(false);
			}
		}
	}

	public void SetAudioClipType(string type)
	{
		try
		{
			audioClipType = (AudioClipType)Enum.Parse(typeof(AudioClipType), type);
		}
		catch (ArgumentException)
		{
			Debug.LogError("Attempted to TriggerActionDiscrete audioClipType to " + type + " which is not a valid type");
		}
	}

	protected void SetClipCategoryPopupValues()
	{
		if (!(audioClipCategoryPopup != null))
		{
			return;
		}
		List<string> list = null;
		if (_audioClipType == AudioClipType.Embedded)
		{
			if (EmbeddedAudioClipManager.singleton != null)
			{
				list = EmbeddedAudioClipManager.singleton.GetCategories();
			}
		}
		else if (_audioClipType == AudioClipType.URL && URLAudioClipManager.singleton != null)
		{
			list = URLAudioClipManager.singleton.GetCategories();
		}
		if (list != null)
		{
			audioClipCategoryPopup.numPopupValues = list.Count + 1;
			int num = 0;
			audioClipCategoryPopup.setPopupValue(num, "None");
			num++;
			{
				foreach (string item in list)
				{
					audioClipCategoryPopup.setPopupValue(num, item);
					num++;
				}
				return;
			}
		}
		audioClipCategoryPopup.numPopupValues = 1;
		int index = 0;
		audioClipCategoryPopup.setPopupValue(index, "None");
	}

	public void SetAudioClipCategory(string cat)
	{
		audioClipCategory = cat;
	}

	protected void SetClipPopupValues()
	{
		if (!(audioClipPopup != null))
		{
			return;
		}
		audioClipPopup.useDifferentDisplayValues = true;
		List<NamedAudioClip> list = null;
		if (_audioClipCategory != null)
		{
			if (_audioClipType == AudioClipType.Embedded)
			{
				if (EmbeddedAudioClipManager.singleton != null)
				{
					list = EmbeddedAudioClipManager.singleton.GetCategoryClips(_audioClipCategory);
				}
			}
			else if (_audioClipType == AudioClipType.URL && URLAudioClipManager.singleton != null)
			{
				list = URLAudioClipManager.singleton.GetCategoryClips(_audioClipCategory);
			}
		}
		if (list != null)
		{
			audioClipPopup.numPopupValues = list.Count + 1;
			int num = 0;
			audioClipPopup.setPopupValue(num, "None");
			audioClipPopup.setDisplayPopupValue(num, "None");
			num++;
			{
				foreach (NamedAudioClip item in list)
				{
					audioClipPopup.setPopupValue(num, item.uid);
					audioClipPopup.setDisplayPopupValue(num, item.displayName);
					num++;
				}
				return;
			}
		}
		audioClipPopup.numPopupValues = 1;
		audioClipPopup.setPopupValue(0, "None");
	}

	public void SetAudioClip(string clipUID)
	{
		if (clipUID != "None")
		{
			if (_audioClipType == AudioClipType.Embedded)
			{
				if (EmbeddedAudioClipManager.singleton != null)
				{
					audioClip = EmbeddedAudioClipManager.singleton.GetClip(clipUID);
				}
			}
			else if (_audioClipType == AudioClipType.URL && URLAudioClipManager.singleton != null)
			{
				audioClip = URLAudioClipManager.singleton.GetClip(clipUID);
			}
		}
		else
		{
			audioClip = null;
		}
	}

	protected void SetStringChooserActionPopupValues()
	{
		if (!(base.receiver != null) || base.receiverTargetName == null)
		{
			return;
		}
		JSONStorableActionStringChooser stringChooserAction = base.receiver.GetStringChooserAction(base.receiverTargetName);
		if (stringChooserAction == null || !(stringChooserActionPopup != null))
		{
			return;
		}
		stringChooserActionPopup.useDifferentDisplayValues = true;
		List<string> choices = stringChooserAction.choices;
		List<string> displayChoices = stringChooserAction.displayChoices;
		if (choices != null)
		{
			stringChooserActionPopup.numPopupValues = choices.Count;
			for (int i = 0; i < choices.Count; i++)
			{
				stringChooserActionPopup.setPopupValue(i, choices[i]);
			}
		}
		else
		{
			stringChooserActionPopup.numPopupValues = 0;
		}
		if (displayChoices != null)
		{
			for (int j = 0; j < displayChoices.Count; j++)
			{
				stringChooserActionPopup.setDisplayPopupValue(j, displayChoices[j]);
			}
		}
	}

	protected void SetStringChooserActionChoice(string choice)
	{
		_stringChooserActionChoice = choice;
		AutoSetPreviewTextAndName();
	}

	protected void GetSceneFilePath()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.GetScenePathDialog(SetSceneFilePath);
		}
	}

	public void SetSceneFilePath(string path)
	{
		path = SuperController.singleton.NormalizeScenePath(path);
		sceneFilePath = path;
	}

	protected void GetPresetFilePath()
	{
		if (base.receiver != null && base.receiverTargetName != null)
		{
			base.receiver.GetPresetFilePathAction(base.receiverTargetName)?.Browse(SetPresetFilePath);
		}
	}

	public void SetPresetFilePath(string path)
	{
		path = SuperController.singleton.NormalizePath(path);
		presetFilePath = path;
	}

	public void SetFloatValue(float f)
	{
		_floatValue = f;
		SyncFloatCurve(true);
		AutoSetPreviewTextAndName();
	}

	protected void SyncFloatCurve(bool useCurrentCurveStart = false)
	{
		if (_useTimer && base.receiver != null && base.receiverTargetName != null)
		{
			ConfigureFloatCurve(startValue: (!useCurrentCurveStart || floatCurve == null) ? base.receiver.GetFloatParamValue(base.receiverTargetName) : _curveValueStart, type: _timerType, length: _timerLength, endValue: _floatValue, useSecondPoint: _useSecondTimerPoint, secondPointValue: _secondTimerPointValue, secondPointLocation: _secondTimerPointCurveLocation);
		}
	}

	protected void ConfigureFloatCurve(TimerType type, float length, float startValue, float endValue, bool useSecondPoint, float secondPointValue, float secondPointLocation)
	{
		if (floatCurve == null || _curveTimerType != type || _curveTimerLength != length || _curveValueStart != startValue || _curveValueEnd != endValue || _curveUseSecondPoint != useSecondPoint || _curveSecondPointValue != secondPointValue || _curveSecondPointLocation != secondPointLocation)
		{
			if (useSecondPoint && secondPointLocation <= 0.01f)
			{
				startValue = secondPointValue;
			}
			switch (type)
			{
			case TimerType.EaseInOut:
				floatCurve = AnimationCurve.EaseInOut(0f, startValue, length, endValue);
				break;
			case TimerType.Linear:
				floatCurve = AnimationCurve.Linear(0f, startValue, length, endValue);
				break;
			}
			if (useSecondPoint && secondPointLocation > 0.01f)
			{
				floatCurve.AddKey(length * secondPointLocation, secondPointValue);
			}
			_curveTimerType = type;
			_curveTimerLength = length;
			_curveValueStart = startValue;
			_curveValueEnd = endValue;
			_curveUseSecondPoint = useSecondPoint;
			_curveSecondPointValue = secondPointValue;
			_curveSecondPointLocation = secondPointLocation;
			SyncFloatCurveRender();
		}
	}

	protected void SyncFloatCurveRender()
	{
		if (base.detailPanelOpen && _useTimer && floatCurve != null && curveLineRenderer != null && curveLineRenderer.Points != null)
		{
			Vector2[] points = curveLineRenderer.Points;
			int num = points.Length;
			curveLineRenderer.RelativeSize = true;
			float num2 = 1f / (float)(num - 1);
			float num3 = paramMax - paramMin;
			float num4 = 1f;
			if (num3 != 0f)
			{
				num4 = 1f / num3;
			}
			float num5 = paramMin * num4;
			for (int i = 0; i < num; i++)
			{
				float num6 = num2 * (float)i;
				points[i].x = num2 * (float)i;
				points[i].y = Mathf.Clamp01(floatCurve.Evaluate(num6 * _curveTimerLength) * num4 - num5);
			}
		}
	}

	protected void SetUseTimer(bool b)
	{
		_useTimer = b;
	}

	protected void SetTimerLength(float f)
	{
		_timerLength = f;
		if (_timerLength > 0f)
		{
			_oneOverTimerLength = 1f / _timerLength;
		}
		SyncFloatCurve(true);
	}

	protected void SyncTimerTypePopup()
	{
		if (timerTypePopup != null)
		{
			timerTypePopup.currentValueNoCallback = _timerType.ToString();
		}
	}

	protected void SetTimerType(string type)
	{
		try
		{
			TimerType timerType = (TimerType)Enum.Parse(typeof(TimerType), type);
			_timerType = timerType;
			SyncFloatCurve(true);
		}
		catch (ArgumentException)
		{
			Debug.LogError("Attempt to set timer type to " + type + " which is not a valid timer type");
		}
	}

	protected void SetUseSecondTimerPoint(bool b)
	{
		_useSecondTimerPoint = b;
		SyncFloatCurve(true);
	}

	public void SetSecondTimerFloatValue(float f)
	{
		_secondTimerPointValue = f;
		SyncFloatCurve(true);
	}

	public void SetSecondTimerPointCurveLocation(float f)
	{
		_secondTimerPointCurveLocation = f;
		SyncFloatCurve(true);
	}

	protected void SetStringValue(string v)
	{
		stringValue = v;
	}

	protected void SetStringValueToField()
	{
		if (stringValueField != null)
		{
			stringValue = stringValueField.text;
		}
	}

	protected virtual void SetColorFromHSV(float h, float s, float v)
	{
		if (_HSVColorH != h || _HSVColorS != s || _HSVColorV != v)
		{
			_HSVColorH = h;
			_HSVColorS = s;
			_HSVColorV = v;
			if (colorPicker != null)
			{
				colorPicker.SetHSV(h, s, v, true);
			}
			AutoSetPreviewTextAndName();
		}
	}

	protected void SetStringChooserValue(string v)
	{
		stringChooserValue = v;
	}

	public override void Validate()
	{
		if (base.receiver != null && base.receiverTargetName != null)
		{
			switch (_actionType)
			{
			case JSONStorable.Type.Bool:
				if (!base.receiver.IsBoolJSONParam(base.receiverTargetName))
				{
					base.receiverTargetName = null;
				}
				break;
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
			case JSONStorable.Type.Action:
				if (!base.receiver.IsAction(base.receiverTargetName))
				{
					base.receiverTargetName = null;
				}
				break;
			case JSONStorable.Type.AudioClipAction:
				if (!base.receiver.IsAudioClipAction(base.receiverTargetName))
				{
					base.receiverTargetName = null;
				}
				if (_audioClip != null && _audioClip.destroyed)
				{
					audioClip = null;
				}
				break;
			case JSONStorable.Type.StringChooserAction:
				if (!base.receiver.IsStringChooserAction(base.receiverTargetName))
				{
					base.receiverTargetName = null;
				}
				break;
			case JSONStorable.Type.SceneFilePathAction:
				if (!base.receiver.IsSceneFilePathAction(base.receiverTargetName))
				{
					base.receiverTargetName = null;
				}
				break;
			case JSONStorable.Type.PresetFilePathAction:
				if (!base.receiver.IsPresetFilePathAction(base.receiverTargetName))
				{
					base.receiverTargetName = null;
				}
				break;
			case JSONStorable.Type.String:
				if (!base.receiver.IsStringJSONParam(base.receiverTargetName))
				{
					base.receiverTargetName = null;
				}
				break;
			case JSONStorable.Type.Url:
				if (!base.receiver.IsUrlJSONParam(base.receiverTargetName))
				{
					base.receiverTargetName = null;
				}
				break;
			case JSONStorable.Type.StringChooser:
				if (!base.receiver.IsStringChooserJSONParam(base.receiverTargetName))
				{
					base.receiverTargetName = null;
				}
				break;
			}
		}
		base.Validate();
	}

	public void Trigger(bool reverse = false, bool force = false)
	{
		CheckMissingReceiver();
		if ((!base.enabled && !force) || !(base.receiver != null) || base.receiverTargetName == null)
		{
			return;
		}
		switch (_actionType)
		{
		case JSONStorable.Type.Bool:
			if (reverse)
			{
				base.receiver.SetBoolParamValue(base.receiverTargetName, !boolValue);
			}
			else
			{
				base.receiver.SetBoolParamValue(base.receiverTargetName, boolValue);
			}
			break;
		case JSONStorable.Type.Float:
			if (_useTimer && _timerLength > 0f)
			{
				SyncFloatCurve();
				if (reverse)
				{
					if (_useSecondTimerPoint)
					{
						base.receiver.SetFloatParamValue(base.receiverTargetName, _secondTimerPointValue);
					}
					else
					{
						base.receiver.SetFloatParamValue(base.receiverTargetName, floatValue);
					}
				}
				else
				{
					currentTimerTime = 0f;
					timerActive = true;
					handler.SetHasActiveTimer(this, true);
				}
			}
			else
			{
				base.receiver.SetFloatParamValue(base.receiverTargetName, floatValue);
			}
			break;
		case JSONStorable.Type.Color:
			triggerColor.H = _HSVColorH;
			triggerColor.S = _HSVColorS;
			triggerColor.V = _HSVColorV;
			base.receiver.SetColorParamValue(base.receiverTargetName, triggerColor);
			break;
		case JSONStorable.Type.Action:
			if (!reverse || doActionsInReverse)
			{
				base.receiver.CallAction(base.receiverTargetName);
			}
			break;
		case JSONStorable.Type.AudioClipAction:
			if (audioClip != null)
			{
				if (reverse)
				{
					base.receiver.CallAction("Stop");
				}
				else
				{
					base.receiver.CallAction(base.receiverTargetName, audioClip);
				}
			}
			break;
		case JSONStorable.Type.StringChooserAction:
			if (!reverse || doActionsInReverse)
			{
				base.receiver.CallStringChooserAction(base.receiverTargetName, _stringChooserActionChoice);
			}
			break;
		case JSONStorable.Type.SceneFilePathAction:
			if (sceneFilePath != string.Empty && !reverse)
			{
				base.receiver.CallAction(base.receiverTargetName, sceneFilePath);
			}
			break;
		case JSONStorable.Type.PresetFilePathAction:
			if (presetFilePath != string.Empty && !reverse)
			{
				base.receiver.CallPresetFileAction(base.receiverTargetName, presetFilePath);
			}
			break;
		case JSONStorable.Type.String:
			base.receiver.SetStringParamValue(base.receiverTargetName, stringValue);
			break;
		case JSONStorable.Type.Url:
			base.receiver.SetUrlParamValue(base.receiverTargetName, stringValue);
			break;
		case JSONStorable.Type.StringChooser:
			base.receiver.SetStringChooserParamValue(base.receiverTargetName, stringChooserValue);
			break;
		case JSONStorable.Type.Vector3:
			break;
		}
	}

	public void Update()
	{
		if (!timerActive)
		{
			return;
		}
		if (_receiver != null && _receiverTargetName != null)
		{
			currentTimerTime += Time.deltaTime;
			if (currentTimerTime < _timerLength)
			{
				base.receiver.SetFloatParamValue(base.receiverTargetName, Mathf.Clamp(floatCurve.Evaluate(currentTimerTime), paramMin, paramMax));
				return;
			}
			base.receiver.SetFloatParamValue(base.receiverTargetName, floatCurve.Evaluate(_timerLength));
			timerActive = false;
			handler.SetHasActiveTimer(this, false);
		}
		else
		{
			timerActive = false;
			handler.SetHasActiveTimer(this, false);
		}
	}

	public override void InitTriggerActionPanelUI()
	{
		CheckMissingReceiver();
		base.InitTriggerActionPanelUI();
		if (triggerActionPanel != null)
		{
			TriggerActionDiscreteUI component = triggerActionPanel.GetComponent<TriggerActionDiscreteUI>();
			if (component != null)
			{
				copyButton = component.copyButton;
				pasteButton = component.pasteButton;
				testButton = component.testButton;
				resetTestButton = component.resetTestButton;
				audioClipPopupsContainer = component.audioClipPopupsContainer;
				audioClipTypePopup = component.audioClipTypePopup;
				audioClipCategoryPopup = component.audioClipCategoryPopup;
				audioClipPopup = component.audioClipPopup;
				stringChooserActionPopup = component.stringChooserActionPopup;
				sceneFilePathText = component.sceneFilePathText;
				presetFilePathText = component.presetFilePathText;
				chooseSceneFilePathButton = component.chooseSceneFilePathButton;
				choosePresetFilePathButton = component.choosePresetFilePathButton;
				boolValueToggle = component.boolValueToggle;
				floatValueSlider = component.floatValueSlider;
				floatValueDynamicSlider = component.floatValueDynamicSlider;
				useTimerToggle = component.useTimerToggle;
				curveContainer = component.curveContainer;
				curveLineRenderer = component.curveLineRenderer;
				timerLengthSlider = component.timerLengthSlider;
				timerTypePopup = component.timerTypePopup;
				useSecondTimerPointToggle = component.useSecondTimerPointToggle;
				secondTimerPointValueSlider = component.secondTimerPointValueSlider;
				secondTimerPointValueDynamicSlider = component.secondTimerPointValueDynamicSlider;
				secondTimerPointCurveLocationSlider = component.secondTimerPointCurveLocationSlider;
				colorPickerContainer = component.colorPickerContainer;
				colorPicker = component.colorPicker;
				stringValueField = component.stringValueField;
				stringValueFieldAction = component.stringValueFieldAction;
				stringChooserValuePopup = component.stringChooserValuePopup;
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
		if (testButton != null)
		{
			testButton.onClick.AddListener(Test);
		}
		if (resetTestButton != null)
		{
			resetTestButton.interactable = false;
			resetTestButton.onClick.AddListener(ResetTest);
		}
		if (audioClipTypePopup != null)
		{
			audioClipTypePopup.numPopupValues = 2;
			audioClipTypePopup.setPopupValue(0, "Embedded");
			audioClipTypePopup.setPopupValue(1, "URL");
			audioClipTypePopup.currentValue = _audioClipType.ToString();
			UIPopup uIPopup = audioClipTypePopup;
			uIPopup.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup.onValueChangeHandlers, new UIPopup.OnValueChange(SetAudioClipType));
		}
		if (audioClipCategoryPopup != null)
		{
			SetClipCategoryPopupValues();
			if (_audioClipCategory == null)
			{
				audioClipCategoryPopup.currentValue = "None";
			}
			else
			{
				audioClipCategoryPopup.currentValue = _audioClipCategory;
			}
			UIPopup uIPopup2 = audioClipCategoryPopup;
			uIPopup2.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Combine(uIPopup2.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetClipCategoryPopupValues));
			UIPopup uIPopup3 = audioClipCategoryPopup;
			uIPopup3.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup3.onValueChangeHandlers, new UIPopup.OnValueChange(SetAudioClipCategory));
		}
		if (audioClipPopup != null)
		{
			SetClipPopupValues();
			if (_audioClip == null)
			{
				audioClipPopup.currentValue = "None";
			}
			else
			{
				audioClipPopup.currentValue = _audioClip.uid;
			}
			UIPopup uIPopup4 = audioClipPopup;
			uIPopup4.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Combine(uIPopup4.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetClipPopupValues));
			UIPopup uIPopup5 = audioClipPopup;
			uIPopup5.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup5.onValueChangeHandlers, new UIPopup.OnValueChange(SetAudioClip));
		}
		if (stringChooserActionPopup != null)
		{
			SetStringChooserActionPopupValues();
			stringChooserActionPopup.currentValue = _stringChooserActionChoice;
			UIPopup uIPopup6 = stringChooserActionPopup;
			uIPopup6.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Combine(uIPopup6.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetStringChooserActionPopupValues));
			UIPopup uIPopup7 = stringChooserActionPopup;
			uIPopup7.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup7.onValueChangeHandlers, new UIPopup.OnValueChange(SetStringChooserActionChoice));
		}
		if (chooseSceneFilePathButton != null)
		{
			chooseSceneFilePathButton.onClick.AddListener(GetSceneFilePath);
		}
		if (choosePresetFilePathButton != null)
		{
			choosePresetFilePathButton.onClick.AddListener(GetPresetFilePath);
		}
		if (sceneFilePathText != null)
		{
			sceneFilePathText.text = _sceneFilePath;
		}
		if (presetFilePathText != null)
		{
			presetFilePathText.text = _presetFilePath;
		}
		if (boolValueToggle != null)
		{
			boolValueToggle.isOn = _boolValue;
			boolValueToggle.onValueChanged.AddListener(delegate
			{
				boolValue = boolValueToggle.isOn;
			});
		}
		if (floatValueSlider != null)
		{
			floatValueSlider.minValue = paramMin;
			if (floatValueSlider.minValue > _floatValue)
			{
				floatValueSlider.minValue = _floatValue;
			}
			floatValueSlider.maxValue = paramMax;
			if (floatValueSlider.maxValue < _floatValue)
			{
				floatValueSlider.maxValue = _floatValue;
			}
			floatValueSlider.value = _floatValue;
			floatValueSlider.onValueChanged.AddListener(SetFloatValue);
		}
		if (floatValueDynamicSlider != null)
		{
			floatValueDynamicSlider.rangeAdjustEnabled = !paramContrained;
			floatValueDynamicSlider.defaultValue = paramDefault;
			if (receiverTargetFloat != null)
			{
				floatValueDynamicSlider.label = receiverTargetFloat.name;
			}
		}
		if (useTimerToggle != null)
		{
			useTimerToggle.isOn = _useTimer;
			useTimerToggle.onValueChanged.AddListener(SetUseTimer);
		}
		if (timerLengthSlider != null)
		{
			if (_timerLength > timerLengthSlider.maxValue)
			{
				timerLengthSlider.maxValue = _timerLength;
			}
			timerLengthSlider.value = _timerLength;
			timerLengthSlider.onValueChanged.AddListener(SetTimerLength);
		}
		if (timerTypePopup != null)
		{
			timerTypePopup.currentValueNoCallback = _timerType.ToString();
			UIPopup uIPopup8 = timerTypePopup;
			uIPopup8.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup8.onValueChangeHandlers, new UIPopup.OnValueChange(SetTimerType));
		}
		if (useSecondTimerPointToggle != null)
		{
			useSecondTimerPointToggle.isOn = _useSecondTimerPoint;
			useSecondTimerPointToggle.onValueChanged.AddListener(SetUseSecondTimerPoint);
		}
		if (secondTimerPointValueSlider != null)
		{
			secondTimerPointValueSlider.minValue = paramMin;
			if (secondTimerPointValueSlider.minValue > _secondTimerPointValue)
			{
				secondTimerPointValueSlider.minValue = _secondTimerPointValue;
			}
			secondTimerPointValueSlider.maxValue = paramMax;
			if (secondTimerPointValueSlider.maxValue < _secondTimerPointValue)
			{
				secondTimerPointValueSlider.maxValue = _secondTimerPointValue;
			}
			secondTimerPointValueSlider.value = _secondTimerPointValue;
			secondTimerPointValueSlider.onValueChanged.AddListener(SetSecondTimerFloatValue);
		}
		if (secondTimerPointValueDynamicSlider != null)
		{
			secondTimerPointValueDynamicSlider.rangeAdjustEnabled = !paramContrained;
			secondTimerPointValueDynamicSlider.defaultValue = paramDefault;
		}
		if (secondTimerPointCurveLocationSlider != null)
		{
			secondTimerPointCurveLocationSlider.value = _secondTimerPointCurveLocation;
			secondTimerPointCurveLocationSlider.onValueChanged.AddListener(SetSecondTimerPointCurveLocation);
		}
		SyncFloatCurve();
		SyncFloatCurveRender();
		if (colorPicker != null)
		{
			colorPicker.SetHSV(_HSVColorH, _HSVColorS, _HSVColorV);
			HSVColorPicker hSVColorPicker = colorPicker;
			hSVColorPicker.onHSVColorChangedHandlers = (HSVColorPicker.OnHSVColorChanged)Delegate.Combine(hSVColorPicker.onHSVColorChangedHandlers, new HSVColorPicker.OnHSVColorChanged(SetColorFromHSV));
		}
		if (stringValueField != null)
		{
			stringValueField.text = _stringValue;
			stringValueField.onEndEdit.AddListener(SetStringValue);
		}
		if (stringValueFieldAction != null)
		{
			InputFieldAction inputFieldAction = stringValueFieldAction;
			inputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(inputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetStringValueToField));
		}
		if (stringChooserValuePopup != null)
		{
			SyncStringChooserPopup();
			stringChooserValuePopup.currentValue = _stringChooserValue;
			UIPopup uIPopup9 = stringChooserValuePopup;
			uIPopup9.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup9.onValueChangeHandlers, new UIPopup.OnValueChange(SetStringChooserValue));
		}
		SyncType();
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
		if (testButton != null)
		{
			testButton.onClick.RemoveListener(Test);
		}
		if (resetTestButton != null)
		{
			resetTestButton.onClick.RemoveListener(ResetTest);
		}
		if (audioClipTypePopup != null)
		{
			UIPopup uIPopup = audioClipTypePopup;
			uIPopup.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Remove(uIPopup.onValueChangeHandlers, new UIPopup.OnValueChange(SetAudioClipType));
		}
		if (audioClipCategoryPopup != null)
		{
			UIPopup uIPopup2 = audioClipCategoryPopup;
			uIPopup2.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Remove(uIPopup2.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetClipCategoryPopupValues));
			UIPopup uIPopup3 = audioClipCategoryPopup;
			uIPopup3.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Remove(uIPopup3.onValueChangeHandlers, new UIPopup.OnValueChange(SetAudioClipCategory));
		}
		if (audioClipPopup != null)
		{
			UIPopup uIPopup4 = audioClipPopup;
			uIPopup4.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Remove(uIPopup4.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetClipPopupValues));
			UIPopup uIPopup5 = audioClipPopup;
			uIPopup5.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Remove(uIPopup5.onValueChangeHandlers, new UIPopup.OnValueChange(SetAudioClip));
		}
		if (stringChooserActionPopup != null)
		{
			UIPopup uIPopup6 = stringChooserActionPopup;
			uIPopup6.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Remove(uIPopup6.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetStringChooserActionPopupValues));
			UIPopup uIPopup7 = stringChooserActionPopup;
			uIPopup7.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Remove(uIPopup7.onValueChangeHandlers, new UIPopup.OnValueChange(SetStringChooserActionChoice));
		}
		if (chooseSceneFilePathButton != null)
		{
			chooseSceneFilePathButton.onClick.RemoveListener(GetSceneFilePath);
		}
		if (choosePresetFilePathButton != null)
		{
			choosePresetFilePathButton.onClick.RemoveListener(GetPresetFilePath);
		}
		if (boolValueToggle != null)
		{
			boolValueToggle.onValueChanged.RemoveAllListeners();
		}
		if (floatValueSlider != null)
		{
			floatValueSlider.onValueChanged.RemoveListener(SetFloatValue);
		}
		if (useTimerToggle != null)
		{
			useTimerToggle.onValueChanged.RemoveListener(SetUseTimer);
		}
		if (timerLengthSlider != null)
		{
			timerLengthSlider.onValueChanged.RemoveListener(SetTimerLength);
		}
		if (timerTypePopup != null)
		{
			UIPopup uIPopup8 = timerTypePopup;
			uIPopup8.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Remove(uIPopup8.onValueChangeHandlers, new UIPopup.OnValueChange(SetTimerType));
		}
		if (useSecondTimerPointToggle != null)
		{
			useSecondTimerPointToggle.onValueChanged.RemoveListener(SetUseSecondTimerPoint);
		}
		if (secondTimerPointValueSlider != null)
		{
			secondTimerPointValueSlider.onValueChanged.RemoveListener(SetSecondTimerFloatValue);
		}
		if (secondTimerPointCurveLocationSlider != null)
		{
			secondTimerPointCurveLocationSlider.onValueChanged.RemoveListener(SetSecondTimerPointCurveLocation);
		}
		if (colorPicker != null)
		{
			HSVColorPicker hSVColorPicker = colorPicker;
			hSVColorPicker.onHSVColorChangedHandlers = (HSVColorPicker.OnHSVColorChanged)Delegate.Remove(hSVColorPicker.onHSVColorChangedHandlers, new HSVColorPicker.OnHSVColorChanged(SetColorFromHSV));
		}
		if (stringValueField != null)
		{
			stringValueField.onEndEdit.RemoveListener(SetStringValue);
		}
		if (stringValueFieldAction != null)
		{
			InputFieldAction inputFieldAction = stringValueFieldAction;
			inputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Remove(inputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetStringValueToField));
		}
		if (stringChooserValuePopup != null)
		{
			UIPopup uIPopup9 = stringChooserValuePopup;
			uIPopup9.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Remove(uIPopup9.onValueChangeHandlers, new UIPopup.OnValueChange(SetStringChooserValue));
		}
	}
}
