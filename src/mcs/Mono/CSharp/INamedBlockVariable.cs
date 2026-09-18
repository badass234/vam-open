namespace Mono.CSharp
{
	public interface INamedBlockVariable
	{
		Block Block { get; }

		bool IsDeclared { get; }

		bool IsParameter { get; }

		Location Location { get; }

		Expression CreateReferenceExpression(ResolveContext rc, Location loc);
	}
}
