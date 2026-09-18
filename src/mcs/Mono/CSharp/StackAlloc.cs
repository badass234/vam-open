using System;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class StackAlloc : Expression
	{
		private TypeSpec otype;

		private Expression texpr;

		private Expression count;

		public Expression TypeExpression => texpr;

		public Expression CountExpression => count;

		public StackAlloc(Expression type, Expression count, Location l)
		{
			texpr = type;
			this.count = count;
			loc = l;
		}

		public override bool ContainsEmitWithAwait()
		{
			return false;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			throw new NotSupportedException("ET");
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			count = count.Resolve(ec);
			if (count == null)
			{
				return null;
			}
			if (count.Type.BuiltinType != BuiltinTypeSpec.Type.UInt)
			{
				count = Convert.ImplicitConversionRequired(ec, count, ec.BuiltinTypes.Int, loc);
				if (count == null)
				{
					return null;
				}
			}
			Constant constant = count as Constant;
			if (constant != null && constant.IsNegative)
			{
				ec.Report.Error(247, loc, "Cannot use a negative size with stackalloc");
			}
			if (ec.HasAny(ResolveContext.Options.CatchScope | ResolveContext.Options.FinallyScope))
			{
				ec.Report.Error(255, loc, "Cannot use stackalloc in finally or catch");
			}
			otype = texpr.ResolveAsType(ec);
			if (otype == null)
			{
				return null;
			}
			if (!TypeManager.VerifyUnmanaged(ec.Module, otype, loc))
			{
				return null;
			}
			type = PointerContainer.MakeType(ec.Module, otype);
			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			int size = BuiltinTypeSpec.GetSize(otype);
			count.Emit(ec);
			if (size == 0)
			{
				ec.Emit(OpCodes.Sizeof, otype);
			}
			else
			{
				ec.EmitInt(size);
			}
			ec.Emit(OpCodes.Mul_Ovf_Un);
			ec.Emit(OpCodes.Localloc);
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			StackAlloc obj = (StackAlloc)t;
			obj.count = count.Clone(clonectx);
			obj.texpr = texpr.Clone(clonectx);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
