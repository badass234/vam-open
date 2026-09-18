using System;
using System.Collections.Generic;

namespace Mono.CSharp
{
	public class AnonymousMethodExpression : Expression
	{
		private class Quote : ShimExpression
		{
			public Quote(Expression expr)
				: base(expr)
			{
			}

			public override Expression CreateExpressionTree(ResolveContext ec)
			{
				Arguments arguments = new Arguments(1);
				arguments.Add(new Argument(expr.CreateExpressionTree(ec)));
				return CreateExpressionFactoryCall(ec, "Quote", arguments);
			}

			protected override Expression DoResolve(ResolveContext rc)
			{
				expr = expr.Resolve(rc);
				if (expr == null)
				{
					return null;
				}
				eclass = expr.eclass;
				type = expr.Type;
				return this;
			}
		}

		private readonly Dictionary<TypeSpec, Expression> compatibles;

		public ParametersBlock Block;

		public override string ExprClassName => "anonymous method";

		public virtual bool HasExplicitParameters => Parameters != ParametersCompiled.Undefined;

		public override bool IsSideEffectFree => true;

		public ParametersCompiled Parameters => Block.Parameters;

		public ReportPrinter TypeInferenceReportPrinter { get; set; }

		public AnonymousMethodExpression(Location loc)
		{
			base.loc = loc;
			compatibles = new Dictionary<TypeSpec, Expression>();
		}

		public bool ImplicitStandardConversionExists(ResolveContext ec, TypeSpec delegate_type)
		{
			using (ec.With(ResolveContext.Options.InferReturnType, false))
			{
				using (ec.Set(ResolveContext.Options.ProbingMode))
				{
					ReportPrinter printer = ec.Report.SetPrinter(TypeInferenceReportPrinter ?? new NullReportPrinter());
					bool result = Compatible(ec, delegate_type) != null;
					ec.Report.SetPrinter(printer);
					return result;
				}
			}
		}

		private TypeSpec CompatibleChecks(ResolveContext ec, TypeSpec delegate_type)
		{
			if (delegate_type.IsDelegate)
			{
				return delegate_type;
			}
			if (delegate_type.IsExpressionTreeType)
			{
				delegate_type = delegate_type.TypeArguments[0];
				if (delegate_type.IsDelegate)
				{
					return delegate_type;
				}
				ec.Report.Error(835, loc, "Cannot convert `{0}' to an expression tree of non-delegate type `{1}'", GetSignatureForError(), delegate_type.GetSignatureForError());
				return null;
			}
			ec.Report.Error(1660, loc, "Cannot convert `{0}' to non-delegate type `{1}'", GetSignatureForError(), delegate_type.GetSignatureForError());
			return null;
		}

		protected bool VerifyExplicitParameters(ResolveContext ec, TypeInferenceContext tic, TypeSpec delegate_type, AParametersCollection parameters)
		{
			if (VerifyParameterCompatibility(ec, tic, delegate_type, parameters, ec.IsInProbingMode))
			{
				return true;
			}
			if (!ec.IsInProbingMode)
			{
				ec.Report.Error(1661, loc, "Cannot convert `{0}' to delegate type `{1}' since there is a parameter mismatch", GetSignatureForError(), delegate_type.GetSignatureForError());
			}
			return false;
		}

		protected bool VerifyParameterCompatibility(ResolveContext ec, TypeInferenceContext tic, TypeSpec delegate_type, AParametersCollection invoke_pd, bool ignore_errors)
		{
			if (Parameters.Count != invoke_pd.Count)
			{
				if (ignore_errors)
				{
					return false;
				}
				ec.Report.Error(1593, loc, "Delegate `{0}' does not take `{1}' arguments", delegate_type.GetSignatureForError(), Parameters.Count.ToString());
				return false;
			}
			bool flag = !HasExplicitParameters;
			bool flag2 = false;
			for (int i = 0; i < Parameters.Count; i++)
			{
				Parameter.Modifier modFlags = invoke_pd.FixedParameters[i].ModFlags;
				if (Parameters.FixedParameters[i].ModFlags != modFlags && modFlags != Parameter.Modifier.PARAMS)
				{
					if (ignore_errors)
					{
						return false;
					}
					if (modFlags == Parameter.Modifier.NONE)
					{
						ec.Report.Error(1677, Parameters[i].Location, "Parameter `{0}' should not be declared with the `{1}' keyword", (i + 1).ToString(), Parameter.GetModifierSignature(Parameters[i].ModFlags));
					}
					else
					{
						ec.Report.Error(1676, Parameters[i].Location, "Parameter `{0}' must be declared with the `{1}' keyword", (i + 1).ToString(), Parameter.GetModifierSignature(modFlags));
					}
					flag2 = true;
				}
				if (flag)
				{
					continue;
				}
				TypeSpec typeSpec = invoke_pd.Types[i];
				if (tic != null)
				{
					typeSpec = tic.InflateGenericArgument(ec, typeSpec);
				}
				if (!TypeSpecComparer.IsEqual(typeSpec, Parameters.Types[i]))
				{
					if (ignore_errors)
					{
						return false;
					}
					ec.Report.Error(1678, Parameters[i].Location, "Parameter `{0}' is declared as type `{1}' but should be `{2}'", (i + 1).ToString(), Parameters.Types[i].GetSignatureForError(), invoke_pd.Types[i].GetSignatureForError());
					flag2 = true;
				}
			}
			return !flag2;
		}

