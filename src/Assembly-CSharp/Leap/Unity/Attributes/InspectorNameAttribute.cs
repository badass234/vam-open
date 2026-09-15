namespace Leap.Unity.Attributes
{
	public class InspectorNameAttribute : CombinablePropertyAttribute, IFullPropertyDrawer
	{
		public readonly string name;

		public InspectorNameAttribute(string name)
		{
			this.name = name;
		}
	}
}
