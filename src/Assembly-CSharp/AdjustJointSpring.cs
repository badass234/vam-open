using UnityEngine;

public class AdjustJointSpring : ScaleChangeReceiver
{
	public bool on = true;

	[SerializeField]
	private float _percent;

	protected float scalePow = 1f;

	[SerializeField]
	private float _lowSpring;

	[SerializeField]
	private float _defaultPercent;

	[SerializeField]
	private float _highSpring;

	[SerializeField]
	private float _yzMultiplier = 1f;

	[SerializeField]
	private float _currentXSpring;

	[SerializeField]
	private float _currentYZSpring;

	[SerializeField]
	private float _currentSpring;

	private ConfigurableJoint CJ;

	private bool _defaultsSet;

	private float _defaultDamper;

	private float _defaultMaxForce;

	private float _defaultXDamper;

	private float _defaultXMaxForce;

	private float _defaultYZDamper;

	private float _defaultYZMaxForce;

	public float percent
	{
		get
		{
			return _percent;
		}
		set
		{
			if (_percent != value)
			{
				_percent = value;
				SetSpringVarsFromPercent();
				Adjust();
			}
		}
	}

	public float lowSpring
	{
		get
		{
			return _lowSpring;
		}
		set
		{
			if (_lowSpring != value)
			{
				_lowSpring = value;
				SetSpringVarsFromPercent();
				Adjust();
			}
		}
	}

	public float defaultPercent
	{
		get
		{
			return _defaultPercent;
		}
		set
		{
			if (_defaultPercent != value)
			{
				_defaultPercent = value;
			}
		}
	}

	public float highSpring
	{
		get
		{
			return _highSpring;
		}
		set
		{
			if (_highSpring != value)
			{
				_highSpring = value;
				SetSpringVarsFromPercent();
				Adjust();
			}
		}
	}

	public float yzMultiplier
	{
		get
		{
			return _yzMultiplier;
		}
		set
		{
			if (_yzMultiplier != value)
			{
				_yzMultiplier = value;
				SetSpringVarsFromPercent();
				Adjust();
			}
		}
	}

	public float currentXSpring
	{
		get
		{
			return _currentXSpring;
		}
		set
		{
			if (_currentXSpring != value)
			{
				_currentXSpring = value;
				Adjust();
			}
		}
	}

	public float currentYZSpring
	{
		get
		{
			return _currentYZSpring;
		}
		set
		{
			if (_currentYZSpring != value)
			{
				_currentYZSpring = value;
				Adjust();
			}
		}
	}

	public float currentSpring
	{
		get
		{
			return _currentSpring;
		}
		set
		{
			if (_currentSpring != value)
			{
				_currentSpring = value;
				_currentXSpring = value;
				_currentYZSpring = value * _yzMultiplier;
				Adjust();
			}
		}
	}

	public ConfigurableJoint controlledJoint
	{
		get
		{
			if (CJ == null)
			{
				CJ = GetComponent<ConfigurableJoint>();
			}
			return CJ;
		}
	}

	private void SetSpringVarsFromPercent()
	{
		_currentSpring = (highSpring - lowSpring) * _percent + lowSpring;
		_currentXSpring = _currentSpring;
		_currentYZSpring = _currentSpring * _yzMultiplier;
	}

	public override void ScaleChanged(float scale)
	{
		base.ScaleChanged(scale);
		scalePow = Mathf.Pow(2f, _scale - 1f);
		Adjust();
	}

	public void SetDefaultPercent()
	{
		percent = _defaultPercent;
	}

	private void Adjust()
	{
		if (!on)
		{
			return;
		}
		if (CJ == null)
		{
			CJ = GetComponent<ConfigurableJoint>();
		}
		if (CJ != null)
		{
			float num = scalePow;
			float num2 = scalePow;
			float num3 = num;
			JointDrive slerpDrive = CJ.slerpDrive;
			if (!_defaultsSet)
			{
				_defaultDamper = slerpDrive.positionDamper;
				_defaultMaxForce = slerpDrive.maximumForce;
			}
			slerpDrive.positionSpring = _currentSpring * num;
			slerpDrive.positionDamper = _defaultDamper * num2;
			slerpDrive.maximumForce = _defaultMaxForce * num3;
			CJ.slerpDrive = slerpDrive;
			slerpDrive = CJ.angularXDrive;
			if (!_defaultsSet)
			{
				_defaultXDamper = slerpDrive.positionDamper;
				_defaultXMaxForce = slerpDrive.maximumForce;
			}
			slerpDrive.positionSpring = _currentXSpring * num;
			slerpDrive.positionDamper = _defaultXDamper * num2;
			slerpDrive.maximumForce = _defaultXMaxForce * num3;
			CJ.angularXDrive = slerpDrive;
			slerpDrive = CJ.angularYZDrive;
			if (!_defaultsSet)
			{
				_defaultYZDamper = slerpDrive.positionDamper;
				_defaultYZMaxForce = slerpDrive.maximumForce;
			}
			slerpDrive.positionSpring = _currentYZSpring * num;
			slerpDrive.positionDamper = _defaultYZDamper * num2;
			slerpDrive.maximumForce = _defaultYZMaxForce * num3;
			CJ.angularYZDrive = slerpDrive;
			_defaultsSet = true;
			Rigidbody component = GetComponent<Rigidbody>();
			component.WakeUp();
		}
	}
}
