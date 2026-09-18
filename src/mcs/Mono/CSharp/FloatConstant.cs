using System;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class FloatConstant : Constant
	{
		public readonly double DoubleValue;

		public float Value => (float)DoubleValue;

		public override bool IsDefaultValue => Value == 0f;

		public override bool IsNegative => Value < 0f;

		public FloatConstant(BuiltinTypes types, double v, Location loc)
			: this(types.Float, v, loc)
		{
		}

		public FloatConstant(TypeSpec type, double v, Location loc)
			: base(loc)
		{
			base.type = type;
			eclass = ExprClass.Value;
			DoubleValue = v;
		}

		public override Constant ConvertImplicitly(TypeSpec type)
		{
			if (type.BuiltinType == BuiltinTypeSpec.Type.Double)
			{
				return new DoubleConstant(type, DoubleValue, loc);
			}
			return base.ConvertImplicitly(type);
		}

		public override void EncodeAttributeValue(IMemberContext rc, AttributeEncoder enc, TypeSpec targetType, TypeSpec parameterType)
		{
			enc.Encode(Value);
		}

		public override void Emit(EmitContext ec)
		{
			ec.Emit(OpCodes.Ldc_R4, Value);
		}

		public override object GetValue()
		{
			return Value;
		}

		public override string GetValueAsLiteral()
		{
			return Value.ToString();
		}

		public override long GetValueAsLong()
		{
			throw new NotSupportedException();
		}

		public override Constant ConvertExplicitly(bool in_checked_context, TypeSpec target_type)
		{
			switch (target_type.BuiltinType)
			{
			case BuiltinTypeSpec.Type.Byte:
				if (in_checked_context && (Value < 0f || Value > 255f || float.IsNaN(Value)))
				{
					throw new OverflowException();
				}
				return new ByteConstant(target_type, (byte)DoubleValue, base.Location);
			case BuiltinTypeSpec.Type.SByte:
				if (in_checked_context && (Value < -128f || Value > 127f || float.IsNaN(Value)))
				{
					throw new OverflowException();
				}
				return new SByteConstant(target_type, (sbyte)DoubleValue, base.Location);
			case BuiltinTypeSpec.Type.Short:
				if (in_checked_context && (Value < -32768f || Value > 32767f || float.IsNaN(Value)))
				{
					throw new OverflowException();
				}
				return new ShortConstant(target_type, (short)DoubleValue, base.Location);
			case BuiltinTypeSpec.Type.UShort:
				if (in_checked_context && (Value < 0f || Value > 65535f || float.IsNaN(Value)))
				{
					throw new OverflowException();
				}
				return new UShortConstant(target_type, (ushort)DoubleValue, base.Location);
			case BuiltinTypeSpec.Type.Int:
				if (in_checked_context && (Value < -2.1474836E+09f || Value > 2.1474836E+09f || float.IsNaN(Value)))
				{
					throw new OverflowException();
				}
				return new IntConstant(target_type, (int)DoubleValue, base.Location);
			case BuiltinTypeSpec.Type.UInt:
				if (in_checked_context && (Value < 0f || Value > 4.2949673E+09f || float.IsNaN(Value)))
				{
					throw new OverflowException();
				}
				return new UIntConstant(target_type, (uint)DoubleValue, base.Location);
			case BuiltinTypeSpec.Type.Long:
				if (in_checked_context && (Value < -9.223372E+18f || Value > 9.223372E+18f || float.IsNaN(Value)))
				{
					throw new OverflowException();
				}
				return new LongConstant(target_type, (long)DoubleValue, base.Location);
			case BuiltinTypeSpec.Type.ULong:
				if (in_checked_context && (Value < 0f || Value > 1.8446744E+19f || float.IsNaN(Value)))
				{
					throw new OverflowException();
				}
				return new ULongConstant(target_type, (ulong)DoubleValue, base.Location);
			case BuiltinTypeSpec.Type.Double:
				return new DoubleConstant(target_type, DoubleValue, base.Location);
			case BuiltinTypeSpec.Type.Char:
				if (in_checked_context && (Value < 0f || Value > 65535f || float.IsNaN(Value)))
				{
					throw new OverflowException();
				}
				return new CharConstant(target_type, (char)DoubleValue, base.Location);
			case BuiltinTypeSpec.Type.Decimal:
				return new DecimalConstant(target_type, (decimal)DoubleValue, base.Location);
			default:
				return null;
			}
		}
	}
}
