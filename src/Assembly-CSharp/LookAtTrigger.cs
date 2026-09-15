using System;
using SimpleJSON;
using UnityEngine;

public class LookAtTrigger : JSONStorableTriggerHandler
{
	public Trigger trigger;

	public float lookAtTime;

	public float lookAwayTime;

	public float lookPercent;

	protected bool isLookingAt;

	protected JSONStorableFloat activationTimeJSON;

	protected JSONStorableFloat deactivationTimeJSON;

	protected JSONStorableBool allowLookAwayDeactivationJSON;

	protected JSONStorableAction resetTriggerJSON;

	public string builtInTriggerSoundCategory = string.Empty;

	public string builtInTriggerSoundClip = string.Empty;

	public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		JSONClass jSON = base.GetJSON(includePhysical, includeAppearance, forceStore);
		if ((includePhysical || forceStore) && trigger != null && (trigger.HasActions() || forceStore))
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
		else if (setMissingToDefault && !base.mergeRestore)
		{
			trigger.RestoreFromJSON(new JSONClass());
		}
	}

	public void StartLookAt()
	{
		lookAtTime = lookPercent * activationTimeJSON.val;
		isLookingAt = true;
	}

	public void EndLookAt()
	{
		lookAwayTime = (1f - lookPercent) * deactivationTimeJSON.val;
		isLookingAt = false;
	}

	protected void SyncActivationTime(float f)
	{
	}

	protected void SyncDeactivationTime(float f)
	{
	}

	protected void SyncAllowLookAwayDeactivation(bool b)
	{
	}

	public void ResetTrigger()
	{
		lookPercent = 0f;
		lookAtTime = 0f;
		lookAwayTime = 0f;
		trigger.active = false;
		trigger.transitionInterpValue = 0f;
	}

	protected void OnAtomRename(string oldid, string newid)
	{
		if (trigger != null)
		{
			trigger.SyncAtomNames();
		}
	}

	public bool hasBuiltInSoundAction()
	{
		if (builtInTriggerSoundCategory != null && builtInTriggerSoundCategory != string.Empty && builtInTriggerSoundClip != null && builtInTriggerSoundClip != string.Empty)
		{
			return true;
		}
		return false;
	}

	protected void SetupBuiltInTrigger()
	{
		if (hasBuiltInSoundAction())
		{
			TriggerActionDiscrete triggerActionDiscrete = trigger.CreateDiscreteActionStartInternal();
			triggerActionDiscrete.receiverAtom = containingAtom;
			triggerActionDiscrete.SetReceiver("AudioSource");
			triggerActionDiscrete.SetReceiverTargetName("PlayNow");
			triggerActionDiscrete.SetAudioClipType("Embedded");
			triggerActionDiscrete.SetAudioClipCategory(builtInTriggerSoundCategory);
			triggerActionDiscrete.SetAudioClip(builtInTriggerSoundClip);
		}
	}

	protected void Init()
	{
		trigger = new Trigger();
		trigger.handler = this;
		activationTimeJSON = new JSONStorableFloat("activationTime", 0f, SyncActivationTime, 0f, 10f, false);
		activationTimeJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(activationTimeJSON);
		deactivationTimeJSON = new JSONStorableFloat("deactivationTime", 0f, SyncDeactivationTime, 0f, 10f, false);
		deactivationTimeJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(deactivationTimeJSON);
		allowLookAwayDeactivationJSON = new JSONStorableBool("allowLookAwayDeactivation", true, SyncAllowLookAwayDeactivation);
		allowLookAwayDeactivationJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(allowLookAwayDeactivationJSON);
		resetTriggerJSON = new JSONStorableAction("resetTrigger", ResetTrigger);
		RegisterAction(resetTriggerJSON);
		if ((bool)SuperController.singleton)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Combine(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomRename));
		}
	}

	public override void InitUI()
	{
		if (!(UITransform != null) || trigger == null)
		{
			return;
		}
		LookAtTriggerUI componentInChildren = UITransform.GetComponentInChildren<LookAtTriggerUI>();
		if (!(componentInChildren != null))
		{
			return;
		}
		if (componentInChildren.createDefaultSoundActionButton != null)
		{
			if (hasBuiltInSoundAction())
			{
				componentInChildren.createDefaultSoundActionButton.gameObject.SetActive(true);
				componentInChildren.createDefaultSoundActionButton.onClick.AddListener(SetupBuiltInTrigger);
			}
			else
			{
				componentInChildren.createDefaultSoundActionButton.gameObject.SetActive(false);
			}
		}
		if (activationTimeJSON != null)
		{
			activationTimeJSON.slider = componentInChildren.activationTimeSlider;
		}
		if (deactivationTimeJSON != null)
		{
			deactivationTimeJSON.slider = componentInChildren.deactivationTimeSlider;
		}
		if (allowLookAwayDeactivationJSON != null)
		{
			allowLookAwayDeactivationJSON.toggle = componentInChildren.allowLookAwayDeactivationToggle;
		}
		trigger.triggerActionsParent = componentInChildren.transform;
		trigger.triggerPanel = componentInChildren.transform;
		trigger.triggerActionsPanel = componentInChildren.transform;
		trigger.InitTriggerUI();
		trigger.InitTriggerActionsUI();
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
			InitUI();
		}
	}

	protected void Update()
	{
		if (isLookingAt)
		{
			if (activationTimeJSON.val > 0f)
			{
				lookAtTime += Time.unscaledDeltaTime;
				lookPercent = Mathf.Clamp01(lookAtTime / activationTimeJSON.val);
			}
			else
			{
				lookPercent = 1f;
			}
			trigger.ForceTransitionsActive();
			trigger.transitionInterpValue = lookPercent;
			if (lookPercent == 1f)
			{
				trigger.active = true;
			}
		}
		else if (allowLookAwayDeactivationJSON.val)
		{
			if (deactivationTimeJSON.val > 0f)
			{
				if (lookAwayTime <= deactivationTimeJSON.val)
				{
					lookAwayTime += Time.unscaledDeltaTime;
					lookPercent = Mathf.Clamp01(1f - lookAwayTime / deactivationTimeJSON.val);
				}
				else
				{
					lookPercent = 0f;
				}
			}
			else
			{
				lookPercent = 0f;
			}
			trigger.transitionInterpValue = lookPercent;
			if (lookPercent == 0f)
			{
				trigger.active = false;
			}
		}
		trigger.Update();
	}

	protected void OnDestroy()
	{
		if (trigger != null)
		{
			trigger.Remove();
		}
		if ((bool)SuperController.singleton)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Remove(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomRename));
		}
	}
}
