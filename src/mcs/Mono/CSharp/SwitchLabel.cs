using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class SwitchLabel : Statement
	{
		private Constant converted;

		private Expression label;

		private Label? il_label;

		public bool IsDefault => label == null;

		public Expression Label => label;

		public Location Location => loc;

		public Constant Converted
		{
			get
			{
				return converted;
			}
			set
			{
				converted = value;
			}
		}

		public bool PatternMatching { get; set; }

		public bool SectionStart { get; set; }

		public SwitchLabel(Expression expr, Location l)
		{
			label = expr;
			loc = l;
		}

		public Label GetILLabel(EmitContext ec)
		{
			if (!il_label.HasValue)
			{
				il_label = ec.DefineLabel();
			}
			return il_label.Value;
		}

		protected override void DoEmit(EmitContext ec)
		{
			ec.MarkLabel(GetILLabel(ec));
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			if (!SectionStart)
			{
				return false;
			}
			fc.BranchDefiniteAssignment(fc.SwitchInitialDefinitiveAssignment);
			return false;
		}

		public override bool Resolve(BlockContext bc)
		{
			if (ResolveAndReduce(bc))
			{
				bc.Switch.RegisterLabel(bc, this);
			}
			return true;
		}

		private bool ResolveAndReduce(BlockContext bc)
		{
			if (IsDefault)
			{
				return true;
			}
			Switch obj = bc.Switch;
			if (PatternMatching)
			{
				label = new Is(obj.ExpressionValue, label, loc).Resolve(bc);
				return label != null;
			}
			Constant constant = label.ResolveLabelConstant(bc);
			if (constant == null)
			{
				return false;
			}
			if (obj.IsNullable && constant is NullLiteral)
			{
				converted = constant;
				return true;
			}
			if (obj.IsPatternMatching)
			{
				label = new Is(obj.ExpressionValue, label, loc).Resolve(bc);
				return true;
			}
			converted = constant.ImplicitConversionRequired(bc, obj.SwitchType);
			return converted != null;
		}

		public void Error_AlreadyOccurs(ResolveContext ec, SwitchLabel collision_with)
		{
			ec.Report.SymbolRelatedToPreviousError(collision_with.loc, null);
			ec.Report.Error(152, loc, "The label `{0}' already occurs in this switch statement", GetSignatureForError());
		}

		protected override void CloneTo(CloneContext clonectx, Statement target)
		{
			SwitchLabel switchLabel = (SwitchLabel)target;
			if (label != null)
			{
				switchLabel.label = label.Clone(clonectx);
			}
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}

		public string GetSignatureForError()
		{
			string arg = ((converted != null) ? converted.GetValueAsLiteral() : "default");
			return $"case {arg}:";
		}
	}
}
