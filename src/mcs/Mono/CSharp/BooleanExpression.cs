namespace Mono.CSharp
{
	public class BooleanExpression : ShimExpression
	{
		public BooleanExpression(Expression expr)
			: base(expr)
		{
			loc = expr.Location;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			return base.CreateExpressionTree(ec);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			expr = expr.Resolve(ec);
			if (expr == null)
			{
				return null;
			}
			Assign assign = expr as Assign;
			if (assign != null && assign.Source is Constant)
			{
				ec.Report.Warning(665, 3, loc, "Assignment in conditional expression is always constant. Did you mean to use `==' instead ?");
			}
			if (expr.Type.BuiltinType == BuiltinTypeSpec.Type.FirstPrimitive)
			{
				return expr;
			}
			if (expr.Type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				Arguments arguments = new Arguments(1);
				arguments.Add(new Argument(expr));
				return DynamicUnaryConversion.CreateIsTrue(ec, arguments, loc).Resolve(ec);
			}
			type = ec.BuiltinTypes.Bool;
			Expression expression = Convert.ImplicitConversion(ec, expr, type, loc);
			if (expression != null)
			{
				return expression;
			}
			expression = Expression.GetOperatorTrue(ec, expr, loc);
			if (expression == null)
			{
				expr.Error_ValueCannotBeConverted(ec, type, false);
				return null;
			}
			return expression;
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
