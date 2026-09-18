namespace IKVM.Reflection
{
	internal interface IGenericBinder
	{
		Type BindTypeParameter(Type type);

		Type BindMethodParameter(Type type);
	}
}
