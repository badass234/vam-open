using UnityEngine;

namespace Leap.Unity.Attributes
{
	public class UnitCurveAttribute : CurveBoundsAttribute
	{
		public UnitCurveAttribute()
			: base(new Rect(0f, 0f, 1f, 1f))
		{
		}
	}
}
