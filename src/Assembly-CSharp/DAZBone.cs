using System.Collections.Generic;
using MVR;
using SimpleJSON;
using UnityEngine;

public class DAZBone : JSONStorable
{
	public enum JointTarget
	{
		X,
		NegX,
		Y,
		NegY,
		Z,
		NegZ
	}

	protected string[] customParamNames = new string[6] { "relativeRootPosition", "relativeRootRotation", "rootPosition", "rootRotation", "position", "rotation" };

	protected const float geoScale = 0.01f;

	public bool isRoot;

	[SerializeField]
	private string _id;

	[SerializeField]
	private Vector3 _worldPosition;

	[SerializeField]
	private Vector3 _maleWorldPosition;

	[SerializeField]
	private Vector3 _worldOrientation;

	[SerializeField]
	private Vector3 _maleWorldOrientation;

	[SerializeField]
	private Vector3 _morphedWorldPosition;

	[SerializeField]
	private Vector3 _morphedWorldOrientation;

	[SerializeField]
	private Matrix4x4 _morphedLocalToWorldMatrix;

	[SerializeField]
	private Matrix4x4 _morphedWorldToLocalMatrix;

	public Matrix4x4 changeFromOriginalMatrix;

	private Matrix4x4 _nonMorphedLocalToWorldMatrix;

	private Matrix4x4 _nonMorphedWorldToLocalMatrix;

	[SerializeField]
	private Quaternion2Angles.RotationOrder _maleRotationOrder;

	[SerializeField]
	private Quaternion2Angles.RotationOrder _rotationOrder;

	[SerializeField]
	private Dictionary<string, Vector3> _morphOffsets;

	[SerializeField]
	private Dictionary<string, Vector3> _morphOrientationOffsets;

	[SerializeField]
	private Dictionary<string, Vector3> _morphRotations;

	public DAZBone parentForMorphOffsets;

	public Vector3 currentAnglesRadians;

	public Vector3 currentAngles;

	public Vector3 presetLocalTranslation;

	public Vector3 presetLocalRotation;

	private Vector3 _startingLocalPosition;

	private Quaternion _inverseStartingLocalRotation;

	private Quaternion _startingLocalRotation;

	private Quaternion _startingRotationRelativeToRoot;

	public DAZBones dazBones;

	public DAZBone parentBone;

	public bool useUnityEulerOrientation;

	public bool disableMorph;

	private Vector3 saveBonePosition;

	private Quaternion saveBoneRotation;

	private Vector3 saveControlPosition;

	private Quaternion saveControlRotation;

	private FreeControllerV3 saveControl;

	private Rigidbody boneRigidbody;

	private ConfigurableJoint saveControlJoint;

	private Rigidbody saveConnectedBody;

	private Vector3 zeroVector = Vector3.zero;

	private Transform saveParent;

	public bool transformDirty;

	public bool childDirty;

	public bool useCustomJointMap;

	public JointTarget xJointMap;

	public JointTarget yJointMap = JointTarget.Y;

	public JointTarget zJointMap = JointTarget.Z;

	protected bool didDetachJoint;

	protected Vector3 _baseJointRotation = Vector3.zero;

	protected bool _rotationMorphsEnabled = true;

	protected bool _jointRotationDisabled;

	public Quaternion2Angles.RotationOrder jointDriveTargetRotationOrder;

	protected bool wasInit;

	public Matrix4x4 worldToLocalMatrix;

	protected Matrix4x4 _localToWorldMatrix;

	protected Quaternion _localRotation;

	protected bool detectedPhysicsCorruptionOnThread;

	protected string physicsCorruptionType = string.Empty;

	public string id
	{
		get
		{
			if (_id == null || _id == string.Empty)
			{
				_id = base.name;
			}
			return _id;
		}
		set
		{
			_id = value;
		}
	}

	public Vector3 worldPosition
	{
		get
		{
			if (dazBones != null && dazBones.isMale)
			{
				return _maleWorldPosition;
			}
			return _worldPosition;
		}
	}

	public Vector3 importWorldPosition => _worldPosition;

	public Vector3 maleWorldPosition => _maleWorldPosition;

	public Vector3 worldOrientation
	{
		get
		{
			if (dazBones != null && dazBones.isMale)
			{
				return _maleWorldOrientation;
			}
			return _worldOrientation;
		}
	}

	public Vector3 importWorldOrientation => _worldOrientation;

	public Vector3 maleWorldOrientation => _maleWorldOrientation;

	public Vector3 morphedWorldPosition => _morphedWorldPosition;

	public Vector3 morphedWorldOrientation => _morphedWorldOrientation;

	public Matrix4x4 morphedLocalToWorldMatrix => _morphedLocalToWorldMatrix;

	public Matrix4x4 morphedWorldToLocalMatrix => _morphedWorldToLocalMatrix;

	public Matrix4x4 nonMorphedLocalToWorldMatrix => _nonMorphedLocalToWorldMatrix;

	public Matrix4x4 nonMorphedWorldToLocalMatrix => _nonMorphedWorldToLocalMatrix;

	public Quaternion2Angles.RotationOrder rotationOrder
	{
		get
		{
			if (dazBones != null && dazBones.isMale)
			{
				return _maleRotationOrder;
			}
			return _rotationOrder;
		}
	}

