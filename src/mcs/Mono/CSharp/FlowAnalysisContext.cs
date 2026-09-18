using System.Collections.Generic;

namespace Mono.CSharp
{
	public class FlowAnalysisContext
	{
		private readonly CompilerContext ctx;

		private DefiniteAssignmentBitSet conditional_access;

		public DefiniteAssignmentBitSet DefiniteAssignment { get; set; }

		public DefiniteAssignmentBitSet DefiniteAssignmentOnTrue { get; set; }

		public DefiniteAssignmentBitSet DefiniteAssignmentOnFalse { get; set; }

		private Dictionary<Statement, List<DefiniteAssignmentBitSet>> LabelStack { get; set; }

		public ParametersBlock ParametersBlock { get; set; }

		public Report Report => ctx.Report;

		public DefiniteAssignmentBitSet SwitchInitialDefinitiveAssignment { get; set; }

		public TryFinally TryFinally { get; set; }

		public bool UnreachableReported { get; set; }

		public FlowAnalysisContext(CompilerContext ctx, ParametersBlock parametersBlock, int definiteAssignmentLength)
		{
			this.ctx = ctx;
			ParametersBlock = parametersBlock;
			DefiniteAssignment = ((definiteAssignmentLength == 0) ? DefiniteAssignmentBitSet.Empty : new DefiniteAssignmentBitSet(definiteAssignmentLength));
		}

		public bool AddReachedLabel(Statement label)
		{
			List<DefiniteAssignmentBitSet> value;
			if (LabelStack == null)
			{
				LabelStack = new Dictionary<Statement, List<DefiniteAssignmentBitSet>>();
				value = null;
			}
			else
			{
				LabelStack.TryGetValue(label, out value);
			}
			if (value == null)
			{
				value = new List<DefiniteAssignmentBitSet>();
				value.Add(new DefiniteAssignmentBitSet(DefiniteAssignment));
				LabelStack.Add(label, value);
				return false;
			}
			foreach (DefiniteAssignmentBitSet item in value)
			{
				if (DefiniteAssignmentBitSet.AreEqual(item, DefiniteAssignment))
				{
					return true;
				}
			}
			if (DefiniteAssignment == DefiniteAssignmentBitSet.Empty)
			{
				value.Add(DefiniteAssignment);
			}
			else
			{
				value.Add(new DefiniteAssignmentBitSet(DefiniteAssignment));
			}
			return false;
		}

		public DefiniteAssignmentBitSet BranchDefiniteAssignment()
		{
			return BranchDefiniteAssignment(DefiniteAssignment);
		}

		public DefiniteAssignmentBitSet BranchDefiniteAssignment(DefiniteAssignmentBitSet da)
		{
			if (da != DefiniteAssignmentBitSet.Empty)
			{
				DefiniteAssignment = new DefiniteAssignmentBitSet(da);
			}
			return da;
		}

		public void BranchConditionalAccessDefiniteAssignment()
		{
			if (conditional_access == null)
			{
				conditional_access = BranchDefiniteAssignment();
			}
		}

		public void ConditionalAccessEnd()
		{
			DefiniteAssignment = conditional_access;
			conditional_access = null;
		}

		public bool IsDefinitelyAssigned(VariableInfo variable)
		{
			return variable.IsAssigned(DefiniteAssignment);
		}

		public bool IsStructFieldDefinitelyAssigned(VariableInfo variable, string name)
		{
			return variable.IsStructFieldAssigned(DefiniteAssignment, name);
		}

		public void SetVariableAssigned(VariableInfo variable, bool generatedAssignment = false)
		{
			variable.SetAssigned(DefiniteAssignment, generatedAssignment);
		}

		public void SetStructFieldAssigned(VariableInfo variable, string name)
		{
			variable.SetStructFieldAssigned(DefiniteAssignment, name);
		}
	}
}
