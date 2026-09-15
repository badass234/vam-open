using System;
using UnityEngine;

namespace Leap.Unity.Animation.Internal
{
	public abstract class GradientInterpolatorBase : IInterpolator, IPoolable, IDisposable
	{
		protected Gradient _gradient;

		public float length => 1f;

		public abstract bool isValid { get; }

		public GradientInterpolatorBase Init(Gradient gradient)
		{
			_gradient = gradient;
			return this;
		}

		public abstract void Interpolate(float percent);

		public void OnSpawn()
		{
		}

		public void Dispose()
		{
		}

		public void OnRecycle()
		{
		}
	}
}
