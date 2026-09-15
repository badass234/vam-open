namespace Leap.Unity.Attributes
{
	public class MinValue : CombinablePropertyAttribute, IPropertyConstrainer
	{
		public float minValue;

		public MinValue(float minValue)
		{
			this.minValue = minValue;
		}
	}
}
