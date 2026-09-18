using System.Reflection.Emit;

namespace Mono.CSharp.Nullable
{
	public class NullCoalescingOperator : Expression
	{
		private Expression left;

		private Expression right;

		private Unwrap unwrap;

		public Expression LeftExpression => left;

		public Expression RightExpression => right;

		public NullCoalescingOperator(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
			loc = left.Location;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			if (left is NullLiteral)
			{
				ec.Report.Error(845, loc, "An expression tree cannot contain a coalescing operator with null left side");
			}
			UserCast userCast = left as UserCast;
			Expression expression = null;
			if (userCast != null)
			{
				left = userCast.Source;
				Arguments arguments = new Arguments(2);
				arguments.Add(new Argument(userCast.CreateExpressionTree(ec)));
				arguments.Add(new Argument(left.CreateExpressionTree(ec)));
				expression = CreateExpressionFactoryCall(ec, "Lambda", arguments);
			}
			Arguments arguments2 = new Arguments(3);
			arguments2.Add(new Argument(left.CreateExpressionTree(ec)));
			arguments2.Add(new Argument(right.CreateExpressionTree(ec)));
			if (expression != null)
			{
				arguments2.Add(new Argument(expression));
			}
			return CreateExpressionFactoryCall(ec, "Coalesce", arguments2);
		}

		private Expression ConvertExpression(ResolveContext ec)
		{
			if (left.eclass == ExprClass.MethodGroup)
			{
				return null;
			}
			TypeSpec typeSpec = left.Type;
			if (typeSpec.IsNullableType)
			{
				unwrap = Unwrap.Create(left, false);
				if (unwrap == null)
				{
					return null;
				}
				if (right.IsNull)
				{
					return ReducedExpression.Create(left, this);
				}
				if (right.Type.IsNullableType)
				{
					Expression expression = ((right.Type == typeSpec) ? right : Convert.ImplicitNulableConversion(ec, right, typeSpec));
					if (expression != null)
					{
						right = expression;
						type = typeSpec;
						return this;
					}
				}
				else
				{
					Expression expression = Convert.ImplicitConversion(ec, right, unwrap.Type, loc);
					if (expression != null)
					{
						left = unwrap;
						typeSpec = left.Type;
						if (right.Type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
						{
							type = right.Type;
							left = Convert.ImplicitBoxingConversion(left, typeSpec, type);
							return this;
						}
						right = expression;
						type = typeSpec;
						return this;
					}
				}
			}
			else
			{
				if (!TypeSpec.IsReferenceType(typeSpec))
				{
					return null;
				}
				if (Convert.ImplicitConversionExists(ec, right, typeSpec))
				{
					if (right.Type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
					{
						type = right.Type;
						return this;
					}
					Constant constant = left as Constant;
					if (constant != null && !constant.IsDefaultValue)
					{
						return ReducedExpression.Create(constant, this, false);
					}
					if (right.IsNull || constant != null)
					{
						if (right.IsNull && typeSpec == right.Type)
						{
							return null;
						}
						return ReducedExpression.Create((constant != null) ? right : left, this, false);
					}
					right = Convert.ImplicitConversion(ec, right, typeSpec, loc);
					type = typeSpec;
					return this;
				}
			}
			TypeSpec target_type = right.Type;
			if (!Convert.ImplicitConversionExists(ec, unwrap ?? left, target_type) || right.eclass == ExprClass.MethodGroup)
			{
				return null;
			}
			if (left.IsNull)
			{
				return ReducedExpression.Create(right, this, false).Resolve(ec);
			}
			left = Convert.ImplicitConversion(ec, unwrap ?? left, target_type, loc);
			type = target_type;
			return this;
		}

		public override bool ContainsEmitWithAwait()
		{
			if (unwrap != null)
			{
				if (!unwrap.ContainsEmitWithAwait())
				{
					return right.ContainsEmitWithAwait();
				}
				return true;
			}
			if (!left.ContainsEmitWithAwait())
			{
				return right.ContainsEmitWithAwait();
			}
			return true;
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			left = left.Resolve(ec);
			right = right.Resolve(ec);
			if (left == null || right == null)
			{
				return null;
			}
			eclass = ExprClass.Value;
			Expression expression = ConvertExpression(ec);
			if (expression == null)
			{
				Binary.Error_OperatorCannotBeApplied(ec, left, right, "??", loc);
				return null;
			}
			return expression;
		}

		public override void Emit(EmitContext ec)
		{
			Label label = ec.DefineLabel();
			if (unwrap != null)
			{
				Label label2 = ec.DefineLabel();
				unwrap.EmitCheck(ec);
				ec.Emit(OpCodes.Brfalse, label2);
				if (type.IsNullableType && TypeSpecComparer.IsEqual(NullableInfo.GetUnderlyingType(type), unwrap.Type))
				{
					unwrap.Load(ec);
				}
				else
				{
					left.Emit(ec);
				}
				ec.Emit(OpCodes.Br, label);
				ec.MarkLabel(label2);
				right.Emit(ec);
				ec.MarkLabel(label);
				return;
			}
			UserCast userCast = left as UserCast;
			if (userCast != null)
			{
				userCast.Source.Emit(ec);
				LocalTemporary localTemporary;
				if (!(userCast.Source is VariableReference))
				{
					localTemporary = new LocalTemporary(userCast.Source.Type);
					localTemporary.Store(ec);
					localTemporary.Emit(ec);
					userCast.Source = localTemporary;
				}
				else
				{
					localTemporary = null;
				}
				Label label3 = ec.DefineLabel();
				ec.Emit(OpCodes.Brfalse_S, label3);
				left.Emit(ec);
				ec.Emit(OpCodes.Br, label);
				ec.MarkLabel(label3);
				localTemporary?.Release(ec);
			}
			else
			{
				left.Emit(ec);
				ec.Emit(OpCodes.Dup);
				if (left.Type.IsGenericParameter)
				{
					ec.Emit(OpCodes.Box, left.Type);
				}
				ec.Emit(OpCodes.Brtrue, label);
				ec.Emit(OpCodes.Pop);
			}
			right.Emit(ec);
			ec.MarkLabel(label);
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			left.FlowAnalysis(fc);
			DefiniteAssignmentBitSet definiteAssignment = fc.BranchDefiniteAssignment();
			right.FlowAnalysis(fc);
			fc.DefiniteAssignment = definiteAssignment;
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			NullCoalescingOperator obj = (NullCoalescingOperator)t;
			obj.left = left.Clone(clonectx);
			obj.right = right.Clone(clonectx);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
