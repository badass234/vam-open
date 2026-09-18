using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class LabeledStatement : Statement
	{
		private string name;

		private bool defined;

		private bool referenced;

		private bool finalTarget;

		private Label label;

		private Block block;

		public Block Block => block;

		public string Name => name;

		public LabeledStatement(string name, Block block, Location l)
		{
			this.name = name;
			this.block = block;
			loc = l;
		}

		public Label LabelTarget(EmitContext ec)
		{
			if (defined)
			{
				return label;
			}
			label = ec.DefineLabel();
			defined = true;
			return label;
		}

		protected override void CloneTo(CloneContext clonectx, Statement target)
		{
			((LabeledStatement)target).block = clonectx.RemapBlockCopy(block);
		}

		public override bool Resolve(BlockContext bc)
		{
			return true;
		}

		protected override void DoEmit(EmitContext ec)
		{
			LabelTarget(ec);
			ec.MarkLabel(label);
			if (finalTarget)
			{
				ec.Emit(OpCodes.Br_S, label);
			}
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			if (!referenced)
			{
				fc.Report.Warning(164, 2, loc, "This label has not been referenced");
			}
			return false;
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			base.MarkReachable(rc);
			if (referenced)
			{
				rc = default(Reachability);
			}
			return rc;
		}

		public void AddGotoReference(Reachability rc, bool finalTarget)
		{
			if (!referenced)
			{
				referenced = true;
				MarkReachable(rc);
				if (finalTarget)
				{
					this.finalTarget = true;
				}
				else
				{
					block.ScanGotoJump(this);
				}
			}
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
