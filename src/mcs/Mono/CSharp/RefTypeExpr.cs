using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class RefTypeExpr : ShimExpression
	{
		public RefTypeExpr(Expression expr, Location loc)
			: base(expr)
		{
			base.loc = loc;
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			expr = expr.Resolve(rc);
			if (expr == null)
			{
				return null;
			}
			expr = Convert.ImplicitConversionRequired(rc, expr, rc.Module.PredefinedTypes.TypedReference.Resolve(), loc);
			if (expr == null)
			{
				return null;
			}
			type = rc.BuiltinTypes.Type;
			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			expr.Emit(ec);
			ec.Emit(OpCodes.Refanytype);
			MethodSpec methodSpec = ec.Module.PredefinedMembers.TypeGetTypeFromHandle.Resolve(loc);
			if (methodSpec != null)
			{
				ec.Emit(OpCodes.Call, methodSpec);
			}
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
