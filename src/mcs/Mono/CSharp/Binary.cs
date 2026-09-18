using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection.Emit;
using Mono.CSharp.Nullable;

namespace Mono.CSharp
{
	public class Binary : Expression, IDynamicBinder
	{
		public class PredefinedOperator
		{
			protected readonly TypeSpec left;

			protected readonly TypeSpec right;

			protected readonly TypeSpec left_unwrap;

			protected readonly TypeSpec right_unwrap;

			public readonly Operator OperatorsMask;

			public TypeSpec ReturnType;

			public bool IsLifted => (OperatorsMask & Operator.NullableMask) != 0;

			public PredefinedOperator(TypeSpec ltype, TypeSpec rtype, Operator op_mask)
				: this(ltype, rtype, op_mask, ltype)
			{
			}

			public PredefinedOperator(TypeSpec type, Operator op_mask, TypeSpec return_type)
				: this(type, type, op_mask, return_type)
			{
			}

			public PredefinedOperator(TypeSpec type, Operator op_mask)
				: this(type, type, op_mask, type)
			{
			}

			public PredefinedOperator(TypeSpec ltype, TypeSpec rtype, Operator op_mask, TypeSpec return_type)
			{
				if ((op_mask & Operator.ValuesOnlyMask) != 0)
				{
					throw new InternalErrorException("Only masked values can be used");
				}
				if ((op_mask & Operator.NullableMask) != 0)
				{
					left_unwrap = NullableInfo.GetUnderlyingType(ltype);
					right_unwrap = NullableInfo.GetUnderlyingType(rtype);
				}
				else
				{
					left_unwrap = ltype;
					right_unwrap = rtype;
				}
				left = ltype;
				right = rtype;
				OperatorsMask = op_mask;
				ReturnType = return_type;
			}

			public virtual Expression ConvertResult(ResolveContext rc, Binary b)
			{
				Expression expression = b.left;
				Expression expression2 = b.right;
				b.type = ReturnType;
				if (IsLifted)
				{
					if (rc.HasSet(ResolveContext.Options.ExpressionTreeConversion))
					{
						b.left = Convert.ImplicitConversion(rc, b.left, left, b.left.Location);
						b.right = Convert.ImplicitConversion(rc, b.right, right, b.right.Location);
					}
					if (expression2.IsNull)
					{
						if ((b.oper & Operator.EqualityMask) != 0)
						{
							if (!expression.Type.IsNullableType && BuiltinTypeSpec.IsPrimitiveType(expression.Type))
							{
								return b.CreateLiftedValueTypeResult(rc, expression.Type);
							}
						}
						else
						{
							if ((b.oper & Operator.BitwiseMask) == 0)
							{
								b.left = Convert.ImplicitConversion(rc, b.left, left, b.left.Location);
								b.right = Convert.ImplicitConversion(rc, b.right, right, b.right.Location);
								if ((b.Oper & (Operator.Multiply | Operator.ShiftMask)) != 0)
								{
									return LiftedNull.CreateFromExpression(rc, b);
								}
								return b.CreateLiftedValueTypeResult(rc, left);
							}
							if (left_unwrap.BuiltinType != BuiltinTypeSpec.Type.FirstPrimitive)
							{
								return LiftedNull.CreateFromExpression(rc, b);
							}
						}
					}
					else if (expression.IsNull)
					{
						if ((b.oper & Operator.EqualityMask) != 0)
						{
							if (!expression2.Type.IsNullableType && BuiltinTypeSpec.IsPrimitiveType(expression2.Type))
							{
								return b.CreateLiftedValueTypeResult(rc, expression2.Type);
							}
						}
						else
						{
							if ((b.oper & Operator.BitwiseMask) == 0)
							{
								b.left = Convert.ImplicitConversion(rc, b.left, left, b.left.Location);
								b.right = Convert.ImplicitConversion(rc, b.right, right, b.right.Location);
								if ((b.Oper & (Operator.Multiply | Operator.ShiftMask)) != 0)
								{
									return LiftedNull.CreateFromExpression(rc, b);
								}
								return b.CreateLiftedValueTypeResult(rc, right);
							}
							if (right_unwrap.BuiltinType != BuiltinTypeSpec.Type.FirstPrimitive)
							{
								return LiftedNull.CreateFromExpression(rc, b);
							}
						}
					}
				}
				if (left.BuiltinType == BuiltinTypeSpec.Type.Decimal)
				{
					b.left = Convert.ImplicitConversion(rc, b.left, left, b.left.Location);
					b.right = Convert.ImplicitConversion(rc, b.right, right, b.right.Location);
					return b.ResolveUserOperator(rc, b.left, b.right);
				}
				Constant constant = expression2 as Constant;
				if (constant != null)
				{
					if (constant.IsDefaultValue)
					{
						if (b.oper == Operator.Addition || b.oper == Operator.Subtraction || (b.oper == Operator.BitwiseOr && left_unwrap.BuiltinType == BuiltinTypeSpec.Type.FirstPrimitive && constant is BoolConstant))
						{
							b.left = Convert.ImplicitConversion(rc, b.left, left, b.left.Location);
							return ReducedExpression.Create(b.left, b).Resolve(rc);
						}
						if ((b.oper == Operator.BitwiseAnd || b.oper == Operator.LogicalAnd) && !IsLifted)
						{
							return ReducedExpression.Create(new SideEffectConstant(constant, b.left, constant.Location), b);
						}
					}
					else if (IsLifted && left_unwrap.BuiltinType == BuiltinTypeSpec.Type.FirstPrimitive && b.oper == Operator.BitwiseAnd)
					{
						return ReducedExpression.Create(b.left, b).Resolve(rc);
					}
					if ((b.oper == Operator.Multiply || b.oper == Operator.Division) && constant.IsOneInteger)
					{
						return ReducedExpression.Create(b.left, b).Resolve(rc);
					}
					if ((b.oper & Operator.ShiftMask) != 0 && constant is IntConstant)
					{
						b.right = new IntConstant(rc.BuiltinTypes, ((IntConstant)constant).Value & GetShiftMask(left_unwrap), b.right.Location);
					}
				}
				constant = b.left as Constant;
				if (constant != null)
				{
					if (constant.IsDefaultValue)
					{
						if (b.oper == Operator.Addition || (b.oper == Operator.BitwiseOr && right_unwrap.BuiltinType == BuiltinTypeSpec.Type.FirstPrimitive && constant is BoolConstant))
						{
							b.right = Convert.ImplicitConversion(rc, b.right, right, b.right.Location);
							return ReducedExpression.Create(b.right, b).Resolve(rc);
						}
						if (b.oper == Operator.LogicalAnd && constant.Type.BuiltinType == BuiltinTypeSpec.Type.FirstPrimitive)
						{
							Expression.Warning_UnreachableExpression(rc, b.right.StartLocation);
							return ReducedExpression.Create(constant, b);
						}
						if (b.oper == Operator.BitwiseAnd && !IsLifted)
						{
							return ReducedExpression.Create(new SideEffectConstant(constant, b.right, constant.Location), b);
						}
					}
					else
					{
						if (IsLifted && left_unwrap.BuiltinType == BuiltinTypeSpec.Type.FirstPrimitive && b.oper == Operator.BitwiseAnd)
						{
							return ReducedExpression.Create(b.right, b).Resolve(rc);
						}
						if (b.oper == Operator.LogicalOr && constant.Type.BuiltinType == BuiltinTypeSpec.Type.FirstPrimitive)
						{
							Expression.Warning_UnreachableExpression(rc, b.right.StartLocation);
							return ReducedExpression.Create(constant, b);
						}
					}
					if (b.oper == Operator.Multiply && constant.IsOneInteger)
					{
						return ReducedExpression.Create(b.right, b).Resolve(rc);
					}
				}
				if (IsLifted)
				{
					LiftedBinaryOperator liftedBinaryOperator = new LiftedBinaryOperator(b);
					TypeSpec target_type;
					if (b.left.Type.IsNullableType)
					{
						liftedBinaryOperator.UnwrapLeft = new Unwrap(b.left);
						target_type = left_unwrap;
					}
					else
					{
						target_type = left;
					}
					TypeSpec target_type2;
					if (b.right.Type.IsNullableType)
					{
						liftedBinaryOperator.UnwrapRight = new Unwrap(b.right);
						target_type2 = right_unwrap;
					}
					else
					{
						target_type2 = right;
					}
					liftedBinaryOperator.Left = (b.left.IsNull ? b.left : Convert.ImplicitConversion(rc, liftedBinaryOperator.UnwrapLeft ?? b.left, target_type, b.left.Location));
					liftedBinaryOperator.Right = (b.right.IsNull ? b.right : Convert.ImplicitConversion(rc, liftedBinaryOperator.UnwrapRight ?? b.right, target_type2, b.right.Location));
					return liftedBinaryOperator.Resolve(rc);
				}
				b.left = Convert.ImplicitConversion(rc, b.left, left, b.left.Location);
				b.right = Convert.ImplicitConversion(rc, b.right, right, b.right.Location);
				return b;
			}

			public bool IsPrimitiveApplicable(TypeSpec ltype, TypeSpec rtype)
			{
				if (left == ltype)
				{
					return ltype == rtype;
				}
				return false;
			}

			public virtual bool IsApplicable(ResolveContext ec, Expression lexpr, Expression rexpr)
			{
				if (left == lexpr.Type && right == rexpr.Type)
				{
					return true;
				}
				if (Convert.ImplicitConversionExists(ec, lexpr, left))
				{
					return Convert.ImplicitConversionExists(ec, rexpr, right);
				}
				return false;
			}

			public PredefinedOperator ResolveBetterOperator(ResolveContext ec, PredefinedOperator best_operator)
			{
				if ((OperatorsMask & Operator.DecomposedMask) != 0)
				{
					return best_operator;
				}
				if ((best_operator.OperatorsMask & Operator.DecomposedMask) != 0)
				{
					return this;
				}
				int num = 0;
				if (left != null && best_operator.left != null)
				{
					num = OverloadResolver.BetterTypeConversion(ec, best_operator.left_unwrap, left_unwrap);
				}
				if (right != null && (left != right || best_operator.left != best_operator.right))
				{
					num |= OverloadResolver.BetterTypeConversion(ec, best_operator.right_unwrap, right_unwrap);
				}
				if (num == 0 || num > 2)
				{
					return null;
				}
				if (num != 1)
				{
					return this;
				}
				return best_operator;
			}
		}

		private sealed class PredefinedStringOperator : PredefinedOperator
		{
			public PredefinedStringOperator(TypeSpec type, Operator op_mask, TypeSpec retType)
				: base(type, type, op_mask, retType)
			{
			}

			public PredefinedStringOperator(TypeSpec ltype, TypeSpec rtype, Operator op_mask, TypeSpec retType)
				: base(ltype, rtype, op_mask, retType)
			{
			}

			public override Expression ConvertResult(ResolveContext ec, Binary b)
			{
				Unwrap unwrap = b.left as Unwrap;
				if (unwrap != null)
				{
					b.left = unwrap.Original;
				}
				unwrap = b.right as Unwrap;
				if (unwrap != null)
				{
					b.right = unwrap.Original;
				}
				b.left = Convert.ImplicitConversion(ec, b.left, left, b.left.Location);
				b.right = Convert.ImplicitConversion(ec, b.right, right, b.right.Location);
				return StringConcat.Create(ec, b.left, b.right, b.loc);
			}
		}

		private sealed class PredefinedEqualityOperator : PredefinedOperator
		{
			private MethodSpec equal_method;

			private MethodSpec inequal_method;

			public PredefinedEqualityOperator(TypeSpec arg, TypeSpec retType)
				: base(arg, arg, Operator.EqualityMask, retType)
			{
			}

			public override Expression ConvertResult(ResolveContext ec, Binary b)
			{
				b.type = ReturnType;
				b.left = Convert.ImplicitConversion(ec, b.left, left, b.left.Location);
				b.right = Convert.ImplicitConversion(ec, b.right, right, b.right.Location);
				Arguments arguments = new Arguments(2);
				arguments.Add(new Argument(b.left));
				arguments.Add(new Argument(b.right));
				MethodSpec oper;
				if (b.oper == Operator.Equality)
				{
					if (equal_method == null)
					{
						if (left.BuiltinType == BuiltinTypeSpec.Type.String)
						{
							equal_method = ec.Module.PredefinedMembers.StringEqual.Resolve(b.loc);
						}
						else
						{
							if (left.BuiltinType != BuiltinTypeSpec.Type.Delegate)
							{
								throw new NotImplementedException(left.GetSignatureForError());
							}
							equal_method = ec.Module.PredefinedMembers.DelegateEqual.Resolve(b.loc);
						}
					}
					oper = equal_method;
				}
				else
				{
					if (inequal_method == null)
					{
						if (left.BuiltinType == BuiltinTypeSpec.Type.String)
						{
							inequal_method = ec.Module.PredefinedMembers.StringInequal.Resolve(b.loc);
						}
						else
						{
							if (left.BuiltinType != BuiltinTypeSpec.Type.Delegate)
							{
								throw new NotImplementedException(left.GetSignatureForError());
							}
							inequal_method = ec.Module.PredefinedMembers.DelegateInequal.Resolve(b.loc);
						}
					}
					oper = inequal_method;
				}
				return new UserOperatorCall(oper, arguments, b.CreateExpressionTree, b.loc);
			}
		}

