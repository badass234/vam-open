using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AlertUI : MonoBehaviour
{
	public Text alertText;

	public Button okButton;

	protected UnityAction _okCallback;

	public Button cancelButton;

	protected UnityAction _cancelCallback;

	private UnityAction _autoCloseCallback;

	private float _autoCloseSeconds;

	private float _autoCloseTimer;

	private float _nextCountdownUpdate;

	private bool _autoCloseArmed;

	private bool _alertClosed;

	private string _alertMessage = string.Empty;

	public string autoCloseCountdownFormat = "Auto-closing in {0} seconds...";

	public bool autoCloseArmed
	{
		get
		{
			return _autoCloseArmed;
		}
	}

	public float autoCloseSecondsRemaining
	{
		get
		{
			if (!_autoCloseArmed)
			{
				return 0f;
			}
			return Mathf.Max(0f, _autoCloseSeconds - _autoCloseTimer);
		}
	}

	public void SetText(string txt)
	{
		_alertMessage = ((txt == null) ? string.Empty : txt);
		SyncCountdownText();
	}

	public void SetOKButtonText(string txt)
	{
		SetButtonText(okButton, txt);
	}

	public void SetCancelButtonText(string txt)
	{
		SetButtonText(cancelButton, txt);
	}

	public void SetAutoClose(float seconds, UnityAction autoCloseCallback)
	{
		_autoCloseSeconds = seconds;
		_autoCloseCallback = autoCloseCallback;
		_autoCloseTimer = 0f;
		_nextCountdownUpdate = 0f;
		_autoCloseArmed = seconds > 0f && !_alertClosed;
		SyncCountdownText();
	}

	public void DoOKCallback()
	{
		if (_alertClosed)
		{
			return;
		}
		_alertClosed = true;
		_autoCloseArmed = false;
		Object.Destroy(base.gameObject);
		if (_okCallback != null)
		{
			_okCallback();
		}
	}

	public void SetOKButton(UnityAction okCallback)
	{
		_okCallback = okCallback;
	}

	public void DoCancelCallback()
	{
		if (_alertClosed)
		{
			return;
		}
		_alertClosed = true;
		_autoCloseArmed = false;
		Object.Destroy(base.gameObject);
		if (_cancelCallback != null)
		{
			_cancelCallback();
		}
	}

	public void SetCancelButton(UnityAction cancelCallback)
	{
		if (cancelButton != null)
		{
			_cancelCallback = cancelCallback;
		}
	}

	private static void SetButtonText(Button button, string txt)
	{
		if (button == null || txt == null)
		{
			return;
		}
		Text[] componentsInChildren = button.GetComponentsInChildren<Text>(true);
		if (componentsInChildren == null || componentsInChildren.Length == 0)
		{
			return;
		}
		componentsInChildren[0].text = txt;
	}

	private void Update()
	{
		if (!_autoCloseArmed || _alertClosed || !Application.isPlaying)
		{
			return;
		}
		_autoCloseTimer += Time.unscaledDeltaTime;
		if (_autoCloseTimer >= _autoCloseSeconds)
		{
			AutoClose();
			return;
		}
		if (_autoCloseTimer >= _nextCountdownUpdate)
		{
			_nextCountdownUpdate = Mathf.Floor(_autoCloseTimer) + 1f;
			SyncCountdownText();
		}
	}

	private void AutoClose()
	{
		if (_alertClosed)
		{
			return;
		}
		_alertClosed = true;
		_autoCloseArmed = false;
		UnityAction unityAction = _autoCloseCallback;
		_autoCloseCallback = null;
		Object.Destroy(base.gameObject);
		if (unityAction != null)
		{
			unityAction();
		}
	}

	private void SyncCountdownText()
	{
		if (alertText == null)
		{
			return;
		}
		if (!_autoCloseArmed)
		{
			alertText.text = _alertMessage;
			return;
		}
		int num = Mathf.CeilToInt(Mathf.Max(0f, _autoCloseSeconds - _autoCloseTimer));
		alertText.text = _alertMessage + "\n\n" + string.Format(autoCloseCountdownFormat, num);
	}

	private void Awake()
	{
		if (okButton != null)
		{
			okButton.onClick.AddListener(DoOKCallback);
		}
		if (cancelButton != null)
		{
			cancelButton.onClick.AddListener(DoCancelCallback);
		}
	}
}
