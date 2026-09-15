using UnityEngine.UI;

public class JSONStorableAction
{
	public delegate void ActionCallback();

	public string name;

	public ActionCallback actionCallback;

	public JSONStorable storable;

	protected UIDynamicButton _dynamicButton;

	protected UIDynamicButton _dynamicButtonAlt;

	protected Button _button;

	protected Button _buttonAlt;

	protected bool _interactable = true;

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
					_button.interactable = _interactable;
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
					_buttonAlt.interactable = _interactable;
					_buttonAlt.onClick.AddListener(DoCallback);
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
				if (_button != null)
				{
					_button.interactable = _interactable;
				}
				if (_buttonAlt != null)
				{
					_buttonAlt.interactable = _interactable;
				}
			}
		}
	}

	public JSONStorableAction(string n, ActionCallback callback)
	{
		name = n;
		actionCallback = callback;
	}

	protected void DoCallback()
	{
		actionCallback();
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
