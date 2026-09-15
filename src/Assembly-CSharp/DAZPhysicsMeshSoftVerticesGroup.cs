using System;
using System.Collections.Generic;
using GPUTools.Physics.Scripts.Behaviours;
using MVR;
using UnityEngine;

[Serializable]
public class DAZPhysicsMeshSoftVerticesGroup
{
	public enum InfluenceType
	{
		Distance,
		DistanceAlongMoveVector,
		HardCopy
	}

	public enum LookAtOption
	{
		Anchor,
		NormalReference,
		VertexNormal,
		VertexNormalRefUp,
		VertexNormalAnchorUp
	}

	public enum ColliderOrient
	{
		Tangent2,
		Tangent,
		Normal
	}

	public enum ColliderType
	{
		Capsule,
		Sphere,
		Box
	}

	public enum MovementType
	{
		Free,
		Limit,
		Lock
	}

	public DAZPhysicsMesh parent;

	protected Vector3 zero = Vector3.zero;

	protected bool _on = true;

	protected bool _freeze;

	public bool enabled = true;

	protected float scale = 1f;

	protected float oneoverscale = 1f;

	protected bool triggerThreadedScaleChange;

	public bool useParentSettings = true;

	public bool useParentColliderSettings = true;

	public bool useParentColliderSettingsForSecondCollider;

	public float parentSettingSpringMultiplier = 1f;

	public float parentSettingDamperMultiplier = 1f;

	public int[] otherGroupNumsCollisionAllowed;

	protected HashSet<int> _otherGroupNumsCollisionAllowedHash;

	protected bool _collisionEnabled = true;

	public string name;

	public bool showSoftSets = true;

	[SerializeField]
	protected List<DAZPhysicsMeshSoftVerticesSet> _softVerticesSets;

	[SerializeField]
	private int _currentSetIndex;

	[SerializeField]
	private InfluenceType _influenceType;

	[SerializeField]
	private bool _autoInfluenceAnchor;

	public bool centerBetweenTargetAndAnchor;

	[SerializeField]
	private float _maxInfluenceDistance = 0.03f;

	[SerializeField]
	private LookAtOption _lookAtOption;

	[SerializeField]
	private float _falloffPower = 2f;

	[SerializeField]
	private float _weightMultiplier = 1f;

	[SerializeField]
	private float _jointSpringNormal = 10f;

	[SerializeField]
	private float _jointDamperNormal = 1f;

	[SerializeField]
	private float _jointSpringTangent = 10f;

	[SerializeField]
	private float _jointDamperTangent = 1f;

	[SerializeField]
	private float _jointSpringTangent2 = 10f;

	[SerializeField]
	private float _jointDamperTangent2 = 1f;

	[SerializeField]
	private float _jointSpringMaxForce = 10000000f;

	[SerializeField]
	private float _jointMass = 0.01f;

	[SerializeField]
	private ColliderOrient _colliderOrient;

	[SerializeField]
	private ColliderType _colliderType;

	protected bool _colliderSyncDirty;

	[SerializeField]
	private float _colliderRadius = 0.003f;

	[SerializeField]
	private float _colliderLength = 0.003f;

	[SerializeField]
	private float _colliderNormalOffset;

	[SerializeField]
	private float _colliderAdditionalNormalOffset;

	[SerializeField]
	private float _colliderTangentOffset;

	[SerializeField]
	private float _colliderTangent2Offset;

	[SerializeField]
	private string _colliderLayer;

	[SerializeField]
	private bool _useSecondCollider;

	[SerializeField]
	private bool _addGPUCollider;

	[SerializeField]
	private bool _addSecondGPUCollider;

	[SerializeField]
	private float _secondColliderRadius = 0.003f;

	[SerializeField]
	private float _secondColliderLength = 0.003f;

	[SerializeField]
	private float _secondColliderNormalOffset;

	[SerializeField]
	private float _secondColliderAdditionalNormalOffset;

	[SerializeField]
	private float _secondColliderTangentOffset;

	[SerializeField]
	private float _secondColliderTangent2Offset;

	[SerializeField]
	private Transform[] _ignoreColliders;

	public AutoColliderGroup[] ignoreAutoColliderGroups;

	[SerializeField]
	private PhysicMaterial _colliderMaterial;

	[SerializeField]
	private float _weightBias;

	[SerializeField]
	private bool _useUniformLimit;

	[SerializeField]
	private MovementType _normalMovementType;

	[SerializeField]
	private MovementType _tangentMovementType;

	[SerializeField]
	private MovementType _tangent2MovementType;

	protected Vector3 _startingNormalReferencePosition;

	[SerializeField]
	private Transform _normalReference;

	private Vector3 _normalReferencePosition;

	[SerializeField]
	private float _normalDistanceLimit = 0.015f;

	[SerializeField]
	private float _normalNegativeDistanceLimit = 0.015f;

	[SerializeField]
	private float _tangentDistanceLimit = 0.015f;

	[SerializeField]
	private float _tangentNegativeDistanceLimit = 0.015f;

	[SerializeField]
	private float _tangent2DistanceLimit = 0.015f;

	[SerializeField]
	private float _tangent2NegativeDistanceLimit = 0.015f;

	public bool useLinkJoints = true;

	public bool tieLinkJointSpringAndDamperToNormalSpringAndDamper;

	[SerializeField]
	private float _linkSpring = 1f;

	[SerializeField]
	private float _linkDamper = 0.1f;

	private bool _resetSimulation;

	[SerializeField]
	private bool _useSimulation;

	public bool useCustomInterpolation = true;

	public bool embedJoints;

	[SerializeField]
	private bool _clampVelocity;

	[SerializeField]
	private float _maxSimulationVelocity = 1f;

	private float _maxSimulationVelocitySqr = 1f;

	[SerializeField]
	private bool _useInterpolation;

	[SerializeField]
	private int _solverIterations = 15;

	public Rigidbody backForceRigidbody;

	public AdjustJoints backForceAdjustJoints;

	public bool backForceAdjustJointsUseJoint2;

	public float backForceAdjustJointsMaxAngle;

	public FreeControllerV3 controller;

	public bool useJointBackForce;

	[SerializeField]
	private float _jointBackForce;

	[SerializeField]
	private float _jointBackForceThresholdDistance;

	[SerializeField]
	private float _jointBackForceMaxForce;

	protected DAZSkinV2 _skin;

	private bool wasInit;

	private bool wasInit2;

	protected List<Collider> allPossibleIgnoreCollidersList;

	protected List<Collider> ignoreCollidersList;

	[SerializeField]
	protected bool _multiplyMassByLimitMultiplier = true;

	public Transform predictionTransform;

	public int numPredictionFrames = 2;

	protected Vector3 predictionTransformPosition1;

	protected Vector3 predictionTransformPosition2;

	protected Vector3 predictionTransformPosition3;

	protected Vector3 softVertexBackForce;

	protected Transform embedTransform;

	protected Rigidbody embedRB;

	protected const float linkAdjustDistanceThresholdSquared = 1E-06f;

	protected bool linkTargetsDirty;

	protected Vector3 _bufferedBackForce;

	protected Vector3 _appliedBackForce;

	public float backForceResponse = 1f;

	public bool useThreading;

	protected float lastSkinTime;

	protected float skinTime;

	protected float skinTimeDelta;

	public bool on
	{
		get
		{
			return _on;
		}
		set
		{
			if (_on != value)
			{
				_on = value;
				SyncOn();
			}
		}
	}

	public bool freeze
	{
		get
		{
			return _freeze;
		}
		set
		{
			if (_freeze != value)
			{
				_freeze = value;
				SyncOn();
			}
		}
	}

	public bool collisionEnabled
	{
		get
		{
			return _collisionEnabled;
		}
		set
		{
			if (_collisionEnabled != value)
			{
				_collisionEnabled = value;
				SyncCollisionEnabled();
			}
		}
	}

	public List<DAZPhysicsMeshSoftVerticesSet> softVerticesSets => _softVerticesSets;

	public int currentSetIndex
	{
		get
		{
			return _currentSetIndex;
		}
		set
		{
			if (_currentSetIndex != value && value >= 0 && value < _softVerticesSets.Count)
			{
				_currentSetIndex = value;
			}
		}
	}

	public DAZPhysicsMeshSoftVerticesSet currentSet
	{
		get
		{
			if (_currentSetIndex >= 0 && _currentSetIndex < _softVerticesSets.Count)
			{
				return _softVerticesSets[_currentSetIndex];
			}
			return null;
		}
		set
		{
			if (value == _softVerticesSets[_currentSetIndex])
			{
				return;
			}
			for (int i = 0; i < _softVerticesSets.Count; i++)
			{
				if (value == _softVerticesSets[i])
				{
					_currentSetIndex = i;
					break;
				}
			}
		}
	}

	public InfluenceType influenceType
	{
		get
		{
			return _influenceType;
		}
		set
		{
			if (_influenceType != value)
			{
				_influenceType = value;
			}
		}
	}

	public bool autoInfluenceAnchor
	{
		get
		{
			return _autoInfluenceAnchor;
		}
		set
		{
			if (_autoInfluenceAnchor != value)
			{
				_autoInfluenceAnchor = value;
				for (int i = 0; i < _softVerticesSets.Count; i++)
				{
					_softVerticesSets[i].autoInfluenceAnchor = _autoInfluenceAnchor;
				}
			}
		}
	}

	public float maxInfluenceDistance
	{
		get
		{
			return _maxInfluenceDistance;
		}
		set
		{
			if (_maxInfluenceDistance != value)
			{
				_maxInfluenceDistance = value;
			}
		}
	}

	public LookAtOption lookAtOption
	{
		get
		{
			return _lookAtOption;
		}
		set
		{
			if (_lookAtOption != value)
			{
				_lookAtOption = value;
				SyncJointParams();
			}
		}
	}

	public float falloffPower
	{
		get
		{
			return _falloffPower;
		}
		set
		{
			if (_falloffPower != value)
			{
				_falloffPower = value;
			}
		}
	}

	public float weightMultiplier
	{
		get
		{
			return _weightMultiplier;
		}
		set
		{
			if (_weightMultiplier != value)
			{
				_weightMultiplier = value;
			}
		}
	}

	public float jointSpringNormal
	{
		get
		{
			return _jointSpringNormal;
		}
		set
		{
			if (_jointSpringNormal != value)
			{
				_jointSpringNormal = value;
				SyncJointParams();
			}
		}
	}

	public float jointDamperNormal
	{
		get
		{
			return _jointDamperNormal;
		}
		set
		{
			if (_jointDamperNormal != value)
			{
				_jointDamperNormal = value;
				SyncJointParams();
			}
		}
	}

	public float jointSpringTangent
	{
		get
		{
			return _jointSpringTangent;
		}
		set
		{
			if (_jointSpringTangent != value)
			{
				_jointSpringTangent = value;
				SyncJointParams();
			}
		}
	}

	public float jointDamperTangent
	{
		get
		{
			return _jointDamperTangent;
		}
		set
		{
			if (_jointDamperTangent != value)
			{
				_jointDamperTangent = value;
				SyncJointParams();
			}
		}
	}

	public float jointSpringTangent2
	{
		get
		{
			return _jointSpringTangent2;
		}
		set
		{
			if (_jointSpringTangent2 != value)
			{
				_jointSpringTangent2 = value;
				SyncJointParams();
			}
		}
	}

	public float jointDamperTangent2
	{
		get
		{
			return _jointDamperTangent2;
		}
		set
		{
			if (_jointDamperTangent2 != value)
			{
				_jointDamperTangent2 = value;
				SyncJointParams();
			}
		}
	}

	public float jointSpringMaxForce
	{
		get
		{
			return _jointSpringMaxForce;
		}
		set
		{
			if (_jointSpringMaxForce != value)
			{
				_jointSpringMaxForce = value;
				SyncJointParams();
			}
		}
	}

	public float jointMass
	{
		get
		{
			return _jointMass;
		}
		set
		{
			if (_jointMass != value)
			{
				_jointMass = value;
				SyncJointParams();
			}
		}
	}

	public ColliderOrient colliderOrient
	{
		get
		{
			return _colliderOrient;
		}
		set
		{
			if (_colliderOrient != value)
			{
				_colliderOrient = value;
				SyncJointParams();
			}
		}
	}

	public ColliderType colliderType
	{
		get
		{
			return _colliderType;
		}
		set
		{
			if (_colliderType != value)
			{
				if (!wasInit)
				{
					_colliderType = value;
				}
				else
				{
					Debug.LogWarning("Cannot change collider type after initialization");
				}
			}
		}
	}

	public bool colliderSyncDirty => _colliderSyncDirty;

	public float colliderRadius
	{
		get
		{
			return _colliderRadius;
		}
		set
		{
			if (_colliderRadius != value)
			{
				_colliderRadius = value;
				SyncColliders();
			}
		}
	}

	public float colliderRadiusNoSync
	{
		set
		{
			if (_colliderRadius != value)
			{
				_colliderRadius = value;
				_colliderSyncDirty = true;
			}
		}
	}

	public float colliderLength
	{
		get
		{
			return _colliderLength;
		}
		set
		{
			if (_colliderLength != value)
			{
				_colliderLength = value;
				SyncColliders();
			}
		}
	}

	public float colliderNormalOffset
	{
		get
		{
			return _colliderNormalOffset;
		}
		set
		{
			if (_colliderNormalOffset != value)
			{
				_colliderNormalOffset = value;
				SyncColliders();
			}
		}
	}

	public float colliderNormalOffsetNoSync
	{
		set
		{
			if (_colliderNormalOffset != value)
			{
				_colliderNormalOffset = value;
				_colliderSyncDirty = true;
			}
		}
	}

	public float colliderAdditionalNormalOffset
	{
		get
		{
			return _colliderAdditionalNormalOffset;
		}
		set
		{
			if (_colliderAdditionalNormalOffset != value)
			{
				_colliderAdditionalNormalOffset = value;
				SyncColliders();
			}
		}
	}

	public float colliderTangentOffset
	{
		get
		{
			return _colliderTangentOffset;
		}
		set
		{
			if (_colliderTangentOffset != value)
			{
				_colliderTangentOffset = value;
				SyncColliders();
			}
		}
	}

	public float colliderTangent2Offset
	{
		get
		{
			return _colliderTangent2Offset;
		}
		set
		{
			if (_colliderTangent2Offset != value)
			{
				_colliderTangent2Offset = value;
				SyncColliders();
			}
		}
	}

	public string colliderLayer
	{
		get
		{
			return _colliderLayer;
		}
		set
		{
			if (_colliderLayer != value)
			{
				if (!wasInit)
				{
					_colliderLayer = value;
				}
				else
				{
					Debug.LogWarning("Cannot change collider layer after initialization");
				}
			}
		}
	}

	public bool useSecondCollider
	{
		get
		{
			return _useSecondCollider;
		}
		set
		{
			if (_useSecondCollider != value)
			{
				if (!wasInit)
				{
					_useSecondCollider = value;
				}
				else
				{
					Debug.LogWarning("Cannot change use second collider after initialization");
				}
			}
		}
	}

	public bool addGPUCollider
	{
		get
		{
			return _addGPUCollider;
		}
		set
		{
			if (_addGPUCollider != value)
			{
				if (!wasInit)
				{
					_addGPUCollider = value;
				}
				else
				{
					Debug.LogWarning("Cannot change add GPU collider after initialization");
				}
			}
		}
	}

	public bool addSecondGPUCollider
	{
		get
		{
			return _addSecondGPUCollider;
		}
		set
		{
			if (_addSecondGPUCollider != value)
			{
				if (!wasInit)
				{
					_addSecondGPUCollider = value;
				}
				else
				{
					Debug.LogWarning("Cannot change add second GPU collider after initialization");
				}
			}
		}
	}

	public float secondColliderRadius
	{
		get
		{
			return _secondColliderRadius;
		}
		set
		{
			if (_secondColliderRadius != value)
			{
				_secondColliderRadius = value;
				SyncColliders();
			}
		}
	}

	public float secondColliderRadiusNoSync
	{
		set
		{
			if (_secondColliderRadius != value)
			{
				_secondColliderRadius = value;
				_colliderSyncDirty = true;
			}
		}
	}

	public float secondColliderLength
	{
		get
		{
			return _secondColliderLength;
		}
		set
		{
			if (_secondColliderLength != value)
			{
				_secondColliderLength = value;
				SyncColliders();
			}
		}
	}

	public float secondColliderNormalOffset
	{
		get
		{
			return _secondColliderNormalOffset;
		}
		set
		{
			if (_secondColliderNormalOffset != value)
			{
				_secondColliderNormalOffset = value;
				SyncColliders();
			}
		}
	}

	public float secondColliderNormalOffsetNoSync
	{
		set
		{
			if (_secondColliderNormalOffset != value)
			{
				_secondColliderNormalOffset = value;
				_colliderSyncDirty = true;
			}
		}
	}

	public float secondColliderAdditionalNormalOffset
	{
		get
		{
			return _secondColliderAdditionalNormalOffset;
		}
		set
		{
			if (_secondColliderAdditionalNormalOffset != value)
			{
				_secondColliderAdditionalNormalOffset = value;
				SyncColliders();
			}
		}
	}

	public float secondColliderTangentOffset
	{
		get
		{
			return _secondColliderTangentOffset;
		}
		set
		{
			if (_secondColliderTangentOffset != value)
			{
				_secondColliderTangentOffset = value;
				SyncColliders();
			}
		}
	}

