using System.Collections.Generic;
using UnityEngine;

public class DAZBones : ScaleChangeReceiverJSONStorable, PhysicsResetter
{
	private Dictionary<string, DAZBone> boneNameToDAZBone;

	private List<DAZBone> boneListInOrder;

	private Dictionary<string, DAZBone> boneIdToDAZBone;

	public DAZBone[] dazBones;

	public bool useScale;

	public bool resetSimulationOnScaleChange;

	public float resetSimulationThreshold = 0.1f;

	[SerializeField]
	private bool _isMale;

	[SerializeField]
	private Dictionary<string, float> _morphGeneralScales;

	private float _currentGeneralScale;

	private bool _wasInit;

	protected List<DAZBone> dirtyBones;

	protected Dictionary<DAZBone, bool> parentDirtyBones;

	protected List<DAZBone> allDirtyBones;

	public bool isMale
	{
		get
		{
			return _isMale;
		}
		set
		{
			if (_isMale != value)
			{
				_isMale = value;
				SetMorphedTransform(true);
			}
		}
	}

	public Dictionary<string, float> morphGeneralScales => _morphGeneralScales;

	public float currentGeneralScale => _currentGeneralScale;

	public bool wasInit => _wasInit;

	public override void PostRestore(bool restorePhysical, bool restoreAppearance)
	{
		if (restorePhysical)
		{
			Init();
			SetMorphedTransform(true);
		}
	}

	public void SnapAllBonesToControls()
	{
		if (dazBones == null)
		{
			return;
		}
		DAZBone[] array = dazBones;
		foreach (DAZBone dAZBone in array)
		{
			if (dAZBone.control != null && dAZBone.control.control != null)
			{
				dAZBone.transform.position = dAZBone.control.control.position;
				dAZBone.transform.rotation = dAZBone.control.control.rotation;
				dAZBone.SetResetVelocity();
			}
			else
			{
				dAZBone.ResetToStartingLocalPositionRotation();
				dAZBone.SetResetVelocity();
			}
		}
		DAZBone[] array2 = dazBones;
		foreach (DAZBone dAZBone2 in array2)
		{
			dAZBone2.RepairJoint();
		}
	}

	public void ResetPhysics()
	{
		SnapAllBonesToControls();
	}

	public override void ScaleChanged(float scale)
	{
		float num = _scale;
		base.ScaleChanged(scale);
		if (_wasInit)
		{
			SetMorphedTransform(true);
			if (resetSimulationOnScaleChange && SuperController.singleton != null && Mathf.Abs(num - _scale) > resetSimulationThreshold)
			{
				SuperController.singleton.ResetSimulation(5, "Bone Scale Change", true);
			}
		}
	}

	public void SetGeneralScale(string morphName, float sc)
	{
		if (_morphGeneralScales == null)
		{
			_morphGeneralScales = new Dictionary<string, float>();
		}
		float value;
		if (_morphGeneralScales.TryGetValue(morphName, out value))
		{
			_morphGeneralScales.Remove(morphName);
		}
		if (sc != 0f)
		{
			_morphGeneralScales.Add(morphName, sc);
		}
		_currentGeneralScale = 0f;
		foreach (float value2 in _morphGeneralScales.Values)
		{
			float num = value2;
			_currentGeneralScale += num;
		}
	}

	public DAZBone GetDAZBone(string boneName)
	{
		Init();
		if (boneNameToDAZBone != null)
		{
			DAZBone value;
			if (boneNameToDAZBone.TryGetValue(boneName, out value))
			{
				return value;
			}
			return null;
		}
		return null;
	}

	public DAZBone GetDAZBoneById(string boneId)
	{
		Init();
		if (boneIdToDAZBone != null)
		{
			DAZBone value;
			if (boneIdToDAZBone.TryGetValue(boneId, out value))
			{
				return value;
			}
			return null;
		}
		return null;
	}

	public void Reset()
	{
		_wasInit = false;
		boneNameToDAZBone = null;
		boneListInOrder = null;
		boneIdToDAZBone = null;
		Init();
	}

	private void InitBonesRecursive(DAZBone parentBone, Transform t)
	{
		foreach (Transform item in t)
		{
			DAZBones component = item.GetComponent<DAZBones>();
			if (!(component == null))
			{
				continue;
			}
			DAZBone component2 = item.GetComponent<DAZBone>();
			if (component2 != null)
			{
				component2.Init();
				component2.dazBones = this;
				component2.parentBone = parentBone;
				if (boneNameToDAZBone.ContainsKey(component2.name))
				{
					Debug.LogError("Found duplicate bone " + component2.name);
				}
				else
				{
					boneNameToDAZBone.Add(component2.name, component2);
					boneListInOrder.Add(component2);
				}
				if (boneIdToDAZBone.ContainsKey(component2.id))
				{
					Debug.LogError("Found duplicate bone id " + component2.id);
				}
				else
				{
					boneIdToDAZBone.Add(component2.id, component2);
				}
				InitBonesRecursive(component2, component2.transform);
			}
			else
			{
				InitBonesRecursive(null, item);
			}
		}
	}

