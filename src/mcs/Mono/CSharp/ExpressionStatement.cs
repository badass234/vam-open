namespace Mono.CSharp
{
	public abstract class ExpressionStatement : Expression
	{
		public virtual void MarkReachable(Reachability rc)
		{
		}

		public ExpressionStatement ResolveStatement(BlockContext ec)
		{
			Expression expression = Resolve(ec);
			if (expression == null)
			{
				return null;
			}
			ExpressionStatement obj = expression as ExpressionStatement;
			if (obj == null || expression is AnonymousMethodBody)
			{
				Error_InvalidExpressionStatement(ec);
			}
			if (MemberAccess.IsValidDotExpression(expression.Type) && !(expression is Assign) && !(expression is Await))
			{
				WarningAsyncWithoutWait(ec, expression);
			}
			return obj;
		}

		private static void WarningAsyncWithoutWait(BlockContext bc, Expression e)
		{
			if (bc.CurrentAnonymousMethod is AsyncInitializer)
			{
				MethodGroupExpr methodGroupExpr = new AwaitStatement.AwaitableMemberAccess(e)
				{
					ProbingMode = true
				}.Resolve(bc) as MethodGroupExpr;
				if (methodGroupExpr == null)
				{
					return;
				}
				Arguments args = new Arguments(0);
				methodGroupExpr = methodGroupExpr.OverloadResolve(bc, ref args, null, OverloadResolver.Restrictions.ProbingOnly);
				if (methodGroupExpr != null)
				{
					AwaiterDefinition awaiter = bc.Module.GetAwaiter(methodGroupExpr.BestCandidateReturnType);
					if (awaiter.IsValidPattern && awaiter.INotifyCompletion)
					{
						bc.Report.Warning(4014, 1, e.Location, "The statement is not awaited and execution of current method continues before the call is completed. Consider using `await' operator");
					}
				}
			}
			else
			{
				Invocation invocation = e as Invocation;
				if (invocation != null && invocation.MethodGroup != null && invocation.MethodGroup.BestCandidate.IsAsync)
				{
					bc.Report.Warning(4014, 1, e.Location, "The statement is not awaited and execution of current method continues before the call is completed. Consider using `await' operator or calling `Wait' method");
				}
			}
		}

		public abstract void EmitStatement(EmitContext ec);

		public override void EmitSideEffect(EmitContext ec)
		{
			EmitStatement(ec);
		}
	}
}
