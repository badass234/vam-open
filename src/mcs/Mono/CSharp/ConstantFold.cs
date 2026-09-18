using System;
using Mono.CSharp.Nullable;

namespace Mono.CSharp
{
	public static class ConstantFold
	{
		public static TypeSpec[] CreateBinaryPromotionsTypes(BuiltinTypes types)
		{
			return new TypeSpec[6] { types.Decimal, types.Double, types.Float, types.ULong, types.Long, types.UInt };
		}

		private static bool DoBinaryNumericPromotions(ResolveContext rc, ref Constant left, ref Constant right)
		{
			TypeSpec type = left.Type;
			TypeSpec type2 = right.Type;
			TypeSpec[] binaryPromotionsTypes = rc.BuiltinTypes.BinaryPromotionsTypes;
			foreach (TypeSpec typeSpec in binaryPromotionsTypes)
			{
				if (typeSpec == type)
				{
					if (typeSpec != type2)
					{
						return ConvertPromotion(rc, ref right, ref left, typeSpec);
					}
					return true;
				}
				if (typeSpec == type2)
				{
					if (typeSpec != type)
					{
						return ConvertPromotion(rc, ref left, ref right, typeSpec);
					}
					return true;
				}
			}
			left = left.ConvertImplicitly(rc.BuiltinTypes.Int);
			right = right.ConvertImplicitly(rc.BuiltinTypes.Int);
			if (left != null)
			{
				return right != null;
			}
			return false;
		}

		private static bool ConvertPromotion(ResolveContext rc, ref Constant prim, ref Constant second, TypeSpec type)
		{
			Constant constant = prim.ConvertImplicitly(type);
			if (constant != null)
			{
				prim = constant;
				return true;
			}
			if (type.BuiltinType == BuiltinTypeSpec.Type.UInt)
			{
				type = rc.BuiltinTypes.Long;
				prim = prim.ConvertImplicitly(type);
				second = second.ConvertImplicitly(type);
				if (prim != null)
				{
					return second != null;
				}
				return false;
			}
			return false;
		}

		public static void Error_CompileTimeOverflow(ResolveContext rc, Location loc)
		{
			rc.Report.Error(220, loc, "The operation overflows at compile time in checked mode");
		}

