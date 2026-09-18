using System.Collections.Generic;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class While : LoopStatement
	{
		public Expression expr;

		private bool empty;

		private bool infinite;

		private bool end_reachable;

		private List<DefiniteAssignmentBitSet> end_reachable_das;

		public While(BooleanExpression bool_expr, Statement statement, Location l)
			: base(statement)
		{
			expr = bool_expr;
			loc = l;
		}

		public override bool Resolve(BlockContext bc)
		{
			bool flag = true;
			expr = expr.Resolve(bc);
			if (expr == null)
			{
				flag = false;
			}
			Constant constant = expr as Constant;
			if (constant != null)
			{
				empty = constant.IsDefaultValue;
				infinite = !empty;
			}
			return flag & base.Resolve(bc);
		}

		protected override void DoEmit(EmitContext ec)
		{
			if (empty)
			{
				expr.EmitSideEffect(ec);
				return;
			}
			Label loopBegin = ec.LoopBegin;
			Label loopEnd = ec.LoopEnd;
			ec.LoopBegin = ec.DefineLabel();
			ec.LoopEnd = ec.DefineLabel();
			if (expr is Constant)
			{
				ec.MarkLabel(ec.LoopBegin);
				if (ec.EmitAccurateDebugInfo)
				{
					ec.Emit(OpCodes.Nop);
				}
				expr.EmitSideEffect(ec);
				base.Statement.Emit(ec);
				ec.Emit(OpCodes.Br, ec.LoopBegin);
				ec.MarkLabel(ec.LoopEnd);
			}
			else
			{
				Label label = ec.DefineLabel();
				ec.Emit(OpCodes.Br, ec.LoopBegin);
				ec.MarkLabel(label);
				base.Statement.Emit(ec);
				ec.MarkLabel(ec.LoopBegin);
				ec.Mark(loc);
				expr.EmitBranchable(ec, label, true);
				ec.MarkLabel(ec.LoopEnd);
			}
			ec.LoopBegin = loopBegin;
			ec.LoopEnd = loopEnd;
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			expr.FlowAnalysisConditional(fc);
			fc.DefiniteAssignment = fc.DefiniteAssignmentOnTrue;
			DefiniteAssignmentBitSet definiteAssignment = new DefiniteAssignmentBitSet(fc.DefiniteAssignmentOnFalse);
			base.Statement.FlowAnalysis(fc);
			if (end_reachable_das != null)
			{
				definiteAssignment = DefiniteAssignmentBitSet.And(end_reachable_das);
				end_reachable_das = null;
			}
			fc.DefiniteAssignment = definiteAssignment;
			if (infinite && !end_reachable)
			{
				return true;
			}
			return false;
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			if (rc.IsUnreachable)
			{
				return rc;
			}
			base.MarkReachable(rc);
			if (empty)
			{
				base.Statement.MarkReachable(Reachability.CreateUnreachable());
				return rc;
			}
			base.Statement.MarkReachable(rc);
			if (infinite && !end_reachable)
			{
				return Reachability.CreateUnreachable();
			}
			return rc;
		}

		protected override void CloneTo(CloneContext clonectx, Statement t)
		{
			While obj = (While)t;
			obj.expr = expr.Clone(clonectx);
			obj.Statement = base.Statement.Clone(clonectx);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}

		public override void AddEndDefiniteAssignment(FlowAnalysisContext fc)
		{
			if (infinite)
			{
				if (end_reachable_das == null)
				{
					end_reachable_das = new List<DefiniteAssignmentBitSet>();
				}
				end_reachable_das.Add(fc.DefiniteAssignment);
			}
		}

		public override void SetEndReachable()
		{
			end_reachable = true;
		}
	}
}
