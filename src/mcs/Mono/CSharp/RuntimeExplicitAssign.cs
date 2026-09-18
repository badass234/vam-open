namespace Mono.CSharp
{
	public class RuntimeExplicitAssign : Assign
	{
		public RuntimeExplicitAssign(Expression target, Expression source)
			: base(target, source, target.Location)
		{
		}

		protected override Expression ResolveConversions(ResolveContext ec)
		{
			source = EmptyCast.Create(source, target.Type);
			return this;
		}
	}
}
