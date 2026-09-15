using System;
using System.Collections.Generic;
using GPUTools.Physics.Scripts.Behaviours;
using UnityEngine;

public class HairSimStyleToolControl : CapsuleToolControl
{
	public enum ToolChoice
	{
		Cut,
		Grow,
		Hold,
		Grab,
		Push,
		Pull,
		Brush,
		RigidityIncrease,
		RigidityDecrease,
		RigiditySet
	}

	public GpuCutCapsule cutCapsule;

	public GpuGrowCapsule growCapsule;

	public GpuHoldCapsule holdCapsule;

	public GpuGrabCapsule grabCapsule;

	public GpuPushCapsule pushCapsule;

	public GpuPullCapsule pullCapsule;

	public GpuBrushCapsule brushCapsule;

	public GpuRigidityIncreaseCapsule rigidityIncreaseCapsule;

	public GpuRigidityDecreaseCapsule rigidityDecreaseCapsule;

	public GpuRigiditySetCapsule rigiditySetCapsule;

	protected JSONStorableBool onJSON;

	protected List<string> allToolChoices;

	protected List<string> standardToolChoices;

	public ToolChoice toolChoice;

	protected JSONStorableStringChooser toolChoiceJSON;

	protected bool _allowRigidityPaint;

	protected JSONStorableFloat cutStrengthJSON;

	protected JSONStorableFloat growStrengthJSON;

	protected JSONStorableFloat pushStrengthJSON;

	protected JSONStorableFloat pullStrengthJSON;

	protected JSONStorableFloat brushStrengthJSON;

	protected JSONStorableFloat holdStrengthJSON;

	protected JSONStorableFloat rigidityIncreaseStrengthJSON;

	protected JSONStorableFloat rigidityDecreaseStrengthJSON;

	protected JSONStorableFloat rigiditySetStrengthJSON;

	protected int burstForFrames;

	protected JSONStorableAction burstAction;

	protected float toolStrengthMultiplier = 1f;

	protected UIDynamicSlider dynamicSlider;

	protected UIDynamicButton dynamicButton;

	public bool allowRigidityPaint
	{
		get
		{
			return _allowRigidityPaint;
		}
		set
		{
			if (_allowRigidityPaint == value)
			{
				return;
			}
			_allowRigidityPaint = value;
			if (toolChoiceJSON == null)
			{
				return;
			}
			if (_allowRigidityPaint)
			{
				toolChoiceJSON.choices = allToolChoices;
				return;
			}
			toolChoiceJSON.choices = standardToolChoices;
			if (toolChoiceJSON.val == "RigidityIncrease" || toolChoiceJSON.val == "RigidityDecrease" || toolChoiceJSON.val == "RigiditySet")
			{
				toolChoiceJSON.val = "Brush";
			}
		}
	}

	protected void SyncOn(bool b)
	{
		SyncTool();
	}

