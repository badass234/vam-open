namespace Mono.CSharp
{
	public abstract class LoopStatement : Statement
	{
		public Statement Statement { get; set; }

		protected LoopStatement(Statement statement)
		{
			Statement = statement;
		}

		public override bool Resolve(BlockContext bc)
		{
			LoopStatement enclosingLoop = bc.EnclosingLoop;
			LoopStatement enclosingLoopOrSwitch = bc.EnclosingLoopOrSwitch;
			LoopStatement enclosingLoopOrSwitch2 = (bc.EnclosingLoop = this);
			bc.EnclosingLoopOrSwitch = enclosingLoopOrSwitch2;
			bool result = Statement.Resolve(bc);
			bc.EnclosingLoopOrSwitch = enclosingLoopOrSwitch;
			bc.EnclosingLoop = enclosingLoop;
			return result;
		}

		public virtual void AddEndDefiniteAssignment(FlowAnalysisContext fc)
		{
		}

		public virtual void SetEndReachable()
		{
		}

		public virtual void SetIteratorReachable()
		{
		}
	}
}
