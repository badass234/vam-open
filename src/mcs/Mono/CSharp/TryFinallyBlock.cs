using System.Reflection.Emit;

namespace Mono.CSharp
{
	public abstract class TryFinallyBlock : ExceptionStatement
	{
		protected Statement stmt;

		private Label dispose_try_block;

		private bool prepared_for_dispose;

		private bool emitted_dispose;

		private Method finally_host;

		public Statement Statement => stmt;

		protected TryFinallyBlock(Statement stmt, Location loc)
			: base(loc)
		{
			this.stmt = stmt;
		}

		protected abstract void EmitTryBody(EmitContext ec);

		public abstract void EmitFinallyBody(EmitContext ec);

		public override Label PrepareForDispose(EmitContext ec, Label end)
		{
			if (!prepared_for_dispose)
			{
				prepared_for_dispose = true;
				dispose_try_block = ec.DefineLabel();
			}
			return dispose_try_block;
		}

		protected sealed override void DoEmit(EmitContext ec)
		{
			EmitTryBodyPrepare(ec);
			EmitTryBody(ec);
			bool flag = EmitBeginFinallyBlock(ec);
			Label label = ec.DefineLabel();
			if ((resume_points != null) & flag)
			{
				StateMachineInitializer stateMachineInitializer = (StateMachineInitializer)ec.CurrentAnonymousMethod;
				ec.Emit(OpCodes.Ldloc, stateMachineInitializer.SkipFinally);
				ec.Emit(OpCodes.Brfalse_S, label);
				ec.Emit(OpCodes.Endfinally);
			}
			ec.MarkLabel(label);
			if (finally_host != null)
			{
				finally_host.Define();
				finally_host.PrepareEmit();
				finally_host.Emit();
				finally_host.Parent.AddMember(finally_host);
				CallEmitter callEmitter = default(CallEmitter);
				callEmitter.InstanceExpression = new CompilerGeneratedThis(ec.CurrentType, loc);
				callEmitter.EmitPredefined(ec, finally_host.Spec, new Arguments(0), true);
			}
			else
			{
				EmitFinallyBody(ec);
			}
			if (flag)
			{
				ec.EndExceptionBlock();
			}
		}

		public override void EmitForDispose(EmitContext ec, LocalBuilder pc, Label end, bool have_dispatcher)
		{
			if (emitted_dispose)
			{
				return;
			}
			emitted_dispose = true;
			Label label = ec.DefineLabel();
			if (have_dispatcher)
			{
				ec.Emit(OpCodes.Br, end);
			}
			ec.BeginExceptionBlock();
			ec.MarkLabel(dispose_try_block);
			Label[] array = null;
			for (int i = 0; i < resume_points.Count; i++)
			{
				Label label2 = resume_points[i].PrepareForDispose(ec, label);
				if (label2.Equals(label) && array == null)
				{
					continue;
				}
				if (array == null)
				{
					array = new Label[resume_points.Count];
					for (int j = 0; j < i; j++)
					{
						array[j] = label;
					}
				}
				array[i] = label2;
			}
			if (array != null)
			{
				int k;
				for (k = 1; k < array.Length && array[0].Equals(array[k]); k++)
				{
				}
				bool flag = k < array.Length;
				if (flag)
				{
					ec.Emit(OpCodes.Ldloc, pc);
					ec.EmitInt(first_resume_pc);
					ec.Emit(OpCodes.Sub);
					ec.Emit(OpCodes.Switch, array);
				}
				foreach (ResumableStatement resume_point in resume_points)
				{
					resume_point.EmitForDispose(ec, pc, label, flag);
				}
			}
			ec.MarkLabel(label);
			ec.BeginFinallyBlock();
			if (finally_host != null)
			{
				CallEmitter callEmitter = default(CallEmitter);
				callEmitter.InstanceExpression = new CompilerGeneratedThis(ec.CurrentType, loc);
				callEmitter.EmitPredefined(ec, finally_host.Spec, new Arguments(0), true);
			}
			else
			{
				EmitFinallyBody(ec);
			}
			ec.EndExceptionBlock();
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			bool result = stmt.FlowAnalysis(fc);
			parent = null;
			return result;
		}

		protected virtual bool EmitBeginFinallyBlock(EmitContext ec)
		{
			ec.BeginFinallyBlock();
			return true;
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			base.MarkReachable(rc);
			return Statement.MarkReachable(rc);
		}

		public override bool Resolve(BlockContext bc)
		{
			parent = bc.CurrentTryBlock;
			bc.CurrentTryBlock = this;
			bool flag;
			using (bc.Set(ResolveContext.Options.TryScope))
			{
				flag = stmt.Resolve(bc);
			}
			bc.CurrentTryBlock = parent;
			if (bc.CurrentIterator != null && !bc.IsInProbingMode)
			{
				Block block = stmt as Block;
				if (block != null && block.Explicit.HasYield)
				{
					finally_host = bc.CurrentIterator.CreateFinallyHost(this);
				}
			}
			return base.Resolve(bc) & flag;
		}
	}
}
