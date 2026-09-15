using UnityEngine;

namespace Leap.Unity.Animation.Internal
{
	public abstract class Vector3InterpolatorBase<ObjType> : InterpolatorBase<Vector3, ObjType>
	{
		public override float length => _b.magnitude;

		public new Vector3InterpolatorBase<ObjType> Init(Vector3 a, Vector3 b, ObjType target)
		{
			_a = a;
			_b = b - a;
			_target = target;
			return this;
		}
	}
}
