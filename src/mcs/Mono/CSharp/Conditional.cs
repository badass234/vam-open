using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class Conditional : Expression
	{
		private Expression expr;

		private Expression true_expr;

		private Expression false_expr;

		public Expression Expr => expr;

		public Expression TrueExpr => true_expr;

		public Expression FalseExpr => false_expr;

		public Conditional(Expression expr, Expression true_expr, Expression false_expr, Location loc)
		{
			this.expr = expr;
			this.true_expr = true_expr;
			this.false_expr = false_expr;
			base.loc = loc;
		}

		public override bool ContainsEmitWithAwait()
		{
			if (!Expr.ContainsEmitWithAwait() && !true_expr.ContainsEmitWithAwait())
			{
				return false_expr.ContainsEmitWithAwait();
			}
			return true;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			Arguments arguments = new Arguments(3);
			arguments.Add(new Argument(expr.CreateExpressionTree(ec)));
			arguments.Add(new Argument(true_expr.CreateExpressionTree(ec)));
			arguments.Add(new Argument(false_expr.CreateExpressionTree(ec)));
			return CreateExpressionFactoryCall(ec, "Condition", arguments);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			expr = expr.Resolve(ec);
			true_expr = true_expr.Resolve(ec);
			false_expr = false_expr.Resolve(ec);
			if (true_expr == null || false_expr == null || expr == null)
			{
				return null;
			}
			eclass = ExprClass.Value;
			TypeSpec typeSpec = true_expr.Type;
			TypeSpec typeSpec2 = false_expr.Type;
			type = typeSpec;
			if (!TypeSpecComparer.IsEqual(typeSpec, typeSpec2))
			{
				Expression expression = Convert.ImplicitConversion(ec, true_expr, typeSpec2, loc);
				if (expression != null && typeSpec.BuiltinType != BuiltinTypeSpec.Type.Dynamic)
				{
					type = typeSpec2;
					if (typeSpec2.BuiltinType != BuiltinTypeSpec.Type.Dynamic)
					{
						Expression expression2 = Convert.ImplicitConversion(ec, false_expr, typeSpec, loc);
						if (expression2 != null)
						{
							if (expression2.Type.BuiltinType == BuiltinTypeSpec.Type.Int && expression is Constant)
							{
								type = typeSpec;
								expression2 = null;
							}
							else if (type.BuiltinType == BuiltinTypeSpec.Type.Int && expression2 is Constant)
							{
								expression2 = null;
							}
						}
						if (expression2 != null)
						{
							ec.Report.Error(172, true_expr.Location, "Type of conditional expression cannot be determined as `{0}' and `{1}' convert implicitly to each other", typeSpec.GetSignatureForError(), typeSpec2.GetSignatureForError());
						}
					}
					true_expr = expression;
					if (true_expr.Type != type)
					{
						true_expr = EmptyCast.Create(true_expr, type);
					}
				}
				else
				{
					if ((expression = Convert.ImplicitConversion(ec, false_expr, typeSpec, loc)) == null)
					{
						if (typeSpec2 != InternalType.ErrorType)
						{
							ec.Report.Error(173, true_expr.Location, "Type of conditional expression cannot be determined because there is no implicit conversion between `{0}' and `{1}'", typeSpec.GetSignatureForError(), typeSpec2.GetSignatureForError());
						}
						return null;
					}
					false_expr = expression;
				}
			}
			Constant constant = expr as Constant;
			if (constant != null)
			{
				bool isDefaultValue = constant.IsDefaultValue;
				if (!(isDefaultValue ? (true_expr is Constant) : (false_expr is Constant)))
				{
					Expression.Warning_UnreachableExpression(ec, isDefaultValue ? true_expr.Location : false_expr.Location);
				}
				return ReducedExpression.Create(isDefaultValue ? false_expr : true_expr, this, false_expr is Constant && true_expr is Constant).Resolve(ec);
			}
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			Label label = ec.DefineLabel();
			Label label2 = ec.DefineLabel();
			expr.EmitBranchable(ec, label, false);
			true_expr.Emit(ec);
			if (type.IsInterface && true_expr is EmptyCast && false_expr is EmptyCast)
			{
				LocalBuilder temporaryLocal = ec.GetTemporaryLocal(type);
				ec.Emit(OpCodes.Stloc, temporaryLocal);
				ec.Emit(OpCodes.Ldloc, temporaryLocal);
				ec.FreeTemporaryLocal(temporaryLocal, type);
			}
			ec.Emit(OpCodes.Br, label2);
			ec.MarkLabel(label);
			false_expr.Emit(ec);
			ec.MarkLabel(label2);
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			expr.FlowAnalysisConditional(fc);
			DefiniteAssignmentBitSet definiteAssignmentOnTrue = fc.DefiniteAssignmentOnTrue;
			DefiniteAssignmentBitSet definiteAssignmentOnFalse = fc.DefiniteAssignmentOnFalse;
			fc.BranchDefiniteAssignment(definiteAssignmentOnTrue);
			true_expr.FlowAnalysis(fc);
			DefiniteAssignmentBitSet definiteAssignment = fc.DefiniteAssignment;
			fc.BranchDefiniteAssignment(definiteAssignmentOnFalse);
			false_expr.FlowAnalysis(fc);
			fc.DefiniteAssignment &= definiteAssignment;
		}

		public override void FlowAnalysisConditional(FlowAnalysisContext fc)
		{
			expr.FlowAnalysisConditional(fc);
			DefiniteAssignmentBitSet definiteAssignmentOnTrue = fc.DefiniteAssignmentOnTrue;
			DefiniteAssignmentBitSet definiteAssignmentOnFalse = fc.DefiniteAssignmentOnFalse;
			DefiniteAssignmentBitSet definiteAssignmentBitSet = (fc.DefiniteAssignment = new DefiniteAssignmentBitSet(definiteAssignmentOnTrue));
			DefiniteAssignmentBitSet definiteAssignmentOnTrue2 = (fc.DefiniteAssignmentOnFalse = definiteAssignmentBitSet);
			fc.DefiniteAssignmentOnTrue = definiteAssignmentOnTrue2;
			true_expr.FlowAnalysisConditional(fc);
			DefiniteAssignmentBitSet definiteAssignment = fc.DefiniteAssignment;
			DefiniteAssignmentBitSet definiteAssignmentOnTrue3 = fc.DefiniteAssignmentOnTrue;
			DefiniteAssignmentBitSet definiteAssignmentOnFalse2 = fc.DefiniteAssignmentOnFalse;
			definiteAssignmentBitSet = (fc.DefiniteAssignment = new DefiniteAssignmentBitSet(definiteAssignmentOnFalse));
			definiteAssignmentOnTrue2 = (fc.DefiniteAssignmentOnFalse = definiteAssignmentBitSet);
			fc.DefiniteAssignmentOnTrue = definiteAssignmentOnTrue2;
			false_expr.FlowAnalysisConditional(fc);
			fc.DefiniteAssignment &= definiteAssignment;
			fc.DefiniteAssignmentOnTrue = definiteAssignmentOnTrue3 & fc.DefiniteAssignmentOnTrue;
			fc.DefiniteAssignmentOnFalse = definiteAssignmentOnFalse2 & fc.DefiniteAssignmentOnFalse;
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			Conditional obj = (Conditional)t;
			obj.expr = expr.Clone(clonectx);
			obj.true_expr = true_expr.Clone(clonectx);
			obj.false_expr = false_expr.Clone(clonectx);
		}
	}
}
