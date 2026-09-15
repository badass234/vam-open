using UnityEngine;

namespace Leap.Unity.Attributes
{
	public class CurveBoundsAttribute : CombinablePropertyAttribute, IFullPropertyDrawer
	{
		public readonly Rect bounds;

		public CurveBoundsAttribute(Rect bounds)
		{
			this.bounds = bounds;
		}

		public CurveBoundsAttribute(float width, float height)
		{
			bounds = new Rect(0f, 0f, width, height);
		}
	}
}
