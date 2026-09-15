using System;
using UnityEngine;

namespace Battlehub.Utils
{
	public class QuaternionAnimationInfo : AnimationInfo<object, Quaternion>
	{
		public QuaternionAnimationInfo(Quaternion from, Quaternion to, float duration, Func<float, float> easingFunction, AnimationCallback<object, Quaternion> callback, object target = null)
			: base(from, to, duration, easingFunction, callback, target)
		{
		}

		protected override Quaternion Lerp(Quaternion from, Quaternion to, float t)
		{
			return Quaternion.Lerp(from, to, t);
		}
	}
}
