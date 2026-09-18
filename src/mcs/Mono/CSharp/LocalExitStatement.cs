namespace Mono.CSharp
{
	public abstract class LocalExitStatement : ExitStatement
	{
		protected LoopStatement enclosing_loop;

		protected override bool IsLocalExit => true;

		protected LocalExitStatement(Location loc)
		{
			base.loc = loc;
		}

		protected override void CloneTo(CloneContext clonectx, Statement t)
		{
		}

		protected override bool DoResolve(BlockContext bc)
		{
			if (enclosing_loop == null)
			{
				bc.Report.Error(139, loc, "No enclosing loop out of which to break or continue");
				return false;
			}
			Block block = enclosing_loop.Statement as Block;
			if (block != null)
			{
				CheckExitBoundaries(bc, block);
			}
			return true;
		}
	}
}
