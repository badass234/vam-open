using GPUTools.Cloth.Scripts;
using GPUTools.Skinner.Scripts.Providers;
using MeshVR;
using UnityEngine;
using UnityEngine.UI;

public class ClothSimControl : PhysicsSimulatorJSONStorable
{
	public ClothSettings clothSettings;

	protected DAZSkinWrap skinWrap;

	public JSONStorableBool simEnabledJSON;

	public JSONStorableBool integrateEnabledJSON;

	protected Button resetButton;

	protected Button resetButtonAlt;

	protected JSONStorableAction resetAction;

	public JSONStorableBool collisionEnabledJSON;

	protected JSONStorableFloat collisionRadiusJSON;

	protected JSONStorableFloat dragJSON;

	protected JSONStorableFloat weightJSON;

	protected JSONStorableFloat distanceScaleJSON;

	protected JSONStorableFloat stiffnessJSON;

	protected JSONStorableFloat compressionResistanceJSON;

	protected JSONStorableFloat frictionJSON;

	protected JSONStorableFloat staticMultiplierJSON;

	protected JSONStorableFloat collisionPowerJSON;

	protected JSONStorableFloat gravityMultiplierJSON;

	protected JSONStorableFloat iterationsJSON;

	protected JSONStorableBool allowDetachJSON;

	protected JSONStorableFloat detachThresholdJSON;

	protected JSONStorableFloat jointStrengthJSON;

	protected JSONStorableVector3 forceJSON;

	public override void ScaleChanged(float scale)
	{
		base.ScaleChanged(scale);
		if (clothSettings != null)
		{
			clothSettings.WorldScale = scale;
		}
		Reset();
	}

	public override void PostRestore()
	{
		base.PostRestore();
		Reset();
	}

	protected void SyncSimEnabled(bool b)
	{
		if (clothSettings != null)
		{
			clothSettings.enabled = b;
			if (b)
			{
				Reset();
			}
		}
	}

	public void SetSimEnabled(bool b)
	{
		if (simEnabledJSON != null)
		{
			simEnabledJSON.val = b;
		}
	}

	protected void SyncIntegrateEnabled(bool b)
	{
		if (clothSettings != null)
		{
			clothSettings.IntegrateEnabled = b;
		}
	}

	public void Reset()
	{
		if (clothSettings != null)
		{
			clothSettings.Reset();
		}
	}

	protected override void SyncCollisionEnabled()
	{
		bool flag = true;
		if (collisionEnabledJSON != null)
		{
			flag = collisionEnabledJSON.val;
		}
		if (clothSettings != null)
		{
			clothSettings.CollisionEnabled = _collisionEnabled && flag;
		}
	}

	protected void SyncCollisionEnabled(bool b)
	{
		SyncCollisionEnabled();
	}

	public void SyncCollisionRadius(float f)
	{
		if (clothSettings != null)
		{
			clothSettings.ParticleRadius = f;
			Reset();
		}
	}

	public void SyncDrag(float f)
	{
		if (clothSettings != null)
		{
			clothSettings.Drag = f;
		}
	}

	public void SyncWeight(float f)
	{
		if (clothSettings != null)
		{
			clothSettings.Weight = f;
		}
	}

	public void SyncDistanceScale(float f)
	{
		if (clothSettings != null)
		{
			clothSettings.DistanceScale = f;
		}
	}

	public void SyncStiffness(float f)
	{
		if (clothSettings != null)
		{
			clothSettings.Stiffness = f;
		}
	}

	public void SyncCompressionResistance(float f)
	{
		if (clothSettings != null)
		{
			clothSettings.CompressionResistance = f;
		}
	}

	public void SyncFriction(float f)
	{
		if (clothSettings != null)
		{
			clothSettings.Friction = f;
		}
	}

	public void SyncStaticMultiplier(float f)
	{
		if (clothSettings != null)
		{
			clothSettings.StaticMultiplier = f;
		}
	}

	public void SyncCollisionPower(float f)
	{
		if (clothSettings != null)
		{
			clothSettings.CollisionPower = f;
		}
	}

	public void SyncGravityMultiplier(float f)
	{
		if (clothSettings != null)
		{
			clothSettings.GravityMultiplier = f;
		}
	}

	public void SyncIterations(float f)
	{
		if (clothSettings != null)
		{
			clothSettings.Iterations = Mathf.FloorToInt(f);
		}
	}

	public void SyncAllowDetatch(bool b)
	{
		if (clothSettings != null)
		{
			clothSettings.BreakEnabled = b;
		}
	}

	public void AllowDetach()
	{
		if (allowDetachJSON != null)
		{
			allowDetachJSON.val = true;
		}
	}

	public void SyncDetachThreshold(float f)
	{
		if (clothSettings != null)
		{
			clothSettings.BreakThreshold = f;
		}
	}

