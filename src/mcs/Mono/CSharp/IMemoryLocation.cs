namespace Mono.CSharp
{
	public interface IMemoryLocation
	{
		void AddressOf(EmitContext ec, AddressOp mode);
	}
}
