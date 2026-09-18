using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Mono.CSharp
{
	public class UserOperatorCall : Expression
	{
		protected readonly Arguments arguments;

		protected readonly MethodSpec oper;

		private readonly Func<ResolveContext, Expression, Expression> expr_tree;

		public UserOperatorCall(MethodSpec oper, Arguments args, Func<ResolveContext, Expression, Expression> expr_tree, Location loc)
		{
			this.oper = oper;
			arguments = args;
			this.expr_tree = expr_tree;
			type = oper.ReturnType;
			eclass = ExprClass.Value;
			base.loc = loc;
		}

		public override bool ContainsEmitWithAwait()
		{
			return arguments.ContainsEmitWithAwait();
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			if (expr_tree != null)
			{
				return expr_tree(ec, new TypeOfMethod(oper, loc));
			}
			Arguments args = Arguments.CreateForExpressionTree(ec, arguments, new NullLiteral(loc), new TypeOfMethod(oper, loc));
			return CreateExpressionFactoryCall(ec, "Call", args);
		}

		protected override void CloneTo(CloneContext context, Expression target)
		{
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			default(CallEmitter).Emit(ec, oper, arguments, loc);
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			arguments.FlowAnalysis(fc);
		}

		public override System.Linq.Expressions.Expression MakeExpression(BuilderContext ctx)
		{
			return System.Linq.Expressions.Expression.Call((MethodInfo)oper.GetMetaInfo(), Arguments.MakeExpression(arguments, ctx));
		}
	}
}
