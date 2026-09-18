using System;
using System.Collections.Generic;

namespace Mono.CSharp
{
	public class Block : Statement
	{
		[Flags]
		public enum Flags
		{
			Unchecked = 1,
			ReachableEnd = 8,
			Unsafe = 0x10,
			HasCapturedVariable = 0x40,
			HasCapturedThis = 0x80,
			IsExpressionTree = 0x100,
			CompilerGenerated = 0x200,
			HasAsyncModifier = 0x400,
			Resolved = 0x800,
			YieldBlock = 0x1000,
			AwaitBlock = 0x2000,
			FinallyBlock = 0x4000,
			CatchBlock = 0x8000,
			Iterator = 0x100000,
			NoFlowAnalysis = 0x200000,
			InitializationEmitted = 0x400000
		}

		public Block Parent;

		public Location StartLocation;

		public Location EndLocation;

		public ExplicitBlock Explicit;

		public ParametersBlock ParametersBlock;

		protected Flags flags;

		protected List<Statement> statements;

		protected List<Statement> scope_initializers;

		private int? resolving_init_idx;

		private Block original;

		public Block Original
		{
			get
			{
				return original;
			}
			protected set
			{
				original = value;
			}
		}

		public bool IsCompilerGenerated
		{
			get
			{
				return (flags & Flags.CompilerGenerated) != 0;
			}
			set
			{
				flags = (value ? (flags | Flags.CompilerGenerated) : (flags & ~Flags.CompilerGenerated));
			}
		}

		public bool IsCatchBlock => (flags & Flags.CatchBlock) != 0;

		public bool IsFinallyBlock => (flags & Flags.FinallyBlock) != 0;

		public bool Unchecked
		{
			get
			{
				return (flags & Flags.Unchecked) != 0;
			}
			set
			{
				flags = (value ? (flags | Flags.Unchecked) : (flags & ~Flags.Unchecked));
			}
		}

		public bool Unsafe
		{
			get
			{
				return (flags & Flags.Unsafe) != 0;
			}
			set
			{
				flags |= Flags.Unsafe;
			}
		}

		public List<Statement> Statements => statements;

		public Block(Block parent, Location start, Location end)
			: this(parent, (Flags)0, start, end)
		{
		}

		public Block(Block parent, Flags flags, Location start, Location end)
		{
			if (parent != null)
			{
				ParametersBlock = parent.ParametersBlock;
				Explicit = parent.Explicit;
			}
			Parent = parent;
			this.flags = flags;
			StartLocation = start;
			EndLocation = end;
			loc = start;
			statements = new List<Statement>(4);
			original = this;
		}

		public void SetEndLocation(Location loc)
		{
			EndLocation = loc;
		}

		public void AddLabel(LabeledStatement target)
		{
			ParametersBlock.TopBlock.AddLabel(target.Name, target);
		}

		public void AddLocalName(LocalVariable li)
		{
			AddLocalName(li.Name, li);
		}

		public void AddLocalName(string name, INamedBlockVariable li)
		{
			ParametersBlock.TopBlock.AddLocalName(name, li, false);
		}

		public virtual void Error_AlreadyDeclared(string name, INamedBlockVariable variable, string reason)
		{
			if (reason == null)
			{
				Error_AlreadyDeclared(name, variable);
			}
			else
			{
				ParametersBlock.TopBlock.Report.Error(136, variable.Location, "A local variable named `{0}' cannot be declared in this scope because it would give a different meaning to `{0}', which is already used in a `{1}' scope to denote something else", name, reason);
			}
		}

		public virtual void Error_AlreadyDeclared(string name, INamedBlockVariable variable)
		{
			ParametersBlock.ParameterInfo parameterInfo = variable as ParametersBlock.ParameterInfo;
			if (parameterInfo != null)
			{
				parameterInfo.Parameter.Error_DuplicateName(ParametersBlock.TopBlock.Report);
			}
			else
			{
				ParametersBlock.TopBlock.Report.Error(128, variable.Location, "A local variable named `{0}' is already defined in this scope", name);
			}
		}

		public virtual void Error_AlreadyDeclaredTypeParameter(string name, Location loc)
		{
			ParametersBlock.TopBlock.Report.Error(412, loc, "The type parameter name `{0}' is the same as local variable or parameter name", name);
		}

		public void AddScopeStatement(Statement s)
		{
			if (scope_initializers == null)
			{
				scope_initializers = new List<Statement>();
			}
			if (resolving_init_idx.HasValue)
			{
				scope_initializers.Insert(resolving_init_idx.Value, s);
				resolving_init_idx++;
			}
			else
			{
				scope_initializers.Add(s);
			}
		}

		public void InsertStatement(int index, Statement s)
		{
			statements.Insert(index, s);
		}

		public void AddStatement(Statement s)
		{
			statements.Add(s);
		}

		public LabeledStatement LookupLabel(string name)
		{
			return ParametersBlock.GetLabel(name, this);
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			if (rc.IsUnreachable)
			{
				return rc;
			}
			MarkReachableScope(rc);
			foreach (Statement statement in statements)
			{
				rc = statement.MarkReachable(rc);
				if (rc.IsUnreachable)
				{
					if ((flags & Flags.ReachableEnd) != 0)
					{
						return default(Reachability);
					}
					return rc;
				}
			}
			flags |= Flags.ReachableEnd;
			return rc;
		}

