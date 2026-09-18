using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class Do : LoopStatement
	{
		public Expression expr;

		private bool iterator_reachable;

		private bool end_reachable;

		public Location WhileLocation { get; private set; }

		public Do(Statement statement, BooleanExpression bool_expr, Location doLocation, Location whileLocation)
			: base(statement)
		{
			expr = bool_expr;
			loc = doLocation;
			WhileLocation = whileLocation;
		}

		public override bool Resolve(BlockContext bc)
		{
			bool result = base.Resolve(bc);
			expr = expr.Resolve(bc);
			return result;
		}

		protected override void DoEmit(EmitContext ec)
		{
			Label label = ec.DefineLabel();
			Label loopBegin = ec.LoopBegin;
			Label loopEnd = ec.LoopEnd;
			ec.LoopBegin = ec.DefineLabel();
			ec.LoopEnd = ec.DefineLabel();
			ec.MarkLabel(label);
			base.Statement.Emit(ec);
			ec.MarkLabel(ec.LoopBegin);
			ec.Mark(WhileLocation);
			if (expr is Constant)
			{
				bool num = !((Constant)expr).IsDefaultValue;
				expr.EmitSideEffect(ec);
				if (num)
				{
					ec.Emit(OpCodes.Br, label);
				}
			}
			else
			{
				expr.EmitBranchable(ec, label, true);
			}
			ec.MarkLabel(ec.LoopEnd);
			ec.LoopBegin = loopBegin;
			ec.LoopEnd = loopEnd;
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			bool num = base.Statement.FlowAnalysis(fc);
			expr.FlowAnalysisConditional(fc);
			fc.DefiniteAssignment = fc.DefiniteAssignmentOnFalse;
			if (num && !iterator_reachable)
			{
				return !end_reachable;
			}
			if (!end_reachable)
			{
				Constant constant = expr as Constant;
				if (constant != null && !constant.IsDefaultValue)
				{
					return true;
				}
			}
			return false;
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			base.MarkReachable(rc);
			if (base.Statement.MarkReachable(rc).IsUnreachable && !iterator_reachable)
			{
				expr = new UnreachableExpression(expr);
				if (!end_reachable)
				{
					return Reachability.CreateUnreachable();
				}
				return rc;
			}
			if (!end_reachable)
			{
				Constant constant = expr as Constant;
				if (constant != null && !constant.IsDefaultValue)
				{
					return Reachability.CreateUnreachable();
				}
			}
			return rc;
		}

		protected override void CloneTo(CloneContext clonectx, Statement t)
		{
			Do obj = (Do)t;
			obj.Statement = base.Statement.Clone(clonectx);
			obj.expr = expr.Clone(clonectx);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}

		public override void SetEndReachable()
		{
			end_reachable = true;
		}

		public override void SetIteratorReachable()
		{
			iterator_reachable = true;
		}
	}
}
