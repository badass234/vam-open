using UnityEngine;
using UnityEngine.UI;

public class UIDynamicColorPicker : UIDynamic
{
	public Text labelText;

	public RectTransform pickerContainer;

	public HSVColorPicker colorPicker;

	[HideInInspector]
	[SerializeField]
	protected bool _showLabel = true;

	public bool showLabel
	{
		get
		{
			return _showLabel;
		}
		set
		{
			if (_showLabel != value)
			{
				_showLabel = value;
				SyncShowLabel();
			}
		}
	}

	public string label
	{
		get
		{
			if (labelText != null)
			{
				return labelText.text;
			}
			return null;
		}
		set
		{
			if (labelText != null)
			{
				labelText.text = value;
			}
		}
	}

	protected void SyncShowLabel()
	{
		if (!(labelText != null))
		{
			return;
		}
		labelText.gameObject.SetActive(_showLabel);
		if (pickerContainer != null)
		{
			Vector2 offsetMax = pickerContainer.offsetMax;
			if (_showLabel)
			{
				offsetMax.y = -50f;
			}
			else
			{
				offsetMax.y = -15f;
			}
			pickerContainer.offsetMax = offsetMax;
		}
	}
}
