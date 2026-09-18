using System.Collections.Generic;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class For : LoopStatement
	{
		private bool infinite;

		private bool empty;

		private bool iterator_reachable;

		private bool end_reachable;

		private List<DefiniteAssignmentBitSet> end_reachable_das;

		public Statement Initializer { get; set; }

		public Expression Condition { get; set; }

		public Statement Iterator { get; set; }

		public For(Location l)
			: base(null)
		{
			loc = l;
		}

		public override bool Resolve(BlockContext bc)
		{
			Initializer.Resolve(bc);
			if (Condition != null)
			{
				Condition = Condition.Resolve(bc);
				Constant constant = Condition as Constant;
				if (constant != null)
				{
					if (constant.IsDefaultValue)
					{
						empty = true;
					}
					else
					{
						infinite = true;
					}
				}
			}
			else
			{
				infinite = true;
			}
			if (base.Resolve(bc))
			{
				return Iterator.Resolve(bc);
			}
			return false;
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			Initializer.FlowAnalysis(fc);
			DefiniteAssignmentBitSet definiteAssignment;
			if (Condition != null)
			{
				Condition.FlowAnalysisConditional(fc);
				fc.DefiniteAssignment = fc.DefiniteAssignmentOnTrue;
				definiteAssignment = new DefiniteAssignmentBitSet(fc.DefiniteAssignmentOnFalse);
			}
			else
			{
				definiteAssignment = fc.BranchDefiniteAssignment();
			}
			base.Statement.FlowAnalysis(fc);
			Iterator.FlowAnalysis(fc);
			if (end_reachable_das != null)
			{
				definiteAssignment = DefiniteAssignmentBitSet.And(end_reachable_das);
				end_reachable_das = null;
			}
			fc.DefiniteAssignment = definiteAssignment;
			if (infinite && !end_reachable)
			{
				return true;
			}
			return false;
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			base.MarkReachable(rc);
			Initializer.MarkReachable(rc);
			if (!base.Statement.MarkReachable(rc).IsUnreachable || iterator_reachable)
			{
				Iterator.MarkReachable(rc);
			}
			if (infinite && !end_reachable)
			{
				return Reachability.CreateUnreachable();
			}
			return rc;
		}

		protected override void DoEmit(EmitContext ec)
		{
			if (Initializer != null)
			{
				Initializer.Emit(ec);
			}
			if (empty)
			{
				Condition.EmitSideEffect(ec);
				return;
			}
			Label loopBegin = ec.LoopBegin;
			Label loopEnd = ec.LoopEnd;
			Label label = ec.DefineLabel();
			Label label2 = ec.DefineLabel();
			ec.LoopBegin = ec.DefineLabel();
			ec.LoopEnd = ec.DefineLabel();
			ec.Emit(OpCodes.Br, label2);
			ec.MarkLabel(label);
			base.Statement.Emit(ec);
			ec.MarkLabel(ec.LoopBegin);
			Iterator.Emit(ec);
			ec.MarkLabel(label2);
			if (Condition != null)
			{
				ec.Mark(Condition.Location);
				if (Condition is Constant)
				{
					Condition.EmitSideEffect(ec);
					ec.Emit(OpCodes.Br, label);
				}
				else
				{
					Condition.EmitBranchable(ec, label, true);
				}
			}
			else
			{
				ec.Emit(OpCodes.Br, label);
			}
			ec.MarkLabel(ec.LoopEnd);
			ec.LoopBegin = loopBegin;
			ec.LoopEnd = loopEnd;
		}

		protected override void CloneTo(CloneContext clonectx, Statement t)
		{
			For obj = (For)t;
			if (Initializer != null)
			{
				obj.Initializer = Initializer.Clone(clonectx);
			}
			if (Condition != null)
			{
				obj.Condition = Condition.Clone(clonectx);
			}
			if (Iterator != null)
			{
				obj.Iterator = Iterator.Clone(clonectx);
			}
			obj.Statement = base.Statement.Clone(clonectx);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}

		public override void AddEndDefiniteAssignment(FlowAnalysisContext fc)
		{
			if (infinite)
			{
				if (end_reachable_das == null)
				{
					end_reachable_das = new List<DefiniteAssignmentBitSet>();
				}
				end_reachable_das.Add(fc.DefiniteAssignment);
			}
		}

		public override void SetEndReachable()
		{
			end_reachable = true;
		}

		public override void SetIteratorReachable()
		{
			iterator_reachable = true;
		}
	}
}
