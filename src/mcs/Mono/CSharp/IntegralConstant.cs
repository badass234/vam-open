namespace Mono.CSharp
{
	public abstract class IntegralConstant : Constant
	{
		protected IntegralConstant(TypeSpec type, Location loc)
			: base(loc)
		{
			base.type = type;
			eclass = ExprClass.Value;
		}

		public override void Error_ValueCannotBeConverted(ResolveContext ec, TypeSpec target, bool expl)
		{
			try
			{
				ConvertExplicitly(true, target);
				base.Error_ValueCannotBeConverted(ec, target, expl);
			}
			catch
			{
				ec.Report.Error(31, loc, "Constant value `{0}' cannot be converted to a `{1}'", GetValue().ToString(), target.GetSignatureForError());
			}
		}

		public override string GetValueAsLiteral()
		{
			return GetValue().ToString();
		}

		public abstract Constant Increment();
	}
}