	protected void SyncTool()
	{
		if (dynamicSlider != null)
		{
			dynamicSlider.gameObject.SetActive(false);
		}
		if (dynamicButton != null)
		{
			dynamicButton.gameObject.SetActive(false);
		}
		if (cutCapsule != null)
		{
			cutCapsule.enabled = false;
		}
		if (growCapsule != null)
		{
			growCapsule.enabled = false;
		}
		if (holdCapsule != null)
		{
			holdCapsule.enabled = false;
		}
		if (grabCapsule != null)
		{
			grabCapsule.enabled = false;
		}
		if (pushCapsule != null)
		{
			pushCapsule.enabled = false;
		}
		if (pullCapsule != null)
		{
			pullCapsule.enabled = false;
		}
		if (brushCapsule != null)
		{
			brushCapsule.enabled = false;
		}
		if (rigidityIncreaseCapsule != null)
		{
			rigidityIncreaseCapsule.enabled = false;
		}
		if (rigidityDecreaseCapsule != null)
		{
			rigidityDecreaseCapsule.enabled = false;
		}
		if (rigiditySetCapsule != null)
		{
			rigiditySetCapsule.enabled = false;
		}
		if (cutStrengthJSON != null)
		{
			cutStrengthJSON.slider = null;
		}
		if (growStrengthJSON != null)
		{
			growStrengthJSON.slider = null;
		}
		if (holdStrengthJSON != null)
		{
			holdStrengthJSON.slider = null;
		}
		if (pushStrengthJSON != null)
		{
			pushStrengthJSON.slider = null;
		}
		if (pullStrengthJSON != null)
		{
			pullStrengthJSON.slider = null;
		}
		if (brushStrengthJSON != null)
		{
			brushStrengthJSON.slider = null;
		}
		if (rigidityIncreaseStrengthJSON != null)
		{
			rigidityIncreaseStrengthJSON.slider = null;
		}
		if (rigidityDecreaseStrengthJSON != null)
		{
			rigidityDecreaseStrengthJSON.slider = null;
		}
		if (rigiditySetStrengthJSON != null)
		{
			rigiditySetStrengthJSON.slider = null;
		}
		switch (toolChoice)
		{
		case ToolChoice.Cut:
			if (cutCapsule != null)
			{
				cutCapsule.enabled = onJSON.val;
			}
			if (dynamicSlider != null && cutStrengthJSON != null)
			{
				dynamicSlider.gameObject.SetActive(true);
				cutStrengthJSON.slider = dynamicSlider.slider;
				dynamicSlider.label = "Cut Strength";
			}
			if (dynamicButton != null)
			{
				dynamicButton.gameObject.SetActive(true);
				dynamicButton.label = "Burst Cut";
			}
			break;
		case ToolChoice.Grow:
			if (growCapsule != null)
			{
				growCapsule.enabled = onJSON.val;
			}
			if (dynamicSlider != null && growStrengthJSON != null)
			{
				dynamicSlider.gameObject.SetActive(true);
				growStrengthJSON.slider = dynamicSlider.slider;
				dynamicSlider.label = "Grow Strength";
			}
			if (dynamicButton != null)
			{
				dynamicButton.gameObject.SetActive(true);
				dynamicButton.label = "Burst Grow";
			}
			break;
		case ToolChoice.Hold:
			if (holdCapsule != null)
			{
				holdCapsule.enabled = onJSON.val;
			}
			if (dynamicSlider != null && holdStrengthJSON != null)
			{
				dynamicSlider.gameObject.SetActive(true);
				holdStrengthJSON.slider = dynamicSlider.slider;
				dynamicSlider.label = "Hold Strength";
			}
			break;
		case ToolChoice.Grab:
			if (grabCapsule != null)
			{
				grabCapsule.enabled = onJSON.val;
			}
			break;
		case ToolChoice.Push:
			if (pushCapsule != null)
			{
				pushCapsule.enabled = onJSON.val;
			}
			if (dynamicSlider != null && pushStrengthJSON != null)
			{
				dynamicSlider.gameObject.SetActive(true);
				pushStrengthJSON.slider = dynamicSlider.slider;
				dynamicSlider.label = "Push Strength";
			}
			if (dynamicButton != null)
			{
				dynamicButton.gameObject.SetActive(true);
				dynamicButton.label = "Burst Push";
			}
			break;
		case ToolChoice.Pull:
			if (pullCapsule != null)
			{
				pullCapsule.enabled = onJSON.val;
			}
			if (dynamicSlider != null && pullStrengthJSON != null)
			{
				dynamicSlider.gameObject.SetActive(true);
				pullStrengthJSON.slider = dynamicSlider.slider;
				dynamicSlider.label = "Pull Strength";
			}
			if (dynamicButton != null)
			{
				dynamicButton.gameObject.SetActive(true);
				dynamicButton.label = "Burst Pull";
			}
			break;
		case ToolChoice.Brush:
			if (brushCapsule != null)
			{
				brushCapsule.enabled = onJSON.val;
			}
			if (dynamicSlider != null && brushStrengthJSON != null)
			{
				dynamicSlider.gameObject.SetActive(true);
				brushStrengthJSON.slider = dynamicSlider.slider;
				dynamicSlider.label = "Brush Strength";
			}
			break;
		case ToolChoice.RigidityIncrease:
			if (rigidityIncreaseCapsule != null)
			{
				rigidityIncreaseCapsule.enabled = onJSON.val;
			}
			if (dynamicSlider != null && rigidityIncreaseStrengthJSON != null)
			{
				dynamicSlider.gameObject.SetActive(true);
				rigidityIncreaseStrengthJSON.slider = dynamicSlider.slider;
				dynamicSlider.label = "Rigidity + Strength";
			}
			if (dynamicButton != null)
			{
				dynamicButton.gameObject.SetActive(true);
				dynamicButton.label = "Burst Rigidity +";
			}
			break;
		case ToolChoice.RigidityDecrease:
			if (rigidityDecreaseCapsule != null)
			{
				rigidityDecreaseCapsule.enabled = onJSON.val;
			}
			if (dynamicSlider != null && rigidityDecreaseStrengthJSON != null)
			{
				dynamicSlider.gameObject.SetActive(true);
				rigidityDecreaseStrengthJSON.slider = dynamicSlider.slider;
				dynamicSlider.label = "Rigidity - Strength";
			}
			if (dynamicButton != null)
			{
				dynamicButton.gameObject.SetActive(true);
				dynamicButton.label = "Burst Rigidity -";
			}
			break;
		case ToolChoice.RigiditySet:
			if (rigiditySetCapsule != null)
			{
				rigiditySetCapsule.enabled = onJSON.val;
			}
			if (dynamicSlider != null && rigiditySetStrengthJSON != null)
			{
				dynamicSlider.gameObject.SetActive(true);
				rigiditySetStrengthJSON.slider = dynamicSlider.slider;
				dynamicSlider.label = "Rigidity Set Level";
			}
			if (dynamicButton != null)
			{
				dynamicButton.gameObject.SetActive(true);
				dynamicButton.label = "Burst Rigidity Set";
			}
			break;
		}
	}

