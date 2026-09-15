using UnityEngine;
using UnityEngine.UI;

public class UIDynamicSlider : UIDynamic
{
	public Text labelText;

	public Slider slider;

	public SliderControl sliderControl;

	public SetTextFromFloat sliderValueTextFromFloat;

	public RectTransform quickButtonsGroup;

	public Button quickButtonM1;

	public Button quickButtonM2;

	public Button quickButtonM3;

	public Button quickButtonM4;

	public Button quickButtonP1;

	public Button quickButtonP2;

	public Button quickButtonP3;

	public Button quickButtonP4;

	[HideInInspector]
	[SerializeField]
	protected bool _quickButtonsEnabled = true;

	[HideInInspector]
	[SerializeField]
	protected bool _autoSetQuickButtons = true;

	public RectTransform rangeAdjustGroup;

	[HideInInspector]
	[SerializeField]
	protected bool _rangeAdjustEnabled;

	public Button defaultButton;

	[HideInInspector]
	[SerializeField]
	protected bool _defaultButtonEnabled = true;

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

	public string valueFormat
	{
		get
		{
			if (sliderValueTextFromFloat != null)
			{
				return sliderValueTextFromFloat.floatFormat;
			}
			return null;
		}
		set
		{
			if (sliderValueTextFromFloat != null && sliderValueTextFromFloat.floatFormat != value)
			{
				sliderValueTextFromFloat.floatFormat = value;
				sliderValueTextFromFloat.SyncText();
				if (_autoSetQuickButtons)
				{
					AutoSetButtons();
				}
			}
		}
	}

	public bool quickButtonsEnabled
	{
		get
		{
			return _quickButtonsEnabled;
		}
		set
		{
			if (_quickButtonsEnabled != value)
			{
				_quickButtonsEnabled = value;
				SyncQuickButtonsGroup();
			}
		}
	}

	public bool autoSetQuickButtons
	{
		get
		{
			return _autoSetQuickButtons;
		}
		set
		{
			if (_autoSetQuickButtons != value)
			{
				_autoSetQuickButtons = value;
				AutoSetButtons();
			}
		}
	}

	public bool rangeAdjustEnabled
	{
		get
		{
			return _rangeAdjustEnabled;
		}
		set
		{
			if (_rangeAdjustEnabled != value)
			{
				_rangeAdjustEnabled = value;
				SyncRangeAdjustGroup();
			}
		}
	}

	public float defaultValue
	{
		get
		{
			if (sliderControl != null)
			{
				return sliderControl.defaultValue;
			}
			return 0f;
		}
		set
		{
			if (sliderControl != null)
			{
				sliderControl.defaultValue = value;
			}
		}
	}

	public bool defaultButtonEnabled
	{
		get
		{
			return _defaultButtonEnabled;
		}
		set
		{
			if (_defaultButtonEnabled != value)
			{
				_defaultButtonEnabled = value;
				SyncDefaultButton();
			}
		}
	}

	public void Configure(string labelString, float min, float max, float defaultValue, bool clamp = true, string valFormat = "F2", bool showQuickButtons = true, bool showRangeAdjust = false)
	{
		label = labelString;
		if (slider != null)
		{
			slider.minValue = min;
			slider.maxValue = max;
			slider.value = defaultValue;
		}
		if (sliderControl != null)
		{
			sliderControl.defaultValue = defaultValue;
			sliderControl.clamp = clamp;
		}
		valueFormat = valFormat;
		quickButtonsEnabled = showQuickButtons;
		rangeAdjustEnabled = showRangeAdjust;
	}

	protected void SyncQuickButtonsGroup()
	{
		if (quickButtonsGroup != null)
		{
			quickButtonsGroup.gameObject.SetActive(_quickButtonsEnabled);
		}
	}

	protected void InitQuickButton(Button b)
	{
		if (b != null)
		{
			float f = GetQuickButtonValue(b);
			if (valueFormat.StartsWith("P"))
			{
				f *= 0.01f;
			}
			b.onClick.RemoveAllListeners();
			b.onClick.AddListener(delegate
			{
				sliderControl.incrementSlider(f);
			});
		}
	}

	protected void InitQuickButtons()
	{
		InitQuickButton(quickButtonM1);
		InitQuickButton(quickButtonM2);
		InitQuickButton(quickButtonM3);
		InitQuickButton(quickButtonM4);
		InitQuickButton(quickButtonP1);
		InitQuickButton(quickButtonP2);
		InitQuickButton(quickButtonP3);
		InitQuickButton(quickButtonP4);
	}

