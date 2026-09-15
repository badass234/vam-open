using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class AnimationPattern : CubicBezierCurve, AnimationTimelineTriggerHandler, TriggerHandler
{
	protected string[] customParamNamesOverride = new string[3] { "curveType", "steps", "triggers" };

	protected Dictionary<string, string> _presetAnimationStepUIDMap;

	protected bool isRestoring;

	public Transform animatedTransform;

	protected JSONStorableBool onJSON;

	protected JSONStorableBool pauseJSON;

	protected JSONStorableBool autoPlayJSON;

	protected JSONStorableBool loopOnceJSON;

	protected JSONStorableFloat speedJSON;

	protected bool _hideCurveUnlessSelected;

	protected JSONStorableBool hideCurveUnlessSelectedJSON;

	protected JSONStorableAction hideAllStepsJSONAction;

	protected JSONStorableAction unhideAllStepsJSONAction;

	protected JSONStorableAction parentAllStepsJSONAction;

	protected JSONStorableAction unparentAllStepsJSONAction;

	[SerializeField]
	protected AnimationStep[] _steps;

	public Atom animationStepPrefab;

	protected bool _autoSyncStepNames = true;

	public JSONStorableBool autoSyncStepNamesJSON;

	public LineDrawer rootLineDrawer;

	public Material rootLineDrawerMaterial;

	protected JSONStorableAction playJSONAction;

	protected JSONStorableAction resetAnimationJSONAction;

	protected JSONStorableAction resetAndPlayJSONAction;

	protected JSONStorableAction pauseJSONAction;

	protected JSONStorableAction unPauseJSONAction;

	protected JSONStorableAction togglePauseJSONAction;

	protected JSONStorableFloat currentTimeJSON;

	protected AnimationStep _currentStep;

	protected AnimationStep _activeStep;

	protected AnimationStep _nextStep;

	protected float _currentStepToNextStepRatio;

	protected bool isPlaying = true;

	protected float _lastTimeStep = -0.1f;

	protected bool _disableTriggers;

	protected bool _autoCounter;

	protected bool _autoReverse;

	protected List<AnimationTimelineTrigger> triggers;

	protected List<AnimationTimelineTrigger> reverseTriggers;

	public RectTransform triggerPrefab;

	public RectTransform triggerActionsPrefab;

	public RectTransform triggerActionMiniPrefab;

	public RectTransform triggerActionDiscretePrefab;

	public RectTransform triggerActionTransitionPrefab;

	public RectTransform triggerActionsParent;

	public ScrollRectContentManager triggerContentManager;

	protected float lastTime;

	public bool hideCurveUnlessSelected
	{
		get
		{
			if (hideCurveUnlessSelectedJSON != null)
			{
				return hideCurveUnlessSelectedJSON.val;
			}
			return _hideCurveUnlessSelected;
		}
		set
		{
			if (hideCurveUnlessSelectedJSON != null)
			{
				hideCurveUnlessSelectedJSON.val = value;
			}
			else if (_hideCurveUnlessSelected != value)
			{
				SyncHideCurveUnlessSelected(value);
			}
		}
	}

	public string uid
	{
		get
		{
			if (containingAtom != null)
			{
				return containingAtom.uid;
			}
			return null;
		}
	}

	public AnimationStep[] steps
	{
		get
		{
			return _steps;
		}
		set
		{
			_steps = value;
			base.points = value;
			SyncStepPositionsNames();
			ResetAnimation();
		}
	}

	protected AnimationStep activeStep
	{
		set
		{
			if (_activeStep != value)
			{
				if (_activeStep != null)
				{
					_activeStep.active = false;
				}
				_activeStep = value;
				if (_activeStep != null)
				{
					_activeStep.active = true;
				}
			}
		}
	}

	protected float currentStepToNextStepRatio
	{
		get
		{
			return _currentStepToNextStepRatio;
		}
		set
		{
			if (_currentStepToNextStepRatio != value)
			{
				_currentStepToNextStepRatio = value;
				if (_currentStep != null)
				{
					_currentStep.stepRatio = value;
				}
			}
		}
	}

	public override string[] GetCustomParamNames()
	{
		return customParamNamesOverride;
	}

	public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		JSONClass jSON = base.GetJSON(includePhysical, includeAppearance, forceStore);
		if (includePhysical || forceStore)
		{
			ResetAnimation();
			if (steps != null)
			{
				needsStore = true;
				JSONArray jSONArray = new JSONArray();
				for (int i = 0; i < steps.Length; i++)
				{
					jSONArray[i] = AtomUidToStoreAtomUid(steps[i].containingAtom.uid);
					SuperController.singleton.SaveAddDependency(steps[i].containingAtom);
				}
				jSON["steps"] = jSONArray;
			}
			if (triggers != null)
			{
				needsStore = true;
				JSONArray jSONArray2 = (JSONArray)(jSON["triggers"] = new JSONArray());
				foreach (AnimationTimelineTrigger trigger in triggers)
				{
					jSONArray2.Add(trigger.GetJSON(base.subScenePrefix));
				}
			}
		}
		return jSON;
	}

	public override void PreRestore(bool restorePhysical, bool restoreAppearance)
	{
		if (restorePhysical && !base.physicalLocked)
		{
			if (!IsCustomPhysicalParamLocked("steps") && (containingAtom == null || !containingAtom.isSubSceneRestore))
			{
				DestroyAllSteps();
			}
			isPlaying = false;
		}
	}

	public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
	{
		isRestoring = true;
		base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
		if (!base.physicalLocked && restorePhysical)
		{
			if (presetAtoms != null)
			{
				_presetAnimationStepUIDMap = new Dictionary<string, string>();
				for (int i = 0; i < presetAtoms.Count; i++)
				{
					JSONClass asObject = presetAtoms[i].AsObject;
					string text = asObject["type"];
					if (!(text == "AnimationStep"))
					{
						continue;
					}
					string key = asObject["id"];
					if (animationStepPrefab != null)
					{
						Transform transform = SuperController.singleton.AddAtom(animationStepPrefab);
						Atom component = transform.GetComponent<Atom>();
						component.RestoreTransform(asObject);
						if (containingAtom != null && component != null)
						{
							component.parentAtom = containingAtom;
						}
						else
						{
							transform.SetParent(base.transform, true);
						}
						string value = component.uid;
						_presetAnimationStepUIDMap.Add(key, value);
						component.Restore(asObject, restorePhysical, restoreAppearance);
						component.LateRestore(asObject, restorePhysical, restoreAppearance);
					}
				}
			}
			if (!IsCustomPhysicalParamLocked("steps"))
			{
				if (jc["steps"] != null)
				{
					JSONArray asArray = jc["steps"].AsArray;
					List<AnimationStep> list = new List<AnimationStep>();
					for (int j = 0; j < asArray.Count; j++)
					{
						string value2;
						if (presetAtoms != null)
						{
							if (!_presetAnimationStepUIDMap.TryGetValue(asArray[j], out value2))
							{
								value2 = asArray[j];
							}
						}
						else
						{
							string text2 = StoredAtomUidToAtomUid(asArray[j]);
							value2 = text2;
						}
						Atom atomByUid = SuperController.singleton.GetAtomByUid(value2);
						if (atomByUid != null)
						{
							if (atomByUid.animationSteps != null)
							{
								AnimationStep animationStep = atomByUid.animationSteps[0];
								animationStep.animationParent = this;
								list.Add(animationStep);
							}
							else
							{
								Debug.LogError("Atom " + value2 + " does not contain an AnimationStep component");
							}
						}
						else
						{
							SuperController.LogError("Atom " + value2 + " referenced by animation pattern " + uid + " does not exist");
						}
					}
					steps = list.ToArray();
				}
				else if (setMissingToDefault)
				{
					steps = new AnimationStep[0];
				}
			}
			isPlaying = autoPlayJSON.val;
		}
		isRestoring = false;
	}

	public override void LateRestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
	{
		isRestoring = true;
		base.LateRestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
		if (!base.physicalLocked && restorePhysical)
		{
			if (!IsCustomPhysicalParamLocked("triggers"))
			{
				if (jc["triggers"] != null)
				{
					if (!base.mergeRestore)
					{
						ClearTriggers();
					}
					JSONArray asArray = jc["triggers"].AsArray;
					if (asArray != null)
					{
						foreach (JSONNode item in asArray)
						{
							JSONClass asObject = item.AsObject;
							if (asObject != null)
							{
								AnimationTimelineTrigger animationTimelineTrigger = AddTriggerInternal();
								animationTimelineTrigger.RestoreFromJSON(asObject, base.subScenePrefix, base.mergeRestore);
							}
						}
					}
				}
				else if (setMissingToDefault)
				{
					ClearTriggers();
				}
			}
			ResetAnimation();
		}
		isRestoring = false;
	}

	public override void Validate()
	{
		base.Validate();
		foreach (AnimationTimelineTrigger trigger in triggers)
		{
			trigger.Validate();
		}
	}

	public override void Remove()
	{
		DestroyAllSteps();
	}

	protected void SyncOn(bool b)
	{
		if (animatedTransform != null)
		{
			MoveProducer component = animatedTransform.GetComponent<MoveProducer>();
			if (component != null)
			{
				component.on = b;
			}
		}
		SetCurrentPositionAndRotation();
	}

	protected void SyncPause(bool b)
	{
		if (b)
		{
			_lastTimeStep = currentTimeJSON.val;
		}
	}

	protected void SyncAutoPlay(bool b)
	{
	}

	protected override void SyncLoop(bool val)
	{
		base.SyncLoop(val);
		RecalculateTimeSteps();
	}

	protected void SyncLoopOnce(bool b)
	{
		if (!loopOnceJSON.val && autoPlayJSON.val)
		{
			isPlaying = true;
		}
	}

	protected void SyncSpeed(float f)
	{
	}

	protected void SyncHideCurveUnlessSelected(bool b)
	{
		_hideCurveUnlessSelected = b;
	}

	public void HideAllSteps()
	{
		AnimationStep[] array = _steps;
		foreach (AnimationStep animationStep in array)
		{
			animationStep.containingAtom.hiddenNoCallback = true;
		}
		SuperController.singleton.SyncHiddenAtoms();
	}

	public void UnhideAllSteps()
	{
		AnimationStep[] array = _steps;
		foreach (AnimationStep animationStep in array)
		{
			animationStep.containingAtom.hiddenNoCallback = false;
		}
		SuperController.singleton.SyncHiddenAtoms();
	}

	public void ParentAllSteps()
	{
		if (!(containingAtom != null))
		{
			return;
		}
		AnimationStep[] array = _steps;
		foreach (AnimationStep animationStep in array)
		{
			if (animationStep.containingAtom != null)
			{
				animationStep.containingAtom.SelectAtomParent(containingAtom);
			}
		}
	}

	public void UnparentAllSteps()
	{
		AnimationStep[] array = _steps;
		foreach (AnimationStep animationStep in array)
		{
			if (animationStep.containingAtom != null)
			{
				animationStep.containingAtom.SelectAtomParent(null);
			}
		}
	}

	public void SyncStepNames()
	{
		if (!_autoSyncStepNames || isRestoring)
		{
			return;
		}
		SuperController.singleton.PauseSyncAtomLists();
		AnimationStep[] array = _steps;
		foreach (AnimationStep animationStep in array)
		{
			string text = containingAtom.uid + "Step" + animationStep.stepNumber;
			if (animationStep.containingAtom.uid != text)
			{
				animationStep.containingAtom.SetUID(SuperController.singleton.GetTempUID());
			}
		}
		AnimationStep[] array2 = _steps;
		foreach (AnimationStep animationStep2 in array2)
		{
			string text2 = containingAtom.uid + "Step" + animationStep2.stepNumber;
			if (animationStep2.containingAtom.uid != text2)
			{
				SuperController.singleton.ReleaseTempUID(animationStep2.containingAtom.uid);
				animationStep2.containingAtom.SetUID(text2);
			}
		}
		SuperController.singleton.ResumeSyncAtomLists();
	}

	protected void SyncAutoSyncStepNames(bool b)
	{
		_autoSyncStepNames = b;
		SyncStepNames();
	}

	protected void DrawRootLine()
	{
		if (rootLineDrawer != null && _draw && _steps.Length > 0)
		{
			rootLineDrawer.SetLinePoints(base.transform.position, _steps[0].transform.position);
			rootLineDrawer.Draw(base.gameObject.layer);
		}
	}

	public AnimationStep CreateStepAtPosition(int position)
	{
		if (animationStepPrefab != null)
		{
			Transform transform = SuperController.singleton.AddAtom(animationStepPrefab);
			AnimationStep componentInChildren = transform.GetComponentInChildren<AnimationStep>();
			Atom component = transform.GetComponent<Atom>();
			if (containingAtom != null && component != null)
			{
				component.parentAtom = containingAtom;
			}
			else
			{
				transform.SetParent(base.transform, true);
			}
			transform.position = base.transform.position;
			transform.rotation = base.transform.rotation;
			if (_steps.Length >= 2)
			{
				if (position == 0)
				{
					if (_loop)
					{
						componentInChildren.point.position = GetPositionFromPoint(_steps.Length - 1, 0.5f);
						componentInChildren.point.rotation = GetRotationFromPoint(_steps.Length - 1, 0.5f);
					}
					else
					{
						Vector3 vector = default(Vector3);
						if (_steps.Length > 1)
						{
							vector = _steps[0].point.position - _steps[1].point.position;
						}
						else
						{
							vector.x = 0f;
							vector.y = 0f;
							vector.z = 0f;
						}
						componentInChildren.point.position = _steps[0].point.position + vector;
						componentInChildren.point.rotation = _steps[0].point.rotation;
					}
				}
				else if (position >= _steps.Length)
				{
					if (_loop)
					{
						componentInChildren.point.position = GetPositionFromPoint(_steps.Length - 1, 0.5f);
						componentInChildren.point.rotation = GetRotationFromPoint(_steps.Length - 1, 0.5f);
					}
					else
					{
						Vector3 vector2 = default(Vector3);
						if (_steps.Length > 1)
						{
							vector2 = _steps[_steps.Length - 1].point.position - _steps[_steps.Length - 2].point.position;
						}
						else
						{
							vector2.x = 0f;
							vector2.y = 0f;
							vector2.z = 0f;
						}
						componentInChildren.point.position = _steps[_steps.Length - 1].point.position + vector2;
						componentInChildren.point.rotation = _steps[_steps.Length - 1].point.rotation;
					}
				}
				else
				{
					componentInChildren.point.position = GetPositionFromPoint(position, 0.5f);
					componentInChildren.point.rotation = GetRotationFromPoint(position, 0.5f);
				}
			}
			else if (_steps.Length == 1)
			{
				Vector3 vector3 = default(Vector3);
				vector3.x = 0.1f;
				vector3.y = 0f;
				vector3.z = 0f;
				componentInChildren.point.position = transform.position + vector3;
				componentInChildren.point.rotation = transform.rotation;
			}
			else
			{
				Vector3 vector4 = default(Vector3);
				vector4.x = 0.1f;
				vector4.y = 0f;
				vector4.z = 0f;
				componentInChildren.point.position = transform.position - vector4;
				componentInChildren.point.rotation = transform.rotation;
			}
			componentInChildren.animationParent = this;
			AddStepAtPosition(componentInChildren, position);
			return componentInChildren;
		}
		return null;
	}

	public AnimationStep CreateStepBeforeStep(AnimationStep step)
	{
		int num = 0;
		AnimationStep[] array = _steps;
		foreach (AnimationStep animationStep in array)
		{
			if (animationStep == step)
			{
				break;
			}
			num++;
		}
		return CreateStepAtPosition(num);
	}

	public AnimationStep CreateStepAfterStep(AnimationStep step)
	{
		int num = 0;
		AnimationStep[] array = _steps;
		foreach (AnimationStep animationStep in array)
		{
			if (animationStep == step)
			{
				num++;
				break;
			}
			num++;
		}
		return CreateStepAtPosition(num);
	}

	public void CreateStepAtEnd()
	{
		CreateStepAtPosition(_steps.Length);
	}

	public void DestroyAllSteps()
	{
		bool autoSyncStepNames = _autoSyncStepNames;
		_autoSyncStepNames = false;
		AnimationStep[] array = _steps;
		foreach (AnimationStep step in array)
		{
			DestroyStep(step);
		}
		steps = new AnimationStep[0];
		_autoSyncStepNames = autoSyncStepNames;
	}

	public void DestroyStep(AnimationStep step)
	{
		RemoveStep(step);
		if (Application.isPlaying)
		{
			if (step.containingAtom != null)
			{
				SuperController.singleton.RemoveAtom(step.containingAtom);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(step.gameObject);
			}
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(step.gameObject);
		}
	}

	public void AddStepAtPosition(AnimationStep step, int position)
	{
		List<AnimationStep> list = new List<AnimationStep>();
		int num = 0;
		bool flag = false;
		bool flag2 = true;
		AnimationStep[] array = _steps;
		foreach (AnimationStep item in array)
		{
			flag2 = false;
			if (num == position)
			{
				flag = true;
				list.Add(step);
			}
			list.Add(item);
			num++;
		}
		if (!flag)
		{
			list.Add(step);
			if (flag2 && animatedTransform != null)
			{
				animatedTransform.position = step.point.position;
				animatedTransform.rotation = step.point.rotation;
			}
		}
		steps = list.ToArray();
	}

	public void SyncStepPositionsNames()
	{
		int num = 1;
		AnimationStep[] array = _steps;
		foreach (AnimationStep animationStep in array)
		{
			animationStep.stepNumber = num;
			num++;
		}
		RecalculateTimeSteps();
		SyncStepNames();
	}

	public void AddStepAtEnd(AnimationStep step)
	{
		AddStepAtPosition(step, _steps.Length);
	}

	public void RemoveStep(AnimationStep step)
	{
		if (_currentStep == step)
		{
			_currentStep = null;
			activeStep = _currentStep;
		}
		List<AnimationStep> list = new List<AnimationStep>();
		AnimationStep[] array = _steps;
		foreach (AnimationStep animationStep in array)
		{
			if (animationStep != step)
			{
				list.Add(animationStep);
			}
		}
		if (base.gameObject != null)
		{
			steps = list.ToArray();
		}
	}

	public void Play()
	{
		pauseJSON.val = false;
		isPlaying = true;
	}

	public void ResetAnimation()
	{
		_disableTriggers = true;
		currentTimeJSON.val = 0f;
		if (_currentStep == null)
		{
			SeekToTimeStep(0f);
		}
		_lastTimeStep = -0.1f;
		_disableTriggers = false;
		isPlaying = autoPlayJSON.val;
		if (steps.Length > 0 && animatedTransform != null && _steps[0] != null && _steps[0].point != null)
		{
			animatedTransform.position = _steps[0].point.position;
			animatedTransform.rotation = _steps[0].point.rotation;
		}
		List<AnimationTimelineTrigger> list = new List<AnimationTimelineTrigger>(triggers);
		list.Sort((AnimationTimelineTrigger t1, AnimationTimelineTrigger t2) => t2.triggerStartTime.CompareTo(t1.triggerStartTime));
		foreach (AnimationTimelineTrigger item in list)
		{
			item.Reset();
		}
	}

	public void ResetAndPlay()
	{
		ResetAnimation();
		Play();
	}

	public void SmoothResetAnimation()
	{
	}

	public void Pause()
	{
		pauseJSON.val = true;
	}

	public void UnPause()
	{
		pauseJSON.val = false;
	}

	public void TogglePause()
	{
		pauseJSON.val = !pauseJSON.val;
	}

	public float GetCurrentTimeCounter()
	{
		if (currentTimeJSON != null)
		{
			return currentTimeJSON.val;
		}
		return 0f;
	}

	public float GetTotalTime()
	{
		if (currentTimeJSON != null)
		{
			return currentTimeJSON.max;
		}
		return 0f;
	}

	public void RecalculateTimeSteps()
	{
		float num = 0f;
		for (int i = 0; i < _steps.Length; i++)
		{
			if (i != 0)
			{
				num += _steps[i].transitionToTime;
			}
			_steps[i].timeStep = num;
		}
		if (_loop && _steps.Length > 0)
		{
			num += _steps[0].transitionToTime;
		}
		if (currentTimeJSON != null)
		{
			currentTimeJSON.max = num;
		}
		foreach (AnimationTimelineTrigger trigger in triggers)
		{
			trigger.ResyncMaxStartAndEndTimes();
		}
	}

	protected void SeekToTimeStep(float timeStep)
	{
		if ((_currentStep == null || timeStep == 0f) && _steps.Length > 0)
		{
			_currentStep = _steps[0];
			if (_steps.Length > 1)
			{
				_nextStep = _steps[1];
			}
			else
			{
				_nextStep = null;
			}
		}
		if (_currentStep != null)
		{
			int num = _currentStep.stepNumber - 1;
			bool flag = false;
			while (!flag)
			{
				flag = true;
				if (timeStep < _currentStep.timeStep)
				{
					if (num > 0)
					{
						_nextStep = _currentStep;
						num--;
						_currentStep = _steps[num];
						flag = false;
					}
				}
				else if (_nextStep != null && timeStep >= _nextStep.timeStep)
				{
					_currentStep = _nextStep;
					num++;
					if (num < _steps.Length)
					{
						_nextStep = _steps[num];
						flag = false;
					}
					else
					{
						_nextStep = null;
					}
				}
			}
			if (_nextStep == null)
			{
				if (_loop)
				{
					float transitionToTime = steps[0].transitionToTime;
					float num2 = timeStep - _currentStep.timeStep;
					currentStepToNextStepRatio = Mathf.Clamp01(num2 / transitionToTime);
				}
				else
				{
					currentStepToNextStepRatio = 0f;
				}
			}
			else
			{
				float num3 = _nextStep.timeStep - _currentStep.timeStep;
				float num4 = timeStep - _currentStep.timeStep;
				currentStepToNextStepRatio = Mathf.Clamp01(num4 / num3);
			}
		}
		activeStep = _currentStep;
		float lastTimeStep = _lastTimeStep;
		_lastTimeStep = timeStep;
		bool disableTriggers = _disableTriggers;
		_disableTriggers = false;
		bool autoCounter = _autoCounter;
		_autoCounter = false;
		SetCurrentPositionAndRotation();
		if (disableTriggers)
		{
			return;
		}
		bool flag2 = (autoCounter && _autoReverse) || (!autoCounter && lastTimeStep > timeStep);
		if (flag2)
		{
			foreach (AnimationTimelineTrigger reverseTrigger in reverseTriggers)
			{
				reverseTrigger.Update(flag2, lastTimeStep);
			}
			return;
		}
		foreach (AnimationTimelineTrigger trigger in triggers)
		{
			trigger.Update(flag2, lastTimeStep);
		}
	}

	protected void SetCurrentPositionAndRotation()
	{
		if (!onJSON.val || !(_currentStep != null))
		{
			return;
		}
		if (_nextStep == null)
		{
			if (_loop)
			{
				float t = _currentStep.curve.Evaluate(_currentStepToNextStepRatio);
				animatedTransform.position = GetPositionFromPoint(_currentStep.stepNumber - 1, t);
				animatedTransform.rotation = GetRotationFromPoint(_currentStep.stepNumber - 1, t);
			}
			else
			{
				animatedTransform.position = _currentStep.point.position;
				animatedTransform.rotation = _currentStep.point.rotation;
			}
		}
		else
		{
			float t2 = _currentStep.curve.Evaluate(_currentStepToNextStepRatio);
			animatedTransform.position = GetPositionFromPoint(_currentStep.stepNumber - 1, t2);
			animatedTransform.rotation = GetRotationFromPoint(_currentStep.stepNumber - 1, t2);
		}
	}

	protected void IncrementPlaybackCounter(float increment)
	{
		if (!isPlaying || !onJSON.val || pauseJSON.val || ((bool)SuperController.singleton && SuperController.singleton.freezeAnimation))
		{
			return;
		}
		_autoCounter = true;
		_autoReverse = speedJSON.val < 0f;
		float num = currentTimeJSON.val + increment * speedJSON.val;
		if (num <= 0f)
		{
			if (_loop)
			{
				if (loopOnceJSON.val)
				{
					currentTimeJSON.val = currentTimeJSON.max;
					isPlaying = false;
				}
				else
				{
					currentTimeJSON.val = currentTimeJSON.max + num;
				}
			}
			else
			{
				currentTimeJSON.val = currentTimeJSON.min;
				isPlaying = false;
			}
		}
		else if (num >= currentTimeJSON.max)
		{
			if (_loop)
			{
				if (loopOnceJSON.val)
				{
					currentTimeJSON.val = currentTimeJSON.min;
					isPlaying = false;
				}
				else
				{
					currentTimeJSON.val = num - currentTimeJSON.val;
				}
			}
			else
			{
				currentTimeJSON.val = currentTimeJSON.max;
				isPlaying = false;
			}
		}
		else
		{
			currentTimeJSON.val = num;
		}
	}

	public void ClearTriggers()
	{
		List<Trigger> list = new List<Trigger>();
		foreach (AnimationTimelineTrigger trigger in triggers)
		{
			list.Add(trigger);
		}
		foreach (Trigger item in list)
		{
			item.Remove();
		}
	}

	protected void CreateTriggerUI(AnimationTimelineTrigger att, int index)
	{
		if (triggerContentManager != null)
		{
			if (triggerPrefab != null)
			{
				RectTransform rectTransform = UnityEngine.Object.Instantiate(triggerPrefab);
				triggerContentManager.AddItem(rectTransform, index);
				att.triggerPanel = rectTransform;
			}
			else
			{
				Debug.LogError("Attempted to make TriggerUI when prefab was not set");
			}
		}
	}

	protected AnimationTimelineTrigger AddTriggerInternal(int index = -1)
	{
		AnimationTimelineTrigger animationTimelineTrigger = new AnimationTimelineTrigger();
		animationTimelineTrigger.timeLineHandler = this;
		animationTimelineTrigger.handler = this;
		animationTimelineTrigger.doActionsInReverse = true;
		if (index == -1)
		{
			triggers.Add(animationTimelineTrigger);
		}
		else
		{
			triggers.Insert(index, animationTimelineTrigger);
		}
		reverseTriggers = new List<AnimationTimelineTrigger>(triggers);
		reverseTriggers.Reverse();
		CreateTriggerUI(animationTimelineTrigger, index);
		animationTimelineTrigger.InitTriggerUI();
		animationTimelineTrigger.triggerActionsParent = triggerActionsParent;
		return animationTimelineTrigger;
	}

	public void AddTrigger()
	{
		AddTriggerInternal();
	}

	public RectTransform CreateTriggerActionsUI()
	{
		RectTransform result = null;
		if (triggerActionsPrefab != null)
		{
			result = UnityEngine.Object.Instantiate(triggerActionsPrefab);
		}
		else
		{
			Debug.LogError("Attempted to make TriggerActionsUI when prefab was not set");
		}
		return result;
	}

	public RectTransform CreateTriggerActionMiniUI()
	{
		RectTransform result = null;
		if (triggerActionMiniPrefab != null)
		{
			result = UnityEngine.Object.Instantiate(triggerActionMiniPrefab);
		}
		else
		{
			Debug.LogError("Attempted to make TriggerActionMiniUI when prefab was not set");
		}
		return result;
	}

	public RectTransform CreateTriggerActionDiscreteUI()
	{
		RectTransform result = null;
		if (triggerActionDiscretePrefab != null)
		{
			result = UnityEngine.Object.Instantiate(triggerActionDiscretePrefab);
		}
		else
		{
			Debug.LogError("Attempted to make TriggerActionDiscreteUI when prefab was not set");
		}
		return result;
	}

	public RectTransform CreateTriggerActionTransitionUI()
	{
		RectTransform result = null;
		if (triggerActionTransitionPrefab != null)
		{
			result = UnityEngine.Object.Instantiate(triggerActionTransitionPrefab);
		}
		else
		{
			Debug.LogError("Attempted to make TriggerActionTransitionUI when prefab was not set");
		}
		return result;
	}

	public void RemoveTriggerActionUI(RectTransform rt)
	{
		if (rt != null)
		{
			UnityEngine.Object.Destroy(rt.gameObject);
		}
	}

	public void RemoveTrigger(Trigger trigger)
	{
		if (triggers.Remove(trigger as AnimationTimelineTrigger))
		{
			reverseTriggers.Remove(trigger as AnimationTimelineTrigger);
			if (trigger.triggerActionsPanel != null)
			{
				UnityEngine.Object.Destroy(trigger.triggerActionsPanel.gameObject);
			}
			if (!(trigger.triggerPanel != null))
			{
				return;
			}
			if (triggerContentManager != null)
			{
				RectTransform component = trigger.triggerPanel.GetComponent<RectTransform>();
				if (component != null)
				{
					triggerContentManager.RemoveItem(component);
				}
			}
			UnityEngine.Object.Destroy(trigger.triggerPanel.gameObject);
		}
		else
		{
			Debug.LogError("Could not remove trigger " + trigger.displayName);
		}
	}

	public void DuplicateTrigger(Trigger trigger)
	{
		AnimationTimelineTrigger animationTimelineTrigger = trigger as AnimationTimelineTrigger;
		if (animationTimelineTrigger != null)
		{
			int num = triggers.IndexOf(animationTimelineTrigger);
			if (num != -1)
			{
				JSONClass jSON = animationTimelineTrigger.GetJSON();
				AnimationTimelineTrigger animationTimelineTrigger2 = AddTriggerInternal(num + 1);
				animationTimelineTrigger2.RestoreFromJSON(jSON);
			}
		}
	}

	protected void OnAtomRename(string fromuid, string touid)
	{
		foreach (AnimationTimelineTrigger trigger in triggers)
		{
			trigger.SyncAtomNames();
		}
		if (touid == uid)
		{
			SyncStepNames();
		}
	}

	protected override void Init()
	{
		base.Init();
		triggers = new List<AnimationTimelineTrigger>();
		reverseTriggers = new List<AnimationTimelineTrigger>();
		isPlaying = true;
		if (rootLineDrawerMaterial != null)
		{
			rootLineDrawer = new LineDrawer(rootLineDrawerMaterial);
		}
		onJSON = new JSONStorableBool("on", true, SyncOn);
		onJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(onJSON);
		pauseJSON = new JSONStorableBool("pause", false, SyncPause);
		pauseJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(pauseJSON);
		autoPlayJSON = new JSONStorableBool("autoPlay", true, SyncAutoPlay);
		autoPlayJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(autoPlayJSON);
		loopOnceJSON = new JSONStorableBool("loopOnce", false, SyncLoopOnce);
		loopOnceJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(loopOnceJSON);
		speedJSON = new JSONStorableFloat("speed", 1f, SyncSpeed, -10f, 10f, false);
		speedJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(speedJSON);
		currentTimeJSON = new JSONStorableFloat("currentTime", 0f, SeekToTimeStep, 0f, 0f);
		currentTimeJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(currentTimeJSON);
		autoSyncStepNamesJSON = new JSONStorableBool("autoSyncStepNames", _autoSyncStepNames, SyncAutoSyncStepNames);
		autoSyncStepNamesJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(autoSyncStepNamesJSON);
		hideCurveUnlessSelectedJSON = new JSONStorableBool("hideCurveUnlessSelected", _hideCurveUnlessSelected, SyncHideCurveUnlessSelected);
		hideCurveUnlessSelectedJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(hideCurveUnlessSelectedJSON);
		playJSONAction = new JSONStorableAction("Play", Play);
		RegisterAction(playJSONAction);
		resetAnimationJSONAction = new JSONStorableAction("ResetAnimation", ResetAnimation);
		RegisterAction(resetAnimationJSONAction);
		resetAndPlayJSONAction = new JSONStorableAction("ResetAndPlay", ResetAndPlay);
		RegisterAction(resetAndPlayJSONAction);
		pauseJSONAction = new JSONStorableAction("Pause", Pause);
		RegisterAction(pauseJSONAction);
		unPauseJSONAction = new JSONStorableAction("UnPause", UnPause);
		RegisterAction(unPauseJSONAction);
		togglePauseJSONAction = new JSONStorableAction("TogglePause", TogglePause);
		RegisterAction(togglePauseJSONAction);
		hideAllStepsJSONAction = new JSONStorableAction("HideAllSteps", HideAllSteps);
		RegisterAction(hideAllStepsJSONAction);
		unhideAllStepsJSONAction = new JSONStorableAction("UnhideAllSteps", UnhideAllSteps);
		RegisterAction(unhideAllStepsJSONAction);
		parentAllStepsJSONAction = new JSONStorableAction("ParentAllSteps", ParentAllSteps);
		RegisterAction(parentAllStepsJSONAction);
		unparentAllStepsJSONAction = new JSONStorableAction("UnparentAllSteps", UnparentAllSteps);
		RegisterAction(unparentAllStepsJSONAction);
		if (SuperController.singleton != null)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Combine(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomRename));
		}
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		AnimationPatternUI componentInChildren = UITransform.GetComponentInChildren<AnimationPatternUI>(true);
		if (componentInChildren != null)
		{
			onJSON.toggle = componentInChildren.onToggle;
			autoPlayJSON.toggle = componentInChildren.autoPlayToggle;
			pauseJSON.toggle = componentInChildren.pauseToggle;
			loopJSON.toggle = componentInChildren.loopToggle;
			loopOnceJSON.toggle = componentInChildren.loopOnceToggle;
			curveTypeJSON.popup = componentInChildren.curveTypeSelector;
			speedJSON.slider = componentInChildren.speedSlider;
			autoSyncStepNamesJSON.toggle = componentInChildren.autoSyncStepNamesToggle;
			hideCurveUnlessSelectedJSON.toggle = componentInChildren.hideCurveUnlessSelectedToggle;
			currentTimeJSON.slider = componentInChildren.currentTimeSlider;
			triggerContentManager = componentInChildren.triggerContentManager;
			triggerActionsParent = componentInChildren.triggerActionsParent;
			for (int i = 0; i < triggers.Count; i++)
			{
				AnimationTimelineTrigger animationTimelineTrigger = triggers[i];
				CreateTriggerUI(animationTimelineTrigger, i);
				animationTimelineTrigger.InitTriggerUI();
				animationTimelineTrigger.triggerActionsParent = triggerActionsParent;
			}
			if (componentInChildren.createStepAtEndButton != null)
			{
				componentInChildren.createStepAtEndButton.onClick.AddListener(CreateStepAtEnd);
			}
			resetAnimationJSONAction.button = componentInChildren.resetAnimationButton;
			playJSONAction.button = componentInChildren.playButton;
			if (componentInChildren.addTriggerButton != null)
			{
				componentInChildren.addTriggerButton.onClick.AddListener(AddTrigger);
			}
			if (componentInChildren.clearAllTriggersButton != null)
			{
				componentInChildren.clearAllTriggersButton.onClick.AddListener(ClearTriggers);
			}
			hideAllStepsJSONAction.button = componentInChildren.hideAllStepsButton;
			unhideAllStepsJSONAction.button = componentInChildren.unhideAllStepsButton;
			parentAllStepsJSONAction.button = componentInChildren.parentAllStepsButton;
			unparentAllStepsJSONAction.button = componentInChildren.unparentAllStepsButton;
		}
	}

	public override void InitUIAlt()
	{
		if (!(UITransformAlt != null))
		{
			return;
		}
		AnimationPatternUI componentInChildren = UITransformAlt.GetComponentInChildren<AnimationPatternUI>(true);
		if (componentInChildren != null)
		{
			onJSON.toggleAlt = componentInChildren.onToggle;
			autoPlayJSON.toggleAlt = componentInChildren.autoPlayToggle;
			pauseJSON.toggleAlt = componentInChildren.pauseToggle;
			loopJSON.toggleAlt = componentInChildren.loopToggle;
			loopOnceJSON.toggleAlt = componentInChildren.loopOnceToggle;
			curveTypeJSON.popupAlt = componentInChildren.curveTypeSelector;
			speedJSON.sliderAlt = componentInChildren.speedSlider;
			autoSyncStepNamesJSON.toggleAlt = componentInChildren.autoSyncStepNamesToggle;
			hideCurveUnlessSelectedJSON.toggleAlt = componentInChildren.hideCurveUnlessSelectedToggle;
			currentTimeJSON.sliderAlt = componentInChildren.currentTimeSlider;
			if (componentInChildren.createStepAtEndButton != null)
			{
				componentInChildren.createStepAtEndButton.onClick.AddListener(CreateStepAtEnd);
			}
			resetAnimationJSONAction.button = componentInChildren.resetAnimationButton;
			playJSONAction.button = componentInChildren.playButton;
			if (componentInChildren.addTriggerButton != null)
			{
				componentInChildren.addTriggerButton.onClick.AddListener(AddTrigger);
			}
			if (componentInChildren.clearAllTriggersButton != null)
			{
				componentInChildren.clearAllTriggersButton.onClick.AddListener(ClearTriggers);
			}
			hideAllStepsJSONAction.buttonAlt = componentInChildren.hideAllStepsButton;
			unhideAllStepsJSONAction.buttonAlt = componentInChildren.unhideAllStepsButton;
			parentAllStepsJSONAction.buttonAlt = componentInChildren.parentAllStepsButton;
			unparentAllStepsJSONAction.buttonAlt = componentInChildren.unparentAllStepsButton;
		}
	}

	protected void FixedUpdate()
	{
		if (Application.isPlaying)
		{
			float fixedTime = Time.fixedTime;
			if (lastTime == 0f)
			{
				lastTime = fixedTime;
			}
			float increment = fixedTime - lastTime;
			lastTime = fixedTime;
			IncrementPlaybackCounter(increment);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_autoSyncStepNames = false;
		if (SuperController.singleton != null)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Remove(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomRename));
		}
	}

	protected new void Update()
	{
		base.Update();
		if (!Application.isPlaying)
		{
			return;
		}
		float time = Time.time;
		if (lastTime == 0f)
		{
			lastTime = time;
		}
		float increment = time - lastTime;
		lastTime = time;
		IncrementPlaybackCounter(increment);
		foreach (AnimationTimelineTrigger trigger in triggers)
		{
			trigger.Update();
		}
		SetCurrentPositionAndRotation();
		DrawRootLine();
	}
}
