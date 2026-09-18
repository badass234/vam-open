using System;

namespace Mono.CSharp
{
	public class ByteConstant : IntegralConstant
	{
		public readonly byte Value;

		public override bool IsDefaultValue => Value == 0;

		public override bool IsOneInteger => Value == 1;

		public override bool IsNegative => false;

		public override bool IsZeroInteger => Value == 0;

		public ByteConstant(BuiltinTypes types, byte v, Location loc)
			: this(types.Byte, v, loc)
		{
		}

		public ByteConstant(TypeSpec type, byte v, Location loc)
			: base(type, loc)
		{
			Value = v;
		}

		public override void EncodeAttributeValue(IMemberContext rc, AttributeEncoder enc, TypeSpec targetType, TypeSpec parameterType)
		{
			enc.Encode(Value);
		}

		public override void Emit(EmitContext ec)
		{
			ec.EmitInt(Value);
		}

		public override object GetValue()
		{
			return Value;
		}

		public override long GetValueAsLong()
		{
			return Value;
		}

		public override Constant Increment()
		{
			return new ByteConstant(type, checked((byte)(Value + 1)), loc);
		}

		public override Constant ConvertExplicitly(bool in_checked_context, TypeSpec target_type)
		{
			switch (target_type.BuiltinType)
			{
			case BuiltinTypeSpec.Type.SByte:
				if (in_checked_context && Value > 127)
				{
					throw new OverflowException();
				}
				return new SByteConstant(target_type, (sbyte)Value, base.Location);
			case BuiltinTypeSpec.Type.Short:
				return new ShortConstant(target_type, Value, base.Location);
			case BuiltinTypeSpec.Type.UShort:
				return new UShortConstant(target_type, Value, base.Location);
			case BuiltinTypeSpec.Type.Int:
				return new IntConstant(target_type, Value, base.Location);
			case BuiltinTypeSpec.Type.UInt:
				return new UIntConstant(target_type, Value, base.Location);
			case BuiltinTypeSpec.Type.Long:
				return new LongConstant(target_type, Value, base.Location);
			case BuiltinTypeSpec.Type.ULong:
				return new ULongConstant(target_type, Value, base.Location);
			case BuiltinTypeSpec.Type.Float:
				return new FloatConstant(target_type, (float)(int)Value, base.Location);
			case BuiltinTypeSpec.Type.Double:
				return new DoubleConstant(target_type, (int)Value, base.Location);
			case BuiltinTypeSpec.Type.Char:
				return new CharConstant(target_type, (char)Value, base.Location);
			case BuiltinTypeSpec.Type.Decimal:
				return new DecimalConstant(target_type, Value, base.Location);
			default:
				return null;
			}
		}
	}
}
