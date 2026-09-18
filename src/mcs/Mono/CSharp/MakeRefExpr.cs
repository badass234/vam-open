using System;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class MakeRefExpr : ShimExpression
	{
		public MakeRefExpr(Expression expr, Location loc)
			: base(expr)
		{
			base.loc = loc;
		}

		public override bool ContainsEmitWithAwait()
		{
			throw new NotImplementedException();
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			expr = expr.ResolveLValue(rc, EmptyExpression.LValueMemberAccess);
			type = rc.Module.PredefinedTypes.TypedReference.Resolve();
			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			((IMemoryLocation)expr).AddressOf(ec, AddressOp.Load);
			ec.Emit(OpCodes.Mkrefany, expr.Type);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