	public void ConfigureQuickButton(Button b, float qv)
	{
		if (!(b != null))
		{
			return;
		}
		Text componentInChildren = b.GetComponentInChildren<Text>();
		if (componentInChildren != null)
		{
			if (qv > 0f)
			{
				componentInChildren.text = "+" + qv;
			}
			else
			{
				componentInChildren.text = qv.ToString();
			}
		}
		if (Application.isPlaying)
		{
			b.onClick.RemoveAllListeners();
			b.onClick.AddListener(delegate
			{
				sliderControl.incrementSlider(qv);
			});
		}
	}

	public void ConfigureQuickButton(Button b, float qv, float sv)
	{
		if (!(b != null))
		{
			return;
		}
		Text componentInChildren = b.GetComponentInChildren<Text>();
		if (componentInChildren != null)
		{
			if (qv > 0f)
			{
				componentInChildren.text = "+" + qv;
			}
			else
			{
				componentInChildren.text = qv.ToString();
			}
		}
		if (Application.isPlaying)
		{
			b.onClick.RemoveAllListeners();
			b.onClick.AddListener(delegate
			{
				sliderControl.incrementSlider(sv);
			});
		}
	}

	public float GetQuickButtonValue(Button b)
	{
		float result = 0f;
		if (b != null)
		{
			Text componentInChildren = b.GetComponentInChildren<Text>();
			if (componentInChildren != null)
			{
				string text = componentInChildren.text;
				text.Replace("+", string.Empty);
				float.TryParse(componentInChildren.text, out result);
			}
		}
		return result;
	}

	public void ConfigureQuickButtons(float m1v, float m2v, float m3v, float m4v, float p1v, float p2v, float p3v, float p4v)
	{
		ConfigureQuickButton(quickButtonM1, m1v);
		ConfigureQuickButton(quickButtonM2, m2v);
		ConfigureQuickButton(quickButtonM3, m3v);
		ConfigureQuickButton(quickButtonM4, m4v);
		ConfigureQuickButton(quickButtonP1, p1v);
		ConfigureQuickButton(quickButtonP2, p2v);
		ConfigureQuickButton(quickButtonP3, p3v);
		ConfigureQuickButton(quickButtonP4, p4v);
	}

