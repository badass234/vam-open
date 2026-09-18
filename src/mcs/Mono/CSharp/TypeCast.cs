using System.Linq.Expressions;

namespace Mono.CSharp
{
	public abstract class TypeCast : Expression
	{
		protected readonly Expression child;

		public Expression Child => child;

		public override bool IsNull => child.IsNull;

		protected TypeCast(Expression child, TypeSpec return_type)
		{
			eclass = child.eclass;
			loc = child.Location;
			type = return_type;
			this.child = child;
		}

		public override bool ContainsEmitWithAwait()
		{
			return child.ContainsEmitWithAwait();
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			Arguments arguments = new Arguments(2);
			arguments.Add(new Argument(child.CreateExpressionTree(ec)));
			arguments.Add(new Argument(new TypeOf(type, loc)));
			if (type.IsPointer || child.Type.IsPointer)
			{
				Error_PointerInsideExpressionTree(ec);
			}
			return CreateExpressionFactoryCall(ec, ec.HasSet(ResolveContext.Options.CheckedScope) ? "ConvertChecked" : "Convert", arguments);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			child.Emit(ec);
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			child.FlowAnalysis(fc);
		}

		public override System.Linq.Expressions.Expression MakeExpression(BuilderContext ctx)
		{
			if (!ctx.HasSet(BuilderContext.Options.CheckedScope))
			{
				return System.Linq.Expressions.Expression.Convert(child.MakeExpression(ctx), type.GetMetaInfo());
			}
			return System.Linq.Expressions.Expression.ConvertChecked(child.MakeExpression(ctx), type.GetMetaInfo());
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
		}
	}
}
