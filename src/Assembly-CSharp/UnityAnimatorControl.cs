using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class UnityAnimatorControl : JSONStorable
{
	public Animator animator;

	[HideInInspector]
	public string[] animationChoices;

	public string startingAnimationChoice;

	protected JSONStorableAction animatorResetAction;

	protected bool _animatorEnabled;

	protected JSONStorableBool animatorEnabledJSON;

	protected bool _animatorIsPlaying = true;

	protected JSONStorableBool animatorIsPlayingJSON;

	protected JSONStorableAction animatorPlayAction;

	protected JSONStorableAction animatorPauseAction;

	protected float _animatorSpeed = 1f;

	protected JSONStorableFloat animatorSpeedJSON;

	protected string _animationSelection;

	protected JSONStorableStringChooser animationSelectionJSON;

	protected bool _useCrossFade = true;

	protected JSONStorableBool useCrossFadeJSON;

	protected float _crossFadeTime = 0.5f;

	protected JSONStorableFloat crossFadeTimeJSON;

	protected Transform animationSequenceContainer;

	public Transform animationSequenceClipPrefab;

	protected string _currentAnimationName = "None";

	protected JSONStorableString currentAnimationNameJSON;

	protected bool triggerSmoothTransition;

	protected int animationSequencePosition = -1;

	protected LinkedListNode<AnimationSequenceClip> _previousNode;

	protected LinkedListNode<AnimationSequenceClip> _currentNode;

	protected LinkedList<AnimationSequenceClip> animationSequence;

	protected bool _loopSequence;

	protected JSONStorableBool loopSequenceJSON;

	protected JSONStorableAction restartAnimationSequenceAction;

	protected JSONStorableActionStringChooser addAnimationToSequenceAction;

	protected JSONStorableActionStringChooser clearAndAddAnimationToSequenceAction;

	protected JSONStorableAction clearSequenceAction;

	protected JSONStorableAction clearSequenceButFinishCurrentClipAction;

	protected JSONStorableAction nextClipInSequenceAction;

	protected JSONStorableAction previousClipInSequenceAction;

	protected float _animationRotationSpeed;

	protected JSONStorableFloat animationRotationSpeedJSON;

	protected float _animationRotationDegreesForAction;

	protected JSONStorableFloat animationRotationDegressForAction;

	protected JSONStorableAction animationRotateAction;

	protected float lastAnimatorStateNormalizedTime;

	protected float lastAnimatorStateLoopCount;

	protected LinkedListNode<AnimationSequenceClip> PreviousNode => _previousNode;

	protected LinkedListNode<AnimationSequenceClip> CurrentNode => _currentNode;

	public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		JSONClass jSON = base.GetJSON(includePhysical, includeAppearance, forceStore);
		if (includePhysical)
		{
			AnimatorReset();
			JSONArray jSONArray = (JSONArray)(jSON["sequence"] = new JSONArray());
			foreach (AnimationSequenceClip item in animationSequence)
			{
				JSONClass jSONClass = new JSONClass();
				jSONClass["name"] = item.Name;
				jSONClass["useCrossFade"].AsBool = item.UseCrossFade;
				jSONClass["crossFadeTime"].AsFloat = item.CrossFadeTime;
				jSONArray.Add(jSONClass);
			}
		}
		return jSON;
	}

	public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
	{
		if (restorePhysical && !base.physicalLocked)
		{
			ClearSequence();
			if (base.mergeRestore)
			{
				triggerSmoothTransition = true;
			}
			else
			{
				AnimatorReset();
			}
		}
		base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
		if (!restorePhysical || base.physicalLocked)
		{
			return;
		}
		JSONArray asArray = jc["sequence"].AsArray;
		if (asArray != null)
		{
			foreach (JSONNode item in asArray)
			{
				JSONClass asObject = item.AsObject;
				if (asObject != null)
				{
					string text = asObject["name"];
					bool asBool = asObject["useCrossFade"].AsBool;
					float asFloat = asObject["crossFadeTime"].AsFloat;
					if (text != null)
					{
						AnimationSequenceClip asc = new AnimationSequenceClip(text, asBool, asFloat);
						AddAnimationToSequence(asc);
					}
				}
			}
		}
		if (!base.mergeRestore)
		{
			AnimatorReset();
		}
		AnimatorPlay();
	}

	protected IEnumerator DelayResetPhysics()
	{
		yield return null;
		containingAtom.ResetPhysics(true, false);
	}

	protected void AnimatorReset()
	{
		if (animator != null && animatorEnabledJSON.val)
		{
			animator.Rebind();
			animator.transform.localPosition = Vector3.zero;
			animator.transform.localRotation = Quaternion.identity;
			SetCurrentNode(animationSequence.First, true);
			StartCoroutine(DelayResetPhysics());
		}
	}

	protected void SyncAnimatorEnabled(bool b)
	{
		_animatorEnabled = b;
		if (animator != null)
		{
			animator.gameObject.SetActive(b);
		}
	}

	protected void SyncAnimatorIsPlaying(bool b)
	{
		_animatorIsPlaying = b;
		if (animatorPlayAction != null && animatorPlayAction.button != null)
		{
			animatorPlayAction.button.gameObject.SetActive(!_animatorIsPlaying);
		}
		if (animatorPauseAction != null && animatorPauseAction.button != null)
		{
			animatorPauseAction.button.gameObject.SetActive(_animatorIsPlaying);
		}
	}

	protected void AnimatorPlay()
	{
		animatorIsPlayingJSON.val = true;
	}

	protected void AnimatorPause()
	{
		animatorIsPlayingJSON.val = false;
	}

	protected void SyncAnimatorSpeed(float f)
	{
		_animatorSpeed = f;
		if (animator != null)
		{
			if (currentAnimationNameJSON.val != "None")
			{
				animator.speed = _animatorSpeed;
			}
			else
			{
				animator.speed = 0f;
			}
		}
	}

	protected void SyncAnimationSelection(string choice)
	{
		_animationSelection = choice;
	}

	protected void SyncUseCrossFade(bool b)
	{
		_useCrossFade = b;
	}

	protected void SyncCrossFadeTime(float f)
	{
		_crossFadeTime = f;
	}

	protected void SyncCurrentAnimationName(string s)
	{
		_currentAnimationName = s;
	}

	protected void SetCurrentAnimation(AnimationSequenceClip animation, bool forceNoCrossFade = false, float fixedTimeOffset = 0f)
	{
		if (animation != null)
		{
			currentAnimationNameJSON.val = animation.Name;
			if (animator != null)
			{
				animator.speed = _animatorSpeed;
				if (triggerSmoothTransition)
				{
					animator.CrossFadeInFixedTime(animation.Name, 0.5f, -1, fixedTimeOffset, 0f);
					triggerSmoothTransition = false;
				}
				else if (forceNoCrossFade)
				{
					animator.Play(animation.Name);
				}
				else if (animation.UseCrossFade)
				{
					animator.CrossFadeInFixedTime(animation.Name, animation.CrossFadeTime, -1, fixedTimeOffset, 0f);
				}
				else
				{
					animator.CrossFadeInFixedTime(animation.Name, 0f, -1, fixedTimeOffset, 0f);
				}
			}
		}
		else
		{
			currentAnimationNameJSON.val = "None";
			if (animator != null)
			{
				animator.speed = 0f;
			}
		}
	}

	protected void SetCurrentNode(LinkedListNode<AnimationSequenceClip> node, bool forceNoCrossFade = false, float fixedTimeOffset = 0f)
	{
		if (_currentNode != null)
		{
			_currentNode.Value.IsPlaying = false;
			_previousNode = _currentNode;
		}
		else
		{
			_previousNode = null;
		}
		_currentNode = node;
		if (_currentNode != null)
		{
			_currentNode.Value.IsPlaying = true;
			SetCurrentAnimation(_currentNode.Value, forceNoCrossFade, fixedTimeOffset);
		}
		else
		{
			SetCurrentAnimation(null, forceNoCrossFade, fixedTimeOffset);
		}
	}

	protected void SyncLoopSequence(bool b)
	{
		_loopSequence = b;
	}

	protected void RestartAnimationSequence()
	{
		SetCurrentNode(animationSequence.First);
	}

	protected void SyncSequenceUIOrder()
	{
		int num = 0;
		foreach (AnimationSequenceClip item in animationSequence)
		{
			item.UI.SetSiblingIndex(num);
			num++;
		}
	}

	protected void AddAnimationToSequence(AnimationSequenceClip asc)
	{
		if (animationSequenceClipPrefab != null && animationSequenceContainer != null)
		{
			RectTransform rt = (RectTransform)Object.Instantiate(animationSequenceClipPrefab);
			asc.UI = rt;
			asc.removeCallback = delegate
			{
				animationSequence.Remove(asc);
				Object.Destroy(rt.gameObject);
			};
			rt.SetParent(animationSequenceContainer, false);
		}
		else
		{
			asc.removeCallback = delegate
			{
				animationSequence.Remove(asc);
			};
		}
		LinkedListNode<AnimationSequenceClip> newNode = animationSequence.AddLast(asc);
		asc.moveBackwardCallback = delegate
		{
			LinkedListNode<AnimationSequenceClip> previous = newNode.Previous;
			if (previous != null)
			{
				animationSequence.Remove(newNode);
				animationSequence.AddBefore(previous, newNode);
				SyncSequenceUIOrder();
			}
		};
		asc.moveForwardCallback = delegate
		{
			LinkedListNode<AnimationSequenceClip> next = newNode.Next;
			if (next != null)
			{
				animationSequence.Remove(newNode);
				animationSequence.AddAfter(next, newNode);
				SyncSequenceUIOrder();
			}
		};
	}

	protected void AddAnimationToSequence(string choice)
	{
		AnimationSequenceClip asc = new AnimationSequenceClip(choice, _useCrossFade, _crossFadeTime);
		AddAnimationToSequence(asc);
	}

	protected void ClearAndAddAnimationToSequence(string choice)
	{
		ClearSequence();
		SetCurrentNode(animationSequence.First);
		AddAnimationToSequence(choice);
	}

	protected void ClearSequence()
	{
		List<AnimationSequenceClip> list = new List<AnimationSequenceClip>();
		foreach (AnimationSequenceClip item in animationSequence)
		{
			list.Add(item);
		}
		foreach (AnimationSequenceClip item2 in list)
		{
			item2.Remove();
		}
		animationSequence.Clear();
		SetCurrentNode(null);
	}

	protected void ClearSequenceButFinishCurrentClip()
	{
		List<AnimationSequenceClip> list = new List<AnimationSequenceClip>();
		foreach (AnimationSequenceClip item in animationSequence)
		{
			list.Add(item);
		}
		foreach (AnimationSequenceClip item2 in list)
		{
			item2.Remove();
		}
		animationSequence.Clear();
	}

	protected void NextClipInSequence()
	{
		if (CurrentNode != null && CurrentNode.Next != null)
		{
			SetCurrentNode(CurrentNode.Next);
		}
	}

	protected void PreviousClipInSequence()
	{
		if (CurrentNode != null && CurrentNode.Previous != null)
		{
			SetCurrentNode(CurrentNode.Previous);
		}
	}

	protected void SyncAnimationRotationSpeed(float f)
	{
		_animationRotationSpeed = f;
	}

	protected void SyncAnimationRotationDegressForAction(float f)
	{
		_animationRotationDegreesForAction = f;
	}

	protected void AnimationRotate()
	{
		if (animator != null)
		{
			animator.transform.Rotate(0f, _animationRotationDegreesForAction, 0f);
		}
	}

	public void RotateAnimation(float degrees)
	{
		if (animator != null)
		{
			animator.transform.Rotate(0f, degrees, 0f);
		}
	}

	protected void Init()
	{
		animationSequence = new LinkedList<AnimationSequenceClip>();
		animatorResetAction = new JSONStorableAction("AnimatorReset", AnimatorReset);
		RegisterAction(animatorResetAction);
		animatorEnabledJSON = new JSONStorableBool("animatorEnabled", _animatorEnabled, SyncAnimatorEnabled);
		animatorEnabledJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(animatorEnabledJSON);
		animatorIsPlayingJSON = new JSONStorableBool("animatorIsPlaying", true, SyncAnimatorIsPlaying);
		animatorIsPlayingJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(animatorIsPlayingJSON);
		animatorPlayAction = new JSONStorableAction("AnimatorPlay", AnimatorPlay);
		RegisterAction(animatorPlayAction);
		animatorPauseAction = new JSONStorableAction("AnimatorPause", AnimatorPause);
		RegisterAction(animatorPauseAction);
		animatorSpeedJSON = new JSONStorableFloat("AnimatorSpeed", _animatorSpeed, SyncAnimatorSpeed, 0f, 5f);
		animatorSpeedJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(animatorSpeedJSON);
		List<string> choicesList = new List<string>(animationChoices);
		_animationSelection = startingAnimationChoice;
		animationSelectionJSON = new JSONStorableStringChooser("animationSelection", choicesList, startingAnimationChoice, "Animation Selection", SyncAnimationSelection);
		animationSelectionJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterStringChooser(animationSelectionJSON);
		useCrossFadeJSON = new JSONStorableBool("useCrossFade", _useCrossFade, SyncUseCrossFade);
		useCrossFadeJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(useCrossFadeJSON);
		crossFadeTimeJSON = new JSONStorableFloat("crossFadeTime", _crossFadeTime, SyncCrossFadeTime, 0f, 5f);
		crossFadeTimeJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(crossFadeTimeJSON);
		currentAnimationNameJSON = new JSONStorableString("currentAnimationName", _currentAnimationName, SyncCurrentAnimationName);
		loopSequenceJSON = new JSONStorableBool("loopSequence", _loopSequence, SyncLoopSequence);
		loopSequenceJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(loopSequenceJSON);
		restartAnimationSequenceAction = new JSONStorableAction("RestartAnimationSequence", RestartAnimationSequence);
		RegisterAction(restartAnimationSequenceAction);
		addAnimationToSequenceAction = new JSONStorableActionStringChooser("AddAnimationToSequence", AddAnimationToSequence, animationSelectionJSON);
		RegisterStringChooserAction(addAnimationToSequenceAction);
		clearAndAddAnimationToSequenceAction = new JSONStorableActionStringChooser("ClearAndAddAnimationToSequence", ClearAndAddAnimationToSequence, animationSelectionJSON);
		RegisterStringChooserAction(clearAndAddAnimationToSequenceAction);
		clearSequenceAction = new JSONStorableAction("ClearSequence", ClearSequence);
		RegisterAction(clearSequenceAction);
		clearSequenceButFinishCurrentClipAction = new JSONStorableAction("ClearSequenceButFinishCurrentClip", ClearSequenceButFinishCurrentClip);
		RegisterAction(clearSequenceButFinishCurrentClipAction);
		nextClipInSequenceAction = new JSONStorableAction("NextClipInSequence", NextClipInSequence);
		RegisterAction(nextClipInSequenceAction);
		previousClipInSequenceAction = new JSONStorableAction("PreviousClipInSequence", PreviousClipInSequence);
		RegisterAction(previousClipInSequenceAction);
		animationRotationSpeedJSON = new JSONStorableFloat("animationRotationSpeed", _animationRotationSpeed, SyncAnimationRotationSpeed, -100f, 100f, false);
		animationRotationSpeedJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(animationRotationSpeedJSON);
		animationRotationDegressForAction = new JSONStorableFloat("animationRotationDegreesForAction", _animationRotationDegreesForAction, SyncAnimationRotationDegressForAction, -90f, 90f, false);
		animationRotationDegressForAction.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(animationRotationDegressForAction);
		animationRotateAction = new JSONStorableAction("AnimationRotate", AnimationRotate);
		RegisterAction(animationRotateAction);
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (!(t != null))
		{
			return;
		}
		UnityAnimatorControlUI componentInChildren = t.GetComponentInChildren<UnityAnimatorControlUI>(true);
		if (componentInChildren != null)
		{
			if (!isAlt)
			{
				animationSequenceContainer = componentInChildren.sequenceContainer;
			}
			animatorResetAction.RegisterButton(componentInChildren.animatorResetButton, isAlt);
			animatorEnabledJSON.RegisterToggle(componentInChildren.animatorEnabledToggle, isAlt);
			animatorIsPlayingJSON.RegisterIndicator(componentInChildren.animatorIsPlayingIndicator, isAlt);
			animatorPlayAction.RegisterButton(componentInChildren.animatorPlayButton, isAlt);
			animatorPauseAction.RegisterButton(componentInChildren.animatorPauseButton, isAlt);
			animatorSpeedJSON.RegisterSlider(componentInChildren.animatorSpeedSlider, isAlt);
			animationSelectionJSON.RegisterPopup(componentInChildren.animationSelectionPopup, isAlt);
			useCrossFadeJSON.RegisterToggle(componentInChildren.useCrossFadeToggle, isAlt);
			crossFadeTimeJSON.RegisterSlider(componentInChildren.crossFadeTimeSlider, isAlt);
			currentAnimationNameJSON.RegisterText(componentInChildren.currentAnimationNameText, isAlt);
			loopSequenceJSON.RegisterToggle(componentInChildren.loopSequenceToggle, isAlt);
			restartAnimationSequenceAction.RegisterButton(componentInChildren.restartAnimationSequenceButton, isAlt);
			addAnimationToSequenceAction.RegisterButton(componentInChildren.addAnimationToSequenceButton, isAlt);
			clearAndAddAnimationToSequenceAction.RegisterButton(componentInChildren.clearAndAddAnimationToSequenceButton, isAlt);
			clearSequenceAction.RegisterButton(componentInChildren.clearSequenceButton, isAlt);
			nextClipInSequenceAction.RegisterButton(componentInChildren.nextClipInSequenceButton, isAlt);
			previousClipInSequenceAction.RegisterButton(componentInChildren.previousClipInSequenceButton, isAlt);
			animationRotationSpeedJSON.RegisterSlider(componentInChildren.animationRotationSpeedSlider, isAlt);
			animationRotationDegressForAction.RegisterSlider(componentInChildren.animationRotationDegressForActionSlider, isAlt);
			animationRotateAction.RegisterButton(componentInChildren.animationRotateButton, isAlt);
			SyncAnimatorIsPlaying(_animatorIsPlaying);
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

	protected void FixedUpdate()
	{
		if (!(animator != null))
		{
			return;
		}
		animator.enabled = _animatorIsPlaying && (SuperController.singleton == null || !SuperController.singleton.freezeAnimation);
		if (!animator.enabled || !_animatorEnabled)
		{
			return;
		}
		AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
		bool loop = currentAnimatorStateInfo.loop;
		float normalizedTime = currentAnimatorStateInfo.normalizedTime;
		float num = Mathf.Floor(normalizedTime);
		float num2 = Mathf.Floor(normalizedTime);
		float playProgress = normalizedTime - num;
		AnimatorStateInfo nextAnimatorStateInfo = animator.GetNextAnimatorStateInfo(0);
		bool flag = false;
		if (CurrentNode != null)
		{
			if (nextAnimatorStateInfo.IsName(CurrentNode.Value.Name))
			{
				flag = true;
				float normalizedTime2 = nextAnimatorStateInfo.normalizedTime;
				float playProgress2 = normalizedTime2 - Mathf.Floor(normalizedTime2);
				CurrentNode.Value.PlayProgress = playProgress2;
				if (PreviousNode != null)
				{
					PreviousNode.Value.PlayProgress = playProgress;
				}
			}
			else if (loop)
			{
				CurrentNode.Value.PlayProgress = playProgress;
			}
			else if (normalizedTime >= 1f)
			{
				lastAnimatorStateLoopCount = 0f;
				CurrentNode.Value.PlayProgress = 1f;
			}
			else
			{
				CurrentNode.Value.PlayProgress = playProgress;
			}
		}
		if (CurrentNode != null)
		{
			if (num2 > lastAnimatorStateLoopCount && !flag)
			{
				if (CurrentNode.List != null)
				{
					if (CurrentNode.Next != null)
					{
						SetCurrentNode(CurrentNode.Next);
					}
					else if (_loopSequence)
					{
						SetCurrentNode(animationSequence.First);
					}
				}
				else
				{
					SetCurrentNode(animationSequence.First);
				}
			}
		}
		else
		{
			SetCurrentNode(animationSequence.First);
		}
		lastAnimatorStateNormalizedTime = normalizedTime;
		lastAnimatorStateLoopCount = num2;
		if (_animationRotationSpeed != 0f)
		{
			animator.transform.Rotate(0f, _animationRotationSpeed * Time.fixedDeltaTime, 0f);
		}
	}
}
