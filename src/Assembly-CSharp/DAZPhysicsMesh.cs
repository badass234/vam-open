using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class DAZPhysicsMesh : PhysicsSimulatorJSONStorable, ISerializationCallbackReceiver
{
	public enum DAZPhysicsMeshTaskType
	{
		UpdateSoftJointTargets,
		MorphVertices
	}

	public class DAZPhysicsMeshTaskInfo
	{
		public DAZPhysicsMeshTaskType taskType;

		public int threadIndex;

		public string name;

		public AutoResetEvent resetEvent;

		public Thread thread;

		public volatile bool working;

		public volatile bool kill;

		public int index1;

		public int index2;
	}

	public enum SelectionMode
	{
		HardTarget,
		HardTargetAuto,
		SoftTarget,
		SoftAnchor,
		SoftInfluenced,
		SoftAuto,
		SoftLink,
		SoftSelect,
		SoftSpringSet,
		ColliderEditEnd1,
		ColliderEditEnd2,
		ColliderEditFront,
		SoftSizeSet,
		SoftLimitSet,
		SoftAutoRadius
	}

	public static bool globalEnable = true;

	public bool editorDirty;

	public Transform transformToEnableWhenOn;

	public Transform transformToEnableWhenOff;

	public Transform[] transformsToEnableWhenOn;

	public Transform[] transformsToEnableWhenOff;

	public DAZCharacterRun characterRun;

	public AutoColliderGroup[] autoColliderGroupsToEnableWhenOff;

	public AutoColliderGroup[] autoColliderGroupsToEnableWhenOn;

	protected bool _globalOn;

	protected JSONStorableBool onJSON;

	[SerializeField]
	protected bool _on = true;

	protected bool _alternateOn = true;

	protected bool morphsChanged;

	public bool useThreading;

	protected DAZPhysicsMeshTaskInfo physicsMeshTask;

	protected bool _threadsRunning;

	[SerializeField]
	protected Transform _skinTransform;

	protected Dictionary<int, List<int>> _baseVertToUVVertFullMap;

	[SerializeField]
	protected DAZSkinV2 _skin;

	[SerializeField]
	protected bool _showHandles;

	[SerializeField]
	protected bool _showBackfaceHandles;

	[SerializeField]
	protected bool _showLinkLines;

	[SerializeField]
	protected bool _showColliders;

	[SerializeField]
	protected bool _showCurrentSoftGroupOnly;

	[SerializeField]
	protected bool _showCurrentSoftSetOnly;

	[SerializeField]
	protected float _handleSize = 0.0002f;

	[SerializeField]
	protected float _softSpringMultiplierSetValue = 1f;

	[SerializeField]
	protected float _softSizeMultiplierSetValue = 1f;

	[SerializeField]
	protected float _softLimitMultiplierSetValue = 1f;

	[SerializeField]
	protected SelectionMode _selectionMode;

	[SerializeField]
	protected int _subMeshSelection = -1;

	[SerializeField]
	protected int _subMeshSelection2 = -1;

	[SerializeField]
	protected bool _showHardGroups;

	[SerializeField]
	protected int _currentHardVerticesGroupIndex;

	[SerializeField]
	protected bool _showSoftGroups;

	[SerializeField]
	protected int _currentSoftVerticesGroupIndex;

	[SerializeField]
	protected bool _showColliderGroups;

	[SerializeField]
	protected int _currentColliderGroupIndex;

	[SerializeField]
	protected List<DAZPhysicsMeshHardVerticesGroup> _hardVerticesGroups;

	[SerializeField]
	protected List<DAZPhysicsMeshSoftVerticesGroup> _softVerticesGroups;

	[SerializeField]
	protected List<DAZPhysicsMeshColliderGroup> _colliderGroups;

	public int[] groupASlots;

	public int[] groupBSlots;

	public int[] groupCSlots;

	public int[] groupDSlots;

	protected JSONStorableFloat groupASpringMultiplierJSON;

	protected JSONStorableFloat groupADamperMultiplierJSON;

	protected JSONStorableFloat groupBSpringMultiplierJSON;

	protected JSONStorableFloat groupBDamperMultiplierJSON;

	protected JSONStorableFloat groupCSpringMultiplierJSON;

	protected JSONStorableFloat groupCDamperMultiplierJSON;

	protected JSONStorableFloat groupDSpringMultiplierJSON;

	protected JSONStorableFloat groupDDamperMultiplierJSON;

	public bool useCombinedSpringAndDamper;

	protected JSONStorableFloat softVerticesCombinedSpringJSON;

	[SerializeField]
	protected float _softVerticesCombinedSpring = 80f;

	protected JSONStorableFloat softVerticesCombinedDamperJSON;

	[SerializeField]
	protected float _softVerticesCombinedDamper = 1f;

	public Slider softVerticesNormalSpringSlider;

	[SerializeField]
	protected float _softVerticesNormalSpring = 10f;

	public Slider softVerticesNormalDamperSlider;

	[SerializeField]
	protected float _softVerticesNormalDamper = 1f;

	public Slider softVerticesTangentSpringSlider;

	[SerializeField]
	protected float _softVerticesTangentSpring = 10f;

	public Slider softVerticesTangentDamperSlider;

	[SerializeField]
	protected float _softVerticesTangentDamper = 1f;

	public Slider softVerticesSpringMaxForceSlider;

	[SerializeField]
	protected float _softVerticesSpringMaxForce = 10f;

	protected JSONStorableFloat softVerticesMassJSON;

	[SerializeField]
	protected float _softVerticesMass = 0.1f;

	protected JSONStorableFloat softVerticesBackForceJSON;

	[SerializeField]
	protected float _softVerticesBackForce;

	protected JSONStorableFloat softVerticesBackForceThresholdDistanceJSON;

	[SerializeField]
	protected float _softVerticesBackForceThresholdDistance = 0.01f;

	protected JSONStorableFloat softVerticesBackForceMaxForceJSON;

	[SerializeField]
	protected float _softVerticesBackForceMaxForce;

	[SerializeField]
	protected bool _multiplyMassByLimitMultiplier = true;

	public Toggle softVerticesUseUniformLimitToggle;

	[SerializeField]
	protected bool _softVerticesUseUniformLimit;

	public Slider softVerticesTangentLimitSlider;

	[SerializeField]
	protected float _softVerticesTangentLimit;

	protected JSONStorableFloat softVerticesNormalLimitJSON;

	[SerializeField]
	protected float _softVerticesNormalLimit;

	public Slider softVerticesNegativeNormalLimitSlider;

	[SerializeField]
	protected float _softVerticesNegativeNormalLimit;

	public int softVerticesAutoColliderVertex1 = -1;

	public int softVerticesAutoColliderVertex2 = -1;

	public float softVerticesAutoColliderMinRadius = 0.02f;

	public float softVerticesAutoColliderMaxRadius = 0.05f;

	public float softVerticesAutoColliderRadiusMultiplier = 1f;

	public float softVerticesAutoColliderRadiusOffset;

	public JSONStorableBool softVerticesUseAutoColliderRadiusJSON;

	[SerializeField]
	protected bool _softVerticesUseAutoColliderRadius;

	protected const float radiusAdjustThreshold = 1000f;

	public JSONStorableFloat softVerticesColliderRadiusJSON;

	protected float _softVerticesColliderRadiusThreaded;

	[SerializeField]
	protected float _softVerticesColliderRadius;

	public JSONStorableFloat softVerticesColliderAdditionalNormalOffsetJSON;

	[SerializeField]
	protected float _softVerticesColliderAdditionalNormalOffset;

	[SerializeField]
	protected int _numPredictionFrames;

	protected Dictionary<int, DAZPhysicsMeshHardVerticesGroup> _hardTargetVerticesDict;

	protected Dictionary<int, DAZPhysicsMeshSoftVerticesSet> _softTargetVerticesDict;

	protected Dictionary<int, List<DAZPhysicsMeshSoftVerticesSet>> _softAnchorVerticesDict;

	protected Dictionary<int, DAZPhysicsMeshSoftVerticesSet> _softInfluenceVerticesDict;

	protected Dictionary<string, bool> _softVerticesInGroupDict;

	protected Dictionary<int, int> _uvVertToBaseVertDict;

	protected Dictionary<DAZPhysicsMeshSoftVerticesSet, DAZPhysicsMeshSoftVerticesGroup> _softSetToGroupDict;

	protected DAZPhysicsMeshSoftVerticesSet startSoftLinkSet;

	protected Vector3 zeroVector = Vector3.zero;

	[SerializeField]
	protected bool _allowSelfCollision;

	protected JSONStorableBool allowSelfCollisionJSON;

	public DAZPhysicsMesh[] ignorePhysicsMeshes;

	protected Mesh _editorMeshForFocus;

	protected bool _wasInit;

	public bool isEnabled;

	public bool updateEnabled = true;

	public bool on
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
				_on = value;
				SyncOn();
			}
		}
	}

	public bool alternateOn
	{
		get
		{
			return _alternateOn;
		}
		set
		{
			if (_alternateOn != value)
			{
				_alternateOn = value;
				SyncOn();
			}
		}
	}

	public Transform skinTransform
	{
		get
		{
			return _skinTransform;
		}
		set
		{
			if (_skinTransform != value)
			{
				_skinTransform = value;
			}
		}
	}

	public DAZSkinV2 skin
	{
		get
		{
			return _skin;
		}
		set
		{
			if (!(_skin != value))
			{
				return;
			}
			_skin = value;
			if (!(_skin != null))
			{
				return;
			}
			Init();
			_skin.Init();
			foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
			{
				softVerticesGroup.skin = _skin;
			}
			if (_skin.dazMesh != null)
			{
				_baseVertToUVVertFullMap = _skin.dazMesh.baseVertToUVVertFullMap;
			}
		}
	}

	public bool showHandles
	{
		get
		{
			return _showHandles;
		}
		set
		{
			if (_showHandles != value)
			{
				_showHandles = value;
			}
		}
	}

	public bool showBackfaceHandles
	{
		get
		{
			return _showBackfaceHandles;
		}
		set
		{
			if (_showBackfaceHandles != value)
			{
				_showBackfaceHandles = value;
			}
		}
	}

	public bool showLinkLines
	{
		get
		{
			return _showLinkLines;
		}
		set
		{
			if (_showLinkLines != value)
			{
				_showLinkLines = value;
			}
		}
	}

	public bool showColliders
	{
		get
		{
			return _showColliders;
		}
		set
		{
			if (_showColliders != value)
			{
				_showColliders = value;
			}
		}
	}

	public bool showCurrentSoftGroupOnly
	{
		get
		{
			return _showCurrentSoftGroupOnly;
		}
		set
		{
			if (_showCurrentSoftGroupOnly != value)
			{
				_showCurrentSoftGroupOnly = value;
			}
		}
	}

	public bool showCurrentSoftSetOnly
	{
		get
		{
			return _showCurrentSoftSetOnly;
		}
		set
		{
			if (_showCurrentSoftSetOnly != value)
			{
				_showCurrentSoftSetOnly = value;
			}
		}
	}

	public float handleSize
	{
		get
		{
			return _handleSize;
		}
		set
		{
			if (_handleSize != value)
			{
				_handleSize = value;
			}
		}
	}

	public float softSpringMultiplierSetValue
	{
		get
		{
			return _softSpringMultiplierSetValue;
		}
		set
		{
			if (_softSpringMultiplierSetValue != value)
			{
				_softSpringMultiplierSetValue = value;
			}
		}
	}

	public float softSizeMultiplierSetValue
	{
		get
		{
			return _softSizeMultiplierSetValue;
		}
		set
		{
			if (_softSizeMultiplierSetValue != value)
			{
				_softSizeMultiplierSetValue = value;
			}
		}
	}

	public float softLimitMultiplierSetValue
	{
		get
		{
			return _softLimitMultiplierSetValue;
		}
		set
		{
			if (_softLimitMultiplierSetValue != value)
			{
				_softLimitMultiplierSetValue = value;
			}
		}
	}

	public SelectionMode selectionMode
	{
		get
		{
			return _selectionMode;
		}
		set
		{
			if (_selectionMode != value)
			{
				_selectionMode = value;
			}
		}
	}

	public int subMeshSelection
	{
		get
		{
			return _subMeshSelection;
		}
		set
		{
			if (value != _subMeshSelection)
			{
				_subMeshSelection = value;
			}
		}
	}

	public int subMeshSelection2
	{
		get
		{
			return _subMeshSelection2;
		}
		set
		{
			if (value != _subMeshSelection2)
			{
				_subMeshSelection2 = value;
			}
		}
	}

	public bool showHardGroups
	{
		get
		{
			return _showHardGroups;
		}
		set
		{
			if (_showHardGroups != value)
			{
				_showHardGroups = value;
			}
		}
	}

	public int currentHardVerticesGroupIndex
	{
		get
		{
			return _currentHardVerticesGroupIndex;
		}
		set
		{
			if (_currentHardVerticesGroupIndex != value)
			{
				_currentHardVerticesGroupIndex = value;
			}
		}
	}

	public DAZPhysicsMeshHardVerticesGroup currentHardVerticesGroup
	{
		get
		{
			if (_currentHardVerticesGroupIndex >= 0 && _currentHardVerticesGroupIndex < _hardVerticesGroups.Count)
			{
				return _hardVerticesGroups[_currentHardVerticesGroupIndex];
			}
			return null;
		}
	}

	public bool showSoftGroups
	{
		get
		{
			return _showSoftGroups;
		}
		set
		{
			if (_showSoftGroups != value)
			{
				_showSoftGroups = value;
			}
		}
	}

	public int currentSoftVerticesGroupIndex
	{
		get
		{
			return _currentSoftVerticesGroupIndex;
		}
		set
		{
			if (_currentSoftVerticesGroupIndex != value)
			{
				_currentSoftVerticesGroupIndex = value;
			}
		}
	}

	public DAZPhysicsMeshSoftVerticesGroup currentSoftVerticesGroup
	{
		get
		{
			if (_currentSoftVerticesGroupIndex >= 0 && _currentSoftVerticesGroupIndex < _softVerticesGroups.Count)
			{
				return _softVerticesGroups[_currentSoftVerticesGroupIndex];
			}
			return null;
		}
		set
		{
			for (int i = 0; i < _softVerticesGroups.Count; i++)
			{
				if (_softVerticesGroups[i] == value)
				{
					_currentSoftVerticesGroupIndex = i;
				}
			}
		}
	}

	public bool showColliderGroups
	{
		get
		{
			return _showColliderGroups;
		}
		set
		{
			if (_showColliderGroups != value)
			{
				_showColliderGroups = value;
			}
		}
	}

	public int currentColliderGroupIndex
	{
		get
		{
			return _currentColliderGroupIndex;
		}
		set
		{
			if (_currentColliderGroupIndex != value)
			{
				_currentColliderGroupIndex = value;
			}
		}
	}

	public List<DAZPhysicsMeshHardVerticesGroup> hardVerticesGroups => _hardVerticesGroups;

	public List<DAZPhysicsMeshSoftVerticesGroup> softVerticesGroups => _softVerticesGroups;

	public List<DAZPhysicsMeshColliderGroup> colliderGroups => _colliderGroups;

	public float softVerticesCombinedSpring
	{
		get
		{
			return _softVerticesCombinedSpring;
		}
		set
		{
			if (softVerticesCombinedSpringJSON != null)
			{
				softVerticesCombinedSpringJSON.val = value;
			}
			else if (_softVerticesCombinedSpring != value)
			{
				SyncSoftVerticesCombinedSpring(value);
			}
		}
	}

	public float softVerticesCombinedDamper
	{
		get
		{
			return _softVerticesCombinedDamper;
		}
		set
		{
			if (softVerticesCombinedDamperJSON != null)
			{
				softVerticesCombinedDamperJSON.val = value;
			}
			else if (_softVerticesCombinedDamper != value)
			{
				SyncSoftVerticesCombinedDamper(value);
			}
		}
	}

	public float softVerticesNormalSpring
	{
		get
		{
			return _softVerticesNormalSpring;
		}
		set
		{
			if (!useCombinedSpringAndDamper && _softVerticesNormalSpring != value)
			{
				_softVerticesNormalSpring = value;
				if (softVerticesNormalSpringSlider != null)
				{
					softVerticesNormalSpringSlider.value = _softVerticesNormalSpring;
				}
				SyncSoftVerticesNormalSpring();
			}
		}
	}

	public float softVerticesNormalDamper
	{
		get
		{
			return _softVerticesNormalDamper;
		}
		set
		{
			if (!useCombinedSpringAndDamper && _softVerticesNormalDamper != value)
			{
				_softVerticesNormalDamper = value;
				if (softVerticesNormalDamperSlider != null)
				{
					softVerticesNormalDamperSlider.value = _softVerticesNormalDamper;
				}
				SyncSoftVerticesNormalDamper();
			}
		}
	}

	public float softVerticesTangentSpring
	{
		get
		{
			return _softVerticesTangentSpring;
		}
		set
		{
			if (!useCombinedSpringAndDamper && _softVerticesTangentSpring != value)
			{
				_softVerticesTangentSpring = value;
				if (softVerticesTangentSpringSlider != null)
				{
					softVerticesTangentSpringSlider.value = _softVerticesTangentSpring;
				}
				SyncSoftVerticesTangentSpring();
			}
		}
	}

	public float softVerticesTangentDamper
	{
		get
		{
			return _softVerticesTangentDamper;
		}
		set
		{
			if (!useCombinedSpringAndDamper && _softVerticesTangentDamper != value)
			{
				_softVerticesTangentDamper = value;
				if (softVerticesTangentDamperSlider != null)
				{
					softVerticesTangentDamperSlider.value = _softVerticesTangentDamper;
				}
				SyncSoftVerticesTangentDamper();
			}
		}
	}

	public float softVerticesSpringMaxForce
	{
		get
		{
			return _softVerticesSpringMaxForce;
		}
		set
		{
			if (_softVerticesSpringMaxForce != value)
			{
				_softVerticesSpringMaxForce = value;
				if (softVerticesSpringMaxForceSlider != null)
				{
					softVerticesSpringMaxForceSlider.value = _softVerticesSpringMaxForce;
				}
				SyncSoftVerticesSpringMaxForce();
			}
		}
	}

	public float softVerticesMass
	{
		get
		{
			return _softVerticesMass;
		}
		set
		{
			if (softVerticesMassJSON != null)
			{
				softVerticesMassJSON.val = value;
			}
			else if (_softVerticesMass != value)
			{
				SyncSoftVerticesMass(value);
			}
		}
	}

	public float softVerticesBackForce
	{
		get
		{
			return _softVerticesBackForce;
		}
		set
		{
			if (softVerticesBackForceJSON != null)
			{
				softVerticesBackForceJSON.val = value;
			}
			else if (_softVerticesBackForce != value)
			{
				SyncSoftVerticesBackForce(value);
			}
		}
	}

	public float softVerticesBackForceThresholdDistance
	{
		get
		{
			return _softVerticesBackForceThresholdDistance;
		}
		set
		{
			if (softVerticesBackForceThresholdDistanceJSON != null)
			{
				softVerticesBackForceThresholdDistanceJSON.val = value;
			}
			else if (_softVerticesBackForceThresholdDistance != value)
			{
				SyncSoftVerticesBackForceThresholdDistance(value);
			}
		}
	}

	public float softVerticesBackForceMaxForce
	{
		get
		{
			return _softVerticesBackForceMaxForce;
		}
		set
		{
			if (softVerticesBackForceMaxForceJSON != null)
			{
				softVerticesBackForceMaxForceJSON.val = value;
			}
			else if (_softVerticesBackForceMaxForce != value)
			{
				SyncSoftVerticesBackForceMaxForce(value);
			}
		}
	}

	public bool multiplyMassByLimitMultiplier
	{
		get
		{
			return _multiplyMassByLimitMultiplier;
		}
		set
		{
			_multiplyMassByLimitMultiplier = value;
			foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in softVerticesGroups)
			{
				softVerticesGroup.multiplyMassByLimitMultiplier = _multiplyMassByLimitMultiplier;
			}
		}
	}

	public bool softVerticesUseUniformLimit
	{
		get
		{
			return _softVerticesUseUniformLimit;
		}
		set
		{
			if (_softVerticesUseUniformLimit != value)
			{
				_softVerticesUseUniformLimit = value;
				if (softVerticesUseUniformLimitToggle != null)
				{
					softVerticesUseUniformLimitToggle.isOn = _softVerticesUseUniformLimit;
				}
				SyncSoftVerticesUseUniformLimit();
			}
		}
	}

	public float softVerticesTangentLimit
	{
		get
		{
			return _softVerticesTangentLimit;
		}
		set
		{
			if (_softVerticesTangentLimit != value)
			{
				_softVerticesTangentLimit = value;
				if (softVerticesTangentLimitSlider != null)
				{
					softVerticesTangentLimitSlider.value = _softVerticesTangentLimit;
				}
				SyncSoftVerticesTangentLimit();
			}
		}
	}

	public float softVerticesNormalLimit
	{
		get
		{
			return _softVerticesNormalLimit;
		}
		set
		{
			if (softVerticesNormalLimitJSON != null)
			{
				softVerticesNormalLimitJSON.val = value;
			}
			else if (_softVerticesNormalLimit != value)
			{
				SyncSoftVerticesNormalLimit(value);
			}
		}
	}

	public float softVerticesNegativeNormalLimit
	{
		get
		{
			return _softVerticesNegativeNormalLimit;
		}
		set
		{
			if (_softVerticesNegativeNormalLimit != value)
			{
				_softVerticesNegativeNormalLimit = value;
				if (softVerticesNegativeNormalLimitSlider != null)
				{
					softVerticesNegativeNormalLimitSlider.value = _softVerticesNegativeNormalLimit;
				}
				SyncSoftVerticesNegativeNormalLimit();
			}
		}
	}

	public bool softVerticesUseAutoColliderRadius
	{
		get
		{
			return _softVerticesUseAutoColliderRadius;
		}
		set
		{
			if (softVerticesUseAutoColliderRadiusJSON != null)
			{
				softVerticesUseAutoColliderRadiusJSON.val = value;
			}
			else if (_softVerticesUseAutoColliderRadius != value)
			{
				SyncSoftVerticesUseAutoColliderRadius(value);
			}
		}
	}

	public float softVerticesColliderRadius
	{
		get
		{
			return _softVerticesColliderRadius;
		}
		set
		{
			if (softVerticesColliderRadiusJSON != null)
			{
				softVerticesColliderRadiusJSON.val = value;
			}
			else if (_softVerticesColliderRadius != value)
			{
				SyncSoftVerticesColliderRadius(value);
			}
		}
	}

	public float softVerticesColliderAdditionalNormalOffset
	{
		get
		{
			return _softVerticesColliderAdditionalNormalOffset;
		}
		set
		{
			if (softVerticesColliderAdditionalNormalOffsetJSON != null)
			{
				softVerticesColliderAdditionalNormalOffsetJSON.val = value;
			}
			else if (_softVerticesColliderAdditionalNormalOffset != value)
			{
				SyncSoftVerticesColliderAdditionalNormalOffset(value);
			}
		}
	}

	public int numPredictionFrames
	{
		get
		{
			return _numPredictionFrames;
		}
		set
		{
			if (_numPredictionFrames == value)
			{
				return;
			}
			_numPredictionFrames = value;
			foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
			{
				softVerticesGroup.numPredictionFrames = _numPredictionFrames;
			}
		}
	}

	public bool allowSelfCollision
	{
		get
		{
			return _allowSelfCollision;
		}
		set
		{
			if (allowSelfCollisionJSON != null)
			{
				allowSelfCollisionJSON.val = value;
			}
			else if (_allowSelfCollision != value)
			{
				SyncAllowSelfCollision(value);
			}
		}
	}

	public Mesh editorMeshForFocus => _editorMeshForFocus;

	public bool wasInit => _wasInit;

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
	}

	protected override void SyncResetSimulation()
	{
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			softVerticesGroup.resetSimulation = _resetSimulation;
		}
	}

	protected override void SyncFreezeSimulation()
	{
		SyncOn();
	}

	protected void SyncOnCallback(bool b)
	{
		_on = b;
		SyncOn();
	}

	protected void SyncOn(bool resetSim = true)
	{
		_globalOn = globalEnable;
		bool flag = _globalOn && _on && _alternateOn;
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			softVerticesGroup.on = flag;
			softVerticesGroup.freeze = _freezeSimulation;
		}
		if (!Application.isPlaying)
		{
			return;
		}
		if (flag)
		{
			if (transformToEnableWhenOn != null)
			{
				transformToEnableWhenOn.gameObject.SetActive(true);
			}
			if (transformsToEnableWhenOn != null)
			{
				Transform[] array = transformsToEnableWhenOn;
				foreach (Transform transform in array)
				{
					if (transform != null)
					{
						transform.gameObject.SetActive(true);
					}
				}
			}
			if (transformToEnableWhenOff != null)
			{
				transformToEnableWhenOff.gameObject.SetActive(false);
			}
			if (transformsToEnableWhenOff != null)
			{
				Transform[] array2 = transformsToEnableWhenOff;
				foreach (Transform transform2 in array2)
				{
					if (transform2 != null)
					{
						transform2.gameObject.SetActive(false);
					}
				}
			}
			bool flag2 = false;
			if (autoColliderGroupsToEnableWhenOff != null)
			{
				AutoColliderGroup[] array3 = autoColliderGroupsToEnableWhenOff;
				foreach (AutoColliderGroup autoColliderGroup in array3)
				{
					if (autoColliderGroup.enabled)
					{
						autoColliderGroup.enabled = false;
						flag2 = true;
					}
				}
			}
			if (autoColliderGroupsToEnableWhenOn != null)
			{
				AutoColliderGroup[] array4 = autoColliderGroupsToEnableWhenOn;
				foreach (AutoColliderGroup autoColliderGroup2 in array4)
				{
					if (!autoColliderGroup2.enabled)
					{
						autoColliderGroup2.enabled = true;
						flag2 = true;
					}
				}
			}
			if (flag2 && characterRun != null)
			{
				characterRun.SetNeedsAutoColliderUpdate();
			}
			base.gameObject.SetActive(false);
			base.gameObject.SetActive(true);
			if (!_freezeSimulation && resetSim)
			{
				containingAtom.ResetPhysics(false);
			}
			return;
		}
		if (transformToEnableWhenOn != null)
		{
			transformToEnableWhenOn.gameObject.SetActive(false);
		}
		if (transformsToEnableWhenOn != null)
		{
			Transform[] array5 = transformsToEnableWhenOn;
			foreach (Transform transform3 in array5)
			{
				if (transform3 != null)
				{
					transform3.gameObject.SetActive(false);
				}
			}
		}
		if (transformToEnableWhenOff != null)
		{
			transformToEnableWhenOff.gameObject.SetActive(true);
		}
		if (transformsToEnableWhenOff != null)
		{
			Transform[] array6 = transformsToEnableWhenOff;
			foreach (Transform transform4 in array6)
			{
				if (transform4 != null)
				{
					transform4.gameObject.SetActive(true);
				}
			}
		}
		bool flag3 = false;
		if (autoColliderGroupsToEnableWhenOff != null)
		{
			AutoColliderGroup[] array7 = autoColliderGroupsToEnableWhenOff;
			foreach (AutoColliderGroup autoColliderGroup3 in array7)
			{
				if (!autoColliderGroup3.enabled)
				{
					autoColliderGroup3.enabled = true;
					flag3 = true;
				}
			}
		}
		if (autoColliderGroupsToEnableWhenOn != null)
		{
			AutoColliderGroup[] array8 = autoColliderGroupsToEnableWhenOn;
			foreach (AutoColliderGroup autoColliderGroup4 in array8)
			{
				if (autoColliderGroup4.enabled)
				{
					autoColliderGroup4.enabled = false;
					flag3 = true;
				}
			}
		}
		if (flag3 && characterRun != null)
		{
			characterRun.SetNeedsAutoColliderUpdate();
		}
	}

	protected void MTTask(object info)
	{
		DAZPhysicsMeshTaskInfo dAZPhysicsMeshTaskInfo = (DAZPhysicsMeshTaskInfo)info;
		while (_threadsRunning)
		{
			dAZPhysicsMeshTaskInfo.resetEvent.WaitOne(-1, true);
			if (dAZPhysicsMeshTaskInfo.kill)
			{
				break;
			}
			if (dAZPhysicsMeshTaskInfo.taskType == DAZPhysicsMeshTaskType.UpdateSoftJointTargets)
			{
				Thread.Sleep(0);
				UpdateSoftJointTargetsThreaded();
			}
			else if (dAZPhysicsMeshTaskInfo.taskType == DAZPhysicsMeshTaskType.MorphVertices)
			{
				Thread.Sleep(0);
				MorphSoftVerticesThreaded();
			}
			dAZPhysicsMeshTaskInfo.working = false;
		}
	}

	protected void StopThreads()
	{
		_threadsRunning = false;
		if (physicsMeshTask != null)
		{
			physicsMeshTask.kill = true;
			physicsMeshTask.resetEvent.Set();
			while (physicsMeshTask.thread.IsAlive)
			{
			}
			physicsMeshTask = null;
		}
	}

	protected void StartThreads()
	{
		if (useThreading && !_threadsRunning)
		{
			_threadsRunning = true;
			physicsMeshTask = new DAZPhysicsMeshTaskInfo();
			physicsMeshTask.threadIndex = 0;
			physicsMeshTask.name = "UpdateSoftJointTargetsTask";
			physicsMeshTask.resetEvent = new AutoResetEvent(false);
			physicsMeshTask.thread = new Thread(MTTask);
			physicsMeshTask.thread.Priority = System.Threading.ThreadPriority.BelowNormal;
			physicsMeshTask.taskType = DAZPhysicsMeshTaskType.UpdateSoftJointTargets;
			physicsMeshTask.thread.Start(physicsMeshTask);
		}
	}

	protected override void SyncCollisionEnabled()
	{
		base.SyncCollisionEnabled();
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			softVerticesGroup.collisionEnabled = _collisionEnabled;
		}
	}

	protected override void SyncUseInterpolation()
	{
		base.SyncUseInterpolation();
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			softVerticesGroup.useInterpolation = _useInterpolation;
		}
	}

	protected override void SyncSolverIterations()
	{
		base.SyncSolverIterations();
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			softVerticesGroup.solverIterations = _solverIterations;
		}
	}

	protected void SyncGroupASpringMultiplier(float f)
	{
		for (int i = 0; i < groupASlots.Length; i++)
		{
			int num = groupASlots[i];
			if (num < softVerticesGroups.Count)
			{
				DAZPhysicsMeshSoftVerticesGroup dAZPhysicsMeshSoftVerticesGroup = softVerticesGroups[num];
				if (dAZPhysicsMeshSoftVerticesGroup != null)
				{
					dAZPhysicsMeshSoftVerticesGroup.parentSettingSpringMultiplier = f;
				}
			}
		}
		SyncSoftVerticesCombinedSpring(_softVerticesCombinedSpring);
	}

	protected void SyncGroupADamperMultiplier(float f)
	{
		for (int i = 0; i < groupASlots.Length; i++)
		{
			int num = groupASlots[i];
			if (num < softVerticesGroups.Count)
			{
				DAZPhysicsMeshSoftVerticesGroup dAZPhysicsMeshSoftVerticesGroup = softVerticesGroups[num];
				if (dAZPhysicsMeshSoftVerticesGroup != null)
				{
					dAZPhysicsMeshSoftVerticesGroup.parentSettingDamperMultiplier = f;
				}
			}
		}
		SyncSoftVerticesCombinedDamper(_softVerticesCombinedDamper);
	}

	protected void SyncGroupBSpringMultiplier(float f)
	{
		for (int i = 0; i < groupBSlots.Length; i++)
		{
			int num = groupBSlots[i];
			if (num < softVerticesGroups.Count)
			{
				DAZPhysicsMeshSoftVerticesGroup dAZPhysicsMeshSoftVerticesGroup = softVerticesGroups[num];
				if (dAZPhysicsMeshSoftVerticesGroup != null)
				{
					dAZPhysicsMeshSoftVerticesGroup.parentSettingSpringMultiplier = f;
				}
			}
		}
		SyncSoftVerticesCombinedSpring(_softVerticesCombinedSpring);
	}

	protected void SyncGroupBDamperMultiplier(float f)
	{
		for (int i = 0; i < groupBSlots.Length; i++)
		{
			int num = groupBSlots[i];
			if (num < softVerticesGroups.Count)
			{
				DAZPhysicsMeshSoftVerticesGroup dAZPhysicsMeshSoftVerticesGroup = softVerticesGroups[num];
				if (dAZPhysicsMeshSoftVerticesGroup != null)
				{
					dAZPhysicsMeshSoftVerticesGroup.parentSettingDamperMultiplier = f;
				}
			}
		}
		SyncSoftVerticesCombinedDamper(_softVerticesCombinedDamper);
	}

	protected void SyncGroupCSpringMultiplier(float f)
	{
		for (int i = 0; i < groupCSlots.Length; i++)
		{
			int num = groupCSlots[i];
			if (num < softVerticesGroups.Count)
			{
				DAZPhysicsMeshSoftVerticesGroup dAZPhysicsMeshSoftVerticesGroup = softVerticesGroups[num];
				if (dAZPhysicsMeshSoftVerticesGroup != null)
				{
					dAZPhysicsMeshSoftVerticesGroup.parentSettingSpringMultiplier = f;
				}
			}
		}
		SyncSoftVerticesCombinedSpring(_softVerticesCombinedSpring);
	}

	protected void SyncGroupCDamperMultiplier(float f)
	{
		for (int i = 0; i < groupCSlots.Length; i++)
		{
			int num = groupCSlots[i];
			if (num < softVerticesGroups.Count)
			{
				DAZPhysicsMeshSoftVerticesGroup dAZPhysicsMeshSoftVerticesGroup = softVerticesGroups[num];
				if (dAZPhysicsMeshSoftVerticesGroup != null)
				{
					dAZPhysicsMeshSoftVerticesGroup.parentSettingDamperMultiplier = f;
				}
			}
		}
		SyncSoftVerticesCombinedDamper(_softVerticesCombinedDamper);
	}

	protected void SyncGroupDSpringMultiplier(float f)
	{
		for (int i = 0; i < groupDSlots.Length; i++)
		{
			int num = groupDSlots[i];
			if (num < softVerticesGroups.Count)
			{
				DAZPhysicsMeshSoftVerticesGroup dAZPhysicsMeshSoftVerticesGroup = softVerticesGroups[num];
				if (dAZPhysicsMeshSoftVerticesGroup != null)
				{
					dAZPhysicsMeshSoftVerticesGroup.parentSettingSpringMultiplier = f;
				}
			}
		}
		SyncSoftVerticesCombinedSpring(_softVerticesCombinedSpring);
	}

	protected void SyncGroupDDamperMultiplier(float f)
	{
		for (int i = 0; i < groupDSlots.Length; i++)
		{
			int num = groupDSlots[i];
			if (num < softVerticesGroups.Count)
			{
				DAZPhysicsMeshSoftVerticesGroup dAZPhysicsMeshSoftVerticesGroup = softVerticesGroups[num];
				if (dAZPhysicsMeshSoftVerticesGroup != null)
				{
					dAZPhysicsMeshSoftVerticesGroup.parentSettingDamperMultiplier = f;
				}
			}
		}
		SyncSoftVerticesCombinedDamper(_softVerticesCombinedDamper);
	}

	protected void SyncSoftVerticesCombinedSpring(float f)
	{
		_softVerticesCombinedSpring = f;
		if (!useCombinedSpringAndDamper)
		{
			return;
		}
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in softVerticesGroups)
		{
			if (softVerticesGroup.useParentSettings)
			{
				float linkSpring = (softVerticesGroup.jointSpringTangent2 = (softVerticesGroup.jointSpringTangent = (softVerticesGroup.jointSpringNormal = _softVerticesCombinedSpring * softVerticesGroup.parentSettingSpringMultiplier)));
				if (softVerticesGroup.tieLinkJointSpringAndDamperToNormalSpringAndDamper)
				{
					softVerticesGroup.linkSpring = linkSpring;
				}
			}
		}
	}

	protected void SyncSoftVerticesCombinedDamper(float f)
	{
		_softVerticesCombinedDamper = f;
		if (!useCombinedSpringAndDamper)
		{
			return;
		}
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in softVerticesGroups)
		{
			if (softVerticesGroup.useParentSettings)
			{
				float linkDamper = (softVerticesGroup.jointDamperTangent2 = (softVerticesGroup.jointDamperTangent = (softVerticesGroup.jointDamperNormal = _softVerticesCombinedDamper * softVerticesGroup.parentSettingDamperMultiplier)));
				if (softVerticesGroup.tieLinkJointSpringAndDamperToNormalSpringAndDamper)
				{
					softVerticesGroup.linkDamper = linkDamper;
				}
			}
		}
	}

	protected void SyncSoftVerticesNormalSpring()
	{
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in softVerticesGroups)
		{
			if (softVerticesGroup.useParentSettings)
			{
				softVerticesGroup.jointSpringNormal = _softVerticesNormalSpring * softVerticesGroup.parentSettingSpringMultiplier;
			}
		}
	}

	protected void SyncSoftVerticesNormalDamper()
	{
		if (useCombinedSpringAndDamper)
		{
			return;
		}
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in softVerticesGroups)
		{
			if (softVerticesGroup.useParentSettings)
			{
				softVerticesGroup.jointDamperNormal = _softVerticesNormalDamper * softVerticesGroup.parentSettingDamperMultiplier;
			}
		}
	}

	protected void SyncSoftVerticesTangentSpring()
	{
		if (useCombinedSpringAndDamper)
		{
			return;
		}
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in softVerticesGroups)
		{
			if (softVerticesGroup.useParentSettings)
			{
				softVerticesGroup.jointSpringTangent = _softVerticesTangentSpring * softVerticesGroup.parentSettingSpringMultiplier;
				softVerticesGroup.jointSpringTangent2 = _softVerticesTangentSpring * softVerticesGroup.parentSettingSpringMultiplier;
			}
		}
	}

	protected void SyncSoftVerticesTangentDamper()
	{
		if (useCombinedSpringAndDamper)
		{
			return;
		}
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in softVerticesGroups)
		{
			if (softVerticesGroup.useParentSettings)
			{
				softVerticesGroup.jointDamperTangent = _softVerticesTangentDamper * softVerticesGroup.parentSettingDamperMultiplier;
				softVerticesGroup.jointDamperTangent2 = _softVerticesTangentDamper * softVerticesGroup.parentSettingDamperMultiplier;
			}
		}
	}

	protected void SyncSoftVerticesSpringMaxForce()
	{
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in softVerticesGroups)
		{
			if (softVerticesGroup.useParentSettings)
			{
				softVerticesGroup.jointSpringMaxForce = _softVerticesSpringMaxForce;
			}
		}
	}

	protected void SyncSoftVerticesMass(float f)
	{
		_softVerticesMass = f;
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in softVerticesGroups)
		{
			if (softVerticesGroup.useParentSettings)
			{
				softVerticesGroup.jointMass = _softVerticesMass;
			}
		}
	}

	protected void SyncSoftVerticesBackForce(float f)
	{
		_softVerticesBackForce = f;
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in softVerticesGroups)
		{
			if (softVerticesGroup.useParentSettings)
			{
				softVerticesGroup.jointBackForce = _softVerticesBackForce;
			}
		}
	}

	protected void SyncSoftVerticesBackForceThresholdDistance(float f)
	{
		_softVerticesBackForceThresholdDistance = f;
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in softVerticesGroups)
		{
			if (softVerticesGroup.useParentSettings)
			{
				softVerticesGroup.jointBackForceThresholdDistance = _softVerticesBackForceThresholdDistance;
			}
		}
	}

	protected void SyncSoftVerticesBackForceMaxForce(float f)
	{
		_softVerticesBackForceMaxForce = f;
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in softVerticesGroups)
		{
			if (softVerticesGroup.useParentSettings)
			{
				softVerticesGroup.jointBackForceMaxForce = _softVerticesBackForceMaxForce;
			}
		}
	}

	protected void SyncSoftVerticesUseUniformLimit()
	{
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in softVerticesGroups)
		{
			if (softVerticesGroup.useParentSettings)
			{
				softVerticesGroup.useUniformLimit = _softVerticesUseUniformLimit;
			}
		}
	}

	protected void SyncSoftVerticesTangentLimit()
	{
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in softVerticesGroups)
		{
			if (softVerticesGroup.useParentSettings)
			{
				softVerticesGroup.tangentDistanceLimit = _softVerticesTangentLimit;
				softVerticesGroup.tangentNegativeDistanceLimit = _softVerticesTangentLimit;
				softVerticesGroup.tangent2DistanceLimit = _softVerticesTangentLimit;
				softVerticesGroup.tangent2NegativeDistanceLimit = _softVerticesTangentLimit;
			}
		}
	}

	protected void SyncSoftVerticesNormalLimit(float f)
	{
		_softVerticesNormalLimit = f;
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in softVerticesGroups)
		{
			if (softVerticesGroup.useParentSettings)
			{
				softVerticesGroup.normalDistanceLimit = _softVerticesNormalLimit;
			}
		}
	}

	protected void SyncSoftVerticesNegativeNormalLimit()
	{
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in softVerticesGroups)
		{
			if (softVerticesGroup.useParentSettings)
			{
				softVerticesGroup.normalNegativeDistanceLimit = _softVerticesNegativeNormalLimit;
			}
		}
	}

	protected void SyncSoftVerticesUseAutoColliderRadius(bool b)
	{
		_softVerticesUseAutoColliderRadius = b;
		if (updateEnabled)
		{
			SoftVerticesSetAutoRadius();
		}
	}

	public void SoftVerticesSetAutoRadiusFast(Vector3[] verts)
	{
		if (!(skin != null) || !_softVerticesUseAutoColliderRadius || softVerticesAutoColliderVertex1 == -1 || softVerticesAutoColliderVertex2 == -1)
		{
			return;
		}
		float num = (verts[softVerticesAutoColliderVertex1] - verts[softVerticesAutoColliderVertex2]).magnitude * softVerticesAutoColliderRadiusMultiplier + softVerticesAutoColliderRadiusOffset;
		if (num < softVerticesAutoColliderMinRadius)
		{
			num = softVerticesAutoColliderMinRadius;
		}
		if (num > softVerticesAutoColliderMaxRadius)
		{
			num = softVerticesAutoColliderMaxRadius;
		}
		int num2 = Mathf.RoundToInt(num * 1000f);
		int num3 = Mathf.RoundToInt(_softVerticesColliderRadius * 1000f);
		if (num2 == num3)
		{
			return;
		}
		_softVerticesColliderRadius = (float)num2 / 1000f;
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in softVerticesGroups)
		{
			if (softVerticesGroup.useParentSettings)
			{
				if (softVerticesGroup.useParentColliderSettings)
				{
					softVerticesGroup.colliderRadiusNoSync = _softVerticesColliderRadius;
					softVerticesGroup.colliderNormalOffsetNoSync = _softVerticesColliderRadius;
				}
				if (softVerticesGroup.useParentColliderSettingsForSecondCollider)
				{
					softVerticesGroup.secondColliderRadiusNoSync = _softVerticesColliderRadius;
					softVerticesGroup.secondColliderNormalOffsetNoSync = _softVerticesColliderRadius;
				}
				if (softVerticesGroup.colliderSyncDirty)
				{
					softVerticesGroup.SyncCollidersThreaded();
				}
			}
		}
	}

	public void SoftVerticesSetAutoRadiusFinishFast()
	{
		if (_softVerticesUseAutoColliderRadius && softVerticesColliderRadiusJSON != null)
		{
			softVerticesColliderRadiusJSON.valNoCallback = _softVerticesColliderRadius;
		}
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in softVerticesGroups)
		{
			if (softVerticesGroup.useParentSettings && softVerticesGroup.colliderSyncDirty)
			{
				softVerticesGroup.SyncCollidersThreadedFinish();
			}
		}
	}

	protected void SoftVerticesSetAutoRadius()
	{
		if (skin != null && softVerticesUseAutoColliderRadius && softVerticesAutoColliderVertex1 != -1 && softVerticesAutoColliderVertex2 != -1)
		{
			Vector3[] visibleMorphedUVVertices = skin.dazMesh.visibleMorphedUVVertices;
			float num = (visibleMorphedUVVertices[softVerticesAutoColliderVertex1] - visibleMorphedUVVertices[softVerticesAutoColliderVertex2]).magnitude * softVerticesAutoColliderRadiusMultiplier + softVerticesAutoColliderRadiusOffset;
			if (num < softVerticesAutoColliderMinRadius)
			{
				num = softVerticesAutoColliderMinRadius;
			}
			if (num > softVerticesAutoColliderMaxRadius)
			{
				num = softVerticesAutoColliderMaxRadius;
			}
			softVerticesColliderRadius = num;
		}
	}

	protected void SyncSoftVerticesColliderRadius(float f)
	{
		if (_softVerticesUseAutoColliderRadius)
		{
			return;
		}
		_softVerticesColliderRadius = f;
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in softVerticesGroups)
		{
			if (softVerticesGroup.useParentSettings)
			{
				if (softVerticesGroup.useParentColliderSettings)
				{
					softVerticesGroup.colliderRadiusNoSync = _softVerticesColliderRadius;
					softVerticesGroup.colliderNormalOffsetNoSync = _softVerticesColliderRadius;
				}
				if (softVerticesGroup.useParentColliderSettingsForSecondCollider)
				{
					softVerticesGroup.secondColliderRadiusNoSync = _softVerticesColliderRadius;
					softVerticesGroup.secondColliderNormalOffsetNoSync = _softVerticesColliderRadius;
				}
				if (softVerticesGroup.colliderSyncDirty)
				{
					softVerticesGroup.SyncColliders();
				}
			}
		}
	}

	protected void SyncSoftVerticesColliderAdditionalNormalOffset(float f)
	{
		_softVerticesColliderAdditionalNormalOffset = f;
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in softVerticesGroups)
		{
			if (softVerticesGroup.useParentSettings)
			{
				if (softVerticesGroup.useParentColliderSettings)
				{
					softVerticesGroup.colliderAdditionalNormalOffset = _softVerticesColliderAdditionalNormalOffset;
				}
				if (softVerticesGroup.useParentColliderSettingsForSecondCollider)
				{
					softVerticesGroup.secondColliderAdditionalNormalOffset = _softVerticesColliderAdditionalNormalOffset;
				}
			}
		}
	}

	public int AddHardVerticesGroup()
	{
		DAZPhysicsMeshHardVerticesGroup item = new DAZPhysicsMeshHardVerticesGroup();
		_hardVerticesGroups.Add(item);
		return _currentHardVerticesGroupIndex = _hardVerticesGroups.Count - 1;
	}

	public void RemoveHardVerticesGroup(int index)
	{
		DAZPhysicsMeshHardVerticesGroup dAZPhysicsMeshHardVerticesGroup = _hardVerticesGroups[index];
		for (int i = 0; i < dAZPhysicsMeshHardVerticesGroup.vertices.Length; i++)
		{
			int key = dAZPhysicsMeshHardVerticesGroup.vertices[i];
			_hardTargetVerticesDict.Remove(key);
		}
		_hardVerticesGroups.RemoveAt(index);
		if (_currentHardVerticesGroupIndex >= _hardVerticesGroups.Count)
		{
			_currentHardVerticesGroupIndex = _hardVerticesGroups.Count - 1;
		}
	}

	public void MoveHardVerticesGroup(int fromindex, int toindex)
	{
		if (toindex >= 0 && toindex < _hardVerticesGroups.Count)
		{
			DAZPhysicsMeshHardVerticesGroup item = _hardVerticesGroups[fromindex];
			_hardVerticesGroups.RemoveAt(fromindex);
			_hardVerticesGroups.Insert(toindex, item);
			if (_currentHardVerticesGroupIndex == fromindex)
			{
				_currentHardVerticesGroupIndex = toindex;
			}
		}
	}

	public int AddSoftVerticesGroup()
	{
		DAZPhysicsMeshSoftVerticesGroup dAZPhysicsMeshSoftVerticesGroup = new DAZPhysicsMeshSoftVerticesGroup();
		_softVerticesGroups.Add(dAZPhysicsMeshSoftVerticesGroup);
		dAZPhysicsMeshSoftVerticesGroup.parent = this;
		return _currentSoftVerticesGroupIndex = _softVerticesGroups.Count - 1;
	}

	public void RemoveSoftVerticesGroup(int index)
	{
		DAZPhysicsMeshSoftVerticesGroup dAZPhysicsMeshSoftVerticesGroup = _softVerticesGroups[index];
		for (int i = 0; i < dAZPhysicsMeshSoftVerticesGroup.softVerticesSets.Count; i++)
		{
			DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = dAZPhysicsMeshSoftVerticesGroup.softVerticesSets[i];
			if (dAZPhysicsMeshSoftVerticesSet.targetVertex != -1)
			{
				_softTargetVerticesDict.Remove(dAZPhysicsMeshSoftVerticesSet.targetVertex);
			}
			List<DAZPhysicsMeshSoftVerticesSet> value;
			if (dAZPhysicsMeshSoftVerticesSet.anchorVertex != -1 && _softAnchorVerticesDict.TryGetValue(dAZPhysicsMeshSoftVerticesSet.anchorVertex, out value))
			{
				RemoveSoftAnchor(value, dAZPhysicsMeshSoftVerticesSet);
			}
			for (int j = 0; j < dAZPhysicsMeshSoftVerticesSet.influenceVertices.Length; j++)
			{
				_softInfluenceVerticesDict.Remove(dAZPhysicsMeshSoftVerticesSet.influenceVertices[j]);
			}
		}
		_softVerticesGroups.RemoveAt(index);
		if (_currentSoftVerticesGroupIndex >= _softVerticesGroups.Count)
		{
			_currentSoftVerticesGroupIndex = _softVerticesGroups.Count - 1;
		}
	}

	public void RemoveSoftVerticesSet(int groupIndex, int setIndex)
	{
		DAZPhysicsMeshSoftVerticesGroup dAZPhysicsMeshSoftVerticesGroup = _softVerticesGroups[groupIndex];
		DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = dAZPhysicsMeshSoftVerticesGroup.softVerticesSets[setIndex];
		if (dAZPhysicsMeshSoftVerticesSet.targetVertex != -1)
		{
			_softTargetVerticesDict.Remove(dAZPhysicsMeshSoftVerticesSet.targetVertex);
		}
		List<DAZPhysicsMeshSoftVerticesSet> value;
		if (dAZPhysicsMeshSoftVerticesSet.anchorVertex != -1 && _softAnchorVerticesDict.TryGetValue(dAZPhysicsMeshSoftVerticesSet.anchorVertex, out value))
		{
			RemoveSoftAnchor(value, dAZPhysicsMeshSoftVerticesSet);
		}
		for (int i = 0; i < dAZPhysicsMeshSoftVerticesSet.influenceVertices.Length; i++)
		{
			_softInfluenceVerticesDict.Remove(dAZPhysicsMeshSoftVerticesSet.influenceVertices[i]);
		}
		if (_softSetToGroupDict.ContainsKey(dAZPhysicsMeshSoftVerticesSet))
		{
			_softSetToGroupDict.Remove(dAZPhysicsMeshSoftVerticesSet);
		}
		dAZPhysicsMeshSoftVerticesGroup.RemoveSet(setIndex);
	}

	public void MoveSoftVerticesGroup(int fromindex, int toindex)
	{
		if (toindex >= 0 && toindex < _softVerticesGroups.Count)
		{
			DAZPhysicsMeshSoftVerticesGroup item = _softVerticesGroups[fromindex];
			_softVerticesGroups.RemoveAt(fromindex);
			_softVerticesGroups.Insert(toindex, item);
			if (_currentSoftVerticesGroupIndex == fromindex)
			{
				_currentSoftVerticesGroupIndex = toindex;
			}
		}
	}

	public int AddColliderGroup()
	{
		DAZPhysicsMeshColliderGroup item = new DAZPhysicsMeshColliderGroup();
		_colliderGroups.Add(item);
		return _currentColliderGroupIndex = _colliderGroups.Count - 1;
	}

	public void RemoveColliderGroup(int index)
	{
		_colliderGroups.RemoveAt(index);
		if (_currentColliderGroupIndex >= _colliderGroups.Count)
		{
			_currentColliderGroupIndex = _colliderGroups.Count - 1;
		}
	}

	public void MoveColliderGroup(int fromindex, int toindex)
	{
		if (toindex >= 0 && toindex < _colliderGroups.Count)
		{
			DAZPhysicsMeshColliderGroup item = _colliderGroups[fromindex];
			_colliderGroups.RemoveAt(fromindex);
			_colliderGroups.Insert(toindex, item);
			if (_currentColliderGroupIndex == fromindex)
			{
				_currentColliderGroupIndex = toindex;
			}
		}
	}

	public DAZPhysicsMeshSoftVerticesSet GetSoftSetByID(string uid)
	{
		if (_softVerticesGroups != null)
		{
			foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
			{
				DAZPhysicsMeshSoftVerticesSet setByID = softVerticesGroup.GetSetByID(uid, true);
				if (setByID != null)
				{
					return setByID;
				}
			}
		}
		return null;
	}

	protected void InitCaches(bool force = false)
	{
		if (_hardVerticesGroups == null)
		{
			_hardVerticesGroups = new List<DAZPhysicsMeshHardVerticesGroup>();
		}
		if (_softVerticesGroups == null)
		{
			_softVerticesGroups = new List<DAZPhysicsMeshSoftVerticesGroup>();
		}
		if (_colliderGroups == null)
		{
			_colliderGroups = new List<DAZPhysicsMeshColliderGroup>();
		}
		if (_hardTargetVerticesDict == null || force)
		{
			_hardTargetVerticesDict = new Dictionary<int, DAZPhysicsMeshHardVerticesGroup>();
			if (_hardVerticesGroups != null)
			{
				foreach (DAZPhysicsMeshHardVerticesGroup hardVerticesGroup in _hardVerticesGroups)
				{
					int[] vertices = hardVerticesGroup.vertices;
					foreach (int key in vertices)
					{
						_hardTargetVerticesDict.Add(key, hardVerticesGroup);
					}
				}
			}
		}
		if (skin != null && skin.dazMesh != null)
		{
			_uvVertToBaseVertDict = skin.dazMesh.uvVertToBaseVert;
		}
		else
		{
			_uvVertToBaseVertDict = new Dictionary<int, int>();
		}
		if (_softSetToGroupDict == null || force)
		{
			_softSetToGroupDict = new Dictionary<DAZPhysicsMeshSoftVerticesSet, DAZPhysicsMeshSoftVerticesGroup>();
			foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
			{
				foreach (DAZPhysicsMeshSoftVerticesSet softVerticesSet in softVerticesGroup.softVerticesSets)
				{
					_softSetToGroupDict.Add(softVerticesSet, softVerticesGroup);
				}
			}
		}
		if (_softTargetVerticesDict == null || force)
		{
			_softTargetVerticesDict = new Dictionary<int, DAZPhysicsMeshSoftVerticesSet>();
			if (_softVerticesGroups != null)
			{
				foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup2 in _softVerticesGroups)
				{
					softVerticesGroup2.parent = this;
					foreach (DAZPhysicsMeshSoftVerticesSet softVerticesSet2 in softVerticesGroup2.softVerticesSets)
					{
						if (softVerticesSet2.targetVertex != -1 && !_softTargetVerticesDict.ContainsKey(softVerticesSet2.targetVertex))
						{
							_softTargetVerticesDict.Add(softVerticesSet2.targetVertex, softVerticesSet2);
						}
					}
				}
			}
		}
		if (_softAnchorVerticesDict == null || force)
		{
			_softAnchorVerticesDict = new Dictionary<int, List<DAZPhysicsMeshSoftVerticesSet>>();
			if (_softVerticesGroups != null)
			{
				foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup3 in _softVerticesGroups)
				{
					foreach (DAZPhysicsMeshSoftVerticesSet softVerticesSet3 in softVerticesGroup3.softVerticesSets)
					{
						if (softVerticesSet3.anchorVertex != -1)
						{
							List<DAZPhysicsMeshSoftVerticesSet> value;
							if (_softAnchorVerticesDict.TryGetValue(softVerticesSet3.anchorVertex, out value))
							{
								value.Add(softVerticesSet3);
								continue;
							}
							value = new List<DAZPhysicsMeshSoftVerticesSet>();
							value.Add(softVerticesSet3);
							_softAnchorVerticesDict.Add(softVerticesSet3.anchorVertex, value);
						}
					}
				}
			}
		}
		if (_softInfluenceVerticesDict == null || force)
		{
			_softInfluenceVerticesDict = new Dictionary<int, DAZPhysicsMeshSoftVerticesSet>();
			if (_softVerticesGroups != null)
			{
				foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup4 in _softVerticesGroups)
				{
					foreach (DAZPhysicsMeshSoftVerticesSet softVerticesSet4 in softVerticesGroup4.softVerticesSets)
					{
						int[] influenceVertices = softVerticesSet4.influenceVertices;
						foreach (int key2 in influenceVertices)
						{
							if (!_softInfluenceVerticesDict.ContainsKey(key2))
							{
								_softInfluenceVerticesDict.Add(key2, softVerticesSet4);
							}
						}
					}
				}
			}
		}
		if (_softVerticesInGroupDict != null && !force)
		{
			return;
		}
		_softVerticesInGroupDict = new Dictionary<string, bool>();
		if (_softVerticesGroups == null)
		{
			return;
		}
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup5 in _softVerticesGroups)
		{
			foreach (DAZPhysicsMeshSoftVerticesSet softVerticesSet5 in softVerticesGroup5.softVerticesSets)
			{
				if (softVerticesSet5.targetVertex != -1)
				{
					string key3 = softVerticesGroup5.name + ":" + softVerticesSet5.targetVertex;
					if (!_softVerticesInGroupDict.ContainsKey(key3))
					{
						_softVerticesInGroupDict.Add(key3, true);
					}
				}
				if (softVerticesSet5.anchorVertex != -1)
				{
					string key4 = softVerticesGroup5.name + ":" + softVerticesSet5.anchorVertex;
					if (!_softVerticesInGroupDict.ContainsKey(key4))
					{
						_softVerticesInGroupDict.Add(key4, true);
					}
				}
				int[] influenceVertices2 = softVerticesSet5.influenceVertices;
				for (int k = 0; k < influenceVertices2.Length; k++)
				{
					int num = influenceVertices2[k];
					string key5 = softVerticesGroup5.name + ":" + num;
					if (!_softVerticesInGroupDict.ContainsKey(key5))
					{
						_softVerticesInGroupDict.Add(key5, true);
					}
				}
			}
		}
	}

	public bool IsHardTargetVertex(int vid)
	{
		return _hardTargetVerticesDict.ContainsKey(vid);
	}

	public DAZPhysicsMeshHardVerticesGroup GetHardVertexGroup(int vid)
	{
		DAZPhysicsMeshHardVerticesGroup value;
		if (_hardTargetVerticesDict.TryGetValue(vid, out value))
		{
			return value;
		}
		return null;
	}

	public bool IsSoftTargetVertex(int vid)
	{
		return _softTargetVerticesDict.ContainsKey(vid);
	}

	public float GetSoftTargetVertexSpringMultipler(int vid)
	{
		DAZPhysicsMeshSoftVerticesSet value;
		if (_softTargetVerticesDict.TryGetValue(vid, out value))
		{
			return value.springMultiplier;
		}
		return 0f;
	}

	public float GetSoftTargetVertexSizeMultipler(int vid)
	{
		DAZPhysicsMeshSoftVerticesSet value;
		if (_softTargetVerticesDict.TryGetValue(vid, out value))
		{
			return value.sizeMultiplier;
		}
		return 0f;
	}

	public float GetSoftTargetVertexLimitMultipler(int vid)
	{
		DAZPhysicsMeshSoftVerticesSet value;
		if (_softTargetVerticesDict.TryGetValue(vid, out value))
		{
			return value.limitMultiplier;
		}
		return 0f;
	}

	public bool IsSoftAnchorVertex(int vid)
	{
		return _softAnchorVerticesDict.ContainsKey(vid);
	}

	public bool IsSoftInfluenceVertex(int vid)
	{
		bool result = false;
		List<DAZPhysicsMeshSoftVerticesSet> value;
		if (_softInfluenceVerticesDict.ContainsKey(vid))
		{
			result = true;
		}
		else if (_softAnchorVerticesDict.TryGetValue(vid, out value))
		{
			foreach (DAZPhysicsMeshSoftVerticesSet item in value)
			{
				if (item.autoInfluenceAnchor)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	public bool IsVertexInCurrentSoftSet(int vid)
	{
		DAZPhysicsMeshSoftVerticesGroup dAZPhysicsMeshSoftVerticesGroup = currentSoftVerticesGroup;
		if (dAZPhysicsMeshSoftVerticesGroup != null)
		{
			DAZPhysicsMeshSoftVerticesSet currentSet = dAZPhysicsMeshSoftVerticesGroup.currentSet;
			DAZPhysicsMeshSoftVerticesSet value;
			List<DAZPhysicsMeshSoftVerticesSet> value2;
			if (_softInfluenceVerticesDict.TryGetValue(vid, out value))
			{
				if (value == currentSet)
				{
					return true;
				}
			}
			else if (_softTargetVerticesDict.TryGetValue(vid, out value))
			{
				if (value == currentSet)
				{
					return true;
				}
			}
			else if (_softAnchorVerticesDict.TryGetValue(vid, out value2) && value2.Contains(currentSet))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsVertexInCurrentSoftGroup(int vid)
	{
		DAZPhysicsMeshSoftVerticesGroup dAZPhysicsMeshSoftVerticesGroup = currentSoftVerticesGroup;
		if (dAZPhysicsMeshSoftVerticesGroup != null)
		{
			string key = dAZPhysicsMeshSoftVerticesGroup.name + ":" + vid;
			if (_softVerticesInGroupDict.ContainsKey(key))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsVertexCurrentSoftSetAnchor(int vid)
	{
		DAZPhysicsMeshSoftVerticesGroup dAZPhysicsMeshSoftVerticesGroup = currentSoftVerticesGroup;
		if (dAZPhysicsMeshSoftVerticesGroup != null)
		{
			DAZPhysicsMeshSoftVerticesSet currentSet = dAZPhysicsMeshSoftVerticesGroup.currentSet;
			List<DAZPhysicsMeshSoftVerticesSet> value;
			if (_softAnchorVerticesDict.TryGetValue(vid, out value) && value.Contains(currentSet))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsVertexInCurrentHardGroup(int vid)
	{
		DAZPhysicsMeshHardVerticesGroup dAZPhysicsMeshHardVerticesGroup = currentHardVerticesGroup;
		DAZPhysicsMeshHardVerticesGroup value;
		if (dAZPhysicsMeshHardVerticesGroup != null && _hardTargetVerticesDict.TryGetValue(vid, out value) && dAZPhysicsMeshHardVerticesGroup == value)
		{
			return true;
		}
		return false;
	}

	public void DrawLinkLines()
	{
		if (_softVerticesGroups == null || !(skin != null) || !(skin.dazMesh != null))
		{
			return;
		}
		Vector3[] array;
		Vector3[] array2;
		if (Application.isPlaying)
		{
			array = skin.drawVerts;
			array2 = skin.drawNormals;
		}
		else
		{
			array = skin.dazMesh.morphedUVVertices;
			array2 = skin.dazMesh.morphedUVNormals;
		}
		for (int i = 0; i < _softVerticesGroups.Count; i++)
		{
			DAZPhysicsMeshSoftVerticesGroup dAZPhysicsMeshSoftVerticesGroup = _softVerticesGroups[i];
			if (((_showCurrentSoftGroupOnly || _showCurrentSoftSetOnly) && i != _currentSoftVerticesGroupIndex) || !dAZPhysicsMeshSoftVerticesGroup.useLinkJoints)
			{
				continue;
			}
			foreach (DAZPhysicsMeshSoftVerticesSet softVerticesSet in dAZPhysicsMeshSoftVerticesGroup.softVerticesSets)
			{
				if ((_showCurrentSoftSetOnly && softVerticesSet != dAZPhysicsMeshSoftVerticesGroup.currentSet) || softVerticesSet.targetVertex == -1 || softVerticesSet.anchorVertex == -1)
				{
					continue;
				}
				Vector3 vector = array[softVerticesSet.targetVertex];
				if (softVerticesSet.links == null)
				{
					continue;
				}
				for (int j = 0; j < softVerticesSet.links.Count; j++)
				{
					DAZPhysicsMeshSoftVerticesSet softSetByID = GetSoftSetByID(softVerticesSet.links[j]);
					if (softSetByID != null && softSetByID.targetVertex != -1)
					{
						Debug.DrawLine(vector, array[softSetByID.targetVertex], Color.yellow);
						Debug.DrawLine((vector + 3f * array[softSetByID.targetVertex]) * 0.25f + array2[softVerticesSet.targetVertex] * 0.003f, array[softSetByID.targetVertex], Color.green);
						continue;
					}
					Debug.LogError("Soft vertices set " + softVerticesSet.uid + " has broken link to " + softVerticesSet.links[j] + " Removing.");
					softVerticesSet.links.RemoveAt(j);
					break;
				}
			}
		}
	}

	protected int FindOrCreateHardGroup(DAZBone db)
	{
		DAZPhysicsMeshHardVerticesGroup dAZPhysicsMeshHardVerticesGroup;
		for (int i = 0; i < _hardVerticesGroups.Count; i++)
		{
			dAZPhysicsMeshHardVerticesGroup = _hardVerticesGroups[i];
			if (dAZPhysicsMeshHardVerticesGroup.bone == db)
			{
				return i;
			}
		}
		int num = AddHardVerticesGroup();
		dAZPhysicsMeshHardVerticesGroup = _hardVerticesGroups[num];
		dAZPhysicsMeshHardVerticesGroup.bone = db;
		dAZPhysicsMeshHardVerticesGroup.name = db.name;
		return num;
	}

	public void ToggleHardVertices(int vid, bool auto = false)
	{
		if (auto)
		{
			DAZBone dAZBone = skin.strongestDAZBone[vid];
			if (dAZBone == null)
			{
				Debug.LogError("Could not find DAZBone for vertex " + vid);
				return;
			}
			_currentHardVerticesGroupIndex = FindOrCreateHardGroup(dAZBone);
		}
		DAZPhysicsMeshHardVerticesGroup dAZPhysicsMeshHardVerticesGroup = currentHardVerticesGroup;
		if (dAZPhysicsMeshHardVerticesGroup == null)
		{
			return;
		}
		DAZPhysicsMeshHardVerticesGroup value;
		if (_hardTargetVerticesDict.TryGetValue(vid, out value))
		{
			if (value != dAZPhysicsMeshHardVerticesGroup)
			{
				value.RemoveVertex(vid);
				_hardTargetVerticesDict.Remove(vid);
				dAZPhysicsMeshHardVerticesGroup.AddVertex(vid);
				_hardTargetVerticesDict.Add(vid, dAZPhysicsMeshHardVerticesGroup);
			}
			else
			{
				value.RemoveVertex(vid);
				_hardTargetVerticesDict.Remove(vid);
			}
		}
		else
		{
			dAZPhysicsMeshHardVerticesGroup.AddVertex(vid);
			_hardTargetVerticesDict.Add(vid, dAZPhysicsMeshHardVerticesGroup);
		}
	}

	public void OnHardVertices(int vid, bool auto = false)
	{
		if (auto)
		{
			DAZBone dAZBone = skin.strongestDAZBone[vid];
			if (dAZBone == null)
			{
				Debug.LogError("Could not find DAZBone for vertex " + vid);
				return;
			}
			_currentHardVerticesGroupIndex = FindOrCreateHardGroup(dAZBone);
		}
		DAZPhysicsMeshHardVerticesGroup dAZPhysicsMeshHardVerticesGroup = currentHardVerticesGroup;
		if (dAZPhysicsMeshHardVerticesGroup == null)
		{
			return;
		}
		DAZPhysicsMeshHardVerticesGroup value;
		if (_hardTargetVerticesDict.TryGetValue(vid, out value))
		{
			if (value != dAZPhysicsMeshHardVerticesGroup)
			{
				value.RemoveVertex(vid);
				_hardTargetVerticesDict.Remove(vid);
				dAZPhysicsMeshHardVerticesGroup.AddVertex(vid);
				_hardTargetVerticesDict.Add(vid, dAZPhysicsMeshHardVerticesGroup);
			}
		}
		else
		{
			dAZPhysicsMeshHardVerticesGroup.AddVertex(vid);
			_hardTargetVerticesDict.Add(vid, dAZPhysicsMeshHardVerticesGroup);
		}
	}

	public void OffHardVertices(int vid, bool auto = false)
	{
		DAZPhysicsMeshHardVerticesGroup dAZPhysicsMeshHardVerticesGroup = currentHardVerticesGroup;
		DAZPhysicsMeshHardVerticesGroup value;
		if (dAZPhysicsMeshHardVerticesGroup != null && _hardTargetVerticesDict.TryGetValue(vid, out value))
		{
			if (auto)
			{
				value.RemoveVertex(vid);
				_hardTargetVerticesDict.Remove(vid);
			}
			else if (value == dAZPhysicsMeshHardVerticesGroup)
			{
				value.RemoveVertex(vid);
				_hardTargetVerticesDict.Remove(vid);
			}
		}
	}

	public void ToggleSoftTargetVertex(int vid)
	{
		DAZPhysicsMeshSoftVerticesGroup dAZPhysicsMeshSoftVerticesGroup = currentSoftVerticesGroup;
		if (dAZPhysicsMeshSoftVerticesGroup == null)
		{
			return;
		}
		DAZPhysicsMeshSoftVerticesSet currentSet = dAZPhysicsMeshSoftVerticesGroup.currentSet;
		if (currentSet == null)
		{
			return;
		}
		DAZPhysicsMeshSoftVerticesSet value;
		if (_softTargetVerticesDict.TryGetValue(vid, out value))
		{
			if (value != currentSet)
			{
				value.targetVertex = -1;
				_softTargetVerticesDict.Remove(vid);
				if (currentSet.targetVertex != -1)
				{
					_softTargetVerticesDict.Remove(currentSet.targetVertex);
				}
				currentSet.targetVertex = vid;
				_softTargetVerticesDict.Add(vid, currentSet);
			}
			else
			{
				value.targetVertex = -1;
				_softTargetVerticesDict.Remove(vid);
			}
		}
		else
		{
			if (currentSet.targetVertex != -1)
			{
				_softTargetVerticesDict.Remove(currentSet.targetVertex);
			}
			currentSet.targetVertex = vid;
			_softTargetVerticesDict.Add(vid, currentSet);
		}
	}

	protected bool RemoveSoftAnchor(List<DAZPhysicsMeshSoftVerticesSet> ssl, DAZPhysicsMeshSoftVerticesSet ss)
	{
		if (ssl.Contains(ss))
		{
			ssl.Remove(ss);
			int anchorVertex = ss.anchorVertex;
			ss.anchorVertex = -1;
			if (ssl.Count == 0)
			{
				_softAnchorVerticesDict.Remove(anchorVertex);
			}
			return true;
		}
		return false;
	}

	protected void AddSoftAnchor(int vid, DAZPhysicsMeshSoftVerticesSet ss)
	{
		List<DAZPhysicsMeshSoftVerticesSet> value;
		if (ss.anchorVertex != -1 && _softAnchorVerticesDict.TryGetValue(ss.anchorVertex, out value))
		{
			RemoveSoftAnchor(value, ss);
		}
		ss.anchorVertex = vid;
		if (_softAnchorVerticesDict.TryGetValue(vid, out value))
		{
			if (!value.Contains(ss))
			{
				value.Add(ss);
			}
		}
		else
		{
			value = new List<DAZPhysicsMeshSoftVerticesSet>();
			value.Add(ss);
			_softAnchorVerticesDict.Add(vid, value);
		}
	}

	public void ToggleSoftAnchorVertex(int vid)
	{
		DAZPhysicsMeshSoftVerticesGroup dAZPhysicsMeshSoftVerticesGroup = currentSoftVerticesGroup;
		if (dAZPhysicsMeshSoftVerticesGroup == null)
		{
			return;
		}
		DAZPhysicsMeshSoftVerticesSet currentSet = dAZPhysicsMeshSoftVerticesGroup.currentSet;
		if (currentSet == null)
		{
			return;
		}
		List<DAZPhysicsMeshSoftVerticesSet> value;
		if (_softAnchorVerticesDict.TryGetValue(vid, out value))
		{
			if (!RemoveSoftAnchor(value, currentSet))
			{
				AddSoftAnchor(vid, currentSet);
			}
		}
		else
		{
			AddSoftAnchor(vid, currentSet);
		}
	}

	public void ToggleSoftInfluenceVertices(int vid)
	{
		DAZPhysicsMeshSoftVerticesGroup dAZPhysicsMeshSoftVerticesGroup = currentSoftVerticesGroup;
		if (dAZPhysicsMeshSoftVerticesGroup == null)
		{
			return;
		}
		DAZPhysicsMeshSoftVerticesSet currentSet = dAZPhysicsMeshSoftVerticesGroup.currentSet;
		if (currentSet == null)
		{
			return;
		}
		DAZPhysicsMeshSoftVerticesSet value;
		if (_softInfluenceVerticesDict.TryGetValue(vid, out value))
		{
			if (value != currentSet)
			{
				value.RemoveInfluenceVertex(vid);
				_softInfluenceVerticesDict.Remove(vid);
				currentSet.AddInfluenceVertex(vid);
				_softInfluenceVerticesDict.Add(vid, currentSet);
			}
			else
			{
				value.RemoveInfluenceVertex(vid);
				_softInfluenceVerticesDict.Remove(vid);
			}
		}
		else
		{
			currentSet.AddInfluenceVertex(vid);
			_softInfluenceVerticesDict.Add(vid, currentSet);
		}
	}

	public void OnSoftInfluenceVertices(int vid)
	{
		DAZPhysicsMeshSoftVerticesGroup dAZPhysicsMeshSoftVerticesGroup = currentSoftVerticesGroup;
		if (dAZPhysicsMeshSoftVerticesGroup == null || _softTargetVerticesDict.ContainsKey(vid))
		{
			return;
		}
		DAZPhysicsMeshSoftVerticesSet currentSet = dAZPhysicsMeshSoftVerticesGroup.currentSet;
		if (currentSet == null)
		{
			return;
		}
		DAZPhysicsMeshSoftVerticesSet value;
		if (_softInfluenceVerticesDict.TryGetValue(vid, out value))
		{
			if (value != currentSet)
			{
				value.RemoveInfluenceVertex(vid);
				_softInfluenceVerticesDict.Remove(vid);
				currentSet.AddInfluenceVertex(vid);
				_softInfluenceVerticesDict.Add(vid, currentSet);
			}
		}
		else
		{
			currentSet.AddInfluenceVertex(vid);
			_softInfluenceVerticesDict.Add(vid, currentSet);
		}
	}

	public void OffSoftInfluenceVertices(int vid)
	{
		DAZPhysicsMeshSoftVerticesGroup dAZPhysicsMeshSoftVerticesGroup = currentSoftVerticesGroup;
		if (dAZPhysicsMeshSoftVerticesGroup != null)
		{
			DAZPhysicsMeshSoftVerticesSet currentSet = dAZPhysicsMeshSoftVerticesGroup.currentSet;
			DAZPhysicsMeshSoftVerticesSet value;
			if (currentSet != null && _softInfluenceVerticesDict.TryGetValue(vid, out value) && value == currentSet)
			{
				value.RemoveInfluenceVertex(vid);
				_softInfluenceVerticesDict.Remove(vid);
			}
		}
	}

	public void SoftAutoRadius(int vid)
	{
		if (softVerticesUseAutoColliderRadius)
		{
			if (softVerticesAutoColliderVertex1 == vid)
			{
				softVerticesAutoColliderVertex1 = -1;
			}
			else if (softVerticesAutoColliderVertex2 == vid)
			{
				softVerticesAutoColliderVertex2 = -1;
			}
			else if (softVerticesAutoColliderVertex1 == -1)
			{
				softVerticesAutoColliderVertex1 = vid;
			}
			else if (softVerticesAutoColliderVertex2 == -1)
			{
				softVerticesAutoColliderVertex2 = vid;
			}
			SoftVerticesSetAutoRadius();
		}
	}

	public void SoftSelect(int vid)
	{
		Debug.Log("SoftSelect " + vid);
		DAZPhysicsMeshSoftVerticesSet value;
		DAZPhysicsMeshSoftVerticesGroup value2;
		if (_softTargetVerticesDict.TryGetValue(vid, out value) && _softSetToGroupDict.TryGetValue(value, out value2))
		{
			Debug.Log("Got set " + value.uid + " in group " + value2.name);
			currentSoftVerticesGroup = value2;
			currentSoftVerticesGroup.currentSet = value;
		}
	}

	public void SoftSpringSet(int vid)
	{
		DAZPhysicsMeshSoftVerticesSet value;
		if (_softTargetVerticesDict.TryGetValue(vid, out value))
		{
			value.springMultiplier = _softSpringMultiplierSetValue;
			if (currentSoftVerticesGroup != null)
			{
				currentSoftVerticesGroup.currentSet = value;
			}
		}
	}

	public void SoftSizeSet(int vid)
	{
		DAZPhysicsMeshSoftVerticesSet value;
		if (_softTargetVerticesDict.TryGetValue(vid, out value))
		{
			value.sizeMultiplier = _softSizeMultiplierSetValue;
			if (currentSoftVerticesGroup != null)
			{
				currentSoftVerticesGroup.currentSet = value;
			}
		}
	}

	public void SoftLimitSet(int vid)
	{
		DAZPhysicsMeshSoftVerticesSet value;
		if (_softTargetVerticesDict.TryGetValue(vid, out value))
		{
			value.limitMultiplier = _softLimitMultiplierSetValue;
			if (currentSoftVerticesGroup != null)
			{
				currentSoftVerticesGroup.currentSet = value;
			}
		}
	}

	public void AddSoftSet(DAZPhysicsMeshSoftVerticesGroup sg)
	{
		int currentSetIndex = sg.AddSet();
		sg.currentSetIndex = currentSetIndex;
		_softSetToGroupDict.Add(sg.currentSet, sg);
	}

	public void AutoSoftVertex(int vid)
	{
		DAZPhysicsMeshSoftVerticesGroup dAZPhysicsMeshSoftVerticesGroup = currentSoftVerticesGroup;
		if (dAZPhysicsMeshSoftVerticesGroup == null)
		{
			AddSoftVerticesGroup();
			dAZPhysicsMeshSoftVerticesGroup = currentSoftVerticesGroup;
		}
		DAZPhysicsMeshSoftVerticesSet currentSet = dAZPhysicsMeshSoftVerticesGroup.currentSet;
		if (currentSet == null)
		{
			dAZPhysicsMeshSoftVerticesGroup.AddSet();
			currentSet = dAZPhysicsMeshSoftVerticesGroup.currentSet;
			_softSetToGroupDict.Add(currentSet, dAZPhysicsMeshSoftVerticesGroup);
		}
		DAZPhysicsMeshSoftVerticesSet value;
		List<DAZPhysicsMeshSoftVerticesSet> value2;
		if (currentSet.targetVertex == -1)
		{
			if (_softTargetVerticesDict.TryGetValue(vid, out value))
			{
				value.targetVertex = -1;
				_softTargetVerticesDict.Remove(vid);
			}
			currentSet.targetVertex = vid;
			_softTargetVerticesDict.Add(vid, currentSet);
		}
		else if (currentSet.anchorVertex == -1)
		{
			AddSoftAnchor(vid, currentSet);
		}
		else if (_softAnchorVerticesDict.TryGetValue(vid, out value2) && value2.Contains(currentSet))
		{
			RemoveSoftAnchor(value2, currentSet);
		}
		else if (_softTargetVerticesDict.TryGetValue(vid, out value))
		{
			if (dAZPhysicsMeshSoftVerticesGroup.currentSet != value)
			{
				dAZPhysicsMeshSoftVerticesGroup.currentSet = value;
				return;
			}
			value.targetVertex = -1;
			_softTargetVerticesDict.Remove(vid);
		}
		else if (_softInfluenceVerticesDict.TryGetValue(vid, out value))
		{
			dAZPhysicsMeshSoftVerticesGroup.currentSet = value;
			value.RemoveInfluenceVertex(vid);
			_softInfluenceVerticesDict.Remove(vid);
		}
		else
		{
			currentSet.AddInfluenceVertex(vid);
			_softInfluenceVerticesDict.Add(vid, currentSet);
		}
	}

	public void StartSoftLink(int vid)
	{
		if (_softTargetVerticesDict.TryGetValue(vid, out startSoftLinkSet))
		{
			Debug.Log("Start Soft Link " + vid);
		}
	}

	public void EndSoftLink(int vid)
	{
		DAZPhysicsMeshSoftVerticesSet value;
		if (startSoftLinkSet != null && _softTargetVerticesDict.TryGetValue(vid, out value))
		{
			Debug.Log("End Soft Link " + vid);
			if (startSoftLinkSet.links.Remove(value.uid))
			{
				Debug.Log("Remove");
			}
			else if (startSoftLinkSet.uid != value.uid)
			{
				Debug.Log("Add " + startSoftLinkSet.uid);
				startSoftLinkSet.links.Add(value.uid);
			}
		}
	}

	public void ClearLinks(DAZPhysicsMeshSoftVerticesSet ss)
	{
		currentSoftVerticesGroup.ClearLinks(ss);
	}

	public void ClickVertex(int vid)
	{
		int value;
		if (_uvVertToBaseVertDict.TryGetValue(vid, out value))
		{
			vid = value;
		}
		switch (_selectionMode)
		{
		case SelectionMode.HardTarget:
			ToggleHardVertices(vid);
			break;
		case SelectionMode.HardTargetAuto:
			ToggleHardVertices(vid, true);
			break;
		case SelectionMode.SoftTarget:
			ToggleSoftTargetVertex(vid);
			break;
		case SelectionMode.SoftAnchor:
			ToggleSoftAnchorVertex(vid);
			break;
		case SelectionMode.SoftInfluenced:
			ToggleSoftInfluenceVertices(vid);
			break;
		case SelectionMode.SoftAuto:
			AutoSoftVertex(vid);
			break;
		case SelectionMode.SoftLink:
			StartSoftLink(vid);
			break;
		case SelectionMode.SoftSelect:
			SoftSelect(vid);
			break;
		case SelectionMode.SoftSpringSet:
			SoftSpringSet(vid);
			break;
		case SelectionMode.SoftSizeSet:
			SoftSizeSet(vid);
			break;
		case SelectionMode.SoftLimitSet:
			SoftLimitSet(vid);
			break;
		case SelectionMode.SoftAutoRadius:
			SoftAutoRadius(vid);
			break;
		case SelectionMode.ColliderEditEnd1:
		case SelectionMode.ColliderEditEnd2:
		case SelectionMode.ColliderEditFront:
			break;
		}
	}

	public void UpclickVertex(int vid)
	{
		int value;
		if (_uvVertToBaseVertDict.TryGetValue(vid, out value))
		{
			vid = value;
		}
		SelectionMode selectionMode = _selectionMode;
		if (selectionMode == SelectionMode.SoftLink)
		{
			EndSoftLink(vid);
		}
	}

	public void OnVertex(int vid)
	{
		int value;
		if (_uvVertToBaseVertDict.TryGetValue(vid, out value))
		{
			vid = value;
		}
		switch (_selectionMode)
		{
		case SelectionMode.HardTarget:
			OnHardVertices(vid);
			break;
		case SelectionMode.HardTargetAuto:
			OnHardVertices(vid, true);
			break;
		case SelectionMode.SoftTarget:
			break;
		case SelectionMode.SoftAnchor:
			break;
		case SelectionMode.SoftInfluenced:
		case SelectionMode.SoftAuto:
			OnSoftInfluenceVertices(vid);
			break;
		}
	}

	public void OffVertex(int vid)
	{
		int value;
		if (_uvVertToBaseVertDict.TryGetValue(vid, out value))
		{
			vid = value;
		}
		switch (_selectionMode)
		{
		case SelectionMode.HardTarget:
			OffHardVertices(vid);
			break;
		case SelectionMode.HardTargetAuto:
			OffHardVertices(vid, true);
			break;
		case SelectionMode.SoftTarget:
			break;
		case SelectionMode.SoftAnchor:
			break;
		case SelectionMode.SoftInfluenced:
		case SelectionMode.SoftAuto:
			OffSoftInfluenceVertices(vid);
			break;
		}
	}

	public int GetBaseVertex(int vid)
	{
		int value;
		if (_uvVertToBaseVertDict.TryGetValue(vid, out value))
		{
			vid = value;
		}
		return vid;
	}

	public bool IsBaseVertex(int vid)
	{
		if (_uvVertToBaseVertDict != null)
		{
			return !_uvVertToBaseVertDict.ContainsKey(vid);
		}
		return true;
	}

	protected void CreateHardVerticesColliders()
	{
		foreach (DAZPhysicsMeshHardVerticesGroup hardVerticesGroup in _hardVerticesGroups)
		{
			hardVerticesGroup.CreateColliders(base.transform, _skin);
		}
	}

	protected void UpdateHardVerticesColliders()
	{
		foreach (DAZPhysicsMeshHardVerticesGroup hardVerticesGroup in _hardVerticesGroups)
		{
			hardVerticesGroup.UpdateColliders();
		}
	}

	protected void InitSoftJoints()
	{
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			softVerticesGroup.Init(base.transform, skin);
		}
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup2 in _softVerticesGroups)
		{
			softVerticesGroup2.InitPass2();
		}
	}

	public void ResetSoftJoints()
	{
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			softVerticesGroup.ResetJoints();
		}
	}

	public override void ScaleChanged(float scale)
	{
		base.ScaleChanged(scale);
		float oneoversc = 1f / _scale;
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			softVerticesGroup.ScaleChanged(_scale, oneoversc);
		}
	}

	public void UpdateSoftJointsFast(bool predictOnly = false)
	{
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			softVerticesGroup.UpdateJointsFast(predictOnly);
		}
	}

	protected void UpdateSimulationSoftJoints()
	{
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			if (softVerticesGroup.useSimulation)
			{
				softVerticesGroup.UpdateJoints();
			}
		}
	}

	protected void UpdateNonSimulationSoftJoints()
	{
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			if (!softVerticesGroup.useSimulation)
			{
				softVerticesGroup.UpdateJoints();
			}
		}
	}

	public void PrepareSoftUpdateJointsThreaded()
	{
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			softVerticesGroup.PrepareUpdateJointsThreaded();
		}
	}

	public void UpdateSoftJointTargetsThreadedFast(Vector3[] verts, Vector3[] normals)
	{
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			softVerticesGroup.UpdateJointTargetsThreadedFast(verts, normals);
		}
	}

	protected void UpdateSoftJointTargetsThreaded()
	{
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			softVerticesGroup.UpdateJointTargetsThreaded();
		}
	}

	public void ApplySoftJointBackForces()
	{
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			softVerticesGroup.ResetAdjustJoints();
		}
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup2 in _softVerticesGroups)
		{
			softVerticesGroup2.ApplyBackForce();
		}
	}

	protected void MorphSoftVertices()
	{
		float num = Time.time - Time.fixedTime;
		float interpFactor = num / Time.fixedDeltaTime;
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			softVerticesGroup.MorphVertices(interpFactor);
		}
	}

	public void PrepareSoftMorphVerticesThreadedFast()
	{
		float num = Time.time - Time.fixedTime;
		float interpFactor = num / Time.fixedDeltaTime;
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			softVerticesGroup.PrepareMorphVerticesThreadedFast(interpFactor);
		}
	}

	protected void PrepareSoftMorphVerticesThreaded()
	{
		float num = Time.time - Time.fixedTime;
		float interpFactor = num / Time.fixedDeltaTime;
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			softVerticesGroup.PrepareMorphVerticesThreaded(interpFactor);
		}
	}

	public void MorphSoftVerticesThreadedFast(Vector3[] verts)
	{
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			softVerticesGroup.MorphVerticesThreadedFast(verts);
		}
	}

	protected void MorphSoftVerticesThreaded()
	{
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			softVerticesGroup.MorphVerticesThreaded();
		}
	}

	public void RecalculateLinkJointsFast(Vector3[] verts)
	{
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			softVerticesGroup.AdjustInitialTargetPositionsFast(verts);
		}
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup2 in _softVerticesGroups)
		{
			softVerticesGroup2.AdjustLinkJointDistancesFast();
		}
	}

	public void RecalculateLinkJointsFinishFast()
	{
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			softVerticesGroup.AdjustLinkJointDistancesFinishFast();
		}
	}

	protected void RecalculateLinkJoints()
	{
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			softVerticesGroup.AdjustInitialTargetPositions();
		}
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup2 in _softVerticesGroups)
		{
			softVerticesGroup2.AdjustLinkJointDistances();
		}
	}

	protected void SyncAllowSelfCollision(bool b)
	{
		_allowSelfCollision = b;
		if (Application.isPlaying)
		{
			InitColliders();
		}
	}

	public void InitColliders()
	{
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
		{
			softVerticesGroup.InitColliders();
		}
		if (_softVerticesGroups.Count > 1)
		{
			for (int i = 0; i < _softVerticesGroups.Count - 1; i++)
			{
				DAZPhysicsMeshSoftVerticesGroup dAZPhysicsMeshSoftVerticesGroup = _softVerticesGroups[i];
				for (int j = i + 1; j < _softVerticesGroups.Count; j++)
				{
					DAZPhysicsMeshSoftVerticesGroup otherGroup = _softVerticesGroups[j];
					dAZPhysicsMeshSoftVerticesGroup.IgnoreOtherSoftGroupColliders(otherGroup, !dAZPhysicsMeshSoftVerticesGroup.IsAllowedToCollideWithGroup(j) || !_allowSelfCollision);
				}
			}
		}
		DAZPhysicsMesh[] array = ignorePhysicsMeshes;
		foreach (DAZPhysicsMesh dAZPhysicsMesh in array)
		{
			foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup2 in _softVerticesGroups)
			{
				foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup3 in dAZPhysicsMesh.softVerticesGroups)
				{
					softVerticesGroup2.IgnoreOtherSoftGroupColliders(softVerticesGroup3);
				}
			}
		}
		foreach (DAZPhysicsMeshHardVerticesGroup hardVerticesGroup in _hardVerticesGroups)
		{
			hardVerticesGroup.InitColliders();
		}
	}

	public override void InitUI()
	{
		if (UITransform != null)
		{
			DAZPhysicsMeshUI componentInChildren = UITransform.GetComponentInChildren<DAZPhysicsMeshUI>(true);
			if (componentInChildren != null)
			{
				onJSON.toggle = componentInChildren.onToggle;
				allowSelfCollisionJSON.toggle = componentInChildren.allowSelfCollisionToggle;
				softVerticesCombinedSpringJSON.slider = componentInChildren.softVerticesCombinedSpringSlider;
				softVerticesCombinedDamperJSON.slider = componentInChildren.softVerticesCombinedDamperSlider;
				softVerticesMassJSON.slider = componentInChildren.softVerticesMassSlider;
				softVerticesBackForceJSON.slider = componentInChildren.softVerticesBackForceSlider;
				softVerticesBackForceThresholdDistanceJSON.slider = componentInChildren.softVerticesBackForceThresholdDistanceSlider;
				softVerticesBackForceMaxForceJSON.slider = componentInChildren.softVerticesBackForceMaxForceSlider;
				softVerticesUseAutoColliderRadiusJSON.toggle = componentInChildren.softVerticesUseAutoColliderRadiusToggle;
				softVerticesColliderRadiusJSON.slider = componentInChildren.softVerticesColliderRadiusSlider;
				softVerticesColliderAdditionalNormalOffsetJSON.slider = componentInChildren.softVerticesColliderAdditionalNormalOffsetSlider;
				softVerticesNormalLimitJSON.slider = componentInChildren.softVerticesNormalLimitSlider;
				if (groupASpringMultiplierJSON != null)
				{
					groupASpringMultiplierJSON.slider = componentInChildren.groupASpringMultplierSlider;
				}
				else if (componentInChildren.groupASpringMultplierSlider != null)
				{
					componentInChildren.groupASpringMultplierSlider.transform.parent.gameObject.SetActive(false);
				}
				if (groupADamperMultiplierJSON != null)
				{
					groupADamperMultiplierJSON.slider = componentInChildren.groupADamperMultplierSlider;
				}
				else if (componentInChildren.groupADamperMultplierSlider != null)
				{
					componentInChildren.groupADamperMultplierSlider.transform.parent.gameObject.SetActive(false);
				}
				if (groupBSpringMultiplierJSON != null)
				{
					groupBSpringMultiplierJSON.slider = componentInChildren.groupBSpringMultplierSlider;
				}
				else if (componentInChildren.groupBSpringMultplierSlider != null)
				{
					componentInChildren.groupBSpringMultplierSlider.transform.parent.gameObject.SetActive(false);
				}
				if (groupBDamperMultiplierJSON != null)
				{
					groupBDamperMultiplierJSON.slider = componentInChildren.groupBDamperMultplierSlider;
				}
				else if (componentInChildren.groupBDamperMultplierSlider != null)
				{
					componentInChildren.groupBDamperMultplierSlider.transform.parent.gameObject.SetActive(false);
				}
				if (groupCSpringMultiplierJSON != null)
				{
					groupCSpringMultiplierJSON.slider = componentInChildren.groupCSpringMultplierSlider;
				}
				else if (componentInChildren.groupCSpringMultplierSlider != null)
				{
					componentInChildren.groupCSpringMultplierSlider.transform.parent.gameObject.SetActive(false);
				}
				if (groupCDamperMultiplierJSON != null)
				{
					groupCDamperMultiplierJSON.slider = componentInChildren.groupCDamperMultplierSlider;
				}
				else if (componentInChildren.groupCDamperMultplierSlider != null)
				{
					componentInChildren.groupCDamperMultplierSlider.transform.parent.gameObject.SetActive(false);
				}
				if (groupDSpringMultiplierJSON != null)
				{
					groupDSpringMultiplierJSON.slider = componentInChildren.groupDSpringMultplierSlider;
				}
				else if (componentInChildren.groupDSpringMultplierSlider != null)
				{
					componentInChildren.groupDSpringMultplierSlider.transform.parent.gameObject.SetActive(false);
				}
				if (groupDDamperMultiplierJSON != null)
				{
					groupDDamperMultiplierJSON.slider = componentInChildren.groupDDamperMultplierSlider;
				}
				else if (componentInChildren.groupDDamperMultplierSlider != null)
				{
					componentInChildren.groupDDamperMultplierSlider.transform.parent.gameObject.SetActive(false);
				}
			}
		}
		if (!useCombinedSpringAndDamper)
		{
			if (softVerticesNormalSpringSlider != null)
			{
				softVerticesNormalSpringSlider.value = _softVerticesNormalSpring;
				softVerticesNormalSpringSlider.onValueChanged.AddListener(delegate
				{
					softVerticesNormalSpring = softVerticesNormalSpringSlider.value;
				});
				SliderControl component = softVerticesNormalSpringSlider.GetComponent<SliderControl>();
				if (component != null)
				{
					component.defaultValue = _softVerticesNormalSpring;
				}
				SyncSoftVerticesNormalSpring();
			}
			if (softVerticesNormalDamperSlider != null)
			{
				softVerticesNormalDamperSlider.value = _softVerticesNormalDamper;
				softVerticesNormalDamperSlider.onValueChanged.AddListener(delegate
				{
					softVerticesNormalDamper = softVerticesNormalDamperSlider.value;
				});
				SliderControl component2 = softVerticesNormalDamperSlider.GetComponent<SliderControl>();
				if (component2 != null)
				{
					component2.defaultValue = _softVerticesNormalDamper;
				}
				SyncSoftVerticesNormalDamper();
			}
			if (softVerticesTangentSpringSlider != null)
			{
				softVerticesTangentSpringSlider.value = _softVerticesTangentSpring;
				softVerticesTangentSpringSlider.onValueChanged.AddListener(delegate
				{
					softVerticesTangentSpring = softVerticesTangentSpringSlider.value;
				});
				SliderControl component3 = softVerticesTangentSpringSlider.GetComponent<SliderControl>();
				if (component3 != null)
				{
					component3.defaultValue = _softVerticesTangentSpring;
				}
				SyncSoftVerticesTangentSpring();
			}
			if (softVerticesTangentDamperSlider != null)
			{
				softVerticesTangentDamperSlider.value = _softVerticesTangentDamper;
				softVerticesTangentDamperSlider.onValueChanged.AddListener(delegate
				{
					softVerticesTangentDamper = softVerticesTangentDamperSlider.value;
				});
				SliderControl component4 = softVerticesTangentDamperSlider.GetComponent<SliderControl>();
				if (component4 != null)
				{
					component4.defaultValue = _softVerticesTangentDamper;
				}
				SyncSoftVerticesTangentDamper();
			}
		}
		if (softVerticesSpringMaxForceSlider != null)
		{
			softVerticesSpringMaxForceSlider.value = _softVerticesSpringMaxForce;
			softVerticesSpringMaxForceSlider.onValueChanged.AddListener(delegate
			{
				softVerticesSpringMaxForce = softVerticesSpringMaxForceSlider.value;
			});
			SliderControl component5 = softVerticesSpringMaxForceSlider.GetComponent<SliderControl>();
			if (component5 != null)
			{
				component5.defaultValue = _softVerticesSpringMaxForce;
			}
			SyncSoftVerticesSpringMaxForce();
		}
		if (softVerticesUseUniformLimitToggle != null)
		{
			softVerticesUseUniformLimitToggle.onValueChanged.AddListener(delegate
			{
				softVerticesUseUniformLimit = softVerticesUseUniformLimitToggle.isOn;
			});
			softVerticesUseUniformLimitToggle.isOn = _softVerticesUseUniformLimit;
			SyncSoftVerticesUseUniformLimit();
		}
		if (softVerticesNegativeNormalLimitSlider != null)
		{
			softVerticesNegativeNormalLimitSlider.value = _softVerticesNegativeNormalLimit;
			softVerticesNegativeNormalLimitSlider.onValueChanged.AddListener(delegate
			{
				softVerticesNegativeNormalLimit = softVerticesNegativeNormalLimitSlider.value;
			});
			SliderControl component6 = softVerticesNegativeNormalLimitSlider.GetComponent<SliderControl>();
			if (component6 != null)
			{
				component6.defaultValue = _softVerticesNegativeNormalLimit;
			}
			SyncSoftVerticesNegativeNormalLimit();
		}
		if (softVerticesTangentLimitSlider != null)
		{
			softVerticesTangentLimitSlider.value = _softVerticesTangentLimit;
			softVerticesTangentLimitSlider.onValueChanged.AddListener(delegate
			{
				softVerticesTangentLimit = softVerticesTangentLimitSlider.value;
			});
			SliderControl component7 = softVerticesTangentLimitSlider.GetComponent<SliderControl>();
			if (component7 != null)
			{
				component7.defaultValue = _softVerticesTangentLimit;
			}
			SyncSoftVerticesTangentLimit();
		}
	}

	public override void InitUIAlt()
	{
		if (!(UITransformAlt != null))
		{
			return;
		}
		DAZPhysicsMeshUI componentInChildren = UITransformAlt.GetComponentInChildren<DAZPhysicsMeshUI>(true);
		if (componentInChildren != null)
		{
			onJSON.toggleAlt = componentInChildren.onToggle;
			allowSelfCollisionJSON.toggleAlt = componentInChildren.allowSelfCollisionToggle;
			softVerticesCombinedSpringJSON.sliderAlt = componentInChildren.softVerticesCombinedSpringSlider;
			softVerticesCombinedDamperJSON.sliderAlt = componentInChildren.softVerticesCombinedDamperSlider;
			softVerticesMassJSON.sliderAlt = componentInChildren.softVerticesMassSlider;
			softVerticesBackForceJSON.sliderAlt = componentInChildren.softVerticesBackForceSlider;
			softVerticesBackForceThresholdDistanceJSON.sliderAlt = componentInChildren.softVerticesBackForceThresholdDistanceSlider;
			softVerticesBackForceMaxForceJSON.sliderAlt = componentInChildren.softVerticesBackForceMaxForceSlider;
			softVerticesUseAutoColliderRadiusJSON.toggleAlt = componentInChildren.softVerticesUseAutoColliderRadiusToggle;
			softVerticesColliderRadiusJSON.sliderAlt = componentInChildren.softVerticesColliderRadiusSlider;
			softVerticesColliderAdditionalNormalOffsetJSON.sliderAlt = componentInChildren.softVerticesColliderAdditionalNormalOffsetSlider;
			softVerticesNormalLimitJSON.sliderAlt = componentInChildren.softVerticesNormalLimitSlider;
			if (groupASpringMultiplierJSON != null)
			{
				groupASpringMultiplierJSON.sliderAlt = componentInChildren.groupASpringMultplierSlider;
			}
			else if (componentInChildren.groupASpringMultplierSlider != null)
			{
				componentInChildren.groupASpringMultplierSlider.transform.parent.gameObject.SetActive(false);
			}
			if (groupADamperMultiplierJSON != null)
			{
				groupADamperMultiplierJSON.sliderAlt = componentInChildren.groupADamperMultplierSlider;
			}
			else if (componentInChildren.groupADamperMultplierSlider != null)
			{
				componentInChildren.groupADamperMultplierSlider.transform.parent.gameObject.SetActive(false);
			}
			if (groupBSpringMultiplierJSON != null)
			{
				groupBSpringMultiplierJSON.sliderAlt = componentInChildren.groupBSpringMultplierSlider;
			}
			else if (componentInChildren.groupBSpringMultplierSlider != null)
			{
				componentInChildren.groupBSpringMultplierSlider.transform.parent.gameObject.SetActive(false);
			}
			if (groupBDamperMultiplierJSON != null)
			{
				groupBDamperMultiplierJSON.sliderAlt = componentInChildren.groupBDamperMultplierSlider;
			}
			else if (componentInChildren.groupBDamperMultplierSlider != null)
			{
				componentInChildren.groupBDamperMultplierSlider.transform.parent.gameObject.SetActive(false);
			}
			if (groupCSpringMultiplierJSON != null)
			{
				groupCSpringMultiplierJSON.sliderAlt = componentInChildren.groupCSpringMultplierSlider;
			}
			else if (componentInChildren.groupCSpringMultplierSlider != null)
			{
				componentInChildren.groupCSpringMultplierSlider.transform.parent.gameObject.SetActive(false);
			}
			if (groupCDamperMultiplierJSON != null)
			{
				groupCDamperMultiplierJSON.sliderAlt = componentInChildren.groupCDamperMultplierSlider;
			}
			else if (componentInChildren.groupCDamperMultplierSlider != null)
			{
				componentInChildren.groupCDamperMultplierSlider.transform.parent.gameObject.SetActive(false);
			}
			if (groupDSpringMultiplierJSON != null)
			{
				groupDSpringMultiplierJSON.sliderAlt = componentInChildren.groupDSpringMultplierSlider;
			}
			else if (componentInChildren.groupDSpringMultplierSlider != null)
			{
				componentInChildren.groupDSpringMultplierSlider.transform.parent.gameObject.SetActive(false);
			}
			if (groupDDamperMultiplierJSON != null)
			{
				groupDDamperMultiplierJSON.sliderAlt = componentInChildren.groupDDamperMultplierSlider;
			}
			else if (componentInChildren.groupDDamperMultplierSlider != null)
			{
				componentInChildren.groupDDamperMultplierSlider.transform.parent.gameObject.SetActive(false);
			}
		}
	}

	public void Init()
	{
		if (_wasInit || !(_skin != null))
		{
			return;
		}
		_wasInit = true;
		if (Application.isPlaying)
		{
			DAZPhysicsMeshEarlyUpdate component = GetComponent<DAZPhysicsMeshEarlyUpdate>();
			if (component == null)
			{
				component = base.gameObject.AddComponent<DAZPhysicsMeshEarlyUpdate>();
				component.dazPhysicsMesh = this;
			}
			_skin.Init();
			InitSoftJoints();
			CreateHardVerticesColliders();
			InitColliders();
			{
				foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in _softVerticesGroups)
				{
					softVerticesGroup.SyncCollisionEnabled();
				}
				return;
			}
		}
		if (_skin.dazMesh != null)
		{
			_skin.Init();
			_baseVertToUVVertFullMap = _skin.dazMesh.baseVertToUVVertFullMap;
		}
	}

	protected void InitJSONParams()
	{
		if (Application.isPlaying)
		{
			onJSON = new JSONStorableBool("on", _on, SyncOnCallback);
			SyncOn(false);
			RegisterBool(onJSON);
			allowSelfCollisionJSON = new JSONStorableBool("allowSelfCollision", _allowSelfCollision, SyncAllowSelfCollision);
			RegisterBool(allowSelfCollisionJSON);
			softVerticesCombinedSpringJSON = new JSONStorableFloat("softVerticesCombinedSpring", _softVerticesCombinedSpring, SyncSoftVerticesCombinedSpring, 0f, 500f);
			RegisterFloat(softVerticesCombinedSpringJSON);
			softVerticesCombinedDamperJSON = new JSONStorableFloat("softVerticesCombinedDamper", _softVerticesCombinedDamper, SyncSoftVerticesCombinedDamper, 0f, 10f);
			RegisterFloat(softVerticesCombinedDamperJSON);
			softVerticesMassJSON = new JSONStorableFloat("softVerticesMass", _softVerticesMass, SyncSoftVerticesMass, 0.05f, 0.5f);
			RegisterFloat(softVerticesMassJSON);
			softVerticesBackForceJSON = new JSONStorableFloat("softVerticesBackForce", _softVerticesBackForce, SyncSoftVerticesBackForce, 0f, 50f);
			RegisterFloat(softVerticesBackForceJSON);
			softVerticesBackForceThresholdDistanceJSON = new JSONStorableFloat("softVerticesBackForceThresholdDistance", _softVerticesBackForceThresholdDistance, SyncSoftVerticesBackForceThresholdDistance, 0f, 0.03f);
			RegisterFloat(softVerticesBackForceThresholdDistanceJSON);
			softVerticesBackForceMaxForceJSON = new JSONStorableFloat("softVerticesBackForceMaxForce", _softVerticesBackForceMaxForce, SyncSoftVerticesBackForceMaxForce, 0f, 50f);
			RegisterFloat(softVerticesBackForceMaxForceJSON);
			softVerticesUseAutoColliderRadiusJSON = new JSONStorableBool("softVerticesUseAutoColliderRadius", _softVerticesUseAutoColliderRadius, SyncSoftVerticesUseAutoColliderRadius);
			RegisterBool(softVerticesUseAutoColliderRadiusJSON);
			int num = Mathf.RoundToInt(_softVerticesColliderRadius * 1000f);
			_softVerticesColliderRadius = (float)num / 1000f;
			softVerticesColliderRadiusJSON = new JSONStorableFloat("softVerticesColliderRadius", _softVerticesColliderRadius, SyncSoftVerticesColliderRadius, 0f, 0.07f);
			RegisterFloat(softVerticesColliderRadiusJSON);
			softVerticesColliderAdditionalNormalOffsetJSON = new JSONStorableFloat("softVerticesColliderAdditionalNormalOffset", _softVerticesColliderAdditionalNormalOffset, SyncSoftVerticesColliderAdditionalNormalOffset, -0.01f, 0.01f);
			RegisterFloat(softVerticesColliderAdditionalNormalOffsetJSON);
			softVerticesNormalLimitJSON = new JSONStorableFloat("softVerticesDistanceLimit", _softVerticesNormalLimit, SyncSoftVerticesNormalLimit, 0f, 0.1f, false);
			RegisterFloat(softVerticesNormalLimitJSON);
			if (groupASlots != null && groupASlots.Length > 0)
			{
				float parentSettingSpringMultiplier = softVerticesGroups[groupASlots[0]].parentSettingSpringMultiplier;
				groupASpringMultiplierJSON = new JSONStorableFloat("groupASpringMultiplier", parentSettingSpringMultiplier, SyncGroupASpringMultiplier, 0f, 5f, false);
				RegisterFloat(groupASpringMultiplierJSON);
				float parentSettingDamperMultiplier = softVerticesGroups[groupASlots[0]].parentSettingDamperMultiplier;
				groupADamperMultiplierJSON = new JSONStorableFloat("groupADamperMultiplier", parentSettingDamperMultiplier, SyncGroupADamperMultiplier, 0f, 5f, false);
				RegisterFloat(groupADamperMultiplierJSON);
			}
			if (groupBSlots != null && groupBSlots.Length > 0)
			{
				float parentSettingSpringMultiplier2 = softVerticesGroups[groupBSlots[0]].parentSettingSpringMultiplier;
				groupBSpringMultiplierJSON = new JSONStorableFloat("groupBSpringMultiplier", parentSettingSpringMultiplier2, SyncGroupBSpringMultiplier, 0f, 5f, false);
				RegisterFloat(groupBSpringMultiplierJSON);
				float parentSettingDamperMultiplier2 = softVerticesGroups[groupBSlots[0]].parentSettingDamperMultiplier;
				groupBDamperMultiplierJSON = new JSONStorableFloat("groupBDamperMultiplier", parentSettingDamperMultiplier2, SyncGroupBDamperMultiplier, 0f, 5f, false);
				RegisterFloat(groupBDamperMultiplierJSON);
			}
			if (groupCSlots != null && groupCSlots.Length > 0)
			{
				float parentSettingSpringMultiplier3 = softVerticesGroups[groupCSlots[0]].parentSettingSpringMultiplier;
				groupCSpringMultiplierJSON = new JSONStorableFloat("groupCSpringMultiplier", parentSettingSpringMultiplier3, SyncGroupCSpringMultiplier, 0f, 5f, false);
				RegisterFloat(groupCSpringMultiplierJSON);
				float parentSettingDamperMultiplier3 = softVerticesGroups[groupCSlots[0]].parentSettingDamperMultiplier;
				groupCDamperMultiplierJSON = new JSONStorableFloat("groupCDamperMultiplier", parentSettingDamperMultiplier3, SyncGroupCDamperMultiplier, 0f, 5f, false);
				RegisterFloat(groupCDamperMultiplierJSON);
			}
			if (groupDSlots != null && groupDSlots.Length > 0)
			{
				float parentSettingSpringMultiplier4 = softVerticesGroups[groupDSlots[0]].parentSettingSpringMultiplier;
				groupDSpringMultiplierJSON = new JSONStorableFloat("groupDSpringMultiplier", parentSettingSpringMultiplier4, SyncGroupDSpringMultiplier, 0f, 5f, false);
				RegisterFloat(groupDSpringMultiplierJSON);
				float parentSettingDamperMultiplier4 = softVerticesGroups[groupDSlots[0]].parentSettingDamperMultiplier;
				groupDDamperMultiplierJSON = new JSONStorableFloat("groupDDamperMultiplier", parentSettingDamperMultiplier4, SyncGroupDDamperMultiplier, 0f, 5f, false);
				RegisterFloat(groupDDamperMultiplierJSON);
			}
		}
	}

	private void OnEnable()
	{
		Init();
		if (Application.isPlaying)
		{
			InitColliders();
		}
		else if (Application.isEditor)
		{
			InitCaches(true);
		}
		isEnabled = true;
	}

	private void OnDisable()
	{
		isEnabled = false;
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			InitJSONParams();
			InitUI();
			InitUIAlt();
		}
	}

	private void LateUpdate()
	{
		if (!_wasInit || !Application.isPlaying || !updateEnabled)
		{
			return;
		}
		foreach (DAZPhysicsMeshSoftVerticesGroup softVerticesGroup in softVerticesGroups)
		{
			softVerticesGroup.useThreading = useThreading;
		}
		if (useThreading)
		{
			StartThreads();
			while (physicsMeshTask.working)
			{
				Thread.Sleep(0);
			}
			PrepareSoftUpdateJointsThreaded();
			physicsMeshTask.taskType = DAZPhysicsMeshTaskType.UpdateSoftJointTargets;
			physicsMeshTask.working = true;
			physicsMeshTask.resetEvent.Set();
		}
	}

	private void FixedUpdate()
	{
		if (!_wasInit || !Application.isPlaying || !updateEnabled)
		{
			return;
		}
		if (_globalOn != globalEnable)
		{
			SyncOn();
		}
		UpdateHardVerticesColliders();
		if (useThreading)
		{
			if (physicsMeshTask != null)
			{
				while (physicsMeshTask.working)
				{
					Thread.Sleep(0);
				}
				UpdateSimulationSoftJoints();
			}
		}
		else
		{
			UpdateSimulationSoftJoints();
		}
		ApplySoftJointBackForces();
	}

	public void EarlyUpdate()
	{
		if (useThreading && updateEnabled)
		{
			StartThreads();
			while (physicsMeshTask.working)
			{
				Thread.Sleep(0);
			}
			UpdateNonSimulationSoftJoints();
			PrepareSoftMorphVerticesThreaded();
			physicsMeshTask.taskType = DAZPhysicsMeshTaskType.MorphVertices;
			physicsMeshTask.working = true;
			physicsMeshTask.resetEvent.Set();
		}
	}

	protected override void Update()
	{
		if (Application.isPlaying)
		{
			if (_globalOn != globalEnable)
			{
				SyncOn();
			}
			CheckResumeSimulation();
			if (!updateEnabled || !_wasInit)
			{
				return;
			}
			if (!useThreading)
			{
				UpdateNonSimulationSoftJoints();
			}
			if (skin != null && (skin.dazMesh.visibleNonPoseVerticesChangedThisFrame || skin.dazMesh.visibleNonPoseVerticesChangedLastFrame))
			{
				SoftVerticesSetAutoRadius();
				RecalculateLinkJoints();
			}
			if (useThreading)
			{
				while (physicsMeshTask.working)
				{
					Thread.Sleep(0);
				}
			}
			else
			{
				MorphSoftVertices();
			}
			return;
		}
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		if (meshFilter == null)
		{
			meshFilter = base.gameObject.AddComponent<MeshFilter>();
		}
		MeshRenderer component = GetComponent<MeshRenderer>();
		if (component == null)
		{
			component = base.gameObject.AddComponent<MeshRenderer>();
		}
		if (_editorMeshForFocus == null)
		{
			_editorMeshForFocus = new Mesh();
		}
		meshFilter.mesh = _editorMeshForFocus;
		if (skin != null && skin.dazMesh != null && skin.dazMesh.morphedUVVertices != null && skin.dazMesh.morphedUVVertices.Length > 0 && currentSoftVerticesGroup != null && currentSoftVerticesGroup.currentSet != null && currentSoftVerticesGroup.currentSet.targetVertex != -1)
		{
			Vector3 center = skin.dazMesh.morphedUVVertices[currentSoftVerticesGroup.currentSet.targetVertex];
			Vector3 size = default(Vector3);
			size.x = _handleSize * 50f;
			size.y = size.x;
			size.z = size.x;
			Bounds bounds = new Bounds(center, size);
			_editorMeshForFocus.bounds = bounds;
		}
	}

	protected void OnDestroy()
	{
		if (Application.isPlaying)
		{
			StopThreads();
			StopAllCoroutines();
		}
	}

	protected void OnApplicationQuit()
	{
		if (Application.isPlaying)
		{
			StopThreads();
			StopAllCoroutines();
		}
	}
}