		private class PredefinedPointerOperator : PredefinedOperator
		{
			public PredefinedPointerOperator(TypeSpec ltype, TypeSpec rtype, Operator op_mask)
				: base(ltype, rtype, op_mask)
			{
			}

			public PredefinedPointerOperator(TypeSpec ltype, TypeSpec rtype, Operator op_mask, TypeSpec retType)
				: base(ltype, rtype, op_mask, retType)
			{
			}

			public PredefinedPointerOperator(TypeSpec type, Operator op_mask, TypeSpec return_type)
				: base(type, op_mask, return_type)
			{
			}

			public override bool IsApplicable(ResolveContext ec, Expression lexpr, Expression rexpr)
			{
				if (left == null)
				{
					if (!lexpr.Type.IsPointer)
					{
						return false;
					}
				}
				else if (!Convert.ImplicitConversionExists(ec, lexpr, left))
				{
					return false;
				}
				if (right == null)
				{
					if (!rexpr.Type.IsPointer)
					{
						return false;
					}
				}
				else if (!Convert.ImplicitConversionExists(ec, rexpr, right))
				{
					return false;
				}
				return true;
			}

			public override Expression ConvertResult(ResolveContext ec, Binary b)
			{
				if (left != null)
				{
					b.left = EmptyCast.Create(b.left, left);
				}
				else if (right != null)
				{
					b.right = EmptyCast.Create(b.right, right);
				}
				TypeSpec typeSpec = ReturnType;
				Expression l;
				Expression r;
				if (typeSpec == null)
				{
					if (left == null)
					{
						l = b.left;
						r = b.right;
						typeSpec = b.left.Type;
					}
					else
					{
						l = b.right;
						r = b.left;
						typeSpec = b.right.Type;
					}
				}
				else
				{
					l = b.left;
					r = b.right;
				}
				return new PointerArithmetic(b.oper, l, r, typeSpec, b.loc).Resolve(ec);
			}
		}

		[Flags]
		public enum Operator
		{
			Multiply = 0x20,
			Division = 0x21,
			Modulus = 0x22,
			Addition = 0x823,
			Subtraction = 0x1024,
			LeftShift = 0x45,
			RightShift = 0x46,
			LessThan = 0x2087,
			GreaterThan = 0x2088,
			LessThanOrEqual = 0x2089,
			GreaterThanOrEqual = 0x208A,
			Equality = 0x18B,
			Inequality = 0x18C,
			BitwiseAnd = 0x20D,
			ExclusiveOr = 0x20E,
			BitwiseOr = 0x20F,
			LogicalAnd = 0x410,
			LogicalOr = 0x411,
			ValuesOnlyMask = 0x1F,
			ArithmeticMask = Multiply,
			ShiftMask = 0x40,
			ComparisonMask = 0x80,
			EqualityMask = 0x100,
			BitwiseMask = 0x200,
			LogicalMask = 0x400,
			AdditionMask = 0x800,
			SubtractionMask = 0x1000,
			RelationalMask = 0x2000,
			DecomposedMask = 0x80000,
			NullableMask = 0x100000
		}

		[Flags]
		public enum State : byte
		{
			None = 0,
			Compound = 2,
			UserOperatorsExcluded = 4
		}

		private readonly Operator oper;

		private Expression left;

		private Expression right;

		private State state;

		private ConvCast.Mode enum_conversion;

		public bool IsCompound => (state & State.Compound) != 0;

		public Operator Oper => oper;

		public Expression Left => left;

		public Expression Right => right;

		public override Location StartLocation => left.StartLocation;

		public Binary(Operator oper, Expression left, Expression right, bool isCompound)
			: this(oper, left, right, State.Compound)
		{
		}

		public Binary(Operator oper, Expression left, Expression right, State state)
			: this(oper, left, right)
		{
			this.state = state;
		}

		public Binary(Operator oper, Expression left, Expression right)
			: this(oper, left, right, left.Location)
		{
		}

		public Binary(Operator oper, Expression left, Expression right, Location loc)
		{
			this.oper = oper;
			this.left = left;
			this.right = right;
			base.loc = loc;
		}

		private string OperName(Operator oper)
		{
			string text;
			switch (oper)
			{
			case Operator.Multiply:
				text = "*";
				break;
			case Operator.Division:
				text = "/";
				break;
			case Operator.Modulus:
				text = "%";
				break;
			case Operator.Addition:
				text = "+";
				break;
			case Operator.Subtraction:
				text = "-";
				break;
			case Operator.LeftShift:
				text = "<<";
				break;
			case Operator.RightShift:
				text = ">>";
				break;
			case Operator.LessThan:
				text = "<";
				break;
			case Operator.GreaterThan:
				text = ">";
				break;
			case Operator.LessThanOrEqual:
				text = "<=";
				break;
			case Operator.GreaterThanOrEqual:
				text = ">=";
				break;
			case Operator.Equality:
				text = "==";
				break;
			case Operator.Inequality:
				text = "!=";
				break;
			case Operator.BitwiseAnd:
				text = "&";
				break;
			case Operator.BitwiseOr:
				text = "|";
				break;
			case Operator.ExclusiveOr:
				text = "^";
				break;
			case Operator.LogicalOr:
				text = "||";
				break;
			case Operator.LogicalAnd:
				text = "&&";
				break;
			default:
				text = oper.ToString();
				break;
			}
			if (IsCompound)
			{
				return text + "=";
			}
			return text;
		}

		public static void Error_OperatorCannotBeApplied(ResolveContext ec, Expression left, Expression right, Operator oper, Location loc)
		{
			new Binary(oper, left, right).Error_OperatorCannotBeApplied(ec, left, right);
		}

		public static void Error_OperatorCannotBeApplied(ResolveContext ec, Expression left, Expression right, string oper, Location loc)
		{
			if (left.Type != InternalType.ErrorType && right.Type != InternalType.ErrorType)
			{
				string signatureForError = left.Type.GetSignatureForError();
				string signatureForError2 = right.Type.GetSignatureForError();
				ec.Report.Error(19, loc, "Operator `{0}' cannot be applied to operands of type `{1}' and `{2}'", oper, signatureForError, signatureForError2);
			}
		}

		private void Error_OperatorCannotBeApplied(ResolveContext ec, Expression left, Expression right)
		{
			Error_OperatorCannotBeApplied(ec, left, right, OperName(oper), loc);
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			if ((oper & Operator.LogicalMask) == 0)
			{
				left.FlowAnalysis(fc);
				right.FlowAnalysis(fc);
				return;
			}
			left.FlowAnalysisConditional(fc);
			DefiniteAssignmentBitSet definiteAssignmentOnTrue = fc.DefiniteAssignmentOnTrue;
			DefiniteAssignmentBitSet definiteAssignmentOnFalse = fc.DefiniteAssignmentOnFalse;
			DefiniteAssignmentBitSet definiteAssignmentBitSet = (fc.DefiniteAssignment = new DefiniteAssignmentBitSet((oper == Operator.LogicalOr) ? definiteAssignmentOnFalse : definiteAssignmentOnTrue));
			DefiniteAssignmentBitSet definiteAssignmentOnTrue2 = (fc.DefiniteAssignmentOnFalse = definiteAssignmentBitSet);
			fc.DefiniteAssignmentOnTrue = definiteAssignmentOnTrue2;
			right.FlowAnalysisConditional(fc);
			if (oper == Operator.LogicalOr)
			{
				fc.DefiniteAssignment = (definiteAssignmentOnFalse | (fc.DefiniteAssignmentOnFalse & fc.DefiniteAssignmentOnTrue)) & definiteAssignmentOnTrue;
			}
			else
			{
				fc.DefiniteAssignment = (definiteAssignmentOnTrue | (fc.DefiniteAssignmentOnFalse & fc.DefiniteAssignmentOnTrue)) & definiteAssignmentOnFalse;
			}
		}

		public override void FlowAnalysisConditional(FlowAnalysisContext fc)
		{
			if ((oper & Operator.LogicalMask) == 0)
			{
				base.FlowAnalysisConditional(fc);
				return;
			}
			left.FlowAnalysisConditional(fc);
			DefiniteAssignmentBitSet definiteAssignmentOnTrue = fc.DefiniteAssignmentOnTrue;
			DefiniteAssignmentBitSet definiteAssignmentOnFalse = fc.DefiniteAssignmentOnFalse;
			DefiniteAssignmentBitSet definiteAssignmentBitSet = (fc.DefiniteAssignment = new DefiniteAssignmentBitSet((oper == Operator.LogicalOr) ? definiteAssignmentOnFalse : definiteAssignmentOnTrue));
			DefiniteAssignmentBitSet definiteAssignmentOnTrue2 = (fc.DefiniteAssignmentOnFalse = definiteAssignmentBitSet);
			fc.DefiniteAssignmentOnTrue = definiteAssignmentOnTrue2;
			right.FlowAnalysisConditional(fc);
			Constant constant = left as Constant;
			if (oper == Operator.LogicalOr)
			{
				fc.DefiniteAssignmentOnFalse = definiteAssignmentOnFalse | fc.DefiniteAssignmentOnFalse;
				if (constant != null && constant.IsDefaultValue)
				{
					fc.DefiniteAssignmentOnTrue = fc.DefiniteAssignmentOnFalse;
				}
				else
				{
					fc.DefiniteAssignmentOnTrue = new DefiniteAssignmentBitSet(definiteAssignmentOnTrue & (definiteAssignmentOnFalse | fc.DefiniteAssignmentOnTrue));
				}
			}
			else
			{
				fc.DefiniteAssignmentOnTrue = definiteAssignmentOnTrue | fc.DefiniteAssignmentOnTrue;
				if (constant != null && !constant.IsDefaultValue)
				{
					fc.DefiniteAssignmentOnFalse = fc.DefiniteAssignmentOnTrue;
				}
				else
				{
					fc.DefiniteAssignmentOnFalse = new DefiniteAssignmentBitSet((definiteAssignmentOnTrue | fc.DefiniteAssignmentOnFalse) & definiteAssignmentOnFalse);
				}
			}
		}

		private string GetOperatorExpressionTypeName()
		{
			switch (oper)
			{
			case Operator.Addition:
				if (!IsCompound)
				{
					return "Add";
				}
				return "AddAssign";
			case Operator.BitwiseAnd:
				if (!IsCompound)
				{
					return "And";
				}
				return "AndAssign";
			case Operator.BitwiseOr:
				if (!IsCompound)
				{
					return "Or";
				}
				return "OrAssign";
			case Operator.Division:
				if (!IsCompound)
				{
					return "Divide";
				}
				return "DivideAssign";
			case Operator.ExclusiveOr:
				if (!IsCompound)
				{
					return "ExclusiveOr";
				}
				return "ExclusiveOrAssign";
			case Operator.Equality:
				return "Equal";
			case Operator.GreaterThan:
				return "GreaterThan";
			case Operator.GreaterThanOrEqual:
				return "GreaterThanOrEqual";
			case Operator.Inequality:
				return "NotEqual";
			case Operator.LeftShift:
				if (!IsCompound)
				{
					return "LeftShift";
				}
				return "LeftShiftAssign";
			case Operator.LessThan:
				return "LessThan";
			case Operator.LessThanOrEqual:
				return "LessThanOrEqual";
			case Operator.LogicalAnd:
				return "And";
			case Operator.LogicalOr:
				return "Or";
			case Operator.Modulus:
				if (!IsCompound)
				{
					return "Modulo";
				}
				return "ModuloAssign";
			case Operator.Multiply:
				if (!IsCompound)
				{
					return "Multiply";
				}
				return "MultiplyAssign";
			case Operator.RightShift:
				if (!IsCompound)
				{
					return "RightShift";
				}
				return "RightShiftAssign";
			case Operator.Subtraction:
				if (!IsCompound)
				{
					return "Subtract";
				}
				return "SubtractAssign";
			default:
				throw new NotImplementedException("Unknown expression type operator " + oper);
			}
		}

