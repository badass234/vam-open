using UnityEngine;
using UnityEngine.UI;

public class UIDynamicTextField : UIDynamic
{
	public Text UItext;

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
			if (UItext != null)
			{
				return UItext.color;
			}
			return Color.black;
		}
		set
		{
			if (UItext != null)
			{
				UItext.color = value;
			}
		}
	}

	public string text
	{
		get
		{
			if (UItext != null)
			{
				return UItext.text;
			}
			return null;
		}
		set
		{
			if (UItext != null)
			{
				UItext.text = value;
			}
		}
	}
}
