namespace Mono.CSharp
{
	internal class PrimaryConstructorField : Field
	{
		private sealed class TypeExpressionFromParameter : TypeExpr
		{
			private Parameter parameter;

			public TypeExpressionFromParameter(Parameter parameter)
			{
				this.parameter = parameter;
				eclass = ExprClass.Type;
				loc = parameter.Location;
			}

			public override TypeSpec ResolveAsType(IMemberContext mc, bool allowUnboundTypeArguments)
			{
				return parameter.Type;
			}
		}

		public PrimaryConstructorField(TypeDefinition parent, Parameter parameter)
			: base(parent, new TypeExpressionFromParameter(parameter), Modifiers.PRIVATE, new MemberName(parameter.Name, parameter.Location), null)
		{
			caching_flags |= Flags.IsUsed | Flags.IsAssigned;
		}

		public override string GetSignatureForError()
		{
			return base.MemberName.Name;
		}
	}
}
