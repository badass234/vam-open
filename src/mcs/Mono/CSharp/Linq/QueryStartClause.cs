using System;

namespace Mono.CSharp.Linq
{
	public class QueryStartClause : ARangeVariableQueryClause
	{
		protected override string MethodName
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public QueryStartClause(QueryBlock block, Expression expr, RangeVariable identifier, Location loc)
			: base(block, identifier, expr, loc)
		{
			block.AddRangeVariable(identifier);
		}

		public override Expression BuildQueryClause(ResolveContext ec, Expression lSide, Parameter parameter)
		{
			if (base.IdentifierType != null)
			{
				expr = CreateCastExpression(expr);
			}
			if (parameter == null)
			{
				lSide = expr;
			}
			return next.BuildQueryClause(ec, lSide, new ImplicitLambdaParameter(identifier.Name, identifier.Location));
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			return BuildQueryClause(ec, null, null).Resolve(ec);
		}
	}
}
