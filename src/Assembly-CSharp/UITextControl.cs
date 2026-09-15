using UnityEngine;
using UnityEngine.UI;

public class UITextControl : JSONStorable
{
	protected Text _text;

	protected JSONStorableFloat alphaJSON;

	protected JSONStorableColor colorJSON;

	public void SyncAlpha(float a)
	{
		if (_text != null)
		{
			Color color = _text.color;
			color.a = a;
			_text.color = color;
		}
	}

	public void SyncColor(float h, float s, float v)
	{
		if (_text != null)
		{
			Color color = HSVColorPicker.HSVToRGB(h, s, v);
			color.a = _text.color.a;
			_text.color = color;
		}
	}

	protected void Init()
	{
		_text = GetComponent<Text>();
		if (_text != null)
		{
			Color color = _text.color;
			HSVColor startingColor = HSVColorPicker.RGBToHSV(color.r, color.g, color.b);
			colorJSON = new JSONStorableColor("color", startingColor, SyncColor);
			RegisterColor(colorJSON);
			alphaJSON = new JSONStorableFloat("alpha", color.a, SyncAlpha, 0f, 1f);
			RegisterFloat(alphaJSON);
		}
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		UITextControlUI componentInChildren = UITransform.GetComponentInChildren<UITextControlUI>(true);
		if (componentInChildren != null)
		{
			if (colorJSON != null)
			{
				colorJSON.colorPicker = componentInChildren.colorPicker;
			}
			if (alphaJSON != null)
			{
				alphaJSON.slider = componentInChildren.alphaSlider;
			}
		}
	}

	public override void InitUIAlt()
	{
		if (!(UITransformAlt != null))
		{
			return;
		}
		UITextControlUI componentInChildren = UITransformAlt.GetComponentInChildren<UITextControlUI>(true);
		if (componentInChildren != null)
		{
			if (colorJSON != null)
			{
				colorJSON.colorPickerAlt = componentInChildren.colorPicker;
			}
			if (alphaJSON != null)
			{
				alphaJSON.sliderAlt = componentInChildren.alphaSlider;
			}
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
			InitUI();
			InitUIAlt();
		}
	}
}