		public static Constant BinaryFold(ResolveContext ec, Binary.Operator oper, Constant left, Constant right, Location loc)
		{
			Constant result = null;
			if (left is EmptyConstantCast)
			{
				return BinaryFold(ec, oper, ((EmptyConstantCast)left).child, right, loc);
			}
			if (left is SideEffectConstant)
			{
				result = BinaryFold(ec, oper, ((SideEffectConstant)left).value, right, loc);
				if (result == null)
				{
					return null;
				}
				return new SideEffectConstant(result, left, loc);
			}
			if (right is EmptyConstantCast)
			{
				return BinaryFold(ec, oper, left, ((EmptyConstantCast)right).child, loc);
			}
			if (right is SideEffectConstant)
			{
				result = BinaryFold(ec, oper, left, ((SideEffectConstant)right).value, loc);
				if (result == null)
				{
					return null;
				}
				return new SideEffectConstant(result, right, loc);
			}
			TypeSpec type = left.Type;
			TypeSpec type2 = right.Type;
			if (type.BuiltinType == BuiltinTypeSpec.Type.FirstPrimitive && type == type2)
			{
				bool flag = (bool)left.GetValue();
				bool flag2 = (bool)right.GetValue();
				switch (oper)
				{
				case Binary.Operator.BitwiseAnd:
				case Binary.Operator.LogicalAnd:
					return new BoolConstant(ec.BuiltinTypes, flag & flag2, left.Location);
				case Binary.Operator.BitwiseOr:
				case Binary.Operator.LogicalOr:
					return new BoolConstant(ec.BuiltinTypes, flag | flag2, left.Location);
				case Binary.Operator.ExclusiveOr:
					return new BoolConstant(ec.BuiltinTypes, flag ^ flag2, left.Location);
				case Binary.Operator.Equality:
					return new BoolConstant(ec.BuiltinTypes, flag == flag2, left.Location);
				case Binary.Operator.Inequality:
					return new BoolConstant(ec.BuiltinTypes, flag != flag2, left.Location);
				default:
					return null;
				}
			}
			if (ec.HasSet(ResolveContext.Options.EnumScope))
			{
				if (left is EnumConstant)
				{
					left = ((EnumConstant)left).Child;
				}
				if (right is EnumConstant)
				{
					right = ((EnumConstant)right).Child;
				}
			}
			else if (left is EnumConstant && type2 == type)
			{
				switch (oper)
				{
				case Binary.Operator.BitwiseAnd:
				case Binary.Operator.ExclusiveOr:
				case Binary.Operator.BitwiseOr:
					result = BinaryFold(ec, oper, ((EnumConstant)left).Child, ((EnumConstant)right).Child, loc);
					if (result != null)
					{
						result = result.Reduce(ec, type);
					}
					return result;
				case Binary.Operator.Subtraction:
					result = BinaryFold(ec, oper, ((EnumConstant)left).Child, ((EnumConstant)right).Child, loc);
					if (result != null)
					{
						result = result.Reduce(ec, EnumSpec.GetUnderlyingType(type));
					}
					return result;
				case Binary.Operator.Equality:
				case Binary.Operator.Inequality:
				case Binary.Operator.LessThan:
				case Binary.Operator.GreaterThan:
				case Binary.Operator.LessThanOrEqual:
				case Binary.Operator.GreaterThanOrEqual:
					return BinaryFold(ec, oper, ((EnumConstant)left).Child, ((EnumConstant)right).Child, loc);
				default:
					return null;
				}
			}
			switch (oper)
			{
			case Binary.Operator.BitwiseOr:
				if ((type.BuiltinType == BuiltinTypeSpec.Type.FirstPrimitive && right is NullLiteral) || (type2.BuiltinType == BuiltinTypeSpec.Type.FirstPrimitive && left is NullLiteral))
				{
					Expression expression = new Binary(oper, left, right).ResolveOperator(ec);
					if ((right is NullLiteral && left.IsDefaultValue) || (left is NullLiteral && right.IsDefaultValue))
					{
						return LiftedNull.CreateFromExpression(ec, expression);
					}
					return ReducedExpression.Create(new BoolConstant(ec.BuiltinTypes, true, loc), expression);
				}
				if (!DoBinaryNumericPromotions(ec, ref left, ref right))
				{
					return null;
				}
				if (left is IntConstant)
				{
					int v = ((IntConstant)left).Value | ((IntConstant)right).Value;
					return new IntConstant(ec.BuiltinTypes, v, left.Location);
				}
				if (left is UIntConstant)
				{
					uint v2 = ((UIntConstant)left).Value | ((UIntConstant)right).Value;
					return new UIntConstant(ec.BuiltinTypes, v2, left.Location);
				}
				if (left is LongConstant)
				{
					long v3 = ((LongConstant)left).Value | ((LongConstant)right).Value;
					return new LongConstant(ec.BuiltinTypes, v3, left.Location);
				}
				if (left is ULongConstant)
				{
					ulong v4 = ((ULongConstant)left).Value | ((ULongConstant)right).Value;
					return new ULongConstant(ec.BuiltinTypes, v4, left.Location);
				}
				break;
			case Binary.Operator.BitwiseAnd:
				if ((type.BuiltinType == BuiltinTypeSpec.Type.FirstPrimitive && right is NullLiteral) || (type2.BuiltinType == BuiltinTypeSpec.Type.FirstPrimitive && left is NullLiteral))
				{
					Expression expression2 = new Binary(oper, left, right).ResolveOperator(ec);
					if ((right is NullLiteral && left.IsDefaultValue) || (left is NullLiteral && right.IsDefaultValue))
					{
						return ReducedExpression.Create(new BoolConstant(ec.BuiltinTypes, false, loc), expression2);
					}
					return LiftedNull.CreateFromExpression(ec, expression2);
				}
				if (!DoBinaryNumericPromotions(ec, ref left, ref right))
				{
					return null;
				}
				if (left is IntConstant)
				{
					int v9 = ((IntConstant)left).Value & ((IntConstant)right).Value;
					return new IntConstant(ec.BuiltinTypes, v9, left.Location);
				}
				if (left is UIntConstant)
				{
					uint v10 = ((UIntConstant)left).Value & ((UIntConstant)right).Value;
					return new UIntConstant(ec.BuiltinTypes, v10, left.Location);
				}
				if (left is LongConstant)
				{
					long v11 = ((LongConstant)left).Value & ((LongConstant)right).Value;
					return new LongConstant(ec.BuiltinTypes, v11, left.Location);
				}
				if (left is ULongConstant)
				{
					ulong v12 = ((ULongConstant)left).Value & ((ULongConstant)right).Value;
					return new ULongConstant(ec.BuiltinTypes, v12, left.Location);
				}
				break;
			case Binary.Operator.ExclusiveOr:
				if (!DoBinaryNumericPromotions(ec, ref left, ref right))
				{
					return null;
				}
				if (left is IntConstant)
				{
					int v5 = ((IntConstant)left).Value ^ ((IntConstant)right).Value;
					return new IntConstant(ec.BuiltinTypes, v5, left.Location);
				}
				if (left is UIntConstant)
				{
					uint v6 = ((UIntConstant)left).Value ^ ((UIntConstant)right).Value;
					return new UIntConstant(ec.BuiltinTypes, v6, left.Location);
				}
				if (left is LongConstant)
				{
					long v7 = ((LongConstant)left).Value ^ ((LongConstant)right).Value;
					return new LongConstant(ec.BuiltinTypes, v7, left.Location);
				}
				if (left is ULongConstant)
				{
					ulong v8 = ((ULongConstant)left).Value ^ ((ULongConstant)right).Value;
					return new ULongConstant(ec.BuiltinTypes, v8, left.Location);
				}
				break;
			case Binary.Operator.Addition:
			{
				if (type.BuiltinType == BuiltinTypeSpec.Type.String || type2.BuiltinType == BuiltinTypeSpec.Type.String)
				{
					if (type == type2)
					{
						return new StringConstant(ec.BuiltinTypes, (string)left.GetValue() + (string)right.GetValue(), left.Location);
					}
					if (type == InternalType.NullLiteral || left.IsNull)
					{
						return new StringConstant(ec.BuiltinTypes, string.Concat(right.GetValue()), left.Location);
					}
					if (type2 == InternalType.NullLiteral || right.IsNull)
					{
						return new StringConstant(ec.BuiltinTypes, string.Concat(left.GetValue()), left.Location);
					}
					return null;
				}
				if (type == InternalType.NullLiteral)
				{
					if (type2.BuiltinType == BuiltinTypeSpec.Type.Object)
					{
						return new StringConstant(ec.BuiltinTypes, string.Concat(right.GetValue()), left.Location);
					}
					if (type == type2)
					{
						ec.Report.Error(34, loc, "Operator `{0}' is ambiguous on operands of type `{1}' and `{2}'", "+", type.GetSignatureForError(), type2.GetSignatureForError());
						return null;
					}
					return right;
				}
				if (type2 == InternalType.NullLiteral)
				{
					if (type.BuiltinType == BuiltinTypeSpec.Type.Object)
					{
						return new StringConstant(ec.BuiltinTypes, string.Concat(right.GetValue()), left.Location);
					}
					return left;
				}
				EnumConstant enumConstant = left as EnumConstant;
				EnumConstant enumConstant2 = right as EnumConstant;
				if (enumConstant != null || enumConstant2 != null)
				{
					if (enumConstant == null)
					{
						enumConstant = enumConstant2;
						type = enumConstant.Type;
						right = left;
					}
					right = right.ConvertImplicitly(enumConstant.Child.Type);
					if (right == null)
					{
						return null;
					}
					result = BinaryFold(ec, oper, enumConstant.Child, right, loc);
					if (result == null)
					{
						return null;
					}
					result = result.Reduce(ec, type);
					if (result == null || type.IsEnum)
					{
						return result;
					}
					return new EnumConstant(result, type);
				}
				if (!DoBinaryNumericPromotions(ec, ref left, ref right))
				{
					return null;
				}
				try
				{
					if (left is DoubleConstant)
					{
						return new DoubleConstant(v: (!ec.ConstantCheckState) ? (((DoubleConstant)left).Value + ((DoubleConstant)right).Value) : (((DoubleConstant)left).Value + ((DoubleConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
					}
					if (left is FloatConstant)
					{
						double doubleValue3 = ((FloatConstant)left).DoubleValue;
						double doubleValue4 = ((FloatConstant)right).DoubleValue;
						result = new FloatConstant(v: (!ec.ConstantCheckState) ? (doubleValue3 + doubleValue4) : (doubleValue3 + doubleValue4), types: ec.BuiltinTypes, loc: left.Location);
					}
					else if (left is ULongConstant)
					{
						result = new ULongConstant(v: (!ec.ConstantCheckState) ? (((ULongConstant)left).Value + ((ULongConstant)right).Value) : checked(((ULongConstant)left).Value + ((ULongConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
					}
					else if (left is LongConstant)
					{
						result = new LongConstant(v: (!ec.ConstantCheckState) ? (((LongConstant)left).Value + ((LongConstant)right).Value) : checked(((LongConstant)left).Value + ((LongConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
					}
					else if (left is UIntConstant)
					{
						result = new UIntConstant(v: (!ec.ConstantCheckState) ? (((UIntConstant)left).Value + ((UIntConstant)right).Value) : checked(((UIntConstant)left).Value + ((UIntConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
					}
					else if (left is IntConstant)
					{
						result = new IntConstant(v: (!ec.ConstantCheckState) ? (((IntConstant)left).Value + ((IntConstant)right).Value) : checked(((IntConstant)left).Value + ((IntConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
					}
					else if (left is DecimalConstant)
					{
						result = new DecimalConstant(d: (!ec.ConstantCheckState) ? (((DecimalConstant)left).Value + ((DecimalConstant)right).Value) : (((DecimalConstant)left).Value + ((DecimalConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
					}
				}
				catch (OverflowException)
				{
					Error_CompileTimeOverflow(ec, loc);
				}
				return result;
			}
			case Binary.Operator.Subtraction:
			{
				EnumConstant enumConstant = left as EnumConstant;
				EnumConstant enumConstant2 = right as EnumConstant;
				if (enumConstant != null || enumConstant2 != null)
				{
					if (enumConstant == null)
					{
						enumConstant = enumConstant2;
						type = enumConstant.Type;
						right = left;
					}
					right = right.ConvertImplicitly(enumConstant.Child.Type);
					if (right == null)
					{
						return null;
					}
					result = BinaryFold(ec, oper, enumConstant.Child, right, loc);
					if (result == null)
					{
						return null;
					}
					result = result.Reduce(ec, type);
					if (result == null)
					{
						return null;
					}
					return new EnumConstant(result, type);
				}
				if (left is NullLiteral && right is NullLiteral)
				{
					NullableType nullableType4 = new NullableType(ec.BuiltinTypes.Int, loc);
					nullableType4.ResolveAsType(ec);
					return (Constant)new Binary(oper, nullableType4, right).ResolveOperator(ec);
				}
				if (!DoBinaryNumericPromotions(ec, ref left, ref right))
				{
					return null;
				}
				try
				{
					if (left is DoubleConstant)
					{
						result = new DoubleConstant(v: (!ec.ConstantCheckState) ? (((DoubleConstant)left).Value - ((DoubleConstant)right).Value) : (((DoubleConstant)left).Value - ((DoubleConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
					}
					else if (left is FloatConstant)
					{
						double doubleValue = ((FloatConstant)left).DoubleValue;
						double doubleValue2 = ((FloatConstant)right).DoubleValue;
						result = new FloatConstant(v: (!ec.ConstantCheckState) ? (doubleValue - doubleValue2) : (doubleValue - doubleValue2), types: ec.BuiltinTypes, loc: left.Location);
					}
					else if (left is ULongConstant)
					{
						result = new ULongConstant(v: (!ec.ConstantCheckState) ? (((ULongConstant)left).Value - ((ULongConstant)right).Value) : checked(((ULongConstant)left).Value - ((ULongConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
					}
					else if (left is LongConstant)
					{
						result = new LongConstant(v: (!ec.ConstantCheckState) ? (((LongConstant)left).Value - ((LongConstant)right).Value) : checked(((LongConstant)left).Value - ((LongConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
					}
					else if (left is UIntConstant)
					{
						result = new UIntConstant(v: (!ec.ConstantCheckState) ? (((UIntConstant)left).Value - ((UIntConstant)right).Value) : checked(((UIntConstant)left).Value - ((UIntConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
					}
					else
					{
						if (!(left is IntConstant))
						{
							if (!(left is DecimalConstant))
							{
								throw new Exception("Unexepected subtraction input: " + left);
							}
							return new DecimalConstant(d: (!ec.ConstantCheckState) ? (((DecimalConstant)left).Value - ((DecimalConstant)right).Value) : (((DecimalConstant)left).Value - ((DecimalConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
						}
						result = new IntConstant(v: (!ec.ConstantCheckState) ? (((IntConstant)left).Value - ((IntConstant)right).Value) : checked(((IntConstant)left).Value - ((IntConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
					}
				}
				catch (OverflowException)
				{
					Error_CompileTimeOverflow(ec, loc);
				}
				return result;
			}
			case Binary.Operator.Multiply:
				if (left is NullLiteral && right is NullLiteral)
				{
					NullableType nullableType10 = new NullableType(ec.BuiltinTypes.Int, loc);
					nullableType10.ResolveAsType(ec);
					return (Constant)new Binary(oper, nullableType10, right).ResolveOperator(ec);
				}
				if (!DoBinaryNumericPromotions(ec, ref left, ref right))
				{
					return null;
				}
				try
				{
					if (!(left is DoubleConstant))
					{
						if (left is FloatConstant)
						{
							double doubleValue9 = ((FloatConstant)left).DoubleValue;
							double doubleValue10 = ((FloatConstant)right).DoubleValue;
							return new FloatConstant(v: (!ec.ConstantCheckState) ? (doubleValue9 * doubleValue10) : (doubleValue9 * doubleValue10), types: ec.BuiltinTypes, loc: left.Location);
						}
						if (!(left is ULongConstant))
						{
							if (!(left is LongConstant))
							{
								if (!(left is UIntConstant))
								{
									if (!(left is IntConstant))
									{
										if (!(left is DecimalConstant))
										{
											throw new Exception("Unexepected multiply input: " + left);
										}
										return new DecimalConstant(d: (!ec.ConstantCheckState) ? (((DecimalConstant)left).Value * ((DecimalConstant)right).Value) : (((DecimalConstant)left).Value * ((DecimalConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
									}
									return new IntConstant(v: (!ec.ConstantCheckState) ? (((IntConstant)left).Value * ((IntConstant)right).Value) : checked(((IntConstant)left).Value * ((IntConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
								}
								return new UIntConstant(v: (!ec.ConstantCheckState) ? (((UIntConstant)left).Value * ((UIntConstant)right).Value) : checked(((UIntConstant)left).Value * ((UIntConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
							}
							return new LongConstant(v: (!ec.ConstantCheckState) ? (((LongConstant)left).Value * ((LongConstant)right).Value) : checked(((LongConstant)left).Value * ((LongConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
						}
						return new ULongConstant(v: (!ec.ConstantCheckState) ? (((ULongConstant)left).Value * ((ULongConstant)right).Value) : checked(((ULongConstant)left).Value * ((ULongConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
					}
					return new DoubleConstant(v: (!ec.ConstantCheckState) ? (((DoubleConstant)left).Value * ((DoubleConstant)right).Value) : (((DoubleConstant)left).Value * ((DoubleConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
				}
				catch (OverflowException)
				{
					Error_CompileTimeOverflow(ec, loc);
				}
				break;
			case Binary.Operator.Division:
				if (left is NullLiteral && right is NullLiteral)
				{
					NullableType nullableType8 = new NullableType(ec.BuiltinTypes.Int, loc);
					nullableType8.ResolveAsType(ec);
					return (Constant)new Binary(oper, nullableType8, right).ResolveOperator(ec);
				}
				if (!DoBinaryNumericPromotions(ec, ref left, ref right))
				{
					return null;
				}
				try
				{
					if (!(left is DoubleConstant))
					{
						if (left is FloatConstant)
						{
							double doubleValue7 = ((FloatConstant)left).DoubleValue;
							double doubleValue8 = ((FloatConstant)right).DoubleValue;
							return new FloatConstant(v: (!ec.ConstantCheckState) ? (doubleValue7 / doubleValue8) : (doubleValue7 / doubleValue8), types: ec.BuiltinTypes, loc: left.Location);
						}
						if (!(left is ULongConstant))
						{
							if (!(left is LongConstant))
							{
								if (!(left is UIntConstant))
								{
									if (!(left is IntConstant))
									{
										if (!(left is DecimalConstant))
										{
											throw new Exception("Unexepected division input: " + left);
										}
										return new DecimalConstant(d: (!ec.ConstantCheckState) ? (((DecimalConstant)left).Value / ((DecimalConstant)right).Value) : (((DecimalConstant)left).Value / ((DecimalConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
									}
									return new IntConstant(v: (!ec.ConstantCheckState) ? (((IntConstant)left).Value / ((IntConstant)right).Value) : (((IntConstant)left).Value / ((IntConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
								}
								return new UIntConstant(v: (!ec.ConstantCheckState) ? (((UIntConstant)left).Value / ((UIntConstant)right).Value) : (((UIntConstant)left).Value / ((UIntConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
							}
							return new LongConstant(v: (!ec.ConstantCheckState) ? (((LongConstant)left).Value / ((LongConstant)right).Value) : (((LongConstant)left).Value / ((LongConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
						}
						return new ULongConstant(v: (!ec.ConstantCheckState) ? (((ULongConstant)left).Value / ((ULongConstant)right).Value) : (((ULongConstant)left).Value / ((ULongConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
					}
					return new DoubleConstant(v: (!ec.ConstantCheckState) ? (((DoubleConstant)left).Value / ((DoubleConstant)right).Value) : (((DoubleConstant)left).Value / ((DoubleConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
				}
				catch (OverflowException)
				{
					Error_CompileTimeOverflow(ec, loc);
				}
				catch (DivideByZeroException)
				{
					ec.Report.Error(20, loc, "Division by constant zero");
				}
				break;
			case Binary.Operator.Modulus:
				if (left is NullLiteral && right is NullLiteral)
				{
					NullableType nullableType7 = new NullableType(ec.BuiltinTypes.Int, loc);
					nullableType7.ResolveAsType(ec);
					return (Constant)new Binary(oper, nullableType7, right).ResolveOperator(ec);
				}
				if (!DoBinaryNumericPromotions(ec, ref left, ref right))
				{
					return null;
				}
				try
				{
					if (!(left is DoubleConstant))
					{
						if (left is FloatConstant)
						{
							double doubleValue5 = ((FloatConstant)left).DoubleValue;
							double doubleValue6 = ((FloatConstant)right).DoubleValue;
							return new FloatConstant(v: (!ec.ConstantCheckState) ? (doubleValue5 % doubleValue6) : (doubleValue5 % doubleValue6), types: ec.BuiltinTypes, loc: left.Location);
						}
						if (!(left is ULongConstant))
						{
							if (!(left is LongConstant))
							{
								if (!(left is UIntConstant))
								{
									if (!(left is IntConstant))
									{
										if (!(left is DecimalConstant))
										{
											throw new Exception("Unexepected modulus input: " + left);
										}
										return new DecimalConstant(d: (!ec.ConstantCheckState) ? (((DecimalConstant)left).Value % ((DecimalConstant)right).Value) : (((DecimalConstant)left).Value % ((DecimalConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
									}
									return new IntConstant(v: (!ec.ConstantCheckState) ? (((IntConstant)left).Value % ((IntConstant)right).Value) : (((IntConstant)left).Value % ((IntConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
								}
								return new UIntConstant(v: (!ec.ConstantCheckState) ? (((UIntConstant)left).Value % ((UIntConstant)right).Value) : (((UIntConstant)left).Value % ((UIntConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
							}
							return new LongConstant(v: (!ec.ConstantCheckState) ? (((LongConstant)left).Value % ((LongConstant)right).Value) : (((LongConstant)left).Value % ((LongConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
						}
						return new ULongConstant(v: (!ec.ConstantCheckState) ? (((ULongConstant)left).Value % ((ULongConstant)right).Value) : (((ULongConstant)left).Value % ((ULongConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
					}
					return new DoubleConstant(v: (!ec.ConstantCheckState) ? (((DoubleConstant)left).Value % ((DoubleConstant)right).Value) : (((DoubleConstant)left).Value % ((DoubleConstant)right).Value), types: ec.BuiltinTypes, loc: left.Location);
				}
				catch (DivideByZeroException)
				{
					ec.Report.Error(20, loc, "Division by constant zero");
				}
				catch (OverflowException)
				{
					Error_CompileTimeOverflow(ec, loc);
				}
				break;
			case Binary.Operator.LeftShift:
			{
				if (left is NullLiteral && right is NullLiteral)
				{
					NullableType nullableType3 = new NullableType(ec.BuiltinTypes.Int, loc);
					nullableType3.ResolveAsType(ec);
					return (Constant)new Binary(oper, nullableType3, right).ResolveOperator(ec);
				}
				IntConstant intConstant2 = right.ConvertImplicitly(ec.BuiltinTypes.Int) as IntConstant;
				if (intConstant2 == null)
				{
					return null;
				}
				int value2 = intConstant2.Value;
				switch (left.Type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.ULong:
					return new ULongConstant(ec.BuiltinTypes, ((ULongConstant)left).Value << value2, left.Location);
				case BuiltinTypeSpec.Type.Long:
					return new LongConstant(ec.BuiltinTypes, ((LongConstant)left).Value << value2, left.Location);
				case BuiltinTypeSpec.Type.UInt:
					return new UIntConstant(ec.BuiltinTypes, ((UIntConstant)left).Value << value2, left.Location);
				default:
					if (left is NullLiteral)
					{
						return (Constant)new Binary(oper, left, right).ResolveOperator(ec);
					}
					left = left.ConvertImplicitly(ec.BuiltinTypes.Int);
					if (left.Type.BuiltinType == BuiltinTypeSpec.Type.Int)
					{
						return new IntConstant(ec.BuiltinTypes, ((IntConstant)left).Value << value2, left.Location);
					}
					return null;
				}
			}
			case Binary.Operator.RightShift:
			{
				if (left is NullLiteral && right is NullLiteral)
				{
					NullableType nullableType2 = new NullableType(ec.BuiltinTypes.Int, loc);
					nullableType2.ResolveAsType(ec);
					return (Constant)new Binary(oper, nullableType2, right).ResolveOperator(ec);
				}
				IntConstant intConstant = right.ConvertImplicitly(ec.BuiltinTypes.Int) as IntConstant;
				if (intConstant == null)
				{
					return null;
				}
				int value = intConstant.Value;
				switch (left.Type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.ULong:
					return new ULongConstant(ec.BuiltinTypes, ((ULongConstant)left).Value >> value, left.Location);
				case BuiltinTypeSpec.Type.Long:
					return new LongConstant(ec.BuiltinTypes, ((LongConstant)left).Value >> value, left.Location);
				case BuiltinTypeSpec.Type.UInt:
					return new UIntConstant(ec.BuiltinTypes, ((UIntConstant)left).Value >> value, left.Location);
				default:
					if (left is NullLiteral)
					{
						return (Constant)new Binary(oper, left, right).ResolveOperator(ec);
					}
					left = left.ConvertImplicitly(ec.BuiltinTypes.Int);
					if (left.Type.BuiltinType == BuiltinTypeSpec.Type.Int)
					{
						return new IntConstant(ec.BuiltinTypes, ((IntConstant)left).Value >> value, left.Location);
					}
					return null;
				}
			}
			case Binary.Operator.Equality:
			{
				if ((TypeSpec.IsReferenceType(type) && TypeSpec.IsReferenceType(type2)) || (left is LiftedNull && right.IsNull) || (right is LiftedNull && left.IsNull))
				{
					if (left.IsNull || right.IsNull)
					{
						return ReducedExpression.Create(new BoolConstant(ec.BuiltinTypes, left.IsNull == right.IsNull, left.Location), new Binary(oper, left, right));
					}
					if (left is StringConstant && right is StringConstant)
					{
						return new BoolConstant(ec.BuiltinTypes, ((StringConstant)left).Value == ((StringConstant)right).Value, left.Location);
					}
					return null;
				}
				if (!DoBinaryNumericPromotions(ec, ref left, ref right))
				{
					return null;
				}
				bool flag3 = false;
				if (left is DoubleConstant)
				{
					flag3 = ((DoubleConstant)left).Value == ((DoubleConstant)right).Value;
				}
				else if (left is FloatConstant)
				{
					flag3 = ((FloatConstant)left).DoubleValue == ((FloatConstant)right).DoubleValue;
				}
				else if (left is ULongConstant)
				{
					flag3 = ((ULongConstant)left).Value == ((ULongConstant)right).Value;
				}
				else if (left is LongConstant)
				{
					flag3 = ((LongConstant)left).Value == ((LongConstant)right).Value;
				}
				else if (left is UIntConstant)
				{
					flag3 = ((UIntConstant)left).Value == ((UIntConstant)right).Value;
				}
				else
				{
					if (!(left is IntConstant))
					{
						return null;
					}
					flag3 = ((IntConstant)left).Value == ((IntConstant)right).Value;
				}
				return new BoolConstant(ec.BuiltinTypes, flag3, left.Location);
			}
			case Binary.Operator.Inequality:
			{
				if ((TypeSpec.IsReferenceType(type) && TypeSpec.IsReferenceType(type2)) || (left is LiftedNull && right.IsNull) || (right is LiftedNull && left.IsNull))
				{
					if (left.IsNull || right.IsNull)
					{
						return ReducedExpression.Create(new BoolConstant(ec.BuiltinTypes, left.IsNull != right.IsNull, left.Location), new Binary(oper, left, right));
					}
					if (left is StringConstant && right is StringConstant)
					{
						return new BoolConstant(ec.BuiltinTypes, ((StringConstant)left).Value != ((StringConstant)right).Value, left.Location);
					}
					return null;
				}
				if (!DoBinaryNumericPromotions(ec, ref left, ref right))
				{
					return null;
				}
				bool flag3 = false;
				if (left is DoubleConstant)
				{
					flag3 = ((DoubleConstant)left).Value != ((DoubleConstant)right).Value;
				}
				else if (left is FloatConstant)
				{
					flag3 = ((FloatConstant)left).DoubleValue != ((FloatConstant)right).DoubleValue;
				}
				else if (left is ULongConstant)
				{
					flag3 = ((ULongConstant)left).Value != ((ULongConstant)right).Value;
				}
				else if (left is LongConstant)
				{
					flag3 = ((LongConstant)left).Value != ((LongConstant)right).Value;
				}
				else if (left is UIntConstant)
				{
					flag3 = ((UIntConstant)left).Value != ((UIntConstant)right).Value;
				}
				else
				{
					if (!(left is IntConstant))
					{
						return null;
					}
					flag3 = ((IntConstant)left).Value != ((IntConstant)right).Value;
				}
				return new BoolConstant(ec.BuiltinTypes, flag3, left.Location);
			}
			case Binary.Operator.LessThan:
			{
				if (right is NullLiteral && left is NullLiteral)
				{
					NullableType nullableType9 = new NullableType(ec.BuiltinTypes.Int, loc);
					nullableType9.ResolveAsType(ec);
					return (Constant)new Binary(oper, nullableType9, right).ResolveOperator(ec);
				}
				if (!DoBinaryNumericPromotions(ec, ref left, ref right))
				{
					return null;
				}
				bool flag3 = false;
				if (left is DoubleConstant)
				{
					flag3 = ((DoubleConstant)left).Value < ((DoubleConstant)right).Value;
				}
				else if (left is FloatConstant)
				{
					flag3 = ((FloatConstant)left).DoubleValue < ((FloatConstant)right).DoubleValue;
				}
				else if (left is ULongConstant)
				{
					flag3 = ((ULongConstant)left).Value < ((ULongConstant)right).Value;
				}
				else if (left is LongConstant)
				{
					flag3 = ((LongConstant)left).Value < ((LongConstant)right).Value;
				}
				else if (left is UIntConstant)
				{
					flag3 = ((UIntConstant)left).Value < ((UIntConstant)right).Value;
				}
				else
				{
					if (!(left is IntConstant))
					{
						return null;
					}
					flag3 = ((IntConstant)left).Value < ((IntConstant)right).Value;
				}
				return new BoolConstant(ec.BuiltinTypes, flag3, left.Location);
			}
			case Binary.Operator.GreaterThan:
			{
				if (right is NullLiteral && left is NullLiteral)
				{
					NullableType nullableType6 = new NullableType(ec.BuiltinTypes.Int, loc);
					nullableType6.ResolveAsType(ec);
					return (Constant)new Binary(oper, nullableType6, right).ResolveOperator(ec);
				}
				if (!DoBinaryNumericPromotions(ec, ref left, ref right))
				{
					return null;
				}
				bool flag3 = false;
				if (left is DoubleConstant)
				{
					flag3 = ((DoubleConstant)left).Value > ((DoubleConstant)right).Value;
				}
				else if (left is FloatConstant)
				{
					flag3 = ((FloatConstant)left).DoubleValue > ((FloatConstant)right).DoubleValue;
				}
				else if (left is ULongConstant)
				{
					flag3 = ((ULongConstant)left).Value > ((ULongConstant)right).Value;
				}
				else if (left is LongConstant)
				{
					flag3 = ((LongConstant)left).Value > ((LongConstant)right).Value;
				}
				else if (left is UIntConstant)
				{
					flag3 = ((UIntConstant)left).Value > ((UIntConstant)right).Value;
				}
				else
				{
					if (!(left is IntConstant))
					{
						return null;
					}
					flag3 = ((IntConstant)left).Value > ((IntConstant)right).Value;
				}
				return new BoolConstant(ec.BuiltinTypes, flag3, left.Location);
			}
			case Binary.Operator.GreaterThanOrEqual:
			{
				if (right is NullLiteral && left is NullLiteral)
				{
					NullableType nullableType5 = new NullableType(ec.BuiltinTypes.Int, loc);
					nullableType5.ResolveAsType(ec);
					return (Constant)new Binary(oper, nullableType5, right).ResolveOperator(ec);
				}
				if (!DoBinaryNumericPromotions(ec, ref left, ref right))
				{
					return null;
				}
				bool flag3 = false;
				if (left is DoubleConstant)
				{
					flag3 = ((DoubleConstant)left).Value >= ((DoubleConstant)right).Value;
				}
				else if (left is FloatConstant)
				{
					flag3 = ((FloatConstant)left).DoubleValue >= ((FloatConstant)right).DoubleValue;
				}
				else if (left is ULongConstant)
				{
					flag3 = ((ULongConstant)left).Value >= ((ULongConstant)right).Value;
				}
				else if (left is LongConstant)
				{
					flag3 = ((LongConstant)left).Value >= ((LongConstant)right).Value;
				}
				else if (left is UIntConstant)
				{
					flag3 = ((UIntConstant)left).Value >= ((UIntConstant)right).Value;
				}
				else
				{
					if (!(left is IntConstant))
					{
						return null;
					}
					flag3 = ((IntConstant)left).Value >= ((IntConstant)right).Value;
				}
				return new BoolConstant(ec.BuiltinTypes, flag3, left.Location);
			}
			case Binary.Operator.LessThanOrEqual:
			{
				if (right is NullLiteral && left is NullLiteral)
				{
					NullableType nullableType = new NullableType(ec.BuiltinTypes.Int, loc);
					nullableType.ResolveAsType(ec);
					return (Constant)new Binary(oper, nullableType, right).ResolveOperator(ec);
				}
				if (!DoBinaryNumericPromotions(ec, ref left, ref right))
				{
					return null;
				}
				bool flag3 = false;
				if (left is DoubleConstant)
				{
					flag3 = ((DoubleConstant)left).Value <= ((DoubleConstant)right).Value;
				}
				else if (left is FloatConstant)
				{
					flag3 = ((FloatConstant)left).DoubleValue <= ((FloatConstant)right).DoubleValue;
				}
				else if (left is ULongConstant)
				{
					flag3 = ((ULongConstant)left).Value <= ((ULongConstant)right).Value;
				}
				else if (left is LongConstant)
				{
					flag3 = ((LongConstant)left).Value <= ((LongConstant)right).Value;
				}
				else if (left is UIntConstant)
				{
					flag3 = ((UIntConstant)left).Value <= ((UIntConstant)right).Value;
				}
				else
				{
					if (!(left is IntConstant))
					{
						return null;
					}
					flag3 = ((IntConstant)left).Value <= ((IntConstant)right).Value;
				}
				return new BoolConstant(ec.BuiltinTypes, flag3, left.Location);
			}
			}
			return null;
		}
	}
}
