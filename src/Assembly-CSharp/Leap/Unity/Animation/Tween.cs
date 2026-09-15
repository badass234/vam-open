using System;
using Leap.Unity.Animation.Internal;
using UnityEngine;

namespace Leap.Unity.Animation
{
	public struct Tween
	{
		private class FloatInterpolator : FloatInterpolatorBase<Action<float>>
		{
			public override bool isValid => true;

			public override void Interpolate(float percent)
			{
				_target(_a + _b * percent);
			}

			public override void Dispose()
			{
				_target = null;
				Pool<FloatInterpolator>.Recycle(this);
			}
		}

		private class Vector2Interpolator : Vector2InterpolatorBase<Action<Vector2>>
		{
			public override bool isValid => true;

			public override void Interpolate(float percent)
			{
				_target(_a + _b * percent);
			}

			public override void Dispose()
			{
				_target = null;
				Pool<Vector2Interpolator>.Recycle(this);
			}
		}

		private class Vector3Interpolator : Vector3InterpolatorBase<Action<Vector3>>
		{
			public override bool isValid => true;

			public override void Interpolate(float percent)
			{
				_target(_a + _b * percent);
			}

			public override void Dispose()
			{
				_target = null;
				Pool<Vector3Interpolator>.Recycle(this);
			}
		}

		private class QuaternionInterpolator : QuaternionInterpolatorBase<Action<Quaternion>>
		{
			public override bool isValid => true;

			public override void Interpolate(float percent)
			{
				_target(Quaternion.Slerp(_a, _b, percent));
			}

			public override void Dispose()
			{
				_target = null;
				Pool<QuaternionInterpolator>.Recycle(this);
			}
		}

		private class ColorInterpolator : ColorInterpolatorBase<Action<Color>>
		{
			public override bool isValid => true;

			public override void Interpolate(float percent)
			{
				_target(_a + _b * percent);
			}

			public override void Dispose()
			{
				_target = null;
				Pool<ColorInterpolator>.Recycle(this);
			}
		}

		private int _id;

		private TweenInstance _instance;

		public bool isValid => _instance != null && _id == _instance.instanceId;

		public bool isRunning
		{
			get
			{
				throwIfInvalid();
				return _instance.runnerIndex != -1;
			}
		}

		public Direction direction
		{
			get
			{
				throwIfInvalid();
				return _instance.direction;
			}
			set
			{
				throwIfInvalid();
				_instance.direction = value;
				_instance.dstPercent = ((value != Direction.Backward) ? 1 : 0);
			}
		}

		public float timeLeft
		{
			get
			{
				throwIfInvalid();
				return Mathf.Abs((_instance.curPercent - _instance.dstPercent) / _instance.velPercent);
			}
		}

		public float progress
		{
			get
			{
				throwIfInvalid();
				return _instance.curPercent;
			}
			set
			{
				throwIfInvalid();
				if (_instance.curPercent == value)
				{
					return;
				}
				if (value < 0f || value > 1f)
				{
					throw new ArgumentException("Progress must be a value from 0 - 1");
				}
				if (_instance.curPercent == 0f)
				{
					if (_instance.OnLeaveStart != null)
					{
						_instance.OnLeaveStart();
					}
				}
				else if (_instance.curPercent == 1f && _instance.OnLeaveEnd != null)
				{
					_instance.OnLeaveEnd();
				}
				_instance.curPercent = value;
				if (_instance.curPercent == 0f)
				{
					if (_instance.OnReachStart != null)
					{
						_instance.OnReachStart();
					}
				}
				else if (_instance.curPercent == 1f && _instance.OnReachEnd != null)
				{
					_instance.OnReachEnd();
				}
				if (_instance.runnerIndex == -1)
				{
					_instance.interpolatePercent();
				}
			}
		}

		private Tween(bool isSingle)
		{
			_instance = Pool<TweenInstance>.Spawn();
			_id = _instance.instanceId;
			_instance.returnToPoolUponStop = isSingle;
		}

		public MaterialSelector Target(Material material)
		{
			return new MaterialSelector(material, this);
		}

		public TransformSelector Target(Transform transform)
		{
			return new TransformSelector(transform, this);
		}

		public Tween Value(float a, float b, Action<float> onValue)
		{
			AddInterpolator(Pool<FloatInterpolator>.Spawn().Init(a, b, onValue));
			return this;
		}

		public Tween Value(Vector2 a, Vector2 b, Action<Vector2> onValue)
		{
			AddInterpolator(Pool<Vector2Interpolator>.Spawn().Init(a, b, onValue));
			return this;
		}

		public Tween Value(Vector3 a, Vector3 b, Action<Vector3> onValue)
		{
			AddInterpolator(Pool<Vector3Interpolator>.Spawn().Init(a, b, onValue));
			return this;
		}

		public Tween Value(Quaternion a, Quaternion b, Action<Quaternion> onValue)
		{
			AddInterpolator(Pool<QuaternionInterpolator>.Spawn().Init(a, b, onValue));
			return this;
		}

