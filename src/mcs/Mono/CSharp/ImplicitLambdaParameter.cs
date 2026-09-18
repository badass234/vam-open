namespace Mono.CSharp
{
	public class ImplicitLambdaParameter : Parameter
	{
		public ImplicitLambdaParameter(string name, Location loc)
			: base(null, name, Modifier.NONE, null, loc)
		{
		}

		public override TypeSpec Resolve(IMemberContext ec, int index)
		{
			if (parameter_type == null)
			{
				throw new InternalErrorException("A type of implicit lambda parameter `{0}' is not set", base.Name);
			}
			idx = index;
			return parameter_type;
		}

		public void SetParameterType(TypeSpec type)
		{
			parameter_type = type;
		}
	}
}
