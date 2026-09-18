using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class GotoDefault : SwitchGoto
	{
		public GotoDefault(Location l)
			: base(l)
		{
		}

		public override bool Resolve(BlockContext bc)
		{
			if (bc.Switch == null)
			{
				Error_GotoCaseRequiresSwitchBlock(bc);
				return false;
			}
			bc.Switch.RegisterGotoCase(null, null);
			base.Resolve(bc);
			return true;
		}

		protected override void DoEmit(EmitContext ec)
		{
			ec.Emit(unwind_protect ? OpCodes.Leave : OpCodes.Br, ec.Switch.DefaultLabel.GetILLabel(ec));
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			if (!rc.IsUnreachable)
			{
				SwitchLabel defaultLabel = switch_statement.DefaultLabel;
				if (defaultLabel.IsUnreachable)
				{
					defaultLabel.MarkReachable(rc);
					switch_statement.Block.ScanGotoJump(defaultLabel);
				}
			}
			return base.MarkReachable(rc);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
