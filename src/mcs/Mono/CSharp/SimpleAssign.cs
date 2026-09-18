namespace Mono.CSharp
{
	public class SimpleAssign : Assign
	{
		public SimpleAssign(Expression target, Expression source)
			: this(target, source, target.Location)
		{
		}

		public SimpleAssign(Expression target, Expression source, Location loc)
			: base(target, source, loc)
		{
		}

		private bool CheckEqualAssign(Expression t)
		{
			if (source is Assign)
			{
				Assign assign = (Assign)source;
				if (t.Equals(assign.Target))
				{
					return true;
				}
				if (assign is SimpleAssign)
				{
					return ((SimpleAssign)assign).CheckEqualAssign(t);
				}
				return false;
			}
			return t.Equals(source);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			Expression expression = base.DoResolve(ec);
			if (expression == null || expression != this)
			{
				return expression;
			}
			if (CheckEqualAssign(target))
			{
				ec.Report.Warning(1717, 3, loc, "Assignment made to same variable; did you mean to assign something else?");
			}
			return this;
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			base.FlowAnalysis(fc);
			VariableReference variableReference = target as VariableReference;
			if (variableReference != null)
			{
				if (variableReference.VariableInfo != null)
				{
					fc.SetVariableAssigned(variableReference.VariableInfo);
				}
				return;
			}
			FieldExpr fieldExpr = target as FieldExpr;
			if (fieldExpr != null)
			{
				fieldExpr.SetFieldAssigned(fc);
			}
			else
			{
				(target as PropertyExpr)?.SetBackingFieldAssigned(fc);
			}
		}

		public override void MarkReachable(Reachability rc)
		{
			(source as ExpressionStatement)?.MarkReachable(rc);
		}
	}
}
