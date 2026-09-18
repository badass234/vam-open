using System;
using System.Collections.Generic;
using Mono.CSharp.Nullable;

namespace Mono.CSharp
{
	public class UnaryMutator : ExpressionStatement
	{
		private class DynamicPostMutator : Expression, IAssignMethod
		{
			private LocalTemporary temp;

			private Expression expr;

			public DynamicPostMutator(Expression expr)
			{
				this.expr = expr;
				type = expr.Type;
				loc = expr.Location;
			}

			public override Expression CreateExpressionTree(ResolveContext ec)
			{
				throw new NotImplementedException("ET");
			}

			protected override Expression DoResolve(ResolveContext rc)
			{
				eclass = expr.eclass;
				return this;
			}

			public override Expression DoResolveLValue(ResolveContext ec, Expression right_side)
			{
				expr.DoResolveLValue(ec, right_side);
				return DoResolve(ec);
			}

			public override void Emit(EmitContext ec)
			{
				temp.Emit(ec);
			}

			public void Emit(EmitContext ec, bool leave_copy)
			{
				throw new NotImplementedException();
			}

			public void EmitAssign(EmitContext ec, Expression source, bool leave_copy, bool isCompound)
			{
				temp = new LocalTemporary(type);
				expr.Emit(ec);
				temp.Store(ec);
				((IAssignMethod)expr).EmitAssign(ec, source, false, isCompound);
				if (leave_copy)
				{
					Emit(ec);
				}
				temp.Release(ec);
				temp = null;
			}
		}

		[Flags]
		public enum Mode : byte
		{
			IsIncrement = 0,
			IsDecrement = 1,
			IsPre = 0,
			IsPost = 2,
			PreIncrement = 0,
			PreDecrement = IsDecrement,
			PostIncrement = IsPost,
			PostDecrement = IsDecrement | IsPost
		}

		private Mode mode;

		private bool is_expr;

		private bool recurse;

		protected Expression expr;

		private Expression operation;

		public Mode UnaryMutatorMode => mode;

		public Expression Expr => expr;

		public override Location StartLocation
		{
			get
			{
				if ((mode & Mode.IsPost) == 0)
				{
					return loc;
				}
				return expr.Location;
			}
		}

		private bool IsDecrement => (mode & Mode.IsDecrement) != 0;

		public UnaryMutator(Mode m, Expression e, Location loc)
		{
			mode = m;
			base.loc = loc;
			expr = e;
		}

		public override bool ContainsEmitWithAwait()
		{
			return expr.ContainsEmitWithAwait();
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			return new SimpleAssign(this, this).CreateExpressionTree(ec);
		}

		public static TypeSpec[] CreatePredefinedOperatorsTable(BuiltinTypes types)
		{
			return new TypeSpec[11]
			{
				types.Int, types.Long, types.SByte, types.Byte, types.Short, types.UInt, types.ULong, types.Char, types.Float, types.Double,
				types.Decimal
			};
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			expr = expr.Resolve(ec);
			if (expr == null || expr.Type == InternalType.ErrorType)
			{
				return null;
			}
			if (expr.Type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				if ((mode & Mode.IsPost) != Mode.IsIncrement)
				{
					expr = new DynamicPostMutator(expr);
				}
				Arguments arguments = new Arguments(1);
				arguments.Add(new Argument(expr));
				return new SimpleAssign(expr, new DynamicUnaryConversion(GetOperatorExpressionTypeName(), arguments, loc)).Resolve(ec);
			}
			if (expr.Type.IsNullableType)
			{
				return new LiftedUnaryMutator(mode, expr, loc).Resolve(ec);
			}
			return DoResolveOperation(ec);
		}

