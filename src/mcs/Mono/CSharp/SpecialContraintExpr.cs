using System;

namespace Mono.CSharp
{
	public class SpecialContraintExpr : FullNamedExpression
	{
		public SpecialConstraint Constraint { get; private set; }

		public SpecialContraintExpr(SpecialConstraint constraint, Location loc)
		{
			base.loc = loc;
			Constraint = constraint;
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			throw new NotImplementedException();
		}

		public override FullNamedExpression ResolveAsTypeOrNamespace(IMemberContext mc, bool allowUnboundTypeArguments)
		{
			throw new NotImplementedException();
		}
	}
}
