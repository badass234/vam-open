namespace Mono.CSharp
{
	public interface ILocalVariable
	{
		void Emit(EmitContext ec);

		void EmitAssign(EmitContext ec);

		void EmitAddressOf(EmitContext ec);
	}
}