		protected Expression DoResolveOperation(ResolveContext ec)
		{
			eclass = ExprClass.Value;
			type = expr.Type;
			if (expr is RuntimeValueExpression)
			{
				operation = expr;
			}
			else
			{
				operation = new EmptyExpression(type);
			}
			if (expr.eclass == ExprClass.Variable || expr.eclass == ExprClass.IndexerAccess || expr.eclass == ExprClass.PropertyAccess)
			{
				expr = expr.ResolveLValue(ec, expr);
			}
			else
			{
				ec.Report.Error(1059, loc, "The operand of an increment or decrement operator must be a variable, property or indexer");
			}
			Operator.OpType opType = (IsDecrement ? Operator.OpType.Decrement : Operator.OpType.Increment);
			IList<MemberSpec> userOperator = MemberCache.GetUserOperator(type, opType, false);
			if (userOperator != null)
			{
				Arguments args = new Arguments(1);
				args.Add(new Argument(expr));
				MethodSpec methodSpec = new OverloadResolver(userOperator, OverloadResolver.Restrictions.NoBaseMembers | OverloadResolver.Restrictions.BaseMembersIncluded, loc).ResolveOperator(ec, ref args);
				if (methodSpec == null)
				{
					return null;
				}
				args[0].Expr = operation;
				operation = new UserOperatorCall(methodSpec, args, null, loc);
				operation = Convert.ImplicitConversionRequired(ec, operation, type, loc);
				return this;
			}
			Expression expression = null;
			bool flag;
			switch (type.BuiltinType)
			{
			case BuiltinTypeSpec.Type.Byte:
			case BuiltinTypeSpec.Type.SByte:
			case BuiltinTypeSpec.Type.Char:
			case BuiltinTypeSpec.Type.Short:
			case BuiltinTypeSpec.Type.UShort:
			case BuiltinTypeSpec.Type.Int:
			case BuiltinTypeSpec.Type.UInt:
			case BuiltinTypeSpec.Type.Long:
			case BuiltinTypeSpec.Type.ULong:
			case BuiltinTypeSpec.Type.Float:
			case BuiltinTypeSpec.Type.Double:
			case BuiltinTypeSpec.Type.Decimal:
				expression = operation;
				flag = true;
				break;
			default:
				flag = false;
				if (type.IsPointer)
				{
					if (((PointerContainer)type).Element.Kind == MemberKind.Void)
					{
						Error_VoidPointerOperation(ec);
						return null;
					}
					expression = operation;
				}
				else
				{
					Expression expression2 = null;
					TypeSpec[] operatorsUnaryMutator = ec.BuiltinTypes.OperatorsUnaryMutator;
					foreach (TypeSpec target in operatorsUnaryMutator)
					{
						expression = Convert.ImplicitUserConversion(ec, operation, target, loc);
						if (expression == null)
						{
							continue;
						}
						if (expression2 == null)
						{
							expression2 = expression;
							continue;
						}
						switch (OverloadResolver.BetterTypeConversion(ec, expression2.Type, expression.Type))
						{
						case 2:
							expression2 = expression;
							continue;
						case 1:
							continue;
						}
						Unary.Error_Ambiguous(ec, OperName(mode), type, loc);
						break;
					}
					expression = expression2;
				}
				if (expression == null && type.IsEnum)
				{
					expression = operation;
				}
				if (expression == null)
				{
					expr.Error_OperatorCannotBeApplied(ec, loc, Operator.GetName(opType), type);
					return null;
				}
				break;
			}
			IntConstant right = new IntConstant(ec.BuiltinTypes, 1, loc);
			Binary.Operator oper = (IsDecrement ? Binary.Operator.Subtraction : Binary.Operator.Addition);
			operation = new Binary(oper, expression, right);
			operation = operation.Resolve(ec);
			if (operation == null)
			{
				throw new NotImplementedException("should not be reached");
			}
			if (operation.Type != type)
			{
				if (flag)
				{
					operation = Convert.ExplicitNumericConversion(ec, operation, type);
				}
				else
				{
					operation = Convert.ImplicitConversionRequired(ec, operation, type, loc);
				}
			}
			return this;
		}

		private void EmitCode(EmitContext ec, bool is_expr)
		{
			recurse = true;
			this.is_expr = is_expr;
			((IAssignMethod)expr).EmitAssign(ec, this, is_expr && (mode == Mode.IsIncrement || mode == Mode.IsDecrement), true);
		}

		public override void Emit(EmitContext ec)
		{
			if (recurse)
			{
				((IAssignMethod)expr).Emit(ec, is_expr && (mode == Mode.IsPost || mode == Mode.PostDecrement));
				EmitOperation(ec);
				recurse = false;
			}
			else
			{
				EmitCode(ec, true);
			}
		}

		protected virtual void EmitOperation(EmitContext ec)
		{
			operation.Emit(ec);
		}

		public override void EmitStatement(EmitContext ec)
		{
			EmitCode(ec, false);
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			expr.FlowAnalysis(fc);
		}

		private string GetOperatorExpressionTypeName()
		{
			if (!IsDecrement)
			{
				return "Increment";
			}
			return "Decrement";
		}

		public static string OperName(Mode oper)
		{
			if ((oper & Mode.IsDecrement) == 0)
			{
				return "++";
			}
			return "--";
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			((UnaryMutator)t).expr = expr.Clone(clonectx);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
