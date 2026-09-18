using System;

namespace Mono.CSharp
{
	internal abstract class PatternExpression : Expression
	{
		protected PatternExpression(Location loc)
		{
			base.loc = loc;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			throw new NotImplementedException();
		}
	}
}
