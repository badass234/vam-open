namespace Mono.CSharp
{
	public class Cast : ShimExpression
	{
		private Expression target_type;

		public Expression TargetType => target_type;

		public Cast(Expression cast_type, Expression expr, Location loc)
			: base(expr)
		{
			target_type = cast_type;
			base.loc = loc;
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			expr = expr.Resolve(ec);
			if (expr == null)
			{
				return null;
			}
			type = target_type.ResolveAsType(ec);
			if (type == null)
			{
				return null;
			}
			if (type.IsStatic)
			{
				ec.Report.Error(716, loc, "Cannot convert to static type `{0}'", type.GetSignatureForError());
				return null;
			}
			if (type.IsPointer && !ec.IsUnsafe)
			{
				Expression.UnsafeError(ec, loc);
			}
			eclass = ExprClass.Value;
			Constant constant = expr as Constant;
			if (constant != null)
			{
				constant = constant.Reduce(ec, type);
				if (constant != null)
				{
					return constant;
				}
			}
			Expression expression = Convert.ExplicitConversion(ec, expr, type, loc);
			if (expression == expr)
			{
				return EmptyCast.Create(expression, type);
			}
			return expression;
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			Cast obj = (Cast)t;
			obj.target_type = target_type.Clone(clonectx);
			obj.expr = expr.Clone(clonectx);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
