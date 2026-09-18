namespace Mono.CSharp
{
	internal class DynamicIndexBinder : DynamicMemberAssignable
	{
		private bool can_be_mutator;

		public DynamicIndexBinder(Arguments args, Location loc)
			: base(args, loc)
		{
		}

		public DynamicIndexBinder(CSharpBinderFlags flags, Arguments args, Location loc)
			: this(args, loc)
		{
			base.flags = flags;
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			can_be_mutator = true;
			return base.DoResolve(ec);
		}

		protected override Expression CreateCallSiteBinder(ResolveContext ec, Arguments args, bool isSet)
		{
			Arguments arguments = new Arguments(3);
			arguments.Add(new Argument(new BinderFlags(flags, this)));
			arguments.Add(new Argument(new TypeOf(ec.CurrentType, loc)));
			arguments.Add(new Argument(new ImplicitlyTypedArrayCreation(args.CreateDynamicBinderArguments(ec), loc)));
			isSet |= (flags & CSharpBinderFlags.ValueFromCompoundAssignment) != 0;
			return new Invocation(GetBinder(isSet ? "SetIndex" : "GetIndex", loc), arguments);
		}

		protected override Arguments CreateSetterArguments(ResolveContext rc, Expression rhs)
		{
			if (!can_be_mutator)
			{
				return base.CreateSetterArguments(rc, rhs);
			}
			Arguments arguments = new Arguments(base.Arguments.Count + 1);
			for (int i = 0; i < base.Arguments.Count; i++)
			{
				Expression expr = base.Arguments[i].Expr;
				if (expr is Constant || expr is VariableReference || expr is This)
				{
					arguments.Add(base.Arguments[i]);
					continue;
				}
				LocalVariable localVariable = LocalVariable.CreateCompilerGenerated(expr.Type, rc.CurrentBlock, loc);
				expr = new SimpleAssign(localVariable.CreateReferenceExpression(rc, expr.Location), expr).Resolve(rc);
				base.Arguments[i].Expr = localVariable.CreateReferenceExpression(rc, expr.Location).Resolve(rc);
				arguments.Add(base.Arguments[i].Clone(expr));
			}
			arguments.Add(new Argument(rhs));
			return arguments;
		}
	}
}
