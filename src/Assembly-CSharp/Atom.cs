using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MVR.FileManagement;
using MVR.FileManagementSecure;
using MeshVR;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class Atom : JSONStorable
{
	public string category;

	public string type;

	public bool isSubSceneType;

	protected SubScene _containingSubScene;

	protected JSONStorableAction SelectContainingSubSceneAction;

	[NonSerialized]
	public bool destroyed;

	[NonSerialized]
	public bool loadedFromBundle;

	public bool isPoolable;

	[NonSerialized]
	public bool inPool;

	public Transform[] onToggleObjects;

	[SerializeField]
	protected bool _useRigidbodyInterpolation = true;

	protected List<AsyncFlag> waitResumeSimulationFlags;

	protected bool _resetSimulation;

	protected AsyncFlag resetPhysicsFlag;

	protected AsyncFlag pauseAutoSimulationFlag;

	protected bool _isResettingPhysics;

	protected bool _isResettingPhysicsFull;

	protected Coroutine resetRoutine;

	protected JSONStorableAction ResetPhysicsJSONAction;

	protected JSONStorableFloat resetPhysicsProgressJSON;

	protected bool _insideSyncOnToggleObjects;

	protected JSONStorableAction toggleOnJSON;

	protected JSONStorableBool onJSON;

	[SerializeField]
	protected bool _on = true;

	protected JSONStorableBool hiddenJSON;

	protected bool _hidden;

	protected bool _tempHidden;

	protected bool _tempOff;

	protected bool _keepParamLocksWhenPuttingBackInPool;

	public JSONStorableBool keepParamLocksWhenPuttingBackInPoolJSON;

	protected Dictionary<Rigidbody, RigidbodyConstraints> saveRigidbodyContraints;

	public bool excludeFromTempFreezePhysics;

	protected bool _tempFreezePhysics;

	protected AsyncFlag grabFreezePhysicsFlag;

	protected bool _grabFreezePhysics;

	public bool excludeFromTempDisableCollision;

	protected bool _tempDisableCollision;

	protected bool _resetPhysicsDisableCollision;

	protected float _currentScale = 1f;

	public JSONStorableBool collisionEnabledJSON;

	[SerializeField]
	protected bool _collisionEnabled = true;

	protected bool _isPhysicsFrozen;

	protected bool _freezePhysics;

	public JSONStorableBool freezePhysicsJSON;

	protected JSONStorableAction IsolateEditAtomAction;

	[SerializeField]
	private Atom _parentAtom;

	private HashSet<Atom> childAtoms;

	public UIPopup parentAtomSelectionPopup;

	public Transform reParentObject;

	public bool doNotZeroReParentObject;

	public Transform childAtomContainer;

	protected string _subScenePath = string.Empty;

	public InputField idText;

	public InputFieldAction idTextAction;

	public InputField idTextAlt;

	public InputFieldAction idTextActionAlt;

	[SerializeField]
	private string _uid;

	public Text descriptionText;

	public Text descriptionTextAlt;

	[SerializeField]
	private string _description;

	private Transform[] masterControllerCorners;

	[SerializeField]
	private FreeControllerV3 _masterController;

	public FreeControllerV3 mainController;

	public PresetManagerControl mainPresetControl;

	public float extentPadding = 0.3f;

	public bool alwaysShowExtents = true;

	private float extentLowX;

	private float extentHighX;

	private float extentLowY;

	private float extentHighY;

	private float extentLowZ;

	private float extentHighZ;

	private Vector3 extentlll;

	private Vector3 extentllh;

	private Vector3 extentlhl;

	private Vector3 extentlhh;

	private Vector3 extenthll;

	private Vector3 extenthlh;

	private Vector3 extenthhl;

	private Vector3 extenthhh;

	private bool _wasInit;

	private bool _callbackRegistered;

	private Vector3 reParentObjectStartingPosition;

	private Quaternion reParentObjectStartingRotation;

	private Vector3 childAtomContainerStartingPosition;

	private Quaternion childAtomContainerStartingRotation;

	private List<JSONStorable> _storables;

	private Dictionary<string, JSONStorable> _storableById;

	protected JSONClass lastRestoredData;

	protected bool lastRestorePhysical;

	protected bool lastRestoreAppearance;

	protected bool saveIncludePhysical;

	protected bool saveIncludeAppearance;

	protected string loadedName;

	protected string loadedPhysicalName;

	protected string loadedAppearanceName;

	protected string lastLoadPresetDir;

	protected string lastLoadAppearanceDir;

	protected string lastLoadPhysicalDir;

	private ForceReceiver[] _forceReceivers;

	private ForceProducerV2[] _forceProducers;

	private GrabPoint[] _grabPoints;

	private ScaleChangeReceiver[] _scaleChangeReceivers;

	private ScaleChangeReceiverJSONStorable[] _scaleChangeReceiverJSONStorables;

	private FreeControllerV3[] _freeControllers;

	private Rigidbody[] _rigidbodies;

	private Rigidbody[] _linkableRigidbodies;

	private Dictionary<Rigidbody, bool> _collisionExemptRigidbodies;

	private Rigidbody[] _realRigidbodies;

	private RigidbodyAttributes[] _rigidbodyAttributes;

	private AnimationPattern[] _animationPatterns;

	private AnimationStep[] _animationSteps;

	private Animator[] _animators;

	private MotionAnimationControl[] _motionAnimationsControls;

	private PlayerNavCollider[] _playerNavColliders;

	private List<Canvas> _canvases;

	private PhysicsResetter[] _physicsResetters;

	private PhysicsSimulator[] _physicsSimulators;

	private PhysicsSimulatorJSONStorable[] _physicsSimulatorsStorable;

	private List<PhysicsSimulator> _dynamicPhysicsSimulators;

	private List<PhysicsSimulatorJSONStorable> _dynamicPhysicsSimulatorsStorable;

	private List<ScaleChangeReceiver> _dynamicScaleChangeReceivers;

	private List<ScaleChangeReceiverJSONStorable> _dynamicScaleChangeReceiverJSONStorables;

	public bool excludeFromTempDisableRender;

	protected bool _tempDisableRender;

	public bool excludeFromGlobalDisableRender;

	protected bool _globalDisableRender;

	private Dictionary<ParticleSystemRenderer, ParticleSystemRenderMode> _particleSystemRenderers;

	private List<MeshRenderer> _meshRenderers;

	private List<MeshRenderer> _dynamicMeshRenderers;

	private List<RenderSuspend> _renderSuspends;

	private List<RenderSuspend> _dynamicRenderSuspends;

	private AutoColliderBatchUpdater[] _autoColliderBatchUpdaters;

	private RhythmController[] _rhythmControllers;

	private AudioSourceControl[] _audioSourceControls;

	public bool canBeParented = true;

	public Button selectAtomParentFromSceneButton;

	public SubScene subSceneComponent { get; protected set; }

	public bool isSubSceneRestore { get; set; }

	public SubScene containingSubScene
	{
		get
		{
			return _containingSubScene;
		}
		protected set
		{
			if (_containingSubScene != value)
			{
				_containingSubScene = value;
				SyncSelectContainingSubSceneButton();
			}
		}
	}

	public bool useRigidbodyInterpolation
	{
		get
		{
			return _useRigidbodyInterpolation;
		}
		set
		{
			if (_useRigidbodyInterpolation != value)
			{
				_useRigidbodyInterpolation = value;
				if (Application.isPlaying)
				{
					SyncRigidbodyInterpolation();
				}
			}
		}
	}

	protected bool resetSimulation
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
			}
		}
	}

	public bool on => _on;

	public bool hidden
	{
		get
		{
			if (hiddenJSON != null)
			{
				return hiddenJSON.val;
			}
			return _hidden;
		}
		set
		{
			if (hiddenJSON != null)
			{
				hiddenJSON.val = value;
			}
			else if (_hidden != value)
			{
				_hidden = value;
				SyncHidden(value);
			}
		}
	}

	public bool hiddenNoCallback
	{
		get
		{
			if (hiddenJSON != null)
			{
				return hiddenJSON.val;
			}
			return _hidden;
		}
		set
		{
			if (hiddenJSON != null)
			{
				hiddenJSON.valNoCallback = value;
			}
			else if (_hidden != value)
			{
				_hidden = value;
				SyncHidden(value);
			}
		}
	}

	public bool tempHidden
	{
		get
		{
			return _tempHidden;
		}
		set
		{
			if (_tempHidden != value)
			{
				_tempHidden = value;
			}
		}
	}

	public bool tempOff
	{
		get
		{
			return _tempOff;
		}
		set
		{
			if (_tempOff != value)
			{
				_tempOff = value;
			}
		}
	}

	public bool keepParamLocksWhenPuttingBackInPool => _keepParamLocksWhenPuttingBackInPool;

	public bool tempFreezePhysics
	{
		get
		{
			return _tempFreezePhysics;
		}
		set
		{
			if (_tempFreezePhysics != value)
			{
				_tempFreezePhysics = value;
				SyncFreezePhysics();
			}
		}
	}

	public bool grabFreezePhysics
	{
		get
		{
			return _grabFreezePhysics;
		}
		set
		{
			if (_grabFreezePhysics == value)
			{
				return;
			}
			_grabFreezePhysics = value;
			if (_grabFreezePhysics)
			{
				grabFreezePhysicsFlag = new AsyncFlag("Grab Freeze Physics");
				if (SuperController.singleton != null)
				{
					SuperController.singleton.PauseAutoSimulation(grabFreezePhysicsFlag);
				}
			}
			else
			{
				grabFreezePhysicsFlag.Raise();
				grabFreezePhysicsFlag = null;
			}
			SyncFreezePhysics();
			if (!(subSceneComponent != null))
			{
				return;
			}
			foreach (Atom item in subSceneComponent.atomsInSubScene)
			{
				item.grabFreezePhysics = _grabFreezePhysics;
			}
		}
	}

	public bool tempDisableCollision
	{
		get
		{
			return _tempDisableCollision;
		}
		set
		{
			if (_tempDisableCollision != value)
			{
				_tempDisableCollision = value;
				SyncCollisionEnabled(_collisionEnabled);
			}
		}
	}

	protected bool resetPhysicsDisableCollision
	{
		get
		{
			return _resetPhysicsDisableCollision;
		}
		set
		{
			if (_resetPhysicsDisableCollision != value)
			{
				_resetPhysicsDisableCollision = value;
				SyncCollisionEnabled(_collisionEnabled);
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

	public bool isPhysicsFrozen => _isPhysicsFrozen;

	public Atom parentAtom
	{
		get
		{
			return _parentAtom;
		}
		set
		{
			if (!(_parentAtom != value) || !(_parentAtom != this))
			{
				return;
			}
			if (_parentAtom != null)
			{
				_parentAtom.RemoveChild(this);
			}
			_parentAtom = value;
			if (reParentObject != null)
			{
				if (_parentAtom != null)
				{
					if (_parentAtom.childAtomContainer != null)
					{
						reParentObject.parent = _parentAtom.childAtomContainer;
					}
					else
					{
						reParentObject.parent = _parentAtom.transform;
					}
				}
				else
				{
					reParentObject.parent = base.transform;
				}
			}
			if (_parentAtom != null)
			{
				_parentAtom.AddChild(this);
			}
			if (SuperController.singleton != null)
			{
				SuperController.singleton.AtomParentChanged(this, value);
			}
			SyncOnToggleObjects();
		}
	}

	public string subScenePath
	{
		get
		{
			return _subScenePath;
		}
		protected set
		{
			if (_subScenePath != value)
			{
				_subScenePath = value;
				SetUID(_uid);
			}
		}
	}

	public string uid
	{
		get
		{
			return _uid;
		}
		set
		{
			_uid = value;
			SyncIdText();
		}
	}

	public string uidWithoutSubScenePath => Regex.Replace(_uid, _subScenePath, string.Empty);

	public string description
	{
		get
		{
			return _description;
		}
		set
		{
			_description = value;
			SyncDescriptionText();
		}
	}

	public FreeControllerV3 masterController
	{
		get
		{
			return _masterController;
		}
		set
		{
			if (_masterController != value)
			{
				_masterController = value;
				SyncMasterControllerCorners();
			}
		}
	}

	public bool isPreparingToPutBackInPool { get; protected set; }

	public ForceReceiver[] forceReceivers => _forceReceivers;

	public ForceProducerV2[] forceProducers => _forceProducers;

	public GrabPoint[] grabPoints => _grabPoints;

	public ScaleChangeReceiver[] scaleChangeReceivers => _scaleChangeReceivers;

	public ScaleChangeReceiverJSONStorable[] scaleChangeReceiverJSONStorables => _scaleChangeReceiverJSONStorables;

	public FreeControllerV3[] freeControllers => _freeControllers;

	public Rigidbody[] rigidbodies => _rigidbodies;

	public Rigidbody[] linkableRigidbodies => _linkableRigidbodies;

	public Rigidbody[] realRigidbodies => _realRigidbodies;

	public AnimationPattern[] animationPatterns => _animationPatterns;

	public AnimationStep[] animationSteps => _animationSteps;

	public Animator[] animators => _animators;

	public MotionAnimationControl[] motionAnimationControls => _motionAnimationsControls;

	public PlayerNavCollider[] playerNavColliders => _playerNavColliders;

	public List<Canvas> canvases => _canvases;

	public List<PresetManagerControl> presetManagerControls { get; protected set; }

	public PhysicsSimulator[] physicsSimulators => _physicsSimulators;

	public PhysicsSimulatorJSONStorable[] physicsSimulatorsStorable => _physicsSimulatorsStorable;

	public bool tempDisableRender
	{
		get
		{
			return _tempDisableRender;
		}
		set
		{
			if (_tempDisableRender != value)
			{
				_tempDisableRender = value;
				SyncRenderSuspend();
			}
		}
	}

	public bool globalDisableRender
	{
		get
		{
			return _globalDisableRender;
		}
		set
		{
			if (_globalDisableRender != value)
			{
				_globalDisableRender = value;
				SyncRenderSuspend();
			}
		}
	}

	public AutoColliderBatchUpdater[] autoColliderBatchUpdaters => _autoColliderBatchUpdaters;

	public RhythmController[] rhythmControllers => _rhythmControllers;

	public AudioSourceControl[] audioSourceControls => _audioSourceControls;

	protected void SyncSelectContainingSubSceneButton()
	{
		if (SelectContainingSubSceneAction != null && SelectContainingSubSceneAction.dynamicButton != null)
		{
			SelectContainingSubSceneAction.dynamicButton.gameObject.SetActive(_containingSubScene != null);
		}
	}

	protected void SelectContainingSubScene()
	{
		if (_containingSubScene != null && _containingSubScene.containingAtom != null && _containingSubScene.containingAtom.mainController != null)
		{
			SuperController.singleton.SelectController(_containingSubScene.containingAtom.mainController);
		}
	}

	protected void CheckResumeSimulation()
	{
		if (waitResumeSimulationFlags == null)
		{
			waitResumeSimulationFlags = new List<AsyncFlag>();
		}
		bool flag = false;
		if (waitResumeSimulationFlags.Count > 0)
		{
			List<AsyncFlag> list = new List<AsyncFlag>();
			foreach (AsyncFlag waitResumeSimulationFlag in waitResumeSimulationFlags)
			{
				if (waitResumeSimulationFlag.Raised)
				{
					list.Add(waitResumeSimulationFlag);
					flag = true;
				}
			}
			foreach (AsyncFlag item in list)
			{
				waitResumeSimulationFlags.Remove(item);
			}
		}
		if (waitResumeSimulationFlags.Count > 0)
		{
			resetSimulation = true;
		}
		else if (flag)
		{
			resetSimulation = false;
		}
	}

	public void ResetSimulation(AsyncFlag af)
	{
		if (waitResumeSimulationFlags == null)
		{
			waitResumeSimulationFlags = new List<AsyncFlag>();
		}
		waitResumeSimulationFlags.Add(af);
		resetSimulation = true;
		if (_autoColliderBatchUpdaters != null)
		{
			AutoColliderBatchUpdater[] array = _autoColliderBatchUpdaters;
			foreach (AutoColliderBatchUpdater autoColliderBatchUpdater in array)
			{
				autoColliderBatchUpdater.ResetSimulation(af);
			}
		}
		if (_physicsSimulators != null)
		{
			PhysicsSimulator[] array2 = _physicsSimulators;
			foreach (PhysicsSimulator physicsSimulator in array2)
			{
				if (physicsSimulator.enabled)
				{
					physicsSimulator.ResetSimulation(af);
				}
			}
		}
		if (_physicsSimulatorsStorable != null)
		{
			PhysicsSimulatorJSONStorable[] array3 = _physicsSimulatorsStorable;
			foreach (PhysicsSimulatorJSONStorable physicsSimulatorJSONStorable in array3)
			{
				if (physicsSimulatorJSONStorable.enabled)
				{
					physicsSimulatorJSONStorable.ResetSimulation(af);
				}
			}
		}
		if (_dynamicPhysicsSimulators != null)
		{
			foreach (PhysicsSimulator dynamicPhysicsSimulator in _dynamicPhysicsSimulators)
			{
				dynamicPhysicsSimulator.ResetSimulation(af);
			}
		}
		if (_dynamicPhysicsSimulatorsStorable == null)
		{
			return;
		}
		foreach (PhysicsSimulatorJSONStorable item in _dynamicPhysicsSimulatorsStorable)
		{
			item.ResetSimulation(af);
		}
	}

	protected IEnumerator ResetPhysicsCo(bool full = true, bool fullResetControls = true)
	{
		_isResettingPhysics = true;
		_isResettingPhysicsFull = full;
		resetPhysicsFlag = new AsyncFlag("Atom Reset Physics");
		ResetSimulation(resetPhysicsFlag);
		if (full)
		{
			resetPhysicsProgressJSON.val = 0f;
			float increment = 0.1f;
			resetPhysicsDisableCollision = true;
			ClearPauseAutoSimulationFlag();
			pauseAutoSimulationFlag = new AsyncFlag("Atom Pause Auto Simulation");
			SuperController.singleton.PauseAutoSimulation(pauseAutoSimulationFlag);
			for (int i = 0; i < 5; i++)
			{
				resetPhysicsProgressJSON.val += increment;
				if (fullResetControls)
				{
					FreeControllerV3[] array = containingAtom.freeControllers;
					foreach (FreeControllerV3 freeControllerV in array)
					{
						freeControllerV.SelectLinkToRigidbody(null);
					}
					FreeControllerV3[] array2 = containingAtom.freeControllers;
					foreach (FreeControllerV3 freeControllerV2 in array2)
					{
						freeControllerV2.ResetControl();
					}
				}
				if (_physicsResetters != null)
				{
					PhysicsResetter[] physicsResetters = _physicsResetters;
					foreach (PhysicsResetter physicsResetter in physicsResetters)
					{
						physicsResetter.ResetPhysics();
					}
				}
				yield return null;
			}
			ClearPauseAutoSimulationFlag();
			resetPhysicsDisableCollision = false;
			for (int m = 0; m < 5; m++)
			{
				resetPhysicsProgressJSON.val += increment;
				yield return null;
			}
		}
		else
		{
			for (int n = 0; n < 5; n++)
			{
				yield return null;
			}
		}
		resetPhysicsFlag.Raise();
		resetPhysicsFlag = null;
		resetRoutine = null;
		_isResettingPhysics = false;
		_isResettingPhysicsFull = false;
	}

	protected void ClearPauseAutoSimulationFlag()
	{
		if (pauseAutoSimulationFlag != null)
		{
			pauseAutoSimulationFlag.Raise();
			pauseAutoSimulationFlag = null;
		}
	}

	protected void ClearResetPhysics()
	{
		if (resetRoutine != null)
		{
			StopCoroutine(resetRoutine);
			resetRoutine = null;
		}
		if (resetPhysicsFlag != null)
		{
			resetPhysicsFlag.Raise();
			resetPhysicsFlag = null;
		}
		resetPhysicsDisableCollision = false;
		_isResettingPhysics = false;
		_isResettingPhysicsFull = false;
	}

	public void AlertPhysicsCorruption(string type)
	{
		if (!_isResettingPhysicsFull)
		{
			ClearResetPhysics();
			SuperController.LogError("Detected physics corruption on Atom " + uid + ". Disabling collision on atom and force resetting physics to prevent game crash. Move atom to different location, then reenable collision. Type of corruption: " + type);
			collisionEnabledJSON.val = false;
			ResetPhysics(true, false);
		}
	}

	public void ResetPhysics(bool fullReset = true, bool fullResetControls = true)
	{
		if (resetPhysicsFlag == null)
		{
			resetRoutine = StartCoroutine(ResetPhysicsCo(fullReset, fullResetControls));
		}
	}

	protected void ResetPhysics()
	{
		ResetPhysics(true, true);
	}

	protected void SyncRigidbodyInterpolation()
	{
		if (_realRigidbodies != null)
		{
			Rigidbody[] array = _realRigidbodies;
			foreach (Rigidbody rigidbody in array)
			{
				RigidbodyAttributes component = rigidbody.GetComponent<RigidbodyAttributes>();
				if (component != null)
				{
					component.useInterpolation = _on && !_tempOff && _useRigidbodyInterpolation;
				}
				else if (_on && !_tempOff && _useRigidbodyInterpolation)
				{
					if (!rigidbody.isKinematic)
					{
						rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
					}
				}
				else
				{
					rigidbody.interpolation = RigidbodyInterpolation.None;
				}
			}
		}
		if (_physicsSimulators != null)
		{
			PhysicsSimulator[] array2 = _physicsSimulators;
			foreach (PhysicsSimulator physicsSimulator in array2)
			{
				physicsSimulator.useInterpolation = _useRigidbodyInterpolation;
			}
		}
		if (_physicsSimulatorsStorable != null)
		{
			PhysicsSimulatorJSONStorable[] array3 = _physicsSimulatorsStorable;
			foreach (PhysicsSimulatorJSONStorable physicsSimulatorJSONStorable in array3)
			{
				physicsSimulatorJSONStorable.useInterpolation = _useRigidbodyInterpolation;
			}
		}
		if (_dynamicPhysicsSimulators != null)
		{
			foreach (PhysicsSimulator dynamicPhysicsSimulator in _dynamicPhysicsSimulators)
			{
				dynamicPhysicsSimulator.useInterpolation = _useRigidbodyInterpolation;
			}
		}
		if (_dynamicPhysicsSimulatorsStorable == null)
		{
			return;
		}
		foreach (PhysicsSimulatorJSONStorable item in _dynamicPhysicsSimulatorsStorable)
		{
			item.useInterpolation = _useRigidbodyInterpolation;
		}
	}

	protected bool IsOnInAtomHierarchy(Atom currentAtom)
	{
		if (currentAtom != null)
		{
			if (currentAtom == this)
			{
				return on;
			}
			return IsOnInAtomHierarchy(currentAtom.parentAtom) && currentAtom.on;
		}
		return true;
	}

	protected void SyncOnToggleObjects()
	{
		if (_insideSyncOnToggleObjects)
		{
			return;
		}
		_insideSyncOnToggleObjects = true;
		bool active = _on && !_tempOff && IsOnInAtomHierarchy(parentAtom);
		if (onToggleObjects != null)
		{
			Transform[] array = onToggleObjects;
			foreach (Transform transform in array)
			{
				if (transform != null)
				{
					transform.gameObject.SetActive(active);
				}
			}
		}
		foreach (Atom childAtom in childAtoms)
		{
			childAtom.SyncOnToggleObjects();
		}
		_insideSyncOnToggleObjects = false;
	}

	protected void SyncOn(bool b)
	{
		_on = b;
		SyncOnToggleObjects();
		SyncRigidbodyInterpolation();
	}

	public void SetOn(bool b)
	{
		onJSON.val = b;
	}

	public void ToggleOn()
	{
		if (onJSON != null)
		{
			onJSON.val = !onJSON.val;
		}
	}

	protected void SyncHidden(bool b)
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.SyncHiddenAtoms();
		}
	}

	protected void SyncKeepParamLocksWhenPuttingBackInPool(bool b)
	{
		_keepParamLocksWhenPuttingBackInPool = b;
	}

	public void ScaleChanged(float sc)
	{
		_currentScale = sc;
		if (_scaleChangeReceivers != null)
		{
			ScaleChangeReceiver[] array = _scaleChangeReceivers;
			foreach (ScaleChangeReceiver scaleChangeReceiver in array)
			{
				scaleChangeReceiver.ScaleChanged(sc);
			}
		}
		if (_scaleChangeReceiverJSONStorables != null)
		{
			ScaleChangeReceiverJSONStorable[] array2 = _scaleChangeReceiverJSONStorables;
			foreach (ScaleChangeReceiverJSONStorable scaleChangeReceiverJSONStorable in array2)
			{
				scaleChangeReceiverJSONStorable.ScaleChanged(sc);
			}
		}
		if (_dynamicScaleChangeReceivers != null)
		{
			foreach (ScaleChangeReceiver dynamicScaleChangeReceiver in _dynamicScaleChangeReceivers)
			{
				dynamicScaleChangeReceiver.ScaleChanged(sc);
			}
		}
		if (_dynamicScaleChangeReceiverJSONStorables == null)
		{
			return;
		}
		foreach (ScaleChangeReceiverJSONStorable dynamicScaleChangeReceiverJSONStorable in _dynamicScaleChangeReceiverJSONStorables)
		{
			dynamicScaleChangeReceiverJSONStorable.ScaleChanged(sc);
		}
	}

	protected void SyncCollisionEnabled(bool b)
	{
		_collisionEnabled = b;
		bool flag = _collisionEnabled && (excludeFromTempDisableCollision || !_tempDisableCollision) && !_resetPhysicsDisableCollision;
		if (_freeControllers != null)
		{
			FreeControllerV3[] array = _freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				if (freeControllerV.controlsCollisionEnabled)
				{
					freeControllerV.globalCollisionEnabled = flag;
				}
			}
		}
		if (_realRigidbodies != null)
		{
			Rigidbody[] array2 = _realRigidbodies;
			foreach (Rigidbody rigidbody in array2)
			{
				if (!_collisionExemptRigidbodies.ContainsKey(rigidbody))
				{
					rigidbody.detectCollisions = flag;
				}
			}
		}
		if (_physicsSimulators != null)
		{
			PhysicsSimulator[] array3 = _physicsSimulators;
			foreach (PhysicsSimulator physicsSimulator in array3)
			{
				physicsSimulator.collisionEnabled = flag;
			}
		}
		if (_physicsSimulatorsStorable != null)
		{
			PhysicsSimulatorJSONStorable[] array4 = _physicsSimulatorsStorable;
			foreach (PhysicsSimulatorJSONStorable physicsSimulatorJSONStorable in array4)
			{
				physicsSimulatorJSONStorable.collisionEnabled = flag;
			}
		}
		if (_dynamicPhysicsSimulators != null)
		{
			foreach (PhysicsSimulator dynamicPhysicsSimulator in _dynamicPhysicsSimulators)
			{
				dynamicPhysicsSimulator.collisionEnabled = flag;
			}
		}
		if (_dynamicPhysicsSimulatorsStorable == null)
		{
			return;
		}
		foreach (PhysicsSimulatorJSONStorable item in _dynamicPhysicsSimulatorsStorable)
		{
			item.collisionEnabled = flag;
		}
	}

	public void SetCollisionEnabled(bool b)
	{
		collisionEnabledJSON.val = b;
	}

	protected void SyncFreezePhysics()
	{
		if ((!excludeFromTempFreezePhysics && _tempFreezePhysics) || _freezePhysics || _grabFreezePhysics)
		{
			if (_isPhysicsFrozen)
			{
				return;
			}
			if (saveRigidbodyContraints == null)
			{
				saveRigidbodyContraints = new Dictionary<Rigidbody, RigidbodyConstraints>();
			}
			else
			{
				saveRigidbodyContraints.Clear();
			}
			Rigidbody[] array = _rigidbodies;
			foreach (Rigidbody rigidbody in array)
			{
				saveRigidbodyContraints.Add(rigidbody, rigidbody.constraints);
				rigidbody.constraints = RigidbodyConstraints.FreezeAll;
			}
			_isPhysicsFrozen = true;
			if (_physicsSimulators != null)
			{
				PhysicsSimulator[] array2 = _physicsSimulators;
				foreach (PhysicsSimulator physicsSimulator in array2)
				{
					physicsSimulator.freezeSimulation = true;
				}
			}
			if (_physicsSimulatorsStorable != null)
			{
				PhysicsSimulatorJSONStorable[] array3 = _physicsSimulatorsStorable;
				foreach (PhysicsSimulatorJSONStorable physicsSimulatorJSONStorable in array3)
				{
					physicsSimulatorJSONStorable.freezeSimulation = true;
				}
			}
			if (_dynamicPhysicsSimulators != null)
			{
				foreach (PhysicsSimulator dynamicPhysicsSimulator in _dynamicPhysicsSimulators)
				{
					dynamicPhysicsSimulator.freezeSimulation = true;
				}
			}
			if (_dynamicPhysicsSimulatorsStorable != null)
			{
				foreach (PhysicsSimulatorJSONStorable item in _dynamicPhysicsSimulatorsStorable)
				{
					item.freezeSimulation = true;
				}
			}
			if (subSceneComponent != null && subSceneComponent.motionAnimationMaster != null)
			{
				subSceneComponent.motionAnimationMaster.freeze = true;
			}
		}
		else
		{
			if (!_isPhysicsFrozen)
			{
				return;
			}
			Rigidbody[] array4 = _rigidbodies;
			foreach (Rigidbody rigidbody2 in array4)
			{
				RigidbodyConstraints value;
				if (saveRigidbodyContraints.TryGetValue(rigidbody2, out value))
				{
					rigidbody2.constraints = value;
				}
			}
			if (_physicsSimulators != null)
			{
				PhysicsSimulator[] array5 = _physicsSimulators;
				foreach (PhysicsSimulator physicsSimulator2 in array5)
				{
					physicsSimulator2.freezeSimulation = false;
				}
			}
			if (_physicsSimulatorsStorable != null)
			{
				PhysicsSimulatorJSONStorable[] array6 = _physicsSimulatorsStorable;
				foreach (PhysicsSimulatorJSONStorable physicsSimulatorJSONStorable2 in array6)
				{
					physicsSimulatorJSONStorable2.freezeSimulation = false;
				}
			}
			if (_dynamicPhysicsSimulators != null)
			{
				foreach (PhysicsSimulator dynamicPhysicsSimulator2 in _dynamicPhysicsSimulators)
				{
					dynamicPhysicsSimulator2.freezeSimulation = false;
				}
			}
			if (_dynamicPhysicsSimulatorsStorable != null)
			{
				foreach (PhysicsSimulatorJSONStorable item2 in _dynamicPhysicsSimulatorsStorable)
				{
					item2.freezeSimulation = false;
				}
			}
			RigidbodyAttributes[] rigidbodyAttributes = _rigidbodyAttributes;
			foreach (RigidbodyAttributes rigidbodyAttributes2 in rigidbodyAttributes)
			{
				rigidbodyAttributes2.SyncTensor();
			}
			if (subSceneComponent != null && subSceneComponent.motionAnimationMaster != null)
			{
				subSceneComponent.motionAnimationMaster.freeze = false;
			}
			ResetPhysics(false);
			_isPhysicsFrozen = false;
		}
	}

	protected void SyncFreezePhysics(bool b)
	{
		_freezePhysics = b;
		SyncFreezePhysics();
	}

	public void SetFreezePhysics(bool b)
	{
		freezePhysicsJSON.val = b;
	}

	public void ResetRigidbodies()
	{
		Rigidbody[] array = _rigidbodies;
		foreach (Rigidbody rigidbody in array)
		{
			if (rigidbody.isKinematic)
			{
				rigidbody.isKinematic = false;
				rigidbody.isKinematic = true;
			}
		}
	}

	protected void IsolateEditAtom()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.StartIsolateEditAtom(this);
		}
	}

	protected void ZeroReParentObjectTransform()
	{
		if (!(reParentObject != null) || doNotZeroReParentObject)
		{
			return;
		}
		List<Vector3> list = new List<Vector3>();
		List<Quaternion> list2 = new List<Quaternion>();
		foreach (Transform item in reParentObject)
		{
			list.Add(item.position);
			list2.Add(item.rotation);
		}
		reParentObject.localPosition = Vector3.zero;
		reParentObject.localRotation = Quaternion.identity;
		int num = 0;
		foreach (Transform item2 in reParentObject)
		{
			item2.position = list[num];
			item2.rotation = list2[num];
			num++;
		}
	}

	public HashSet<Atom> GetChildren()
	{
		return childAtoms;
	}

	public void AddChild(Atom a)
	{
		childAtoms.Add(a);
	}

	public void RemoveChild(Atom a)
	{
		childAtoms.Remove(a);
	}

	public void RemoveAllChildAtoms()
	{
		List<Atom> list = new List<Atom>();
		foreach (Atom childAtom in childAtoms)
		{
			list.Add(childAtom);
		}
		foreach (Atom item in list)
		{
			item.SelectAtomParent(null);
		}
	}

	public void RecalculateSubScenePath()
	{
		string text = string.Empty;
		Atom atom = parentAtom;
		while (atom != null)
		{
			if (atom.isSubSceneType)
			{
				text = atom.uidWithoutSubScenePath + "/" + text;
			}
			atom = atom.parentAtom;
		}
		subScenePath = text;
	}

	protected void SyncIdText()
	{
		if (idText != null)
		{
			idText.text = _uid;
		}
		if (idTextAlt != null)
		{
			idTextAlt.text = _uid;
		}
	}

	public void SetUID(string val)
	{
		string text = Regex.Replace(val, ".*/", string.Empty);
		text = _subScenePath + text;
		if (SuperController.singleton != null)
		{
			SuperController.singleton.RenameAtom(this, text);
		}
	}

	protected void SetUIDToInputField()
	{
		if (idText != null)
		{
			SetUID(idText.text);
		}
	}

	protected void SetUIDToInputFieldAlt()
	{
		if (idTextAlt != null)
		{
			SetUID(idTextAlt.text);
		}
	}

	protected void SyncDescriptionText()
	{
		if (descriptionText != null)
		{
			descriptionText.text = _description;
		}
		if (descriptionTextAlt != null)
		{
			descriptionTextAlt.text = _description;
		}
	}

	private void SyncMasterControllerCorners()
	{
		List<Transform> list = new List<Transform>();
		if (_masterController != null)
		{
			foreach (Transform item in _masterController.transform)
			{
				list.Add(item);
			}
		}
		masterControllerCorners = list.ToArray();
	}

	private void walkAndGetComponents(Transform t, List<ForceReceiver> receivers, List<ForceProducerV2> producers, List<GrabPoint> gpoints, List<FreeControllerV3> controllers, List<Rigidbody> rbs, List<Rigidbody> linkablerbs, List<Rigidbody> realrbs, List<RigidbodyAttributes> rbattrs, List<AnimationPattern> ans, List<AnimationStep> asts, List<Animator> anms, List<JSONStorable> jss, List<Canvas> cvs, List<AutoColliderBatchUpdater> acbus, List<PhysicsSimulator> psms, List<PhysicsSimulatorJSONStorable> psmjss, List<MotionAnimationControl> macs, List<PlayerNavCollider> pncs, List<JSONStorableDynamic> jsds, List<RhythmController> rcs, List<AudioSourceControl> ascs, List<ScaleChangeReceiver> scrs, List<ScaleChangeReceiverJSONStorable> scrjss, List<RenderSuspend> rss, List<MeshRenderer> mrs, List<PresetManagerControl> pmcs, Dictionary<ParticleSystemRenderer, ParticleSystemRenderMode> psrm, List<PhysicsResetter> prs, bool insidePhysicsSimulator)
	{
		Rigidbody component = t.GetComponent<Rigidbody>();
		if (component != null)
		{
			rbs.Add(component);
		}
		RigidbodyAttributes component2 = t.GetComponent<RigidbodyAttributes>();
		if (component2 != null)
		{
			rbattrs.Add(component2);
		}
		ForceReceiver component3 = t.GetComponent<ForceReceiver>();
		if (component3 != null)
		{
			component3.containingAtom = this;
			receivers.Add(component3);
		}
		ForceProducerV2 component4 = t.GetComponent<ForceProducerV2>();
		if (component4 != null)
		{
			component4.containingAtom = this;
			producers.Add(component4);
		}
		GrabPoint component5 = t.GetComponent<GrabPoint>();
		if (component5 != null)
		{
			component5.containingAtom = this;
			gpoints.Add(component5);
		}
		FreeControllerV3 component6 = t.GetComponent<FreeControllerV3>();
		if (component6 != null)
		{
			component6.containingAtom = this;
			controllers.Add(component6);
			if (component6.controlsCollisionEnabled && component6.followWhenOffRB != null)
			{
				_collisionExemptRigidbodies.Add(component6.followWhenOffRB, true);
			}
		}
		MotionAnimationControl component7 = t.GetComponent<MotionAnimationControl>();
		if (component7 != null)
		{
			component7.containingAtom = this;
			macs.Add(component7);
		}
		PhysicsSimulator component8 = t.GetComponent<PhysicsSimulator>();
		if (component8 != null)
		{
			insidePhysicsSimulator = true;
			psms.Add(component8);
		}
		PhysicsResetter component9 = t.GetComponent<PhysicsResetter>();
		if (component9 != null)
		{
			prs.Add(component9);
		}
		PhysicsSimulatorJSONStorable component10 = t.GetComponent<PhysicsSimulatorJSONStorable>();
		if (component10 != null)
		{
			insidePhysicsSimulator = true;
			psmjss.Add(component10);
		}
		if (component != null && component6 == null && !insidePhysicsSimulator)
		{
			realrbs.Add(component);
		}
		if (component != null && (component3 != null || component6 != null))
		{
			linkablerbs.Add(component);
		}
		AnimationPattern component11 = t.GetComponent<AnimationPattern>();
		if (component11 != null)
		{
			component11.containingAtom = this;
			ans.Add(component11);
		}
		AnimationStep component12 = t.GetComponent<AnimationStep>();
		if (component12 != null)
		{
			component12.containingAtom = this;
			asts.Add(component12);
		}
		Animator component13 = t.GetComponent<Animator>();
		if (component13 != null)
		{
			anms.Add(component13);
		}
		Canvas component14 = t.GetComponent<Canvas>();
		if (component14 != null)
		{
			cvs.Add(component14);
		}
		JSONStorable[] components = t.GetComponents<JSONStorable>();
		if (components != null)
		{
			JSONStorable[] array = components;
			foreach (JSONStorable jSONStorable in array)
			{
				jSONStorable.containingAtom = this;
				jss.Add(jSONStorable);
			}
		}
		JSONStorableDynamic[] components2 = t.GetComponents<JSONStorableDynamic>();
		if (components2 != null)
		{
			JSONStorableDynamic[] array2 = components2;
			foreach (JSONStorableDynamic jSONStorableDynamic in array2)
			{
				jSONStorableDynamic.containingAtom = this;
				jsds.Add(jSONStorableDynamic);
			}
		}
		PlayerNavCollider component15 = t.GetComponent<PlayerNavCollider>();
		if (component15 != null)
		{
			component15.containingAtom = this;
			pncs.Add(component15);
		}
		AutoColliderBatchUpdater component16 = t.GetComponent<AutoColliderBatchUpdater>();
		if (component16 != null)
		{
			acbus.Add(component16);
		}
		RhythmController component17 = t.GetComponent<RhythmController>();
		if (component17 != null)
		{
			component17.containingAtom = this;
			rcs.Add(component17);
		}
		AudioSourceControl component18 = t.GetComponent<AudioSourceControl>();
		if (component18 != null)
		{
			component18.containingAtom = this;
			ascs.Add(component18);
		}
		UIConnectorMaster component19 = t.GetComponent<UIConnectorMaster>();
		if (component19 != null)
		{
			component19.containingAtom = this;
		}
		SetTransformScale component20 = t.GetComponent<SetTransformScale>();
		if (component20 != null)
		{
			component20.containingAtom = this;
		}
		ScaleChangeReceiver[] components3 = t.GetComponents<ScaleChangeReceiver>();
		ScaleChangeReceiver[] array3 = components3;
		foreach (ScaleChangeReceiver scaleChangeReceiver in array3)
		{
			if (scaleChangeReceiver != null)
			{
				scrs.Add(scaleChangeReceiver);
			}
		}
		ScaleChangeReceiverJSONStorable[] components4 = t.GetComponents<ScaleChangeReceiverJSONStorable>();
		ScaleChangeReceiverJSONStorable[] array4 = components4;
		foreach (ScaleChangeReceiverJSONStorable scaleChangeReceiverJSONStorable in array4)
		{
			if (scaleChangeReceiverJSONStorable != null)
			{
				scrjss.Add(scaleChangeReceiverJSONStorable);
			}
		}
		RenderSuspend[] components5 = t.GetComponents<RenderSuspend>();
		RenderSuspend[] array5 = components5;
		foreach (RenderSuspend renderSuspend in array5)
		{
			if (renderSuspend != null)
			{
				rss.Add(renderSuspend);
			}
		}
		MeshRenderer[] components6 = t.GetComponents<MeshRenderer>();
		MeshRenderer[] array6 = components6;
		foreach (MeshRenderer meshRenderer in array6)
		{
			if (meshRenderer != null && meshRenderer.enabled)
			{
				mrs.Add(meshRenderer);
			}
		}
		ParticleSystemRenderer[] components7 = t.GetComponents<ParticleSystemRenderer>();
		ParticleSystemRenderer[] array7 = components7;
		foreach (ParticleSystemRenderer particleSystemRenderer in array7)
		{
			psrm.Add(particleSystemRenderer, particleSystemRenderer.renderMode);
		}
		PresetManagerControl component21 = t.GetComponent<PresetManagerControl>();
		if (component21 != null)
		{
			pmcs.Add(component21);
		}
		foreach (Transform item in t)
		{
			if (!item.GetComponent<Atom>())
			{
				walkAndGetComponents(item, receivers, producers, gpoints, controllers, rbs, linkablerbs, realrbs, rbattrs, ans, asts, anms, jss, cvs, acbus, psms, psmjss, macs, pncs, jsds, rcs, ascs, scrs, scrjss, rss, mrs, pmcs, psrm, prs, insidePhysicsSimulator);
			}
		}
	}

	private void Init()
	{
		if (_wasInit)
		{
			Debug.LogError("Init was already called");
		}
		_wasInit = true;
		onJSON = new JSONStorableBool("on", _on, SyncOn);
		RegisterBool(onJSON);
		onJSON.isStorable = false;
		onJSON.isRestorable = false;
		toggleOnJSON = new JSONStorableAction("ToggleOn", ToggleOn);
		RegisterAction(toggleOnJSON);
		hiddenJSON = new JSONStorableBool("hidden", _hidden, SyncHidden);
		hiddenJSON.storeType = JSONStorableParam.StoreType.Full;
		RegisterBool(hiddenJSON);
		keepParamLocksWhenPuttingBackInPoolJSON = new JSONStorableBool("keepParamLocksWhenPuttingBackInPool", _keepParamLocksWhenPuttingBackInPool, SyncKeepParamLocksWhenPuttingBackInPool);
		keepParamLocksWhenPuttingBackInPoolJSON.isStorable = false;
		keepParamLocksWhenPuttingBackInPoolJSON.isRestorable = false;
		RegisterBool(keepParamLocksWhenPuttingBackInPoolJSON);
		collisionEnabledJSON = new JSONStorableBool("collisionEnabled", _collisionEnabled, SyncCollisionEnabled);
		RegisterBool(collisionEnabledJSON);
		collisionEnabledJSON.isStorable = false;
		collisionEnabledJSON.isRestorable = false;
		freezePhysicsJSON = new JSONStorableBool("freezePhysics", _freezePhysics, SyncFreezePhysics);
		RegisterBool(freezePhysicsJSON);
		freezePhysicsJSON.isStorable = false;
		freezePhysicsJSON.isRestorable = false;
		IsolateEditAtomAction = new JSONStorableAction("IsolateEditAtom", IsolateEditAtom);
		RegisterAction(IsolateEditAtomAction);
		SelectContainingSubSceneAction = new JSONStorableAction("SelectContainingSubScene", SelectContainingSubScene);
		RegisterAction(SelectContainingSubSceneAction);
		ResetPhysicsJSONAction = new JSONStorableAction("ResetPhysics", ResetPhysics);
		RegisterAction(ResetPhysicsJSONAction);
		resetPhysicsProgressJSON = new JSONStorableFloat("resetPhysicsProgress", 0f, 0f, 1f);
		if (isSubSceneType)
		{
			subSceneComponent = GetComponentInChildren<SubScene>(true);
		}
		_collisionExemptRigidbodies = new Dictionary<Rigidbody, bool>();
		childAtoms = new HashSet<Atom>();
		List<ForceReceiver> list = new List<ForceReceiver>();
		List<ForceProducerV2> list2 = new List<ForceProducerV2>();
		List<GrabPoint> list3 = new List<GrabPoint>();
		List<FreeControllerV3> list4 = new List<FreeControllerV3>();
		List<Rigidbody> list5 = new List<Rigidbody>();
		List<Rigidbody> list6 = new List<Rigidbody>();
		List<Rigidbody> list7 = new List<Rigidbody>();
		List<RigidbodyAttributes> list8 = new List<RigidbodyAttributes>();
		List<AnimationPattern> list9 = new List<AnimationPattern>();
		List<AnimationStep> list10 = new List<AnimationStep>();
		List<Animator> list11 = new List<Animator>();
		List<Canvas> cvs = new List<Canvas>();
		List<JSONStorable> list12 = new List<JSONStorable>();
		List<AutoColliderBatchUpdater> list13 = new List<AutoColliderBatchUpdater>();
		List<PhysicsResetter> list14 = new List<PhysicsResetter>();
		List<PhysicsSimulator> list15 = new List<PhysicsSimulator>();
		List<PhysicsSimulatorJSONStorable> list16 = new List<PhysicsSimulatorJSONStorable>();
		List<MotionAnimationControl> list17 = new List<MotionAnimationControl>();
		List<PlayerNavCollider> list18 = new List<PlayerNavCollider>();
		List<JSONStorableDynamic> jsds = new List<JSONStorableDynamic>();
		List<RhythmController> list19 = new List<RhythmController>();
		List<AudioSourceControl> list20 = new List<AudioSourceControl>();
		List<ScaleChangeReceiver> list21 = new List<ScaleChangeReceiver>();
		List<ScaleChangeReceiverJSONStorable> list22 = new List<ScaleChangeReceiverJSONStorable>();
		List<RenderSuspend> list23 = new List<RenderSuspend>();
		List<MeshRenderer> list24 = new List<MeshRenderer>();
		Dictionary<ParticleSystemRenderer, ParticleSystemRenderMode> dictionary = new Dictionary<ParticleSystemRenderer, ParticleSystemRenderMode>();
		List<PresetManagerControl> pmcs = new List<PresetManagerControl>();
		walkAndGetComponents(base.transform, list, list2, list3, list4, list5, list6, list7, list8, list9, list10, list11, list12, cvs, list13, list15, list16, list17, list18, jsds, list19, list20, list21, list22, list23, list24, pmcs, dictionary, list14, false);
		_forceReceivers = list.ToArray();
		_forceProducers = list2.ToArray();
		_rhythmControllers = list19.ToArray();
		_audioSourceControls = list20.ToArray();
		_grabPoints = list3.ToArray();
		_freeControllers = list4.ToArray();
		_rigidbodies = list5.ToArray();
		_linkableRigidbodies = list6.ToArray();
		_realRigidbodies = list7.ToArray();
		_rigidbodyAttributes = list8.ToArray();
		_animationPatterns = list9.ToArray();
		_animationSteps = list10.ToArray();
		_animators = list11.ToArray();
		_motionAnimationsControls = list17.ToArray();
		_playerNavColliders = list18.ToArray();
		_canvases = cvs;
		_storables = list12;
		_physicsResetters = list14.ToArray();
		_physicsSimulators = list15.ToArray();
		_physicsSimulatorsStorable = list16.ToArray();
		_autoColliderBatchUpdaters = list13.ToArray();
		_scaleChangeReceivers = list21.ToArray();
		_scaleChangeReceiverJSONStorables = list22.ToArray();
		_renderSuspends = list23;
		_meshRenderers = list24;
		_particleSystemRenderers = dictionary;
		presetManagerControls = pmcs;
		_storableById = new Dictionary<string, JSONStorable>();
		foreach (JSONStorable storable in _storables)
		{
			if (!storable.exclude)
			{
				if (_storableById.ContainsKey(storable.storeId))
				{
					Debug.LogError("Found duplicate storable uid " + storable.storeId + " in atom " + uid + " of type " + type);
				}
				else
				{
					_storableById.Add(storable.storeId, storable);
				}
			}
		}
		SyncOnToggleObjects();
		SyncRigidbodyInterpolation();
		SyncCollisionEnabled(_collisionEnabled);
		if (SuperController.singleton != null)
		{
			_callbackRegistered = true;
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Combine(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomRenamed));
			SuperController singleton2 = SuperController.singleton;
			singleton2.onAtomSubSceneChangedHandlers = (SuperController.OnAtomSubSceneChanged)Delegate.Combine(singleton2.onAtomSubSceneChangedHandlers, new SuperController.OnAtomSubSceneChanged(OnAtomSubSceneChanged));
		}
	}

	public List<string> GetStorableIDs()
	{
		List<string> list = new List<string>();
		foreach (string key in _storableById.Keys)
		{
			JSONStorable value;
			if (_storableById.TryGetValue(key, out value) && !value.exclude && (!value.onlyStoreIfActive || value.gameObject.activeInHierarchy))
			{
				list.Add(key);
			}
		}
		list.Sort();
		return list;
	}

	public JSONStorable GetStorableByID(string storeid)
	{
		JSONStorable value = null;
		_storableById.TryGetValue(storeid, out value);
		return value;
	}

	public void Store(JSONArray atoms, bool includePhysical = true, bool includeAppearance = true)
	{
		JSONClass jSONClass = new JSONClass();
		jSONClass["id"] = uid;
		if (includePhysical)
		{
			jSONClass["on"].AsBool = _on;
		}
		if (includePhysical && collisionEnabledJSON.val != collisionEnabledJSON.defaultVal)
		{
			jSONClass["collisionEnabled"].AsBool = _collisionEnabled;
		}
		if (includePhysical && freezePhysicsJSON.val != freezePhysicsJSON.defaultVal)
		{
			jSONClass["freezePhysics"].AsBool = _freezePhysics;
		}
		if (type != null)
		{
			jSONClass["type"] = type;
		}
		else
		{
			Debug.LogWarning("Atom " + uid + " does not have a type set");
		}
		if (parentAtom != null)
		{
			jSONClass["parentAtom"] = parentAtom.uid;
		}
		if (reParentObject != null && includePhysical)
		{
			Vector3 position = reParentObject.position;
			jSONClass["position"]["x"].AsFloat = position.x;
			jSONClass["position"]["y"].AsFloat = position.y;
			jSONClass["position"]["z"].AsFloat = position.z;
			Vector3 eulerAngles = reParentObject.eulerAngles;
			jSONClass["rotation"]["x"].AsFloat = eulerAngles.x;
			jSONClass["rotation"]["y"].AsFloat = eulerAngles.y;
			jSONClass["rotation"]["z"].AsFloat = eulerAngles.z;
		}
		if (childAtomContainer != null && includePhysical)
		{
			Vector3 position2 = childAtomContainer.position;
			jSONClass["containerPosition"]["x"].AsFloat = position2.x;
			jSONClass["containerPosition"]["y"].AsFloat = position2.y;
			jSONClass["containerPosition"]["z"].AsFloat = position2.z;
			Vector3 eulerAngles2 = childAtomContainer.eulerAngles;
			jSONClass["containerRotation"]["x"].AsFloat = eulerAngles2.x;
			jSONClass["containerRotation"]["y"].AsFloat = eulerAngles2.y;
			jSONClass["containerRotation"]["z"].AsFloat = eulerAngles2.z;
		}
		atoms.Add(jSONClass);
		JSONArray jSONArray = (JSONArray)(jSONClass["storables"] = new JSONArray());
		foreach (JSONStorable storable in _storables)
		{
			if (!(storable != null) || storable.exclude || (storable.onlyStoreIfActive && !storable.gameObject.activeInHierarchy))
			{
				continue;
			}
			try
			{
				JSONClass jSON = storable.GetJSON(includePhysical, includeAppearance);
				if (storable.needsStore)
				{
					jSONArray.Add(jSON);
				}
			}
			catch (Exception ex)
			{
				SuperController.LogError("Exception during Store of " + storable.storeId + ": " + ex);
			}
		}
	}

	public void StoreForSubScene(JSONClass jc, bool isTheSubSceneAtom = false)
	{
		if (!isTheSubSceneAtom)
		{
			jc["id"] = uidWithoutSubScenePath;
			jc["type"] = type;
			jc["on"].AsBool = _on;
			jc["collisionEnabled"].AsBool = _collisionEnabled;
			if (parentAtom != null && !parentAtom.isSubSceneType)
			{
				jc["parentAtom"] = parentAtom.uidWithoutSubScenePath;
			}
			if (reParentObject != null)
			{
				Vector3 localPosition = reParentObject.localPosition;
				jc["localPosition"]["x"].AsFloat = localPosition.x;
				jc["localPosition"]["y"].AsFloat = localPosition.y;
				jc["localPosition"]["z"].AsFloat = localPosition.z;
				Vector3 localEulerAngles = reParentObject.localEulerAngles;
				jc["localRotation"]["x"].AsFloat = localEulerAngles.x;
				jc["localRotation"]["y"].AsFloat = localEulerAngles.y;
				jc["localRotation"]["z"].AsFloat = localEulerAngles.z;
			}
			if (childAtomContainer != null)
			{
				Vector3 localPosition2 = childAtomContainer.localPosition;
				jc["containerLocalPosition"]["x"].AsFloat = localPosition2.x;
				jc["containerLocalPosition"]["y"].AsFloat = localPosition2.y;
				jc["containerLocalPosition"]["z"].AsFloat = localPosition2.z;
				Vector3 localEulerAngles2 = childAtomContainer.localEulerAngles;
				jc["containerLocalRotation"]["x"].AsFloat = localEulerAngles2.x;
				jc["containerLocalRotation"]["y"].AsFloat = localEulerAngles2.y;
				jc["containerLocalRotation"]["z"].AsFloat = localEulerAngles2.z;
			}
		}
		JSONArray jSONArray = (JSONArray)(jc["storables"] = new JSONArray());
		foreach (JSONStorable storable in _storables)
		{
			if (!(storable != null) || storable.exclude || (isTheSubSceneAtom && (storable.gameObject == mainController.gameObject || storable.gameObject == childAtomContainer.gameObject || storable is Atom || storable is SubScene)) || (storable.onlyStoreIfActive && !storable.gameObject.activeInHierarchy))
			{
				continue;
			}
			try
			{
				if (isTheSubSceneAtom)
				{
					storable.subScenePrefix = uid + "/";
				}
				else
				{
					storable.subScenePrefix = subScenePath;
				}
				JSONClass jSON = storable.GetJSON(true, true, true);
				storable.subScenePrefix = null;
				jSONArray.Add(jSON);
			}
			catch (Exception ex)
			{
				SuperController.LogError("Exception during SubScene Store of " + storable.storeId + ": " + ex);
			}
		}
	}

	public void RestoreForceInitialize()
	{
		onJSON.val = true;
	}

	public void RestoreStartingOn()
	{
		onJSON.val = onJSON.defaultVal;
	}

	public void RestoreTransform(JSONClass jc, bool setUnlistedToDefault = true)
	{
		if (reParentObject != null)
		{
			if (jc["position"] != null)
			{
				Vector3 position = reParentObject.position;
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
				reParentObject.position = position;
			}
			else if (jc["localPosition"] != null)
			{
				Vector3 localPosition = reParentObject.localPosition;
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
				reParentObject.localPosition = localPosition;
			}
			else if (setUnlistedToDefault)
			{
				reParentObject.position = reParentObjectStartingPosition;
			}
			if (jc["rotation"] != null)
			{
				Vector3 eulerAngles = reParentObject.eulerAngles;
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
				reParentObject.eulerAngles = eulerAngles;
			}
			else if (jc["localRotation"] != null)
			{
				Vector3 localEulerAngles = reParentObject.localEulerAngles;
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
				reParentObject.localEulerAngles = localEulerAngles;
			}
			else if (setUnlistedToDefault)
			{
				reParentObject.rotation = reParentObjectStartingRotation;
			}
		}
		if (!(childAtomContainer != null))
		{
			return;
		}
		if (jc["containerPosition"] != null)
		{
			Vector3 position2 = childAtomContainer.position;
			if (jc["containerPosition"]["x"] != null)
			{
				position2.x = jc["containerPosition"]["x"].AsFloat;
			}
			if (jc["containerPosition"]["y"] != null)
			{
				position2.y = jc["containerPosition"]["y"].AsFloat;
			}
			if (jc["containerPosition"]["z"] != null)
			{
				position2.z = jc["containerPosition"]["z"].AsFloat;
			}
			childAtomContainer.position = position2;
		}
		else if (jc["containerLocalPosition"] != null)
		{
			Vector3 localPosition2 = childAtomContainer.localPosition;
			if (jc["containerLocalPosition"]["x"] != null)
			{
				localPosition2.x = jc["containerLocalPosition"]["x"].AsFloat;
			}
			if (jc["containerLocalPosition"]["y"] != null)
			{
				localPosition2.y = jc["containerLocalPosition"]["y"].AsFloat;
			}
			if (jc["containerLocalPosition"]["z"] != null)
			{
				localPosition2.z = jc["containerLocalPosition"]["z"].AsFloat;
			}
			childAtomContainer.localPosition = localPosition2;
		}
		else if (setUnlistedToDefault)
		{
			childAtomContainer.position = childAtomContainerStartingPosition;
		}
		if (jc["containerRotation"] != null)
		{
			Vector3 eulerAngles2 = childAtomContainer.eulerAngles;
			if (jc["containerRotation"]["x"] != null)
			{
				eulerAngles2.x = jc["containerRotation"]["x"].AsFloat;
			}
			if (jc["containerRotation"]["y"] != null)
			{
				eulerAngles2.y = jc["containerRotation"]["y"].AsFloat;
			}
			if (jc["containerRotation"]["z"] != null)
			{
				eulerAngles2.z = jc["containerRotation"]["z"].AsFloat;
			}
			childAtomContainer.eulerAngles = eulerAngles2;
		}
		else if (jc["containerLocalRotation"] != null)
		{
			Vector3 localEulerAngles2 = childAtomContainer.localEulerAngles;
			if (jc["containerLocalRotation"]["x"] != null)
			{
				localEulerAngles2.x = jc["containerLocalRotation"]["x"].AsFloat;
			}
			if (jc["containerLocalRotation"]["y"] != null)
			{
				localEulerAngles2.y = jc["containerLocalRotation"]["y"].AsFloat;
			}
			if (jc["containerLocalRotation"]["z"] != null)
			{
				localEulerAngles2.z = jc["containerLocalRotation"]["z"].AsFloat;
			}
			childAtomContainer.localEulerAngles = localEulerAngles2;
		}
		else if (setUnlistedToDefault)
		{
			childAtomContainer.rotation = childAtomContainerStartingRotation;
		}
	}

	public void ClearParentAtom()
	{
		SelectAtomParent(null);
	}

	public void RestoreParentAtom(JSONClass jc)
	{
		if (jc["parentAtom"] != null)
		{
			Atom atomByUid = SuperController.singleton.GetAtomByUid(jc["parentAtom"]);
			SelectAtomParent(atomByUid);
		}
		else
		{
			SelectAtomParent(null);
		}
	}

	public void SetLastRestoredData(JSONClass jc, bool isAppearance = true, bool isPhysical = true)
	{
		lastRestoredData = jc;
		lastRestorePhysical = isPhysical;
		lastRestoreAppearance = isAppearance;
	}

	public void RestoreFromLast(JSONStorable js)
	{
		if (!(lastRestoredData != null))
		{
			return;
		}
		bool flag = false;
		foreach (JSONClass item in lastRestoredData["storables"].AsArray)
		{
			string text = item["id"];
			if (text == js.storeId)
			{
				flag = true;
				try
				{
					js.RestoreFromJSON(item, lastRestorePhysical, lastRestoreAppearance);
					js.LateRestoreFromJSON(item, lastRestorePhysical, lastRestoreAppearance);
				}
				catch (Exception ex)
				{
					SuperController.LogError("Exception during RestoreFromLast of " + js.storeId + ": " + ex);
				}
				break;
			}
		}
		if (!flag)
		{
			JSONClass jc = new JSONClass();
			try
			{
				js.RestoreFromJSON(jc, lastRestorePhysical, lastRestoreAppearance);
			}
			catch (Exception ex2)
			{
				SuperController.LogError("Exception during RestoreFromLast of " + js.storeId + ": " + ex2);
			}
		}
	}

	public void Restore(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool restoreCore = true, JSONArray presetAtoms = null, bool isClear = false, bool isSubSceneRestore = false, bool setMissingToDefault = true, bool isTheSubSceneAtom = false)
	{
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		lastRestoredData = jc;
		lastRestoreAppearance = restoreAppearance;
		lastRestorePhysical = restorePhysical;
		if (restoreCore)
		{
			if (jc["on"] != null)
			{
				onJSON.val = jc["on"].AsBool;
			}
			else if (setMissingToDefault)
			{
				onJSON.val = onJSON.defaultVal;
			}
		}
		foreach (JSONClass item in jc["storables"].AsArray)
		{
			JSONStorable value;
			if (!_storableById.TryGetValue(item["id"], out value) || !(value != null) || value.exclude || (isTheSubSceneAtom && (value.gameObject == mainController.gameObject || value.gameObject == childAtomContainer.gameObject || value is Atom || value is SubScene)))
			{
				continue;
			}
			if (isSubSceneRestore)
			{
				if (isTheSubSceneAtom)
				{
					value.subScenePrefix = uid + "/";
				}
				else
				{
					value.subScenePrefix = subScenePath;
				}
			}
			try
			{
				value.RestoreFromJSON(item, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
			}
			catch (Exception ex)
			{
				SuperController.LogError("Exception during Restore of " + value.storeId + ": " + ex);
			}
			if (isSubSceneRestore)
			{
				value.subScenePrefix = null;
			}
			if (!dictionary.ContainsKey(item["id"]))
			{
				dictionary.Add(item["id"], true);
			}
		}
		if (!setMissingToDefault)
		{
			return;
		}
		JSONStorable[] array = _storables.ToArray();
		JSONStorable[] array2 = array;
		foreach (JSONStorable jSONStorable in array2)
		{
			if (jSONStorable != null && !jSONStorable.exclude && (!isTheSubSceneAtom || (!(jSONStorable == null) && !(jSONStorable.gameObject == mainController.gameObject) && !(jSONStorable.gameObject == childAtomContainer.gameObject) && !(jSONStorable is Atom) && !(jSONStorable is SubScene))) && !dictionary.ContainsKey(jSONStorable.storeId) && (isClear || !jSONStorable.onlyStoreIfActive || jSONStorable.gameObject.activeInHierarchy))
			{
				try
				{
					JSONClass jc2 = new JSONClass();
					jSONStorable.RestoreFromJSON(jc2, restorePhysical, restoreAppearance);
				}
				catch (Exception ex2)
				{
					SuperController.LogError("Exception during Restore of " + jSONStorable.storeId + ": " + ex2);
				}
			}
		}
	}

	public void LateRestore(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool restoreCore = true, bool isSubSceneRestore = false, bool setMissingToDefault = true, bool isTheSubSceneAtom = false)
	{
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		if (restoreCore)
		{
			if (jc["collisionEnabled"] != null)
			{
				collisionEnabledJSON.val = jc["collisionEnabled"].AsBool;
			}
			else if (setMissingToDefault)
			{
				collisionEnabledJSON.val = collisionEnabledJSON.defaultVal;
			}
			if (jc["freezePhysics"] != null)
			{
				freezePhysicsJSON.val = jc["freezePhysics"].AsBool;
			}
			else if (setMissingToDefault)
			{
				freezePhysicsJSON.val = freezePhysicsJSON.defaultVal;
			}
		}
		foreach (JSONClass item in jc["storables"].AsArray)
		{
			JSONStorable value;
			if (!_storableById.TryGetValue(item["id"], out value) || !(value != null) || value.exclude || (isTheSubSceneAtom && (value == null || value.gameObject == mainController.gameObject || value.gameObject == childAtomContainer.gameObject || value is Atom || value is SubScene)))
			{
				continue;
			}
			if (isSubSceneRestore)
			{
				if (isTheSubSceneAtom)
				{
					value.subScenePrefix = uid + "/";
				}
				else
				{
					value.subScenePrefix = subScenePath;
				}
			}
			try
			{
				value.LateRestoreFromJSON(item, restorePhysical, restoreAppearance, setMissingToDefault);
			}
			catch (Exception ex)
			{
				SuperController.LogError("Exception during LateRestore of " + value.storeId + ": " + ex);
			}
			if (isSubSceneRestore)
			{
				value.subScenePrefix = null;
			}
			if (!dictionary.ContainsKey(item["id"]))
			{
				dictionary.Add(item["id"], true);
			}
		}
		if (!setMissingToDefault)
		{
			return;
		}
		JSONStorable[] array = _storables.ToArray();
		JSONStorable[] array2 = array;
		foreach (JSONStorable jSONStorable in array2)
		{
			if (jSONStorable != null && !jSONStorable.exclude && (!isTheSubSceneAtom || (!(jSONStorable.gameObject == mainController.gameObject) && !(jSONStorable.gameObject == childAtomContainer.gameObject) && !(jSONStorable is Atom) && !(jSONStorable is SubScene))) && !dictionary.ContainsKey(jSONStorable.storeId))
			{
				try
				{
					JSONClass jc2 = new JSONClass();
					jSONStorable.LateRestoreFromJSON(jc2, restorePhysical, restoreAppearance);
				}
				catch (Exception ex2)
				{
					SuperController.LogError("Exception during LateRestore of " + jSONStorable.storeId + ": " + ex2);
				}
			}
		}
	}

	public new void Validate()
	{
		foreach (JSONStorable storable in _storables)
		{
			storable.Validate();
		}
	}

	public void PreRestoreForSubScene()
	{
		JSONStorable[] array = _storables.ToArray();
		JSONStorable[] array2 = array;
		foreach (JSONStorable jSONStorable in array2)
		{
			if (jSONStorable != null && !jSONStorable.exclude && !(jSONStorable.gameObject == mainController.gameObject) && !(jSONStorable.gameObject == childAtomContainer.gameObject) && !(jSONStorable is Atom) && !(jSONStorable is SubScene))
			{
				try
				{
					jSONStorable.PreRestore();
					jSONStorable.PreRestore(true, true);
				}
				catch (Exception ex)
				{
					SuperController.LogError("Exception during PreRestore of " + jSONStorable.storeId + ": " + ex);
				}
			}
		}
	}

	public new void PreRestore()
	{
		PreRestore(true, true);
	}

	public new void PreRestore(bool restorePhysical, bool restoreAppearance)
	{
		JSONStorable[] array = _storables.ToArray();
		JSONStorable[] array2 = array;
		foreach (JSONStorable jSONStorable in array2)
		{
			if (jSONStorable != null && !jSONStorable.exclude)
			{
				try
				{
					jSONStorable.PreRestore();
					jSONStorable.PreRestore(restorePhysical, restoreAppearance);
				}
				catch (Exception ex)
				{
					SuperController.LogError("Exception during PreRestore of " + jSONStorable.storeId + ": " + ex);
				}
			}
		}
	}

	public new void PostRestore()
	{
		PostRestore(true, true);
	}

	public new void PostRestore(bool restorePhysical, bool restoreAppearance)
	{
		JSONStorable[] array = _storables.ToArray();
		JSONStorable[] array2 = array;
		foreach (JSONStorable jSONStorable in array2)
		{
			if (jSONStorable != null && !jSONStorable.exclude)
			{
				try
				{
					jSONStorable.PostRestore();
					jSONStorable.PostRestore(restorePhysical, restoreAppearance);
				}
				catch (Exception ex)
				{
					SuperController.LogError("Exception during PostRestore of " + jSONStorable.storeId + ": " + ex);
				}
			}
		}
	}

	public void OnPreRemove()
	{
		JSONStorable[] array = _storables.ToArray();
		JSONStorable[] array2 = array;
		foreach (JSONStorable jSONStorable in array2)
		{
			try
			{
				jSONStorable.PreRemove();
			}
			catch (Exception ex)
			{
				SuperController.LogError("Exception during PreRemove of " + jSONStorable.storeId + ": " + ex);
			}
		}
	}

	public new void Remove()
	{
		SuperController.singleton.RemoveAtom(this);
	}

	public void OnRemove()
	{
		JSONStorable[] array = _storables.ToArray();
		JSONStorable[] array2 = array;
		foreach (JSONStorable jSONStorable in array2)
		{
			if (jSONStorable != null)
			{
				try
				{
					jSONStorable.Remove();
				}
				catch (Exception ex)
				{
					SuperController.LogError("Exception during Remove of " + jSONStorable.storeId + ": " + ex);
				}
			}
		}
	}

	public void PrepareToPutBackInPool()
	{
		if (!isPoolable)
		{
			return;
		}
		if (!_keepParamLocksWhenPuttingBackInPool)
		{
			foreach (PresetManagerControl presetManagerControl in presetManagerControls)
			{
				presetManagerControl.lockParams = false;
			}
		}
		isPreparingToPutBackInPool = true;
		Reset(false);
		isPreparingToPutBackInPool = false;
	}

	public void Reset(bool resetSimulation = true)
	{
		JSONClass jc = new JSONClass();
		loadedName = null;
		loadedPhysicalName = null;
		loadedAppearanceName = null;
		PreRestore(true, true);
		RestoreTransform(jc);
		RestoreParentAtom(jc);
		Restore(jc, true, true, true, null, true);
		Restore(jc);
		LateRestore(jc);
		PostRestore(true, true);
		if (resetSimulation)
		{
			ResetPhysics(false);
		}
	}

	public void ResetPhysical()
	{
		JSONClass jc = new JSONClass();
		loadedName = null;
		loadedPhysicalName = null;
		loadedAppearanceName = null;
		PreRestore(true, false);
		RestoreTransform(jc);
		RestoreParentAtom(jc);
		Restore(jc, true, false, false, null, true);
		Restore(jc, true, false, false);
		LateRestore(jc, true, false, false);
		PostRestore(true, false);
		ResetPhysics(false);
	}

	public void ResetAppearance()
	{
		JSONClass jc = new JSONClass();
		loadedName = null;
		loadedPhysicalName = null;
		loadedAppearanceName = null;
		PreRestore(false, true);
		Restore(jc, false, true, false, null, true);
		Restore(jc, false, true, false);
		LateRestore(jc, false, true, false);
		PostRestore(false, true);
	}

	public void SavePresetDialog(bool includePhysical = false, bool includeAppearance = false)
	{
		if (!(SuperController.singleton != null) || !(SuperController.singleton.fileBrowserUI != null))
		{
			return;
		}
		saveIncludePhysical = includePhysical;
		saveIncludeAppearance = includeAppearance;
		string text = SuperController.singleton.savesDir + type;
		if (saveIncludePhysical && saveIncludeAppearance)
		{
			text += "\\full";
			if (lastLoadPresetDir != string.Empty && FileManager.DirectoryExists(lastLoadPresetDir))
			{
				string suggestedBrowserDirectoryFromDirectoryPath = FileManager.GetSuggestedBrowserDirectoryFromDirectoryPath(text, lastLoadPresetDir, false);
				if (suggestedBrowserDirectoryFromDirectoryPath != null && FileManager.DirectoryExists(suggestedBrowserDirectoryFromDirectoryPath))
				{
					text = suggestedBrowserDirectoryFromDirectoryPath;
				}
			}
			else if (!FileManager.DirectoryExists(text))
			{
				FileManager.CreateDirectory(text);
			}
		}
		else if (saveIncludePhysical)
		{
			text += "\\pose";
			if (lastLoadPhysicalDir != string.Empty && FileManager.DirectoryExists(lastLoadPhysicalDir))
			{
				string suggestedBrowserDirectoryFromDirectoryPath2 = FileManager.GetSuggestedBrowserDirectoryFromDirectoryPath(text, lastLoadPhysicalDir, false);
				if (suggestedBrowserDirectoryFromDirectoryPath2 != null && FileManager.DirectoryExists(suggestedBrowserDirectoryFromDirectoryPath2))
				{
					text = suggestedBrowserDirectoryFromDirectoryPath2;
				}
			}
			else if (!FileManager.DirectoryExists(text))
			{
				FileManager.CreateDirectory(text);
			}
		}
		else if (saveIncludeAppearance)
		{
			text += "\\appearance";
			if (lastLoadAppearanceDir != string.Empty && FileManager.DirectoryExists(lastLoadAppearanceDir))
			{
				string suggestedBrowserDirectoryFromDirectoryPath3 = FileManager.GetSuggestedBrowserDirectoryFromDirectoryPath(text, lastLoadAppearanceDir, false);
				if (suggestedBrowserDirectoryFromDirectoryPath3 != null && FileManager.DirectoryExists(suggestedBrowserDirectoryFromDirectoryPath3))
				{
					text = suggestedBrowserDirectoryFromDirectoryPath3;
				}
			}
			else if (!FileManager.DirectoryExists(text))
			{
				FileManager.CreateDirectory(text);
			}
		}
		SuperController.singleton.fileBrowserUI.SetShortCuts(null);
		SuperController.singleton.fileBrowserUI.keepOpen = false;
		SuperController.singleton.fileBrowserUI.SetTitle("Select Save Preset File");
		SuperController.singleton.fileBrowserUI.defaultPath = text;
		SuperController.singleton.fileBrowserUI.SetTextEntry(true);
		SuperController.singleton.fileBrowserUI.Show(SavePreset);
		if (SuperController.singleton.fileBrowserUI.fileEntryField != null)
		{
			string text2 = ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds/*cast due to .constrained prefix*/).ToString();
			SuperController.singleton.fileBrowserUI.fileEntryField.text = text2;
			SuperController.singleton.fileBrowserUI.ActivateFileNameField();
		}
	}

	public void SavePreset(string saveName)
	{
		if (saveName != string.Empty)
		{
			if (saveIncludePhysical && saveIncludeAppearance)
			{
				loadedName = saveName;
				lastLoadPresetDir = FileManager.GetDirectoryName(saveName, true);
			}
			else if (saveIncludePhysical)
			{
				loadedPhysicalName = saveName;
				lastLoadPhysicalDir = FileManager.GetDirectoryName(saveName, true);
			}
			else if (saveIncludeAppearance)
			{
				loadedAppearanceName = saveName;
				lastLoadAppearanceDir = FileManager.GetDirectoryName(saveName, true);
			}
			SuperController.singleton.SaveFromAtom(saveName, this, saveIncludePhysical, saveIncludeAppearance);
		}
	}

	public void LoadPresetDialog()
	{
		string text = SuperController.singleton.savesDir + type + "\\full";
		string text2 = text;
		if (lastLoadPresetDir != string.Empty && FileManager.DirectoryExists(lastLoadPresetDir))
		{
			string suggestedBrowserDirectoryFromDirectoryPath = FileManager.GetSuggestedBrowserDirectoryFromDirectoryPath(text2, lastLoadPresetDir);
			if (suggestedBrowserDirectoryFromDirectoryPath != null && FileManager.DirectoryExists(suggestedBrowserDirectoryFromDirectoryPath))
			{
				text2 = suggestedBrowserDirectoryFromDirectoryPath;
			}
		}
		else if (!FileManager.DirectoryExists(text2))
		{
			FileManager.CreateDirectory(text2);
		}
		List<ShortCut> shortCutsForDirectory = FileManager.GetShortCutsForDirectory(text, true, false, true, true);
		SuperController.singleton.fileBrowserUI.SetShortCuts(shortCutsForDirectory);
		SuperController.singleton.fileBrowserUI.keepOpen = false;
		SuperController.singleton.fileBrowserUI.defaultPath = text2;
		SuperController.singleton.fileBrowserUI.SetTitle("Select Preset File");
		SuperController.singleton.fileBrowserUI.SetTextEntry(false);
		SuperController.singleton.fileBrowserUI.Show(LoadPreset);
	}

	public void LoadPreset(string saveName = "savefile")
	{
		if (!(saveName != string.Empty))
		{
			return;
		}
		try
		{
			using (FileEntryStreamReader fileEntryStreamReader = FileManager.OpenStreamReader(saveName, true))
			{
				lastLoadPresetDir = FileManager.GetDirectoryName(saveName);
				FileManager.PushLoadDirFromFilePath(saveName);
				string aJSON = fileEntryStreamReader.ReadToEnd();
				JSONNode jSONNode = JSON.Parse(aJSON);
				JSONArray asArray = jSONNode["atoms"].AsArray;
				loadedName = saveName;
				JSONClass asObject = asArray[0].AsObject;
				if (!(asObject != null))
				{
					return;
				}
				string text = asObject["type"];
				if (!(text == type))
				{
					return;
				}
				PreRestore(true, true);
				RestoreTransform(asObject);
				Restore(asObject, true, true, false, asArray);
				LateRestore(asObject, true, true, false);
				PostRestore(true, true);
				if (SuperController.singleton != null)
				{
					if (asObject["id"] != null)
					{
						SuperController.singleton.RenameAtom(this, asObject["id"]);
					}
					ResetPhysics(false);
				}
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Exception during LoadPreset " + ex);
		}
	}

	public void LoadPhysicalPresetDialog()
	{
		string text = SuperController.singleton.savesDir + type + "\\pose";
		string text2 = text;
		if (lastLoadPhysicalDir != string.Empty && FileManager.DirectoryExists(lastLoadPhysicalDir))
		{
			string suggestedBrowserDirectoryFromDirectoryPath = FileManager.GetSuggestedBrowserDirectoryFromDirectoryPath(text2, lastLoadPhysicalDir);
			if (suggestedBrowserDirectoryFromDirectoryPath != null && FileManager.DirectoryExists(suggestedBrowserDirectoryFromDirectoryPath))
			{
				text2 = suggestedBrowserDirectoryFromDirectoryPath;
			}
		}
		else if (!FileManager.DirectoryExists(text2))
		{
			FileManager.CreateDirectory(text2);
		}
		List<ShortCut> shortCutsForDirectory = FileManager.GetShortCutsForDirectory(text, true, false, true, true);
		SuperController.singleton.fileBrowserUI.SetShortCuts(shortCutsForDirectory);
		SuperController.singleton.fileBrowserUI.keepOpen = false;
		SuperController.singleton.fileBrowserUI.defaultPath = text2;
		SuperController.singleton.fileBrowserUI.SetTitle("Select Preset File");
		SuperController.singleton.fileBrowserUI.SetTextEntry(false);
		SuperController.singleton.fileBrowserUI.Show(LoadPhysicalPreset);
	}

	public void LoadPhysicalPreset(string saveName = "savefile")
	{
		if (!(saveName != string.Empty))
		{
			return;
		}
		try
		{
			using (FileEntryStreamReader fileEntryStreamReader = FileManager.OpenStreamReader(saveName, true))
			{
				lastLoadPhysicalDir = FileManager.GetDirectoryName(saveName, true);
				FileManager.PushLoadDirFromFilePath(saveName);
				string aJSON = fileEntryStreamReader.ReadToEnd();
				JSONNode jSONNode = JSON.Parse(aJSON);
				JSONArray asArray = jSONNode["atoms"].AsArray;
				loadedPhysicalName = saveName;
				JSONClass asObject = asArray[0].AsObject;
				if (asObject != null)
				{
					string text = asObject["type"];
					if (text == type)
					{
						PreRestore(true, false);
						RestoreTransform(asObject);
						Restore(asObject, true, false, false, asArray);
						LateRestore(asObject, true, false, false);
						PostRestore(true, false);
						ResetPhysics(false);
					}
				}
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Error during LoadPhysicalPreset " + ex);
		}
	}

	public void LoadAppearancePresetDialog()
	{
		string text = SuperController.singleton.savesDir + type + "\\appearance";
		string text2 = text;
		if (lastLoadAppearanceDir != string.Empty && FileManager.DirectoryExists(lastLoadAppearanceDir))
		{
			string suggestedBrowserDirectoryFromDirectoryPath = FileManager.GetSuggestedBrowserDirectoryFromDirectoryPath(text2, lastLoadAppearanceDir);
			if (suggestedBrowserDirectoryFromDirectoryPath != null && FileManager.DirectoryExists(suggestedBrowserDirectoryFromDirectoryPath))
			{
				text2 = suggestedBrowserDirectoryFromDirectoryPath;
			}
		}
		else if (!FileManager.DirectoryExists(text2))
		{
			FileManager.CreateDirectory(text2);
		}
		List<ShortCut> shortCutsForDirectory = FileManager.GetShortCutsForDirectory(text, true, false, true, true);
		SuperController.singleton.fileBrowserUI.SetShortCuts(shortCutsForDirectory);
		SuperController.singleton.fileBrowserUI.keepOpen = false;
		SuperController.singleton.fileBrowserUI.defaultPath = text2;
		SuperController.singleton.fileBrowserUI.SetTitle("Select Preset File");
		SuperController.singleton.fileBrowserUI.SetTextEntry(false);
		SuperController.singleton.fileBrowserUI.Show(LoadAppearancePreset);
	}

	public void LoadAppearancePreset(string saveName = "savefile")
	{
		if (!(saveName != string.Empty))
		{
			return;
		}
		try
		{
			using (FileEntryStreamReader fileEntryStreamReader = FileManager.OpenStreamReader(saveName, true))
			{
				lastLoadAppearanceDir = FileManager.GetDirectoryName(saveName, true);
				FileManager.PushLoadDirFromFilePath(saveName);
				string aJSON = fileEntryStreamReader.ReadToEnd();
				JSONNode jSONNode = JSON.Parse(aJSON);
				JSONArray asArray = jSONNode["atoms"].AsArray;
				loadedAppearanceName = saveName;
				JSONClass asObject = asArray[0].AsObject;
				if (!(asObject != null))
				{
					return;
				}
				string text = asObject["type"];
				if (text == type)
				{
					PreRestore(false, true);
					Restore(asObject, false, true, false);
					LateRestore(asObject, false, true, false);
					PostRestore(false, true);
					if (SuperController.singleton != null && asObject["id"] != null)
					{
						SuperController.singleton.RenameAtom(this, asObject["id"]);
					}
				}
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Exception during LoadAppearancePreset " + ex);
		}
	}

	public void RegisterDynamicPhysicsSimulator(PhysicsSimulator ps)
	{
		if (_dynamicPhysicsSimulators == null)
		{
			_dynamicPhysicsSimulators = new List<PhysicsSimulator>();
		}
		ps.useInterpolation = _useRigidbodyInterpolation;
		ps.collisionEnabled = _collisionEnabled;
		ps.freezeSimulation = _isPhysicsFrozen;
		if (waitResumeSimulationFlags != null)
		{
			foreach (AsyncFlag waitResumeSimulationFlag in waitResumeSimulationFlags)
			{
				ps.ResetSimulation(waitResumeSimulationFlag);
			}
		}
		_dynamicPhysicsSimulators.Add(ps);
	}

	public void DeregisterDynamicPhysicsSimulator(PhysicsSimulator ps)
	{
		_dynamicPhysicsSimulators.Remove(ps);
	}

	public void RegisterDynamicPhysicsSimulatorJSONStorable(PhysicsSimulatorJSONStorable ps)
	{
		if (_dynamicPhysicsSimulatorsStorable == null)
		{
			_dynamicPhysicsSimulatorsStorable = new List<PhysicsSimulatorJSONStorable>();
		}
		ps.useInterpolation = _useRigidbodyInterpolation;
		ps.collisionEnabled = _collisionEnabled;
		ps.freezeSimulation = _isPhysicsFrozen;
		if (waitResumeSimulationFlags != null)
		{
			foreach (AsyncFlag waitResumeSimulationFlag in waitResumeSimulationFlags)
			{
				ps.ResetSimulation(waitResumeSimulationFlag);
			}
		}
		_dynamicPhysicsSimulatorsStorable.Add(ps);
	}

	public void DeregisterDynamicPhysicsSimulatorJSONStorable(PhysicsSimulatorJSONStorable ps)
	{
		_dynamicPhysicsSimulatorsStorable.Remove(ps);
	}

	public void RegisterDynamicScaleChangeReceiver(ScaleChangeReceiver scr)
	{
		if (_dynamicScaleChangeReceivers == null)
		{
			_dynamicScaleChangeReceivers = new List<ScaleChangeReceiver>();
		}
		scr.ScaleChanged(_currentScale);
		_dynamicScaleChangeReceivers.Add(scr);
	}

	public void DeregisterDynamicScaleChangeReceiver(ScaleChangeReceiver scr)
	{
		_dynamicScaleChangeReceivers.Remove(scr);
	}

	public void RegisterDynamicScaleChangeReceiverJSONStorable(ScaleChangeReceiverJSONStorable scr)
	{
		if (_dynamicScaleChangeReceiverJSONStorables == null)
		{
			_dynamicScaleChangeReceiverJSONStorables = new List<ScaleChangeReceiverJSONStorable>();
		}
		scr.ScaleChanged(_currentScale);
		_dynamicScaleChangeReceiverJSONStorables.Add(scr);
	}

	public void DeregisterDynamicScaleChangeReceiverJSONStorable(ScaleChangeReceiverJSONStorable scr)
	{
		_dynamicScaleChangeReceiverJSONStorables.Remove(scr);
	}

	protected void SyncRenderSuspend()
	{
		bool flag = (!excludeFromTempDisableRender && _tempDisableRender) || (!excludeFromGlobalDisableRender && _globalDisableRender);
		if (_renderSuspends != null)
		{
			foreach (RenderSuspend renderSuspend in _renderSuspends)
			{
				renderSuspend.renderSuspend = flag;
			}
		}
		if (_dynamicRenderSuspends != null)
		{
			foreach (RenderSuspend dynamicRenderSuspend in _dynamicRenderSuspends)
			{
				dynamicRenderSuspend.renderSuspend = flag;
			}
		}
		if (_meshRenderers != null)
		{
			foreach (MeshRenderer meshRenderer in _meshRenderers)
			{
				meshRenderer.enabled = !flag;
			}
		}
		if (_dynamicMeshRenderers != null)
		{
			foreach (MeshRenderer dynamicMeshRenderer in _dynamicMeshRenderers)
			{
				dynamicMeshRenderer.enabled = !flag;
			}
		}
		if (_particleSystemRenderers == null)
		{
			return;
		}
		foreach (KeyValuePair<ParticleSystemRenderer, ParticleSystemRenderMode> particleSystemRenderer in _particleSystemRenderers)
		{
			if (flag)
			{
				particleSystemRenderer.Key.renderMode = ParticleSystemRenderMode.None;
			}
			else
			{
				particleSystemRenderer.Key.renderMode = particleSystemRenderer.Value;
			}
		}
	}

	public void RegisterDynamicMeshRenderer(MeshRenderer mr)
	{
		if (mr.enabled)
		{
			if (_dynamicMeshRenderers == null)
			{
				_dynamicMeshRenderers = new List<MeshRenderer>();
			}
			_dynamicMeshRenderers.Add(mr);
			SyncRenderSuspend();
		}
	}

	public void DeregisterDynamicMeshRenderer(MeshRenderer mr)
	{
		_dynamicMeshRenderers.Remove(mr);
	}

	public void RegisterDynamicRenderSuspend(RenderSuspend rs)
	{
		if (_dynamicRenderSuspends == null)
		{
			_dynamicRenderSuspends = new List<RenderSuspend>();
		}
		_dynamicRenderSuspends.Add(rs);
		SyncRenderSuspend();
	}

	public void DeregisterDynamicRenderSuspend(RenderSuspend rs)
	{
		rs.renderSuspend = false;
		_dynamicRenderSuspends.Remove(rs);
	}

	public void SetParentAtomSelectPopupValues()
	{
		if (!(parentAtomSelectionPopup != null) || !(SuperController.singleton != null))
		{
			return;
		}
		List<string> visibleAtomUIDs = SuperController.singleton.GetVisibleAtomUIDs();
		if (visibleAtomUIDs == null)
		{
			parentAtomSelectionPopup.numPopupValues = 1;
			parentAtomSelectionPopup.setPopupValue(0, "None");
			return;
		}
		parentAtomSelectionPopup.numPopupValues = visibleAtomUIDs.Count + 1;
		parentAtomSelectionPopup.setPopupValue(0, "None");
		for (int i = 0; i < visibleAtomUIDs.Count; i++)
		{
			parentAtomSelectionPopup.setPopupValue(i + 1, visibleAtomUIDs[i]);
		}
	}

	public bool RegisterAdditionalStorable(JSONStorable js)
	{
		if (js != null)
		{
			if (!_storableById.ContainsKey(js.storeId))
			{
				js.containingAtom = this;
				_storables.Add(js);
				_storableById.Add(js.storeId, js);
				return true;
			}
			Debug.LogError("Found duplicate storable uid " + js.storeId + " in atom " + uid);
		}
		return false;
	}

	public void AddCanvas(Canvas c)
	{
		_canvases.Add(c);
		if (SuperController.singleton != null)
		{
			SuperController.singleton.AddCanvas(c);
		}
	}

	public void RemoveCanvas(Canvas c)
	{
		_canvases.Remove(c);
		if (SuperController.singleton != null)
		{
			SuperController.singleton.RemoveCanvas(c);
		}
	}

	public void UnregisterAdditionalStorable(JSONStorable js)
	{
		if (js != null)
		{
			if (_storableById.ContainsKey(js.storeId))
			{
				_storableById.Remove(js.storeId);
			}
			js.containingAtom = null;
			_storables.Remove(js);
		}
	}

	public void SetParentAtom(string atomUID)
	{
		if (SuperController.singleton != null)
		{
			Atom atomByUid = SuperController.singleton.GetAtomByUid(atomUID);
			SetParentAtom(atomByUid);
		}
	}

	public void SetParentAtom(Atom a)
	{
		if (a == this)
		{
			SelectAtomParent(parentAtom);
		}
		else
		{
			parentAtom = a;
		}
	}

	protected void WalkAndRecalculateSubScenePath(Atom atom)
	{
		atom.RecalculateSubScenePath();
		foreach (Atom child in atom.GetChildren())
		{
			WalkAndRecalculateSubScenePath(child);
		}
	}

	protected void OnAtomRenamed(string oldid, string newid)
	{
		if (parentAtom != null && parentAtomSelectionPopup != null)
		{
			parentAtomSelectionPopup.currentValueNoCallback = parentAtom.uid;
		}
		if (newid == uid && isSubSceneType)
		{
			WalkAndRecalculateSubScenePath(this);
		}
	}

	protected void OnAtomSubSceneChanged(Atom atom, SubScene subScene)
	{
		if (atom == this)
		{
			containingSubScene = subScene;
			RecalculateSubScenePath();
		}
	}

	public void SelectAtomParent(Atom a)
	{
		if (a == this)
		{
			a = parentAtom;
		}
		if (parentAtomSelectionPopup != null)
		{
			if (a == null)
			{
				parentAtomSelectionPopup.currentValue = "None";
			}
			else
			{
				parentAtomSelectionPopup.currentValue = a.uid;
			}
		}
		parentAtom = a;
	}

	public void SelectAtomParentFromScene()
	{
		SetParentAtomSelectPopupValues();
		SuperController.singleton.SelectModeAtom(SelectAtomParent);
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		AtomUI componentInChildren = UITransform.GetComponentInChildren<AtomUI>(true);
		if (!(componentInChildren != null))
		{
			return;
		}
		onJSON.toggle = componentInChildren.onToggle;
		hiddenJSON.toggle = componentInChildren.hiddenToggle;
		keepParamLocksWhenPuttingBackInPoolJSON.toggle = componentInChildren.keepParamLocksWhenPuttingBackInPoolToggle;
		collisionEnabledJSON.toggle = componentInChildren.collisionEnabledToggle;
		freezePhysicsJSON.toggle = componentInChildren.freezePhysicsToggle;
		ResetPhysicsJSONAction.button = componentInChildren.resetPhysicsButton;
		resetPhysicsProgressJSON.slider = componentInChildren.resetPhysicsProgressSlider;
		Canvas componentInChildren2 = UITransform.GetComponentInChildren<Canvas>();
		if (componentInChildren2 != null)
		{
			RectTransform component = componentInChildren2.GetComponent<RectTransform>();
			if (component != null)
			{
				UIDynamicButton uIDynamicButton = SuperController.singleton.CreateDynamicButton(component);
				if (uIDynamicButton != null)
				{
					RectTransform component2 = uIDynamicButton.GetComponent<RectTransform>();
					component2.anchorMin = new Vector2(0.5f, 1f);
					component2.anchorMax = new Vector2(1f, 1f);
					component2.anchoredPosition = new Vector2(5f, 45f);
					component2.sizeDelta = new Vector2(-10f, 70f);
					uIDynamicButton.label = "Select Containing SubScene";
					SelectContainingSubSceneAction.dynamicButton = uIDynamicButton;
					SyncSelectContainingSubSceneButton();
				}
				if (!isSubSceneType)
				{
					uIDynamicButton = SuperController.singleton.CreateDynamicButton(component);
					if (uIDynamicButton != null)
					{
						RectTransform component3 = uIDynamicButton.GetComponent<RectTransform>();
						component3.anchorMin = new Vector2(0f, 1f);
						component3.anchorMax = new Vector2(0.5f, 1f);
						component3.anchoredPosition = new Vector2(-5f, 45f);
						component3.sizeDelta = new Vector2(-10f, 70f);
						uIDynamicButton.label = "Isolate Edit This Atom";
						IsolateEditAtomAction.dynamicButton = uIDynamicButton;
					}
				}
			}
		}
		parentAtomSelectionPopup = componentInChildren.parentAtomSelectionPopup;
		if (parentAtomSelectionPopup != null)
		{
			parentAtomSelectionPopup.numPopupValues = 1;
			parentAtomSelectionPopup.setPopupValue(0, "None");
			if (parentAtom != null)
			{
				parentAtomSelectionPopup.currentValue = parentAtom.uid;
			}
			else
			{
				parentAtomSelectionPopup.currentValue = "None";
			}
			UIPopup uIPopup = parentAtomSelectionPopup;
			uIPopup.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Combine(uIPopup.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetParentAtomSelectPopupValues));
			UIPopup uIPopup2 = parentAtomSelectionPopup;
			uIPopup2.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup2.onValueChangeHandlers, new UIPopup.OnValueChange(SetParentAtom));
		}
		if (componentInChildren.selectAtomParentFromSceneButton != null)
		{
			componentInChildren.selectAtomParentFromSceneButton.onClick.AddListener(SelectAtomParentFromScene);
		}
		if (componentInChildren.resetButton != null)
		{
			componentInChildren.resetButton.onClick.AddListener(delegate
			{
				Reset();
			});
		}
		if (componentInChildren.resetPhysicalButton != null)
		{
			componentInChildren.resetPhysicalButton.onClick.AddListener(delegate
			{
				ResetPhysical();
			});
		}
		if (componentInChildren.resetAppearanceButton != null)
		{
			componentInChildren.resetAppearanceButton.onClick.AddListener(delegate
			{
				ResetAppearance();
			});
		}
		if (componentInChildren.removeButton != null)
		{
			componentInChildren.removeButton.onClick.AddListener(delegate
			{
				Remove();
			});
		}
		if (componentInChildren.savePresetButton != null)
		{
			componentInChildren.savePresetButton.onClick.AddListener(delegate
			{
				SavePresetDialog(true, true);
			});
		}
		if (componentInChildren.saveAppearancePresetButton != null)
		{
			componentInChildren.saveAppearancePresetButton.onClick.AddListener(delegate
			{
				SavePresetDialog(false, true);
			});
		}
		if (componentInChildren.savePhysicalPresetButton != null)
		{
			componentInChildren.savePhysicalPresetButton.onClick.AddListener(delegate
			{
				SavePresetDialog(true);
			});
		}
		if (componentInChildren.loadPresetButton != null)
		{
			componentInChildren.loadPresetButton.onClick.AddListener(delegate
			{
				LoadPresetDialog();
			});
		}
		if (componentInChildren.loadAppearancePresetButton != null)
		{
			componentInChildren.loadAppearancePresetButton.onClick.AddListener(delegate
			{
				LoadAppearancePresetDialog();
			});
		}
		if (componentInChildren.loadPhysicalPresetButton != null)
		{
			componentInChildren.loadPhysicalPresetButton.onClick.AddListener(delegate
			{
				LoadPhysicalPresetDialog();
			});
		}
		idText = componentInChildren.idText;
		if (idText != null)
		{
			idText.onEndEdit.AddListener(SetUID);
		}
		idTextAction = componentInChildren.idTextAction;
		if (idTextAction != null)
		{
			InputFieldAction inputFieldAction = idTextAction;
			inputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(inputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetUIDToInputField));
		}
		descriptionText = componentInChildren.descriptionText;
		SyncIdText();
		SyncDescriptionText();
	}

	public override void InitUIAlt()
	{
		if (!(UITransformAlt != null))
		{
			return;
		}
		AtomUI componentInChildren = UITransformAlt.GetComponentInChildren<AtomUI>(true);
		if (!(componentInChildren != null))
		{
			return;
		}
		onJSON.toggleAlt = componentInChildren.onToggle;
		hiddenJSON.toggleAlt = componentInChildren.hiddenToggle;
		collisionEnabledJSON.toggleAlt = componentInChildren.collisionEnabledToggle;
		freezePhysicsJSON.toggleAlt = componentInChildren.freezePhysicsToggle;
		ResetPhysicsJSONAction.buttonAlt = componentInChildren.resetPhysicsButton;
		resetPhysicsProgressJSON.sliderAlt = componentInChildren.resetPhysicsProgressSlider;
		SelectContainingSubSceneAction.buttonAlt = componentInChildren.selectAtomParentFromSceneButton;
		if (componentInChildren.selectAtomParentFromSceneButton != null)
		{
			componentInChildren.selectAtomParentFromSceneButton.onClick.AddListener(SelectAtomParentFromScene);
		}
		if (componentInChildren.resetButton != null)
		{
			componentInChildren.resetButton.onClick.AddListener(delegate
			{
				Reset();
			});
		}
		if (componentInChildren.resetPhysicalButton != null)
		{
			componentInChildren.resetPhysicalButton.onClick.AddListener(delegate
			{
				ResetPhysical();
			});
		}
		if (componentInChildren.resetAppearanceButton != null)
		{
			componentInChildren.resetAppearanceButton.onClick.AddListener(delegate
			{
				ResetAppearance();
			});
		}
		if (componentInChildren.removeButton != null)
		{
			componentInChildren.removeButton.onClick.AddListener(delegate
			{
				Remove();
			});
		}
		if (componentInChildren.savePresetButton != null)
		{
			componentInChildren.savePresetButton.onClick.AddListener(delegate
			{
				SavePresetDialog(true, true);
			});
		}
		if (componentInChildren.saveAppearancePresetButton != null)
		{
			componentInChildren.saveAppearancePresetButton.onClick.AddListener(delegate
			{
				SavePresetDialog(false, true);
			});
		}
		if (componentInChildren.savePhysicalPresetButton != null)
		{
			componentInChildren.savePhysicalPresetButton.onClick.AddListener(delegate
			{
				SavePresetDialog(true);
			});
		}
		if (componentInChildren.loadPresetButton != null)
		{
			componentInChildren.loadPresetButton.onClick.AddListener(delegate
			{
				LoadPresetDialog();
			});
		}
		if (componentInChildren.loadAppearancePresetButton != null)
		{
			componentInChildren.loadAppearancePresetButton.onClick.AddListener(delegate
			{
				LoadAppearancePresetDialog();
			});
		}
		if (componentInChildren.loadPhysicalPresetButton != null)
		{
			componentInChildren.loadPhysicalPresetButton.onClick.AddListener(delegate
			{
				LoadPhysicalPresetDialog();
			});
		}
		idTextAlt = componentInChildren.idText;
		if (idTextAlt != null)
		{
			idTextAlt.onEndEdit.AddListener(SetUID);
		}
		idTextActionAlt = componentInChildren.idTextAction;
		if (idTextActionAlt != null)
		{
			InputFieldAction inputFieldAction = idTextActionAlt;
			inputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(inputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetUIDToInputField));
		}
		descriptionTextAlt = componentInChildren.descriptionText;
		SyncIdText();
		SyncDescriptionText();
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

	private void Start()
	{
		if (reParentObject != null)
		{
			reParentObjectStartingPosition = reParentObject.position;
			reParentObjectStartingRotation = reParentObject.rotation;
		}
		if (childAtomContainer != null)
		{
			childAtomContainerStartingPosition = childAtomContainer.position;
			childAtomContainerStartingRotation = childAtomContainer.rotation;
		}
		SyncMasterControllerCorners();
	}

	private void Update()
	{
		if (Application.isPlaying)
		{
			CheckResumeSimulation();
		}
		if (!(_masterController != null) || masterControllerCorners == null || masterControllerCorners.Length < 8)
		{
			return;
		}
		if (_freeControllers.Length > 1 || (alwaysShowExtents && _freeControllers.Length > 0))
		{
			Vector3 position = _freeControllers[0].transform.position;
			extentLowX = position.x;
			extentHighX = position.x;
			extentLowY = position.y;
			extentHighY = position.y;
			extentLowZ = position.z;
			extentHighZ = position.z;
			FreeControllerV3[] array = _freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				if (freeControllerV != _masterController)
				{
					position = freeControllerV.transform.position;
					if (position.x > extentHighX)
					{
						extentHighX = position.x;
					}
					else if (position.x < extentLowX)
					{
						extentLowX = position.x;
					}
					if (position.y > extentHighY)
					{
						extentHighY = position.y;
					}
					else if (position.y < extentLowY)
					{
						extentLowY = position.y;
					}
					if (position.z > extentHighZ)
					{
						extentHighZ = position.z;
					}
					else if (position.z < extentLowZ)
					{
						extentLowZ = position.z;
					}
				}
			}
			extentLowX -= extentPadding;
			extentLowY -= extentPadding;
			extentLowZ -= extentPadding;
			extentHighX += extentPadding;
			extentHighY += extentPadding;
			extentHighZ += extentPadding;
			extentlll.x = extentLowX;
			extentlll.y = extentLowY;
			extentlll.z = extentLowZ;
			extentllh.x = extentLowX;
			extentllh.y = extentLowY;
			extentllh.z = extentHighZ;
			extentlhl.x = extentLowX;
			extentlhl.y = extentHighY;
			extentlhl.z = extentLowZ;
			extentlhh.x = extentLowX;
			extentlhh.y = extentHighY;
			extentlhh.z = extentHighZ;
			extenthll.x = extentHighX;
			extenthll.y = extentLowY;
			extenthll.z = extentLowZ;
			extenthlh.x = extentHighX;
			extenthlh.y = extentLowY;
			extenthlh.z = extentHighZ;
			extenthhl.x = extentHighX;
			extenthhl.y = extentHighY;
			extenthhl.z = extentLowZ;
			extenthhh.x = extentHighX;
			extenthhh.y = extentHighY;
			extenthhh.z = extentHighZ;
			masterControllerCorners[0].position = extentlll;
			masterControllerCorners[1].position = extentllh;
			masterControllerCorners[2].position = extentlhl;
			masterControllerCorners[3].position = extentlhh;
			masterControllerCorners[4].position = extenthll;
			masterControllerCorners[5].position = extenthlh;
			masterControllerCorners[6].position = extenthhl;
			masterControllerCorners[7].position = extenthhh;
			Transform[] array2 = masterControllerCorners;
			foreach (Transform transform in array2)
			{
				transform.gameObject.SetActive(true);
			}
		}
		else
		{
			Transform[] array3 = masterControllerCorners;
			foreach (Transform transform2 in array3)
			{
				transform2.gameObject.SetActive(false);
			}
		}
	}

	private void OnEnable()
	{
		ClearResetPhysics();
	}

	private void OnDisable()
	{
		ClearResetPhysics();
		ClearPauseAutoSimulationFlag();
		grabFreezePhysics = false;
		tempOff = false;
		tempHidden = false;
		tempFreezePhysics = false;
		tempDisableRender = false;
		tempDisableCollision = false;
		globalDisableRender = false;
	}

	private void OnDestroy()
	{
		if (_callbackRegistered && SuperController.singleton != null)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Remove(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomRenamed));
			SuperController singleton2 = SuperController.singleton;
			singleton2.onAtomSubSceneChangedHandlers = (SuperController.OnAtomSubSceneChanged)Delegate.Remove(singleton2.onAtomSubSceneChangedHandlers, new SuperController.OnAtomSubSceneChanged(OnAtomSubSceneChanged));
		}
	}
}
