using System;
using System.Collections.Generic;

namespace Mono.CSharp
{
	public class ParametersBlock : ExplicitBlock
	{
		public class ParameterInfo : INamedBlockVariable
		{
			private readonly ParametersBlock block;

			private readonly int index;

			public VariableInfo VariableInfo;

			private bool is_locked;

			public ParametersBlock Block => block;

			Block INamedBlockVariable.Block => block;

			public bool IsDeclared => true;

			public bool IsParameter => true;

			public bool IsLocked
			{
				get
				{
					return is_locked;
				}
				set
				{
					is_locked = value;
				}
			}

			public Location Location => Parameter.Location;

			public Parameter Parameter => block.Parameters[index];

			public TypeSpec ParameterType => Parameter.Type;

			public ParameterInfo(ParametersBlock block, int index)
			{
				this.block = block;
				this.index = index;
			}

			public Expression CreateReferenceExpression(ResolveContext rc, Location loc)
			{
				return new ParameterReference(this, loc);
			}
		}

		private sealed class BlockScopeExpression : Expression
		{
			private Expression child;

			private readonly ParametersBlock block;

			public BlockScopeExpression(Expression child, ParametersBlock block)
			{
				this.child = child;
				this.block = block;
			}

			public override bool ContainsEmitWithAwait()
			{
				return child.ContainsEmitWithAwait();
			}

			public override Expression CreateExpressionTree(ResolveContext ec)
			{
				throw new NotSupportedException();
			}

			protected override Expression DoResolve(ResolveContext ec)
			{
				if (child == null)
				{
					return null;
				}
				child = child.Resolve(ec);
				if (child == null)
				{
					return null;
				}
				eclass = child.eclass;
				type = child.Type;
				return this;
			}

			public override void Emit(EmitContext ec)
			{
				block.EmitScopeInitializers(ec);
				child.Emit(ec);
			}
		}

		protected ParametersCompiled parameters;

		protected ParameterInfo[] parameter_info;

		protected bool resolved;

		protected ToplevelBlock top_block;

		protected StateMachine state_machine;

		protected Dictionary<string, object> labels;

		public bool IsAsync
		{
			get
			{
				return (flags & Flags.HasAsyncModifier) != 0;
			}
			set
			{
				flags = (value ? (flags | Flags.HasAsyncModifier) : (flags & ~Flags.HasAsyncModifier));
			}
		}

		public bool IsExpressionTree => (flags & Flags.IsExpressionTree) != 0;

		public ParametersCompiled Parameters => parameters;

		public StateMachine StateMachine => state_machine;

		public ToplevelBlock TopBlock
		{
			get
			{
				return top_block;
			}
			set
			{
				top_block = value;
			}
		}

		public bool Resolved => (flags & Flags.Resolved) != 0;

		public int TemporaryLocalsCount { get; set; }

		public ParametersBlock(Block parent, ParametersCompiled parameters, Location start, Flags flags = (Flags)0)
			: base(parent, (Flags)0, start, start)
		{
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			this.parameters = parameters;
			ParametersBlock = this;
			base.flags |= flags | (parent.ParametersBlock.flags & (Flags.YieldBlock | Flags.AwaitBlock));
			top_block = parent.ParametersBlock.top_block;
			ProcessParameters();
		}

		protected ParametersBlock(ParametersCompiled parameters, Location start)
			: base(null, (Flags)0, start, start)
		{
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			this.parameters = parameters;
			ParametersBlock = this;
		}

		protected ParametersBlock(ParametersBlock source, ParametersCompiled parameters)
			: base(null, (Flags)0, source.StartLocation, source.EndLocation)
		{
			this.parameters = parameters;
			statements = source.statements;
			scope_initializers = source.scope_initializers;
			resolved = true;
			reachable = source.reachable;
			am_storey = source.am_storey;
			state_machine = source.state_machine;
			flags = source.flags & Flags.ReachableEnd;
			ParametersBlock = this;
			base.Original = source.Original;
		}

		public void CheckControlExit(FlowAnalysisContext fc)
		{
			CheckControlExit(fc, fc.DefiniteAssignment);
		}

		public virtual void CheckControlExit(FlowAnalysisContext fc, DefiniteAssignmentBitSet dat)
		{
			if (parameter_info == null)
			{
				return;
			}
			ParameterInfo[] array = parameter_info;
			foreach (ParameterInfo parameterInfo in array)
			{
				if (parameterInfo.VariableInfo != null && !parameterInfo.VariableInfo.IsAssigned(dat))
				{
					fc.Report.Error(177, parameterInfo.Location, "The out parameter `{0}' must be assigned to before control leaves the current method", parameterInfo.Parameter.Name);
				}
			}
		}