	public void SyncJointStrength(float f)
	{
		if (clothSettings != null)
		{
			clothSettings.JointStrength = f;
		}
	}

	protected void SyncForce()
	{
		if (forceJSON != null)
		{
			SyncForce(forceJSON.val);
		}
	}

	protected void SyncForce(Vector3 v)
	{
		if (clothSettings != null && clothSettings.Runtime != null)
		{
			clothSettings.Runtime.Wind = v + WindControl.globalWind;
		}
	}

	protected void Init()
	{
		if (clothSettings == null)
		{
			clothSettings = GetComponent<ClothSettings>();
		}
		if (clothSettings != null)
		{
			if (clothSettings.MeshProvider.Type == ScalpMeshType.PreCalc)
			{
				skinWrap = clothSettings.MeshProvider.PreCalcProvider as DAZSkinWrap;
			}
			resetAction = new JSONStorableAction("Reset", Reset);
			RegisterAction(resetAction);
			simEnabledJSON = new JSONStorableBool("simEnabled", clothSettings.enabled, SyncSimEnabled);
			RegisterBool(simEnabledJSON);
			integrateEnabledJSON = new JSONStorableBool("integrateEnabled", clothSettings.IntegrateEnabled, SyncIntegrateEnabled);
			RegisterBool(integrateEnabledJSON);
			collisionEnabledJSON = new JSONStorableBool("collisionEnabled", clothSettings.CollisionEnabled, SyncCollisionEnabled);
			RegisterBool(collisionEnabledJSON);
			collisionRadiusJSON = new JSONStorableFloat("collisionRadius", clothSettings.ParticleRadius, SyncCollisionRadius, 0.001f, 0.1f);
			RegisterFloat(collisionRadiusJSON);
			dragJSON = new JSONStorableFloat("drag", clothSettings.Drag, SyncDrag, 0f, 1f);
			RegisterFloat(dragJSON);
			weightJSON = new JSONStorableFloat("weight", clothSettings.Weight, SyncWeight, 0f, 2f);
			RegisterFloat(weightJSON);
			distanceScaleJSON = new JSONStorableFloat("distanceScale", clothSettings.DistanceScale, SyncDistanceScale, 0.1f, 2f);
			RegisterFloat(distanceScaleJSON);
			stiffnessJSON = new JSONStorableFloat("stiffness", clothSettings.Stiffness, SyncStiffness, 0f, 1f);
			RegisterFloat(stiffnessJSON);
			compressionResistanceJSON = new JSONStorableFloat("compressionResistance", clothSettings.CompressionResistance, SyncCompressionResistance, 0f, 1f);
			RegisterFloat(compressionResistanceJSON);
			frictionJSON = new JSONStorableFloat("friction", clothSettings.Friction, SyncFriction, 0f, 1f);
			RegisterFloat(frictionJSON);
			staticMultiplierJSON = new JSONStorableFloat("staticMultiplier", clothSettings.StaticMultiplier, SyncStaticMultiplier, 0f, 10f);
			RegisterFloat(staticMultiplierJSON);
			collisionPowerJSON = new JSONStorableFloat("collisionPower", clothSettings.CollisionPower, SyncCollisionPower, 0f, 1f);
			RegisterFloat(collisionPowerJSON);
			gravityMultiplierJSON = new JSONStorableFloat("gravityMultiplier", clothSettings.GravityMultiplier, SyncGravityMultiplier, -4f, 4f);
			RegisterFloat(gravityMultiplierJSON);
			iterationsJSON = new JSONStorableFloat("iterations", clothSettings.Iterations, SyncIterations, 1f, 7f);
			RegisterFloat(iterationsJSON);
			allowDetachJSON = new JSONStorableBool("allowDetach", clothSettings.BreakEnabled, SyncAllowDetatch);
			RegisterBool(allowDetachJSON);
			detachThresholdJSON = new JSONStorableFloat("detachThreshold", clothSettings.BreakThreshold, SyncDetachThreshold, 0f, 0.05f, false);
			RegisterFloat(detachThresholdJSON);
			jointStrengthJSON = new JSONStorableFloat("jointStrength", clothSettings.JointStrength, SyncJointStrength, 0f, 1f);
			RegisterFloat(jointStrengthJSON);
			forceJSON = new JSONStorableVector3("force", Vector3.zero, new Vector3(-50f, -50f, -50f), new Vector3(50f, 50f, 50f), false);
			RegisterVector3(forceJSON);
		}
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (!(t != null))
		{
			return;
		}
		ClothSimControlUI componentInChildren = t.GetComponentInChildren<ClothSimControlUI>(true);
		if (!(componentInChildren != null))
		{
			return;
		}
		if (isAlt)
		{
			resetButtonAlt = componentInChildren.resetButton;
			if (resetButtonAlt != null)
			{
				resetButtonAlt.onClick.AddListener(Reset);
			}
		}
		else
		{
			resetButton = componentInChildren.resetButton;
			if (resetButton != null)
			{
				resetButton.onClick.AddListener(Reset);
			}
		}
		simEnabledJSON.RegisterToggle(componentInChildren.simEnabledToggle, isAlt);
		integrateEnabledJSON.RegisterToggle(componentInChildren.integrateEnabledToggle, isAlt);
		collisionEnabledJSON.RegisterToggle(componentInChildren.collisionEnabledToggle, isAlt);
		collisionRadiusJSON.RegisterSlider(componentInChildren.collisionRadiusSlider, isAlt);
		dragJSON.RegisterSlider(componentInChildren.dragSlider, isAlt);
		weightJSON.RegisterSlider(componentInChildren.weightSlider, isAlt);
		distanceScaleJSON.RegisterSlider(componentInChildren.distanceScaleSlider, isAlt);
		stiffnessJSON.RegisterSlider(componentInChildren.stiffnessSlider, isAlt);
		compressionResistanceJSON.RegisterSlider(componentInChildren.compressionResistanceSlider, isAlt);
		frictionJSON.RegisterSlider(componentInChildren.frictionSlider, isAlt);
		staticMultiplierJSON.RegisterSlider(componentInChildren.staticMultiplierSlider, isAlt);
		collisionPowerJSON.RegisterSlider(componentInChildren.collisionPowerSlider, isAlt);
		gravityMultiplierJSON.RegisterSlider(componentInChildren.gravityMultiplierSlider, isAlt);
		iterationsJSON.RegisterSlider(componentInChildren.iterationsSlider, isAlt);
		allowDetachJSON.RegisterToggle(componentInChildren.allowDetachToggle, isAlt);
		detachThresholdJSON.RegisterSlider(componentInChildren.detachThresholdSlider, isAlt);
		jointStrengthJSON.RegisterSlider(componentInChildren.jointStrengthSlider, isAlt);
		forceJSON.RegisterSliderX(componentInChildren.forceXSlider, isAlt);
		forceJSON.RegisterSliderY(componentInChildren.forceYSlider, isAlt);
		forceJSON.RegisterSliderZ(componentInChildren.forceZSlider, isAlt);
	}