		private static Mono.CSharp.Operator.OpType ConvertBinaryToUserOperator(Operator op)
		{
			switch (op)
			{
			case Operator.Addition:
				return Mono.CSharp.Operator.OpType.Addition;
			case Operator.BitwiseAnd:
			case Operator.LogicalAnd:
				return Mono.CSharp.Operator.OpType.BitwiseAnd;
			case Operator.BitwiseOr:
			case Operator.LogicalOr:
				return Mono.CSharp.Operator.OpType.BitwiseOr;
			case Operator.Division:
				return Mono.CSharp.Operator.OpType.Division;
			case Operator.Equality:
				return Mono.CSharp.Operator.OpType.Equality;
			case Operator.ExclusiveOr:
				return Mono.CSharp.Operator.OpType.ExclusiveOr;
			case Operator.GreaterThan:
				return Mono.CSharp.Operator.OpType.GreaterThan;
			case Operator.GreaterThanOrEqual:
				return Mono.CSharp.Operator.OpType.GreaterThanOrEqual;
			case Operator.Inequality:
				return Mono.CSharp.Operator.OpType.Inequality;
			case Operator.LeftShift:
				return Mono.CSharp.Operator.OpType.LeftShift;
			case Operator.LessThan:
				return Mono.CSharp.Operator.OpType.LessThan;
			case Operator.LessThanOrEqual:
				return Mono.CSharp.Operator.OpType.LessThanOrEqual;
			case Operator.Modulus:
				return Mono.CSharp.Operator.OpType.Modulus;
			case Operator.Multiply:
				return Mono.CSharp.Operator.OpType.Multiply;
			case Operator.RightShift:
				return Mono.CSharp.Operator.OpType.RightShift;
			case Operator.Subtraction:
				return Mono.CSharp.Operator.OpType.Subtraction;
			default:
				throw new InternalErrorException(op.ToString());
			}
		}

		public override bool ContainsEmitWithAwait()
		{
			if (!left.ContainsEmitWithAwait())
			{
				return right.ContainsEmitWithAwait();
			}
			return true;
		}

		public static void EmitOperatorOpcode(EmitContext ec, Operator oper, TypeSpec l, Expression right)
		{
			OpCode opcode;
			switch (oper)
			{
			case Operator.Multiply:
				opcode = ((!ec.HasSet(BuilderContext.Options.CheckedScope)) ? OpCodes.Mul : ((l.BuiltinType != BuiltinTypeSpec.Type.Int && l.BuiltinType != BuiltinTypeSpec.Type.Long) ? (IsFloat(l) ? OpCodes.Mul : OpCodes.Mul_Ovf_Un) : OpCodes.Mul_Ovf));
				break;
			case Operator.Division:
				opcode = ((!IsUnsigned(l)) ? OpCodes.Div : OpCodes.Div_Un);
				break;
			case Operator.Modulus:
				opcode = ((!IsUnsigned(l)) ? OpCodes.Rem : OpCodes.Rem_Un);
				break;
			case Operator.Addition:
				opcode = ((!ec.HasSet(BuilderContext.Options.CheckedScope)) ? OpCodes.Add : ((l.BuiltinType != BuiltinTypeSpec.Type.Int && l.BuiltinType != BuiltinTypeSpec.Type.Long) ? (IsFloat(l) ? OpCodes.Add : OpCodes.Add_Ovf_Un) : OpCodes.Add_Ovf));
				break;
			case Operator.Subtraction:
				opcode = ((!ec.HasSet(BuilderContext.Options.CheckedScope)) ? OpCodes.Sub : ((l.BuiltinType != BuiltinTypeSpec.Type.Int && l.BuiltinType != BuiltinTypeSpec.Type.Long) ? (IsFloat(l) ? OpCodes.Sub : OpCodes.Sub_Ovf_Un) : OpCodes.Sub_Ovf));
				break;
			case Operator.RightShift:
				if (!(right is IntConstant))
				{
					ec.EmitInt(GetShiftMask(l));
					ec.Emit(OpCodes.And);
				}
				opcode = ((!IsUnsigned(l)) ? OpCodes.Shr : OpCodes.Shr_Un);
				break;
			case Operator.LeftShift:
				if (!(right is IntConstant))
				{
					ec.EmitInt(GetShiftMask(l));
					ec.Emit(OpCodes.And);
				}
				opcode = OpCodes.Shl;
				break;
			case Operator.Equality:
				opcode = OpCodes.Ceq;
				break;
			case Operator.Inequality:
				ec.Emit(OpCodes.Ceq);
				ec.EmitInt(0);
				opcode = OpCodes.Ceq;
				break;
			case Operator.LessThan:
				opcode = ((!IsUnsigned(l)) ? OpCodes.Clt : OpCodes.Clt_Un);
				break;
			case Operator.GreaterThan:
				opcode = ((!IsUnsigned(l)) ? OpCodes.Cgt : OpCodes.Cgt_Un);
				break;
			case Operator.LessThanOrEqual:
				if (IsUnsigned(l) || IsFloat(l))
				{
					ec.Emit(OpCodes.Cgt_Un);
				}
				else
				{
					ec.Emit(OpCodes.Cgt);
				}
				ec.EmitInt(0);
				opcode = OpCodes.Ceq;
				break;
			case Operator.GreaterThanOrEqual:
				if (IsUnsigned(l) || IsFloat(l))
				{
					ec.Emit(OpCodes.Clt_Un);
				}
				else
				{
					ec.Emit(OpCodes.Clt);
				}
				ec.EmitInt(0);
				opcode = OpCodes.Ceq;
				break;
			case Operator.BitwiseOr:
				opcode = OpCodes.Or;
				break;
			case Operator.BitwiseAnd:
				opcode = OpCodes.And;
				break;
			case Operator.ExclusiveOr:
				opcode = OpCodes.Xor;
				break;
			default:
				throw new InternalErrorException(oper.ToString());
			}
			ec.Emit(opcode);
		}

		private static int GetShiftMask(TypeSpec type)
		{
			if (type.BuiltinType != BuiltinTypeSpec.Type.Int && type.BuiltinType != BuiltinTypeSpec.Type.UInt)
			{
				return 63;
			}
			return 31;
		}

		private static bool IsUnsigned(TypeSpec t)
		{
			switch (t.BuiltinType)
			{
			case BuiltinTypeSpec.Type.Byte:
			case BuiltinTypeSpec.Type.Char:
			case BuiltinTypeSpec.Type.UShort:
			case BuiltinTypeSpec.Type.UInt:
			case BuiltinTypeSpec.Type.ULong:
				return true;
			default:
				return t.IsPointer;
			}
		}

		private static bool IsFloat(TypeSpec t)
		{
			if (t.BuiltinType != BuiltinTypeSpec.Type.Float)
			{
				return t.BuiltinType == BuiltinTypeSpec.Type.Double;
			}
			return true;
		}

		public Expression ResolveOperator(ResolveContext rc)
		{
			eclass = ExprClass.Value;
			TypeSpec typeSpec = left.Type;
			TypeSpec typeSpec2 = right.Type;
			bool flag = false;
			Expression expression;
			if ((BuiltinTypeSpec.IsPrimitiveType(typeSpec) || (typeSpec.IsNullableType && BuiltinTypeSpec.IsPrimitiveType(NullableInfo.GetUnderlyingType(typeSpec)))) && (BuiltinTypeSpec.IsPrimitiveType(typeSpec2) || (typeSpec2.IsNullableType && BuiltinTypeSpec.IsPrimitiveType(NullableInfo.GetUnderlyingType(typeSpec2)))))
			{
				if ((oper & Operator.ShiftMask) == 0)
				{
					if (!DoBinaryOperatorPromotion(rc))
					{
						return null;
					}
					flag = BuiltinTypeSpec.IsPrimitiveType(typeSpec) && BuiltinTypeSpec.IsPrimitiveType(typeSpec2);
				}
			}
			else
			{
				if (typeSpec.IsPointer || typeSpec2.IsPointer)
				{
					return ResolveOperatorPointer(rc, typeSpec, typeSpec2);
				}
				if ((state & State.UserOperatorsExcluded) == 0)
				{
					expression = ResolveUserOperator(rc, left, right);
					if (expression != null)
					{
						return expression;
					}
				}
				bool isEnum = typeSpec.IsEnum;
				bool isEnum2 = typeSpec2.IsEnum;
				if ((oper & (Operator.ComparisonMask | Operator.BitwiseMask)) != 0)
				{
					if (IsEnumOrNullableEnum(typeSpec) || IsEnumOrNullableEnum(typeSpec2))
					{
						expression = ResolveSingleEnumOperators(rc, isEnum, isEnum2, typeSpec, typeSpec2);
						if (expression == null)
						{
							return null;
						}
						if ((oper & Operator.BitwiseMask) != 0)
						{
							expression = EmptyCast.Create(expression, type);
							enum_conversion = GetEnumResultCast(type);
							if (oper == Operator.BitwiseAnd && left.Type.IsEnum && right.Type.IsEnum)
							{
								expression = OptimizeAndOperation(expression);
							}
						}
						left = ConvertEnumOperandToUnderlyingType(rc, left, typeSpec2.IsNullableType);
						right = ConvertEnumOperandToUnderlyingType(rc, right, typeSpec.IsNullableType);
						return expression;
					}
				}
				else if (oper == Operator.Addition || oper == Operator.Subtraction)
				{
					if (IsEnumOrNullableEnum(typeSpec) || IsEnumOrNullableEnum(typeSpec2))
					{
						expression = ResolveEnumOperators(rc, isEnum, isEnum2, typeSpec, typeSpec2);
						if (expression != null)
						{
							left = ConvertEnumOperandToUnderlyingType(rc, left, false);
							right = ConvertEnumOperandToUnderlyingType(rc, right, false);
							return expression;
						}
					}
					else if (typeSpec.IsDelegate || typeSpec2.IsDelegate)
					{
						expression = ResolveOperatorDelegate(rc, typeSpec, typeSpec2);
						if (expression != null)
						{
							return expression;
						}
					}
				}
			}
			if ((oper & Operator.EqualityMask) != 0)
			{
				return ResolveEquality(rc, typeSpec, typeSpec2, flag);
			}
			expression = ResolveOperatorPredefined(rc, rc.BuiltinTypes.OperatorsBinaryStandard, flag);
			if (expression != null)
			{
				return expression;
			}
			if (flag)
			{
				return null;
			}
			return ResolveOperatorPredefined(rc, rc.Module.OperatorsBinaryLifted, false);
		}

		private static bool IsEnumOrNullableEnum(TypeSpec type)
		{
			if (!type.IsEnum)
			{
				if (type.IsNullableType)
				{
					return NullableInfo.GetUnderlyingType(type).IsEnum;
				}
				return false;
			}
			return true;
		}

		private Constant EnumLiftUp(ResolveContext ec, Constant left, Constant right)
		{
			switch (oper)
			{
			case Operator.Equality:
			case Operator.Inequality:
			case Operator.BitwiseAnd:
			case Operator.ExclusiveOr:
			case Operator.BitwiseOr:
			case Operator.LessThan:
			case Operator.GreaterThan:
			case Operator.LessThanOrEqual:
			case Operator.GreaterThanOrEqual:
				if (left.Type.IsEnum)
				{
					return left;
				}
				if (left.IsZeroInteger)
				{
					return left.Reduce(ec, right.Type);
				}
				break;
			case Operator.Addition:
			case Operator.Subtraction:
				return left;
			case Operator.Multiply:
			case Operator.Division:
			case Operator.Modulus:
			case Operator.LeftShift:
			case Operator.RightShift:
				if (!right.Type.IsEnum && !left.Type.IsEnum)
				{
					return left;
				}
				break;
			}
			return null;
		}

		private void CheckBitwiseOrOnSignExtended(ResolveContext ec)
		{
			OpcodeCast opcodeCast = left as OpcodeCast;
			if (opcodeCast != null && IsUnsigned(opcodeCast.UnderlyingType))
			{
				opcodeCast = null;
			}
			OpcodeCast opcodeCast2 = right as OpcodeCast;
			if (opcodeCast2 != null && IsUnsigned(opcodeCast2.UnderlyingType))
			{
				opcodeCast2 = null;
			}
			if (opcodeCast != null || opcodeCast2 != null)
			{
				TypeSpec typeSpec = ((opcodeCast != null) ? opcodeCast.UnderlyingType : opcodeCast2.UnderlyingType);
				ec.Report.Warning(675, 3, loc, "The operator `|' used on the sign-extended type `{0}'. Consider casting to a smaller unsigned type first", typeSpec.GetSignatureForError());
			}
		}

