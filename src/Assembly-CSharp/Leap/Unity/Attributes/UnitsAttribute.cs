namespace Leap.Unity.Attributes
{
	public class UnitsAttribute : CombinablePropertyAttribute, IAfterFieldAdditiveDrawer, IAdditiveDrawer
	{
		public readonly string unitsName;

		public UnitsAttribute(string unitsName)
		{
			this.unitsName = unitsName;
		}
	}
}
