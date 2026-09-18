using System;

namespace Mono.CSharp
{
	public class YieldBreak : ExitStatement
	{
		private Iterator iterator;

		protected override bool IsLocalExit => false;

		public YieldBreak(Location l)
		{
			loc = l;
		}

		protected override void CloneTo(CloneContext clonectx, Statement target)
		{
			throw new NotSupportedException();
		}

		protected override bool DoResolve(BlockContext bc)
		{
			iterator = bc.CurrentIterator;
			return Yield.CheckContext(bc, loc);
		}

		protected override void DoEmit(EmitContext ec)
		{
			iterator.EmitYieldBreak(ec, unwind_protect);
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			return true;
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			base.MarkReachable(rc);
			return Reachability.CreateUnreachable();
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
