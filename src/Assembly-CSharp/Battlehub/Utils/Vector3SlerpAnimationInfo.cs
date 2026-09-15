using System;
using UnityEngine;

namespace Battlehub.Utils
{
	public class Vector3SlerpAnimationInfo : AnimationInfo<object, Vector3>
	{
		public Vector3SlerpAnimationInfo(Vector3 from, Vector3 to, float duration, Func<float, float> easingFunction, AnimationCallback<object, Vector3> callback, object target = null)
			: base(from, to, duration, easingFunction, callback, target)
		{
		}

		protected override Vector3 Lerp(Vector3 from, Vector3 to, float t)
		{
			return Vector3.Slerp(from, to, t);
		}
	}
}
