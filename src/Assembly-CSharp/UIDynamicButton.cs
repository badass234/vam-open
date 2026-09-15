using UnityEngine;
using UnityEngine.UI;

public class UIDynamicButton : UIDynamic
{
	public Button button;

	public Text buttonText;

	public Image buttonImage;

	public Color buttonColor
	{
		get
		{
			if (buttonImage != null)
			{
				return buttonImage.color;
			}
			return Color.black;
		}
		set
		{
			if (buttonImage != null)
			{
				buttonImage.color = value;
			}
		}
	}

	public Color textColor
	{
		get
		{
			if (buttonText != null)
			{
				return buttonText.color;
			}
			return Color.black;
		}
		set
		{
			if (buttonText != null)
			{
				buttonText.color = value;
			}
		}
	}

	public string label
	{
		get
		{
			if (buttonText != null)
			{
				return buttonText.text;
			}
			return null;
		}
		set
		{
			if (buttonText != null)
			{
				buttonText.text = value;
			}
		}
	}
}
