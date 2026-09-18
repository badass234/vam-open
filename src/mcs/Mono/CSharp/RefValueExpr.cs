using System;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class RefValueExpr : ShimExpression, IAssignMethod, IMemoryLocation
	{
		private FullNamedExpression texpr;

		public FullNamedExpression TypeExpression => texpr;

		public RefValueExpr(Expression expr, FullNamedExpression texpr, Location loc)
			: base(expr)
		{
			this.texpr = texpr;
			base.loc = loc;
		}

		public override bool ContainsEmitWithAwait()
		{
			return false;
		}

		public void AddressOf(EmitContext ec, AddressOp mode)
		{
			expr.Emit(ec);
			ec.Emit(OpCodes.Refanyval, type);
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			expr = expr.Resolve(rc);
			type = texpr.ResolveAsType(rc);
			if (expr == null || type == null)
			{
				return null;
			}
			expr = Convert.ImplicitConversionRequired(rc, expr, rc.Module.PredefinedTypes.TypedReference.Resolve(), loc);
			eclass = ExprClass.Variable;
			return this;
		}

		public override Expression DoResolveLValue(ResolveContext rc, Expression right_side)
		{
			return DoResolve(rc);
		}

		public override void Emit(EmitContext ec)
		{
			expr.Emit(ec);
			ec.Emit(OpCodes.Refanyval, type);
			ec.EmitLoadFromPtr(type);
		}

		public void Emit(EmitContext ec, bool leave_copy)
		{
			throw new NotImplementedException();
		}

		public void EmitAssign(EmitContext ec, Expression source, bool leave_copy, bool isCompound)
		{
			expr.Emit(ec);
			ec.Emit(OpCodes.Refanyval, type);
			source.Emit(ec);
			LocalTemporary localTemporary = null;
			if (leave_copy)
			{
				ec.Emit(OpCodes.Dup);
				localTemporary = new LocalTemporary(source.Type);
				localTemporary.Store(ec);
			}
			ec.EmitStoreFromPtr(type);
			if (localTemporary != null)
			{
				localTemporary.Emit(ec);
				localTemporary.Release(ec);
			}
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
