using System.Collections.Generic;

namespace Mono.CSharp
{
	internal class CollectionElementInitializer : Invocation
	{
		public class ElementInitializerArgument : Argument
		{
			public ElementInitializerArgument(Expression e)
				: base(e)
			{
			}
		}

		private sealed class AddMemberAccess : MemberAccess
		{
			public AddMemberAccess(Expression expr, Location loc)
				: base(expr, "Add", loc)
			{
			}

			public override void Error_TypeDoesNotContainDefinition(ResolveContext ec, TypeSpec type, string name)
			{
				if (!TypeManager.HasElementType(type))
				{
					base.Error_TypeDoesNotContainDefinition(ec, type, name);
				}
			}
		}

		public CollectionElementInitializer(Expression argument)
			: base(null, new Arguments(1))
		{
			arguments.Add(new ElementInitializerArgument(argument));
			loc = argument.Location;
		}

		public CollectionElementInitializer(List<Expression> arguments, Location loc)
			: base(null, new Arguments(arguments.Count))
		{
			foreach (Expression argument in arguments)
			{
				base.arguments.Add(new ElementInitializerArgument(argument));
			}
			base.loc = loc;
		}

		public CollectionElementInitializer(Location loc)
			: base(null, null)
		{
			base.loc = loc;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			Arguments arguments = new Arguments(2);
			arguments.Add(new Argument(mg.CreateExpressionTree(ec)));
			ArrayInitializer arrayInitializer = new ArrayInitializer(base.arguments.Count, loc);
			foreach (Argument argument in base.arguments)
			{
				arrayInitializer.Add(argument.CreateExpressionTree(ec));
			}
			arguments.Add(new Argument(new ArrayCreation(Expression.CreateExpressionTypeExpression(ec, loc), arrayInitializer, loc)));
			return CreateExpressionFactoryCall(ec, "ElementInit", arguments);
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			CollectionElementInitializer collectionElementInitializer = (CollectionElementInitializer)t;
			if (arguments != null)
			{
				collectionElementInitializer.arguments = arguments.Clone(clonectx);
			}
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			expr = new AddMemberAccess(ec.CurrentInitializerVariable, loc);
			return base.DoResolve(ec);
		}
	}
}
