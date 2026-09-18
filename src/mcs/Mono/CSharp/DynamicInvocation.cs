namespace Mono.CSharp
{
	internal class DynamicInvocation : DynamicExpressionStatement, IDynamicBinder
	{
		private readonly ATypeNameExpression member;

		public DynamicInvocation(ATypeNameExpression member, Arguments args, Location loc)
			: base(null, args, loc)
		{
			binder = this;
			this.member = member;
		}

		public static DynamicInvocation CreateSpecialNameInvoke(ATypeNameExpression member, Arguments args, Location loc)
		{
			return new DynamicInvocation(member, args, loc)
			{
				flags = CSharpBinderFlags.InvokeSpecialName
			};
		}

		public Expression CreateCallSiteBinder(ResolveContext ec, Arguments args)
		{
			Arguments arguments = new Arguments((member != null) ? 5 : 3);
			bool flag = member is MemberAccess;
			CSharpBinderFlags cSharpBinderFlags;
			if (!flag && member is SimpleName)
			{
				cSharpBinderFlags = CSharpBinderFlags.InvokeSimpleName;
				flag = true;
			}
			else
			{
				cSharpBinderFlags = CSharpBinderFlags.None;
			}
			arguments.Add(new Argument(new BinderFlags(cSharpBinderFlags, this)));
			if (flag)
			{
				arguments.Add(new Argument(new StringLiteral(ec.BuiltinTypes, member.Name, member.Location)));
			}
			if (member != null && member.HasTypeArguments)
			{
				TypeArguments typeArguments = member.TypeArguments;
				if (typeArguments.Resolve(ec, false))
				{
					ArrayInitializer arrayInitializer = new ArrayInitializer(typeArguments.Count, loc);
					TypeSpec[] array = typeArguments.Arguments;
					foreach (TypeSpec typeSpec in array)
					{
						arrayInitializer.Add(new TypeOf(typeSpec, loc));
					}
					arguments.Add(new Argument(new ImplicitlyTypedArrayCreation(arrayInitializer, loc)));
				}
			}
			else if (flag)
			{
				arguments.Add(new Argument(new NullLiteral(loc)));
			}
			arguments.Add(new Argument(new TypeOf(ec.CurrentType, loc)));
			Expression expr = ((args != null) ? new ImplicitlyTypedArrayCreation(args.CreateDynamicBinderArguments(ec), loc) : new ArrayCreation(new MemberAccess(DynamicExpressionStatement.GetBinderNamespace(loc), "CSharpArgumentInfo", loc), new ArrayInitializer(0, loc), loc));
			arguments.Add(new Argument(expr));
			return new Invocation(GetBinder(flag ? "InvokeMember" : "Invoke", loc), arguments);
		}

		public override void EmitStatement(EmitContext ec)
		{
			flags |= CSharpBinderFlags.ResultDiscarded;
			base.EmitStatement(ec);
		}
	}
}
