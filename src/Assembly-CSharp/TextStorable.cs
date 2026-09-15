using UnityEngine;
using UnityEngine.UI;

public class TextStorable : JSONStorable
{
	public Text displayField;

	protected string _text;

	protected JSONStorableString textJSON;

	protected JSONStorableFloat fontSizeJSON;

	public void SyncDisplayField(string s)
	{
		_text = s;
		if (displayField != null)
		{
			displayField.text = s;
		}
	}

	public void SetFontSize(float s)
	{
		if (displayField != null)
		{
			displayField.fontSize = Mathf.RoundToInt(s);
		}
	}

	protected void Init()
	{
		if (displayField != null)
		{
			textJSON = new JSONStorableString("text", string.Empty, SyncDisplayField);
			RegisterString(textJSON);
			fontSizeJSON = new JSONStorableFloat("fontSize", displayField.fontSize, SetFontSize, 10f, 400f);
			RegisterFloat(fontSizeJSON);
		}
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		TextStorableUI componentInChildren = UITransform.GetComponentInChildren<TextStorableUI>(true);
		if (componentInChildren != null)
		{
			if (textJSON != null)
			{
				textJSON.inputField = componentInChildren.inputField;
				textJSON.inputFieldAction = componentInChildren.inputFieldAction;
			}
			if (fontSizeJSON != null)
			{
				fontSizeJSON.slider = componentInChildren.fontSizeSlider;
			}
		}
	}

	public override void InitUIAlt()
	{
		if (!(UITransformAlt != null))
		{
			return;
		}
		TextStorableUI componentInChildren = UITransformAlt.GetComponentInChildren<TextStorableUI>(true);
		if (componentInChildren != null)
		{
			if (textJSON != null)
			{
				textJSON.inputFieldAlt = componentInChildren.inputField;
				textJSON.inputFieldActionAlt = componentInChildren.inputFieldAction;
			}
			if (fontSizeJSON != null)
			{
				fontSizeJSON.sliderAlt = componentInChildren.fontSizeSlider;
			}
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
		}
	}
}
