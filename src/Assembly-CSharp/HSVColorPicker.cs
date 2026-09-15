using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class HSVColorPicker : TwoDPicker
{
	public delegate void OnColorChanged(Color color);

	public delegate void OnHSVColorChanged(float hue, float saturation, float value);

	public Slider hueSlider;

	public Image hueImage;

	public Image saturationImage;

	public float defaultHue;

	[SerializeField]
	private float _hue;

	[SerializeField]
	private int _hueInt;

	private bool ignoreRGBCallbacks;

	private bool skipRGBSliderSet;

	public float defaultSaturation;

	public float defaultCvalue;

	public Slider redSlider;

	public Slider greenSlider;

	public Slider blueSlider;

	public Image colorSample;

	public Transform colorObject;

	[SerializeField]
	private float _red;

	[SerializeField]
	private float _green;

	[SerializeField]
	private float _blue;

	public Color currentColor;

	public HSVColor currentHSVColor;

	public static HSVColor clipboardColor;

	private static bool cacheBuildTriggered;

	private static Dictionary<int, Sprite> cachedSprites;

	public OnColorChanged onColorChangedHandlers;

	public OnHSVColorChanged onHSVColorChangedHandlers;

	private Texture2D lastHueTexture;

	public float hue
	{
		get
		{
			return _hue;
		}
		set
		{
			if (_hue != value)
			{
				SetHue(value);
			}
		}
	}

	public float hueNoCallback
	{
		get
		{
			return _hue;
		}
		set
		{
			if (_hue != value)
			{
				SetHue(value, true);
			}
		}
	}

	public float saturation
	{
		get
		{
			return _xVal;
		}
		set
		{
			if (base.xVal != value)
			{
				SetSaturation(value);
			}
		}
	}

	public float saturationNoCallback
	{
		get
		{
			return _xVal;
		}
		set
		{
			if (base.xVal != value)
			{
				SetSaturation(value, true);
			}
		}
	}

	public float cvalue
	{
		get
		{
			return _yVal;
		}
		set
		{
			if (base.yVal != value)
			{
				SetCValue(value);
			}
		}
	}

	public float cvalueNoCallback
	{
		get
		{
			return _yVal;
		}
		set
		{
			if (base.yVal != value)
			{
				SetCValue(value, true);
			}
		}
	}

	public float red
	{
		get
		{
			return _red;
		}
		set
		{
			if (_red != value)
			{
				_red = value;
				_red = Mathf.Clamp01(_red);
				RecalcHSV();
			}
		}
	}

	public int red255
	{
		get
		{
			return Mathf.FloorToInt(_red * 255f);
		}
		set
		{
			if (Mathf.FloorToInt(_red * 255f) != value)
			{
				red = (float)value / 255f;
			}
		}
	}

	public string red255string
	{
		get
		{
			return red255.ToString();
		}
		set
		{
			int result;
			if (int.TryParse(value, out result))
			{
				red255 = result;
			}
		}
	}

	public float green
	{
		get
		{
			return _green;
		}
		set
		{
			if (_green != value)
			{
				_green = value;
				_green = Mathf.Clamp01(_green);
				RecalcHSV();
			}
		}
	}

	public int green255
	{
		get
		{
			return Mathf.FloorToInt(_green * 255f);
		}
		set
		{
			if (Mathf.FloorToInt(_green * 255f) != value)
			{
				green = (float)value / 255f;
			}
		}
	}

	public string green255string
	{
		get
		{
			return green255.ToString();
		}
		set
		{
			int result;
			if (int.TryParse(value, out result))
			{
				green255 = result;
			}
		}
	}

	public float blue
	{
		get
		{
			return _blue;
		}
		set
		{
			if (_blue != value)
			{
				_blue = value;
				_blue = Mathf.Clamp01(_blue);
				RecalcHSV();
			}
		}
	}

	public int blue255
	{
		get
		{
			return Mathf.FloorToInt(_blue * 255f);
		}
		set
		{
			if (Mathf.FloorToInt(_blue * 255f) != value)
			{
				blue = (float)value / 255f;
			}
		}
	}

	public string blue255string
	{
		get
		{
			return blue255.ToString();
		}
		set
		{
			int result;
			if (int.TryParse(value, out result))
			{
				blue255 = result;
			}
		}
	}

	public void SetHSV(HSVColor hc, bool noCallback = false)
	{
		if (_hue != hc.H || _xVal != hc.S || _yVal != hc.V)
		{
			SetHueVal(hc.H);
			base.xVal = hc.S;
			base.yVal = hc.V;
			RegenerateSVImage();
			ignoreRGBCallbacks = true;
			RecalcRGB(noCallback);
			ignoreRGBCallbacks = false;
		}
	}

	public void SetHSV(float h, float s, float v, bool noCallback = false)
	{
		if (_hue != h || _xVal != s || _yVal != v)
		{
			SetHueVal(h);
			base.xVal = s;
			base.yVal = v;
			RegenerateSVImage();
			ignoreRGBCallbacks = true;
			RecalcRGB(noCallback);
			ignoreRGBCallbacks = false;
		}
	}

	private void SetHueVal(float h)
	{
		_hue = h;
		_hue = Mathf.Clamp01(_hue);
		_hueInt = Mathf.FloorToInt(_hue * 255f);
		if (hueSlider != null)
		{
			hueSlider.value = _hue;
		}
	}

	private void SetHue(float h, bool noCallback = false)
	{
		ignoreRGBCallbacks = true;
		SetHueVal(h);
		RegenerateSVImage();
		RecalcRGB(noCallback);
		ignoreRGBCallbacks = false;
	}

	private void SetSaturation(float s, bool noCallback = false)
	{
		ignoreRGBCallbacks = true;
		base.xVal = s;
		RecalcRGB(noCallback);
		ignoreRGBCallbacks = false;
	}

	private void SetCValue(float cv, bool noCallback = false)
	{
		ignoreRGBCallbacks = true;
		base.yVal = cv;
		RecalcRGB(noCallback);
		ignoreRGBCallbacks = false;
	}

	public void SetRedFromSlider255(float r)
	{
		skipRGBSliderSet = true;
		red = r / 255f;
		skipRGBSliderSet = false;
	}

	public void SetGreenFromSlider255(float g)
	{
		skipRGBSliderSet = true;
		green = g / 255f;
		skipRGBSliderSet = false;
	}

	public void SetBlueFromSlider255(float b)
	{
		skipRGBSliderSet = true;
		blue = b / 255f;
		skipRGBSliderSet = false;
	}

	public void CopyToClipboard()
	{
		clipboardColor = currentHSVColor;
	}

	public void PasteFromClipboard()
	{
		SetHSV(clipboardColor);
	}

	private void SetCurrentColorFromRGB()
	{
		currentColor = new Color(_red, _green, _blue);
		if (colorSample != null)
		{
			colorSample.color = currentColor;
		}
		if (colorObject != null)
		{
			Component[] components = colorObject.GetComponents<Component>();
			foreach (Component component in components)
			{
				Type type = component.GetType();
				type.GetProperty("color")?.SetValue(component, currentColor, null);
			}
		}
	}

	private void SetRGBSliders()
	{
		if (!skipRGBSliderSet)
		{
			if (redSlider != null)
			{
				redSlider.value = _red * 255f;
			}
			if (greenSlider != null)
			{
				greenSlider.value = _green * 255f;
			}
			if (blueSlider != null)
			{
				blueSlider.value = _blue * 255f;
			}
		}
	}

	private void RecalcRGB(bool noCallback = false)
	{
		currentHSVColor.H = hue;
		currentHSVColor.S = saturation;
		currentHSVColor.V = cvalue;
		Color color = HSVToRGB(hue, saturation, cvalue);
		_red = color.r;
		_green = color.g;
		_blue = color.b;
		SetRGBSliders();
		SetCurrentColorFromRGB();
		if (!noCallback)
		{
			if (onColorChangedHandlers != null)
			{
				onColorChangedHandlers(color);
			}
			if (onHSVColorChangedHandlers != null)
			{
				onHSVColorChangedHandlers(hue, saturation, cvalue);
			}
		}
	}

	private void RecalcHSV()
	{
		if (!ignoreRGBCallbacks)
		{
			HSVColor hSVColor = RGBToHSV(_red, _green, _blue);
			hue = hSVColor.H;
			saturation = hSVColor.S;
			cvalue = hSVColor.V;
		}
	}

	public static HSVColor RGBToHSV(float r, float g, float b)
	{
		HSVColor result = default(HSVColor);
		float a = Mathf.Min(r, b);
		a = Mathf.Min(a, g);
		float a2 = Mathf.Max(r, b);
		a2 = (result.V = Mathf.Max(a2, g));
		float num = a2 - a;
		if (a2 != 0f)
		{
			result.S = num / a2;
			if (num == 0f)
			{
				result.H = 0f;
			}
			else if (r == a2)
			{
				result.H = (g - b) / num;
			}
			else if (g == a2)
			{
				result.H = 2f + (b - r) / num;
			}
			else
			{
				result.H = 4f + (r - g) / num;
			}
			result.H /= 6f;
			if (result.H < 0f)
			{
				result.H += 1f;
			}
			return result;
		}
		result.S = 0f;
		result.H = 0f;
		return result;
	}

	public static Color HSVToRGB(HSVColor hsvColor)
	{
		return HSVToRGB(hsvColor.H, hsvColor.S, hsvColor.V);
	}

	public static Color HSVToRGB(float H, float S, float V)
	{
		Color white = Color.white;
		if (S == 0f)
		{
			white.r = V;
			white.g = V;
			white.b = V;
		}
		else if (V == 0f)
		{
			white.r = 0f;
			white.g = 0f;
			white.b = 0f;
		}
		else
		{
			white.r = 0f;
			white.g = 0f;
			white.b = 0f;
			float num = H * 6f;
			int num2 = (int)Mathf.Floor(num);
			float num3 = num - (float)num2;
			float num4 = V * (1f - S);
			float num5 = V * (1f - S * num3);
			float num6 = V * (1f - S * (1f - num3));
			switch (num2)
			{
			case -1:
				white.r = V;
				white.g = num4;
				white.b = num5;
				break;
			case 0:
				white.r = V;
				white.g = num6;
				white.b = num4;
				break;
			case 1:
				white.r = num5;
				white.g = V;
				white.b = num4;
				break;
			case 2:
				white.r = num4;
				white.g = V;
				white.b = num6;
				break;
			case 3:
				white.r = num4;
				white.g = num5;
				white.b = V;
				break;
			case 4:
				white.r = num6;
				white.g = num4;
				white.b = V;
				break;
			case 5:
				white.r = V;
				white.g = num4;
				white.b = num5;
				break;
			case 6:
				white.r = V;
				white.g = num6;
				white.b = num4;
				break;
			}
			white.r = Mathf.Clamp(white.r, 0f, 1f);
			white.g = Mathf.Clamp(white.g, 0f, 1f);
			white.b = Mathf.Clamp(white.b, 0f, 1f);
		}
		return white;
	}

	public void RegenerateSVImage()
	{
		if (cachedSprites == null)
		{
			cachedSprites = new Dictionary<int, Sprite>();
		}
		Sprite value;
		if (!cachedSprites.TryGetValue(_hueInt, out value))
		{
			Texture2D texture2D = new Texture2D(256, 256);
			for (int i = 0; i < 256; i++)
			{
				for (int j = 0; j < 256; j++)
				{
					texture2D.SetPixel(i, j, HSVToRGB(_hue, (float)i / 255f, (float)j / 255f));
				}
			}
			texture2D.Apply();
			Rect rect = new Rect(0f, 0f, texture2D.width, texture2D.height);
			value = Sprite.Create(texture2D, rect, new Vector2(0.5f, 0.5f));
			cachedSprites.Add(_hueInt, value);
		}
		if (saturationImage == null)
		{
			saturationImage = GetComponent<Image>();
		}
		if (saturationImage != null)
		{
			saturationImage.sprite = value;
			saturationImage.color = Color.white;
			saturationImage.type = Image.Type.Simple;
		}
	}

	private void RegenerateHueImage()
	{
		if (hueImage != null)
		{
			Texture2D texture2D = new Texture2D(1, 256);
			for (int i = 0; i < 256; i++)
			{
				texture2D.SetPixel(0, i, HSVToRGB((float)i / 255f, 1f, 1f));
			}
			texture2D.Apply();
			Rect rect = new Rect(0f, 0f, 1f, texture2D.height);
			Sprite sprite = Sprite.Create(texture2D, rect, new Vector2(0.5f, 0.5f));
			hueImage.sprite = sprite;
			hueImage.color = Color.white;
			hueImage.type = Image.Type.Simple;
			if (lastHueTexture != null && Application.isPlaying)
			{
				UnityEngine.Object.Destroy(lastHueTexture);
			}
			lastHueTexture = texture2D;
		}
	}

	private IEnumerator GenerateAllCachedImages()
	{
		int saveHueInt = _hueInt;
		float saveHue = _hue;
		for (int i = 0; i < 256; i++)
		{
			_hueInt = i;
			_hue = (float)_hueInt / 255f;
			RegenerateSVImage();
			yield return null;
		}
		_hueInt = saveHueInt;
		_hue = saveHue;
	}

	private void RegenerateImages()
	{
		RegenerateSVImage();
		RegenerateHueImage();
	}

	protected override void SetXYValFromSelectorPosition()
	{
		base.SetXYValFromSelectorPosition();
		ignoreRGBCallbacks = true;
		RecalcRGB();
		ignoreRGBCallbacks = false;
	}

	public void Reset()
	{
		hue = defaultHue;
		saturation = defaultSaturation;
		cvalue = defaultCvalue;
		RecalcRGB();
	}

	private void OnEnable()
	{
		RegenerateImages();
	}

	public override void Awake()
	{
		base.Awake();
		if (!cacheBuildTriggered)
		{
			cacheBuildTriggered = true;
			GenerateAllCachedImages();
		}
		RegenerateImages();
	}

	private void OnDestroy()
	{
		if (Application.isPlaying && lastHueTexture != null)
		{
			UnityEngine.Object.Destroy(lastHueTexture);
		}
	}
}
