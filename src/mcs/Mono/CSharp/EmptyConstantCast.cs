using System;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class EmptyConstantCast : Constant
	{
		public readonly Constant child;

		public override bool IsDefaultValue => child.IsDefaultValue;

		public override bool IsNegative => child.IsNegative;

		public override bool IsNull => child.IsNull;

		public override bool IsOneInteger => child.IsOneInteger;

		public override bool IsSideEffectFree => child.IsSideEffectFree;

		public override bool IsZeroInteger => child.IsZeroInteger;

		public EmptyConstantCast(Constant child, TypeSpec type)
			: base(child.Location)
		{
			if (child == null)
			{
				throw new ArgumentNullException("child");
			}
			this.child = child;
			eclass = child.eclass;
			base.type = type;
		}

		public override Constant ConvertExplicitly(bool in_checked_context, TypeSpec target_type)
		{
			if (child.Type == target_type)
			{
				return child;
			}
			return child.ConvertExplicitly(in_checked_context, target_type);
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			Arguments args = Arguments.CreateForExpressionTree(ec, null, child.CreateExpressionTree(ec), new TypeOf(type, loc));
			if (type.IsPointer)
			{
				Error_PointerInsideExpressionTree(ec);
			}
			return CreateExpressionFactoryCall(ec, "Convert", args);
		}

		public override void Emit(EmitContext ec)
		{
			child.Emit(ec);
		}

		public override void EmitBranchable(EmitContext ec, Label label, bool on_true)
		{
			child.EmitBranchable(ec, label, on_true);
			if (TypeManager.IsGenericParameter(type) && child.IsNull)
			{
				ec.Emit(OpCodes.Unbox_Any, type);
			}
		}

		public override void EmitSideEffect(EmitContext ec)
		{
			child.EmitSideEffect(ec);
		}

		public override object GetValue()
		{
			return child.GetValue();
		}

		public override string GetValueAsLiteral()
		{
			return child.GetValueAsLiteral();
		}

		public override long GetValueAsLong()
		{
			return child.GetValueAsLong();
		}

		public override Constant ConvertImplicitly(TypeSpec target_type)
		{
			if (type == target_type)
			{
				return this;
			}
			if (!Convert.ImplicitStandardConversionExists(this, target_type))
			{
				return null;
			}
			return child.ConvertImplicitly(target_type);
		}
	}
}