	public void Init()
	{
		if (!_wasInit || boneNameToDAZBone == null)
		{
			_wasInit = true;
			boneNameToDAZBone = new Dictionary<string, DAZBone>();
			boneListInOrder = new List<DAZBone>();
			boneIdToDAZBone = new Dictionary<string, DAZBone>();
			InitBonesRecursive(null, base.transform);
			dazBones = boneListInOrder.ToArray();
			SetMorphedTransform(true);
		}
	}

	public void SetTransformsToImportValues()
	{
		if (dazBones != null)
		{
			DAZBone[] array = dazBones;
			foreach (DAZBone dAZBone in array)
			{
				dAZBone.SetTransformToImportValues();
			}
		}
	}

	public void SetMorphedTransform(bool forceAllDirty = false)
	{
		if (dazBones != null)
		{
			if (dirtyBones == null)
			{
				dirtyBones = new List<DAZBone>();
			}
			else
			{
				dirtyBones.Clear();
			}
			if (parentDirtyBones == null)
			{
				parentDirtyBones = new Dictionary<DAZBone, bool>();
			}
			else
			{
				parentDirtyBones.Clear();
			}
			if (allDirtyBones == null)
			{
				allDirtyBones = new List<DAZBone>();
			}
			else
			{
				allDirtyBones.Clear();
			}
			if (Application.isPlaying)
			{
				DAZBone[] array = dazBones;
				foreach (DAZBone dAZBone in array)
				{
					if (dAZBone.parentForMorphOffsets != null && dAZBone.parentForMorphOffsets.transformDirty)
					{
						dAZBone.transformDirty = true;
					}
					if (dAZBone.transformDirty && dAZBone.parentBone != null)
					{
						dAZBone.parentBone.childDirty = true;
					}
				}
				DAZBone[] array2 = dazBones;
				foreach (DAZBone dAZBone2 in array2)
				{
					if (forceAllDirty || dAZBone2.transformDirty || (dAZBone2.parentBone != null && dAZBone2.parentBone.transformDirty))
					{
						dirtyBones.Add(dAZBone2);
						allDirtyBones.Add(dAZBone2);
						dAZBone2.SaveTransform();
					}
					else if (dAZBone2.childDirty)
					{
						parentDirtyBones.Add(dAZBone2, true);
						allDirtyBones.Add(dAZBone2);
						dAZBone2.SaveTransform();
					}
				}
			}
			else
			{
				DAZBone[] array3 = dazBones;
				foreach (DAZBone item in array3)
				{
					dirtyBones.Add(item);
					allDirtyBones.Add(item);
				}
			}
			foreach (DAZBone dirtyBone in dirtyBones)
			{
				dirtyBone.DetachJoint();
				dirtyBone.SaveAndDetachParent();
				dirtyBone.ResetScale();
			}
			foreach (DAZBone allDirtyBone in allDirtyBones)
			{
				if (parentDirtyBones.ContainsKey(allDirtyBone))
				{
					allDirtyBone.SetTransformToMorphPositionAndRotation(useScale, _scale);
				}
				else
				{
					allDirtyBone.SetMorphedTransform(useScale, _scale);
				}
			}
			if (!Application.isPlaying)
			{
				foreach (DAZBone dirtyBone2 in dirtyBones)
				{
					dirtyBone2.ApplyOffsetTransform();
				}
			}
			foreach (DAZBone dirtyBone3 in dirtyBones)
			{
				if (dirtyBone3.parentBone != null && parentDirtyBones.ContainsKey(dirtyBone3.parentBone))
				{
					dirtyBone3.parentBone.SetTransformToMorphPositionAndRotation(useScale, _scale);
				}
				dirtyBone3.RestoreParent();
				dirtyBone3.AttachJoint();
			}
			if (Application.isPlaying)
			{
				foreach (DAZBone allDirtyBone2 in allDirtyBones)
				{
					allDirtyBone2.RestoreTransform();
				}
				return;
			}
			{
				foreach (DAZBone allDirtyBone3 in allDirtyBones)
				{
					allDirtyBone3.ApplyPresetLocalTransforms();
				}
				return;
			}
		}
		Debug.LogWarning("SetMorphedTransform called when bones were not init");
	}

	private void OnEnable()
	{
		Init();
		SetMorphedTransform(true);
	}

	private void Start()
	{
		Init();
	}
}
