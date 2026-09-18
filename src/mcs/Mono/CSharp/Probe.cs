namespace Mono.CSharp
{
	public abstract class Probe : Expression
	{
		public Expression ProbeType;

		protected Expression expr;

		protected TypeSpec probe_type_expr;

		public Expression Expr => expr;

		protected abstract string OperatorName { get; }

		protected Probe(Expression expr, Expression probe_type, Location l)
		{
			ProbeType = probe_type;
			loc = l;
			this.expr = expr;
		}

		public override bool ContainsEmitWithAwait()
		{
			return expr.ContainsEmitWithAwait();
		}

		protected Expression ResolveCommon(ResolveContext rc)
		{
			expr = expr.Resolve(rc);
			if (expr == null)
			{
				return null;
			}
			ResolveProbeType(rc);
			if (probe_type_expr == null)
			{
				return this;
			}
			if (probe_type_expr.IsStatic)
			{
				rc.Report.Error(7023, loc, "The second operand of `is' or `as' operator cannot be static type `{0}'", probe_type_expr.GetSignatureForError());
				return null;
			}
			if (expr.Type.IsPointer || probe_type_expr.IsPointer)
			{
				rc.Report.Error(244, loc, "The `{0}' operator cannot be applied to an operand of pointer type", OperatorName);
				return null;
			}
			if (expr.Type == InternalType.AnonymousMethod || expr.Type == InternalType.MethodGroup)
			{
				rc.Report.Error(837, loc, "The `{0}' operator cannot be applied to a lambda expression, anonymous method, or method group", OperatorName);
				return null;
			}
			return this;
		}

		protected virtual void ResolveProbeType(ResolveContext rc)
		{
			probe_type_expr = ProbeType.ResolveAsType(rc);
		}

		public override void EmitSideEffect(EmitContext ec)
		{
			expr.EmitSideEffect(ec);
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			expr.FlowAnalysis(fc);
		}

		public override bool HasConditionalAccess()
		{
			return expr.HasConditionalAccess();
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			Probe obj = (Probe)t;
			obj.expr = expr.Clone(clonectx);
			obj.ProbeType = ProbeType.Clone(clonectx);
		}
	}
}