	public Dictionary<string, Vector3> morphOffsets => _morphOffsets;

	public Dictionary<string, Vector3> morphOrientationOffsets => _morphOrientationOffsets;

	public Dictionary<string, Vector3> morphRotations => _morphRotations;

	public Vector3 startingLocalPosition => _startingLocalPosition;

	public Quaternion inverseStartingLocalRotation => _inverseStartingLocalRotation;

	public Quaternion startingLocalRotation => _startingLocalRotation;

	public Quaternion startingRotationRelativeToRoot => _startingRotationRelativeToRoot;

	public FreeControllerV3 control => saveControl;

	public Vector3 baseJointRotation
	{
		get
		{
			return _baseJointRotation;
		}
		set
		{
			_baseJointRotation = value;
			SyncMorphBoneRotations();
		}
	}

	public bool rotationMorphsEnabled
	{
		get
		{
			return _rotationMorphsEnabled;
		}
		set
		{
			if (_rotationMorphsEnabled != value)
			{
				_rotationMorphsEnabled = value;
				SyncMorphBoneRotations(true);
			}
		}
	}

	public bool jointRotationDisabled
	{
		get
		{
			return _jointRotationDisabled;
		}
		set
		{
			if (jointRotationDisabled != value)
			{
				_jointRotationDisabled = value;
				SyncMorphBoneRotations(true);
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
		if (includePhysical || forceStore)
		{
			needsStore = true;
			if (isRoot)
			{
				Vector3 localPosition;
				Vector3 localEulerAngles;
				if (saveControl != null)
				{
					Transform parent = base.transform.parent;
					base.transform.parent = saveControl.transform;
					localPosition = base.transform.localPosition;
					jSON["relativeRootPosition"]["x"].AsFloat = localPosition.x;
					jSON["relativeRootPosition"]["y"].AsFloat = localPosition.y;
					jSON["relativeRootPosition"]["z"].AsFloat = localPosition.z;
					localEulerAngles = base.transform.localEulerAngles;
					jSON["relativeRootRotation"]["x"].AsFloat = localEulerAngles.x;
					jSON["relativeRootRotation"]["y"].AsFloat = localEulerAngles.y;
					jSON["relativeRootRotation"]["z"].AsFloat = localEulerAngles.z;
					base.transform.parent = parent;
				}
				localPosition = base.transform.position;
				jSON["rootPosition"]["x"].AsFloat = localPosition.x;
				jSON["rootPosition"]["y"].AsFloat = localPosition.y;
				jSON["rootPosition"]["z"].AsFloat = localPosition.z;
				localEulerAngles = base.transform.eulerAngles;
				jSON["rootRotation"]["x"].AsFloat = localEulerAngles.x;
				jSON["rootRotation"]["y"].AsFloat = localEulerAngles.y;
				jSON["rootRotation"]["z"].AsFloat = localEulerAngles.z;
			}
			else
			{
				Vector3 localPosition = base.transform.localPosition;
				jSON["position"]["x"].AsFloat = localPosition.x;
				jSON["position"]["y"].AsFloat = localPosition.y;
				jSON["position"]["z"].AsFloat = localPosition.z;
				Vector3 localEulerAngles = base.transform.localEulerAngles;
				jSON["rotation"]["x"].AsFloat = localEulerAngles.x;
				jSON["rotation"]["y"].AsFloat = localEulerAngles.y;
				jSON["rotation"]["z"].AsFloat = localEulerAngles.z;
			}
		}
		return jSON;
	}

	public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
	{
		Init();
		base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
		if (base.physicalLocked || !restorePhysical)
		{
			return;
		}
		Transform parent = base.transform.parent;
		bool flag = false;
		if (isRoot && saveControl != null && jc["relativeRootPosition"] != null)
		{
			if (!IsCustomPhysicalParamLocked("relativeRootPosition"))
			{
				flag = true;
				base.transform.parent = saveControl.transform;
				Vector3 localPosition = base.transform.localPosition;
				if (jc["relativeRootPosition"]["x"] != null)
				{
					localPosition.x = jc["relativeRootPosition"]["x"].AsFloat;
				}
				if (jc["relativeRootPosition"]["y"] != null)
				{
					localPosition.y = jc["relativeRootPosition"]["y"].AsFloat;
				}
				if (jc["relativeRootPosition"]["z"] != null)
				{
					localPosition.z = jc["relativeRootPosition"]["z"].AsFloat;
				}
				base.transform.localPosition = localPosition;
			}
		}
		else if (isRoot && jc["rootPosition"] != null)
		{
			if (!IsCustomPhysicalParamLocked("rootPosition"))
			{
				Vector3 position = base.transform.position;
				if (jc["rootPosition"]["x"] != null)
				{
					position.x = jc["rootPosition"]["x"].AsFloat;
				}
				if (jc["rootPosition"]["y"] != null)
				{
					position.y = jc["rootPosition"]["y"].AsFloat;
				}
				if (jc["rootPosition"]["z"] != null)
				{
					position.z = jc["rootPosition"]["z"].AsFloat;
				}
				base.transform.position = position;
			}
		}
		else if (jc["position"] != null)
		{
			if (!IsCustomPhysicalParamLocked("position"))
			{
				Vector3 localPosition2 = base.transform.localPosition;
				if (jc["position"]["x"] != null)
				{
					localPosition2.x = jc["position"]["x"].AsFloat;
				}
				if (jc["position"]["y"] != null)
				{
					localPosition2.y = jc["position"]["y"].AsFloat;
				}
				if (jc["position"]["z"] != null)
				{
					localPosition2.z = jc["position"]["z"].AsFloat;
				}
				base.transform.localPosition = localPosition2;
			}
		}
		else if (setMissingToDefault)
		{
			if (isRoot)
			{
				if (saveControl != null)
				{
					if (!IsCustomPhysicalParamLocked("relativeRootPosition"))
					{
						base.transform.position = saveControl.transform.position;
					}
				}
				else if (!IsCustomPhysicalParamLocked("rootPosition"))
				{
					base.transform.localPosition = _startingLocalPosition;
				}
			}
			else if (!IsCustomPhysicalParamLocked("position"))
			{
				base.transform.localPosition = _startingLocalPosition;
			}
		}
		if (isRoot && saveControl != null && jc["relativeRootRotation"] != null)
		{
			if (!IsCustomPhysicalParamLocked("relativeRootRotation"))
			{
				flag = true;
				base.transform.parent = saveControl.transform;
				Vector3 localEulerAngles = base.transform.localEulerAngles;
				if (jc["relativeRootRotation"]["x"] != null)
				{
					localEulerAngles.x = jc["relativeRootRotation"]["x"].AsFloat;
				}
				if (jc["relativeRootRotation"]["y"] != null)
				{
					localEulerAngles.y = jc["relativeRootRotation"]["y"].AsFloat;
				}
				if (jc["relativeRootRotation"]["z"] != null)
				{
					localEulerAngles.z = jc["relativeRootRotation"]["z"].AsFloat;
				}
				base.transform.localEulerAngles = localEulerAngles;
			}
		}
		else if (isRoot && jc["rootRotation"] != null)
		{
			if (!IsCustomPhysicalParamLocked("rootRotation"))
			{
				Vector3 eulerAngles = base.transform.eulerAngles;
				if (jc["rootRotation"]["x"] != null)
				{
					eulerAngles.x = jc["rootRotation"]["x"].AsFloat;
				}
				if (jc["rootRotation"]["y"] != null)
				{
					eulerAngles.y = jc["rootRotation"]["y"].AsFloat;
				}
				if (jc["rootRotation"]["z"] != null)
				{
					eulerAngles.z = jc["rootRotation"]["z"].AsFloat;
				}
				base.transform.eulerAngles = eulerAngles;
			}
		}
		else if (jc["rotation"] != null)
		{
			if (!IsCustomPhysicalParamLocked("rotation"))
			{
				Vector3 localEulerAngles2 = base.transform.localEulerAngles;
				if (jc["rotation"]["x"] != null)
				{
					localEulerAngles2.x = jc["rotation"]["x"].AsFloat;
				}
				if (jc["rotation"]["y"] != null)
				{
					localEulerAngles2.y = jc["rotation"]["y"].AsFloat;
				}
				if (jc["rotation"]["z"] != null)
				{
					localEulerAngles2.z = jc["rotation"]["z"].AsFloat;
				}
				base.transform.localEulerAngles = localEulerAngles2;
			}
		}
		else if (setMissingToDefault)
		{
			if (isRoot)
			{
				if (saveControl != null)
				{
					if (!IsCustomPhysicalParamLocked("relativeRootRotation"))
					{
						base.transform.rotation = control.transform.rotation;
					}
				}
				else if (!IsCustomPhysicalParamLocked("rootRotation"))
				{
					base.transform.localRotation = _startingLocalRotation;
				}
			}
			else if (!IsCustomPhysicalParamLocked("rotation"))
			{
				base.transform.localRotation = _startingLocalRotation;
			}
		}
		if (flag)
		{
			base.transform.parent = parent;
		}
	}

	public void ForceClearMorphs()
	{
		_morphOffsets.Clear();
		_morphOrientationOffsets.Clear();
		_morphRotations.Clear();
	}

	public void ImportNode(JSONNode jn, bool isMale)
	{
		_id = jn["id"];
		foreach (JSONNode item in jn["center_point"].AsArray)
		{
			switch (item["id"])
			{
			case "x":
				if (isMale)
				{
					_maleWorldPosition.x = item["value"].AsFloat * -0.01f;
				}
				else
				{
					_worldPosition.x = item["value"].AsFloat * -0.01f;
				}
				break;
			case "y":
				if (isMale)
				{
					_maleWorldPosition.y = item["value"].AsFloat * 0.01f;
				}
				else
				{
					_worldPosition.y = item["value"].AsFloat * 0.01f;
				}
				break;
			case "z":
				if (isMale)
				{
					_maleWorldPosition.z = item["value"].AsFloat * 0.01f;
				}
				else
				{
					_worldPosition.z = item["value"].AsFloat * 0.01f;
				}
				break;
			}
		}
		foreach (JSONNode item2 in jn["orientation"].AsArray)
		{
			switch (item2["id"])
			{
			case "x":
				if (isMale)
				{
					_maleWorldOrientation.x = item2["value"].AsFloat;
				}
				else
				{
					_worldOrientation.x = item2["value"].AsFloat;
				}
				break;
			case "y":
				if (isMale)
				{
					_maleWorldOrientation.y = 0f - item2["value"].AsFloat;
				}
				else
				{
					_worldOrientation.y = 0f - item2["value"].AsFloat;
				}
				break;
			case "z":
				if (isMale)
				{
					_maleWorldOrientation.z = 0f - item2["value"].AsFloat;
				}
				else
				{
					_worldOrientation.z = 0f - item2["value"].AsFloat;
				}
				break;
			}
		}
		string text = jn["rotation_order"];
		Quaternion2Angles.RotationOrder maleRotationOrder;
		switch (text)
		{
		case "XYZ":
			maleRotationOrder = Quaternion2Angles.RotationOrder.ZYX;
			break;
		case "XZY":
			maleRotationOrder = Quaternion2Angles.RotationOrder.YZX;
			break;
		case "YXZ":
			maleRotationOrder = Quaternion2Angles.RotationOrder.ZXY;
			break;
		case "YZX":
			maleRotationOrder = Quaternion2Angles.RotationOrder.XZY;
			break;
		case "ZXY":
			maleRotationOrder = Quaternion2Angles.RotationOrder.YXZ;
			break;
		case "ZYX":
			maleRotationOrder = Quaternion2Angles.RotationOrder.XYZ;
			break;
		default:
			Debug.LogError("Bad rotation order in json: " + text);
			maleRotationOrder = Quaternion2Angles.RotationOrder.XYZ;
			break;
		}
		if (isMale)
		{
			_maleRotationOrder = maleRotationOrder;
		}
		else
		{
			_rotationOrder = maleRotationOrder;
		}
		SetTransformToImportValues();
	}

	public void Rotate(Vector3 rotationToUse)
	{
		switch (rotationOrder)
		{
		case Quaternion2Angles.RotationOrder.XYZ:
			base.transform.Rotate(rotationToUse.x, 0f, 0f);
			base.transform.Rotate(0f, rotationToUse.y, 0f);
			base.transform.Rotate(0f, 0f, rotationToUse.z);
			break;
		case Quaternion2Angles.RotationOrder.XZY:
			base.transform.Rotate(rotationToUse.x, 0f, 0f);
			base.transform.Rotate(0f, 0f, rotationToUse.z);
			base.transform.Rotate(0f, rotationToUse.y, 0f);
			break;
		case Quaternion2Angles.RotationOrder.YXZ:
			base.transform.Rotate(0f, rotationToUse.y, 0f);
			base.transform.Rotate(rotationToUse.x, 0f, 0f);
			base.transform.Rotate(0f, 0f, rotationToUse.z);
			break;
		case Quaternion2Angles.RotationOrder.YZX:
			base.transform.Rotate(0f, rotationToUse.y, 0f);
			base.transform.Rotate(0f, 0f, rotationToUse.z);
			base.transform.Rotate(rotationToUse.x, 0f, 0f);
			break;
		case Quaternion2Angles.RotationOrder.ZXY:
			base.transform.Rotate(0f, 0f, rotationToUse.z);
			base.transform.Rotate(rotationToUse.x, 0f, 0f);
			base.transform.Rotate(0f, rotationToUse.y, 0f);
			break;
		case Quaternion2Angles.RotationOrder.ZYX:
			base.transform.Rotate(0f, 0f, rotationToUse.z);
			base.transform.Rotate(0f, rotationToUse.y, 0f);
			base.transform.Rotate(rotationToUse.x, 0f, 0f);
			break;
		}
	}

	public void ApplyOffsetTransform()
	{
		if (dazBones != null)
		{
			base.transform.position += dazBones.transform.position;
		}
	}

	public void SetTransformToImportValues()
	{
		if (!Application.isPlaying)
		{
			base.transform.position = worldPosition;
			if (useUnityEulerOrientation)
			{
				base.transform.rotation = Quaternion.Euler(worldOrientation);
			}
			else
			{
				base.transform.rotation = Quaternion2Angles.EulerToQuaternion(worldOrientation, Quaternion2Angles.RotationOrder.ZYX);
			}
			ApplyOffsetTransform();
		}
	}

	public void ApplyPresetLocalTransforms()
	{
		base.transform.localPosition += presetLocalTranslation;
		Rotate(presetLocalRotation);
	}

	public void SetImportValuesToTransform()
	{
		if (!Application.isPlaying)
		{
			if (dazBones != null && dazBones.isMale)
			{
				_maleWorldPosition = base.transform.position;
				_maleWorldOrientation = base.transform.eulerAngles;
			}
			else
			{
				_worldPosition = base.transform.position;
				_worldOrientation = base.transform.eulerAngles;
			}
		}
	}

	public void SetTransformToMorphPositionAndRotation(bool useScale, float globalScale)
	{
		transformDirty = false;
		childDirty = false;
		base.transform.position = _morphedWorldPosition;
		if (useUnityEulerOrientation)
		{
			base.transform.rotation = Quaternion.Euler(_morphedWorldOrientation);
		}
		else
		{
			base.transform.rotation = Quaternion2Angles.EulerToQuaternion(_morphedWorldOrientation, Quaternion2Angles.RotationOrder.ZYX);
		}
		if (useScale)
		{
			base.transform.position *= globalScale;
		}
	}

	public void SetMorphedTransform(bool useScale, float globalScale)
	{
		transformDirty = false;
		childDirty = false;
		_nonMorphedLocalToWorldMatrix = _morphedLocalToWorldMatrix;
		_nonMorphedWorldToLocalMatrix = _morphedWorldToLocalMatrix;
		if (disableMorph)
		{
			return;
		}
		base.transform.position = worldPosition;
		if (useUnityEulerOrientation)
		{
			base.transform.rotation = Quaternion.Euler(worldOrientation);
		}
		else
		{
			base.transform.rotation = Quaternion2Angles.EulerToQuaternion(worldOrientation, Quaternion2Angles.RotationOrder.ZYX);
		}
		_nonMorphedLocalToWorldMatrix = base.transform.localToWorldMatrix;
		_nonMorphedWorldToLocalMatrix = base.transform.worldToLocalMatrix;
		_morphedWorldPosition = worldPosition;
		InitMorphOffsets();
		foreach (string key in _morphOffsets.Keys)
		{
			Vector3 value;
			if (_morphOffsets.TryGetValue(key, out value))
			{
				_morphedWorldPosition += value;
			}
		}
		if (parentForMorphOffsets != null)
		{
			foreach (string key2 in parentForMorphOffsets.morphOffsets.Keys)
			{
				Vector3 value2;
				if (parentForMorphOffsets.morphOffsets.TryGetValue(key2, out value2))
				{
					_morphedWorldPosition += value2;
				}
			}
		}
		base.transform.position = _morphedWorldPosition;
		_morphedWorldOrientation = worldOrientation;
		foreach (string key3 in _morphOrientationOffsets.Keys)
		{
			Vector3 value3;
			if (_morphOrientationOffsets.TryGetValue(key3, out value3))
			{
				_morphedWorldOrientation += value3;
			}
		}
		if (parentForMorphOffsets != null)
		{
			foreach (string key4 in parentForMorphOffsets.morphOrientationOffsets.Keys)
			{
				Vector3 value4;
				if (parentForMorphOffsets.morphOrientationOffsets.TryGetValue(key4, out value4))
				{
					_morphedWorldOrientation += value4;
				}
			}
		}
		if (useUnityEulerOrientation)
		{
			base.transform.rotation = Quaternion.Euler(_morphedWorldOrientation);
		}
		else
		{
			base.transform.rotation = Quaternion2Angles.EulerToQuaternion(_morphedWorldOrientation, Quaternion2Angles.RotationOrder.ZYX);
		}
		_morphedLocalToWorldMatrix = base.transform.localToWorldMatrix;
		_morphedWorldToLocalMatrix = base.transform.worldToLocalMatrix;
		if (saveControl != null)
		{
			saveControl.transform.position = base.transform.position;
			saveControl.transform.rotation = base.transform.rotation;
		}
		if (useScale)
		{
			base.transform.position *= globalScale;
		}
	}

	public void SaveTransform()
	{
		saveBonePosition = base.transform.position;
		saveBoneRotation = base.transform.rotation;
		if (saveControl != null)
		{
			saveControl.PauseComply(10);
			saveControlPosition = saveControl.transform.position;
			saveControlRotation = saveControl.transform.rotation;
		}
	}

	public void RestoreTransform()
	{
		if (disableMorph)
		{
			return;
		}
		ConfigurableJoint[] components = GetComponents<ConfigurableJoint>();
		if (components.Length > 0)
		{
			if (saveControl != null)
			{
				saveControl.transform.position = saveControlPosition;
				saveControl.transform.rotation = saveControlRotation;
			}
			if (didDetachJoint)
			{
				didDetachJoint = false;
				if (boneRigidbody != null && boneRigidbody.constraints != RigidbodyConstraints.FreezeAll)
				{
					base.transform.position = saveBonePosition;
				}
			}
			else
			{
				base.transform.position = saveBonePosition;
			}
			base.transform.rotation = saveBoneRotation;
			RigidbodyAttributes component = GetComponent<RigidbodyAttributes>();
			if (component != null)
			{
				component.TempDisableInterpolation();
			}
			JointPositionHardLimit component2 = GetComponent<JointPositionHardLimit>();
			if (component2 != null && component2.useOffsetPosition)
			{
				component2.SetTargetPositionFromPercent();
			}
		}
		else
		{
			AdjustRotationTarget component3 = GetComponent<AdjustRotationTarget>();
			if (component3 != null)
			{
				component3.Adjust();
			}
			else
			{
				base.transform.rotation = saveBoneRotation;
			}
		}
	}

	public void SaveAndDetachParent()
	{
		if (!disableMorph)
		{
			saveParent = base.transform.parent;
			base.transform.parent = null;
		}
	}

	public void RestoreParent()
	{
		if (!disableMorph)
		{
			base.transform.parent = saveParent;
			if (!isRoot)
			{
				_startingLocalPosition = base.transform.localPosition;
				_startingLocalRotation = base.transform.localRotation;
				_inverseStartingLocalRotation = Quaternion.Inverse(_startingLocalRotation);
			}
			ResetScale();
		}
	}

	public void ResetScale()
	{
		base.transform.localScale = Vector3.one;
	}

	public void ResetToStartingLocalPositionRotation()
	{
		if (!isRoot)
		{
			base.transform.localPosition = _startingLocalPosition;
			base.transform.localRotation = _startingLocalRotation;
		}
	}

	public void DetachJoint()
	{
		if (disableMorph || isRoot)
		{
			return;
		}
		ConfigurableJoint[] components = GetComponents<ConfigurableJoint>();
		if (components.Length <= 0)
		{
			return;
		}
		ConfigurableJoint configurableJoint = components[0];
		if (configurableJoint != null && configurableJoint.connectedBody != null && !configurableJoint.connectedBody.GetComponent<FreeControllerV3>())
		{
			didDetachJoint = true;
			saveConnectedBody = configurableJoint.connectedBody;
			configurableJoint.connectedBody = null;
			if (saveControlJoint != null)
			{
				saveControlJoint.connectedBody = null;
			}
		}
	}

	public void AttachJoint()
	{
		if (isRoot)
		{
			return;
		}
		ConfigurableJoint[] components = GetComponents<ConfigurableJoint>();
		if (components.Length <= 0)
		{
			return;
		}
		ConfigurableJoint configurableJoint = components[0];
		if (didDetachJoint)
		{
			configurableJoint.connectedBody = saveConnectedBody;
			Vector3 localPosition = base.transform.localPosition;
			if (configurableJoint.connectedAnchor != localPosition)
			{
				configurableJoint.connectedAnchor = localPosition;
			}
			JointPositionHardLimit component = GetComponent<JointPositionHardLimit>();
			if (component != null)
			{
				component.startAnchor = configurableJoint.connectedAnchor;
				component.startRotation = base.transform.localRotation;
			}
			if (saveControl != null)
			{
				Rigidbody component2 = saveControl.GetComponent<Rigidbody>();
				saveControlJoint.connectedBody = component2;
			}
		}
	}

	public void RepairJoint()
	{
		if (disableMorph || isRoot)
		{
			return;
		}
		ConfigurableJoint[] components = GetComponents<ConfigurableJoint>();
		if (components.Length > 0)
		{
			ConfigurableJoint configurableJoint = components[0];
			if (configurableJoint != null && configurableJoint.connectedBody != null && !configurableJoint.connectedBody.GetComponent<FreeControllerV3>())
			{
				base.transform.localPosition = configurableJoint.connectedAnchor;
			}
		}
	}

	private void InitMorphOffsets()
	{
		if (_morphOffsets == null)
		{
			_morphOffsets = new Dictionary<string, Vector3>();
		}
		if (_morphOrientationOffsets == null)
		{
			_morphOrientationOffsets = new Dictionary<string, Vector3>();
		}
		if (_morphRotations == null)
		{
			_morphRotations = new Dictionary<string, Vector3>();
		}
	}

	public void SetBoneXOffset(string morphName, float xoffset)
	{
		InitMorphOffsets();
		Vector3 value = zeroVector;
		if (_morphOffsets.TryGetValue(morphName, out value))
		{
			value.x = xoffset;
			_morphOffsets.Remove(morphName);
			if (value != zeroVector)
			{
				_morphOffsets.Add(morphName, value);
			}
		}
		else
		{
			value.x = xoffset;
			if (value != zeroVector)
			{
				_morphOffsets.Add(morphName, value);
			}
		}
		transformDirty = true;
	}

	public void SetBoneYOffset(string morphName, float yoffset)
	{
		InitMorphOffsets();
		Vector3 value = zeroVector;
		if (_morphOffsets.TryGetValue(morphName, out value))
		{
			value.y = yoffset;
			_morphOffsets.Remove(morphName);
			if (value != zeroVector)
			{
				_morphOffsets.Add(morphName, value);
			}
		}
		else
		{
			value.y = yoffset;
			if (value != zeroVector)
			{
				_morphOffsets.Add(morphName, value);
			}
		}
		transformDirty = true;
	}

	public void SetBoneZOffset(string morphName, float zoffset)
	{
		InitMorphOffsets();
		Vector3 value = zeroVector;
		if (_morphOffsets.TryGetValue(morphName, out value))
		{
			value.z = zoffset;
			_morphOffsets.Remove(morphName);
			if (value != zeroVector)
			{
				_morphOffsets.Add(morphName, value);
			}
		}
		else
		{
			value.z = zoffset;
			if (value != zeroVector)
			{
				_morphOffsets.Add(morphName, value);
			}
		}
		transformDirty = true;
	}

	public void SetBoneOrientationXOffset(string morphName, float xoffset)
	{
		InitMorphOffsets();
		Vector3 value = zeroVector;
		if (_morphOrientationOffsets.TryGetValue(morphName, out value))
		{
			value.x = xoffset;
			_morphOrientationOffsets.Remove(morphName);
			if (value != zeroVector)
			{
				_morphOrientationOffsets.Add(morphName, value);
			}
		}
		else
		{
			value.x = xoffset;
			if (value != zeroVector)
			{
				_morphOrientationOffsets.Add(morphName, value);
			}
		}
		transformDirty = true;
	}

	public void SetBoneOrientationYOffset(string morphName, float yoffset)
	{
		InitMorphOffsets();
		Vector3 value = zeroVector;
		if (_morphOrientationOffsets.TryGetValue(morphName, out value))
		{
			value.y = yoffset;
			_morphOrientationOffsets.Remove(morphName);
			if (value != zeroVector)
			{
				_morphOrientationOffsets.Add(morphName, value);
			}
		}
		else
		{
			value.y = yoffset;
			if (value != zeroVector)
			{
				_morphOrientationOffsets.Add(morphName, value);
			}
		}
		transformDirty = true;
	}

	public void SetBoneOrientationZOffset(string morphName, float zoffset)
	{
		InitMorphOffsets();
		Vector3 value = zeroVector;
		if (_morphOrientationOffsets.TryGetValue(morphName, out value))
		{
			value.z = zoffset;
			_morphOrientationOffsets.Remove(morphName);
			if (value != zeroVector)
			{
				_morphOrientationOffsets.Add(morphName, value);
			}
		}
		else
		{
			value.z = zoffset;
			if (value != zeroVector)
			{
				_morphOrientationOffsets.Add(morphName, value);
			}
		}
		transformDirty = true;
	}

	public void SyncMorphBoneRotations(bool force = false)
	{
		if (_jointRotationDisabled || (!_rotationMorphsEnabled && !force))
		{
			return;
		}
		Vector3 vector = _baseJointRotation;
		if (_rotationMorphsEnabled)
		{
			foreach (string key in morphRotations.Keys)
			{
				Vector3 value;
				if (morphRotations.TryGetValue(key, out value))
				{
					vector += value;
				}
			}
		}
		ConfigurableJoint[] components = GetComponents<ConfigurableJoint>();
		if (components.Length <= 0)
		{
			return;
		}
		Vector3 r = vector;
		if (useCustomJointMap)
		{
			switch (xJointMap)
			{
			case JointTarget.X:
				r.x = vector.x;
				break;
			case JointTarget.NegX:
				r.x = 0f - vector.x;
				break;
			case JointTarget.Y:
				r.x = vector.y;
				break;
			case JointTarget.NegY:
				r.x = 0f - vector.y;
				break;
			case JointTarget.Z:
				r.x = vector.z;
				break;
			case JointTarget.NegZ:
				r.x = 0f - vector.z;
				break;
			}
			switch (yJointMap)
			{
			case JointTarget.X:
				r.y = vector.x;
				break;
			case JointTarget.NegX:
				r.y = 0f - vector.x;
				break;
			case JointTarget.Y:
				r.y = vector.y;
				break;
			case JointTarget.NegY:
				r.y = 0f - vector.y;
				break;
			case JointTarget.Z:
				r.y = vector.z;
				break;
			case JointTarget.NegZ:
				r.y = 0f - vector.z;
				break;
			}
			switch (zJointMap)
			{
			case JointTarget.X:
				r.z = vector.x;
				break;
			case JointTarget.NegX:
				r.z = 0f - vector.x;
				break;
			case JointTarget.Y:
				r.z = vector.y;
				break;
			case JointTarget.NegY:
				r.z = 0f - vector.y;
				break;
			case JointTarget.Z:
				r.z = vector.z;
				break;
			case JointTarget.NegZ:
				r.z = 0f - vector.z;
				break;
			}
		}
		else
		{
			ConfigurableJoint configurableJoint = components[0];
			if (configurableJoint.axis.x == 1f)
			{
				r.x = 0f - vector.x;
				if (configurableJoint.secondaryAxis.y == 1f)
				{
					r.y = vector.y;
					r.z = vector.z;
				}
				else
				{
					r.y = vector.z;
					r.z = vector.y;
				}
			}
			else if (configurableJoint.axis.y == 1f)
			{
				r.x = vector.y;
				if (configurableJoint.secondaryAxis.x == 1f)
				{
					r.y = 0f - vector.x;
					r.z = vector.z;
				}
				else
				{
					r.y = vector.z;
					r.z = 0f - vector.x;
				}
			}
			else
			{
				r.x = vector.z;
				if (configurableJoint.secondaryAxis.x == 1f)
				{
					r.y = 0f - vector.x;
					r.z = vector.y;
				}
				else
				{
					r.y = vector.y;
					r.z = 0f - vector.x;
				}
			}
		}
		if (components.Length > 1)
		{
			Rigidbody connectedBody = components[1].connectedBody;
			FreeControllerV3 component = connectedBody.GetComponent<FreeControllerV3>();
			if (component != null)
			{
				component.jointRotationDriveXTargetAdditional = r.x;
				component.jointRotationDriveYTargetAdditional = r.y;
				component.jointRotationDriveZTargetAdditional = r.z;
			}
			else
			{
				SetJointDriveTargetRotation(components[0], r);
			}
		}
		else
		{
			SetJointDriveTargetRotation(components[0], r);
		}
	}

	protected void SetJointDriveTargetRotation(ConfigurableJoint cj, Vector3 r)
	{
		Quaternion targetRotation = Quaternion2Angles.EulerToQuaternion(r, jointDriveTargetRotationOrder);
		cj.targetRotation = targetRotation;
	}

	public void SetBoneXRotation(string morphName, float rot)
	{
		InitMorphOffsets();
		Vector3 value = zeroVector;
		if (_morphRotations.TryGetValue(morphName, out value))
		{
			value.x = rot;
			_morphRotations.Remove(morphName);
			if (value != zeroVector)
			{
				_morphRotations.Add(morphName, value);
			}
		}
		else
		{
			value.x = rot;
			if (value != zeroVector)
			{
				_morphRotations.Add(morphName, value);
			}
		}
	}

	public void SetBoneYRotation(string morphName, float rot)
	{
		InitMorphOffsets();
		Vector3 value = zeroVector;
		if (_morphRotations.TryGetValue(morphName, out value))
		{
			value.y = rot;
			_morphRotations.Remove(morphName);
			if (value != zeroVector)
			{
				_morphRotations.Add(morphName, value);
			}
		}
		else
		{
			value.y = rot;
			if (value != zeroVector)
			{
				_morphRotations.Add(morphName, value);
			}
		}
	}

	public void SetBoneZRotation(string morphName, float rot)
	{
		InitMorphOffsets();
		Vector3 value = zeroVector;
		if (_morphRotations.TryGetValue(morphName, out value))
		{
			value.z = rot;
			_morphRotations.Remove(morphName);
			if (value != zeroVector)
			{
				_morphRotations.Add(morphName, value);
			}
		}
		else
		{
			value.z = rot;
			if (value != zeroVector)
			{
				_morphRotations.Add(morphName, value);
			}
		}
	}

	public void SetBoneScaleX(string morphName, float xscale)
	{
	}

	public Vector3 GetAngles()
	{
		return currentAnglesRadians;
	}

	public Vector3 GetAnglesDegrees()
	{
		return currentAngles;
	}

	public void Init()
	{
		if (wasInit)
		{
			return;
		}
		wasInit = true;
		_startingLocalPosition = base.transform.localPosition;
		_startingLocalRotation = base.transform.localRotation;
		_inverseStartingLocalRotation = Quaternion.Inverse(_startingLocalRotation);
		if (dazBones != null)
		{
			_startingRotationRelativeToRoot = Quaternion.Inverse(dazBones.transform.rotation) * base.transform.rotation;
		}
		changeFromOriginalMatrix = base.transform.localToWorldMatrix * _morphedLocalToWorldMatrix;
		currentAnglesRadians = Quaternion2Angles.GetAngles(_inverseStartingLocalRotation * base.transform.localRotation, rotationOrder);
		currentAngles = currentAnglesRadians * 57.29578f;
		InitMorphOffsets();
		saveControl = null;
		saveControlJoint = null;
		boneRigidbody = GetComponent<Rigidbody>();
		ConfigurableJoint[] components = GetComponents<ConfigurableJoint>();
		ConfigurableJoint[] array = components;
		foreach (ConfigurableJoint configurableJoint in array)
		{
			Rigidbody connectedBody = configurableJoint.connectedBody;
			if (connectedBody != null)
			{
				saveControl = connectedBody.GetComponent<FreeControllerV3>();
				saveControlJoint = configurableJoint;
			}
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
		}
	}

	public void SetResetVelocity()
	{
		if (boneRigidbody != null)
		{
			boneRigidbody.velocity = Vector3.zero;
			boneRigidbody.angularVelocity = Vector3.zero;
		}
	}

	public void PrepThreadUpdate()
	{
		_localToWorldMatrix = base.transform.localToWorldMatrix;
		_localRotation = base.transform.localRotation;
	}

	public void ThreadsafeUpdate()
	{
		if (NaNUtils.IsMatrixValid(_localToWorldMatrix) && NaNUtils.IsMatrixValid(_morphedLocalToWorldMatrix) && NaNUtils.IsQuaternionValid(_localRotation))
		{
			worldToLocalMatrix = _localToWorldMatrix.inverse;
			changeFromOriginalMatrix = _localToWorldMatrix * _morphedWorldToLocalMatrix;
			Vector3 angles = Quaternion2Angles.GetAngles(_inverseStartingLocalRotation * _localRotation, rotationOrder);
			if (NaNUtils.IsVector3Valid(angles))
			{
				currentAnglesRadians = angles;
				currentAngles = currentAnglesRadians * 57.29578f;
			}
			else
			{
				detectedPhysicsCorruptionOnThread = true;
				physicsCorruptionType = "Quaternion2Angles";
			}
		}
		else
		{
			detectedPhysicsCorruptionOnThread = true;
			physicsCorruptionType = "Matrix";
		}
	}

	public void FinishThreadUpdate()
	{
		if (detectedPhysicsCorruptionOnThread)
		{
			if (containingAtom != null)
			{
				containingAtom.AlertPhysicsCorruption("DAZBone " + physicsCorruptionType + " " + base.name);
			}
			detectedPhysicsCorruptionOnThread = false;
		}
	}

	private void Update()
	{
		changeFromOriginalMatrix = base.transform.localToWorldMatrix * _morphedWorldToLocalMatrix;
		currentAnglesRadians = Quaternion2Angles.GetAngles(_inverseStartingLocalRotation * base.transform.localRotation, rotationOrder);
		currentAngles = currentAnglesRadians * 57.29578f;
	}
}
