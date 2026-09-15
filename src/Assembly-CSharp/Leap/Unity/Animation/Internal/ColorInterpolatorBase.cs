using UnityEngine;

namespace Leap.Unity.Animation.Internal
{
	public abstract class ColorInterpolatorBase<ObjType> : InterpolatorBase<Color, ObjType>
	{
		public override float length => ((Vector4)_b).magnitude;

		public new ColorInterpolatorBase<ObjType> Init(Color a, Color b, ObjType target)
		{
			_a = a;
			_b = b - a;
			_target = target;
			return this;
		}
	}
}
