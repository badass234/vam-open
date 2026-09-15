namespace Leap.Unity.Attributes
{
	public class MaxValue : CombinablePropertyAttribute, IPropertyConstrainer
	{
		public float maxValue;

		public MaxValue(float maxValue)
		{
			this.maxValue = maxValue;
		}
	}
}
