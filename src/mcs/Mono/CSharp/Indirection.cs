using System;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class Indirection : Expression, IMemoryLocation, IAssignMethod, IFixedExpression
	{
		private Expression expr;

		private LocalTemporary temporary;

		private bool prepared;

		public Expression Expr => expr;

		public bool IsFixed => true;

		public override Location StartLocation => expr.StartLocation;

		public Indirection(Expression expr, Location l)
		{
			this.expr = expr;
			loc = l;
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			((Indirection)t).expr = expr.Clone(clonectx);
		}

		public override bool ContainsEmitWithAwait()
		{
			throw new NotImplementedException();
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			Error_PointerInsideExpressionTree(ec);
			return null;
		}

		public override void Emit(EmitContext ec)
		{
			if (!prepared)
			{
				expr.Emit(ec);
			}
			ec.EmitLoadFromPtr(base.Type);
		}

		public void Emit(EmitContext ec, bool leave_copy)
		{
			Emit(ec);
			if (leave_copy)
			{
				ec.Emit(OpCodes.Dup);
				temporary = new LocalTemporary(expr.Type);
				temporary.Store(ec);
			}
		}

		public void EmitAssign(EmitContext ec, Expression source, bool leave_copy, bool isCompound)
		{
			prepared = isCompound;
			expr.Emit(ec);
			if (isCompound)
			{
				ec.Emit(OpCodes.Dup);
			}
			source.Emit(ec);
			if (leave_copy)
			{
				ec.Emit(OpCodes.Dup);
				temporary = new LocalTemporary(source.Type);
				temporary.Store(ec);
			}
			ec.EmitStoreFromPtr(type);
			if (temporary != null)
			{
				temporary.Emit(ec);
				temporary.Release(ec);
			}
		}

		public void AddressOf(EmitContext ec, AddressOp Mode)
		{
			expr.Emit(ec);
		}

		public override Expression DoResolveLValue(ResolveContext ec, Expression right_side)
		{
			return DoResolve(ec);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			expr = expr.Resolve(ec);
			if (expr == null)
			{
				return null;
			}
			if (!ec.IsUnsafe)
			{
				Expression.UnsafeError(ec, loc);
			}
			PointerContainer pointerContainer = expr.Type as PointerContainer;
			if (pointerContainer == null)
			{
				ec.Report.Error(193, loc, "The * or -> operator must be applied to a pointer");
				return null;
			}
			type = pointerContainer.Element;
			if (type.Kind == MemberKind.Void)
			{
				Error_VoidPointerOperation(ec);
				return null;
			}
			eclass = ExprClass.Variable;
			return this;
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
