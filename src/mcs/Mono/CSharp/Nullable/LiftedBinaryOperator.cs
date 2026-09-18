using System.Linq.Expressions;
using System.Reflection.Emit;

namespace Mono.CSharp.Nullable
{
	internal class LiftedBinaryOperator : Expression
	{
		public Binary Binary { get; private set; }

		public Expression Left { get; set; }

		public Expression Right { get; set; }

		public Unwrap UnwrapLeft { get; set; }

		public Unwrap UnwrapRight { get; set; }

		public MethodSpec UserOperator { get; set; }

		private bool IsBitwiseBoolean
		{
			get
			{
				if (Binary.Oper == Binary.Operator.BitwiseAnd || Binary.Oper == Binary.Operator.BitwiseOr)
				{
					if (UnwrapLeft == null || UnwrapLeft.Type.BuiltinType != BuiltinTypeSpec.Type.FirstPrimitive)
					{
						if (UnwrapRight != null)
						{
							return UnwrapRight.Type.BuiltinType == BuiltinTypeSpec.Type.FirstPrimitive;
						}
						return false;
					}
					return true;
				}
				return false;
			}
		}

		public LiftedBinaryOperator(Binary b)
		{
			Binary = b;
			loc = b.Location;
		}

		public override bool ContainsEmitWithAwait()
		{
			if (!Left.ContainsEmitWithAwait())
			{
				return Right.ContainsEmitWithAwait();
			}
			return true;
		}

		public override Expression CreateExpressionTree(ResolveContext rc)
		{
			if (UserOperator != null)
			{
				Arguments arguments = new Arguments(2);
				arguments.Add(new Argument(Binary.Left));
				arguments.Add(new Argument(Binary.Right));
				return new UserOperatorCall(UserOperator, arguments, Binary.CreateExpressionTree, loc).CreateExpressionTree(rc);
			}
			return Binary.CreateExpressionTree(rc);
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			if (rc.IsRuntimeBinder)
			{
				if (UnwrapLeft == null && !Left.Type.IsNullableType)
				{
					Left = LiftOperand(rc, Left);
				}
				if (UnwrapRight == null && !Right.Type.IsNullableType)
				{
					Right = LiftOperand(rc, Right);
				}
			}
			else
			{
				if (UnwrapLeft == null && Left != null && Left.Type.IsNullableType)
				{
					Left = Unwrap.CreateUnwrapped(Left);
					UnwrapLeft = Left as Unwrap;
				}
				if (UnwrapRight == null && Right != null && Right.Type.IsNullableType)
				{
					Right = Unwrap.CreateUnwrapped(Right);
					UnwrapRight = Right as Unwrap;
				}
			}
			type = Binary.Type;
			eclass = Binary.eclass;
			return this;
		}

		private Expression LiftOperand(ResolveContext rc, Expression expr)
		{
			TypeSpec typeSpec = ((!expr.IsNull) ? expr.Type : (Left.IsNull ? Right.Type : Left.Type));
			if (!typeSpec.IsNullableType)
			{
				typeSpec = NullableInfo.MakeType(rc.Module, typeSpec);
			}
			return Wrap.Create(expr, typeSpec);
		}

		public override void Emit(EmitContext ec)
		{
			if (IsBitwiseBoolean && UserOperator == null)
			{
				EmitBitwiseBoolean(ec);
				return;
			}
			if ((Binary.Oper & Binary.Operator.EqualityMask) != 0)
			{
				EmitEquality(ec);
				return;
			}
			Label label = ec.DefineLabel();
			Label label2 = ec.DefineLabel();
			if (ec.HasSet(BuilderContext.Options.AsyncBody) && Right.ContainsEmitWithAwait())
			{
				Left = Left.EmitToField(ec);
				Right = Right.EmitToField(ec);
			}
			if (UnwrapLeft != null)
			{
				UnwrapLeft.EmitCheck(ec);
			}
			if (UnwrapRight != null && !Binary.Left.Equals(Binary.Right))
			{
				UnwrapRight.EmitCheck(ec);
				if (UnwrapLeft != null)
				{
					ec.Emit(OpCodes.And);
				}
			}
			ec.Emit(OpCodes.Brfalse, label);
			if (UserOperator != null)
			{
				Arguments arguments = new Arguments(2);
				arguments.Add(new Argument(Left));
				arguments.Add(new Argument(Right));
				default(CallEmitter).EmitPredefined(ec, UserOperator, arguments);
			}
			else
			{
				Binary.EmitOperator(ec, Left, Right);
			}
			if (type.IsNullableType)
			{
				ec.Emit(OpCodes.Newobj, NullableInfo.GetConstructor(type));
			}
			ec.Emit(OpCodes.Br_S, label2);
			ec.MarkLabel(label);
			if ((Binary.Oper & Binary.Operator.ComparisonMask) != 0)
			{
				ec.EmitInt(0);
			}
			else
			{
				LiftedNull.Create(type, loc).Emit(ec);
			}
			ec.MarkLabel(label2);
		}

