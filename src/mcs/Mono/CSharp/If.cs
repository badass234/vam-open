using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class If : Statement
	{
		private Expression expr;

		public Statement TrueStatement;

		public Statement FalseStatement;

		private bool true_returns;

		private bool false_returns;

		public Expression Expr => expr;

		public If(Expression bool_expr, Statement true_statement, Location l)
			: this(bool_expr, true_statement, null, l)
		{
		}

		public If(Expression bool_expr, Statement true_statement, Statement false_statement, Location l)
		{
			expr = bool_expr;
			TrueStatement = true_statement;
			FalseStatement = false_statement;
			loc = l;
		}

		public override bool Resolve(BlockContext ec)
		{
			expr = expr.Resolve(ec);
			bool flag = TrueStatement.Resolve(ec);
			if (FalseStatement != null)
			{
				flag &= FalseStatement.Resolve(ec);
			}
			return flag;
		}

		protected override void DoEmit(EmitContext ec)
		{
			Label label = ec.DefineLabel();
			Constant constant = expr as Constant;
			if (constant != null)
			{
				constant.EmitSideEffect(ec);
				if (!constant.IsDefaultValue)
				{
					TrueStatement.Emit(ec);
				}
				else if (FalseStatement != null)
				{
					FalseStatement.Emit(ec);
				}
				return;
			}
			expr.EmitBranchable(ec, label, false);
			TrueStatement.Emit(ec);
			if (FalseStatement != null)
			{
				bool flag = false;
				Label label2 = ec.DefineLabel();
				if (!true_returns)
				{
					ec.Emit(OpCodes.Br, label2);
					flag = true;
				}
				ec.MarkLabel(label);
				FalseStatement.Emit(ec);
				if (flag)
				{
					ec.MarkLabel(label2);
				}
			}
			else
			{
				ec.MarkLabel(label);
			}
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			expr.FlowAnalysisConditional(fc);
			DefiniteAssignmentBitSet definiteAssignmentBitSet = new DefiniteAssignmentBitSet(fc.DefiniteAssignmentOnFalse);
			fc.DefiniteAssignment = fc.DefiniteAssignmentOnTrue;
			bool flag = TrueStatement.FlowAnalysis(fc);
			if (FalseStatement == null)
			{
				Constant constant = expr as Constant;
				if (constant != null && !constant.IsDefaultValue)
				{
					return true_returns;
				}
				if (true_returns)
				{
					fc.DefiniteAssignment = definiteAssignmentBitSet;
				}
				else
				{
					fc.DefiniteAssignment &= definiteAssignmentBitSet;
				}
				return false;
			}
			if (true_returns)
			{
				fc.DefiniteAssignment = definiteAssignmentBitSet;
				return FalseStatement.FlowAnalysis(fc);
			}
			DefiniteAssignmentBitSet definiteAssignment = fc.DefiniteAssignment;
			fc.DefiniteAssignment = definiteAssignmentBitSet;
			flag &= FalseStatement.FlowAnalysis(fc);
			if (!TrueStatement.IsUnreachable)
			{
				if (false_returns || FalseStatement.IsUnreachable)
				{
					fc.DefiniteAssignment = definiteAssignment;
				}
				else
				{
					fc.DefiniteAssignment &= definiteAssignment;
				}
			}
			return flag;
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			if (rc.IsUnreachable)
			{
				return rc;
			}
			base.MarkReachable(rc);
			Constant constant = expr as Constant;
			if (constant != null)
			{
				if (!constant.IsDefaultValue)
				{
					rc = TrueStatement.MarkReachable(rc);
				}
				else if (FalseStatement != null)
				{
					rc = FalseStatement.MarkReachable(rc);
				}
				return rc;
			}
			Reachability reachability = TrueStatement.MarkReachable(rc);
			true_returns = reachability.IsUnreachable;
			if (FalseStatement == null)
			{
				return rc;
			}
			Reachability reachability2 = FalseStatement.MarkReachable(rc);
			false_returns = reachability2.IsUnreachable;
			return reachability & reachability2;
		}

		protected override void CloneTo(CloneContext clonectx, Statement t)
		{
			If obj = (If)t;
			obj.expr = expr.Clone(clonectx);
			obj.TrueStatement = TrueStatement.Clone(clonectx);
			if (FalseStatement != null)
			{
				obj.FalseStatement = FalseStatement.Clone(clonectx);
			}
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
