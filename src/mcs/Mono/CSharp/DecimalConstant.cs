using System;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class DecimalConstant : Constant
	{
		public readonly decimal Value;

		public override bool IsDefaultValue => Value == 0m;

		public override bool IsNegative => Value < 0m;

		public DecimalConstant(BuiltinTypes types, decimal d, Location loc)
			: this(types.Decimal, d, loc)
		{
		}

		public DecimalConstant(TypeSpec type, decimal d, Location loc)
			: base(loc)
		{
			base.type = type;
			eclass = ExprClass.Value;
			Value = d;
		}

		public override void Emit(EmitContext ec)
		{
			int[] bits = decimal.GetBits(Value);
			int num = (bits[3] >> 16) & 0xFF;
			MethodSpec methodSpec;
			if (num == 0)
			{
				if (Value <= 2147483647m && Value >= -2147483648m)
				{
					methodSpec = ec.Module.PredefinedMembers.DecimalCtorInt.Resolve(loc);
					if (methodSpec != null)
					{
						ec.EmitInt((int)Value);
						ec.Emit(OpCodes.Newobj, methodSpec);
					}
					return;
				}
				if (Value <= 9223372036854775807m && Value >= -9223372036854775808m)
				{
					methodSpec = ec.Module.PredefinedMembers.DecimalCtorLong.Resolve(loc);
					if (methodSpec != null)
					{
						ec.EmitLong((long)Value);
						ec.Emit(OpCodes.Newobj, methodSpec);
					}
					return;
				}
			}
			ec.EmitInt(bits[0]);
			ec.EmitInt(bits[1]);
			ec.EmitInt(bits[2]);
			ec.EmitInt(bits[3] >> 31);
			ec.EmitInt(num);
			methodSpec = ec.Module.PredefinedMembers.DecimalCtor.Resolve(loc);
			if (methodSpec != null)
			{
				ec.Emit(OpCodes.Newobj, methodSpec);
			}
		}

		public override Constant ConvertExplicitly(bool in_checked_context, TypeSpec target_type)
		{
			switch (target_type.BuiltinType)
			{
			case BuiltinTypeSpec.Type.SByte:
				return new SByteConstant(target_type, (sbyte)Value, loc);
			case BuiltinTypeSpec.Type.Byte:
				return new ByteConstant(target_type, (byte)Value, loc);
			case BuiltinTypeSpec.Type.Short:
				return new ShortConstant(target_type, (short)Value, loc);
			case BuiltinTypeSpec.Type.UShort:
				return new UShortConstant(target_type, (ushort)Value, loc);
			case BuiltinTypeSpec.Type.Int:
				return new IntConstant(target_type, (int)Value, loc);
			case BuiltinTypeSpec.Type.UInt:
				return new UIntConstant(target_type, (uint)Value, loc);
			case BuiltinTypeSpec.Type.Long:
				return new LongConstant(target_type, (long)Value, loc);
			case BuiltinTypeSpec.Type.ULong:
				return new ULongConstant(target_type, (ulong)Value, loc);
			case BuiltinTypeSpec.Type.Char:
				return new CharConstant(target_type, (char)Value, loc);
			case BuiltinTypeSpec.Type.Float:
				return new FloatConstant(target_type, (float)Value, loc);
			case BuiltinTypeSpec.Type.Double:
				return new DoubleConstant(target_type, (double)Value, loc);
			default:
				return null;
			}
		}

		public override object GetValue()
		{
			return Value;
		}

		public override string GetValueAsLiteral()
		{
			return Value + "M";
		}

		public override long GetValueAsLong()
		{
			throw new NotSupportedException();
		}
	}
}
