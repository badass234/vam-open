namespace Mono.CSharp
{
	internal class LambdaMethod : AnonymousMethodBody
	{
		public override string ContainerType => "lambda expression";

		public LambdaMethod(ParametersCompiled parameters, ParametersBlock block, TypeSpec return_type, TypeSpec delegate_type, Location loc)
			: base(parameters, block, return_type, delegate_type, loc)
		{
		}

		protected override void CloneTo(CloneContext clonectx, Expression target)
		{
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			BlockContext ec2 = new BlockContext(ec.MemberContext, base.Block, ReturnType);
			Expression expr = parameters.CreateExpressionTree(ec2, loc);
			Expression expression = base.Block.CreateExpressionTree(ec);
			if (expression == null)
			{
				return null;
			}
			Arguments arguments = new Arguments(2);
			arguments.Add(new Argument(expression));
			arguments.Add(new Argument(expr));
			return CreateExpressionFactoryCall(ec, "Lambda", new TypeArguments(new TypeExpression(type, loc)), arguments);
		}
	}
}