		private void EmitBitwiseBoolean(EmitContext ec)
		{
			Label label = ec.DefineLabel();
			Label label2 = ec.DefineLabel();
			Label label3 = ec.DefineLabel();
			Label label4 = ec.DefineLabel();
			bool flag = Binary.Oper == Binary.Operator.BitwiseOr;
			if (UnwrapLeft != null && UnwrapRight != null)
			{
				if (ec.HasSet(BuilderContext.Options.AsyncBody) && Binary.Right.ContainsEmitWithAwait())
				{
					Left = Left.EmitToField(ec);
					Right = Right.EmitToField(ec);
				}
				else
				{
					UnwrapLeft.Store(ec);
					UnwrapRight.Store(ec);
				}
				Left.Emit(ec);
				ec.Emit(OpCodes.Brtrue_S, label2);
				Right.Emit(ec);
				ec.Emit(OpCodes.Brtrue_S, label);
				UnwrapLeft.EmitCheck(ec);
				ec.Emit(OpCodes.Brfalse_S, label2);
				ec.MarkLabel(label);
				if (flag)
				{
					UnwrapRight.Load(ec);
				}
				else
				{
					UnwrapLeft.Load(ec);
				}
				ec.Emit(OpCodes.Br_S, label3);
				ec.MarkLabel(label2);
				if (flag)
				{
					UnwrapLeft.Load(ec);
				}
				else
				{
					UnwrapRight.Load(ec);
				}
				ec.MarkLabel(label3);
				return;
			}
			if (UnwrapLeft == null)
			{
				if (Left is BoolConstant)
				{
					UnwrapRight.Store(ec);
					ec.EmitInt(flag ? 1 : 0);
					ec.Emit(OpCodes.Newobj, NullableInfo.GetConstructor(type));
				}
				else if (Left.IsNull)
				{
					UnwrapRight.Emit(ec);
					ec.Emit(flag ? OpCodes.Brfalse_S : OpCodes.Brtrue_S, label4);
					UnwrapRight.Load(ec);
					ec.Emit(OpCodes.Br_S, label3);
					ec.MarkLabel(label4);
					LiftedNull.Create(type, loc).Emit(ec);
				}
				else
				{
					Left.Emit(ec);
					ec.Emit(flag ? OpCodes.Brfalse_S : OpCodes.Brtrue_S, label2);
					ec.EmitInt(flag ? 1 : 0);
					ec.Emit(OpCodes.Newobj, NullableInfo.GetConstructor(type));
					ec.Emit(OpCodes.Br_S, label3);
					ec.MarkLabel(label2);
					UnwrapRight.Original.Emit(ec);
				}
			}
			else
			{
				UnwrapLeft.Store(ec);
				if (Right is BoolConstant)
				{
					ec.EmitInt(flag ? 1 : 0);
					ec.Emit(OpCodes.Newobj, NullableInfo.GetConstructor(type));
				}
				else if (Right.IsNull)
				{
					UnwrapLeft.Emit(ec);
					ec.Emit(flag ? OpCodes.Brfalse_S : OpCodes.Brtrue_S, label4);
					UnwrapLeft.Load(ec);
					ec.Emit(OpCodes.Br_S, label3);
					ec.MarkLabel(label4);
					LiftedNull.Create(type, loc).Emit(ec);
				}
				else
				{
					Right.Emit(ec);
					ec.Emit(flag ? OpCodes.Brfalse_S : OpCodes.Brtrue_S, label2);
					ec.EmitInt(flag ? 1 : 0);
					ec.Emit(OpCodes.Newobj, NullableInfo.GetConstructor(type));
					ec.Emit(OpCodes.Br_S, label3);
					ec.MarkLabel(label2);
					UnwrapLeft.Load(ec);
				}
			}
			ec.MarkLabel(label3);
		}