	protected void DeregisterUI()
	{
		collisionRadiusJSON.slider = null;
		dragJSON.slider = null;
		weightJSON.slider = null;
		distanceScaleJSON.slider = null;
		stiffnessJSON.slider = null;
		compressionResistanceJSON.slider = null;
		frictionJSON.slider = null;
		staticMultiplierJSON.slider = null;
		collisionPowerJSON.slider = null;
		gravityMultiplierJSON.slider = null;
		iterationsJSON.slider = null;
		allowDetachJSON.toggle = null;
		detachThresholdJSON.slider = null;
		jointStrengthJSON.slider = null;
		if (resetButton != null)
		{
			resetButton.onClick.RemoveListener(Reset);
		}
		forceJSON.sliderX = null;
		forceJSON.sliderY = null;
		forceJSON.sliderZ = null;
		collisionRadiusJSON.sliderAlt = null;
		dragJSON.sliderAlt = null;
		weightJSON.sliderAlt = null;
		distanceScaleJSON.sliderAlt = null;
		stiffnessJSON.sliderAlt = null;
		compressionResistanceJSON.sliderAlt = null;
		frictionJSON.sliderAlt = null;
		staticMultiplierJSON.sliderAlt = null;
		collisionPowerJSON.sliderAlt = null;
		gravityMultiplierJSON.sliderAlt = null;
		iterationsJSON.sliderAlt = null;
		allowDetachJSON.toggleAlt = null;
		detachThresholdJSON.sliderAlt = null;
		jointStrengthJSON.sliderAlt = null;
		if (resetButtonAlt != null)
		{
			resetButtonAlt.onClick.RemoveListener(Reset);
		}
		forceJSON.sliderXAlt = null;
		forceJSON.sliderYAlt = null;
		forceJSON.sliderZAlt = null;
	}

	public override void SetUI(Transform t)
	{
		if (UITransform != t)
		{
			UITransform = t;
			if (base.isActiveAndEnabled)
			{
				InitUI();
			}
		}
	}

	public override void SetUIAlt(Transform t)
	{
		if (UITransformAlt != t)
		{
			UITransformAlt = t;
			if (base.isActiveAndEnabled)
			{
				InitUIAlt();
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

	protected override void Update()
	{
		base.Update();
		if (_resetSimulation && clothSettings != null && clothSettings.builder != null && clothSettings.builder.physics != null)
		{
			clothSettings.Reset();
		}
		SyncForce();
	}

	private void OnEnable()
	{
		InitUI();
		InitUIAlt();
	}

	private void OnDisable()
	{
		DeregisterUI();
	}
}
