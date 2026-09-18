using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class DefaultValueExpression : Expression
	{
		private Expression expr;

		public Expression Expr => expr;

		public override bool IsSideEffectFree => true;

		public DefaultValueExpression(Expression expr, Location loc)
		{
			this.expr = expr;
			base.loc = loc;
		}

		public override bool ContainsEmitWithAwait()
		{
			return false;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			Arguments arguments = new Arguments(2);
			arguments.Add(new Argument(this));
			arguments.Add(new Argument(new TypeOf(type, loc)));
			return CreateExpressionFactoryCall(ec, "Constant", arguments);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			type = expr.ResolveAsType(ec);
			if (type == null)
			{
				return null;
			}
			if (type.IsStatic)
			{
				ec.Report.Error(-244, loc, "The `default value' operator cannot be applied to an operand of a static type");
			}
			if (type.IsPointer)
			{
				return new NullLiteral(base.Location).ConvertImplicitly(type);
			}
			if (TypeSpec.IsReferenceType(type))
			{
				return new NullConstant(type, loc);
			}
			Constant constant = New.Constantify(type, expr.Location);
			if (constant != null)
			{
				return constant;
			}
			eclass = ExprClass.Variable;
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			LocalTemporary localTemporary = new LocalTemporary(type);
			localTemporary.AddressOf(ec, AddressOp.LoadStore);
			ec.Emit(OpCodes.Initobj, type);
			localTemporary.Emit(ec);
			localTemporary.Release(ec);
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			((DefaultValueExpression)t).expr = expr.Clone(clonectx);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
