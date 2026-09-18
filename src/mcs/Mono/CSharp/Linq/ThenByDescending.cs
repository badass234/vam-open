namespace Mono.CSharp.Linq
{
	public class ThenByDescending : OrderByDescending
	{
		protected override string MethodName => "ThenByDescending";

		public ThenByDescending(QueryBlock block, Expression expr)
			: base(block, expr)
		{
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