	protected void SyncToolChoice(string s)
	{
		try
		{
			toolChoice = (ToolChoice)Enum.Parse(typeof(ToolChoice), s);
			burstForFrames = 0;
			SyncTool();
		}
		catch (ArgumentException)
		{
			Debug.LogError("Attempted to set tool to " + s + " which is not a valid ToolChoice");
		}
	}

	protected void SyncCutStrength(float f)
	{
		if (cutCapsule != null)
		{
			cutCapsule.strength = f * toolStrengthMultiplier;
		}
	}

	protected void SyncGrowStrength(float f)
	{
		if (growCapsule != null)
		{
			growCapsule.strength = f * toolStrengthMultiplier;
		}
	}

	protected void SyncPushStrength(float f)
	{
		if (pushCapsule != null)
		{
			pushCapsule.strength = f * toolStrengthMultiplier;
		}
	}

	protected void SyncPullStrength(float f)
	{
		if (pullCapsule != null)
		{
			pullCapsule.strength = f * toolStrengthMultiplier;
		}
	}

	protected void SyncBrushStrength(float f)
	{
		if (brushCapsule != null)
		{
			brushCapsule.strength = f * toolStrengthMultiplier;
		}
	}

	protected void SyncHoldStrength(float f)
	{
		if (holdCapsule != null)
		{
			holdCapsule.strength = f;
		}
	}

	protected void SyncGrabStrength()
	{
		if (grabCapsule != null)
		{
			if (toolStrengthMultiplier > 0.5f)
			{
				grabCapsule.strength = 1f;
			}
			else
			{
				grabCapsule.strength = 0f;
			}
		}
	}

	protected void SyncRigidityIncreaseStrength(float f)
	{
		if (rigidityIncreaseCapsule != null)
		{
			rigidityIncreaseCapsule.strength = f * toolStrengthMultiplier;
		}
	}

	protected void SyncRigidityDecreaseStrength(float f)
	{
		if (rigidityDecreaseCapsule != null)
		{
			rigidityDecreaseCapsule.strength = f * toolStrengthMultiplier;
		}
	}

	protected void SyncRigiditySetStrength(float f)
	{
		if (rigiditySetCapsule != null)
		{
			if (toolStrengthMultiplier > 0.5f)
			{
				rigiditySetCapsule.strength = f;
			}
			else
			{
				rigiditySetCapsule.strength = -1f;
			}
		}
	}

	protected void BurstAction()
	{
		onJSON.val = true;
		burstForFrames = 10;
	}

	public void ResetToolStrengthMultiplier()
	{
		SetToolStrengthMultiplier(1f);
	}

	public void SetToolStrengthMultiplier(float f)
	{
		toolStrengthMultiplier = f;
		if (brushStrengthJSON != null)
		{
			SyncBrushStrength(brushStrengthJSON.val);
		}
		if (cutStrengthJSON != null)
		{
			SyncCutStrength(cutStrengthJSON.val);
		}
		if (growStrengthJSON != null)
		{
			SyncGrowStrength(growStrengthJSON.val);
		}
		SyncGrabStrength();
		if (pullStrengthJSON != null)
		{
			SyncPullStrength(pullStrengthJSON.val);
		}
		if (pushStrengthJSON != null)
		{
			SyncPushStrength(pushStrengthJSON.val);
		}
		if (rigidityIncreaseStrengthJSON != null)
		{
			SyncRigidityIncreaseStrength(rigidityIncreaseStrengthJSON.val);
		}
		if (rigidityDecreaseStrengthJSON != null)
		{
			SyncRigidityDecreaseStrength(rigidityDecreaseStrengthJSON.val);
		}
		if (rigiditySetStrengthJSON != null)
		{
			SyncRigiditySetStrength(rigiditySetStrengthJSON.val);
		}
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (!wasInit)
		{
			return;
		}
		base.InitUI(t, isAlt);
		if (t != null)
		{
			HairSimStyleToolControlUI componentInChildren = UITransform.GetComponentInChildren<HairSimStyleToolControlUI>();
			if (componentInChildren != null)
			{
				onJSON.RegisterToggle(componentInChildren.onToggle, isAlt);
				toolChoiceJSON.RegisterPopup(componentInChildren.toolChoicePopup, isAlt);
				if (!isAlt)
				{
					dynamicButton = componentInChildren.dynamicButton;
					if (dynamicButton != null)
					{
						burstAction.RegisterButton(componentInChildren.dynamicButton.button, isAlt);
					}
					dynamicSlider = componentInChildren.dynamicSlider;
				}
			}
		}
		SyncTool();
	}

