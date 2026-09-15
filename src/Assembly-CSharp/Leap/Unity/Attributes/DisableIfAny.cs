namespace Leap.Unity.Attributes
{
	public class DisableIfAny : DisableIfBase
	{
		public DisableIfAny(string propertyName1, string propertyName2, object areEqualTo = null, object areNotEqualTo = null)
			: base(areEqualTo, areNotEqualTo, false, propertyName1, propertyName2)
		{
		}

		public DisableIfAny(string propertyName1, string propertyName2, string propertyName3, object areEqualTo = null, object areNotEqualTo = null)
			: base(areEqualTo, areNotEqualTo, false, propertyName1, propertyName2, propertyName3)
		{
		}

		public DisableIfAny(string propertyName1, string propertyName2, string propertyName3, string propertyName4, object areEqualTo = null, object areNotEqualTo = null)
			: base(areEqualTo, areNotEqualTo, false, propertyName1, propertyName2, propertyName3, propertyName4)
		{
		}
	}
}
