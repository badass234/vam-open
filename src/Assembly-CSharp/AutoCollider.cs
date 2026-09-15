using System.Collections.Generic;
using GPUTools.Physics.Scripts.Behaviours;
using UnityEngine;

[ExecuteInEditMode]
public class AutoCollider : PhysicsSimulator
{
	public enum JointType
	{
		Configurable,
		Spring
	}

	public enum SoftJointType
	{
		FloatingKinematic,
		Direct
	}

	public enum ColliderOrient
	{
		Look,
		Up,
		Right
	}

	public enum ColliderType
	{
		Capsule,
		Sphere,
		Box
	}

	public enum ResizeTrigger
	{
		MorphChangeOnly,
		None,
		Always
	}

	public enum LookAtOption
	{
		VertexNormal,
		Anchor1,
		Opposite,
		AnchorCenters,
		Reference
	}

	public enum PositionUpdateTrigger
	{
		MorphChangeOnly,
		None,
		Always
	}

	public static bool globalEnable = true;

	public bool ignoreGroupSettings;

	public bool skipResetOnPause;

	public int targetVertex = -1;

	public int anchorVertex1 = -1;

	public int anchorVertex2 = -1;

	public int oppositeVertex = -1;

	public Transform reference;

	public Transform otherTransformToReset;

	public Transform kinematicTransform;

	public Rigidbody kinematicRB;

	public Transform jointTransform;

	public Rigidbody jointRB;

	public SpringJoint springJoint;

	public ConfigurableJoint joint;

	public Collider jointCollider;

	public Transform hardTransform;

	public Collider hardCollider;

	public bool debug;

	public bool morphVertex;

	public bool allowBatchUpdate = true;

	protected bool jointTransformInitialRotationWasInit;

	public Quaternion jointTransformInitialRotation;

	protected Vector3 currentTargetVertexPosition;

	protected Vector3 currentAnchorVertex1Position;

	protected Vector3 currentAnchorVertex2Position;

	protected Vector3 currentOppositePosition;

	protected bool _globalOn;

	protected bool _on = true;

	[SerializeField]
	protected bool _createSoftCollider = true;

	[SerializeField]
	protected JointType _jointType;

	[SerializeField]
	protected SoftJointType _softJointType;

	[SerializeField]
	protected bool _createHardCollider = true;

	[SerializeField]
	protected float _softJointLimit;

	[SerializeField]
	protected float _softJointLimitSpring;

	[SerializeField]
	protected float _softJointLimitDamper;

	[SerializeField]
	private float _jointSpringLook = 1000f;

	[SerializeField]
	private float _jointDamperLook = 100f;

	[SerializeField]
	private float _jointSpringUp = 1000f;

	[SerializeField]
	private float _jointDamperUp = 100f;

	[SerializeField]
	private float _jointSpringRight = 1000f;

	[SerializeField]
	private float _jointDamperRight = 100f;

	[SerializeField]
	private float _jointSpringMaxForce = 1E+18f;

	[SerializeField]
	private float _jointMass = 0.1f;

	[SerializeField]
	private ColliderOrient _colliderOrient;

	[SerializeField]
	private ColliderType _colliderType;

	[SerializeField]
	private float _colliderRadius = 0.003f;

	[SerializeField]
	private float _colliderLength = 0.003f;

	public ResizeTrigger resizeTrigger;

	[SerializeField]
	private bool _useAutoRadius;

	[SerializeField]
	private bool _useAutoLength;

	[SerializeField]
	private LookAtOption _lookAtOption;

	[SerializeField]
	private bool _useAnchor2AsUp;

	[SerializeField]
	private float _autoRadiusMultiplier = 1f;

	[SerializeField]
	private float _autoRadiusBuffer = 0.001f;

	[SerializeField]
	private float _autoLengthBuffer = 0.001f;

	[SerializeField]
	protected bool _centerJoint;

	[SerializeField]
	private float _colliderLookOffset;

	[SerializeField]
	private float _colliderUpOffset;

	[SerializeField]
	private float _colliderRightOffset;

	[SerializeField]
	private string _colliderLayer;

	public PositionUpdateTrigger hardPositionUpdateTrigger;

	public bool updateHardColliderSize;

	[SerializeField]
	protected float _hardColliderBuffer = 0.01f;

	[SerializeField]
	private Transform[] _ignoreColliders;

	[SerializeField]
	private PhysicMaterial _colliderMaterial;

	public Rigidbody backForceRigidbody;

	[SerializeField]
	private float _jointBackForce = 1000f;

	[SerializeField]
	private float _jointBackForceThresholdDistance = 0.001f;

	[SerializeField]
	private float _jointBackForceMaxForce = 100f;

	[SerializeField]
	protected Transform _skinTransform;

	[SerializeField]
	protected DAZSkinV2 _skin;

	public FreeControllerV3 controller;

	public bool showHandles = true;

	public bool showBackfaceHandles;

	[SerializeField]
	protected bool _showUsedVerts = true;

	public float handleSize = 0.001f;

	public int subMeshSelection = -1;

	public int subMeshSelection2 = -1;

	protected Dictionary<int, int> _uvVertToBaseVertDict;

	private bool wasInit;

	protected Vector3 _bufferedBackForce;

	protected Vector3 _appliedBackForce;

	public DAZBone bone;

	public Vector3 anchorTarget;

	public Vector3 transformedAnchorTarget;

	public bool transformedAnchorTargetDirty;

	protected Vector3 hardPositionTarget;

	public DAZBone hardParent;

	protected bool applyBackForce;

	protected float oneoverscale = 1f;

	public bool colliderDirty;

	protected const float sizeAdjustThreshold = 0.001f;

	protected bool morphsChanged;

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

	public bool createSoftCollider
	{
		get
		{
			return _createSoftCollider;
		}
		set
		{
			if (_createSoftCollider != value)
			{
				_createSoftCollider = value;
				CreateColliders();
			}
		}
	}

	public JointType jointType
	{
		get
		{
			return _jointType;
		}
		set
		{
			if (_jointType != value)
			{
				_jointType = value;
				CreateColliders();
			}
		}
	}

	public SoftJointType softJointType
	{
		get
		{
			return _softJointType;
		}
		set
		{
			if (_softJointType != value)
			{
				_softJointType = value;
				CreateColliders();
			}
		}
	}

	public bool createHardCollider
	{
		get
		{
			return _createHardCollider;
		}
		set
		{
			if (_createHardCollider != value)
			{
				_createHardCollider = value;
				CreateColliders();
			}
		}
	}

	public float softJointLimit
	{
		get
		{
			return _softJointLimit;
		}
		set
		{
			if (_softJointLimit != value)
			{
				_softJointLimit = value;
				SyncJointParams();
			}
		}
	}

	public float softJointLimitSpring
	{
		get
		{
			return _softJointLimitSpring;
		}
		set
		{
			if (_softJointLimitSpring != value)
			{
				_softJointLimitSpring = value;
				SyncJointParams();
			}
		}
	}

	public float softJointLimitDamper
	{
		get
		{
			return _softJointLimitDamper;
		}
		set
		{
			if (_softJointLimitDamper != value)
			{
				_softJointLimitDamper = value;
				SyncJointParams();
			}
		}
	}

	public float jointSpringLook
	{
		get
		{
			return _jointSpringLook;
		}
		set
		{
			if (_jointSpringLook != value)
			{
				_jointSpringLook = value;
				SyncJointParams();
			}
		}
	}

	public float jointDamperLook
	{
		get
		{
			return _jointDamperLook;
		}
		set
		{
			if (_jointDamperLook != value)
			{
				_jointDamperLook = value;
				SyncJointParams();
			}
		}
	}

	public float jointSpringUp
	{
		get
		{
			return _jointSpringUp;
		}
		set
		{
			if (_jointSpringUp != value)
			{
				_jointSpringUp = value;
				SyncJointParams();
			}
		}
	}

	public float jointDamperUp
	{
		get
		{
			return _jointDamperUp;
		}
		set
		{
			if (_jointDamperUp != value)
			{
				_jointDamperUp = value;
				SyncJointParams();
			}
		}
	}

	public float jointSpringRight
	{
		get
		{
			return _jointSpringRight;
		}
		set
		{
			if (_jointSpringRight != value)
			{
				_jointSpringRight = value;
				SyncJointParams();
			}
		}
	}