	public float secondColliderTangent2Offset
	{
		get
		{
			return _secondColliderTangent2Offset;
		}
		set
		{
			if (_secondColliderTangent2Offset != value)
			{
				_secondColliderTangent2Offset = value;
				SyncColliders();
			}
		}
	}

	public Transform[] ignoreColliders
	{
		get
		{
			return _ignoreColliders;
		}
		set
		{
			if (_ignoreColliders != value)
			{
				_ignoreColliders = value;
			}
		}
	}

	public PhysicMaterial colliderMaterial
	{
		get
		{
			return _colliderMaterial;
		}
		set
		{
			if (_colliderMaterial != value)
			{
				_colliderMaterial = value;
				SyncJointParams();
			}
		}
	}

	public float weightBias
	{
		get
		{
			return _weightBias;
		}
		set
		{
			if (_weightBias != value)
			{
				_weightBias = value;
			}
		}
	}

	public bool useUniformLimit
	{
		get
		{
			return _useUniformLimit;
		}
		set
		{
			if (_useUniformLimit != value)
			{
				_useUniformLimit = value;
				SyncJointParams();
			}
		}
	}

	public MovementType normalMovementType
	{
		get
		{
			return _normalMovementType;
		}
		set
		{
			if (_normalMovementType != value)
			{
				_normalMovementType = value;
				SyncJointParams();
			}
		}
	}

	public MovementType tangentMovementType
	{
		get
		{
			return _tangentMovementType;
		}
		set
		{
			if (_tangentMovementType != value)
			{
				_tangentMovementType = value;
				SyncJointParams();
			}
		}
	}

	public MovementType tangent2MovementType
	{
		get
		{
			return _tangent2MovementType;
		}
		set
		{
			if (_tangent2MovementType != value)
			{
				_tangent2MovementType = value;
				SyncJointParams();
			}
		}
	}

	public Transform normalReference
	{
		get
		{
			return _normalReference;
		}
		set
		{
			if (_normalReference != value)
			{
				_normalReference = value;
			}
		}
	}

	public float normalDistanceLimit
	{
		get
		{
			return _normalDistanceLimit;
		}
		set
		{
			if (_normalDistanceLimit != value)
			{
				_normalDistanceLimit = value;
				if (_useUniformLimit)
				{
					SyncJointParams();
				}
			}
		}
	}

	public float normalNegativeDistanceLimit
	{
		get
		{
			return _normalNegativeDistanceLimit;
		}
		set
		{
			if (_normalNegativeDistanceLimit != value)
			{
				_normalNegativeDistanceLimit = value;
			}
		}
	}

	public float tangentDistanceLimit
	{
		get
		{
			return _tangentDistanceLimit;
		}
		set
		{
			if (_tangentDistanceLimit != value)
			{
				_tangentDistanceLimit = value;
			}
		}
	}

	public float tangentNegativeDistanceLimit
	{
		get
		{
			return _tangentNegativeDistanceLimit;
		}
		set
		{
			if (_tangentNegativeDistanceLimit != value)
			{
				_tangentNegativeDistanceLimit = value;
			}
		}
	}

	public float tangent2DistanceLimit
	{
		get
		{
			return _tangent2DistanceLimit;
		}
		set
		{
			if (_tangent2DistanceLimit != value)
			{
				_tangent2DistanceLimit = value;
			}
		}
	}

	public float tangent2NegativeDistanceLimit
	{
		get
		{
			return _tangent2NegativeDistanceLimit;
		}
		set
		{
			if (_tangent2NegativeDistanceLimit != value)
			{
				_tangent2NegativeDistanceLimit = value;
			}
		}
	}

	public float linkSpring
	{
		get
		{
			return _linkSpring;
		}
		set
		{
			if (_linkSpring != value)
			{
				_linkSpring = value;
				SyncLinkParams();
			}
		}
	}

	public float linkDamper
	{
		get
		{
			return _linkDamper;
		}
		set
		{
			if (_linkDamper != value)
			{
				_linkDamper = value;
				SyncLinkParams();
			}
		}
	}

	public bool resetSimulation
	{
		get
		{
			return _resetSimulation;
		}
		set
		{
			if (_resetSimulation != value)
			{
				_resetSimulation = value;
				SyncCollisionEnabled();
			}
		}
	}

	public bool useSimulation
	{
		get
		{
			return _useSimulation;
		}
		set
		{
			if (_useSimulation != value)
			{
				if (!wasInit)
				{
					_useSimulation = value;
				}
				else
				{
					Debug.LogWarning("Cannot change use simulation after initialization");
				}
			}
		}
	}

	public bool clampVelocity
	{
		get
		{
			return _clampVelocity;
		}
		set
		{
			if (_clampVelocity != value)
			{
				_clampVelocity = value;
			}
		}
	}

	public float maxSimulationVelocity
	{
		get
		{
			return _maxSimulationVelocity;
		}
		set
		{
			if (_maxSimulationVelocity != value)
			{
				_maxSimulationVelocity = value;
				_maxSimulationVelocitySqr = value * value;
			}
		}
	}

	public bool useInterpolation
	{
		get
		{
			return _useInterpolation;
		}
		set
		{
			if (_useInterpolation != value)
			{
				_useInterpolation = value;
				SyncJointParams();
			}
		}
	}

	public int solverIterations
	{
		get
		{
			return _solverIterations;
		}
		set
		{
			if (_solverIterations != value)
			{
				_solverIterations = value;
				SyncJointParams();
			}
		}
	}

	public float jointBackForce
	{
		get
		{
			return _jointBackForce;
		}
		set
		{
			if (_jointBackForce != value)
			{
				_jointBackForce = value;
			}
		}
	}

	public float jointBackForceThresholdDistance
	{
		get
		{
			return _jointBackForceThresholdDistance;
		}
		set
		{
			if (_jointBackForceThresholdDistance != value)
			{
				_jointBackForceThresholdDistance = value;
			}
		}
	}

	public float jointBackForceMaxForce
	{
		get
		{
			return _jointBackForceMaxForce;
		}
		set
		{
			if (_jointBackForceMaxForce != value)
			{
				_jointBackForceMaxForce = value;
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
			if (_skin != value)
			{
				_skin = value;
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
			for (int i = 0; i < _softVerticesSets.Count; i++)
			{
				DAZPhysicsMeshSoftVerticesSet mass = _softVerticesSets[i];
				SetMass(mass);
			}
		}
	}

	public Vector3 bufferedBackForce => _bufferedBackForce;

	public DAZPhysicsMeshSoftVerticesGroup()
	{
		_softVerticesSets = new List<DAZPhysicsMeshSoftVerticesSet>();
	}

	protected void SyncOn()
	{
		if (_on && !_freeze)
		{
			ResetJoints();
			for (int i = 0; i < _softVerticesSets.Count; i++)
			{
				DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = _softVerticesSets[i];
				if (dAZPhysicsMeshSoftVerticesSet.jointTransform != null)
				{
					dAZPhysicsMeshSoftVerticesSet.jointTransform.gameObject.SetActive(true);
				}
			}
			return;
		}
		for (int j = 0; j < _softVerticesSets.Count; j++)
		{
			DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet2 = _softVerticesSets[j];
			if (dAZPhysicsMeshSoftVerticesSet2.jointTransform != null)
			{
				dAZPhysicsMeshSoftVerticesSet2.jointTransform.gameObject.SetActive(false);
			}
			if (!(_skin != null) || _freeze)
			{
				continue;
			}
			if (_skin.wasInit)
			{
				_skin.postSkinMorphs[dAZPhysicsMeshSoftVerticesSet2.targetVertex] = zero;
				for (int k = 0; k < dAZPhysicsMeshSoftVerticesSet2.influenceVertices.Length; k++)
				{
					int num = dAZPhysicsMeshSoftVerticesSet2.influenceVertices[k];
					_skin.postSkinMorphs[num] = zero;
				}
			}
			_skin.postSkinVertsChanged = true;
		}
	}

	public void ScaleChanged(float sc, float oneoversc)
	{
		scale = sc;
		oneoverscale = oneoversc;
		SyncJointParams();
		AdjustLinkJointDistances(true);
		triggerThreadedScaleChange = true;
	}

	public bool IsAllowedToCollideWithGroup(int groupNum)
	{
		if (_otherGroupNumsCollisionAllowedHash == null)
		{
			_otherGroupNumsCollisionAllowedHash = new HashSet<int>();
			int[] array = otherGroupNumsCollisionAllowed;
			foreach (int item in array)
			{
				_otherGroupNumsCollisionAllowedHash.Add(item);
			}
		}
		return _otherGroupNumsCollisionAllowedHash.Contains(groupNum);
	}

	public void SyncCollisionEnabled()
	{
		for (int i = 0; i < _softVerticesSets.Count; i++)
		{
			DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = _softVerticesSets[i];
			if (dAZPhysicsMeshSoftVerticesSet.jointRB != null)
			{
				dAZPhysicsMeshSoftVerticesSet.jointRB.detectCollisions = _collisionEnabled && !_resetSimulation;
			}
		}
	}

	public void SyncColliders()
	{
		if (wasInit)
		{
			_colliderSyncDirty = false;
			for (int i = 0; i < _softVerticesSets.Count; i++)
			{
				DAZPhysicsMeshSoftVerticesSet colliders = _softVerticesSets[i];
				SetColliders(colliders);
			}
		}
	}

	public void SyncCollidersThreaded()
	{
		if (wasInit)
		{
			for (int i = 0; i < _softVerticesSets.Count; i++)
			{
				DAZPhysicsMeshSoftVerticesSet collidersThreaded = _softVerticesSets[i];
				SetCollidersThreaded(collidersThreaded);
			}
		}
	}

	public void SyncCollidersThreadedFinish()
	{
		if (wasInit)
		{
			_colliderSyncDirty = false;
			for (int i = 0; i < _softVerticesSets.Count; i++)
			{
				DAZPhysicsMeshSoftVerticesSet collidersThreadedFinish = _softVerticesSets[i];
				SetCollidersThreadedFinish(collidersThreadedFinish);
			}
		}
	}

	public int AddSet()
	{
		DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = new DAZPhysicsMeshSoftVerticesSet();
		dAZPhysicsMeshSoftVerticesSet.autoInfluenceAnchor = _autoInfluenceAnchor;
		_softVerticesSets.Add(dAZPhysicsMeshSoftVerticesSet);
		return _softVerticesSets.Count - 1;
	}

	public void RemoveSet(int index)
	{
		DAZPhysicsMeshSoftVerticesSet ss = _softVerticesSets[index];
		ClearLinks(ss);
		_softVerticesSets.RemoveAt(index);
		if (_currentSetIndex >= _softVerticesSets.Count)
		{
			_currentSetIndex--;
		}
	}

	public void ClearLinks(DAZPhysicsMeshSoftVerticesSet ss)
	{
		bool flag = false;
		foreach (DAZPhysicsMeshSoftVerticesSet softVerticesSet in softVerticesSets)
		{
			if (ss == softVerticesSet)
			{
				flag = true;
			}
		}
		if (flag)
		{
			ss.links.Clear();
		}
	}

	public void MoveSet(int fromindex, int toindex)
	{
		if (toindex >= 0 && toindex < _softVerticesSets.Count)
		{
			DAZPhysicsMeshSoftVerticesSet item = _softVerticesSets[fromindex];
			_softVerticesSets.RemoveAt(fromindex);
			_softVerticesSets.Insert(toindex, item);
			if (_currentSetIndex == fromindex)
			{
				_currentSetIndex = toindex;
			}
		}
	}

	public DAZPhysicsMeshSoftVerticesSet GetSetByID(string uid, bool skipCheckParent = false)
	{
		for (int i = 0; i < _softVerticesSets.Count; i++)
		{
			if (_softVerticesSets[i].uid == uid)
			{
				return _softVerticesSets[i];
			}
		}
		if (parent != null && !skipCheckParent)
		{
			return parent.GetSoftSetByID(uid);
		}
		return null;
	}

	public void Init(Transform transform, DAZSkinV2 sk)
	{
		if (enabled && !wasInit && sk != null)
		{
			skin = sk;
			CreateJoints(transform);
			InitWeights();
			SyncOn();
			ResetJoints();
			wasInit = true;
		}
	}

	public void InitPass2()
	{
		if (enabled && !wasInit2)
		{
			CreateLinkJoints();
			wasInit2 = true;
		}
	}

	private void InitWeights()
	{
		if (_influenceType == InfluenceType.HardCopy)
		{
			return;
		}
		for (int i = 0; i < _softVerticesSets.Count; i++)
		{
			DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = _softVerticesSets[i];
			if (dAZPhysicsMeshSoftVerticesSet.influenceVertices.Length <= 0)
			{
				continue;
			}
			dAZPhysicsMeshSoftVerticesSet.influenceVerticesDistances = new float[dAZPhysicsMeshSoftVerticesSet.influenceVertices.Length];
			dAZPhysicsMeshSoftVerticesSet.influenceVerticesWeights = new float[dAZPhysicsMeshSoftVerticesSet.influenceVertices.Length];
			Vector3 vector = (embedJoints ? ((lookAtOption != 0) ? dAZPhysicsMeshSoftVerticesSet.jointTransform.up : dAZPhysicsMeshSoftVerticesSet.jointTransform.forward) : ((lookAtOption != 0) ? dAZPhysicsMeshSoftVerticesSet.kinematicTransform.up : dAZPhysicsMeshSoftVerticesSet.kinematicTransform.forward));
			for (int j = 0; j < dAZPhysicsMeshSoftVerticesSet.influenceVertices.Length; j++)
			{
				Vector3 rhs = _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.influenceVertices[j]] - _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex];
				if (_influenceType == InfluenceType.Distance)
				{
					dAZPhysicsMeshSoftVerticesSet.influenceVerticesDistances[j] = rhs.magnitude;
				}
				else if (_influenceType == InfluenceType.DistanceAlongMoveVector)
				{
					Vector3 vector2 = vector * Vector3.Dot(vector, rhs);
					dAZPhysicsMeshSoftVerticesSet.influenceVerticesDistances[j] = vector2.magnitude;
				}
			}
		}
	}

	public void SyncJointParams()
	{
		if (wasInit)
		{
			for (int i = 0; i < _softVerticesSets.Count; i++)
			{
				DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = _softVerticesSets[i];
				SetMass(dAZPhysicsMeshSoftVerticesSet);
				SetSolverIterations(dAZPhysicsMeshSoftVerticesSet);
				SetInterpolation(dAZPhysicsMeshSoftVerticesSet);
				SetJointLimits(dAZPhysicsMeshSoftVerticesSet.joint, dAZPhysicsMeshSoftVerticesSet);
				SetJointDrive(dAZPhysicsMeshSoftVerticesSet.joint, dAZPhysicsMeshSoftVerticesSet);
				SetColliders(dAZPhysicsMeshSoftVerticesSet);
			}
		}
	}

	private void SyncLinkParams()
	{
		if (!wasInit || !useLinkJoints)
		{
			return;
		}
		for (int i = 0; i < _softVerticesSets.Count; i++)
		{
			DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = _softVerticesSets[i];
			for (int j = 0; j < dAZPhysicsMeshSoftVerticesSet.linkJoints.Length; j++)
			{
				SpringJoint springJoint = dAZPhysicsMeshSoftVerticesSet.linkJoints[j];
				if (springJoint != null)
				{
					springJoint.spring = _linkSpring;
					springJoint.damper = _linkDamper;
				}
			}
		}
	}

	private void CreateLinkJoints()
	{
		if (!useLinkJoints)
		{
			return;
		}
		for (int i = 0; i < _softVerticesSets.Count; i++)
		{
			DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = _softVerticesSets[i];
			GameObject gameObject = dAZPhysicsMeshSoftVerticesSet.jointTransform.gameObject;
			if (dAZPhysicsMeshSoftVerticesSet.links == null)
			{
				continue;
			}
			dAZPhysicsMeshSoftVerticesSet.linkJoints = new SpringJoint[_softVerticesSets[i].links.Count];
			dAZPhysicsMeshSoftVerticesSet.linkJointDistances = new float[_softVerticesSets[i].links.Count];
			for (int j = 0; j < dAZPhysicsMeshSoftVerticesSet.links.Count; j++)
			{
				DAZPhysicsMeshSoftVerticesSet setByID = GetSetByID(dAZPhysicsMeshSoftVerticesSet.links[j]);
				if (setByID != null && setByID.jointRB != null)
				{
					SpringJoint springJoint = gameObject.AddComponent<SpringJoint>();
					dAZPhysicsMeshSoftVerticesSet.linkJoints[j] = springJoint;
					springJoint.spring = _linkSpring;
					springJoint.damper = _linkDamper;
					float magnitude = (dAZPhysicsMeshSoftVerticesSet.initialTargetPosition - setByID.initialTargetPosition).magnitude;
					dAZPhysicsMeshSoftVerticesSet.linkJointDistances[j] = magnitude;
					springJoint.minDistance = magnitude;
					springJoint.maxDistance = magnitude;
					springJoint.tolerance = 0.001f;
					springJoint.autoConfigureConnectedAnchor = false;
					springJoint.connectedBody = setByID.jointRB;
					springJoint.connectedAnchor = Vector3.zero;
				}
				else
				{
					dAZPhysicsMeshSoftVerticesSet.linkJoints[j] = null;
				}
			}
		}
	}

	private void GetCollidersRecursive(Transform rootTransform, Transform t, List<Collider> colliders)
	{
		if (t != rootTransform && (bool)t.GetComponent<Rigidbody>())
		{
			return;
		}
		Collider[] components = t.GetComponents<Collider>();
		foreach (Collider collider in components)
		{
			if (collider != null)
			{
				colliders.Add(collider);
			}
		}
		foreach (Transform item in t)
		{
			GetCollidersRecursive(rootTransform, item, colliders);
		}
	}

