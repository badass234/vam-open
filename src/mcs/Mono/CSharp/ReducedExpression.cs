using System;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class ReducedExpression : Expression
	{
		public sealed class ReducedConstantExpression : EmptyConstantCast
		{
			private readonly Expression orig_expr;

			public Expression OriginalExpression => orig_expr;

			public ReducedConstantExpression(Constant expr, Expression orig_expr)
				: base(expr, expr.Type)
			{
				this.orig_expr = orig_expr;
			}

			public override Constant ConvertImplicitly(TypeSpec target_type)
			{
				Constant constant = base.ConvertImplicitly(target_type);
				if (constant != null)
				{
					constant = new ReducedConstantExpression(constant, orig_expr);
				}
				return constant;
			}

			public override Expression CreateExpressionTree(ResolveContext ec)
			{
				return orig_expr.CreateExpressionTree(ec);
			}

			public override Constant ConvertExplicitly(bool in_checked_context, TypeSpec target_type)
			{
				Constant constant = base.ConvertExplicitly(in_checked_context, target_type);
				if (constant != null)
				{
					constant = new ReducedConstantExpression(constant, orig_expr);
				}
				return constant;
			}

			public override void EncodeAttributeValue(IMemberContext rc, AttributeEncoder enc, TypeSpec targetType, TypeSpec parameterType)
			{
				if (orig_expr is Conditional)
				{
					child.EncodeAttributeValue(rc, enc, targetType, parameterType);
				}
				else
				{
					base.EncodeAttributeValue(rc, enc, targetType, parameterType);
				}
			}
		}

		private sealed class ReducedExpressionStatement : ExpressionStatement
		{
			private readonly Expression orig_expr;

			private readonly ExpressionStatement stm;

			public ReducedExpressionStatement(ExpressionStatement stm, Expression orig)
			{
				orig_expr = orig;
				this.stm = stm;
				eclass = stm.eclass;
				type = stm.Type;
				loc = orig.Location;
			}

			public override bool ContainsEmitWithAwait()
			{
				return stm.ContainsEmitWithAwait();
			}

			public override Expression CreateExpressionTree(ResolveContext ec)
			{
				return orig_expr.CreateExpressionTree(ec);
			}

			protected override Expression DoResolve(ResolveContext ec)
			{
				return this;
			}

			public override void Emit(EmitContext ec)
			{
				stm.Emit(ec);
			}

			public override void EmitStatement(EmitContext ec)
			{
				stm.EmitStatement(ec);
			}

			public override void FlowAnalysis(FlowAnalysisContext fc)
			{
				stm.FlowAnalysis(fc);
			}
		}

		private readonly Expression expr;

		private readonly Expression orig_expr;

		public override bool IsSideEffectFree => expr.IsSideEffectFree;

		public Expression OriginalExpression => orig_expr;

		private ReducedExpression(Expression expr, Expression orig_expr)
		{
			this.expr = expr;
			eclass = expr.eclass;
			type = expr.Type;
			this.orig_expr = orig_expr;
			loc = orig_expr.Location;
		}

		public override bool ContainsEmitWithAwait()
		{
			return expr.ContainsEmitWithAwait();
		}

		public static Constant Create(Constant expr, Expression original_expr)
		{
			if (expr.eclass == ExprClass.Unresolved)
			{
				throw new ArgumentException("Unresolved expression");
			}
			return new ReducedConstantExpression(expr, original_expr);
		}

		public static ExpressionStatement Create(ExpressionStatement s, Expression orig)
		{
			return new ReducedExpressionStatement(s, orig);
		}

		public static Expression Create(Expression expr, Expression original_expr)
		{
			return Create(expr, original_expr, true);
		}

		public static Expression Create(Expression expr, Expression original_expr, bool canBeConstant)
		{
			if (canBeConstant)
			{
				Constant constant = expr as Constant;
				if (constant != null)
				{
					return Create(constant, original_expr);
				}
			}
			ExpressionStatement expressionStatement = expr as ExpressionStatement;
			if (expressionStatement != null)
			{
				return Create(expressionStatement, original_expr);
			}
			if (expr.eclass == ExprClass.Unresolved)
			{
				throw new ArgumentException("Unresolved expression");
			}
			return new ReducedExpression(expr, original_expr);
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			return orig_expr.CreateExpressionTree(ec);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			expr.Emit(ec);
		}

		public override Expression EmitToField(EmitContext ec)
		{
			return expr.EmitToField(ec);
		}

		public override void EmitBranchable(EmitContext ec, Label target, bool on_true)
		{
			expr.EmitBranchable(ec, target, on_true);
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			expr.FlowAnalysis(fc);
		}

		public override System.Linq.Expressions.Expression MakeExpression(BuilderContext ctx)
		{
			return orig_expr.MakeExpression(ctx);
		}
	}
}
