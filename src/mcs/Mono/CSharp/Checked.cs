namespace Mono.CSharp
{
	public class Checked : Statement
	{
		public Block Block;

		public Checked(Block b, Location loc)
		{
			Block = b;
			b.Unchecked = false;
			base.loc = loc;
		}

		public override bool Resolve(BlockContext ec)
		{
			using (ec.With(ResolveContext.Options.AllCheckStateFlags, true))
			{
				return Block.Resolve(ec);
			}
		}

		protected override void DoEmit(EmitContext ec)
		{
			using (ec.With(BuilderContext.Options.CheckedScope, true))
			{
				Block.Emit(ec);
			}
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			return Block.FlowAnalysis(fc);
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			base.MarkReachable(rc);
			return Block.MarkReachable(rc);
		}

		protected override void CloneTo(CloneContext clonectx, Statement t)
		{
			((Checked)t).Block = clonectx.LookupBlock(Block);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
