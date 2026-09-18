namespace Mono.CSharp
{
	internal class DynamicConversion : DynamicExpressionStatement, IDynamicBinder
	{
		public DynamicConversion(TypeSpec targetType, CSharpBinderFlags flags, Arguments args, Location loc)
			: base(null, args, loc)
		{
			type = targetType;
			base.flags = flags;
			binder = this;
		}

		public Expression CreateCallSiteBinder(ResolveContext ec, Arguments args)
		{
			Arguments arguments = new Arguments(3);
			flags |= (CSharpBinderFlags)(ec.HasSet(ResolveContext.Options.CheckedScope) ? 1 : 0);
			arguments.Add(new Argument(new BinderFlags(flags, this)));
			arguments.Add(new Argument(new TypeOf(type, loc)));
			arguments.Add(new Argument(new TypeOf(ec.CurrentType, loc)));
			return new Invocation(GetBinder("Convert", loc), arguments);
		}
	}
}
