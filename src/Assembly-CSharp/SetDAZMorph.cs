using UnityEngine;

public class SetDAZMorph : MonoBehaviour
{
	protected bool isOn;

	[SerializeField]
	protected DAZMorphBank _morphBank;

	[SerializeField]
	protected DAZMorphBank _morphBankAlt;

	[SerializeField]
	protected DAZMorphBank _morphBankAlt2;

	public string morph1Name;

	public float morph1Low;

	public float morph1High = 1f;

	public float currentMorph1Value;

	public bool updateOnEnableAndDisable = true;

	private float _morphPercent;

	protected DAZMorph morph1;

	public DAZMorphBank morphBank
	{
		get
		{
			return _morphBank;
		}
		set
		{
			if (_morphBank != value)
			{
				_morphBank = value;
				InitMorphs();
			}
		}
	}

	public DAZMorphBank morphBankAlt
	{
		get
		{
			return _morphBankAlt;
		}
		set
		{
			if (_morphBankAlt != value)
			{
				_morphBankAlt = value;
				InitMorphs();
			}
		}
	}

	public DAZMorphBank morphBankAlt2
	{
		get
		{
			return _morphBankAlt2;
		}
		set
		{
			if (_morphBankAlt2 != value)
			{
				_morphBankAlt2 = value;
				InitMorphs();
			}
		}
	}

	public float morphPercent
	{
		get
		{
			return _morphPercent;
		}
		set
		{
			_morphPercent = value;
			if (morph1 != null)
			{
				currentMorph1Value = Mathf.Lerp(morph1Low, morph1High, _morphPercent);
				morph1.morphValue = currentMorph1Value;
			}
		}
	}

	public float morphPercentUnclamped
	{
		get
		{
			return _morphPercent;
		}
		set
		{
			_morphPercent = value;
			if (morph1 != null)
			{
				currentMorph1Value = Mathf.LerpUnclamped(morph1Low, morph1High, _morphPercent);
				morph1.morphValue = currentMorph1Value;
			}
		}
	}

	public float morphRawValue
	{
		get
		{
			if (morph1 != null)
			{
				return morph1.morphValue;
			}
			return 0f;
		}
		set
		{
			if (morph1 != null)
			{
				currentMorph1Value = value;
				morph1.morphValue = value;
			}
		}
	}

	protected void InitMorphs(bool isEnable = false)
	{
		if (_morphBank != null)
		{
			_morphBank.Init();
			DAZMorph builtInMorph = _morphBank.GetBuiltInMorph(morph1Name);
			if (builtInMorph == null)
			{
				if (_morphBankAlt != null)
				{
					_morphBankAlt.Init();
					builtInMorph = _morphBankAlt.GetBuiltInMorph(morph1Name);
				}
				if (builtInMorph == null && _morphBankAlt2 != null)
				{
					_morphBankAlt2.Init();
					builtInMorph = _morphBankAlt2.GetBuiltInMorph(morph1Name);
				}
			}
			if (morph1 != null && morph1 != builtInMorph)
			{
				morph1.morphValue = 0f;
			}
			morph1 = builtInMorph;
			if (morph1 != null && isEnable && updateOnEnableAndDisable)
			{
				morph1.morphValue = currentMorph1Value;
			}
		}
		else
		{
			if (morph1 != null)
			{
				morph1.morphValue = 0f;
			}
			morph1 = null;
		}
	}

	private void OnEnable()
	{
		isOn = true;
		InitMorphs(true);
	}

	private void Start()
	{
		InitMorphs(true);
	}

	private void OnDisable()
	{
		isOn = false;
		if (morph1 != null && updateOnEnableAndDisable)
		{
			morph1.morphValue = 0f;
		}
	}
}
