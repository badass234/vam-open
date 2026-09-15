using UnityEngine;
using UnityEngine.UI;

public class UIImageControl : JSONStorable
{
	public Image imageForBackground;

	protected Image _image;

	protected RawImage _rawImage;

	protected JSONStorableFloat alphaJSON;

	protected JSONStorableColor colorJSON;

	protected JSONStorableBool enableImageForBackgroundJSON;

	public void SyncAlpha(float a)
	{
		if (_image != null)
		{
			Color color = _image.color;
			color.a = a;
			_image.color = color;
		}
		else if (_rawImage != null)
		{
			Color color2 = _rawImage.color;
			color2.a = a;
			_rawImage.color = color2;
		}
	}

	public void SyncColor(float h, float s, float v)
	{
		Color color = HSVColorPicker.HSVToRGB(h, s, v);
		if (_image != null)
		{
			color.a = _image.color.a;
			_image.color = color;
		}
		else if (_rawImage != null)
		{
			color.a = _rawImage.color.a;
			_rawImage.color = color;
		}
	}

	protected void SyncEnableImageForBackground(bool b)
	{
		if (imageForBackground != null)
		{
			imageForBackground.enabled = b;
		}
	}

	protected void Init()
	{
		_image = GetComponent<Image>();
		_rawImage = GetComponent<RawImage>();
		if (_image != null || _rawImage != null)
		{
			Color color = ((!(_image != null)) ? _rawImage.color : _image.color);
			HSVColor startingColor = HSVColorPicker.RGBToHSV(color.r, color.g, color.b);
			colorJSON = new JSONStorableColor("color", startingColor, SyncColor);
			RegisterColor(colorJSON);
			alphaJSON = new JSONStorableFloat("alpha", color.a, SyncAlpha, 0f, 1f);
			RegisterFloat(alphaJSON);
			if (imageForBackground != null)
			{
				enableImageForBackgroundJSON = new JSONStorableBool("enableImageForBackground", imageForBackground.enabled, SyncEnableImageForBackground);
				RegisterBool(enableImageForBackgroundJSON);
			}
		}
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (!(t != null) || colorJSON == null)
		{
			return;
		}
		UIImageControlUI componentInChildren = UITransform.GetComponentInChildren<UIImageControlUI>(true);
		if (componentInChildren != null)
		{
			colorJSON.RegisterColorPicker(componentInChildren.colorPicker, isAlt);
			alphaJSON.RegisterSlider(componentInChildren.alphaSlider, isAlt);
			if (enableImageForBackgroundJSON != null)
			{
				enableImageForBackgroundJSON.RegisterToggle(componentInChildren.enableImageForBackgroundToggle, isAlt);
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
