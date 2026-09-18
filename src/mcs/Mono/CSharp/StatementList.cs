using System.Collections.Generic;

namespace Mono.CSharp
{
	public class StatementList : Statement
	{
		private List<Statement> statements;

		public IList<Statement> Statements => statements;

		public StatementList(Statement first, Statement second)
		{
			statements = new List<Statement> { first, second };
		}

		public void Add(Statement statement)
		{
			statements.Add(statement);
		}

		public override bool Resolve(BlockContext ec)
		{
			foreach (Statement statement in statements)
			{
				statement.Resolve(ec);
			}
			return true;
		}

		protected override void DoEmit(EmitContext ec)
		{
			foreach (Statement statement in statements)
			{
				statement.Emit(ec);
			}
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			foreach (Statement statement in statements)
			{
				statement.FlowAnalysis(fc);
			}
			return false;
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			base.MarkReachable(rc);
			Reachability result = rc;
			foreach (Statement statement in statements)
			{
				result = statement.MarkReachable(rc);
			}
			return result;
		}

		protected override void CloneTo(CloneContext clonectx, Statement target)
		{
			StatementList statementList = (StatementList)target;
			statementList.statements = new List<Statement>(statements.Count);
			foreach (Statement statement in statements)
			{
				statementList.statements.Add(statement.Clone(clonectx));
			}
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
