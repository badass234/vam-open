using System;

namespace Mono.CSharp
{
	public class LongConstant : IntegralConstant
	{
		public readonly long Value;

		public override bool IsDefaultValue => Value == 0;

		public override bool IsNegative => Value < 0;

		public override bool IsOneInteger => Value == 1;

		public override bool IsZeroInteger => Value == 0;

		public LongConstant(BuiltinTypes types, long v, Location loc)
			: this(types.Long, v, loc)
		{
		}

		public LongConstant(TypeSpec type, long v, Location loc)
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
			ec.EmitLong(Value);
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
			return new LongConstant(type, checked(Value + 1), loc);
		}

		public override Constant ConvertExplicitly(bool in_checked_context, TypeSpec target_type)
		{
			switch (target_type.BuiltinType)
			{
			case BuiltinTypeSpec.Type.Byte:
				if (in_checked_context && (Value < 0 || Value > 255))
				{
					throw new OverflowException();
				}
				return new ByteConstant(target_type, (byte)Value, base.Location);
			case BuiltinTypeSpec.Type.SByte:
				if (in_checked_context && (Value < -128 || Value > 127))
				{
					throw new OverflowException();
				}
				return new SByteConstant(target_type, (sbyte)Value, base.Location);
			case BuiltinTypeSpec.Type.Short:
				if (in_checked_context && (Value < -32768 || Value > 32767))
				{
					throw new OverflowException();
				}
				return new ShortConstant(target_type, (short)Value, base.Location);
			case BuiltinTypeSpec.Type.UShort:
				if (in_checked_context && (Value < 0 || Value > 65535))
				{
					throw new OverflowException();
				}
				return new UShortConstant(target_type, (ushort)Value, base.Location);
			case BuiltinTypeSpec.Type.Int:
				if (in_checked_context && (Value < int.MinValue || Value > int.MaxValue))
				{
					throw new OverflowException();
				}
				return new IntConstant(target_type, (int)Value, base.Location);
			case BuiltinTypeSpec.Type.UInt:
				if (in_checked_context && (Value < 0 || Value > uint.MaxValue))
				{
					throw new OverflowException();
				}
				return new UIntConstant(target_type, (uint)Value, base.Location);
			case BuiltinTypeSpec.Type.ULong:
				if (in_checked_context && Value < 0)
				{
					throw new OverflowException();
				}
				return new ULongConstant(target_type, (ulong)Value, base.Location);
			case BuiltinTypeSpec.Type.Float:
				return new FloatConstant(target_type, (float)Value, base.Location);
			case BuiltinTypeSpec.Type.Double:
				return new DoubleConstant(target_type, Value, base.Location);
			case BuiltinTypeSpec.Type.Char:
				if (in_checked_context && (Value < 0 || Value > 65535))
				{
					throw new OverflowException();
				}
				return new CharConstant(target_type, (char)Value, base.Location);
			case BuiltinTypeSpec.Type.Decimal:
				return new DecimalConstant(target_type, Value, base.Location);
			default:
				return null;
			}
		}

		public override Constant ConvertImplicitly(TypeSpec type)
		{
			if (Value >= 0 && type.BuiltinType == BuiltinTypeSpec.Type.ULong)
			{
				return new ULongConstant(type, (ulong)Value, loc);
			}
			return base.ConvertImplicitly(type);
		}
	}
}
