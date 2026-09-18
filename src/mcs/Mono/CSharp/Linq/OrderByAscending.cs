namespace Mono.CSharp.Linq
{
	public class OrderByAscending : AQueryClause
	{
		protected override string MethodName => "OrderBy";

		public OrderByAscending(QueryBlock block, Expression expr)
			: base(block, expr, expr.Location)
		{
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
