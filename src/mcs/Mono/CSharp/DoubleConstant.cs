using System;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class DoubleConstant : Constant
	{
		public readonly double Value;

		public override bool IsDefaultValue => Value == 0.0;

		public override bool IsNegative => Value < 0.0;

		public DoubleConstant(BuiltinTypes types, double v, Location loc)
			: this(types.Double, v, loc)
		{
		}

		public DoubleConstant(TypeSpec type, double v, Location loc)
			: base(loc)
		{
			base.type = type;
			eclass = ExprClass.Value;
			Value = v;
		}

		public override void EncodeAttributeValue(IMemberContext rc, AttributeEncoder enc, TypeSpec targetType, TypeSpec parameterType)
		{
			enc.Encode(Value);
		}

		public override void Emit(EmitContext ec)
		{
			ec.Emit(OpCodes.Ldc_R8, Value);
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
				if (in_checked_context && (Value < 0.0 || Value > 255.0 || double.IsNaN(Value)))
				{
					throw new OverflowException();
				}
				return new ByteConstant(target_type, (byte)Value, base.Location);
			case BuiltinTypeSpec.Type.SByte:
				if (in_checked_context && (Value < -128.0 || Value > 127.0 || double.IsNaN(Value)))
				{
					throw new OverflowException();
				}
				return new SByteConstant(target_type, (sbyte)Value, base.Location);
			case BuiltinTypeSpec.Type.Short:
				if (in_checked_context && (Value < -32768.0 || Value > 32767.0 || double.IsNaN(Value)))
				{
					throw new OverflowException();
				}
				return new ShortConstant(target_type, (short)Value, base.Location);
			case BuiltinTypeSpec.Type.UShort:
				if (in_checked_context && (Value < 0.0 || Value > 65535.0 || double.IsNaN(Value)))
				{
					throw new OverflowException();
				}
				return new UShortConstant(target_type, (ushort)Value, base.Location);
			case BuiltinTypeSpec.Type.Int:
				if (in_checked_context && (Value < -2147483648.0 || Value > 2147483647.0 || double.IsNaN(Value)))
				{
					throw new OverflowException();
				}
				return new IntConstant(target_type, (int)Value, base.Location);
			case BuiltinTypeSpec.Type.UInt:
				if (in_checked_context && (Value < 0.0 || Value > 4294967295.0 || double.IsNaN(Value)))
				{
					throw new OverflowException();
				}
				return new UIntConstant(target_type, (uint)Value, base.Location);
			case BuiltinTypeSpec.Type.Long:
				if (in_checked_context && (Value < -9.223372036854776E+18 || Value > 9.223372036854776E+18 || double.IsNaN(Value)))
				{
					throw new OverflowException();
				}
				return new LongConstant(target_type, (long)Value, base.Location);
			case BuiltinTypeSpec.Type.ULong:
				if (in_checked_context && (Value < 0.0 || Value > 1.8446744073709552E+19 || double.IsNaN(Value)))
				{
					throw new OverflowException();
				}
				return new ULongConstant(target_type, (ulong)Value, base.Location);
			case BuiltinTypeSpec.Type.Float:
				return new FloatConstant(target_type, (float)Value, base.Location);
			case BuiltinTypeSpec.Type.Char:
				if (in_checked_context && (Value < 0.0 || Value > 65535.0 || double.IsNaN(Value)))
				{
					throw new OverflowException();
				}
				return new CharConstant(target_type, (char)Value, base.Location);
			case BuiltinTypeSpec.Type.Decimal:
				return new DecimalConstant(target_type, (decimal)Value, base.Location);
			default:
				return null;
			}
		}
	}
}
