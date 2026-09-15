using UnityEngine;

namespace Leap.Unity.Animation.Internal
{
	public abstract class QuaternionInterpolatorBase<ObjType> : InterpolatorBase<Quaternion, ObjType>
	{
		public override float length => Quaternion.Angle(_a, _b);

		public new QuaternionInterpolatorBase<ObjType> Init(Quaternion a, Quaternion b, ObjType target)
		{
			_a = a;
			_b = b;
			_target = target;
			return this;
		}
	}
}
