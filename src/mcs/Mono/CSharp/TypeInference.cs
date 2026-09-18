namespace Mono.CSharp
{
	internal class TypeInference
	{
		private int score;

		private readonly Arguments arguments;

		private readonly int arg_count;

		public int InferenceScore => score;

		public TypeInference(Arguments arguments)
		{
			this.arguments = arguments;
			if (arguments != null)
			{
				arg_count = arguments.Count;
			}
		}

		public TypeSpec[] InferMethodArguments(ResolveContext ec, MethodSpec method)
		{
			TypeInferenceContext typeInferenceContext = new TypeInferenceContext(method.GenericDefinition.TypeParameters);
			if (!typeInferenceContext.UnfixedVariableExists)
			{
				return TypeSpec.EmptyTypes;
			}
			AParametersCollection parameters = method.Parameters;
			if (!InferInPhases(ec, typeInferenceContext, parameters))
			{
				return null;
			}
			return typeInferenceContext.InferredTypeArguments;
		}

		private bool InferInPhases(ResolveContext ec, TypeInferenceContext tic, AParametersCollection methodParameters)
		{
			int num = ((!methodParameters.HasParams) ? arg_count : (methodParameters.Count - 1));
			TypeSpec[] array = methodParameters.Types;
			TypeSpec typeSpec = null;
			for (int i = 0; i < arg_count; i++)
			{
				Argument argument = arguments[i];
				if (argument == null)
				{
					continue;
				}
				if (i < num)
				{
					typeSpec = methodParameters.Types[i];
				}
				else if (i == num)
				{
					typeSpec = ((arg_count != num + 1 || !TypeManager.HasElementType(argument.Type)) ? TypeManager.GetElementType(methodParameters.Types[num]) : methodParameters.Types[num]);
					array = (TypeSpec[])array.Clone();
					array[i] = typeSpec;
				}
				AnonymousMethodExpression anonymousMethodExpression = argument.Expr as AnonymousMethodExpression;
				if (anonymousMethodExpression != null)
				{
					if (anonymousMethodExpression.ExplicitTypeInference(tic, typeSpec))
					{
						score++;
					}
				}
				else if (argument.IsByRef)
				{
					score += tic.ExactInference(argument.Type, typeSpec);
				}
				else if (argument.Expr.Type != InternalType.NullLiteral)
				{
					if (TypeSpec.IsValueType(typeSpec))
					{
						score += tic.LowerBoundInference(argument.Type, typeSpec);
					}
					else
					{
						score += tic.OutputTypeInference(ec, argument.Expr, typeSpec);
					}
				}
			}
			bool fixed_any = false;
			if (!tic.FixIndependentTypeArguments(ec, array, ref fixed_any))
			{
				return false;
			}
			return DoSecondPhase(ec, tic, array, !fixed_any);
		}

		private bool DoSecondPhase(ResolveContext ec, TypeInferenceContext tic, TypeSpec[] methodParameters, bool fixDependent)
		{
			bool fixed_any = false;
			if (fixDependent && !tic.FixDependentTypes(ec, ref fixed_any))
			{
				return false;
			}
			if (!tic.UnfixedVariableExists)
			{
				return true;
			}
			if (!fixed_any & fixDependent)
			{
				return false;
			}
			for (int i = 0; i < arg_count; i++)
			{
				TypeSpec typeSpec = methodParameters[(i >= methodParameters.Length) ? (methodParameters.Length - 1) : i];
				if (!typeSpec.IsDelegate)
				{
					if (!typeSpec.IsExpressionTreeType)
					{
						continue;
					}
					typeSpec = TypeManager.GetTypeArguments(typeSpec)[0];
				}
				MethodSpec invokeMethod = Delegate.GetInvokeMethod(typeSpec);
				TypeSpec returnType = invokeMethod.ReturnType;
				if (tic.IsReturnTypeNonDependent(invokeMethod, returnType) && arguments[i] != null)
				{
					score += tic.OutputTypeInference(ec, arguments[i].Expr, typeSpec);
				}
			}
			return DoSecondPhase(ec, tic, methodParameters, true);
		}
	}
}
