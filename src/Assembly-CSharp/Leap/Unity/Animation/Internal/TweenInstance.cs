using System;
using System.Collections;
using UnityEngine;

namespace Leap.Unity.Animation.Internal
{
	public class TweenInstance : IPoolable, IDisposable
	{
		public struct TweenYieldInstruction : IEnumerator
		{
			private TweenInstance _instance;

			private int _instanceId;

			public object Current => null;

			public TweenYieldInstruction(TweenInstance instance)
			{
				_instance = instance;
				_instanceId = _instance.instanceId;
			}

			public bool MoveNext()
			{
				return _instanceId == _instance.instanceId && _instance.runnerIndex != -1;
			}

			public void Reset()
			{
			}
		}

		private static int _nextInstanceId = 1;

		public const int ID_UNUSED = 0;

		public const int ID_IN_POOL = -1;

		public const int ID_WAITING_FOR_RECYCLE = -2;

		public const int ID_INVALID_STATE = -3;

		public int instanceId = -3;

		public const int NOT_RUNNING = -1;

		public int runnerIndex = -1;

		public bool returnToPoolUponStop;

		public IInterpolator[] interpolators = new IInterpolator[1];

		public int interpolatorCount;

		public float curPercent;

		public float dstPercent;

		public float velPercent;

		public Direction direction;

		public SmoothType smoothType;

		public Func<float, float> smoothFunction;

		public Action<float> OnProgress;

		public Action OnLeaveEnd;

		public Action OnReachEnd;

		public Action OnLeaveStart;

		public Action OnReachStart;

		public TweenYieldInstruction yieldInstruction;

		public TweenInstance()
		{
			ResetDefaults();
		}

		public void OnSpawn()
		{
			instanceId = _nextInstanceId++;
			yieldInstruction = new TweenYieldInstruction(this);
		}

		public void OnRecycle()
		{
		}

		public void ResetDefaults()
		{
			returnToPoolUponStop = true;
			curPercent = 0f;
			dstPercent = 1f;
			velPercent = 1f;
			direction = Direction.Forward;
			smoothType = SmoothType.Linear;
			smoothFunction = null;
			OnProgress = null;
			OnLeaveEnd = null;
			OnReachEnd = null;
			OnLeaveStart = null;
			OnReachStart = null;
		}

		public void Dispose()
		{
			instanceId = -1;
			for (int i = 0; i < interpolatorCount; i++)
			{
				interpolators[i].Dispose();
				interpolators[i] = null;
			}
			interpolatorCount = 0;
			ResetDefaults();
			Pool<TweenInstance>.Recycle(this);
		}

		public void Step(TweenRunner runner)
		{
			curPercent = Mathf.MoveTowards(curPercent, dstPercent, Time.deltaTime * velPercent);
			interpolatePercent();
			if (curPercent == dstPercent)
			{
				runner.RemoveTween(this);
			}
		}

		public void interpolatePercent()
		{
			float percent;
			switch (smoothType)
			{
			case SmoothType.Linear:
				percent = curPercent;
				break;
			case SmoothType.Smooth:
				percent = Mathf.SmoothStep(0f, 1f, curPercent);
				break;
			case SmoothType.SmoothEnd:
				percent = 1f - (curPercent - 1f) * (curPercent - 1f);
				break;
			case SmoothType.SmoothStart:
				percent = curPercent * curPercent;
				break;
			default:
				percent = smoothFunction(curPercent);
				break;
			}
			int num = interpolatorCount;
			while (num-- != 0)
			{
				IInterpolator interpolator = interpolators[num];
				if (interpolator.isValid)
				{
					interpolators[num].Interpolate(percent);
				}
				else
				{
					interpolators[num] = interpolators[--interpolatorCount];
				}
			}
			if (OnProgress != null)
			{
				OnProgress(curPercent);
			}
		}
	}
}
