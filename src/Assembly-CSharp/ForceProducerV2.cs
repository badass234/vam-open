using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class ForceProducerV2 : JSONStorable
{
	public enum AxisName
	{
		X,
		Y,
		Z,
		NegX,
		NegY,
		NegZ
	}

	protected string[] customParamNames = new string[1] { "receiver" };

	protected JSONStorableBool onJSON;

	[SerializeField]
	protected bool _on = true;

	protected string receiverAtomUID;

	[SerializeField]
	protected ForceReceiver _receiver;

	public AxisName forceAxis;

	public AxisName torqueAxis = AxisName.Z;

	protected JSONStorableBool useForceJSON;

	[SerializeField]
	protected bool _useForce = true;

	protected JSONStorableBool useTorqueJSON;

	[SerializeField]
	protected bool _useTorque = true;

	protected JSONStorableFloat forceFactorJSON;

	[SerializeField]
	protected float _forceFactor = 200f;

	protected JSONStorableFloat maxForceJSON;

	[SerializeField]
	protected float _maxForce = 5000f;

	protected JSONStorableFloat torqueFactorJSON;

	[SerializeField]
	protected float _torqueFactor = 100f;

	protected JSONStorableFloat maxTorqueJSON;

	[SerializeField]
	protected float _maxTorque = 1000f;

	protected JSONStorableFloat forceQuicknessJSON;

	[SerializeField]
	protected float _forceQuickness = 10f;

	protected JSONStorableFloat torqueQuicknessJSON;

	[SerializeField]
	protected float _torqueQuickness = 10f;

	public Material linkLineMaterial;

	public Material forceLineMaterial;

	public Material targetForceLineMaterial;

	public Material rawForceLineMaterial;

	public bool drawLines = true;

	public float linesScale = 0.001f;

	public float lineOffset = 0.1f;

	public float lineSpacing = 0.01f;

	public UIPopup receiverAtomSelectionPopup;

	public UIPopup receiverSelectionPopup;

	public float targetForcePercent;

	public Vector3 appliedForce;

	public Vector3 appliedTorque;

	protected Vector3 forceDirection;

	protected Vector3 torqueDirection;

	protected Vector3 currentForce;

	protected Vector3 rawForce;

	protected Vector3 targetForce;

	protected Vector3 currentTorque;

	public Vector3 rawTorque;

	public Vector3 targetTorque;

	protected LineDrawer linkLineDrawer;

	protected LineDrawer forceLineDrawer;

	protected LineDrawer targetForceLineDrawer;

	protected LineDrawer rawForceLineDrawer;

	protected Rigidbody RB;

	protected float torqueLineMult = 10f;

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
				RB = _receiver.GetComponent<Rigidbody>();
			}
			else
			{
				RB = null;
			}
		}
	}

	public virtual bool useForce
	{
		get
		{
			return _useForce;
		}
		set
		{
			if (useForceJSON != null)
			{
				useForceJSON.val = value;
			}
			else if (_useForce != value)
			{
				SyncUseForce(value);
			}
		}
	}

	public virtual bool useTorque
	{
		get
		{
			return _useTorque;
		}
		set
		{
			if (useTorqueJSON != null)
			{
				useTorqueJSON.val = value;
			}
			else if (_useTorque != value)
			{
				SyncUseTorque(value);
			}
		}
	}

	public virtual float forceFactor
	{
		get
		{
			return _forceFactor;
		}
		set
		{
			if (forceFactorJSON != null)
			{
				forceFactorJSON.val = value;
			}
			else if (_forceFactor != value)
			{
				SyncForceFactor(value);
			}
		}
	}

	public virtual float maxForce
	{
		get
		{
			return _maxForce;
		}
		set
		{
			if (maxForceJSON != null)
			{
				maxForceJSON.val = value;
			}
			else if (_maxForce != value)
			{
				SyncMaxForce(value);
			}
		}
	}

	public virtual float torqueFactor
	{
		get
		{
			return _torqueFactor;
		}
		set
		{
			if (torqueFactorJSON != null)
			{
				torqueFactorJSON.val = value;
			}
			else if (_torqueFactor != value)
			{
				SyncTorqueFactor(value);
			}
		}
	}

	public virtual float maxTorque
	{
		get
		{
			return _maxTorque;
		}
		set
		{
			if (maxTorqueJSON != null)
			{
				maxTorqueJSON.val = value;
			}
			else if (_maxTorque != value)
			{
				SyncMaxTorque(value);
			}
		}
	}

	public virtual float forceQuickness
	{
		get
		{
			return _forceQuickness;
		}
		set
		{
			if (forceQuicknessJSON != null)
			{
				forceQuicknessJSON.val = value;
			}
			else if (_forceQuickness != value)
			{
				SyncForceQuickness(value);
			}
		}
	}

	public virtual float torqueQuickness
	{
		get
		{
			return _torqueQuickness;
		}
		set
		{
			if (torqueQuicknessJSON != null)
			{
				torqueQuicknessJSON.val = value;
			}
			else if (_torqueQuickness != value)
			{
				SyncTorqueQuickness(value);
			}
		}
	}

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
				SuperController.LogError(string.Concat("Warning: ForceProducer in atom ", containingAtom, " uses receiver atom ", _receiver.containingAtom.uid, " that is not in subscene and cannot be saved"));
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
				string forceReceiver = StoredAtomUidToAtomUid(jc["receiver"]);
				SetForceReceiver(forceReceiver);
			}
			else if (setMissingToDefault)
			{
				SetForceReceiver(string.Empty);
			}
		}
	}

	protected void SyncOn(bool b)
	{
		_on = b;
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

	protected virtual void OnAtomUIDRename(string fromid, string toid)
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
		if (!(SuperController.singleton != null))
		{
			return;
		}
		Atom atomByUid = SuperController.singleton.GetAtomByUid(atomUID);
		if (atomByUid != null)
		{
			receiverAtomUID = atomUID;
			List<string> forceReceiverNamesInAtom = SuperController.singleton.GetForceReceiverNamesInAtom(receiverAtomUID);
			onReceiverNamesChanged(forceReceiverNamesInAtom);
			if (receiverSelectionPopup != null)
			{
				receiverSelectionPopup.currentValue = "None";
			}
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

	protected void SyncUseForce(bool b)
	{
		_useForce = b;
	}

	protected void SyncUseTorque(bool b)
	{
		_useTorque = b;
	}

	protected virtual void SyncForceFactor(float f)
	{
		_forceFactor = f;
	}

	protected void SyncMaxForce(float f)
	{
		_maxForce = f;
	}

	protected virtual void SyncTorqueFactor(float f)
	{
		_torqueFactor = f;
	}

	protected void SyncMaxTorque(float f)
	{
		_maxTorque = f;
	}

	protected void SyncForceQuickness(float f)
	{
		_forceQuickness = f;
	}

	protected void SyncTorqueQuickness(float f)
	{
		_torqueQuickness = f;
	}

	protected virtual void Init()
	{
		onJSON = new JSONStorableBool("on", _on, SyncOn);
		onJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(onJSON);
		useForceJSON = new JSONStorableBool("useForce", _useForce, SyncUseForce);
		useForceJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(useForceJSON);
		useTorqueJSON = new JSONStorableBool("useTorque", _useTorque, SyncUseTorque);
		useTorqueJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(useTorqueJSON);
		forceFactorJSON = new JSONStorableFloat("forceFactor", _forceFactor, SyncForceFactor, 0f, 1000f, false);
		forceFactorJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(forceFactorJSON);
		maxForceJSON = new JSONStorableFloat("maxForce", _maxForce, SyncMaxForce, 0f, 100f, false);
		maxForceJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(maxForceJSON);
		forceQuicknessJSON = new JSONStorableFloat("forceQuickness", _forceQuickness, SyncForceQuickness, 0f, 50f, false);
		forceQuicknessJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(forceQuicknessJSON);
		torqueFactorJSON = new JSONStorableFloat("torqueFactor", _torqueFactor, SyncTorqueFactor, 0f, 100f, false);
		torqueFactorJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(torqueFactorJSON);
		maxTorqueJSON = new JSONStorableFloat("maxTorque", _maxTorque, SyncMaxTorque, 0f, 100f, false);
		maxTorqueJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(maxTorqueJSON);
		torqueQuicknessJSON = new JSONStorableFloat("torqueQuickness", _torqueQuickness, SyncTorqueQuickness, 0f, 50f, false);
		torqueQuicknessJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(torqueQuicknessJSON);
		if ((bool)linkLineMaterial)
		{
			linkLineDrawer = new LineDrawer(linkLineMaterial);
		}
		if ((bool)forceLineMaterial)
		{
			forceLineDrawer = new LineDrawer(2, forceLineMaterial);
		}
		if ((bool)targetForceLineMaterial)
		{
			targetForceLineDrawer = new LineDrawer(6, targetForceLineMaterial);
		}
		if ((bool)rawForceLineMaterial)
		{
			rawForceLineDrawer = new LineDrawer(6, rawForceLineMaterial);
		}
		if (SuperController.singleton != null)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Combine(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomUIDRename));
		}
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		ForceProducerV2UI componentInChildren = UITransform.GetComponentInChildren<ForceProducerV2UI>(true);
		if (!(componentInChildren != null))
		{
			return;
		}
		onJSON.toggle = componentInChildren.onToggle;
		useForceJSON.toggle = componentInChildren.useForceToggle;
		useTorqueJSON.toggle = componentInChildren.useTorqueToggle;
		forceFactorJSON.slider = componentInChildren.forceFactorSlider;
		torqueFactorJSON.slider = componentInChildren.torqueFactorSlider;
		maxForceJSON.slider = componentInChildren.maxForceSlider;
		maxTorqueJSON.slider = componentInChildren.maxTorqueSlider;
		forceQuicknessJSON.slider = componentInChildren.forceQuicknessSlider;
		torqueQuicknessJSON.slider = componentInChildren.torqueQuicknessSlider;
		if (componentInChildren.selectReceiverAtomFromSceneButton != null)
		{
			componentInChildren.selectReceiverAtomFromSceneButton.onClick.AddListener(SelectForceReceiverFromScene);
		}
		receiverAtomSelectionPopup = componentInChildren.receiverAtomSelectionPopup;
		receiverSelectionPopup = componentInChildren.receiverSelectionPopup;
		if (receiverAtomSelectionPopup != null)
		{
			if (receiver != null)
			{
				if (receiver.containingAtom != null)
				{
					SetForceReceiverAtom(receiver.containingAtom.uid);
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
			uIPopup2.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup2.onValueChangeHandlers, new UIPopup.OnValueChange(SetForceReceiverAtom));
		}
		if (receiverSelectionPopup != null)
		{
			if (receiver != null)
			{
				receiverSelectionPopup.currentValueNoCallback = receiver.name;
			}
			else
			{
				onReceiverNamesChanged(null);
				receiverSelectionPopup.currentValue = "None";
			}
			UIPopup uIPopup3 = receiverSelectionPopup;
			uIPopup3.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup3.onValueChangeHandlers, new UIPopup.OnValueChange(SetForceReceiverObject));
		}
	}

	public override void InitUIAlt()
	{
		if (!(UITransformAlt != null))
		{
			return;
		}
		ForceProducerV2UI componentInChildren = UITransformAlt.GetComponentInChildren<ForceProducerV2UI>(true);
		if (componentInChildren != null)
		{
			onJSON.toggleAlt = componentInChildren.onToggle;
			useForceJSON.toggleAlt = componentInChildren.useForceToggle;
			useTorqueJSON.toggleAlt = componentInChildren.useTorqueToggle;
			forceFactorJSON.sliderAlt = componentInChildren.forceFactorSlider;
			torqueFactorJSON.sliderAlt = componentInChildren.torqueFactorSlider;
			maxForceJSON.sliderAlt = componentInChildren.maxForceSlider;
			maxTorqueJSON.sliderAlt = componentInChildren.maxTorqueSlider;
			forceQuicknessJSON.sliderAlt = componentInChildren.forceQuicknessSlider;
			torqueQuicknessJSON.sliderAlt = componentInChildren.torqueQuicknessSlider;
			if (componentInChildren.selectReceiverAtomFromSceneButton != null)
			{
				componentInChildren.selectReceiverAtomFromSceneButton.onClick.AddListener(SelectForceReceiverFromScene);
			}
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
		if (SuperController.singleton != null)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Remove(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomUIDRename));
		}
	}

	protected virtual void Start()
	{
		if ((bool)_receiver)
		{
			RB = _receiver.GetComponent<Rigidbody>();
		}
	}

	protected virtual void SetTargetForcePercent(float forcePercent)
	{
		targetForcePercent = Mathf.Clamp(forcePercent, -1f, 1f);
		if (useForce)
		{
			rawForce = forceDirection * targetForcePercent * _forceFactor;
			if (rawForce.magnitude > _maxForce)
			{
				targetForce = Vector3.ClampMagnitude(rawForce, _maxForce);
			}
			else
			{
				targetForce = rawForce;
			}
		}
		if (useTorque)
		{
			rawTorque = torqueDirection * targetForcePercent * _torqueFactor;
			if (rawTorque.magnitude > _maxTorque)
			{
				targetTorque = Vector3.ClampMagnitude(rawTorque, _maxTorque);
			}
			else
			{
				targetTorque = rawTorque;
			}
		}
	}

	protected virtual void ApplyForce()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		float num = ((!(TimeControl.singleton != null) || !TimeControl.singleton.compensateFixedTimestep) ? 1f : (Mathf.Approximately(Time.timeScale, 0f) ? 1f : (1f / Time.timeScale)));
		currentForce = Vector3.Lerp(currentForce, targetForce, fixedDeltaTime * _forceQuickness);
		currentTorque = Vector3.Lerp(currentTorque, targetTorque, fixedDeltaTime * _torqueQuickness);
		if ((bool)RB && on && (!SuperController.singleton || !SuperController.singleton.freezeAnimation))
		{
			if (useForce)
			{
				appliedForce = currentForce * num;
				RB.AddForce(appliedForce, ForceMode.Force);
			}
			if (useTorque)
			{
				appliedTorque = currentTorque * num;
				RB.AddTorque(appliedTorque, ForceMode.Force);
				RB.maxAngularVelocity = 20f;
			}
		}
	}

	protected virtual Vector3 AxisToVector(AxisName axis)
	{
		switch (axis)
		{
		case AxisName.X:
			return base.transform.right;
		case AxisName.NegX:
			return -base.transform.right;
		case AxisName.Y:
			return base.transform.up;
		case AxisName.NegY:
			return -base.transform.up;
		case AxisName.Z:
			return base.transform.forward;
		case AxisName.NegZ:
			return -base.transform.forward;
		default:
			return Vector3.zero;
		}
	}

	protected virtual Vector3 AxisToUpVector(AxisName axis)
	{
		switch (axis)
		{
		case AxisName.X:
			return base.transform.up;
		case AxisName.NegX:
			return base.transform.up;
		case AxisName.Y:
			return base.transform.forward;
		case AxisName.NegY:
			return base.transform.forward;
		case AxisName.Z:
			return base.transform.up;
		case AxisName.NegZ:
			return base.transform.up;
		default:
			return Vector3.zero;
		}
	}

	protected virtual Vector3 getDrawTorque(Vector3 trq)
	{
		Quaternion quaternion;
		switch (torqueAxis)
		{
		case AxisName.X:
			quaternion = Quaternion.FromToRotation(-base.transform.right, AxisToVector(forceAxis));
			break;
		case AxisName.NegX:
			quaternion = Quaternion.FromToRotation(-base.transform.right, AxisToVector(forceAxis));
			break;
		case AxisName.Y:
			quaternion = Quaternion.FromToRotation(base.transform.up, AxisToVector(forceAxis));
			break;
		case AxisName.NegY:
			quaternion = Quaternion.FromToRotation(-base.transform.up, AxisToVector(forceAxis));
			break;
		case AxisName.Z:
			quaternion = Quaternion.FromToRotation(base.transform.forward, AxisToVector(forceAxis));
			break;
		case AxisName.NegZ:
			quaternion = Quaternion.FromToRotation(-base.transform.forward, AxisToVector(forceAxis));
			break;
		default:
			quaternion = Quaternion.identity;
			break;
		}
		return quaternion * trq;
	}

	protected virtual void FixedUpdate()
	{
		ApplyForce();
	}

	protected virtual void Update()
	{
		if (useForce)
		{
			forceDirection = AxisToVector(forceAxis);
		}
		if (useTorque)
		{
			torqueDirection = AxisToVector(torqueAxis);
		}
		if (_on && _receiver != null && drawLines)
		{
			Vector3 vector = AxisToVector(forceAxis);
			Vector3 drawTorque = getDrawTorque(AxisToVector(torqueAxis));
			Vector3 vector2 = AxisToUpVector(forceAxis);
			if (linkLineDrawer != null)
			{
				linkLineDrawer.SetLinePoints(base.transform.position, receiver.transform.position);
				linkLineDrawer.Draw(base.gameObject.layer);
			}
			if (forceLineDrawer != null)
			{
				Vector3 vector3 = base.transform.position + vector2 * lineOffset;
				forceLineDrawer.SetLinePoints(0, vector3, vector3 + currentForce * linesScale);
				vector3 += vector2 * lineSpacing * 5f;
				Vector3 drawTorque2 = getDrawTorque(currentTorque);
				forceLineDrawer.SetLinePoints(1, vector3, vector3 + drawTorque2 * linesScale * torqueLineMult);
				targetForceLineDrawer.Draw(base.gameObject.layer);
				forceLineDrawer.Draw(base.gameObject.layer);
			}
			if (targetForceLineDrawer != null)
			{
				Vector3 vector4 = base.transform.position + vector2 * (lineOffset + lineSpacing);
				targetForceLineDrawer.SetLinePoints(0, vector4, vector4 + targetForce * linesScale);
				Vector3 vector5 = vector * _maxForce * linesScale;
				Vector3 vector6 = vector4 + vector5;
				targetForceLineDrawer.SetLinePoints(1, vector6 - vector2 * lineSpacing, vector6 + vector2 * lineSpacing);
				vector6 = vector4 - vector5;
				targetForceLineDrawer.SetLinePoints(2, vector6 - vector2 * lineSpacing, vector6 + vector2 * lineSpacing);
				vector4 += vector2 * lineSpacing * 5f;
				Vector3 drawTorque3 = getDrawTorque(targetTorque);
				targetForceLineDrawer.SetLinePoints(3, vector4, vector4 + drawTorque3 * linesScale * torqueLineMult);
				vector5 = drawTorque * _maxTorque * linesScale * torqueLineMult;
				vector6 = vector4 + vector5;
				targetForceLineDrawer.SetLinePoints(4, vector6 - vector2 * lineSpacing, vector6 + vector2 * lineSpacing);
				vector6 = vector4 - vector5;
				targetForceLineDrawer.SetLinePoints(5, vector6 - vector2 * lineSpacing, vector6 + vector2 * lineSpacing);
				targetForceLineDrawer.Draw(base.gameObject.layer);
			}
			if (rawForceLineDrawer != null)
			{
				Vector3 vector7 = base.transform.position + vector2 * (lineOffset + lineSpacing * 2f);
				rawForceLineDrawer.SetLinePoints(0, vector7, vector7 + rawForce * linesScale);
				Vector3 vector8 = vector * _forceFactor * linesScale;
				Vector3 vector9 = vector7 + vector8;
				rawForceLineDrawer.SetLinePoints(1, vector9 - vector2 * lineSpacing, vector9 + vector2 * lineSpacing);
				vector9 = vector7 - vector8;
				rawForceLineDrawer.SetLinePoints(2, vector9 - vector2 * lineSpacing, vector9 + vector2 * lineSpacing);
				vector7 += vector2 * lineSpacing * 5f;
				Vector3 drawTorque4 = getDrawTorque(rawTorque);
				rawForceLineDrawer.SetLinePoints(3, vector7, vector7 + drawTorque4 * linesScale * torqueLineMult);
				vector8 = drawTorque * _torqueFactor * linesScale * torqueLineMult;
				vector9 = vector7 + vector8;
				rawForceLineDrawer.SetLinePoints(4, vector9 - vector2 * lineSpacing, vector9 + vector2 * lineSpacing);
				vector9 = vector7 - vector8;
				rawForceLineDrawer.SetLinePoints(5, vector9 - vector2 * lineSpacing, vector9 + vector2 * lineSpacing);
				rawForceLineDrawer.Draw(base.gameObject.layer);
			}
		}
	}
}
