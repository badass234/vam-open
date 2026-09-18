namespace Mono.CSharp
{
	internal class DynamicEventCompoundAssign : ExpressionStatement
	{
		private class IsEvent : DynamicExpressionStatement, IDynamicBinder
		{
			private string name;

			public IsEvent(string name, Arguments args, Location loc)
				: base(null, args, loc)
			{
				this.name = name;
				binder = this;
			}

			public Expression CreateCallSiteBinder(ResolveContext ec, Arguments args)
			{
				type = ec.BuiltinTypes.Bool;
				Arguments arguments = new Arguments(3);
				arguments.Add(new Argument(new BinderFlags(CSharpBinderFlags.None, this)));
				arguments.Add(new Argument(new StringLiteral(ec.BuiltinTypes, name, loc)));
				arguments.Add(new Argument(new TypeOf(ec.CurrentType, loc)));
				return new Invocation(GetBinder("IsEvent", loc), arguments);
			}
		}

		private Expression condition;

		private ExpressionStatement invoke;

		private ExpressionStatement assign;

		public DynamicEventCompoundAssign(string name, Arguments args, ExpressionStatement assignment, ExpressionStatement invoke, Location loc)
		{
			condition = new IsEvent(name, args, loc);
			this.invoke = invoke;
			assign = assignment;
			base.loc = loc;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			return condition.CreateExpressionTree(ec);
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			type = rc.BuiltinTypes.Dynamic;
			eclass = ExprClass.Value;
			condition = condition.Resolve(rc);
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			ResolveContext rc = new ResolveContext(ec.MemberContext);
			new Conditional(new BooleanExpression(condition), invoke, assign, loc).Resolve(rc).Emit(ec);
		}

		public override void EmitStatement(EmitContext ec)
		{
			If obj = new If(condition, new StatementExpression(invoke), new StatementExpression(assign), loc);
			using (ec.With(BuilderContext.Options.OmitDebugInfo, true))
			{
				obj.Emit(ec);
			}
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			invoke.FlowAnalysis(fc);
		}
	}
}