	public void InitColliders()
	{
		if (ignoreCollidersList == null)
		{
			ignoreCollidersList = new List<Collider>();
		}
		else
		{
			ignoreCollidersList.Clear();
		}
		if (allPossibleIgnoreCollidersList == null)
		{
			allPossibleIgnoreCollidersList = new List<Collider>();
			Transform[] array = _ignoreColliders;
			foreach (Transform transform in array)
			{
				GetCollidersRecursive(transform, transform, allPossibleIgnoreCollidersList);
			}
		}
		foreach (Collider allPossibleIgnoreColliders in allPossibleIgnoreCollidersList)
		{
			if (allPossibleIgnoreColliders != null && allPossibleIgnoreColliders.gameObject.activeInHierarchy && allPossibleIgnoreColliders.enabled)
			{
				ignoreCollidersList.Add(allPossibleIgnoreColliders);
			}
		}
		for (int j = 0; j < _softVerticesSets.Count; j++)
		{
			DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = _softVerticesSets[j];
			if (dAZPhysicsMeshSoftVerticesSet.jointCollider != null)
			{
				foreach (Collider ignoreColliders in ignoreCollidersList)
				{
					Physics.IgnoreCollision(dAZPhysicsMeshSoftVerticesSet.jointCollider, ignoreColliders);
					if (dAZPhysicsMeshSoftVerticesSet.jointCollider2 != null)
					{
						Physics.IgnoreCollision(dAZPhysicsMeshSoftVerticesSet.jointCollider2, ignoreColliders);
					}
				}
			}
			if (dAZPhysicsMeshSoftVerticesSet.jointCollider != null && dAZPhysicsMeshSoftVerticesSet.jointCollider2 != null)
			{
				Physics.IgnoreCollision(dAZPhysicsMeshSoftVerticesSet.jointCollider, dAZPhysicsMeshSoftVerticesSet.jointCollider2);
			}
		}
		AutoColliderGroup[] array2 = ignoreAutoColliderGroups;
		foreach (AutoColliderGroup autoColliderGroup in array2)
		{
			if (!(autoColliderGroup != null))
			{
				continue;
			}
			AutoCollider[] autoColliders = autoColliderGroup.GetAutoColliders();
			AutoCollider[] array3 = autoColliders;
			foreach (AutoCollider autoCollider in array3)
			{
				if (!(autoCollider.jointCollider != null))
				{
					continue;
				}
				for (int m = 0; m < _softVerticesSets.Count; m++)
				{
					DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet2 = _softVerticesSets[m];
					if (dAZPhysicsMeshSoftVerticesSet2.jointCollider != null)
					{
						Physics.IgnoreCollision(dAZPhysicsMeshSoftVerticesSet2.jointCollider, autoCollider.jointCollider);
						if (dAZPhysicsMeshSoftVerticesSet2.jointCollider2 != null)
						{
							Physics.IgnoreCollision(dAZPhysicsMeshSoftVerticesSet2.jointCollider2, autoCollider.jointCollider);
						}
					}
				}
			}
		}
		for (int n = 0; n < _softVerticesSets.Count - 1; n++)
		{
			DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet3 = _softVerticesSets[n];
			for (int num = n + 1; num < _softVerticesSets.Count; num++)
			{
				DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet4 = _softVerticesSets[num];
				if (!(dAZPhysicsMeshSoftVerticesSet3.jointCollider != null) || !(dAZPhysicsMeshSoftVerticesSet4.jointCollider != null))
				{
					continue;
				}
				Physics.IgnoreCollision(dAZPhysicsMeshSoftVerticesSet3.jointCollider, dAZPhysicsMeshSoftVerticesSet4.jointCollider);
				if (dAZPhysicsMeshSoftVerticesSet3.jointCollider2 != null)
				{
					Physics.IgnoreCollision(dAZPhysicsMeshSoftVerticesSet3.jointCollider2, dAZPhysicsMeshSoftVerticesSet4.jointCollider);
					if (dAZPhysicsMeshSoftVerticesSet4.jointCollider2 != null)
					{
						Physics.IgnoreCollision(dAZPhysicsMeshSoftVerticesSet3.jointCollider2, dAZPhysicsMeshSoftVerticesSet4.jointCollider2);
					}
				}
				if (dAZPhysicsMeshSoftVerticesSet4.jointCollider2 != null)
				{
					Physics.IgnoreCollision(dAZPhysicsMeshSoftVerticesSet3.jointCollider, dAZPhysicsMeshSoftVerticesSet4.jointCollider2);
				}
			}
		}
	}

	public void IgnoreOtherSoftGroupColliders(DAZPhysicsMeshSoftVerticesGroup otherGroup, bool ignore = true)
	{
		List<DAZPhysicsMeshSoftVerticesSet> list = otherGroup.softVerticesSets;
		for (int i = 0; i < list.Count; i++)
		{
			DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = list[i];
			for (int j = 0; j < _softVerticesSets.Count; j++)
			{
				DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet2 = _softVerticesSets[j];
				if (!(dAZPhysicsMeshSoftVerticesSet.jointCollider != null) || !(dAZPhysicsMeshSoftVerticesSet2.jointCollider != null))
				{
					continue;
				}
				Physics.IgnoreCollision(dAZPhysicsMeshSoftVerticesSet.jointCollider, dAZPhysicsMeshSoftVerticesSet2.jointCollider, ignore);
				if (dAZPhysicsMeshSoftVerticesSet.jointCollider2 != null)
				{
					Physics.IgnoreCollision(dAZPhysicsMeshSoftVerticesSet.jointCollider2, dAZPhysicsMeshSoftVerticesSet2.jointCollider, ignore);
					if (dAZPhysicsMeshSoftVerticesSet2.jointCollider2 != null)
					{
						Physics.IgnoreCollision(dAZPhysicsMeshSoftVerticesSet.jointCollider2, dAZPhysicsMeshSoftVerticesSet2.jointCollider2, ignore);
					}
				}
				if (dAZPhysicsMeshSoftVerticesSet2.jointCollider2 != null)
				{
					Physics.IgnoreCollision(dAZPhysicsMeshSoftVerticesSet.jointCollider, dAZPhysicsMeshSoftVerticesSet2.jointCollider2, ignore);
				}
			}
		}
	}

	private void SetInterpolation(DAZPhysicsMeshSoftVerticesSet ss)
	{
		if (_useInterpolation && _useSimulation && !useCustomInterpolation)
		{
			ss.jointRB.interpolation = RigidbodyInterpolation.Interpolate;
		}
		else
		{
			ss.jointRB.interpolation = RigidbodyInterpolation.None;
		}
	}

	private void SetJointLimits(ConfigurableJoint joint, DAZPhysicsMeshSoftVerticesSet ss, bool forceUniform = false)
	{
		if (_useUniformLimit || forceUniform)
		{
			if (lookAtOption == LookAtOption.Anchor)
			{
				if (_normalMovementType == MovementType.Lock)
				{
					joint.yMotion = ConfigurableJointMotion.Locked;
				}
				else if (_normalMovementType == MovementType.Free)
				{
					joint.yMotion = ConfigurableJointMotion.Free;
				}
				else
				{
					joint.yMotion = ConfigurableJointMotion.Limited;
				}
				if (_tangentMovementType == MovementType.Lock)
				{
					joint.zMotion = ConfigurableJointMotion.Locked;
				}
				else if (_tangentMovementType == MovementType.Free)
				{
					joint.zMotion = ConfigurableJointMotion.Free;
				}
				else
				{
					joint.zMotion = ConfigurableJointMotion.Limited;
				}
			}
			else
			{
				if (_normalMovementType == MovementType.Lock)
				{
					joint.zMotion = ConfigurableJointMotion.Locked;
				}
				else if (_normalMovementType == MovementType.Free)
				{
					joint.zMotion = ConfigurableJointMotion.Free;
				}
				else
				{
					joint.zMotion = ConfigurableJointMotion.Limited;
				}
				if (_tangentMovementType == MovementType.Lock)
				{
					joint.yMotion = ConfigurableJointMotion.Locked;
				}
				else if (_tangentMovementType == MovementType.Free)
				{
					joint.yMotion = ConfigurableJointMotion.Free;
				}
				else
				{
					joint.yMotion = ConfigurableJointMotion.Limited;
				}
			}
			if (_tangent2MovementType == MovementType.Lock)
			{
				joint.xMotion = ConfigurableJointMotion.Locked;
			}
			else if (_tangent2MovementType == MovementType.Free)
			{
				joint.xMotion = ConfigurableJointMotion.Free;
			}
			else
			{
				joint.xMotion = ConfigurableJointMotion.Limited;
			}
			SoftJointLimit linearLimit = joint.linearLimit;
			linearLimit.limit = _normalDistanceLimit * ss.limitMultiplier * scale;
			joint.linearLimit = linearLimit;
		}
		else
		{
			if (lookAtOption == LookAtOption.Anchor)
			{
				if (_normalMovementType == MovementType.Lock)
				{
					joint.yMotion = ConfigurableJointMotion.Locked;
				}
				else
				{
					joint.yMotion = ConfigurableJointMotion.Free;
				}
				if (_tangentMovementType == MovementType.Lock)
				{
					joint.zMotion = ConfigurableJointMotion.Locked;
				}
				else
				{
					joint.zMotion = ConfigurableJointMotion.Free;
				}
			}
			else
			{
				if (_normalMovementType == MovementType.Lock)
				{
					joint.zMotion = ConfigurableJointMotion.Locked;
				}
				else
				{
					joint.zMotion = ConfigurableJointMotion.Free;
				}
				if (_tangentMovementType == MovementType.Lock)
				{
					joint.yMotion = ConfigurableJointMotion.Locked;
				}
				else
				{
					joint.yMotion = ConfigurableJointMotion.Free;
				}
			}
			if (_tangent2MovementType == MovementType.Lock)
			{
				joint.xMotion = ConfigurableJointMotion.Locked;
			}
			else
			{
				joint.xMotion = ConfigurableJointMotion.Free;
			}
		}
		joint.angularXMotion = ConfigurableJointMotion.Locked;
		joint.angularYMotion = ConfigurableJointMotion.Locked;
		joint.angularZMotion = ConfigurableJointMotion.Locked;
	}

	private void SetJointDrive(ConfigurableJoint joint, DAZPhysicsMeshSoftVerticesSet ss)
	{
		JointDrive zDrive = default(JointDrive);
		zDrive.positionSpring = _jointSpringNormal * ss.springMultiplier;
		zDrive.positionDamper = _jointDamperNormal;
		zDrive.maximumForce = _jointSpringMaxForce;
		JointDrive xDrive = default(JointDrive);
		xDrive.positionSpring = _jointSpringTangent * ss.springMultiplier;
		xDrive.positionDamper = _jointDamperTangent;
		xDrive.maximumForce = _jointSpringMaxForce;
		JointDrive yDrive = default(JointDrive);
		yDrive.positionSpring = _jointSpringTangent2 * ss.springMultiplier;
		yDrive.positionDamper = _jointDamperTangent2;
		yDrive.maximumForce = _jointSpringMaxForce;
		joint.xDrive = xDrive;
		joint.yDrive = yDrive;
		joint.zDrive = zDrive;
	}

	private void SetCollidersThreaded(DAZPhysicsMeshSoftVerticesSet ss)
	{
		int direction = 0;
		Vector3 vector = default(Vector3);
		if (lookAtOption != 0)
		{
			switch (_colliderOrient)
			{
			case ColliderOrient.Normal:
				direction = 2;
				break;
			case ColliderOrient.Tangent:
				direction = 1;
				break;
			case ColliderOrient.Tangent2:
				direction = 0;
				break;
			}
			vector.x = _colliderTangent2Offset;
			vector.y = _colliderTangentOffset;
			vector.z = _colliderNormalOffset + _colliderAdditionalNormalOffset;
		}
		else
		{
			direction = (int)_colliderOrient;
			vector.x = _colliderTangent2Offset;
			vector.z = _colliderTangentOffset;
			vector.y = _colliderNormalOffset + _colliderAdditionalNormalOffset;
		}
		switch (colliderType)
		{
		case ColliderType.Capsule:
			ss.threadedColliderRadius = _colliderRadius * ss.sizeMultiplier;
			ss.threadedColliderHeight = _colliderLength * ss.sizeMultiplier;
			ss.threadedColliderCenter = vector * ss.sizeMultiplier;
			if (ss.capsuleLineSphereCollider != null)
			{
				ss.capsuleLineSphereCollider.UpdateData(ss.threadedColliderRadius, ss.threadedColliderHeight, direction, ss.threadedColliderCenter);
			}
			break;
		case ColliderType.Sphere:
			ss.threadedColliderRadius = _colliderRadius * ss.sizeMultiplier;
			ss.threadedColliderCenter = vector * ss.sizeMultiplier;
			break;
		case ColliderType.Box:
			ss.threadedColliderRadius = _colliderRadius * 2f * ss.sizeMultiplier;
			ss.threadedColliderCenter = vector * ss.sizeMultiplier;
			break;
		}
		if (!_useSecondCollider)
		{
			return;
		}
		Vector3 vector2 = default(Vector3);
		if (lookAtOption != 0)
		{
			vector2.x = _secondColliderTangent2Offset;
			vector2.y = _secondColliderTangentOffset;
			vector2.z = _secondColliderNormalOffset + _secondColliderAdditionalNormalOffset;
		}
		else
		{
			vector2.x = _secondColliderTangent2Offset;
			vector2.z = _secondColliderTangentOffset;
			vector2.y = _secondColliderNormalOffset + _secondColliderAdditionalNormalOffset;
		}
		switch (colliderType)
		{
		case ColliderType.Capsule:
			ss.threadedCollider2Radius = _secondColliderRadius * ss.sizeMultiplier;
			ss.threadedCollider2Height = _secondColliderLength * ss.sizeMultiplier;
			ss.threadedCollider2Center = vector2 * ss.sizeMultiplier;
			if (ss.capsuleLineSphereCollider2 != null)
			{
				ss.capsuleLineSphereCollider2.UpdateData(ss.threadedCollider2Radius, ss.threadedCollider2Height, direction, ss.threadedCollider2Center);
			}
			break;
		case ColliderType.Sphere:
			ss.threadedCollider2Radius = _secondColliderRadius * ss.sizeMultiplier;
			ss.threadedCollider2Center = vector2 * ss.sizeMultiplier;
			break;
		case ColliderType.Box:
			ss.threadedCollider2Radius = _secondColliderRadius * 2f * ss.sizeMultiplier;
			ss.threadedCollider2Center = vector2 * ss.sizeMultiplier;
			break;
		}
	}

	private void SetCollidersThreadedFinish(DAZPhysicsMeshSoftVerticesSet ss)
	{
		switch (colliderType)
		{
		case ColliderType.Capsule:
		{
			CapsuleCollider capsuleCollider = ss.jointCollider as CapsuleCollider;
			capsuleCollider.radius = ss.threadedColliderRadius;
			capsuleCollider.height = ss.threadedColliderHeight;
			capsuleCollider.center = ss.threadedColliderCenter;
			break;
		}
		case ColliderType.Sphere:
		{
			SphereCollider sphereCollider = ss.jointCollider as SphereCollider;
			sphereCollider.radius = ss.threadedColliderRadius;
			sphereCollider.center = ss.threadedColliderCenter;
			break;
		}
		case ColliderType.Box:
		{
			BoxCollider boxCollider = ss.jointCollider as BoxCollider;
			boxCollider.size = new Vector3(ss.threadedColliderRadius, ss.threadedColliderRadius, ss.threadedColliderRadius);
			boxCollider.center = ss.threadedColliderCenter;
			break;
		}
		}
		if (_useSecondCollider && ss.jointCollider2 != null)
		{
			switch (colliderType)
			{
			case ColliderType.Capsule:
			{
				CapsuleCollider capsuleCollider2 = ss.jointCollider2 as CapsuleCollider;
				capsuleCollider2.radius = ss.threadedCollider2Radius;
				capsuleCollider2.height = ss.threadedCollider2Height;
				capsuleCollider2.center = ss.threadedCollider2Center;
				break;
			}
			case ColliderType.Sphere:
			{
				SphereCollider sphereCollider2 = ss.jointCollider2 as SphereCollider;
				sphereCollider2.radius = ss.threadedCollider2Radius;
				sphereCollider2.center = ss.threadedCollider2Center;
				break;
			}
			case ColliderType.Box:
			{
				BoxCollider boxCollider2 = ss.jointCollider2 as BoxCollider;
				boxCollider2.size = new Vector3(ss.threadedCollider2Radius, ss.threadedCollider2Radius, ss.threadedCollider2Radius);
				boxCollider2.center = ss.threadedCollider2Center;
				break;
			}
			}
		}
	}

