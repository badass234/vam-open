using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public abstract class TriggerAction
{
	public TriggerActionHandler handler;

	public RectTransform triggerActionMiniPanel;

	public RectTransform triggerActionPanel;

	protected JSONStorableString previewTextJSON;

	protected JSONStorableString nameJSON;

	protected JSONStorableBool enabledJSON;

	public Button openDetailPanelButton;

	public Button closeDetailPanelButton;

	public Button removeButton;

	public Button duplicateButton;

	public UIPopup receiverAtomPopup;

	protected Atom _receiverAtom;

	protected string _missingReceiverStoreid = string.Empty;

	public UIPopup receiverPopup;

	protected JSONStorable _receiver;

	protected string _receiverStoreId;

	protected List<string> receiverTargetNames;

	protected bool receiverSetFromPopup;

	public UIPopup receiverTargetNamePopup;

	protected string _receiverTargetName;

	protected JSONStorableFloat receiverTargetFloat;

	protected bool paramContrained;

	protected float paramDefault;

	protected float paramMin;

	protected float paramMax = 1f;

	public string previewText
	{
		get
		{
			return previewTextJSON.val;
		}
		set
		{
			previewTextJSON.val = value;
		}
	}

	public string name
	{
		get
		{
			return nameJSON.val;
		}
		set
		{
			nameJSON.val = value;
		}
	}

	public bool enabled
	{
		get
		{
			return enabledJSON.val;
		}
		set
		{
			enabledJSON.val = value;
		}
	}

	public bool detailPanelOpen { get; protected set; }

	public Atom receiverAtom
	{
		get
		{
			return _receiverAtom;
		}
		set
		{
			if (!(_receiverAtom != value))
			{
				return;
			}
			_receiverAtom = value;
			if (receiverAtomPopup != null)
			{
				if (_receiverAtom == null)
				{
					receiverAtomPopup.currentValue = "None";
				}
				else
				{
					receiverAtomPopup.currentValue = _receiverAtom.uid;
				}
			}
			receiver = null;
			SetReceiverPopupValues();
			if (receiverPopup != null && receiverPopup.numPopupValues > 1)
			{
				SetReceiver(receiverPopup.popupValues[1]);
			}
		}
	}

	public JSONStorable receiver
	{
		get
		{
			return _receiver;
		}
		set
		{
			if (!(_receiver != value))
			{
				return;
			}
			_receiver = value;
			if (_receiver != null)
			{
				_receiverStoreId = _receiver.storeId;
			}
			else
			{
				_receiverStoreId = null;
			}
			if (receiverPopup != null)
			{
				if (_receiver == null)
				{
					receiverPopup.currentValue = "None";
				}
				else
				{
					receiverPopup.currentValue = receiver.storeId;
				}
			}
			receiverTargetName = null;
			SetReceiverTargetPopupNames();
		}
	}

	public string receiverTargetName
	{
		get
		{
			return _receiverTargetName;
		}
		set
		{
			if (!(_receiverTargetName != value))
			{
				return;
			}
			_receiverTargetName = value;
			if (receiverTargetNamePopup != null)
			{
				if (_receiverTargetName == null)
				{
					receiverTargetNamePopup.currentValue = "None";
				}
				else
				{
					receiverTargetNamePopup.currentValue = _receiverTargetName;
				}
			}
			SyncFromReceiverTarget();
		}
	}

	public TriggerAction()
	{
		previewTextJSON = new JSONStorableString("previewText", string.Empty, SyncPreviewText);
		nameJSON = new JSONStorableString("name", string.Empty, SyncName);
		enabledJSON = new JSONStorableBool("enabled", true, SyncEnabled);
	}

	public virtual JSONClass GetJSON()
	{
		return GetJSON(null);
	}

	public virtual JSONClass GetJSON(string subScenePrefix)
	{
		JSONClass jSONClass = new JSONClass();
		nameJSON.StoreJSON(jSONClass);
		enabledJSON.StoreJSON(jSONClass);
		if (_receiverAtom != null)
		{
			if (subScenePrefix != null)
			{
				string text = "^" + subScenePrefix;
				if (Regex.IsMatch(_receiverAtom.uid, text + "[^/]+$"))
				{
					jSONClass["receiverAtom"] = Regex.Replace(_receiverAtom.uid, text, string.Empty);
				}
				else
				{
					SuperController.LogError("Warning: trigger referencing atom " + _receiverAtom.uid + " is outside of subscene. This trigger will be stored as external_ref which will only work if you load into a compatible scene.");
					jSONClass["receiverAtom"] = "external_ref:" + _receiverAtom.uid;
				}
			}
			else
			{
				jSONClass["receiverAtom"] = _receiverAtom.uid;
			}
		}
		if (_receiver != null)
		{
			jSONClass["receiver"] = _receiver.storeId;
		}
		if (_receiverTargetName != null)
		{
			jSONClass["receiverTargetName"] = _receiverTargetName;
		}
		return jSONClass;
	}

	public virtual void RestoreFromJSON(JSONClass jc)
	{
		RestoreFromJSON(jc, null);
	}

	public virtual void RestoreFromJSON(JSONClass jc, string subScenePrefix)
	{
		nameJSON.RestoreFromJSON(jc);
		enabledJSON.RestoreFromJSON(jc);
		if (jc["receiverAtom"] != null)
		{
			string text = jc["receiverAtom"];
			if (subScenePrefix != null)
			{
				if (Regex.IsMatch(text, "^external_ref:"))
				{
					SetReceiverAtom(Regex.Replace(text, "^external_ref:", string.Empty));
				}
				else
				{
					SetReceiverAtom(subScenePrefix + text);
				}
			}
			else
			{
				SetReceiverAtom(text);
			}
		}
		if (jc["receiver"] != null)
		{
			SetReceiver(jc["receiver"]);
		}
		if (jc["receiverTargetName"] != null)
		{
			SetReceiverTargetName(jc["receiverTargetName"]);
		}
	}

	protected void SyncPreviewText(string n)
	{
		if (handler != null)
		{
			handler.TriggerActionNameChange(this);
		}
	}

	protected virtual void AutoSetPreviewText()
	{
		if (_receiverAtom != null && !string.IsNullOrEmpty(_receiverStoreId) && !string.IsNullOrEmpty(_receiverTargetName))
		{
			previewText = _receiverAtom.uid + ":" + _receiverStoreId + ":" + _receiverTargetName;
		}
	}

	protected void SyncName(string n)
	{
		if (handler != null)
		{
			handler.TriggerActionNameChange(this);
		}
	}

	protected virtual void AutoSetName()
	{
		if (receiver != null && !string.IsNullOrEmpty(_receiverTargetName))
		{
			name = "A_" + _receiverTargetName;
		}
	}

	protected virtual void AutoSetPreviewTextAndName()
	{
		AutoSetPreviewText();
		if (nameJSON.val != null && (nameJSON.val == string.Empty || nameJSON.val.StartsWith("A_")))
		{
			AutoSetName();
		}
	}

	protected virtual void SyncEnabled(bool b)
	{
	}

	protected abstract void CreateTriggerActionPanel();

	public virtual void OpenDetailPanel()
	{
		detailPanelOpen = true;
		if (triggerActionPanel == null)
		{
			CreateTriggerActionPanel();
		}
		if (triggerActionPanel != null)
		{
			triggerActionPanel.gameObject.SetActive(true);
		}
	}

	public virtual void CloseDetailPanel()
	{
		if (triggerActionPanel != null)
		{
			triggerActionPanel.gameObject.SetActive(false);
		}
		detailPanelOpen = false;
	}

	public virtual void Remove()
	{
		if (handler != null)
		{
			DeregisterUI();
			handler.RemoveTriggerAction(this);
		}
		else
		{
			Debug.LogError("Attempt to Remove() when handler is null");
		}
	}

	public virtual void Duplicate()
	{
		if (handler != null)
		{
			handler.DuplicateTriggerAction(this);
		}
		else
		{
			Debug.LogError("Attempt to Duplicate() when handler is null");
		}
	}

	public virtual void Validate()
	{
		if (receiverAtom != null && receiverAtom.destroyed)
		{
			Remove();
		}
	}

	protected virtual void SetReceiverAtomPopupValues()
	{
		if (!(SuperController.singleton != null))
		{
			return;
		}
		List<string> visibleAtomUIDs = SuperController.singleton.GetVisibleAtomUIDs();
		int num = 0;
		receiverAtomPopup.numPopupValues = visibleAtomUIDs.Count + 1;
		receiverAtomPopup.setPopupValue(num, "None");
		num++;
		foreach (string item in visibleAtomUIDs)
		{
			receiverAtomPopup.setPopupValue(num, item);
			num++;
		}
	}

	public virtual void SetReceiverAtom(string uid)
	{
		if (SuperController.singleton != null)
		{
			if (uid == "None")
			{
				receiverAtom = null;
			}
			else
			{
				receiverAtom = SuperController.singleton.GetAtomByUid(uid);
			}
		}
	}

	public virtual void SyncAtomName()
	{
		if (_receiverAtom != null && receiverAtomPopup != null)
		{
			receiverAtomPopup.currentValueNoCallback = _receiverAtom.uid;
		}
	}

	protected virtual void SetReceiverPopupValues()
	{
		if (!(receiverPopup != null))
		{
			return;
		}
		if (_receiverAtom != null)
		{
			List<string> storableIDs = _receiverAtom.GetStorableIDs();
			List<string> list = new List<string>();
			foreach (string item in storableIDs)
			{
				JSONStorable storableByID = _receiverAtom.GetStorableByID(item);
				if (storableByID != null && storableByID.HasParamsOrActions())
				{
					list.Add(item);
				}
			}
			receiverPopup.numPopupValues = list.Count + 1;
			int num = 0;
			receiverPopup.setPopupValue(num, "None");
			num++;
			{
				foreach (string item2 in list)
				{
					receiverPopup.setPopupValue(num, item2);
					num++;
				}
				return;
			}
		}
		receiverPopup.numPopupValues = 1;
		int index = 0;
		receiverPopup.setPopupValue(index, "None");
	}

	protected virtual void CheckMissingReceiver()
	{
		if (_missingReceiverStoreid != string.Empty)
		{
			JSONStorable storableByID = _receiverAtom.GetStorableByID(_missingReceiverStoreid);
			if (storableByID != null)
			{
				_missingReceiverStoreid = string.Empty;
				string text = _receiverTargetName;
				receiver = storableByID;
				receiverTargetName = text;
			}
		}
		else if (_receiverAtom != null && _receiverStoreId != null)
		{
			JSONStorable storableByID2 = _receiverAtom.GetStorableByID(_receiverStoreId);
			if (storableByID2 != null && storableByID2 != _receiver)
			{
				_receiver = storableByID2;
			}
		}
	}

	public virtual void SetReceiver(string storeid)
	{
		if (storeid == "None")
		{
			receiver = null;
		}
		else if (_receiverAtom != null)
		{
			_missingReceiverStoreid = string.Empty;
			receiver = _receiverAtom.GetStorableByID(storeid);
			if (receiver == null)
			{
				_missingReceiverStoreid = storeid;
			}
		}
	}

	protected virtual void SyncTargetPopupNames()
	{
		if (!(receiverTargetNamePopup != null))
		{
			return;
		}
		if (receiverTargetNames != null)
		{
			receiverTargetNamePopup.numPopupValues = receiverTargetNames.Count + 1;
			int num = 0;
			receiverTargetNamePopup.setPopupValue(num, "None");
			num++;
			{
				foreach (string receiverTargetName in receiverTargetNames)
				{
					receiverTargetNamePopup.setPopupValue(num, receiverTargetName);
					num++;
				}
				return;
			}
		}
		receiverTargetNamePopup.numPopupValues = 1;
		int index = 0;
		receiverTargetNamePopup.setPopupValue(index, "None");
	}

	protected virtual void SetReceiverTargetPopupNames()
	{
		receiverTargetNames = null;
		if (_receiver != null)
		{
			receiverTargetNames = _receiver.GetFloatParamNames();
		}
		SyncTargetPopupNames();
	}

	public virtual void SetReceiverTargetName(string targetName)
	{
		receiverSetFromPopup = false;
		if (targetName == "None")
		{
			receiverTargetName = null;
		}
		else
		{
			receiverTargetName = targetName;
		}
	}

	public virtual void SetReceiverTargetNameAndSetInitialParams(string targetName)
	{
		receiverSetFromPopup = true;
		if (targetName == "None")
		{
			receiverTargetName = null;
		}
		else if (_receiverTargetName != targetName)
		{
			receiverTargetName = targetName;
			SetInitialParamsFromReceiverTarget();
		}
		receiverSetFromPopup = false;
	}

	protected virtual void SetInitialParamsFromReceiverTarget()
	{
	}

	protected virtual void SyncFromReceiverTarget()
	{
		if (_receiver != null && _receiverTargetName != null)
		{
			receiverTargetFloat = _receiver.GetFloatJSONParam(_receiverTargetName);
			if (receiverTargetFloat != null)
			{
				paramContrained = receiverTargetFloat.constrained;
				paramDefault = receiverTargetFloat.defaultVal;
				paramMin = receiverTargetFloat.min;
				paramMax = receiverTargetFloat.max;
			}
			else
			{
				paramContrained = false;
				paramDefault = 0f;
				paramMin = 0f;
				paramMax = 1f;
			}
		}
	}

	public virtual void InitTriggerActionMiniPanelUI()
	{
		if (triggerActionMiniPanel != null)
		{
			TriggerActionMiniUI component = triggerActionMiniPanel.GetComponent<TriggerActionMiniUI>();
			if (component != null)
			{
				openDetailPanelButton = component.openDetailPanelButton;
				removeButton = component.removeButton;
				duplicateButton = component.duplicateButton;
				previewTextJSON.text = component.previewTextPopupText;
				nameJSON.inputField = component.nameField;
				nameJSON.inputFieldAction = component.nameFieldAction;
				enabledJSON.toggle = component.enabledToggle;
			}
			if (openDetailPanelButton != null)
			{
				openDetailPanelButton.onClick.AddListener(OpenDetailPanel);
			}
			if (removeButton != null)
			{
				removeButton.onClick.AddListener(Remove);
			}
			if (duplicateButton != null)
			{
				duplicateButton.onClick.AddListener(Duplicate);
			}
		}
	}

	public virtual void InitTriggerActionPanelUI()
	{
		if (triggerActionPanel != null)
		{
			TriggerActionUI component = triggerActionPanel.GetComponent<TriggerActionUI>();
			if (component != null)
			{
				closeDetailPanelButton = component.closeDetailPanelButton;
				receiverAtomPopup = component.receiverAtomPopup;
				receiverPopup = component.receiverPopup;
				receiverTargetNamePopup = component.receiverTargetNamePopup;
				nameJSON.inputFieldAlt = component.nameField;
				nameJSON.inputFieldActionAlt = component.nameFieldAction;
				enabledJSON.toggleAlt = component.enabledToggle;
			}
		}
		if (closeDetailPanelButton != null)
		{
			closeDetailPanelButton.onClick.AddListener(CloseDetailPanel);
		}
		if (receiverAtomPopup != null)
		{
			if (_receiverAtom == null)
			{
				receiverAtomPopup.currentValue = "None";
			}
			else
			{
				receiverAtomPopup.currentValue = _receiverAtom.uid;
			}
			UIPopup uIPopup = receiverAtomPopup;
			uIPopup.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Combine(uIPopup.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetReceiverAtomPopupValues));
			UIPopup uIPopup2 = receiverAtomPopup;
			uIPopup2.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup2.onValueChangeHandlers, new UIPopup.OnValueChange(SetReceiverAtom));
		}
		if (receiverPopup != null)
		{
			SetReceiverPopupValues();
			if (_receiver == null)
			{
				if (_receiverStoreId == null)
				{
					receiverPopup.currentValue = "None";
				}
				else
				{
					receiverPopup.currentValue = _receiverStoreId;
				}
			}
			else
			{
				receiverPopup.currentValue = _receiver.storeId;
			}
			UIPopup uIPopup3 = receiverPopup;
			uIPopup3.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Combine(uIPopup3.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetReceiverPopupValues));
			UIPopup uIPopup4 = receiverPopup;
			uIPopup4.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup4.onValueChangeHandlers, new UIPopup.OnValueChange(SetReceiver));
		}
		if (receiverTargetNamePopup != null)
		{
			SetReceiverTargetPopupNames();
			if (_receiverTargetName == null)
			{
				receiverTargetNamePopup.currentValue = "None";
			}
			else
			{
				receiverTargetNamePopup.currentValue = _receiverTargetName;
			}
			UIPopup uIPopup5 = receiverTargetNamePopup;
			uIPopup5.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Combine(uIPopup5.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetReceiverTargetPopupNames));
			UIPopup uIPopup6 = receiverTargetNamePopup;
			uIPopup6.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup6.onValueChangeHandlers, new UIPopup.OnValueChange(SetReceiverTargetNameAndSetInitialParams));
		}
		SyncFromReceiverTarget();
	}

	public virtual void DeregisterUI()
	{
		previewTextJSON.text = null;
		nameJSON.inputField = null;
		nameJSON.inputFieldAction = null;
		nameJSON.inputFieldAlt = null;
		nameJSON.inputFieldActionAlt = null;
		enabledJSON.toggle = null;
		enabledJSON.toggleAlt = null;
		if (openDetailPanelButton != null)
		{
			openDetailPanelButton.onClick.RemoveListener(OpenDetailPanel);
		}
		if (closeDetailPanelButton != null)
		{
			closeDetailPanelButton.onClick.RemoveListener(CloseDetailPanel);
		}
		if (removeButton != null)
		{
			removeButton.onClick.RemoveListener(Remove);
		}
		if (duplicateButton != null)
		{
			duplicateButton.onClick.RemoveListener(Duplicate);
		}
		if (receiverAtomPopup != null)
		{
			UIPopup uIPopup = receiverAtomPopup;
			uIPopup.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Remove(uIPopup.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetReceiverAtomPopupValues));
			UIPopup uIPopup2 = receiverAtomPopup;
			uIPopup2.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Remove(uIPopup2.onValueChangeHandlers, new UIPopup.OnValueChange(SetReceiverAtom));
		}
		if (receiverPopup != null)
		{
			UIPopup uIPopup3 = receiverPopup;
			uIPopup3.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Remove(uIPopup3.onValueChangeHandlers, new UIPopup.OnValueChange(SetReceiver));
		}
		if (receiverTargetNamePopup != null)
		{
			UIPopup uIPopup4 = receiverTargetNamePopup;
			uIPopup4.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Remove(uIPopup4.onValueChangeHandlers, new UIPopup.OnValueChange(SetReceiverTargetNameAndSetInitialParams));
		}
	}
}
