namespace Mono.CSharp
{
	internal sealed class CompilerGeneratedThis : This
	{
		public CompilerGeneratedThis(TypeSpec type, Location loc)
			: base(loc)
		{
			base.type = type;
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			eclass = ExprClass.Variable;
			Block currentBlock = rc.CurrentBlock;
			if (currentBlock != null)
			{
				ToplevelBlock topBlock = currentBlock.ParametersBlock.TopBlock;
				if (topBlock.ThisVariable != null)
				{
					variable_info = topBlock.ThisVariable.VariableInfo;
				}
			}
			return this;
		}

		public override Expression DoResolveLValue(ResolveContext rc, Expression right_side)
		{
			return DoResolve(rc);
		}

		public override HoistedVariable GetHoistedVariable(AnonymousExpression ae)
		{
			return null;
		}
	}
}