		public static PredefinedOperator[] CreatePointerOperatorsTable(BuiltinTypes types)
		{
			return new PredefinedOperator[9]
			{
				new PredefinedPointerOperator(null, types.Int, Operator.AdditionMask | Operator.SubtractionMask),
				new PredefinedPointerOperator(null, types.UInt, Operator.AdditionMask | Operator.SubtractionMask),
				new PredefinedPointerOperator(null, types.Long, Operator.AdditionMask | Operator.SubtractionMask),
				new PredefinedPointerOperator(null, types.ULong, Operator.AdditionMask | Operator.SubtractionMask),
				new PredefinedPointerOperator(types.Int, null, Operator.AdditionMask, null),
				new PredefinedPointerOperator(types.UInt, null, Operator.AdditionMask, null),
				new PredefinedPointerOperator(types.Long, null, Operator.AdditionMask, null),
				new PredefinedPointerOperator(types.ULong, null, Operator.AdditionMask, null),
				new PredefinedPointerOperator(null, Operator.SubtractionMask, types.Long)
			};
		}

		public static PredefinedOperator[] CreateStandardOperatorsTable(BuiltinTypes types)
		{
			TypeSpec return_type = types.Bool;
			return new PredefinedOperator[19]
			{
				new PredefinedOperator(types.Int, Operator.Multiply | Operator.ShiftMask | Operator.BitwiseMask),
				new PredefinedOperator(types.UInt, Operator.Multiply | Operator.BitwiseMask),
				new PredefinedOperator(types.Long, Operator.Multiply | Operator.BitwiseMask),
				new PredefinedOperator(types.ULong, Operator.Multiply | Operator.BitwiseMask),
				new PredefinedOperator(types.Float, Operator.Multiply),
				new PredefinedOperator(types.Double, Operator.Multiply),
				new PredefinedOperator(types.Decimal, Operator.Multiply),
				new PredefinedOperator(types.Int, Operator.ComparisonMask, return_type),
				new PredefinedOperator(types.UInt, Operator.ComparisonMask, return_type),
				new PredefinedOperator(types.Long, Operator.ComparisonMask, return_type),
				new PredefinedOperator(types.ULong, Operator.ComparisonMask, return_type),
				new PredefinedOperator(types.Float, Operator.ComparisonMask, return_type),
				new PredefinedOperator(types.Double, Operator.ComparisonMask, return_type),
				new PredefinedOperator(types.Decimal, Operator.ComparisonMask, return_type),
				new PredefinedStringOperator(types.String, Operator.AdditionMask, types.String),
				new PredefinedOperator(return_type, Operator.EqualityMask | Operator.BitwiseMask | Operator.LogicalMask, return_type),
				new PredefinedOperator(types.UInt, types.Int, Operator.ShiftMask),
				new PredefinedOperator(types.Long, types.Int, Operator.ShiftMask),
				new PredefinedOperator(types.ULong, types.Int, Operator.ShiftMask)
			};
		}

		public static PredefinedOperator[] CreateStandardLiftedOperatorsTable(ModuleContainer module)
		{
			BuiltinTypes builtinTypes = module.Compiler.BuiltinTypes;
			PredefinedStringOperator[] array = new PredefinedStringOperator[2]
			{
				new PredefinedStringOperator(builtinTypes.String, builtinTypes.Object, Operator.AdditionMask, builtinTypes.String),
				new PredefinedStringOperator(builtinTypes.Object, builtinTypes.String, Operator.AdditionMask, builtinTypes.String)
			};
			TypeSpec typeSpec = module.PredefinedTypes.Nullable.TypeSpec;
			if (typeSpec == null)
			{
				return array;
			}
			BuiltinTypeSpec builtinTypeSpec = builtinTypes.Bool;
			InflatedTypeSpec return_type = typeSpec.MakeGenericType(module, new BuiltinTypeSpec[1] { builtinTypeSpec });
			InflatedTypeSpec rtype = typeSpec.MakeGenericType(module, new BuiltinTypeSpec[1] { builtinTypes.Int });
			InflatedTypeSpec ltype = typeSpec.MakeGenericType(module, new BuiltinTypeSpec[1] { builtinTypes.UInt });
			InflatedTypeSpec ltype2 = typeSpec.MakeGenericType(module, new BuiltinTypeSpec[1] { builtinTypes.Long });
			InflatedTypeSpec ltype3 = typeSpec.MakeGenericType(module, new BuiltinTypeSpec[1] { builtinTypes.ULong });
			InflatedTypeSpec inflatedTypeSpec = typeSpec.MakeGenericType(module, new BuiltinTypeSpec[1] { builtinTypes.Float });
			InflatedTypeSpec inflatedTypeSpec2 = typeSpec.MakeGenericType(module, new BuiltinTypeSpec[1] { builtinTypes.Double });
			InflatedTypeSpec inflatedTypeSpec3 = typeSpec.MakeGenericType(module, new BuiltinTypeSpec[1] { builtinTypes.Decimal });
			return new PredefinedOperator[20]
			{
				new PredefinedOperator(rtype, Operator.Multiply | Operator.ShiftMask | Operator.BitwiseMask | Operator.NullableMask),
				new PredefinedOperator(ltype, Operator.Multiply | Operator.BitwiseMask | Operator.NullableMask),
				new PredefinedOperator(ltype2, Operator.Multiply | Operator.BitwiseMask | Operator.NullableMask),
				new PredefinedOperator(ltype3, Operator.Multiply | Operator.BitwiseMask | Operator.NullableMask),
				new PredefinedOperator(inflatedTypeSpec, Operator.Multiply | Operator.NullableMask),
				new PredefinedOperator(inflatedTypeSpec2, Operator.Multiply | Operator.NullableMask),
				new PredefinedOperator(inflatedTypeSpec3, Operator.Multiply | Operator.NullableMask),
				new PredefinedOperator(rtype, Operator.ComparisonMask | Operator.NullableMask, builtinTypeSpec),
				new PredefinedOperator(ltype, Operator.ComparisonMask | Operator.NullableMask, builtinTypeSpec),
				new PredefinedOperator(ltype2, Operator.ComparisonMask | Operator.NullableMask, builtinTypeSpec),
				new PredefinedOperator(ltype3, Operator.ComparisonMask | Operator.NullableMask, builtinTypeSpec),
				new PredefinedOperator(inflatedTypeSpec, Operator.ComparisonMask | Operator.NullableMask, builtinTypeSpec),
				new PredefinedOperator(inflatedTypeSpec2, Operator.ComparisonMask | Operator.NullableMask, builtinTypeSpec),
				new PredefinedOperator(inflatedTypeSpec3, Operator.ComparisonMask | Operator.NullableMask, builtinTypeSpec),
				new PredefinedOperator(return_type, Operator.BitwiseMask | Operator.NullableMask, return_type),
				new PredefinedOperator(ltype, rtype, Operator.ShiftMask | Operator.NullableMask),
				new PredefinedOperator(ltype2, rtype, Operator.ShiftMask | Operator.NullableMask),
				new PredefinedOperator(ltype3, rtype, Operator.ShiftMask | Operator.NullableMask),
				array[0],
				array[1]
			};
		}

		public static PredefinedOperator[] CreateEqualityOperatorsTable(BuiltinTypes types)
		{
			TypeSpec typeSpec = types.Bool;
			return new PredefinedOperator[10]
			{
				new PredefinedEqualityOperator(types.String, typeSpec),
				new PredefinedEqualityOperator(types.Delegate, typeSpec),
				new PredefinedOperator(typeSpec, Operator.EqualityMask, typeSpec),
				new PredefinedOperator(types.Int, Operator.EqualityMask, typeSpec),
				new PredefinedOperator(types.UInt, Operator.EqualityMask, typeSpec),
				new PredefinedOperator(types.Long, Operator.EqualityMask, typeSpec),
				new PredefinedOperator(types.ULong, Operator.EqualityMask, typeSpec),
				new PredefinedOperator(types.Float, Operator.EqualityMask, typeSpec),
				new PredefinedOperator(types.Double, Operator.EqualityMask, typeSpec),
				new PredefinedOperator(types.Decimal, Operator.EqualityMask, typeSpec)
			};
		}

		public static PredefinedOperator[] CreateEqualityLiftedOperatorsTable(ModuleContainer module)
		{
			TypeSpec typeSpec = module.PredefinedTypes.Nullable.TypeSpec;
			if (typeSpec == null)
			{
				return new PredefinedOperator[0];
			}
			BuiltinTypes builtinTypes = module.Compiler.BuiltinTypes;
			BuiltinTypeSpec builtinTypeSpec = builtinTypes.Bool;
			InflatedTypeSpec inflatedTypeSpec = typeSpec.MakeGenericType(module, new BuiltinTypeSpec[1] { builtinTypeSpec });
			InflatedTypeSpec inflatedTypeSpec2 = typeSpec.MakeGenericType(module, new BuiltinTypeSpec[1] { builtinTypes.Int });
			InflatedTypeSpec inflatedTypeSpec3 = typeSpec.MakeGenericType(module, new BuiltinTypeSpec[1] { builtinTypes.UInt });
			InflatedTypeSpec inflatedTypeSpec4 = typeSpec.MakeGenericType(module, new BuiltinTypeSpec[1] { builtinTypes.Long });
			InflatedTypeSpec inflatedTypeSpec5 = typeSpec.MakeGenericType(module, new BuiltinTypeSpec[1] { builtinTypes.ULong });
			InflatedTypeSpec inflatedTypeSpec6 = typeSpec.MakeGenericType(module, new BuiltinTypeSpec[1] { builtinTypes.Float });
			InflatedTypeSpec inflatedTypeSpec7 = typeSpec.MakeGenericType(module, new BuiltinTypeSpec[1] { builtinTypes.Double });
			InflatedTypeSpec inflatedTypeSpec8 = typeSpec.MakeGenericType(module, new BuiltinTypeSpec[1] { builtinTypes.Decimal });
			return new PredefinedOperator[8]
			{
				new PredefinedOperator(inflatedTypeSpec, Operator.EqualityMask | Operator.NullableMask, builtinTypeSpec),
				new PredefinedOperator(inflatedTypeSpec2, Operator.EqualityMask | Operator.NullableMask, builtinTypeSpec),
				new PredefinedOperator(inflatedTypeSpec3, Operator.EqualityMask | Operator.NullableMask, builtinTypeSpec),
				new PredefinedOperator(inflatedTypeSpec4, Operator.EqualityMask | Operator.NullableMask, builtinTypeSpec),
				new PredefinedOperator(inflatedTypeSpec5, Operator.EqualityMask | Operator.NullableMask, builtinTypeSpec),
				new PredefinedOperator(inflatedTypeSpec6, Operator.EqualityMask | Operator.NullableMask, builtinTypeSpec),
				new PredefinedOperator(inflatedTypeSpec7, Operator.EqualityMask | Operator.NullableMask, builtinTypeSpec),
				new PredefinedOperator(inflatedTypeSpec8, Operator.EqualityMask | Operator.NullableMask, builtinTypeSpec)
			};
		}

		private bool DoBinaryOperatorPromotion(ResolveContext rc)
		{
			TypeSpec underlyingType = left.Type;
			if (underlyingType.IsNullableType)
			{
				underlyingType = NullableInfo.GetUnderlyingType(underlyingType);
			}
			if (underlyingType.BuiltinType == BuiltinTypeSpec.Type.FirstPrimitive)
			{
				return true;
			}
			TypeSpec underlyingType2 = right.Type;
			if (underlyingType2.IsNullableType)
			{
				underlyingType2 = NullableInfo.GetUnderlyingType(underlyingType2);
			}
			BuiltinTypeSpec.Type builtinType = underlyingType.BuiltinType;
			BuiltinTypeSpec.Type builtinType2 = underlyingType2.BuiltinType;
			TypeSpec typeSpec;
			if (builtinType == BuiltinTypeSpec.Type.Decimal || builtinType2 == BuiltinTypeSpec.Type.Decimal)
			{
				typeSpec = rc.BuiltinTypes.Decimal;
			}
			else if (builtinType == BuiltinTypeSpec.Type.Double || builtinType2 == BuiltinTypeSpec.Type.Double)
			{
				typeSpec = rc.BuiltinTypes.Double;
			}
			else if (builtinType == BuiltinTypeSpec.Type.Float || builtinType2 == BuiltinTypeSpec.Type.Float)
			{
				typeSpec = rc.BuiltinTypes.Float;
			}
			else if (builtinType == BuiltinTypeSpec.Type.ULong || builtinType2 == BuiltinTypeSpec.Type.ULong)
			{
				typeSpec = rc.BuiltinTypes.ULong;
				if (IsSignedType(builtinType))
				{
					Expression expression = ConvertSignedConstant(left, typeSpec);
					if (expression == null)
					{
						return false;
					}
					left = expression;
				}
				else if (IsSignedType(builtinType2))
				{
					Expression expression = ConvertSignedConstant(right, typeSpec);
					if (expression == null)
					{
						return false;
					}
					right = expression;
				}
			}
			else if (builtinType == BuiltinTypeSpec.Type.Long || builtinType2 == BuiltinTypeSpec.Type.Long)
			{
				typeSpec = rc.BuiltinTypes.Long;
			}
			else if (builtinType == BuiltinTypeSpec.Type.UInt || builtinType2 == BuiltinTypeSpec.Type.UInt)
			{
				typeSpec = rc.BuiltinTypes.UInt;
				if (IsSignedType(builtinType))
				{
					Expression expression = ConvertSignedConstant(left, typeSpec);
					if (expression == null)
					{
						typeSpec = rc.BuiltinTypes.Long;
					}
				}
				else if (IsSignedType(builtinType2))
				{
					Expression expression = ConvertSignedConstant(right, typeSpec);
					if (expression == null)
					{
						typeSpec = rc.BuiltinTypes.Long;
					}
				}
			}
			else
			{
				typeSpec = rc.BuiltinTypes.Int;
			}
			if (underlyingType != typeSpec)
			{
				Expression expression = PromoteExpression(rc, left, typeSpec);
				if (expression == null)
				{
					return false;
				}
				left = expression;
			}
			if (underlyingType2 != typeSpec)
			{
				Expression expression = PromoteExpression(rc, right, typeSpec);
				if (expression == null)
				{
					return false;
				}
				right = expression;
			}
			return true;
		}

