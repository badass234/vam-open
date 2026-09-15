using System.Collections.Generic;
using UnityEngine.UI;

public class JSONStorableActionStringChooser
{
	public delegate void StringChoiceActionCallback(string choice);

	protected JSONStorableStringChooser _chooser;

	public string name;

	public StringChoiceActionCallback actionCallback;

	public JSONStorable storable;

	protected UIDynamicButton _dynamicButton;

	protected UIDynamicButton _dynamicButtonAlt;

	protected Button _button;

	protected Button _buttonAlt;

	public List<string> choices
	{
		get
		{
			if (_chooser != null)
			{
				return _chooser.choices;
			}
			return null;
		}
	}

	public List<string> displayChoices
	{
		get
		{
			if (_chooser != null)
			{
				return _chooser.displayChoices;
			}
			return null;
		}
	}

	public string choice
	{
		get
		{
			if (_chooser != null)
			{
				return _chooser.val;
			}
			return null;
		}
	}

	public UIDynamicButton dynamicButton
	{
		get
		{
			return _dynamicButton;
		}
		set
		{
			if (_dynamicButton != value)
			{
				_dynamicButton = value;
				if (_dynamicButton != null)
				{
					button = _dynamicButton.button;
				}
				else
				{
					button = null;
				}
			}
		}
	}

	public UIDynamicButton dynamicButtonAlt
	{
		get
		{
			return _dynamicButtonAlt;
		}
		set
		{
			if (_dynamicButtonAlt != value)
			{
				_dynamicButtonAlt = value;
				if (_dynamicButtonAlt != null)
				{
					buttonAlt = _dynamicButtonAlt.button;
				}
				else
				{
					buttonAlt = null;
				}
			}
		}
	}

	public Button button
	{
		get
		{
			return _button;
		}
		set
		{
			if (_button != value)
			{
				if (_button != null)
				{
					_button.onClick.RemoveListener(DoCallback);
				}
				_button = value;
				if (_button != null)
				{
					_button.onClick.AddListener(DoCallback);
				}
			}
		}
	}

	public Button buttonAlt
	{
		get
		{
			return _buttonAlt;
		}
		set
		{
			if (_buttonAlt != value)
			{
				if (_buttonAlt != null)
				{
					_buttonAlt.onClick.RemoveListener(DoCallback);
				}
				_buttonAlt = value;
				if (_buttonAlt != null)
				{
					_buttonAlt.onClick.AddListener(DoCallback);
				}
			}
		}
	}

	public JSONStorableActionStringChooser(string n, StringChoiceActionCallback callback, JSONStorableStringChooser chooser)
	{
		name = n;
		actionCallback = callback;
		_chooser = chooser;
	}

	protected void DoCallback()
	{
		actionCallback(choice);
	}

	public void RegisterButton(UIDynamicButton b, bool isAlt = false)
	{
		if (isAlt)
		{
			dynamicButtonAlt = b;
		}
		else
		{
			dynamicButton = b;
		}
	}

	public void RegisterButton(Button b, bool isAlt = false)
	{
		if (isAlt)
		{
			buttonAlt = b;
		}
		else
		{
			button = b;
		}
	}
}
