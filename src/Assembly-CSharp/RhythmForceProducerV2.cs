using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class RhythmForceProducerV2 : ForceProducerV2
{
	public enum RangeSelect
	{
		Low,
		Mid,
		High
	}

	protected string[] customParamNamesOverride = new string[2] { "receiver", "rhythmController" };

	public UIPopup rhythmControllerAtomSelectionPopup;

	public UIPopup rhythmControllerSelectionPopup;

	protected string rhythmControllerAtomUID;

	[SerializeField]
	protected RhythmController _rhythmController;

	protected JSONStorableFloat alternateBeatRatioJSON;

	[SerializeField]
	protected float _alternateBeatRatio = 1f;

	protected JSONStorableFloat thresholdJSON;

	[SerializeField]
	protected float _threshold = 1f;

	protected JSONStorableFloat burstLengthJSON;

	[SerializeField]
	protected float _burstLength = 1f;

	protected JSONStorableFloat minSpacingJSON;

	[SerializeField]
	protected float _minSpacing = 0.4f;

	protected JSONStorableFloat randomFactorJSON;

	[SerializeField]
	protected float _randomFactor = 0.1f;

	public RangeSelect rangeSelect;

	protected JSONStorableStringChooser rangeSelectJSON;

	public Material rhythmLineMaterial;

	public Material rawRhythmLineMaterial;

	protected float minThreshold = 1f;

	protected float flip = 1f;

	protected bool timerOn;

	protected float timer;

	protected float forceTimer;

	protected float maxOnset = 60f;

	public float peakOnset;

	protected float oneOverMaxOnset;

	protected float onsetMult = 2f;

	protected float rawOnset;

	protected float onset;

	protected LineDrawer rhythmLineDrawer;

	protected LineDrawer rawRhythmLineDrawer;

	protected float rhythmLineLength;

	protected float rawRhythmLineLength;

	public virtual RhythmController rhythmController
	{
		get
		{
			return _rhythmController;
		}
		set
		{
			_rhythmController = value;
		}
	}

	public float alternateBeatRatio
	{
		get
		{
			return _alternateBeatRatio;
		}
		set
		{
			if (alternateBeatRatioJSON != null)
			{
				alternateBeatRatioJSON.val = value;
			}
			else if (_alternateBeatRatio != value)
			{
				SyncAlternateBeatRatio(value);
			}
		}
	}

	public float threshold
	{
		get
		{
			return _threshold;
		}
		set
		{
			if (thresholdJSON != null)
			{
				thresholdJSON.val = value;
			}
			else if (_threshold != value)
			{
				SyncThreshold(value);
			}
		}
	}

	public float burstLength
	{
		get
		{
			return _burstLength;
		}
		set
		{
			if (burstLengthJSON != null)
			{
				burstLengthJSON.val = value;
			}
			else if (_burstLength != value)
			{
				SyncBurstLength(value);
			}
		}
	}

	public float minSpacing
	{
		get
		{
			return _minSpacing;
		}
		set
		{
			if (minSpacingJSON != null)
			{
				minSpacingJSON.val = value;
			}
			else if (_minSpacing != value)
			{
				SyncMinSpacing(value);
			}
		}
	}

	public float randomFactor
	{
		get
		{
			return _randomFactor;
		}
		set
		{
			if (randomFactorJSON != null)
			{
				randomFactorJSON.val = value;
			}
			else if (_randomFactor != value)
			{
				SyncRandomFactor(value);
			}
		}
	}

	public override string[] GetCustomParamNames()
	{
		return customParamNamesOverride;
	}

	public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		JSONClass jSON = base.GetJSON(includePhysical, includeAppearance, forceStore);
		if ((includePhysical || forceStore) && _rhythmController != null && _rhythmController.containingAtom != null)
		{
			string text = AtomUidToStoreAtomUid(_rhythmController.containingAtom.uid);
			if (text != null)
			{
				needsStore = true;
				jSON["rhythmController"] = text + ":" + _rhythmController.name;
			}
			else
			{
				SuperController.LogError(string.Concat("Warning: RhythmForceProducer in atom ", containingAtom, " uses rhythm controller in atom ", _rhythmController.containingAtom.uid, " that is not in subscene and cannot be saved"));
			}
		}
		return jSON;
	}

	public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
	{
		base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
		if (!base.physicalLocked && restorePhysical && !IsCustomPhysicalParamLocked("rhythmController"))
		{
			if (jc["rhythmController"] != null)
			{
				string text = StoredAtomUidToAtomUid(jc["rhythmController"]);
				SetRhythmController(text);
			}
			else if (setMissingToDefault)
			{
				SetRhythmController(string.Empty);
			}
		}
	}

	protected override void OnAtomUIDRename(string fromid, string toid)
	{
		base.OnAtomUIDRename(fromid, toid);
		if (rhythmController != null && rhythmControllerAtomSelectionPopup != null)
		{
			rhythmControllerAtomSelectionPopup.currentValueNoCallback = rhythmController.containingAtom.uid;
		}
	}

	protected virtual void SetRhythmControllerAtomNames()
	{
		if (!(rhythmControllerAtomSelectionPopup != null) || !(SuperController.singleton != null))
		{
			return;
		}
		List<string> atomUIDsWithRhythmControllers = SuperController.singleton.GetAtomUIDsWithRhythmControllers();
		if (atomUIDsWithRhythmControllers == null)
		{
			rhythmControllerAtomSelectionPopup.numPopupValues = 1;
			rhythmControllerAtomSelectionPopup.setPopupValue(0, "None");
			return;
		}
		rhythmControllerAtomSelectionPopup.numPopupValues = atomUIDsWithRhythmControllers.Count + 1;
		rhythmControllerAtomSelectionPopup.setPopupValue(0, "None");
		for (int i = 0; i < atomUIDsWithRhythmControllers.Count; i++)
		{
			rhythmControllerAtomSelectionPopup.setPopupValue(i + 1, atomUIDsWithRhythmControllers[i]);
		}
	}

	protected virtual void onRhythmControllerNamesChanged(List<string> rcNames)
	{
		if (!(rhythmControllerSelectionPopup != null))
		{
			return;
		}
		if (rcNames == null)
		{
			rhythmControllerSelectionPopup.numPopupValues = 1;
			rhythmControllerSelectionPopup.setPopupValue(0, "None");
			return;
		}
		rhythmControllerSelectionPopup.numPopupValues = rcNames.Count + 1;
		rhythmControllerSelectionPopup.setPopupValue(0, "None");
		for (int i = 0; i < rcNames.Count; i++)
		{
			rhythmControllerSelectionPopup.setPopupValue(i + 1, rcNames[i]);
		}
	}

	public virtual void SetRhythmControllerAtom(string atomUID)
	{
		if (!(SuperController.singleton != null))
		{
			return;
		}
		Atom atomByUid = SuperController.singleton.GetAtomByUid(atomUID);
		if (atomByUid != null)
		{
			rhythmControllerAtomUID = atomUID;
			List<string> rhythmControllerNamesInAtom = SuperController.singleton.GetRhythmControllerNamesInAtom(rhythmControllerAtomUID);
			onRhythmControllerNamesChanged(rhythmControllerNamesInAtom);
			if (rhythmControllerSelectionPopup != null)
			{
				rhythmControllerSelectionPopup.currentValue = "None";
			}
		}
		else
		{
			onRhythmControllerNamesChanged(null);
		}
	}

	public virtual void SetRhythmControllerObject(string objectName)
	{
		if (rhythmControllerAtomUID != null && SuperController.singleton != null)
		{
			rhythmController = SuperController.singleton.RhythmControllerrNameToRhythmController(rhythmControllerAtomUID + ":" + objectName);
		}
	}

	public virtual void SetRhythmController(string controllerName)
	{
		if (!(SuperController.singleton != null))
		{
			return;
		}
		RhythmController rhythmController = SuperController.singleton.RhythmControllerrNameToRhythmController(controllerName);
		if (rhythmController != null)
		{
			if (rhythmControllerAtomSelectionPopup != null && rhythmController.containingAtom != null)
			{
				rhythmControllerAtomSelectionPopup.currentValue = rhythmController.containingAtom.uid;
			}
			if (rhythmControllerSelectionPopup != null)
			{
				rhythmControllerSelectionPopup.currentValue = rhythmController.name;
			}
		}
		else
		{
			if (rhythmControllerAtomSelectionPopup != null)
			{
				rhythmControllerAtomSelectionPopup.currentValue = "None";
			}
			if (rhythmControllerSelectionPopup != null)
			{
				rhythmControllerSelectionPopup.currentValue = "None";
			}
		}
		this.rhythmController = rhythmController;
	}

	protected void SyncAlternateBeatRatio(float f)
	{
		_alternateBeatRatio = f;
	}

	protected void SyncThreshold(float f)
	{
		_threshold = f;
	}

	protected void SyncBurstLength(float f)
	{
		_burstLength = f;
	}

	protected void SyncMinSpacing(float f)
	{
		_minSpacing = f;
	}

	protected void SyncRandomFactor(float f)
	{
		_randomFactor = f;
	}

	public void SetRangeSelect(string range)
	{
		try
		{
			rangeSelect = (RangeSelect)Enum.Parse(typeof(RangeSelect), range, true);
		}
		catch (ArgumentException)
		{
		}
	}

	protected override void Init()
	{
		base.Init();
		if (rhythmController != null)
		{
			rhythmControllerAtomUID = rhythmController.containingAtom.uid;
		}
		string[] names = Enum.GetNames(typeof(RangeSelect));
		List<string> choicesList = new List<string>(names);
		rangeSelectJSON = new JSONStorableStringChooser("range", choicesList, rangeSelect.ToString(), "Pitch Range", SetRangeSelect);
		rangeSelectJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterStringChooser(rangeSelectJSON);
		alternateBeatRatioJSON = new JSONStorableFloat("alternateBeatRatio", _alternateBeatRatio, SyncAlternateBeatRatio, 0f, 1f);
		alternateBeatRatioJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(alternateBeatRatioJSON);
		burstLengthJSON = new JSONStorableFloat("burstLength", _burstLength, SyncBurstLength, 0f, 10f, false);
		burstLengthJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(burstLengthJSON);
		minSpacingJSON = new JSONStorableFloat("minSpacing", _minSpacing, SyncMinSpacing, 0f, 10f, false);
		minSpacingJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(minSpacingJSON);
		randomFactorJSON = new JSONStorableFloat("randomFactor", _randomFactor, SyncRandomFactor, 0f, 1f);
		randomFactorJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(randomFactorJSON);
		thresholdJSON = new JSONStorableFloat("threshold", _threshold, SyncThreshold, 1f, 100f);
		thresholdJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(thresholdJSON);
		if ((bool)rhythmLineMaterial)
		{
			rhythmLineDrawer = new LineDrawer(rhythmLineMaterial);
		}
		if ((bool)rawRhythmLineMaterial)
		{
			rawRhythmLineDrawer = new LineDrawer(rawRhythmLineMaterial);
		}
	}

	public override void InitUI()
	{
		base.InitUI();
		if (!(UITransform != null))
		{
			return;
		}
		RhythmForceProducerV2UI componentInChildren = UITransform.GetComponentInChildren<RhythmForceProducerV2UI>(true);
		if (!(componentInChildren != null))
		{
			return;
		}
		rangeSelectJSON.popup = componentInChildren.rangeSelectPopup;
		alternateBeatRatioJSON.slider = componentInChildren.alternateBeatRatioSlider;
		burstLengthJSON.slider = componentInChildren.burstLengthSlider;
		minSpacingJSON.slider = componentInChildren.minSpacingSlider;
		randomFactorJSON.slider = componentInChildren.randomFactorSlider;
		thresholdJSON.slider = componentInChildren.thresholdSlider;
		rhythmControllerAtomSelectionPopup = componentInChildren.rhythmControllerAtomSelectionPopup;
		if (rhythmControllerAtomSelectionPopup != null)
		{
			if (rhythmController != null)
			{
				if (rhythmController.containingAtom != null)
				{
					SetRhythmControllerAtom(rhythmController.containingAtom.uid);
					rhythmControllerAtomSelectionPopup.currentValue = rhythmController.containingAtom.uid;
				}
				else
				{
					rhythmControllerAtomSelectionPopup.currentValue = "None";
				}
			}
			else
			{
				rhythmControllerAtomSelectionPopup.currentValue = "None";
			}
			UIPopup uIPopup = rhythmControllerAtomSelectionPopup;
			uIPopup.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Combine(uIPopup.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetRhythmControllerAtomNames));
			UIPopup uIPopup2 = rhythmControllerAtomSelectionPopup;
			uIPopup2.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup2.onValueChangeHandlers, new UIPopup.OnValueChange(SetRhythmControllerAtom));
		}
		rhythmControllerSelectionPopup = componentInChildren.rhythmControllerSelectionPopup;
		if (rhythmControllerSelectionPopup != null)
		{
			if (rhythmController != null)
			{
				rhythmControllerSelectionPopup.currentValueNoCallback = rhythmController.name;
			}
			else
			{
				onRhythmControllerNamesChanged(null);
				rhythmControllerSelectionPopup.currentValue = "None";
			}
			UIPopup uIPopup3 = rhythmControllerSelectionPopup;
			uIPopup3.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup3.onValueChangeHandlers, new UIPopup.OnValueChange(SetRhythmControllerObject));
		}
	}

	public override void InitUIAlt()
	{
		base.InitUIAlt();
		if (UITransformAlt != null)
		{
			RhythmForceProducerV2UI componentInChildren = UITransformAlt.GetComponentInChildren<RhythmForceProducerV2UI>(true);
			if (componentInChildren != null)
			{
				rangeSelectJSON.popupAlt = componentInChildren.rangeSelectPopup;
				alternateBeatRatioJSON.sliderAlt = componentInChildren.alternateBeatRatioSlider;
				burstLengthJSON.sliderAlt = componentInChildren.burstLengthSlider;
				minSpacingJSON.sliderAlt = componentInChildren.minSpacingSlider;
				randomFactorJSON.sliderAlt = componentInChildren.randomFactorSlider;
				thresholdJSON.sliderAlt = componentInChildren.thresholdSlider;
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		oneOverMaxOnset = 1f / maxOnset;
	}

	protected override void Update()
	{
		base.Update();
		forceTimer -= Time.deltaTime;
		if (forceTimer > 0f)
		{
			SetTargetForcePercent(flip * onset * oneOverMaxOnset);
			if (onset > peakOnset)
			{
				peakOnset = onset;
			}
		}
		else
		{
			SetTargetForcePercent(0f);
		}
		if (!(rhythmController != null) || rhythmController.rhythmTool == null || rhythmController.low == null)
		{
			return;
		}
		float num;
		if (!rhythmController.IsPlaying)
		{
			num = 0f;
		}
		else if (rangeSelect != 0)
		{
			num = ((rangeSelect != RangeSelect.Mid) ? (rhythmController.high[rhythmController.rhythmTool.CurrentFrame].onset * onsetMult) : (rhythmController.mid[rhythmController.rhythmTool.CurrentFrame].onset * onsetMult));
		}
		else if (rhythmController.rhythmTool.CurrentFrame >= rhythmController.low.Length)
		{
			Debug.LogError("Rhythm frame " + rhythmController.rhythmTool.CurrentFrame + " is greater than analysis length " + rhythmController.low.Length);
			num = 0f;
		}
		else
		{
			num = rhythmController.low[rhythmController.rhythmTool.CurrentFrame].onset * onsetMult;
		}
		if (num > maxOnset)
		{
			num = maxOnset;
		}
		if (num > minThreshold)
		{
			rawOnset = num;
			rawRhythmLineLength = rawOnset * linesScale * _forceFactor * oneOverMaxOnset;
		}
		if (timerOn)
		{
			timer -= Time.deltaTime;
			if (timer < 0f)
			{
				timerOn = false;
			}
		}
		else
		{
			onset = num;
			if (onset > threshold)
			{
				if (flip > 0f)
				{
					flip = 0f - _alternateBeatRatio;
				}
				else
				{
					flip = 1f;
				}
				timerOn = true;
				timer = _minSpacing;
				forceTimer = _burstLength;
				if (UnityEngine.Random.value < _randomFactor)
				{
					timer += _minSpacing;
					forceTimer += _burstLength;
				}
				rhythmLineLength = rawRhythmLineLength * flip;
			}
		}
		rawRhythmLineLength = Mathf.Lerp(rawRhythmLineLength, 0f, Time.deltaTime * 5f);
		if (on && receiver != null && drawLines)
		{
			Vector3 vector = AxisToVector(forceAxis);
			Vector3 vector2 = AxisToUpVector(forceAxis);
			Vector3 vector3 = base.transform.position + vector2 * (lineOffset + lineSpacing * 10f);
			if (rhythmLineDrawer != null)
			{
				rhythmLineDrawer.SetLinePoints(vector3, vector3 + vector * rhythmLineLength);
				rhythmLineDrawer.Draw(base.gameObject.layer);
			}
			vector3 += vector2 * lineSpacing;
			if (rawRhythmLineDrawer != null)
			{
				rawRhythmLineDrawer.SetLinePoints(vector3, vector3 + vector * (0f - rawRhythmLineLength));
				rawRhythmLineDrawer.Draw(base.gameObject.layer);
			}
		}
	}
}