		private void EmitEquality(EmitContext ec)
		{
			if (UnwrapLeft != null && Binary.Right.IsNull)
			{
				UnwrapLeft.EmitCheck(ec);
				if (Binary.Oper == Binary.Operator.Equality)
				{
					ec.EmitInt(0);
					ec.Emit(OpCodes.Ceq);
				}
				return;
			}
			if (UnwrapRight != null && Binary.Left.IsNull)
			{
				UnwrapRight.EmitCheck(ec);
				if (Binary.Oper == Binary.Operator.Equality)
				{
					ec.EmitInt(0);
					ec.Emit(OpCodes.Ceq);
				}
				return;
			}
			Label label = ec.DefineLabel();
			Label label2 = ec.DefineLabel();
			if (UserOperator != null)
			{
				Expression expr = Left;
				if (UnwrapLeft != null)
				{
					UnwrapLeft.EmitCheck(ec);
				}
				else if (!(Left is VariableReference))
				{
					Left.Emit(ec);
					LocalTemporary localTemporary = new LocalTemporary(Left.Type);
					localTemporary.Store(ec);
					expr = localTemporary;
				}
				if (UnwrapRight != null)
				{
					UnwrapRight.EmitCheck(ec);
					if (UnwrapLeft != null)
					{
						ec.Emit(OpCodes.Bne_Un, label);
						Label label3 = ec.DefineLabel();
						UnwrapLeft.EmitCheck(ec);
						ec.Emit(OpCodes.Brtrue, label3);
						if (Binary.Oper == Binary.Operator.Equality)
						{
							ec.EmitInt(1);
						}
						else
						{
							ec.EmitInt(0);
						}
						ec.Emit(OpCodes.Br, label2);
						ec.MarkLabel(label3);
					}
					else
					{
						ec.Emit(OpCodes.Brfalse, label);
					}
				}
				else
				{
					ec.Emit(OpCodes.Brfalse, label);
				}
				Arguments arguments = new Arguments(2);
				arguments.Add(new Argument(expr));
				arguments.Add(new Argument(Right));
				default(CallEmitter).EmitPredefined(ec, UserOperator, arguments);
			}
			else
			{
				if (ec.HasSet(BuilderContext.Options.AsyncBody) && Binary.Right.ContainsEmitWithAwait())
				{
					Left = Left.EmitToField(ec);
					Right = Right.EmitToField(ec);
				}
				Left.Emit(ec);
				Right.Emit(ec);
				ec.Emit(OpCodes.Bne_Un_S, label);
				if (UnwrapLeft != null)
				{
					UnwrapLeft.EmitCheck(ec);
				}
				if (UnwrapRight != null)
				{
					UnwrapRight.EmitCheck(ec);
				}
				if (UnwrapLeft != null && UnwrapRight != null)
				{
					if (Binary.Oper == Binary.Operator.Inequality)
					{
						ec.Emit(OpCodes.Xor);
					}
					else
					{
						ec.Emit(OpCodes.Ceq);
					}
				}
				else if (Binary.Oper == Binary.Operator.Inequality)
				{
					ec.EmitInt(0);
					ec.Emit(OpCodes.Ceq);
				}
			}
			ec.Emit(OpCodes.Br_S, label2);
			ec.MarkLabel(label);
			if (Binary.Oper == Binary.Operator.Inequality)
			{
				ec.EmitInt(1);
			}
			else
			{
				ec.EmitInt(0);
			}
			ec.MarkLabel(label2);
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			Binary.FlowAnalysis(fc);
		}

		public override System.Linq.Expressions.Expression MakeExpression(BuilderContext ctx)
		{
			return Binary.MakeExpression(ctx, Left, Right);
		}
	}
}
