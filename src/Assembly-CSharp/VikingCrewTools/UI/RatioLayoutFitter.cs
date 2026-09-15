using UnityEngine;
using UnityEngine.UI;

namespace VikingCrewTools.UI
{
	[RequireComponent(typeof(ContentSizeFitter))]
	public class RatioLayoutFitter : HorizontalLayoutGroup
	{
		[Header("Use these to set the ratio [0..1] of child size needed as padding on each side")]
		public Vector2 startPad = Vector2.zero;

		public Vector2 stopPad = Vector2.zero;

		public RectTransform childToFit;

		public override void CalculateLayoutInputHorizontal()
		{
			float num = LayoutUtility.GetPreferredWidth(childToFit);
			float num2 = LayoutUtility.GetPreferredHeight(childToFit);
			base.padding.left = (int)(num * startPad.x / (1f - startPad.x - stopPad.x));
			base.padding.right = (int)(num * stopPad.x / (1f - startPad.x - stopPad.x));
			base.padding.top = (int)(num2 * startPad.y / (1f - startPad.y - stopPad.y));
			base.padding.bottom = (int)(num2 * stopPad.y / (1f - startPad.y - stopPad.y));
			base.CalculateLayoutInputHorizontal();
		}
	}
}
