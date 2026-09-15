using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
	public RectTransform _rect;

	public Text _progressText;

	private float _currentProgress;

	private float _maximumValue;

	private float _minimumValue;

	private float _stepValue;

	private Vector3 _localScale;

	public float Progress
	{
		get
		{
			return _currentProgress;
		}
		set
		{
			_currentProgress = value;
		}
	}

	public float Maximum
	{
		get
		{
			return _maximumValue;
		}
		set
		{
			_maximumValue = value;
		}
	}

	public float Minimum
	{
		get
		{
			return _minimumValue;
		}
		set
		{
			_minimumValue = value;
		}
	}

	public float Step
	{
		get
		{
			return _stepValue;
		}
		set
		{
			_stepValue = value;
		}
	}

	private void Start()
	{
		_currentProgress = 0f;
		_maximumValue = 1f;
		_minimumValue = 0f;
		_stepValue = 0.1f;
		_localScale = new Vector3(_currentProgress / _maximumValue + _minimumValue, _rect.localScale.y, _rect.localScale.z);
		UpdateProgressBar();
	}

	private void OnGUI()
	{
	}

	private void UpdateProgressBar()
	{
		_localScale.x = _currentProgress / _maximumValue + _minimumValue;
		_rect.localScale = _localScale;
		if (_currentProgress >= 1f)
		{
			_localScale.x = 1f;
			_rect.localScale = _localScale;
		}
	}

	public void PerformStep()
	{
		_currentProgress += _stepValue;
		UpdateProgressBar();
	}

	public void Clear()
	{
		_currentProgress = 0f;
		_maximumValue = 1f;
		_minimumValue = 0f;
		_stepValue = 0.1f;
	}

	public void SetProgressText(string text)
	{
		_progressText.text = text;
	}
}
