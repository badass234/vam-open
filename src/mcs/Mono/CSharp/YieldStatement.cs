namespace Mono.CSharp
{
	public abstract class YieldStatement<T> : ResumableStatement where T : StateMachineInitializer
	{
		protected Expression expr;

		protected bool unwind_protect;

		protected T machine_initializer;

		private int resume_pc;

		private ExceptionStatement inside_try_block;

		public Expression Expr => expr;

		protected YieldStatement(Expression expr, Location l)
		{
			this.expr = expr;
			loc = l;
		}

		protected override void CloneTo(CloneContext clonectx, Statement t)
		{
			((YieldStatement<T>)t).expr = expr.Clone(clonectx);
		}

		protected override void DoEmit(EmitContext ec)
		{
			machine_initializer.InjectYield(ec, expr, resume_pc, unwind_protect, resume_point);
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			expr.FlowAnalysis(fc);
			RegisterResumePoint();
			return false;
		}

		public override bool Resolve(BlockContext bc)
		{
			expr = expr.Resolve(bc);
			if (expr == null)
			{
				return false;
			}
			machine_initializer = bc.CurrentAnonymousMethod as T;
			inside_try_block = bc.CurrentTryBlock;
			return true;
		}

		public void RegisterResumePoint()
		{
			if (resume_pc == 0)
			{
				if (inside_try_block == null)
				{
					resume_pc = machine_initializer.AddResumePoint(this);
					return;
				}
				resume_pc = inside_try_block.AddResumePoint(this, resume_pc, machine_initializer);
				unwind_protect = true;
				inside_try_block = null;
			}
		}
	}
}
