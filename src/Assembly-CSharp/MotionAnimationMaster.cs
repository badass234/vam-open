using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class MotionAnimationMaster : JSONStorableTriggerHandler, AnimationTimelineTriggerHandler
{
	protected string[] customParamNames = new string[3] { "recordedLength", "triggers", "audioSourceControl" };

	public bool isSceneMasterController;

	protected HashSet<MotionAnimationControl> controllers;

	[SerializeField]
	protected bool _linkToAudioSourceControl;

	protected JSONStorableBool linkToAudioSourceControlJSON;

	protected float _audioSourceTimeOffset;

	protected JSONStorableFloat audioSourceTimeOffsetJSON;

	public UIPopup audioSourceControlAtomSelectionPopup;

	public UIPopup audioSourceControlSelectionPopup;

	protected string audioSourceControlAtomUID;

	[SerializeField]
	protected AudioSourceControl _audioSourceControl;

	public Slider playbackCounterSlider;

	protected float _playbackCounter;

	protected float _lastPlaybackCounter;

	protected JSONStorableFloat playbackCounterJSON;

	public Slider startTimestepSlider;

	protected float _startTimestep;

	protected JSONStorableFloat startTimestepJSON;

	public Slider stopTimestepSlider;

	protected float _stopTimestep;

	protected JSONStorableFloat stopTimestepJSON;

	protected JSONStorableAction ResetAnimationAction;

	protected bool _disableTriggers;

	public bool freeze;

	protected JSONStorableFloat loopbackTimeJSON;

	protected float _loopbackCounter;

	public Slider loopbackTimeSlider;

	[SerializeField]
	protected float _loopbackTime = 1f;

	protected JSONStorableBool autoPlayJSON;

	protected bool _autoPlay = true;

	protected JSONStorableBool loopJSON;

	public Toggle loopToggle;

	[SerializeField]
	protected bool _loop;

	protected JSONStorableBool autoRecordStopJSON;

	public Toggle autoRecordStopToggle;

	protected bool _ignoreAutoRecordStop;

	[SerializeField]
	protected bool _autoRecordStop = true;

	protected JSONStorableFloat playbackSpeedJSON;

	public Slider playbackSpeedSlider;

	[SerializeField]
	protected float _playbackSpeed = 1f;

	protected JSONStorableBool showRecordPathsJSON;

	public Toggle showRecordPathsToggle;

	[SerializeField]
	protected bool _showRecordPaths;

	protected JSONStorableBool showStartMarkersJSON;

	public Toggle showStartMarkersToggle;

	[SerializeField]
	protected bool _showStartMarkers;

	protected List<AnimationTimelineTrigger> triggers;

	protected List<AnimationTimelineTrigger> reverseTriggers;

	public RectTransform triggerActionsParent;

	public RectTransform triggerPrefab;

	public ScrollRectContentManager triggerContentManager;

	public Button clearAllTriggersButton;

	protected GameObject advancedPanel;

	protected bool advancedPanelOpen;

	public Button addTriggerButton;

	protected HashSet<AnimationTimelineTrigger> _selectedTriggers;

	protected JSONStorableFloat triggerSelectFromTimeJSON;

	protected JSONStorableFloat triggerSelectToTimeJSON;

	protected JSONStorableAction SelectTriggersInTimeRangeAction;

	protected JSONStorableAction ClearSelectedTriggersAction;

	protected JSONStorableFloat triggerTimeAdjustmentJSON;

	protected JSONStorableAction AdjustTimeOfSelectedTriggersAction;

	protected JSONStorableAction SortTriggersByStartTimeAction;

	protected JSONStorableFloat triggerPasteToTimeJSON;

	protected JSONStorableAction CopySelectedTriggersAndPasteToTimeAction;

	protected float _lastRecordTime;

	public float recordInterval = 0.02f;

	protected bool _isRecording;

	public GameObject activeWhilePlaying;

	public GameObject activeWhileStopped;

	protected bool _isPlaying;

	protected bool _isLoopingBack;

	protected float _recordedLength;

	protected JSONStorableAction ClearAllAnimationAction;

	protected JSONStorableAction SelectControllersArmedForRecordAction;

	protected JSONStorableAction ArmAllControlledControllersForRecordAction;

	protected JSONStorableAction StartRecordModeAction;

	protected JSONStorableAction StartRecordAction;

	protected JSONStorableAction StopRecordAction;

	protected JSONStorableAction StopRecordModeAction;

	protected JSONStorableAction StartPlaybackAction;

	protected JSONStorableAction StopPlaybackAction;

	protected JSONStorableAction TrimAnimationAction;

	protected JSONStorableAction SetToDesiredLengthAction;

	protected float _desiredLength = 60f;

	protected JSONStorableFloat desiredLengthJSON;

	protected JSONStorableAction CopyFromSceneMasterAction;

	protected JSONStorableAction CopyToSceneMasterAction;

	protected JSONStorableAction SeekToBeginningAction;

	public bool linkToAudioSourceControl
	{
		get
		{
			return _linkToAudioSourceControl;
		}
		set
		{
			if (linkToAudioSourceControlJSON != null)
			{
				linkToAudioSourceControlJSON.val = value;
			}
			else
			{
				SyncLinkToAudioSourceControl(value);
			}
		}
	}

	public float audioSourceTimeOffset
	{
		get
		{
			return _audioSourceTimeOffset;
		}
		set
		{
			if (audioSourceTimeOffsetJSON != null)
			{
				audioSourceTimeOffsetJSON.val = value;
			}
			else
			{
				SyncAudioSourceTimeOffset(value);
			}
		}
	}

	public AudioSourceControl audioSourceControl
	{
		get
		{
			return _audioSourceControl;
		}
		set
		{
			_audioSourceControl = value;
		}
	}

	public float playbackCounter
	{
		get
		{
			return _playbackCounter;
		}
		set
		{
			if (playbackCounterJSON != null)
			{
				playbackCounterJSON.val = value;
			}
			else
			{
				SyncPlaybackCounter(value);
			}
		}
	}

	public float loopbackTime
	{
		get
		{
			return _loopbackTime;
		}
		set
		{
			if (loopbackTimeJSON != null)
			{
				loopbackTimeJSON.val = value;
			}
			else if (_loopbackTime != value)
			{
				_loopbackTime = value;
				if (loopbackTimeSlider != null)
				{
					loopbackTimeSlider.value = value;
				}
			}
		}
	}

	public bool autoPlay
	{
		get
		{
			return _autoPlay;
		}
		set
		{
			if (autoPlayJSON != null)
			{
				autoPlayJSON.val = value;
			}
			else if (_autoPlay != value)
			{
				SyncAutoPlay(value);
			}
		}
	}

	public bool loop
	{
		get
		{
			return _loop;
		}
		set
		{
			if (loopJSON != null)
			{
				loopJSON.val = value;
			}
			else if (_loop != value)
			{
				_loop = value;
				if (loopToggle != null)
				{
					loopToggle.isOn = value;
				}
			}
		}
	}

	public bool autoRecordStop
	{
		get
		{
			return _autoRecordStop;
		}
		set
		{
			if (autoRecordStopJSON != null)
			{
				autoRecordStopJSON.val = value;
			}
			else if (_autoRecordStop != value)
			{
				_autoRecordStop = value;
				if (autoRecordStopToggle != null)
				{
					autoRecordStopToggle.isOn = value;
				}
			}
		}
	}

	public float playbackSpeed
	{
		get
		{
			return _playbackSpeed;
		}
		set
		{
			if (playbackSpeedJSON != null)
			{
				playbackSpeedJSON.val = value;
			}
			else if (_playbackSpeed != value)
			{
				SyncPlaybackSpeed(value);
				if (playbackSpeedSlider != null)
				{
					playbackSpeedSlider.value = value;
				}
			}
		}
	}

	public bool showRecordPaths
	{
		get
		{
			return _showRecordPaths;
		}
		set
		{
			if (showRecordPathsJSON != null)
			{
				showRecordPathsJSON.val = value;
			}
			else if (_showRecordPaths != value)
			{
				if (showRecordPathsToggle != null)
				{
					showRecordPathsToggle.isOn = value;
				}
				SyncShowRecordPaths(value);
			}
		}
	}

	public bool showStartMarkers
	{
		get
		{
			return _showStartMarkers;
		}
		set
		{
			if (showStartMarkersJSON != null)
			{
				showStartMarkersJSON.val = value;
			}
			else if (_showStartMarkers != value)
			{
				_showStartMarkers = value;
				if (showStartMarkersToggle != null)
				{
					showStartMarkersToggle.isOn = value;
				}
			}
		}
	}

	public IEnumerable<AnimationTimelineTrigger> selectedTriggers => _selectedTriggers;

	public float triggerSelectFromTime
	{
		get
		{
			return triggerSelectFromTimeJSON.val;
		}
		set
		{
			triggerSelectFromTimeJSON.val = value;
		}
	}

	public float triggerSelectToTime
	{
		get
		{
			return triggerSelectToTimeJSON.val;
		}
		set
		{
			triggerSelectToTimeJSON.val = value;
		}
	}

	public float triggerPasteToTime
	{
		get
		{
			return triggerPasteToTimeJSON.val;
		}
		set
		{
			triggerPasteToTimeJSON.val = value;
		}
	}

	public bool isRecording => _isRecording;

	protected float recordedLength
	{
		get
		{
			return _recordedLength;
		}
		set
		{
			_recordedLength = value;
			if (_recordedLength < 0f)
			{
				_recordedLength = 0f;
			}
			playbackCounterJSON.max = _recordedLength;
			startTimestepJSON.max = _recordedLength;
			stopTimestepJSON.max = _recordedLength;
			stopTimestepJSON.val = _recordedLength;
			triggerSelectFromTimeJSON.max = _recordedLength;
			triggerSelectToTimeJSON.max = _recordedLength;
			triggerPasteToTimeJSON.max = _recordedLength;
			foreach (AnimationTimelineTrigger trigger in triggers)
			{
				trigger.ResyncMaxStartAndEndTimes();
			}
		}
	}

	public float totalTime => _recordedLength;

	public override string[] GetCustomParamNames()
	{
		return customParamNames;
	}

	public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		JSONClass jSON = base.GetJSON(includePhysical, includeAppearance, forceStore);
		if (includePhysical || forceStore)
		{
			if (_recordedLength > 0f)
			{
				needsStore = true;
				jSON["recordedLength"].AsFloat = _recordedLength;
			}
			if (triggers != null)
			{
				needsStore = true;
				JSONArray jSONArray = (JSONArray)(jSON["triggers"] = new JSONArray());
				foreach (AnimationTimelineTrigger trigger in triggers)
				{
					jSONArray.Add(trigger.GetJSON(base.subScenePrefix));
				}
			}
			if (_audioSourceControl != null && _audioSourceControl.containingAtom != null)
			{
				string text = AtomUidToStoreAtomUid(_audioSourceControl.containingAtom.uid);
				if (text != null)
				{
					needsStore = true;
					jSON["audioSourceControl"] = text + ":" + _audioSourceControl.name;
				}
				else
				{
					SuperController.LogError(string.Concat("Warning: AudioSourceControl in atom ", containingAtom, " uses audio source control in atom ", _audioSourceControl.containingAtom.uid, " that is not in subscene and cannot be saved"));
				}
			}
		}
		return jSON;
	}

	public override void PreRestore(bool restorePhysical, bool restoreAppearance)
	{
		if (restorePhysical && !base.physicalLocked)
		{
			if (!base.mergeRestore && !IsCustomPhysicalParamLocked("triggers"))
			{
				ClearTriggers();
			}
			if (!IsCustomPhysicalParamLocked("recordedLength"))
			{
				StopPlayback();
				_playbackCounter = 0f;
			}
		}
	}

	public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
	{
		base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
		if (base.physicalLocked || !restorePhysical)
		{
			return;
		}
		if (!IsCustomPhysicalParamLocked("recordedLength"))
		{
			if (jc["recordedLength"] != null)
			{
				recordedLength = jc["recordedLength"].AsFloat;
				if (_recordedLength > 0f)
				{
					InternalSetPlaybackCounter(_startTimestep);
					if (_autoPlay && (!isSceneMasterController || SuperController.singleton == null || SuperController.singleton.gameMode == SuperController.GameMode.Play))
					{
						StartPlayback();
					}
				}
			}
			else if (setMissingToDefault)
			{
				recordedLength = 0f;
			}
		}
		if (!IsCustomPhysicalParamLocked("audioSourceControl"))
		{
			if (jc["audioSourceControl"] != null)
			{
				string text = StoredAtomUidToAtomUid(jc["audioSourceControl"]);
				SetAudioSourceControl(text);
			}
			else if (setMissingToDefault)
			{
				SetAudioSourceControl(string.Empty);
			}
		}
	}

	public override void LateRestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
	{
		base.LateRestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
		if (base.physicalLocked || !restorePhysical || IsCustomPhysicalParamLocked("triggers"))
		{
			return;
		}
		if (jc["triggers"] != null)
		{
			if (!base.mergeRestore)
			{
				ClearTriggers();
			}
			JSONArray asArray = jc["triggers"].AsArray;
			if (!(asArray != null))
			{
				return;
			}
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
				return;
			}
		}
		if (setMissingToDefault && !base.mergeRestore)
		{
			ClearTriggers();
		}
	}

	public override void Validate()
	{
		base.Validate();
		foreach (AnimationTimelineTrigger trigger in triggers)
		{
			trigger.Validate();
		}
	}

	public void RegisterAnimationControl(MotionAnimationControl mac)
	{
		if (controllers.Add(mac))
		{
			if (mac.animationMaster != null)
			{
				mac.animationMaster.DeregisterAnimationControl(mac);
			}
			mac.animationMaster = this;
		}
	}

	public void DeregisterAnimationControl(MotionAnimationControl mac)
	{
		if (controllers.Remove(mac))
		{
			mac.animationMaster = null;
		}
	}

	protected void SyncLinkToAudioSourceControl(bool b)
	{
		_linkToAudioSourceControl = b;
	}

	protected void SyncAudioSourceTimeOffset(float f)
	{
		_audioSourceTimeOffset = f;
	}

	protected virtual void SetAudioSourceControlAtomNames()
	{
		if (!(audioSourceControlAtomSelectionPopup != null) || !(SuperController.singleton != null))
		{
			return;
		}
		List<string> atomUIDsWithAudioSourceControls = SuperController.singleton.GetAtomUIDsWithAudioSourceControls();
		if (atomUIDsWithAudioSourceControls == null)
		{
			audioSourceControlAtomSelectionPopup.numPopupValues = 1;
			audioSourceControlAtomSelectionPopup.setPopupValue(0, "None");
			return;
		}
		audioSourceControlAtomSelectionPopup.numPopupValues = atomUIDsWithAudioSourceControls.Count + 1;
		audioSourceControlAtomSelectionPopup.setPopupValue(0, "None");
		for (int i = 0; i < atomUIDsWithAudioSourceControls.Count; i++)
		{
			audioSourceControlAtomSelectionPopup.setPopupValue(i + 1, atomUIDsWithAudioSourceControls[i]);
		}
	}

	protected virtual void onAudioSourceControlNamesChanged(List<string> rcNames)
	{
		if (!(audioSourceControlSelectionPopup != null))
		{
			return;
		}
		if (rcNames == null)
		{
			audioSourceControlSelectionPopup.numPopupValues = 1;
			audioSourceControlSelectionPopup.setPopupValue(0, "None");
			return;
		}
		audioSourceControlSelectionPopup.numPopupValues = rcNames.Count + 1;
		audioSourceControlSelectionPopup.setPopupValue(0, "None");
		for (int i = 0; i < rcNames.Count; i++)
		{
			audioSourceControlSelectionPopup.setPopupValue(i + 1, rcNames[i]);
		}
	}

	public virtual void SetAudioSourceControlAtom(string atomUID)
	{
		if (!(SuperController.singleton != null))
		{
			return;
		}
		Atom atomByUid = SuperController.singleton.GetAtomByUid(atomUID);
		if (atomByUid != null)
		{
			audioSourceControlAtomUID = atomUID;
			List<string> audioSourceControlNamesInAtom = SuperController.singleton.GetAudioSourceControlNamesInAtom(audioSourceControlAtomUID);
			onAudioSourceControlNamesChanged(audioSourceControlNamesInAtom);
			if (audioSourceControlSelectionPopup != null)
			{
				audioSourceControlSelectionPopup.currentValue = "None";
			}
		}
		else
		{
			onAudioSourceControlNamesChanged(null);
		}
	}

	public virtual void SetAudioSourceControlObject(string objectName)
	{
		if (audioSourceControlAtomUID != null && SuperController.singleton != null)
		{
			audioSourceControl = SuperController.singleton.AudioSourceControlrNameToAudioSourceControl(audioSourceControlAtomUID + ":" + objectName);
		}
	}

	public virtual void SetAudioSourceControl(string controllerName)
	{
		if (!(SuperController.singleton != null))
		{
			return;
		}
		AudioSourceControl audioSourceControl = SuperController.singleton.AudioSourceControlrNameToAudioSourceControl(controllerName);
		if (audioSourceControl != null)
		{
			if (audioSourceControlAtomSelectionPopup != null && audioSourceControl.containingAtom != null)
			{
				audioSourceControlAtomSelectionPopup.currentValue = audioSourceControl.containingAtom.uid;
			}
			if (audioSourceControlSelectionPopup != null)
			{
				audioSourceControlSelectionPopup.currentValue = audioSourceControl.name;
			}
		}
		else
		{
			if (audioSourceControlAtomSelectionPopup != null)
			{
				audioSourceControlAtomSelectionPopup.currentValue = "None";
			}
			if (audioSourceControlSelectionPopup != null)
			{
				audioSourceControlSelectionPopup.currentValue = "None";
			}
		}
		this.audioSourceControl = audioSourceControl;
	}

	protected void SetAudioSourceTime(float f)
	{
		float num = f - _audioSourceTimeOffset;
		if (num < 0f)
		{
			audioSourceControl.Stop();
		}
		else if (num > audioSourceControl.audioSource.clip.length)
		{
			audioSourceControl.Stop();
		}
		else
		{
			audioSourceControl.audioSource.time = num;
		}
	}

	public float GetTotalTime()
	{
		return totalTime;
	}

	public float GetCurrentTimeCounter()
	{
		return _playbackCounter;
	}

	protected void SyncPlaybackCounter(float f)
	{
		if (!_isRecording)
		{
			if (_linkToAudioSourceControl && audioSourceControl != null && audioSourceControl.audioSource.clip != null)
			{
				SetAudioSourceTime(f);
			}
			InternalSetPlaybackCounter(f, true);
		}
	}

	protected void SyncStartTimestep(float f)
	{
		_startTimestep = f;
	}

	protected void SyncStopTimestep(float f)
	{
		_stopTimestep = f;
	}

	protected void SetLoopback()
	{
		PlaybackStep();
		_isLoopingBack = true;
		_loopbackCounter = 0f;
	}

	public void ResetAnimation()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.ResetSimulation(5, "MotionAnimation ResetAnimation", true);
		}
		if (_linkToAudioSourceControl && audioSourceControl != null && audioSourceControl.audioSource.clip != null)
		{
			SetAudioSourceTime(_startTimestep);
		}
		_isLoopingBack = false;
		float val = (_playbackCounter = (_lastPlaybackCounter = _startTimestep));
		_isPlaying = false;
		SyncActive();
		playbackCounterJSON.val = val;
		PlaybackStep();
		List<AnimationTimelineTrigger> list = new List<AnimationTimelineTrigger>(triggers);
		list.Sort((AnimationTimelineTrigger t1, AnimationTimelineTrigger t2) => t2.triggerStartTime.CompareTo(t1.triggerStartTime));
		foreach (AnimationTimelineTrigger item in list)
		{
			item.Reset();
		}
	}

	protected void InternalSetPlaybackCounter(float val, bool manualSet = false, bool forceAlign = false)
	{
		_isLoopingBack = false;
		if (_playbackCounter == val)
		{
			return;
		}
		bool flag = false;
		if (manualSet && _playbackCounter > val)
		{
			flag = true;
		}
		_lastPlaybackCounter = _playbackCounter;
		_playbackCounter = val;
		if (_isRecording)
		{
			if (_playbackCounter > _recordedLength)
			{
				if (_autoRecordStop && !_ignoreAutoRecordStop)
				{
					_playbackCounter = _recordedLength;
					StopRecordMode();
				}
				else
				{
					recordedLength = _playbackCounter;
				}
			}
			playbackCounterJSON.valNoCallback = val;
		}
		else if (_playbackCounter > _stopTimestep)
		{
			if (!manualSet)
			{
				if (_loop)
				{
					SetLoopback();
				}
				else
				{
					StopPlayback();
				}
			}
			_playbackCounter = _stopTimestep;
			playbackCounterJSON.valNoCallback = _playbackCounter;
		}
		else if (_playbackCounter > playbackCounterJSON.max)
		{
			if (_loop)
			{
				SetLoopback();
			}
			else
			{
				StopPlayback();
			}
			_playbackCounter = playbackCounterJSON.max;
			playbackCounterJSON.valNoCallback = _playbackCounter;
		}
		else
		{
			playbackCounterJSON.valNoCallback = val;
		}
		if (!_isLoopingBack)
		{
			if (forceAlign)
			{
				PlaybackStepForceAlign();
			}
			else
			{
				PlaybackStep();
			}
		}
		if (flag)
		{
			foreach (AnimationTimelineTrigger reverseTrigger in reverseTriggers)
			{
				reverseTrigger.Update(flag, _lastPlaybackCounter);
			}
			return;
		}
		foreach (AnimationTimelineTrigger trigger in triggers)
		{
			trigger.Update(flag, _lastPlaybackCounter);
		}
	}

	protected void SyncLoopbackTime(float f)
	{
		_loopbackTime = f;
	}

	protected void SyncAutoPlay(bool b)
	{
		_autoPlay = b;
	}

	protected void SyncLoop(bool b)
	{
		_loop = b;
	}

	protected void SyncAutoRecordStop(bool b)
	{
		_autoRecordStop = b;
	}

	protected void SyncPlaybackSpeed(float f)
	{
		_playbackSpeed = f;
	}

	protected void SyncShowRecordPaths(bool b)
	{
		_showRecordPaths = b;
		if (controllers == null)
		{
			return;
		}
		foreach (MotionAnimationControl controller in controllers)
		{
			controller.drawPathOpt = _showRecordPaths;
		}
	}

	protected void SyncShowStartMarkers(bool b)
	{
		_showStartMarkers = b;
	}

	public IEnumerable<AnimationTimelineTrigger> GetTriggers()
	{
		return triggers;
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

	protected void OpenAdvancedPanel()
	{
		if (advancedPanel != null)
		{
			advancedPanel.SetActive(true);
		}
		advancedPanelOpen = true;
	}

	protected void CloseAdvancedPanel()
	{
		if (advancedPanel != null)
		{
			advancedPanel.SetActive(false);
		}
		advancedPanelOpen = false;
	}

	protected void ReOpenAdvancedPanelIfWasOpen()
	{
		if (advancedPanel != null && advancedPanelOpen)
		{
			advancedPanel.SetActive(true);
		}
	}

	protected void TempCloseAdvancedPanelIfOpen()
	{
		if (advancedPanel != null && advancedPanelOpen)
		{
			advancedPanel.SetActive(false);
		}
	}

	protected AnimationTimelineTrigger AddTriggerInternal(int index = -1)
	{
		AnimationTimelineTrigger animationTimelineTrigger = new AnimationTimelineTrigger();
		animationTimelineTrigger.timeLineHandler = this;
		animationTimelineTrigger.handler = this;
		animationTimelineTrigger.doActionsInReverse = false;
		animationTimelineTrigger.onSelectedHandlers = (AnimationTimelineTrigger.OnSelected)Delegate.Combine(animationTimelineTrigger.onSelectedHandlers, new AnimationTimelineTrigger.OnSelected(TriggerSelectionChange));
		animationTimelineTrigger.onOpenTriggerActionsPanel = (Trigger.OnOpenTriggerActionsPanel)Delegate.Combine(animationTimelineTrigger.onOpenTriggerActionsPanel, new Trigger.OnOpenTriggerActionsPanel(TempCloseAdvancedPanelIfOpen));
		animationTimelineTrigger.onCloseTriggerActionsPanel = (Trigger.OnCloseTriggerActionsPanel)Delegate.Combine(animationTimelineTrigger.onCloseTriggerActionsPanel, new Trigger.OnCloseTriggerActionsPanel(ReOpenAdvancedPanelIfWasOpen));
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

	public AnimationTimelineTrigger AddAndReturnTrigger()
	{
		return AddTriggerInternal();
	}

	public override void RemoveTrigger(Trigger trigger)
	{
		AnimationTimelineTrigger animationTimelineTrigger = trigger as AnimationTimelineTrigger;
		if (triggers.Remove(animationTimelineTrigger))
		{
			animationTimelineTrigger.onSelectedHandlers = (AnimationTimelineTrigger.OnSelected)Delegate.Remove(animationTimelineTrigger.onSelectedHandlers, new AnimationTimelineTrigger.OnSelected(TriggerSelectionChange));
			animationTimelineTrigger.onOpenTriggerActionsPanel = (Trigger.OnOpenTriggerActionsPanel)Delegate.Remove(animationTimelineTrigger.onOpenTriggerActionsPanel, new Trigger.OnOpenTriggerActionsPanel(TempCloseAdvancedPanelIfOpen));
			animationTimelineTrigger.onCloseTriggerActionsPanel = (Trigger.OnCloseTriggerActionsPanel)Delegate.Remove(animationTimelineTrigger.onCloseTriggerActionsPanel, new Trigger.OnCloseTriggerActionsPanel(ReOpenAdvancedPanelIfWasOpen));
			reverseTriggers.Remove(animationTimelineTrigger);
			_selectedTriggers.Remove(animationTimelineTrigger);
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
			Debug.Log("Could not remove trigger " + trigger.displayName);
		}
	}

	public override void DuplicateTrigger(Trigger trigger)
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

	public void TriggerSelectionChange(AnimationTimelineTrigger att)
	{
		if (att.selected)
		{
			_selectedTriggers.Add(att);
		}
		else
		{
			_selectedTriggers.Remove(att);
		}
	}

	protected void SelectTriggersInTimeRange()
	{
		foreach (AnimationTimelineTrigger trigger in triggers)
		{
			if (trigger.triggerStartTime >= triggerSelectFromTimeJSON.val && trigger.triggerStartTime <= triggerSelectToTimeJSON.val)
			{
				trigger.selected = true;
			}
			else
			{
				trigger.selected = false;
			}
		}
	}

	public void ClearSelectedTriggers()
	{
		foreach (AnimationTimelineTrigger trigger in triggers)
		{
			trigger.selected = false;
		}
	}

	public void AdjustTimeOfSelectedTriggers()
	{
		float val = triggerTimeAdjustmentJSON.val;
		foreach (AnimationTimelineTrigger selectedTrigger in _selectedTriggers)
		{
			if (val > 0f)
			{
				selectedTrigger.triggerEndTime += val;
				selectedTrigger.triggerStartTime += val;
			}
			else
			{
				selectedTrigger.triggerStartTime += val;
				selectedTrigger.triggerEndTime += val;
			}
			if (selectedTrigger.triggerEndTime > _recordedLength)
			{
				recordedLength = selectedTrigger.triggerEndTime;
			}
		}
		SortTriggersByStartTime();
	}

	public void SortTriggersByStartTime()
	{
		triggers.Sort((AnimationTimelineTrigger a, AnimationTimelineTrigger b) => a.triggerStartTime.CompareTo(b.triggerStartTime));
		reverseTriggers = new List<AnimationTimelineTrigger>(triggers);
		reverseTriggers.Reverse();
		if (!(triggerContentManager != null))
		{
			return;
		}
		triggerContentManager.RemoveAllItems();
		int num = 0;
		foreach (AnimationTimelineTrigger trigger in triggers)
		{
			if (trigger.triggerPanel != null)
			{
				RectTransform component = trigger.triggerPanel.GetComponent<RectTransform>();
				if (component != null)
				{
					triggerContentManager.AddItem(component, num, true);
				}
			}
			num++;
		}
		triggerContentManager.RelayoutPanel();
	}

	public void CopySelectedTriggersAndPasteToTime()
	{
		AnimationTimelineTrigger animationTimelineTrigger = null;
		float num = float.MaxValue;
		foreach (AnimationTimelineTrigger selectedTrigger in _selectedTriggers)
		{
			if (selectedTrigger.triggerStartTime < num)
			{
				num = selectedTrigger.triggerStartTime;
				animationTimelineTrigger = selectedTrigger;
			}
		}
		if (animationTimelineTrigger == null)
		{
			return;
		}
		float num2 = triggerPasteToTimeJSON.val - animationTimelineTrigger.triggerStartTime;
		foreach (AnimationTimelineTrigger selectedTrigger2 in selectedTriggers)
		{
			JSONClass jSON = selectedTrigger2.GetJSON();
			AnimationTimelineTrigger animationTimelineTrigger2 = AddTriggerInternal();
			animationTimelineTrigger2.RestoreFromJSON(jSON);
			if (num2 > 0f)
			{
				animationTimelineTrigger2.triggerEndTime += num2;
				animationTimelineTrigger2.triggerStartTime += num2;
			}
			else
			{
				animationTimelineTrigger2.triggerStartTime += num2;
				animationTimelineTrigger2.triggerEndTime += num2;
			}
			if (animationTimelineTrigger2.triggerEndTime > _recordedLength)
			{
				recordedLength = animationTimelineTrigger2.triggerEndTime;
			}
		}
		SortTriggersByStartTime();
	}

	protected void SyncActive()
	{
		if (activeWhilePlaying != null)
		{
			activeWhilePlaying.SetActive(_isPlaying);
		}
		if (activeWhileStopped != null)
		{
			activeWhileStopped.SetActive(!_isPlaying);
		}
	}

	public void ClearAllAnimation()
	{
		StopPlayback();
		foreach (MotionAnimationControl controller in controllers)
		{
			controller.ClearAnimation();
		}
		recordedLength = 0f;
		_playbackCounter = 0f;
	}

	public void SelectControllersArmedForRecord()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.SelectModeArmedForRecord(controllers);
		}
	}

	public void ArmAllControlledControllersForRecord()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.ArmAllControlledControllersForRecord(controllers);
		}
	}

	public void StartRecordMode()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.SelectModeAnimationRecord(this);
		}
	}

	public void StartRecord()
	{
		SyncShowRecordPaths(_showRecordPaths);
		_isRecording = true;
		_isPlaying = true;
		SyncActive();
		_lastRecordTime = -1f;
		if (_recordedLength == 0f)
		{
			_ignoreAutoRecordStop = true;
		}
		else
		{
			_ignoreAutoRecordStop = false;
		}
		TimeControl.singleton.currentScale = 1f;
		if (SuperController.singleton != null)
		{
			SuperController.singleton.SetFreezeAnimation(false);
		}
		int recordCounter = Mathf.FloorToInt(_playbackCounter);
		foreach (MotionAnimationControl controller in controllers)
		{
			controller.PrepareRecord(recordCounter);
		}
		if (!_showStartMarkers)
		{
			return;
		}
		foreach (MotionAnimationControl controller2 in controllers)
		{
			if (controller2.armedForRecord && controller2.controller != null)
			{
				controller2.controller.TakeSnapshot();
				controller2.controller.drawSnapshot = true;
			}
		}
	}

	public void StopRecord()
	{
		if (!_isRecording)
		{
			return;
		}
		RecordStep(true);
		_isRecording = false;
		foreach (MotionAnimationControl controller in controllers)
		{
			if (controller.controller != null)
			{
				controller.controller.drawSnapshot = false;
			}
			controller.FinalizeRecord();
			controller.armedForRecord = false;
		}
		if (_autoPlay)
		{
			StopLoopback();
			StartPlayback();
		}
		else
		{
			_isPlaying = false;
			SyncActive();
		}
	}

	public void StopRecordMode()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.StopRecording();
		}
		StopRecord();
	}

	protected void SetIsPlayingFromAudioSource()
	{
		_isPlaying = true;
		SyncActive();
	}

	public void StartPlayback()
	{
		if (_linkToAudioSourceControl && audioSourceControl != null)
		{
			audioSourceControl.UnPause();
		}
		_isPlaying = true;
		SyncActive();
	}

	public void StopPlayback()
	{
		if (_linkToAudioSourceControl && audioSourceControl != null)
		{
			audioSourceControl.Pause();
		}
		if (_isLoopingBack)
		{
			StopLoopback();
		}
		if (_isRecording)
		{
			StopRecordMode();
		}
		_isPlaying = false;
		SyncActive();
	}

	public void TrimAnimation()
	{
		float val = startTimestepJSON.val;
		float val2 = stopTimestepJSON.val;
		foreach (MotionAnimationControl controller in controllers)
		{
			controller.TrimClip(val, val2);
		}
		recordedLength = val2 - val;
		startTimestepJSON.val = 0f;
	}

	protected void SetToDesiredLength()
	{
		recordedLength = _desiredLength;
		float val = startTimestepJSON.val;
		float val2 = stopTimestepJSON.val;
		foreach (MotionAnimationControl controller in controllers)
		{
			controller.TrimClip(val, val2);
		}
	}

	protected void SyncDesiredLength(float f)
	{
		_desiredLength = f;
	}

	public void CopyFromSceneMaster()
	{
		if (!isSceneMasterController)
		{
			MotionAnimationMaster motionAnimationMaster = SuperController.singleton.motionAnimationMaster;
			JSONClass jSON = motionAnimationMaster.GetJSON(true, true, true);
			RestoreFromJSON(jSON);
			LateRestoreFromJSON(jSON);
		}
	}

	public void CopyToSceneMaster()
	{
		if (!isSceneMasterController)
		{
			MotionAnimationMaster motionAnimationMaster = SuperController.singleton.motionAnimationMaster;
			JSONClass jSON = GetJSON(true, true, true);
			motionAnimationMaster.RestoreFromJSON(jSON);
			motionAnimationMaster.LateRestoreFromJSON(jSON);
		}
	}

	protected void StopLoopback()
	{
		if (_linkToAudioSourceControl && audioSourceControl != null && audioSourceControl.audioSource.clip != null)
		{
			SetAudioSourceTime(startTimestepJSON.val);
		}
		InternalSetPlaybackCounter(startTimestepJSON.val);
	}

	protected void RecordStep(bool forceRecord = false)
	{
		if (!(_playbackCounter - _lastRecordTime > recordInterval))
		{
			return;
		}
		_lastRecordTime = _playbackCounter;
		foreach (MotionAnimationControl controller in controllers)
		{
			controller.RecordStep(_playbackCounter, forceRecord);
		}
	}

	protected IEnumerator SeekToBeginningCo()
	{
		for (int i = 0; i < 10; i++)
		{
			PlaybackStepForceAlign();
			yield return null;
		}
	}

	public void SeekToBeginning()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.ResetSimulation(15, "MotionAnimation SeekToBeginning", true);
		}
		if (_linkToAudioSourceControl && audioSourceControl != null && audioSourceControl.audioSource.clip != null)
		{
			SetAudioSourceTime(startTimestepJSON.val);
		}
		InternalSetPlaybackCounter(startTimestepJSON.val, true, true);
		StartCoroutine(SeekToBeginningCo());
	}

	protected void PlaybackStep()
	{
		foreach (MotionAnimationControl controller in controllers)
		{
			if (!_isRecording || !controller.armedForRecord)
			{
				controller.PlaybackStep(_playbackCounter);
			}
		}
	}

	protected void PlaybackStepForceAlign()
	{
		foreach (MotionAnimationControl controller in controllers)
		{
			if (!_isRecording || !controller.armedForRecord)
			{
				controller.PlaybackStepForceAlign(_playbackCounter);
			}
		}
	}

	protected void LoopbackStep()
	{
		float val = startTimestepJSON.val;
		foreach (MotionAnimationControl controller in controllers)
		{
			controller.LoopbackStep(_loopbackCounter / _loopbackTime, val);
		}
	}

	protected void OnAtomRename(string fromuid, string touid)
	{
		foreach (AnimationTimelineTrigger trigger in triggers)
		{
			trigger.SyncAtomNames();
		}
		if (audioSourceControl != null && audioSourceControlAtomSelectionPopup != null)
		{
			audioSourceControlAtomSelectionPopup.currentValueNoCallback = audioSourceControl.containingAtom.uid;
		}
	}

	protected void Init()
	{
		linkToAudioSourceControlJSON = new JSONStorableBool("linkToAudioSourceControl", _linkToAudioSourceControl, SyncLinkToAudioSourceControl);
		linkToAudioSourceControlJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(linkToAudioSourceControlJSON);
		audioSourceTimeOffsetJSON = new JSONStorableFloat("audioSourceTimeOffset", _audioSourceTimeOffset, SyncAudioSourceTimeOffset, -10f, 10f, false);
		audioSourceTimeOffsetJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(audioSourceTimeOffsetJSON);
		playbackCounterJSON = new JSONStorableFloat("playbackCounter", _playbackCounter, SyncPlaybackCounter, 0f, 0f);
		playbackCounterJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(playbackCounterJSON);
		startTimestepJSON = new JSONStorableFloat("startTimestep", _startTimestep, SyncStartTimestep, 0f, 0f);
		startTimestepJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(startTimestepJSON);
		stopTimestepJSON = new JSONStorableFloat("stopTimestep", _stopTimestep, SyncStopTimestep, 0f, 0f);
		stopTimestepJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(stopTimestepJSON);
		loopbackTimeJSON = new JSONStorableFloat("loopbackTime", _loopbackTime, SyncLoopbackTime, 0f, 10f);
		loopbackTimeJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(loopbackTimeJSON);
		autoPlayJSON = new JSONStorableBool("autoPlay", _autoPlay, SyncAutoPlay);
		autoPlayJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(autoPlayJSON);
		loopJSON = new JSONStorableBool("loop", _loop, SyncLoop);
		loopJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(loopJSON);
		playbackSpeedJSON = new JSONStorableFloat("playbackSpeed", _playbackSpeed, SyncPlaybackSpeed, 0f, 10f);
		playbackSpeedJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(playbackSpeedJSON);
		desiredLengthJSON = new JSONStorableFloat("desiredLength", _desiredLength, SyncDesiredLength, 0f, 100f, false);
		desiredLengthJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(desiredLengthJSON);
		StartPlaybackAction = new JSONStorableAction("StartPlayback", StartPlayback);
		RegisterAction(StartPlaybackAction);
		StopPlaybackAction = new JSONStorableAction("StopPlayback", StopPlayback);
		RegisterAction(StopPlaybackAction);
		SelectControllersArmedForRecordAction = new JSONStorableAction("SelectControllersArmedForRecord", SelectControllersArmedForRecord);
		RegisterAction(SelectControllersArmedForRecordAction);
		ArmAllControlledControllersForRecordAction = new JSONStorableAction("ArmAllControlledControllesForRecord", ArmAllControlledControllersForRecord);
		RegisterAction(ArmAllControlledControllersForRecordAction);
		StartRecordModeAction = new JSONStorableAction("StartRecordMode", StartRecordMode);
		RegisterAction(StartRecordModeAction);
		StartRecordAction = new JSONStorableAction("StartRecord", StartRecord);
		RegisterAction(StartRecordAction);
		StopRecordAction = new JSONStorableAction("StopRecord", StopRecord);
		RegisterAction(StopRecordAction);
		StopRecordModeAction = new JSONStorableAction("StopRecordMode", StopRecordMode);
		RegisterAction(StopRecordModeAction);
		ClearAllAnimationAction = new JSONStorableAction("ClearAllAnimation", ClearAllAnimation);
		RegisterAction(ClearAllAnimationAction);
		TrimAnimationAction = new JSONStorableAction("TrimAnimation", TrimAnimation);
		RegisterAction(TrimAnimationAction);
		SetToDesiredLengthAction = new JSONStorableAction("SetToDesiredLength", SetToDesiredLength);
		RegisterAction(SetToDesiredLengthAction);
		SeekToBeginningAction = new JSONStorableAction("SeekToBeginning", SeekToBeginning);
		RegisterAction(SeekToBeginningAction);
		ResetAnimationAction = new JSONStorableAction("ResetAnimation", ResetAnimation);
		RegisterAction(ResetAnimationAction);
		if (!isSceneMasterController)
		{
			CopyFromSceneMasterAction = new JSONStorableAction("CopyFromSceneMaster", CopyFromSceneMaster);
			RegisterAction(CopyFromSceneMasterAction);
			CopyToSceneMasterAction = new JSONStorableAction("CopyToSceneMaster", CopyToSceneMaster);
			RegisterAction(CopyToSceneMasterAction);
		}
		SelectTriggersInTimeRangeAction = new JSONStorableAction("SelectTriggersInTimeRange", SelectTriggersInTimeRange);
		RegisterAction(SelectTriggersInTimeRangeAction);
		ClearSelectedTriggersAction = new JSONStorableAction("ClearSelectedTriggers", ClearSelectedTriggers);
		RegisterAction(ClearSelectedTriggersAction);
		AdjustTimeOfSelectedTriggersAction = new JSONStorableAction("AdjustTimeOfSelectedTriggers", AdjustTimeOfSelectedTriggers);
		RegisterAction(AdjustTimeOfSelectedTriggersAction);
		SortTriggersByStartTimeAction = new JSONStorableAction("SortTriggersByStartTime", SortTriggersByStartTime);
		RegisterAction(SortTriggersByStartTimeAction);
		CopySelectedTriggersAndPasteToTimeAction = new JSONStorableAction("CopySelectedTriggersAndPasteToTime", CopySelectedTriggersAndPasteToTime);
		RegisterAction(CopySelectedTriggersAndPasteToTimeAction);
		triggerSelectFromTimeJSON = new JSONStorableFloat("triggerSelectFromTime", 0f, 0f, 0f);
		triggerSelectFromTimeJSON.isStorable = false;
		triggerSelectFromTimeJSON.isRestorable = false;
		triggerSelectToTimeJSON = new JSONStorableFloat("triggerSelectToTime", 0f, 0f, 0f);
		triggerSelectToTimeJSON.isStorable = false;
		triggerSelectToTimeJSON.isRestorable = false;
		triggerTimeAdjustmentJSON = new JSONStorableFloat("triggerTimeAdjustment", 0f, -100f, 100f, false);
		triggerTimeAdjustmentJSON.isStorable = false;
		triggerTimeAdjustmentJSON.isRestorable = false;
		triggerPasteToTimeJSON = new JSONStorableFloat("triggerPasteToTime", 0f, 0f, 0f);
		triggerPasteToTimeJSON.isStorable = false;
		triggerPasteToTimeJSON.isRestorable = false;
		autoRecordStopJSON = new JSONStorableBool("autoRecordStop", _autoRecordStop, SyncAutoRecordStop);
		autoRecordStopJSON.isStorable = false;
		autoRecordStopJSON.isRestorable = false;
		RegisterBool(autoRecordStopJSON);
		showRecordPathsJSON = new JSONStorableBool("showRecordPath", _showRecordPaths, SyncShowRecordPaths);
		showRecordPathsJSON.isStorable = false;
		showRecordPathsJSON.isRestorable = false;
		RegisterBool(showRecordPathsJSON);
		showStartMarkersJSON = new JSONStorableBool("showStartMarkers", _showStartMarkers, SyncShowStartMarkers);
		showStartMarkersJSON.isStorable = false;
		showStartMarkersJSON.isRestorable = false;
		RegisterBool(showStartMarkersJSON);
		SyncShowRecordPaths(_showRecordPaths);
		controllers = new HashSet<MotionAnimationControl>();
		triggers = new List<AnimationTimelineTrigger>();
		reverseTriggers = new List<AnimationTimelineTrigger>();
		_selectedTriggers = new HashSet<AnimationTimelineTrigger>();
		if (SuperController.singleton != null)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Combine(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomRename));
		}
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (!(t != null))
		{
			return;
		}
		MotionAnimationMasterUI componentInChildren = t.GetComponentInChildren<MotionAnimationMasterUI>(true);
		if (!(componentInChildren != null))
		{
			return;
		}
		linkToAudioSourceControlJSON.RegisterToggle(componentInChildren.linkToAudioSourceControlToggle, isAlt);
		audioSourceTimeOffsetJSON.RegisterSlider(componentInChildren.audioSourceTimeOffsetSlider, isAlt);
		playbackCounterJSON.RegisterSlider(componentInChildren.playbackCounterSlider, isAlt);
		startTimestepJSON.RegisterSlider(componentInChildren.startTimestepSlider, isAlt);
		stopTimestepJSON.RegisterSlider(componentInChildren.stopTimestepSlider, isAlt);
		loopbackTimeJSON.RegisterSlider(componentInChildren.loopbackTimeSlider, isAlt);
		playbackSpeedJSON.RegisterSlider(componentInChildren.playbackSpeedSlider, isAlt);
		autoPlayJSON.RegisterToggle(componentInChildren.autoPlayToggle, isAlt);
		loopJSON.RegisterToggle(componentInChildren.loopToggle, isAlt);
		autoRecordStopJSON.RegisterToggle(componentInChildren.autoRecordStopToggle, isAlt);
		showRecordPathsJSON.RegisterToggle(componentInChildren.showRecordPathsToggle, isAlt);
		showStartMarkersJSON.RegisterToggle(componentInChildren.showStartMarkersToggle, isAlt);
		ClearAllAnimationAction.RegisterButton(componentInChildren.clearAllAnimationButton, isAlt);
		SelectControllersArmedForRecordAction.RegisterButton(componentInChildren.selectControllersArmedForRecordButton, isAlt);
		ArmAllControlledControllersForRecordAction.RegisterButton(componentInChildren.armAllControlledControllersForRecordButton, isAlt);
		StartRecordModeAction.RegisterButton(componentInChildren.startRecordModeButton, isAlt);
		StartRecordAction.RegisterButton(componentInChildren.startRecordButton, isAlt);
		StopRecordAction.RegisterButton(componentInChildren.stopRecordButton, isAlt);
		StopRecordModeAction.RegisterButton(componentInChildren.stopRecordModeButton, isAlt);
		StartPlaybackAction.RegisterButton(componentInChildren.startPlaybackButton, isAlt);
		StopPlaybackAction.RegisterButton(componentInChildren.stopPlaybackButton, isAlt);
		TrimAnimationAction.RegisterButton(componentInChildren.trimAnimationButton, isAlt);
		desiredLengthJSON.RegisterSlider(componentInChildren.desiredLengthSlider, isAlt);
		SetToDesiredLengthAction.RegisterButton(componentInChildren.setToDesiredLengthButton, isAlt);
		SeekToBeginningAction.RegisterButton(componentInChildren.seekToBeginningButton, isAlt);
		ResetAnimationAction.RegisterButton(componentInChildren.resetAnimationButton, isAlt);
		SelectTriggersInTimeRangeAction.RegisterButton(componentInChildren.selectTriggersInTimeRangeButton, isAlt);
		ClearSelectedTriggersAction.RegisterButton(componentInChildren.clearSelectedTriggersButton, isAlt);
		AdjustTimeOfSelectedTriggersAction.RegisterButton(componentInChildren.adjustTimeOfSelectedTriggersButton, isAlt);
		SortTriggersByStartTimeAction.RegisterButton(componentInChildren.sortTriggersByStartTimeButton, isAlt);
		CopySelectedTriggersAndPasteToTimeAction.RegisterButton(componentInChildren.copySelectedTriggersAndPasteToTimeButton, isAlt);
		triggerSelectFromTimeJSON.RegisterSlider(componentInChildren.triggerSelectFromTimeSlider, isAlt);
		triggerSelectToTimeJSON.RegisterSlider(componentInChildren.triggerSelectToTimeSlider, isAlt);
		triggerTimeAdjustmentJSON.RegisterSlider(componentInChildren.triggerTimeAdjustmentSlider, isAlt);
		triggerPasteToTimeJSON.RegisterSlider(componentInChildren.triggerPasteToTimeSlider, isAlt);
		if (!isSceneMasterController)
		{
			CopyFromSceneMasterAction.RegisterButton(componentInChildren.copyFromSceneMasterButton, isAlt);
			CopyToSceneMasterAction.RegisterButton(componentInChildren.copyToSceneMasterButton, isAlt);
		}
		else
		{
			if (componentInChildren.copyFromSceneMasterButton != null)
			{
				componentInChildren.copyFromSceneMasterButton.gameObject.SetActive(false);
			}
			if (componentInChildren.copyToSceneMasterButton != null)
			{
				componentInChildren.copyToSceneMasterButton.gameObject.SetActive(false);
			}
		}
		if (!isAlt)
		{
			advancedPanel = componentInChildren.advancedPanel;
			if (componentInChildren.openAdvancedPanelButton != null)
			{
				componentInChildren.openAdvancedPanelButton.onClick.AddListener(OpenAdvancedPanel);
			}
			if (componentInChildren.closeAdvancedPanelButton != null)
			{
				componentInChildren.closeAdvancedPanelButton.onClick.AddListener(CloseAdvancedPanel);
			}
			audioSourceControlAtomSelectionPopup = componentInChildren.audioSourceControlAtomSelectionPopup;
			if (audioSourceControlAtomSelectionPopup != null)
			{
				UIPopup uIPopup = audioSourceControlAtomSelectionPopup;
				uIPopup.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Combine(uIPopup.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetAudioSourceControlAtomNames));
				UIPopup uIPopup2 = audioSourceControlAtomSelectionPopup;
				uIPopup2.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup2.onValueChangeHandlers, new UIPopup.OnValueChange(SetAudioSourceControlAtom));
			}
			audioSourceControlSelectionPopup = componentInChildren.audioSourceControlSelectionPopup;
			if (audioSourceControlSelectionPopup != null)
			{
				UIPopup uIPopup3 = audioSourceControlSelectionPopup;
				uIPopup3.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup3.onValueChangeHandlers, new UIPopup.OnValueChange(SetAudioSourceControlObject));
			}
			triggerContentManager = componentInChildren.triggerContentManager;
			triggerActionsParent = componentInChildren.triggerActionsParent;
			for (int i = 0; i < triggers.Count; i++)
			{
				AnimationTimelineTrigger animationTimelineTrigger = triggers[i];
				CreateTriggerUI(animationTimelineTrigger, i);
				animationTimelineTrigger.InitTriggerUI();
				animationTimelineTrigger.triggerActionsParent = triggerActionsParent;
			}
			playbackCounterSlider = componentInChildren.playbackCounterSlider;
			startTimestepSlider = componentInChildren.startTimestepSlider;
			stopTimestepSlider = componentInChildren.stopTimestepSlider;
			loopbackTimeSlider = componentInChildren.loopbackTimeSlider;
			playbackSpeedSlider = componentInChildren.playbackSpeedSlider;
			loopToggle = componentInChildren.loopToggle;
			autoRecordStopToggle = componentInChildren.autoRecordStopToggle;
			showRecordPathsToggle = componentInChildren.showRecordPathsToggle;
			showStartMarkersToggle = componentInChildren.showStartMarkersToggle;
			if (componentInChildren.addTriggerButton != null)
			{
				addTriggerButton = componentInChildren.addTriggerButton;
				componentInChildren.addTriggerButton.onClick.AddListener(AddTrigger);
			}
			if (componentInChildren.clearAllTriggersButton != null)
			{
				clearAllTriggersButton = componentInChildren.clearAllTriggersButton;
				componentInChildren.clearAllTriggersButton.onClick.AddListener(ClearTriggers);
			}
		}
	}

	protected void OnDestroy()
	{
		if (SuperController.singleton != null)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Remove(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomRename));
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

	private void Update()
	{
		if (!freeze && (SuperController.singleton == null || !SuperController.singleton.freezeAnimation))
		{
			if (_isRecording)
			{
				RecordStep();
			}
			if (_linkToAudioSourceControl && audioSourceControl != null && audioSourceControl.audioSource.clip != null && audioSourceControl.audioSource.isPlaying)
			{
				if (_recordedLength < audioSourceControl.audioSource.clip.length + _audioSourceTimeOffset)
				{
					recordedLength = audioSourceControl.audioSource.clip.length + _audioSourceTimeOffset;
				}
				SetIsPlayingFromAudioSource();
				InternalSetPlaybackCounter(audioSourceControl.audioSource.time + _audioSourceTimeOffset, true);
			}
			else if (_isLoopingBack)
			{
				_loopbackCounter += Time.deltaTime * _playbackSpeed;
				if (_loopbackCounter > _loopbackTime)
				{
					StopLoopback();
				}
				else
				{
					LoopbackStep();
				}
			}
			else if (_isPlaying)
			{
				InternalSetPlaybackCounter(playbackCounter + Time.deltaTime * _playbackSpeed);
			}
		}
		foreach (AnimationTimelineTrigger trigger in triggers)
		{
			trigger.Update();
		}
	}
}
