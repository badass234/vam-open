namespace IKVM.Reflection
{
	internal interface IGenericContext
	{
		Type GetGenericTypeArgument(int index);

		Type GetGenericMethodArgument(int index);
	}
}
