namespace Mono.CSharp
{
	public class ImplicitCast : ShimExpression
	{
		private bool arrayAccess;

		public ImplicitCast(Expression expr, TypeSpec target, bool arrayAccess)
			: base(expr)
		{
			loc = expr.Location;
			type = target;
			this.arrayAccess = arrayAccess;
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			expr = expr.Resolve(ec);
			if (expr == null)
			{
				return null;
			}
			if (arrayAccess)
			{
				expr = ConvertExpressionToArrayIndex(ec, expr);
			}
			else
			{
				expr = Convert.ImplicitConversionRequired(ec, expr, type, loc);
			}
			return expr;
		}
	}
}