		private static bool IsSignedType(BuiltinTypeSpec.Type type)
		{
			switch (type)
			{
			case BuiltinTypeSpec.Type.SByte:
			case BuiltinTypeSpec.Type.Short:
			case BuiltinTypeSpec.Type.Int:
			case BuiltinTypeSpec.Type.Long:
				return true;
			default:
				return false;
			}
		}

		private static Expression ConvertSignedConstant(Expression expr, TypeSpec type)
		{
			return (expr as Constant)?.ConvertImplicitly(type);
		}

		private static Expression PromoteExpression(ResolveContext rc, Expression expr, TypeSpec type)
		{
			if (expr.Type.IsNullableType)
			{
				return Convert.ImplicitConversionStandard(rc, expr, rc.Module.PredefinedTypes.Nullable.TypeSpec.MakeGenericType(rc, new TypeSpec[1] { type }), expr.Location);
			}
			Constant constant = expr as Constant;
			if (constant != null)
			{
				return constant.ConvertImplicitly(type);
			}
			return Convert.ImplicitNumericConversion(expr, type);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			if (left == null)
			{
				return null;
			}
			if (oper == Operator.Subtraction && left is ParenthesizedExpression)
			{
				left = ((ParenthesizedExpression)left).Expr;
				left = left.Resolve(ec, ResolveFlags.VariableOrValue | ResolveFlags.Type);
				if (left == null)
				{
					return null;
				}
				if (left.eclass == ExprClass.Type)
				{
					ec.Report.Error(75, loc, "To cast a negative value, you must enclose the value in parentheses");
					return null;
				}
			}
			else
			{
				left = left.Resolve(ec);
			}
			if (left == null)
			{
				return null;
			}
			right = right.Resolve(ec);
			if (right == null)
			{
				return null;
			}
			Constant constant = left as Constant;
			Constant constant2 = right as Constant;
			if (!ec.HasSet(ResolveContext.Options.EnumScope) && constant != null && constant2 != null && (left.Type.IsEnum || right.Type.IsEnum))
			{
				constant = EnumLiftUp(ec, constant, constant2);
				if (constant != null)
				{
					constant2 = EnumLiftUp(ec, constant2, constant);
				}
			}
			if (constant2 != null && constant != null)
			{
				int errors = ec.Report.Errors;
				Expression expression = ConstantFold.BinaryFold(ec, oper, constant, constant2, loc);
				if (expression != null || ec.Report.Errors != errors)
				{
					return expression;
				}
			}
			if ((oper & Operator.ComparisonMask) != 0)
			{
				if (left.Equals(right))
				{
					ec.Report.Warning(1718, 3, loc, "A comparison made to same variable. Did you mean to compare something else?");
				}
				CheckOutOfRangeComparison(ec, constant, right.Type);
				CheckOutOfRangeComparison(ec, constant2, left.Type);
			}
			if (left.Type.BuiltinType == BuiltinTypeSpec.Type.Dynamic || right.Type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				return DoResolveDynamic(ec);
			}
			return DoResolveCore(ec, left, right);
		}

