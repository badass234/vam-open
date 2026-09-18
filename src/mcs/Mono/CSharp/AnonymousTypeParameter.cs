namespace Mono.CSharp
{
	public class AnonymousTypeParameter : ShimExpression
	{
		public readonly string Name;

		public AnonymousTypeParameter(Expression initializer, string name, Location loc)
			: base(initializer)
		{
			Name = name;
			base.loc = loc;
		}

		public AnonymousTypeParameter(Parameter parameter)
			: base(new SimpleName(parameter.Name, parameter.Location))
		{
			Name = parameter.Name;
			loc = parameter.Location;
		}

		public override bool Equals(object o)
		{
			AnonymousTypeParameter anonymousTypeParameter = o as AnonymousTypeParameter;
			if (anonymousTypeParameter != null)
			{
				return Name == anonymousTypeParameter.Name;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			Expression expression = expr.Resolve(ec);
			if (expression == null)
			{
				return null;
			}
			if (expression.eclass == ExprClass.MethodGroup)
			{
				Error_InvalidInitializer(ec, expression.ExprClassName);
				return null;
			}
			type = expression.Type;
			if (type.Kind == MemberKind.Void || type == InternalType.NullLiteral || type == InternalType.AnonymousMethod || type.IsPointer)
			{
				Error_InvalidInitializer(ec, type.GetSignatureForError());
				return null;
			}
			return expression;
		}

		protected virtual void Error_InvalidInitializer(ResolveContext ec, string initializer)
		{
			ec.Report.Error(828, loc, "An anonymous type property `{0}' cannot be initialized with `{1}'", Name, initializer);
		}
	}
}
