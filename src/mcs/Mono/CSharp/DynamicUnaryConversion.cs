namespace Mono.CSharp
{
	internal class DynamicUnaryConversion : DynamicExpressionStatement, IDynamicBinder
	{
		private readonly string name;

		public DynamicUnaryConversion(string name, Arguments args, Location loc)
			: base(null, args, loc)
		{
			this.name = name;
			binder = this;
		}

		public static DynamicUnaryConversion CreateIsTrue(ResolveContext rc, Arguments args, Location loc)
		{
			return new DynamicUnaryConversion("IsTrue", args, loc)
			{
				type = rc.BuiltinTypes.Bool
			};
		}

		public static DynamicUnaryConversion CreateIsFalse(ResolveContext rc, Arguments args, Location loc)
		{
			return new DynamicUnaryConversion("IsFalse", args, loc)
			{
				type = rc.BuiltinTypes.Bool
			};
		}

		public Expression CreateCallSiteBinder(ResolveContext ec, Arguments args)
		{
			Arguments arguments = new Arguments(4);
			MemberAccess expr = new MemberAccess(new MemberAccess(new QualifiedAliasMember(QualifiedAliasMember.GlobalAlias, "System", loc), "Linq", loc), "Expressions", loc);
			CSharpBinderFlags cSharpBinderFlags = (ec.HasSet(ResolveContext.Options.CheckedScope) ? CSharpBinderFlags.CheckedContext : CSharpBinderFlags.None);
			arguments.Add(new Argument(new BinderFlags(cSharpBinderFlags, this)));
			arguments.Add(new Argument(new MemberAccess(new MemberAccess(expr, "ExpressionType", loc), name, loc)));
			arguments.Add(new Argument(new TypeOf(ec.CurrentType, loc)));
			arguments.Add(new Argument(new ImplicitlyTypedArrayCreation(args.CreateDynamicBinderArguments(ec), loc)));
			return new Invocation(GetBinder("UnaryOperation", loc), arguments);
		}
	}
}
