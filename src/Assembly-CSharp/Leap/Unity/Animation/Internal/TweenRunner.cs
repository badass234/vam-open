using System;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Animation.Internal
{
	public class TweenRunner : MonoBehaviour
	{
		private TweenInstance[] _runningTweens = new TweenInstance[16];

		private int _runningCount;

		private Queue<TweenInstance> _toRecycle = new Queue<TweenInstance>();

		private static TweenRunner _cachedInstance;

		public static TweenRunner instance
		{
			get
			{
				if (_cachedInstance == null)
				{
					_cachedInstance = UnityEngine.Object.FindObjectOfType<TweenRunner>();
					if (_cachedInstance == null)
					{
						_cachedInstance = new GameObject("__Tween Runner__").AddComponent<TweenRunner>();
						_cachedInstance.gameObject.hideFlags = HideFlags.HideAndDontSave;
					}
				}
				return _cachedInstance;
			}
		}

		private void Update()
		{
			int runningCount = _runningCount;
			while (runningCount-- != 0)
			{
				TweenInstance tweenInstance = _runningTweens[runningCount];
				try
				{
					tweenInstance.Step(this);
				}
				catch (Exception exception)
				{
					Debug.LogError("Error occured inside of tween!  Tween has been terminated");
					Debug.LogException(exception);
					if (tweenInstance.runnerIndex != -1)
					{
						RemoveTween(tweenInstance);
					}
				}
			}
			while (_toRecycle.Count > 0)
			{
				TweenInstance tweenInstance2 = _toRecycle.Dequeue();
				if (tweenInstance2.instanceId == -2)
				{
					tweenInstance2.Dispose();
				}
			}
		}

		public void ScheduleForRecycle(TweenInstance instance)
		{
			instance.instanceId = -2;
			_toRecycle.Enqueue(instance);
		}

		public void AddTween(TweenInstance instance)
		{
			if (_runningCount >= _runningTweens.Length)
			{
				Utils.DoubleCapacity(ref _runningTweens);
			}
			instance.runnerIndex = _runningCount;
			_runningTweens[_runningCount++] = instance;
			if (instance.curPercent == 0f)
			{
				if (instance.OnLeaveStart != null)
				{
					instance.OnLeaveStart();
				}
			}
			else if (instance.curPercent == 1f && instance.OnLeaveEnd != null)
			{
				instance.OnLeaveEnd();
			}
		}

		public void RemoveTween(TweenInstance instance)
		{
			if (instance.runnerIndex == -1)
			{
				return;
			}
			_runningCount--;
			if (_runningCount < 0)
			{
				throw new Exception("Removed more tweens than were started!");
			}
			int runnerIndex = instance.runnerIndex;
			_runningTweens[_runningCount].runnerIndex = runnerIndex;
			_runningTweens[runnerIndex] = _runningTweens[_runningCount];
			instance.runnerIndex = -1;
			if (instance.curPercent == 1f)
			{
				if (instance.OnReachEnd != null)
				{
					instance.OnReachEnd();
				}
			}
			else if (instance.curPercent == 0f && instance.OnReachStart != null)
			{
				instance.OnReachStart();
			}
			if (instance.runnerIndex == -1 && instance.returnToPoolUponStop)
			{
				ScheduleForRecycle(instance);
			}
		}
	}
}
