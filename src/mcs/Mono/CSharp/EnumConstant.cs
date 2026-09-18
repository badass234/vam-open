using System;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class EnumConstant : Constant
	{
		public Constant Child;

		public override bool IsDefaultValue => Child.IsDefaultValue;

		public override bool IsSideEffectFree => Child.IsSideEffectFree;

		public override bool IsZeroInteger => Child.IsZeroInteger;

		public override bool IsNegative => Child.IsNegative;

		public EnumConstant(Constant child, TypeSpec enum_type)
			: base(child.Location)
		{
			Child = child;
			eclass = ExprClass.Value;
			type = enum_type;
		}

		protected EnumConstant(Location loc)
			: base(loc)
		{
		}

		public override void Emit(EmitContext ec)
		{
			Child.Emit(ec);
		}

		public override void EncodeAttributeValue(IMemberContext rc, AttributeEncoder enc, TypeSpec targetType, TypeSpec parameterType)
		{
			Child.EncodeAttributeValue(rc, enc, Child.Type, parameterType);
		}

		public override void EmitBranchable(EmitContext ec, Label label, bool on_true)
		{
			Child.EmitBranchable(ec, label, on_true);
		}

		public override void EmitSideEffect(EmitContext ec)
		{
			Child.EmitSideEffect(ec);
		}

		public override string GetSignatureForError()
		{
			return base.Type.GetSignatureForError();
		}

		public override object GetValue()
		{
			return Child.GetValue();
		}

		public override object GetTypedValue()
		{
			return System.Enum.ToObject(type.GetMetaInfo(), Child.GetValue());
		}

		public override string GetValueAsLiteral()
		{
			return Child.GetValueAsLiteral();
		}

		public override long GetValueAsLong()
		{
			return Child.GetValueAsLong();
		}

		public EnumConstant Increment()
		{
			return new EnumConstant(((IntegralConstant)Child).Increment(), type);
		}

		public override Constant ConvertExplicitly(bool in_checked_context, TypeSpec target_type)
		{
			if (Child.Type == target_type)
			{
				return Child;
			}
			return Child.ConvertExplicitly(in_checked_context, target_type);
		}

		public override Constant ConvertImplicitly(TypeSpec type)
		{
			if (base.type == type)
			{
				return this;
			}
			if (!Convert.ImplicitStandardConversionExists(this, type))
			{
				return null;
			}
			return Child.ConvertImplicitly(type);
		}
	}
}
