namespace Mono.CSharp
{
	public class NamedArgument : MovableArgument
	{
		public readonly string Name;

		private readonly Location loc;

		public Location Location => loc;

		public NamedArgument(string name, Location loc, Expression expr)
			: this(name, loc, expr, AType.None)
		{
		}

		public NamedArgument(string name, Location loc, Expression expr, AType modifier)
			: base(expr, modifier)
		{
			Name = name;
			this.loc = loc;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			ec.Report.Error(853, loc, "An expression tree cannot contain named argument");
			return base.CreateExpressionTree(ec);
		}
	}
}