	public float jointDamperRight
	{
		get
		{
			return _jointDamperRight;
		}
		set
		{
			if (_jointDamperRight != value)
			{
				_jointDamperRight = value;
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
				AutoColliderSizeSet();
				SetColliders();
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
				if (!Application.isPlaying)
				{
					_colliderType = value;
				}
				else
				{
					Debug.LogWarning("Cannot change colliderType at runtime");
				}
			}
		}
	}

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
				SetColliders();
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
				SetColliders();
			}
		}
	}

	public bool useAutoRadius
	{
		get
		{
			return _useAutoRadius;
		}
		set
		{
			if (_useAutoRadius != value)
			{
				_useAutoRadius = value;
				AutoColliderSizeSet();
			}
		}
	}

	public bool useAutoLength
	{
		get
		{
			return _useAutoLength;
		}
		set
		{
			if (_useAutoLength != value)
			{
				_useAutoLength = value;
				AutoColliderSizeSet();
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
				UpdateTransforms();
				AutoColliderSizeSet();
				SetColliders();
			}
		}
	}

	public bool useAnchor2AsUp
	{
		get
		{
			return _useAnchor2AsUp;
		}
		set
		{
			if (_useAnchor2AsUp != value)
			{
				_useAnchor2AsUp = value;
				UpdateTransforms();
			}
		}
	}

	public float autoRadiusMultiplier
	{
		get
		{
			return _autoRadiusMultiplier;
		}
		set
		{
			if (_autoRadiusMultiplier != value)
			{
				_autoRadiusMultiplier = value;
				AutoColliderSizeSet(true);
			}
		}
	}

	public float autoRadiusBuffer
	{
		get
		{
			return _autoRadiusBuffer;
		}
		set
		{
			if (_autoRadiusBuffer != value)
			{
				_autoRadiusBuffer = value;
				AutoColliderSizeSet(true);
			}
		}
	}

	public float autoLengthBuffer
	{
		get
		{
			return _autoLengthBuffer;
		}
		set
		{
			if (_autoLengthBuffer != value)
			{
				_autoLengthBuffer = value;
				AutoColliderSizeSet(true);
			}
		}
	}

	public bool centerJoint
	{
		get
		{
			return _centerJoint;
		}
		set
		{
			if (_centerJoint != value)
			{
				_centerJoint = value;
				UpdateTransforms();
				SetColliders();
			}
		}
	}

	public float colliderLookOffset
	{
		get
		{
			return _colliderLookOffset;
		}
		set
		{
			if (_colliderLookOffset != value)
			{
				_colliderLookOffset = value;
				SetColliders();
			}
		}
	}

	public float colliderUpOffset
	{
		get
		{
			return _colliderUpOffset;
		}
		set
		{
			if (_colliderUpOffset != value)
			{
				_colliderUpOffset = value;
				SetColliders();
			}
		}
	}

	public float colliderRightOffset
	{
		get
		{
			return _colliderRightOffset;
		}
		set
		{
			if (_colliderRightOffset != value)
			{
				_colliderRightOffset = value;
				SetColliders();
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
				if (!Application.isPlaying)
				{
					_colliderLayer = value;
				}
				else
				{
					Debug.LogWarning("Cannot change colliderLayer at runtime");
				}
			}
		}
	}

	public float hardColliderBuffer
	{
		get
		{
			return _hardColliderBuffer;
		}
		set
		{
			if (_hardColliderBuffer != value)
			{
				_hardColliderBuffer = value;
				SetColliders();
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
				SetColliders();
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
			_skin = value;
			_uvVertToBaseVertDict = null;
			if (_skin != null)
			{
				wasInit = false;
				Init();
			}
		}
	}

	public bool showUsedVerts
	{
		get
		{
			return _showUsedVerts;
		}
		set
		{
			if (_showUsedVerts != value)
			{
				_showUsedVerts = value;
			}
		}
	}

	protected Dictionary<int, int> uvVertToBaseVertDict
	{
		get
		{
			if (_uvVertToBaseVertDict == null)
			{
				if (_skin != null && _skin.dazMesh != null)
				{
					_uvVertToBaseVertDict = _skin.dazMesh.uvVertToBaseVert;
				}
				else
				{
					_uvVertToBaseVertDict = new Dictionary<int, int>();
				}
			}
			return _uvVertToBaseVertDict;
		}
	}

	public Vector3 bufferedBackForce => _bufferedBackForce;

	public Vector3 appliedBackForce => _appliedBackForce;

	protected void SyncOn()
	{
		_globalOn = globalEnable;
		if (_on && globalEnable)
		{
			if (softJointType == SoftJointType.FloatingKinematic)
			{
				ResetJointInternal();
			}
			if (jointTransform != null)
			{
				jointTransform.gameObject.SetActive(true);
			}
			return;
		}
		Vector3 zero = Vector3.zero;
		if (jointTransform != null)
		{
			jointTransform.gameObject.SetActive(false);
		}
		if (_skin.wasInit && morphVertex)
		{
			_skin.postSkinMorphs[targetVertex].x = 0f;
			_skin.postSkinMorphs[targetVertex].y = 0f;
			_skin.postSkinMorphs[targetVertex].z = 0f;
		}
	}

	protected override void SyncCollisionEnabled()
	{
		base.SyncCollisionEnabled();
		if (jointRB != null)
		{
			jointRB.detectCollisions = _collisionEnabled && !_resetSimulation;
		}
	}

	protected override void SyncUseInterpolation()
	{
		base.SyncUseInterpolation();
		SyncJointParams();
	}

	public int GetBaseVertex(int vid)
	{
		int value;
		if (_skin != null && _skin.dazMesh != null && uvVertToBaseVertDict.TryGetValue(vid, out value))
		{
			vid = value;
		}
		return vid;
	}

	public bool IsBaseVertex(int vid)
	{
		if (_skin != null && _skin.dazMesh != null)
		{
			return !uvVertToBaseVertDict.ContainsKey(vid);
		}
		return true;
	}

	public void ClickVertex(int vid)
	{
		if (_skin != null && _skin.dazMesh != null)
		{
			int value;
			if (uvVertToBaseVertDict.TryGetValue(vid, out value))
			{
				vid = value;
			}
			if (targetVertex == -1 && anchorVertex1 != vid && anchorVertex2 != vid && oppositeVertex != vid)
			{
				targetVertex = vid;
			}
			else if (targetVertex == vid)
			{
				targetVertex = -1;
			}
			else if (anchorVertex1 == -1 && anchorVertex2 != vid && oppositeVertex != vid)
			{
				anchorVertex1 = vid;
			}
			else if (anchorVertex1 == vid)
			{
				anchorVertex1 = -1;
			}
			else if (anchorVertex2 == -1 && oppositeVertex != vid)
			{
				anchorVertex2 = vid;
			}
			else if (anchorVertex2 == vid)
			{
				anchorVertex2 = -1;
			}
			else if (oppositeVertex == -1)
			{
				oppositeVertex = vid;
			}
			else if (oppositeVertex == vid)
			{
				oppositeVertex = -1;
			}
			UpdateTransforms();
			AutoColliderSizeSet();
			FixNames();
		}
	}

	public void Init()
	{
		if (Application.isPlaying && !wasInit && _skin != null && targetVertex != -1 && anchorVertex1 != -1)
		{
			if (debug)
			{
				Debug.Log("AutoCollider Init() " + base.name);
			}
			wasInit = true;
			_skin.Init();
			_skin.postSkinVerts[targetVertex] = true;
			_skin.postSkinVerts[anchorVertex1] = true;
			if (anchorVertex2 != -1)
			{
				_skin.postSkinVerts[anchorVertex2] = true;
			}
			if (oppositeVertex != -1)
			{
				_skin.postSkinVerts[oppositeVertex] = true;
			}
			if ((_softJointType == SoftJointType.FloatingKinematic || (_createHardCollider && hardPositionUpdateTrigger != PositionUpdateTrigger.None)) && (_lookAtOption == LookAtOption.VertexNormal || (_lookAtOption == LookAtOption.Anchor1 && oppositeVertex == -1)))
			{
				_skin.postSkinNormalVerts[targetVertex] = true;
			}
			if (_softJointType == SoftJointType.Direct && _lookAtOption == LookAtOption.VertexNormal && _centerJoint)
			{
				_skin.postSkinNormalVerts[targetVertex] = true;
			}
			_skin.postSkinVertsChanged = true;
			if (jointTransform != null && !jointTransformInitialRotationWasInit)
			{
				jointTransformInitialRotation = jointTransform.localRotation;
				jointTransformInitialRotationWasInit = true;
			}
			if (bone == null && backForceRigidbody != null)
			{
				bone = backForceRigidbody.GetComponent<DAZBone>();
			}
		}
	}

	private void SyncJointParams()
	{
		SetMass();
		SetInterpolation();
		SetJointLimits();
		SetJointDrive();
		SetColliders();
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
			if (collider != null && collider.gameObject.activeInHierarchy && collider.enabled)
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
		if (!(jointCollider != null))
		{
			return;
		}
		List<Collider> list = new List<Collider>();
		Transform[] array = _ignoreColliders;
		foreach (Transform transform in array)
		{
			GetCollidersRecursive(transform, transform, list);
		}
		foreach (Collider item in list)
		{
			Physics.IgnoreCollision(jointCollider, item);
		}
	}

	private void SetInterpolation()
	{
		if (!(jointRB != null))
		{
			return;
		}
		if (_useInterpolation)
		{
			if (morphVertex)
			{
				jointRB.interpolation = RigidbodyInterpolation.Interpolate;
			}
			else
			{
				jointRB.interpolation = RigidbodyInterpolation.None;
			}
		}
		else
		{
			jointRB.interpolation = RigidbodyInterpolation.None;
		}
	}

	private void SetJointLimits()
	{
		if (_jointType == JointType.Configurable && joint != null)
		{
			if (_softJointLimit != 0f)
			{
				if (_lookAtOption == LookAtOption.Opposite || _lookAtOption == LookAtOption.AnchorCenters)
				{
					if (_colliderOrient == ColliderOrient.Look)
					{
						joint.xMotion = ConfigurableJointMotion.Limited;
						joint.yMotion = ConfigurableJointMotion.Limited;
						joint.zMotion = ConfigurableJointMotion.Limited;
					}
					else
					{
						joint.xMotion = ConfigurableJointMotion.Limited;
						joint.yMotion = ConfigurableJointMotion.Limited;
						joint.zMotion = ConfigurableJointMotion.Limited;
					}
				}
				else
				{
					joint.xMotion = ConfigurableJointMotion.Limited;
					joint.yMotion = ConfigurableJointMotion.Limited;
					joint.zMotion = ConfigurableJointMotion.Limited;
				}
				SoftJointLimit linearLimit = joint.linearLimit;
				linearLimit.limit = _softJointLimit * _scale;
				joint.linearLimit = linearLimit;
				SoftJointLimitSpring linearLimitSpring = joint.linearLimitSpring;
				linearLimitSpring.spring = _softJointLimitSpring * oneoverscale;
				linearLimitSpring.damper = _softJointLimitDamper * oneoverscale;
				joint.linearLimitSpring = linearLimitSpring;
			}
			else
			{
				joint.xMotion = ConfigurableJointMotion.Free;
				joint.yMotion = ConfigurableJointMotion.Free;
				joint.zMotion = ConfigurableJointMotion.Free;
			}
			joint.angularXMotion = ConfigurableJointMotion.Locked;
			joint.angularYMotion = ConfigurableJointMotion.Locked;
			joint.angularZMotion = ConfigurableJointMotion.Locked;
			joint.projectionMode = JointProjectionMode.None;
			joint.projectionDistance = 0.01f;
			joint.projectionAngle = 1f;
		}
		else if (_jointType == JointType.Spring && springJoint != null)
		{
			springJoint.spring = _softJointLimitSpring * oneoverscale;
			springJoint.damper = _softJointLimitDamper * oneoverscale;
			springJoint.tolerance = 0f;
			springJoint.minDistance = 0f;
			springJoint.maxDistance = 0f;
		}
	}

	private void SetJointDrive()
	{
		if (_jointType == JointType.Configurable && joint != null)
		{
			float num = 1f / _scale;
			JointDrive zDrive = default(JointDrive);
			zDrive.positionSpring = _jointSpringLook * num;
			zDrive.positionDamper = _jointDamperLook * num;
			zDrive.maximumForce = _jointSpringMaxForce * num;
			JointDrive xDrive = default(JointDrive);
			xDrive.positionSpring = _jointSpringUp * num;
			xDrive.positionDamper = _jointDamperUp * num;
			xDrive.maximumForce = _jointSpringMaxForce * num;
			JointDrive yDrive = default(JointDrive);
			yDrive.positionSpring = _jointSpringRight * num;
			yDrive.positionDamper = _jointDamperRight * num;
			yDrive.maximumForce = _jointSpringMaxForce * num;
			joint.xDrive = xDrive;
			joint.yDrive = yDrive;
			joint.zDrive = zDrive;
		}
	}

	private void SetColliders()
	{
		int direction = 0;
		Vector3 center = default(Vector3);
		center.x = _colliderRightOffset;
		center.y = _colliderUpOffset;
		center.z = _colliderLookOffset;
		switch (_colliderOrient)
		{
		case ColliderOrient.Look:
			direction = 2;
			if (_lookAtOption == LookAtOption.Anchor1)
			{
				center.y += _colliderRadius;
			}
			else if (!_centerJoint)
			{
				float num = _colliderLength * 0.5f;
				if (num < _colliderRadius)
				{
					num = _colliderRadius;
				}
				center.z += num;
			}
			break;
		case ColliderOrient.Up:
			direction = 1;
			if (!_centerJoint)
			{
				center.z += _colliderRadius;
			}
			break;
		case ColliderOrient.Right:
			direction = 0;
			if (!_centerJoint)
			{
				center.z += _colliderRadius;
			}
			break;
		}
		if (jointCollider != null)
		{
			switch (colliderType)
			{
			case ColliderType.Capsule:
			{
				CapsuleCollider capsuleCollider = jointCollider as CapsuleCollider;
				capsuleCollider.radius = _colliderRadius;
				capsuleCollider.height = _colliderLength;
				capsuleCollider.direction = direction;
				capsuleCollider.center = center;
				CapsuleLineSphereCollider component = capsuleCollider.GetComponent<CapsuleLineSphereCollider>();
				if (component != null)
				{
					component.UpdateData();
				}
				break;
			}
			case ColliderType.Sphere:
			{
				SphereCollider sphereCollider = jointCollider as SphereCollider;
				sphereCollider.radius = _colliderRadius;
				sphereCollider.center = center;
				break;
			}
			case ColliderType.Box:
			{
				BoxCollider boxCollider = jointCollider as BoxCollider;
				float num2 = _colliderRadius * 2f;
				boxCollider.size = new Vector3(num2, num2, num2);
				boxCollider.center = center;
				break;
			}
			}
			if (_colliderMaterial != null)
			{
				jointCollider.sharedMaterial = _colliderMaterial;
			}
		}
		if (!(hardCollider != null) || (Application.isPlaying && !updateHardColliderSize))
		{
			return;
		}
		switch (colliderType)
		{
		case ColliderType.Capsule:
		{
			CapsuleCollider capsuleCollider2 = hardCollider as CapsuleCollider;
			capsuleCollider2.radius = _colliderRadius - hardColliderBuffer;
			capsuleCollider2.height = _colliderLength - hardColliderBuffer * 2f;
			capsuleCollider2.direction = direction;
			capsuleCollider2.center = center;
			CapsuleLineSphereCollider component2 = capsuleCollider2.GetComponent<CapsuleLineSphereCollider>();
			if (component2 != null)
			{
				component2.UpdateData();
			}
			break;
		}
		case ColliderType.Sphere:
		{
			SphereCollider sphereCollider2 = hardCollider as SphereCollider;
			sphereCollider2.radius = _colliderRadius - hardColliderBuffer;
			sphereCollider2.center = center;
			break;
		}
		case ColliderType.Box:
		{
			BoxCollider boxCollider2 = hardCollider as BoxCollider;
			float num3 = (_colliderRadius - hardColliderBuffer) * 2f;
			boxCollider2.size = new Vector3(num3, num3, num3);
			boxCollider2.center = center;
			break;
		}
		}
	}

	private void SetMass()
	{
		if (jointRB != null)
		{
			jointRB.mass = _jointMass;
		}
	}

	protected void FixNames()
	{
		if (jointTransform != null && jointTransform.name != base.name + "Joint")
		{
			jointTransform.name = base.name + "Joint";
		}
		if (kinematicTransform != null && kinematicTransform.name != base.name + "KO")
		{
			kinematicTransform.name = base.name + "KO";
		}
		if (hardTransform != null && hardTransform.name != base.name + "Hard")
		{
			hardTransform.name = base.name + "Hard";
		}
	}

	public void CreateColliders()
	{
		if (Application.isPlaying || targetVertex == -1 || anchorVertex1 == -1)
		{
			return;
		}
		if (_createSoftCollider)
		{
			if (_softJointType == SoftJointType.Direct)
			{
				if (kinematicTransform != null)
				{
					Object.DestroyImmediate(kinematicTransform.gameObject);
					kinematicTransform = null;
					kinematicRB = null;
				}
			}
			else if (kinematicTransform == null)
			{
				GameObject gameObject = new GameObject(base.name + "KO");
				kinematicTransform = gameObject.transform;
				kinematicTransform.SetParent(base.transform);
				kinematicRB = gameObject.AddComponent<Rigidbody>();
				kinematicRB.isKinematic = true;
			}
			else
			{
				kinematicTransform.name = base.name + "KO";
			}
			GameObject gameObject2;
			if (jointTransform == null)
			{
				gameObject2 = new GameObject(base.name + "Joint");
				jointTransform = gameObject2.transform;
			}
			else
			{
				jointTransform.name = base.name + "Joint";
				gameObject2 = jointTransform.gameObject;
			}
			if (jointRB == null)
			{
				jointRB = jointTransform.gameObject.AddComponent<Rigidbody>();
			}
			if (joint == null && _jointType == JointType.Configurable)
			{
				joint = jointTransform.gameObject.AddComponent<ConfigurableJoint>();
				if (springJoint != null)
				{
					Object.DestroyImmediate(springJoint);
					springJoint = null;
				}
				if (controller != null)
				{
					ConfigurableJoint configurableJoint = gameObject2.AddComponent<ConfigurableJoint>();
					configurableJoint.rotationDriveMode = RotationDriveMode.Slerp;
					configurableJoint.autoConfigureConnectedAnchor = false;
					configurableJoint.connectedAnchor = Vector3.zero;
					Rigidbody component = controller.GetComponent<Rigidbody>();
					controller.transform.position = gameObject2.transform.position;
					controller.transform.rotation = gameObject2.transform.rotation;
					if (component != null)
					{
						configurableJoint.connectedBody = component;
					}
					controller.followWhenOffRB = jointRB;
				}
			}
			if (springJoint == null && _jointType == JointType.Spring)
			{
				springJoint = jointTransform.gameObject.AddComponent<SpringJoint>();
				if (joint != null)
				{
					Object.DestroyImmediate(joint);
					joint = null;
				}
			}
			if (jointCollider != null)
			{
				bool flag = false;
				if (jointCollider.GetType() == typeof(CapsuleCollider) && _colliderType != 0)
				{
					flag = true;
				}
				if (jointCollider.GetType() == typeof(SphereCollider) && _colliderType != ColliderType.Sphere)
				{
					flag = true;
				}
				if (jointCollider.GetType() == typeof(BoxCollider) && _colliderType != ColliderType.Box)
				{
					flag = true;
				}
				if (flag)
				{
					Object.DestroyImmediate(jointCollider);
					jointCollider = null;
				}
			}
			if (jointCollider == null)
			{
				switch (colliderType)
				{
				case ColliderType.Capsule:
				{
					CapsuleCollider capsuleCollider = gameObject2.AddComponent<CapsuleCollider>();
					jointCollider = capsuleCollider;
					break;
				}
				case ColliderType.Sphere:
				{
					SphereCollider sphereCollider = gameObject2.AddComponent<SphereCollider>();
					jointCollider = sphereCollider;
					break;
				}
				case ColliderType.Box:
				{
					BoxCollider boxCollider = gameObject2.AddComponent<BoxCollider>();
					jointCollider = boxCollider;
					break;
				}
				}
			}
			jointRB.useGravity = false;
			jointRB.drag = 0.1f;
			jointRB.angularDrag = 0f;
			jointRB.collisionDetectionMode = CollisionDetectionMode.Discrete;
			jointRB.isKinematic = false;
			if (_softJointType == SoftJointType.Direct)
			{
				jointTransform.SetParent(base.transform);
				if (joint != null)
				{
					joint.autoConfigureConnectedAnchor = false;
				}
				if (springJoint != null)
				{
					springJoint.autoConfigureConnectedAnchor = false;
				}
				UpdateTransforms();
				UpdateAnchor();
			}
			else
			{
				UpdateTransforms();
				jointTransform.SetParent(kinematicTransform);
				jointTransform.position = kinematicTransform.position;
				if (joint != null)
				{
					joint.connectedBody = kinematicRB;
					joint.autoConfigureConnectedAnchor = false;
					joint.anchor = Vector3.zero;
					joint.connectedAnchor = Vector3.zero;
				}
				if (springJoint != null)
				{
					springJoint.connectedBody = kinematicRB;
					springJoint.autoConfigureConnectedAnchor = false;
					springJoint.anchor = Vector3.zero;
					springJoint.connectedAnchor = Vector3.zero;
				}
				jointTransform.rotation = kinematicTransform.rotation;
			}
			if (_colliderLayer != null && _colliderLayer != string.Empty)
			{
				if (joint != null)
				{
					joint.gameObject.layer = LayerMask.NameToLayer(_colliderLayer);
				}
				if (springJoint != null)
				{
					springJoint.gameObject.layer = LayerMask.NameToLayer(_colliderLayer);
				}
			}
			else
			{
				if (joint != null)
				{
					joint.gameObject.layer = base.gameObject.layer;
				}
				if (springJoint != null)
				{
					springJoint.gameObject.layer = base.gameObject.layer;
				}
			}
		}
		else
		{
			if (jointTransform != null)
			{
				Object.DestroyImmediate(jointTransform.gameObject);
				jointTransform = null;
				jointRB = null;
				joint = null;
				springJoint = null;
				jointCollider = null;
			}
			if (kinematicTransform != null)
			{
				Object.DestroyImmediate(kinematicTransform.gameObject);
				kinematicTransform = null;
				kinematicRB = null;
			}
		}
		if (_createHardCollider)
		{
			GameObject gameObject3;
			if (hardTransform == null)
			{
				gameObject3 = new GameObject(base.name + "Hard");
				hardTransform = gameObject3.transform;
			}
			else
			{
				gameObject3 = hardTransform.gameObject;
				hardTransform.name = base.name + "Hard";
			}
			hardTransform.SetParent(base.transform);
			UpdateTransforms();
			if (_colliderLayer != null && _colliderLayer != string.Empty)
			{
				gameObject3.layer = LayerMask.NameToLayer(_colliderLayer);
			}
			else
			{
				gameObject3.layer = base.gameObject.layer;
			}
			if (hardCollider != null)
			{
				bool flag2 = false;
				if (hardCollider.GetType() == typeof(CapsuleCollider) && _colliderType != 0)
				{
					flag2 = true;
				}
				if (hardCollider.GetType() == typeof(SphereCollider) && _colliderType != ColliderType.Sphere)
				{
					flag2 = true;
				}
				if (hardCollider.GetType() == typeof(BoxCollider) && _colliderType != ColliderType.Box)
				{
					flag2 = true;
				}
				if (flag2)
				{
					Object.DestroyImmediate(hardCollider);
					hardCollider = null;
				}
			}
			if (hardCollider == null)
			{
				switch (colliderType)
				{
				case ColliderType.Capsule:
				{
					CapsuleCollider capsuleCollider2 = gameObject3.AddComponent<CapsuleCollider>();
					hardCollider = capsuleCollider2;
					break;
				}
				case ColliderType.Sphere:
				{
					SphereCollider sphereCollider2 = gameObject3.AddComponent<SphereCollider>();
					hardCollider = sphereCollider2;
					break;
				}
				case ColliderType.Box:
				{
					BoxCollider boxCollider2 = gameObject3.AddComponent<BoxCollider>();
					hardCollider = boxCollider2;
					break;
				}
				}
			}
		}
		else if (hardTransform != null)
		{
			Object.DestroyImmediate(hardTransform.gameObject);
			hardTransform = null;
			hardCollider = null;
		}
		SetJointLimits();
		SetJointDrive();
		SetColliders();
		SetMass();
		if (jointRB != null)
		{
			jointRB.centerOfMass = Vector3.zero;
		}
		InitColliders();
	}

	public void UpdateAnchorTarget()
	{
		if (_softJointType != SoftJointType.Direct || !(backForceRigidbody != null))
		{
			return;
		}
		bool flag = true;
		Vector3[] array;
		Vector3[] array2;
		if (Application.isPlaying)
		{
			array = _skin.rawSkinnedVerts;
			array2 = _skin.postSkinNormals;
			flag = _skin.postSkinVertsReady[targetVertex];
		}
		else
		{
			array = _skin.dazMesh.morphedUVVertices;
			array2 = _skin.dazMesh.morphedUVNormals;
		}
		if (!flag)
		{
			return;
		}
		if (_centerJoint)
		{
			if (lookAtOption == LookAtOption.Opposite && oppositeVertex != -1)
			{
				anchorTarget = (array[targetVertex] + array[oppositeVertex]) * 0.5f;
			}
			else if (lookAtOption == LookAtOption.AnchorCenters && anchorVertex1 != -1 && anchorVertex2 != -1)
			{
				anchorTarget = (array[anchorVertex1] + array[anchorVertex2]) * 0.5f;
			}
			else
			{
				if (lookAtOption != 0)
				{
					return;
				}
				if (_colliderOrient == ColliderOrient.Look)
				{
					float num = _colliderLength * 0.5f * _scale;
					if (num < _colliderRadius * _scale)
					{
						num = _colliderRadius * _scale;
					}
					anchorTarget = array[targetVertex] + array2[targetVertex] * (0f - num);
				}
				else
				{
					anchorTarget = array[targetVertex] + array2[targetVertex] * (0f - _colliderRadius) * _scale;
				}
			}
		}
		else
		{
			anchorTarget = array[targetVertex];
		}
	}

	protected void UpdateAnchor()
	{
		if (_softJointType == SoftJointType.Direct && backForceRigidbody != null)
		{
			UpdateAnchorTarget();
			if (joint != null)
			{
				joint.connectedAnchor = backForceRigidbody.transform.InverseTransformPoint(anchorTarget);
			}
			if (springJoint != null)
			{
				springJoint.connectedAnchor = backForceRigidbody.transform.InverseTransformPoint(anchorTarget);
			}
		}
	}

	public void UpdateHardTransformPositionFast(Vector3[] verts, Vector3[] norms)
	{
		if (!_createHardCollider || hardPositionUpdateTrigger != 0 || targetVertex == -1 || anchorVertex1 == -1)
		{
			return;
		}
		switch (_lookAtOption)
		{
		case LookAtOption.Opposite:
			if (oppositeVertex != -1)
			{
				if (_centerJoint)
				{
					hardPositionTarget = (verts[targetVertex] + verts[oppositeVertex]) * 0.5f;
				}
				else
				{
					hardPositionTarget = verts[targetVertex];
				}
			}
			break;
		case LookAtOption.VertexNormal:
			if (_centerJoint)
			{
				if (_colliderOrient == ColliderOrient.Look)
				{
					float num = _colliderLength * 0.5f;
					if (num < _colliderRadius)
					{
						num = _colliderRadius;
					}
					hardPositionTarget = verts[targetVertex] + norms[targetVertex] * (0f - num);
				}
				else
				{
					hardPositionTarget = verts[targetVertex] + norms[targetVertex] * (0f - _colliderRadius);
				}
			}
			else
			{
				hardPositionTarget = verts[targetVertex];
			}
			break;
		case LookAtOption.Anchor1:
			hardPositionTarget = verts[targetVertex];
			break;
		case LookAtOption.AnchorCenters:
			if (anchorVertex2 != -1)
			{
				Vector3 vector = (verts[anchorVertex1] + verts[anchorVertex2]) * 0.5f;
				if (_centerJoint)
				{
					hardPositionTarget = vector;
				}
				else
				{
					hardPositionTarget = verts[targetVertex];
				}
			}
			break;
		case LookAtOption.Reference:
			hardPositionTarget = verts[targetVertex];
			break;
		}
	}

	public void UpdateTransforms()
	{
		if (targetVertex == -1 || anchorVertex1 == -1)
		{
			return;
		}
		Transform transform = null;
		bool flag = false;
		if (_createSoftCollider && _softJointType == SoftJointType.Direct)
		{
			if (joint != null && !Application.isPlaying)
			{
				transform = jointTransform;
				joint.connectedBody = null;
			}
			if (springJoint != null && !Application.isPlaying)
			{
				transform = jointTransform;
				springJoint.connectedBody = null;
			}
		}
		else if (kinematicTransform != null)
		{
			transform = kinematicTransform;
		}
		else if (hardTransform != null && (hardPositionUpdateTrigger == PositionUpdateTrigger.Always || (hardPositionUpdateTrigger == PositionUpdateTrigger.MorphChangeOnly && morphsChanged) || !Application.isPlaying))
		{
			transform = hardTransform;
			if (hardParent != null)
			{
				flag = true;
			}
		}
		if (!(transform != null))
		{
			return;
		}
		bool flag2 = true;
		Vector3[] array;
		Vector3[] array2;
		if (Application.isPlaying)
		{
			array = _skin.rawSkinnedVerts;
			array2 = _skin.postSkinNormals;
			flag2 = _skin.postSkinVertsReady[targetVertex];
		}
		else
		{
			array = _skin.dazMesh.morphedUVVertices;
			array2 = _skin.dazMesh.morphedUVNormals;
		}
		if (!flag2)
		{
			return;
		}
		switch (_lookAtOption)
		{
		case LookAtOption.Opposite:
		{
			if (oppositeVertex == -1)
			{
				break;
			}
			if (_centerJoint)
			{
				if (flag)
				{
					transform.localPosition = hardParent.morphedWorldToLocalMatrix.MultiplyPoint3x4((array[targetVertex] + array[oppositeVertex]) * 0.5f);
				}
				else
				{
					transform.position = (array[targetVertex] + array[oppositeVertex]) * 0.5f;
				}
			}
			else if (flag)
			{
				transform.localPosition = hardParent.morphedWorldToLocalMatrix.MultiplyPoint3x4(array[targetVertex]);
			}
			else
			{
				transform.position = array[targetVertex];
			}
			Quaternion rotation = Quaternion.LookRotation(array[oppositeVertex] - array[targetVertex], array[anchorVertex1] - array[targetVertex]);
			transform.rotation = rotation;
			break;
		}
		case LookAtOption.VertexNormal:
		{
			if (_centerJoint)
			{
				if (flag)
				{
					if (_colliderOrient == ColliderOrient.Look)
					{
						float num = _colliderLength * 0.5f;
						if (num < _colliderRadius)
						{
							num = _colliderRadius;
						}
						transform.localPosition = hardParent.morphedWorldToLocalMatrix.MultiplyPoint3x4(array[targetVertex] + array2[targetVertex] * (0f - num));
					}
					else
					{
						transform.localPosition = hardParent.morphedWorldToLocalMatrix.MultiplyPoint3x4(array[targetVertex] + array2[targetVertex] * (0f - _colliderRadius));
					}
				}
				else if (_colliderOrient == ColliderOrient.Look)
				{
					float num2 = _colliderLength * 0.5f;
					if (num2 < _colliderRadius)
					{
						num2 = _colliderRadius;
					}
					transform.position = array[targetVertex] + array2[targetVertex] * (0f - num2);
				}
				else
				{
					transform.position = array[targetVertex] + array2[targetVertex] * (0f - _colliderRadius);
				}
			}
			else if (flag)
			{
				transform.localPosition = hardParent.morphedWorldToLocalMatrix.MultiplyPoint3x4(array[targetVertex]);
			}
			else
			{
				transform.position = array[targetVertex];
			}
			Quaternion rotation;
			if (_useAnchor2AsUp)
			{
				Vector3 upwards = Vector3.Cross(array2[targetVertex], array[anchorVertex2] - array[targetVertex]);
				rotation = Quaternion.LookRotation(-array2[targetVertex], upwards);
			}
			else
			{
				rotation = Quaternion.LookRotation(-array2[targetVertex], array[anchorVertex1] - array[targetVertex]);
			}
			transform.rotation = rotation;
			break;
		}
		case LookAtOption.Anchor1:
		{
			if (flag)
			{
				transform.localPosition = hardParent.morphedWorldToLocalMatrix.MultiplyPoint3x4(array[targetVertex]);
			}
			else
			{
				transform.position = array[targetVertex];
			}
			Quaternion rotation = ((oppositeVertex == -1) ? Quaternion.LookRotation(array[anchorVertex1] - array[targetVertex], -array2[targetVertex]) : Quaternion.LookRotation(array[anchorVertex1] - array[targetVertex], array[oppositeVertex] - array[targetVertex]));
			transform.rotation = rotation;
			break;
		}
		case LookAtOption.AnchorCenters:
		{
			if (anchorVertex2 == -1)
			{
				break;
			}
			Vector3 vector = (array[anchorVertex1] + array[anchorVertex2]) * 0.5f;
			Quaternion rotation;
			if (_centerJoint)
			{
				if (flag)
				{
					transform.localPosition = hardParent.morphedWorldToLocalMatrix.MultiplyPoint3x4(vector);
				}
				else
				{
					transform.position = vector;
				}
				rotation = Quaternion.LookRotation(array[targetVertex] - vector, array[anchorVertex1] - vector);
			}
			else
			{
				if (flag)
				{
					transform.localPosition = hardParent.morphedWorldToLocalMatrix.MultiplyPoint3x4(array[targetVertex]);
				}
				else
				{
					transform.position = array[targetVertex];
				}
				rotation = Quaternion.LookRotation(vector - array[targetVertex], array[anchorVertex1] - array[targetVertex]);
			}
			transform.rotation = rotation;
			break;
		}
		case LookAtOption.Reference:
		{
			if (flag)
			{
				transform.localPosition = hardParent.morphedWorldToLocalMatrix.MultiplyPoint3x4(array[targetVertex]);
			}
			else
			{
				transform.position = array[targetVertex];
			}
			Quaternion rotation = Quaternion.LookRotation(reference.position - array[targetVertex], array[anchorVertex1] - array[targetVertex]);
			transform.rotation = rotation;
			break;
		}
		}
		if (hardTransform != null && transform != hardTransform && (hardPositionUpdateTrigger == PositionUpdateTrigger.Always || (hardPositionUpdateTrigger == PositionUpdateTrigger.MorphChangeOnly && morphsChanged) || !Application.isPlaying))
		{
			hardTransform.position = transform.position;
			hardTransform.rotation = transform.rotation;
		}
		if (_softJointType == SoftJointType.Direct && !Application.isPlaying)
		{
			if (joint != null)
			{
				joint.connectedBody = backForceRigidbody;
			}
			if (springJoint != null)
			{
				springJoint.connectedBody = backForceRigidbody;
			}
		}
	}

	public void ResetJointPhysics()
	{
		if (joint != null)
		{
			jointTransform.localPosition = joint.connectedAnchor;
			jointTransform.localRotation = jointTransformInitialRotation;
		}
		else if (jointTransform != null)
		{
			jointTransform.position = anchorTarget;
			jointTransform.localRotation = jointTransformInitialRotation;
		}
		if (jointRB != null)
		{
			jointRB.velocity = Vector3.zero;
			jointRB.angularVelocity = Vector3.zero;
		}
		if (otherTransformToReset != null)
		{
			otherTransformToReset.localPosition = Vector3.zero;
			otherTransformToReset.localRotation = Quaternion.identity;
		}
	}

	protected void ResetJointInternal()
	{
		if (!wasInit)
		{
			return;
		}
		if (_softJointType == SoftJointType.FloatingKinematic)
		{
			UpdateTransforms();
			if (jointTransform != null)
			{
				jointTransform.position = kinematicTransform.position;
				jointTransform.localRotation = jointTransformInitialRotation;
			}
		}
		else
		{
			UpdateAnchor();
			ResetJointPhysics();
		}
	}

	public void ResetBackForceTrigger()
	{
		applyBackForce = true;
	}

	public override void ScaleChanged(float scale)
	{
		base.ScaleChanged(scale);
		oneoverscale = 1f / _scale;
		SetJointLimits();
		SetJointDrive();
		AutoColliderSizeSet();
	}

	public void DoUpdate()
	{
		if (!_on)
		{
			return;
		}
		if (resetSimulation)
		{
			ResetJointInternal();
			return;
		}
		UpdateAnchor();
		UpdateTransforms();
		if (resizeTrigger == ResizeTrigger.Always || morphsChanged)
		{
			if (debug)
			{
				Debug.Log("Morph changed - apply and resize");
			}
			morphsChanged = false;
			AutoColliderSizeSet();
		}
		if (!(jointTransform != null) || _softJointType != 0 || !(backForceRigidbody != null) || !(_jointBackForce > 0f))
		{
			return;
		}
		if (applyBackForce)
		{
			applyBackForce = false;
			Vector3 vector = (jointTransform.position - kinematicTransform.position) * oneoverscale;
			float magnitude = vector.magnitude;
			float num = magnitude - _jointBackForceThresholdDistance;
			if (num > 0f)
			{
				float num2 = num / magnitude;
				vector *= num2;
				float num3 = ((!(TimeControl.singleton != null) || !TimeControl.singleton.compensateFixedTimestep) ? 1f : (Mathf.Approximately(Time.timeScale, 0f) ? 1f : (1f / Time.timeScale)));
				Vector3 vector2 = vector * _jointBackForce * num3;
				_appliedBackForce = Vector3.ClampMagnitude(vector2, _jointBackForceMaxForce);
			}
			else
			{
				_appliedBackForce.x = 0f;
				_appliedBackForce.y = 0f;
				_appliedBackForce.z = 0f;
			}
		}
		float num4 = Time.fixedDeltaTime * 90f;
		float num5 = 0.5f;
		_bufferedBackForce = Vector3.Lerp(_bufferedBackForce, _appliedBackForce, num5 * num4);
		backForceRigidbody.AddForce(_bufferedBackForce, ForceMode.Force);
	}

	public void AutoColliderSizeSetFast(Vector3[] verts)
	{
		if (targetVertex == -1 || anchorVertex1 == -1 || (!_useAutoRadius && !_useAutoLength))
		{
			return;
		}
		oneoverscale = 1f;
		bool flag = false;
		if (verts[targetVertex] != currentTargetVertexPosition)
		{
			flag = true;
		}
		else if (verts[anchorVertex1] != currentAnchorVertex1Position)
		{
			flag = true;
		}
		else if (anchorVertex2 != -1 && verts[anchorVertex2] != currentAnchorVertex2Position)
		{
			flag = true;
		}
		else if (oppositeVertex != -1 && verts[oppositeVertex] != currentOppositePosition)
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		if (debug)
		{
			Debug.Log("Verts Changed");
		}
		currentTargetVertexPosition = verts[targetVertex];
		currentAnchorVertex1Position = verts[anchorVertex1];
		if (anchorVertex2 != -1)
		{
			currentAnchorVertex2Position = verts[anchorVertex2];
		}
		if (oppositeVertex != -1)
		{
			currentOppositePosition = verts[oppositeVertex];
		}
		if (lookAtOption == LookAtOption.VertexNormal)
		{
			switch (_colliderOrient)
			{
			case ColliderOrient.Look:
				if (_useAutoRadius)
				{
					float num3 = ((verts[anchorVertex1] - verts[targetVertex]).magnitude * oneoverscale - _autoRadiusBuffer) * _autoRadiusMultiplier;
					if (Mathf.Abs(num3 - _colliderRadius) >= 0.001f)
					{
						_colliderRadius = num3;
						colliderDirty = true;
					}
				}
				break;
			case ColliderOrient.Up:
				if (_useAutoLength)
				{
					float num4 = (verts[anchorVertex1] - verts[targetVertex]).magnitude * 2f * oneoverscale - _autoLengthBuffer;
					if (Mathf.Abs(num4 - _colliderLength) >= 0.001f)
					{
						_colliderLength = num4;
						colliderDirty = true;
					}
				}
				if (_useAutoRadius && anchorVertex2 != -1)
				{
					float num5 = ((verts[anchorVertex2] - verts[targetVertex]).magnitude * oneoverscale - _autoRadiusBuffer) * _autoRadiusMultiplier;
					if (Mathf.Abs(num5 - _colliderRadius) >= 0.001f)
					{
						_colliderRadius = num5;
						colliderDirty = true;
					}
				}
				break;
			case ColliderOrient.Right:
				if (_useAutoLength && anchorVertex2 != -1)
				{
					float num = (verts[anchorVertex2] - verts[targetVertex]).magnitude * 2f * oneoverscale - _autoLengthBuffer;
					if (Mathf.Abs(num - _colliderLength) >= 0.001f)
					{
						_colliderLength = num;
						colliderDirty = true;
					}
				}
				if (_useAutoRadius)
				{
					float num2 = ((verts[anchorVertex1] - verts[targetVertex]).magnitude * oneoverscale - _autoRadiusBuffer) * _autoRadiusMultiplier;
					if (Mathf.Abs(num2 - _colliderRadius) >= 0.001f)
					{
						_colliderRadius = num2;
						colliderDirty = true;
					}
				}
				break;
			}
		}
		else if (lookAtOption == LookAtOption.Opposite)
		{
			switch (_colliderOrient)
			{
			case ColliderOrient.Look:
				if (_useAutoLength && oppositeVertex != -1)
				{
					float num9 = (verts[oppositeVertex] - verts[targetVertex]).magnitude * oneoverscale - _autoLengthBuffer;
					if (Mathf.Abs(num9 - _colliderLength) >= 0.001f)
					{
						_colliderLength = num9;
						colliderDirty = true;
					}
				}
				if (!_useAutoRadius)
				{
					break;
				}
				if (anchorVertex2 != -1)
				{
					float num10 = ((verts[anchorVertex2] - verts[anchorVertex1]).magnitude * 0.5f * oneoverscale - _autoRadiusBuffer) * _autoRadiusMultiplier;
					if (Mathf.Abs(num10 - _colliderRadius) >= 0.001f)
					{
						_colliderRadius = num10;
						colliderDirty = true;
					}
				}
				else
				{
					float num11 = ((verts[anchorVertex1] - verts[targetVertex]).magnitude * oneoverscale - _autoRadiusBuffer) * _autoRadiusMultiplier;
					if (Mathf.Abs(num11 - _colliderRadius) >= 0.001f)
					{
						_colliderRadius = num11;
						colliderDirty = true;
					}
				}
				break;
			case ColliderOrient.Up:
				if (_useAutoLength)
				{
					if (anchorVertex2 != -1)
					{
						float num6 = (verts[anchorVertex2] - verts[anchorVertex1]).magnitude * oneoverscale - _autoLengthBuffer;
						if (Mathf.Abs(num6 - _colliderLength) >= 0.001f)
						{
							_colliderLength = num6;
							colliderDirty = true;
						}
					}
					else
					{
						float num7 = (verts[anchorVertex1] - verts[targetVertex]).magnitude * 2f * oneoverscale - _autoLengthBuffer;
						if (Mathf.Abs(num7 - _colliderLength) >= 0.001f)
						{
							_colliderLength = num7;
							colliderDirty = true;
						}
					}
				}
				if (_useAutoRadius && oppositeVertex != -1)
				{
					float num8 = ((verts[oppositeVertex] - verts[targetVertex]).magnitude * 0.5f * oneoverscale - _autoRadiusBuffer) * _autoRadiusMultiplier;
					if (Mathf.Abs(num8 - _colliderRadius) >= 0.001f)
					{
						_colliderRadius = num8;
						colliderDirty = true;
					}
				}
				break;
			}
		}
		else if (lookAtOption == LookAtOption.Anchor1)
		{
			if (_colliderOrient != 0)
			{
				return;
			}
			if (_useAutoLength)
			{
				float num12 = (verts[anchorVertex1] - verts[targetVertex]).magnitude * 2f * oneoverscale - _autoLengthBuffer;
				if (Mathf.Abs(num12 - _colliderLength) >= 0.001f)
				{
					_colliderLength = num12;
					colliderDirty = true;
				}
			}
			if (!_useAutoRadius)
			{
				return;
			}
			if (oppositeVertex != -1)
			{
				float num13 = ((verts[oppositeVertex] - verts[targetVertex]).magnitude * 0.5f * oneoverscale - _autoRadiusBuffer) * _autoRadiusMultiplier;
				if (Mathf.Abs(num13 - _colliderRadius) >= 0.001f)
				{
					_colliderRadius = num13;
					colliderDirty = true;
				}
			}
			else if (anchorVertex2 != -1)
			{
				float num14 = ((verts[anchorVertex2] - verts[targetVertex]).magnitude * oneoverscale - _autoRadiusBuffer) * _autoRadiusMultiplier;
				if (Mathf.Abs(num14 - _colliderRadius) >= 0.001f)
				{
					_colliderRadius = num14;
					colliderDirty = true;
				}
			}
		}
		else
		{
			if (lookAtOption != LookAtOption.AnchorCenters || anchorVertex2 == -1)
			{
				return;
			}
			switch (_colliderOrient)
			{
			case ColliderOrient.Look:
				if (_useAutoLength)
				{
					Vector3 vector = (verts[anchorVertex2] + verts[anchorVertex1]) * 0.5f;
					float num17 = (vector - verts[targetVertex]).magnitude * 2f * oneoverscale - _autoLengthBuffer;
					if (Mathf.Abs(num17 - _colliderLength) >= 0.001f)
					{
						_colliderLength = num17;
						colliderDirty = true;
					}
				}
				if (_useAutoRadius)
				{
					float num18 = ((verts[anchorVertex2] - verts[anchorVertex1]).magnitude * 0.5f * oneoverscale - _autoRadiusBuffer) * _autoRadiusMultiplier;
					if (Mathf.Abs(num18 - _colliderRadius) >= 0.001f)
					{
						_colliderRadius = num18;
						colliderDirty = true;
					}
				}
				break;
			case ColliderOrient.Up:
				if (_useAutoLength)
				{
					float num19 = (verts[anchorVertex2] - verts[anchorVertex1]).magnitude * oneoverscale - _autoLengthBuffer;
					if (Mathf.Abs(num19 - _colliderLength) >= 0.001f)
					{
						_colliderLength = num19;
						colliderDirty = true;
					}
				}
				if (_useAutoRadius)
				{
					Vector3 vector2 = (verts[anchorVertex2] + verts[anchorVertex1]) * 0.5f;
					float num20 = ((vector2 - verts[targetVertex]).magnitude * oneoverscale - _autoRadiusBuffer) * _autoRadiusMultiplier;
					if (Mathf.Abs(num20 - _colliderRadius) >= 0.001f)
					{
						_colliderRadius = num20;
						colliderDirty = true;
					}
				}
				break;
			case ColliderOrient.Right:
				if (_useAutoLength && oppositeVertex != -1)
				{
					float num15 = (verts[oppositeVertex] - verts[targetVertex]).magnitude * 2f * oneoverscale - _autoLengthBuffer;
					if (Mathf.Abs(num15 - _colliderLength) >= 0.001f)
					{
						_colliderLength = num15;
						colliderDirty = true;
					}
				}
				if (_useAutoRadius)
				{
					float num16 = ((verts[anchorVertex2] - verts[anchorVertex1]).magnitude * 0.5f * oneoverscale - _autoRadiusBuffer) * _autoRadiusMultiplier;
					if (Mathf.Abs(num16 - _colliderRadius) >= 0.001f)
					{
						_colliderRadius = num16;
						colliderDirty = true;
					}
				}
				break;
			}
		}
	}

	public void AutoColliderSizeSetFinishFast()
	{
		colliderDirty = false;
		SetColliders();
		if (hardCollider != null && hardParent != null)
		{
			Matrix4x4 morphedWorldToLocalMatrix = hardParent.morphedWorldToLocalMatrix;
			hardCollider.transform.localPosition = morphedWorldToLocalMatrix.MultiplyPoint3x4(hardPositionTarget);
		}
	}

	public void AutoColliderSizeSet(bool force = false)
	{
		if (targetVertex == -1 || anchorVertex1 == -1 || !(_skin != null) || (!_useAutoRadius && !_useAutoLength))
		{
			return;
		}
		bool flag = force;
		Vector3[] array;
		Vector3[] array2;
		if (Application.isPlaying)
		{
			if (resizeTrigger == ResizeTrigger.Always)
			{
				array = _skin.rawSkinnedVerts;
				array2 = _skin.postSkinNormals;
			}
			else
			{
				array = _skin.dazMesh.visibleMorphedUVVertices;
				array2 = _skin.dazMesh.morphedUVNormals;
				oneoverscale = 1f;
			}
		}
		else
		{
			array = _skin.dazMesh.morphedUVVertices;
			array2 = _skin.dazMesh.morphedUVNormals;
			flag = true;
			oneoverscale = 1f;
		}
		if (array[targetVertex] != currentTargetVertexPosition)
		{
			flag = true;
		}
		else if (array[anchorVertex1] != currentAnchorVertex1Position)
		{
			flag = true;
		}
		else if (anchorVertex2 != -1 && array[anchorVertex2] != currentAnchorVertex2Position)
		{
			flag = true;
		}
		else if (oppositeVertex != -1 && array[oppositeVertex] != currentOppositePosition)
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		currentTargetVertexPosition = array[targetVertex];
		currentAnchorVertex1Position = array[anchorVertex1];
		if (anchorVertex2 != -1)
		{
			currentAnchorVertex2Position = array[anchorVertex2];
		}
		if (oppositeVertex != -1)
		{
			currentOppositePosition = array[oppositeVertex];
		}
		if (lookAtOption == LookAtOption.VertexNormal)
		{
			switch (_colliderOrient)
			{
			case ColliderOrient.Look:
				if (_useAutoRadius)
				{
					_colliderRadius = ((array[anchorVertex1] - array[targetVertex]).magnitude * oneoverscale - _autoRadiusBuffer) * _autoRadiusMultiplier;
				}
				break;
			case ColliderOrient.Up:
				if (_useAutoLength)
				{
					_colliderLength = (array[anchorVertex1] - array[targetVertex]).magnitude * 2f * oneoverscale - _autoLengthBuffer;
				}
				if (_useAutoRadius && anchorVertex2 != -1)
				{
					_colliderRadius = ((array[anchorVertex2] - array[targetVertex]).magnitude * oneoverscale - _autoRadiusBuffer) * _autoRadiusMultiplier;
				}
				break;
			case ColliderOrient.Right:
				if (_useAutoLength && anchorVertex2 != -1)
				{
					_colliderLength = (array[anchorVertex2] - array[targetVertex]).magnitude * 2f * oneoverscale - _autoLengthBuffer;
				}
				if (_useAutoRadius)
				{
					_colliderRadius = ((array[anchorVertex1] - array[targetVertex]).magnitude * oneoverscale - _autoRadiusBuffer) * _autoRadiusMultiplier;
				}
				break;
			}
		}
		else if (lookAtOption == LookAtOption.Opposite)
		{
			switch (_colliderOrient)
			{
			case ColliderOrient.Look:
				if (_useAutoLength && oppositeVertex != -1)
				{
					_colliderLength = (array[oppositeVertex] - array[targetVertex]).magnitude * oneoverscale - _autoLengthBuffer;
				}
				if (_useAutoRadius)
				{
					if (anchorVertex2 != -1)
					{
						_colliderRadius = ((array[anchorVertex2] - array[anchorVertex1]).magnitude * 0.5f * oneoverscale - _autoRadiusBuffer) * _autoRadiusMultiplier;
					}
					else
					{
						_colliderRadius = ((array[anchorVertex1] - array[targetVertex]).magnitude * oneoverscale - _autoRadiusBuffer) * _autoRadiusMultiplier;
					}
				}
				break;
			case ColliderOrient.Up:
				if (_useAutoLength)
				{
					if (anchorVertex2 != -1)
					{
						_colliderLength = (array[anchorVertex2] - array[anchorVertex1]).magnitude * oneoverscale - _autoLengthBuffer;
					}
					else
					{
						_colliderLength = (array[anchorVertex1] - array[targetVertex]).magnitude * 2f * oneoverscale - _autoLengthBuffer;
					}
				}
				if (_useAutoRadius && oppositeVertex != -1)
				{
					_colliderRadius = ((array[oppositeVertex] - array[targetVertex]).magnitude * 0.5f * oneoverscale - _autoRadiusBuffer) * _autoRadiusMultiplier;
				}
				break;
			}
		}
		else if (lookAtOption == LookAtOption.Anchor1)
		{
			if (_colliderOrient == ColliderOrient.Look)
			{
				if (_useAutoLength)
				{
					_colliderLength = (array[anchorVertex1] - array[targetVertex]).magnitude * 2f * oneoverscale - _autoLengthBuffer;
				}
				if (_useAutoRadius)
				{
					if (oppositeVertex != -1)
					{
						_colliderRadius = ((array[oppositeVertex] - array[targetVertex]).magnitude * 0.5f * oneoverscale - _autoRadiusBuffer) * _autoRadiusMultiplier;
					}
					else if (anchorVertex2 != -1)
					{
						_colliderRadius = ((array[anchorVertex2] - array[targetVertex]).magnitude * oneoverscale - _autoRadiusBuffer) * _autoRadiusMultiplier;
					}
				}
			}
		}
		else if (lookAtOption == LookAtOption.AnchorCenters && anchorVertex2 != -1)
		{
			switch (_colliderOrient)
			{
			case ColliderOrient.Look:
				if (_useAutoLength)
				{
					Vector3 vector = (array[anchorVertex2] + array[anchorVertex1]) * 0.5f;
					_colliderLength = (vector - array[targetVertex]).magnitude * 2f * oneoverscale - _autoLengthBuffer;
				}
				if (_useAutoRadius)
				{
					_colliderRadius = ((array[anchorVertex2] - array[anchorVertex1]).magnitude * 0.5f * oneoverscale - _autoRadiusBuffer) * _autoRadiusMultiplier;
				}
				break;
			case ColliderOrient.Up:
				if (_useAutoLength)
				{
					_colliderLength = (array[anchorVertex2] - array[anchorVertex1]).magnitude * oneoverscale - _autoLengthBuffer;
				}
				if (_useAutoRadius)
				{
					Vector3 vector2 = (array[anchorVertex2] + array[anchorVertex1]) * 0.5f;
					_colliderRadius = ((vector2 - array[targetVertex]).magnitude * oneoverscale - _autoRadiusBuffer) * _autoRadiusMultiplier;
				}
				break;
			case ColliderOrient.Right:
				if (_useAutoLength && oppositeVertex != -1)
				{
					_colliderLength = (array[oppositeVertex] - array[targetVertex]).magnitude * 2f * oneoverscale - _autoLengthBuffer;
				}
				if (_useAutoRadius)
				{
					_colliderRadius = ((array[anchorVertex2] - array[anchorVertex1]).magnitude * 0.5f * oneoverscale - _autoRadiusBuffer) * _autoRadiusMultiplier;
				}
				break;
			}
		}
		SetColliders();
		if (Application.isPlaying)
		{
			return;
		}
		UpdateTransforms();
		if (showUsedVerts && oppositeVertex != -1)
		{
			Vector3 lhs = array[oppositeVertex] - array[targetVertex];
			if (lookAtOption == LookAtOption.VertexNormal)
			{
				float num = Vector3.Dot(lhs, array2[targetVertex]);
				Debug.DrawLine(array[targetVertex], array[targetVertex] + array2[targetVertex] * num, Color.yellow);
			}
			else
			{
				Debug.DrawLine(array[targetVertex], array[oppositeVertex], Color.yellow);
			}
		}
	}

	private void OnEnable()
	{
		if (Application.isPlaying)
		{
			InitColliders();
			SetInterpolation();
			if (_skin != null && _skin.dazMesh != null)
			{
				_skin.Init();
			}
		}
	}

	private void Start()
	{
		Init();
	}

	protected override void Update()
	{
		base.Update();
		applyBackForce = true;
		if (Application.isPlaying)
		{
			if (resizeTrigger == ResizeTrigger.MorphChangeOnly && (_skin.dazMesh.visibleNonPoseVerticesChangedThisFrame || _skin.dazMesh.visibleNonPoseVerticesChangedLastFrame))
			{
				if (debug)
				{
					Debug.Log("Morphs changed - trigger");
				}
				morphsChanged = true;
			}
			if (morphVertex && targetVertex != -1 && jointTransform != null)
			{
				Vector3 vector = jointTransform.position - anchorTarget;
				_skin.postSkinMorphs[targetVertex] = vector;
			}
		}
		if ((!debug && Application.isPlaying) || !showUsedVerts || !(_skin != null) || !(_skin.dazMesh != null))
		{
			return;
		}
		Vector3[] array = ((!Application.isPlaying) ? _skin.dazMesh.morphedUVVertices : _skin.rawSkinnedVerts);
		if (targetVertex != -1)
		{
			MyDebug.DrawWireCube(array[targetVertex], 0.002f, Color.green);
		}
		if (anchorVertex1 != -1)
		{
			MyDebug.DrawWireCube(array[anchorVertex1], 0.001f, Color.blue);
		}
		if (anchorVertex2 != -1)
		{
			MyDebug.DrawWireCube(array[anchorVertex2], 0.001f, Color.cyan);
		}
		if (oppositeVertex != -1)
		{
			MyDebug.DrawWireCube(array[oppositeVertex], 0.001f, Color.yellow);
		}
		if (debug)
		{
			array = _skin.dazMesh.morphedUVVertices;
			if (targetVertex != -1)
			{
				MyDebug.DrawWireCube(array[targetVertex], 0.002f, Color.green);
			}
			if (anchorVertex1 != -1)
			{
				MyDebug.DrawWireCube(array[anchorVertex1], 0.001f, Color.blue);
			}
			if (anchorVertex2 != -1)
			{
				MyDebug.DrawWireCube(array[anchorVertex2], 0.001f, Color.cyan);
			}
			if (oppositeVertex != -1)
			{
				MyDebug.DrawWireCube(array[oppositeVertex], 0.001f, Color.yellow);
			}
		}
	}

	private void FixedUpdate()
	{
		if (wasInit)
		{
			if (_globalOn != globalEnable)
			{
				SyncOn();
			}
			DoUpdate();
		}
	}
}