	public void AutoSetButtons()
	{
		switch (valueFormat)
		{
		case "F0":
			ConfigureQuickButton(quickButtonM1, -1f);
			ConfigureQuickButton(quickButtonM2, -10f);
			ConfigureQuickButton(quickButtonM3, -100f);
			ConfigureQuickButton(quickButtonM4, -1000f);
			ConfigureQuickButton(quickButtonP1, 1f);
			ConfigureQuickButton(quickButtonP2, 10f);
			ConfigureQuickButton(quickButtonP3, 100f);
			ConfigureQuickButton(quickButtonP4, 1000f);
			break;
		case "F1":
			ConfigureQuickButton(quickButtonM1, -0.1f);
			ConfigureQuickButton(quickButtonM2, -1f);
			ConfigureQuickButton(quickButtonM3, -10f);
			ConfigureQuickButton(quickButtonM4, -100f);
			ConfigureQuickButton(quickButtonP1, 0.1f);
			ConfigureQuickButton(quickButtonP2, 1f);
			ConfigureQuickButton(quickButtonP3, 10f);
			ConfigureQuickButton(quickButtonP4, 100f);
			break;
		case "F2":
			ConfigureQuickButton(quickButtonM1, -0.01f);
			ConfigureQuickButton(quickButtonM2, -0.1f);
			ConfigureQuickButton(quickButtonM3, -1f);
			ConfigureQuickButton(quickButtonM4, -10f);
			ConfigureQuickButton(quickButtonP1, 0.01f);
			ConfigureQuickButton(quickButtonP2, 0.1f);
			ConfigureQuickButton(quickButtonP3, 1f);
			ConfigureQuickButton(quickButtonP4, 10f);
			break;
		case "F3":
			ConfigureQuickButton(quickButtonM1, -0.001f);
			ConfigureQuickButton(quickButtonM2, -0.01f);
			ConfigureQuickButton(quickButtonM3, -0.1f);
			ConfigureQuickButton(quickButtonM4, -1f);
			ConfigureQuickButton(quickButtonP1, 0.001f);
			ConfigureQuickButton(quickButtonP2, 0.01f);
			ConfigureQuickButton(quickButtonP3, 0.1f);
			ConfigureQuickButton(quickButtonP4, 1f);
			break;
		case "F4":
			ConfigureQuickButton(quickButtonM1, -0.0001f);
			ConfigureQuickButton(quickButtonM2, -0.001f);
			ConfigureQuickButton(quickButtonM3, -0.01f);
			ConfigureQuickButton(quickButtonM4, -0.1f);
			ConfigureQuickButton(quickButtonP1, 0.0001f);
			ConfigureQuickButton(quickButtonP2, 0.001f);
			ConfigureQuickButton(quickButtonP3, 0.01f);
			ConfigureQuickButton(quickButtonP4, 0.1f);
			break;
		case "F5":
			ConfigureQuickButton(quickButtonM1, -1E-05f);
			ConfigureQuickButton(quickButtonM2, -0.0001f);
			ConfigureQuickButton(quickButtonM3, -0.001f);
			ConfigureQuickButton(quickButtonM4, -0.01f);
			ConfigureQuickButton(quickButtonP1, 1E-05f);
			ConfigureQuickButton(quickButtonP2, 0.0001f);
			ConfigureQuickButton(quickButtonP3, 0.001f);
			ConfigureQuickButton(quickButtonP4, 0.01f);
			break;
		case "P0":
			ConfigureQuickButton(quickButtonM1, -1f, -0.01f);
			ConfigureQuickButton(quickButtonM2, -10f, -0.1f);
			ConfigureQuickButton(quickButtonM3, -100f, -1f);
			ConfigureQuickButton(quickButtonM4, -1000f, -10f);
			ConfigureQuickButton(quickButtonP1, 1f, 0.01f);
			ConfigureQuickButton(quickButtonP2, 10f, 0.1f);
			ConfigureQuickButton(quickButtonP3, 100f, 1f);
			ConfigureQuickButton(quickButtonP4, 1000f, 10f);
			break;
		case "P1":
			ConfigureQuickButton(quickButtonM1, -0.1f, -0.001f);
			ConfigureQuickButton(quickButtonM2, -1f, -0.01f);
			ConfigureQuickButton(quickButtonM3, -10f, -0.1f);
			ConfigureQuickButton(quickButtonM4, -100f, -1f);
			ConfigureQuickButton(quickButtonP1, 0.1f, 0.001f);
			ConfigureQuickButton(quickButtonP2, 1f, 0.01f);
			ConfigureQuickButton(quickButtonP3, 10f, 0.1f);
			ConfigureQuickButton(quickButtonP4, 100f, 1f);
			break;
		case "P2":
			ConfigureQuickButton(quickButtonM1, -0.01f, -0.0001f);
			ConfigureQuickButton(quickButtonM2, -0.1f, -0.001f);
			ConfigureQuickButton(quickButtonM3, -1f, -0.01f);
			ConfigureQuickButton(quickButtonM4, -10f, -0.1f);
			ConfigureQuickButton(quickButtonP1, 0.01f, 0.0001f);
			ConfigureQuickButton(quickButtonP2, 0.1f, 0.001f);
			ConfigureQuickButton(quickButtonP3, 1f, 0.01f);
			ConfigureQuickButton(quickButtonP4, 10f, 0.1f);
			break;
		}
	}

	protected void SyncRangeAdjustGroup()
	{
		if (!(rangeAdjustGroup != null))
		{
			return;
		}
		rangeAdjustGroup.gameObject.SetActive(_rangeAdjustEnabled);
		if (slider != null)
		{
			RectTransform component = slider.GetComponent<RectTransform>();
			Vector2 offsetMax = component.offsetMax;
			if (_rangeAdjustEnabled)
			{
				offsetMax.x = -90f;
			}
			else
			{
				offsetMax.x = -10f;
			}
			component.offsetMax = offsetMax;
		}
	}

	protected void SyncDefaultButton()
	{
		if (defaultButton != null)
		{
			defaultButton.gameObject.SetActive(_defaultButtonEnabled);
		}
	}

	protected void Init()
	{
		InitQuickButtons();
		SyncQuickButtonsGroup();
		SyncRangeAdjustGroup();
		SyncDefaultButton();
	}

	private void Awake()
	{
		Init();
	}
}
