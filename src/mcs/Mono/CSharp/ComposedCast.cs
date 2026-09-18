using System;
using Mono.CSharp.Nullable;

namespace Mono.CSharp
{
	public class ComposedCast : TypeExpr
	{
		private FullNamedExpression left;

		private ComposedTypeSpecifier spec;

		public ComposedCast(FullNamedExpression left, ComposedTypeSpecifier spec)
		{
			if (spec == null)
			{
				throw new ArgumentNullException("spec");
			}
			this.left = left;
			this.spec = spec;
			loc = left.Location;
		}

		public override TypeSpec ResolveAsType(IMemberContext ec, bool allowUnboundTypeArguments)
		{
			type = left.ResolveAsType(ec);
			if (type == null)
			{
				return null;
			}
			eclass = ExprClass.Type;
			ComposedTypeSpecifier next = spec;
			if (next.IsNullable)
			{
				type = new NullableType(type, loc).ResolveAsType(ec);
				if (type == null)
				{
					return null;
				}
				next = next.Next;
			}
			else if (next.IsPointer)
			{
				if (!TypeManager.VerifyUnmanaged(ec.Module, type, loc))
				{
					return null;
				}
				if (!ec.IsUnsafe)
				{
					Expression.UnsafeError(ec.Module.Compiler.Report, loc);
				}
				do
				{
					type = PointerContainer.MakeType(ec.Module, type);
					next = next.Next;
				}
				while (next != null && next.IsPointer);
			}
			if (next != null && next.Dimension > 0)
			{
				if (type.IsSpecialRuntimeType)
				{
					ec.Module.Compiler.Report.Error(611, loc, "Array elements cannot be of type `{0}'", type.GetSignatureForError());
				}
				else if (type.IsStatic)
				{
					ec.Module.Compiler.Report.SymbolRelatedToPreviousError(type);
					ec.Module.Compiler.Report.Error(719, loc, "Array elements cannot be of static type `{0}'", type.GetSignatureForError());
				}
				else
				{
					MakeArray(ec.Module, next);
				}
			}
			return type;
		}

		private void MakeArray(ModuleContainer module, ComposedTypeSpecifier spec)
		{
			if (spec.Next != null)
			{
				MakeArray(module, spec.Next);
			}
			type = ArrayContainer.MakeType(module, type, spec.Dimension);
		}

		public override string GetSignatureForError()
		{
			return left.GetSignatureForError() + spec.GetSignatureForError();
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
