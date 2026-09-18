namespace Mono.CSharp
{
	public abstract class CompositeExpression : Expression
	{
		protected Expression expr;

		public Expression Child => expr;

		public override bool IsNull => expr.IsNull;

		protected CompositeExpression(Expression expr)
		{
			this.expr = expr;
			loc = expr.Location;
		}

		public override bool ContainsEmitWithAwait()
		{
			return expr.ContainsEmitWithAwait();
		}

		public override Expression CreateExpressionTree(ResolveContext rc)
		{
			return expr.CreateExpressionTree(rc);
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			expr = expr.Resolve(rc);
			if (expr == null)
			{
				return null;
			}
			type = expr.Type;
			eclass = expr.eclass;
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			expr.Emit(ec);
		}
	}
}
