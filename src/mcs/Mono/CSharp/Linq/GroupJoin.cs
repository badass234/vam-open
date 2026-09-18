namespace Mono.CSharp.Linq
{
	public class GroupJoin : Join
	{
		private readonly RangeVariable into;

		protected override string MethodName => "GroupJoin";

		public GroupJoin(QueryBlock block, RangeVariable lt, Expression inner, QueryBlock outerSelector, QueryBlock innerSelector, RangeVariable into, Location loc)
			: base(block, lt, inner, outerSelector, innerSelector, loc)
		{
			this.into = into;
		}

		protected override RangeVariable GetIntoVariable()
		{
			return into;
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
