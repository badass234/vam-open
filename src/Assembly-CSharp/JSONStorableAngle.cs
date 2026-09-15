public class JSONStorableAngle : JSONStorableFloat
{
	public override float val
	{
		get
		{
			return _val;
		}
		set
		{
			float num;
			for (num = value; num > 180f; num -= 360f)
			{
			}
			for (; num < -180f; num += 360f)
			{
			}
			if (_val != num)
			{
				_val = num;
				if (_slider != null)
				{
					_slider.value = _val;
				}
				if (_sliderAlt != null)
				{
					_sliderAlt.value = _val;
				}
				if (setCallbackFunction != null)
				{
					setCallbackFunction(_val);
				}
			}
		}
	}

	public JSONStorableAngle(string n, float v, SetFloatCallback callback)
		: base(n, v, callback, -180f, 180f)
	{
	}
}
