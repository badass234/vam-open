using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class Goto : ExitStatement
	{
		private string target;

		private LabeledStatement label;

		private TryFinally try_finally;

		public string Target => target;

		protected override bool IsLocalExit => true;

		public Goto(string label, Location l)
		{
			loc = l;
			target = label;
		}

		protected override bool DoResolve(BlockContext bc)
		{
			label = bc.CurrentBlock.LookupLabel(target);
			if (label == null)
			{
				Error_UnknownLabel(bc, target, loc);
				return false;
			}
			try_finally = bc.CurrentTryBlock as TryFinally;
			CheckExitBoundaries(bc, label.Block);
			return true;
		}

		public static void Error_UnknownLabel(BlockContext bc, string label, Location loc)
		{
			bc.Report.Error(159, loc, "The label `{0}:' could not be found within the scope of the goto statement", label);
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			if (fc.AddReachedLabel(label))
			{
				return true;
			}
			label.Block.ScanGotoJump(label, fc);
			return true;
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			if (rc.IsUnreachable)
			{
				return rc;
			}
			base.MarkReachable(rc);
			if (try_finally != null)
			{
				if (try_finally.FinallyBlock.HasReachableClosingBrace)
				{
					label.AddGotoReference(rc, false);
				}
				else
				{
					label.AddGotoReference(rc, true);
				}
			}
			else
			{
				label.AddGotoReference(rc, false);
			}
			return Reachability.CreateUnreachable();
		}

		protected override void CloneTo(CloneContext clonectx, Statement target)
		{
		}

		protected override void DoEmit(EmitContext ec)
		{
			if (this.label == null)
			{
				throw new InternalErrorException("goto emitted before target resolved");
			}
			Label label = this.label.LabelTarget(ec);
			if (ec.TryFinallyUnwind != null && IsLeavingFinally(this.label.Block))
			{
				AsyncInitializer initializer = (AsyncInitializer)ec.CurrentAnonymousMethod;
				label = TryFinally.EmitRedirectedJump(ec, initializer, label, this.label.Block);
			}
			ec.Emit(unwind_protect ? OpCodes.Leave : OpCodes.Br, label);
		}

		private bool IsLeavingFinally(Block labelBlock)
		{
			for (Block block = try_finally.Statement as Block; block != null; block = block.Parent)
			{
				if (block == labelBlock)
				{
					return true;
				}
			}
			return false;
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
