using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public abstract class StateMachineInitializer : AnonymousExpression
	{
		private sealed class MoveNextBodyStatement : Statement
		{
			private readonly StateMachineInitializer state_machine;

			public MoveNextBodyStatement(StateMachineInitializer stateMachine)
			{
				state_machine = stateMachine;
				loc = stateMachine.Location;
			}

			protected override void CloneTo(CloneContext clonectx, Statement target)
			{
				throw new NotSupportedException();
			}

			public override bool Resolve(BlockContext ec)
			{
				return true;
			}

			protected override void DoEmit(EmitContext ec)
			{
				state_machine.EmitMoveNext(ec);
			}

			public override void Emit(EmitContext ec)
			{
				DoEmit(ec);
			}

			protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
			{
				return state_machine.ReturnType.Kind != MemberKind.Void;
			}

			public override Reachability MarkReachable(Reachability rc)
			{
				base.MarkReachable(rc);
				if (state_machine.ReturnType.Kind != MemberKind.Void)
				{
					rc = Reachability.CreateUnreachable();
				}
				return rc;
			}
		}

		public readonly TypeDefinition Host;

		protected StateMachine storey;

		protected Label move_next_ok;

		protected Label move_next_error;

		private LocalBuilder skip_finally;

		protected LocalBuilder current_pc;

		protected List<ResumableStatement> resume_points;

		public Label BodyEnd { get; set; }

		public LocalBuilder CurrentPC => current_pc;

		public LocalBuilder SkipFinally => skip_finally;

		public override AnonymousMethodStorey Storey => storey;

		protected StateMachineInitializer(ParametersBlock block, TypeDefinition host, TypeSpec returnType)
			: base(block, returnType, block.StartLocation)
		{
			Host = host;
		}

		public int AddResumePoint(ResumableStatement stmt)
		{
			if (resume_points == null)
			{
				resume_points = new List<ResumableStatement>();
			}
			resume_points.Add(stmt);
			return resume_points.Count;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			throw new NotSupportedException("ET");
		}

		protected virtual BlockContext CreateBlockContext(BlockContext bc)
		{
			return new BlockContext(bc, block, bc.ReturnType)
			{
				CurrentAnonymousMethod = this,
				AssignmentInfoOffset = bc.AssignmentInfoOffset,
				EnclosingLoop = bc.EnclosingLoop,
				EnclosingLoopOrSwitch = bc.EnclosingLoopOrSwitch,
				Switch = bc.Switch
			};
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			BlockContext blockContext = (BlockContext)rc;
			BlockContext blockContext2 = CreateBlockContext(blockContext);
			base.Block.Resolve(blockContext2);
			if (!rc.IsInProbingMode)
			{
				StateMachineMethod stateMachineMethod = new StateMachineMethod(storey, this, new TypeExpression(ReturnType, loc), Modifiers.PUBLIC, new MemberName("MoveNext", loc), (Block.Flags)0);
				stateMachineMethod.Block.AddStatement(new MoveNextBodyStatement(this));
				storey.AddEntryMethod(stateMachineMethod);
			}
			blockContext.AssignmentInfoOffset = blockContext2.AssignmentInfoOffset;
			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			storey.Instance.Emit(ec);
		}

		private void EmitMoveNext_NoResumePoints(EmitContext ec)
		{
			ec.EmitThis();
			ec.Emit(OpCodes.Ldfld, storey.PC.Spec);
			ec.EmitThis();
			ec.EmitInt(-1);
			ec.Emit(OpCodes.Stfld, storey.PC.Spec);
			ec.Emit(OpCodes.Brtrue, move_next_error);
			BodyEnd = ec.DefineLabel();
			AsyncInitializer asyncInitializer = this as AsyncInitializer;
			if (asyncInitializer != null)
			{
				ec.BeginExceptionBlock();
			}
			block.EmitEmbedded(ec);
			asyncInitializer?.EmitCatchBlock(ec);
			ec.MarkLabel(BodyEnd);
			EmitMoveNextEpilogue(ec);
			ec.MarkLabel(move_next_error);
			if (ReturnType.Kind != MemberKind.Void)
			{
				ec.EmitInt(0);
				ec.Emit(OpCodes.Ret);
			}
			ec.MarkLabel(move_next_ok);
		}

		private void EmitMoveNext(EmitContext ec)
		{
			move_next_ok = ec.DefineLabel();
			move_next_error = ec.DefineLabel();
			if (resume_points == null)
			{
				EmitMoveNext_NoResumePoints(ec);
				return;
			}
			current_pc = ec.GetTemporaryLocal(ec.BuiltinTypes.UInt);
			ec.EmitThis();
			ec.Emit(OpCodes.Ldfld, storey.PC.Spec);
			ec.Emit(OpCodes.Stloc, current_pc);
			ec.EmitThis();
			ec.EmitInt(-1);
			ec.Emit(OpCodes.Stfld, storey.PC.Spec);
			Label[] array = new Label[1 + resume_points.Count];
			array[0] = ec.DefineLabel();
			bool flag = false;
			for (int i = 0; i < resume_points.Count; i++)
			{
				ResumableStatement resumableStatement = resume_points[i];
				flag |= resumableStatement is ExceptionStatement;
				array[i + 1] = resumableStatement.PrepareForEmit(ec);
			}
			if (flag)
			{
				skip_finally = ec.GetTemporaryLocal(ec.BuiltinTypes.Bool);
				ec.EmitInt(0);
				ec.Emit(OpCodes.Stloc, skip_finally);
			}
			AsyncInitializer asyncInitializer = this as AsyncInitializer;
			if (asyncInitializer != null)
			{
				ec.BeginExceptionBlock();
			}
			ec.Emit(OpCodes.Ldloc, current_pc);
			ec.Emit(OpCodes.Switch, array);
			ec.Emit((asyncInitializer != null) ? OpCodes.Leave : OpCodes.Br, move_next_error);
			ec.MarkLabel(array[0]);
			BodyEnd = ec.DefineLabel();
			block.EmitEmbedded(ec);
			ec.MarkLabel(BodyEnd);
			asyncInitializer?.EmitCatchBlock(ec);
			ec.Mark(base.Block.Original.EndLocation);
			ec.EmitThis();
			ec.EmitInt(-1);
			ec.Emit(OpCodes.Stfld, storey.PC.Spec);
			EmitMoveNextEpilogue(ec);
			ec.MarkLabel(move_next_error);
			if (ReturnType.Kind != MemberKind.Void)
			{
				ec.EmitInt(0);
				ec.Emit(OpCodes.Ret);
			}
			ec.MarkLabel(move_next_ok);
			if (ReturnType.Kind != MemberKind.Void)
			{
				ec.EmitInt(1);
				ec.Emit(OpCodes.Ret);
			}
		}

		protected virtual void EmitMoveNextEpilogue(EmitContext ec)
		{
		}

		public void EmitLeave(EmitContext ec, bool unwind_protect)
		{
			ec.Emit(unwind_protect ? OpCodes.Leave : OpCodes.Br, move_next_ok);
		}

		public virtual void InjectYield(EmitContext ec, Expression expr, int resume_pc, bool unwind_protect, Label resume_point)
		{
			Label label = ec.DefineLabel();
			IteratorStorey iteratorStorey = storey as IteratorStorey;
			if (iteratorStorey != null)
			{
				ec.EmitThis();
				ec.Emit(OpCodes.Ldfld, iteratorStorey.DisposingField.Spec);
				ec.Emit(OpCodes.Brtrue_S, label);
			}
			ec.EmitThis();
			ec.EmitInt(resume_pc);
			ec.Emit(OpCodes.Stfld, storey.PC.Spec);
			if (iteratorStorey != null)
			{
				ec.MarkLabel(label);
			}
			if (unwind_protect && skip_finally != null)
			{
				ec.EmitInt(1);
				ec.Emit(OpCodes.Stloc, skip_finally);
			}
		}

		public void SetStateMachine(StateMachine stateMachine)
		{
			storey = stateMachine;
		}
	}
}
