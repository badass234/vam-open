namespace Mono.CSharp
{
	public interface IExpressionCleanup
	{
		void EmitCleanup(EmitContext ec);
	}
}
