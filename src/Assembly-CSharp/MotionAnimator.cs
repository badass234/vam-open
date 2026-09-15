using SimpleJSON;

public class MotionAnimator : JSONStorable
{
	public MotionAnimationControl[] controllers;

	public int recordInterval = 3;

	protected bool _isRecording;

	protected bool _isPlaying;

	protected int _recordCounter;

	protected int _patternLength;

	protected int _playbackCounter;

	public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		JSONClass jSON = base.GetJSON(includePhysical, includeAppearance, forceStore);
		if (includePhysical)
		{
		}
		return jSON;
	}

	public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
	{
		base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
		if (!restorePhysical)
		{
		}
	}

	public void SaveSequence()
	{
	}

	public void LoadSequence()
	{
	}

	public void SavePattern()
	{
	}

	public void LoadPattern()
	{
	}

	public void ClearAllAnimation()
	{
		MotionAnimationControl[] array = controllers;
		foreach (MotionAnimationControl motionAnimationControl in array)
		{
			motionAnimationControl.ClearAnimation();
		}
		_isPlaying = false;
	}

	public void StartRecord()
	{
		_recordCounter = 0;
		_isRecording = true;
		MotionAnimationControl[] array = controllers;
		foreach (MotionAnimationControl motionAnimationControl in array)
		{
			motionAnimationControl.PrepareRecord(_recordCounter);
		}
	}

	public void FinishRecord()
	{
		_isRecording = false;
		if (_recordCounter > _patternLength)
		{
			_patternLength = _recordCounter;
		}
		MotionAnimationControl[] array = controllers;
		foreach (MotionAnimationControl motionAnimationControl in array)
		{
			motionAnimationControl.FinalizeRecord();
		}
		StartPlayback();
	}

	public void StartPlayback()
	{
		_playbackCounter = 0;
		_isPlaying = true;
	}

	protected void RecordStep()
	{
		MotionAnimationControl[] array = controllers;
		foreach (MotionAnimationControl motionAnimationControl in array)
		{
			motionAnimationControl.RecordStep(_recordCounter);
		}
	}

	protected void PlaybackStep()
	{
		MotionAnimationControl[] array = controllers;
		foreach (MotionAnimationControl motionAnimationControl in array)
		{
			motionAnimationControl.PlaybackStep(_playbackCounter);
		}
	}

	private void Update()
	{
		if (_isPlaying)
		{
			_playbackCounter++;
			if (_playbackCounter > _patternLength)
			{
				_playbackCounter = 0;
			}
			PlaybackStep();
		}
		else if (_isRecording)
		{
			_recordCounter++;
			if (_recordCounter % recordInterval == 0)
			{
				RecordStep();
			}
		}
	}
}
