using System;
using SimpleJSON;
using UnityEngine;

public class SyncForceProducerV2 : ForceProducerV2
{
	protected string[] customParamNamesOverride = new string[2] { "receiver", "syncTo" };

	public ForceProducerV2 syncProducer;

	public UIPopup syncProducerSelectionPopup;

	protected JSONStorableBool autoSyncJSON;

	[SerializeField]
	protected bool _autoSync;

	public bool autoSync
	{
		get
		{
			return _autoSync;
		}
		set
		{
			if (autoSyncJSON != null)
			{
				autoSyncJSON.val = value;
			}
			else if (_autoSync != value)
			{
				SyncAutoSync(value);
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
		if ((includePhysical || forceStore) && syncProducer != null && syncProducer.containingAtom != null)
		{
			needsStore = true;
			jSON["syncTo"] = syncProducer.containingAtom.uid + ":" + syncProducer.name;
		}
		return jSON;
	}

	public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
	{
		base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
		if (!base.physicalLocked && restorePhysical && !IsCustomPhysicalParamLocked("syncTo"))
		{
			if (jc["syncTo"] != null)
			{
				SetSyncProducer(jc["syncTo"]);
			}
			else if (setMissingToDefault)
			{
				SetSyncProducer(string.Empty);
			}
		}
	}

	protected void SyncAutoSync(bool b)
	{
		_autoSync = b;
	}

	protected override void OnAtomUIDRename(string fromid, string toid)
	{
		base.OnAtomUIDRename(fromid, toid);
		if (syncProducer != null && syncProducerSelectionPopup != null)
		{
			syncProducerSelectionPopup.currentValueNoCallback = syncProducer.containingAtom.uid + ":" + syncProducer.name;
		}
	}

	protected virtual void SetProducerNames()
	{
		if (!(syncProducerSelectionPopup != null) || !(SuperController.singleton != null))
		{
			return;
		}
		string[] forceProducerNames = SuperController.singleton.forceProducerNames;
		if (forceProducerNames == null)
		{
			syncProducerSelectionPopup.numPopupValues = 1;
			syncProducerSelectionPopup.setPopupValue(0, "None");
			return;
		}
		syncProducerSelectionPopup.numPopupValues = forceProducerNames.Length + 1;
		syncProducerSelectionPopup.setPopupValue(0, "None");
		for (int i = 0; i < forceProducerNames.Length; i++)
		{
			syncProducerSelectionPopup.setPopupValue(i + 1, forceProducerNames[i]);
		}
	}

	public void SetSyncProducer(string producerName)
	{
		if (SuperController.singleton != null)
		{
			ForceProducerV2 forceProducerV = SuperController.singleton.ProducerNameToForceProducer(producerName);
			syncProducer = forceProducerV;
			if (syncProducerSelectionPopup != null)
			{
				syncProducerSelectionPopup.currentValue = producerName;
			}
		}
	}

	public void SyncAllParameters()
	{
		if (syncProducer != null)
		{
			forceFactor = syncProducer.forceFactor;
			forceQuickness = syncProducer.forceQuickness;
			maxForce = syncProducer.maxForce;
			torqueFactor = syncProducer.torqueFactor;
			torqueQuickness = syncProducer.torqueQuickness;
			maxTorque = syncProducer.maxTorque;
		}
	}

	protected override void Init()
	{
		base.Init();
		autoSyncJSON = new JSONStorableBool("autoSync", _autoSync, SyncAutoSync);
		autoSyncJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(autoSyncJSON);
	}

	public override void InitUI()
	{
		base.InitUI();
		if (!(UITransform != null))
		{
			return;
		}
		SyncForceProducerV2UI componentInChildren = UITransform.GetComponentInChildren<SyncForceProducerV2UI>(true);
		if (!(componentInChildren != null))
		{
			return;
		}
		autoSyncJSON.toggle = componentInChildren.autoSyncToggle;
		if (componentInChildren.manualSyncButton != null)
		{
			componentInChildren.manualSyncButton.onClick.AddListener(SyncAllParameters);
		}
		syncProducerSelectionPopup = componentInChildren.syncProducerSelectionPopup;
		if (syncProducerSelectionPopup != null)
		{
			if (syncProducer != null && syncProducer.containingAtom != null)
			{
				syncProducerSelectionPopup.currentValue = syncProducer.containingAtom.uid + ":" + syncProducer.name;
			}
			else
			{
				syncProducerSelectionPopup.currentValue = "None";
			}
			UIPopup uIPopup = syncProducerSelectionPopup;
			uIPopup.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Combine(uIPopup.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetProducerNames));
			UIPopup uIPopup2 = syncProducerSelectionPopup;
			uIPopup2.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup2.onValueChangeHandlers, new UIPopup.OnValueChange(SetSyncProducer));
		}
	}

	public override void InitUIAlt()
	{
		base.InitUIAlt();
		if (!(UITransformAlt != null))
		{
			return;
		}
		SyncForceProducerV2UI componentInChildren = UITransformAlt.GetComponentInChildren<SyncForceProducerV2UI>(true);
		if (componentInChildren != null)
		{
			autoSyncJSON.toggleAlt = componentInChildren.autoSyncToggle;
			if (componentInChildren.manualSyncButton != null)
			{
				componentInChildren.manualSyncButton.onClick.AddListener(SyncAllParameters);
			}
		}
	}

	protected override void Update()
	{
		if (syncProducer != null)
		{
			SetTargetForcePercent(syncProducer.targetForcePercent);
		}
		if (autoSync)
		{
			SyncAllParameters();
		}
		base.Update();
	}

	protected override void Start()
	{
		base.Start();
	}
}
