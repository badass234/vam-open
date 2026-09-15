using System;

namespace Leap.Unity.Animation.Internal
{
	public abstract class InterpolatorBase<ValueType, ObjType> : IInterpolator, IPoolable, IDisposable
	{
		protected ValueType _a;

		protected ValueType _b;

		protected ObjType _target;

		public abstract float length { get; }

		public abstract bool isValid { get; }

		public InterpolatorBase<ValueType, ObjType> Init(ValueType a, ValueType b, ObjType target)
		{
			_a = a;
			_b = b;
			_target = target;
			return this;
		}

		public abstract void Interpolate(float percent);

		public void OnSpawn()
		{
		}

		public void OnRecycle()
		{
		}

		public abstract void Dispose();
	}
}
