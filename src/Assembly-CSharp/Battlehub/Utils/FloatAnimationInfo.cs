using System;

namespace Battlehub.Utils
{
	public class FloatAnimationInfo : AnimationInfo<object, float>
	{
		public FloatAnimationInfo(float from, float to, float duration, Func<float, float> easingFunction, AnimationCallback<object, float> callback, object target = null)
			: base(from, to, duration, easingFunction, callback, target)
		{
		}

		protected override float Lerp(float from, float to, float t)
		{
			return to * t + from * (1f - t);
		}
	}
}
