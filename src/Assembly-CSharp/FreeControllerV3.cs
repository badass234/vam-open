using System;
using System.Collections.Generic;
using MVR;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class FreeControllerV3 : PhysicsSimulatorJSONStorable
{
	public enum SelectLinkState
	{
		PositionAndRotation,
		Position,
		Rotation
	}

	public enum PositionState
	{
		On,
		Off,
		Following,
		Hold,
		Lock,
		ParentLink,
		PhysicsLink,
		Comply
	}

	public enum RotationState
	{
		On,
		Off,
		Following,
		Hold,
		Lock,
		LookAt,
		ParentLink,
		PhysicsLink,
		Comply
	}

	public enum GridMode
	{
		None,
		Local,
		Global
	}

	public enum MoveAxisnames
	{
		X,
		Y,
		Z,
		CameraRight,
		CameraUp,
		CameraForward,
		CameraRightNoY,
		CameraForwardNoY,
		None
	}

	public enum RotateAxisnames
	{
		X,
		Y,
		Z,
		NegX,
		NegY,
		NegZ,
		WorldY,
		None
	}

	public enum DrawAxisnames
	{
		X,
		Y,
		Z,
		NegX,
		NegY,
		NegZ
	}

	public enum ControlMode
	{
		Off,
		Position,
		Rotation
	}

	public delegate void OnPositionChange(FreeControllerV3 fcv3);

	public delegate void OnRotationChange(FreeControllerV3 fcv3);

	public delegate void OnMovement(FreeControllerV3 fcv3);

	public delegate void OnGrabStart(FreeControllerV3 fcv3);

	public delegate void OnGrabEnd(FreeControllerV3 fcv3);

	public delegate void OnPossessStart(FreeControllerV3 fcv3);

	public delegate void OnPossessEnd(FreeControllerV3 fcv3);

	public static float targetAlpha = 1f;

	protected List<UnityEngine.Object> allocatedObjects;

	public bool storePositionRotationAsLocal;

	protected bool _forceStorePositionRotationAsLocal;

	protected string[] customParamNames = new string[5] { "localPosition", "localRotation", "position", "rotation", "linkTo" };

	protected JSONStorableAction resetAction;

	public bool enableSelectRoot;

	public UIPopup linkToSelectionPopup;

	public UIPopup linkToAtomSelectionPopup;

	private Rigidbody _linkToRB;

	private Transform _linkToConnector;

	private ConfigurableJoint _linkToJoint;

	protected string linkToAtomUID;

	private Rigidbody preLinkRB;

	private PositionState preLinkPositionState;

	private RotationState preLinkRotationState;

	public Rigidbody startingLinkToRigidbody;

	protected bool _isGrabbing;

	public bool stateCanBeModified = true;

	public PositionState startingPositionState;

	private JSONStorableStringChooser currentPositionStateJSON;

	private PositionState _currentPositionState;

	public RotationState startingRotationState;

	private JSONStorableStringChooser currentRotationStateJSON;

	private RotationState _currentRotationState;

	protected float scalePow = 1f;

	public Quaternion2Angles.RotationOrder naturalJointDriveRotationOrder;

	private JSONStorableBool xLockJSON;

	[SerializeField]
	private bool _xLock;

	private JSONStorableBool yLockJSON;

	[SerializeField]
	private bool _yLock;

	private JSONStorableBool zLockJSON;

	[SerializeField]
	private bool _zLock;

	private JSONStorableBool xLocalLockJSON;

	[SerializeField]
	private bool _xLocalLock;

	private JSONStorableBool yLocalLockJSON;

	[SerializeField]
	private bool _yLocalLock;

	private JSONStorableBool zLocalLockJSON;

	[SerializeField]
	private bool _zLocalLock;

	private JSONStorableBool xRotLockJSON;

	[SerializeField]
	private bool _xRotLock;

	private JSONStorableBool yRotLockJSON;

	[SerializeField]
	private bool _yRotLock;

	private JSONStorableBool zRotLockJSON;

	[SerializeField]
	private bool _zRotLock;

	public SetTextFromFloat xPositionText;

	public InputField xPositionInputField;

	public SetTextFromFloat yPositionText;

	public InputField yPositionInputField;

	public SetTextFromFloat zPositionText;

	public InputField zPositionInputField;

	public SetTextFromFloat xRotationText;

	public InputField xRotationInputField;

	public SetTextFromFloat yRotationText;

	public InputField yRotationInputField;

	public SetTextFromFloat zRotationText;

	public InputField zRotationInputField;

	public SetTextFromFloat xLocalPositionText;

	public InputField xLocalPositionInputField;

	public SetTextFromFloat yLocalPositionText;

	public InputField yLocalPositionInputField;

	public SetTextFromFloat zLocalPositionText;

	public InputField zLocalPositionInputField;

	public SetTextFromFloat xLocalRotationText;

	public InputField xLocalRotationInputField;

	public SetTextFromFloat yLocalRotationText;

	public InputField yLocalRotationInputField;

	public SetTextFromFloat zLocalRotationText;

	public InputField zLocalRotationInputField;

	public bool controlsOn;

	protected JSONStorableBool onJSON;

	[SerializeField]
	private bool _on = true;

	protected JSONStorableBool interactableInPlayModeJSON;

	[SerializeField]
	private bool _interactableInPlayMode = true;

	public FreeControllerV3[] onPossessDeactiveList;

	[SerializeField]
	private bool _possessed;

	protected JSONStorableBool deactivateOtherControlsOnPossessJSON;

	private bool _deactivateOtherControlsOnPossess = true;

	public bool startedPossess;

	public Transform possessPoint;

	protected JSONStorableBool possessableJSON;

	[SerializeField]
	private bool _possessable;

	protected JSONStorableBool canGrabPositionJSON;

	[SerializeField]
	private bool _canGrabPosition = true;

	protected JSONStorableBool canGrabRotationJSON;

	[SerializeField]
	private bool _canGrabRotation = true;

	public bool freezeAtomPhysicsWhenGrabbed;

	protected JSONStorableBool freezeAtomPhysicsWhenGrabbedJSON;

	private JSONStorableStringChooser positionGridModeJSON;

	[SerializeField]
	private GridMode _positionGridMode;

	private JSONStorableFloat positionGridJSON;

	[SerializeField]
	private float _positionGrid = 0.1f;

	private JSONStorableStringChooser rotationGridModeJSON;

	[SerializeField]
	private GridMode _rotationGridMode;

	private JSONStorableFloat rotationGridJSON;

	[SerializeField]
	private float _rotationGrid = 15f;

	private JSONStorableBool useGravityJSON;

	[SerializeField]
	private bool _useGravityOnRBWhenOff = true;

	private JSONStorableBool physicsEnabledJSON;

	public bool controlsCollisionEnabled;

	private bool _globalCollisionEnabled = true;

	private JSONStorableBool collisionEnabledJSON;

	private Rigidbody _followWhenOffRB;

	private Rigidbody kinematicRB;

	private ConfigurableJoint connectedJoint;

	private ConfigurableJoint naturalJoint;

	public bool useForceWhenOff = true;

	public float distanceHolder;

	public float forceFactor = 10000f;

	public float torqueFactor = 2000f;

	public Rigidbody[] rigidbodySlavesForMass;

	private JSONStorableFloat RBMassJSON;

	private JSONStorableFloat RBDragJSON;

	private bool _RBMaxVelocityEnable = true;

	private JSONStorableBool RBMaxVelocityEnableJSON;

	private float _RBMaxVelocity = 10f;

	private JSONStorableFloat RBMaxVelocityJSON;

	private JSONStorableFloat RBAngularDragJSON;

	[SerializeField]
	private float _RBLockPositionSpring = 250000f;

	[SerializeField]
	private float _RBLockPositionDamper = 250f;

	[SerializeField]
	public float _RBLockPositionMaxForce = 100000000f;

	private JSONStorableFloat RBHoldPositionSpringJSON;

	[SerializeField]
	private float _RBHoldPositionSpring = 1000f;

	private JSONStorableFloat RBHoldPositionDamperJSON;

	[SerializeField]
	private float _RBHoldPositionDamper = 50f;

	private JSONStorableFloat RBHoldPositionMaxForceJSON;

	[SerializeField]
	private float _RBHoldPositionMaxForce = 10000f;

	private JSONStorableFloat RBComplyPositionSpringJSON;

	[SerializeField]
	private float _RBComplyPositionSpring = 1500f;

	private JSONStorableFloat RBComplyPositionDamperJSON;

	[SerializeField]
	private float _RBComplyPositionDamper = 100f;

	private float _RBComplyPositionMaxForce = 1E+13f;

	private JSONStorableFloat RBLinkPositionSpringJSON;

	[SerializeField]
	private float _RBLinkPositionSpring = 250000f;

	private JSONStorableFloat RBLinkPositionDamperJSON;

	[SerializeField]
	private float _RBLinkPositionDamper = 250f;

	private JSONStorableFloat RBLinkPositionMaxForceJSON;

	[SerializeField]
	private float _RBLinkPositionMaxForce = 100000000f;

	[SerializeField]
	private float _RBLockRotationSpring = 250000f;

	[SerializeField]
	private float _RBLockRotationDamper = 250f;

	[SerializeField]
	public float _RBLockRotationMaxForce = 100000000f;

	private JSONStorableFloat RBHoldRotationSpringJSON;

	[SerializeField]
	private float _RBHoldRotationSpring = 1000f;

	private JSONStorableFloat RBHoldRotationDamperJSON;

	[SerializeField]
	private float _RBHoldRotationDamper = 50f;

	private JSONStorableFloat RBHoldRotationMaxForceJSON;

	[SerializeField]
	private float _RBHoldRotationMaxForce = 10000f;

	private JSONStorableFloat RBComplyRotationSpringJSON;

	[SerializeField]
	private float _RBComplyRotationSpring = 150f;

	private JSONStorableFloat RBComplyRotationDamperJSON;

	[SerializeField]
	private float _RBComplyRotationDamper = 10f;

	private float _RBComplyRotationMaxForce = 1E+13f;

	private JSONStorableFloat RBLinkRotationSpringJSON;

	[SerializeField]
	private float _RBLinkRotationSpring = 250000f;

	private JSONStorableFloat RBLinkRotationDamperJSON;

	[SerializeField]
	private float _RBLinkRotationDamper = 250f;

	private JSONStorableFloat RBLinkRotationMaxForceJSON;

	[SerializeField]
	private float _RBLinkRotationMaxForce = 100000000f;

	private JSONStorableFloat RBComplyJointRotationDriveSpringJSON;

	[SerializeField]
	private float _RBComplyJointRotationDriveSpring = 20f;

	private JSONStorableFloat jointRotationDriveSpringJSON;

	[SerializeField]
	private float _jointRotationDriveSpring;

	private JSONStorableFloat jointRotationDriveDamperJSON;

	[SerializeField]
	private float _jointRotationDriveDamper;

	private JSONStorableFloat jointRotationDriveMaxForceJSON;

	[SerializeField]
	private float _jointRotationDriveMaxForce;

	private JSONStorableFloat jointRotationDriveXTargetJSON;

	private float _jointRotationDriveXTargetMin;

	private float _jointRotationDriveXTargetMax;

	[SerializeField]
	private float _jointRotationDriveXTarget;

	[SerializeField]
	private float _jointRotationDriveXTargetAdditional;

	private JSONStorableFloat jointRotationDriveYTargetJSON;

	private float _jointRotationDriveYTargetMin;

	private float _jointRotationDriveYTargetMax;

	[SerializeField]
	private float _jointRotationDriveYTarget;

	[SerializeField]
	private float _jointRotationDriveYTargetAdditional;

	private JSONStorableFloat jointRotationDriveZTargetJSON;

	private float _jointRotationDriveZTargetMin;

	private float _jointRotationDriveZTargetMax;

	[SerializeField]
	private float _jointRotationDriveZTarget;

	[SerializeField]
	private float _jointRotationDriveZTargetAdditional;

	private bool _detachControl;

	private JSONStorableBool detachControlJSON;

	public Text UIDText;

	public Text UIDTextAlt;

	public Transform[] UITransforms;

	public Transform[] UITransformsPlayMode;

	public bool GUIalwaysVisibleWhenSelected;

	public bool useContainedMeshRenderers = true;

	private bool _hidden = true;

	private bool _guihidden = true;

	public float unhighlightedScale = 0.5f;

	public float highlightedScale = 0.5f;

	public float selectedScale = 1f;

	private bool _highlighted;

	private Vector3 _selectedPosition;

	private bool _selected;

	public Color onColor = new Color(0f, 1f, 0f, 0.5f);

	public Color offColor = new Color(1f, 0f, 0f, 0.5f);

	public Color followingColor = new Color(1f, 0f, 1f, 0.5f);

	public Color holdColor = new Color(1f, 0.5f, 0f, 0.5f);

	public Color lockColor = new Color(0.5f, 0.25f, 0f, 0.5f);

	public Color lookAtColor = new Color(0f, 1f, 1f, 0.5f);

	public Color highlightColor = new Color(1f, 1f, 0f, 0.5f);

	public Color selectedColor = new Color(0f, 0f, 1f, 0.5f);

	public Color overlayColor = new Color(1f, 1f, 1f, 0.5f);

	private Color _currentPositionColor;

	private Color _currentRotationColor;

	public Material material;

	public Material linkLineMaterial;

	private LineDrawer linkLineDrawer;

	private Material positionMaterialLocal;

	private Material rotationMaterialLocal;

	private Material snapshotMaterialLocal;

	private Material linkLineMaterialLocal;

	private Material materialOverlay;

	public float meshScale = 0.5f;

	private Mesh _currentPositionMesh;

	private Mesh _currentRotationMesh;

	public bool drawSnapshot;

	private Matrix4x4 snapshotMatrix;

	public bool drawMesh = true;

	public bool drawMeshWhenDeselected = true;

	public Mesh onPositionMesh;

	public Mesh offPositionMesh;

	public Mesh followingPositionMesh;

	public Mesh holdPositionMesh;

	public Mesh lockPositionMesh;

	public Mesh onRotationMesh;

	public Mesh offRotationMesh;

	public Mesh followingRotationMesh;

	public Mesh holdRotationMesh;

	public Mesh lockRotationMesh;

	public Mesh lookAtRotationMesh;

	public Mesh moveModeOverlayMesh;

	public Mesh rotateModeOverlayMesh;

	public Mesh deselectedMesh;

	public float deselectedMeshScale = 0.5f;

	public bool debug;

	public Transform control;

	public Transform follow;

	public Transform followWhenOff;

	public Transform lookAt;

	public Transform alsoMoveWhenInactive;

	public Transform alsoMoveWhenInactiveParentWhenActive;

	public Transform alsoMoveWhenInactiveParentWhenInactive;

	public Transform alsoMoveWhenInactiveAlternate;

	public Transform focusPoint;

	public MoveAxisnames MoveAxis1 = MoveAxisnames.CameraRightNoY;

	public MoveAxisnames MoveAxis2 = MoveAxisnames.CameraForwardNoY;

	public MoveAxisnames MoveAxis3 = MoveAxisnames.Y;

	public RotateAxisnames RotateAxis1 = RotateAxisnames.Z;

	public RotateAxisnames RotateAxis2;

	public RotateAxisnames RotateAxis3 = RotateAxisnames.Y;

	public DrawAxisnames MeshForwardAxis = DrawAxisnames.Y;

	public DrawAxisnames MeshUpAxis = DrawAxisnames.Z;

	public DrawAxisnames DrawForwardAxis = DrawAxisnames.Z;

	public DrawAxisnames DrawUpAxis = DrawAxisnames.Y;

	public DrawAxisnames PossessForwardAxis = DrawAxisnames.Z;

	public DrawAxisnames PossessUpAxis = DrawAxisnames.Y;

	public float moveFactor = 1f;

	public float rotateFactor = 60f;

	private bool _moveEnabled = true;

	private bool _moveForceEnabled;

	private bool _rotationEnabled = true;

	private bool _rotationForceEnabled;

	private Vector3 appliedForce;

	private Vector3 appliedTorque;

	private ControlMode _controlMode = ControlMode.Position;

	public Vector3 startingPosition;

	public Quaternion startingRotation;

	public Vector3 startingLocalPosition;

	public Quaternion startingLocalRotation;

	private Vector3 initialLocalPosition;

	private Quaternion initialLocalRotation;

	private MeshRenderer[] mrs;

	protected FreeControllerV3UI currentFCUI;

	protected FreeControllerV3UI currentFCUIAlt;

	protected int complyPauseFrames;

	[SerializeField]
	protected float complyPositionThreshold = 0.001f;

	protected JSONStorableFloat complyPositionThresholdJSON;

	[SerializeField]
	protected float complyRotationThreshold = 5f;

	protected JSONStorableFloat complyRotationThresholdJSON;

	[SerializeField]
	protected float complySpeed = 10f;

	protected JSONStorableFloat complySpeedJSON;

	public OnPositionChange onPositionChangeHandlers;

	public OnPositionChange onRotationChangeHandlers;

	public OnMovement onMovementHandlers;

	public OnGrabStart onGrabStartHandlers;

	public OnGrabEnd onGrabEndHandlers;

	public OnPossessStart onPossessStartHandlers;

	public OnPossessEnd onPossessEndHandlers;

	private bool wasInit;

	public bool forceStorePositionRotationAsLocal
	{
		get
		{
			return _forceStorePositionRotationAsLocal;
		}
		set
		{
			if (_forceStorePositionRotationAsLocal != value)
			{
				_forceStorePositionRotationAsLocal = value;
			}
		}
	}

	public Rigidbody linkToRB
	{
		get
		{
			return _linkToRB;
		}
		set
		{
			if (!(_linkToRB != value))
			{
				return;
			}
			if (_linkToConnector != null)
			{
				UnityEngine.Object.DestroyImmediate(_linkToConnector.gameObject);
				_linkToConnector = null;
			}
			if (_linkToJoint != null)
			{
				UnityEngine.Object.DestroyImmediate(_linkToJoint);
				_linkToJoint = null;
				if (kinematicRB != null)
				{
					kinematicRB.isKinematic = false;
					kinematicRB.isKinematic = true;
				}
			}
			_linkToRB = value;
			if (_linkToRB != null)
			{
				if (_followWhenOffRB != null && _linkToRB != null)
				{
					GameObject gameObject = _followWhenOffRB.gameObject;
					_linkToJoint = gameObject.AddComponent<ConfigurableJoint>();
					_linkToJoint.connectedBody = _linkToRB;
					_linkToJoint.xMotion = ConfigurableJointMotion.Free;
					_linkToJoint.yMotion = ConfigurableJointMotion.Free;
					_linkToJoint.zMotion = ConfigurableJointMotion.Free;
					_linkToJoint.angularXMotion = ConfigurableJointMotion.Free;
					_linkToJoint.angularYMotion = ConfigurableJointMotion.Free;
					_linkToJoint.angularZMotion = ConfigurableJointMotion.Free;
					_linkToJoint.rotationDriveMode = RotationDriveMode.Slerp;
					SetLinkedJointSprings();
				}
				GameObject gameObject2 = new GameObject();
				_linkToConnector = gameObject2.transform;
				_linkToConnector.position = base.transform.position;
				_linkToConnector.rotation = base.transform.rotation;
				_linkToConnector.SetParent(_linkToRB.transform);
			}
		}
	}

	public bool isGrabbing
	{
		get
		{
			return _isGrabbing;
		}
		set
		{
			if (_isGrabbing == value)
			{
				return;
			}
			_isGrabbing = value;
			if (_isGrabbing)
			{
				if (onGrabStartHandlers != null)
				{
					onGrabStartHandlers(this);
				}
			}
			else if (onGrabEndHandlers != null)
			{
				onGrabEndHandlers(this);
			}
			SyncGrabFreezePhysics();
		}
	}

	public PositionState currentPositionState
	{
		get
		{
			return _currentPositionState;
		}
		set
		{
			if (stateCanBeModified)
			{
				if (currentPositionStateJSON != null)
				{
					currentPositionStateJSON.val = value.ToString();
				}
				else if (_currentPositionState != value)
				{
					_currentPositionState = value;
					SyncPositionState();
				}
			}
		}
	}

	public bool isPositionOn => _currentPositionState == PositionState.On || _currentPositionState == PositionState.Comply || _currentPositionState == PositionState.Following || _currentPositionState == PositionState.Hold || _currentPositionState == PositionState.ParentLink || _currentPositionState == PositionState.PhysicsLink;

	public RotationState currentRotationState
	{
		get
		{
			return _currentRotationState;
		}
		set
		{
			if (stateCanBeModified)
			{
				if (currentRotationStateJSON != null)
				{
					currentRotationStateJSON.val = value.ToString();
				}
				else if (_currentRotationState != value)
				{
					_currentRotationState = value;
					SyncRotationState();
				}
			}
		}
	}

	public bool isRotationOn => _currentRotationState == RotationState.On || _currentRotationState == RotationState.Comply || _currentRotationState == RotationState.Following || _currentRotationState == RotationState.Hold || _currentRotationState == RotationState.LookAt || _currentRotationState == RotationState.ParentLink || _currentPositionState == PositionState.PhysicsLink;

	public bool xLock
	{
		get
		{
			return _xLock;
		}
		set
		{
			if (xLockJSON != null)
			{
				xLockJSON.val = value;
			}
			else if (_xLock != value)
			{
				SyncXLock(value);
			}
		}
	}

	public bool yLock
	{
		get
		{
			return _yLock;
		}
		set
		{
			if (yLockJSON != null)
			{
				yLockJSON.val = value;
			}
			else if (_yLock != value)
			{
				SyncYLock(value);
			}
		}
	}

	public bool zLock
	{
		get
		{
			return _zLock;
		}
		set
		{
			if (zLockJSON != null)
			{
				zLockJSON.val = value;
			}
			else if (_zLock != value)
			{
				SyncZLock(value);
			}
		}
	}

	public bool xLocalLock
	{
		get
		{
			return _xLocalLock;
		}
		set
		{
			if (xLocalLockJSON != null)
			{
				xLocalLockJSON.val = value;
			}
			else if (_xLocalLock != value)
			{
				SyncXLocalLock(value);
			}
		}
	}

	public bool yLocalLock
	{
		get
		{
			return _yLocalLock;
		}
		set
		{
			if (yLocalLockJSON != null)
			{
				yLocalLockJSON.val = value;
			}
			else if (_yLocalLock != value)
			{
				SyncYLocalLock(value);
			}
		}
	}

	public bool zLocalLock
	{
		get
		{
			return _zLocalLock;
		}
		set
		{
			if (zLocalLockJSON != null)
			{
				zLocalLockJSON.val = value;
			}
			else if (_zLocalLock != value)
			{
				SyncZLocalLock(value);
			}
		}
	}

	public bool xRotLock
	{
		get
		{
			return _xRotLock;
		}
		set
		{
			if (xRotLockJSON != null)
			{
				xRotLockJSON.val = value;
			}
			else if (_xRotLock != value)
			{
				SyncXRotLock(value);
			}
		}
	}

	public bool yRotLock
	{
		get
		{
			return _yRotLock;
		}
		set
		{
			if (yRotLockJSON != null)
			{
				yRotLockJSON.val = value;
			}
			else if (_yRotLock != value)
			{
				SyncYRotLock(value);
			}
		}
	}

	public bool zRotLock
	{
		get
		{
			return _zRotLock;
		}
		set
		{
			if (zRotLockJSON != null)
			{
				zRotLockJSON.val = value;
			}
			else if (_zRotLock != value)
			{
				SyncZRotLock(value);
			}
		}
	}

	public Transform ControlParentToUse
	{
		get
		{
			if (containingAtom != null && containingAtom.reParentObject != null && containingAtom.reParentObject == control.parent)
			{
				return containingAtom.reParentObject.parent;
			}
			return control.parent;
		}
	}

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
				SyncOn(value);
			}
		}
	}

	public bool interactableInPlayMode
	{
		get
		{
			return _interactableInPlayMode;
		}
		set
		{
			if (interactableInPlayModeJSON != null)
			{
				interactableInPlayModeJSON.val = value;
			}
			else if (_interactableInPlayMode != value)
			{
				SyncInteractableInPlayMode(value);
			}
		}
	}

	public bool possessed
	{
		get
		{
			return _possessed;
		}
		set
		{
			if (_possessed == value)
			{
				return;
			}
			_possessed = value;
			if (_possessed)
			{
				if (onPossessDeactiveList != null && _deactivateOtherControlsOnPossess)
				{
					FreeControllerV3[] array = onPossessDeactiveList;
					foreach (FreeControllerV3 freeControllerV in array)
					{
						freeControllerV.currentPositionState = PositionState.Off;
						freeControllerV.currentRotationState = RotationState.Off;
					}
				}
				if (onPossessStartHandlers != null)
				{
					onPossessStartHandlers(this);
				}
			}
			else if (onPossessEndHandlers != null)
			{
				onPossessEndHandlers(this);
			}
		}
	}

	public bool deactivateOtherControlsOnPossess
	{
		get
		{
			return _deactivateOtherControlsOnPossess;
		}
		set
		{
			if (deactivateOtherControlsOnPossessJSON != null)
			{
				deactivateOtherControlsOnPossessJSON.val = value;
			}
			else
			{
				SyncDeactivateOtherControlsOnPossess(value);
			}
		}
	}

	public bool possessable
	{
		get
		{
			return _possessable;
		}
		set
		{
			if (possessableJSON != null)
			{
				possessableJSON.val = value;
			}
			else if (_possessable != value)
			{
				SyncPossessable(value);
			}
		}
	}

	public bool canGrabPosition
	{
		get
		{
			return _canGrabPosition;
		}
		set
		{
			if (canGrabPositionJSON != null)
			{
				canGrabPositionJSON.val = value;
			}
			else if (_canGrabPosition != value)
			{
				SyncCanGrabPosition(value);
			}
		}
	}

	public bool canGrabRotation
	{
		get
		{
			return _canGrabRotation;
		}
		set
		{
			if (canGrabRotationJSON != null)
			{
				canGrabRotationJSON.val = value;
			}
			else if (_canGrabRotation != value)
			{
				SyncCanGrabRotation(value);
			}
		}
	}

	public GridMode positionGridMode
	{
		get
		{
			return _positionGridMode;
		}
		set
		{
			if (positionGridModeJSON != null)
			{
				positionGridModeJSON.val = value.ToString();
			}
			else if (_positionGridMode != value)
			{
				_positionGridMode = value;
			}
		}
	}

	public float positionGrid
	{
		get
		{
			return _positionGrid;
		}
		set
		{
			if (positionGridJSON != null)
			{
				positionGridJSON.val = value;
			}
			else
			{
				SyncPositionGrid(value);
			}
		}
	}

	public GridMode rotationGridMode
	{
		get
		{
			return _rotationGridMode;
		}
		set
		{
			if (rotationGridModeJSON != null)
			{
				rotationGridModeJSON.val = value.ToString();
			}
			else if (_rotationGridMode != value)
			{
				_rotationGridMode = value;
			}
		}
	}

	public float rotationGrid
	{
		get
		{
			return _rotationGrid;
		}
		set
		{
			if (rotationGridJSON != null)
			{
				rotationGridJSON.val = value;
			}
			else
			{
				SyncRotationGrid(value);
			}
		}
	}

	public bool useGravityOnRBWhenOff
	{
		get
		{
			return _useGravityOnRBWhenOff;
		}
		set
		{
			if (useGravityJSON != null)
			{
				useGravityJSON.val = value;
			}
			else if (_useGravityOnRBWhenOff != value)
			{
				SyncUseGravityOnRBWhenOff(value);
			}
		}
	}

	public bool physicsEnabled
	{
		get
		{
			if (_followWhenOffRB != null)
			{
				return !_followWhenOffRB.isKinematic;
			}
			return false;
		}
		set
		{
			if (physicsEnabledJSON != null)
			{
				physicsEnabledJSON.val = value;
			}
			else if (_followWhenOffRB != null && _followWhenOffRB.isKinematic == value)
			{
				SyncPhysicsEnabled(value);
			}
		}
	}

	public bool globalCollisionEnabled
	{
		get
		{
			return _globalCollisionEnabled;
		}
		set
		{
			if (_globalCollisionEnabled != value)
			{
				_globalCollisionEnabled = value;
				SyncCollisionEnabled(_collisionEnabled);
			}
		}
	}

	public override bool collisionEnabled
	{
		get
		{
			return _collisionEnabled;
		}
		set
		{
			if (collisionEnabledJSON != null)
			{
				collisionEnabledJSON.val = value;
			}
			else if (_collisionEnabled != value)
			{
				SyncCollisionEnabled(value);
			}
		}
	}

	public Rigidbody followWhenOffRB
	{
		get
		{
			return _followWhenOffRB;
		}
		set
		{
			if (!(_followWhenOffRB != value))
			{
				return;
			}
			_followWhenOffRB = value;
			followWhenOff = _followWhenOffRB.transform;
			ConfigurableJoint[] components = followWhenOff.GetComponents<ConfigurableJoint>();
			ConfigurableJoint[] array = components;
			foreach (ConfigurableJoint configurableJoint in array)
			{
				if (configurableJoint.connectedBody == kinematicRB)
				{
					connectedJoint = configurableJoint;
					SetJointSprings();
				}
			}
		}
	}

	public float RBMass
	{
		get
		{
			if (_followWhenOffRB != null)
			{
				return _followWhenOffRB.mass;
			}
			return 0f;
		}
		set
		{
			if (RBMassJSON != null)
			{
				RBMassJSON.val = value;
			}
			else if (_followWhenOffRB != null && _followWhenOffRB.mass != value)
			{
				SyncRBMass(value);
			}
		}
	}

	public float RBDrag
	{
		get
		{
			if (_followWhenOffRB != null)
			{
				return _followWhenOffRB.drag;
			}
			return 0f;
		}
		set
		{
			if (RBDragJSON != null)
			{
				RBDragJSON.val = value;
			}
			else if (_followWhenOffRB != null && _followWhenOffRB.drag != value)
			{
				SyncRBDrag(value);
			}
		}
	}

	public bool RBMaxVelocityEnable
	{
		get
		{
			return _RBMaxVelocityEnable;
		}
		set
		{
			if (RBMaxVelocityEnableJSON != null)
			{
				RBMaxVelocityEnableJSON.val = value;
			}
			else
			{
				_RBMaxVelocityEnable = value;
			}
		}
	}

	public float RBMaxVelocity
	{
		get
		{
			return _RBMaxVelocity;
		}
		set
		{
			if (RBMaxVelocityJSON != null)
			{
				RBMaxVelocityJSON.val = value;
			}
			else
			{
				_RBMaxVelocity = value;
			}
		}
	}

	public float RBAngularDrag
	{
		get
		{
			if (_followWhenOffRB != null)
			{
				return _followWhenOffRB.angularDrag;
			}
			return 0f;
		}
		set
		{
			if (RBAngularDragJSON != null)
			{
				RBAngularDragJSON.val = value;
			}
			else if (_followWhenOffRB != null && _followWhenOffRB.angularDrag != value)
			{
				SyncRBAngularDrag(value);
			}
		}
	}

	public float RBLockPositionSpring
	{
		get
		{
			return _RBLockPositionSpring;
		}
		set
		{
			if (_RBLockPositionSpring != value)
			{
				_RBLockPositionSpring = value;
				SetJointSprings();
			}
		}
	}

	public float RBLockPositionDamper
	{
		get
		{
			return _RBLockPositionDamper;
		}
		set
		{
			if (_RBLockPositionDamper != value)
			{
				_RBLockPositionDamper = value;
				SetJointSprings();
			}
		}
	}

	public float RBLockPositionMaxForce
	{
		get
		{
			return _RBLockPositionMaxForce;
		}
		set
		{
			if (_RBLockPositionMaxForce != value)
			{
				_RBLockPositionMaxForce = value;
				SetJointSprings();
			}
		}
	}

	public float RBHoldPositionSpring
	{
		get
		{
			return _RBHoldPositionSpring;
		}
		set
		{
			if (RBHoldPositionSpringJSON != null)
			{
				RBHoldPositionSpringJSON.val = value;
			}
			else if (_RBHoldPositionSpring != value)
			{
				SyncRBHoldPositionSpring(value);
			}
		}
	}

	public float RBHoldPositionDamper
	{
		get
		{
			return _RBHoldPositionDamper;
		}
		set
		{
			if (RBHoldPositionDamperJSON != null)
			{
				RBHoldPositionDamperJSON.val = value;
			}
			else if (_RBHoldPositionDamper != value)
			{
				SyncRBHoldPositionDamper(value);
			}
		}
	}

	public float RBHoldPositionMaxForce
	{
		get
		{
			return _RBHoldPositionMaxForce;
		}
		set
		{
			if (RBHoldPositionMaxForceJSON != null)
			{
				RBHoldPositionMaxForceJSON.val = value;
			}
			else if (_RBHoldPositionMaxForce != value)
			{
				SyncRBHoldPositionMaxForce(value);
				SetJointSprings();
			}
		}
	}

	public float RBComplyPositionSpring
	{
		get
		{
			return _RBComplyPositionSpring;
		}
		set
		{
			if (RBComplyPositionSpringJSON != null)
			{
				RBComplyPositionSpringJSON.val = value;
			}
			else if (_RBComplyPositionSpring != value)
			{
				SyncRBComplyPositionSpring(value);
			}
		}
	}

	public float RBComplyPositionDamper
	{
		get
		{
			return _RBComplyPositionDamper;
		}
		set
		{
			if (RBComplyPositionDamperJSON != null)
			{
				RBComplyPositionDamperJSON.val = value;
			}
			else if (_RBComplyPositionDamper != value)
			{
				SyncRBComplyPositionDamper(value);
			}
		}
	}

	public float RBLinkPositionSpring
	{
		get
		{
			return _RBLinkPositionSpring;
		}
		set
		{
			if (RBLinkPositionSpringJSON != null)
			{
				RBLinkPositionSpringJSON.val = value;
			}
			else if (_RBLinkPositionSpring != value)
			{
				SyncRBLinkPositionSpring(value);
			}
		}
	}

	public float RBLinkPositionDamper
	{
		get
		{
			return _RBLinkPositionDamper;
		}
		set
		{
			if (RBLinkPositionDamperJSON != null)
			{
				RBLinkPositionDamperJSON.val = value;
			}
			else if (_RBLinkPositionDamper != value)
			{
				SyncRBLinkPositionDamper(value);
			}
		}
	}

	public float RBLinkPositionMaxForce
	{
		get
		{
			return _RBLinkPositionMaxForce;
		}
		set
		{
			if (RBLinkPositionMaxForceJSON != null)
			{
				RBLinkPositionMaxForceJSON.val = value;
			}
			else if (_RBLinkPositionMaxForce != value)
			{
				SyncRBLinkPositionMaxForce(value);
			}
		}
	}

	public float RBLockRotationSpring
	{
		get
		{
			return _RBLockRotationSpring;
		}
		set
		{
			if (_RBLockRotationSpring != value)
			{
				_RBLockRotationSpring = value;
				SetJointSprings();
			}
		}
	}

	public float RBLockRotationDamper
	{
		get
		{
			return _RBLockRotationDamper;
		}
		set
		{
			if (_RBLockRotationDamper != value)
			{
				_RBLockRotationDamper = value;
				SetJointSprings();
			}
		}
	}

	public float RBLockRotationMaxForce
	{
		get
		{
			return _RBLockRotationMaxForce;
		}
		set
		{
			if (_RBLockRotationMaxForce != value)
			{
				_RBLockRotationMaxForce = value;
				SetJointSprings();
			}
		}
	}

	public float RBHoldRotationSpring
	{
		get
		{
			return _RBHoldRotationSpring;
		}
		set
		{
			if (RBHoldRotationSpringJSON != null)
			{
				RBHoldRotationSpringJSON.val = value;
			}
			else if (_RBHoldRotationSpring != value)
			{
				SyncRBHoldRotationSpring(value);
			}
		}
	}

	public float RBHoldRotationDamper
	{
		get
		{
			return _RBHoldRotationDamper;
		}
		set
		{
			if (RBHoldRotationDamperJSON != null)
			{
				RBHoldRotationDamperJSON.val = value;
			}
			else if (_RBHoldRotationDamper != value)
			{
				SyncRBHoldRotationSpring(value);
			}
		}
	}

	public float RBHoldRotationMaxForce
	{
		get
		{
			return _RBHoldRotationMaxForce;
		}
		set
		{
			if (RBHoldRotationMaxForceJSON != null)
			{
				RBHoldRotationMaxForceJSON.val = value;
			}
			else if (_RBHoldRotationMaxForce != value)
			{
				SyncRBHoldRotationMaxForce(value);
			}
		}
	}

	public float RBComplyRotationSpring
	{
		get
		{
			return _RBComplyRotationSpring;
		}
		set
		{
			if (RBComplyRotationSpringJSON != null)
			{
				RBComplyRotationSpringJSON.val = value;
			}
			else
			{
				SyncRBComplyRotationSpring(value);
			}
		}
	}

	public float RBComplyRotationDamper
	{
		get
		{
			return _RBComplyRotationDamper;
		}
		set
		{
			if (RBComplyRotationDamperJSON != null)
			{
				RBComplyRotationDamperJSON.val = value;
			}
			else if (_RBComplyRotationDamper != value)
			{
				SyncRBComplyRotationDamper(value);
			}
		}
	}

	public float RBLinkRotationSpring
	{
		get
		{
			return _RBLinkRotationSpring;
		}
		set
		{
			if (RBLinkRotationSpringJSON != null)
			{
				RBLinkRotationSpringJSON.val = value;
			}
			else
			{
				SyncRBLinkRotationSpring(value);
			}
		}
	}

	public float RBLinkRotationDamper
	{
		get
		{
			return _RBLinkRotationDamper;
		}
		set
		{
			if (RBLinkRotationDamperJSON != null)
			{
				RBLinkRotationDamperJSON.val = value;
			}
			else if (_RBLinkRotationDamper != value)
			{
				SyncRBLinkRotationDamper(value);
			}
		}
	}

	public float RBLinkRotationMaxForce
	{
		get
		{
			return _RBLinkRotationMaxForce;
		}
		set
		{
			if (RBLinkRotationMaxForceJSON != null)
			{
				RBLinkRotationMaxForceJSON.val = value;
			}
			else if (_RBLinkRotationMaxForce != value)
			{
				SyncRBLinkRotationMaxForce(value);
			}
		}
	}

	public float RBComplyJointRotationDriveSpring
	{
		get
		{
			return _RBComplyJointRotationDriveSpring;
		}
		set
		{
			if (RBComplyJointRotationDriveSpringJSON != null)
			{
				RBComplyJointRotationDriveSpringJSON.val = value;
			}
			else
			{
				SyncRBComplyJointRotationDriveSpring(value);
			}
		}
	}

	public float jointRotationDriveSpring
	{
		get
		{
			return _jointRotationDriveSpring;
		}
		set
		{
			if (jointRotationDriveSpringJSON != null)
			{
				jointRotationDriveSpringJSON.val = value;
			}
			else if (_jointRotationDriveSpring != value)
			{
				SyncJointRotationDriveSpring(value);
				SetNaturalJointDrive();
			}
		}
	}

	public float jointRotationDriveDamper
	{
		get
		{
			return _jointRotationDriveDamper;
		}
		set
		{
			if (jointRotationDriveDamperJSON != null)
			{
				jointRotationDriveDamperJSON.val = value;
			}
			else if (_jointRotationDriveDamper != value)
			{
				SyncJointRotationDriveDamper(value);
			}
		}
	}

	public float jointRotationDriveMaxForce
	{
		get
		{
			return _jointRotationDriveMaxForce;
		}
		set
		{
			if (jointRotationDriveMaxForceJSON != null)
			{
				jointRotationDriveMaxForceJSON.val = value;
			}
			else if (_jointRotationDriveMaxForce != value)
			{
				SyncJointRotationDriveMaxForce(value);
			}
		}
	}

	public float jointRotationDriveXTarget
	{
		get
		{
			return _jointRotationDriveXTarget;
		}
		set
		{
			if (jointRotationDriveXTargetJSON != null)
			{
				jointRotationDriveXTargetJSON.val = value;
			}
			else if (_jointRotationDriveXTarget != value)
			{
				SyncJointRotationDriveXTarget(value);
			}
		}
	}

	public float jointRotationDriveXTargetAdditional
	{
		get
		{
			return _jointRotationDriveXTargetAdditional;
		}
		set
		{
			if (_jointRotationDriveXTargetAdditional != value)
			{
				_jointRotationDriveXTargetAdditional = value;
				SetNaturalJointDriveTarget();
			}
		}
	}

	public float jointRotationDriveYTarget
	{
		get
		{
			return _jointRotationDriveYTarget;
		}
		set
		{
			if (jointRotationDriveYTargetJSON != null)
			{
				jointRotationDriveYTargetJSON.val = value;
			}
			else if (_jointRotationDriveYTarget != value)
			{
				SyncJointRotationDriveYTarget(value);
			}
		}
	}

	public float jointRotationDriveYTargetAdditional
	{
		get
		{
			return _jointRotationDriveYTargetAdditional;
		}
		set
		{
			if (_jointRotationDriveYTargetAdditional != value)
			{
				_jointRotationDriveYTargetAdditional = value;
				SetNaturalJointDriveTarget();
			}
		}
	}

	public float jointRotationDriveZTarget
	{
		get
		{
			return _jointRotationDriveZTarget;
		}
		set
		{
			if (jointRotationDriveZTargetJSON != null)
			{
				jointRotationDriveZTargetJSON.val = value;
			}
			else if (_jointRotationDriveZTarget != value)
			{
				SyncJointRotationDriveZTarget(value);
				SetNaturalJointDriveTarget();
			}
		}
	}

	public float jointRotationDriveZTargetAdditional
	{
		get
		{
			return _jointRotationDriveZTargetAdditional;
		}
		set
		{
			if (_jointRotationDriveZTargetAdditional != value)
			{
				_jointRotationDriveZTargetAdditional = value;
				SetNaturalJointDriveTarget();
			}
		}
	}

	public bool hidden
	{
		get
		{
			return _hidden;
		}
		set
		{
			_hidden = value;
			if (_hidden)
			{
				if (mrs != null)
				{
					MeshRenderer[] array = mrs;
					foreach (MeshRenderer meshRenderer in array)
					{
						meshRenderer.enabled = false;
					}
				}
			}
			else if (mrs != null)
			{
				MeshRenderer[] array2 = mrs;
				foreach (MeshRenderer meshRenderer2 in array2)
				{
					meshRenderer2.enabled = true;
				}
			}
		}
	}

	public bool guihidden
	{
		get
		{
			return _guihidden;
		}
		set
		{
			_guihidden = value;
			if (_guihidden)
			{
				if (!GUIalwaysVisibleWhenSelected || !_selected)
				{
					HideGUI();
				}
			}
			else if (_selected)
			{
				ShowGUI();
			}
		}
	}

	public bool highlighted
	{
		get
		{
			return _highlighted;
		}
		set
		{
			_highlighted = value;
			SetColor();
		}
	}

	public Vector3 selectedPosition => _selectedPosition;

	public bool selected
	{
		get
		{
			return _selected;
		}
		set
		{
			_selected = value;
			if (_selected)
			{
				if (!_guihidden || GUIalwaysVisibleWhenSelected)
				{
					ShowGUI();
				}
				_selectedPosition = control.position;
			}
			else
			{
				HideGUI();
			}
			SetColor();
		}
	}

	public Color currentPositionColor => _currentPositionColor;

	public Color currentRotationColor => _currentRotationColor;

	public ControlMode controlMode
	{
		get
		{
			return _controlMode;
		}
		set
		{
			switch (value)
			{
			case ControlMode.Position:
				if (_moveEnabled || _moveForceEnabled)
				{
					_controlMode = value;
				}
				break;
			case ControlMode.Rotation:
				if (_rotationEnabled || _rotationForceEnabled)
				{
					_controlMode = value;
				}
				break;
			default:
				_controlMode = ControlMode.Off;
				break;
			}
		}
	}

	protected void RegisterAllocatedObject(UnityEngine.Object o)
	{
		if (Application.isPlaying)
		{
			if (allocatedObjects == null)
			{
				allocatedObjects = new List<UnityEngine.Object>();
			}
			allocatedObjects.Add(o);
		}
	}

	protected void DestroyAllocatedObjects()
	{
		if (!Application.isPlaying || allocatedObjects == null)
		{
			return;
		}
		foreach (UnityEngine.Object allocatedObject in allocatedObjects)
		{
			UnityEngine.Object.Destroy(allocatedObject);
		}
	}

	public override string[] GetCustomParamNames()
	{
		return customParamNames;
	}

	public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = true)
	{
		JSONClass jSON = base.GetJSON(includePhysical, includeAppearance, forceStore);
		if (includePhysical || forceStore)
		{
			needsStore = true;
			if (storePositionRotationAsLocal || _forceStorePositionRotationAsLocal)
			{
				Vector3 localPosition = base.transform.localPosition;
				jSON["localPosition"]["x"].AsFloat = localPosition.x;
				jSON["localPosition"]["y"].AsFloat = localPosition.y;
				jSON["localPosition"]["z"].AsFloat = localPosition.z;
				Vector3 localEulerAngles = base.transform.localEulerAngles;
				jSON["localRotation"]["x"].AsFloat = localEulerAngles.x;
				jSON["localRotation"]["y"].AsFloat = localEulerAngles.y;
				jSON["localRotation"]["z"].AsFloat = localEulerAngles.z;
			}
			else
			{
				Vector3 position = base.transform.position;
				jSON["position"]["x"].AsFloat = position.x;
				jSON["position"]["y"].AsFloat = position.y;
				jSON["position"]["z"].AsFloat = position.z;
				Vector3 eulerAngles = base.transform.eulerAngles;
				jSON["rotation"]["x"].AsFloat = eulerAngles.x;
				jSON["rotation"]["y"].AsFloat = eulerAngles.y;
				jSON["rotation"]["z"].AsFloat = eulerAngles.z;
			}
			if (_linkToRB != null && linkToAtomUID != null && linkToAtomUID != "[CameraRig]")
			{
				string text = AtomUidToStoreAtomUid(linkToAtomUID);
				if (text != null)
				{
					jSON["linkTo"] = text + ":" + _linkToRB.name;
				}
				else
				{
					SuperController.LogError(string.Concat("Warning: FreeController ", containingAtom, ":", base.name, " is linked to object in atom ", linkToAtomUID, " that is not in subscene and cannot be saved"));
				}
			}
		}
		return jSON;
	}

	public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
	{
		if (!base.physicalLocked && restorePhysical && !IsCustomPhysicalParamLocked("linkTo"))
		{
			SelectLinkToRigidbody(null);
		}
		base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
		if (base.physicalLocked || !restorePhysical)
		{
			return;
		}
		PauseComply();
		bool flag = false;
		if (jc["position"] != null)
		{
			if (!IsCustomPhysicalParamLocked("position"))
			{
				Vector3 position = base.transform.position;
				if (jc["position"]["x"] != null)
				{
					position.x = jc["position"]["x"].AsFloat;
				}
				if (jc["position"]["y"] != null)
				{
					position.y = jc["position"]["y"].AsFloat;
				}
				if (jc["position"]["z"] != null)
				{
					position.z = jc["position"]["z"].AsFloat;
				}
				base.transform.position = position;
				if (control != null)
				{
					control.position = position;
					flag = true;
					if (onPositionChangeHandlers != null)
					{
						onPositionChangeHandlers(this);
					}
				}
				if (followWhenOff != null && !followWhenOff.GetComponent<JSONStorable>() && !_detachControl)
				{
					followWhenOff.position = position;
				}
			}
		}
		else if (jc["localPosition"] != null)
		{
			if (!IsCustomPhysicalParamLocked("localPosition"))
			{
				Vector3 localPosition = base.transform.localPosition;
				if (jc["localPosition"]["x"] != null)
				{
					localPosition.x = jc["localPosition"]["x"].AsFloat;
				}
				if (jc["localPosition"]["y"] != null)
				{
					localPosition.y = jc["localPosition"]["y"].AsFloat;
				}
				if (jc["localPosition"]["z"] != null)
				{
					localPosition.z = jc["localPosition"]["z"].AsFloat;
				}
				base.transform.localPosition = localPosition;
				if (control != null)
				{
					control.position = base.transform.position;
					flag = true;
					if (onPositionChangeHandlers != null)
					{
						onPositionChangeHandlers(this);
					}
				}
				if (followWhenOff != null && !followWhenOff.GetComponent<JSONStorable>() && !_detachControl)
				{
					followWhenOff.position = base.transform.position;
				}
			}
		}
		else if (setMissingToDefault && !IsCustomPhysicalParamLocked("position") && !IsCustomPhysicalParamLocked("localPosition"))
		{
			if (storePositionRotationAsLocal || _forceStorePositionRotationAsLocal)
			{
				base.transform.localPosition = startingLocalPosition;
			}
			else
			{
				base.transform.position = startingPosition;
			}
			if (control != null)
			{
				control.position = base.transform.position;
				flag = true;
				if (onPositionChangeHandlers != null)
				{
					onPositionChangeHandlers(this);
				}
			}
			if (followWhenOff != null && !followWhenOff.GetComponent<JSONStorable>() && !_detachControl)
			{
				followWhenOff.position = base.transform.position;
			}
		}
		if (jc["rotation"] != null)
		{
			if (!IsCustomPhysicalParamLocked("rotation"))
			{
				Vector3 eulerAngles = base.transform.eulerAngles;
				if (jc["rotation"]["x"] != null)
				{
					eulerAngles.x = jc["rotation"]["x"].AsFloat;
				}
				if (jc["rotation"]["y"] != null)
				{
					eulerAngles.y = jc["rotation"]["y"].AsFloat;
				}
				if (jc["rotation"]["z"] != null)
				{
					eulerAngles.z = jc["rotation"]["z"].AsFloat;
				}
				base.transform.eulerAngles = eulerAngles;
				if (control != null)
				{
					control.rotation = base.transform.rotation;
					flag = true;
					if (onRotationChangeHandlers != null)
					{
						onRotationChangeHandlers(this);
					}
				}
				if (followWhenOff != null && !followWhenOff.GetComponent<JSONStorable>() && !_detachControl)
				{
					followWhenOff.rotation = base.transform.rotation;
				}
			}
		}
		else if (jc["localRotation"] != null)
		{
			if (!IsCustomPhysicalParamLocked("localRotation"))
			{
				Vector3 localEulerAngles = base.transform.localEulerAngles;
				if (jc["localRotation"]["x"] != null)
				{
					localEulerAngles.x = jc["localRotation"]["x"].AsFloat;
				}
				if (jc["localRotation"]["y"] != null)
				{
					localEulerAngles.y = jc["localRotation"]["y"].AsFloat;
				}
				if (jc["localRotation"]["z"] != null)
				{
					localEulerAngles.z = jc["localRotation"]["z"].AsFloat;
				}
				base.transform.localEulerAngles = localEulerAngles;
				if (control != null)
				{
					control.rotation = base.transform.rotation;
					flag = true;
					if (onRotationChangeHandlers != null)
					{
						onRotationChangeHandlers(this);
					}
				}
				if (followWhenOff != null && !followWhenOff.GetComponent<JSONStorable>() && !_detachControl)
				{
					followWhenOff.rotation = base.transform.rotation;
				}
			}
		}
		else if (setMissingToDefault && !IsCustomPhysicalParamLocked("rotation") && !IsCustomPhysicalParamLocked("localRotation"))
		{
			if (storePositionRotationAsLocal || _forceStorePositionRotationAsLocal)
			{
				base.transform.localRotation = startingLocalRotation;
			}
			else
			{
				base.transform.rotation = startingRotation;
			}
			if (control != null)
			{
				control.rotation = base.transform.rotation;
				flag = true;
				if (onRotationChangeHandlers != null)
				{
					onRotationChangeHandlers(this);
				}
			}
			if (followWhenOff != null && !followWhenOff.GetComponent<JSONStorable>() && !_detachControl)
			{
				followWhenOff.rotation = base.transform.rotation;
			}
		}
		if (flag)
		{
			SyncMoveWhenInactive();
			if (onMovementHandlers != null)
			{
				onMovementHandlers(this);
			}
		}
	}

	public override void LateRestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
	{
		base.LateRestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
		if (base.physicalLocked || !restorePhysical || IsCustomPhysicalParamLocked("linkTo"))
		{
			return;
		}
		if (jc["linkTo"] != null)
		{
			if (SuperController.singleton != null)
			{
				string rigidbodyName = StoredAtomUidToAtomUid(jc["linkTo"]);
				Rigidbody rb = SuperController.singleton.RigidbodyNameToRigidbody(rigidbodyName);
				SelectLinkToRigidbody(rb, SelectLinkState.PositionAndRotation, false, false);
			}
		}
		else if (setMissingToDefault)
		{
			if (startingLinkToRigidbody != null)
			{
				SelectLinkToRigidbody(startingLinkToRigidbody, SelectLinkState.PositionAndRotation, false, false);
			}
			else
			{
				SelectLinkToRigidbody(null);
			}
		}
		jointRotationDriveXTargetJSON.RestoreFromJSON(jc);
	}

	public void Reset()
	{
		JSONStorable[] components = GetComponents<JSONStorable>();
		JSONStorable[] array = components;
		foreach (JSONStorable jSONStorable in array)
		{
			if (!jSONStorable.exclude)
			{
				jSONStorable.RestoreAllFromDefaults();
			}
		}
		if (!(followWhenOff != null))
		{
			return;
		}
		components = followWhenOff.GetComponents<JSONStorable>();
		JSONStorable[] array2 = components;
		foreach (JSONStorable jSONStorable2 in array2)
		{
			if (!jSONStorable2.exclude)
			{
				jSONStorable2.RestoreAllFromDefaults();
			}
		}
	}

	public void SelectRoot()
	{
		if (enableSelectRoot && containingAtom != null && containingAtom.mainController != null)
		{
			SuperController.singleton.SelectController(containingAtom.mainController);
		}
	}

	public void MovePlayerTo()
	{
	}

	public void MovePlayerToAndControl()
	{
	}

	protected virtual void SetLinkToAtomNames()
	{
		if (!(linkToAtomSelectionPopup != null) || !(SuperController.singleton != null))
		{
			return;
		}
		List<string> atomUIDsWithRigidbodies = SuperController.singleton.GetAtomUIDsWithRigidbodies();
		if (atomUIDsWithRigidbodies == null)
		{
			linkToAtomSelectionPopup.numPopupValues = 1;
			linkToAtomSelectionPopup.setPopupValue(0, "None");
			return;
		}
		linkToAtomSelectionPopup.numPopupValues = atomUIDsWithRigidbodies.Count + 1;
		linkToAtomSelectionPopup.setPopupValue(0, "None");
		for (int i = 0; i < atomUIDsWithRigidbodies.Count; i++)
		{
			linkToAtomSelectionPopup.setPopupValue(i + 1, atomUIDsWithRigidbodies[i]);
		}
	}

	protected void OnAtomUIDRename(string fromid, string toid)
	{
		SyncAtomUID();
		if (linkToAtomUID == fromid)
		{
			linkToAtomUID = toid;
			if (linkToAtomSelectionPopup != null)
			{
				linkToAtomSelectionPopup.currentValueNoCallback = toid;
			}
		}
	}

	protected void onLinkToRigidbodyNamesChanged(List<string> rbNames)
	{
		if (!(linkToSelectionPopup != null))
		{
			return;
		}
		if (rbNames == null)
		{
			linkToSelectionPopup.numPopupValues = 1;
			linkToSelectionPopup.setPopupValue(0, "None");
			return;
		}
		linkToSelectionPopup.numPopupValues = rbNames.Count + 1;
		linkToSelectionPopup.setPopupValue(0, "None");
		for (int i = 0; i < rbNames.Count; i++)
		{
			linkToSelectionPopup.setPopupValue(i + 1, rbNames[i]);
		}
	}

	public virtual void SetLinkToAtom(string atomUID)
	{
		if (!(SuperController.singleton != null))
		{
			return;
		}
		Atom atomByUid = SuperController.singleton.GetAtomByUid(atomUID);
		if (atomByUid != null)
		{
			linkToAtomUID = atomUID;
			List<string> rigidbodyNamesInAtom = SuperController.singleton.GetRigidbodyNamesInAtom(linkToAtomUID);
			onLinkToRigidbodyNamesChanged(rigidbodyNamesInAtom);
			if (linkToSelectionPopup != null)
			{
				linkToSelectionPopup.currentValue = "None";
			}
		}
	}

	public void SetLinkToRigidbody(string rigidbodyName)
	{
		if (SuperController.singleton != null)
		{
			Rigidbody rigidbody = SuperController.singleton.RigidbodyNameToRigidbody(rigidbodyName);
			linkToRB = rigidbody;
		}
	}

	public virtual void SetLinkToRigidbodyObject(string objectName)
	{
		if (linkToAtomUID != null)
		{
			SetLinkToRigidbody(linkToAtomUID + ":" + objectName);
		}
	}

	private void GetLinkToAtomUIDFromLinkToRB(Rigidbody rb)
	{
		linkToAtomUID = "None";
		Atom atom = null;
		FreeControllerV3 component = rb.GetComponent<FreeControllerV3>();
		if (component != null)
		{
			atom = component.containingAtom;
		}
		else
		{
			ForceReceiver component2 = rb.GetComponent<ForceReceiver>();
			if (component2 != null)
			{
				atom = component2.containingAtom;
			}
		}
		if (atom != null)
		{
			linkToAtomUID = atom.uid;
		}
	}

	public void SelectLinkToRigidbody(Rigidbody rb)
	{
		SelectLinkToRigidbody(rb, SelectLinkState.PositionAndRotation);
	}

	public void SelectLinkToRigidbody(Rigidbody rb, SelectLinkState linkState, bool usePhysicalLink = false, bool modifyState = true)
	{
		if (rb != null)
		{
			preLinkRB = linkToRB;
			preLinkPositionState = currentPositionState;
			preLinkRotationState = currentRotationState;
			GetLinkToAtomUIDFromLinkToRB(rb);
			if (linkToAtomSelectionPopup != null)
			{
				linkToAtomSelectionPopup.currentValue = linkToAtomUID;
			}
		}
		else if (linkToAtomSelectionPopup != null)
		{
			linkToAtomSelectionPopup.currentValue = "None";
		}
		if (linkToSelectionPopup != null)
		{
			if (rb != null)
			{
				linkToSelectionPopup.currentValueNoCallback = rb.name;
			}
			else
			{
				linkToSelectionPopup.currentValueNoCallback = "None";
			}
		}
		if (rb != null)
		{
			linkToRB = rb;
			if (!modifyState)
			{
				return;
			}
			if (linkState == SelectLinkState.Position || linkState == SelectLinkState.PositionAndRotation)
			{
				if (usePhysicalLink && _currentPositionState != PositionState.PhysicsLink)
				{
					currentPositionState = PositionState.PhysicsLink;
				}
				else if (_currentPositionState != PositionState.ParentLink && _currentPositionState != PositionState.PhysicsLink)
				{
					currentPositionState = PositionState.ParentLink;
				}
			}
			if (linkState == SelectLinkState.Rotation || linkState == SelectLinkState.PositionAndRotation)
			{
				if (usePhysicalLink && _currentRotationState != RotationState.PhysicsLink)
				{
					currentRotationState = RotationState.PhysicsLink;
				}
				else if (_currentRotationState != RotationState.ParentLink && _currentRotationState != RotationState.PhysicsLink)
				{
					currentRotationState = RotationState.ParentLink;
				}
			}
			return;
		}
		linkToRB = null;
		if (modifyState)
		{
			if (_currentPositionState == PositionState.ParentLink || _currentPositionState == PositionState.PhysicsLink)
			{
				currentPositionState = preLinkPositionState;
			}
			if (_currentRotationState == RotationState.ParentLink || _currentRotationState == RotationState.PhysicsLink)
			{
				currentRotationState = preLinkRotationState;
			}
		}
	}

	public void RestorePreLinkState()
	{
		if (preLinkRB != null)
		{
			GetLinkToAtomUIDFromLinkToRB(preLinkRB);
			if (linkToAtomUID == "None")
			{
				if (linkToAtomSelectionPopup != null)
				{
					linkToAtomSelectionPopup.currentValue = "None";
				}
				if (linkToSelectionPopup != null)
				{
					linkToSelectionPopup.currentValueNoCallback = "None";
				}
			}
			else
			{
				if (linkToAtomSelectionPopup != null)
				{
					linkToAtomSelectionPopup.currentValue = linkToAtomUID;
				}
				if (linkToSelectionPopup != null)
				{
					linkToSelectionPopup.currentValueNoCallback = preLinkRB.name;
				}
			}
		}
		else
		{
			if (linkToAtomSelectionPopup != null)
			{
				linkToAtomSelectionPopup.currentValue = "None";
			}
			if (linkToSelectionPopup != null)
			{
				linkToSelectionPopup.currentValueNoCallback = "None";
			}
		}
		if (preLinkRB != null && linkToAtomUID != "None")
		{
			linkToRB = preLinkRB;
			if (currentPositionState != preLinkPositionState)
			{
				currentPositionState = preLinkPositionState;
			}
			if (currentRotationState != preLinkRotationState)
			{
				currentRotationState = preLinkRotationState;
			}
		}
		else
		{
			linkToRB = null;
			if (_currentPositionState == PositionState.ParentLink || _currentPositionState == PositionState.PhysicsLink)
			{
				currentPositionState = preLinkPositionState;
			}
			if (_currentRotationState == RotationState.ParentLink || _currentRotationState == RotationState.PhysicsLink)
			{
				currentRotationState = preLinkRotationState;
			}
		}
	}

	protected void SyncGrabFreezePhysics()
	{
		if (freezeAtomPhysicsWhenGrabbed)
		{
			if (_isGrabbing && !_detachControl)
			{
				containingAtom.grabFreezePhysics = true;
			}
			else
			{
				containingAtom.grabFreezePhysics = false;
			}
		}
	}

	public void SelectLinkToRigidbodyFromScene()
	{
		SetLinkToAtomNames();
		SuperController.singleton.SelectModeRigidbody(SelectLinkToRigidbody);
	}

	public void SelectAlignToRigidbody(Rigidbody rb)
	{
		control.position = rb.transform.position;
		control.rotation = rb.transform.rotation;
		if (onPositionChangeHandlers != null)
		{
			onPositionChangeHandlers(this);
		}
		if (onRotationChangeHandlers != null)
		{
			onRotationChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void SelectAlignToRigidbodyFromScene()
	{
		SuperController.singleton.SelectModeRigidbody(SelectAlignToRigidbody);
	}

	public void SetPositionStateFromString(string state)
	{
		try
		{
			PositionState positionState = (PositionState)Enum.Parse(typeof(PositionState), state);
			_currentPositionState = positionState;
			SyncPositionState();
		}
		catch (ArgumentException)
		{
			Debug.LogError("State " + state + " is not a valid position state");
		}
	}

	private void SyncPositionState()
	{
		switch (_currentPositionState)
		{
		case PositionState.On:
		case PositionState.Following:
		case PositionState.Hold:
		case PositionState.Lock:
		case PositionState.ParentLink:
		case PositionState.PhysicsLink:
		case PositionState.Comply:
			if (_followWhenOffRB != null)
			{
				_followWhenOffRB.useGravity = _useGravityOnRBWhenOff;
			}
			break;
		case PositionState.Off:
			if (_followWhenOffRB != null)
			{
				_followWhenOffRB.useGravity = _useGravityOnRBWhenOff;
			}
			break;
		}
		switch (_currentPositionState)
		{
		case PositionState.On:
		case PositionState.Comply:
			_moveEnabled = true;
			_moveForceEnabled = false;
			break;
		case PositionState.Off:
		case PositionState.Following:
		case PositionState.Hold:
			_moveEnabled = useForceWhenOff;
			_moveForceEnabled = useForceWhenOff;
			break;
		case PositionState.ParentLink:
		case PositionState.PhysicsLink:
			_moveEnabled = useForceWhenOff;
			_moveForceEnabled = useForceWhenOff;
			if (_linkToConnector != null)
			{
				_linkToConnector.position = base.transform.position;
			}
			if (_linkToJoint != null)
			{
				_linkToJoint.connectedBody = null;
				_linkToJoint.connectedBody = _linkToRB;
			}
			break;
		case PositionState.Lock:
			_moveEnabled = false;
			_moveForceEnabled = false;
			break;
		}
		if (kinematicRB != null)
		{
			kinematicRB.isKinematic = false;
			kinematicRB.isKinematic = true;
		}
		SetLinkedJointSprings();
		SetJointSprings();
		StateChanged();
	}

	public void SetRotationStateFromString(string state)
	{
		try
		{
			RotationState rotationState = (RotationState)Enum.Parse(typeof(RotationState), state);
			_currentRotationState = rotationState;
			SyncRotationState();
		}
		catch (ArgumentException)
		{
			Debug.LogError("State " + state + " is not a valid rotation state");
		}
	}

	private void SyncRotationState()
	{
		switch (_currentRotationState)
		{
		case RotationState.On:
		case RotationState.Comply:
			_rotationEnabled = true;
			_rotationForceEnabled = false;
			break;
		case RotationState.Off:
		case RotationState.Following:
		case RotationState.Hold:
		case RotationState.LookAt:
			_rotationEnabled = useForceWhenOff;
			_rotationForceEnabled = useForceWhenOff;
			break;
		case RotationState.ParentLink:
		case RotationState.PhysicsLink:
			_rotationEnabled = useForceWhenOff;
			_rotationForceEnabled = useForceWhenOff;
			if (_linkToConnector != null)
			{
				_linkToConnector.rotation = base.transform.rotation;
			}
			if (_linkToJoint != null)
			{
				_linkToJoint.connectedBody = null;
				_linkToJoint.connectedBody = _linkToRB;
			}
			break;
		case RotationState.Lock:
			_rotationEnabled = false;
			_rotationForceEnabled = false;
			break;
		}
		if (kinematicRB != null)
		{
			kinematicRB.isKinematic = false;
			kinematicRB.isKinematic = true;
		}
		SetLinkedJointSprings();
		SetJointSprings();
		SetNaturalJointDrive();
		StateChanged();
	}

	public override void ScaleChanged(float scale)
	{
		base.ScaleChanged(scale);
		PauseComply(10);
		scalePow = Mathf.Pow(1.7f, scale - 1f);
		SetJointSprings();
		SetNaturalJointDrive();
	}

	private void SetLinkedJointSprings()
	{
		if (_linkToJoint != null)
		{
			JointDrive xDrive = _linkToJoint.xDrive;
			if (_currentPositionState == PositionState.PhysicsLink)
			{
				xDrive.positionSpring = _RBLinkPositionSpring;
				xDrive.positionDamper = _RBLinkPositionDamper;
				xDrive.maximumForce = _RBLinkPositionMaxForce;
			}
			else
			{
				xDrive.positionSpring = 0f;
				xDrive.positionDamper = 0f;
				xDrive.maximumForce = 0f;
			}
			_linkToJoint.xDrive = xDrive;
			_linkToJoint.yDrive = xDrive;
			_linkToJoint.zDrive = xDrive;
			xDrive = _linkToJoint.slerpDrive;
			if (_currentRotationState == RotationState.PhysicsLink)
			{
				xDrive.positionSpring = _RBLinkRotationSpring;
				xDrive.positionDamper = _RBLinkRotationDamper;
				xDrive.maximumForce = _RBLinkRotationMaxForce;
			}
			else
			{
				xDrive.positionSpring = 0f;
				xDrive.positionDamper = 0f;
				xDrive.maximumForce = 0f;
			}
			_linkToJoint.slerpDrive = xDrive;
			_linkToJoint.angularXDrive = xDrive;
			_linkToJoint.angularYZDrive = xDrive;
		}
	}

	private void SetJointSprings()
	{
		if (!(connectedJoint != null))
		{
			return;
		}
		float num = scalePow;
		float num2 = scalePow;
		float num3 = scalePow;
		JointDrive xDrive = connectedJoint.xDrive;
		switch (_currentPositionState)
		{
		case PositionState.On:
		case PositionState.Following:
		case PositionState.Hold:
		case PositionState.ParentLink:
		case PositionState.PhysicsLink:
			if (_isGrabbing && _currentPositionState == PositionState.ParentLink && preLinkPositionState == PositionState.Lock)
			{
				xDrive.positionSpring = _RBLockPositionSpring;
				xDrive.positionDamper = _RBLockPositionDamper;
				xDrive.maximumForce = _RBLockPositionMaxForce;
			}
			else
			{
				xDrive.positionSpring = _RBHoldPositionSpring;
				xDrive.positionDamper = _RBHoldPositionDamper;
				xDrive.maximumForce = _RBHoldPositionMaxForce;
			}
			break;
		case PositionState.Comply:
			xDrive.positionSpring = _RBComplyPositionSpring * RBMass;
			xDrive.positionDamper = _RBComplyPositionDamper * RBMass;
			xDrive.maximumForce = _RBComplyPositionMaxForce;
			break;
		case PositionState.Lock:
			xDrive.positionSpring = _RBLockPositionSpring;
			xDrive.positionDamper = _RBLockPositionDamper;
			xDrive.maximumForce = _RBLockPositionMaxForce;
			break;
		case PositionState.Off:
			xDrive.positionSpring = 0f;
			xDrive.positionDamper = 0f;
			xDrive.maximumForce = 0f;
			break;
		}
		connectedJoint.xDrive = xDrive;
		connectedJoint.yDrive = xDrive;
		connectedJoint.zDrive = xDrive;
		xDrive = connectedJoint.slerpDrive;
		switch (_currentRotationState)
		{
		case RotationState.On:
		case RotationState.Following:
		case RotationState.Hold:
		case RotationState.LookAt:
		case RotationState.ParentLink:
		case RotationState.PhysicsLink:
			if (_isGrabbing && _currentRotationState == RotationState.ParentLink && preLinkRotationState == RotationState.Lock)
			{
				xDrive.positionSpring = _RBLockRotationSpring * num;
				xDrive.positionDamper = _RBLockRotationDamper * num2;
				xDrive.maximumForce = _RBLockRotationMaxForce * num3;
			}
			else
			{
				xDrive.positionSpring = _RBHoldRotationSpring * num;
				xDrive.positionDamper = _RBHoldRotationDamper * num2;
				xDrive.maximumForce = _RBHoldRotationMaxForce * num3;
			}
			break;
		case RotationState.Comply:
			xDrive.positionSpring = _RBComplyRotationSpring * num * RBMass;
			xDrive.positionDamper = _RBComplyRotationDamper * num2 * RBMass;
			xDrive.maximumForce = _RBComplyRotationMaxForce * num3;
			break;
		case RotationState.Lock:
			xDrive.positionSpring = _RBLockRotationSpring * num;
			xDrive.positionDamper = _RBLockRotationDamper * num2;
			xDrive.maximumForce = _RBLockRotationMaxForce * num3;
			break;
		case RotationState.Off:
			xDrive.positionSpring = 0f;
			xDrive.positionDamper = 0f;
			xDrive.maximumForce = 0f;
			break;
		}
		connectedJoint.slerpDrive = xDrive;
		connectedJoint.angularXDrive = xDrive;
		connectedJoint.angularYZDrive = xDrive;
		_followWhenOffRB.WakeUp();
	}

	private void SetNaturalJointDrive()
	{
		if (naturalJoint != null)
		{
			float num = scalePow;
			float num2 = scalePow;
			float num3 = scalePow;
			JointDrive slerpDrive = naturalJoint.slerpDrive;
			if (_currentRotationState == RotationState.Comply)
			{
				slerpDrive.positionSpring = _RBComplyJointRotationDriveSpring * num;
			}
			else
			{
				slerpDrive.positionSpring = _jointRotationDriveSpring * num;
			}
			slerpDrive.positionDamper = _jointRotationDriveDamper * num2;
			slerpDrive.maximumForce = _jointRotationDriveMaxForce * num3;
			naturalJoint.slerpDrive = slerpDrive;
		}
	}

	private void SetNaturalJointDriveTarget()
	{
		if (naturalJoint != null)
		{
			Quaternion quaternion = Quaternion.Euler(_jointRotationDriveXTarget + _jointRotationDriveXTargetAdditional, 0f, 0f);
			Quaternion quaternion2 = Quaternion.Euler(0f, _jointRotationDriveYTarget + _jointRotationDriveYTargetAdditional, 0f);
			Quaternion quaternion3 = Quaternion.Euler(0f, 0f, _jointRotationDriveZTarget + _jointRotationDriveZTargetAdditional);
			Quaternion targetRotation = quaternion;
			switch (naturalJointDriveRotationOrder)
			{
			case Quaternion2Angles.RotationOrder.XYZ:
				targetRotation = quaternion * quaternion2 * quaternion3;
				break;
			case Quaternion2Angles.RotationOrder.XZY:
				targetRotation = quaternion * quaternion3 * quaternion2;
				break;
			case Quaternion2Angles.RotationOrder.YXZ:
				targetRotation = quaternion2 * quaternion * quaternion3;
				break;
			case Quaternion2Angles.RotationOrder.YZX:
				targetRotation = quaternion2 * quaternion3 * quaternion3;
				break;
			case Quaternion2Angles.RotationOrder.ZXY:
				targetRotation = quaternion3 * quaternion * quaternion2;
				break;
			case Quaternion2Angles.RotationOrder.ZYX:
				targetRotation = quaternion3 * quaternion2 * quaternion;
				break;
			}
			naturalJoint.targetRotation = targetRotation;
		}
	}

	private void SyncXLock(bool b)
	{
		_xLock = b;
	}

	private void SyncYLock(bool b)
	{
		_yLock = b;
	}

	private void SyncZLock(bool b)
	{
		_zLock = b;
	}

	private void SyncXLocalLock(bool b)
	{
		_xLocalLock = b;
	}

	private void SyncYLocalLock(bool b)
	{
		_yLocalLock = b;
	}

	private void SyncZLocalLock(bool b)
	{
		_zLocalLock = b;
	}

	private void SyncXRotLock(bool b)
	{
		_xRotLock = b;
	}

	private void SyncYRotLock(bool b)
	{
		_yRotLock = b;
	}

	private void SyncZRotLock(bool b)
	{
		_zRotLock = b;
	}

	public void SetXPositionNoForce(float f)
	{
		Vector3 position = control.position;
		position.x = f;
		SetPositionNoForce(position);
	}

	private void SetXPositionNoForce(string val)
	{
		float result;
		if (float.TryParse(val, out result))
		{
			SetXPositionNoForce(result);
		}
	}

	private void SetXPositionToInputField()
	{
		if (xPositionInputField != null)
		{
			SetXPositionNoForce(xPositionInputField.text);
			xPositionInputField.text = string.Empty;
		}
	}

	public void SetYPositionNoForce(float f)
	{
		Vector3 position = control.position;
		position.y = f;
		SetPositionNoForce(position);
	}

	private void SetYPositionNoForce(string val)
	{
		float result;
		if (float.TryParse(val, out result))
		{
			SetYPositionNoForce(result);
		}
	}

	private void SetYPositionToInputField()
	{
		if (yPositionInputField != null)
		{
			SetYPositionNoForce(yPositionInputField.text);
			yPositionInputField.text = string.Empty;
		}
	}

	public void SetZPositionNoForce(float f)
	{
		Vector3 position = control.position;
		position.z = f;
		SetPositionNoForce(position);
	}

	private void SetZPositionNoForce(string val)
	{
		float result;
		if (float.TryParse(val, out result))
		{
			SetZPositionNoForce(result);
		}
	}

	private void SetZPositionToInputField()
	{
		if (zPositionInputField != null)
		{
			SetZPositionNoForce(zPositionInputField.text);
			zPositionInputField.text = string.Empty;
		}
	}

	public void SetXRotationNoForce(float f)
	{
		Vector3 eulerAngles = control.eulerAngles;
		eulerAngles.x = f;
		SetRotationNoForce(eulerAngles);
	}

	private void SetXRotationNoForce(string val)
	{
		float result;
		if (float.TryParse(val, out result))
		{
			SetXRotationNoForce(result);
		}
	}

	private void SetXRotationToInputField()
	{
		if (xRotationInputField != null)
		{
			SetXRotationNoForce(xRotationInputField.text);
			xRotationInputField.text = string.Empty;
		}
	}

	public void SetYRotationNoForce(float f)
	{
		Vector3 eulerAngles = control.eulerAngles;
		eulerAngles.y = f;
		SetRotationNoForce(eulerAngles);
	}

	private void SetYRotationNoForce(string val)
	{
		float result;
		if (float.TryParse(val, out result))
		{
			SetYRotationNoForce(result);
		}
	}

	private void SetYRotationToInputField()
	{
		if (yRotationInputField != null)
		{
			SetYRotationNoForce(yRotationInputField.text);
			yRotationInputField.text = string.Empty;
		}
	}

	public void SetZRotationNoForce(float f)
	{
		Vector3 eulerAngles = control.eulerAngles;
		eulerAngles.z = f;
		SetRotationNoForce(eulerAngles);
	}

	private void SetZRotationNoForce(string val)
	{
		float result;
		if (float.TryParse(val, out result))
		{
			SetZRotationNoForce(result);
		}
	}

	private void SetZRotationToInputField()
	{
		if (zRotationInputField != null)
		{
			SetZRotationNoForce(zRotationInputField.text);
			zRotationInputField.text = string.Empty;
		}
	}

	public Vector3 GetLocalPosition()
	{
		Transform controlParentToUse = ControlParentToUse;
		if (controlParentToUse != null)
		{
			return controlParentToUse.InverseTransformPoint(control.position);
		}
		return control.localPosition;
	}

	public void SetLocalPosition(Vector3 localPosition)
	{
		Transform controlParentToUse = ControlParentToUse;
		if (controlParentToUse != null)
		{
			control.position = controlParentToUse.TransformPoint(localPosition);
		}
		else
		{
			control.localPosition = localPosition;
		}
	}

	public Vector3 GetLocalEulerAngles()
	{
		Transform controlParentToUse = ControlParentToUse;
		if (controlParentToUse != null && controlParentToUse != control.parent)
		{
			return (Quaternion.Inverse(controlParentToUse.rotation) * control.rotation).eulerAngles;
		}
		return control.localEulerAngles;
	}

	public void SetLocalEulerAngles(Vector3 eulerAngles)
	{
		Transform controlParentToUse = ControlParentToUse;
		if (controlParentToUse != null && controlParentToUse != control.parent)
		{
			Quaternion quaternion = Quaternion.Euler(eulerAngles);
			control.rotation = controlParentToUse.rotation * quaternion;
		}
		else
		{
			control.localEulerAngles = eulerAngles;
		}
	}

	public void SetXLocalPositionNoForce(float f)
	{
		Vector3 localPosition = GetLocalPosition();
		localPosition.x = f;
		SetLocalPositionNoForce(localPosition);
	}

	private void SetXLocalPositionNoForce(string val)
	{
		float result;
		if (float.TryParse(val, out result))
		{
			SetXLocalPositionNoForce(result);
		}
		if (xLocalPositionInputField != null)
		{
			xLocalPositionInputField.text = string.Empty;
		}
	}

	private void SetXLocalPositionToInputField()
	{
		if (xLocalPositionInputField != null)
		{
			SetXLocalPositionNoForce(xLocalPositionInputField.text);
			xLocalPositionInputField.text = string.Empty;
		}
	}

	public void SetYLocalPositionNoForce(float f)
	{
		Vector3 localPosition = GetLocalPosition();
		localPosition.y = f;
		SetLocalPositionNoForce(localPosition);
	}

	private void SetYLocalPositionNoForce(string val)
	{
		float result;
		if (float.TryParse(val, out result))
		{
			SetYLocalPositionNoForce(result);
		}
		if (yLocalPositionInputField != null)
		{
			yLocalPositionInputField.text = string.Empty;
		}
	}

	private void SetYLocalPositionToInputField()
	{
		if (yLocalPositionInputField != null)
		{
			SetYLocalPositionNoForce(yLocalPositionInputField.text);
			yLocalPositionInputField.text = string.Empty;
		}
	}

	public void SetZLocalPositionNoForce(float f)
	{
		Vector3 localPosition = GetLocalPosition();
		localPosition.z = f;
		SetLocalPositionNoForce(localPosition);
	}

	private void SetZLocalPositionNoForce(string val)
	{
		float result;
		if (float.TryParse(val, out result))
		{
			SetZLocalPositionNoForce(result);
		}
		if (zLocalPositionInputField != null)
		{
			zLocalPositionInputField.text = string.Empty;
		}
	}

	private void SetZLocalPositionToInputField()
	{
		if (zLocalPositionInputField != null)
		{
			SetZLocalPositionNoForce(zLocalPositionInputField.text);
			zLocalPositionInputField.text = string.Empty;
		}
	}

	public void SetXLocalRotationNoForce(float f)
	{
		Vector3 localEulerAngles = GetLocalEulerAngles();
		localEulerAngles.x = f;
		SetLocalRotationNoForce(localEulerAngles);
	}

	private void SetXLocalRotationNoForce(string val)
	{
		float result;
		if (float.TryParse(val, out result))
		{
			SetXLocalRotationNoForce(result);
		}
		if (xLocalRotationInputField != null)
		{
			xLocalRotationInputField.text = string.Empty;
		}
	}

	private void SetXLocalRotationToInputField()
	{
		if (xLocalRotationInputField != null)
		{
			SetXLocalRotationNoForce(xLocalRotationInputField.text);
			xLocalRotationInputField.text = string.Empty;
		}
	}

	public void SetYLocalRotationNoForce(float f)
	{
		Vector3 localEulerAngles = GetLocalEulerAngles();
		localEulerAngles.y = f;
		SetLocalRotationNoForce(localEulerAngles);
	}

	private void SetYLocalRotationNoForce(string val)
	{
		float result;
		if (float.TryParse(val, out result))
		{
			SetYLocalRotationNoForce(result);
		}
		if (yLocalRotationInputField != null)
		{
			yLocalRotationInputField.text = string.Empty;
		}
	}

	private void SetYLocalRotationToInputField()
	{
		if (yLocalRotationInputField != null)
		{
			SetYLocalRotationNoForce(yLocalRotationInputField.text);
			yLocalRotationInputField.text = string.Empty;
		}
	}

	public void SetZLocalRotationNoForce(float f)
	{
		Vector3 localEulerAngles = GetLocalEulerAngles();
		localEulerAngles.z = f;
		SetLocalRotationNoForce(localEulerAngles);
	}

	private void SetZLocalRotationNoForce(string val)
	{
		float result;
		if (float.TryParse(val, out result))
		{
			SetZLocalRotationNoForce(result);
		}
		if (zLocalRotationInputField != null)
		{
			zLocalRotationInputField.text = string.Empty;
		}
	}

	private void SetZLocalRotationToInputField()
	{
		if (zLocalRotationInputField != null)
		{
			SetZLocalRotationNoForce(zLocalRotationInputField.text);
			zLocalRotationInputField.text = string.Empty;
		}
	}

	public void XPositionSnapPoint1()
	{
		Vector3 position = control.position;
		position.x *= 10f;
		position.x = Mathf.Round(position.x);
		position.x /= 10f;
		control.position = position;
		if (onPositionChangeHandlers != null)
		{
			onPositionChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void YPositionSnapPoint1()
	{
		Vector3 position = control.position;
		position.y *= 10f;
		position.y = Mathf.Round(position.y);
		position.y /= 10f;
		control.position = position;
		if (onPositionChangeHandlers != null)
		{
			onPositionChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void ZPositionSnapPoint1()
	{
		Vector3 position = control.position;
		position.z *= 10f;
		position.z = Mathf.Round(position.z);
		position.z /= 10f;
		control.position = position;
		if (onPositionChangeHandlers != null)
		{
			onPositionChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void XLocalPositionSnapPoint1()
	{
		Vector3 localPosition = GetLocalPosition();
		localPosition.x *= 10f;
		localPosition.x = Mathf.Round(localPosition.x);
		localPosition.x /= 10f;
		SetLocalPosition(localPosition);
		if (onPositionChangeHandlers != null)
		{
			onPositionChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void YLocalPositionSnapPoint1()
	{
		Vector3 localPosition = GetLocalPosition();
		localPosition.y *= 10f;
		localPosition.y = Mathf.Round(localPosition.y);
		localPosition.y /= 10f;
		SetLocalPosition(localPosition);
		if (onPositionChangeHandlers != null)
		{
			onPositionChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void ZLocalPositionSnapPoint1()
	{
		Vector3 localPosition = GetLocalPosition();
		localPosition.z *= 10f;
		localPosition.z = Mathf.Round(localPosition.x);
		localPosition.z /= 10f;
		SetLocalPosition(localPosition);
		if (onPositionChangeHandlers != null)
		{
			onPositionChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void XRotationSnap1()
	{
		Vector3 eulerAngles = control.eulerAngles;
		eulerAngles.x = Mathf.Round(eulerAngles.x);
		control.eulerAngles = eulerAngles;
		if (onRotationChangeHandlers != null)
		{
			onRotationChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void YRotationSnap1()
	{
		Vector3 eulerAngles = control.eulerAngles;
		eulerAngles.y = Mathf.Round(eulerAngles.y);
		control.eulerAngles = eulerAngles;
		if (onRotationChangeHandlers != null)
		{
			onRotationChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void ZRotationSnap1()
	{
		Vector3 eulerAngles = control.eulerAngles;
		eulerAngles.z = Mathf.Round(eulerAngles.z);
		control.eulerAngles = eulerAngles;
		if (onRotationChangeHandlers != null)
		{
			onRotationChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void XLocalRotationSnap1()
	{
		Vector3 localEulerAngles = GetLocalEulerAngles();
		localEulerAngles.x = Mathf.Round(localEulerAngles.x);
		SetLocalEulerAngles(localEulerAngles);
		if (onRotationChangeHandlers != null)
		{
			onRotationChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void YLocalRotationSnap1()
	{
		Vector3 localEulerAngles = GetLocalEulerAngles();
		localEulerAngles.y = Mathf.Round(localEulerAngles.y);
		SetLocalEulerAngles(localEulerAngles);
		if (onRotationChangeHandlers != null)
		{
			onRotationChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void ZLocalRotationSnap1()
	{
		Vector3 localEulerAngles = GetLocalEulerAngles();
		localEulerAngles.z = Mathf.Round(localEulerAngles.z);
		SetLocalEulerAngles(localEulerAngles);
		if (onRotationChangeHandlers != null)
		{
			onRotationChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void XRotation0()
	{
		Vector3 eulerAngles = control.eulerAngles;
		eulerAngles.x = 0f;
		control.eulerAngles = eulerAngles;
		if (onRotationChangeHandlers != null)
		{
			onRotationChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void YRotation0()
	{
		Vector3 eulerAngles = control.eulerAngles;
		eulerAngles.y = 0f;
		control.eulerAngles = eulerAngles;
		if (onRotationChangeHandlers != null)
		{
			onRotationChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void ZRotation0()
	{
		Vector3 eulerAngles = control.eulerAngles;
		eulerAngles.z = 0f;
		control.eulerAngles = eulerAngles;
		if (onRotationChangeHandlers != null)
		{
			onRotationChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void XLocalRotation0()
	{
		Vector3 localEulerAngles = GetLocalEulerAngles();
		localEulerAngles.x = 0f;
		SetLocalEulerAngles(localEulerAngles);
		if (onRotationChangeHandlers != null)
		{
			onRotationChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void YLocalRotation0()
	{
		Vector3 localEulerAngles = GetLocalEulerAngles();
		localEulerAngles.y = 0f;
		SetLocalEulerAngles(localEulerAngles);
		if (onRotationChangeHandlers != null)
		{
			onRotationChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void ZLocalRotation0()
	{
		Vector3 localEulerAngles = GetLocalEulerAngles();
		localEulerAngles.z = 0f;
		SetLocalEulerAngles(localEulerAngles);
		if (onRotationChangeHandlers != null)
		{
			onRotationChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void XRotationAdd(float a)
	{
		Vector3 eulerAngles = control.eulerAngles;
		eulerAngles.x += a;
		control.eulerAngles = eulerAngles;
		if (onRotationChangeHandlers != null)
		{
			onRotationChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void YRotationAdd(float a)
	{
		Vector3 eulerAngles = control.eulerAngles;
		eulerAngles.y += a;
		control.eulerAngles = eulerAngles;
		if (onRotationChangeHandlers != null)
		{
			onRotationChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void ZRotationAdd(float a)
	{
		Vector3 eulerAngles = control.eulerAngles;
		eulerAngles.z += a;
		control.eulerAngles = eulerAngles;
		if (onRotationChangeHandlers != null)
		{
			onRotationChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void XLocalRotationAdd(float a)
	{
		Vector3 localEulerAngles = GetLocalEulerAngles();
		localEulerAngles.x += a;
		SetLocalEulerAngles(localEulerAngles);
		if (onRotationChangeHandlers != null)
		{
			onRotationChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void YLocalRotationAdd(float a)
	{
		Vector3 localEulerAngles = GetLocalEulerAngles();
		localEulerAngles.y += a;
		SetLocalEulerAngles(localEulerAngles);
		if (onRotationChangeHandlers != null)
		{
			onRotationChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void ZLocalRotationAdd(float a)
	{
		Vector3 localEulerAngles = GetLocalEulerAngles();
		localEulerAngles.z += a;
		SetLocalEulerAngles(localEulerAngles);
		if (onRotationChangeHandlers != null)
		{
			onRotationChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	protected void SyncOn(bool b)
	{
		_on = b;
		if (controlsOn && followWhenOff != null)
		{
			followWhenOff.gameObject.SetActive(_on);
		}
	}

	protected void SyncInteractableInPlayMode(bool b)
	{
		_interactableInPlayMode = b;
	}

	protected void SyncDeactivateOtherControlsOnPossess(bool b)
	{
		_deactivateOtherControlsOnPossess = b;
	}

	protected void SyncPossessable(bool b)
	{
		_possessable = b;
	}

	protected void SyncCanGrabPosition(bool b)
	{
		_canGrabPosition = b;
	}

	protected void SyncCanGrabRotation(bool b)
	{
		_canGrabRotation = b;
	}

	protected void SyncFreezeAtomPhysicsWhenGrabbed(bool b)
	{
		freezeAtomPhysicsWhenGrabbed = b;
	}

	public void SetPositionGridModeFromString(string gridModeString)
	{
		try
		{
			GridMode gridMode = (GridMode)Enum.Parse(typeof(GridMode), gridModeString);
			_positionGridMode = gridMode;
		}
		catch (ArgumentException)
		{
			Debug.LogError("Grid Mode " + gridModeString + " is not a valid grid mode");
		}
	}

	private void SyncPositionGrid(float f)
	{
		float num = f;
		num *= 1000f;
		num = Mathf.Round(num);
		num /= 1000f;
		if (positionGridJSON != null && num != f)
		{
			positionGridJSON.valNoCallback = num;
		}
		_positionGrid = num;
	}

	public void SetRotationGridModeFromString(string gridModeString)
	{
		try
		{
			GridMode gridMode = (GridMode)Enum.Parse(typeof(GridMode), gridModeString);
			_rotationGridMode = gridMode;
		}
		catch (ArgumentException)
		{
			Debug.LogError("Grid Mode " + gridModeString + " is not a valid grid mode");
		}
	}

	private void SyncRotationGrid(float f)
	{
		float num = f;
		num *= 100f;
		num = Mathf.Round(num);
		num /= 100f;
		if (rotationGridJSON != null && num != f)
		{
			rotationGridJSON.valNoCallback = num;
		}
		_rotationGrid = num;
	}

	private void SyncUseGravityOnRBWhenOff(bool b)
	{
		_useGravityOnRBWhenOff = b;
		if (_followWhenOffRB != null)
		{
			_followWhenOffRB.useGravity = _useGravityOnRBWhenOff;
		}
	}

	private void SyncPhysicsEnabled(bool b)
	{
		if (_followWhenOffRB != null)
		{
			_followWhenOffRB.isKinematic = !b;
			MeshCollider component = _followWhenOffRB.GetComponent<MeshCollider>();
			if (component != null)
			{
				component.convex = b;
			}
		}
	}

	private void SyncCollisionEnabled(bool b)
	{
		_collisionEnabled = b;
		if (controlsCollisionEnabled && _followWhenOffRB != null)
		{
			_followWhenOffRB.detectCollisions = _collisionEnabled && _globalCollisionEnabled;
		}
	}

	private void SyncRBMass(float f)
	{
		if (_followWhenOffRB != null)
		{
			_followWhenOffRB.mass = f;
			_followWhenOffRB.WakeUp();
			SetJointSprings();
		}
		if (rigidbodySlavesForMass != null)
		{
			Rigidbody[] array = rigidbodySlavesForMass;
			foreach (Rigidbody rigidbody in array)
			{
				rigidbody.mass = f;
			}
		}
	}

	private void SyncRBDrag(float f)
	{
		if (_followWhenOffRB != null)
		{
			_followWhenOffRB.drag = f;
			_followWhenOffRB.WakeUp();
		}
	}

	private void SyncRBMaxVelocityEnable(bool b)
	{
		_RBMaxVelocityEnable = b;
	}

	private void SyncRBMaxVelocity(float f)
	{
		_RBMaxVelocity = f;
	}

	private void SyncRBAngularDrag(float f)
	{
		if (_followWhenOffRB != null)
		{
			_followWhenOffRB.angularDrag = f;
			_followWhenOffRB.WakeUp();
		}
	}

	private void SyncRBHoldPositionSpring(float f)
	{
		_RBHoldPositionSpring = f;
		SetJointSprings();
	}

	public void SetHoldPositionSpringMin()
	{
		if (RBHoldPositionSpringJSON != null)
		{
			RBHoldPositionSpringJSON.val = RBHoldPositionSpringJSON.min;
		}
	}

	public void SetHoldPositionSpringMax()
	{
		if (RBHoldPositionSpringJSON != null)
		{
			RBHoldPositionSpringJSON.val = RBHoldPositionSpringJSON.max;
		}
	}

	public void SetHoldPositionSpringPercent(float percent)
	{
		if (RBHoldPositionSpringJSON != null)
		{
			RBHoldPositionSpringJSON.val = (RBHoldPositionSpringJSON.max - RBHoldPositionSpringJSON.min) * percent + RBHoldPositionSpringJSON.min;
		}
	}

	private void SyncRBHoldPositionDamper(float f)
	{
		_RBHoldPositionDamper = f;
		SetJointSprings();
	}

	public void SetHoldPositionDamperMin()
	{
		if (RBHoldPositionDamperJSON != null)
		{
			RBHoldPositionDamperJSON.val = RBHoldPositionDamperJSON.min;
		}
	}

	public void SetHoldPositionDamperMax()
	{
		if (RBHoldPositionDamperJSON != null)
		{
			RBHoldPositionDamperJSON.val = RBHoldPositionDamperJSON.max;
		}
	}

	public void SetHoldPositionDamperPercent(float percent)
	{
		if (RBHoldPositionDamperJSON != null)
		{
			RBHoldPositionDamperJSON.val = (RBHoldPositionDamperJSON.max - RBHoldPositionDamperJSON.min) * percent + RBHoldPositionDamperJSON.min;
		}
	}

	private void SyncRBHoldPositionMaxForce(float f)
	{
		_RBHoldPositionMaxForce = f;
		SetJointSprings();
	}

	private void SyncRBComplyPositionSpring(float f)
	{
		_RBComplyPositionSpring = f;
		SetJointSprings();
	}

	private void SyncRBComplyPositionDamper(float f)
	{
		_RBComplyPositionDamper = f;
		SetJointSprings();
	}

	private void SyncRBLinkPositionSpring(float f)
	{
		_RBLinkPositionSpring = f;
		SetLinkedJointSprings();
	}

	private void SyncRBLinkPositionDamper(float f)
	{
		_RBLinkPositionDamper = f;
		SetLinkedJointSprings();
	}

	private void SyncRBLinkPositionMaxForce(float f)
	{
		_RBLinkPositionMaxForce = f;
		SetLinkedJointSprings();
	}

	private void SyncRBHoldRotationSpring(float f)
	{
		_RBHoldRotationSpring = f;
		SetJointSprings();
	}

	public void SetHoldRotationSpringMin()
	{
		if (RBHoldRotationSpringJSON != null)
		{
			RBHoldRotationSpringJSON.val = RBHoldRotationSpringJSON.min;
		}
	}

	public void SetHoldRotationSpringMax()
	{
		if (RBHoldRotationSpringJSON != null)
		{
			RBHoldRotationSpringJSON.val = RBHoldRotationSpringJSON.max;
		}
	}

	public void SetHoldRotationSpringPercent(float percent)
	{
		if (RBHoldRotationSpringJSON != null)
		{
			RBHoldRotationSpringJSON.val = (RBHoldRotationSpringJSON.max - RBHoldRotationSpringJSON.min) * percent + RBHoldRotationSpringJSON.min;
		}
	}

	private void SyncRBHoldRotationDamper(float f)
	{
		_RBHoldRotationDamper = f;
		SetJointSprings();
	}

	public void SetHoldRotationDamperMin()
	{
		if (RBHoldRotationDamperJSON != null)
		{
			RBHoldRotationDamperJSON.val = RBHoldRotationDamperJSON.min;
		}
	}

	public void SetHoldRotationDamperMax()
	{
		if (RBHoldRotationDamperJSON != null)
		{
			RBHoldRotationDamperJSON.val = RBHoldRotationDamperJSON.max;
		}
	}

	public void SetHoldRotationDamperPercent(float percent)
	{
		if (RBHoldRotationDamperJSON != null)
		{
			RBHoldRotationDamperJSON.val = (RBHoldRotationDamperJSON.max - RBHoldRotationDamperJSON.min) * percent + RBHoldRotationDamperJSON.min;
		}
	}

	private void SyncRBHoldRotationMaxForce(float f)
	{
		_RBHoldRotationMaxForce = f;
		SetJointSprings();
	}

	private void SyncRBComplyRotationSpring(float f)
	{
		_RBComplyRotationSpring = f;
		SetJointSprings();
	}

	private void SyncRBComplyRotationDamper(float f)
	{
		_RBComplyRotationDamper = f;
		SetJointSprings();
	}

	private void SyncRBLinkRotationSpring(float f)
	{
		_RBLinkRotationSpring = f;
		SetLinkedJointSprings();
	}

	private void SyncRBLinkRotationDamper(float f)
	{
		_RBLinkRotationDamper = f;
		SetLinkedJointSprings();
	}

	private void SyncRBLinkRotationMaxForce(float f)
	{
		_RBLinkRotationMaxForce = f;
		SetLinkedJointSprings();
	}

	private void SyncRBComplyJointRotationDriveSpring(float f)
	{
		_RBComplyJointRotationDriveSpring = f;
		SetNaturalJointDrive();
	}

	private void SyncJointRotationDriveSpring(float f)
	{
		_jointRotationDriveSpring = f;
		SetNaturalJointDrive();
	}

	private void SyncJointRotationDriveDamper(float f)
	{
		_jointRotationDriveDamper = f;
		SetNaturalJointDrive();
	}

	private void SyncJointRotationDriveMaxForce(float f)
	{
		_jointRotationDriveMaxForce = f;
		SetNaturalJointDrive();
	}

	private void SyncJointRotationDriveXTarget(float f)
	{
		_jointRotationDriveXTarget = f;
		SetNaturalJointDriveTarget();
	}

	private void SyncJointRotationDriveYTarget(float f)
	{
		_jointRotationDriveYTarget = f;
		SetNaturalJointDriveTarget();
	}

	private void SyncJointRotationDriveZTarget(float f)
	{
		_jointRotationDriveZTarget = f;
		SetNaturalJointDriveTarget();
	}

	private void SyncDetachControl(bool b)
	{
		_detachControl = b;
		if (!_detachControl && followWhenOff != null && _followWhenOffRB.isKinematic)
		{
			Dictionary<FreeControllerV3, Transform> dictionary = new Dictionary<FreeControllerV3, Transform>();
			FreeControllerV3[] componentsInChildren = followWhenOff.GetComponentsInChildren<FreeControllerV3>(true);
			FreeControllerV3[] array = componentsInChildren;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				if (freeControllerV.transform.parent != null)
				{
					dictionary.Add(freeControllerV, freeControllerV.transform.parent);
					freeControllerV.transform.parent = null;
				}
			}
			followWhenOff.position = control.position;
			followWhenOff.rotation = control.rotation;
			FreeControllerV3[] array2 = componentsInChildren;
			foreach (FreeControllerV3 freeControllerV2 in array2)
			{
				Transform value;
				if (dictionary.TryGetValue(freeControllerV2, out value))
				{
					freeControllerV2.transform.parent = value;
				}
			}
		}
		SyncGrabFreezePhysics();
	}

	public void NextControlMode()
	{
		if (_controlMode == ControlMode.Off)
		{
			controlMode = ControlMode.Position;
		}
		else if (_controlMode == ControlMode.Position)
		{
			controlMode = ControlMode.Rotation;
		}
		else
		{
			controlMode = ControlMode.Position;
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
			HideGUI();
		}
	}

	protected void SyncAtomUID()
	{
		if (containingAtom != null)
		{
			if (UIDText != null)
			{
				UIDText.text = containingAtom.uid + ":" + base.name;
			}
			if (UIDTextAlt != null)
			{
				UIDTextAlt.text = containingAtom.uid + ":" + base.name;
			}
		}
	}

	protected void RegisterFCUI(FreeControllerV3UI fcui, bool isAlt)
	{
		resetAction.RegisterButton(fcui.resetButton, isAlt);
		if (currentPositionStateJSON != null)
		{
			currentPositionStateJSON.RegisterToggleGroupValue(fcui.positionToggleGroup, isAlt);
		}
		if (currentRotationStateJSON != null)
		{
			currentRotationStateJSON.RegisterToggleGroupValue(fcui.rotationToggleGroup, isAlt);
		}
		RBHoldPositionSpringJSON.RegisterSlider(fcui.holdPositionSpringSlider, isAlt);
		RBHoldPositionDamperJSON.RegisterSlider(fcui.holdPositionDamperSlider, isAlt);
		RBHoldPositionMaxForceJSON.RegisterSlider(fcui.holdPositionMaxForceSlider, isAlt);
		RBHoldRotationSpringJSON.RegisterSlider(fcui.holdRotationSpringSlider, isAlt);
		RBHoldRotationDamperJSON.RegisterSlider(fcui.holdRotationDamperSlider, isAlt);
		RBHoldRotationMaxForceJSON.RegisterSlider(fcui.holdRotationMaxForceSlider, isAlt);
		RBComplyPositionSpringJSON.RegisterSlider(fcui.complyPositionSpringSlider, isAlt);
		RBComplyPositionDamperJSON.RegisterSlider(fcui.complyPositionDamperSlider, isAlt);
		RBComplyRotationSpringJSON.RegisterSlider(fcui.complyRotationSpringSlider, isAlt);
		RBComplyRotationDamperJSON.RegisterSlider(fcui.complyRotationDamperSlider, isAlt);
		RBComplyJointRotationDriveSpringJSON.RegisterSlider(fcui.complyJointRotationDriveSpringSlider, isAlt);
		if (complyPositionThresholdJSON != null)
		{
			complyPositionThresholdJSON.RegisterSlider(fcui.complyPositionThresholdSlider, isAlt);
		}
		if (complyRotationThresholdJSON != null)
		{
			complyRotationThresholdJSON.RegisterSlider(fcui.complyRotationThresholdSlider, isAlt);
		}
		if (complySpeedJSON != null)
		{
			complySpeedJSON.RegisterSlider(fcui.complySpeedSlider, isAlt);
		}
		RBLinkPositionSpringJSON.RegisterSlider(fcui.linkPositionSpringSlider, isAlt);
		RBLinkPositionDamperJSON.RegisterSlider(fcui.linkPositionDamperSlider, isAlt);
		RBLinkPositionMaxForceJSON.RegisterSlider(fcui.linkPositionMaxForceSlider, isAlt);
		RBLinkRotationSpringJSON.RegisterSlider(fcui.linkRotationSpringSlider, isAlt);
		RBLinkRotationDamperJSON.RegisterSlider(fcui.linkRotationDamperSlider, isAlt);
		RBLinkRotationMaxForceJSON.RegisterSlider(fcui.linkRotationMaxForceSlider, isAlt);
		jointRotationDriveSpringJSON.RegisterSlider(fcui.jointRotationDriveSpringSlider, isAlt);
		jointRotationDriveDamperJSON.RegisterSlider(fcui.jointRotationDriveDamperSlider, isAlt);
		jointRotationDriveMaxForceJSON.RegisterSlider(fcui.jointRotationDriveMaxForceSlider, isAlt);
		jointRotationDriveXTargetJSON.RegisterSlider(fcui.jointRotationDriveXTargetSlider, isAlt);
		jointRotationDriveYTargetJSON.RegisterSlider(fcui.jointRotationDriveYTargetSlider, isAlt);
		jointRotationDriveZTargetJSON.RegisterSlider(fcui.jointRotationDriveZTargetSlider, isAlt);
		if (onJSON != null)
		{
			onJSON.RegisterToggle(fcui.onToggle, isAlt);
		}
		if (detachControlJSON != null)
		{
			detachControlJSON.RegisterToggle(fcui.detachControlToggle, isAlt);
		}
		RBMassJSON.RegisterSlider(fcui.massSlider, isAlt);
		RBDragJSON.RegisterSlider(fcui.dragSlider, isAlt);
		RBMaxVelocityEnableJSON.RegisterToggle(fcui.maxVelocityEnableToggle, isAlt);
		RBMaxVelocityJSON.RegisterSlider(fcui.maxVelocitySlider, isAlt);
		RBAngularDragJSON.RegisterSlider(fcui.angularDragSlider, isAlt);
		physicsEnabledJSON.RegisterToggle(fcui.physicsEnabledToggle, isAlt);
		if (collisionEnabledJSON != null)
		{
			collisionEnabledJSON.RegisterToggle(fcui.collisionEnabledToggle, isAlt);
		}
		useGravityJSON.RegisterToggle(fcui.useGravityWhenOffToggle, isAlt);
		interactableInPlayModeJSON.RegisterToggle(fcui.interactableInPlayModeToggle, isAlt);
		deactivateOtherControlsOnPossessJSON.RegisterToggle(fcui.deactivateOtherControlsOnPossessToggle, isAlt);
		if (fcui.deactivateOtherControlsListText != null)
		{
			string text = string.Empty;
			if (onPossessDeactiveList != null)
			{
				FreeControllerV3[] array = onPossessDeactiveList;
				foreach (FreeControllerV3 freeControllerV in array)
				{
					text = text + freeControllerV.name + " ";
				}
			}
			fcui.deactivateOtherControlsListText.text = text;
		}
		possessableJSON.RegisterToggle(fcui.possessableToggle, isAlt);
		canGrabPositionJSON.RegisterToggle(fcui.canGrabPositionToggle, isAlt);
		canGrabRotationJSON.RegisterToggle(fcui.canGrabRotationToggle, isAlt);
		freezeAtomPhysicsWhenGrabbedJSON.RegisterToggle(fcui.freezeAtomPhysicsWhenGrabbedToggle, isAlt);
		positionGridModeJSON.RegisterPopup(fcui.positionGridModePopup, isAlt);
		rotationGridModeJSON.RegisterPopup(fcui.rotationGridModePopup, isAlt);
		positionGridJSON.RegisterSlider(fcui.positionGridSlider, isAlt);
		rotationGridJSON.RegisterSlider(fcui.rotationGridSlider, isAlt);
		xLockJSON.RegisterToggle(fcui.xPositionLockToggle, isAlt);
		yLockJSON.RegisterToggle(fcui.yPositionLockToggle, isAlt);
		zLockJSON.RegisterToggle(fcui.zPositionLockToggle, isAlt);
		xLocalLockJSON.RegisterToggle(fcui.xPositionLocalLockToggle, isAlt);
		yLocalLockJSON.RegisterToggle(fcui.yPositionLocalLockToggle, isAlt);
		zLocalLockJSON.RegisterToggle(fcui.zPositionLocalLockToggle, isAlt);
		xRotLockJSON.RegisterToggle(fcui.xRotationLockToggle, isAlt);
		yRotLockJSON.RegisterToggle(fcui.yRotationLockToggle, isAlt);
		zRotLockJSON.RegisterToggle(fcui.zRotationLockToggle, isAlt);
		if (fcui.linkToAtomSelectionPopup != null)
		{
			fcui.linkToAtomSelectionPopup.numPopupValues = 1;
			fcui.linkToAtomSelectionPopup.setPopupValue(0, "None");
			if (linkToRB != null)
			{
				GetLinkToAtomUIDFromLinkToRB(linkToRB);
				SetLinkToAtom(linkToAtomUID);
				fcui.linkToAtomSelectionPopup.currentValue = linkToAtomUID;
			}
			else
			{
				fcui.linkToAtomSelectionPopup.currentValue = "None";
			}
			UIPopup uIPopup = fcui.linkToAtomSelectionPopup;
			uIPopup.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Combine(uIPopup.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetLinkToAtomNames));
			UIPopup uIPopup2 = fcui.linkToAtomSelectionPopup;
			uIPopup2.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup2.onValueChangeHandlers, new UIPopup.OnValueChange(SetLinkToAtom));
		}
		if (fcui.linkToSelectionPopup != null)
		{
			fcui.linkToSelectionPopup.numPopupValues = 1;
			fcui.linkToSelectionPopup.setPopupValue(0, "None");
			if (linkToRB != null)
			{
				fcui.linkToSelectionPopup.currentValue = linkToRB.name;
			}
			else
			{
				onLinkToRigidbodyNamesChanged(null);
				fcui.linkToSelectionPopup.currentValue = "None";
			}
			UIPopup uIPopup3 = fcui.linkToSelectionPopup;
			uIPopup3.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup3.onValueChangeHandlers, new UIPopup.OnValueChange(SetLinkToRigidbodyObject));
		}
		if (fcui.selectLinkToFromSceneButton != null)
		{
			fcui.selectLinkToFromSceneButton.onClick.AddListener(SelectLinkToRigidbodyFromScene);
		}
		if (fcui.selectAlignToFromSceneButton != null)
		{
			fcui.selectAlignToFromSceneButton.onClick.AddListener(SelectAlignToRigidbodyFromScene);
		}
		if (fcui.xPositionMinus1Button != null)
		{
			fcui.xPositionMinus1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(-1f, 0f, 0f);
			});
		}
		if (fcui.xPositionMinusPoint1Button != null)
		{
			fcui.xPositionMinusPoint1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(-0.1f, 0f, 0f);
			});
		}
		if (fcui.xPositionMinusPoint01Button != null)
		{
			fcui.xPositionMinusPoint01Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(-0.01f, 0f, 0f);
			});
		}
		if (fcui.xPositionPlusPoint01Button != null)
		{
			fcui.xPositionPlusPoint01Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0.01f, 0f, 0f);
			});
		}
		if (fcui.xPositionPlusPoint1Button != null)
		{
			fcui.xPositionPlusPoint1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0.1f, 0f, 0f);
			});
		}
		if (fcui.xPositionPlus1Button != null)
		{
			fcui.xPositionPlus1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(1f, 0f, 0f);
			});
		}
		if (fcui.yPositionMinus1Button != null)
		{
			fcui.yPositionMinus1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, -1f, 0f);
			});
		}
		if (fcui.yPositionMinusPoint1Button != null)
		{
			fcui.yPositionMinusPoint1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, -0.1f, 0f);
			});
		}
		if (fcui.yPositionMinusPoint01Button != null)
		{
			fcui.yPositionMinusPoint01Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, -0.01f, 0f);
			});
		}
		if (fcui.yPositionPlusPoint01Button != null)
		{
			fcui.yPositionPlusPoint01Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, 0.01f, 0f);
			});
		}
		if (fcui.yPositionPlusPoint1Button != null)
		{
			fcui.yPositionPlusPoint1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, 0.1f, 0f);
			});
		}
		if (fcui.yPositionPlus1Button != null)
		{
			fcui.yPositionPlus1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, 1f, 0f);
			});
		}
		if (fcui.zPositionMinus1Button != null)
		{
			fcui.zPositionMinus1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, 0f, -1f);
			});
		}
		if (fcui.zPositionMinusPoint1Button != null)
		{
			fcui.zPositionMinusPoint1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, 0f, -0.1f);
			});
		}
		if (fcui.zPositionMinusPoint01Button != null)
		{
			fcui.zPositionMinusPoint01Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, 0f, -0.01f);
			});
		}
		if (fcui.zPositionPlusPoint01Button != null)
		{
			fcui.zPositionPlusPoint01Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, 0f, 0.01f);
			});
		}
		if (fcui.zPositionPlusPoint1Button != null)
		{
			fcui.zPositionPlusPoint1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, 0f, 0.1f);
			});
		}
		if (fcui.zPositionPlus1Button != null)
		{
			fcui.zPositionPlus1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, 0f, 1f);
			});
		}
		if (fcui.xRotationMinus45Button != null)
		{
			fcui.xRotationMinus45Button.onClick.AddListener(delegate
			{
				XRotationAdd(-45f);
			});
		}
		if (fcui.xRotationMinus5Button != null)
		{
			fcui.xRotationMinus5Button.onClick.AddListener(delegate
			{
				XRotationAdd(-5f);
			});
		}
		if (fcui.xRotationMinusPoint5Button != null)
		{
			fcui.xRotationMinusPoint5Button.onClick.AddListener(delegate
			{
				XRotationAdd(-0.5f);
			});
		}
		if (fcui.xRotationPlusPoint5Button != null)
		{
			fcui.xRotationPlusPoint5Button.onClick.AddListener(delegate
			{
				XRotationAdd(0.5f);
			});
		}
		if (fcui.xRotationPlus5Button != null)
		{
			fcui.xRotationPlus5Button.onClick.AddListener(delegate
			{
				XRotationAdd(5f);
			});
		}
		if (fcui.xRotationPlus45Button != null)
		{
			fcui.xRotationPlus45Button.onClick.AddListener(delegate
			{
				XRotationAdd(45f);
			});
		}
		if (fcui.yRotationMinus45Button != null)
		{
			fcui.yRotationMinus45Button.onClick.AddListener(delegate
			{
				YRotationAdd(-45f);
			});
		}
		if (fcui.yRotationMinus5Button != null)
		{
			fcui.yRotationMinus5Button.onClick.AddListener(delegate
			{
				YRotationAdd(-5f);
			});
		}
		if (fcui.yRotationMinusPoint5Button != null)
		{
			fcui.yRotationMinusPoint5Button.onClick.AddListener(delegate
			{
				YRotationAdd(-0.5f);
			});
		}
		if (fcui.yRotationPlusPoint5Button != null)
		{
			fcui.yRotationPlusPoint5Button.onClick.AddListener(delegate
			{
				YRotationAdd(0.5f);
			});
		}
		if (fcui.yRotationPlus5Button != null)
		{
			fcui.yRotationPlus5Button.onClick.AddListener(delegate
			{
				YRotationAdd(5f);
			});
		}
		if (fcui.yRotationPlus45Button != null)
		{
			fcui.yRotationPlus45Button.onClick.AddListener(delegate
			{
				YRotationAdd(45f);
			});
		}
		if (fcui.zRotationMinus45Button != null)
		{
			fcui.zRotationMinus45Button.onClick.AddListener(delegate
			{
				ZRotationAdd(-45f);
			});
		}
		if (fcui.zRotationMinus5Button != null)
		{
			fcui.zRotationMinus5Button.onClick.AddListener(delegate
			{
				ZRotationAdd(-5f);
			});
		}
		if (fcui.zRotationMinusPoint5Button != null)
		{
			fcui.zRotationMinusPoint5Button.onClick.AddListener(delegate
			{
				ZRotationAdd(-0.5f);
			});
		}
		if (fcui.zRotationPlusPoint5Button != null)
		{
			fcui.zRotationPlusPoint5Button.onClick.AddListener(delegate
			{
				ZRotationAdd(0.5f);
			});
		}
		if (fcui.zRotationPlus5Button != null)
		{
			fcui.zRotationPlus5Button.onClick.AddListener(delegate
			{
				ZRotationAdd(5f);
			});
		}
		if (fcui.zRotationPlus45Button != null)
		{
			fcui.zRotationPlus45Button.onClick.AddListener(delegate
			{
				ZRotationAdd(45f);
			});
		}
		if (fcui.xPosition0Button != null)
		{
			fcui.xPosition0Button.onClick.AddListener(delegate
			{
				SetXPositionNoForce(0f);
			});
		}
		if (fcui.yPosition0Button != null)
		{
			fcui.yPosition0Button.onClick.AddListener(delegate
			{
				SetYPositionNoForce(0f);
			});
		}
		if (fcui.zPosition0Button != null)
		{
			fcui.zPosition0Button.onClick.AddListener(delegate
			{
				SetZPositionNoForce(0f);
			});
		}
		if (fcui.xRotation0Button != null)
		{
			fcui.xRotation0Button.onClick.AddListener(XRotation0);
		}
		if (fcui.yRotation0Button != null)
		{
			fcui.yRotation0Button.onClick.AddListener(YRotation0);
		}
		if (fcui.zRotation0Button != null)
		{
			fcui.zRotation0Button.onClick.AddListener(ZRotation0);
		}
		if (fcui.xPositionSnapPoint1Button != null)
		{
			fcui.xPositionSnapPoint1Button.onClick.AddListener(XPositionSnapPoint1);
		}
		if (fcui.yPositionSnapPoint1Button != null)
		{
			fcui.yPositionSnapPoint1Button.onClick.AddListener(YPositionSnapPoint1);
		}
		if (fcui.zPositionSnapPoint1Button != null)
		{
			fcui.zPositionSnapPoint1Button.onClick.AddListener(ZPositionSnapPoint1);
		}
		if (fcui.xRotationSnap1Button != null)
		{
			fcui.xRotationSnap1Button.onClick.AddListener(XRotationSnap1);
		}
		if (fcui.yRotationSnap1Button != null)
		{
			fcui.yRotationSnap1Button.onClick.AddListener(YRotationSnap1);
		}
		if (fcui.zRotationSnap1Button != null)
		{
			fcui.zRotationSnap1Button.onClick.AddListener(ZRotationSnap1);
		}
		if (fcui.xSelfRelativePositionAdjustInputField != null)
		{
			fcui.xSelfRelativePositionAdjustInputField.onEndEdit.AddListener(delegate
			{
				MoveXPositionRelativeNoForce(fcui.xSelfRelativePositionAdjustInputField.text);
				fcui.xSelfRelativePositionAdjustInputField.text = string.Empty;
			});
		}
		if (fcui.xSelfRelativePositionAdjustInputFieldAction != null)
		{
			InputFieldAction xSelfRelativePositionAdjustInputFieldAction = fcui.xSelfRelativePositionAdjustInputFieldAction;
			xSelfRelativePositionAdjustInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(xSelfRelativePositionAdjustInputFieldAction.onSubmitHandlers, (InputFieldAction.OnSubmit)delegate
			{
				MoveXPositionRelativeNoForce(fcui.xSelfRelativePositionAdjustInputField.text);
				fcui.xSelfRelativePositionAdjustInputField.text = string.Empty;
			});
		}
		if (fcui.xSelfRelativePositionMinus1Button != null)
		{
			fcui.xSelfRelativePositionMinus1Button.onClick.AddListener(delegate
			{
				MoveRelativeNoForce(-1f, 0f, 0f);
			});
		}
		if (fcui.xSelfRelativePositionMinusPoint1Button != null)
		{
			fcui.xSelfRelativePositionMinusPoint1Button.onClick.AddListener(delegate
			{
				MoveRelativeNoForce(-0.1f, 0f, 0f);
			});
		}
		if (fcui.xSelfRelativePositionMinusPoint01Button != null)
		{
			fcui.xSelfRelativePositionMinusPoint01Button.onClick.AddListener(delegate
			{
				MoveRelativeNoForce(-0.01f, 0f, 0f);
			});
		}
		if (fcui.xSelfRelativePositionPlusPoint01Button != null)
		{
			fcui.xSelfRelativePositionPlusPoint01Button.onClick.AddListener(delegate
			{
				MoveRelativeNoForce(0.01f, 0f, 0f);
			});
		}
		if (fcui.xSelfRelativePositionPlusPoint1Button != null)
		{
			fcui.xSelfRelativePositionPlusPoint1Button.onClick.AddListener(delegate
			{
				MoveRelativeNoForce(0.1f, 0f, 0f);
			});
		}
		if (fcui.xSelfRelativePositionPlus1Button != null)
		{
			fcui.xSelfRelativePositionPlus1Button.onClick.AddListener(delegate
			{
				MoveRelativeNoForce(1f, 0f, 0f);
			});
		}
		if (fcui.ySelfRelativePositionAdjustInputField != null)
		{
			fcui.ySelfRelativePositionAdjustInputField.onEndEdit.AddListener(delegate
			{
				MoveYPositionRelativeNoForce(fcui.ySelfRelativePositionAdjustInputField.text);
				fcui.ySelfRelativePositionAdjustInputField.text = string.Empty;
			});
		}
		if (fcui.ySelfRelativePositionAdjustInputFieldAction != null)
		{
			InputFieldAction ySelfRelativePositionAdjustInputFieldAction = fcui.ySelfRelativePositionAdjustInputFieldAction;
			ySelfRelativePositionAdjustInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(ySelfRelativePositionAdjustInputFieldAction.onSubmitHandlers, (InputFieldAction.OnSubmit)delegate
			{
				MoveYPositionRelativeNoForce(fcui.ySelfRelativePositionAdjustInputField.text);
				fcui.ySelfRelativePositionAdjustInputField.text = string.Empty;
			});
		}
		if (fcui.ySelfRelativePositionMinus1Button != null)
		{
			fcui.ySelfRelativePositionMinus1Button.onClick.AddListener(delegate
			{
				MoveRelativeNoForce(0f, -1f, 0f);
			});
		}
		if (fcui.ySelfRelativePositionMinusPoint1Button != null)
		{
			fcui.ySelfRelativePositionMinusPoint1Button.onClick.AddListener(delegate
			{
				MoveRelativeNoForce(0f, -0.1f, 0f);
			});
		}
		if (fcui.ySelfRelativePositionMinusPoint01Button != null)
		{
			fcui.ySelfRelativePositionMinusPoint01Button.onClick.AddListener(delegate
			{
				MoveRelativeNoForce(0f, -0.01f, 0f);
			});
		}
		if (fcui.ySelfRelativePositionPlusPoint01Button != null)
		{
			fcui.ySelfRelativePositionPlusPoint01Button.onClick.AddListener(delegate
			{
				MoveRelativeNoForce(0f, 0.01f, 0f);
			});
		}
		if (fcui.ySelfRelativePositionPlusPoint1Button != null)
		{
			fcui.ySelfRelativePositionPlusPoint1Button.onClick.AddListener(delegate
			{
				MoveRelativeNoForce(0f, 0.1f, 0f);
			});
		}
		if (fcui.ySelfRelativePositionPlus1Button != null)
		{
			fcui.ySelfRelativePositionPlus1Button.onClick.AddListener(delegate
			{
				MoveRelativeNoForce(0f, 1f, 0f);
			});
		}
		if (fcui.zSelfRelativePositionAdjustInputField != null)
		{
			fcui.zSelfRelativePositionAdjustInputField.onEndEdit.AddListener(delegate
			{
				MoveZPositionRelativeNoForce(fcui.zSelfRelativePositionAdjustInputField.text);
				fcui.zSelfRelativePositionAdjustInputField.text = string.Empty;
			});
		}
		if (fcui.zSelfRelativePositionAdjustInputFieldAction != null)
		{
			InputFieldAction zSelfRelativePositionAdjustInputFieldAction = fcui.zSelfRelativePositionAdjustInputFieldAction;
			zSelfRelativePositionAdjustInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(zSelfRelativePositionAdjustInputFieldAction.onSubmitHandlers, (InputFieldAction.OnSubmit)delegate
			{
				MoveZPositionRelativeNoForce(fcui.zSelfRelativePositionAdjustInputField.text);
				fcui.zSelfRelativePositionAdjustInputField.text = string.Empty;
			});
		}
		if (fcui.zSelfRelativePositionMinus1Button != null)
		{
			fcui.zSelfRelativePositionMinus1Button.onClick.AddListener(delegate
			{
				MoveRelativeNoForce(0f, 0f, -1f);
			});
		}
		if (fcui.zSelfRelativePositionMinusPoint1Button != null)
		{
			fcui.zSelfRelativePositionMinusPoint1Button.onClick.AddListener(delegate
			{
				MoveRelativeNoForce(0f, 0f, -0.1f);
			});
		}
		if (fcui.zSelfRelativePositionMinusPoint01Button != null)
		{
			fcui.zSelfRelativePositionMinusPoint01Button.onClick.AddListener(delegate
			{
				MoveRelativeNoForce(0f, 0f, -0.01f);
			});
		}
		if (fcui.zSelfRelativePositionPlusPoint01Button != null)
		{
			fcui.zSelfRelativePositionPlusPoint01Button.onClick.AddListener(delegate
			{
				MoveRelativeNoForce(0f, 0f, 0.01f);
			});
		}
		if (fcui.zSelfRelativePositionPlusPoint1Button != null)
		{
			fcui.zSelfRelativePositionPlusPoint1Button.onClick.AddListener(delegate
			{
				MoveRelativeNoForce(0f, 0f, 0.1f);
			});
		}
		if (fcui.zSelfRelativePositionPlus1Button != null)
		{
			fcui.zSelfRelativePositionPlus1Button.onClick.AddListener(delegate
			{
				MoveRelativeNoForce(0f, 0f, 1f);
			});
		}
		if (fcui.xSelfRelativeRotationAdjustInputField != null)
		{
			fcui.xSelfRelativeRotationAdjustInputField.onEndEdit.AddListener(delegate
			{
				RotateXSelfRelativeNoForce(fcui.xSelfRelativePositionAdjustInputField.text);
				fcui.xSelfRelativeRotationAdjustInputField.text = string.Empty;
			});
		}
		if (fcui.xSelfRelativePositionAdjustInputFieldAction != null)
		{
			InputFieldAction xSelfRelativePositionAdjustInputFieldAction2 = fcui.xSelfRelativePositionAdjustInputFieldAction;
			xSelfRelativePositionAdjustInputFieldAction2.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(xSelfRelativePositionAdjustInputFieldAction2.onSubmitHandlers, (InputFieldAction.OnSubmit)delegate
			{
				RotateXSelfRelativeNoForce(fcui.xSelfRelativePositionAdjustInputField.text);
				fcui.xSelfRelativeRotationAdjustInputField.text = string.Empty;
			});
		}
		if (fcui.xSelfRelativeRotationMinus45Button != null)
		{
			fcui.xSelfRelativeRotationMinus45Button.onClick.AddListener(delegate
			{
				RotateSelfRelativeNoForce(new Vector3(-45f, 0f, 0f));
			});
		}
		if (fcui.xSelfRelativeRotationMinus5Button != null)
		{
			fcui.xSelfRelativeRotationMinus5Button.onClick.AddListener(delegate
			{
				RotateSelfRelativeNoForce(new Vector3(-5f, 0f, 0f));
			});
		}
		if (fcui.xSelfRelativeRotationMinusPoint5Button != null)
		{
			fcui.xSelfRelativeRotationMinusPoint5Button.onClick.AddListener(delegate
			{
				RotateSelfRelativeNoForce(new Vector3(-0.5f, 0f, 0f));
			});
		}
		if (fcui.xSelfRelativeRotationPlusPoint5Button != null)
		{
			fcui.xSelfRelativeRotationPlusPoint5Button.onClick.AddListener(delegate
			{
				RotateSelfRelativeNoForce(new Vector3(0.5f, 0f, 0f));
			});
		}
		if (fcui.xSelfRelativeRotationPlus5Button != null)
		{
			fcui.xSelfRelativeRotationPlus5Button.onClick.AddListener(delegate
			{
				RotateSelfRelativeNoForce(new Vector3(5f, 0f, 0f));
			});
		}
		if (fcui.xSelfRelativeRotationPlus45Button != null)
		{
			fcui.xSelfRelativeRotationPlus45Button.onClick.AddListener(delegate
			{
				RotateSelfRelativeNoForce(new Vector3(45f, 0f, 0f));
			});
		}
		if (fcui.ySelfRelativeRotationAdjustInputField != null)
		{
			fcui.ySelfRelativeRotationAdjustInputField.onEndEdit.AddListener(delegate
			{
				RotateYSelfRelativeNoForce(fcui.ySelfRelativePositionAdjustInputField.text);
				fcui.ySelfRelativeRotationAdjustInputField.text = string.Empty;
			});
		}
		if (fcui.ySelfRelativePositionAdjustInputFieldAction != null)
		{
			InputFieldAction ySelfRelativePositionAdjustInputFieldAction2 = fcui.ySelfRelativePositionAdjustInputFieldAction;
			ySelfRelativePositionAdjustInputFieldAction2.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(ySelfRelativePositionAdjustInputFieldAction2.onSubmitHandlers, (InputFieldAction.OnSubmit)delegate
			{
				RotateYSelfRelativeNoForce(fcui.ySelfRelativePositionAdjustInputField.text);
				fcui.ySelfRelativeRotationAdjustInputField.text = string.Empty;
			});
		}
		if (fcui.ySelfRelativeRotationMinus45Button != null)
		{
			fcui.ySelfRelativeRotationMinus45Button.onClick.AddListener(delegate
			{
				RotateSelfRelativeNoForce(new Vector3(0f, -45f, 0f));
			});
		}
		if (fcui.ySelfRelativeRotationMinus5Button != null)
		{
			fcui.ySelfRelativeRotationMinus5Button.onClick.AddListener(delegate
			{
				RotateSelfRelativeNoForce(new Vector3(0f, -5f, 0f));
			});
		}
		if (fcui.ySelfRelativeRotationMinusPoint5Button != null)
		{
			fcui.ySelfRelativeRotationMinusPoint5Button.onClick.AddListener(delegate
			{
				RotateSelfRelativeNoForce(new Vector3(0f, -0.5f, 0f));
			});
		}
		if (fcui.ySelfRelativeRotationPlusPoint5Button != null)
		{
			fcui.ySelfRelativeRotationPlusPoint5Button.onClick.AddListener(delegate
			{
				RotateSelfRelativeNoForce(new Vector3(0f, 0.5f, 0f));
			});
		}
		if (fcui.ySelfRelativeRotationPlus5Button != null)
		{
			fcui.ySelfRelativeRotationPlus5Button.onClick.AddListener(delegate
			{
				RotateSelfRelativeNoForce(new Vector3(0f, 5f, 0f));
			});
		}
		if (fcui.ySelfRelativeRotationPlus45Button != null)
		{
			fcui.ySelfRelativeRotationPlus45Button.onClick.AddListener(delegate
			{
				RotateSelfRelativeNoForce(new Vector3(0f, 45f, 0f));
			});
		}
		if (fcui.zSelfRelativeRotationAdjustInputField != null)
		{
			fcui.zSelfRelativeRotationAdjustInputField.onEndEdit.AddListener(delegate
			{
				RotateZSelfRelativeNoForce(fcui.zSelfRelativePositionAdjustInputField.text);
				fcui.zSelfRelativeRotationAdjustInputField.text = string.Empty;
			});
		}
		if (fcui.zSelfRelativePositionAdjustInputFieldAction != null)
		{
			InputFieldAction zSelfRelativePositionAdjustInputFieldAction2 = fcui.zSelfRelativePositionAdjustInputFieldAction;
			zSelfRelativePositionAdjustInputFieldAction2.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(zSelfRelativePositionAdjustInputFieldAction2.onSubmitHandlers, (InputFieldAction.OnSubmit)delegate
			{
				RotateZSelfRelativeNoForce(fcui.zSelfRelativePositionAdjustInputField.text);
				fcui.zSelfRelativeRotationAdjustInputField.text = string.Empty;
			});
		}
		if (fcui.zSelfRelativeRotationMinus45Button != null)
		{
			fcui.zSelfRelativeRotationMinus45Button.onClick.AddListener(delegate
			{
				RotateSelfRelativeNoForce(new Vector3(0f, 0f, -45f));
			});
		}
		if (fcui.zSelfRelativeRotationMinus5Button != null)
		{
			fcui.zSelfRelativeRotationMinus5Button.onClick.AddListener(delegate
			{
				RotateSelfRelativeNoForce(new Vector3(0f, 0f, -5f));
			});
		}
		if (fcui.zSelfRelativeRotationMinusPoint5Button != null)
		{
			fcui.zSelfRelativeRotationMinusPoint5Button.onClick.AddListener(delegate
			{
				RotateSelfRelativeNoForce(new Vector3(0f, 0f, -0.5f));
			});
		}
		if (fcui.zSelfRelativeRotationPlusPoint5Button != null)
		{
			fcui.zSelfRelativeRotationPlusPoint5Button.onClick.AddListener(delegate
			{
				RotateSelfRelativeNoForce(new Vector3(0f, 0f, 0.5f));
			});
		}
		if (fcui.zSelfRelativeRotationPlus5Button != null)
		{
			fcui.zSelfRelativeRotationPlus5Button.onClick.AddListener(delegate
			{
				RotateSelfRelativeNoForce(new Vector3(0f, 0f, 5f));
			});
		}
		if (fcui.zSelfRelativeRotationPlus45Button != null)
		{
			fcui.zSelfRelativeRotationPlus45Button.onClick.AddListener(delegate
			{
				RotateSelfRelativeNoForce(new Vector3(0f, 0f, 45f));
			});
		}
		if (fcui.selectRootButton != null)
		{
			if (enableSelectRoot)
			{
				fcui.selectRootButton.gameObject.SetActive(true);
				fcui.selectRootButton.onClick.AddListener(SelectRoot);
			}
			else
			{
				fcui.selectRootButton.gameObject.SetActive(false);
			}
		}
		if (fcui.xPositionInputField != null)
		{
			fcui.xPositionInputField.text = string.Empty;
			fcui.xPositionInputField.onEndEdit.AddListener(SetXPositionNoForce);
		}
		if (fcui.xPositionInputFieldAction != null)
		{
			InputFieldAction xPositionInputFieldAction = fcui.xPositionInputFieldAction;
			xPositionInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(xPositionInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetXPositionToInputField));
		}
		if (fcui.yPositionInputField != null)
		{
			fcui.yPositionInputField.text = string.Empty;
			fcui.yPositionInputField.onEndEdit.AddListener(SetYPositionNoForce);
		}
		if (fcui.yPositionInputFieldAction != null)
		{
			InputFieldAction yPositionInputFieldAction = fcui.yPositionInputFieldAction;
			yPositionInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(yPositionInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetYPositionToInputField));
		}
		if (fcui.zPositionInputField != null)
		{
			fcui.zPositionInputField.text = string.Empty;
			fcui.zPositionInputField.onEndEdit.AddListener(SetZPositionNoForce);
		}
		if (fcui.zPositionInputFieldAction != null)
		{
			InputFieldAction zPositionInputFieldAction = fcui.zPositionInputFieldAction;
			zPositionInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(zPositionInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetZPositionToInputField));
		}
		if (fcui.xRotationInputField != null)
		{
			fcui.xRotationInputField.text = string.Empty;
			fcui.xRotationInputField.onEndEdit.AddListener(SetXRotationNoForce);
		}
		if (fcui.xRotationInputFieldAction != null)
		{
			InputFieldAction xRotationInputFieldAction = fcui.xRotationInputFieldAction;
			xRotationInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(xRotationInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetXRotationToInputField));
		}
		if (fcui.yRotationInputField != null)
		{
			fcui.yRotationInputField.text = string.Empty;
			fcui.yRotationInputField.onEndEdit.AddListener(SetYRotationNoForce);
		}
		if (fcui.yRotationInputFieldAction != null)
		{
			InputFieldAction yRotationInputFieldAction = fcui.yRotationInputFieldAction;
			yRotationInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(yRotationInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetYRotationToInputField));
		}
		if (fcui.zRotationInputField != null)
		{
			fcui.zRotationInputField.text = string.Empty;
			fcui.zRotationInputField.onEndEdit.AddListener(SetZRotationNoForce);
		}
		if (fcui.zRotationInputFieldAction != null)
		{
			InputFieldAction zRotationInputFieldAction = fcui.zRotationInputFieldAction;
			zRotationInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(zRotationInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetZRotationToInputField));
		}
		if (fcui.xLocalPositionInputField != null)
		{
			fcui.xLocalPositionInputField.text = string.Empty;
			fcui.xLocalPositionInputField.onEndEdit.AddListener(SetXLocalPositionNoForce);
		}
		if (fcui.xLocalPositionInputFieldAction != null)
		{
			InputFieldAction xLocalPositionInputFieldAction = fcui.xLocalPositionInputFieldAction;
			xLocalPositionInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(xLocalPositionInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetXLocalPositionToInputField));
		}
		if (fcui.yLocalPositionInputField != null)
		{
			fcui.yLocalPositionInputField.text = string.Empty;
			fcui.yLocalPositionInputField.onEndEdit.AddListener(SetYLocalPositionNoForce);
		}
		if (fcui.yLocalPositionInputFieldAction != null)
		{
			InputFieldAction yLocalPositionInputFieldAction = fcui.yLocalPositionInputFieldAction;
			yLocalPositionInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(yLocalPositionInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetYLocalPositionToInputField));
		}
		if (fcui.zLocalPositionInputField != null)
		{
			fcui.zLocalPositionInputField.text = string.Empty;
			fcui.zLocalPositionInputField.onEndEdit.AddListener(SetZLocalPositionNoForce);
		}
		if (fcui.zLocalPositionInputFieldAction != null)
		{
			InputFieldAction zLocalPositionInputFieldAction = fcui.zLocalPositionInputFieldAction;
			zLocalPositionInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(zLocalPositionInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetZLocalPositionToInputField));
		}
		if (fcui.xLocalRotationInputField != null)
		{
			fcui.xLocalRotationInputField.text = string.Empty;
			fcui.xLocalRotationInputField.onEndEdit.AddListener(SetXLocalRotationNoForce);
		}
		if (fcui.xLocalRotationInputFieldAction != null)
		{
			InputFieldAction xLocalRotationInputFieldAction = fcui.xLocalRotationInputFieldAction;
			xLocalRotationInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(xLocalRotationInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetXLocalRotationToInputField));
		}
		if (fcui.yLocalRotationInputField != null)
		{
			fcui.yLocalRotationInputField.text = string.Empty;
			fcui.yLocalRotationInputField.onEndEdit.AddListener(SetYLocalRotationNoForce);
		}
		if (fcui.yLocalRotationInputFieldAction != null)
		{
			InputFieldAction yLocalRotationInputFieldAction = fcui.yLocalRotationInputFieldAction;
			yLocalRotationInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(yLocalRotationInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetYLocalRotationToInputField));
		}
		if (fcui.zLocalRotationInputField != null)
		{
			fcui.zLocalRotationInputField.text = string.Empty;
			fcui.zLocalRotationInputField.onEndEdit.AddListener(SetZLocalRotationNoForce);
		}
		if (fcui.zLocalRotationInputFieldAction != null)
		{
			InputFieldAction zLocalRotationInputFieldAction = fcui.zLocalRotationInputFieldAction;
			zLocalRotationInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(zLocalRotationInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetZLocalRotationToInputField));
		}
		if (fcui.zeroXLocalPositionButton != null)
		{
			fcui.zeroXLocalPositionButton.onClick.AddListener(delegate
			{
				SetXLocalPositionNoForce(0f);
			});
		}
		if (fcui.zeroYLocalPositionButton != null)
		{
			fcui.zeroYLocalPositionButton.onClick.AddListener(delegate
			{
				SetYLocalPositionNoForce(0f);
			});
		}
		if (fcui.zeroZLocalPositionButton != null)
		{
			fcui.zeroZLocalPositionButton.onClick.AddListener(delegate
			{
				SetZLocalPositionNoForce(0f);
			});
		}
		if (fcui.zeroXLocalRotationButton != null)
		{
			fcui.zeroXLocalRotationButton.onClick.AddListener(delegate
			{
				SetXLocalRotationNoForce(0f);
			});
		}
		if (fcui.zeroYLocalRotationButton != null)
		{
			fcui.zeroYLocalRotationButton.onClick.AddListener(delegate
			{
				SetYLocalRotationNoForce(0f);
			});
		}
		if (fcui.zeroZLocalRotationButton != null)
		{
			fcui.zeroZLocalRotationButton.onClick.AddListener(delegate
			{
				SetZLocalRotationNoForce(0f);
			});
		}
		if (isAlt)
		{
			UIDTextAlt = fcui.UIDText;
			if (UIDTextAlt != null)
			{
				if (containingAtom != null)
				{
					UIDTextAlt.text = containingAtom.uid + ":" + base.name;
				}
				else
				{
					UIDTextAlt.text = base.name;
				}
			}
			return;
		}
		UIDText = fcui.UIDText;
		if (UIDText != null)
		{
			if (containingAtom != null)
			{
				UIDText.text = containingAtom.uid + ":" + base.name;
			}
			else
			{
				UIDText.text = base.name;
			}
		}
		xPositionInputField = fcui.xPositionInputField;
		yPositionInputField = fcui.yPositionInputField;
		zPositionInputField = fcui.zPositionInputField;
		xRotationInputField = fcui.xRotationInputField;
		yRotationInputField = fcui.yRotationInputField;
		zRotationInputField = fcui.zRotationInputField;
		xLocalPositionInputField = fcui.xLocalPositionInputField;
		yLocalPositionInputField = fcui.yLocalPositionInputField;
		zLocalPositionInputField = fcui.zLocalPositionInputField;
		xLocalRotationInputField = fcui.xLocalRotationInputField;
		yLocalRotationInputField = fcui.yLocalRotationInputField;
		zLocalRotationInputField = fcui.zLocalRotationInputField;
		xPositionText = fcui.xPositionText;
		yPositionText = fcui.yPositionText;
		zPositionText = fcui.zPositionText;
		xLocalPositionText = fcui.xLocalPositionText;
		yLocalPositionText = fcui.yLocalPositionText;
		zLocalPositionText = fcui.zLocalPositionText;
		xRotationText = fcui.xRotationText;
		yRotationText = fcui.yRotationText;
		zRotationText = fcui.zRotationText;
		xLocalRotationText = fcui.xLocalRotationText;
		yLocalRotationText = fcui.yLocalRotationText;
		zLocalRotationText = fcui.zLocalRotationText;
		linkToAtomSelectionPopup = fcui.linkToAtomSelectionPopup;
		linkToSelectionPopup = fcui.linkToSelectionPopup;
	}

	protected void DeregisterFCUI(FreeControllerV3UI fcui)
	{
		if (fcui.linkToAtomSelectionPopup != null)
		{
			UIPopup uIPopup = linkToAtomSelectionPopup;
			uIPopup.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Remove(uIPopup.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetLinkToAtomNames));
			UIPopup uIPopup2 = linkToAtomSelectionPopup;
			uIPopup2.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Remove(uIPopup2.onValueChangeHandlers, new UIPopup.OnValueChange(SetLinkToAtom));
		}
		if (fcui.linkToSelectionPopup != null)
		{
			UIPopup uIPopup3 = linkToSelectionPopup;
			uIPopup3.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup3.onValueChangeHandlers, new UIPopup.OnValueChange(SetLinkToRigidbodyObject));
		}
		if (fcui.selectLinkToFromSceneButton != null)
		{
			fcui.selectLinkToFromSceneButton.onClick.RemoveListener(SelectLinkToRigidbodyFromScene);
		}
		if (fcui.selectAlignToFromSceneButton != null)
		{
			fcui.selectAlignToFromSceneButton.onClick.RemoveListener(SelectAlignToRigidbodyFromScene);
		}
		if (fcui.xPositionMinus1Button != null)
		{
			fcui.xPositionMinus1Button.onClick.RemoveAllListeners();
		}
		if (fcui.xPositionMinusPoint1Button != null)
		{
			fcui.xPositionMinusPoint1Button.onClick.RemoveAllListeners();
		}
		if (fcui.xPositionMinusPoint01Button != null)
		{
			fcui.xPositionMinusPoint01Button.onClick.RemoveAllListeners();
		}
		if (fcui.xPositionPlusPoint01Button != null)
		{
			fcui.xPositionPlusPoint01Button.onClick.RemoveAllListeners();
		}
		if (fcui.xPositionPlusPoint1Button != null)
		{
			fcui.xPositionPlusPoint1Button.onClick.RemoveAllListeners();
		}
		if (fcui.xPositionPlus1Button != null)
		{
			fcui.xPositionPlus1Button.onClick.RemoveAllListeners();
		}
		if (fcui.yPositionMinus1Button != null)
		{
			fcui.yPositionMinus1Button.onClick.RemoveAllListeners();
		}
		if (fcui.yPositionMinusPoint1Button != null)
		{
			fcui.yPositionMinusPoint1Button.onClick.RemoveAllListeners();
		}
		if (fcui.yPositionMinusPoint01Button != null)
		{
			fcui.yPositionMinusPoint01Button.onClick.RemoveAllListeners();
		}
		if (fcui.yPositionPlusPoint01Button != null)
		{
			fcui.yPositionPlusPoint01Button.onClick.RemoveAllListeners();
		}
		if (fcui.yPositionPlusPoint1Button != null)
		{
			fcui.yPositionPlusPoint1Button.onClick.RemoveAllListeners();
		}
		if (fcui.yPositionPlus1Button != null)
		{
			fcui.yPositionPlus1Button.onClick.RemoveAllListeners();
		}
		if (fcui.zPositionMinus1Button != null)
		{
			fcui.zPositionMinus1Button.onClick.RemoveAllListeners();
		}
		if (fcui.zPositionMinusPoint1Button != null)
		{
			fcui.zPositionMinusPoint1Button.onClick.RemoveAllListeners();
		}
		if (fcui.zPositionMinusPoint01Button != null)
		{
			fcui.zPositionMinusPoint01Button.onClick.RemoveAllListeners();
		}
		if (fcui.zPositionPlusPoint01Button != null)
		{
			fcui.zPositionPlusPoint01Button.onClick.RemoveAllListeners();
		}
		if (fcui.zPositionPlusPoint1Button != null)
		{
			fcui.zPositionPlusPoint1Button.onClick.RemoveAllListeners();
		}
		if (fcui.zPositionPlus1Button != null)
		{
			fcui.zPositionPlus1Button.onClick.RemoveAllListeners();
		}
		if (fcui.xRotationMinus45Button != null)
		{
			fcui.xRotationMinus45Button.onClick.RemoveAllListeners();
		}
		if (fcui.xRotationMinus5Button != null)
		{
			fcui.xRotationMinus5Button.onClick.RemoveAllListeners();
		}
		if (fcui.xRotationMinusPoint5Button != null)
		{
			fcui.xRotationMinusPoint5Button.onClick.RemoveAllListeners();
		}
		if (fcui.xRotationPlusPoint5Button != null)
		{
			fcui.xRotationPlusPoint5Button.onClick.RemoveAllListeners();
		}
		if (fcui.xRotationPlus5Button != null)
		{
			fcui.xRotationPlus5Button.onClick.RemoveAllListeners();
		}
		if (fcui.xRotationPlus45Button != null)
		{
			fcui.xRotationPlus45Button.onClick.RemoveAllListeners();
		}
		if (fcui.yRotationMinus45Button != null)
		{
			fcui.yRotationMinus45Button.onClick.RemoveAllListeners();
		}
		if (fcui.yRotationMinus5Button != null)
		{
			fcui.yRotationMinus5Button.onClick.RemoveAllListeners();
		}
		if (fcui.yRotationMinusPoint5Button != null)
		{
			fcui.yRotationMinusPoint5Button.onClick.RemoveAllListeners();
		}
		if (fcui.yRotationPlusPoint5Button != null)
		{
			fcui.yRotationPlusPoint5Button.onClick.RemoveAllListeners();
		}
		if (fcui.yRotationPlus5Button != null)
		{
			fcui.yRotationPlus5Button.onClick.RemoveAllListeners();
		}
		if (fcui.yRotationPlus45Button != null)
		{
			fcui.yRotationPlus45Button.onClick.RemoveAllListeners();
		}
		if (fcui.zRotationMinus45Button != null)
		{
			fcui.zRotationMinus45Button.onClick.RemoveAllListeners();
		}
		if (fcui.zRotationMinus5Button != null)
		{
			fcui.zRotationMinus5Button.onClick.RemoveAllListeners();
		}
		if (fcui.zRotationMinusPoint5Button != null)
		{
			fcui.zRotationMinusPoint5Button.onClick.RemoveAllListeners();
		}
		if (fcui.zRotationPlusPoint5Button != null)
		{
			fcui.zRotationPlusPoint5Button.onClick.RemoveAllListeners();
		}
		if (fcui.zRotationPlus5Button != null)
		{
			fcui.zRotationPlus5Button.onClick.RemoveAllListeners();
		}
		if (fcui.zRotationPlus45Button != null)
		{
			fcui.zRotationPlus45Button.onClick.RemoveAllListeners();
		}
		if (fcui.xPosition0Button != null)
		{
			fcui.xPosition0Button.onClick.RemoveAllListeners();
		}
		if (fcui.yPosition0Button != null)
		{
			fcui.yPosition0Button.onClick.RemoveAllListeners();
		}
		if (fcui.zPosition0Button != null)
		{
			fcui.zPosition0Button.onClick.RemoveAllListeners();
		}
		if (fcui.xRotation0Button != null)
		{
			fcui.xRotation0Button.onClick.RemoveListener(XRotation0);
		}
		if (fcui.yRotation0Button != null)
		{
			fcui.yRotation0Button.onClick.RemoveListener(YRotation0);
		}
		if (fcui.zRotation0Button != null)
		{
			fcui.zRotation0Button.onClick.RemoveListener(ZRotation0);
		}
		if (fcui.xPositionSnapPoint1Button != null)
		{
			fcui.xPositionSnapPoint1Button.onClick.RemoveListener(XPositionSnapPoint1);
		}
		if (fcui.yPositionSnapPoint1Button != null)
		{
			fcui.yPositionSnapPoint1Button.onClick.RemoveListener(YPositionSnapPoint1);
		}
		if (fcui.zPositionSnapPoint1Button != null)
		{
			fcui.zPositionSnapPoint1Button.onClick.RemoveListener(ZPositionSnapPoint1);
		}
		if (fcui.xRotationSnap1Button != null)
		{
			fcui.xRotationSnap1Button.onClick.RemoveListener(XRotationSnap1);
		}
		if (fcui.yRotationSnap1Button != null)
		{
			fcui.yRotationSnap1Button.onClick.RemoveListener(YRotationSnap1);
		}
		if (fcui.zRotationSnap1Button != null)
		{
			fcui.zRotationSnap1Button.onClick.RemoveListener(ZRotationSnap1);
		}
		if (fcui.xSelfRelativePositionAdjustInputField != null)
		{
			fcui.xSelfRelativePositionAdjustInputField.onEndEdit.RemoveAllListeners();
		}
		if (fcui.xSelfRelativePositionAdjustInputFieldAction != null)
		{
			fcui.xSelfRelativePositionAdjustInputFieldAction.onSubmitHandlers = null;
		}
		if (fcui.xSelfRelativePositionMinus1Button != null)
		{
			fcui.xSelfRelativePositionMinus1Button.onClick.RemoveAllListeners();
		}
		if (fcui.xSelfRelativePositionMinusPoint1Button != null)
		{
			fcui.xSelfRelativePositionMinusPoint1Button.onClick.RemoveAllListeners();
		}
		if (fcui.xSelfRelativePositionMinusPoint01Button != null)
		{
			fcui.xSelfRelativePositionMinusPoint01Button.onClick.RemoveAllListeners();
		}
		if (fcui.xSelfRelativePositionPlusPoint01Button != null)
		{
			fcui.xSelfRelativePositionPlusPoint01Button.onClick.RemoveAllListeners();
		}
		if (fcui.xSelfRelativePositionPlusPoint1Button != null)
		{
			fcui.xSelfRelativePositionPlusPoint1Button.onClick.RemoveAllListeners();
		}
		if (fcui.xSelfRelativePositionPlus1Button != null)
		{
			fcui.xSelfRelativePositionPlus1Button.onClick.RemoveAllListeners();
		}
		if (fcui.ySelfRelativePositionAdjustInputField != null)
		{
			fcui.ySelfRelativePositionAdjustInputField.onEndEdit.RemoveAllListeners();
		}
		if (fcui.ySelfRelativePositionAdjustInputFieldAction != null)
		{
			fcui.ySelfRelativePositionAdjustInputFieldAction.onSubmitHandlers = null;
		}
		if (fcui.ySelfRelativePositionMinus1Button != null)
		{
			fcui.ySelfRelativePositionMinus1Button.onClick.RemoveAllListeners();
		}
		if (fcui.ySelfRelativePositionMinusPoint1Button != null)
		{
			fcui.ySelfRelativePositionMinusPoint1Button.onClick.RemoveAllListeners();
		}
		if (fcui.ySelfRelativePositionMinusPoint01Button != null)
		{
			fcui.ySelfRelativePositionMinusPoint01Button.onClick.RemoveAllListeners();
		}
		if (fcui.ySelfRelativePositionPlusPoint01Button != null)
		{
			fcui.ySelfRelativePositionPlusPoint01Button.onClick.RemoveAllListeners();
		}
		if (fcui.ySelfRelativePositionPlusPoint1Button != null)
		{
			fcui.ySelfRelativePositionPlusPoint1Button.onClick.RemoveAllListeners();
		}
		if (fcui.ySelfRelativePositionPlus1Button != null)
		{
			fcui.ySelfRelativePositionPlus1Button.onClick.RemoveAllListeners();
		}
		if (fcui.zSelfRelativePositionAdjustInputField != null)
		{
			fcui.zSelfRelativePositionAdjustInputField.onEndEdit.RemoveAllListeners();
		}
		if (fcui.zSelfRelativePositionAdjustInputFieldAction != null)
		{
			fcui.zSelfRelativePositionAdjustInputFieldAction.onSubmitHandlers = null;
		}
		if (fcui.zSelfRelativePositionMinus1Button != null)
		{
			fcui.zSelfRelativePositionMinus1Button.onClick.RemoveAllListeners();
		}
		if (fcui.zSelfRelativePositionMinusPoint1Button != null)
		{
			fcui.zSelfRelativePositionMinusPoint1Button.onClick.RemoveAllListeners();
		}
		if (fcui.zSelfRelativePositionMinusPoint01Button != null)
		{
			fcui.zSelfRelativePositionMinusPoint01Button.onClick.RemoveAllListeners();
		}
		if (fcui.zSelfRelativePositionPlusPoint01Button != null)
		{
			fcui.zSelfRelativePositionPlusPoint01Button.onClick.RemoveAllListeners();
		}
		if (fcui.zSelfRelativePositionPlusPoint1Button != null)
		{
			fcui.zSelfRelativePositionPlusPoint1Button.onClick.RemoveAllListeners();
		}
		if (fcui.zSelfRelativePositionPlus1Button != null)
		{
			fcui.zSelfRelativePositionPlus1Button.onClick.RemoveAllListeners();
		}
		if (fcui.xSelfRelativeRotationAdjustInputField != null)
		{
			fcui.xSelfRelativeRotationAdjustInputField.onEndEdit.RemoveAllListeners();
		}
		if (fcui.xSelfRelativeRotationAdjustInputFieldAction != null)
		{
			fcui.xSelfRelativeRotationAdjustInputFieldAction.onSubmitHandlers = null;
		}
		if (fcui.xSelfRelativeRotationMinus45Button != null)
		{
			fcui.xSelfRelativeRotationMinus45Button.onClick.RemoveAllListeners();
		}
		if (fcui.xSelfRelativeRotationMinus5Button != null)
		{
			fcui.xSelfRelativeRotationMinus5Button.onClick.RemoveAllListeners();
		}
		if (fcui.xSelfRelativeRotationMinusPoint5Button != null)
		{
			fcui.xSelfRelativeRotationMinusPoint5Button.onClick.RemoveAllListeners();
		}
		if (fcui.xSelfRelativeRotationPlusPoint5Button != null)
		{
			fcui.xSelfRelativeRotationPlusPoint5Button.onClick.RemoveAllListeners();
		}
		if (fcui.xSelfRelativeRotationPlus5Button != null)
		{
			fcui.xSelfRelativeRotationPlus5Button.onClick.RemoveAllListeners();
		}
		if (fcui.xSelfRelativeRotationPlus45Button != null)
		{
			fcui.xSelfRelativeRotationPlus45Button.onClick.RemoveAllListeners();
		}
		if (fcui.ySelfRelativeRotationAdjustInputField != null)
		{
			fcui.ySelfRelativeRotationAdjustInputField.onEndEdit.RemoveAllListeners();
		}
		if (fcui.ySelfRelativeRotationAdjustInputFieldAction != null)
		{
			fcui.ySelfRelativeRotationAdjustInputFieldAction.onSubmitHandlers = null;
		}
		if (fcui.ySelfRelativeRotationMinus45Button != null)
		{
			fcui.ySelfRelativeRotationMinus45Button.onClick.RemoveAllListeners();
		}
		if (fcui.ySelfRelativeRotationMinus5Button != null)
		{
			fcui.ySelfRelativeRotationMinus5Button.onClick.RemoveAllListeners();
		}
		if (fcui.ySelfRelativeRotationMinusPoint5Button != null)
		{
			fcui.ySelfRelativeRotationMinusPoint5Button.onClick.RemoveAllListeners();
		}
		if (fcui.ySelfRelativeRotationPlusPoint5Button != null)
		{
			fcui.ySelfRelativeRotationPlusPoint5Button.onClick.RemoveAllListeners();
		}
		if (fcui.ySelfRelativeRotationPlus5Button != null)
		{
			fcui.ySelfRelativeRotationPlus5Button.onClick.RemoveAllListeners();
		}
		if (fcui.ySelfRelativeRotationPlus45Button != null)
		{
			fcui.ySelfRelativeRotationPlus45Button.onClick.RemoveAllListeners();
		}
		if (fcui.zSelfRelativeRotationAdjustInputField != null)
		{
			fcui.zSelfRelativeRotationAdjustInputField.onEndEdit.RemoveAllListeners();
		}
		if (fcui.zSelfRelativeRotationAdjustInputFieldAction != null)
		{
			fcui.zSelfRelativeRotationAdjustInputFieldAction.onSubmitHandlers = null;
		}
		if (fcui.zSelfRelativeRotationMinus45Button != null)
		{
			fcui.zSelfRelativeRotationMinus45Button.onClick.RemoveAllListeners();
		}
		if (fcui.zSelfRelativeRotationMinus5Button != null)
		{
			fcui.zSelfRelativeRotationMinus5Button.onClick.RemoveAllListeners();
		}
		if (fcui.zSelfRelativeRotationMinusPoint5Button != null)
		{
			fcui.zSelfRelativeRotationMinusPoint5Button.onClick.RemoveAllListeners();
		}
		if (fcui.zSelfRelativeRotationPlusPoint5Button != null)
		{
			fcui.zSelfRelativeRotationPlusPoint5Button.onClick.RemoveAllListeners();
		}
		if (fcui.zSelfRelativeRotationPlus5Button != null)
		{
			fcui.zSelfRelativeRotationPlus5Button.onClick.RemoveAllListeners();
		}
		if (fcui.zSelfRelativeRotationPlus45Button != null)
		{
			fcui.zSelfRelativeRotationPlus45Button.onClick.RemoveAllListeners();
		}
		if (enableSelectRoot && fcui.selectRootButton != null)
		{
			fcui.selectRootButton.onClick.RemoveListener(SelectRoot);
		}
		if (fcui.xPositionInputField != null)
		{
			fcui.xPositionInputField.onEndEdit.RemoveAllListeners();
		}
		if (fcui.xPositionInputFieldAction != null)
		{
			InputFieldAction xPositionInputFieldAction = fcui.xPositionInputFieldAction;
			xPositionInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Remove(xPositionInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetXPositionToInputField));
		}
		if (fcui.yPositionInputField != null)
		{
			fcui.yPositionInputField.onEndEdit.RemoveAllListeners();
		}
		if (fcui.yPositionInputFieldAction != null)
		{
			InputFieldAction yPositionInputFieldAction = fcui.yPositionInputFieldAction;
			yPositionInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Remove(yPositionInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetYPositionToInputField));
		}
		if (fcui.zPositionInputField != null)
		{
			fcui.zPositionInputField.onEndEdit.RemoveAllListeners();
		}
		if (fcui.zPositionInputFieldAction != null)
		{
			InputFieldAction zPositionInputFieldAction = fcui.zPositionInputFieldAction;
			zPositionInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Remove(zPositionInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetZPositionToInputField));
		}
		if (fcui.xRotationInputField != null)
		{
			fcui.xRotationInputField.onEndEdit.RemoveAllListeners();
		}
		if (fcui.xRotationInputFieldAction != null)
		{
			InputFieldAction xRotationInputFieldAction = fcui.xRotationInputFieldAction;
			xRotationInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Remove(xRotationInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetXRotationToInputField));
		}
		if (fcui.yRotationInputField != null)
		{
			fcui.yRotationInputField.onEndEdit.RemoveAllListeners();
		}
		if (fcui.yRotationInputFieldAction != null)
		{
			InputFieldAction yRotationInputFieldAction = fcui.yRotationInputFieldAction;
			yRotationInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Remove(yRotationInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetYRotationToInputField));
		}
		if (fcui.zRotationInputField != null)
		{
			fcui.zRotationInputField.onEndEdit.RemoveAllListeners();
		}
		if (fcui.zRotationInputFieldAction != null)
		{
			InputFieldAction zRotationInputFieldAction = fcui.zRotationInputFieldAction;
			zRotationInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Remove(zRotationInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetZRotationToInputField));
		}
		if (fcui.xLocalPositionInputField != null)
		{
			fcui.xLocalPositionInputField.onEndEdit.RemoveAllListeners();
		}
		if (fcui.xLocalPositionInputFieldAction != null)
		{
			InputFieldAction xLocalPositionInputFieldAction = fcui.xLocalPositionInputFieldAction;
			xLocalPositionInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Remove(xLocalPositionInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetXLocalPositionToInputField));
		}
		if (fcui.yLocalPositionInputField != null)
		{
			fcui.yLocalPositionInputField.onEndEdit.RemoveAllListeners();
		}
		if (fcui.yLocalPositionInputFieldAction != null)
		{
			InputFieldAction yLocalPositionInputFieldAction = fcui.yLocalPositionInputFieldAction;
			yLocalPositionInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Remove(yLocalPositionInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetYLocalPositionToInputField));
		}
		if (fcui.zLocalPositionInputField != null)
		{
			fcui.zLocalPositionInputField.onEndEdit.RemoveAllListeners();
		}
		if (fcui.zLocalPositionInputFieldAction != null)
		{
			InputFieldAction zLocalPositionInputFieldAction = fcui.zLocalPositionInputFieldAction;
			zLocalPositionInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Remove(zLocalPositionInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetZLocalPositionToInputField));
		}
		if (fcui.xLocalRotationInputField != null)
		{
			fcui.xLocalRotationInputField.onEndEdit.RemoveAllListeners();
		}
		if (fcui.xLocalRotationInputFieldAction != null)
		{
			InputFieldAction xLocalRotationInputFieldAction = fcui.xLocalRotationInputFieldAction;
			xLocalRotationInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Remove(xLocalRotationInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetXLocalRotationToInputField));
		}
		if (fcui.yLocalRotationInputField != null)
		{
			fcui.yLocalRotationInputField.onEndEdit.RemoveAllListeners();
		}
		if (fcui.yLocalRotationInputFieldAction != null)
		{
			InputFieldAction yLocalRotationInputFieldAction = fcui.yLocalRotationInputFieldAction;
			yLocalRotationInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Remove(yLocalRotationInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetYLocalRotationToInputField));
		}
		if (fcui.zLocalRotationInputField != null)
		{
			fcui.zLocalRotationInputField.onEndEdit.RemoveAllListeners();
		}
		if (fcui.zLocalRotationInputFieldAction != null)
		{
			InputFieldAction zLocalRotationInputFieldAction = fcui.zLocalRotationInputFieldAction;
			zLocalRotationInputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Remove(zLocalRotationInputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetZLocalRotationToInputField));
		}
		if (fcui.zeroXLocalPositionButton != null)
		{
			fcui.zeroXLocalPositionButton.onClick.RemoveAllListeners();
		}
		if (fcui.zeroYLocalPositionButton != null)
		{
			fcui.zeroYLocalPositionButton.onClick.RemoveAllListeners();
		}
		if (fcui.zeroZLocalPositionButton != null)
		{
			fcui.zeroZLocalPositionButton.onClick.RemoveAllListeners();
		}
		if (fcui.zeroXLocalRotationButton != null)
		{
			fcui.zeroXLocalRotationButton.onClick.RemoveAllListeners();
		}
		if (fcui.zeroYLocalRotationButton != null)
		{
			fcui.zeroYLocalRotationButton.onClick.RemoveAllListeners();
		}
		if (fcui.zeroZLocalRotationButton != null)
		{
			fcui.zeroZLocalRotationButton.onClick.RemoveAllListeners();
		}
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		FreeControllerV3UI componentInChildren = UITransform.GetComponentInChildren<FreeControllerV3UI>(true);
		if (componentInChildren != null)
		{
			if (currentFCUI != null)
			{
				DeregisterFCUI(currentFCUI);
			}
			currentFCUI = componentInChildren;
			RegisterFCUI(componentInChildren, false);
		}
	}

	public override void InitUIAlt()
	{
		if (!(UITransformAlt != null))
		{
			return;
		}
		FreeControllerV3UI componentInChildren = UITransformAlt.GetComponentInChildren<FreeControllerV3UI>(true);
		if (componentInChildren != null)
		{
			if (currentFCUIAlt != null)
			{
				DeregisterFCUI(currentFCUIAlt);
			}
			currentFCUIAlt = componentInChildren;
			RegisterFCUI(componentInChildren, true);
		}
	}

	public void TakeSnapshot()
	{
		snapshotMatrix = control.localToWorldMatrix;
	}

	private Vector3 GetVectorFromAxis(DrawAxisnames axis)
	{
		Vector3 zero = Vector3.zero;
		switch (axis)
		{
		case DrawAxisnames.X:
			zero.x = 1f;
			break;
		case DrawAxisnames.Y:
			zero.y = 1f;
			break;
		case DrawAxisnames.Z:
			zero.z = 1f;
			break;
		case DrawAxisnames.NegX:
			zero.x = -1f;
			break;
		case DrawAxisnames.NegY:
			zero.y = -1f;
			break;
		case DrawAxisnames.NegZ:
			zero.z = -1f;
			break;
		}
		return zero;
	}

	private void ApplyForce()
	{
		if ((bool)_followWhenOffRB)
		{
			if (_moveForceEnabled)
			{
				_followWhenOffRB.AddForce(appliedForce * Time.fixedDeltaTime, ForceMode.Force);
			}
			if (_rotationForceEnabled)
			{
				_followWhenOffRB.AddRelativeTorque(appliedTorque * Time.fixedDeltaTime, ForceMode.Force);
			}
		}
	}

	protected void SyncComplyPositionThreshold(float f)
	{
		complyPositionThreshold = f;
	}

	protected void SyncComplyRotationThreshold(float f)
	{
		complyRotationThreshold = f;
	}

	protected void SyncComplySpeed(float f)
	{
		complySpeed = f;
	}

	public void PauseComply(int numFrames = 100)
	{
		if (complyPauseFrames < numFrames)
		{
			complyPauseFrames = numFrames;
		}
	}

	private void ApplyComply()
	{
		if (_resetSimulation || _freezeSimulation)
		{
			return;
		}
		if (complyPauseFrames > 0)
		{
			complyPauseFrames--;
		}
		if (complyPauseFrames != 0 || !(control != null) || !(_followWhenOffRB != null) || _followWhenOffRB.isKinematic)
		{
			return;
		}
		bool flag = false;
		if (_currentPositionState == PositionState.Comply)
		{
			Vector3 vector = _followWhenOffRB.position - Physics.gravity * Time.fixedDeltaTime * Time.fixedDeltaTime;
			Vector3 vector2 = (vector - control.position) / _scale;
			if (vector2.sqrMagnitude > complyPositionThreshold)
			{
				control.position += vector2 * Time.fixedDeltaTime * complySpeed;
				if (onPositionChangeHandlers != null)
				{
					onPositionChangeHandlers(this);
				}
				flag = true;
			}
		}
		if (_currentRotationState == RotationState.Comply)
		{
			float num = Quaternion.Angle(control.rotation, _followWhenOffRB.rotation);
			if (num > complyRotationThreshold)
			{
				control.rotation = Quaternion.Lerp(control.rotation, _followWhenOffRB.rotation, Mathf.Clamp01(Time.fixedDeltaTime * complySpeed));
				if (onRotationChangeHandlers != null)
				{
					onRotationChangeHandlers(this);
				}
				flag = true;
			}
		}
		if (flag && onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void MoveControlRelatve(Vector3 move)
	{
		MoveControl(control.position + move);
	}

	public void MoveControl(Vector3 newPosition)
	{
		MoveControl(newPosition, true);
	}

	public void MoveControl(Vector3 newPosition, bool callHandlers)
	{
		if (_positionGridMode == GridMode.Global)
		{
			if (GridControl.singleton != null)
			{
				float num = GridControl.singleton.positionGrid;
				float num2 = Mathf.Round(newPosition.x / num);
				newPosition.x = num2 * num;
				float num3 = Mathf.Round(newPosition.y / num);
				newPosition.y = num3 * num;
				float num4 = Mathf.Round(newPosition.z / num);
				newPosition.z = num4 * num;
			}
		}
		else if (_positionGridMode == GridMode.Local)
		{
			float num5 = Mathf.Round(newPosition.x / _positionGrid);
			newPosition.x = num5 * _positionGrid;
			float num6 = Mathf.Round(newPosition.y / _positionGrid);
			newPosition.y = num6 * _positionGrid;
			float num7 = Mathf.Round(newPosition.z / _positionGrid);
			newPosition.z = num7 * _positionGrid;
		}
		Vector3 position = control.position;
		Vector3 vector = Vector3.zero;
		if (_xLocalLock || _yLocalLock || _zLocalLock)
		{
			vector = GetLocalPosition();
		}
		if (!_xLock)
		{
			position.x = newPosition.x;
		}
		if (!_yLock)
		{
			position.y = newPosition.y;
		}
		if (!_zLock)
		{
			position.z = newPosition.z;
		}
		control.position = position;
		if (_xLocalLock || _yLocalLock || _zLocalLock)
		{
			Vector3 localPosition = GetLocalPosition();
			if (_xLocalLock)
			{
				localPosition.x = vector.x;
			}
			if (_yLocalLock)
			{
				localPosition.y = vector.y;
			}
			if (_zLocalLock)
			{
				localPosition.z = vector.z;
			}
			SetLocalPosition(localPosition);
		}
		if (callHandlers)
		{
			if (onPositionChangeHandlers != null)
			{
				onPositionChangeHandlers(this);
			}
			if (onMovementHandlers != null)
			{
				onMovementHandlers(this);
			}
		}
	}

	public void MoveLinkConnectorTowards(Transform t, float moveDistance)
	{
		if (_linkToConnector != null)
		{
			_linkToConnector.transform.Translate(0f, 0f, moveDistance, t);
		}
	}

	public void RotateControl(Vector3 axis, float angle)
	{
		RotateControl(axis, angle, true);
	}

	public void RotateControl(Vector3 axis, float angle, bool callHandlers)
	{
		Vector3 eulerAngles = control.eulerAngles;
		control.transform.Rotate(axis, angle, Space.World);
		Vector3 eulerAngles2 = control.eulerAngles;
		RotateControlContrained(eulerAngles, eulerAngles2, callHandlers);
	}

	public void RotateControl(Vector3 newWorldRotation)
	{
		RotateControl(newWorldRotation, true);
	}

	public void RotateControl(Vector3 newWorldRotation, bool callHandlers)
	{
		Vector3 eulerAngles = control.eulerAngles;
		RotateControlContrained(eulerAngles, newWorldRotation, callHandlers);
	}

	private void RotateControlContrained(Vector3 oldRotation, Vector3 newRotation, bool callHandlers)
	{
		if (_rotationGridMode == GridMode.Global)
		{
			if (GridControl.singleton != null)
			{
				float num = GridControl.singleton.rotationGrid;
				float num2 = Mathf.Round(newRotation.x / num);
				newRotation.x = num2 * num;
				float num3 = Mathf.Round(newRotation.y / num);
				newRotation.y = num3 * num;
				float num4 = Mathf.Round(newRotation.z / num);
				newRotation.z = num4 * num;
			}
		}
		else if (_rotationGridMode == GridMode.Local)
		{
			float num5 = Mathf.Round(newRotation.x / _rotationGrid);
			newRotation.x = num5 * _rotationGrid;
			float num6 = Mathf.Round(newRotation.y / _rotationGrid);
			newRotation.y = num6 * _rotationGrid;
			float num7 = Mathf.Round(newRotation.z / _rotationGrid);
			newRotation.z = num7 * _rotationGrid;
		}
		if (!xRotLock)
		{
			oldRotation.x = newRotation.x;
		}
		if (!yRotLock)
		{
			oldRotation.y = newRotation.y;
		}
		if (!zRotLock)
		{
			oldRotation.z = newRotation.z;
		}
		control.eulerAngles = oldRotation;
		if (callHandlers)
		{
			if (onRotationChangeHandlers != null)
			{
				onRotationChangeHandlers(this);
			}
			if (onMovementHandlers != null)
			{
				onMovementHandlers(this);
			}
		}
	}

	private void SyncMoveWhenInactive()
	{
		if (!(alsoMoveWhenInactive != null) || !(alsoMoveWhenInactiveParentWhenActive != null) || !(alsoMoveWhenInactiveParentWhenInactive != null))
		{
			return;
		}
		if (alsoMoveWhenInactive.gameObject.activeInHierarchy && !_freezeSimulation)
		{
			if (alsoMoveWhenInactive.parent != alsoMoveWhenInactiveParentWhenActive)
			{
				alsoMoveWhenInactive.SetParent(alsoMoveWhenInactiveParentWhenActive);
				if (alsoMoveWhenInactiveAlternate != null)
				{
					Vector3 position = alsoMoveWhenInactiveAlternate.position;
					Quaternion rotation = alsoMoveWhenInactiveAlternate.rotation;
					alsoMoveWhenInactive.localPosition = Vector3.zero;
					alsoMoveWhenInactive.localRotation = Quaternion.identity;
					alsoMoveWhenInactive.localScale = Vector3.one;
					alsoMoveWhenInactiveAlternate.position = position;
					alsoMoveWhenInactiveAlternate.rotation = rotation;
				}
			}
			return;
		}
		if (!_detachControl)
		{
			alsoMoveWhenInactiveParentWhenInactive.position = control.position;
			alsoMoveWhenInactiveParentWhenInactive.rotation = control.rotation;
		}
		if (alsoMoveWhenInactive.parent != alsoMoveWhenInactiveParentWhenInactive)
		{
			alsoMoveWhenInactive.SetParent(alsoMoveWhenInactiveParentWhenInactive);
			if (alsoMoveWhenInactiveAlternate != null)
			{
				Vector3 position2 = alsoMoveWhenInactiveAlternate.position;
				Quaternion rotation2 = alsoMoveWhenInactiveAlternate.rotation;
				alsoMoveWhenInactive.localPosition = Vector3.zero;
				alsoMoveWhenInactive.localRotation = Quaternion.identity;
				alsoMoveWhenInactive.localScale = Vector3.one;
				alsoMoveWhenInactiveAlternate.position = position2;
				alsoMoveWhenInactiveAlternate.rotation = rotation2;
			}
		}
	}

	private void UpdateTransform(bool updateGUI)
	{
		if (_currentPositionState == PositionState.Off)
		{
			if ((bool)followWhenOff)
			{
				Vector3 position = followWhenOff.position;
				if (NaNUtils.IsVector3Valid(position))
				{
					control.position = followWhenOff.position;
				}
				else if (containingAtom != null)
				{
					containingAtom.AlertPhysicsCorruption("FreeController position " + base.name);
				}
			}
		}
		else if (_currentPositionState == PositionState.Following)
		{
			if ((bool)follow)
			{
				MoveControl(follow.position, false);
			}
		}
		else if ((_currentPositionState == PositionState.ParentLink || _currentPositionState == PositionState.PhysicsLink) && (bool)_linkToConnector)
		{
			MoveControl(_linkToConnector.position, _isGrabbing);
		}
		if (_currentRotationState == RotationState.Off)
		{
			if ((bool)followWhenOff)
			{
				control.rotation = followWhenOff.rotation;
			}
		}
		else if (_currentRotationState == RotationState.Following)
		{
			if ((bool)follow)
			{
				RotateControl(follow.eulerAngles, false);
			}
		}
		else if (_currentRotationState == RotationState.LookAt)
		{
			control.LookAt(lookAt.position);
		}
		else if ((_currentRotationState == RotationState.ParentLink || _currentPositionState == PositionState.PhysicsLink) && (bool)_linkToConnector)
		{
			RotateControl(_linkToConnector.eulerAngles, _isGrabbing);
		}
		if (control != base.transform)
		{
			base.transform.position = control.position;
			base.transform.rotation = control.rotation;
		}
		SyncMoveWhenInactive();
		if (_followWhenOffRB != null)
		{
			if (!_followWhenOffRB.gameObject.activeInHierarchy)
			{
				if (!_followWhenOffRB.isKinematic || !_detachControl)
				{
					_followWhenOffRB.position = control.position;
					followWhenOff.position = control.position;
					_followWhenOffRB.rotation = control.rotation;
					followWhenOff.rotation = control.rotation;
				}
			}
			else if (_followWhenOffRB.isKinematic && !_detachControl)
			{
				followWhenOff.position = control.position;
				followWhenOff.rotation = control.rotation;
			}
		}
		if (SuperController.singleton != null && !SuperController.singleton.autoSimulation && kinematicRB != null)
		{
			kinematicRB.position = control.position;
			kinematicRB.rotation = control.rotation;
		}
		if (updateGUI && !_guihidden)
		{
			Vector3 position2 = control.position;
			if (xPositionText != null)
			{
				xPositionText.floatVal = position2.x;
			}
			if (yPositionText != null)
			{
				yPositionText.floatVal = position2.y;
			}
			if (zPositionText != null)
			{
				zPositionText.floatVal = position2.z;
			}
			Vector3 eulerAngles = control.eulerAngles;
			if (xRotationText != null)
			{
				xRotationText.floatVal = eulerAngles.x;
			}
			if (yRotationText != null)
			{
				yRotationText.floatVal = eulerAngles.y;
			}
			if (zRotationText != null)
			{
				zRotationText.floatVal = eulerAngles.z;
			}
			position2 = GetLocalPosition();
			if (xLocalPositionText != null)
			{
				xLocalPositionText.floatVal = position2.x;
			}
			if (yLocalPositionText != null)
			{
				yLocalPositionText.floatVal = position2.y;
			}
			if (zLocalPositionText != null)
			{
				zLocalPositionText.floatVal = position2.z;
			}
			eulerAngles = GetLocalEulerAngles();
			if (xLocalRotationText != null)
			{
				xLocalRotationText.floatVal = eulerAngles.x;
			}
			if (yLocalRotationText != null)
			{
				yLocalRotationText.floatVal = eulerAngles.y;
			}
			if (zLocalRotationText != null)
			{
				zLocalRotationText.floatVal = eulerAngles.z;
			}
		}
	}

	public void ShowGUI()
	{
		SyncAtomUID();
		bool flag = false;
		if (SuperController.singleton != null && SuperController.singleton.gameMode == SuperController.GameMode.Play)
		{
			flag = true;
		}
		if (flag)
		{
			Transform[] uITransforms = UITransforms;
			foreach (Transform transform in uITransforms)
			{
				if (transform != null)
				{
					transform.gameObject.SetActive(false);
				}
			}
			Transform[] uITransformsPlayMode = UITransformsPlayMode;
			foreach (Transform transform2 in uITransformsPlayMode)
			{
				if (transform2 != null)
				{
					transform2.gameObject.SetActive(true);
				}
			}
			return;
		}
		Transform[] uITransformsPlayMode2 = UITransformsPlayMode;
		foreach (Transform transform3 in uITransformsPlayMode2)
		{
			if (transform3 != null)
			{
				transform3.gameObject.SetActive(false);
			}
		}
		Transform[] uITransforms2 = UITransforms;
		foreach (Transform transform4 in uITransforms2)
		{
			if (transform4 != null)
			{
				transform4.gameObject.SetActive(true);
			}
		}
	}

	public void HideGUI()
	{
		bool flag = false;
		if (SuperController.singleton != null && SuperController.singleton.gameMode == SuperController.GameMode.Play)
		{
			flag = true;
		}
		Transform[] uITransforms = UITransforms;
		foreach (Transform transform in uITransforms)
		{
			if (!(transform != null))
			{
				continue;
			}
			if (flag)
			{
				transform.gameObject.SetActive(false);
				continue;
			}
			UIVisibility component = transform.GetComponent<UIVisibility>();
			if (component != null)
			{
				if (!component.keepVisible)
				{
					transform.gameObject.SetActive(false);
				}
			}
			else
			{
				transform.gameObject.SetActive(false);
			}
		}
		Transform[] uITransformsPlayMode = UITransformsPlayMode;
		foreach (Transform transform2 in uITransformsPlayMode)
		{
			if (!(transform2 != null))
			{
				continue;
			}
			if (!flag)
			{
				transform2.gameObject.SetActive(false);
				continue;
			}
			UIVisibility component2 = transform2.GetComponent<UIVisibility>();
			if (component2 != null)
			{
				if (!component2.keepVisible)
				{
					transform2.gameObject.SetActive(false);
				}
			}
			else
			{
				transform2.gameObject.SetActive(false);
			}
		}
	}

	private void SetColor()
	{
		if (_selected)
		{
			_currentPositionColor = selectedColor;
			_currentRotationColor = selectedColor;
		}
		else if (_highlighted)
		{
			_currentPositionColor = highlightColor;
			_currentRotationColor = highlightColor;
		}
		else
		{
			switch (_currentPositionState)
			{
			case PositionState.On:
			case PositionState.Comply:
				_currentPositionColor = onColor;
				break;
			case PositionState.Off:
				_currentPositionColor = offColor;
				break;
			case PositionState.Following:
			case PositionState.ParentLink:
			case PositionState.PhysicsLink:
				_currentPositionColor = followingColor;
				break;
			case PositionState.Hold:
				_currentPositionColor = holdColor;
				break;
			case PositionState.Lock:
				_currentPositionColor = lockColor;
				break;
			}
			switch (_currentRotationState)
			{
			case RotationState.On:
			case RotationState.Comply:
				_currentRotationColor = onColor;
				break;
			case RotationState.Off:
				_currentRotationColor = offColor;
				break;
			case RotationState.Following:
			case RotationState.ParentLink:
			case RotationState.PhysicsLink:
				_currentRotationColor = followingColor;
				break;
			case RotationState.Hold:
				_currentRotationColor = holdColor;
				break;
			case RotationState.Lock:
				_currentRotationColor = lockColor;
				break;
			case RotationState.LookAt:
				_currentRotationColor = lookAtColor;
				break;
			}
		}
		if (mrs != null)
		{
			MeshRenderer[] array = mrs;
			foreach (MeshRenderer meshRenderer in array)
			{
				meshRenderer.material.color = _currentPositionColor;
			}
		}
	}

	private void SetMesh()
	{
		switch (_currentPositionState)
		{
		case PositionState.Off:
			_currentPositionMesh = offPositionMesh;
			break;
		case PositionState.On:
		case PositionState.Comply:
			_currentPositionMesh = onPositionMesh;
			break;
		case PositionState.Following:
		case PositionState.ParentLink:
		case PositionState.PhysicsLink:
			_currentPositionMesh = followingPositionMesh;
			break;
		case PositionState.Hold:
			_currentPositionMesh = holdPositionMesh;
			break;
		case PositionState.Lock:
			_currentPositionMesh = lockPositionMesh;
			break;
		}
		switch (_currentRotationState)
		{
		case RotationState.Off:
			_currentRotationMesh = offRotationMesh;
			break;
		case RotationState.On:
		case RotationState.Comply:
			_currentRotationMesh = onRotationMesh;
			break;
		case RotationState.Following:
		case RotationState.ParentLink:
		case RotationState.PhysicsLink:
			_currentRotationMesh = followingRotationMesh;
			break;
		case RotationState.Hold:
			_currentRotationMesh = holdRotationMesh;
			break;
		case RotationState.Lock:
			_currentRotationMesh = lockRotationMesh;
			break;
		case RotationState.LookAt:
			_currentRotationMesh = lookAtRotationMesh;
			break;
		}
	}

	private void StateChanged()
	{
		SetMesh();
		SetColor();
	}

	public void ResetControl()
	{
		if (wasInit)
		{
			control.localPosition = initialLocalPosition;
			control.localRotation = initialLocalRotation;
			if (onPositionChangeHandlers != null)
			{
				onPositionChangeHandlers(this);
			}
			if (onRotationChangeHandlers != null)
			{
				onRotationChangeHandlers(this);
			}
			if (onMovementHandlers != null)
			{
				onMovementHandlers(this);
			}
		}
	}

	private void Move(Vector3 direction)
	{
		if (_moveForceEnabled && (bool)_followWhenOffRB && useForceWhenOff)
		{
			appliedForce += direction * forceFactor;
		}
		else if (_moveEnabled)
		{
			Vector3 translation = direction * moveFactor * Time.unscaledDeltaTime;
			control.Translate(translation, Space.World);
			if (connectedJoint != null)
			{
				_followWhenOffRB.WakeUp();
			}
			if (onPositionChangeHandlers != null)
			{
				onPositionChangeHandlers(this);
			}
			if (onMovementHandlers != null)
			{
				onMovementHandlers(this);
			}
		}
	}

	public void SetPositionNoForce(Vector3 position)
	{
		if (!_moveForceEnabled && _moveEnabled)
		{
			control.position = position;
			if (onPositionChangeHandlers != null)
			{
				onPositionChangeHandlers(this);
			}
			if (onMovementHandlers != null)
			{
				onMovementHandlers(this);
			}
		}
	}

	public void SetLocalPositionNoForce(Vector3 position)
	{
		if (!_moveForceEnabled && _moveEnabled)
		{
			SetLocalPosition(position);
			if (onPositionChangeHandlers != null)
			{
				onPositionChangeHandlers(this);
			}
			if (onMovementHandlers != null)
			{
				onMovementHandlers(this);
			}
		}
	}

	public void MoveAbsoluteNoForce(Vector3 direction)
	{
		if (!_moveForceEnabled && _moveEnabled)
		{
			control.Translate(direction, Space.World);
			if (onPositionChangeHandlers != null)
			{
				onPositionChangeHandlers(this);
			}
			if (onMovementHandlers != null)
			{
				onMovementHandlers(this);
			}
		}
	}

	public void MoveAbsoluteNoForce(float x, float y, float z)
	{
		Vector3 direction = default(Vector3);
		direction.x = x;
		direction.y = y;
		direction.z = z;
		MoveAbsoluteNoForce(direction);
	}

	public void MoveRelativeNoForce(Vector3 direction)
	{
		if (!_moveForceEnabled && _moveEnabled)
		{
			control.Translate(direction, Space.Self);
			if (onPositionChangeHandlers != null)
			{
				onPositionChangeHandlers(this);
			}
			if (onMovementHandlers != null)
			{
				onMovementHandlers(this);
			}
		}
	}

	public void MoveRelativeNoForce(float x, float y, float z)
	{
		Vector3 direction = default(Vector3);
		direction.x = x;
		direction.y = y;
		direction.z = z;
		MoveRelativeNoForce(direction);
	}

	public void MoveXPositionRelativeNoForce(float f)
	{
		MoveRelativeNoForce(f, 0f, 0f);
	}

	public void MoveXPositionRelativeNoForce(string val)
	{
		float result;
		if (float.TryParse(val, out result))
		{
			MoveRelativeNoForce(result, 0f, 0f);
		}
	}

	public void MoveYPositionRelativeNoForce(float f)
	{
		MoveRelativeNoForce(0f, f, 0f);
	}

	public void MoveYPositionRelativeNoForce(string val)
	{
		float result;
		if (float.TryParse(val, out result))
		{
			MoveRelativeNoForce(0f, result, 0f);
		}
	}

	public void MoveZPositionRelativeNoForce(float f)
	{
		MoveRelativeNoForce(0f, 0f, f);
	}

	public void MoveZPositionRelativeNoForce(string val)
	{
		float result;
		if (float.TryParse(val, out result))
		{
			MoveRelativeNoForce(0f, 0f, result);
		}
	}

	public void MoveTo(Vector3 pos, bool alsoMoveRB = false)
	{
		if (!_moveForceEnabled && _moveEnabled)
		{
			control.position = pos;
			if (alsoMoveRB && followWhenOff != null && !_detachControl)
			{
				followWhenOff.position = pos;
			}
			if (onPositionChangeHandlers != null)
			{
				onPositionChangeHandlers(this);
			}
			if (onMovementHandlers != null)
			{
				onMovementHandlers(this);
			}
		}
	}

	public void MoveTo(float x, float y, float z)
	{
		Vector3 pos = default(Vector3);
		pos.x = x;
		pos.y = y;
		pos.z = z;
		MoveTo(pos);
	}

	public void PossessMoveAndAlignTo(Transform t)
	{
		if (_canGrabRotation)
		{
			AlignTo(t, true);
		}
		if (!_canGrabPosition)
		{
			return;
		}
		if (possessPoint != null && followWhenOff != null)
		{
			Vector3 position = t.position + (followWhenOff.position - possessPoint.position);
			control.position = position;
			if (!_detachControl)
			{
				followWhenOff.position = position;
			}
		}
		else
		{
			control.position = t.position;
			if (followWhenOff != null && !_detachControl)
			{
				followWhenOff.position = t.position;
			}
		}
		if (onPositionChangeHandlers != null)
		{
			onPositionChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void SetRotationNoForce(Vector3 rotation)
	{
		if (!_rotationForceEnabled && _rotationEnabled)
		{
			control.eulerAngles = rotation;
			if (onRotationChangeHandlers != null)
			{
				onRotationChangeHandlers(this);
			}
			if (onMovementHandlers != null)
			{
				onMovementHandlers(this);
			}
		}
	}

	public void SetLocalRotationNoForce(Vector3 rotation)
	{
		if (!_rotationForceEnabled && _rotationEnabled)
		{
			SetLocalEulerAngles(rotation);
			if (onRotationChangeHandlers != null)
			{
				onRotationChangeHandlers(this);
			}
			if (onMovementHandlers != null)
			{
				onMovementHandlers(this);
			}
		}
	}

	public void RotateSelfRelativeNoForce(Vector3 rotation)
	{
		if (!_rotationForceEnabled && _rotationEnabled)
		{
			control.Rotate(rotation, Space.Self);
			if (onRotationChangeHandlers != null)
			{
				onRotationChangeHandlers(this);
			}
			if (onMovementHandlers != null)
			{
				onMovementHandlers(this);
			}
		}
	}

	public void RotateXSelfRelativeNoForce(float f)
	{
		RotateSelfRelativeNoForce(new Vector3(f, 0f, 0f));
	}

	public void RotateXSelfRelativeNoForce(string val)
	{
		float result;
		if (float.TryParse(val, out result))
		{
			RotateSelfRelativeNoForce(new Vector3(result, 0f, 0f));
		}
	}

	public void RotateYSelfRelativeNoForce(float f)
	{
		RotateSelfRelativeNoForce(new Vector3(0f, f, 0f));
	}

	public void RotateYSelfRelativeNoForce(string val)
	{
		float result;
		if (float.TryParse(val, out result))
		{
			RotateSelfRelativeNoForce(new Vector3(0f, result, 0f));
		}
	}

	public void RotateZSelfRelativeNoForce(float f)
	{
		RotateSelfRelativeNoForce(new Vector3(0f, 0f, f));
	}

	public void RotateZSelfRelativeNoForce(string val)
	{
		float result;
		if (float.TryParse(val, out result))
		{
			RotateSelfRelativeNoForce(new Vector3(0f, 0f, result));
		}
	}

	public void RotateTo(Quaternion q)
	{
		if (!_rotationForceEnabled && _rotationEnabled)
		{
			control.rotation = q;
			if (onRotationChangeHandlers != null)
			{
				onRotationChangeHandlers(this);
			}
			if (onMovementHandlers != null)
			{
				onMovementHandlers(this);
			}
		}
	}

	public Vector3 GetForwardPossessAxis()
	{
		Vector3 result = Vector3.forward;
		switch (PossessForwardAxis)
		{
		case DrawAxisnames.X:
			result = base.transform.right;
			break;
		case DrawAxisnames.NegX:
			result = -base.transform.right;
			break;
		case DrawAxisnames.Y:
			result = base.transform.up;
			break;
		case DrawAxisnames.NegY:
			result = -base.transform.up;
			break;
		case DrawAxisnames.Z:
			result = base.transform.forward;
			break;
		case DrawAxisnames.NegZ:
			result = -base.transform.forward;
			break;
		}
		return result;
	}

	public Vector3 GetUpPossessAxis()
	{
		Vector3 result = Vector3.up;
		switch (PossessUpAxis)
		{
		case DrawAxisnames.X:
			result = base.transform.right;
			break;
		case DrawAxisnames.NegX:
			result = -base.transform.right;
			break;
		case DrawAxisnames.Y:
			result = base.transform.up;
			break;
		case DrawAxisnames.NegY:
			result = -base.transform.up;
			break;
		case DrawAxisnames.Z:
			result = base.transform.forward;
			break;
		case DrawAxisnames.NegZ:
			result = -base.transform.forward;
			break;
		}
		return result;
	}

	public void AlignTo(Transform t, bool alsoRotateRB = false)
	{
		Quaternion rotation = control.rotation;
		Vector3 view = Vector3.forward;
		Vector3 up = Vector3.up;
		switch (PossessForwardAxis)
		{
		case DrawAxisnames.X:
			view = t.right;
			break;
		case DrawAxisnames.NegX:
			view = -t.right;
			break;
		case DrawAxisnames.Y:
			view = t.up;
			break;
		case DrawAxisnames.NegY:
			view = -t.up;
			break;
		case DrawAxisnames.Z:
			view = t.forward;
			break;
		case DrawAxisnames.NegZ:
			view = -t.forward;
			break;
		}
		switch (PossessUpAxis)
		{
		case DrawAxisnames.X:
			up = t.right;
			break;
		case DrawAxisnames.NegX:
			up = -t.right;
			break;
		case DrawAxisnames.Y:
			up = t.up;
			break;
		case DrawAxisnames.NegY:
			up = -t.up;
			break;
		case DrawAxisnames.Z:
			up = t.forward;
			break;
		case DrawAxisnames.NegZ:
			up = -t.forward;
			break;
		}
		rotation.SetLookRotation(view, up);
		control.rotation = rotation;
		if (alsoRotateRB && followWhenOff != null && !_detachControl)
		{
			followWhenOff.rotation = rotation;
		}
		if (onRotationChangeHandlers != null)
		{
			onRotationChangeHandlers(this);
		}
		if (onMovementHandlers != null)
		{
			onMovementHandlers(this);
		}
	}

	public void RotateX(float val)
	{
		if (_rotationForceEnabled && (bool)_followWhenOffRB && useForceWhenOff)
		{
			appliedTorque.x = val * torqueFactor;
		}
		else if (_rotationEnabled)
		{
			control.Rotate(new Vector3(val * rotateFactor * Time.unscaledDeltaTime, 0f, 0f));
			if (connectedJoint != null)
			{
				_followWhenOffRB.WakeUp();
			}
			if (onRotationChangeHandlers != null)
			{
				onRotationChangeHandlers(this);
			}
			if (onMovementHandlers != null)
			{
				onMovementHandlers(this);
			}
		}
	}

	public void RotateY(float val)
	{
		if (_rotationForceEnabled && (bool)_followWhenOffRB && useForceWhenOff)
		{
			appliedTorque.y = val * torqueFactor;
		}
		else if (_rotationEnabled)
		{
			control.Rotate(new Vector3(0f, val * rotateFactor * Time.unscaledDeltaTime, 0f));
			if (connectedJoint != null)
			{
				_followWhenOffRB.WakeUp();
			}
			if (onRotationChangeHandlers != null)
			{
				onRotationChangeHandlers(this);
			}
			if (onMovementHandlers != null)
			{
				onMovementHandlers(this);
			}
		}
	}

	public void RotateZ(float val)
	{
		if (_rotationForceEnabled && (bool)_followWhenOffRB && useForceWhenOff)
		{
			appliedTorque.z = val * torqueFactor;
		}
		else if (_rotationEnabled)
		{
			control.Rotate(new Vector3(0f, 0f, val * rotateFactor * Time.unscaledDeltaTime));
			if (connectedJoint != null)
			{
				_followWhenOffRB.WakeUp();
			}
			if (onRotationChangeHandlers != null)
			{
				onRotationChangeHandlers(this);
			}
			if (onMovementHandlers != null)
			{
				onMovementHandlers(this);
			}
		}
	}

	public void RotateWorldX(float val, bool absolute = false)
	{
		if (!_rotationForceEnabled && _rotationEnabled)
		{
			float num = val;
			if (!absolute)
			{
				num *= rotateFactor * Time.unscaledDeltaTime;
			}
			control.Rotate(num, 0f, 0f, Space.World);
			if (onRotationChangeHandlers != null)
			{
				onRotationChangeHandlers(this);
			}
			if (onMovementHandlers != null)
			{
				onMovementHandlers(this);
			}
		}
	}

	public void RotateWorldY(float val, bool absolute = false)
	{
		if (!_rotationForceEnabled && _rotationEnabled)
		{
			float num = val;
			if (!absolute)
			{
				num *= rotateFactor * Time.unscaledDeltaTime;
			}
			control.Rotate(0f, num, 0f, Space.World);
			if (onRotationChangeHandlers != null)
			{
				onRotationChangeHandlers(this);
			}
			if (onMovementHandlers != null)
			{
				onMovementHandlers(this);
			}
		}
	}

	public void RotateWorldZ(float val, bool absolute = false)
	{
		if (!_rotationForceEnabled && _rotationEnabled)
		{
			float num = val;
			if (!absolute)
			{
				num *= rotateFactor * Time.unscaledDeltaTime;
			}
			control.Rotate(0f, 0f, num, Space.World);
			if (onRotationChangeHandlers != null)
			{
				onRotationChangeHandlers(this);
			}
			if (onMovementHandlers != null)
			{
				onMovementHandlers(this);
			}
		}
	}

	public void ResetAppliedForces()
	{
		appliedForce.x = 0f;
		appliedForce.y = 0f;
		appliedForce.z = 0f;
		appliedTorque.x = 0f;
		appliedTorque.y = 0f;
		appliedTorque.z = 0f;
	}

	public void MoveAxis(MoveAxisnames man, float val)
	{
		switch (man)
		{
		case MoveAxisnames.X:
			Move(new Vector3(val, 0f, 0f));
			break;
		case MoveAxisnames.Y:
			Move(new Vector3(0f, val, 0f));
			break;
		case MoveAxisnames.Z:
			Move(new Vector3(0f, 0f, val));
			break;
		case MoveAxisnames.CameraRight:
		{
			Vector3 direction5 = Camera.main.transform.right * val;
			Move(direction5);
			break;
		}
		case MoveAxisnames.CameraRightNoY:
		{
			Vector3 direction4 = Camera.main.transform.right * val;
			direction4.y = 0f;
			Move(direction4);
			break;
		}
		case MoveAxisnames.CameraForward:
		{
			Vector3 direction3 = Camera.main.transform.forward * val;
			Move(direction3);
			break;
		}
		case MoveAxisnames.CameraForwardNoY:
		{
			Vector3 direction2 = Camera.main.transform.forward * val;
			direction2.y = 0f;
			Move(direction2);
			break;
		}
		case MoveAxisnames.CameraUp:
		{
			Vector3 direction = Camera.main.transform.up * val;
			Move(direction);
			break;
		}
		}
	}

	public void RotateAxis(RotateAxisnames ran, float val)
	{
		switch (ran)
		{
		case RotateAxisnames.X:
			RotateX(val);
			break;
		case RotateAxisnames.NegX:
			RotateX(0f - val);
			break;
		case RotateAxisnames.Y:
			RotateY(val);
			break;
		case RotateAxisnames.NegY:
			RotateY(0f - val);
			break;
		case RotateAxisnames.Z:
			RotateZ(val);
			break;
		case RotateAxisnames.NegZ:
			RotateZ(0f - val);
			break;
		case RotateAxisnames.WorldY:
			RotateWorldY(val);
			break;
		}
	}

	public void ControlAxis1(float val)
	{
		if (controlMode == ControlMode.Rotation)
		{
			RotateAxis(RotateAxis1, val);
		}
		else if (controlMode == ControlMode.Position)
		{
			MoveAxis(MoveAxis1, val);
		}
	}

	public void ControlAxis2(float val)
	{
		if (controlMode == ControlMode.Rotation)
		{
			RotateAxis(RotateAxis2, val);
		}
		else if (controlMode == ControlMode.Position)
		{
			MoveAxis(MoveAxis2, val);
		}
	}

	public void ControlAxis3(float val)
	{
		if (controlMode == ControlMode.Rotation)
		{
			RotateAxis(RotateAxis3, val);
		}
		else if (controlMode == ControlMode.Position)
		{
			MoveAxis(MoveAxis3, val);
		}
	}

	private void Init()
	{
		if (wasInit)
		{
			return;
		}
		wasInit = true;
		if (control == null)
		{
			control = base.transform;
		}
		if ((bool)linkLineMaterial)
		{
			linkLineMaterialLocal = UnityEngine.Object.Instantiate(linkLineMaterial);
			linkLineDrawer = new LineDrawer(linkLineMaterialLocal);
			RegisterAllocatedObject(linkLineMaterialLocal);
		}
		if (useContainedMeshRenderers)
		{
			mrs = GetComponentsInChildren<MeshRenderer>();
		}
		if ((bool)material)
		{
			positionMaterialLocal = UnityEngine.Object.Instantiate(material);
			RegisterAllocatedObject(positionMaterialLocal);
			rotationMaterialLocal = UnityEngine.Object.Instantiate(material);
			RegisterAllocatedObject(rotationMaterialLocal);
			snapshotMaterialLocal = UnityEngine.Object.Instantiate(material);
			RegisterAllocatedObject(snapshotMaterialLocal);
			materialOverlay = UnityEngine.Object.Instantiate(material);
			RegisterAllocatedObject(materialOverlay);
		}
		if ((bool)followWhenOff)
		{
			_followWhenOffRB = followWhenOff.GetComponent<Rigidbody>();
			detachControlJSON = new JSONStorableBool("detachControl", _detachControl, SyncDetachControl);
			detachControlJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterBool(detachControlJSON);
		}
		kinematicRB = GetComponent<Rigidbody>();
		if (kinematicRB != null && (bool)followWhenOff)
		{
			control.position = followWhenOff.position;
			control.rotation = followWhenOff.rotation;
			ConfigurableJoint[] components = followWhenOff.GetComponents<ConfigurableJoint>();
			ConfigurableJoint[] array = components;
			foreach (ConfigurableJoint configurableJoint in array)
			{
				if (configurableJoint.connectedBody == kinematicRB)
				{
					connectedJoint = configurableJoint;
					configurableJoint.connectedAnchor = Vector3.zero;
					SetJointSprings();
					continue;
				}
				naturalJoint = configurableJoint;
				JointDrive slerpDrive = naturalJoint.slerpDrive;
				_jointRotationDriveSpring = slerpDrive.positionSpring;
				_jointRotationDriveDamper = slerpDrive.positionDamper;
				_jointRotationDriveMaxForce = slerpDrive.maximumForce;
				Vector3 eulerAngles = naturalJoint.targetRotation.eulerAngles;
				if (eulerAngles.x > 180f)
				{
					eulerAngles.x -= 360f;
				}
				else if (eulerAngles.x < -180f)
				{
					eulerAngles.x += 360f;
				}
				if (eulerAngles.y > 180f)
				{
					eulerAngles.y -= 360f;
				}
				else if (eulerAngles.y < -180f)
				{
					eulerAngles.y += 360f;
				}
				if (eulerAngles.z > 180f)
				{
					eulerAngles.z -= 360f;
				}
				else if (eulerAngles.z < -180f)
				{
					eulerAngles.z += 360f;
				}
				_jointRotationDriveXTarget = eulerAngles.x;
				_jointRotationDriveYTarget = eulerAngles.y;
				_jointRotationDriveZTarget = eulerAngles.z;
				if (naturalJoint.lowAngularXLimit.limit < naturalJoint.highAngularXLimit.limit)
				{
					_jointRotationDriveXTargetMin = naturalJoint.lowAngularXLimit.limit;
					_jointRotationDriveXTargetMax = naturalJoint.highAngularXLimit.limit;
				}
				else
				{
					_jointRotationDriveXTargetMin = naturalJoint.highAngularXLimit.limit;
					_jointRotationDriveXTargetMax = naturalJoint.lowAngularXLimit.limit;
				}
				_jointRotationDriveYTargetMin = 0f - naturalJoint.angularYLimit.limit;
				_jointRotationDriveYTargetMax = naturalJoint.angularYLimit.limit;
				_jointRotationDriveZTargetMin = 0f - naturalJoint.angularZLimit.limit;
				_jointRotationDriveZTargetMax = naturalJoint.angularZLimit.limit;
			}
		}
		startingPosition = base.transform.position;
		startingRotation = base.transform.rotation;
		startingLocalPosition = base.transform.localPosition;
		startingLocalRotation = base.transform.localRotation;
		if (stateCanBeModified)
		{
			string[] names = Enum.GetNames(typeof(PositionState));
			List<string> choicesList = new List<string>(names);
			currentPositionStateJSON = new JSONStorableStringChooser("positionState", choicesList, startingPositionState.ToString(), "Position State", SetPositionStateFromString);
			currentPositionStateJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterStringChooser(currentPositionStateJSON);
			string[] names2 = Enum.GetNames(typeof(RotationState));
			List<string> choicesList2 = new List<string>(names2);
			currentRotationStateJSON = new JSONStorableStringChooser("rotationState", choicesList2, startingRotationState.ToString(), "Rotation State", SetRotationStateFromString);
			currentRotationStateJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterStringChooser(currentRotationStateJSON);
			complyPositionThresholdJSON = new JSONStorableFloat("complyPositionThreshold", complyPositionThreshold, SyncComplyPositionThreshold, 0.0001f, 0.1f);
			complyPositionThresholdJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(complyPositionThresholdJSON);
			complyRotationThresholdJSON = new JSONStorableFloat("complyRotationThreshold", complyRotationThreshold, SyncComplyRotationThreshold, 0.1f, 30f);
			complyRotationThresholdJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(complyRotationThresholdJSON);
			complySpeedJSON = new JSONStorableFloat("complySpeed", complySpeed, SyncComplySpeed, 0f, 100f);
			complySpeedJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(complySpeedJSON);
		}
		if (controlsOn)
		{
			onJSON = new JSONStorableBool("on", _on, SyncOn);
			onJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterBool(onJSON);
			SyncOn(_on);
		}
		resetAction = new JSONStorableAction("Reset", Reset);
		RegisterAction(resetAction);
		interactableInPlayModeJSON = new JSONStorableBool("interactableInPlayMode", _interactableInPlayMode, SyncInteractableInPlayMode);
		interactableInPlayModeJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(interactableInPlayModeJSON);
		deactivateOtherControlsOnPossessJSON = new JSONStorableBool("deactivateOtherControlsOnPossess", _deactivateOtherControlsOnPossess, SyncDeactivateOtherControlsOnPossess);
		deactivateOtherControlsOnPossessJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(deactivateOtherControlsOnPossessJSON);
		possessableJSON = new JSONStorableBool("possessable", _possessable, SyncPossessable);
		possessableJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(possessableJSON);
		canGrabPositionJSON = new JSONStorableBool("canGrabPosition", _canGrabPosition, SyncCanGrabPosition);
		canGrabPositionJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(canGrabPositionJSON);
		canGrabRotationJSON = new JSONStorableBool("canGrabRotation", _canGrabRotation, SyncCanGrabRotation);
		canGrabRotationJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(canGrabRotationJSON);
		freezeAtomPhysicsWhenGrabbedJSON = new JSONStorableBool("freezeAtomPhysicsWhenGrabbed", freezeAtomPhysicsWhenGrabbed, SyncFreezeAtomPhysicsWhenGrabbed);
		freezeAtomPhysicsWhenGrabbedJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(freezeAtomPhysicsWhenGrabbedJSON);
		string[] names3 = Enum.GetNames(typeof(GridMode));
		List<string> choicesList3 = new List<string>(names3);
		positionGridModeJSON = new JSONStorableStringChooser("positionGridMode", choicesList3, positionGridMode.ToString(), "Position Grid Mode", SetPositionGridModeFromString);
		positionGridModeJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterStringChooser(positionGridModeJSON);
		rotationGridModeJSON = new JSONStorableStringChooser("rotationGridMode", choicesList3, rotationGridMode.ToString(), "Rotation Grid Mode", SetRotationGridModeFromString);
		rotationGridModeJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterStringChooser(rotationGridModeJSON);
		positionGridJSON = new JSONStorableFloat("positionGrid", _positionGrid, SyncPositionGrid, 0.001f, 1f);
		positionGridJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(positionGridJSON);
		rotationGridJSON = new JSONStorableFloat("rotationGrid", _rotationGrid, SyncRotationGrid, 0.01f, 90f);
		rotationGridJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(rotationGridJSON);
		xLockJSON = new JSONStorableBool("xPositionLock", _xLock, SyncXLock);
		xLockJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(xLockJSON);
		yLockJSON = new JSONStorableBool("yPositionLock", _yLock, SyncYLock);
		yLockJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(yLockJSON);
		zLockJSON = new JSONStorableBool("zPositionLock", _zLock, SyncZLock);
		zLockJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(zLockJSON);
		xLocalLockJSON = new JSONStorableBool("xPositionLocalLock", _xLocalLock, SyncXLocalLock);
		xLocalLockJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(xLocalLockJSON);
		yLocalLockJSON = new JSONStorableBool("yPositionLocalLock", _yLocalLock, SyncYLocalLock);
		yLocalLockJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(yLocalLockJSON);
		zLocalLockJSON = new JSONStorableBool("zPositionLocalLock", _zLocalLock, SyncZLocalLock);
		zLocalLockJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(zLocalLockJSON);
		xRotLockJSON = new JSONStorableBool("xRotationLock", _xRotLock, SyncXRotLock);
		xRotLockJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(xRotLockJSON);
		yRotLockJSON = new JSONStorableBool("yRotationLock", _yRotLock, SyncYRotLock);
		yRotLockJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(yRotLockJSON);
		zRotLockJSON = new JSONStorableBool("zRotationLock", _zRotLock, SyncZRotLock);
		zRotLockJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(zRotLockJSON);
		if (controlsCollisionEnabled)
		{
			if (_followWhenOffRB != null)
			{
				_collisionEnabled = _followWhenOffRB.detectCollisions;
			}
			collisionEnabledJSON = new JSONStorableBool("collisionEnabled", _collisionEnabled, SyncCollisionEnabled);
			collisionEnabledJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterBool(collisionEnabledJSON);
		}
		physicsEnabledJSON = new JSONStorableBool("physicsEnabled", physicsEnabled, SyncPhysicsEnabled);
		physicsEnabledJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(physicsEnabledJSON);
		useGravityJSON = new JSONStorableBool("useGravity", useGravityOnRBWhenOff, SyncUseGravityOnRBWhenOff);
		useGravityJSON.altName = "useGravityWhenOff";
		useGravityJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(useGravityJSON);
		RBMassJSON = new JSONStorableFloat("mass", RBMass, SyncRBMass, 0.01f, 10f);
		RBMassJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBMassJSON);
		RBDragJSON = new JSONStorableFloat("drag", RBDrag, SyncRBDrag, 0f, 10f, false);
		RBDragJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBDragJSON);
		RBMaxVelocityEnableJSON = new JSONStorableBool("maxVelocityEnable", _RBMaxVelocityEnable, SyncRBMaxVelocityEnable);
		RBMaxVelocityEnableJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(RBMaxVelocityEnableJSON);
		RBMaxVelocityJSON = new JSONStorableFloat("maxVelocity", _RBMaxVelocity, SyncRBMaxVelocity, 0f, 100f);
		RBMaxVelocityJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBMaxVelocityJSON);
		RBAngularDragJSON = new JSONStorableFloat("angularDrag", RBAngularDrag, SyncRBAngularDrag, 0f, 10f, false);
		RBAngularDragJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBAngularDragJSON);
		RBHoldPositionSpringJSON = new JSONStorableFloat("holdPositionSpring", _RBHoldPositionSpring, SyncRBHoldPositionSpring, 0f, 10000f, false);
		RBHoldPositionSpringJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBHoldPositionSpringJSON);
		RBHoldPositionDamperJSON = new JSONStorableFloat("holdPositionDamper", _RBHoldPositionDamper, SyncRBHoldPositionDamper, 0f, 100f, false);
		RBHoldPositionDamperJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBHoldPositionDamperJSON);
		RBHoldPositionMaxForceJSON = new JSONStorableFloat("holdPositionMaxForce", _RBHoldPositionMaxForce, SyncRBHoldPositionMaxForce, 0f, 10000f, false);
		RBHoldPositionMaxForceJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBHoldPositionMaxForceJSON);
		RBHoldRotationSpringJSON = new JSONStorableFloat("holdRotationSpring", _RBHoldRotationSpring, SyncRBHoldRotationSpring, 0f, 1000f, false);
		RBHoldRotationSpringJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBHoldRotationSpringJSON);
		RBHoldRotationDamperJSON = new JSONStorableFloat("holdRotationDamper", _RBHoldRotationDamper, SyncRBHoldRotationDamper, 0f, 10f, false);
		RBHoldRotationDamperJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBHoldRotationDamperJSON);
		RBHoldRotationMaxForceJSON = new JSONStorableFloat("holdRotationMaxForce", _RBHoldRotationMaxForce, SyncRBHoldRotationMaxForce, 0f, 1000f, false);
		RBHoldRotationMaxForceJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBHoldRotationMaxForceJSON);
		RBComplyPositionSpringJSON = new JSONStorableFloat("complyPositionSpring", _RBComplyPositionSpring, SyncRBComplyPositionSpring, 0f, 10000f, false);
		RBComplyPositionSpringJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBComplyPositionSpringJSON);
		RBComplyPositionDamperJSON = new JSONStorableFloat("complyPositionDamper", _RBComplyPositionDamper, SyncRBComplyPositionDamper, 0f, 1000f, false);
		RBComplyPositionDamperJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBComplyPositionDamperJSON);
		RBComplyRotationSpringJSON = new JSONStorableFloat("complyRotationSpring", _RBComplyRotationSpring, SyncRBComplyRotationSpring, 0f, 1000f, false);
		RBComplyRotationSpringJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBComplyRotationSpringJSON);
		RBComplyRotationDamperJSON = new JSONStorableFloat("complyRotationDamper", _RBComplyRotationDamper, SyncRBComplyRotationDamper, 0f, 100f, false);
		RBComplyRotationDamperJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBComplyRotationDamperJSON);
		RBLinkPositionSpringJSON = new JSONStorableFloat("linkPositionSpring", _RBLinkPositionSpring, SyncRBLinkPositionSpring, 0f, 100000f, false);
		RBLinkPositionSpringJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBLinkPositionSpringJSON);
		RBLinkPositionDamperJSON = new JSONStorableFloat("linkPositionDamper", _RBLinkPositionDamper, SyncRBLinkPositionDamper, 0f, 1000f, false);
		RBLinkPositionDamperJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBLinkPositionDamperJSON);
		RBLinkPositionMaxForceJSON = new JSONStorableFloat("linkPositionMaxForce", _RBLinkPositionMaxForce, SyncRBLinkPositionMaxForce, 0f, 100000f, false);
		RBLinkPositionMaxForceJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBLinkPositionMaxForceJSON);
		RBLinkRotationSpringJSON = new JSONStorableFloat("linkRotationSpring", _RBLinkRotationSpring, SyncRBLinkRotationSpring, 0f, 100000f, false);
		RBLinkRotationSpringJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBLinkRotationSpringJSON);
		RBLinkRotationDamperJSON = new JSONStorableFloat("linkRotationDamper", _RBLinkRotationDamper, SyncRBLinkRotationDamper, 0f, 1000f, false);
		RBLinkRotationDamperJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBLinkRotationDamperJSON);
		RBLinkRotationMaxForceJSON = new JSONStorableFloat("linkRotationMaxForce", _RBLinkRotationMaxForce, SyncRBLinkRotationMaxForce, 0f, 100000f, false);
		RBLinkRotationMaxForceJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBLinkRotationMaxForceJSON);
		RBComplyJointRotationDriveSpringJSON = new JSONStorableFloat("complyJointDriveSpring", _RBComplyJointRotationDriveSpring, SyncRBComplyJointRotationDriveSpring, 0f, 100f, false);
		RBComplyJointRotationDriveSpringJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBComplyJointRotationDriveSpringJSON);
		jointRotationDriveSpringJSON = new JSONStorableFloat("jointDriveSpring", _jointRotationDriveSpring, SyncJointRotationDriveSpring, 0f, 200f, false);
		jointRotationDriveSpringJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(jointRotationDriveSpringJSON);
		jointRotationDriveDamperJSON = new JSONStorableFloat("jointDriveDamper", _jointRotationDriveDamper, SyncJointRotationDriveDamper, 0f, 10f, false);
		jointRotationDriveDamperJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(jointRotationDriveDamperJSON);
		jointRotationDriveMaxForceJSON = new JSONStorableFloat("jointDriveMaxForce", _jointRotationDriveMaxForce, SyncJointRotationDriveMaxForce, 0f, 100f, false);
		jointRotationDriveMaxForceJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(jointRotationDriveMaxForceJSON);
		jointRotationDriveXTargetJSON = new JSONStorableFloat("jointDriveXTarget", _jointRotationDriveXTarget, SyncJointRotationDriveXTarget, _jointRotationDriveXTargetMin, _jointRotationDriveXTargetMax);
		jointRotationDriveXTargetJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(jointRotationDriveXTargetJSON);
		jointRotationDriveYTargetJSON = new JSONStorableFloat("jointDriveYTarget", _jointRotationDriveYTarget, SyncJointRotationDriveYTarget, _jointRotationDriveYTargetMin, _jointRotationDriveYTargetMax);
		jointRotationDriveYTargetJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(jointRotationDriveYTargetJSON);
		jointRotationDriveZTargetJSON = new JSONStorableFloat("jointDriveZTarget", _jointRotationDriveZTarget, SyncJointRotationDriveZTarget, _jointRotationDriveZTargetMin, _jointRotationDriveZTargetMax);
		jointRotationDriveZTargetJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(jointRotationDriveZTargetJSON);
		initialLocalPosition = control.localPosition;
		initialLocalRotation = control.localRotation;
		_currentPositionState = startingPositionState;
		SyncPositionState();
		_currentRotationState = startingRotationState;
		SyncRotationState();
		if (startingLinkToRigidbody != null)
		{
			SelectLinkToRigidbody(startingLinkToRigidbody, SelectLinkState.PositionAndRotation, false, false);
		}
		if (SuperController.singleton != null)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Combine(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomUIDRename));
		}
	}

	private void OnDestroy()
	{
		if (SuperController.singleton != null && wasInit)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Remove(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomUIDRename));
		}
		DestroyAllocatedObjects();
		if (linkLineDrawer != null)
		{
			linkLineDrawer.Destroy();
		}
	}

	private void FixedUpdate()
	{
		if (_followWhenOffRB != null)
		{
			if (_followWhenOffRB.isKinematic)
			{
				UpdateTransform(false);
			}
			if (_RBMaxVelocityEnable)
			{
				if (_RBMaxVelocity == 0f)
				{
					_followWhenOffRB.velocity = Vector3.zero;
				}
				else
				{
					Vector3 velocity = Vector3.ClampMagnitude(_followWhenOffRB.velocity, _RBMaxVelocity);
					if (float.IsNaN(velocity.x) || float.IsNaN(velocity.y) || float.IsNaN(velocity.z))
					{
						_followWhenOffRB.velocity = Vector3.zero;
						if (containingAtom != null && containingAtom != null)
						{
							containingAtom.AlertPhysicsCorruption("FreeController velocity " + base.name);
						}
					}
					else
					{
						_followWhenOffRB.velocity = velocity;
					}
				}
			}
		}
		ApplyComply();
		ApplyForce();
	}

	protected override void Update()
	{
		base.Update();
		UpdateTransform(true);
		if (((bool)_currentPositionMesh || (bool)_currentRotationMesh) && !hidden)
		{
			if (deselectedMesh != null && !_selected && SuperController.singleton != null && SuperController.singleton.centerCameraTarget != null)
			{
				if (drawMeshWhenDeselected)
				{
					Transform transform = SuperController.singleton.centerCameraTarget.transform;
					Vector3 forward = ((!(transform.position == base.transform.position)) ? (transform.position - base.transform.position) : transform.forward);
					Vector3 up = transform.up;
					Quaternion q = Quaternion.LookRotation(forward, up);
					Vector3 s = new Vector3(deselectedMeshScale, deselectedMeshScale, deselectedMeshScale);
					Matrix4x4 identity = Matrix4x4.identity;
					identity.SetTRS(base.transform.position, q, s);
					positionMaterialLocal.SetFloat("_Alpha", targetAlpha);
					positionMaterialLocal.color = _currentPositionColor;
					Graphics.DrawMesh(deselectedMesh, identity, positionMaterialLocal, base.gameObject.layer, null, 0, null, false, false);
				}
			}
			else if (drawMesh)
			{
				Matrix4x4 localToWorldMatrix = control.localToWorldMatrix;
				Vector3 vectorFromAxis = GetVectorFromAxis(MeshForwardAxis);
				Vector3 vectorFromAxis2 = GetVectorFromAxis(MeshUpAxis);
				Quaternion quaternion = Quaternion.LookRotation(vectorFromAxis, vectorFromAxis2);
				Vector3 vectorFromAxis3 = GetVectorFromAxis(DrawForwardAxis);
				Vector3 vectorFromAxis4 = GetVectorFromAxis(DrawUpAxis);
				Quaternion quaternion2 = Quaternion.LookRotation(vectorFromAxis3, vectorFromAxis4);
				Quaternion q2 = quaternion2 * quaternion;
				float num = meshScale;
				num = (_selected ? (num * selectedScale) : ((!_highlighted) ? (num * unhighlightedScale) : (num * highlightedScale)));
				Vector3 s2 = new Vector3(num, num, num);
				Matrix4x4 identity2 = Matrix4x4.identity;
				identity2.SetTRS(Vector3.zero, q2, s2);
				Matrix4x4 matrix = localToWorldMatrix * identity2;
				if ((bool)_currentPositionMesh)
				{
					positionMaterialLocal.SetFloat("_Alpha", targetAlpha);
					positionMaterialLocal.color = _currentPositionColor;
					Graphics.DrawMesh(_currentPositionMesh, matrix, positionMaterialLocal, base.gameObject.layer, null, 0, null, false, false);
				}
				if ((bool)_currentRotationMesh)
				{
					rotationMaterialLocal.SetFloat("_Alpha", targetAlpha);
					rotationMaterialLocal.color = _currentRotationColor;
					Graphics.DrawMesh(_currentRotationMesh, matrix, rotationMaterialLocal, base.gameObject.layer, null, 0, null, false, false);
				}
				if (_selected)
				{
					materialOverlay.SetFloat("_Alpha", targetAlpha);
					materialOverlay.color = overlayColor;
					if (_controlMode == ControlMode.Position)
					{
						if ((bool)moveModeOverlayMesh)
						{
							Graphics.DrawMesh(moveModeOverlayMesh, matrix, materialOverlay, base.gameObject.layer, null, 0, null, false, false);
						}
					}
					else if (_controlMode == ControlMode.Rotation && (bool)rotateModeOverlayMesh)
					{
						Graphics.DrawMesh(rotateModeOverlayMesh, matrix, materialOverlay, base.gameObject.layer, null, 0, null, false, false);
					}
				}
			}
		}
		if (drawSnapshot)
		{
			Matrix4x4 matrix4x = snapshotMatrix;
			Vector3 vectorFromAxis5 = GetVectorFromAxis(MeshForwardAxis);
			Vector3 vectorFromAxis6 = GetVectorFromAxis(MeshUpAxis);
			Quaternion quaternion3 = Quaternion.LookRotation(vectorFromAxis5, vectorFromAxis6);
			Vector3 vectorFromAxis7 = GetVectorFromAxis(DrawForwardAxis);
			Vector3 vectorFromAxis8 = GetVectorFromAxis(DrawUpAxis);
			Quaternion quaternion4 = Quaternion.LookRotation(vectorFromAxis7, vectorFromAxis8);
			Quaternion q3 = quaternion4 * quaternion3;
			float num2 = meshScale * unhighlightedScale;
			Vector3 s3 = new Vector3(num2, num2, num2);
			Matrix4x4 identity3 = Matrix4x4.identity;
			identity3.SetTRS(Vector3.zero, q3, s3);
			Matrix4x4 matrix2 = matrix4x * identity3;
			snapshotMaterialLocal.SetFloat("_Alpha", targetAlpha);
			snapshotMaterialLocal.color = lockColor;
			if ((bool)_currentPositionMesh)
			{
				Graphics.DrawMesh(_currentPositionMesh, matrix2, snapshotMaterialLocal, base.gameObject.layer, null, 0, null, false, false);
			}
			if ((bool)_currentRotationMesh)
			{
				Graphics.DrawMesh(_currentRotationMesh, matrix2, snapshotMaterialLocal, base.gameObject.layer, null, 0, null, false, false);
			}
		}
		if (linkLineDrawer != null && _linkToRB != null && !_hidden)
		{
			ForceReceiver component = _linkToRB.GetComponent<ForceReceiver>();
			if (component == null || !component.skipUIDrawing)
			{
				linkLineMaterialLocal.SetFloat("_Alpha", targetAlpha);
				linkLineDrawer.SetLinePoints(base.transform.position, _linkToRB.transform.position);
				linkLineDrawer.Draw(base.gameObject.layer);
			}
		}
	}
}
