namespace Mono.CSharp
{
	public class LambdaExpression : AnonymousMethodExpression
	{
		public override bool HasExplicitParameters
		{
			get
			{
				if (base.Parameters.Count > 0)
				{
					return !(base.Parameters.FixedParameters[0] is ImplicitLambdaParameter);
				}
				return false;
			}
		}

		public LambdaExpression(Location loc)
			: base(loc)
		{
		}

		protected override Expression CreateExpressionTree(ResolveContext ec, TypeSpec delegate_type)
		{
			if (ec.IsInProbingMode)
			{
				return this;
			}
			BlockContext ec2 = new BlockContext(ec.MemberContext, ec.ConstructorBlock, ec.BuiltinTypes.Void)
			{
				CurrentAnonymousMethod = ec.CurrentAnonymousMethod
			};
			Expression expr = base.Parameters.CreateExpressionTree(ec2, loc);
			Expression expression = Block.CreateExpressionTree(ec);
			if (expression == null)
			{
				return null;
			}
			Arguments arguments = new Arguments(2);
			arguments.Add(new Argument(expression));
			arguments.Add(new Argument(expr));
			return CreateExpressionFactoryCall(ec, "Lambda", new TypeArguments(new TypeExpression(delegate_type, loc)), arguments);
		}

		protected override ParametersCompiled ResolveParameters(ResolveContext ec, TypeInferenceContext tic, TypeSpec delegateType)
		{
			if (!delegateType.IsDelegate)
			{
				return null;
			}
			AParametersCollection parameters = Delegate.GetParameters(delegateType);
			if (HasExplicitParameters)
			{
				if (!VerifyExplicitParameters(ec, tic, delegateType, parameters))
				{
					return null;
				}
				return base.Parameters;
			}
			if (!VerifyParameterCompatibility(ec, tic, delegateType, parameters, ec.IsInProbingMode))
			{
				return null;
			}
			TypeSpec[] array = new TypeSpec[base.Parameters.Count];
			for (int i = 0; i < parameters.Count; i++)
			{
				if ((parameters.FixedParameters[i].ModFlags & Parameter.Modifier.RefOutMask) != Parameter.Modifier.NONE)
				{
					return null;
				}
				TypeSpec typeSpec = parameters.Types[i];
				if (tic != null)
				{
					typeSpec = tic.InflateGenericArgument(ec, typeSpec);
				}
				array[i] = typeSpec;
				ImplicitLambdaParameter obj = (ImplicitLambdaParameter)base.Parameters.FixedParameters[i];
				obj.SetParameterType(typeSpec);
				obj.Resolve(null, i);
			}
			base.Parameters.Types = array;
			return base.Parameters;
		}

		protected override AnonymousMethodBody CompatibleMethodFactory(TypeSpec returnType, TypeSpec delegateType, ParametersCompiled p, ParametersBlock b)
		{
			return new LambdaMethod(p, b, returnType, delegateType, loc);
		}

		protected override bool DoResolveParameters(ResolveContext rc)
		{
			if (HasExplicitParameters)
			{
				return base.Parameters.Resolve(rc);
			}
			return true;
		}

		public override string GetSignatureForError()
		{
			return "lambda expression";
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
