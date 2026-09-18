using System;

namespace Mono.CSharp
{
	public class CharConstant : Constant
	{
		public readonly char Value;

		public override bool IsDefaultValue => Value == '\0';

		public override bool IsNegative => false;

		public override bool IsZeroInteger => Value == '\0';

		public CharConstant(BuiltinTypes types, char v, Location loc)
			: this(types.Char, v, loc)
		{
		}

		public CharConstant(TypeSpec type, char v, Location loc)
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
			ec.EmitInt(Value);
		}

		private static string descape(char c)
		{
			switch (c)
			{
			case '\a':
				return "\\a";
			case '\b':
				return "\\b";
			case '\n':
				return "\\n";
			case '\t':
				return "\\t";
			case '\v':
				return "\\v";
			case '\r':
				return "\\r";
			case '\\':
				return "\\\\";
			case '\f':
				return "\\f";
			case '\0':
				return "\\0";
			case '"':
				return "\\\"";
			case '\'':
				return "\\'";
			default:
				return c.ToString();
			}
		}

		public override object GetValue()
		{
			return Value;
		}

		public override long GetValueAsLong()
		{
			return Value;
		}

		public override string GetValueAsLiteral()
		{
			return "\"" + descape(Value) + "\"";
		}

		public override Constant ConvertExplicitly(bool in_checked_context, TypeSpec target_type)
		{
			switch (target_type.BuiltinType)
			{
			case BuiltinTypeSpec.Type.Byte:
				if (in_checked_context && (Value < '\0' || Value > 'ÿ'))
				{
					throw new OverflowException();
				}
				return new ByteConstant(target_type, (byte)Value, base.Location);
			case BuiltinTypeSpec.Type.SByte:
				if (in_checked_context && Value > '\u007f')
				{
					throw new OverflowException();
				}
				return new SByteConstant(target_type, (sbyte)Value, base.Location);
			case BuiltinTypeSpec.Type.Short:
				if (in_checked_context && Value > '翿')
				{
					throw new OverflowException();
				}
				return new ShortConstant(target_type, (short)Value, base.Location);
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
			case BuiltinTypeSpec.Type.Decimal:
				return new DecimalConstant(target_type, Value, base.Location);
			default:
				return null;
			}
		}
	}
}
