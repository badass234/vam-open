using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class VariableTrigger : JSONStorableTriggerHandler
{
	protected Atom driverAtom;

	protected JSONStorableStringChooser driverAtomJSON;

	protected string _missingDriverStoreId = string.Empty;

	protected JSONStorable driver;

	protected JSONStorableStringChooser driverJSON;

	protected string _driverTargetName;

	protected JSONStorableFloat driverTarget;

	protected JSONStorableStringChooser driverTargetJSON;

	protected UIDynamicSlider driverStartValDynamicSlider;

	protected JSONStorableFloat driverStartValJSON;

	protected UIDynamicSlider driverEndValDynamicSlider;

	protected JSONStorableFloat driverEndValJSON;

	public Trigger trigger;

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

	protected void SyncAtomChocies()
	{
		List<string> list = new List<string>();
		list.Add("None");
		foreach (string visibleAtomUID in SuperController.singleton.GetVisibleAtomUIDs())
		{
			list.Add(visibleAtomUID);
		}
		driverAtomJSON.choices = list;
	}

	protected void SyncDriverAtom(string atomUID)
	{
		List<string> list = new List<string>();
		list.Add("None");
		if (atomUID != null)
		{
			driverAtom = SuperController.singleton.GetAtomByUid(atomUID);
			if (driverAtom != null)
			{
				foreach (string storableID in driverAtom.GetStorableIDs())
				{
					list.Add(storableID);
				}
			}
		}
		else
		{
			driverAtom = null;
		}
		driverJSON.choices = list;
		driverJSON.val = "None";
	}

	protected void CheckMissingDriver()
	{
		if (_missingDriverStoreId != string.Empty && driverAtom != null)
		{
			JSONStorable storableByID = driverAtom.GetStorableByID(_missingDriverStoreId);
			if (storableByID != null)
			{
				string driverTargetName = _driverTargetName;
				SyncDriver(_missingDriverStoreId);
				_missingDriverStoreId = string.Empty;
				insideRestore = true;
				driverTargetJSON.val = driverTargetName;
				insideRestore = false;
			}
		}
	}

	protected void SyncDriver(string driverID)
	{
		List<string> list = new List<string>();
		list.Add("None");
		if (driverAtom != null && driverID != null)
		{
			driver = driverAtom.GetStorableByID(driverID);
			if (driver != null)
			{
				foreach (string floatParamName in driver.GetFloatParamNames())
				{
					list.Add(floatParamName);
				}
			}
			else if (driverID != "None")
			{
				_missingDriverStoreId = driverID;
			}
		}
		else
		{
			driver = null;
		}
		driverTargetJSON.choices = list;
		driverTargetJSON.val = "None";
	}

	protected void SyncDriverTarget(string driverTargetName)
	{
		_driverTargetName = driverTargetName;
		driverTarget = null;
		if (!(driver != null) || driverTargetName == null)
		{
			return;
		}
		driverTarget = driver.GetFloatJSONParam(driverTargetName);
		if (driverTarget != null)
		{
			float val = driverStartValJSON.val;
			float val2 = driverEndValJSON.val;
			driverStartValJSON.constrained = driverTarget.constrained;
			driverEndValJSON.constrained = driverTarget.constrained;
			driverStartValJSON.min = driverTarget.min;
			driverStartValJSON.max = driverTarget.max;
			driverEndValJSON.min = driverTarget.min;
			driverEndValJSON.max = driverTarget.max;
			if (insideRestore)
			{
				driverStartValJSON.val = val;
				driverEndValJSON.val = val2;
			}
			else
			{
				driverStartValJSON.val = driverTarget.val;
				driverEndValJSON.val = driverTarget.val;
			}
			SyncDriverStartValSlider();
			SyncDriverEndValSlider();
		}
	}

	protected void SyncDriverStartValSlider()
	{
		if (driverStartValDynamicSlider != null)
		{
			driverStartValDynamicSlider.rangeAdjustEnabled = driverStartValJSON.constrained;
			float max = driverStartValJSON.max;
			if (max <= 2f)
			{
				driverStartValDynamicSlider.valueFormat = "F3";
			}
			else if (max <= 20f)
			{
				driverStartValDynamicSlider.valueFormat = "F2";
			}
			else if (max <= 200f)
			{
				driverStartValDynamicSlider.valueFormat = "F1";
			}
			else
			{
				driverStartValDynamicSlider.valueFormat = "F0";
			}
			if (driverTarget != null)
			{
				driverStartValDynamicSlider.label = "(Start) " + driverTarget.name;
			}
			else
			{
				driverEndValDynamicSlider.label = "Driver Start Value";
			}
		}
	}

	protected void SyncDriverEndValSlider()
	{
		if (driverEndValDynamicSlider != null)
		{
			driverEndValDynamicSlider.rangeAdjustEnabled = driverEndValJSON.constrained;
			float max = driverEndValJSON.max;
			if (max <= 2f)
			{
				driverEndValDynamicSlider.valueFormat = "F3";
			}
			else if (max <= 20f)
			{
				driverEndValDynamicSlider.valueFormat = "F2";
			}
			else if (max <= 200f)
			{
				driverEndValDynamicSlider.valueFormat = "F1";
			}
			else
			{
				driverEndValDynamicSlider.valueFormat = "F0";
			}
			if (driverTarget != null)
			{
				driverEndValDynamicSlider.label = "(End) " + driverTarget.name;
			}
			else
			{
				driverEndValDynamicSlider.label = "Driver End Value";
			}
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
		SyncAtomChocies();
		if (driverAtomJSON != null && driverAtom != null)
		{
			driverAtomJSON.valNoCallback = driverAtom.uid;
		}
	}

	protected virtual void CreateFloatJSON()
	{
		floatJSON = new JSONStorableFloat("value", 0f, SyncFloat, 0f, 1f);
		floatJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(floatJSON);
	}

	protected virtual void Init()
	{
		trigger = new Trigger();
		trigger.handler = this;
		trigger.active = true;
		CreateFloatJSON();
		driverAtomJSON = new JSONStorableStringChooser("driverAtom", SuperController.singleton.GetAtomUIDs(), null, "Driver Atom", SyncDriverAtom);
		driverAtomJSON.representsAtomUid = true;
		driverAtomJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterStringChooser(driverAtomJSON);
		driverAtomJSON.popupOpenCallback = SyncAtomChocies;
		driverJSON = new JSONStorableStringChooser("driver", null, null, "Driver", SyncDriver);
		driverJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterStringChooser(driverJSON);
		driverTargetJSON = new JSONStorableStringChooser("driverTarget", null, null, "Driver Target", SyncDriverTarget);
		driverTargetJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterStringChooser(driverTargetJSON);
		driverStartValJSON = new JSONStorableFloat("driverStartVal", 0f, 0f, 1f, false);
		driverStartValJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(driverStartValJSON);
		driverEndValJSON = new JSONStorableFloat("driverEndVal", 0f, 0f, 1f, false);
		driverEndValJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(driverEndValJSON);
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
			VariableTriggerUI componentInChildren = UITransform.GetComponentInChildren<VariableTriggerUI>(true);
			if (componentInChildren != null)
			{
				floatJSON.slider = componentInChildren.variableSlider;
				trigger.triggerActionsParent = componentInChildren.transform;
				trigger.triggerPanel = componentInChildren.transform;
				trigger.triggerActionsPanel = componentInChildren.transform;
				trigger.InitTriggerUI();
				trigger.InitTriggerActionsUI();
				driverAtomJSON.RegisterPopup(componentInChildren.driverAtomPopup);
				driverJSON.RegisterPopup(componentInChildren.driverPopup);
				driverTargetJSON.RegisterPopup(componentInChildren.driverTargetPopup);
				driverStartValJSON.RegisterSlider(componentInChildren.driverStartValSlider);
				driverEndValJSON.RegisterSlider(componentInChildren.driverEndValSlider);
				driverStartValDynamicSlider = componentInChildren.driverStartValDynamicSlider;
				driverEndValDynamicSlider = componentInChildren.driverEndValDynamicSlider;
				SyncDriverStartValSlider();
				SyncDriverEndValSlider();
			}
		}
	}

	public override void InitUIAlt()
	{
		if (UITransformAlt != null)
		{
			VariableTriggerUI componentInChildren = UITransformAlt.GetComponentInChildren<VariableTriggerUI>(true);
			if (componentInChildren != null)
			{
				floatJSON.sliderAlt = componentInChildren.variableSlider;
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
			InitUIAlt();
		}
	}

	protected void LateUpdate()
	{
		CheckMissingDriver();
		if (driverTarget != null)
		{
			float num = driverEndValJSON.val - driverStartValJSON.val;
			if (num != 0f)
			{
				floatJSON.val = Mathf.Clamp01((driverTarget.val - driverStartValJSON.val) / num);
			}
			else
			{
				floatJSON.val = 0f;
			}
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
