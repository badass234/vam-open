namespace Mono.CSharp
{
	internal class PropertyPatternMember
	{
		public string Name { get; private set; }

		public Expression Expr { get; private set; }

		public Location Location { get; private set; }

		public PropertyPatternMember(string name, Expression expr, Location loc)
		{
			Name = name;
			Expr = expr;
			Location = loc;
		}
	}
}
