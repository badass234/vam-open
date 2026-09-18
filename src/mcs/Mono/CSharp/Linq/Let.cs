namespace Mono.CSharp.Linq
{
	public class Let : ARangeVariableQueryClause
	{
		protected override string MethodName => "Select";

		public Let(QueryBlock block, RangeVariable identifier, Expression expr, Location loc)
			: base(block, identifier, expr, loc)
		{
		}

		protected override void CreateArguments(ResolveContext ec, Parameter parameter, ref Arguments args)
		{
			expr = ARangeVariableQueryClause.CreateRangeVariableType(ec, parameter, identifier, expr);
			base.CreateArguments(ec, parameter, ref args);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
