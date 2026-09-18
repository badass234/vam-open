using System;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class ArglistAccess : Expression
	{
		public ArglistAccess(Location loc)
		{
			base.loc = loc;
		}

		protected override void CloneTo(CloneContext clonectx, Expression target)
		{
		}

		public override bool ContainsEmitWithAwait()
		{
			return false;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			throw new NotSupportedException("ET");
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			eclass = ExprClass.Variable;
			type = ec.Module.PredefinedTypes.RuntimeArgumentHandle.Resolve();
			if (ec.HasSet(ResolveContext.Options.FieldInitializerScope) || !ec.CurrentBlock.ParametersBlock.Parameters.HasArglist)
			{
				ec.Report.Error(190, loc, "The __arglist construct is valid only within a variable argument method");
			}
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			ec.Emit(OpCodes.Arglist);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
