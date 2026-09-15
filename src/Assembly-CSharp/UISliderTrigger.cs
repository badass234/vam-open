using System;
using SimpleJSON;
using UnityEngine.UI;

public class UISliderTrigger : JSONStorableTriggerHandler
{
	public Trigger trigger;

	public Slider slider;

	protected JSONStorableFloat floatJSON;

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

	public override void Validate()
	{
		base.Validate();
		if (trigger != null)
		{
			trigger.Validate();
		}
	}

	protected void SyncFloat(float f)
	{
		if (trigger != null)
		{
			trigger.transitionInterpValue = floatJSON.val;
		}
	}

	protected void OnAtomRename(string oldid, string newid)
	{
		if (trigger != null)
		{
			trigger.SyncAtomNames();
		}
	}

	protected virtual void CreateFloatJSON()
	{
		floatJSON = new JSONStorableFloat("value", 0f, SyncFloat, 0f, 1f);
		floatJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(floatJSON);
		if (slider != null)
		{
			floatJSON.RegisterSlider(slider);
		}
	}

	protected void Init()
	{
		trigger = new Trigger();
		trigger.handler = this;
		trigger.active = true;
		CreateFloatJSON();
		if ((bool)SuperController.singleton)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Combine(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomRename));
		}
	}

	public override void InitUI()
	{
		if (UITransform != null && trigger != null)
		{
			UISliderTriggerUI componentInChildren = UITransform.GetComponentInChildren<UISliderTriggerUI>();
			if (componentInChildren != null)
			{
				trigger.triggerActionsParent = componentInChildren.transform;
				trigger.triggerPanel = componentInChildren.transform;
				trigger.triggerActionsPanel = componentInChildren.transform;
				trigger.InitTriggerUI();
				trigger.InitTriggerActionsUI();
			}
		}
	}

	protected virtual void OnEnable()
	{
		if (trigger != null)
		{
			trigger.active = true;
		}
	}

	protected virtual void OnDisable()
	{
		trigger.active = false;
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
