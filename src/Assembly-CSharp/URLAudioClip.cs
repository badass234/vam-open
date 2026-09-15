using UnityEngine;
using UnityEngine.UI;

public class URLAudioClip : NamedAudioClip
{
	public string url;

	public bool fromRestore;

	public Toggle readyToggle;

	protected bool _ready;

	public Text sizeText;

	public bool error;

	protected string _errorMsg;

	protected bool _removed;

	public RectTransform UIpanel;

	public Button removeButton;

	public Slider loadProgressSlider;

	public bool ready
	{
		get
		{
			return _ready;
		}
		set
		{
			if (_ready != value)
			{
				_ready = value;
				if (readyToggle != null)
				{
					readyToggle.isOn = _ready;
				}
			}
		}
	}

	public string errorMsg
	{
		get
		{
			return _errorMsg;
		}
		set
		{
			if (_errorMsg != value)
			{
				_errorMsg = value;
				if (_errorMsg != string.Empty && uidText != null)
				{
					uidText.text = _errorMsg;
				}
			}
		}
	}

	public bool removed => _removed;

	public void Remove()
	{
		_removed = true;
		if (manager != null)
		{
			manager.RemoveClip(this);
		}
	}

	public override void InitUI()
	{
		base.InitUI();
		if (removeButton != null)
		{
			removeButton.onClick.AddListener(Remove);
		}
		if (readyToggle != null)
		{
			readyToggle.isOn = _ready;
		}
	}
}
