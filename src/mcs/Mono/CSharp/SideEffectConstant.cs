namespace Mono.CSharp
{
	public class SideEffectConstant : Constant
	{
		public readonly Constant value;

		private Expression side_effect;

		public override bool IsSideEffectFree => false;

		public override bool IsDefaultValue => value.IsDefaultValue;

		public override bool IsNegative => value.IsNegative;

		public override bool IsZeroInteger => value.IsZeroInteger;

		public SideEffectConstant(Constant value, Expression side_effect, Location loc)
			: base(loc)
		{
			this.value = value;
			type = value.Type;
			eclass = ExprClass.Value;
			while (side_effect is SideEffectConstant)
			{
				side_effect = ((SideEffectConstant)side_effect).side_effect;
			}
			this.side_effect = side_effect;
		}

		public override bool ContainsEmitWithAwait()
		{
			return side_effect.ContainsEmitWithAwait();
		}

		public override object GetValue()
		{
			return value.GetValue();
		}

		public override string GetValueAsLiteral()
		{
			return value.GetValueAsLiteral();
		}

		public override long GetValueAsLong()
		{
			return value.GetValueAsLong();
		}

		public override void Emit(EmitContext ec)
		{
			side_effect.EmitSideEffect(ec);
			value.Emit(ec);
		}

		public override void EmitSideEffect(EmitContext ec)
		{
			side_effect.EmitSideEffect(ec);
			value.EmitSideEffect(ec);
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			side_effect.FlowAnalysis(fc);
		}

		public override Constant ConvertExplicitly(bool in_checked_context, TypeSpec target_type)
		{
			Constant constant = value.ConvertExplicitly(in_checked_context, target_type);
			if (constant == null)
			{
				return null;
			}
			return new SideEffectConstant(constant, side_effect, constant.Location)
			{
				type = target_type,
				eclass = eclass
			};
		}
	}
}