	private void SetColliders(DAZPhysicsMeshSoftVerticesSet ss)
	{
		int direction = 0;
		Vector3 vector = default(Vector3);
		if (lookAtOption != 0)
		{
			switch (_colliderOrient)
			{
			case ColliderOrient.Normal:
				direction = 2;
				break;
			case ColliderOrient.Tangent:
				direction = 1;
				break;
			case ColliderOrient.Tangent2:
				direction = 0;
				break;
			}
			vector.x = _colliderTangent2Offset;
			vector.y = _colliderTangentOffset;
			vector.z = _colliderNormalOffset + _colliderAdditionalNormalOffset;
		}
		else
		{
			direction = (int)_colliderOrient;
			vector.x = _colliderTangent2Offset;
			vector.z = _colliderTangentOffset;
			vector.y = _colliderNormalOffset + _colliderAdditionalNormalOffset;
		}
		switch (colliderType)
		{
		case ColliderType.Capsule:
		{
			CapsuleCollider capsuleCollider = ss.jointCollider as CapsuleCollider;
			capsuleCollider.radius = _colliderRadius * ss.sizeMultiplier;
			capsuleCollider.height = _colliderLength * ss.sizeMultiplier;
			capsuleCollider.direction = direction;
			capsuleCollider.center = vector * ss.sizeMultiplier;
			if (ss.capsuleLineSphereCollider != null)
			{
				ss.capsuleLineSphereCollider.UpdateData();
			}
			break;
		}
		case ColliderType.Sphere:
		{
			SphereCollider sphereCollider = ss.jointCollider as SphereCollider;
			sphereCollider.radius = _colliderRadius * ss.sizeMultiplier;
			sphereCollider.center = vector * ss.sizeMultiplier;
			break;
		}
		case ColliderType.Box:
		{
			BoxCollider boxCollider = ss.jointCollider as BoxCollider;
			float num = _colliderRadius * 2f * ss.sizeMultiplier;
			boxCollider.size = new Vector3(num, num, num);
			boxCollider.center = vector * ss.sizeMultiplier;
			break;
		}
		}
		if (_colliderMaterial != null)
		{
			ss.jointCollider.sharedMaterial = _colliderMaterial;
		}
		if (!_useSecondCollider || !(ss.jointCollider2 != null))
		{
			return;
		}
		Vector3 vector2 = default(Vector3);
		if (lookAtOption != 0)
		{
			vector2.x = _secondColliderTangent2Offset;
			vector2.y = _secondColliderTangentOffset;
			vector2.z = _secondColliderNormalOffset + _secondColliderAdditionalNormalOffset;
		}
		else
		{
			vector2.x = _secondColliderTangent2Offset;
			vector2.z = _secondColliderTangentOffset;
			vector2.y = _secondColliderNormalOffset + _secondColliderAdditionalNormalOffset;
		}
		switch (colliderType)
		{
		case ColliderType.Capsule:
		{
			CapsuleCollider capsuleCollider2 = ss.jointCollider2 as CapsuleCollider;
			capsuleCollider2.radius = _secondColliderRadius * ss.sizeMultiplier;
			capsuleCollider2.height = _secondColliderLength * ss.sizeMultiplier;
			capsuleCollider2.direction = direction;
			capsuleCollider2.center = vector2 * ss.sizeMultiplier;
			if (ss.capsuleLineSphereCollider2 != null)
			{
				ss.capsuleLineSphereCollider2.UpdateData();
			}
			break;
		}
		case ColliderType.Sphere:
		{
			SphereCollider sphereCollider2 = ss.jointCollider2 as SphereCollider;
			sphereCollider2.radius = _secondColliderRadius * ss.sizeMultiplier;
			sphereCollider2.center = vector2 * ss.sizeMultiplier;
			break;
		}
		case ColliderType.Box:
		{
			BoxCollider boxCollider2 = ss.jointCollider2 as BoxCollider;
			float num2 = _secondColliderRadius * 2f * ss.sizeMultiplier;
			boxCollider2.size = new Vector3(num2, num2, num2);
			boxCollider2.center = vector2 * ss.sizeMultiplier;
			break;
		}
		}
		ss.jointCollider2.sharedMaterial = _colliderMaterial;
	}

	private void SetMass(DAZPhysicsMeshSoftVerticesSet ss)
	{
		if (ss.jointRB != null)
		{
			if (multiplyMassByLimitMultiplier)
			{
				ss.jointRB.mass = _jointMass * ss.limitMultiplier;
			}
			else
			{
				ss.jointRB.mass = _jointMass;
			}
		}
	}

	private void SetSolverIterations(DAZPhysicsMeshSoftVerticesSet ss)
	{
		ss.jointRB.solverIterations = _solverIterations;
	}

