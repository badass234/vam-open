namespace Mono.CSharp
{
	public class Unsafe : Statement
	{
		public Block Block;

		public Unsafe(Block b, Location loc)
		{
			Block = b;
			Block.Unsafe = true;
			base.loc = loc;
		}

		public override bool Resolve(BlockContext ec)
		{
			if (ec.CurrentIterator != null)
			{
				ec.Report.Error(1629, loc, "Unsafe code may not appear in iterators");
			}
			using (ec.Set(ResolveContext.Options.UnsafeScope))
			{
				return Block.Resolve(ec);
			}
		}

		protected override void DoEmit(EmitContext ec)
		{
			Block.Emit(ec);
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
			((Unsafe)t).Block = clonectx.LookupBlock(Block);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
