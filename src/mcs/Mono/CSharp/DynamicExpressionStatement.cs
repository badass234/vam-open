namespace Mono.CSharp
{
	internal class DynamicExpressionStatement : ExpressionStatement
	{
		protected class BinderFlags : EnumConstant
		{
			private readonly DynamicExpressionStatement statement;

			private readonly CSharpBinderFlags flags;

			public BinderFlags(CSharpBinderFlags flags, DynamicExpressionStatement statement)
				: base(statement.loc)
			{
				this.flags = flags;
				this.statement = statement;
				eclass = ExprClass.Unresolved;
			}

			protected override Expression DoResolve(ResolveContext ec)
			{
				Child = new IntConstant(ec.BuiltinTypes, (int)(flags | statement.flags), statement.loc);
				type = ec.Module.PredefinedTypes.BinderFlags.Resolve();
				eclass = Child.eclass;
				return this;
			}
		}

		private readonly Arguments arguments;

		protected IDynamicBinder binder;

		protected Expression binder_expr;

		protected CSharpBinderFlags flags;

		private TypeSpec binder_type;

		private TypeParameters context_mvars;

		public Arguments Arguments => arguments;

		public DynamicExpressionStatement(IDynamicBinder binder, Arguments args, Location loc)
		{
			this.binder = binder;
			arguments = args;
			base.loc = loc;
		}

		public override bool ContainsEmitWithAwait()
		{
			return arguments.ContainsEmitWithAwait();
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			ec.Report.Error(1963, loc, "An expression tree cannot contain a dynamic operation");
			return null;
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			if (DoResolveCore(rc))
			{
				binder_expr = binder.CreateCallSiteBinder(rc, arguments);
			}
			return this;
		}

		protected bool DoResolveCore(ResolveContext rc)
		{
			foreach (Argument argument in arguments)
			{
				if (argument.Type == InternalType.VarOutType)
				{
					rc.Report.Error(8047, argument.Expr.Location, "Declaration expression cannot be used in this context");
				}
			}
			if (rc.CurrentTypeParameters != null && rc.CurrentTypeParameters[0].IsMethodTypeParameter)
			{
				context_mvars = rc.CurrentTypeParameters;
			}
			int errors = rc.Report.Errors;
			PredefinedTypes predefinedTypes = rc.Module.PredefinedTypes;
			binder_type = predefinedTypes.Binder.Resolve();
			predefinedTypes.CallSite.Resolve();
			predefinedTypes.CallSiteGeneric.Resolve();
			eclass = ExprClass.Value;
			if (type == null)
			{
				type = rc.BuiltinTypes.Dynamic;
			}
			if (rc.Report.Errors == errors)
			{
				return true;
			}
			rc.Report.Error(1969, loc, "Dynamic operation cannot be compiled without `Microsoft.CSharp.dll' assembly reference");
			return false;
		}

		public override void Emit(EmitContext ec)
		{
			EmitCall(ec, binder_expr, arguments, false);
		}

		public override void EmitStatement(EmitContext ec)
		{
			EmitCall(ec, binder_expr, arguments, true);
		}

		protected void EmitCall(EmitContext ec, Expression binder, Arguments arguments, bool isStatement)
		{
			int num = arguments?.Count ?? 0;
			int num2 = (isStatement ? 1 : 2);
			ModuleContainer module = ec.Module;
			bool flag = false;
			TypeExpression[] array = new TypeExpression[num + num2];
			array[0] = new TypeExpression(module.PredefinedTypes.CallSite.TypeSpec, loc);
			TypeExpression[] array2 = null;
			DynamicSiteClass dynamicSiteClass = ec.CreateDynamicSite();
			TypeParameterMutator typeParameterMutator;
			if (context_mvars != null)
			{
				TypeContainer typeContainer = dynamicSiteClass;
				TypeParameters currentTypeParameters;
				do
				{
					currentTypeParameters = typeContainer.CurrentTypeParameters;
					typeContainer = typeContainer.Parent;
				}
				while (currentTypeParameters == null);
				typeParameterMutator = new TypeParameterMutator(context_mvars, currentTypeParameters);
				if (!ec.IsAnonymousStoreyMutateRequired)
				{
					array2 = new TypeExpression[array.Length];
					array2[0] = array[0];
				}
			}
			else
			{
				typeParameterMutator = null;
			}
			for (int i = 0; i < num; i++)
			{
				Argument argument = arguments[i];
				if (argument.ArgType == Argument.AType.Out || argument.ArgType == Argument.AType.Ref)
				{
					flag = true;
				}
				TypeSpec typeSpec = argument.Type;
				if (typeSpec.Kind == MemberKind.InternalCompilerType)
				{
					typeSpec = ec.BuiltinTypes.Object;
				}
				if (array2 != null)
				{
					array2[i + 1] = new TypeExpression(typeSpec, loc);
				}
				if (typeParameterMutator != null)
				{
					typeSpec = typeSpec.Mutate(typeParameterMutator);
				}
				array[i + 1] = new TypeExpression(typeSpec, loc);
			}
			TypeExpr typeExpr = null;
			TypeExpr typeExpr2 = null;
			if (!flag)
			{
				string name = (isStatement ? "Action" : "Func");
				TypeSpec typeSpec2 = null;
				Namespace obj = module.GlobalRootNamespace.GetNamespace("System", true);
				if (obj != null)
				{
					typeSpec2 = obj.LookupType(module, name, num + num2, LookupMode.Normal, loc);
				}
				if (typeSpec2 != null)
				{
					if (!isStatement)
					{
						TypeSpec typeSpec3 = type;
						if (typeSpec3.Kind == MemberKind.InternalCompilerType)
						{
							typeSpec3 = ec.BuiltinTypes.Object;
						}
						if (array2 != null)
						{
							TypeExpression[] array3 = array2;
							array3[array3.Length - 1] = new TypeExpression(typeSpec3, loc);
						}
						if (typeParameterMutator != null)
						{
							typeSpec3 = typeSpec3.Mutate(typeParameterMutator);
						}
						array[array.Length - 1] = new TypeExpression(typeSpec3, loc);
					}
					typeExpr = new GenericTypeExpr(typeSpec2, new TypeArguments(array), loc);
					typeExpr2 = ((array2 == null) ? typeExpr : new GenericTypeExpr(typeSpec2, new TypeArguments(array2), loc));
				}
			}
			Delegate obj2;
			if (typeExpr == null)
			{
				TypeSpec typeSpec4 = (isStatement ? ec.BuiltinTypes.Void : type);
				Parameter[] array4 = new Parameter[num + 1];
				array4[0] = new Parameter(array[0], "p0", Parameter.Modifier.NONE, null, loc);
				DynamicSiteClass dynamicSiteClass2 = ec.CreateDynamicSite();
				int num3 = ((dynamicSiteClass2.Containers != null) ? dynamicSiteClass2.Containers.Count : 0);
				if (typeParameterMutator != null)
				{
					typeSpec4 = typeParameterMutator.Mutate(typeSpec4);
				}
				for (int j = 1; j < num + 1; j++)
				{
					array4[j] = new Parameter(array[j], "p" + j.ToString("X"), arguments[j - 1].Modifier, null, loc);
				}
				obj2 = new Delegate(dynamicSiteClass2, new TypeExpression(typeSpec4, loc), Modifiers.INTERNAL | Modifiers.COMPILER_GENERATED, new MemberName("Container" + num3.ToString("X")), new ParametersCompiled(array4), null);
				obj2.CreateContainer();
				obj2.DefineContainer();
				obj2.Define();
				obj2.PrepareEmit();
				dynamicSiteClass2.AddTypeContainer(obj2);
				if (dynamicSiteClass2.CurrentType is InflatedTypeSpec && num3 > 0)
				{
					dynamicSiteClass2.CurrentType.MemberCache.AddMember(obj2.CurrentType);
				}
				typeExpr = new TypeExpression(obj2.CurrentType, loc);
				typeExpr2 = ((array2 == null) ? typeExpr : null);
			}
			else
			{
				obj2 = null;
			}
			GenericTypeExpr genericTypeExpr = new GenericTypeExpr(module.PredefinedTypes.CallSiteGeneric.TypeSpec, new TypeArguments(typeExpr), loc);
			FieldSpec fieldSpec = dynamicSiteClass.CreateCallSiteField(genericTypeExpr, loc);
			if (fieldSpec == null)
			{
				return;
			}
			if (typeExpr2 == null)
			{
				typeExpr2 = new TypeExpression(MemberCache.GetMember(obj2.CurrentType.DeclaringType.MakeGenericType(module, context_mvars.Types), obj2.CurrentType), loc);
			}
			GenericTypeExpr genericTypeExpr2 = new GenericTypeExpr(module.PredefinedTypes.CallSiteGeneric.TypeSpec, new TypeArguments(typeExpr2), loc);
			if (genericTypeExpr2.ResolveAsType(ec.MemberContext) == null)
			{
				return;
			}
			bool flag2 = context_mvars != null && ec.IsAnonymousStoreyMutateRequired;
			TypeSpec typeSpec5 = ((!flag2 && context_mvars != null) ? dynamicSiteClass.CurrentType.MakeGenericType(module, context_mvars.Types) : dynamicSiteClass.CurrentType);
			if (typeSpec5 is InflatedTypeSpec && dynamicSiteClass.AnonymousMethodsCounter > 1)
			{
				TypeParameterSpec[] tparams = ((typeSpec5.MemberDefinition.TypeParametersCount > 0) ? typeSpec5.MemberDefinition.TypeParameters : TypeParameterSpec.EmptyTypes);
				TypeParameterInflator inflator = new TypeParameterInflator(module, typeSpec5, tparams, typeSpec5.TypeArguments);
				typeSpec5.MemberCache.AddMember(fieldSpec.InflateMember(inflator));
			}
			FieldExpr fieldExpr = new FieldExpr(MemberCache.GetMember(typeSpec5, fieldSpec), loc);
			BlockContext blockContext = new BlockContext(ec.MemberContext, null, ec.BuiltinTypes.Void);
			Arguments arguments2 = new Arguments(1);
			arguments2.Add(new Argument(binder));
			StatementExpression statementExpression = new StatementExpression(new SimpleAssign(fieldExpr, new Invocation(new MemberAccess(genericTypeExpr2, "Create"), arguments2)));
			using (ec.With(BuilderContext.Options.OmitDebugInfo, true))
			{
				if (statementExpression.Resolve(blockContext))
				{
					new If(new Binary(Binary.Operator.Equality, fieldExpr, new NullLiteral(loc)), statementExpression, loc).Emit(ec);
				}
				arguments2 = new Arguments(1 + num);
				arguments2.Add(new Argument(fieldExpr));
				if (arguments != null)
				{
					int num4 = 1;
					foreach (Argument argument2 in arguments)
					{
						if (argument2 is NamedArgument)
						{
							arguments2.Add(new Argument(argument2.Expr, argument2.ArgType));
						}
						else
						{
							arguments2.Add(argument2);
						}
						if (flag2 && argument2.Type != array[num4].Type)
						{
							argument2.Expr.Type = array[num4].Type;
						}
						num4++;
					}
				}
				new DelegateInvocation(new MemberAccess(fieldExpr, "Target", loc).Resolve(blockContext), arguments2, false, loc).Resolve(blockContext)?.Emit(ec);
			}
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			arguments.FlowAnalysis(fc);
		}

		public static MemberAccess GetBinderNamespace(Location loc)
		{
			return new MemberAccess(new MemberAccess(new QualifiedAliasMember(QualifiedAliasMember.GlobalAlias, "Microsoft", loc), "CSharp", loc), "RuntimeBinder", loc);
		}

		protected MemberAccess GetBinder(string name, Location loc)
		{
			return new MemberAccess(new TypeExpression(binder_type, loc), name, loc);
		}
	}
}
