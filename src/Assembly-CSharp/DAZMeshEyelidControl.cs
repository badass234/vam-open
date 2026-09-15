using UnityEngine;

public class DAZMeshEyelidControl : JSONStorable
{
	[SerializeField]
	private DAZMorphBank _morphBank;

	public Transform leftEye;

	public Transform rightEye;

	public EyesControl eyesControl;

	public string LeftTopEyelidDownMorphName;

	public string RightTopEyelidDownMorphName;

	public string LeftBottomEyelidUpMorphName;

	public string RightBottomEyelidUpMorphName;

	public string LeftTopEyelidUpMorphName;

	public string RightTopEyelidUpMorphName;

	public string LeftBottomEyelidDownMorphName;

	public string RightBottomEyelidDownMorphName;

	public float blinkSpaceMin = 1f;

	public float blinkSpaceMax = 7f;

	public float blinkTimeMin = 0.1f;

	public float blinkTimeMax = 0.4f;

	public float blinkDownUpRatio = 0.4f;

	public float blinkBottomEyelidFactor = 0.5f;

	public float lookUpTopEyelidFactor = 3f;

	public float lookUpBottomEyelidFactor = 1f;

	public float lookDownTopEyelidFactor = 1.5f;

	public float lookDownBottomEyelidFactor = 4f;

	private DAZMorph LeftTopEyelidDownMorph;

	private DAZMorph RightTopEyelidDownMorph;

	private DAZMorph LeftBottomEyelidUpMorph;

	private DAZMorph RightBottomEyelidUpMorph;

	private DAZMorph LeftTopEyelidUpMorph;

	private DAZMorph RightTopEyelidUpMorph;

	private DAZMorph LeftBottomEyelidDownMorph;

	private DAZMorph RightBottomEyelidDownMorph;

	protected JSONStorableBool blinkEnabledJSON;

	protected JSONStorableBool eyelidLookMorphsEnabledJSON;

	private bool closed;

	private bool blinking;

	private float blinkStartTimer;

	public float blinkTime;

	public float currentWeight;

	public float leftEyeWeight;

	public float rightEyeWeight;

