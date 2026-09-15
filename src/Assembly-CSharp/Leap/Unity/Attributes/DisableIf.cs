namespace Leap.Unity.Attributes
{
	public class DisableIf : DisableIfBase
	{
		public DisableIf(string propertyName, object isEqualTo = null, object isNotEqualTo = null)
			: base(isEqualTo, isNotEqualTo, true, propertyName)
		{
		}
	}
}
