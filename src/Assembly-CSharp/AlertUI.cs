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

	public void SetText(string txt)
	{
		if (alertText != null)
		{
			alertText.text = txt;
		}
	}

	public void DoOKCallback()
	{
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