	protected override void Init()
	{
		base.Init();
		onJSON = new JSONStorableBool("on", true, SyncOn);
		RegisterBool(onJSON);
		allToolChoices = new List<string>();
		allToolChoices.Add("Cut");
		allToolChoices.Add("Grow");
		allToolChoices.Add("Hold");
		allToolChoices.Add("Grab");
		allToolChoices.Add("Push");
		allToolChoices.Add("Pull");
		allToolChoices.Add("Brush");
		allToolChoices.Add("RigidityIncrease");
		allToolChoices.Add("RigidityDecrease");
		allToolChoices.Add("RigiditySet");
		standardToolChoices = new List<string>();
		standardToolChoices.Add("Cut");
		standardToolChoices.Add("Grow");
		standardToolChoices.Add("Hold");
		standardToolChoices.Add("Grab");
		standardToolChoices.Add("Push");
		standardToolChoices.Add("Pull");
		standardToolChoices.Add("Brush");
		List<string> choicesList = ((!allowRigidityPaint) ? standardToolChoices : allToolChoices);
		toolChoiceJSON = new JSONStorableStringChooser("toolChoice", choicesList, toolChoice.ToString(), "Tool", SyncToolChoice);
		RegisterStringChooser(toolChoiceJSON);
		burstAction = new JSONStorableAction("burstAction", BurstAction);
		RegisterAction(burstAction);
		if (cutCapsule != null)
		{
			cutStrengthJSON = new JSONStorableFloat("cutStrength", cutCapsule.strength, SyncCutStrength, 0f, 1f);
			RegisterFloat(cutStrengthJSON);
		}
		if (growCapsule != null)
		{
			growStrengthJSON = new JSONStorableFloat("growStrength", growCapsule.strength, SyncGrowStrength, 0f, 1f);
			RegisterFloat(growStrengthJSON);
		}
		if (pushCapsule != null)
		{
			pushStrengthJSON = new JSONStorableFloat("pushStrength", pushCapsule.strength, SyncPushStrength, 0f, 1f);
			RegisterFloat(pushStrengthJSON);
		}
		if (pullCapsule != null)
		{
			pullStrengthJSON = new JSONStorableFloat("pullStrength", pullCapsule.strength, SyncPullStrength, 0f, 1f);
			RegisterFloat(pullStrengthJSON);
		}
		if (brushCapsule != null)
		{
			brushStrengthJSON = new JSONStorableFloat("brushStrength", brushCapsule.strength, SyncBrushStrength, 0f, 1f);
			RegisterFloat(brushStrengthJSON);
		}
		if (holdCapsule != null)
		{
			holdStrengthJSON = new JSONStorableFloat("holdStrength", holdCapsule.strength, SyncHoldStrength, 0f, 1f);
			RegisterFloat(holdStrengthJSON);
		}
		if (rigidityIncreaseCapsule != null)
		{
			rigidityIncreaseStrengthJSON = new JSONStorableFloat("rigidityIncreaseStrength", rigidityIncreaseCapsule.strength, SyncRigidityIncreaseStrength, 0f, 1f);
			RegisterFloat(rigidityIncreaseStrengthJSON);
		}
		if (rigidityDecreaseCapsule != null)
		{
			rigidityDecreaseStrengthJSON = new JSONStorableFloat("rigidityDecreaseStrength", rigidityDecreaseCapsule.strength, SyncRigidityDecreaseStrength, 0f, 1f);
			RegisterFloat(rigidityDecreaseStrengthJSON);
		}
		if (rigiditySetCapsule != null)
		{
			rigiditySetStrengthJSON = new JSONStorableFloat("rigiditySetStrength", rigiditySetCapsule.strength, SyncRigiditySetStrength, 0f, 1f);
			RegisterFloat(rigiditySetStrengthJSON);
		}
		SyncTool();
	}

	private void FixedUpdate()
	{
		if (burstForFrames > 0)
		{
			burstForFrames--;
			if (burstForFrames == 0)
			{
				onJSON.val = false;
			}
			else
			{
				onJSON.val = true;
			}
		}
	}
}
