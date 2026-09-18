using System.Collections.Generic;

namespace Mono.CSharp
{
	public class BlockVariable : Statement
	{
		private Expression initializer;

		protected FullNamedExpression type_expr;

		protected LocalVariable li;

		protected List<BlockVariableDeclarator> declarators;

		private TypeSpec type;

		public List<BlockVariableDeclarator> Declarators => declarators;

		public Expression Initializer
		{
			get
			{
				return initializer;
			}
			set
			{
				initializer = value;
			}
		}

		public FullNamedExpression TypeExpression => type_expr;

		public LocalVariable Variable => li;

		public BlockVariable(FullNamedExpression type, LocalVariable li)
		{
			type_expr = type;
			this.li = li;
			loc = type_expr.Location;
		}

		protected BlockVariable(LocalVariable li)
		{
			this.li = li;
		}

		public void AddDeclarator(BlockVariableDeclarator decl)
		{
			if (declarators == null)
			{
				declarators = new List<BlockVariableDeclarator>();
			}
			declarators.Add(decl);
		}

		private static void CreateEvaluatorVariable(BlockContext bc, LocalVariable li)
		{
			if (bc.Report.Errors == 0)
			{
				TypeDefinition partialContainer = bc.CurrentMemberDefinition.Parent.PartialContainer;
				Field field = new Field(partialContainer, new TypeExpression(li.Type, li.Location), Modifiers.PUBLIC | Modifiers.STATIC, new MemberName(li.Name, li.Location), null);
				partialContainer.AddField(field);
				field.Define();
				li.HoistedVariant = new HoistedEvaluatorVariable(field);
				li.SetIsUsed();
			}
		}

		public override bool Resolve(BlockContext bc)
		{
			return Resolve(bc, true);
		}

		public bool Resolve(BlockContext bc, bool resolveDeclaratorInitializers)
		{
			if (type == null && !li.IsCompilerGenerated)
			{
				VarExpr varExpr = type_expr as VarExpr;
				if (varExpr != null && !varExpr.IsPossibleType(bc))
				{
					if (bc.Module.Compiler.Settings.Version < LanguageVersion.V_3)
					{
						bc.Report.FeatureIsNotAvailable(bc.Module.Compiler, loc, "implicitly typed local variable");
					}
					if (li.IsFixed)
					{
						bc.Report.Error(821, loc, "A fixed statement cannot use an implicitly typed local variable");
						return false;
					}
					if (li.IsConstant)
					{
						bc.Report.Error(822, loc, "An implicitly typed local variable cannot be a constant");
						return false;
					}
					if (Initializer == null)
					{
						bc.Report.Error(818, loc, "An implicitly typed local variable declarator must include an initializer");
						return false;
					}
					if (declarators != null)
					{
						bc.Report.Error(819, loc, "An implicitly typed local variable declaration cannot include multiple declarators");
						declarators = null;
					}
					Initializer = Initializer.Resolve(bc);
					if (Initializer != null)
					{
						((VarExpr)type_expr).InferType(bc, Initializer);
						type = type_expr.Type;
					}
					else
					{
						type = InternalType.ErrorType;
					}
				}
				if (type == null)
				{
					type = type_expr.ResolveAsType(bc);
					if (type == null)
					{
						return false;
					}
					if (li.IsConstant && !type.IsConstantCompatible)
					{
						Const.Error_InvalidConstantType(type, loc, bc.Report);
					}
				}
				if (type.IsStatic)
				{
					FieldBase.Error_VariableOfStaticClass(loc, li.Name, type, bc.Report);
				}
				li.Type = type;
			}
			bool flag = bc.Module.Compiler.Settings.StatementMode && bc.CurrentBlock is ToplevelBlock;
			if (flag)
			{
				CreateEvaluatorVariable(bc, li);
			}
			else if (type != InternalType.ErrorType)
			{
				li.PrepareAssignmentAnalysis(bc);
			}
			if (initializer != null)
			{
				initializer = ResolveInitializer(bc, li, initializer);
			}
			if (declarators != null)
			{
				foreach (BlockVariableDeclarator declarator in declarators)
				{
					declarator.Variable.Type = li.Type;
					if (flag)
					{
						CreateEvaluatorVariable(bc, declarator.Variable);
					}
					else if (type != InternalType.ErrorType)
					{
						declarator.Variable.PrepareAssignmentAnalysis(bc);
					}
					if ((declarator.Initializer != null) & resolveDeclaratorInitializers)
					{
						declarator.Initializer = ResolveInitializer(bc, declarator.Variable, declarator.Initializer);
					}
				}
			}
			return true;
		}

		protected virtual Expression ResolveInitializer(BlockContext bc, LocalVariable li, Expression initializer)
		{
			return new SimpleAssign(li.CreateReferenceExpression(bc, li.Location), initializer, li.Location).ResolveStatement(bc);
		}

		protected override void DoEmit(EmitContext ec)
		{
			li.CreateBuilder(ec);
			if (Initializer != null && !base.IsUnreachable)
			{
				((ExpressionStatement)Initializer).EmitStatement(ec);
			}
			if (declarators == null)
			{
				return;
			}
			foreach (BlockVariableDeclarator declarator in declarators)
			{
				declarator.Variable.CreateBuilder(ec);
				if (declarator.Initializer != null && !base.IsUnreachable)
				{
					ec.Mark(declarator.Variable.Location);
					((ExpressionStatement)declarator.Initializer).EmitStatement(ec);
				}
			}
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			if (Initializer != null)
			{
				Initializer.FlowAnalysis(fc);
			}
			if (declarators != null)
			{
				foreach (BlockVariableDeclarator declarator in declarators)
				{
					if (declarator.Initializer != null)
					{
						declarator.Initializer.FlowAnalysis(fc);
					}
				}
			}
			return false;
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			(initializer as ExpressionStatement)?.MarkReachable(rc);
			return base.MarkReachable(rc);
		}

		protected override void CloneTo(CloneContext clonectx, Statement target)
		{
			BlockVariable blockVariable = (BlockVariable)target;
			if (type_expr != null)
			{
				blockVariable.type_expr = (FullNamedExpression)type_expr.Clone(clonectx);
			}
			if (initializer != null)
			{
				blockVariable.initializer = initializer.Clone(clonectx);
			}
			if (declarators == null)
			{
				return;
			}
			blockVariable.declarators = null;
			foreach (BlockVariableDeclarator declarator in declarators)
			{
				blockVariable.AddDeclarator(declarator.Clone(clonectx));
			}
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
