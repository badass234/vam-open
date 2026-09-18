using System;

namespace Mono.CSharp
{
	public class TemporaryVariableReference : VariableReference
	{
		public class Declarator : Statement
		{
			private TemporaryVariableReference variable;

			public Declarator(TemporaryVariableReference variable)
			{
				this.variable = variable;
				loc = variable.loc;
			}

			protected override void DoEmit(EmitContext ec)
			{
				variable.li.CreateBuilder(ec);
			}

			public override void Emit(EmitContext ec)
			{
				DoEmit(ec);
			}

			protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
			{
				return false;
			}

			protected override void CloneTo(CloneContext clonectx, Statement target)
			{
			}
		}

		private LocalVariable li;

		public override bool IsLockedByStatement
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public LocalVariable LocalInfo => li;

		public override bool IsFixed => true;

		public override bool IsRef => false;

		public override string Name
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		protected override ILocalVariable Variable => li;

		public override VariableInfo VariableInfo => null;

		public TemporaryVariableReference(LocalVariable li, Location loc)
		{
			this.li = li;
			type = li.Type;
			base.loc = loc;
		}

		public static TemporaryVariableReference Create(TypeSpec type, Block block, Location loc)
		{
			return new TemporaryVariableReference(LocalVariable.CreateCompilerGenerated(type, block, loc), loc);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			eclass = ExprClass.Variable;
			if (ec.CurrentAnonymousMethod is StateMachineInitializer && (ec.CurrentBlock.Explicit.HasYield || ec.CurrentBlock.Explicit.HasAwait) && ec.IsVariableCapturingRequired)
			{
				li.Block.Explicit.CreateAnonymousMethodStorey(ec).CaptureLocalVariable(ec, li);
			}
			return this;
		}

		public override Expression DoResolveLValue(ResolveContext ec, Expression right_side)
		{
			return Resolve(ec);
		}

		public override void Emit(EmitContext ec)
		{
			li.CreateBuilder(ec);
			Emit(ec, false);
		}

		public void EmitAssign(EmitContext ec, Expression source)
		{
			li.CreateBuilder(ec);
			EmitAssign(ec, source, false, false);
		}

		public override HoistedVariable GetHoistedVariable(AnonymousExpression ae)
		{
			return li.HoistedVariant;
		}

		public override void SetHasAddressTaken()
		{
			throw new NotImplementedException();
		}
	}
}
