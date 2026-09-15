using System;
using SimpleJSON;
using UnityEngine.UI;

public class JSONStorableString : JSONStorableParam
{
	public delegate void SetJSONStringCallback(JSONStorableString js);

	public delegate void SetStringCallback(string val);

	public bool representsAtomUid;

	public string defaultVal;

	protected string _val;

	public SetStringCallback setCallbackFunction;

	public SetJSONStringCallback setJSONCallbackFunction;

	protected Text _text;

	protected Text _textAlt;

	protected UIDynamicTextField _dynamicText;

	protected UIDynamicTextField _dynamicTextAlt;

	public bool disableOnEndEdit;

	public bool enableOnChange;

	protected bool _interactable = true;

	protected InputField _inputField;

	protected InputField _inputFieldAlt;

	protected InputFieldAction _inputFieldAction;

	protected InputFieldAction _inputFieldActionAlt;

	protected Button _setValToInputFieldButton;

	protected Button _setValToInputFieldButtonAlt;

	protected Button _clearValButton;

	protected Button _clearValButtonAlt;

	public virtual string val
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

	public string valNoCallback
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

	public Text text
	{
		get
		{
			return _text;
		}
		set
		{
			if (_text != value)
			{
				_text = value;
				if (_text != null)
				{
					_text.text = _val;
				}
			}
		}
	}

	public Text textAlt
	{
		get
		{
			return _textAlt;
		}
		set
		{
			if (_textAlt != value)
			{
				_textAlt = value;
				if (_textAlt != null)
				{
					_textAlt.text = _val;
				}
			}
		}
	}

	public UIDynamicTextField dynamicText
	{
		get
		{
			return _dynamicText;
		}
		set
		{
			if (_dynamicText != value)
			{
				_dynamicText = value;
				if (_dynamicText != null)
				{
					text = dynamicText.UItext;
				}
			}
		}
	}

	public UIDynamicTextField dynamicTextAlt
	{
		get
		{
			return _dynamicTextAlt;
		}
		set
		{
			if (_dynamicTextAlt != value)
			{
				_dynamicTextAlt = value;
				if (_dynamicTextAlt != null)
				{
					textAlt = dynamicTextAlt.UItext;
				}
			}
		}
	}

	public bool interactable
	{
		get
		{
			return _interactable;
		}
		set
		{
			if (_interactable != value)
			{
				_interactable = value;
				if (_inputField != null)
				{
					_inputField.interactable = _interactable;
				}
				if (_inputFieldAlt != null)
				{
					_inputFieldAlt.interactable = _interactable;
				}
			}
		}
	}

	public InputField inputField
	{
		get
		{
			return _inputField;
		}
		set
		{
			if (!(_inputField != value))
			{
				return;
			}
			if (_inputField != null)
			{
				if (!disableOnEndEdit)
				{
					_inputField.onEndEdit.RemoveListener(SetVal);
				}
				if (enableOnChange)
				{
					_inputField.onValueChanged.RemoveListener(SetVal);
				}
			}
			_inputField = value;
			if (_inputField != null)
			{
				_inputField.text = _val;
				_inputField.interactable = _interactable;
				if (!disableOnEndEdit)
				{
					_inputField.onEndEdit.AddListener(SetVal);
				}
				if (enableOnChange)
				{
					_inputField.onValueChanged.AddListener(SetVal);
				}
			}
		}
	}

	public InputField inputFieldAlt
	{
		get
		{
			return _inputFieldAlt;
		}
		set
		{
			if (!(_inputFieldAlt != value))
			{
				return;
			}
			if (_inputFieldAlt != null)
			{
				if (!disableOnEndEdit)
				{
					_inputFieldAlt.onEndEdit.RemoveListener(SetVal);
				}
				if (enableOnChange)
				{
					_inputFieldAlt.onValueChanged.RemoveListener(SetVal);
				}
			}
			_inputFieldAlt = value;
			if (_inputFieldAlt != null)
			{
				_inputFieldAlt.text = _val;
				_inputFieldAlt.interactable = _interactable;
				if (!disableOnEndEdit)
				{
					_inputFieldAlt.onEndEdit.AddListener(SetVal);
				}
				if (enableOnChange)
				{
					_inputFieldAlt.onValueChanged.AddListener(SetVal);
				}
			}
		}
	}

	public InputFieldAction inputFieldAction
	{
		get
		{
			return _inputFieldAction;
		}
		set
		{
			if (_inputFieldAction != value)
			{
				if (_inputFieldAction != null)
				{
					InputFieldAction obj = _inputFieldAction;
					obj.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Remove(obj.onSubmitHandlers, new InputFieldAction.OnSubmit(SetValToInputField));
				}
				_inputFieldAction = value;
				if (_inputFieldAction != null)
				{
					InputFieldAction obj2 = _inputFieldAction;
					obj2.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(obj2.onSubmitHandlers, new InputFieldAction.OnSubmit(SetValToInputField));
				}
			}
		}
	}

	public InputFieldAction inputFieldActionAlt
	{
		get
		{
			return _inputFieldActionAlt;
		}
		set
		{
			if (_inputFieldActionAlt != value)
			{
				if (_inputFieldActionAlt != null)
				{
					InputFieldAction obj = _inputFieldActionAlt;
					obj.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Remove(obj.onSubmitHandlers, new InputFieldAction.OnSubmit(SetValToInputFieldAlt));
				}
				_inputFieldActionAlt = value;
				if (_inputFieldActionAlt != null)
				{
					InputFieldAction obj2 = _inputFieldActionAlt;
					obj2.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(obj2.onSubmitHandlers, new InputFieldAction.OnSubmit(SetValToInputFieldAlt));
				}
			}
		}
	}

