namespace Leap.Unity.Attributes
{
	public class OnEditorChangeAttribute : CombinablePropertyAttribute
	{
		public readonly string methodName;

		public OnEditorChangeAttribute(string methodName)
		{
			this.methodName = methodName;
		}
	}
}
