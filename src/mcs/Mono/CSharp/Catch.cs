using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class Catch : Statement
	{
		private class CatchVariableStore : Statement
		{
			private readonly Catch ctch;

			public CatchVariableStore(Catch ctch)
			{
				this.ctch = ctch;
			}

			protected override void CloneTo(CloneContext clonectx, Statement target)
			{
			}

			protected override void DoEmit(EmitContext ec)
			{
				ctch.EmitCatchVariableStore(ec);
			}

			protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
			{
				return true;
			}
		}

		private class FilterStatement : Statement
		{
			private readonly Catch ctch;

			public FilterStatement(Catch ctch)
			{
				this.ctch = ctch;
			}

			protected override void CloneTo(CloneContext clonectx, Statement target)
			{
			}

			protected override void DoEmit(EmitContext ec)
			{
				if (ctch.li != null)
				{
					if (ctch.hoisted_temp != null)
					{
						ctch.hoisted_temp.Emit(ec);
					}
					else
					{
						ctch.li.Emit(ec);
					}
					if (!ctch.IsGeneral && ctch.type.Kind == MemberKind.TypeParameter)
					{
						ec.Emit(OpCodes.Box, ctch.type);
					}
				}
				Label label = ec.DefineLabel();
				Label label2 = ec.DefineLabel();
				ec.Emit(OpCodes.Brtrue_S, label);
				ec.EmitInt(0);
				ec.Emit(OpCodes.Br, label2);
				ec.MarkLabel(label);
				ctch.Filter.Emit(ec);
				ec.MarkLabel(label2);
				ec.Emit(OpCodes.Endfilter);
				ec.BeginFilterHandler();
				ec.Emit(OpCodes.Pop);
			}

			protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
			{
				ctch.Filter.FlowAnalysis(fc);
				return true;
			}

			public override bool Resolve(BlockContext bc)
			{
				ctch.Filter = ctch.Filter.Resolve(bc);
				if (ctch.Filter != null)
				{
					if (ctch.Filter.ContainsEmitWithAwait())
					{
						bc.Report.Error(7094, ctch.Filter.Location, "The `await' operator cannot be used in the filter expression of a catch clause");
					}
					Constant constant = ctch.Filter as Constant;
					if (constant != null && !constant.IsDefaultValue)
					{
						bc.Report.Warning(7095, 1, ctch.Filter.Location, "Exception filter expression is a constant");
					}
				}
				return true;
			}
		}

		private ExplicitBlock block;

		private LocalVariable li;

		private FullNamedExpression type_expr;

		private CompilerAssign assign;

		private TypeSpec type;

		private LocalTemporary hoisted_temp;

		public ExplicitBlock Block => block;

		public TypeSpec CatchType => type;

		public Expression Filter { get; set; }

		public bool IsGeneral => type_expr == null;

		public FullNamedExpression TypeExpression
		{
			get
			{
				return type_expr;
			}
			set
			{
				type_expr = value;
			}
		}

		public LocalVariable Variable
		{
			get
			{
				return li;
			}
			set
			{
				li = value;
			}
		}

		public Catch(ExplicitBlock block, Location loc)
		{
			this.block = block;
			base.loc = loc;
		}

		protected override void DoEmit(EmitContext ec)
		{
			if (Filter != null)
			{
				ec.BeginExceptionFilterBlock();
				ec.Emit(OpCodes.Isinst, IsGeneral ? ec.BuiltinTypes.Object : CatchType);
				if (Block.HasAwait)
				{
					Block.EmitScopeInitialization(ec);
				}
				else
				{
					Block.Emit(ec);
				}
				return;
			}
			if (IsGeneral)
			{
				ec.BeginCatchBlock(ec.BuiltinTypes.Object);
			}
			else
			{
				ec.BeginCatchBlock(CatchType);
			}
			if (li == null)
			{
				ec.Emit(OpCodes.Pop);
			}
			if (Block.HasAwait)
			{
				if (li != null)
				{
					EmitCatchVariableStore(ec);
				}
			}
			else
			{
				Block.Emit(ec);
			}
		}

		private void EmitCatchVariableStore(EmitContext ec)
		{
			li.CreateBuilder(ec);
			if (li.HoistedVariant != null)
			{
				hoisted_temp = new LocalTemporary(li.Type);
				hoisted_temp.Store(ec);
				assign.UpdateSource(hoisted_temp);
			}
		}

		public override bool Resolve(BlockContext bc)
		{
			using (bc.Set(ResolveContext.Options.CatchScope))
			{
				if (type_expr == null)
				{
					if (CreateExceptionVariable(bc.Module.Compiler.BuiltinTypes.Object))
					{
						if (!block.HasAwait || Filter != null)
						{
							block.AddScopeStatement(new CatchVariableStore(this));
						}
						Expression source = new EmptyExpression(li.Type);
						assign = new CompilerAssign(new LocalVariableReference(li, Location.Null), source, Location.Null);
						Block.AddScopeStatement(new StatementExpression(assign, Location.Null));
					}
				}
				else
				{
					type = type_expr.ResolveAsType(bc);
					if (type == null)
					{
						return false;
					}
					if (li == null)
					{
						CreateExceptionVariable(type);
					}
					if (type.BuiltinType != BuiltinTypeSpec.Type.Exception && !TypeSpec.IsBaseClass(type, bc.BuiltinTypes.Exception, false))
					{
						bc.Report.Error(155, loc, "The type caught or thrown must be derived from System.Exception");
					}
					else if (li != null)
					{
						li.Type = type;
						li.PrepareAssignmentAnalysis(bc);
						Expression expression = new EmptyExpression(li.Type);
						if (li.Type.IsGenericParameter)
						{
							expression = new UnboxCast(expression, li.Type);
						}
						if (!block.HasAwait || Filter != null)
						{
							block.AddScopeStatement(new CatchVariableStore(this));
						}
						assign = new CompilerAssign(new LocalVariableReference(li, Location.Null), expression, Location.Null);
						Block.AddScopeStatement(new StatementExpression(assign, Location.Null));
					}
				}
				if (Filter != null)
				{
					Block.AddScopeStatement(new FilterStatement(this));
				}
				Block.SetCatchBlock();
				return Block.Resolve(bc);
			}
		}

		private bool CreateExceptionVariable(TypeSpec type)
		{
			if (!Block.HasAwait)
			{
				return false;
			}
			li = LocalVariable.CreateCompilerGenerated(type, block, Location.Null);
			return true;
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			if (li != null && !li.IsCompilerGenerated)
			{
				fc.SetVariableAssigned(li.VariableInfo, true);
			}
			return block.FlowAnalysis(fc);
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			base.MarkReachable(rc);
			Constant constant = Filter as Constant;
			if (constant != null && constant.IsDefaultValue)
			{
				return Reachability.CreateUnreachable();
			}
			return block.MarkReachable(rc);
		}

		protected override void CloneTo(CloneContext clonectx, Statement t)
		{
			Catch obj = (Catch)t;
			if (type_expr != null)
			{
				obj.type_expr = (FullNamedExpression)type_expr.Clone(clonectx);
			}
			if (Filter != null)
			{
				obj.Filter = Filter.Clone(clonectx);
			}
			obj.block = (ExplicitBlock)clonectx.LookupBlock(block);
		}
	}
}
