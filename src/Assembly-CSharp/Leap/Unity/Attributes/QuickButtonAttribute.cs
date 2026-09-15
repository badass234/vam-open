namespace Leap.Unity.Attributes
{
	public class QuickButtonAttribute : CombinablePropertyAttribute, IAfterFieldAdditiveDrawer, IAdditiveDrawer
	{
		public const float PADDING_RIGHT = 12f;

		public readonly string label = "Quick Button";

		public readonly string methodOnPress;

		public readonly string tooltip = string.Empty;

		public QuickButtonAttribute(string buttonLabel, string methodOnPress, string tooltip = "")
		{
			label = buttonLabel;
			this.methodOnPress = methodOnPress;
			this.tooltip = tooltip;
		}
	}
}