		public void MarkReachableScope(Reachability rc)
		{
			base.MarkReachable(rc);
			if (scope_initializers == null)
			{
				return;
			}
			foreach (Statement scope_initializer in scope_initializers)
			{
				scope_initializer.MarkReachable(rc);
			}
		}

		public override bool Resolve(BlockContext bc)
		{
			if ((flags & Flags.Resolved) != 0)
			{
				return true;
			}
			Block currentBlock = bc.CurrentBlock;
			bc.CurrentBlock = this;
			if (scope_initializers != null)
			{
				for (resolving_init_idx = 0; resolving_init_idx < scope_initializers.Count; resolving_init_idx++)
				{
					scope_initializers[resolving_init_idx.Value].Resolve(bc);
				}
				resolving_init_idx = null;
			}
			bool result = true;
			int count = statements.Count;
			for (int i = 0; i < count; i++)
			{
				Statement statement = statements[i];
				if (!statement.Resolve(bc))
				{
					result = false;
					statements[i] = new EmptyStatement(statement.loc);
				}
			}
			bc.CurrentBlock = currentBlock;
			flags |= Flags.Resolved;
			return result;
		}

		protected override void DoEmit(EmitContext ec)
		{
			for (int i = 0; i < statements.Count; i++)
			{
				statements[i].Emit(ec);
			}
		}

		public override void Emit(EmitContext ec)
		{
			if (scope_initializers != null)
			{
				EmitScopeInitializers(ec);
			}
			DoEmit(ec);
		}

		protected void EmitScopeInitializers(EmitContext ec)
		{
			foreach (Statement scope_initializer in scope_initializers)
			{
				scope_initializer.Emit(ec);
			}
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			if (scope_initializers != null)
			{
				foreach (Statement scope_initializer in scope_initializers)
				{
					scope_initializer.FlowAnalysis(fc);
				}
			}
			return DoFlowAnalysis(fc, 0);
		}

		private bool DoFlowAnalysis(FlowAnalysisContext fc, int startIndex)
		{
			bool flag = !reachable;
			bool flag2 = startIndex != 0;
			while (startIndex < statements.Count)
			{
				Statement statement = statements[startIndex];
				flag = statement.FlowAnalysis(fc);
				if (statement.IsUnreachable)
				{
					statements[startIndex] = RewriteUnreachableStatement(statement);
				}
				else if (flag)
				{
					bool flag3 = flag2 && statement is GotoCase;
					for (startIndex++; startIndex < statements.Count; startIndex++)
					{
						statement = statements[startIndex];
						if (statement is SwitchLabel)
						{
							if (!flag3)
							{
								statement.FlowAnalysis(fc);
							}
							break;
						}
						if (statement.IsUnreachable)
						{
							statement.FlowAnalysis(fc);
							statements[startIndex] = RewriteUnreachableStatement(statement);
						}
					}
					if (flag3)
					{
						break;
					}
				}
				else
				{
					LabeledStatement labeledStatement = statement as LabeledStatement;
					if (labeledStatement != null && fc.AddReachedLabel(labeledStatement))
					{
						break;
					}
				}
				startIndex++;
			}
			return !Explicit.HasReachableClosingBrace;
		}

		private static Statement RewriteUnreachableStatement(Statement s)
		{
			if (s is BlockVariable || s is EmptyStatement)
			{
				return s;
			}
			return new EmptyStatement(s.loc);
		}

		public void ScanGotoJump(Statement label)
		{
			int i;
			for (i = 0; i < statements.Count && statements[i] != label; i++)
			{
			}
			Reachability rc = default(Reachability);
			for (i++; i < statements.Count; i++)
			{
				rc = statements[i].MarkReachable(rc);
				if (rc.IsUnreachable)
				{
					return;
				}
			}
			flags |= Flags.ReachableEnd;
		}

		public void ScanGotoJump(Statement label, FlowAnalysisContext fc)
		{
			int i;
			for (i = 0; i < statements.Count && statements[i] != label; i++)
			{
			}
			DoFlowAnalysis(fc, ++i);
		}

		protected override void CloneTo(CloneContext clonectx, Statement t)
		{
			Block block = (Block)t;
			clonectx.AddBlockMap(this, block);
			if (original != this)
			{
				clonectx.AddBlockMap(original, block);
			}
			block.ParametersBlock = (ParametersBlock)((ParametersBlock == this) ? block : clonectx.RemapBlockCopy(ParametersBlock));
			block.Explicit = (ExplicitBlock)((Explicit == this) ? block : clonectx.LookupBlock(Explicit));
			if (Parent != null)
			{
				block.Parent = clonectx.RemapBlockCopy(Parent);
			}
			block.statements = new List<Statement>(statements.Count);
			foreach (Statement statement in statements)
			{
				block.statements.Add(statement.Clone(clonectx));
			}
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
