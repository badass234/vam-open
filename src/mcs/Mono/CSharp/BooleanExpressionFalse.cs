namespace Mono.CSharp
{
	public class BooleanExpressionFalse : Unary
	{
		public BooleanExpressionFalse(Expression expr)
			: base(Operator.LogicalNot, expr, expr.Location)
		{
		}

		protected override Expression ResolveOperator(ResolveContext ec, Expression expr)
		{
			return Expression.GetOperatorFalse(ec, expr, loc) ?? base.ResolveOperator(ec, expr);
		}
	}
}
