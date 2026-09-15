using UnityEngine;
using UnityEngine.UI;

public class SliderControl : MonoBehaviour
{
	public Slider slider;

	public bool clamp = true;

	public float defaultValue;

	public bool disableLookDrag;

	public float valueAdjustRange
	{
		get
		{
			if (slider != null)
			{
				return slider.value;
			}
			return 0f;
		}
		set
		{
			if (!(slider != null))
			{
				return;
			}
			if (value > slider.maxValue)
			{
				if (slider.maxValue > 0f)
				{
					while (value > slider.maxValue)
					{
						slider.maxValue *= 10f;
					}
				}
				else
				{
					slider.maxValue = value;
				}
			}
			if (value < slider.minValue)
			{
				slider.minValue = value;
			}
			slider.value = value;
		}
	}

	public void setSliderToMinimum()
	{
		if (slider != null)
		{
			slider.value = slider.minValue;
		}
	}

	public void setSliderToMaximum()
	{
		if (slider != null)
		{
			slider.value = slider.maxValue;
		}
	}

	public void setSliderToValue(string value)
	{
		float result;
		if (float.TryParse(value, out result))
		{
			setSliderToValue(result);
		}
	}

	public void setSliderToValue(float value)
	{
		if (!(slider != null))
		{
			return;
		}
		if (!clamp)
		{
			if (value > slider.maxValue)
			{
				slider.maxValue = value;
			}
			if (value < slider.minValue)
			{
				slider.minValue = value;
			}
		}
		slider.value = value;
	}

	public void incrementSlider(float value)
	{
		float sliderToValue = slider.value + value;
		setSliderToValue(sliderToValue);
	}

	public void decrementSlider(float value)
	{
		float sliderToValue = slider.value - value;
		setSliderToValue(sliderToValue);
	}

	public void setSliderToDefaultValue()
	{
		if (slider != null)
		{
			valueAdjustRange = defaultValue;
		}
	}

	public void multiplyMaxRange(float multiplier)
	{
		if (slider != null)
		{
			slider.maxValue *= multiplier;
		}
	}

	public void multiplyRange(float multiplier)
	{
		if (slider != null)
		{
			if (slider.minValue == 0f)
			{
				multiplyMaxRange(multiplier);
				return;
			}
			float num = (slider.minValue + slider.maxValue) * 0.5f;
			float num2 = slider.maxValue - slider.minValue;
			float num3 = num2 * multiplier * 0.5f;
			float minValue = num - num3;
			float maxValue = num + num3;
			slider.minValue = minValue;
			slider.maxValue = maxValue;
		}
	}
}
