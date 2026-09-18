namespace Mono.CSharp
{
	public class DocumentationParameter
	{
		public readonly Parameter.Modifier Modifier;

		public FullNamedExpression Type;

		private TypeSpec type;

		public TypeSpec TypeSpec => type;

		public DocumentationParameter(Parameter.Modifier modifier, FullNamedExpression type)
			: this(type)
		{
			Modifier = modifier;
		}

		public DocumentationParameter(FullNamedExpression type)
		{
			Type = type;
		}

		public void Resolve(IMemberContext context)
		{
			type = Type.ResolveAsType(context);
		}
	}
}
