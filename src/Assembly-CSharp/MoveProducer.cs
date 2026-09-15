using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class MoveProducer : JSONStorable
{
	public enum ControlOption
	{
		Both,
		Position,
		Rotation
	}

	protected string[] customParamNames = new string[1] { "receiver" };

	protected JSONStorableBool onJSON;

	[SerializeField]
	protected bool _on = true;

	protected JSONStorableStringChooser controlOptionJSON;

	[SerializeField]
	protected ControlOption _controlOption;

	[SerializeField]
	protected FreeControllerV3 _receiver;

	protected string freeControllerAtomUID;

	protected Vector3 _currentPosition;

	protected Quaternion _currentRotation;

	public UIPopup receiverAtomSelectionPopup;

	public UIPopup receiverSelectionPopup;

	public Button selectReceiverFromSceneButton;

	public virtual bool on
	{
		get
		{
			return _on;
		}
		set
		{
			if (onJSON != null)
			{
				onJSON.val = value;
			}
			else if (_on != value)
			{
				SyncOn(value);
			}
		}
	}

	public ControlOption controlOption
	{
		get
		{
			return _controlOption;
		}
		set
		{
			if (controlOptionJSON != null)
			{
				controlOptionJSON.val = value.ToString();
			}
			else if (_controlOption != value)
			{
				_controlOption = value;
			}
		}
	}

	public virtual FreeControllerV3 receiver
	{
		get
		{
			return _receiver;
		}
		set
		{
			_receiver = value;
		}
	}

	public virtual Vector3 currentPosition => _currentPosition;

	public virtual Quaternion currentRotation => _currentRotation;

	public override string[] GetCustomParamNames()
	{
		return customParamNames;
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
				jSON["receiver"] = text + ":" + _receiver.name;
			}
			else
			{
				SuperController.LogError(string.Concat("Warning: MoveProducer in atom ", containingAtom, " uses receiver atom ", _receiver.containingAtom.uid, " that is not in subscene and cannot be saved"));
			}
		}
		return jSON;
	}

	public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
	{
		base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
		if (!base.physicalLocked && restorePhysical && !IsCustomPhysicalParamLocked("receiver"))
		{
			if (jc["receiver"] != null)
			{
				string receiverByName = StoredAtomUidToAtomUid(jc["receiver"]);
				SetReceiverByName(receiverByName);
			}
			else if (setMissingToDefault)
			{
				SetReceiverByName(string.Empty);
			}
		}
	}

	protected void SyncOn(bool b)
	{
		_on = b;
	}

	protected virtual void SetControlOption(string option)
	{
		try
		{
			_controlOption = (ControlOption)Enum.Parse(typeof(ControlOption), option, true);
		}
		catch (ArgumentException)
		{
		}
	}

	protected void OnAtomUIDRename(string fromid, string toid)
	{
		if (freeControllerAtomUID == fromid)
		{
			freeControllerAtomUID = toid;
			if (receiverAtomSelectionPopup != null)
			{
				receiverAtomSelectionPopup.currentValueNoCallback = toid;
			}
		}
	}

	public virtual void SetReceiverAtom(string atomUID)
	{
		if (!(SuperController.singleton != null))
		{
			return;
		}
		Atom atomByUid = SuperController.singleton.GetAtomByUid(atomUID);
		if (atomByUid != null)
		{
			freeControllerAtomUID = atomUID;
			List<string> freeControllerNamesInAtom = SuperController.singleton.GetFreeControllerNamesInAtom(freeControllerAtomUID);
			onFreeControllerNamesChanged(freeControllerNamesInAtom);
			if (receiverSelectionPopup != null)
			{
				receiverSelectionPopup.currentValue = "None";
			}
		}
		else
		{
			onFreeControllerNamesChanged(null);
		}
	}

	public virtual void SetReceiverObject(string objectName)
	{
		if (freeControllerAtomUID != null && SuperController.singleton != null)
		{
			FreeControllerV3 freeControllerV = SuperController.singleton.FreeControllerNameToFreeController(freeControllerAtomUID + ":" + objectName);
			receiver = freeControllerV;
		}
	}

	public void SetReceiverByName(string controllerName)
	{
		if (!(SuperController.singleton != null))
		{
			return;
		}
		FreeControllerV3 freeControllerV = SuperController.singleton.FreeControllerNameToFreeController(controllerName);
		if (freeControllerV != null)
		{
			if (receiverAtomSelectionPopup != null && freeControllerV.containingAtom != null)
			{
				receiverAtomSelectionPopup.currentValue = freeControllerV.containingAtom.uid;
			}
			if (receiverSelectionPopup != null)
			{
				receiverSelectionPopup.currentValue = freeControllerV.name;
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
		receiver = freeControllerV;
	}

	protected virtual void SetReceiverAtomNames()
	{
		if (!(receiverAtomSelectionPopup != null) || !(SuperController.singleton != null))
		{
			return;
		}
		List<string> atomUIDsWithFreeControllers = SuperController.singleton.GetAtomUIDsWithFreeControllers();
		if (atomUIDsWithFreeControllers == null)
		{
			receiverAtomSelectionPopup.numPopupValues = 1;
			receiverAtomSelectionPopup.setPopupValue(0, "None");
			return;
		}
		receiverAtomSelectionPopup.numPopupValues = atomUIDsWithFreeControllers.Count + 1;
		receiverAtomSelectionPopup.setPopupValue(0, "None");
		for (int i = 0; i < atomUIDsWithFreeControllers.Count; i++)
		{
			receiverAtomSelectionPopup.setPopupValue(i + 1, atomUIDsWithFreeControllers[i]);
		}
	}

	protected void onFreeControllerNamesChanged(List<string> controllerNames)
	{
		if (!(receiverSelectionPopup != null))
		{
			return;
		}
		if (controllerNames == null)
		{
			receiverSelectionPopup.numPopupValues = 1;
			receiverSelectionPopup.setPopupValue(0, "None");
			return;
		}
		receiverSelectionPopup.numPopupValues = controllerNames.Count + 1;
		receiverSelectionPopup.setPopupValue(0, "None");
		for (int i = 0; i < controllerNames.Count; i++)
		{
			receiverSelectionPopup.setPopupValue(i + 1, controllerNames[i]);
		}
	}

	public void SelectReceiver(FreeControllerV3 rcvr)
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

	public void SelectControllerFromScene()
	{
		SuperController.singleton.SelectModeControllers(SelectReceiver);
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		MoveProducerUI componentInChildren = UITransform.GetComponentInChildren<MoveProducerUI>();
		if (!(componentInChildren != null))
		{
			return;
		}
		onJSON.toggle = componentInChildren.onToggle;
		controlOptionJSON.popup = componentInChildren.controlOptionPopup;
		receiverAtomSelectionPopup = componentInChildren.receiverAtomSelectionPopup;
		receiverSelectionPopup = componentInChildren.receiverSelectionPopup;
		if (receiverAtomSelectionPopup != null)
		{
			if (receiver != null)
			{
				if (receiver.containingAtom != null)
				{
					SetReceiverAtomNames();
					SetReceiverAtom(receiver.containingAtom.uid);
					receiverAtomSelectionPopup.currentValue = receiver.containingAtom.uid;
				}
				else
				{
					receiverAtomSelectionPopup.currentValue = "None";
				}
			}
			else
			{
				receiverAtomSelectionPopup.currentValue = "None";
			}
			UIPopup uIPopup = receiverAtomSelectionPopup;
			uIPopup.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Combine(uIPopup.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetReceiverAtomNames));
			UIPopup uIPopup2 = receiverAtomSelectionPopup;
			uIPopup2.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup2.onValueChangeHandlers, new UIPopup.OnValueChange(SetReceiverAtom));
		}
		if (receiverSelectionPopup != null)
		{
			if (receiver != null)
			{
				receiverSelectionPopup.currentValueNoCallback = receiver.name;
			}
			else
			{
				onFreeControllerNamesChanged(null);
				receiverSelectionPopup.currentValue = "None";
			}
			UIPopup uIPopup3 = receiverSelectionPopup;
			uIPopup3.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup3.onValueChangeHandlers, new UIPopup.OnValueChange(SetReceiverObject));
		}
		if (componentInChildren.selectReceiverFromSceneButton != null)
		{
			componentInChildren.selectReceiverFromSceneButton.onClick.AddListener(SelectControllerFromScene);
		}
	}

	public override void InitUIAlt()
	{
	}

	protected virtual void Init()
	{
		if (SuperController.singleton != null)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Combine(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomUIDRename));
		}
		onJSON = new JSONStorableBool("on", _on, SyncOn);
		onJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(onJSON);
		string[] names = Enum.GetNames(typeof(ControlOption));
		List<string> choicesList = new List<string>(names);
		controlOptionJSON = new JSONStorableStringChooser("controlOption", choicesList, _controlOption.ToString(), "Control Option", SetControlOption);
		controlOptionJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterStringChooser(controlOptionJSON);
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
			InitUIAlt();
		}
	}

	protected virtual void Start()
	{
		SetCurrentPositionAndRotation();
	}

	protected virtual void SetCurrentPositionAndRotation()
	{
		_currentPosition = base.transform.position;
		_currentRotation = base.transform.rotation;
	}

	protected virtual void UpdateTransform()
	{
		if (!_on || (!(SuperController.singleton == null) && SuperController.singleton.freezeAnimation))
		{
			return;
		}
		SetCurrentPositionAndRotation();
		if (_receiver != null)
		{
			if ((_controlOption == ControlOption.Both || _controlOption == ControlOption.Position) && _receiver.currentPositionState == FreeControllerV3.PositionState.On)
			{
				_receiver.control.position = _currentPosition;
			}
			if ((_controlOption == ControlOption.Both || _controlOption == ControlOption.Rotation) && _receiver.currentRotationState == FreeControllerV3.RotationState.On)
			{
				_receiver.control.rotation = _currentRotation;
			}
		}
	}

	protected virtual void FixedUpdate()
	{
		UpdateTransform();
	}

	protected virtual void Update()
	{
		UpdateTransform();
	}
}
