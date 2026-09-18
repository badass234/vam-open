using System.Reflection.Emit;

namespace Mono.CSharp
{
	internal abstract class ComplexPatternExpression : PatternExpression
	{
		protected Expression[] comparisons;

		public ATypeNameExpression TypeExpression { get; private set; }

		protected ComplexPatternExpression(ATypeNameExpression typeExpresion, Location loc)
			: base(loc)
		{
			TypeExpression = typeExpresion;
		}

		public override void Emit(EmitContext ec)
		{
			EmitBranchable(ec, ec.RecursivePatternLabel, false);
		}

		public override void EmitBranchable(EmitContext ec, Label target, bool on_true)
		{
			if (comparisons != null)
			{
				Expression[] array = comparisons;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].EmitBranchable(ec, target, false);
				}
			}
		}
	}
}
