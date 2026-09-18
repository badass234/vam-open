namespace Mono.CSharp
{
	public class FieldInitializer : Assign
	{
		private sealed class FieldInitializerContext : BlockContext
		{
			private readonly ExplicitBlock ctor_block;

			public override ExplicitBlock ConstructorBlock => ctor_block;

			public FieldInitializerContext(IMemberContext mc, BlockContext constructorContext)
				: base(mc, null, constructorContext.ReturnType)
			{
				flags |= Options.FieldInitializerScope | Options.ConstructorScope;
				ctor_block = constructorContext.CurrentBlock.Explicit;
				if (ctor_block.IsCompilerGenerated)
				{
					CurrentBlock = ctor_block;
				}
			}
		}

		private ExpressionStatement resolved;

		private FieldBase mc;

		public int AssignmentOffset { get; private set; }

		public FieldBase Field => mc;

		public override Location StartLocation => loc;

		public bool IsDefaultInitializer
		{
			get
			{
				Constant constant = source as Constant;
				if (constant == null)
				{
					return false;
				}
				FieldExpr fieldExpr = (FieldExpr)target;
				return constant.IsDefaultInitializer(fieldExpr.Type);
			}
		}

		public override bool IsSideEffectFree => source.IsSideEffectFree;

		public FieldInitializer(FieldBase mc, Expression expression, Location loc)
			: base(new FieldExpr(mc.Spec, expression.Location), expression, loc)
		{
			this.mc = mc;
			if (!mc.IsStatic)
			{
				((FieldExpr)target).InstanceExpression = new CompilerGeneratedThis(mc.CurrentType, expression.Location);
			}
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			if (source == null)
			{
				return null;
			}
			if (resolved == null)
			{
				BlockContext blockContext = (BlockContext)rc;
				FieldInitializerContext fieldInitializerContext = new FieldInitializerContext(mc, blockContext);
				resolved = base.DoResolve((ResolveContext)fieldInitializerContext) as ExpressionStatement;
				AssignmentOffset = fieldInitializerContext.AssignmentInfoOffset - blockContext.AssignmentInfoOffset;
			}
			return resolved;
		}

		public override void EmitStatement(EmitContext ec)
		{
			if (resolved == null)
			{
				return;
			}
			if (ec.HasSet(BuilderContext.Options.OmitDebugInfo) && ec.HasMethodSymbolBuilder)
			{
				using (ec.With(BuilderContext.Options.OmitDebugInfo, false))
				{
					ec.Mark(loc);
				}
			}
			if (resolved != this)
			{
				resolved.EmitStatement(ec);
			}
			else
			{
				base.EmitStatement(ec);
			}
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			source.FlowAnalysis(fc);
			((FieldExpr)target).SetFieldAssigned(fc);
		}
	}
}
