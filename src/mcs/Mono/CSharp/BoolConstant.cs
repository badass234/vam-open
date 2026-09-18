namespace Mono.CSharp
{
	public class BoolConstant : Constant
	{
		public readonly bool Value;

		public override bool IsDefaultValue => !Value;

		public override bool IsNegative => false;

		public override bool IsZeroInteger => !Value;

		public BoolConstant(BuiltinTypes types, bool val, Location loc)
			: this(types.Bool, val, loc)
		{
		}

		public BoolConstant(TypeSpec type, bool val, Location loc)
			: base(loc)
		{
			eclass = ExprClass.Value;
			base.type = type;
			Value = val;
		}

		public override object GetValue()
		{
			return Value;
		}

		public override string GetValueAsLiteral()
		{
			if (!Value)
			{
				return "false";
			}
			return "true";
		}

		public override long GetValueAsLong()
		{
			return Value ? 1 : 0;
		}

		public override void EncodeAttributeValue(IMemberContext rc, AttributeEncoder enc, TypeSpec targetType, TypeSpec parameterType)
		{
			enc.Encode(Value);
		}

		public override void Emit(EmitContext ec)
		{
			if (Value)
			{
				ec.EmitInt(1);
			}
			else
			{
				ec.EmitInt(0);
			}
		}

		public override Constant ConvertExplicitly(bool in_checked_context, TypeSpec target_type)
		{
			return null;
		}
	}
}
