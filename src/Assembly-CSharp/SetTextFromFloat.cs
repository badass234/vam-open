using UnityEngine;
using UnityEngine.UI;

public class SetTextFromFloat : MonoBehaviour
{
	protected bool _isActive;

	public Text UIText;

	public InputField UIInputField;

	public string floatFormat = "F2";

	[SerializeField]
	private float _floatVal;

	public float floatVal
	{
		get
		{
			return _floatVal;
		}
		set
		{
			_floatVal = value;
			SyncText();
		}
	}

	public void SyncText()
	{
		if (_isActive)
		{
			if (UIInputField != null)
			{
				UIInputField.text = _floatVal.ToString(floatFormat);
			}
			if (UIText != null)
			{
				UIText.text = _floatVal.ToString(floatFormat);
			}
		}
	}

	private void OnDisable()
	{
		_isActive = false;
	}

	private void OnEnable()
	{
		_isActive = true;
		SyncText();
	}
}