		public bool ExplicitTypeInference(TypeInferenceContext type_inference, TypeSpec delegate_type)
		{
			if (!HasExplicitParameters)
			{
				return false;
			}
			if (!delegate_type.IsDelegate)
			{
				if (!delegate_type.IsExpressionTreeType)
				{
					return false;
				}
				delegate_type = TypeManager.GetTypeArguments(delegate_type)[0];
				if (!delegate_type.IsDelegate)
				{
					return false;
				}
			}
			AParametersCollection parameters = Delegate.GetParameters(delegate_type);
			if (parameters.Count != Parameters.Count)
			{
				return false;
			}
			TypeSpec[] types = Parameters.Types;
			TypeSpec[] types2 = parameters.Types;
			for (int i = 0; i < Parameters.Count; i++)
			{
				if (type_inference.ExactInference(types[i], types2[i]) == 0 && types[i] != types2[i])
				{
					return false;
				}
			}
			return true;
		}

		public TypeSpec InferReturnType(ResolveContext ec, TypeInferenceContext tic, TypeSpec delegate_type)
		{
			Expression value;
			if (compatibles.TryGetValue(delegate_type, out value))
			{
				return (value as AnonymousExpression)?.ReturnType;
			}
			AnonymousExpression anonymousExpression;
			using (ec.Set(ResolveContext.Options.ProbingMode | ResolveContext.Options.InferReturnType))
			{
				ReportPrinter printer = ((TypeInferenceReportPrinter == null) ? null : ec.Report.SetPrinter(TypeInferenceReportPrinter));
				AnonymousMethodBody anonymousMethodBody = CompatibleMethodBody(ec, tic, null, delegate_type);
				anonymousExpression = anonymousMethodBody?.Compatible(ec, anonymousMethodBody);
				if (TypeInferenceReportPrinter != null)
				{
					ec.Report.SetPrinter(printer);
				}
			}
			return anonymousExpression?.ReturnType;
		}

		public override bool ContainsEmitWithAwait()
		{
			return false;
		}

		public Expression Compatible(ResolveContext ec, TypeSpec type)
		{
			Expression value;
			if (compatibles.TryGetValue(type, out value))
			{
				return value;
			}
			if (type == InternalType.ErrorType)
			{
				return null;
			}
			TypeSpec typeSpec = CompatibleChecks(ec, type);
			if (typeSpec == null)
			{
				return null;
			}
			TypeSpec returnType = Delegate.GetInvokeMethod(typeSpec).ReturnType;
			AnonymousMethodBody anonymousMethodBody = CompatibleMethodBody(ec, null, returnType, typeSpec);
			if (anonymousMethodBody == null)
			{
				return null;
			}
			bool flag = typeSpec != type;
			try
			{
				if (flag)
				{
					if (ec.HasSet(ResolveContext.Options.ExpressionTreeConversion))
					{
						value = anonymousMethodBody.Compatible(ec, ec.CurrentAnonymousMethod);
						if (value != null)
						{
							value = new Quote(value);
						}
					}
					else
					{
						int errors = ec.Report.Errors;
						if (Block.IsAsync)
						{
							ec.Report.Error(1989, loc, "Async lambda expressions cannot be converted to expression trees");
						}
						using (ec.Set(ResolveContext.Options.ExpressionTreeConversion))
						{
							value = anonymousMethodBody.Compatible(ec);
						}
						if (value != null && errors == ec.Report.Errors)
						{
							value = CreateExpressionTree(ec, typeSpec);
						}
					}
				}
				else
				{
					value = anonymousMethodBody.Compatible(ec);
					if (anonymousMethodBody.DirectMethodGroupConversion != null)
					{
						SessionReportPrinter sessionReportPrinter = new SessionReportPrinter();
						ReportPrinter printer = ec.Report.SetPrinter(sessionReportPrinter);
						Expression expression = new ImplicitDelegateCreation(typeSpec, anonymousMethodBody.DirectMethodGroupConversion, loc)
						{
							AllowSpecialMethodsInvocation = true
						}.Resolve(ec);
						ec.Report.SetPrinter(printer);
						if (expression != null && sessionReportPrinter.ErrorsCount == 0)
						{
							value = expression;
						}
					}
				}
			}
			catch (CompletionResult)
			{
				throw;
			}
			catch (FatalException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new InternalErrorException(e, loc);
			}
			if (!ec.IsInProbingMode && !flag)
			{
				compatibles.Add(type, value ?? EmptyExpression.Null);
			}
			return value;
		}

