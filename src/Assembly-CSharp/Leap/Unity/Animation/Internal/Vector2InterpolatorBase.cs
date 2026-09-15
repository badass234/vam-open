using UnityEngine;

namespace Leap.Unity.Animation.Internal
{
	public abstract class Vector2InterpolatorBase<ObjType> : InterpolatorBase<Vector2, ObjType>
	{
		public override float length => _b.magnitude;

		public new Vector2InterpolatorBase<ObjType> Init(Vector2 a, Vector2 b, ObjType target)
		{
			_a = a;
			_b = b - a;
			_target = target;
			return this;
		}
	}
}