		public Tween Value(Color a, Color b, Action<Color> onValue)
		{
			AddInterpolator(Pool<ColorInterpolator>.Spawn().Init(a, b, onValue));
			return this;
		}

		public static Tween Single()
		{
			return new Tween(true);
		}

		public static Tween Persistent()
		{
			return new Tween(false);
		}

		public static Tween AfterDelay(float delay, Action onReachEnd)
		{
			return Single().Value(0f, 1f, delegate
			{
			}).OverTime(delay).OnReachEnd(onReachEnd)
				.Play();
		}

		public Tween AddInterpolator(IInterpolator interpolator)
		{
			throwIfInvalid();
			if (_instance.interpolatorCount >= _instance.interpolators.Length)
			{
				Utils.DoubleCapacity(ref _instance.interpolators);
			}
			_instance.interpolators[_instance.interpolatorCount++] = interpolator;
			return this;
		}

		public Tween OverTime(float seconds)
		{
			throwIfInvalid();
			_instance.velPercent = 1f / seconds;
			return this;
		}

		public Tween AtRate(float unitsPerSecond)
		{
			throwIfInvalid();
			_instance.velPercent = unitsPerSecond / _instance.interpolators[0].length;
			return this;
		}

		public Tween Smooth(SmoothType type = SmoothType.Smooth)
		{
			throwIfInvalid();
			_instance.smoothType = type;
			_instance.smoothFunction = null;
			return this;
		}

		public Tween Smooth(AnimationCurve curve)
		{
			throwIfInvalid();
			_instance.smoothType = (SmoothType)0;
			_instance.smoothFunction = curve.Evaluate;
			return this;
		}

		public Tween Smooth(Func<float, float> smoothFunction)
		{
			throwIfInvalid();
			_instance.smoothType = (SmoothType)0;
			_instance.smoothFunction = smoothFunction;
			return this;
		}

		public Tween OnProgress(Action<float> action)
		{
			throwIfInvalid();
			TweenInstance instance = _instance;
			instance.OnProgress = (Action<float>)Delegate.Combine(instance.OnProgress, action);
			return this;
		}

		public Tween OnLeaveStart(Action action)
		{
			throwIfInvalid();
			TweenInstance instance = _instance;
			instance.OnLeaveStart = (Action)Delegate.Combine(instance.OnLeaveStart, action);
			return this;
		}

		public Tween OnReachStart(Action action)
		{
			throwIfInvalid();
			TweenInstance instance = _instance;
			instance.OnReachStart = (Action)Delegate.Combine(instance.OnReachStart, action);
			return this;
		}

		public Tween OnLeaveEnd(Action action)
		{
			throwIfInvalid();
			TweenInstance instance = _instance;
			instance.OnLeaveEnd = (Action)Delegate.Combine(instance.OnLeaveEnd, action);
			return this;
		}

		public Tween OnReachEnd(Action action)
		{
			throwIfInvalid();
			TweenInstance instance = _instance;
			instance.OnReachEnd = (Action)Delegate.Combine(instance.OnReachEnd, action);
			return this;
		}

		public Tween Play()
		{
			throwIfInvalid();
			if (_instance.curPercent == _instance.dstPercent)
			{
				return this;
			}
			if (_instance.runnerIndex != -1)
			{
				return this;
			}
			TweenRunner.instance.AddTween(_instance);
			return this;
		}

		public Tween Play(Direction direction)
		{
			throwIfInvalid();
			this.direction = direction;
			Play();
			return this;
		}

		public Tween Play(float destinationPercent)
		{
			throwIfInvalid();
			if (destinationPercent < 0f || destinationPercent > 1f)
			{
				throw new ArgumentException("Destination percent must be within the range [0-1]");
			}
			direction = ((destinationPercent >= _instance.curPercent) ? Direction.Forward : Direction.Backward);
			_instance.dstPercent = destinationPercent;
			Play();
			return this;
		}

		public TweenInstance.TweenYieldInstruction Yield()
		{
			throwIfInvalid();
			return _instance.yieldInstruction;
		}

		public void Pause()
		{
			throwIfInvalid();
			if (_instance.runnerIndex != -1)
			{
				TweenRunner.instance.RemoveTween(_instance);
			}
		}

		public void Stop()
		{
			throwIfInvalid();
			progress = 0f;
			direction = Direction.Forward;
			Pause();
			if (isValid && _instance.returnToPoolUponStop)
			{
				Release();
			}
		}

		public void Release()
		{
			throwIfInvalid();
			Pause();
			TweenRunner.instance.ScheduleForRecycle(_instance);
		}

		private void throwIfInvalid()
		{
			if (!isValid)
			{
				if (_id == 0)
				{
					throw new InvalidOperationException("Tween is invalid.  Make sure you use Tween.Single or Tween.Persistant to create your Tween instead of the default constructor.");
				}
				throw new InvalidOperationException("Tween is invalid or was recycled.  Make sure to use Tween.Persistant if you want to keep a tween around after it finishes playing.");
			}
		}
	}
}
