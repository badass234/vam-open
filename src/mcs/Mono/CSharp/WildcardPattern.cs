namespace Mono.CSharp
{
	internal class WildcardPattern : PatternExpression
	{
		public WildcardPattern(Location loc)
			: base(loc)
		{
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			eclass = ExprClass.Value;
			type = rc.BuiltinTypes.Object;
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			ec.EmitInt(1);
		}
	}
}