		private Expression DoResolveDynamic(ResolveContext rc)
		{
			TypeSpec typeSpec = left.Type;
			TypeSpec typeSpec2 = right.Type;
			if (typeSpec.Kind == MemberKind.Void || typeSpec == InternalType.MethodGroup || typeSpec == InternalType.AnonymousMethod || typeSpec2.Kind == MemberKind.Void || typeSpec2 == InternalType.MethodGroup || typeSpec2 == InternalType.AnonymousMethod)
			{
				Error_OperatorCannotBeApplied(rc, left, right);
				return null;
			}
			Arguments arguments;
			if ((oper & Operator.LogicalMask) != 0)
			{
				arguments = new Arguments(2);
				Expression expr;
				Expression true_expr;
				Expression false_expr;
				if (typeSpec.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
				{
					LocalVariable localVariable = LocalVariable.CreateCompilerGenerated(typeSpec, rc.CurrentBlock, loc);
					Arguments arguments2 = new Arguments(1);
					arguments2.Add(new Argument(new SimpleAssign(localVariable.CreateReferenceExpression(rc, loc), left).Resolve(rc)));
					left = localVariable.CreateReferenceExpression(rc, loc);
					if (oper == Operator.LogicalAnd)
					{
						expr = DynamicUnaryConversion.CreateIsFalse(rc, arguments2, loc);
						true_expr = left;
					}
					else
					{
						expr = DynamicUnaryConversion.CreateIsTrue(rc, arguments2, loc);
						true_expr = left;
					}
					arguments.Add(new Argument(left));
					arguments.Add(new Argument(right));
					false_expr = new DynamicExpressionStatement(this, arguments, loc);
				}
				else
				{
					LocalVariable localVariable2 = LocalVariable.CreateCompilerGenerated(rc.BuiltinTypes.Bool, rc.CurrentBlock, loc);
					if (!Convert.ImplicitConversionExists(rc, left, localVariable2.Type) && ((oper == Operator.LogicalAnd) ? Expression.GetOperatorFalse(rc, left, loc) : Expression.GetOperatorTrue(rc, left, loc)) == null)
					{
						rc.Report.Error(7083, left.Location, "Expression must be implicitly convertible to Boolean or its type `{0}' must define operator `{1}'", typeSpec.GetSignatureForError(), (oper == Operator.LogicalAnd) ? "false" : "true");
						return null;
					}
					arguments.Add(new Argument(localVariable2.CreateReferenceExpression(rc, loc).Resolve(rc)));
					arguments.Add(new Argument(right));
					right = new DynamicExpressionStatement(this, arguments, loc);
					if (oper == Operator.LogicalAnd)
					{
						true_expr = right;
						false_expr = localVariable2.CreateReferenceExpression(rc, loc);
					}
					else
					{
						true_expr = localVariable2.CreateReferenceExpression(rc, loc);
						false_expr = right;
					}
					expr = new BooleanExpression(new SimpleAssign(localVariable2.CreateReferenceExpression(rc, loc), left));
				}
				return new Conditional(expr, true_expr, false_expr, loc).Resolve(rc);
			}
			arguments = new Arguments(2);
			arguments.Add(new Argument(left));
			arguments.Add(new Argument(right));
			return new DynamicExpressionStatement(this, arguments, loc).Resolve(rc);
		}

		private Expression DoResolveCore(ResolveContext ec, Expression left_orig, Expression right_orig)
		{
			Expression expression = ResolveOperator(ec);
			if (expression == null)
			{
				Error_OperatorCannotBeApplied(ec, left_orig, right_orig);
			}
			if (left == null || right == null)
			{
				throw new InternalErrorException("Invalid conversion");
			}
			if (oper == Operator.BitwiseOr)
			{
				CheckBitwiseOrOnSignExtended(ec);
			}
			return expression;
		}

		public override System.Linq.Expressions.Expression MakeExpression(BuilderContext ctx)
		{
			return MakeExpression(ctx, left, right);
		}

		public System.Linq.Expressions.Expression MakeExpression(BuilderContext ctx, Expression left, Expression right)
		{
			System.Linq.Expressions.Expression expression = left.MakeExpression(ctx);
			System.Linq.Expressions.Expression expression2 = right.MakeExpression(ctx);
			bool flag = ctx.HasSet(BuilderContext.Options.CheckedScope);
			switch (oper)
			{
			case Operator.Addition:
				if (!flag)
				{
					return System.Linq.Expressions.Expression.Add(expression, expression2);
				}
				return System.Linq.Expressions.Expression.AddChecked(expression, expression2);
			case Operator.BitwiseAnd:
				return System.Linq.Expressions.Expression.And(expression, expression2);
			case Operator.BitwiseOr:
				return System.Linq.Expressions.Expression.Or(expression, expression2);
			case Operator.Division:
				return System.Linq.Expressions.Expression.Divide(expression, expression2);
			case Operator.Equality:
				return System.Linq.Expressions.Expression.Equal(expression, expression2);
			case Operator.ExclusiveOr:
				return System.Linq.Expressions.Expression.ExclusiveOr(expression, expression2);
			case Operator.GreaterThan:
				return System.Linq.Expressions.Expression.GreaterThan(expression, expression2);
			case Operator.GreaterThanOrEqual:
				return System.Linq.Expressions.Expression.GreaterThanOrEqual(expression, expression2);
			case Operator.Inequality:
				return System.Linq.Expressions.Expression.NotEqual(expression, expression2);
			case Operator.LeftShift:
				return System.Linq.Expressions.Expression.LeftShift(expression, expression2);
			case Operator.LessThan:
				return System.Linq.Expressions.Expression.LessThan(expression, expression2);
			case Operator.LessThanOrEqual:
				return System.Linq.Expressions.Expression.LessThanOrEqual(expression, expression2);
			case Operator.LogicalAnd:
				return System.Linq.Expressions.Expression.AndAlso(expression, expression2);
			case Operator.LogicalOr:
				return System.Linq.Expressions.Expression.OrElse(expression, expression2);
			case Operator.Modulus:
				return System.Linq.Expressions.Expression.Modulo(expression, expression2);
			case Operator.Multiply:
				if (!flag)
				{
					return System.Linq.Expressions.Expression.Multiply(expression, expression2);
				}
				return System.Linq.Expressions.Expression.MultiplyChecked(expression, expression2);
			case Operator.RightShift:
				return System.Linq.Expressions.Expression.RightShift(expression, expression2);
			case Operator.Subtraction:
				if (!flag)
				{
					return System.Linq.Expressions.Expression.Subtract(expression, expression2);
				}
				return System.Linq.Expressions.Expression.SubtractChecked(expression, expression2);
			default:
				throw new NotImplementedException(oper.ToString());
			}
		}

		private Expression ResolveOperatorDelegate(ResolveContext ec, TypeSpec l, TypeSpec r)
		{
			if (l != r && !TypeSpecComparer.Variant.IsEqual(r, l))
			{
				if (right.eclass == ExprClass.MethodGroup || r == InternalType.AnonymousMethod || r == InternalType.NullLiteral)
				{
					Expression expression = Convert.ImplicitConversionRequired(ec, right, l, loc);
					if (expression == null)
					{
						return null;
					}
					right = expression;
					r = right.Type;
				}
				else
				{
					if (left.eclass != ExprClass.MethodGroup && l != InternalType.AnonymousMethod && l != InternalType.NullLiteral)
					{
						return null;
					}
					Expression expression = Convert.ImplicitConversionRequired(ec, left, r, loc);
					if (expression == null)
					{
						return null;
					}
					left = expression;
					l = left.Type;
				}
			}
			MethodSpec methodSpec = null;
			Arguments arguments = new Arguments(2);
			arguments.Add(new Argument(left));
			arguments.Add(new Argument(right));
			if (oper == Operator.Addition)
			{
				methodSpec = ec.Module.PredefinedMembers.DelegateCombine.Resolve(loc);
			}
			else if (oper == Operator.Subtraction)
			{
				methodSpec = ec.Module.PredefinedMembers.DelegateRemove.Resolve(loc);
			}
			if (methodSpec == null)
			{
				return new EmptyExpression(ec.BuiltinTypes.Decimal);
			}
			return new ClassCast(new UserOperatorCall(methodSpec, arguments, CreateExpressionTree, loc), l);
		}

		private Expression ResolveSingleEnumOperators(ResolveContext rc, bool lenum, bool renum, TypeSpec ltype, TypeSpec rtype)
		{
			if ((oper & Operator.ComparisonMask) != 0)
			{
				type = rc.BuiltinTypes.Bool;
			}
			else if (lenum)
			{
				type = ltype;
			}
			else if (renum)
			{
				type = rtype;
			}
			else if (ltype.IsNullableType && NullableInfo.GetUnderlyingType(ltype).IsEnum)
			{
				type = ltype;
			}
			else
			{
				type = rtype;
			}
			if (ltype == rtype)
			{
				if (lenum | renum)
				{
					return this;
				}
				return new LiftedBinaryOperator(this)
				{
					Left = left,
					Right = right
				}.Resolve(rc);
			}
			if (renum && !ltype.IsNullableType)
			{
				Expression expression = Convert.ImplicitConversion(rc, left, rtype, loc);
				if (expression != null)
				{
					left = expression;
					return this;
				}
			}
			else if (lenum && !rtype.IsNullableType)
			{
				Expression expression = Convert.ImplicitConversion(rc, right, ltype, loc);
				if (expression != null)
				{
					right = expression;
					return this;
				}
			}
			TypeSpec typeSpec = rc.Module.PredefinedTypes.Nullable.TypeSpec;
			if (typeSpec != null)
			{
				if (renum && !ltype.IsNullableType)
				{
					InflatedTypeSpec target_type = typeSpec.MakeGenericType(rc.Module, new TypeSpec[1] { rtype });
					Expression expression = Convert.ImplicitConversion(rc, left, target_type, loc);
					if (expression != null)
					{
						left = expression;
						right = Convert.ImplicitConversion(rc, right, target_type, loc);
					}
					if ((oper & Operator.BitwiseMask) != 0)
					{
						type = target_type;
					}
					if (left.IsNull)
					{
						if ((oper & Operator.BitwiseMask) != 0)
						{
							return LiftedNull.CreateFromExpression(rc, this);
						}
						return CreateLiftedValueTypeResult(rc, rtype);
					}
					if (expression != null)
					{
						return new LiftedBinaryOperator(this)
						{
							Left = expression,
							Right = right
						}.Resolve(rc);
					}
				}
				else if (lenum && !rtype.IsNullableType)
				{
					InflatedTypeSpec target_type2 = typeSpec.MakeGenericType(rc.Module, new TypeSpec[1] { ltype });
					Expression expression = Convert.ImplicitConversion(rc, right, target_type2, loc);
					if (expression != null)
					{
						right = expression;
						left = Convert.ImplicitConversion(rc, left, target_type2, loc);
					}
					if ((oper & Operator.BitwiseMask) != 0)
					{
						type = target_type2;
					}
					if (right.IsNull)
					{
						if ((oper & Operator.BitwiseMask) != 0)
						{
							return LiftedNull.CreateFromExpression(rc, this);
						}
						return CreateLiftedValueTypeResult(rc, ltype);
					}
					if (expression != null)
					{
						return new LiftedBinaryOperator(this)
						{
							Left = left,
							Right = expression
						}.Resolve(rc);
					}
				}
				else if (rtype.IsNullableType && NullableInfo.GetUnderlyingType(rtype).IsEnum)
				{
					Unwrap unwrapRight = null;
					Expression expression;
					if (left.IsNull || right.IsNull)
					{
						if (rc.HasSet(ResolveContext.Options.ExpressionTreeConversion))
						{
							left = Convert.ImplicitConversion(rc, left, rtype, left.Location);
						}
						if ((oper & Operator.RelationalMask) != 0)
						{
							return CreateLiftedValueTypeResult(rc, rtype);
						}
						if ((oper & Operator.BitwiseMask) != 0)
						{
							return LiftedNull.CreateFromExpression(rc, this);
						}
						if (right.IsNull)
						{
							return CreateLiftedValueTypeResult(rc, left.Type);
						}
						expression = left;
						unwrapRight = new Unwrap(right);
					}
					else
					{
						expression = Convert.ImplicitConversion(rc, left, NullableInfo.GetUnderlyingType(rtype), loc);
						if (expression == null)
						{
							return null;
						}
					}
					if (expression != null)
					{
						return new LiftedBinaryOperator(this)
						{
							Left = expression,
							Right = right,
							UnwrapRight = unwrapRight
						}.Resolve(rc);
					}
				}
				else if (ltype.IsNullableType && NullableInfo.GetUnderlyingType(ltype).IsEnum)
				{
					Unwrap unwrapLeft = null;
					Expression expression;
					if (right.IsNull || left.IsNull)
					{
						if (rc.HasSet(ResolveContext.Options.ExpressionTreeConversion))
						{
							right = Convert.ImplicitConversion(rc, right, ltype, right.Location);
						}
						if ((oper & Operator.RelationalMask) != 0)
						{
							return CreateLiftedValueTypeResult(rc, ltype);
						}
						if ((oper & Operator.BitwiseMask) != 0)
						{
							return LiftedNull.CreateFromExpression(rc, this);
						}
						if (left.IsNull)
						{
							return CreateLiftedValueTypeResult(rc, right.Type);
						}
						expression = right;
						unwrapLeft = new Unwrap(left);
					}
					else
					{
						expression = Convert.ImplicitConversion(rc, right, NullableInfo.GetUnderlyingType(ltype), loc);
						if (expression == null)
						{
							return null;
						}
					}
					if (expression != null)
					{
						return new LiftedBinaryOperator(this)
						{
							Left = left,
							UnwrapLeft = unwrapLeft,
							Right = expression
						}.Resolve(rc);
					}
				}
			}
			return null;
		}

		private static Expression ConvertEnumOperandToUnderlyingType(ResolveContext rc, Expression expr, bool liftType)
		{
			TypeSpec typeSpec;
			if (!expr.Type.IsNullableType)
			{
				typeSpec = ((!expr.Type.IsEnum) ? expr.Type : EnumSpec.GetUnderlyingType(expr.Type));
			}
			else
			{
				TypeSpec underlyingType = NullableInfo.GetUnderlyingType(expr.Type);
				typeSpec = ((!underlyingType.IsEnum) ? underlyingType : EnumSpec.GetUnderlyingType(underlyingType));
			}
			switch (typeSpec.BuiltinType)
			{
			case BuiltinTypeSpec.Type.Byte:
			case BuiltinTypeSpec.Type.SByte:
			case BuiltinTypeSpec.Type.Short:
			case BuiltinTypeSpec.Type.UShort:
				typeSpec = rc.BuiltinTypes.Int;
				break;
			}
			if (expr.Type.IsNullableType | liftType)
			{
				typeSpec = rc.Module.PredefinedTypes.Nullable.TypeSpec.MakeGenericType(rc.Module, new TypeSpec[1] { typeSpec });
			}
			if (expr.Type == typeSpec)
			{
				return expr;
			}
			return EmptyCast.Create(expr, typeSpec);
		}

		private Expression ResolveEnumOperators(ResolveContext rc, bool lenum, bool renum, TypeSpec ltype, TypeSpec rtype)
		{
			TypeSpec typeSpec = (lenum ? ltype : (renum ? rtype : ((!ltype.IsNullableType || !NullableInfo.GetUnderlyingType(ltype).IsEnum) ? rtype : ltype)));
			Expression expression;
			if (!typeSpec.IsNullableType)
			{
				expression = ResolveOperatorPredefined(rc, rc.Module.GetPredefinedEnumAritmeticOperators(typeSpec, false), false);
				if (expression != null)
				{
					expression = ((oper != Operator.Subtraction) ? ConvertEnumAdditionalResult(expression, typeSpec) : ConvertEnumSubtractionResult(rc, expression));
					enum_conversion = GetEnumResultCast(expression.Type);
					return expression;
				}
				PredefinedType nullable = rc.Module.PredefinedTypes.Nullable;
				if (!nullable.IsDefined)
				{
					return null;
				}
				typeSpec = nullable.TypeSpec.MakeGenericType(rc.Module, new TypeSpec[1] { typeSpec });
			}
			expression = ResolveOperatorPredefined(rc, rc.Module.GetPredefinedEnumAritmeticOperators(typeSpec, true), false);
			if (expression != null)
			{
				expression = ((oper != Operator.Subtraction) ? ConvertEnumAdditionalResult(expression, typeSpec) : ConvertEnumSubtractionResult(rc, expression));
				enum_conversion = GetEnumResultCast(expression.Type);
			}
			return expression;
		}

		private static Expression ConvertEnumAdditionalResult(Expression expr, TypeSpec enumType)
		{
			return EmptyCast.Create(expr, enumType);
		}

		private Expression ConvertEnumSubtractionResult(ResolveContext rc, Expression expr)
		{
			TypeSpec typeSpec;
			if (left.Type == right.Type)
			{
				EnumConstant enumConstant = right as EnumConstant;
				typeSpec = ((enumConstant == null || !enumConstant.IsZeroInteger || right.Type.IsEnum) ? (left.Type.IsNullableType ? NullableInfo.GetEnumUnderlyingType(rc.Module, left.Type) : EnumSpec.GetUnderlyingType(left.Type)) : left.Type);
			}
			else
			{
				typeSpec = ((!IsEnumOrNullableEnum(left.Type)) ? right.Type : left.Type);
				if (expr is LiftedBinaryOperator && !typeSpec.IsNullableType)
				{
					typeSpec = rc.Module.PredefinedTypes.Nullable.TypeSpec.MakeGenericType(rc.Module, new TypeSpec[1] { typeSpec });
				}
			}
			return EmptyCast.Create(expr, typeSpec);
		}

		public static ConvCast.Mode GetEnumResultCast(TypeSpec type)
		{
			if (type.IsNullableType)
			{
				type = NullableInfo.GetUnderlyingType(type);
			}
			if (type.IsEnum)
			{
				type = EnumSpec.GetUnderlyingType(type);
			}
			switch (type.BuiltinType)
			{
			case BuiltinTypeSpec.Type.SByte:
				return ConvCast.Mode.I4_I1;
			case BuiltinTypeSpec.Type.Byte:
				return ConvCast.Mode.I4_U1;
			case BuiltinTypeSpec.Type.Short:
				return ConvCast.Mode.I4_I2;
			case BuiltinTypeSpec.Type.UShort:
				return ConvCast.Mode.I4_U2;
			default:
				return ConvCast.Mode.I1_U1;
			}
		}

		private Expression ResolveEquality(ResolveContext ec, TypeSpec l, TypeSpec r, bool primitives_only)
		{
			type = ec.BuiltinTypes.Bool;
			bool flag = false;
			if (!primitives_only)
			{
				TypeParameterSpec typeParameterSpec = l as TypeParameterSpec;
				TypeParameterSpec typeParameterSpec2 = r as TypeParameterSpec;
				if (typeParameterSpec != null)
				{
					if (right is NullLiteral)
					{
						if (typeParameterSpec.GetEffectiveBase().BuiltinType == BuiltinTypeSpec.Type.ValueType)
						{
							return null;
						}
						left = new BoxedCast(left, ec.BuiltinTypes.Object);
						return this;
					}
					if (!typeParameterSpec.IsReferenceType)
					{
						return null;
					}
					l = typeParameterSpec.GetEffectiveBase();
					left = new BoxedCast(left, l);
				}
				else if (left is NullLiteral && typeParameterSpec2 == null)
				{
					if (TypeSpec.IsReferenceType(r))
					{
						return this;
					}
					if (r.Kind == MemberKind.InternalCompilerType)
					{
						return null;
					}
				}
				if (typeParameterSpec2 != null)
				{
					if (left is NullLiteral)
					{
						if (typeParameterSpec2.GetEffectiveBase().BuiltinType == BuiltinTypeSpec.Type.ValueType)
						{
							return null;
						}
						right = new BoxedCast(right, ec.BuiltinTypes.Object);
						return this;
					}
					if (!typeParameterSpec2.IsReferenceType)
					{
						return null;
					}
					r = typeParameterSpec2.GetEffectiveBase();
					right = new BoxedCast(right, r);
				}
				else if (right is NullLiteral)
				{
					if (TypeSpec.IsReferenceType(l))
					{
						return this;
					}
					if (l.Kind == MemberKind.InternalCompilerType)
					{
						return null;
					}
				}
				if (l.IsDelegate)
				{
					if (right.eclass == ExprClass.MethodGroup)
					{
						Expression expression = Convert.ImplicitConversion(ec, right, l, loc);
						if (expression == null)
						{
							return null;
						}
						right = expression;
						r = l;
					}
					else if (r.IsDelegate && l != r)
					{
						return null;
					}
				}
				else if (left.eclass == ExprClass.MethodGroup && r.IsDelegate)
				{
					Expression expression = Convert.ImplicitConversionRequired(ec, left, r, loc);
					if (expression == null)
					{
						return null;
					}
					left = expression;
					l = r;
				}
				else
				{
					flag = l == r && !l.IsStruct;
				}
			}
			if (r.BuiltinType != BuiltinTypeSpec.Type.Object && l.BuiltinType != BuiltinTypeSpec.Type.Object)
			{
				Expression expression = ResolveOperatorPredefined(ec, ec.BuiltinTypes.OperatorsBinaryEquality, flag);
				if (expression != null)
				{
					return expression;
				}
				if (!flag || l.IsNullableType)
				{
					expression = ResolveOperatorPredefined(ec, ec.Module.OperatorsBinaryEqualityLifted, flag);
					if (expression != null)
					{
						return expression;
					}
				}
				if ((l.IsNullableType && right.IsNull) || (r.IsNullableType && left.IsNull))
				{
					return new LiftedBinaryOperator(this)
					{
						Left = left,
						Right = right
					}.Resolve(ec);
				}
			}
			if (l == r)
			{
				if (l.Kind != MemberKind.InternalCompilerType && l.Kind != MemberKind.Struct)
				{
					return this;
				}
				return null;
			}
			if (!Convert.ExplicitReferenceConversionExists(l, r) && !Convert.ExplicitReferenceConversionExists(r, l))
			{
				return null;
			}
			if (!TypeSpec.IsReferenceType(l) || !TypeSpec.IsReferenceType(r))
			{
				return null;
			}
			if (l.BuiltinType == BuiltinTypeSpec.Type.String || l.BuiltinType == BuiltinTypeSpec.Type.Delegate || l.IsDelegate || MemberCache.GetUserOperator(l, Mono.CSharp.Operator.OpType.Equality, false) != null)
			{
				ec.Report.Warning(253, 2, loc, "Possible unintended reference comparison. Consider casting the right side expression to type `{0}' to get value comparison", l.GetSignatureForError());
			}
			if (r.BuiltinType == BuiltinTypeSpec.Type.String || r.BuiltinType == BuiltinTypeSpec.Type.Delegate || r.IsDelegate || MemberCache.GetUserOperator(r, Mono.CSharp.Operator.OpType.Equality, false) != null)
			{
				ec.Report.Warning(252, 2, loc, "Possible unintended reference comparison. Consider casting the left side expression to type `{0}' to get value comparison", r.GetSignatureForError());
			}
			return this;
		}

		private Expression ResolveOperatorPointer(ResolveContext ec, TypeSpec l, TypeSpec r)
		{
			if ((oper & Operator.ComparisonMask) != 0)
			{
				if (!l.IsPointer)
				{
					Expression expression = Convert.ImplicitConversion(ec, left, r, left.Location);
					if (expression == null)
					{
						return null;
					}
					left = expression;
				}
				if (!r.IsPointer)
				{
					Expression expression = Convert.ImplicitConversion(ec, right, l, right.Location);
					if (expression == null)
					{
						return null;
					}
					right = expression;
				}
				type = ec.BuiltinTypes.Bool;
				return this;
			}
			return ResolveOperatorPredefined(ec, ec.BuiltinTypes.OperatorsBinaryUnsafe, false);
		}

		private Expression ResolveOperatorPredefined(ResolveContext ec, PredefinedOperator[] operators, bool primitives_only)
		{
			PredefinedOperator predefinedOperator = null;
			TypeSpec typeSpec = left.Type;
			TypeSpec typeSpec2 = right.Type;
			Operator obj = oper & ~Operator.ValuesOnlyMask;
			foreach (PredefinedOperator predefinedOperator2 in operators)
			{
				if ((predefinedOperator2.OperatorsMask & obj) == 0)
				{
					continue;
				}
				if (primitives_only)
				{
					if (!predefinedOperator2.IsPrimitiveApplicable(typeSpec, typeSpec2))
					{
						continue;
					}
				}
				else if (!predefinedOperator2.IsApplicable(ec, left, right))
				{
					continue;
				}
				if (predefinedOperator == null)
				{
					predefinedOperator = predefinedOperator2;
					if (primitives_only)
					{
						break;
					}
					continue;
				}
				predefinedOperator = predefinedOperator2.ResolveBetterOperator(ec, predefinedOperator);
				if (predefinedOperator == null)
				{
					ec.Report.Error(34, loc, "Operator `{0}' is ambiguous on operands of type `{1}' and `{2}'", OperName(oper), typeSpec.GetSignatureForError(), typeSpec2.GetSignatureForError());
					predefinedOperator = predefinedOperator2;
					break;
				}
			}
			return predefinedOperator?.ConvertResult(ec, this);
		}

		private Expression OptimizeAndOperation(Expression expr)
		{
			Constant constant = right as Constant;
			Constant constant2 = left as Constant;
			if ((constant2 != null && constant2.IsDefaultValue) || (constant != null && constant.IsDefaultValue))
			{
				return ReducedExpression.Create((constant == null) ? new SideEffectConstant(constant2, right, loc) : new SideEffectConstant(constant, left, loc), expr);
			}
			return expr;
		}

		public Expression CreateLiftedValueTypeResult(ResolveContext rc, TypeSpec valueType)
		{
			if (rc.HasSet(ResolveContext.Options.ExpressionTreeConversion))
			{
				type = rc.BuiltinTypes.Bool;
				return this;
			}
			Constant constant = new BoolConstant(rc.BuiltinTypes, Oper == Operator.Inequality, loc);
			if ((Oper & Operator.EqualityMask) != 0)
			{
				rc.Report.Warning(472, 2, loc, "The result of comparing value type `{0}' with null is always `{1}'", valueType.GetSignatureForError(), constant.GetValueAsLiteral());
			}
			else
			{
				rc.Report.Warning(464, 2, loc, "The result of comparing type `{0}' with null is always `{1}'", valueType.GetSignatureForError(), constant.GetValueAsLiteral());
			}
			return constant;
		}

		private Expression ResolveUserOperator(ResolveContext rc, Expression left, Expression right)
		{
			Mono.CSharp.Operator.OpType op = ConvertBinaryToUserOperator(oper);
			TypeSpec underlyingType = left.Type;
			if (underlyingType.IsNullableType)
			{
				underlyingType = NullableInfo.GetUnderlyingType(underlyingType);
			}
			TypeSpec underlyingType2 = right.Type;
			if (underlyingType2.IsNullableType)
			{
				underlyingType2 = NullableInfo.GetUnderlyingType(underlyingType2);
			}
			IList<MemberSpec> list = MemberCache.GetUserOperator(underlyingType, op, false);
			IList<MemberSpec> list2 = null;
			if (underlyingType != underlyingType2)
			{
				list2 = MemberCache.GetUserOperator(underlyingType2, op, false);
				if (list2 == null && list == null)
				{
					return null;
				}
			}
			else if (list == null)
			{
				return null;
			}
			Arguments args = new Arguments(2);
			Argument argument = new Argument(left);
			args.Add(argument);
			Argument argument2 = new Argument(right);
			args.Add(argument2);
			if (list != null && list2 != null)
			{
				list = CombineUserOperators(list, list2);
			}
			else if (list2 != null)
			{
				list = list2;
			}
			MethodSpec methodSpec = new OverloadResolver(list, OverloadResolver.Restrictions.ProbingOnly | OverloadResolver.Restrictions.NoBaseMembers | OverloadResolver.Restrictions.BaseMembersIncluded, loc).ResolveOperator(rc, ref args);
			if (methodSpec == null)
			{
				if ((oper & Operator.LogicalMask) != 0)
				{
					return null;
				}
				if (!IsLiftedOperatorApplicable())
				{
					return null;
				}
				List<MemberSpec> list3 = CreateLiftedOperators(rc, list);
				if (list3 == null)
				{
					return null;
				}
				methodSpec = new OverloadResolver(list3, OverloadResolver.Restrictions.ProbingOnly | OverloadResolver.Restrictions.NoBaseMembers | OverloadResolver.Restrictions.BaseMembersIncluded, loc).ResolveOperator(rc, ref args);
				if (methodSpec == null)
				{
					return null;
				}
				MethodSpec methodSpec2 = null;
				foreach (MethodSpec item in list)
				{
					if (item.MemberDefinition == methodSpec.MemberDefinition)
					{
						methodSpec2 = item;
						break;
					}
				}
				if (rc.HasSet(ResolveContext.Options.ExpressionTreeConversion))
				{
					this.left = Convert.ImplicitConversion(rc, left, methodSpec.Parameters.Types[0], left.Location);
					this.right = Convert.ImplicitConversion(rc, right, methodSpec.Parameters.Types[1], left.Location);
				}
				TypeSpec[] types = methodSpec2.Parameters.Types;
				if (left.IsNull || right.IsNull)
				{
					if ((oper & (Operator.Multiply | Operator.ShiftMask | Operator.BitwiseMask)) != 0)
					{
						type = methodSpec.ReturnType;
						return LiftedNull.CreateFromExpression(rc, this);
					}
					if ((oper & Operator.RelationalMask) != 0)
					{
						return CreateLiftedValueTypeResult(rc, left.IsNull ? types[1] : types[0]);
					}
					if ((oper & Operator.EqualityMask) != 0 && ((left.IsNull && !right.Type.IsNullableType) || !left.Type.IsNullableType))
					{
						return CreateLiftedValueTypeResult(rc, left.IsNull ? types[1] : types[0]);
					}
				}
				type = methodSpec.ReturnType;
				LiftedBinaryOperator liftedBinaryOperator = new LiftedBinaryOperator(this);
				liftedBinaryOperator.UserOperator = methodSpec2;
				if (left.Type.IsNullableType && !types[0].IsNullableType)
				{
					liftedBinaryOperator.UnwrapLeft = new Unwrap(left);
				}
				if (right.Type.IsNullableType && !types[1].IsNullableType)
				{
					liftedBinaryOperator.UnwrapRight = new Unwrap(right);
				}
				liftedBinaryOperator.Left = Convert.ImplicitConversion(rc, liftedBinaryOperator.UnwrapLeft ?? left, types[0], left.Location);
				liftedBinaryOperator.Right = Convert.ImplicitConversion(rc, liftedBinaryOperator.UnwrapRight ?? right, types[1], right.Location);
				return liftedBinaryOperator.Resolve(rc);
			}
			Expression result = (((oper & Operator.LogicalMask) == 0) ? new UserOperatorCall(methodSpec, args, CreateExpressionTree, loc) : new ConditionalLogicalOperator(methodSpec, args, CreateExpressionTree, oper == Operator.LogicalAnd, loc).Resolve(rc));
			this.left = argument.Expr;
			this.right = argument2.Expr;
			return result;
		}

		private bool IsLiftedOperatorApplicable()
		{
			if (left.Type.IsNullableType)
			{
				if ((oper & Operator.EqualityMask) != 0)
				{
					return !right.IsNull;
				}
				return true;
			}
			if (right.Type.IsNullableType)
			{
				if ((oper & Operator.EqualityMask) != 0)
				{
					return !left.IsNull;
				}
				return true;
			}
			if (TypeSpec.IsValueType(left.Type))
			{
				return right.IsNull;
			}
			if (TypeSpec.IsValueType(right.Type))
			{
				return left.IsNull;
			}
			return false;
		}

		private List<MemberSpec> CreateLiftedOperators(ResolveContext rc, IList<MemberSpec> operators)
		{
			TypeSpec typeSpec = rc.Module.PredefinedTypes.Nullable.TypeSpec;
			if (typeSpec == null)
			{
				return null;
			}
			List<MemberSpec> list = null;
			foreach (MethodSpec @operator in operators)
			{
				TypeSpec typeSpec2;
				if ((Oper & Operator.ComparisonMask) != 0)
				{
					typeSpec2 = @operator.ReturnType;
					if (typeSpec2.BuiltinType != BuiltinTypeSpec.Type.FirstPrimitive)
					{
						continue;
					}
				}
				else
				{
					if (!TypeSpec.IsNonNullableValueType(@operator.ReturnType))
					{
						continue;
					}
					typeSpec2 = null;
				}
				TypeSpec[] types = @operator.Parameters.Types;
				if (TypeSpec.IsNonNullableValueType(types[0]) && TypeSpec.IsNonNullableValueType(types[1]) && ((Oper & Operator.EqualityMask) == 0 || types[0] == types[1]))
				{
					if (list == null)
					{
						list = new List<MemberSpec>();
					}
					if (typeSpec2 == null)
					{
						typeSpec2 = typeSpec.MakeGenericType(rc.Module, new TypeSpec[1] { @operator.ReturnType });
					}
					AParametersCollection parameters = ParametersCompiled.CreateFullyResolved(typeSpec.MakeGenericType(rc.Module, new TypeSpec[1] { types[0] }), typeSpec.MakeGenericType(rc.Module, new TypeSpec[1] { types[1] }));
					MethodSpec item = new MethodSpec(@operator.Kind, @operator.DeclaringType, @operator.MemberDefinition, typeSpec2, parameters, @operator.Modifiers);
					list.Add(item);
				}
			}
			return list;
		}

		private static IList<MemberSpec> CombineUserOperators(IList<MemberSpec> left, IList<MemberSpec> right)
		{
			List<MemberSpec> list = new List<MemberSpec>(left.Count + right.Count);
			list.AddRange(left);
			foreach (MemberSpec item in right)
			{
				bool flag = false;
				foreach (MemberSpec item2 in left)
				{
					if (item2.DeclaringType == item.DeclaringType)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.Add(item);
				}
			}
			return list;
		}

		private void CheckOutOfRangeComparison(ResolveContext ec, Constant c, TypeSpec type)
		{
			if (c is IntegralConstant || c is CharConstant)
			{
				try
				{
					c.ConvertExplicitly(true, type);
				}
				catch (OverflowException)
				{
					ec.Report.Warning(652, 2, loc, "A comparison between a constant and a variable is useless. The constant is out of the range of the variable type `{0}'", type.GetSignatureForError());
				}
			}
		}

		public override void EmitBranchable(EmitContext ec, Label target, bool on_true)
		{
			if (ec.HasSet(BuilderContext.Options.AsyncBody) && right.ContainsEmitWithAwait())
			{
				left = left.EmitToField(ec);
				if ((oper & Operator.LogicalMask) == 0)
				{
					right = right.EmitToField(ec);
				}
			}
			if ((oper & Operator.EqualityMask) != 0 && (left is Constant || right is Constant))
			{
				bool flag = ((oper == Operator.Inequality) ? on_true : (!on_true));
				if (left is Constant)
				{
					Expression expression = right;
					right = left;
					left = expression;
				}
				if (((Constant)right).IsZeroInteger && right.Type.BuiltinType != BuiltinTypeSpec.Type.Long && right.Type.BuiltinType != BuiltinTypeSpec.Type.ULong)
				{
					left.EmitBranchable(ec, target, flag);
					return;
				}
				if (right.Type.BuiltinType == BuiltinTypeSpec.Type.FirstPrimitive)
				{
					left.EmitBranchable(ec, target, !flag);
					return;
				}
			}
			else
			{
				if (oper == Operator.LogicalAnd)
				{
					if (on_true)
					{
						Label label = ec.DefineLabel();
						left.EmitBranchable(ec, label, false);
						right.EmitBranchable(ec, target, true);
						ec.MarkLabel(label);
						return;
					}
					if (!(left is Constant))
					{
						left.EmitBranchable(ec, target, false);
					}
					if (!(right is Constant))
					{
						right.EmitBranchable(ec, target, false);
					}
					return;
				}
				if (oper == Operator.LogicalOr)
				{
					if (on_true)
					{
						left.EmitBranchable(ec, target, true);
						right.EmitBranchable(ec, target, true);
						return;
					}
					Label label2 = ec.DefineLabel();
					left.EmitBranchable(ec, label2, true);
					right.EmitBranchable(ec, target, false);
					ec.MarkLabel(label2);
					return;
				}
				if ((oper & Operator.ComparisonMask) == 0)
				{
					base.EmitBranchable(ec, target, on_true);
					return;
				}
			}
			left.Emit(ec);
			right.Emit(ec);
			TypeSpec t = left.Type;
			bool flag2 = IsFloat(t);
			bool flag3 = flag2 || IsUnsigned(t);
			switch (oper)
			{
			case Operator.Equality:
				if (on_true)
				{
					ec.Emit(OpCodes.Beq, target);
				}
				else
				{
					ec.Emit(OpCodes.Bne_Un, target);
				}
				break;
			case Operator.Inequality:
				if (on_true)
				{
					ec.Emit(OpCodes.Bne_Un, target);
				}
				else
				{
					ec.Emit(OpCodes.Beq, target);
				}
				break;
			case Operator.LessThan:
				if (on_true)
				{
					if (flag3 && !flag2)
					{
						ec.Emit(OpCodes.Blt_Un, target);
					}
					else
					{
						ec.Emit(OpCodes.Blt, target);
					}
				}
				else if (flag3)
				{
					ec.Emit(OpCodes.Bge_Un, target);
				}
				else
				{
					ec.Emit(OpCodes.Bge, target);
				}
				break;
			case Operator.GreaterThan:
				if (on_true)
				{
					if (flag3 && !flag2)
					{
						ec.Emit(OpCodes.Bgt_Un, target);
					}
					else
					{
						ec.Emit(OpCodes.Bgt, target);
					}
				}
				else if (flag3)
				{
					ec.Emit(OpCodes.Ble_Un, target);
				}
				else
				{
					ec.Emit(OpCodes.Ble, target);
				}
				break;
			case Operator.LessThanOrEqual:
				if (on_true)
				{
					if (flag3 && !flag2)
					{
						ec.Emit(OpCodes.Ble_Un, target);
					}
					else
					{
						ec.Emit(OpCodes.Ble, target);
					}
				}
				else if (flag3)
				{
					ec.Emit(OpCodes.Bgt_Un, target);
				}
				else
				{
					ec.Emit(OpCodes.Bgt, target);
				}
				break;
			case Operator.GreaterThanOrEqual:
				if (on_true)
				{
					if (flag3 && !flag2)
					{
						ec.Emit(OpCodes.Bge_Un, target);
					}
					else
					{
						ec.Emit(OpCodes.Bge, target);
					}
				}
				else if (flag3)
				{
					ec.Emit(OpCodes.Blt_Un, target);
				}
				else
				{
					ec.Emit(OpCodes.Blt, target);
				}
				break;
			default:
				throw new InternalErrorException(oper.ToString());
			}
		}

		public override void Emit(EmitContext ec)
		{
			if (ec.HasSet(BuilderContext.Options.AsyncBody) && right.ContainsEmitWithAwait())
			{
				left = left.EmitToField(ec);
				if ((oper & Operator.LogicalMask) == 0)
				{
					right = right.EmitToField(ec);
				}
			}
			if ((oper & Operator.LogicalMask) != 0)
			{
				Label label = ec.DefineLabel();
				Label label2 = ec.DefineLabel();
				bool flag = oper == Operator.LogicalOr;
				left.EmitBranchable(ec, label, flag);
				right.Emit(ec);
				ec.Emit(OpCodes.Br_S, label2);
				ec.MarkLabel(label);
				ec.EmitInt(flag ? 1 : 0);
				ec.MarkLabel(label2);
				return;
			}
			if (oper == Operator.Subtraction)
			{
				IntegralConstant integralConstant = left as IntegralConstant;
				if (integralConstant != null && integralConstant.IsDefaultValue)
				{
					right.Emit(ec);
					ec.Emit(OpCodes.Neg);
					return;
				}
			}
			EmitOperator(ec, left, right);
		}

		public void EmitOperator(EmitContext ec, Expression left, Expression right)
		{
			left.Emit(ec);
			right.Emit(ec);
			EmitOperatorOpcode(ec, oper, left.Type, right);
			if (enum_conversion != ConvCast.Mode.I1_U1)
			{
				ConvCast.Emit(ec, enum_conversion);
			}
		}

		public override void EmitSideEffect(EmitContext ec)
		{
			if ((oper & Operator.LogicalMask) != 0 || (ec.HasSet(BuilderContext.Options.CheckedScope) && (oper == Operator.Multiply || oper == Operator.Addition || oper == Operator.Subtraction)))
			{
				base.EmitSideEffect(ec);
				return;
			}
			left.EmitSideEffect(ec);
			right.EmitSideEffect(ec);
		}

		public override Expression EmitToField(EmitContext ec)
		{
			if ((oper & Operator.LogicalMask) == 0)
			{
				Await obj = left as Await;
				if (obj != null && right.IsSideEffectFree)
				{
					obj.Statement.EmitPrologue(ec);
					left = obj.Statement.GetResultExpression(ec);
					return this;
				}
				obj = right as Await;
				if (obj != null && left.IsSideEffectFree)
				{
					obj.Statement.EmitPrologue(ec);
					right = obj.Statement.GetResultExpression(ec);
					return this;
				}
			}
			return base.EmitToField(ec);
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			Binary obj = (Binary)t;
			obj.left = left.Clone(clonectx);
			obj.right = right.Clone(clonectx);
		}

		public Expression CreateCallSiteBinder(ResolveContext ec, Arguments args)
		{
			Arguments arguments = new Arguments(4);
			MemberAccess expr = new MemberAccess(new MemberAccess(new QualifiedAliasMember(QualifiedAliasMember.GlobalAlias, "System", loc), "Linq", loc), "Expressions", loc);
			CSharpBinderFlags cSharpBinderFlags = CSharpBinderFlags.None;
			if (ec.HasSet(ResolveContext.Options.CheckedScope))
			{
				cSharpBinderFlags = CSharpBinderFlags.CheckedContext;
			}
			if ((oper & Operator.LogicalMask) != 0)
			{
				cSharpBinderFlags |= CSharpBinderFlags.BinaryOperationLogical;
			}
			arguments.Add(new Argument(new EnumConstant(new IntLiteral(ec.BuiltinTypes, (int)cSharpBinderFlags, loc), ec.Module.PredefinedTypes.BinderFlags.Resolve())));
			arguments.Add(new Argument(new MemberAccess(new MemberAccess(expr, "ExpressionType", loc), GetOperatorExpressionTypeName(), loc)));
			arguments.Add(new Argument(new TypeOf(ec.CurrentType, loc)));
			arguments.Add(new Argument(new ImplicitlyTypedArrayCreation(args.CreateDynamicBinderArguments(ec), loc)));
			return new Invocation(new MemberAccess(new TypeExpression(ec.Module.PredefinedTypes.Binder.TypeSpec, loc), "BinaryOperation", loc), arguments);
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			return CreateExpressionTree(ec, null);
		}

		public Expression CreateExpressionTree(ResolveContext ec, Expression method)
		{
			bool flag = false;
			string name;
			switch (oper)
			{
			case Operator.Addition:
				name = ((method != null || !ec.HasSet(ResolveContext.Options.CheckedScope) || IsFloat(type)) ? "Add" : "AddChecked");
				break;
			case Operator.BitwiseAnd:
				name = "And";
				break;
			case Operator.BitwiseOr:
				name = "Or";
				break;
			case Operator.Division:
				name = "Divide";
				break;
			case Operator.Equality:
				name = "Equal";
				flag = true;
				break;
			case Operator.ExclusiveOr:
				name = "ExclusiveOr";
				break;
			case Operator.GreaterThan:
				name = "GreaterThan";
				flag = true;
				break;
			case Operator.GreaterThanOrEqual:
				name = "GreaterThanOrEqual";
				flag = true;
				break;
			case Operator.Inequality:
				name = "NotEqual";
				flag = true;
				break;
			case Operator.LeftShift:
				name = "LeftShift";
				break;
			case Operator.LessThan:
				name = "LessThan";
				flag = true;
				break;
			case Operator.LessThanOrEqual:
				name = "LessThanOrEqual";
				flag = true;
				break;
			case Operator.LogicalAnd:
				name = "AndAlso";
				break;
			case Operator.LogicalOr:
				name = "OrElse";
				break;
			case Operator.Modulus:
				name = "Modulo";
				break;
			case Operator.Multiply:
				name = ((method != null || !ec.HasSet(ResolveContext.Options.CheckedScope) || IsFloat(type)) ? "Multiply" : "MultiplyChecked");
				break;
			case Operator.RightShift:
				name = "RightShift";
				break;
			case Operator.Subtraction:
				name = ((method != null || !ec.HasSet(ResolveContext.Options.CheckedScope) || IsFloat(type)) ? "Subtract" : "SubtractChecked");
				break;
			default:
				throw new InternalErrorException("Unknown expression tree binary operator " + oper);
			}
			Arguments arguments = new Arguments(2);
			arguments.Add(new Argument(left.CreateExpressionTree(ec)));
			arguments.Add(new Argument(right.CreateExpressionTree(ec)));
			if (method != null)
			{
				if (flag)
				{
					arguments.Add(new Argument(new BoolLiteral(ec.BuiltinTypes, false, loc)));
				}
				arguments.Add(new Argument(method));
			}
			return CreateExpressionFactoryCall(ec, name, arguments);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