	public float targetWeight;

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
				ZeroMorphs();
				InitMorphs();
			}
		}
	}

	protected void SyncBlinkEnabled(bool b)
	{
	}

	protected void SyncEyelidLookMorphsEnabled(bool b)
	{
		if (!b)
		{
			if (LeftTopEyelidUpMorph != null)
			{
				LeftTopEyelidUpMorph.morphValue = 0f;
			}
			if (LeftBottomEyelidDownMorph != null)
			{
				LeftBottomEyelidDownMorph.morphValue = 0f;
			}
			if (RightTopEyelidUpMorph != null)
			{
				RightTopEyelidUpMorph.morphValue = 0f;
			}
			if (RightBottomEyelidDownMorph != null)
			{
				RightBottomEyelidDownMorph.morphValue = 0f;
			}
		}
	}

	public void Close()
	{
		closed = true;
		BlinkClose();
	}

	public void Open()
	{
		closed = false;
		BlinkOpen();
	}

	public void Blink()
	{
		blinking = true;
		BlinkClose();
		blinkTime = Random.Range(blinkTimeMin, blinkTimeMax);
	}

	private void BlinkClose()
	{
		targetWeight = 1f;
	}

	private void BlinkOpen()
	{
		if (!closed)
		{
			targetWeight = 0f;
		}
	}

	private void ZeroMorphs()
	{
		if (LeftTopEyelidDownMorph != null)
		{
			LeftTopEyelidDownMorph.morphValue = 0f;
		}
		if (RightTopEyelidDownMorph != null)
		{
			RightTopEyelidDownMorph.morphValue = 0f;
		}
		if (LeftTopEyelidUpMorph != null)
		{
			LeftTopEyelidUpMorph.morphValue = 0f;
		}
		if (RightTopEyelidUpMorph != null)
		{
			RightTopEyelidUpMorph.morphValue = 0f;
		}
		if (LeftBottomEyelidDownMorph != null)
		{
			LeftBottomEyelidDownMorph.morphValue = 0f;
		}
		if (RightBottomEyelidDownMorph != null)
		{
			RightBottomEyelidDownMorph.morphValue = 0f;
		}
		if (LeftBottomEyelidUpMorph != null)
		{
			LeftBottomEyelidUpMorph.morphValue = 0f;
		}
		if (RightBottomEyelidUpMorph != null)
		{
			RightBottomEyelidUpMorph.morphValue = 0f;
		}
	}

	private void InitMorphs()
	{
		if ((bool)_morphBank)
		{
			DAZMorph builtInMorph = _morphBank.GetBuiltInMorph(LeftTopEyelidDownMorphName);
			if (builtInMorph != null)
			{
				LeftTopEyelidDownMorph = builtInMorph;
			}
			else
			{
				Debug.LogError("Could not get eyelid morph " + LeftTopEyelidDownMorphName);
			}
			builtInMorph = _morphBank.GetBuiltInMorph(RightTopEyelidDownMorphName);
			if (builtInMorph != null)
			{
				RightTopEyelidDownMorph = builtInMorph;
			}
			else
			{
				Debug.LogError("Could not get eyelid morph " + RightTopEyelidDownMorphName);
			}
			builtInMorph = _morphBank.GetBuiltInMorph(LeftTopEyelidUpMorphName);
			if (builtInMorph != null)
			{
				LeftTopEyelidUpMorph = builtInMorph;
			}
			else
			{
				Debug.LogError("Could not get eyelid morph " + LeftTopEyelidUpMorphName);
			}
			builtInMorph = _morphBank.GetBuiltInMorph(RightTopEyelidUpMorphName);
			if (builtInMorph != null)
			{
				RightTopEyelidUpMorph = builtInMorph;
			}
			else
			{
				Debug.LogError("Could not get eyelid morph " + RightTopEyelidUpMorphName);
			}
			builtInMorph = _morphBank.GetBuiltInMorph(LeftBottomEyelidDownMorphName);
			if (builtInMorph != null)
			{
				LeftBottomEyelidDownMorph = builtInMorph;
			}
			else
			{
				Debug.LogError("Could not get eyelid morph " + LeftBottomEyelidDownMorphName);
			}
			builtInMorph = _morphBank.GetBuiltInMorph(RightBottomEyelidDownMorphName);
			if (builtInMorph != null)
			{
				RightBottomEyelidDownMorph = builtInMorph;
			}
			else
			{
				Debug.LogError("Could not get eyelid morph " + RightBottomEyelidDownMorphName);
			}
			builtInMorph = _morphBank.GetBuiltInMorph(LeftBottomEyelidUpMorphName);
			if (builtInMorph != null)
			{
				LeftBottomEyelidUpMorph = builtInMorph;
			}
			else
			{
				Debug.LogError("Could not get eyelid morph " + LeftBottomEyelidUpMorphName);
			}
			builtInMorph = _morphBank.GetBuiltInMorph(RightBottomEyelidUpMorphName);
			if (builtInMorph != null)
			{
				RightBottomEyelidUpMorph = builtInMorph;
			}
			else
			{
				Debug.LogError("Could not get eyelid morph " + RightBottomEyelidUpMorphName);
			}
		}
	}

	private void Start()
	{
		InitMorphs();
	}

	private void UpdateBlink()
	{
		if (blinking)
		{
			if (currentWeight > targetWeight)
			{
				currentWeight -= Time.deltaTime / (blinkTime * (1f - blinkDownUpRatio));
			}
			else
			{
				currentWeight += Time.deltaTime / (blinkTime * blinkDownUpRatio);
			}
			if (currentWeight < 0f)
			{
				currentWeight = 0f;
				blinking = false;
			}
			else if (currentWeight > 1f)
			{
				currentWeight = 1f;
				BlinkOpen();
			}
			if (eyesControl != null)
			{
				eyesControl.blinkWeight = currentWeight;
			}
		}
		if (blinkEnabledJSON.val)
		{
			blinkStartTimer -= Time.deltaTime;
			if (blinkStartTimer < 0f)
			{
				Blink();
				blinkStartTimer = Random.Range(blinkSpaceMin, blinkSpaceMax);
			}
		}
	}

	private void UpdateWeights()
	{
		leftEyeWeight = currentWeight;
		if (leftEye != null && eyelidLookMorphsEnabledJSON.val)
		{
			float x = Quaternion2Angles.GetAngles(leftEye.localRotation, Quaternion2Angles.RotationOrder.ZYX).x;
			if (x > 0f)
			{
				leftEyeWeight += x * lookDownTopEyelidFactor;
				if (LeftBottomEyelidDownMorph != null)
				{
					LeftBottomEyelidDownMorph.morphValue = x * lookDownBottomEyelidFactor;
				}
				if (LeftTopEyelidUpMorph != null)
				{
					LeftTopEyelidUpMorph.morphValue = 0f;
				}
			}
			else
			{
				if (LeftBottomEyelidDownMorph != null)
				{
					LeftBottomEyelidDownMorph.morphValue = 0f;
				}
				if (LeftTopEyelidUpMorph != null)
				{
					float morphValue = (0f - x) * lookUpTopEyelidFactor * Mathf.Max(0f, 1f - leftEyeWeight);
					LeftTopEyelidUpMorph.morphValue = morphValue;
				}
				if (LeftBottomEyelidUpMorph != null)
				{
					LeftBottomEyelidUpMorph.morphValue = currentWeight * blinkBottomEyelidFactor + (0f - x) * lookUpBottomEyelidFactor;
				}
			}
		}
		if (LeftTopEyelidDownMorph != null)
		{
			LeftTopEyelidDownMorph.morphValue = leftEyeWeight;
		}
		rightEyeWeight = currentWeight;
		if (rightEye != null && eyelidLookMorphsEnabledJSON.val)
		{
			float x2 = Quaternion2Angles.GetAngles(rightEye.localRotation, Quaternion2Angles.RotationOrder.ZYX).x;
			if (x2 > 0f)
			{
				rightEyeWeight += x2 * lookDownTopEyelidFactor;
				if (RightBottomEyelidDownMorph != null)
				{
					RightBottomEyelidDownMorph.morphValue = x2 * lookDownBottomEyelidFactor;
				}
				if (RightTopEyelidUpMorph != null)
				{
					RightTopEyelidUpMorph.morphValue = 0f;
				}
				if (RightBottomEyelidUpMorph != null)
				{
					RightBottomEyelidUpMorph.morphValue = currentWeight * blinkBottomEyelidFactor;
				}
			}
			else
			{
				if (RightBottomEyelidDownMorph != null)
				{
					RightBottomEyelidDownMorph.morphValue = 0f;
				}
				if (RightTopEyelidUpMorph != null)
				{
					float morphValue2 = (0f - x2) * lookUpTopEyelidFactor * Mathf.Max(0f, 1f - rightEyeWeight);
					RightTopEyelidUpMorph.morphValue = morphValue2;
				}
				if (RightBottomEyelidUpMorph != null)
				{
					RightBottomEyelidUpMorph.morphValue = currentWeight * blinkBottomEyelidFactor + (0f - x2) * lookUpBottomEyelidFactor;
				}
			}
		}
		if (RightTopEyelidDownMorph != null)
		{
			RightTopEyelidDownMorph.morphValue = rightEyeWeight;
		}
	}

	private void Update()
	{
		UpdateBlink();
		UpdateWeights();
	}

	protected void Init()
	{
		blinkEnabledJSON = new JSONStorableBool("blinkEnabled", true, SyncBlinkEnabled);
		blinkEnabledJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(blinkEnabledJSON);
		eyelidLookMorphsEnabledJSON = new JSONStorableBool("eyelidLookMorphsEnabled", true, SyncEyelidLookMorphsEnabled);
		RegisterBool(eyelidLookMorphsEnabledJSON);
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		DAZMeshEyelidControlUI componentInChildren = UITransform.GetComponentInChildren<DAZMeshEyelidControlUI>(true);
		if (componentInChildren != null)
		{
			if (blinkEnabledJSON != null)
			{
				blinkEnabledJSON.toggle = componentInChildren.blinkEnabledToggle;
			}
			if (eyelidLookMorphsEnabledJSON != null)
			{
				eyelidLookMorphsEnabledJSON.toggle = componentInChildren.eyelidLookMorphsEnabledToggle;
			}
		}
	}

	public override void InitUIAlt()
	{
		if (!(UITransformAlt != null))
		{
			return;
		}
		DAZMeshEyelidControlUI componentInChildren = UITransformAlt.GetComponentInChildren<DAZMeshEyelidControlUI>(true);
		if (componentInChildren != null)
		{
			if (blinkEnabledJSON != null)
			{
				blinkEnabledJSON.toggleAlt = componentInChildren.blinkEnabledToggle;
			}
			if (eyelidLookMorphsEnabledJSON != null)
			{
				eyelidLookMorphsEnabledJSON.toggleAlt = componentInChildren.eyelidLookMorphsEnabledToggle;
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
