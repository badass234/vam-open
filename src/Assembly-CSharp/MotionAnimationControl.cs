using System;
using SimpleJSON;
using UnityEngine;

public class MotionAnimationControl : JSONStorable
{
	protected MotionAnimationMaster _animationMaster;

	protected string[] customParamNames = new string[1] { "steps" };

	public bool suspendPositionPlayback;

	public bool suspendRotationPlayback;

	protected JSONStorableBool armedForRecordJSON;

	protected JSONStorableBool playbackEnabledJSON;

	protected JSONStorableBool drawPathJSON;

	public FreeControllerV3 controller;

	[NonSerialized]
	public MotionAnimationClip clip;

	public Material material;

	protected Mesh mesh;

	public bool drawPathOpt;

	public MotionAnimationMaster animationMaster
	{
		get
		{
			return _animationMaster;
		}
		set
		{
			if (_animationMaster != value)
			{
				_animationMaster = value;
			}
		}
	}

	public bool armedForRecord
	{
		get
		{
			if (armedForRecordJSON != null)
			{
				return armedForRecordJSON.val;
			}
			return false;
		}
		set
		{
			if (armedForRecordJSON != null)
			{
				armedForRecordJSON.val = value;
			}
		}
	}

	public bool playbackEnabled
	{
		get
		{
			if (playbackEnabledJSON != null)
			{
				return playbackEnabledJSON.val;
			}
			return false;
		}
		set
		{
			if (playbackEnabledJSON != null)
			{
				playbackEnabledJSON.val = value;
			}
		}
	}

	public bool drawPath
	{
		get
		{
			if (drawPathJSON != null)
			{
				return drawPathJSON.val;
			}
			return false;
		}
		set
		{
			if (drawPathJSON != null)
			{
				drawPathJSON.val = value;
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
		if ((includePhysical || forceStore) && clip != null && clip.SaveToJSON(jSON))
		{
			needsStore = true;
		}
		return jSON;
	}

	public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
	{
		base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
		if (!base.physicalLocked && restorePhysical && !IsCustomPhysicalParamLocked("steps") && clip != null)
		{
			clip.RestoreFromJSON(jc, setMissingToDefault);
		}
	}

	protected void Init()
	{
		armedForRecordJSON = new JSONStorableBool("armedForRecord", false);
		armedForRecordJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(armedForRecordJSON);
		playbackEnabledJSON = new JSONStorableBool("playbackEnabled", true);
		playbackEnabledJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(playbackEnabledJSON);
		drawPathJSON = new JSONStorableBool("drawPath", false);
		drawPathJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(drawPathJSON);
		clip = new MotionAnimationClip();
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		MotionAnimationControlUI componentInChildren = UITransform.GetComponentInChildren<MotionAnimationControlUI>(true);
		if (componentInChildren != null)
		{
			armedForRecordJSON.toggle = componentInChildren.armedForRecordToggle;
			playbackEnabledJSON.toggle = componentInChildren.playbackEnabledToggle;
			drawPathJSON.toggle = componentInChildren.drawPathToggle;
			if (componentInChildren.clearAnimationButton != null)
			{
				componentInChildren.clearAnimationButton.onClick.AddListener(ClearAnimation);
			}
		}
	}

	public override void InitUIAlt()
	{
		if (!(UITransformAlt != null))
		{
			return;
		}
		MotionAnimationControlUI componentInChildren = UITransformAlt.GetComponentInChildren<MotionAnimationControlUI>(true);
		if (componentInChildren != null)
		{
			armedForRecordJSON.toggleAlt = componentInChildren.armedForRecordToggle;
			playbackEnabledJSON.toggleAlt = componentInChildren.playbackEnabledToggle;
			drawPathJSON.toggleAlt = componentInChildren.drawPathToggle;
			if ((bool)componentInChildren.clearAnimationButton)
			{
				componentInChildren.clearAnimationButton.onClick.AddListener(ClearAnimation);
			}
		}
	}

	public void ClearAnimation()
	{
		if (clip != null)
		{
			clip.ClearAllSteps();
		}
	}

	public void PrepareRecord(int recordCounter)
	{
		if (armedForRecordJSON != null && armedForRecordJSON.val)
		{
			clip.PrepareRecord(recordCounter);
		}
	}

	public void RecordStep(float recordCounter, bool forceRecord = false)
	{
		if (armedForRecordJSON != null && armedForRecordJSON.val && controller != null)
		{
			clip.RecordStep(controller.transform, recordCounter, controller.currentPositionState != FreeControllerV3.PositionState.Off, controller.currentRotationState != FreeControllerV3.RotationState.Off, forceRecord);
		}
	}

	public void FinalizeRecord()
	{
		if (armedForRecordJSON != null && armedForRecordJSON.val)
		{
			clip.FinalizeRecord();
		}
	}

	public void TrimClip(float startStep, float stopStep)
	{
		if (clip != null && clip.clipLength > 0f)
		{
			clip.ClearAllStepsStartingAt(stopStep);
			clip.ShiftAllSteps(0f - startStep);
		}
	}

	protected void ApplyStep(MotionAnimationStep step, bool forceAlignControlled = false)
	{
		if (!(controller != null))
		{
			return;
		}
		if (!suspendPositionPlayback)
		{
			if (step.positionOn)
			{
				controller.currentPositionState = FreeControllerV3.PositionState.On;
			}
			else
			{
				controller.currentPositionState = FreeControllerV3.PositionState.Off;
			}
			controller.transform.localPosition = step.position;
			if (forceAlignControlled && controller.followWhenOff != null)
			{
				controller.followWhenOff.position = controller.transform.position;
			}
		}
		if (!suspendRotationPlayback)
		{
			if (step.rotationOn)
			{
				controller.currentRotationState = FreeControllerV3.RotationState.On;
			}
			else
			{
				controller.currentRotationState = FreeControllerV3.RotationState.Off;
			}
			controller.transform.localRotation = step.rotation;
			if (forceAlignControlled && controller.followWhenOff != null)
			{
				controller.followWhenOff.rotation = controller.transform.rotation;
			}
		}
	}

	public void PlaybackStepForceAlign(float playbackCounter)
	{
		if (playbackEnabledJSON != null && playbackEnabledJSON.val && clip != null && clip.clipLength > 0f)
		{
			MotionAnimationStep step = clip.PlaybackStep(playbackCounter);
			ApplyStep(step, true);
		}
	}

	public void PlaybackStep(float playbackCounter)
	{
		if (playbackEnabledJSON != null && playbackEnabledJSON.val && clip != null && clip.clipLength > 0f)
		{
			MotionAnimationStep step = clip.PlaybackStep(playbackCounter);
			ApplyStep(step);
		}
	}

	public void LoopbackStep(float percent, float toTimeStep)
	{
		if (playbackEnabledJSON != null && playbackEnabledJSON.val && clip != null && clip.clipLength > 0f)
		{
			MotionAnimationStep step = clip.LoopbackStep(percent, toTimeStep);
			ApplyStep(step);
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

	protected void Update()
	{
		if ((drawPathJSON.val || drawPathOpt) && clip != null && material != null)
		{
			Mesh mesh = clip.GetMesh();
			Graphics.DrawMesh(mesh, base.transform.parent.localToWorldMatrix, material, base.gameObject.layer, null, 0, null, false, false);
		}
	}
}
