using System;

namespace Leap.Unity.Animation
{
	public interface IInterpolator : IPoolable, IDisposable
	{
		float length { get; }

		bool isValid { get; }

		void Interpolate(float percent);
	}
}
