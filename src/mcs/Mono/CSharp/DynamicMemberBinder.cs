namespace Mono.CSharp
{
	internal class DynamicMemberBinder : DynamicMemberAssignable
	{
		private readonly string name;

		public DynamicMemberBinder(string name, Arguments args, Location loc)
			: base(args, loc)
		{
			this.name = name;
		}

		public DynamicMemberBinder(string name, CSharpBinderFlags flags, Arguments args, Location loc)
			: this(name, args, loc)
		{
			base.flags = flags;
		}

		protected override Expression CreateCallSiteBinder(ResolveContext ec, Arguments args, bool isSet)
		{
			Arguments arguments = new Arguments(4);
			arguments.Add(new Argument(new BinderFlags(flags, this)));
			arguments.Add(new Argument(new StringLiteral(ec.BuiltinTypes, name, loc)));
			arguments.Add(new Argument(new TypeOf(ec.CurrentType, loc)));
			arguments.Add(new Argument(new ImplicitlyTypedArrayCreation(args.CreateDynamicBinderArguments(ec), loc)));
			isSet |= (flags & CSharpBinderFlags.ValueFromCompoundAssignment) != 0;
			return new Invocation(GetBinder(isSet ? "SetMember" : "GetMember", loc), arguments);
		}
	}
}
