namespace Mono.CSharp
{
	public interface IAssignMethod
	{
		void Emit(EmitContext ec, bool leave_copy);

		void EmitAssign(EmitContext ec, Expression source, bool leave_copy, bool isCompound);
	}
}
