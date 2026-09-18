namespace Mono.CSharp
{
	public class NewDelegate : DelegateCreation
	{
		public Arguments Arguments;

		public NewDelegate(TypeSpec type, Arguments Arguments, Location loc)
		{
			base.type = type;
			this.Arguments = Arguments;
			base.loc = loc;
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			if (Arguments == null || Arguments.Count != 1)
			{
				ec.Report.Error(149, loc, "Method name expected");
				return null;
			}
			Argument argument = Arguments[0];
			if (!argument.ResolveMethodGroup(ec))
			{
				return null;
			}
			Expression expression = argument.Expr;
			AnonymousMethodExpression anonymousMethodExpression = expression as AnonymousMethodExpression;
			if (anonymousMethodExpression != null && ec.Module.Compiler.Settings.Version != LanguageVersion.ISO_1)
			{
				return anonymousMethodExpression.Compatible(ec, type)?.Resolve(ec);
			}
			method_group = expression as MethodGroupExpr;
			if (method_group == null)
			{
				if (expression.Type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
				{
					expression = Convert.ImplicitConversionRequired(ec, expression, type, loc);
				}
				else if (!expression.Type.IsDelegate)
				{
					expression.Error_UnexpectedKind(ec, ResolveFlags.Type | ResolveFlags.MethodGroup, loc);
					return null;
				}
				method_group = new MethodGroupExpr(Delegate.GetInvokeMethod(expression.Type), expression.Type, loc);
				method_group.InstanceExpression = expression;
			}
			return base.DoResolve(ec);
		}
	}
}
