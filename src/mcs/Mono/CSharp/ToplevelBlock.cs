using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class ToplevelBlock : ParametersBlock
	{
		private LocalVariable this_variable;

		private CompilerContext compiler;

		private Dictionary<string, object> names;

		private List<ExplicitBlock> this_references;

		public bool IsIterator
		{
			get
			{
				return (flags & Flags.Iterator) != 0;
			}
			set
			{
				flags = (value ? (flags | Flags.Iterator) : (flags & ~Flags.Iterator));
			}
		}

		public Report Report => compiler.Report;

		public List<ExplicitBlock> ThisReferencesFromChildrenBlock => this_references;

		public LocalVariable ThisVariable => this_variable;

		public ToplevelBlock(CompilerContext ctx, Location loc)
			: this(ctx, ParametersCompiled.EmptyReadOnlyParameters, loc)
		{
		}

		public ToplevelBlock(CompilerContext ctx, ParametersCompiled parameters, Location start, Flags flags = (Flags)0)
			: base(parameters, start)
		{
			compiler = ctx;
			base.flags = flags;
			top_block = this;
			ProcessParameters();
		}

		public ToplevelBlock(ParametersBlock source, ParametersCompiled parameters)
			: base(source, parameters)
		{
			compiler = source.TopBlock.compiler;
			top_block = this;
		}

		public void AddLocalName(string name, INamedBlockVariable li, bool ignoreChildrenBlocks)
		{
			if (names == null)
			{
				names = new Dictionary<string, object>();
			}
			object value;
			if (!names.TryGetValue(name, out value))
			{
				names.Add(name, li);
				return;
			}
			INamedBlockVariable namedBlockVariable = value as INamedBlockVariable;
			List<INamedBlockVariable> list;
			if (namedBlockVariable != null)
			{
				list = new List<INamedBlockVariable>();
				list.Add(namedBlockVariable);
				names[name] = list;
			}
			else
			{
				list = (List<INamedBlockVariable>)value;
			}
			ExplicitBlock explicitBlock = li.Block.Explicit;
			for (int i = 0; i < list.Count; i++)
			{
				namedBlockVariable = list[i];
				Block block = namedBlockVariable.Block.Explicit;
				if (explicitBlock == block)
				{
					li.Block.Error_AlreadyDeclared(name, li);
					break;
				}
				Block block2 = explicitBlock;
				while ((block2 = block2.Parent) != null)
				{
					if (block2 == block)
					{
						li.Block.Error_AlreadyDeclared(name, li, "parent or current");
						i = list.Count;
						break;
					}
				}
				if (ignoreChildrenBlocks || explicitBlock.Parent == block.Parent)
				{
					continue;
				}
				while ((block = block.Parent) != null)
				{
					if (explicitBlock == block)
					{
						li.Block.Error_AlreadyDeclared(name, li, "child");
						i = list.Count;
						break;
					}
				}
			}
			list.Add(li);
		}

		public void AddLabel(string name, LabeledStatement label)
		{
			if (labels == null)
			{
				labels = new Dictionary<string, object>();
			}
			object value;
			if (!labels.TryGetValue(name, out value))
			{
				labels.Add(name, label);
				return;
			}
			LabeledStatement labeledStatement = value as LabeledStatement;
			List<LabeledStatement> list;
			if (labeledStatement != null)
			{
				list = new List<LabeledStatement>();
				list.Add(labeledStatement);
				labels[name] = list;
			}
			else
			{
				list = (List<LabeledStatement>)value;
			}
			for (int i = 0; i < list.Count; i++)
			{
				labeledStatement = list[i];
				Block block = labeledStatement.Block;
				if (label.Block == block)
				{
					Report.SymbolRelatedToPreviousError(labeledStatement.loc, name);
					Report.Error(140, label.loc, "The label `{0}' is a duplicate", name);
					break;
				}
				block = label.Block;
				while ((block = block.Parent) != null)
				{
					if (labeledStatement.Block == block)
					{
						Report.Error(158, label.loc, "The label `{0}' shadows another label by the same name in a contained scope", name);
						i = list.Count;
						break;
					}
				}
				block = labeledStatement.Block;
				while ((block = block.Parent) != null)
				{
					if (label.Block == block)
					{
						Report.Error(158, label.loc, "The label `{0}' shadows another label by the same name in a contained scope", name);
						i = list.Count;
						break;
					}
				}
			}
			list.Add(label);
		}

		public void AddThisReferenceFromChildrenBlock(ExplicitBlock block)
		{
			if (this_references == null)
			{
				this_references = new List<ExplicitBlock>();
			}
			if (!this_references.Contains(block))
			{
				this_references.Add(block);
			}
		}

		public void RemoveThisReferenceFromChildrenBlock(ExplicitBlock block)
		{
			this_references.Remove(block);
		}

		public Arguments GetAllParametersArguments()
		{
			int count = parameters.Count;
			Arguments arguments = new Arguments(count);
			for (int i = 0; i < count; i++)
			{
				ParameterInfo parameterInfo = parameter_info[i];
				ParameterReference parameterReference = GetParameterReference(i, parameterInfo.Location);
				Argument.AType type;
				switch (parameterInfo.Parameter.ParameterModifier & Parameter.Modifier.RefOutMask)
				{
				case Parameter.Modifier.REF:
					type = Argument.AType.Ref;
					break;
				case Parameter.Modifier.OUT:
					type = Argument.AType.Out;
					break;
				default:
					type = Argument.AType.None;
					break;
				}
				arguments.Add(new Argument(parameterReference, type));
			}
			return arguments;
		}

		public bool GetLocalName(string name, Block block, ref INamedBlockVariable variable)
		{
			if (names == null)
			{
				return false;
			}
			object value;
			if (!names.TryGetValue(name, out value))
			{
				return false;
			}
			variable = value as INamedBlockVariable;
			Block block2 = block;
			if (variable != null)
			{
				do
				{
					if (variable.Block == block2.Original)
					{
						return true;
					}
					block2 = block2.Parent;
				}
				while (block2 != null);
				block2 = variable.Block;
				do
				{
					if (block == block2)
					{
						return false;
					}
					block2 = block2.Parent;
				}
				while (block2 != null);
			}
			else
			{
				List<INamedBlockVariable> list = (List<INamedBlockVariable>)value;
				for (int i = 0; i < list.Count; i++)
				{
					variable = list[i];
					do
					{
						if (variable.Block == block2.Original)
						{
							return true;
						}
						block2 = block2.Parent;
					}
					while (block2 != null);
					block2 = variable.Block;
					do
					{
						if (block == block2)
						{
							return false;
						}
						block2 = block2.Parent;
					}
					while (block2 != null);
					block2 = block;
				}
			}
			variable = null;
			return false;
		}

		public void IncludeBlock(ParametersBlock pb, ToplevelBlock block)
		{
			if (block.names == null)
			{
				return;
			}
			foreach (KeyValuePair<string, object> name in block.names)
			{
				INamedBlockVariable namedBlockVariable = name.Value as INamedBlockVariable;
				if (namedBlockVariable != null)
				{
					if (namedBlockVariable.Block.ParametersBlock == pb)
					{
						AddLocalName(name.Key, namedBlockVariable, false);
					}
					continue;
				}
				foreach (INamedBlockVariable item in (List<INamedBlockVariable>)name.Value)
				{
					if (item.Block.ParametersBlock == pb)
					{
						AddLocalName(name.Key, item, false);
					}
				}
			}
		}

		public void AddThisVariable(BlockContext bc)
		{
			if (this_variable != null)
			{
				throw new InternalErrorException(StartLocation.ToString());
			}
			this_variable = new LocalVariable(this, "this", LocalVariable.Flags.Used | LocalVariable.Flags.IsThis, StartLocation);
			this_variable.Type = bc.CurrentType;
			this_variable.PrepareAssignmentAnalysis(bc);
		}

		public override void CheckControlExit(FlowAnalysisContext fc, DefiniteAssignmentBitSet dat)
		{
			if (this_variable != null)
			{
				this_variable.IsThisAssigned(fc, this);
			}
			base.CheckControlExit(fc, dat);
		}

		public override void Emit(EmitContext ec)
		{
			if (Report.Errors > 0)
			{
				return;
			}
			try
			{
				if (base.IsCompilerGenerated)
				{
					using (ec.With(BuilderContext.Options.OmitDebugInfo, true))
					{
						base.Emit(ec);
					}
				}
				else
				{
					base.Emit(ec);
				}
				if (ec.HasReturnLabel || base.HasReachableClosingBrace)
				{
					if (ec.HasReturnLabel)
					{
						ec.MarkLabel(ec.ReturnLabel);
					}
					if (ec.EmitAccurateDebugInfo && !base.IsCompilerGenerated)
					{
						ec.Mark(EndLocation);
					}
					if (ec.ReturnType.Kind != MemberKind.Void)
					{
						ec.Emit(OpCodes.Ldloc, ec.TemporaryReturn());
					}
					ec.Emit(OpCodes.Ret);
				}
			}
			catch (Exception e)
			{
				throw new InternalErrorException(e, StartLocation);
			}
		}

		public bool Resolve(BlockContext bc, IMethodData md)
		{
			if (resolved)
			{
				return true;
			}
			int errors = bc.Report.Errors;
			Resolve(bc);
			if (bc.Report.Errors > errors)
			{
				return false;
			}
			MarkReachable(default(Reachability));
			if (base.HasReachableClosingBrace && bc.ReturnType.Kind != MemberKind.Void)
			{
				bc.Report.Error(161, md.Location, "`{0}': not all code paths return a value", md.GetSignatureForError());
			}
			if ((flags & Flags.NoFlowAnalysis) != 0)
			{
				return true;
			}
			FlowAnalysisContext fc = new FlowAnalysisContext(bc.Module.Compiler, this, bc.AssignmentInfoOffset);
			try
			{
				FlowAnalysis(fc);
			}
			catch (Exception e)
			{
				throw new InternalErrorException(e, StartLocation);
			}
			return true;
		}
	}
}
