namespace Mono.CSharp
{
	internal class OptionalAssign : SimpleAssign
	{
		public override Location StartLocation => Location.Null;

		public OptionalAssign(Expression s, Location loc)
			: base(null, s, loc)
		{
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			Expression expression = source.Clone(new CloneContext());
			expression = expression.Resolve(ec);
			if (expression == null)
			{
				return null;
			}
			if (ec.Module.Evaluator.DescribeTypeExpressions && !(ec.CurrentAnonymousMethod is AsyncInitializer))
			{
				ReportPrinter printer = ec.Report.SetPrinter(new SessionReportPrinter());
				Expression expression2;
				try
				{
					expression2 = source.Clone(new CloneContext());
					expression2 = expression2.Resolve(ec, ResolveFlags.Type);
					if (ec.Report.Errors > 0)
					{
						expression2 = null;
					}
				}
				finally
				{
					ec.Report.SetPrinter(printer);
				}
				if (expression2 is TypeExpr)
				{
					Arguments arguments = new Arguments(1);
					arguments.Add(new Argument(new TypeOf((TypeExpr)expression, base.Location)));
					return new Invocation(new SimpleName("Describe", base.Location), arguments).Resolve(ec);
				}
			}
			if (expression.Type.Kind == MemberKind.Void || expression is DynamicInvocation || expression is Assign)
			{
				return expression;
			}
			source = expression;
			Method method = (Method)ec.MemberContext.CurrentMemberDefinition;
			if (method.ParameterInfo.IsEmpty)
			{
				eclass = ExprClass.Value;
				type = InternalType.FakeInternalType;
				return this;
			}
			target = new SimpleName(method.ParameterInfo[0].Name, base.Location);
			return base.DoResolve(ec);
		}

		public override void EmitStatement(EmitContext ec)
		{
			if (target == null)
			{
				source.Emit(ec);
			}
			else
			{
				base.EmitStatement(ec);
			}
		}
	}
}
