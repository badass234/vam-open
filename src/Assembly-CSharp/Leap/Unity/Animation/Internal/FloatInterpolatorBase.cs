using UnityEngine;

namespace Leap.Unity.Animation.Internal
{
	public abstract class FloatInterpolatorBase<ObjType> : InterpolatorBase<float, ObjType>
	{
		public override float length => Mathf.Abs(_b);

		public new FloatInterpolatorBase<ObjType> Init(float a, float b, ObjType target)
		{
			_a = a;
			_b = b - a;
			_target = target;
			return this;
		}
	}
}
