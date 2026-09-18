using System.Collections.Generic;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class TryFinally : TryFinallyBlock
	{
		private ExplicitBlock fini;

		private List<DefiniteAssignmentBitSet> try_exit_dat;

		private List<Label> redirected_jumps;

		private Label? start_fin_label;

		public ExplicitBlock FinallyBlock => fini;

		public TryFinally(Statement stmt, ExplicitBlock fini, Location loc)
			: base(stmt, loc)
		{
			this.fini = fini;
		}

		public void RegisterForControlExitCheck(DefiniteAssignmentBitSet vector)
		{
			if (try_exit_dat == null)
			{
				try_exit_dat = new List<DefiniteAssignmentBitSet>();
			}
			try_exit_dat.Add(vector);
		}

		public override bool Resolve(BlockContext bc)
		{
			bool flag = base.Resolve(bc);
			fini.SetFinallyBlock();
			using (bc.Set(ResolveContext.Options.FinallyScope))
			{
				return flag & fini.Resolve(bc);
			}
		}

		protected override void EmitBeginException(EmitContext ec)
		{
			if (fini.HasAwait && stmt is TryCatch)
			{
				ec.BeginExceptionBlock();
			}
			base.EmitBeginException(ec);
		}

		protected override void EmitTryBody(EmitContext ec)
		{
			if (fini.HasAwait)
			{
				if (ec.TryFinallyUnwind == null)
				{
					ec.TryFinallyUnwind = new List<TryFinally>();
				}
				ec.TryFinallyUnwind.Add(this);
				stmt.Emit(ec);
				if (stmt is TryCatch)
				{
					ec.EndExceptionBlock();
				}
				ec.TryFinallyUnwind.Remove(this);
				if (start_fin_label.HasValue)
				{
					ec.MarkLabel(start_fin_label.Value);
				}
			}
			else
			{
				stmt.Emit(ec);
			}
		}

		protected override bool EmitBeginFinallyBlock(EmitContext ec)
		{
			if (fini.HasAwait)
			{
				return false;
			}
			return base.EmitBeginFinallyBlock(ec);
		}

		public override void EmitFinallyBody(EmitContext ec)
		{
			if (!fini.HasAwait)
			{
				fini.Emit(ec);
				return;
			}
			BuiltinTypeSpec builtinTypeSpec = ec.BuiltinTypes.Object;
			ec.BeginCatchBlock(builtinTypeSpec);
			LocalBuilder temporaryLocal = ec.GetTemporaryLocal(builtinTypeSpec);
			ec.Emit(OpCodes.Stloc, temporaryLocal);
			StackFieldExpr temporaryField = ec.GetTemporaryField(builtinTypeSpec);
			ec.EmitThis();
			ec.Emit(OpCodes.Ldloc, temporaryLocal);
			temporaryField.EmitAssignFromStack(ec);
			ec.EndExceptionBlock();
			ec.FreeTemporaryLocal(temporaryLocal, builtinTypeSpec);
			fini.Emit(ec);
			temporaryField.Emit(ec);
			Label label = ec.DefineLabel();
			ec.Emit(OpCodes.Brfalse_S, label);
			temporaryField.Emit(ec);
			ec.Emit(OpCodes.Throw);
			ec.MarkLabel(label);
			temporaryField.IsAvailableForReuse = true;
			EmitUnwindFinallyTable(ec);
		}

		private bool IsParentBlock(Block block)
		{
			for (Block block2 = fini; block2 != null; block2 = block2.Parent)
			{
				if (block2 == block)
				{
					return true;
				}
			}
			return false;
		}

		public static Label EmitRedirectedJump(EmitContext ec, AsyncInitializer initializer, Label label, Block labelBlock)
		{
			int i;
			if (labelBlock != null)
			{
				i = ec.TryFinallyUnwind.Count;
				while (i != 0 && ec.TryFinallyUnwind[i - 1].IsParentBlock(labelBlock))
				{
					i--;
				}
			}
			else
			{
				i = 0;
			}
			bool setReturnState = true;
			for (; i < ec.TryFinallyUnwind.Count; i++)
			{
				TryFinally tryFinally = ec.TryFinallyUnwind[i];
				if (labelBlock != null && !tryFinally.IsParentBlock(labelBlock))
				{
					break;
				}
				tryFinally.EmitRedirectedExit(ec, label, initializer, setReturnState);
				setReturnState = false;
				if (!tryFinally.start_fin_label.HasValue)
				{
					tryFinally.start_fin_label = ec.DefineLabel();
				}
				label = tryFinally.start_fin_label.Value;
			}
			return label;
		}

		public static Label EmitRedirectedReturn(EmitContext ec, AsyncInitializer initializer)
		{
			return EmitRedirectedJump(ec, initializer, initializer.BodyEnd, null);
		}

		private void EmitRedirectedExit(EmitContext ec, Label label, AsyncInitializer initializer, bool setReturnState)
		{
			if (redirected_jumps == null)
			{
				redirected_jumps = new List<Label>();
				redirected_jumps.Add(ec.DefineLabel());
				if (setReturnState)
				{
					initializer.HoistedReturnState = ec.GetTemporaryField(ec.Module.Compiler.BuiltinTypes.Int, true);
				}
			}
			int num = redirected_jumps.IndexOf(label);
			if (num < 0)
			{
				redirected_jumps.Add(label);
				num = redirected_jumps.Count - 1;
			}
			if (setReturnState)
			{
				IntConstant source = new IntConstant(initializer.HoistedReturnState.Type, num, Location.Null);
				initializer.HoistedReturnState.EmitAssign(ec, source, false, false);
			}
		}

		private void EmitUnwindFinallyTable(EmitContext ec)
		{
			if (redirected_jumps != null)
			{
				((AsyncInitializer)ec.CurrentAnonymousMethod).HoistedReturnState.EmitLoad(ec);
				ec.Emit(OpCodes.Switch, redirected_jumps.ToArray());
				ec.MarkLabel(redirected_jumps[0]);
			}
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			DefiniteAssignmentBitSet definiteAssignment = fc.BranchDefiniteAssignment();
			TryFinally tryFinally = fc.TryFinally;
			fc.TryFinally = this;
			bool flag = base.Statement.FlowAnalysis(fc);
			fc.TryFinally = tryFinally;
			DefiniteAssignmentBitSet definiteAssignment2 = fc.DefiniteAssignment;
			fc.DefiniteAssignment = definiteAssignment;
			bool flag2 = fini.FlowAnalysis(fc);
			if (try_exit_dat != null)
			{
				foreach (DefiniteAssignmentBitSet item in try_exit_dat)
				{
					fc.ParametersBlock.CheckControlExit(fc, fc.DefiniteAssignment | item);
				}
				try_exit_dat = null;
			}
			fc.DefiniteAssignment |= definiteAssignment2;
			return flag | flag2;
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			return fini.MarkReachable(rc) | base.MarkReachable(rc);
		}

		protected override void CloneTo(CloneContext clonectx, Statement t)
		{
			TryFinally tryFinally = (TryFinally)t;
			tryFinally.stmt = stmt.Clone(clonectx);
			if (fini != null)
			{
				tryFinally.fini = (ExplicitBlock)clonectx.LookupBlock(fini);
			}
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
