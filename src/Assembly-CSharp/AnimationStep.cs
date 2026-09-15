using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class AnimationStep : CubicBezierPoint, TriggerHandler
{
	public enum CurveType
	{
		Linear,
		EaseInOut
	}

	public Trigger trigger;

	public RectTransform triggerActionsPrefab;

	public RectTransform triggerActionMiniPrefab;

	public RectTransform triggerActionDiscretePrefab;

	public RectTransform triggerActionTransitionPrefab;

	public AnimationPattern animationParent;

	[SerializeField]
	protected int _stepNumber;

	public Text stepNameText;

	protected bool _active;

	protected float _stepRatio;

	public float timeStep;

	protected JSONStorableFloat transitionToTimeJSON;

	protected float _startingTransitionTime;

	[SerializeField]
	protected float _transitionToTime = 1f;

	protected JSONStorableStringChooser curveTypeJSON;

	[SerializeField]
	protected CurveType _curveType;

	public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	protected FreeControllerV3 fcv3;

	public int stepNumber
	{
		get
		{
			return _stepNumber;
		}
		set
		{
			if (_stepNumber == value)
			{
				return;
			}
			_stepNumber = value;
			if (stepNameText != null)
			{
				string text = string.Empty;
				if (animationParent != null)
				{
					text = animationParent.uid;
				}
				stepNameText.text = text + " Step " + _stepNumber;
			}
		}
	}

	public bool active
	{
		get
		{
			return _active;
		}
		set
		{
			if (_active != value)
			{
				_active = value;
				if (trigger != null)
				{
					trigger.active = _active;
				}
			}
		}
	}

	public float stepRatio
	{
		get
		{
			return _stepRatio;
		}
		set
		{
			if (_stepRatio != value)
			{
				_stepRatio = value;
				if (trigger != null)
				{
					trigger.transitionInterpValue = _stepRatio;
				}
			}
		}
	}

	public float transitionToTime
	{
		get
		{
			return _transitionToTime;
		}
		set
		{
			if (transitionToTimeJSON != null)
			{
				transitionToTimeJSON.val = value;
			}
			else if (_transitionToTime != value)
			{
				SyncTransitionToTime(value);
			}
		}
	}

	public CurveType curveType
	{
		get
		{
			return _curveType;
		}
		set
		{
			if (curveTypeJSON != null)
			{
				curveTypeJSON.val = value.ToString();
			}
			else if (_curveType != value)
			{
				_curveType = value;
				SyncCurveType();
			}
		}
	}

	public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		JSONClass jSON = base.GetJSON(includePhysical, includeAppearance, forceStore);
		if ((includePhysical || forceStore) && trigger != null)
		{
			needsStore = true;
			jSON["trigger"] = trigger.GetJSON(base.subScenePrefix);
		}
		return jSON;
	}

	public override void LateRestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
	{
		base.LateRestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
		if (base.physicalLocked || !restorePhysical || IsCustomPhysicalParamLocked("trigger"))
		{
			return;
		}
		if (jc["trigger"] != null)
		{
			JSONClass asObject = jc["trigger"].AsObject;
			if (asObject != null)
			{
				trigger.RestoreFromJSON(asObject, base.subScenePrefix, base.mergeRestore);
			}
		}
		else if (setMissingToDefault)
		{
			trigger.RestoreFromJSON(new JSONClass());
		}
	}

	public override void Validate()
	{
		base.Validate();
		if (trigger != null)
		{
			trigger.Validate();
		}
	}

	protected void OnAtomRename(string oldid, string newid)
	{
		if (trigger != null)
		{
			trigger.SyncAtomNames();
		}
	}

	public virtual void RemoveTrigger(Trigger trigger)
	{
	}

	public virtual void DuplicateTrigger(Trigger trigger)
	{
	}

	public RectTransform CreateTriggerActionsUI()
	{
		RectTransform result = null;
		if (triggerActionsPrefab != null)
		{
			result = UnityEngine.Object.Instantiate(triggerActionsPrefab);
		}
		else
		{
			Debug.LogError("Attempted to make TriggerActionsUI when prefab was not set");
		}
		return result;
	}

	public RectTransform CreateTriggerActionMiniUI()
	{
		RectTransform result = null;
		if (triggerActionMiniPrefab != null)
		{
			result = UnityEngine.Object.Instantiate(triggerActionMiniPrefab);
		}
		else
		{
			Debug.LogError("Attempted to make TriggerActionMiniUI when prefab was not set");
		}
		return result;
	}

	public RectTransform CreateTriggerActionDiscreteUI()
	{
		RectTransform result = null;
		if (triggerActionDiscretePrefab != null)
		{
			result = UnityEngine.Object.Instantiate(triggerActionDiscretePrefab);
		}
		else
		{
			Debug.LogError("Attempted to make TriggerActionDiscreteUI when prefab was not set");
		}
		return result;
	}

	public RectTransform CreateTriggerActionTransitionUI()
	{
		RectTransform result = null;
		if (triggerActionTransitionPrefab != null)
		{
			result = UnityEngine.Object.Instantiate(triggerActionTransitionPrefab);
		}
		else
		{
			Debug.LogError("Attempted to make TriggerActionTransitionUI when prefab was not set");
		}
		return result;
	}

	public void RemoveTriggerActionUI(RectTransform rt)
	{
		if (rt != null)
		{
			UnityEngine.Object.Destroy(rt.gameObject);
		}
	}

	protected void SyncTransitionToTime(float f)
	{
		_transitionToTime = f;
		if (animationParent != null)
		{
			animationParent.RecalculateTimeSteps();
		}
	}

	protected void SyncCurveType()
	{
		switch (_curveType)
		{
		case CurveType.Linear:
			curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
			break;
		case CurveType.EaseInOut:
			curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
			break;
		}
	}

	protected void SetCurveType(string type)
	{
		try
		{
			CurveType curveType = (CurveType)Enum.Parse(typeof(CurveType), type);
			_curveType = curveType;
			SyncCurveType();
		}
		catch (ArgumentException)
		{
			Debug.LogError("Attempted to set curve type to " + type + " which is not a valid curve type");
		}
	}

	public void CreateStepBefore()
	{
		if (animationParent != null)
		{
			animationParent.CreateStepBeforeStep(this);
		}
	}

	public void CreateStepAfter()
	{
		if (animationParent != null)
		{
			animationParent.CreateStepAfterStep(this);
		}
	}

	public void DestroyStep()
	{
		if (animationParent != null)
		{
			DeregisterUI();
			animationParent.DestroyStep(this);
		}
		else if (containingAtom != null)
		{
			DeregisterUI();
			SuperController.singleton.RemoveAtom(containingAtom);
		}
	}

	public void AlignPositionToRoot()
	{
		if (animationParent != null && point != null)
		{
			if (fcv3 != null)
			{
				fcv3.MoveControl(animationParent.transform.position);
			}
			else
			{
				point.position = animationParent.transform.position;
			}
		}
	}

	public void AlignRotationToRoot()
	{
		if (animationParent != null && point != null)
		{
			if (fcv3 != null)
			{
				fcv3.RotateControl(animationParent.transform.eulerAngles);
			}
			else
			{
				point.rotation = animationParent.transform.rotation;
			}
		}
	}

	public void AlignPositionToReceiver()
	{
		if (animationParent != null && animationParent.animatedTransform != null)
		{
			MoveProducer component = animationParent.animatedTransform.GetComponent<MoveProducer>();
			Transform transform = ((!(component != null) || !(component.receiver != null)) ? animationParent.animatedTransform : component.receiver.transform);
			if (fcv3 != null)
			{
				fcv3.MoveControl(transform.position);
			}
			else
			{
				point.position = transform.position;
			}
		}
	}

	public void AlignRotationToReceiver()
	{
		if (animationParent != null && animationParent.animatedTransform != null)
		{
			MoveProducer component = animationParent.animatedTransform.GetComponent<MoveProducer>();
			Transform transform = ((!(component != null) || !(component.receiver != null)) ? animationParent.animatedTransform : component.receiver.transform);
			if (fcv3 != null)
			{
				fcv3.RotateControl(transform.eulerAngles);
			}
			else
			{
				point.rotation = transform.rotation;
			}
		}
	}

	public override void InitUI()
	{
		if (UITransform != null)
		{
			AnimationStepUI componentInChildren = UITransform.GetComponentInChildren<AnimationStepUI>(true);
			if (componentInChildren != null)
			{
				transitionToTimeJSON.slider = componentInChildren.transitionToTimeSlider;
				curveTypeJSON.popup = componentInChildren.curveTypePopup;
			}
			if (componentInChildren.createStepBeforeButton != null)
			{
				componentInChildren.createStepBeforeButton.onClick.AddListener(CreateStepBefore);
			}
			if (componentInChildren.createStepAfterButton != null)
			{
				componentInChildren.createStepAfterButton.onClick.AddListener(CreateStepAfter);
			}
			if (componentInChildren.alignPositionToRootButton != null)
			{
				componentInChildren.alignPositionToRootButton.onClick.AddListener(AlignPositionToRoot);
			}
			if (componentInChildren.alignRotationToRootButton != null)
			{
				componentInChildren.alignRotationToRootButton.onClick.AddListener(AlignRotationToRoot);
			}
			if (componentInChildren.alignPositionToReceiverButton != null)
			{
				componentInChildren.alignPositionToReceiverButton.onClick.AddListener(AlignPositionToReceiver);
			}
			if (componentInChildren.alignRotationToReceiverButton != null)
			{
				componentInChildren.alignRotationToReceiverButton.onClick.AddListener(AlignRotationToReceiver);
			}
			if (componentInChildren.removeStepButton != null)
			{
				componentInChildren.removeStepButton.onClick.AddListener(DestroyStep);
			}
			AnimationStepTriggerUI componentInChildren2 = UITransform.GetComponentInChildren<AnimationStepTriggerUI>();
			if (componentInChildren2 != null)
			{
				trigger.triggerActionsParent = componentInChildren2.transform;
				trigger.triggerPanel = componentInChildren2.transform;
				trigger.triggerActionsPanel = componentInChildren2.transform;
				trigger.InitTriggerUI();
				trigger.InitTriggerActionsUI();
			}
		}
	}

	public override void InitUIAlt()
	{
		if (UITransformAlt != null)
		{
			AnimationStepUI componentInChildren = UITransformAlt.GetComponentInChildren<AnimationStepUI>(true);
			if (componentInChildren != null)
			{
				transitionToTimeJSON.sliderAlt = componentInChildren.transitionToTimeSlider;
				curveTypeJSON.popupAlt = componentInChildren.curveTypePopup;
			}
			if (componentInChildren.createStepBeforeButton != null)
			{
				componentInChildren.createStepBeforeButton.onClick.AddListener(CreateStepBefore);
			}
			if (componentInChildren.createStepAfterButton != null)
			{
				componentInChildren.createStepAfterButton.onClick.AddListener(CreateStepAfter);
			}
			if (componentInChildren.alignPositionToRootButton != null)
			{
				componentInChildren.alignPositionToRootButton.onClick.AddListener(AlignPositionToRoot);
			}
			if (componentInChildren.alignRotationToRootButton != null)
			{
				componentInChildren.alignRotationToRootButton.onClick.AddListener(AlignRotationToRoot);
			}
			if (componentInChildren.alignPositionToReceiverButton != null)
			{
				componentInChildren.alignPositionToReceiverButton.onClick.AddListener(AlignPositionToReceiver);
			}
			if (componentInChildren.alignRotationToReceiverButton != null)
			{
				componentInChildren.alignRotationToReceiverButton.onClick.AddListener(AlignRotationToReceiver);
			}
			if (componentInChildren.removeStepButton != null)
			{
				componentInChildren.removeStepButton.onClick.AddListener(DestroyStep);
			}
		}
	}

	protected void DeregisterUI()
	{
		transitionToTimeJSON.slider = null;
		transitionToTimeJSON.sliderAlt = null;
		curveTypeJSON.popup = null;
		curveTypeJSON.popupAlt = null;
		if (UITransform != null)
		{
			AnimationStepUI componentInChildren = UITransform.GetComponentInChildren<AnimationStepUI>();
			if (componentInChildren != null)
			{
				if ((bool)componentInChildren.createStepBeforeButton)
				{
					componentInChildren.createStepBeforeButton.onClick.RemoveAllListeners();
				}
				if ((bool)componentInChildren.createStepAfterButton)
				{
					componentInChildren.createStepAfterButton.onClick.RemoveAllListeners();
				}
				if ((bool)componentInChildren.alignPositionToRootButton)
				{
					componentInChildren.alignPositionToRootButton.onClick.RemoveAllListeners();
				}
				if ((bool)componentInChildren.alignRotationToRootButton)
				{
					componentInChildren.alignRotationToRootButton.onClick.RemoveAllListeners();
				}
				if (componentInChildren.alignPositionToReceiverButton != null)
				{
					componentInChildren.alignPositionToReceiverButton.onClick.RemoveAllListeners();
				}
				if (componentInChildren.alignRotationToReceiverButton != null)
				{
					componentInChildren.alignRotationToReceiverButton.onClick.RemoveAllListeners();
				}
				if ((bool)componentInChildren.removeStepButton)
				{
					componentInChildren.removeStepButton.onClick.RemoveAllListeners();
				}
			}
		}
		if (!(UITransformAlt != null))
		{
			return;
		}
		AnimationStepUI componentInChildren2 = UITransformAlt.GetComponentInChildren<AnimationStepUI>();
		if (componentInChildren2 != null)
		{
			if ((bool)componentInChildren2.createStepBeforeButton)
			{
				componentInChildren2.createStepBeforeButton.onClick.RemoveAllListeners();
			}
			if ((bool)componentInChildren2.createStepAfterButton)
			{
				componentInChildren2.createStepAfterButton.onClick.RemoveAllListeners();
			}
			if ((bool)componentInChildren2.alignPositionToRootButton)
			{
				componentInChildren2.alignPositionToRootButton.onClick.RemoveAllListeners();
			}
			if ((bool)componentInChildren2.alignRotationToRootButton)
			{
				componentInChildren2.alignRotationToRootButton.onClick.RemoveAllListeners();
			}
			if (componentInChildren2.alignPositionToReceiverButton != null)
			{
				componentInChildren2.alignPositionToReceiverButton.onClick.RemoveAllListeners();
			}
			if (componentInChildren2.alignRotationToReceiverButton != null)
			{
				componentInChildren2.alignRotationToReceiverButton.onClick.RemoveAllListeners();
			}
			if ((bool)componentInChildren2.removeStepButton)
			{
				componentInChildren2.removeStepButton.onClick.RemoveAllListeners();
			}
		}
	}

	protected void Init()
	{
		if (point != null)
		{
			fcv3 = point.GetComponent<FreeControllerV3>();
		}
		trigger = new Trigger();
		trigger.handler = this;
		transitionToTimeJSON = new JSONStorableFloat("transitionToTime", _transitionToTime, SyncTransitionToTime, 0f, 10f, false);
		transitionToTimeJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(transitionToTimeJSON);
		string[] names = Enum.GetNames(typeof(CurveType));
		List<string> choicesList = new List<string>(names);
		curveTypeJSON = new JSONStorableStringChooser("curveType", choicesList, _curveType.ToString(), "Curve Type", SetCurveType);
		curveTypeJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterStringChooser(curveTypeJSON);
		if ((bool)SuperController.singleton)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Combine(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomRename));
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

	private void OnDestroy()
	{
		if ((bool)SuperController.singleton)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Remove(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomRename));
		}
		if (animationParent != null)
		{
			animationParent.RemoveStep(this);
		}
	}
}
