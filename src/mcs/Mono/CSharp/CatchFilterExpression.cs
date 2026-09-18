namespace Mono.CSharp
{
	public class CatchFilterExpression : BooleanExpression
	{
		public CatchFilterExpression(Expression expr, Location loc)
			: base(expr)
		{
			base.loc = loc;
		}
	}
}
