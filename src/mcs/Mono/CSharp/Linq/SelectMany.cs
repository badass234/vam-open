namespace Mono.CSharp.Linq
{
	public class SelectMany : ARangeVariableQueryClause
	{
		protected override string MethodName => "SelectMany";

		public SelectMany(QueryBlock block, RangeVariable identifier, Expression expr, Location loc)
			: base(block, identifier, expr, loc)
		{
		}

		protected override void CreateArguments(ResolveContext ec, Parameter parameter, ref Arguments args)
		{
			if (args == null)
			{
				if (base.IdentifierType != null)
				{
					expr = CreateCastExpression(expr);
				}
				base.CreateArguments(ec, parameter.Clone(), ref args);
			}
			RangeVariable intoVariable = GetIntoVariable();
			ImplicitLambdaParameter second = new ImplicitLambdaParameter(intoVariable.Name, intoVariable.Location);
			Expression expression;
			QueryBlock queryBlock;
			if (next is Select)
			{
				expression = next.Expr;
				queryBlock = next.block;
				queryBlock.SetParameters(parameter, second);
				next = next.next;
			}
			else
			{
				expression = ARangeVariableQueryClause.CreateRangeVariableType(ec, parameter, intoVariable, new SimpleName(intoVariable.Name, intoVariable.Location));
				queryBlock = new QueryBlock(block.Parent, block.StartLocation);
				queryBlock.SetParameters(parameter, second);
			}
			LambdaExpression lambdaExpression = new LambdaExpression(base.Location);
			lambdaExpression.Block = queryBlock;
			lambdaExpression.Block.AddStatement(new ContextualReturn(expression));
			args.Add(new Argument(lambdaExpression));
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
