namespace Mono.CSharp
{
	public abstract class ExitStatement : Statement
	{
		protected bool unwind_protect;

		protected abstract bool IsLocalExit { get; }

		protected abstract bool DoResolve(BlockContext bc);

		public override bool Resolve(BlockContext bc)
		{
			bool result = DoResolve(bc);
			if (!IsLocalExit && bc.HasSet(ResolveContext.Options.FinallyScope))
			{
				for (Block block = bc.CurrentBlock; block != null; block = block.Parent)
				{
					if (block.IsFinallyBlock)
					{
						Error_FinallyClauseExit(bc);
						break;
					}
					if (block is ParametersBlock)
					{
						break;
					}
				}
			}
			unwind_protect = bc.HasAny(ResolveContext.Options.CatchScope | ResolveContext.Options.TryScope);
			return result;
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			if (IsLocalExit)
			{
				return true;
			}
			if (fc.TryFinally != null)
			{
				fc.TryFinally.RegisterForControlExitCheck(new DefiniteAssignmentBitSet(fc.DefiniteAssignment));
			}
			else
			{
				fc.ParametersBlock.CheckControlExit(fc);
			}
			return true;
		}
	}
}