	private void CreateJoints(Transform transform)
	{
		if (_normalReference != null)
		{
			_startingNormalReferencePosition = _normalReference.position;
			Transform transform2;
			if (name != null && name != string.Empty)
			{
				GameObject gameObject = new GameObject("PhysicsMesh" + name);
				transform2 = gameObject.transform;
				transform2.SetParent(transform);
				transform2.localScale = Vector3.one;
				transform2.position = transform.position;
				transform2.rotation = transform.rotation;
			}
			else
			{
				transform2 = transform;
			}
			Vector3 vector;
			if (_normalReference != null)
			{
				DAZBone component = _normalReference.GetComponent<DAZBone>();
				vector = ((!(component != null)) ? transform2.position : component.importWorldPosition);
			}
			else
			{
				vector = Vector3.zero;
			}
			if (embedJoints)
			{
				GameObject gameObject2 = new GameObject("PhysicsMeshKRB" + name);
				embedRB = gameObject2.AddComponent<Rigidbody>();
				embedRB.isKinematic = true;
				embedTransform = embedRB.transform;
				embedTransform.SetParent(transform2);
				embedTransform.localScale = Vector3.one;
				embedTransform.position = _normalReference.position;
				embedTransform.rotation = _normalReference.rotation;
			}
			else
			{
				embedRB = null;
				embedTransform = null;
			}
			bool flag = lookAtOption == LookAtOption.VertexNormal || lookAtOption == LookAtOption.VertexNormalRefUp || lookAtOption == LookAtOption.VertexNormalAnchorUp;
			for (int i = 0; i < _softVerticesSets.Count; i++)
			{
				DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = _softVerticesSets[i];
				Vector3[] visibleMorphedUVVertices = _skin.dazMesh.visibleMorphedUVVertices;
				Vector3[] morphedUVNormals = _skin.dazMesh.morphedUVNormals;
				Vector3 vector2 = (dAZPhysicsMeshSoftVerticesSet.initialTargetPosition = ((!centerBetweenTargetAndAnchor) ? visibleMorphedUVVertices[dAZPhysicsMeshSoftVerticesSet.targetVertex] : ((visibleMorphedUVVertices[dAZPhysicsMeshSoftVerticesSet.targetVertex] + visibleMorphedUVVertices[dAZPhysicsMeshSoftVerticesSet.anchorVertex]) * 0.5f)));
				if (!embedJoints)
				{
					GameObject gameObject3 = new GameObject("PhysicsMeshKRB" + name + i);
					dAZPhysicsMeshSoftVerticesSet.kinematicTransform = gameObject3.transform;
					dAZPhysicsMeshSoftVerticesSet.kinematicTransform.SetParent(transform2);
					dAZPhysicsMeshSoftVerticesSet.kinematicTransform.localScale = Vector3.one;
					dAZPhysicsMeshSoftVerticesSet.kinematicRB = gameObject3.AddComponent<Rigidbody>();
					dAZPhysicsMeshSoftVerticesSet.kinematicRB.isKinematic = true;
					dAZPhysicsMeshSoftVerticesSet.kinematicTransform.position = vector2;
				}
				_skin.postSkinVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex] = true;
				_skin.postSkinVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex] = true;
				if (flag)
				{
					_skin.postSkinNormalVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex] = true;
					if (centerBetweenTargetAndAnchor)
					{
						_skin.postSkinNormalVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex] = true;
					}
				}
				_skin.postSkinVertsChanged = true;
				Quaternion rotation = ((lookAtOption == LookAtOption.NormalReference || dAZPhysicsMeshSoftVerticesSet.forceLookAtReference) ? Quaternion.LookRotation(vector - vector2, visibleMorphedUVVertices[dAZPhysicsMeshSoftVerticesSet.anchorVertex] - visibleMorphedUVVertices[dAZPhysicsMeshSoftVerticesSet.targetVertex]) : ((lookAtOption == LookAtOption.VertexNormal) ? ((!centerBetweenTargetAndAnchor) ? Quaternion.LookRotation(-morphedUVNormals[dAZPhysicsMeshSoftVerticesSet.targetVertex]) : Quaternion.LookRotation((-morphedUVNormals[dAZPhysicsMeshSoftVerticesSet.targetVertex] - morphedUVNormals[dAZPhysicsMeshSoftVerticesSet.anchorVertex]) * 0.5f)) : ((lookAtOption == LookAtOption.VertexNormalRefUp) ? ((!centerBetweenTargetAndAnchor) ? Quaternion.LookRotation(-morphedUVNormals[dAZPhysicsMeshSoftVerticesSet.targetVertex], vector - vector2) : Quaternion.LookRotation((-morphedUVNormals[dAZPhysicsMeshSoftVerticesSet.targetVertex] - morphedUVNormals[dAZPhysicsMeshSoftVerticesSet.anchorVertex]) * 0.5f, vector - vector2)) : ((lookAtOption != LookAtOption.VertexNormalAnchorUp) ? Quaternion.LookRotation(visibleMorphedUVVertices[dAZPhysicsMeshSoftVerticesSet.anchorVertex] - visibleMorphedUVVertices[dAZPhysicsMeshSoftVerticesSet.targetVertex], vector - vector2) : ((!centerBetweenTargetAndAnchor) ? Quaternion.LookRotation(-morphedUVNormals[dAZPhysicsMeshSoftVerticesSet.targetVertex], visibleMorphedUVVertices[dAZPhysicsMeshSoftVerticesSet.anchorVertex] - visibleMorphedUVVertices[dAZPhysicsMeshSoftVerticesSet.targetVertex]) : Quaternion.LookRotation((-morphedUVNormals[dAZPhysicsMeshSoftVerticesSet.targetVertex] - morphedUVNormals[dAZPhysicsMeshSoftVerticesSet.anchorVertex]) * 0.5f, visibleMorphedUVVertices[dAZPhysicsMeshSoftVerticesSet.anchorVertex] - visibleMorphedUVVertices[dAZPhysicsMeshSoftVerticesSet.targetVertex]))))));
				if (!embedJoints)
				{
					dAZPhysicsMeshSoftVerticesSet.kinematicTransform.rotation = rotation;
				}
				if (_useSimulation && !_useUniformLimit && !embedJoints)
				{
					GameObject gameObject4 = new GameObject("JointTracker");
					dAZPhysicsMeshSoftVerticesSet.jointTrackerTransform = gameObject4.transform;
					dAZPhysicsMeshSoftVerticesSet.jointTrackerTransform.SetParent(dAZPhysicsMeshSoftVerticesSet.kinematicTransform);
					dAZPhysicsMeshSoftVerticesSet.jointTrackerTransform.localScale = Vector3.one;
					dAZPhysicsMeshSoftVerticesSet.jointTrackerTransform.localPosition = Vector3.zero;
					dAZPhysicsMeshSoftVerticesSet.jointTrackerTransform.localRotation = Quaternion.identity;
				}
				GameObject gameObject5 = new GameObject("PhysicsMeshJoint" + name + i);
				dAZPhysicsMeshSoftVerticesSet.jointTransform = gameObject5.transform;
				if (_useSimulation)
				{
					if (embedJoints)
					{
						dAZPhysicsMeshSoftVerticesSet.jointTransform.SetParent(transform2);
						dAZPhysicsMeshSoftVerticesSet.jointTransform.localScale = Vector3.one;
					}
					else
					{
						dAZPhysicsMeshSoftVerticesSet.jointTransform.SetParent(transform2);
						dAZPhysicsMeshSoftVerticesSet.jointTransform.localScale = Vector3.one;
					}
				}
				else if (embedJoints)
				{
					dAZPhysicsMeshSoftVerticesSet.jointTransform.SetParent(embedTransform);
					dAZPhysicsMeshSoftVerticesSet.jointTransform.localScale = Vector3.one;
				}
				else
				{
					dAZPhysicsMeshSoftVerticesSet.jointTransform.SetParent(dAZPhysicsMeshSoftVerticesSet.kinematicTransform);
					dAZPhysicsMeshSoftVerticesSet.jointTransform.localScale = Vector3.one;
				}
				dAZPhysicsMeshSoftVerticesSet.jointTransform.position = vector2;
				dAZPhysicsMeshSoftVerticesSet.jointTransform.rotation = rotation;
				dAZPhysicsMeshSoftVerticesSet.jointRB = gameObject5.AddComponent<Rigidbody>();
				dAZPhysicsMeshSoftVerticesSet.jointRB.useGravity = false;
				dAZPhysicsMeshSoftVerticesSet.jointRB.drag = 0f;
				dAZPhysicsMeshSoftVerticesSet.jointRB.angularDrag = 0f;
				dAZPhysicsMeshSoftVerticesSet.jointRB.maxAngularVelocity = SuperController.singleton.maxAngularVelocity;
				dAZPhysicsMeshSoftVerticesSet.jointRB.maxDepenetrationVelocity = SuperController.singleton.maxDepenetrationVelocity;
				dAZPhysicsMeshSoftVerticesSet.jointRB.collisionDetectionMode = CollisionDetectionMode.Discrete;
				dAZPhysicsMeshSoftVerticesSet.jointRB.isKinematic = false;
				dAZPhysicsMeshSoftVerticesSet.jointRB.detectCollisions = false;
				dAZPhysicsMeshSoftVerticesSet.joint = gameObject5.AddComponent<ConfigurableJoint>();
				if (embedJoints)
				{
					dAZPhysicsMeshSoftVerticesSet.joint.connectedBody = embedRB;
					dAZPhysicsMeshSoftVerticesSet.joint.autoConfigureConnectedAnchor = false;
					dAZPhysicsMeshSoftVerticesSet.joint.anchor = Vector3.zero;
					dAZPhysicsMeshSoftVerticesSet.joint.connectedAnchor = _normalReference.InverseTransformPoint(vector2);
				}
				else
				{
					dAZPhysicsMeshSoftVerticesSet.joint.connectedBody = dAZPhysicsMeshSoftVerticesSet.kinematicRB;
					dAZPhysicsMeshSoftVerticesSet.joint.autoConfigureConnectedAnchor = false;
					dAZPhysicsMeshSoftVerticesSet.joint.anchor = Vector3.zero;
					dAZPhysicsMeshSoftVerticesSet.joint.connectedAnchor = Vector3.zero;
				}
				SetJointLimits(dAZPhysicsMeshSoftVerticesSet.joint, dAZPhysicsMeshSoftVerticesSet);
				SetJointDrive(dAZPhysicsMeshSoftVerticesSet.joint, dAZPhysicsMeshSoftVerticesSet);
				SetInterpolation(dAZPhysicsMeshSoftVerticesSet);
				switch (colliderType)
				{
				case ColliderType.Capsule:
				{
					CapsuleCollider capsuleCollider = (CapsuleCollider)(dAZPhysicsMeshSoftVerticesSet.jointCollider = gameObject5.AddComponent<CapsuleCollider>());
					if (_addGPUCollider)
					{
						(dAZPhysicsMeshSoftVerticesSet.capsuleLineSphereCollider = gameObject5.AddComponent<CapsuleLineSphereCollider>()).capsuleCollider = capsuleCollider;
					}
					if (_useSecondCollider)
					{
						capsuleCollider = (CapsuleCollider)(dAZPhysicsMeshSoftVerticesSet.jointCollider2 = gameObject5.AddComponent<CapsuleCollider>());
						if (_addSecondGPUCollider)
						{
							(dAZPhysicsMeshSoftVerticesSet.capsuleLineSphereCollider2 = gameObject5.AddComponent<CapsuleLineSphereCollider>()).capsuleCollider = capsuleCollider;
						}
					}
					break;
				}
				case ColliderType.Sphere:
				{
					SphereCollider sphereCollider = (SphereCollider)(dAZPhysicsMeshSoftVerticesSet.jointCollider = gameObject5.AddComponent<SphereCollider>());
					if (_addGPUCollider)
					{
						GpuSphereCollider gpuSphereCollider = gameObject5.AddComponent<GpuSphereCollider>();
						gpuSphereCollider.sphereCollider = sphereCollider;
					}
					if (_useSecondCollider)
					{
						sphereCollider = (SphereCollider)(dAZPhysicsMeshSoftVerticesSet.jointCollider2 = gameObject5.AddComponent<SphereCollider>());
						if (_addSecondGPUCollider)
						{
							GpuSphereCollider gpuSphereCollider2 = gameObject5.AddComponent<GpuSphereCollider>();
							gpuSphereCollider2.sphereCollider = sphereCollider;
						}
					}
					break;
				}
				case ColliderType.Box:
				{
					BoxCollider boxCollider = gameObject5.AddComponent<BoxCollider>();
					if (_useSecondCollider)
					{
						boxCollider = gameObject5.AddComponent<BoxCollider>();
						dAZPhysicsMeshSoftVerticesSet.jointCollider2 = boxCollider;
					}
					break;
				}
				}
				if (_colliderLayer != null && _colliderLayer != string.Empty)
				{
					gameObject5.layer = LayerMask.NameToLayer(_colliderLayer);
				}
				SetColliders(dAZPhysicsMeshSoftVerticesSet);
				SetMass(dAZPhysicsMeshSoftVerticesSet);
				SetSolverIterations(dAZPhysicsMeshSoftVerticesSet);
				dAZPhysicsMeshSoftVerticesSet.jointRB.centerOfMass = Vector3.zero;
				if (i == 0 && controller != null)
				{
					ConfigurableJoint configurableJoint = gameObject5.AddComponent<ConfigurableJoint>();
					configurableJoint.rotationDriveMode = RotationDriveMode.Slerp;
					configurableJoint.autoConfigureConnectedAnchor = false;
					configurableJoint.connectedAnchor = Vector3.zero;
					Rigidbody component2 = controller.GetComponent<Rigidbody>();
					controller.transform.position = gameObject5.transform.position;
					controller.transform.rotation = gameObject5.transform.rotation;
					if (component2 != null)
					{
						configurableJoint.connectedBody = component2;
					}
					controller.followWhenOffRB = dAZPhysicsMeshSoftVerticesSet.jointRB;
				}
			}
			InitColliders();
		}
		else
		{
			Debug.LogError("Can't create joints without up reference set");
		}
	}

	public void AdjustInitialTargetPositionsFast(Vector3[] initVerts)
	{
		if (!wasInit)
		{
			return;
		}
		linkTargetsDirty = false;
		for (int i = 0; i < _softVerticesSets.Count; i++)
		{
			DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = _softVerticesSets[i];
			Vector3 vector = ((!centerBetweenTargetAndAnchor) ? initVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex] : ((initVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex] + initVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex]) * 0.5f));
			float sqrMagnitude = (vector - dAZPhysicsMeshSoftVerticesSet.initialTargetPosition).sqrMagnitude;
			if (sqrMagnitude >= 1E-06f)
			{
				dAZPhysicsMeshSoftVerticesSet.initialTargetPosition = vector;
				linkTargetsDirty = true;
				dAZPhysicsMeshSoftVerticesSet.linkTargetPositionDirty = true;
			}
			else
			{
				dAZPhysicsMeshSoftVerticesSet.linkTargetPositionDirty = false;
			}
		}
	}

	public void AdjustInitialTargetPositions()
	{
		if (wasInit)
		{
			Vector3[] visibleMorphedUVVertices = _skin.dazMesh.visibleMorphedUVVertices;
			for (int i = 0; i < _softVerticesSets.Count; i++)
			{
				DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = _softVerticesSets[i];
				Vector3 initialTargetPosition = ((!centerBetweenTargetAndAnchor) ? visibleMorphedUVVertices[dAZPhysicsMeshSoftVerticesSet.targetVertex] : ((visibleMorphedUVVertices[dAZPhysicsMeshSoftVerticesSet.targetVertex] + visibleMorphedUVVertices[dAZPhysicsMeshSoftVerticesSet.anchorVertex]) * 0.5f));
				dAZPhysicsMeshSoftVerticesSet.initialTargetPosition = initialTargetPosition;
			}
		}
	}

	public void AdjustLinkJointDistancesFast()
	{
		if (!wasInit || !useLinkJoints || !linkTargetsDirty)
		{
			return;
		}
		for (int i = 0; i < _softVerticesSets.Count; i++)
		{
			DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = _softVerticesSets[i];
			for (int j = 0; j < dAZPhysicsMeshSoftVerticesSet.links.Count; j++)
			{
				DAZPhysicsMeshSoftVerticesSet setByID = GetSetByID(dAZPhysicsMeshSoftVerticesSet.links[j]);
				if (setByID != null && (dAZPhysicsMeshSoftVerticesSet.linkTargetPositionDirty || setByID.linkTargetPositionDirty || triggerThreadedScaleChange))
				{
					float num = (dAZPhysicsMeshSoftVerticesSet.initialTargetPosition - setByID.initialTargetPosition).magnitude * scale;
					dAZPhysicsMeshSoftVerticesSet.linkJointDistances[j] = num;
				}
			}
		}
	}

	public void AdjustLinkJointDistancesFinishFast()
	{
		if (!wasInit || !useLinkJoints || !linkTargetsDirty)
		{
			return;
		}
		for (int i = 0; i < _softVerticesSets.Count; i++)
		{
			DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = _softVerticesSets[i];
			for (int j = 0; j < dAZPhysicsMeshSoftVerticesSet.links.Count; j++)
			{
				DAZPhysicsMeshSoftVerticesSet setByID = GetSetByID(dAZPhysicsMeshSoftVerticesSet.links[j]);
				if (setByID != null && (dAZPhysicsMeshSoftVerticesSet.linkTargetPositionDirty || setByID.linkTargetPositionDirty || triggerThreadedScaleChange))
				{
					SpringJoint springJoint = dAZPhysicsMeshSoftVerticesSet.linkJoints[j];
					if (springJoint != null)
					{
						float maxDistance = (springJoint.minDistance = dAZPhysicsMeshSoftVerticesSet.linkJointDistances[j]);
						springJoint.maxDistance = maxDistance;
					}
				}
			}
		}
		triggerThreadedScaleChange = false;
	}

	public void AdjustLinkJointDistances(bool force = false)
	{
		if (!wasInit || !useLinkJoints)
		{
			return;
		}
		for (int i = 0; i < _softVerticesSets.Count; i++)
		{
			DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = _softVerticesSets[i];
			for (int j = 0; j < dAZPhysicsMeshSoftVerticesSet.links.Count; j++)
			{
				DAZPhysicsMeshSoftVerticesSet setByID = GetSetByID(dAZPhysicsMeshSoftVerticesSet.links[j]);
				if (setByID != null && (dAZPhysicsMeshSoftVerticesSet.linkTargetPositionDirty || setByID.linkTargetPositionDirty || force))
				{
					float num = (dAZPhysicsMeshSoftVerticesSet.initialTargetPosition - setByID.initialTargetPosition).magnitude * scale;
					dAZPhysicsMeshSoftVerticesSet.linkJointDistances[j] = num;
					SpringJoint springJoint = dAZPhysicsMeshSoftVerticesSet.linkJoints[j];
					if (springJoint != null)
					{
						springJoint.minDistance = num;
						springJoint.maxDistance = num;
					}
				}
			}
		}
	}

	public void ResetJoints()
	{
		if (!wasInit)
		{
			return;
		}
		_appliedBackForce.x = 0f;
		_appliedBackForce.y = 0f;
		_appliedBackForce.z = 0f;
		_bufferedBackForce.x = 0f;
		_bufferedBackForce.y = 0f;
		_bufferedBackForce.z = 0f;
		if (embedJoints)
		{
			embedTransform.position = _normalReference.position;
			embedTransform.rotation = _normalReference.rotation;
		}
		if (predictionTransform != null)
		{
			predictionTransformPosition1 = predictionTransform.position;
			predictionTransformPosition2 = predictionTransformPosition1;
			predictionTransformPosition3 = predictionTransformPosition1;
		}
		for (int i = 0; i < _softVerticesSets.Count; i++)
		{
			DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = _softVerticesSets[i];
			if (!_skin.postSkinVertsReady[dAZPhysicsMeshSoftVerticesSet.targetVertex])
			{
				continue;
			}
			Vector3 vector = ((!centerBetweenTargetAndAnchor) ? _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex] : ((_skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex] + _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex]) * 0.5f));
			if (embedJoints)
			{
				dAZPhysicsMeshSoftVerticesSet.joint.connectedAnchor = _normalReference.InverseTransformPoint(vector);
			}
			else
			{
				dAZPhysicsMeshSoftVerticesSet.lastKinematicPosition = vector;
				dAZPhysicsMeshSoftVerticesSet.kinematicTransform.position = vector;
			}
			dAZPhysicsMeshSoftVerticesSet.jointTransform.position = vector;
			dAZPhysicsMeshSoftVerticesSet.lastPosition = dAZPhysicsMeshSoftVerticesSet.jointTransform.position;
			dAZPhysicsMeshSoftVerticesSet.jointTargetPosition = vector;
			dAZPhysicsMeshSoftVerticesSet.lastJointTargetPosition = dAZPhysicsMeshSoftVerticesSet.jointTargetPosition;
			dAZPhysicsMeshSoftVerticesSet.jointTargetVelocity = Vector3.zero;
			if (_useSimulation && !_useUniformLimit && !embedJoints)
			{
				dAZPhysicsMeshSoftVerticesSet.jointTrackerTransform.position = dAZPhysicsMeshSoftVerticesSet.kinematicTransform.position;
			}
			if (embedJoints)
			{
				continue;
			}
			Quaternion identity = Quaternion.identity;
			if (lookAtOption == LookAtOption.NormalReference || dAZPhysicsMeshSoftVerticesSet.forceLookAtReference)
			{
				identity.SetLookRotation(_normalReference.position - vector, _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex] - _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex]);
			}
			else if (lookAtOption == LookAtOption.VertexNormal)
			{
				if (centerBetweenTargetAndAnchor)
				{
					identity.SetLookRotation((-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet.targetVertex] - _skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet.anchorVertex]) * 0.5f);
				}
				else
				{
					identity.SetLookRotation(-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet.targetVertex]);
				}
			}
			else if (lookAtOption == LookAtOption.VertexNormalRefUp)
			{
				if (centerBetweenTargetAndAnchor)
				{
					identity.SetLookRotation((-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet.targetVertex] - _skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet.anchorVertex]) * 0.5f, _normalReference.position - vector);
				}
				else
				{
					identity.SetLookRotation(-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet.targetVertex], _normalReference.position - vector);
				}
			}
			else if (lookAtOption == LookAtOption.VertexNormalAnchorUp)
			{
				if (centerBetweenTargetAndAnchor)
				{
					identity.SetLookRotation((-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet.targetVertex] - _skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet.anchorVertex]) * 0.5f, _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex] - _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex]);
				}
				else
				{
					identity.SetLookRotation(-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet.targetVertex], _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex] - _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex]);
				}
			}
			else
			{
				identity.SetLookRotation(_skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex] - _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex], normalReference.position - vector);
			}
			dAZPhysicsMeshSoftVerticesSet.kinematicTransform.rotation = identity;
			dAZPhysicsMeshSoftVerticesSet.jointTransform.rotation = identity;
			dAZPhysicsMeshSoftVerticesSet.jointRB.velocity = Vector3.zero;
			dAZPhysicsMeshSoftVerticesSet.jointRB.angularVelocity = Vector3.zero;
		}
	}

	public void PrepareUpdateJointsThreaded()
	{
		if (_normalReference != null)
		{
			Vector3 position = _normalReference.position;
			if (NaNUtils.IsVector3Valid(position))
			{
				_normalReferencePosition = position;
			}
			else
			{
				parent.containingAtom.AlertPhysicsCorruption("PhysicsMesh normal reference " + _normalReference.name);
			}
		}
	}

	public void UpdateJointTargetsThreadedFast(Vector3[] verts, Vector3[] normals)
	{
		for (int i = 0; i < _softVerticesSets.Count; i++)
		{
			DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = _softVerticesSets[i];
			Vector3 vector = ((!centerBetweenTargetAndAnchor) ? verts[dAZPhysicsMeshSoftVerticesSet.targetVertex] : ((verts[dAZPhysicsMeshSoftVerticesSet.targetVertex] + verts[dAZPhysicsMeshSoftVerticesSet.anchorVertex]) * 0.5f));
			dAZPhysicsMeshSoftVerticesSet.lastJointTargetPosition = dAZPhysicsMeshSoftVerticesSet.jointTargetPosition;
			dAZPhysicsMeshSoftVerticesSet.jointTargetPosition = vector;
			dAZPhysicsMeshSoftVerticesSet.jointTargetVelocity = (dAZPhysicsMeshSoftVerticesSet.jointTargetPosition - dAZPhysicsMeshSoftVerticesSet.lastJointTargetPosition) / skinTimeDelta;
			Quaternion identity = Quaternion.identity;
			if (lookAtOption == LookAtOption.NormalReference || dAZPhysicsMeshSoftVerticesSet.forceLookAtReference)
			{
				identity.SetLookRotation(_normalReferencePosition - vector, verts[dAZPhysicsMeshSoftVerticesSet.anchorVertex] - verts[dAZPhysicsMeshSoftVerticesSet.targetVertex]);
			}
			else if (lookAtOption == LookAtOption.VertexNormal)
			{
				if (centerBetweenTargetAndAnchor)
				{
					identity.SetLookRotation((-normals[dAZPhysicsMeshSoftVerticesSet.targetVertex] - normals[dAZPhysicsMeshSoftVerticesSet.anchorVertex]) * 0.5f);
				}
				else
				{
					identity.SetLookRotation(-normals[dAZPhysicsMeshSoftVerticesSet.targetVertex]);
				}
			}
			else if (lookAtOption == LookAtOption.VertexNormalRefUp)
			{
				if (centerBetweenTargetAndAnchor)
				{
					identity.SetLookRotation((-normals[dAZPhysicsMeshSoftVerticesSet.targetVertex] - normals[dAZPhysicsMeshSoftVerticesSet.anchorVertex]) * 0.5f, _normalReferencePosition - vector);
				}
				else
				{
					identity.SetLookRotation(-normals[dAZPhysicsMeshSoftVerticesSet.targetVertex], _normalReferencePosition - vector);
				}
			}
			else if (lookAtOption == LookAtOption.VertexNormalAnchorUp)
			{
				if (centerBetweenTargetAndAnchor)
				{
					identity.SetLookRotation((-normals[dAZPhysicsMeshSoftVerticesSet.targetVertex] - normals[dAZPhysicsMeshSoftVerticesSet.anchorVertex]) * 0.5f, verts[dAZPhysicsMeshSoftVerticesSet.anchorVertex] - verts[dAZPhysicsMeshSoftVerticesSet.targetVertex]);
				}
				else
				{
					identity.SetLookRotation(-normals[dAZPhysicsMeshSoftVerticesSet.targetVertex], verts[dAZPhysicsMeshSoftVerticesSet.anchorVertex] - vector);
				}
			}
			else
			{
				identity.SetLookRotation(verts[dAZPhysicsMeshSoftVerticesSet.anchorVertex] - verts[dAZPhysicsMeshSoftVerticesSet.targetVertex], _normalReferencePosition - vector);
			}
			dAZPhysicsMeshSoftVerticesSet.jointTargetLookAt = identity;
		}
	}

	public void UpdateJointTargetsThreaded()
	{
		if (!wasInit || embedJoints)
		{
			return;
		}
		for (int i = 0; i < _softVerticesSets.Count; i++)
		{
			DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = _softVerticesSets[i];
			Vector3 vector = ((!centerBetweenTargetAndAnchor) ? _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex] : ((_skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex] + _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex]) * 0.5f));
			dAZPhysicsMeshSoftVerticesSet.lastJointTargetPosition = dAZPhysicsMeshSoftVerticesSet.jointTargetPosition;
			dAZPhysicsMeshSoftVerticesSet.jointTargetPosition = vector;
			Quaternion identity = Quaternion.identity;
			if (lookAtOption == LookAtOption.NormalReference || dAZPhysicsMeshSoftVerticesSet.forceLookAtReference)
			{
				identity.SetLookRotation(_normalReferencePosition - vector, _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex] - _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex]);
			}
			else if (lookAtOption == LookAtOption.VertexNormal)
			{
				if (centerBetweenTargetAndAnchor)
				{
					identity.SetLookRotation((-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet.targetVertex] - _skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet.anchorVertex]) * 0.5f);
				}
				else
				{
					identity.SetLookRotation(-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet.targetVertex]);
				}
			}
			else if (lookAtOption == LookAtOption.VertexNormalRefUp)
			{
				if (centerBetweenTargetAndAnchor)
				{
					identity.SetLookRotation((-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet.targetVertex] - _skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet.anchorVertex]) * 0.5f, _normalReferencePosition - vector);
				}
				else
				{
					identity.SetLookRotation(-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet.targetVertex], _normalReferencePosition - vector);
				}
			}
			else if (lookAtOption == LookAtOption.VertexNormalAnchorUp)
			{
				if (centerBetweenTargetAndAnchor)
				{
					identity.SetLookRotation((-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet.targetVertex] - _skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet.anchorVertex]) * 0.5f, _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex] - _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex]);
				}
				else
				{
					identity.SetLookRotation(-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet.targetVertex], _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex] - vector);
				}
			}
			else
			{
				identity.SetLookRotation(_skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex] - _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex], _normalReferencePosition - vector);
			}
			dAZPhysicsMeshSoftVerticesSet.jointTargetLookAt = identity;
		}
	}

	public void UpdateJointsFast(bool predictOnly = false)
	{
		if (!wasInit || !(_normalReference != null) || !_on || _freeze)
		{
			return;
		}
		Vector3 vector = default(Vector3);
		vector.x = 0f;
		vector.y = 0f;
		vector.z = 0f;
		float num = 0f;
		float num2 = _jointBackForceThresholdDistance * scale;
		if (useJointBackForce && _jointBackForce > 0f)
		{
			num = ((_jointBackForceThresholdDistance != 0f) ? (1f / num2) : 1E+20f);
		}
		float num3 = _normalDistanceLimit * scale;
		float num4 = _normalNegativeDistanceLimit * scale;
		float num5 = _tangentDistanceLimit * scale;
		float num6 = _tangentNegativeDistanceLimit * scale;
		float num7 = _tangent2DistanceLimit * scale;
		float num8 = _tangent2NegativeDistanceLimit * scale;
		if (resetSimulation)
		{
			ResetJoints();
			return;
		}
		Vector3 vector2 = default(Vector3);
		vector2.x = 0f;
		vector2.y = 0f;
		vector2.z = 0f;
		if (predictionTransform != null)
		{
			switch (numPredictionFrames)
			{
			case 1:
				vector2 = predictionTransform.position - predictionTransformPosition1;
				break;
			case 2:
				vector2 = predictionTransform.position - predictionTransformPosition2;
				break;
			case 3:
				vector2 = predictionTransform.position - predictionTransformPosition3;
				break;
			}
			predictionTransformPosition3 = predictionTransformPosition2;
			predictionTransformPosition2 = predictionTransformPosition1;
			predictionTransformPosition1 = predictionTransform.position;
		}
		float num9 = Time.fixedTime - skinTime;
		if (useSimulation)
		{
			for (int i = 0; i < _softVerticesSets.Count; i++)
			{
				DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = _softVerticesSets[i];
				dAZPhysicsMeshSoftVerticesSet.lastKinematicPosition = dAZPhysicsMeshSoftVerticesSet.kinematicRB.position;
				dAZPhysicsMeshSoftVerticesSet.kinematicRB.MovePosition(dAZPhysicsMeshSoftVerticesSet.jointTargetPosition + dAZPhysicsMeshSoftVerticesSet.jointTargetVelocity * num9);
				dAZPhysicsMeshSoftVerticesSet.kinematicRB.MoveRotation(dAZPhysicsMeshSoftVerticesSet.jointTargetLookAt);
				if (!_useUniformLimit)
				{
					dAZPhysicsMeshSoftVerticesSet.jointTrackerTransform.position = dAZPhysicsMeshSoftVerticesSet.jointTransform.position;
					Vector3 localPosition = dAZPhysicsMeshSoftVerticesSet.jointTrackerTransform.localPosition;
					bool flag = false;
					if (_normalMovementType == MovementType.Limit)
					{
						float num10 = num3 * dAZPhysicsMeshSoftVerticesSet.limitMultiplier;
						float num11 = num4 * dAZPhysicsMeshSoftVerticesSet.limitMultiplier;
						if (lookAtOption != 0)
						{
							if (localPosition.z > num10)
							{
								localPosition.z = num10;
								flag = true;
							}
							else if (localPosition.z < 0f - num11)
							{
								localPosition.z = 0f - num11;
								flag = true;
							}
						}
						else if (localPosition.y > num10)
						{
							localPosition.y = num10;
							flag = true;
						}
						else if (localPosition.y < 0f - num11)
						{
							localPosition.y = 0f - num11;
							flag = true;
						}
					}
					if (_tangentMovementType == MovementType.Limit)
					{
						float num12 = num5 * dAZPhysicsMeshSoftVerticesSet.limitMultiplier;
						float num13 = num6 * dAZPhysicsMeshSoftVerticesSet.limitMultiplier;
						if (lookAtOption != 0)
						{
							if (localPosition.y > num12)
							{
								localPosition.y = num12;
								flag = true;
							}
							else if (localPosition.y < 0f - num13)
							{
								localPosition.y = 0f - num13;
								flag = true;
							}
						}
						else if (localPosition.z > num12)
						{
							localPosition.z = num12;
							flag = true;
						}
						else if (localPosition.z < 0f - num13)
						{
							localPosition.z = 0f - num13;
							flag = true;
						}
					}
					if (_tangent2MovementType == MovementType.Limit)
					{
						float num14 = num7 * dAZPhysicsMeshSoftVerticesSet.limitMultiplier;
						float num15 = num8 * dAZPhysicsMeshSoftVerticesSet.limitMultiplier;
						if (localPosition.x > num14)
						{
							localPosition.x = num14;
							flag = true;
						}
						else if (localPosition.x < 0f - num15)
						{
							localPosition.x = 0f - num15;
							flag = true;
						}
					}
					if (flag)
					{
						dAZPhysicsMeshSoftVerticesSet.jointTrackerTransform.localPosition = localPosition;
						dAZPhysicsMeshSoftVerticesSet.jointRB.position = dAZPhysicsMeshSoftVerticesSet.jointTrackerTransform.position;
					}
				}
				dAZPhysicsMeshSoftVerticesSet.lastPosition = dAZPhysicsMeshSoftVerticesSet.jointRB.position;
				if (useJointBackForce && _jointBackForce > 0f)
				{
					Vector3 vector3 = dAZPhysicsMeshSoftVerticesSet.lastPosition - dAZPhysicsMeshSoftVerticesSet.lastKinematicPosition;
					if (vector3.x > num2)
					{
						vector3.x -= num2;
					}
					else if (vector3.x < 0f - num2)
					{
						vector3.x += num2;
					}
					else
					{
						vector3.x = 0f;
					}
					if (vector3.y > num2)
					{
						vector3.y -= num2;
					}
					else if (vector3.y < 0f - num2)
					{
						vector3.y += num2;
					}
					else
					{
						vector3.y = 0f;
					}
					if (vector3.z > num2)
					{
						vector3.z -= num2;
					}
					else if (vector3.z < 0f - num2)
					{
						vector3.z += num2;
					}
					else
					{
						vector3.z = 0f;
					}
					vector += vector3;
				}
			}
		}
		else
		{
			for (int j = 0; j < _softVerticesSets.Count; j++)
			{
				DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet2 = _softVerticesSets[j];
				dAZPhysicsMeshSoftVerticesSet2.lastKinematicPosition = dAZPhysicsMeshSoftVerticesSet2.kinematicTransform.position;
				dAZPhysicsMeshSoftVerticesSet2.kinematicTransform.SetPositionAndRotation(dAZPhysicsMeshSoftVerticesSet2.jointTargetPosition + dAZPhysicsMeshSoftVerticesSet2.jointTargetVelocity * num9, dAZPhysicsMeshSoftVerticesSet2.jointTargetLookAt);
				if (!_useUniformLimit)
				{
					Vector3 localPosition2 = dAZPhysicsMeshSoftVerticesSet2.jointTransform.localPosition;
					bool flag2 = false;
					if (_normalMovementType == MovementType.Limit)
					{
						float num16 = num3 * dAZPhysicsMeshSoftVerticesSet2.limitMultiplier;
						float num17 = num4 * dAZPhysicsMeshSoftVerticesSet2.limitMultiplier;
						if (lookAtOption != 0)
						{
							if (localPosition2.z > num16)
							{
								localPosition2.z = num16;
								flag2 = true;
							}
							else if (localPosition2.z < 0f - num17)
							{
								localPosition2.z = 0f - num17;
								flag2 = true;
							}
						}
						else if (localPosition2.y > num16)
						{
							localPosition2.y = num16;
							flag2 = true;
						}
						else if (localPosition2.y < 0f - num17)
						{
							localPosition2.y = 0f - num17;
							flag2 = true;
						}
					}
					if (_tangentMovementType == MovementType.Limit)
					{
						float num18 = num5 * dAZPhysicsMeshSoftVerticesSet2.limitMultiplier;
						float num19 = num6 * dAZPhysicsMeshSoftVerticesSet2.limitMultiplier;
						if (lookAtOption != 0)
						{
							if (localPosition2.y > num18)
							{
								localPosition2.y = num18;
								flag2 = true;
							}
							else if (localPosition2.y < 0f - num19)
							{
								localPosition2.y = 0f - num19;
								flag2 = true;
							}
						}
						else if (localPosition2.z > num18)
						{
							localPosition2.z = num18;
							flag2 = true;
						}
						else if (localPosition2.z < 0f - num19)
						{
							localPosition2.z = 0f - num19;
							flag2 = true;
						}
					}
					if (_tangent2MovementType == MovementType.Limit)
					{
						float num20 = num7 * dAZPhysicsMeshSoftVerticesSet2.limitMultiplier;
						float num21 = num8 * dAZPhysicsMeshSoftVerticesSet2.limitMultiplier;
						if (localPosition2.x > num20)
						{
							localPosition2.x = num20;
							flag2 = true;
						}
						else if (localPosition2.x < 0f - num21)
						{
							localPosition2.x = 0f - num21;
							flag2 = true;
						}
					}
					if (flag2)
					{
						dAZPhysicsMeshSoftVerticesSet2.jointTransform.localPosition = localPosition2;
					}
				}
				dAZPhysicsMeshSoftVerticesSet2.lastPosition = dAZPhysicsMeshSoftVerticesSet2.jointRB.position;
				if (useJointBackForce && _jointBackForce > 0f)
				{
					Vector3 vector4 = dAZPhysicsMeshSoftVerticesSet2.lastPosition - dAZPhysicsMeshSoftVerticesSet2.lastKinematicPosition;
					float num22 = Mathf.Abs(vector4.x) + Mathf.Abs(vector4.y) + Mathf.Abs(vector4.z);
					if (num22 > num2)
					{
						float num23 = Mathf.Clamp01((num22 - num2) * num);
						vector += vector4 * num23;
					}
				}
			}
		}
		if (useJointBackForce && _jointBackForce > 0f && backForceRigidbody != null)
		{
			float num24 = ((!(TimeControl.singleton != null) || !TimeControl.singleton.compensateFixedTimestep) ? 1f : ((!Mathf.Approximately(Time.timeScale, 0f)) ? 1f : (1f / Time.timeScale)));
			Vector3 vector5 = vector * _jointBackForce * num24;
			_appliedBackForce = Vector3.ClampMagnitude(vector5, _jointBackForceMaxForce * scale);
		}
	}

	public void UpdateJoints()
	{
		if (!wasInit || !(_normalReference != null) || !_on || _freeze)
		{
			return;
		}
		Vector3 vector = default(Vector3);
		vector.x = 0f;
		vector.y = 0f;
		vector.z = 0f;
		float num = 0f;
		float num2 = _jointBackForceThresholdDistance * scale;
		if (useJointBackForce && _jointBackForce > 0f)
		{
			num = ((_jointBackForceThresholdDistance != 0f) ? (1f / num2) : 1E+20f);
		}
		float num3 = _normalDistanceLimit * scale;
		float num4 = _normalNegativeDistanceLimit * scale;
		float num5 = _tangentDistanceLimit * scale;
		float num6 = _tangentNegativeDistanceLimit * scale;
		float num7 = _tangent2DistanceLimit * scale;
		float num8 = _tangent2NegativeDistanceLimit * scale;
		if (resetSimulation)
		{
			ResetJoints();
			return;
		}
		Vector3 vector2 = default(Vector3);
		vector2.x = 0f;
		vector2.y = 0f;
		vector2.z = 0f;
		if (predictionTransform != null)
		{
			switch (numPredictionFrames)
			{
			case 1:
				vector2 = predictionTransform.position - predictionTransformPosition1;
				break;
			case 2:
				vector2 = predictionTransform.position - predictionTransformPosition2;
				break;
			case 3:
				vector2 = predictionTransform.position - predictionTransformPosition3;
				break;
			}
			predictionTransformPosition3 = predictionTransformPosition2;
			predictionTransformPosition2 = predictionTransformPosition1;
			predictionTransformPosition1 = predictionTransform.position;
		}
		if (_useSimulation)
		{
			if (embedJoints)
			{
				embedRB.MovePosition(_normalReference.position);
				embedRB.MoveRotation(_normalReference.rotation);
				for (int i = 0; i < _softVerticesSets.Count; i++)
				{
					DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = _softVerticesSets[i];
					Vector3 vector3 = ((!centerBetweenTargetAndAnchor) ? _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex] : ((_skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex] + _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex]) * 0.5f));
					dAZPhysicsMeshSoftVerticesSet.joint.connectedAnchor = _normalReference.InverseTransformPoint(vector3);
					dAZPhysicsMeshSoftVerticesSet.lastPosition = dAZPhysicsMeshSoftVerticesSet.jointRB.position;
					if (_clampVelocity && dAZPhysicsMeshSoftVerticesSet.jointRB.velocity.sqrMagnitude > _maxSimulationVelocitySqr)
					{
						dAZPhysicsMeshSoftVerticesSet.jointRB.velocity = dAZPhysicsMeshSoftVerticesSet.jointRB.velocity.normalized * _maxSimulationVelocity;
					}
					if (useJointBackForce && _jointBackForce > 0f)
					{
						Vector3 vector4 = dAZPhysicsMeshSoftVerticesSet.jointTransform.position - vector3;
						float num9 = Mathf.Abs(vector4.x) + Mathf.Abs(vector4.y) + Mathf.Abs(vector4.z);
						if (num9 > num2)
						{
							float num10 = Mathf.Clamp01((num9 - num2) * num);
							vector += vector4 * num10;
						}
					}
				}
			}
			else if (useThreading)
			{
				for (int j = 0; j < _softVerticesSets.Count; j++)
				{
					DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet2 = _softVerticesSets[j];
					dAZPhysicsMeshSoftVerticesSet2.lastKinematicPosition = dAZPhysicsMeshSoftVerticesSet2.kinematicTransform.position;
					dAZPhysicsMeshSoftVerticesSet2.kinematicRB.MovePosition(dAZPhysicsMeshSoftVerticesSet2.jointTargetPosition + vector2);
					dAZPhysicsMeshSoftVerticesSet2.kinematicRB.MoveRotation(dAZPhysicsMeshSoftVerticesSet2.jointTargetLookAt);
					if (!_useUniformLimit)
					{
						dAZPhysicsMeshSoftVerticesSet2.jointTrackerTransform.position = dAZPhysicsMeshSoftVerticesSet2.jointTransform.position;
						Vector3 localPosition = dAZPhysicsMeshSoftVerticesSet2.jointTrackerTransform.localPosition;
						bool flag = false;
						if (_normalMovementType == MovementType.Limit)
						{
							float num11 = num3 * dAZPhysicsMeshSoftVerticesSet2.limitMultiplier;
							float num12 = num4 * dAZPhysicsMeshSoftVerticesSet2.limitMultiplier;
							if (lookAtOption != 0)
							{
								if (localPosition.z > num11)
								{
									localPosition.z = num11;
									flag = true;
								}
								else if (localPosition.z < 0f - num12)
								{
									localPosition.z = 0f - num12;
									flag = true;
								}
							}
							else if (localPosition.y > num11)
							{
								localPosition.y = num11;
								flag = true;
							}
							else if (localPosition.y < 0f - num12)
							{
								localPosition.y = 0f - num12;
								flag = true;
							}
						}
						if (_tangentMovementType == MovementType.Limit)
						{
							float num13 = num5 * dAZPhysicsMeshSoftVerticesSet2.limitMultiplier;
							float num14 = num6 * dAZPhysicsMeshSoftVerticesSet2.limitMultiplier;
							if (lookAtOption != 0)
							{
								if (localPosition.y > num13)
								{
									localPosition.y = num13;
									flag = true;
								}
								else if (localPosition.y < 0f - num14)
								{
									localPosition.y = 0f - num14;
									flag = true;
								}
							}
							else if (localPosition.z > num13)
							{
								localPosition.z = num13;
								flag = true;
							}
							else if (localPosition.z < 0f - num14)
							{
								localPosition.z = 0f - num14;
								flag = true;
							}
						}
						if (_tangent2MovementType == MovementType.Limit)
						{
							float num15 = num7 * dAZPhysicsMeshSoftVerticesSet2.limitMultiplier;
							float num16 = num8 * dAZPhysicsMeshSoftVerticesSet2.limitMultiplier;
							if (localPosition.x > num15)
							{
								localPosition.x = num15;
								flag = true;
							}
							else if (localPosition.x < 0f - num16)
							{
								localPosition.x = 0f - num16;
								flag = true;
							}
						}
						if (flag)
						{
							dAZPhysicsMeshSoftVerticesSet2.jointTrackerTransform.localPosition = localPosition;
							dAZPhysicsMeshSoftVerticesSet2.jointRB.position = dAZPhysicsMeshSoftVerticesSet2.jointTrackerTransform.position;
						}
					}
					dAZPhysicsMeshSoftVerticesSet2.lastPosition = dAZPhysicsMeshSoftVerticesSet2.jointRB.position;
					if (useJointBackForce && _jointBackForce > 0f)
					{
						Vector3 vector5 = dAZPhysicsMeshSoftVerticesSet2.jointTransform.position - dAZPhysicsMeshSoftVerticesSet2.lastJointTargetPosition - vector2;
						if (vector5.x > num2)
						{
							vector5.x -= num2;
						}
						else if (vector5.x < 0f - num2)
						{
							vector5.x += num2;
						}
						else
						{
							vector5.x = 0f;
						}
						if (vector5.y > num2)
						{
							vector5.y -= num2;
						}
						else if (vector5.y < 0f - num2)
						{
							vector5.y += num2;
						}
						else
						{
							vector5.y = 0f;
						}
						if (vector5.z > num2)
						{
							vector5.z -= num2;
						}
						else if (vector5.z < 0f - num2)
						{
							vector5.z += num2;
						}
						else
						{
							vector5.z = 0f;
						}
						vector += vector5;
					}
				}
			}
			else
			{
				for (int k = 0; k < _softVerticesSets.Count; k++)
				{
					DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet3 = _softVerticesSets[k];
					Vector3 vector6 = ((!centerBetweenTargetAndAnchor) ? _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet3.targetVertex] : ((_skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet3.targetVertex] + _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet3.anchorVertex]) * 0.5f));
					Quaternion identity = Quaternion.identity;
					if (lookAtOption == LookAtOption.NormalReference || dAZPhysicsMeshSoftVerticesSet3.forceLookAtReference)
					{
						identity.SetLookRotation(_normalReference.position - vector6, _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet3.anchorVertex] - _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet3.targetVertex]);
					}
					else if (lookAtOption == LookAtOption.VertexNormal)
					{
						if (centerBetweenTargetAndAnchor)
						{
							identity.SetLookRotation((-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet3.targetVertex] - _skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet3.anchorVertex]) * 0.5f);
						}
						else
						{
							identity.SetLookRotation(-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet3.targetVertex]);
						}
					}
					else if (lookAtOption == LookAtOption.VertexNormalRefUp)
					{
						if (centerBetweenTargetAndAnchor)
						{
							identity.SetLookRotation((-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet3.targetVertex] - _skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet3.anchorVertex]) * 0.5f, _normalReference.position - vector6);
						}
						else
						{
							identity.SetLookRotation(-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet3.targetVertex], _normalReference.position - vector6);
						}
					}
					else if (lookAtOption == LookAtOption.VertexNormalAnchorUp)
					{
						if (centerBetweenTargetAndAnchor)
						{
							identity.SetLookRotation((-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet3.targetVertex] - _skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet3.anchorVertex]) * 0.5f, _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet3.anchorVertex] - _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet3.targetVertex]);
						}
						else
						{
							identity.SetLookRotation(-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet3.targetVertex], _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet3.anchorVertex] - _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet3.targetVertex]);
						}
					}
					else
					{
						identity.SetLookRotation(_skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet3.anchorVertex] - _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet3.targetVertex], normalReference.position - vector6);
					}
					dAZPhysicsMeshSoftVerticesSet3.lastKinematicPosition = dAZPhysicsMeshSoftVerticesSet3.kinematicTransform.position;
					dAZPhysicsMeshSoftVerticesSet3.kinematicRB.MovePosition(vector6 + vector2);
					dAZPhysicsMeshSoftVerticesSet3.kinematicRB.MoveRotation(identity);
					if (!_useUniformLimit)
					{
						dAZPhysicsMeshSoftVerticesSet3.jointTrackerTransform.position = dAZPhysicsMeshSoftVerticesSet3.jointTransform.position;
						Vector3 localPosition2 = dAZPhysicsMeshSoftVerticesSet3.jointTrackerTransform.localPosition;
						bool flag2 = false;
						if (_normalMovementType == MovementType.Limit)
						{
							float num17 = num3 * dAZPhysicsMeshSoftVerticesSet3.limitMultiplier;
							float num18 = num4 * dAZPhysicsMeshSoftVerticesSet3.limitMultiplier;
							if (lookAtOption != 0)
							{
								if (localPosition2.z > num17)
								{
									localPosition2.z = num17;
									flag2 = true;
								}
								else if (localPosition2.z < 0f - num18)
								{
									localPosition2.z = 0f - num18;
									flag2 = true;
								}
							}
							else if (localPosition2.y > num17)
							{
								localPosition2.y = num17;
								flag2 = true;
							}
							else if (localPosition2.y < 0f - num18)
							{
								localPosition2.y = 0f - num18;
								flag2 = true;
							}
						}
						if (_tangentMovementType == MovementType.Limit)
						{
							float num19 = num5 * dAZPhysicsMeshSoftVerticesSet3.limitMultiplier;
							float num20 = num6 * dAZPhysicsMeshSoftVerticesSet3.limitMultiplier;
							if (lookAtOption != 0)
							{
								if (localPosition2.y > num19)
								{
									localPosition2.y = num19;
									flag2 = true;
								}
								else if (localPosition2.y < 0f - num20)
								{
									localPosition2.y = 0f - num20;
									flag2 = true;
								}
							}
							else if (localPosition2.z > num19)
							{
								localPosition2.z = num19;
								flag2 = true;
							}
							else if (localPosition2.z < 0f - num20)
							{
								localPosition2.z = 0f - num20;
								flag2 = true;
							}
						}
						if (_tangent2MovementType == MovementType.Limit)
						{
							float num21 = num7 * dAZPhysicsMeshSoftVerticesSet3.limitMultiplier;
							float num22 = num8 * dAZPhysicsMeshSoftVerticesSet3.limitMultiplier;
							if (localPosition2.x > num21)
							{
								localPosition2.x = num21;
								flag2 = true;
							}
							else if (localPosition2.x < 0f - num22)
							{
								localPosition2.x = 0f - num22;
								flag2 = true;
							}
						}
						if (flag2)
						{
							dAZPhysicsMeshSoftVerticesSet3.jointTrackerTransform.localPosition = localPosition2;
							dAZPhysicsMeshSoftVerticesSet3.jointRB.position = dAZPhysicsMeshSoftVerticesSet3.jointTrackerTransform.position;
						}
					}
					dAZPhysicsMeshSoftVerticesSet3.lastPosition = dAZPhysicsMeshSoftVerticesSet3.jointRB.position;
					if (_clampVelocity && dAZPhysicsMeshSoftVerticesSet3.jointRB.velocity.sqrMagnitude > _maxSimulationVelocitySqr)
					{
						dAZPhysicsMeshSoftVerticesSet3.jointRB.velocity = dAZPhysicsMeshSoftVerticesSet3.jointRB.velocity.normalized * _maxSimulationVelocity;
					}
					if (useJointBackForce && _jointBackForce > 0f)
					{
						Vector3 vector7 = dAZPhysicsMeshSoftVerticesSet3.jointTransform.position - vector6 - vector2;
						float num23 = Mathf.Abs(vector7.x) + Mathf.Abs(vector7.y) + Mathf.Abs(vector7.z);
						if (num23 > num2)
						{
							float num24 = Mathf.Clamp01((num23 - num2) * num);
							vector += vector7 * num24;
						}
					}
				}
			}
		}
		else if (useThreading)
		{
			for (int l = 0; l < _softVerticesSets.Count; l++)
			{
				DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet4 = _softVerticesSets[l];
				dAZPhysicsMeshSoftVerticesSet4.kinematicTransform.SetPositionAndRotation(dAZPhysicsMeshSoftVerticesSet4.jointTargetPosition + vector2, dAZPhysicsMeshSoftVerticesSet4.jointTargetLookAt);
				if (!_useUniformLimit)
				{
					Vector3 localPosition3 = dAZPhysicsMeshSoftVerticesSet4.jointTransform.localPosition;
					bool flag3 = false;
					if (_normalMovementType == MovementType.Limit)
					{
						float num25 = num3 * dAZPhysicsMeshSoftVerticesSet4.limitMultiplier;
						float num26 = num4 * dAZPhysicsMeshSoftVerticesSet4.limitMultiplier;
						if (lookAtOption != 0)
						{
							if (localPosition3.z > num25)
							{
								localPosition3.z = num25;
								flag3 = true;
							}
							else if (localPosition3.z < 0f - num26)
							{
								localPosition3.z = 0f - num26;
								flag3 = true;
							}
						}
						else if (localPosition3.y > num25)
						{
							localPosition3.y = num25;
							flag3 = true;
						}
						else if (localPosition3.y < 0f - num26)
						{
							localPosition3.y = 0f - num26;
							flag3 = true;
						}
					}
					if (_tangentMovementType == MovementType.Limit)
					{
						float num27 = num5 * dAZPhysicsMeshSoftVerticesSet4.limitMultiplier;
						float num28 = num6 * dAZPhysicsMeshSoftVerticesSet4.limitMultiplier;
						if (lookAtOption != 0)
						{
							if (localPosition3.y > num27)
							{
								localPosition3.y = num27;
								flag3 = true;
							}
							else if (localPosition3.y < 0f - num28)
							{
								localPosition3.y = 0f - num28;
								flag3 = true;
							}
						}
						else if (localPosition3.z > num27)
						{
							localPosition3.z = num27;
							flag3 = true;
						}
						else if (localPosition3.z < 0f - num28)
						{
							localPosition3.z = 0f - num28;
							flag3 = true;
						}
					}
					if (_tangent2MovementType == MovementType.Limit)
					{
						float num29 = num7 * dAZPhysicsMeshSoftVerticesSet4.limitMultiplier;
						float num30 = num8 * dAZPhysicsMeshSoftVerticesSet4.limitMultiplier;
						if (localPosition3.x > num29)
						{
							localPosition3.x = num29;
							flag3 = true;
						}
						else if (localPosition3.x < 0f - num30)
						{
							localPosition3.x = 0f - num30;
							flag3 = true;
						}
					}
					if (flag3)
					{
						dAZPhysicsMeshSoftVerticesSet4.jointTransform.localPosition = localPosition3;
					}
				}
				if (useJointBackForce && _jointBackForce > 0f)
				{
					Vector3 vector8 = dAZPhysicsMeshSoftVerticesSet4.jointTransform.position - dAZPhysicsMeshSoftVerticesSet4.jointTargetPosition - vector2;
					float num31 = Mathf.Abs(vector8.x) + Mathf.Abs(vector8.y) + Mathf.Abs(vector8.z);
					if (num31 > num2)
					{
						float num32 = Mathf.Clamp01((num31 - num2) * num);
						vector += vector8 * num32;
					}
				}
			}
		}
		else
		{
			for (int m = 0; m < _softVerticesSets.Count; m++)
			{
				DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet5 = _softVerticesSets[m];
				Vector3 vector9 = ((!centerBetweenTargetAndAnchor) ? _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet5.targetVertex] : ((_skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet5.targetVertex] + _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet5.anchorVertex]) * 0.5f));
				Quaternion identity2 = Quaternion.identity;
				if (lookAtOption == LookAtOption.NormalReference || dAZPhysicsMeshSoftVerticesSet5.forceLookAtReference)
				{
					identity2.SetLookRotation(_normalReference.position - vector9, _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet5.anchorVertex] - _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet5.targetVertex]);
				}
				else if (lookAtOption == LookAtOption.VertexNormal)
				{
					if (centerBetweenTargetAndAnchor)
					{
						identity2.SetLookRotation((-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet5.targetVertex] - _skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet5.anchorVertex]) * 0.5f);
					}
					else
					{
						identity2.SetLookRotation(-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet5.targetVertex]);
					}
				}
				else if (lookAtOption == LookAtOption.VertexNormalRefUp)
				{
					if (centerBetweenTargetAndAnchor)
					{
						identity2.SetLookRotation((-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet5.targetVertex] - _skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet5.anchorVertex]) * 0.5f, _normalReference.position - vector9);
					}
					else
					{
						identity2.SetLookRotation(-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet5.targetVertex], _normalReference.position - vector9);
					}
				}
				else if (lookAtOption == LookAtOption.VertexNormalAnchorUp)
				{
					if (centerBetweenTargetAndAnchor)
					{
						identity2.SetLookRotation((-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet5.targetVertex] - _skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet5.anchorVertex]) * 0.5f, _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet5.anchorVertex] - _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet5.targetVertex]);
					}
					else
					{
						identity2.SetLookRotation(-_skin.postSkinNormals[dAZPhysicsMeshSoftVerticesSet5.targetVertex], _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet5.anchorVertex] - _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet5.targetVertex]);
					}
				}
				else
				{
					identity2.SetLookRotation(_skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet5.anchorVertex] - _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet5.targetVertex], normalReference.position - vector9);
				}
				dAZPhysicsMeshSoftVerticesSet5.kinematicTransform.SetPositionAndRotation(vector9 + vector2, identity2);
				if (!_useUniformLimit)
				{
					Vector3 localPosition4 = dAZPhysicsMeshSoftVerticesSet5.jointTransform.localPosition;
					bool flag4 = false;
					if (_normalMovementType == MovementType.Limit)
					{
						float num33 = num3 * dAZPhysicsMeshSoftVerticesSet5.limitMultiplier;
						float num34 = num4 * dAZPhysicsMeshSoftVerticesSet5.limitMultiplier;
						if (lookAtOption != 0)
						{
							if (localPosition4.z > num33)
							{
								localPosition4.z = num33;
								flag4 = true;
							}
							else if (localPosition4.z < 0f - num34)
							{
								localPosition4.z = 0f - num34;
								flag4 = true;
							}
						}
						else if (localPosition4.y > num33)
						{
							localPosition4.y = num33;
							flag4 = true;
						}
						else if (localPosition4.y < 0f - num34)
						{
							localPosition4.y = 0f - num34;
							flag4 = true;
						}
					}
					if (_tangentMovementType == MovementType.Limit)
					{
						float num35 = num5 * dAZPhysicsMeshSoftVerticesSet5.limitMultiplier;
						float num36 = num6 * dAZPhysicsMeshSoftVerticesSet5.limitMultiplier;
						if (lookAtOption != 0)
						{
							if (localPosition4.y > num35)
							{
								localPosition4.y = num35;
								flag4 = true;
							}
							else if (localPosition4.y < 0f - num36)
							{
								localPosition4.y = 0f - num36;
								flag4 = true;
							}
						}
						else if (localPosition4.z > num35)
						{
							localPosition4.z = num35;
							flag4 = true;
						}
						else if (localPosition4.z < 0f - num36)
						{
							localPosition4.z = 0f - num36;
							flag4 = true;
						}
					}
					if (_tangent2MovementType == MovementType.Limit)
					{
						float num37 = num7 * dAZPhysicsMeshSoftVerticesSet5.limitMultiplier;
						float num38 = num8 * dAZPhysicsMeshSoftVerticesSet5.limitMultiplier;
						if (localPosition4.x > num37)
						{
							localPosition4.x = num37;
							flag4 = true;
						}
						else if (localPosition4.x < 0f - num38)
						{
							localPosition4.x = 0f - num38;
							flag4 = true;
						}
					}
					if (flag4)
					{
						dAZPhysicsMeshSoftVerticesSet5.jointTransform.localPosition = localPosition4;
					}
				}
				if (useJointBackForce && _jointBackForce > 0f)
				{
					Vector3 vector10 = dAZPhysicsMeshSoftVerticesSet5.jointTransform.position - vector9;
					float num39 = Mathf.Abs(vector10.x) + Mathf.Abs(vector10.y) + Mathf.Abs(vector10.z);
					if (num39 > num2)
					{
						float num40 = Mathf.Clamp01((num39 - num2) * num);
						vector += vector10 * num40;
					}
				}
			}
		}
		if (useJointBackForce && _jointBackForce > 0f && backForceRigidbody != null)
		{
			float num41 = ((!(TimeControl.singleton != null) || !TimeControl.singleton.compensateFixedTimestep) ? 1f : ((!Mathf.Approximately(Time.timeScale, 0f)) ? 1f : (1f / Time.timeScale)));
			Vector3 vector11 = vector * _jointBackForce * num41;
			_appliedBackForce = Vector3.ClampMagnitude(vector11, _jointBackForceMaxForce * scale);
		}
	}

	public void ResetAdjustJoints()
	{
		if (wasInit && backForceAdjustJoints != null)
		{
			if (backForceAdjustJointsUseJoint2)
			{
				backForceAdjustJoints.additionalJoint2RotationX = 0f;
				backForceAdjustJoints.additionalJoint2RotationY = 0f;
				backForceAdjustJoints.additionalJoint2RotationZ = 0f;
			}
			else
			{
				backForceAdjustJoints.additionalJoint1RotationX = 0f;
				backForceAdjustJoints.additionalJoint1RotationY = 0f;
				backForceAdjustJoints.additionalJoint1RotationZ = 0f;
			}
		}
	}

	public void ApplyBackForce()
	{
		if (!wasInit || !useJointBackForce || !(_jointBackForce > 0f) || !(backForceRigidbody != null))
		{
			return;
		}
		float num = Time.fixedDeltaTime * 90f;
		_bufferedBackForce = Vector3.Lerp(_bufferedBackForce, _appliedBackForce, backForceResponse * num);
		if (backForceAdjustJoints != null)
		{
			Vector3 vector = backForceRigidbody.transform.InverseTransformVector(_bufferedBackForce);
			if (backForceAdjustJointsUseJoint2)
			{
				backForceAdjustJoints.additionalJoint2RotationX += vector.y;
				backForceAdjustJoints.additionalJoint2RotationX = Mathf.Clamp(backForceAdjustJoints.additionalJoint2RotationX, 0f - backForceAdjustJointsMaxAngle, backForceAdjustJointsMaxAngle);
				backForceAdjustJoints.additionalJoint2RotationY -= vector.x;
				backForceAdjustJoints.additionalJoint2RotationY = Mathf.Clamp(backForceAdjustJoints.additionalJoint2RotationY, 0f - backForceAdjustJointsMaxAngle, backForceAdjustJointsMaxAngle);
			}
			else
			{
				backForceAdjustJoints.additionalJoint1RotationX += vector.y;
				backForceAdjustJoints.additionalJoint1RotationX = Mathf.Clamp(backForceAdjustJoints.additionalJoint1RotationX, 0f - backForceAdjustJointsMaxAngle, backForceAdjustJointsMaxAngle);
				backForceAdjustJoints.additionalJoint1RotationY -= vector.x;
				backForceAdjustJoints.additionalJoint1RotationY = Mathf.Clamp(backForceAdjustJoints.additionalJoint1RotationY, 0f - backForceAdjustJointsMaxAngle, backForceAdjustJointsMaxAngle);
			}
			backForceAdjustJoints.SyncTargetRotation();
		}
		else
		{
			backForceRigidbody.AddForce(_bufferedBackForce, ForceMode.Force);
		}
	}

	public void PrepareMorphVerticesThreaded(float interpFactor)
	{
		if (!wasInit || !(_normalReference != null) || !_on || _freeze)
		{
			return;
		}
		bool flag = _useSimulation && _useInterpolation && useCustomInterpolation;
		for (int i = 0; i < _softVerticesSets.Count; i++)
		{
			DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = _softVerticesSets[i];
			if (dAZPhysicsMeshSoftVerticesSet.influenceVertices.Length > 0 && _influenceType == InfluenceType.DistanceAlongMoveVector)
			{
				if (lookAtOption == LookAtOption.Anchor)
				{
					dAZPhysicsMeshSoftVerticesSet.primaryMove = dAZPhysicsMeshSoftVerticesSet.kinematicTransform.forward;
				}
				else
				{
					dAZPhysicsMeshSoftVerticesSet.primaryMove = dAZPhysicsMeshSoftVerticesSet.kinematicTransform.up;
				}
			}
			if (flag)
			{
				Vector3 vector = Vector3.Lerp(dAZPhysicsMeshSoftVerticesSet.lastPosition, dAZPhysicsMeshSoftVerticesSet.jointRB.position, interpFactor);
				Vector3 vector2 = Vector3.Lerp(dAZPhysicsMeshSoftVerticesSet.lastKinematicPosition, dAZPhysicsMeshSoftVerticesSet.kinematicTransform.position, interpFactor);
				dAZPhysicsMeshSoftVerticesSet.move = vector - vector2;
			}
			else
			{
				dAZPhysicsMeshSoftVerticesSet.move = dAZPhysicsMeshSoftVerticesSet.jointTransform.position - dAZPhysicsMeshSoftVerticesSet.kinematicTransform.position;
			}
		}
	}

	public void PrepareMorphVerticesThreadedFast(float interpFactor)
	{
		if (!wasInit || !(_normalReference != null) || !_on || _freeze)
		{
			return;
		}
		bool flag = _useSimulation && _useInterpolation && useCustomInterpolation;
		lastSkinTime = skinTime;
		if (_useInterpolation)
		{
			skinTime = Time.time;
		}
		else
		{
			skinTime = Time.fixedTime + Time.fixedDeltaTime;
		}
		skinTimeDelta = skinTime - lastSkinTime;
		if (skinTimeDelta <= 0f)
		{
			skinTimeDelta = Time.fixedDeltaTime;
		}
		for (int i = 0; i < _softVerticesSets.Count; i++)
		{
			DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = _softVerticesSets[i];
			if (dAZPhysicsMeshSoftVerticesSet.influenceVertices.Length > 0 && _influenceType == InfluenceType.DistanceAlongMoveVector)
			{
				if (lookAtOption == LookAtOption.Anchor)
				{
					dAZPhysicsMeshSoftVerticesSet.primaryMove = dAZPhysicsMeshSoftVerticesSet.kinematicTransform.forward;
				}
				else
				{
					dAZPhysicsMeshSoftVerticesSet.primaryMove = dAZPhysicsMeshSoftVerticesSet.kinematicTransform.up;
				}
			}
			if (flag)
			{
				dAZPhysicsMeshSoftVerticesSet.lastKinematicPositionThreaded = dAZPhysicsMeshSoftVerticesSet.lastKinematicPosition;
				dAZPhysicsMeshSoftVerticesSet.lastPositionThreaded = dAZPhysicsMeshSoftVerticesSet.lastPosition;
				dAZPhysicsMeshSoftVerticesSet.currentKinematicPosition = dAZPhysicsMeshSoftVerticesSet.kinematicTransform.position;
				dAZPhysicsMeshSoftVerticesSet.currentPosition = dAZPhysicsMeshSoftVerticesSet.jointRB.position;
				dAZPhysicsMeshSoftVerticesSet.interpFactor = interpFactor;
			}
			else
			{
				dAZPhysicsMeshSoftVerticesSet.move = dAZPhysicsMeshSoftVerticesSet.jointTransform.position - dAZPhysicsMeshSoftVerticesSet.kinematicTransform.position;
			}
		}
	}

	public void MorphVerticesThreadedFast(Vector3[] verts)
	{
		if (!wasInit || !_on || _freeze)
		{
			return;
		}
		bool flag = _useSimulation && _useInterpolation && useCustomInterpolation;
		float num = 1f / _maxInfluenceDistance;
		bool flag2 = lookAtOption == LookAtOption.VertexNormal || lookAtOption == LookAtOption.VertexNormalRefUp || lookAtOption == LookAtOption.VertexNormalAnchorUp;
		for (int i = 0; i < _softVerticesSets.Count; i++)
		{
			DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = _softVerticesSets[i];
			Vector3 vector = ((!centerBetweenTargetAndAnchor) ? verts[dAZPhysicsMeshSoftVerticesSet.targetVertex] : ((verts[dAZPhysicsMeshSoftVerticesSet.targetVertex] + verts[dAZPhysicsMeshSoftVerticesSet.anchorVertex]) * 0.5f));
			if (dAZPhysicsMeshSoftVerticesSet.influenceVertices.Length > 0 && _influenceType != InfluenceType.HardCopy)
			{
				if (_influenceType == InfluenceType.DistanceAlongMoveVector)
				{
					for (int j = 0; j < dAZPhysicsMeshSoftVerticesSet.influenceVertices.Length; j++)
					{
						Vector3 rhs = (verts[dAZPhysicsMeshSoftVerticesSet.influenceVertices[j]] - vector) * oneoverscale;
						Vector3 vector2 = dAZPhysicsMeshSoftVerticesSet.primaryMove * Vector3.Dot(dAZPhysicsMeshSoftVerticesSet.primaryMove, rhs);
						dAZPhysicsMeshSoftVerticesSet.influenceVerticesDistances[j] = vector2.magnitude;
					}
				}
				else
				{
					for (int k = 0; k < dAZPhysicsMeshSoftVerticesSet.influenceVertices.Length; k++)
					{
						Vector3 vector3 = (verts[dAZPhysicsMeshSoftVerticesSet.influenceVertices[k]] - vector) * oneoverscale;
						dAZPhysicsMeshSoftVerticesSet.influenceVerticesDistances[k] = vector3.magnitude;
					}
				}
			}
			if (!_skin.postSkinVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex])
			{
				_skin.postSkinVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex] = true;
				_skin.postSkinVertsChanged = true;
			}
			if (flag2 && !_skin.postSkinNormalVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex])
			{
				_skin.postSkinNormalVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex] = true;
				_skin.postSkinVertsChanged = true;
			}
			if (!_skin.postSkinVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex])
			{
				_skin.postSkinVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex] = true;
				_skin.postSkinVertsChanged = true;
			}
			if (flag)
			{
				Vector3 vector4 = Vector3.Lerp(dAZPhysicsMeshSoftVerticesSet.lastPositionThreaded, dAZPhysicsMeshSoftVerticesSet.currentPosition, dAZPhysicsMeshSoftVerticesSet.interpFactor);
				Vector3 vector5 = Vector3.Lerp(dAZPhysicsMeshSoftVerticesSet.lastKinematicPositionThreaded, dAZPhysicsMeshSoftVerticesSet.currentKinematicPosition, dAZPhysicsMeshSoftVerticesSet.interpFactor);
				dAZPhysicsMeshSoftVerticesSet.move = vector4 - vector5;
			}
			_skin.postSkinMorphs[dAZPhysicsMeshSoftVerticesSet.targetVertex] = dAZPhysicsMeshSoftVerticesSet.move;
			if (_influenceType == InfluenceType.HardCopy)
			{
				if (autoInfluenceAnchor)
				{
					_skin.postSkinMorphs[dAZPhysicsMeshSoftVerticesSet.anchorVertex] = dAZPhysicsMeshSoftVerticesSet.move;
				}
				for (int l = 0; l < dAZPhysicsMeshSoftVerticesSet.influenceVertices.Length; l++)
				{
					int num2 = dAZPhysicsMeshSoftVerticesSet.influenceVertices[l];
					if (!_skin.postSkinVerts[num2])
					{
						_skin.postSkinVerts[num2] = true;
						_skin.postSkinVertsChanged = true;
					}
					_skin.postSkinMorphs[num2] = dAZPhysicsMeshSoftVerticesSet.move;
				}
				continue;
			}
			for (int m = 0; m < dAZPhysicsMeshSoftVerticesSet.influenceVertices.Length; m++)
			{
				int num3 = dAZPhysicsMeshSoftVerticesSet.influenceVertices[m];
				float num4 = dAZPhysicsMeshSoftVerticesSet.influenceVerticesDistances[m];
				float f = Mathf.Min(1f, num4 * num);
				float num5 = 1f - Mathf.Pow(f, falloffPower);
				num5 = ((!(weightBias > 0f)) ? ((1f + weightBias) * num5) : ((1f - weightBias) * num5 + weightBias));
				dAZPhysicsMeshSoftVerticesSet.influenceVerticesWeights[m] = num5;
				if (!_skin.postSkinVerts[num3])
				{
					_skin.postSkinVerts[num3] = true;
					_skin.postSkinVertsChanged = true;
				}
				_skin.postSkinMorphs[num3] = dAZPhysicsMeshSoftVerticesSet.move * num5 * weightMultiplier;
			}
		}
	}

	public void MorphVerticesThreaded()
	{
		if (!wasInit || !_on || _freeze)
		{
			return;
		}
		float num = 1f / _maxInfluenceDistance;
		bool flag = lookAtOption == LookAtOption.VertexNormal || lookAtOption == LookAtOption.VertexNormalRefUp || lookAtOption == LookAtOption.VertexNormalAnchorUp;
		for (int i = 0; i < _softVerticesSets.Count; i++)
		{
			DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = _softVerticesSets[i];
			Vector3 vector = ((!centerBetweenTargetAndAnchor) ? _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex] : ((_skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex] + _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex]) * 0.5f));
			if (dAZPhysicsMeshSoftVerticesSet.influenceVertices.Length > 0 && _influenceType != InfluenceType.HardCopy)
			{
				if (_influenceType == InfluenceType.DistanceAlongMoveVector)
				{
					for (int j = 0; j < dAZPhysicsMeshSoftVerticesSet.influenceVertices.Length; j++)
					{
						Vector3 rhs = (_skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.influenceVertices[j]] - vector) * oneoverscale;
						Vector3 vector2 = dAZPhysicsMeshSoftVerticesSet.primaryMove * Vector3.Dot(dAZPhysicsMeshSoftVerticesSet.primaryMove, rhs);
						dAZPhysicsMeshSoftVerticesSet.influenceVerticesDistances[j] = vector2.magnitude;
					}
				}
				else
				{
					for (int k = 0; k < dAZPhysicsMeshSoftVerticesSet.influenceVertices.Length; k++)
					{
						Vector3 vector3 = (_skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.influenceVertices[k]] - vector) * oneoverscale;
						dAZPhysicsMeshSoftVerticesSet.influenceVerticesDistances[k] = vector3.magnitude;
					}
				}
			}
			if (!_skin.postSkinVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex])
			{
				_skin.postSkinVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex] = true;
				_skin.postSkinVertsChanged = true;
			}
			if (flag && !_skin.postSkinNormalVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex])
			{
				_skin.postSkinNormalVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex] = true;
				_skin.postSkinVertsChanged = true;
			}
			if (!_skin.postSkinVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex])
			{
				_skin.postSkinVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex] = true;
				_skin.postSkinVertsChanged = true;
			}
			_skin.postSkinMorphs[dAZPhysicsMeshSoftVerticesSet.targetVertex] = dAZPhysicsMeshSoftVerticesSet.move;
			if (_influenceType == InfluenceType.HardCopy)
			{
				if (autoInfluenceAnchor)
				{
					_skin.postSkinMorphs[dAZPhysicsMeshSoftVerticesSet.anchorVertex] = dAZPhysicsMeshSoftVerticesSet.move;
				}
				for (int l = 0; l < dAZPhysicsMeshSoftVerticesSet.influenceVertices.Length; l++)
				{
					int num2 = dAZPhysicsMeshSoftVerticesSet.influenceVertices[l];
					if (!_skin.postSkinVerts[num2])
					{
						_skin.postSkinVerts[num2] = true;
						_skin.postSkinVertsChanged = true;
					}
					_skin.postSkinMorphs[num2] = dAZPhysicsMeshSoftVerticesSet.move;
				}
				continue;
			}
			for (int m = 0; m < dAZPhysicsMeshSoftVerticesSet.influenceVertices.Length; m++)
			{
				int num3 = dAZPhysicsMeshSoftVerticesSet.influenceVertices[m];
				float num4 = dAZPhysicsMeshSoftVerticesSet.influenceVerticesDistances[m];
				float f = Mathf.Min(1f, num4 * num);
				float num5 = 1f - Mathf.Pow(f, falloffPower);
				num5 = ((!(weightBias > 0f)) ? ((1f + weightBias) * num5) : ((1f - weightBias) * num5 + weightBias));
				dAZPhysicsMeshSoftVerticesSet.influenceVerticesWeights[m] = num5;
				if (!_skin.postSkinVerts[num3])
				{
					_skin.postSkinVerts[num3] = true;
					_skin.postSkinVertsChanged = true;
				}
				_skin.postSkinMorphs[num3] = dAZPhysicsMeshSoftVerticesSet.move * num5 * weightMultiplier;
			}
		}
	}

	public void MorphVertices(float interpFactor)
	{
		if (!wasInit || !(_normalReference != null) || !_on || _freeze)
		{
			return;
		}
		float num = 1f / _maxInfluenceDistance;
		bool flag = lookAtOption == LookAtOption.VertexNormal || lookAtOption == LookAtOption.VertexNormalRefUp || lookAtOption == LookAtOption.VertexNormalAnchorUp;
		bool flag2 = _useSimulation && useCustomInterpolation;
		for (int i = 0; i < _softVerticesSets.Count; i++)
		{
			DAZPhysicsMeshSoftVerticesSet dAZPhysicsMeshSoftVerticesSet = _softVerticesSets[i];
			Vector3 vector = ((!centerBetweenTargetAndAnchor) ? _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex] : ((_skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex] + _skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex]) * 0.5f));
			if (dAZPhysicsMeshSoftVerticesSet.influenceVertices.Length > 0 && _influenceType != InfluenceType.HardCopy)
			{
				if (_influenceType == InfluenceType.DistanceAlongMoveVector)
				{
					Vector3 vector2 = ((lookAtOption != 0) ? dAZPhysicsMeshSoftVerticesSet.kinematicTransform.up : dAZPhysicsMeshSoftVerticesSet.kinematicTransform.forward);
					for (int j = 0; j < dAZPhysicsMeshSoftVerticesSet.influenceVertices.Length; j++)
					{
						Vector3 rhs = (_skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.influenceVertices[j]] - vector) * oneoverscale;
						Vector3 vector3 = vector2 * Vector3.Dot(vector2, rhs);
						dAZPhysicsMeshSoftVerticesSet.influenceVerticesDistances[j] = vector3.magnitude;
					}
				}
				else
				{
					for (int k = 0; k < dAZPhysicsMeshSoftVerticesSet.influenceVertices.Length; k++)
					{
						Vector3 vector4 = (_skin.rawSkinnedVerts[dAZPhysicsMeshSoftVerticesSet.influenceVertices[k]] - vector) * oneoverscale;
						dAZPhysicsMeshSoftVerticesSet.influenceVerticesDistances[k] = vector4.magnitude;
					}
				}
			}
			Vector3 vector6;
			if (flag2)
			{
				Vector3 vector5 = Vector3.Lerp(dAZPhysicsMeshSoftVerticesSet.lastPosition, dAZPhysicsMeshSoftVerticesSet.jointRB.position, interpFactor);
				vector6 = vector5 - vector;
			}
			else
			{
				vector6 = dAZPhysicsMeshSoftVerticesSet.jointTransform.position - vector;
			}
			if (!_skin.postSkinVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex])
			{
				_skin.postSkinVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex] = true;
				_skin.postSkinVertsChanged = true;
			}
			if (flag)
			{
				if (!_skin.postSkinNormalVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex])
				{
					_skin.postSkinNormalVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex] = true;
					_skin.postSkinVertsChanged = true;
				}
				if (centerBetweenTargetAndAnchor && !_skin.postSkinNormalVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex])
				{
					_skin.postSkinNormalVerts[dAZPhysicsMeshSoftVerticesSet.anchorVertex] = true;
				}
			}
			if (!_skin.postSkinVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex])
			{
				_skin.postSkinVerts[dAZPhysicsMeshSoftVerticesSet.targetVertex] = true;
				_skin.postSkinVertsChanged = true;
			}
			_skin.postSkinMorphs[dAZPhysicsMeshSoftVerticesSet.targetVertex] = vector6;
			if (_influenceType == InfluenceType.HardCopy)
			{
				if (autoInfluenceAnchor)
				{
					_skin.postSkinMorphs[dAZPhysicsMeshSoftVerticesSet.anchorVertex] = vector6;
				}
				for (int l = 0; l < dAZPhysicsMeshSoftVerticesSet.influenceVertices.Length; l++)
				{
					int num2 = dAZPhysicsMeshSoftVerticesSet.influenceVertices[l];
					if (!_skin.postSkinVerts[num2])
					{
						_skin.postSkinVerts[num2] = true;
						_skin.postSkinVertsChanged = true;
					}
					_skin.postSkinMorphs[num2] = vector6;
				}
				continue;
			}
			for (int m = 0; m < dAZPhysicsMeshSoftVerticesSet.influenceVertices.Length; m++)
			{
				int num3 = dAZPhysicsMeshSoftVerticesSet.influenceVertices[m];
				float num4 = dAZPhysicsMeshSoftVerticesSet.influenceVerticesDistances[m];
				float f = Mathf.Min(1f, num4 * num);
				float num5 = 1f - Mathf.Pow(f, falloffPower);
				num5 = ((!(weightBias > 0f)) ? ((1f + weightBias) * num5) : ((1f - weightBias) * num5 + weightBias));
				dAZPhysicsMeshSoftVerticesSet.influenceVerticesWeights[m] = num5;
				if (!_skin.postSkinVerts[num3])
				{
					_skin.postSkinVerts[num3] = true;
					_skin.postSkinVertsChanged = true;
				}
				_skin.postSkinMorphs[num3] = vector6 * num5 * weightMultiplier;
			}
		}
	}
}
