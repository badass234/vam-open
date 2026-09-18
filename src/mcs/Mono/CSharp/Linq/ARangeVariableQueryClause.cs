using System.Collections.Generic;

namespace Mono.CSharp.Linq
{
	public abstract class ARangeVariableQueryClause : AQueryClause
	{
		private sealed class RangeAnonymousTypeParameter : AnonymousTypeParameter
		{
			public RangeAnonymousTypeParameter(Expression initializer, RangeVariable parameter)
				: base(initializer, parameter.Name, parameter.Location)
			{
			}

			protected override void Error_InvalidInitializer(ResolveContext ec, string initializer)
			{
				ec.Report.Error(1932, loc, "A range variable `{0}' cannot be initialized with `{1}'", Name, initializer);
			}
		}

		private class RangeParameterReference : ParameterReference
		{
			private Parameter parameter;

			public RangeParameterReference(Parameter p)
				: base(null, p.Location)
			{
				parameter = p;
			}

			protected override Expression DoResolve(ResolveContext ec)
			{
				pi = ec.CurrentBlock.ParametersBlock.GetParameterInfo(parameter);
				return base.DoResolve(ec);
			}
		}

		protected RangeVariable identifier;

		public RangeVariable Identifier => identifier;

		public FullNamedExpression IdentifierType { get; set; }

		protected ARangeVariableQueryClause(QueryBlock block, RangeVariable identifier, Expression expr, Location loc)
			: base(block, expr, loc)
		{
			this.identifier = identifier;
		}

		protected Invocation CreateCastExpression(Expression lSide)
		{
			return new QueryExpressionInvocation(new QueryExpressionAccess(lSide, "Cast", new TypeArguments(IdentifierType), loc), null);
		}

		protected override Parameter CreateChildrenParameters(Parameter parameter)
		{
			return new QueryBlock.TransparentParameter(parameter.Clone(), GetIntoVariable());
		}

		protected static Expression CreateRangeVariableType(ResolveContext rc, Parameter parameter, RangeVariable name, Expression init)
		{
			return new NewAnonymousType(new List<AnonymousTypeParameter>(2)
			{
				new AnonymousTypeParameter(new RangeParameterReference(parameter), parameter.Name, parameter.Location),
				new RangeAnonymousTypeParameter(init, name)
			}, rc.MemberContext.CurrentMemberDefinition.Parent, name.Location);
		}

		protected virtual RangeVariable GetIntoVariable()
		{
			return identifier;
		}
	}
}
