using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class GotoCase : SwitchGoto
	{
		private Expression expr;

		public Expression Expr => expr;

		public SwitchLabel Label { get; set; }

		public GotoCase(Expression e, Location l)
			: base(l)
		{
			expr = e;
		}

		public override bool Resolve(BlockContext ec)
		{
			if (ec.Switch == null)
			{
				Error_GotoCaseRequiresSwitchBlock(ec);
				return false;
			}
			Constant constant = expr.ResolveLabelConstant(ec);
			if (constant == null)
			{
				return false;
			}
			Constant constant2;
			if (ec.Switch.IsNullable && constant is NullLiteral)
			{
				constant2 = constant;
			}
			else
			{
				TypeSpec switchType = ec.Switch.SwitchType;
				constant2 = constant.Reduce(ec, switchType);
				if (constant2 == null)
				{
					constant.Error_ValueCannotBeConverted(ec, switchType, true);
					return false;
				}
				if (!Convert.ImplicitStandardConversionExists(constant, switchType))
				{
					ec.Report.Warning(469, 2, loc, "The `goto case' value is not implicitly convertible to type `{0}'", switchType.GetSignatureForError());
				}
			}
			ec.Switch.RegisterGotoCase(this, constant2);
			base.Resolve(ec);
			expr = constant2;
			return true;
		}

		protected override void DoEmit(EmitContext ec)
		{
			ec.Emit(unwind_protect ? OpCodes.Leave : OpCodes.Br, Label.GetILLabel(ec));
		}

		protected override void CloneTo(CloneContext clonectx, Statement t)
		{
			((GotoCase)t).expr = expr.Clone(clonectx);
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			if (!rc.IsUnreachable)
			{
				SwitchLabel switchLabel = switch_statement.FindLabel((Constant)expr);
				if (switchLabel.IsUnreachable)
				{
					switchLabel.MarkReachable(rc);
					switch_statement.Block.ScanGotoJump(switchLabel);
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