		protected virtual Expression CreateExpressionTree(ResolveContext ec, TypeSpec delegate_type)
		{
			return CreateExpressionTree(ec);
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			ec.Report.Error(1946, loc, "An anonymous method cannot be converted to an expression tree");
			return null;
		}

		protected virtual ParametersCompiled ResolveParameters(ResolveContext ec, TypeInferenceContext tic, TypeSpec delegate_type)
		{
			AParametersCollection parameters = Delegate.GetParameters(delegate_type);
			if (Parameters == ParametersCompiled.Undefined)
			{
				Parameter[] array = new Parameter[parameters.Count];
				for (int i = 0; i < parameters.Count; i++)
				{
					if ((parameters.FixedParameters[i].ModFlags & Parameter.Modifier.OUT) != Parameter.Modifier.NONE)
					{
						if (!ec.IsInProbingMode)
						{
							ec.Report.Error(1688, loc, "Cannot convert anonymous method block without a parameter list to delegate type `{0}' because it has one or more `out' parameters", delegate_type.GetSignatureForError());
						}
						return null;
					}
					array[i] = new Parameter(new TypeExpression(parameters.Types[i], loc), null, parameters.FixedParameters[i].ModFlags, null, loc);
				}
				return ParametersCompiled.CreateFullyResolved(array, parameters.Types);
			}
			if (!VerifyExplicitParameters(ec, tic, delegate_type, parameters))
			{
				return null;
			}
			return Parameters;
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			if (rc.HasSet(ResolveContext.Options.ConstantScope))
			{
				rc.Report.Error(1706, loc, "Anonymous methods and lambda expressions cannot be used in the current context");
				return null;
			}
			if (rc.HasAny(ResolveContext.Options.FieldInitializerScope | ResolveContext.Options.BaseInitializer) && rc.CurrentMemberDefinition.Parent.PartialContainer.PrimaryConstructorParameters != null)
			{
				ToplevelBlock topBlock = rc.ConstructorBlock.ParametersBlock.TopBlock;
				if (Block.TopBlock != topBlock)
				{
					Block block = Block;
					while (block.Parent != Block.TopBlock && block != Block.TopBlock)
					{
						block = block.Parent;
					}
					block.Parent = topBlock;
					topBlock.IncludeBlock(Block, Block.TopBlock);
					block.ParametersBlock.TopBlock = topBlock;
				}
			}
			eclass = ExprClass.Value;
			type = InternalType.AnonymousMethod;
			if (!DoResolveParameters(rc))
			{
				return null;
			}
			return this;
		}

		protected virtual bool DoResolveParameters(ResolveContext rc)
		{
			return Parameters.Resolve(rc);
		}

		public override void Emit(EmitContext ec)
		{
		}

		public static void Error_AddressOfCapturedVar(ResolveContext rc, IVariableReference var, Location loc)
		{
			if (!(rc.CurrentAnonymousMethod is AsyncInitializer))
			{
				rc.Report.Error(1686, loc, "Local variable or parameter `{0}' cannot have their address taken and be used inside an anonymous method, lambda expression or query expression", var.Name);
			}
		}

		public override string GetSignatureForError()
		{
			return ExprClassName;
		}

		private AnonymousMethodBody CompatibleMethodBody(ResolveContext ec, TypeInferenceContext tic, TypeSpec return_type, TypeSpec delegate_type)
		{
			ParametersCompiled parametersCompiled = ResolveParameters(ec, tic, delegate_type);
			if (parametersCompiled == null)
			{
				return null;
			}
			ParametersBlock parametersBlock = (ec.IsInProbingMode ? ((ParametersBlock)Block.PerformClone()) : Block);
			if (parametersBlock.IsAsync)
			{
				if (return_type != null && return_type.Kind != MemberKind.Void && return_type != ec.Module.PredefinedTypes.Task.TypeSpec && !return_type.IsGenericTask)
				{
					ec.Report.Error(4010, loc, "Cannot convert async {0} to delegate type `{1}'", GetSignatureForError(), delegate_type.GetSignatureForError());
					return null;
				}
				parametersBlock = parametersBlock.ConvertToAsyncTask(ec, ec.CurrentMemberDefinition.Parent.PartialContainer, parametersCompiled, return_type, delegate_type, loc);
			}
			return CompatibleMethodFactory(return_type ?? InternalType.ErrorType, delegate_type, parametersCompiled, parametersBlock);
		}

		protected virtual AnonymousMethodBody CompatibleMethodFactory(TypeSpec return_type, TypeSpec delegate_type, ParametersCompiled p, ParametersBlock b)
		{
			return new AnonymousMethodBody(p, b, return_type, delegate_type, loc);
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			((AnonymousMethodExpression)t).Block = (ParametersBlock)clonectx.LookupBlock(Block);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
