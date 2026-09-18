namespace Mono.CSharp
{
	internal class DynamicConstructorBinder : DynamicExpressionStatement, IDynamicBinder
	{
		public DynamicConstructorBinder(TypeSpec type, Arguments args, Location loc)
			: base(null, args, loc)
		{
			base.type = type;
			binder = this;
		}

		public Expression CreateCallSiteBinder(ResolveContext ec, Arguments args)
		{
			Arguments arguments = new Arguments(3);
			arguments.Add(new Argument(new BinderFlags(CSharpBinderFlags.None, this)));
			arguments.Add(new Argument(new TypeOf(ec.CurrentType, loc)));
			arguments.Add(new Argument(new ImplicitlyTypedArrayCreation(args.CreateDynamicBinderArguments(ec), loc)));
			return new Invocation(GetBinder("InvokeConstructor", loc), arguments);
		}
	}
}
