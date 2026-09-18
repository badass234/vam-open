using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class Continue : LocalExitStatement
	{
		public Continue(Location l)
			: base(l)
		{
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}

		protected override void DoEmit(EmitContext ec)
		{
			Label label = ec.LoopBegin;
			if (ec.TryFinallyUnwind != null)
			{
				AsyncInitializer initializer = (AsyncInitializer)ec.CurrentAnonymousMethod;
				label = TryFinally.EmitRedirectedJump(ec, initializer, label, enclosing_loop.Statement as Block);
			}
			ec.Emit(unwind_protect ? OpCodes.Leave : OpCodes.Br, label);
		}

		protected override bool DoResolve(BlockContext bc)
		{
			enclosing_loop = bc.EnclosingLoop;
			return base.DoResolve(bc);
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			base.MarkReachable(rc);
			if (!rc.IsUnreachable)
			{
				enclosing_loop.SetIteratorReachable();
			}
			return Reachability.CreateUnreachable();
		}
	}
}