	public Button setValToInputFieldButton
	{
		get
		{
			return _setValToInputFieldButton;
		}
		set
		{
			if (_setValToInputFieldButton != value)
			{
				if (_setValToInputFieldButton != null)
				{
					_setValToInputFieldButton.onClick.RemoveListener(SetValToInputField);
				}
				_setValToInputFieldButton = value;
				if (_setValToInputFieldButton != null)
				{
					_setValToInputFieldButton.onClick.AddListener(SetValToInputField);
				}
			}
		}
	}

	public Button setValToInputFieldButtonAlt
	{
		get
		{
			return _setValToInputFieldButtonAlt;
		}
		set
		{
			if (_setValToInputFieldButtonAlt != value)
			{
				if (_setValToInputFieldButtonAlt != null)
				{
					_setValToInputFieldButtonAlt.onClick.RemoveListener(SetValToInputFieldAlt);
				}
				_setValToInputFieldButtonAlt = value;
				if (_setValToInputFieldButtonAlt != null)
				{
					_setValToInputFieldButtonAlt.onClick.AddListener(SetValToInputFieldAlt);
				}
			}
		}
	}

	public Button clearValButton
	{
		get
		{
			return _clearValButton;
		}
		set
		{
			if (_clearValButton != value)
			{
				if (_clearValButton != null)
				{
					_clearValButton.onClick.RemoveListener(ClearVal);
				}
				_clearValButton = value;
				if (_clearValButton != null)
				{
					_clearValButton.onClick.AddListener(ClearVal);
				}
			}
		}
	}

	public Button clearValButtonAlt
	{
		get
		{
			return _clearValButtonAlt;
		}
		set
		{
			if (_clearValButtonAlt != value)
			{
				if (_clearValButtonAlt != null)
				{
					_clearValButtonAlt.onClick.RemoveListener(ClearVal);
				}
				_clearValButtonAlt = value;
				if (_clearValButtonAlt != null)
				{
					_clearValButtonAlt.onClick.AddListener(ClearVal);
				}
			}
		}
	}

	public JSONStorableString(string paramName, string startingValue)
	{
		type = JSONStorable.Type.String;
		name = paramName;
		defaultVal = startingValue;
		val = startingValue;
		setCallbackFunction = null;
		setJSONCallbackFunction = null;
	}

	public JSONStorableString(string paramName, string startingValue, SetStringCallback callback)
	{
		type = JSONStorable.Type.String;
		name = paramName;
		defaultVal = startingValue;
		val = startingValue;
		setCallbackFunction = callback;
		setJSONCallbackFunction = null;
	}

	public JSONStorableString(string paramName, string startingValue, SetJSONStringCallback callback)
	{
		type = JSONStorable.Type.String;
		name = paramName;
		defaultVal = startingValue;
		val = startingValue;
		setCallbackFunction = null;
		setJSONCallbackFunction = callback;
	}

	public override bool StoreJSON(JSONClass jc, bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		bool flag = NeedsStore(jc, includePhysical, includeAppearance) && (forceStore || _val != defaultVal);
		if (flag)
		{
			if (representsAtomUid && storable != null)
			{
				string text = storable.AtomUidToStoreAtomUid(val);
				if (text == null)
				{
					return false;
				}
				jc[name] = text;
			}
			else
			{
				jc[name] = val;
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
		if (jc[name] != null)
		{
			if (representsAtomUid && storable != null)
			{
				val = storable.StoredAtomUidToAtomUid(jc[name]);
			}
			else
			{
				val = jc[name];
			}
		}
		else if (setMissingToDefault)
		{
			val = defaultVal;
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
			if (representsAtomUid && storable != null)
			{
				val = storable.StoredAtomUidToAtomUid(jc[name]);
			}
			else
			{
				val = jc[name];
			}
		}
		else if (setMissingToDefault)
		{
			val = defaultVal;
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

	protected void InternalSetVal(string s, bool doCallback = true)
	{
		if (!(_val != s))
		{
			return;
		}
		_val = s;
		if (_text != null)
		{
			_text.text = _val;
		}
		if (_textAlt != null)
		{
			_textAlt.text = _val;
		}
		if (_inputField != null)
		{
			_inputField.text = _val;
		}
		if (_inputFieldAlt != null)
		{
			_inputFieldAlt.text = _val;
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

	public void SetVal(string value)
	{
		val = value;
	}

	public void SetValToInputField()
	{
		if (_inputField != null)
		{
			val = _inputField.text;
		}
	}

	public void SetValToInputFieldAlt()
	{
		if (_inputFieldAlt != null)
		{
			val = _inputFieldAlt.text;
		}
	}

	public void ClearVal()
	{
		val = string.Empty;
	}

	public void RegisterText(Text t, bool isAlt = false)
	{
		if (isAlt)
		{
			textAlt = t;
		}
		else
		{
			text = t;
		}
	}

	public void RegisterInputField(InputField inf, bool isAlt = false)
	{
		if (isAlt)
		{
			inputFieldAlt = inf;
		}
		else
		{
			inputField = inf;
		}
	}

	public void RegisterInputFieldAction(InputFieldAction infa, bool isAlt = false)
	{
		if (isAlt)
		{
			inputFieldActionAlt = infa;
		}
		else
		{
			inputFieldAction = infa;
		}
	}

	public void RegisterSetValToInputFieldButton(Button but, bool isAlt = false)
	{
		if (isAlt)
		{
			setValToInputFieldButtonAlt = but;
		}
		else
		{
			setValToInputFieldButton = but;
		}
	}

	public void RegisterClearValButton(Button but, bool isAlt = false)
	{
		if (isAlt)
		{
			clearValButtonAlt = but;
		}
		else
		{
			clearValButton = but;
		}
	}
}
