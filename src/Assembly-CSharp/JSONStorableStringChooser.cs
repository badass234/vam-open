using System;
using System.Collections.Generic;
using SimpleJSON;

public class JSONStorableStringChooser : JSONStorableParam
{
	public delegate void PopupOpenCallback();

	public delegate void SetStringCallback(string val);

	public delegate void SetJSONStringCallback(JSONStorableStringChooser js);

	public bool representsAtomUid;

	protected List<string> _choices;

	protected bool useDifferentDisplayChoices;

	protected List<string> _displayChoices;

	public string defaultVal;

	protected bool _valueSetFromUI;

	protected string _val;

	public PopupOpenCallback popupOpenCallback;

	public SetStringCallback setCallbackFunction;

	public SetJSONStringCallback setJSONCallbackFunction;

	protected UIPopup _popup;

	protected UIPopup _popupAlt;

	protected ToggleGroupValue _toggleGroupValue;

	protected ToggleGroupValue _toggleGroupValueAlt;

	protected string _label;

	public List<string> choices
	{
		get
		{
			return _choices;
		}
		set
		{
			if (_choices != value)
			{
				_choices = value;
				if (!useDifferentDisplayChoices)
				{
					_displayChoices = _choices;
				}
				SyncPopup();
				SyncPopupAlt();
			}
		}
	}

	public List<string> displayChoices
	{
		get
		{
			return _displayChoices;
		}
		set
		{
			if (_displayChoices != value)
			{
				_displayChoices = value;
				useDifferentDisplayChoices = true;
				SyncPopup();
				SyncPopupAlt();
			}
		}
	}

	public bool valueSetFromUI => _valueSetFromUI;

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

