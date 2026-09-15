using UnityEngine;
using UnityEngine.UI;

public class UIDynamicToggle : UIDynamic
{
	public Toggle toggle;

	public Text labelText;

	public Image backgroundImage;

	public Color backgroundColor
	{
		get
		{
			if (backgroundImage != null)
			{
				return backgroundImage.color;
			}
			return Color.black;
		}
		set
		{
			if (backgroundImage != null)
			{
				backgroundImage.color = value;
			}
		}
	}

	public Color textColor
	{
		get
		{
			if (labelText != null)
			{
				return labelText.color;
			}
			return Color.black;
		}
		set
		{
			if (labelText != null)
			{
				labelText.color = value;
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
}
