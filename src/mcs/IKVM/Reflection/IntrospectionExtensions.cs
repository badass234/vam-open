namespace IKVM.Reflection
{
	public static class IntrospectionExtensions
	{
		public static TypeInfo GetTypeInfo(Type type)
		{
			return type.GetTypeInfo();
		}
	}
}
