using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class GrabPoint : JSONStorable
{
	public enum GrabIconType
	{
		Hand,
		Head
	}

	public FreeControllerV3 controller;

	public Mesh handIconMesh;

	public Mesh headIconMesh;

	protected JSONStorableStringChooser grabIconTypeJSON;

	[SerializeField]
	protected GrabIconType _grabIconType;

	public Rigidbody rigidbodyToConnect;

	protected FixedJoint joint;

	[SerializeField]
	protected ForceReceiver _receiver;

	public UIPopup receiverAtomSelectionPopup;

	public UIPopup receiverSelectionPopup;

	public Material linkLineMaterial;

	protected LineDrawer linkLineDrawer;

	public bool drawLines;

	protected string receiverAtomUID;

	public Button selectFromSceneButton;

	public GrabIconType grabIconType
	{
		get
		{
			return _grabIconType;
		}
		set
		{
			if (grabIconTypeJSON != null)
			{
				grabIconTypeJSON.val = value.ToString();
			}
			else if (_grabIconType != value)
			{
				_grabIconType = value;
				SyncGrabIcon();
			}
		}
	}

	public virtual ForceReceiver receiver
	{
		get
		{
			return _receiver;
		}
		set
		{
			_receiver = value;
			if (_receiver != null)
			{
				Rigidbody component = _receiver.GetComponent<Rigidbody>();
				if (joint != null)
				{
					UnityEngine.Object.Destroy(joint);
					joint = null;
				}
				if (rigidbodyToConnect != null)
				{
					joint = rigidbodyToConnect.gameObject.AddComponent<FixedJoint>();
					joint.connectedBody = component;
				}
			}
			else if (joint != null)
			{
				UnityEngine.Object.Destroy(joint);
				joint = null;
			}
		}
	}

	public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		JSONClass jSON = base.GetJSON(includePhysical, includeAppearance, forceStore);
		if ((includePhysical || forceStore) && _receiver != null && _receiver.containingAtom != null)
		{
			string text = AtomUidToStoreAtomUid(_receiver.containingAtom.uid);
			if (text != null)
			{
				needsStore = true;
				jSON["controlled"] = text + ":" + _receiver.name;
			}
			else
			{
				SuperController.LogError(string.Concat("Warning: GrabPoint in atom ", containingAtom, " uses receiver atom ", _receiver.containingAtom.uid, " that is not in subscene and cannot be saved"));
			}
		}
		return jSON;
	}

	public override void LateRestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
	{
		base.LateRestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
		if (!base.physicalLocked && restorePhysical && !IsCustomPhysicalParamLocked("controlled"))
		{
			SetReceiverAtomNames();
			if (jc["controlled"] != null)
			{
				string forceReceiver = StoredAtomUidToAtomUid(jc["controlled"]);
				SetForceReceiver(forceReceiver);
			}
			else if (setMissingToDefault)
			{
				SetForceReceiver(string.Empty);
			}
		}
	}

	protected void SyncGrabIcon()
	{
		if (controller != null)
		{
			if (_grabIconType == GrabIconType.Hand && handIconMesh != null)
			{
				controller.deselectedMesh = handIconMesh;
			}
			if (_grabIconType == GrabIconType.Head && headIconMesh != null)
			{
				controller.deselectedMesh = headIconMesh;
			}
		}
	}

	public void SetGrabIconType(string type)
	{
		try
		{
			GrabIconType grabIconType = (GrabIconType)Enum.Parse(typeof(GrabIconType), type);
			_grabIconType = grabIconType;
			SyncGrabIcon();
		}
		catch (ArgumentException)
		{
			Debug.LogError("Attempted to set grab icon type to " + type + " which is not a valid grab icon type");
		}
	}

	protected virtual void SetReceiverAtomNames()
	{
		if (!(receiverAtomSelectionPopup != null) || !(SuperController.singleton != null))
		{
			return;
		}
		List<string> atomUIDsWithForceReceivers = SuperController.singleton.GetAtomUIDsWithForceReceivers();
		if (atomUIDsWithForceReceivers == null)
		{
			receiverAtomSelectionPopup.numPopupValues = 1;
			receiverAtomSelectionPopup.setPopupValue(0, "None");
			return;
		}
		receiverAtomSelectionPopup.numPopupValues = atomUIDsWithForceReceivers.Count + 1;
		receiverAtomSelectionPopup.setPopupValue(0, "None");
		for (int i = 0; i < atomUIDsWithForceReceivers.Count; i++)
		{
			receiverAtomSelectionPopup.setPopupValue(i + 1, atomUIDsWithForceReceivers[i]);
		}
	}

	protected virtual void onReceiverNamesChanged(List<string> rcvrNames)
	{
		if (!(receiverSelectionPopup != null))
		{
			return;
		}
		if (rcvrNames == null)
		{
			receiverSelectionPopup.numPopupValues = 1;
			receiverSelectionPopup.setPopupValue(0, "None");
			return;
		}
		receiverSelectionPopup.numPopupValues = rcvrNames.Count + 1;
		receiverSelectionPopup.setPopupValue(0, "None");
		for (int i = 0; i < rcvrNames.Count; i++)
		{
			receiverSelectionPopup.setPopupValue(i + 1, rcvrNames[i]);
		}
	}

	protected void OnAtomUIDRename(string fromid, string toid)
	{
		if (receiverAtomUID == fromid)
		{
			receiverAtomUID = toid;
			if (receiverAtomSelectionPopup != null)
			{
				receiverAtomSelectionPopup.currentValueNoCallback = toid;
			}
		}
	}

	public virtual void SetForceReceiverAtom(string atomUID)
	{
		if (SuperController.singleton != null)
		{
			Atom atomByUid = SuperController.singleton.GetAtomByUid(atomUID);
			receiver = null;
			if (atomByUid != null)
			{
				receiverAtomUID = atomUID;
				UpdateReceiverNames();
				receiverSelectionPopup.currentValue = "None";
			}
			else
			{
				receiverAtomUID = string.Empty;
				UpdateReceiverNames();
			}
		}
	}

	protected void UpdateReceiverNames()
	{
		if (receiverAtomUID != null && receiverAtomUID != string.Empty)
		{
			List<string> forceReceiverNamesInAtom = SuperController.singleton.GetForceReceiverNamesInAtom(receiverAtomUID);
			onReceiverNamesChanged(forceReceiverNamesInAtom);
		}
		else
		{
			onReceiverNamesChanged(null);
		}
	}

	public virtual void SetForceReceiverObject(string objectName)
	{
		if (receiverAtomUID != null && SuperController.singleton != null)
		{
			receiver = SuperController.singleton.ReceiverNameToForceReceiver(receiverAtomUID + ":" + objectName);
		}
		else
		{
			receiver = null;
		}
	}

	public virtual void SetForceReceiver(string receiverName)
	{
		if (!(SuperController.singleton != null))
		{
			return;
		}
		ForceReceiver forceReceiver = SuperController.singleton.ReceiverNameToForceReceiver(receiverName);
		if (forceReceiver != null)
		{
			if (receiverAtomSelectionPopup != null && forceReceiver.containingAtom != null)
			{
				receiverAtomSelectionPopup.currentValue = forceReceiver.containingAtom.uid;
			}
			if (receiverSelectionPopup != null)
			{
				receiverSelectionPopup.currentValue = forceReceiver.name;
			}
		}
		else
		{
			if (receiverAtomSelectionPopup != null)
			{
				receiverAtomSelectionPopup.currentValue = "None";
			}
			if (receiverSelectionPopup != null)
			{
				receiverSelectionPopup.currentValue = "None";
			}
		}
		receiver = forceReceiver;
	}

	public void SelectForceReceiver(ForceReceiver rcvr)
	{
		if (receiverAtomSelectionPopup != null && rcvr != null && rcvr.containingAtom != null)
		{
			receiverAtomSelectionPopup.currentValue = rcvr.containingAtom.uid;
		}
		if (receiverSelectionPopup != null && rcvr != null)
		{
			receiverSelectionPopup.currentValueNoCallback = rcvr.name;
		}
		receiver = rcvr;
	}

	public void SelectForceReceiverFromScene()
	{
		SetReceiverAtomNames();
		SuperController.singleton.SelectModeForceReceivers(SelectForceReceiver);
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		GrabPointUI componentInChildren = UITransform.GetComponentInChildren<GrabPointUI>();
		if (componentInChildren != null)
		{
			receiverAtomSelectionPopup = componentInChildren.receiverAtomSelectionPopup;
			receiverSelectionPopup = componentInChildren.receiverSelectionPopup;
			grabIconTypeJSON.popup = componentInChildren.grabIconTypePopup;
			selectFromSceneButton = componentInChildren.selectFromSceneButton;
		}
		if (receiverAtomSelectionPopup != null)
		{
			SetReceiverAtomNames();
			if (_receiver != null && _receiver.containingAtom != null)
			{
				receiverAtomSelectionPopup.currentValue = _receiver.containingAtom.uid;
			}
			else
			{
				receiverAtomSelectionPopup.currentValue = "None";
			}
			UIPopup uIPopup = receiverAtomSelectionPopup;
			uIPopup.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Combine(uIPopup.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetReceiverAtomNames));
			UIPopup uIPopup2 = receiverAtomSelectionPopup;
			uIPopup2.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup2.onValueChangeHandlers, new UIPopup.OnValueChange(SetForceReceiverAtom));
		}
		if (receiverSelectionPopup != null)
		{
			UpdateReceiverNames();
			if (_receiver != null)
			{
				receiverSelectionPopup.currentValue = _receiver.name;
			}
			else
			{
				receiverSelectionPopup.currentValue = "None";
			}
			UIPopup uIPopup3 = receiverSelectionPopup;
			uIPopup3.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Combine(uIPopup3.onOpenPopupHandlers, new UIPopup.OnOpenPopup(UpdateReceiverNames));
			UIPopup uIPopup4 = receiverSelectionPopup;
			uIPopup4.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup4.onValueChangeHandlers, new UIPopup.OnValueChange(SetForceReceiverObject));
		}
		if (selectFromSceneButton != null)
		{
			selectFromSceneButton.onClick.AddListener(SelectForceReceiverFromScene);
		}
	}

	protected void Init()
	{
		if (SuperController.singleton != null)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Combine(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomUIDRename));
		}
		if ((bool)linkLineMaterial)
		{
			linkLineDrawer = new LineDrawer(linkLineMaterial);
		}
		string[] names = Enum.GetNames(typeof(GrabIconType));
		List<string> choicesList = new List<string>(names);
		grabIconTypeJSON = new JSONStorableStringChooser("grabIconType", choicesList, _grabIconType.ToString(), "Display Icon", SetGrabIconType);
		grabIconTypeJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterStringChooser(grabIconTypeJSON);
		SyncGrabIcon();
	}

	private void OnDestroy()
	{
		if (SuperController.singleton != null)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Remove(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomUIDRename));
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

	private void Update()
	{
		if (drawLines && receiver != null && rigidbodyToConnect != null && linkLineDrawer != null)
		{
			linkLineDrawer.SetLinePoints(rigidbodyToConnect.transform.position, receiver.transform.position);
			linkLineDrawer.Draw(base.gameObject.layer);
		}
	}
}
