using UnityEngine;

namespace Leap.Unity.Animation.Internal
{
	public abstract class Vector4InterpolatorBase<ObjType> : InterpolatorBase<Vector4, ObjType>
	{
		public override float length => _b.magnitude;

		public new Vector4InterpolatorBase<ObjType> Init(Vector4 a, Vector4 b, ObjType target)
		{
			_a = a;
			_b = b - a;
			_target = target;
			return this;
		}
	}
}
