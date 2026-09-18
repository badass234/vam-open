namespace Mono.CSharp.Linq
{
	public class Join : SelectMany
	{
		private QueryBlock inner_selector;

		private QueryBlock outer_selector;

		public QueryBlock InnerSelector => inner_selector;

		public QueryBlock OuterSelector => outer_selector;

		protected override string MethodName => "Join";

		public Join(QueryBlock block, RangeVariable lt, Expression inner, QueryBlock outerSelector, QueryBlock innerSelector, Location loc)
			: base(block, lt, inner, loc)
		{
			outer_selector = outerSelector;
			inner_selector = innerSelector;
		}

		protected override void CreateArguments(ResolveContext ec, Parameter parameter, ref Arguments args)
		{
			args = new Arguments(4);
			if (base.IdentifierType != null)
			{
				expr = CreateCastExpression(expr);
			}
			args.Add(new Argument(expr));
			outer_selector.SetParameter(parameter.Clone());
			LambdaExpression lambdaExpression = new LambdaExpression(outer_selector.StartLocation);
			lambdaExpression.Block = outer_selector;
			args.Add(new Argument(lambdaExpression));
			inner_selector.SetParameter(new ImplicitLambdaParameter(identifier.Name, identifier.Location));
			lambdaExpression = new LambdaExpression(inner_selector.StartLocation);
			lambdaExpression.Block = inner_selector;
			args.Add(new Argument(lambdaExpression));
			base.CreateArguments(ec, parameter, ref args);
		}

		protected override void CloneTo(CloneContext clonectx, Expression target)
		{
			Join obj = (Join)target;
			obj.inner_selector = (QueryBlock)inner_selector.Clone(clonectx);
			obj.outer_selector = (QueryBlock)outer_selector.Clone(clonectx);
			base.CloneTo(clonectx, (Expression)obj);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
