namespace Mono.CSharp.Linq
{
	internal sealed class TransparentMemberAccess : MemberAccess
	{
		public TransparentMemberAccess(Expression expr, string name)
			: base(expr, name)
		{
		}

		public override Expression DoResolveLValue(ResolveContext rc, Expression right_side)
		{
			rc.Report.Error(1947, loc, "A range variable `{0}' cannot be assigned to. Consider using `let' clause to store the value", base.Name);
			return null;
		}
	}
}