	public UIPopup popup
	{
		get
		{
			return _popup;
		}
		set
		{
			if (!(_popup != value))
			{
				return;
			}
			if (_popup != null)
			{
				UIPopup uIPopup = _popup;
				uIPopup.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Remove(uIPopup.onValueChangeHandlers, new UIPopup.OnValueChange(SetValFromUI));
				UIPopup uIPopup2 = _popup;
				uIPopup2.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Remove(uIPopup2.onOpenPopupHandlers, new UIPopup.OnOpenPopup(PopupOpen));
			}
			_popup = value;
			if (_popup != null)
			{
				if (_label != null)
				{
					_popup.label = _label;
				}
				SyncPopup();
				_popup.currentValue = _val;
				UIPopup uIPopup3 = _popup;
				uIPopup3.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup3.onValueChangeHandlers, new UIPopup.OnValueChange(SetValFromUI));
				UIPopup uIPopup4 = _popup;
				uIPopup4.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Combine(uIPopup4.onOpenPopupHandlers, new UIPopup.OnOpenPopup(PopupOpen));
			}
		}
	}

	public UIPopup popupAlt
	{
		get
		{
			return _popupAlt;
		}
		set
		{
			if (!(_popupAlt != value))
			{
				return;
			}
			if (_popupAlt != null)
			{
				UIPopup uIPopup = _popupAlt;
				uIPopup.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Remove(uIPopup.onValueChangeHandlers, new UIPopup.OnValueChange(SetValFromUI));
				UIPopup uIPopup2 = _popupAlt;
				uIPopup2.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Remove(uIPopup2.onOpenPopupHandlers, new UIPopup.OnOpenPopup(PopupOpen));
			}
			_popupAlt = value;
			if (_popupAlt != null)
			{
				if (_label != null)
				{
					_popupAlt.label = _label;
				}
				SyncPopupAlt();
				_popupAlt.currentValue = _val;
				UIPopup uIPopup3 = _popupAlt;
				uIPopup3.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup3.onValueChangeHandlers, new UIPopup.OnValueChange(SetValFromUI));
				UIPopup uIPopup4 = _popupAlt;
				uIPopup4.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Combine(uIPopup4.onOpenPopupHandlers, new UIPopup.OnOpenPopup(PopupOpen));
			}
		}
	}

	public ToggleGroupValue toggleGroupValue
	{
		get
		{
			return _toggleGroupValue;
		}
		set
		{
			if (_toggleGroupValue != value)
			{
				if (_toggleGroupValue != null)
				{
					ToggleGroupValue obj = _toggleGroupValue;
					obj.onToggleChangedHandlers = (ToggleGroupValue.OnToggleChanged)Delegate.Remove(obj.onToggleChangedHandlers, new ToggleGroupValue.OnToggleChanged(SetValFromUI));
				}
				_toggleGroupValue = value;
				if (_toggleGroupValue != null)
				{
					_toggleGroupValue.activeToggleNameNoCallback = _val;
					ToggleGroupValue obj2 = _toggleGroupValue;
					obj2.onToggleChangedHandlers = (ToggleGroupValue.OnToggleChanged)Delegate.Combine(obj2.onToggleChangedHandlers, new ToggleGroupValue.OnToggleChanged(SetValFromUI));
				}
			}
		}
	}

	public ToggleGroupValue toggleGroupValueAlt
	{
		get
		{
			return _toggleGroupValueAlt;
		}
		set
		{
			if (_toggleGroupValueAlt != value)
			{
				if (_toggleGroupValueAlt != null)
				{
					ToggleGroupValue obj = _toggleGroupValueAlt;
					obj.onToggleChangedHandlers = (ToggleGroupValue.OnToggleChanged)Delegate.Remove(obj.onToggleChangedHandlers, new ToggleGroupValue.OnToggleChanged(SetValFromUI));
				}
				_toggleGroupValueAlt = value;
				if (_toggleGroupValueAlt != null)
				{
					_toggleGroupValueAlt.activeToggleNameNoCallback = _val;
					ToggleGroupValue obj2 = _toggleGroupValueAlt;
					obj2.onToggleChangedHandlers = (ToggleGroupValue.OnToggleChanged)Delegate.Combine(obj2.onToggleChangedHandlers, new ToggleGroupValue.OnToggleChanged(SetValFromUI));
				}
			}
		}
	}

	public string label
	{
		get
		{
			return _label;
		}
		set
		{
			if (!(_label != value))
			{
				return;
			}
			_label = value;
			if (_label != null)
			{
				if (_popup != null)
				{
					_popup.label = _label;
				}
				if (_popupAlt != null)
				{
					_popupAlt.label = _label;
				}
			}
		}
	}

	public JSONStorableStringChooser(string paramName, List<string> choicesList, string startingValue, string displayName)
	{
		type = JSONStorable.Type.StringChooser;
		name = paramName;
		_choices = choicesList;
		useDifferentDisplayChoices = false;
		_displayChoices = choicesList;
		defaultVal = startingValue;
		val = startingValue;
		label = displayName;
		setCallbackFunction = null;
		setJSONCallbackFunction = null;
	}

	public JSONStorableStringChooser(string paramName, List<string> choicesList, string startingValue, string displayName, SetStringCallback callback)
	{
		type = JSONStorable.Type.StringChooser;
		name = paramName;
		_choices = choicesList;
		useDifferentDisplayChoices = false;
		_displayChoices = choicesList;
		defaultVal = startingValue;
		val = startingValue;
		label = displayName;
		setCallbackFunction = callback;
		setJSONCallbackFunction = null;
	}

	public JSONStorableStringChooser(string paramName, List<string> choicesList, string startingValue, string displayName, SetJSONStringCallback callback)
	{
		type = JSONStorable.Type.StringChooser;
		name = paramName;
		_choices = choicesList;
		useDifferentDisplayChoices = false;
		_displayChoices = choicesList;
		defaultVal = startingValue;
		val = startingValue;
		label = displayName;
		setCallbackFunction = null;
		setJSONCallbackFunction = callback;
	}

	public JSONStorableStringChooser(string paramName, List<string> choicesList, List<string> displayChoicesList, string startingValue, string displayName)
	{
		type = JSONStorable.Type.StringChooser;
		name = paramName;
		_choices = choicesList;
		useDifferentDisplayChoices = true;
		_displayChoices = displayChoicesList;
		defaultVal = startingValue;
		val = startingValue;
		label = displayName;
		setCallbackFunction = null;
		setJSONCallbackFunction = null;
	}

	public JSONStorableStringChooser(string paramName, List<string> choicesList, List<string> displayChoicesList, string startingValue, string displayName, SetStringCallback callback)
	{
		type = JSONStorable.Type.StringChooser;
		name = paramName;
		_choices = choicesList;
		useDifferentDisplayChoices = true;
		_displayChoices = displayChoicesList;
		defaultVal = startingValue;
		val = startingValue;
		label = displayName;
		setCallbackFunction = callback;
		setJSONCallbackFunction = null;
	}

	public JSONStorableStringChooser(string paramName, List<string> choicesList, List<string> displayChoicesList, string startingValue, string displayName, SetJSONStringCallback callback)
	{
		type = JSONStorable.Type.StringChooser;
		name = paramName;
		_choices = choicesList;
		useDifferentDisplayChoices = true;
		_displayChoices = displayChoicesList;
		defaultVal = startingValue;
		val = startingValue;
		label = displayName;
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
		if (NeedsLateRestore(jc, restorePhysical, restoreAppearance))
		{
			if (jc[name] != null)
			{
				val = jc[name];
			}
			else if (setMissingToDefault)
			{
				val = defaultVal;
			}
		}
	}

	protected void SyncPopup()
	{
		if (!(_popup != null))
		{
			return;
		}
		_popup.useDifferentDisplayValues = true;
		if (_choices != null)
		{
			_popup.numPopupValues = _choices.Count;
			for (int i = 0; i < _choices.Count; i++)
			{
				_popup.setPopupValue(i, _choices[i]);
			}
		}
		else
		{
			_popup.numPopupValues = 0;
		}
		if (_displayChoices != null)
		{
			for (int j = 0; j < _displayChoices.Count; j++)
			{
				_popup.setDisplayPopupValue(j, _displayChoices[j]);
			}
		}
	}

	protected void SyncPopupAlt()
	{
		if (!(_popupAlt != null))
		{
			return;
		}
		_popupAlt.useDifferentDisplayValues = true;
		if (_choices != null)
		{
			_popupAlt.numPopupValues = _choices.Count;
			for (int i = 0; i < _choices.Count; i++)
			{
				_popupAlt.setPopupValue(i, _choices[i]);
			}
		}
		else
		{
			_popupAlt.numPopupValues = 0;
		}
		if (_displayChoices != null)
		{
			for (int j = 0; j < _displayChoices.Count; j++)
			{
				_popupAlt.setDisplayPopupValue(j, _displayChoices[j]);
			}
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
		if (_popup != null)
		{
			_popup.currentValueNoCallback = _val;
		}
		if (_popupAlt != null)
		{
			_popupAlt.currentValueNoCallback = _val;
		}
		if (_toggleGroupValue != null)
		{
			_toggleGroupValue.activeToggleNameNoCallback = _val;
		}
		if (_toggleGroupValueAlt != null)
		{
			_toggleGroupValueAlt.activeToggleNameNoCallback = _val;
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

	protected void SetValFromUI(string value)
	{
		_valueSetFromUI = true;
		val = value;
		_valueSetFromUI = false;
	}

	public void PopupOpen()
	{
		if (popupOpenCallback != null)
		{
			popupOpenCallback();
		}
	}

	public void RegisterPopup(UIPopup p, bool isAlt = false)
	{
		if (isAlt)
		{
			popupAlt = p;
		}
		else
		{
			popup = p;
		}
	}

	public void RegisterToggleGroupValue(ToggleGroupValue tgv, bool isAlt = false)
	{
		if (isAlt)
		{
			toggleGroupValueAlt = tgv;
		}
		else
		{
			toggleGroupValue = tgv;
		}
	}
}
