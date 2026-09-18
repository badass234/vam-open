using System;

namespace Mono.CSharp
{
	public sealed class EmptyStatement : Statement
	{
		public EmptyStatement(Location loc)
		{
			base.loc = loc;
		}

		public override bool Resolve(BlockContext ec)
		{
			return true;
		}

		public override void Emit(EmitContext ec)
		{
		}

		protected override void DoEmit(EmitContext ec)
		{
			throw new NotSupportedException();
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			return false;
		}

		protected override void CloneTo(CloneContext clonectx, Statement target)
		{
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
