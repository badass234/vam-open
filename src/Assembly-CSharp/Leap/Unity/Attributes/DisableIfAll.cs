namespace Leap.Unity.Attributes
{
	public class DisableIfAll : DisableIfBase
	{
		public DisableIfAll(string propertyName1, string propertyName2, object areEqualTo = null, object areNotEqualTo = null)
			: base(areEqualTo, areNotEqualTo, true, propertyName1, propertyName2)
		{
		}

		public DisableIfAll(string propertyName1, string propertyName2, string propertyName3, object areEqualTo = null, object areNotEqualTo = null)
			: base(areEqualTo, areNotEqualTo, true, propertyName1, propertyName2, propertyName3)
		{
		}

		public DisableIfAll(string propertyName1, string propertyName2, string propertyName3, string propertyName4, object areEqualTo = null, object areNotEqualTo = null)
			: base(areEqualTo, areNotEqualTo, true, propertyName1, propertyName2, propertyName3, propertyName4)
		{
		}
	}
}
