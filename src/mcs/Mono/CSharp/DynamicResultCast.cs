namespace Mono.CSharp
{
	public class DynamicResultCast : ShimExpression
	{
		public DynamicResultCast(TypeSpec type, Expression expr)
			: base(expr)
		{
			base.type = type;
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			expr = expr.Resolve(ec);
			eclass = ExprClass.Value;
			return this;
		}
	}
}