		protected override void CloneTo(CloneContext clonectx, Statement t)
		{
			base.CloneTo(clonectx, t);
			ParametersBlock parametersBlock = (ParametersBlock)t;
			ParametersBlock parametersBlock2 = this;
			while (true)
			{
				if (parametersBlock2.labels != null)
				{
					parametersBlock.labels = new Dictionary<string, object>();
					{
						foreach (KeyValuePair<string, object> label in parametersBlock2.labels)
						{
							List<LabeledStatement> list = label.Value as List<LabeledStatement>;
							if (list != null)
							{
								List<LabeledStatement> list2 = new List<LabeledStatement>();
								foreach (LabeledStatement item in list)
								{
									list2.Add(RemapLabeledStatement(item, clonectx.RemapBlockCopy(item.Block)));
								}
								parametersBlock.labels.Add(label.Key, list2);
							}
							else
							{
								LabeledStatement labeledStatement = (LabeledStatement)label.Value;
								parametersBlock.labels.Add(label.Key, RemapLabeledStatement(labeledStatement, clonectx.RemapBlockCopy(labeledStatement.Block)));
							}
						}
						break;
					}
				}
				if (parametersBlock2.Parent != null)
				{
					parametersBlock2 = parametersBlock2.Parent.ParametersBlock;
					continue;
				}
				break;
			}
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			if (statements.Count == 1)
			{
				Expression expression = statements[0].CreateExpressionTree(ec);
				if (scope_initializers != null)
				{
					expression = new BlockScopeExpression(expression, this);
				}
				return expression;
			}
			return base.CreateExpressionTree(ec);
		}

		public override void Emit(EmitContext ec)
		{
			if (state_machine != null && state_machine.OriginalSourceBlock != this)
			{
				DefineStoreyContainer(ec, state_machine);
				state_machine.EmitStoreyInstantiation(ec, this);
			}
			base.Emit(ec);
		}

		public void EmitEmbedded(EmitContext ec)
		{
			if (state_machine != null && state_machine.OriginalSourceBlock != this)
			{
				DefineStoreyContainer(ec, state_machine);
				state_machine.EmitStoreyInstantiation(ec, this);
			}
			base.Emit(ec);
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			bool result = base.DoFlowAnalysis(fc);
			if (base.HasReachableClosingBrace)
			{
				CheckControlExit(fc);
			}
			return result;
		}

		public LabeledStatement GetLabel(string name, Block block)
		{
			if (labels == null)
			{
				if (Parent != null)
				{
					return Parent.ParametersBlock.GetLabel(name, block);
				}
				return null;
			}
			object value;
			if (!labels.TryGetValue(name, out value))
			{
				return null;
			}
			LabeledStatement labeledStatement = value as LabeledStatement;
			if (labeledStatement != null)
			{
				if (IsLabelVisible(labeledStatement, block))
				{
					return labeledStatement;
				}
			}
			else
			{
				List<LabeledStatement> list = (List<LabeledStatement>)value;
				for (int i = 0; i < list.Count; i++)
				{
					labeledStatement = list[i];
					if (IsLabelVisible(labeledStatement, block))
					{
						return labeledStatement;
					}
				}
			}
			return null;
		}

		private static bool IsLabelVisible(LabeledStatement label, Block b)
		{
			do
			{
				if (label.Block == b)
				{
					return true;
				}
				b = b.Parent;
			}
			while (b != null);
			return false;
		}

		public ParameterInfo GetParameterInfo(Parameter p)
		{
			for (int i = 0; i < parameters.Count; i++)
			{
				if (parameters[i] == p)
				{
					return parameter_info[i];
				}
			}
			throw new ArgumentException("Invalid parameter");
		}

		public ParameterReference GetParameterReference(int index, Location loc)
		{
			return new ParameterReference(parameter_info[index], loc);
		}

		public Statement PerformClone()
		{
			CloneContext clonectx = new CloneContext();
			return Clone(clonectx);
		}

		protected void ProcessParameters()
		{
			if (parameters.Count == 0)
			{
				return;
			}
			parameter_info = new ParameterInfo[parameters.Count];
			for (int i = 0; i < parameter_info.Length; i++)
			{
				IParameterData parameterData = parameters.FixedParameters[i];
				if (parameterData != null)
				{
					parameter_info[i] = new ParameterInfo(this, i);
					if (parameterData.Name != null)
					{
						AddLocalName(parameterData.Name, parameter_info[i]);
					}
				}
			}
		}

