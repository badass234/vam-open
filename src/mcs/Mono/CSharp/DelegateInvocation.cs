using System.Linq.Expressions;

namespace Mono.CSharp
{
	internal class DelegateInvocation : ExpressionStatement
	{
		private readonly Expression InstanceExpr;

		private readonly bool conditionalAccessReceiver;

		private Arguments arguments;

		private MethodSpec method;

		public DelegateInvocation(Expression instance_expr, Arguments args, bool conditionalAccessReceiver, Location loc)
		{
			InstanceExpr = instance_expr;
			arguments = args;
			this.conditionalAccessReceiver = conditionalAccessReceiver;
			base.loc = loc;
		}

		public override bool ContainsEmitWithAwait()
		{
			if (!InstanceExpr.ContainsEmitWithAwait())
			{
				if (arguments != null)
				{
					return arguments.ContainsEmitWithAwait();
				}
				return false;
			}
			return true;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			Arguments args = Arguments.CreateForExpressionTree(ec, arguments, InstanceExpr.CreateExpressionTree(ec));
			return CreateExpressionFactoryCall(ec, "Invoke", args);
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			InstanceExpr.FlowAnalysis(fc);
			if (arguments != null)
			{
				arguments.FlowAnalysis(fc);
			}
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			TypeSpec typeSpec = InstanceExpr.Type;
			if (typeSpec == null)
			{
				return null;
			}
			method = Delegate.GetInvokeMethod(typeSpec);
			OverloadResolver overloadResolver = new OverloadResolver(new MemberSpec[1] { method }, OverloadResolver.Restrictions.DelegateInvoke, loc);
			if (overloadResolver.ResolveMember<MethodSpec>(ec, ref arguments) == null && !overloadResolver.BestCandidateIsDynamic)
			{
				return null;
			}
			type = method.ReturnType;
			if (conditionalAccessReceiver)
			{
				type = Expression.LiftMemberType(ec, type);
			}
			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			if (conditionalAccessReceiver)
			{
				ec.ConditionalAccess = new ConditionalAccessContext(type, ec.DefineLabel());
			}
			CallEmitter callEmitter = default(CallEmitter);
			callEmitter.InstanceExpression = InstanceExpr;
			callEmitter.Emit(ec, method, arguments, loc);
			if (conditionalAccessReceiver)
			{
				ec.CloseConditionalAccess((type.IsNullableType && type != method.ReturnType) ? type : null);
			}
		}

		public override void EmitStatement(EmitContext ec)
		{
			if (conditionalAccessReceiver)
			{
				ec.ConditionalAccess = new ConditionalAccessContext(type, ec.DefineLabel())
				{
					Statement = true
				};
			}
			CallEmitter callEmitter = default(CallEmitter);
			callEmitter.InstanceExpression = InstanceExpr;
			callEmitter.EmitStatement(ec, method, arguments, loc);
			if (conditionalAccessReceiver)
			{
				ec.CloseConditionalAccess(null);
			}
		}

		public override System.Linq.Expressions.Expression MakeExpression(BuilderContext ctx)
		{
			return Invocation.MakeExpression(ctx, InstanceExpr, method, arguments);
		}
	}
}
