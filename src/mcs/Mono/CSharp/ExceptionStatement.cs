using System.Collections.Generic;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public abstract class ExceptionStatement : ResumableStatement
	{
		protected List<ResumableStatement> resume_points;

		protected int first_resume_pc;

		protected ExceptionStatement parent;

		protected ExceptionStatement(Location loc)
		{
			base.loc = loc;
		}

		protected virtual void EmitBeginException(EmitContext ec)
		{
			ec.BeginExceptionBlock();
		}

		protected virtual void EmitTryBodyPrepare(EmitContext ec)
		{
			StateMachineInitializer stateMachineInitializer = null;
			if (resume_points != null)
			{
				stateMachineInitializer = (StateMachineInitializer)ec.CurrentAnonymousMethod;
				ec.EmitInt(-3);
				ec.Emit(OpCodes.Stloc, stateMachineInitializer.CurrentPC);
			}
			EmitBeginException(ec);
			if (resume_points != null)
			{
				ec.MarkLabel(resume_point);
				ec.Emit(OpCodes.Ldloc, stateMachineInitializer.CurrentPC);
				ec.EmitInt(first_resume_pc);
				ec.Emit(OpCodes.Sub);
				Label[] array = new Label[resume_points.Count];
				for (int i = 0; i < resume_points.Count; i++)
				{
					array[i] = resume_points[i].PrepareForEmit(ec);
				}
				ec.Emit(OpCodes.Switch, array);
			}
		}

		public virtual int AddResumePoint(ResumableStatement stmt, int pc, StateMachineInitializer stateMachine)
		{
			if (parent != null)
			{
				TryCatch tryCatch = this as TryCatch;
				ResumableStatement stmt2 = ((tryCatch != null && tryCatch.IsTryCatchFinally) ? stmt : this);
				pc = parent.AddResumePoint(stmt2, pc, stateMachine);
			}
			else
			{
				pc = stateMachine.AddResumePoint(this);
			}
			if (resume_points == null)
			{
				resume_points = new List<ResumableStatement>();
				first_resume_pc = pc;
			}
			if (pc != first_resume_pc + resume_points.Count)
			{
				throw new InternalErrorException("missed an intervening AddResumePoint?");
			}
			resume_points.Add(stmt);
			return pc;
		}
	}
}
