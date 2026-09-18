namespace Mono.CSharp.Linq
{
	public abstract class AQueryClause : ShimExpression
	{
		protected class QueryExpressionAccess : MemberAccess
		{
			public QueryExpressionAccess(Expression expr, string methodName, Location loc)
				: base(expr, methodName, loc)
			{
			}

			public QueryExpressionAccess(Expression expr, string methodName, TypeArguments typeArguments, Location loc)
				: base(expr, methodName, typeArguments, loc)
			{
			}

			public override void Error_TypeDoesNotContainDefinition(ResolveContext ec, TypeSpec type, string name)
			{
				ec.Report.Error(1935, loc, "An implementation of `{0}' query expression pattern could not be found. Are you missing `System.Linq' using directive or `System.Core.dll' assembly reference?", name);
			}
		}

		protected class QueryExpressionInvocation : Invocation, OverloadResolver.IErrorHandler
		{
			public QueryExpressionInvocation(QueryExpressionAccess expr, Arguments arguments)
				: base(expr, arguments)
			{
			}

			protected override MethodGroupExpr DoResolveOverload(ResolveContext ec)
			{
				return mg.OverloadResolve(ec, ref arguments, this, OverloadResolver.Restrictions.None);
			}

			protected override Expression DoResolveDynamic(ResolveContext ec, Expression memberExpr)
			{
				ec.Report.Error(1979, loc, "Query expressions with a source or join sequence of type `dynamic' are not allowed");
				return null;
			}

			bool OverloadResolver.IErrorHandler.AmbiguousCandidates(ResolveContext ec, MemberSpec best, MemberSpec ambiguous)
			{
				ec.Report.SymbolRelatedToPreviousError(best);
				ec.Report.SymbolRelatedToPreviousError(ambiguous);
				ec.Report.Error(1940, loc, "Ambiguous implementation of the query pattern `{0}' for source type `{1}'", best.Name, mg.InstanceExpression.GetSignatureForError());
				return true;
			}

			bool OverloadResolver.IErrorHandler.ArgumentMismatch(ResolveContext rc, MemberSpec best, Argument arg, int index)
			{
				return false;
			}

			bool OverloadResolver.IErrorHandler.NoArgumentMatch(ResolveContext rc, MemberSpec best)
			{
				return false;
			}

			bool OverloadResolver.IErrorHandler.TypeInferenceFailed(ResolveContext rc, MemberSpec best)
			{
				TypeSpec typeSpec = ((MethodSpec)best).Parameters.ExtensionMethodType;
				if (typeSpec != null)
				{
					Argument argument = arguments[0];
					if (TypeManager.IsGenericType(typeSpec) && InflatedTypeSpec.ContainsTypeParameter(typeSpec))
					{
						TypeInferenceContext typeInferenceContext = new TypeInferenceContext(typeSpec.TypeArguments);
						typeInferenceContext.OutputTypeInference(rc, argument.Expr, typeSpec);
						if (typeInferenceContext.FixAllTypes(rc))
						{
							typeSpec = typeSpec.GetDefinition().MakeGenericType(rc, typeInferenceContext.InferredTypeArguments);
						}
					}
					if (!Convert.ImplicitConversionExists(rc, argument.Expr, typeSpec))
					{
						rc.Report.Error(1936, loc, "An implementation of `{0}' query expression pattern for source type `{1}' could not be found", best.Name, argument.Type.GetSignatureForError());
						return true;
					}
				}
				if (best.Name == "SelectMany")
				{
					rc.Report.Error(1943, loc, "An expression type is incorrect in a subsequent `from' clause in a query expression with source type `{0}'", arguments[0].GetSignatureForError());
				}
				else
				{
					rc.Report.Error(1942, loc, "An expression type in `{0}' clause is incorrect. Type inference failed in the call to `{1}'", best.Name.ToLowerInvariant(), best.Name);
				}
				return true;
			}
		}

		public AQueryClause next;

		public QueryBlock block;

		protected abstract string MethodName { get; }

		public AQueryClause Next
		{
			set
			{
				next = value;
			}
		}

		public AQueryClause Tail
		{
			get
			{
				if (next != null)
				{
					return next.Tail;
				}
				return this;
			}
		}

		protected AQueryClause(QueryBlock block, Expression expr, Location loc)
			: base(expr)
		{
			this.block = block;
			base.loc = loc;
		}

		protected override void CloneTo(CloneContext clonectx, Expression target)
		{
			base.CloneTo(clonectx, target);
			AQueryClause aQueryClause = (AQueryClause)target;
			if (block != null)
			{
				aQueryClause.block = (QueryBlock)clonectx.LookupBlock(block);
			}
			if (next != null)
			{
				aQueryClause.next = (AQueryClause)next.Clone(clonectx);
			}
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			return expr.Resolve(ec);
		}

		public virtual Expression BuildQueryClause(ResolveContext ec, Expression lSide, Parameter parameter)
		{
			Arguments args = null;
			CreateArguments(ec, parameter, ref args);
			lSide = CreateQueryExpression(lSide, args);
			if (next != null)
			{
				parameter = CreateChildrenParameters(parameter);
				Select obj = next as Select;
				if (obj == null || obj.IsRequired(parameter))
				{
					return next.BuildQueryClause(ec, lSide, parameter);
				}
				if (next.next != null)
				{
					return next.next.BuildQueryClause(ec, lSide, parameter);
				}
			}
			return lSide;
		}

		protected virtual Parameter CreateChildrenParameters(Parameter parameter)
		{
			return parameter.Clone();
		}

		protected virtual void CreateArguments(ResolveContext ec, Parameter parameter, ref Arguments args)
		{
			args = new Arguments(2);
			LambdaExpression lambdaExpression = new LambdaExpression(loc);
			block.SetParameter(parameter);
			lambdaExpression.Block = block;
			lambdaExpression.Block.AddStatement(new ContextualReturn(expr));
			args.Add(new Argument(lambdaExpression));
		}

		protected Invocation CreateQueryExpression(Expression lSide, Arguments arguments)
		{
			return new QueryExpressionInvocation(new QueryExpressionAccess(lSide, MethodName, loc), arguments);
		}
	}
}
