namespace MVR
{
	public class Metric
	{
		protected string _name;

		protected string _valueFormat;

		protected float _value;

		protected int _accumulateCount;

		protected float _accumulateValue;

		protected float _averageValue;

		protected MetricUI _UI;

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if (_name != value)
				{
					_name = value;
					SyncNameText();
				}
			}
		}

		public string ValueFormat
		{
			get
			{
				return _valueFormat;
			}
			set
			{
				if (_valueFormat != value)
				{
					_valueFormat = value;
					SyncValueText();
					SyncAverageValueText();
				}
			}
		}

		public float Value
		{
			get
			{
				return _value;
			}
			set
			{
				if (_value != value)
				{
					_value = value;
					SyncValueText();
				}
			}
		}

		public float AverageValue
		{
			get
			{
				return _averageValue;
			}
			set
			{
				if (_averageValue != value)
				{
					_averageValue = value;
					SyncAverageValueText();
				}
			}
		}

		public MetricUI UI
		{
			get
			{
				return _UI;
			}
			set
			{
				if (_UI != value)
				{
					_UI = value;
					SyncNameText();
					SyncValueText();
					SyncAverageValueText();
				}
			}
		}

		public Metric(string name, string valueFormat = "F2")
		{
			Name = name;
			ValueFormat = valueFormat;
		}

		protected void SyncNameText()
		{
			if (_UI != null && _UI.nameText != null)
			{
				_UI.nameText.text = _name;
			}
		}

		protected void SyncValueText()
		{
			if (_UI != null && _UI.valueText != null)
			{
				_UI.valueText.text = _value.ToString(_valueFormat);
			}
		}

		public void Accumulate(float valueToAdd)
		{
			Value = valueToAdd;
			_accumulateValue += valueToAdd;
			_accumulateCount++;
		}

		public void CalculateAverage()
		{
			AverageValue = _accumulateValue / (float)_accumulateCount;
			_accumulateValue = 0f;
			_accumulateCount = 0;
		}

		protected void SyncAverageValueText()
		{
			if (_UI != null && _UI.averageValueText != null)
			{
				_UI.averageValueText.text = _averageValue.ToString(_valueFormat);
			}
		}
	}
}
