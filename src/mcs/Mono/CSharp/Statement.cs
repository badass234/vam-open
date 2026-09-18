namespace Mono.CSharp
{
	public abstract class Statement
	{
		public Location loc;

		protected bool reachable;

		public bool IsUnreachable => !reachable;

		public virtual bool Resolve(BlockContext bc)
		{
			return true;
		}

		protected abstract void DoEmit(EmitContext ec);

		public virtual void Emit(EmitContext ec)
		{
			ec.Mark(loc);
			DoEmit(ec);
			if (ec.StatementEpilogue != null)
			{
				ec.EmitEpilogue();
			}
		}

		protected abstract void CloneTo(CloneContext clonectx, Statement target);

		public Statement Clone(CloneContext clonectx)
		{
			Statement statement = (Statement)MemberwiseClone();
			CloneTo(clonectx, statement);
			return statement;
		}

		public virtual Expression CreateExpressionTree(ResolveContext ec)
		{
			ec.Report.Error(834, loc, "A lambda expression with statement body cannot be converted to an expresion tree");
			return null;
		}

		public virtual object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}

		protected abstract bool DoFlowAnalysis(FlowAnalysisContext fc);

		public bool FlowAnalysis(FlowAnalysisContext fc)
		{
			if (reachable)
			{
				fc.UnreachableReported = false;
				return DoFlowAnalysis(fc);
			}
			if (this is Block)
			{
				return DoFlowAnalysis(fc);
			}
			if (this is EmptyStatement || loc.IsNull)
			{
				return true;
			}
			if (fc.UnreachableReported)
			{
				return true;
			}
			fc.Report.Warning(162, 2, loc, "Unreachable code detected");
			fc.UnreachableReported = true;
			return true;
		}

		public virtual Reachability MarkReachable(Reachability rc)
		{
			if (!rc.IsUnreachable)
			{
				reachable = true;
			}
			return rc;
		}

		protected void CheckExitBoundaries(BlockContext bc, Block scope)
		{
			if (bc.CurrentBlock.ParametersBlock.Original != scope.ParametersBlock.Original)
			{
				bc.Report.Error(1632, loc, "Control cannot leave the body of an anonymous method");
				return;
			}
			Block block = bc.CurrentBlock;
			while (block != null && block != scope)
			{
				if (block.IsFinallyBlock)
				{
					Error_FinallyClauseExit(bc);
					break;
				}
				block = block.Parent;
			}
		}

		protected void Error_FinallyClauseExit(BlockContext bc)
		{
			bc.Report.Error(157, loc, "Control cannot leave the body of a finally clause");
		}
	}
}