		private LabeledStatement RemapLabeledStatement(LabeledStatement stmt, Block dst)
		{
			Block block = stmt.Block;
			if (block.ParametersBlock != this)
			{
				return stmt;
			}
			List<Statement> list = block.Statements;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] == stmt)
				{
					return (LabeledStatement)dst.Statements[i];
				}
			}
			throw new InternalErrorException("Should never be reached");
		}

		public override bool Resolve(BlockContext bc)
		{
			if (resolved)
			{
				return true;
			}
			resolved = true;
			if (bc.HasSet(ResolveContext.Options.ExpressionTreeConversion))
			{
				flags |= Flags.IsExpressionTree;
			}
			try
			{
				PrepareAssignmentAnalysis(bc);
				if (!base.Resolve(bc))
				{
					return false;
				}
			}
			catch (Exception ex)
			{
				if (ex is CompletionResult || bc.Report.IsDisabled || ex is FatalException || bc.Report.Printer is NullReportPrinter || bc.Module.Compiler.Settings.BreakOnInternalError)
				{
					throw;
				}
				if (bc.CurrentBlock != null)
				{
					bc.Report.Error(584, bc.CurrentBlock.StartLocation, "Internal compiler error: {0}", ex.Message);
				}
				else
				{
					bc.Report.Error(587, "Internal compiler error: {0}", ex.Message);
				}
			}
			if (IsAsync)
			{
				AnonymousMethodBody anonymousMethodBody = bc.CurrentAnonymousMethod as AnonymousMethodBody;
				if (anonymousMethodBody != null && anonymousMethodBody.ReturnTypeInference != null && !anonymousMethodBody.ReturnTypeInference.HasBounds(0))
				{
					anonymousMethodBody.ReturnTypeInference = null;
					anonymousMethodBody.ReturnType = bc.Module.PredefinedTypes.Task.TypeSpec;
					return true;
				}
			}
			return true;
		}

		private void PrepareAssignmentAnalysis(BlockContext bc)
		{
			for (int i = 0; i < parameters.Count; i++)
			{
				IParameterData parameterData = parameters.FixedParameters[i];
				if ((parameterData.ModFlags & Parameter.Modifier.OUT) != Parameter.Modifier.NONE)
				{
					parameter_info[i].VariableInfo = VariableInfo.Create(bc, (Parameter)parameterData);
				}
			}
		}

		public ToplevelBlock ConvertToIterator(IMethodData method, TypeDefinition host, TypeSpec iterator_type, bool is_enumerable)
		{
			Iterator iterator = new Iterator(this, method, host, iterator_type, is_enumerable);
			IteratorStorey stateMachine = (IteratorStorey)(state_machine = new IteratorStorey(iterator));
			iterator.SetStateMachine(stateMachine);
			ToplevelBlock toplevelBlock = new ToplevelBlock(host.Compiler, Parameters, Location.Null, Flags.CompilerGenerated);
			toplevelBlock.Original = this;
			toplevelBlock.state_machine = stateMachine;
			toplevelBlock.AddStatement(new Return(iterator, iterator.Location));
			return toplevelBlock;
		}

		public ParametersBlock ConvertToAsyncTask(IMemberContext context, TypeDefinition host, ParametersCompiled parameters, TypeSpec returnType, TypeSpec delegateType, Location loc)
		{
			for (int i = 0; i < parameters.Count; i++)
			{
				Parameter parameter = parameters[i];
				if ((parameter.ModFlags & Parameter.Modifier.RefOutMask) != Parameter.Modifier.NONE)
				{
					host.Compiler.Report.Error(1988, parameter.Location, "Async methods cannot have ref or out parameters");
					return this;
				}
				if (parameter is ArglistParameter)
				{
					host.Compiler.Report.Error(4006, parameter.Location, "__arglist is not allowed in parameter list of async methods");
					return this;
				}
				if (parameters.Types[i].IsPointer)
				{
					host.Compiler.Report.Error(4005, parameter.Location, "Async methods cannot have unsafe parameters");
					return this;
				}
			}
			if (!base.HasAwait)
			{
				host.Compiler.Report.Warning(1998, 1, loc, "Async block lacks `await' operator and will run synchronously");
			}
			BuiltinTypeSpec builtinTypeSpec = host.Module.Compiler.BuiltinTypes.Void;
			AsyncInitializer asyncInitializer = new AsyncInitializer(this, host, builtinTypeSpec);
			asyncInitializer.Type = builtinTypeSpec;
			asyncInitializer.DelegateType = delegateType;
			AsyncTaskStorey stateMachine = (AsyncTaskStorey)(state_machine = new AsyncTaskStorey(this, context, asyncInitializer, returnType));
			asyncInitializer.SetStateMachine(stateMachine);
			ParametersBlock obj = ((this is ToplevelBlock) ? new ToplevelBlock(host.Compiler, Parameters, Location.Null, Flags.CompilerGenerated) : new ParametersBlock(Parent, parameters, Location.Null, Flags.CompilerGenerated | Flags.HasAsyncModifier));
			obj.Original = this;
			obj.state_machine = stateMachine;
			obj.AddStatement(new AsyncInitializerStatement(asyncInitializer));
			return obj;
		}
	}
}
