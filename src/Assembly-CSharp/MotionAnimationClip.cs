using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class MotionAnimationClip
{
	protected float _clipLength;

	protected List<MotionAnimationStep> _steps;

	protected MotionAnimationStep _currentStep;

	protected MotionAnimationStep _nextStep;

	protected int _currentStepIndex;

	protected MotionAnimationStep _interpolatedStep;

	protected float _loopbackStepSearchedTimeStep;

	protected MotionAnimationStep _loopbackStep;

	protected int[] indices;

	protected Vector3[] vertices;

	protected Mesh mesh;

	protected bool _meshDirty = true;

	public float clipLength => _clipLength;

	public List<MotionAnimationStep> steps => _steps;

	public MotionAnimationClip()
	{
		_steps = new List<MotionAnimationStep>();
		_interpolatedStep = new MotionAnimationStep();
	}

	public bool SaveToJSON(JSONClass jc)
	{
		if (_steps.Count > 0)
		{
			JSONArray jSONArray = new JSONArray();
			foreach (MotionAnimationStep step in _steps)
			{
				JSONClass jSONClass = new JSONClass();
				jSONClass["timeStep"].AsFloat = step.timeStep;
				jSONClass["positionOn"].AsBool = step.positionOn;
				jSONClass["rotationOn"].AsBool = step.rotationOn;
				jSONClass["position"]["x"].AsFloat = step.position.x;
				jSONClass["position"]["y"].AsFloat = step.position.y;
				jSONClass["position"]["z"].AsFloat = step.position.z;
				jSONClass["rotation"]["x"].AsFloat = step.rotation.x;
				jSONClass["rotation"]["y"].AsFloat = step.rotation.y;
				jSONClass["rotation"]["z"].AsFloat = step.rotation.z;
				jSONClass["rotation"]["w"].AsFloat = step.rotation.w;
				jSONArray.Add(jSONClass);
			}
			jc["steps"] = jSONArray;
			return true;
		}
		return false;
	}

	public void RestoreFromJSON(JSONClass jc, bool setMissingToDefault = true)
	{
		JSONArray asArray = jc["steps"].AsArray;
		if (asArray != null)
		{
			ClearAllSteps();
			Vector3 position = default(Vector3);
			Quaternion rotation = default(Quaternion);
			for (int i = 0; i < asArray.Count; i++)
			{
				JSONClass asObject = asArray[i].AsObject;
				if (asObject != null)
				{
					MotionAnimationStep motionAnimationStep = new MotionAnimationStep();
					if (asObject["positionOn"] != null)
					{
						motionAnimationStep.positionOn = asObject["positionOn"].AsBool;
					}
					else
					{
						motionAnimationStep.positionOn = true;
					}
					if (asObject["rotationOn"] != null)
					{
						motionAnimationStep.rotationOn = asObject["rotationOn"].AsBool;
					}
					else
					{
						motionAnimationStep.rotationOn = true;
					}
					if (asObject["position"]["x"] != null)
					{
						position.x = asObject["position"]["x"].AsFloat;
					}
					else
					{
						position.x = 0f;
						Debug.LogWarning("JSON file format error. Missing position x for animation step");
					}
					if (asObject["position"]["y"] != null)
					{
						position.y = asObject["position"]["y"].AsFloat;
					}
					else
					{
						position.y = 0f;
						Debug.LogWarning("JSON file format error. Missing position y for animation step");
					}
					if (asObject["position"]["z"] != null)
					{
						position.z = asObject["position"]["z"].AsFloat;
					}
					else
					{
						position.z = 0f;
						Debug.LogWarning("JSON file format error. Missing position z for animation step");
					}
					if (asObject["rotation"]["x"] != null)
					{
						rotation.x = asObject["rotation"]["x"].AsFloat;
					}
					else
					{
						rotation.x = 0f;
						Debug.LogWarning("JSON file format error. Missing rotation x for animation step");
					}
					if (asObject["rotation"]["y"] != null)
					{
						rotation.y = asObject["rotation"]["y"].AsFloat;
					}
					else
					{
						rotation.y = 0f;
						Debug.LogWarning("JSON file format error. Missing rotation y for animation step");
					}
					if (asObject["rotation"]["z"] != null)
					{
						rotation.z = asObject["rotation"]["z"].AsFloat;
					}
					else
					{
						rotation.z = 0f;
						Debug.LogWarning("JSON file format error. Missing rotation z for animation step");
					}
					if (asObject["rotation"]["w"] != null)
					{
						rotation.w = asObject["rotation"]["w"].AsFloat;
					}
					else
					{
						rotation.w = 0f;
						Debug.LogWarning("JSON file format error. Missing rotation w for animation step");
					}
					motionAnimationStep.position = position;
					motionAnimationStep.rotation = rotation;
					if (asObject["timeStep"] != null)
					{
						motionAnimationStep.timeStep = asObject["timeStep"].AsFloat;
					}
					else
					{
						Debug.LogWarning("JSON file format error. Missing timeStep for animation step");
					}
					_steps.Add(motionAnimationStep);
					_meshDirty = true;
					_clipLength = motionAnimationStep.timeStep;
				}
			}
		}
		else if (setMissingToDefault)
		{
			ClearAllSteps();
		}
	}

	public void ClearAllSteps()
	{
		_steps = new List<MotionAnimationStep>();
		_meshDirty = true;
		_currentStep = null;
		_nextStep = null;
		_currentStepIndex = 0;
		_clipLength = 0f;
		_loopbackStep = null;
	}

	public void ClearAllStepsStartingAt(float timeStep)
	{
		SeekToTimeStep(timeStep);
		if (_currentStep != null && _currentStep.timeStep >= timeStep)
		{
			_steps.RemoveRange(_currentStepIndex, _steps.Count - _currentStepIndex);
			_meshDirty = true;
		}
		else if (_nextStep != null)
		{
			int num = _currentStepIndex + 1;
			_steps.RemoveRange(num, _steps.Count - num);
			_meshDirty = true;
			_nextStep = null;
		}
		if (_currentStep != null)
		{
			_clipLength = _currentStep.timeStep;
		}
		else
		{
			_clipLength = 0f;
		}
		_loopbackStep = null;
	}

	public void ShiftAllSteps(float timeShift)
	{
		foreach (MotionAnimationStep step in _steps)
		{
			step.timeStep += timeShift;
		}
		_clipLength += timeShift;
		if (timeShift < 0f)
		{
			ClearAllNegativeTimeStepSteps();
			if (_clipLength < 0f)
			{
				_clipLength = 0f;
			}
		}
		_meshDirty = true;
		_currentStep = null;
		_currentStepIndex = 0;
	}

	protected void ClearAllNegativeTimeStepSteps()
	{
		List<MotionAnimationStep> list = new List<MotionAnimationStep>();
		foreach (MotionAnimationStep step in _steps)
		{
			if (step.timeStep >= 0f)
			{
				list.Add(step);
			}
		}
		_steps = list;
	}

	public void PrepareRecord(float timeStep)
	{
		ClearAllStepsStartingAt(timeStep);
	}

	public void RecordStep(Transform t, float timeStep, bool positionOn, bool rotationOn, bool forceRecord)
	{
		bool flag = true;
		if (_steps.Count > 0)
		{
			MotionAnimationStep motionAnimationStep = _steps[_steps.Count - 1];
			if (!motionAnimationStep.positionOn && !positionOn && !motionAnimationStep.rotationOn && !rotationOn)
			{
				flag = false;
			}
		}
		if (flag || forceRecord)
		{
			MotionAnimationStep motionAnimationStep2 = new MotionAnimationStep();
			motionAnimationStep2.positionOn = positionOn;
			motionAnimationStep2.rotationOn = rotationOn;
			motionAnimationStep2.position = t.localPosition;
			motionAnimationStep2.rotation = t.localRotation;
			motionAnimationStep2.timeStep = timeStep;
			_steps.Add(motionAnimationStep2);
			_meshDirty = true;
			_clipLength = timeStep;
		}
	}

	public void FinalizeRecord()
	{
	}

	protected void SeekToTimeStep(float timeStep)
	{
		if ((_currentStep == null || timeStep == 0f) && _steps.Count > 0)
		{
			_currentStepIndex = 0;
			_currentStep = _steps[0];
			if (_steps.Count > 1)
			{
				_nextStep = _steps[1];
			}
			else
			{
				_nextStep = null;
			}
		}
		if (_currentStep == null)
		{
			return;
		}
		bool flag = false;
		while (!flag)
		{
			flag = true;
			if (timeStep < _currentStep.timeStep)
			{
				if (_currentStepIndex > 0)
				{
					_nextStep = _currentStep;
					_currentStepIndex--;
					_currentStep = _steps[_currentStepIndex];
					flag = false;
				}
			}
			else if (_nextStep != null && timeStep >= _nextStep.timeStep)
			{
				_currentStep = _nextStep;
				_currentStepIndex++;
				if (_currentStepIndex < _steps.Count)
				{
					_nextStep = _steps[_currentStepIndex];
					flag = false;
				}
				else
				{
					_nextStep = null;
				}
			}
		}
	}

	protected MotionAnimationStep CalculateInterpolatedStep(float timeStep)
	{
		if (_currentStep != null)
		{
			_interpolatedStep.positionOn = _currentStep.positionOn;
			_interpolatedStep.rotationOn = _currentStep.rotationOn;
			if (_nextStep != null)
			{
				float t = (timeStep - _currentStep.timeStep) / (_nextStep.timeStep - _currentStep.timeStep);
				_interpolatedStep.position = Vector3.Lerp(_currentStep.position, _nextStep.position, t);
				_interpolatedStep.rotation = Quaternion.Lerp(_currentStep.rotation, _nextStep.rotation, t);
			}
			else
			{
				_interpolatedStep.position = _currentStep.position;
				_interpolatedStep.rotation = _currentStep.rotation;
			}
		}
		return _interpolatedStep;
	}

	public MotionAnimationStep PlaybackStep(float timeStep)
	{
		SeekToTimeStep(timeStep);
		return CalculateInterpolatedStep(timeStep);
	}

	protected void FindLoopbackStep(float timeStep)
	{
		if (_steps == null || (_loopbackStep != null && _loopbackStepSearchedTimeStep == timeStep))
		{
			return;
		}
		for (int i = 0; i < _steps.Count; i++)
		{
			MotionAnimationStep motionAnimationStep = _steps[i];
			if (motionAnimationStep.timeStep >= timeStep || i == _steps.Count - 1)
			{
				_loopbackStepSearchedTimeStep = timeStep;
				_loopbackStep = motionAnimationStep;
				break;
			}
		}
	}

	public MotionAnimationStep LoopbackStep(float percent, float toTimeStep)
	{
		if (_steps.Count > 0)
		{
			FindLoopbackStep(toTimeStep);
			if (_currentStep == null)
			{
				_currentStep = _loopbackStep;
			}
			_interpolatedStep.positionOn = true;
			_interpolatedStep.rotationOn = true;
			_interpolatedStep.position = Vector3.Lerp(_currentStep.position, _loopbackStep.position, percent);
			_interpolatedStep.rotation = Quaternion.Lerp(_currentStep.rotation, _loopbackStep.rotation, percent);
		}
		return _interpolatedStep;
	}

	protected void RegenerateMesh()
	{
		if (!_meshDirty)
		{
			return;
		}
		_meshDirty = false;
		if (_steps != null && _steps.Count > 0)
		{
			mesh = new Mesh();
			indices = new int[_steps.Count];
			vertices = new Vector3[_steps.Count];
			for (int i = 0; i < _steps.Count; i++)
			{
				indices[i] = i;
				vertices[i] = _steps[i].position;
			}
			mesh.vertices = vertices;
			mesh.SetIndices(indices, MeshTopology.LineStrip, 0);
		}
		else
		{
			mesh = new Mesh();
		}
	}

	public Mesh GetMesh()
	{
		RegenerateMesh();
		return mesh;
	}
}
