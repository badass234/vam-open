using System;
using SimpleJSON;
using UnityEngine.UI;

public class UIToggleTrigger : JSONStorableTriggerHandler
{
	public Trigger trigger;

	public Toggle toggle;

	protected JSONStorableBool boolJSON;

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

	protected void SyncBool(bool b)
	{
		if (trigger != null)
		{
			if (b)
			{
				trigger.reverse = false;
				trigger.active = true;
			}
			else
			{
				trigger.reverse = true;
				trigger.active = false;
			}
		}
	}

	protected void OnAtomRename(string oldid, string newid)
	{
		if (trigger != null)
		{
			trigger.SyncAtomNames();
		}
	}

	protected virtual void CreateBoolJSON()
	{
		boolJSON = new JSONStorableBool("value", false, SyncBool);
		boolJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(boolJSON);
		if (toggle != null)
		{
			boolJSON.RegisterToggle(toggle);
		}
	}

	protected void Init()
	{
		trigger = new Trigger();
		trigger.handler = this;
		CreateBoolJSON();
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
			UIToggleTriggerUI componentInChildren = UITransform.GetComponentInChildren<UIToggleTriggerUI>();
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
		if (boolJSON != null)
		{
			SyncBool(boolJSON.val);
		}
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
		if (trigger != null)
		{
			trigger.Update();
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
