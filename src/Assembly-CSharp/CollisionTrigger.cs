using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class CollisionTrigger : JSONStorableTriggerHandler
{
	public Trigger trigger;

	public MeshRenderer collisionTriggerRenderer;

	protected CollisionTriggerEventHandler collisionTriggerEventHandler;

	public JSONStorableBool triggerEnabledJSON;

	[SerializeField]
	protected bool _triggerEnabled = true;

	public UIPopup atomFilterPopup;

	protected string _atomFilterUID = "None";

	protected JSONStorableBool invertAtomFilterJSON;

	[SerializeField]
	protected bool _invertAtomFilter;

	protected JSONStorableBool useRelativeVelocityFilterJSON;

	[SerializeField]
	protected bool _useRelativeVelocityFilter;

	protected JSONStorableBool invertRelativeVelocityFilterJSON;

	[SerializeField]
	protected bool _invertRelativeVelocityFilter;

	protected JSONStorableFloat relativeVelocityFilterJSON;

	[SerializeField]
	protected float _relativeVelocityFilter = 1f;

	protected JSONStorableFloat lastRelativeVelocityJSON;

	public string builtInTriggerSoundCategory = string.Empty;

	public string builtInTriggerSoundClip = string.Empty;

	public bool triggerEnabled
	{
		get
		{
			return _triggerEnabled;
		}
		set
		{
			if (triggerEnabledJSON != null)
			{
				triggerEnabledJSON.val = value;
			}
			else
			{
				SyncTriggerEnabled(value);
			}
		}
	}

	protected string atomFilterUID
	{
		get
		{
			return _atomFilterUID;
		}
		set
		{
			if (_atomFilterUID != value)
			{
				_atomFilterUID = value;
				if (atomFilterPopup != null)
				{
					atomFilterPopup.currentValueNoCallback = atomFilterUID;
				}
				if (collisionTriggerEventHandler != null)
				{
					collisionTriggerEventHandler.atomFilterUID = _atomFilterUID;
				}
			}
		}
	}

	public bool invertAtomFilter
	{
		get
		{
			return _invertAtomFilter;
		}
		set
		{
			if (invertAtomFilterJSON != null)
			{
				invertAtomFilterJSON.val = value;
			}
			else if (_invertAtomFilter != value)
			{
				SyncInvertAtomFilter(value);
			}
		}
	}

	public bool useRelativeVelocityFilter
	{
		get
		{
			return _useRelativeVelocityFilter;
		}
		set
		{
			if (useRelativeVelocityFilterJSON != null)
			{
				useRelativeVelocityFilterJSON.val = value;
			}
			else if (_useRelativeVelocityFilter != value)
			{
				SyncUseRelativeVelocityFilter(value);
			}
		}
	}

	public bool invertRelativeVelocityFilter
	{
		get
		{
			return _invertRelativeVelocityFilter;
		}
		set
		{
			if (invertRelativeVelocityFilterJSON != null)
			{
				invertRelativeVelocityFilterJSON.val = value;
			}
			else if (_invertRelativeVelocityFilter != value)
			{
				SyncInvertRelativeVelocityFilter(value);
			}
		}
	}

	public float relativeVelocityFilter
	{
		get
		{
			return _relativeVelocityFilter;
		}
		set
		{
			if (relativeVelocityFilterJSON != null)
			{
				relativeVelocityFilterJSON.val = value;
			}
			else if (_relativeVelocityFilter != value)
			{
				SyncRelativeVelocityFilter(value);
			}
		}
	}

	public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		JSONClass jSON = base.GetJSON(includePhysical, includeAppearance, forceStore);
		if (includePhysical || forceStore)
		{
			if (trigger != null && (trigger.HasActions() || forceStore))
			{
				needsStore = true;
				jSON["trigger"] = trigger.GetJSON(base.subScenePrefix);
			}
			if (atomFilterUID != null && atomFilterUID != "None")
			{
				string text = AtomUidToStoreAtomUid(atomFilterUID);
				if (text != null)
				{
					needsStore = true;
					jSON["atomFilter"] = text;
				}
				else
				{
					SuperController.LogError(string.Concat("Warning: CollisionTrigger on atom ", containingAtom, " uses atom filter ", atomFilterUID, " which is outside of subscene and cannot be saved"));
				}
			}
		}
		return jSON;
	}

	public override void LateRestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
	{
		base.LateRestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
		if (base.physicalLocked || !restorePhysical)
		{
			return;
		}
		if (!IsCustomPhysicalParamLocked("trigger"))
		{
			if (collisionTriggerEventHandler != null)
			{
				collisionTriggerEventHandler.Reset();
			}
			trigger.Reset();
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
		if (!IsCustomPhysicalParamLocked("atomFilter"))
		{
			if (jc["atomFilter"] != null)
			{
				string atomFilter = StoredAtomUidToAtomUid(jc["atomFilter"]);
				SetAtomFilter(atomFilter);
			}
			else if (setMissingToDefault)
			{
				SetAtomFilter("None");
			}
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

	public void ForceTrigger()
	{
		if (!trigger.active)
		{
			trigger.active = true;
			trigger.transitionInterpValue = 1f;
			trigger.active = false;
			trigger.transitionInterpValue = 0f;
		}
	}

	protected void SyncTriggerEnabled(bool b)
	{
		_triggerEnabled = b;
		if (_triggerEnabled)
		{
			collisionTriggerEventHandler = base.gameObject.AddComponent<CollisionTriggerEventHandler>();
			collisionTriggerEventHandler.collisionTrigger = this;
			collisionTriggerEventHandler.atomFilterUID = atomFilterUID;
			collisionTriggerEventHandler.invertAtomFilter = _invertAtomFilter;
			collisionTriggerEventHandler.useRelativeVelocityFilter = _useRelativeVelocityFilter;
			collisionTriggerEventHandler.invertRelativeVelocityFilter = _invertRelativeVelocityFilter;
			collisionTriggerEventHandler.relativeVelocityFilter = _relativeVelocityFilter;
			CollisionTriggerEventHandler obj = collisionTriggerEventHandler;
			obj.relativeVelocityHandlers = (CollisionTriggerEventHandler.RelativeVelocityCallback)Delegate.Combine(obj.relativeVelocityHandlers, new CollisionTriggerEventHandler.RelativeVelocityCallback(LastRelativeVelocityCallback));
			collisionTriggerEventHandler.Reset();
		}
		else if (collisionTriggerEventHandler != null)
		{
			CollisionTriggerEventHandler obj2 = collisionTriggerEventHandler;
			obj2.relativeVelocityHandlers = (CollisionTriggerEventHandler.RelativeVelocityCallback)Delegate.Remove(obj2.relativeVelocityHandlers, new CollisionTriggerEventHandler.RelativeVelocityCallback(LastRelativeVelocityCallback));
			UnityEngine.Object.Destroy(collisionTriggerEventHandler);
			collisionTriggerEventHandler = null;
		}
		if (collisionTriggerRenderer != null)
		{
			collisionTriggerRenderer.enabled = _triggerEnabled;
		}
	}

	protected void SetAtomNames()
	{
		if (!(atomFilterPopup != null) || !(SuperController.singleton != null))
		{
			return;
		}
		List<string> atomUIDsWithRigidbodies = SuperController.singleton.GetAtomUIDsWithRigidbodies();
		if (atomUIDsWithRigidbodies == null)
		{
			atomFilterPopup.numPopupValues = 1;
			atomFilterPopup.setPopupValue(0, "None");
			return;
		}
		atomFilterPopup.numPopupValues = atomUIDsWithRigidbodies.Count + 1;
		atomFilterPopup.setPopupValue(0, "None");
		for (int i = 0; i < atomUIDsWithRigidbodies.Count; i++)
		{
			atomFilterPopup.setPopupValue(i + 1, atomUIDsWithRigidbodies[i]);
		}
	}

	protected void SetAtomFilter(string atomUID)
	{
		atomFilterUID = atomUID;
	}

	protected void SyncInvertAtomFilter(bool b)
	{
		_invertAtomFilter = b;
		if (collisionTriggerEventHandler != null)
		{
			collisionTriggerEventHandler.invertAtomFilter = _invertAtomFilter;
		}
	}

	protected void SyncUseRelativeVelocityFilter(bool b)
	{
		_useRelativeVelocityFilter = b;
		if (collisionTriggerEventHandler != null)
		{
			collisionTriggerEventHandler.useRelativeVelocityFilter = _useRelativeVelocityFilter;
		}
	}

	protected void SyncInvertRelativeVelocityFilter(bool b)
	{
		_invertRelativeVelocityFilter = b;
		if (collisionTriggerEventHandler != null)
		{
			collisionTriggerEventHandler.invertRelativeVelocityFilter = _invertRelativeVelocityFilter;
		}
	}

	protected void SyncRelativeVelocityFilter(float f)
	{
		_relativeVelocityFilter = f;
		if (collisionTriggerEventHandler != null)
		{
			collisionTriggerEventHandler.relativeVelocityFilter = _relativeVelocityFilter;
		}
	}

	protected void LastRelativeVelocityCallback(float f)
	{
		lastRelativeVelocityJSON.val = f;
	}

	protected void OnAtomRename(string oldid, string newid)
	{
		if (trigger != null)
		{
			trigger.SyncAtomNames();
		}
		if (atomFilterUID == oldid)
		{
			atomFilterUID = newid;
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
			triggerActionDiscrete.name = "Built-In Audio";
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
		triggerEnabledJSON = new JSONStorableBool("triggerEnabled", _triggerEnabled, SyncTriggerEnabled);
		triggerEnabledJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(triggerEnabledJSON);
		invertAtomFilterJSON = new JSONStorableBool("invertAtomFilter", _invertAtomFilter, SyncInvertAtomFilter);
		invertAtomFilterJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(invertAtomFilterJSON);
		useRelativeVelocityFilterJSON = new JSONStorableBool("useRelativeVelocityFilter", _useRelativeVelocityFilter, SyncUseRelativeVelocityFilter);
		useRelativeVelocityFilterJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(useRelativeVelocityFilterJSON);
		invertRelativeVelocityFilterJSON = new JSONStorableBool("invertRelativeVelocityFilter", _invertRelativeVelocityFilter, SyncInvertRelativeVelocityFilter);
		invertRelativeVelocityFilterJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(invertRelativeVelocityFilterJSON);
		relativeVelocityFilterJSON = new JSONStorableFloat("relativeVelocityFilter", _relativeVelocityFilter, SyncRelativeVelocityFilter, 0f, 10f, false);
		relativeVelocityFilterJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(relativeVelocityFilterJSON);
		lastRelativeVelocityJSON = new JSONStorableFloat("lastRelativeVelocity", 0f, 0f, 10f, false, false);
		SyncTriggerEnabled(_triggerEnabled);
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
		CollisionTriggerUI componentInChildren = UITransform.GetComponentInChildren<CollisionTriggerUI>();
		if (componentInChildren != null)
		{
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
			trigger.triggerActionsParent = componentInChildren.transform;
			triggerEnabledJSON.toggle = componentInChildren.triggerEnabledToggle;
			atomFilterPopup = componentInChildren.atomFilterPopup;
			invertAtomFilterJSON.toggle = componentInChildren.invertAtomFilterToggle;
			if (trigger.triggerPanel == null)
			{
				trigger.triggerPanel = componentInChildren.transform;
			}
			trigger.triggerActionsPanel = componentInChildren.transform;
			trigger.InitTriggerUI();
			trigger.InitTriggerActionsUI();
			useRelativeVelocityFilterJSON.toggle = componentInChildren.useRelativeVelocityFilterToggle;
			invertRelativeVelocityFilterJSON.toggle = componentInChildren.invertRelativeVelocityFilterToggle;
			relativeVelocityFilterJSON.slider = componentInChildren.relativeVelocityFilterSlider;
			lastRelativeVelocityJSON.slider = componentInChildren.lastRelativeVelocitySlider;
		}
		if (atomFilterPopup != null)
		{
			atomFilterPopup.currentValue = atomFilterUID;
			UIPopup uIPopup = atomFilterPopup;
			uIPopup.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Combine(uIPopup.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetAtomNames));
			UIPopup uIPopup2 = atomFilterPopup;
			uIPopup2.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup2.onValueChangeHandlers, new UIPopup.OnValueChange(SetAtomFilter));
		}
	}

	protected void Update()
	{
		if (trigger != null)
		{
			trigger.Update();
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
