namespace Mono.CSharp.Linq
{
	public class GroupBy : AQueryClause
	{
		private Expression element_selector;

		private QueryBlock element_block;

		public Expression SelectorExpression => element_selector;

		protected override string MethodName => "GroupBy";

		public GroupBy(QueryBlock block, Expression elementSelector, QueryBlock elementBlock, Expression keySelector, Location loc)
			: base(block, keySelector, loc)
		{
			if (!elementSelector.Equals(keySelector))
			{
				element_selector = elementSelector;
				element_block = elementBlock;
			}
		}

		protected override void CreateArguments(ResolveContext ec, Parameter parameter, ref Arguments args)
		{
			base.CreateArguments(ec, parameter, ref args);
			if (element_selector != null)
			{
				LambdaExpression lambdaExpression = new LambdaExpression(element_selector.Location);
				element_block.SetParameter(parameter.Clone());
				lambdaExpression.Block = element_block;
				lambdaExpression.Block.AddStatement(new ContextualReturn(element_selector));
				args.Add(new Argument(lambdaExpression));
			}
		}

		protected override void CloneTo(CloneContext clonectx, Expression target)
		{
			GroupBy groupBy = (GroupBy)target;
			if (element_selector != null)
			{
				groupBy.element_selector = element_selector.Clone(clonectx);
				groupBy.element_block = (QueryBlock)element_block.Clone(clonectx);
			}
			base.CloneTo(clonectx, (Expression)groupBy);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
