namespace Mono.CSharp
{
	public interface IParametersMember : IInterfaceMemberSpec
	{
		AParametersCollection Parameters { get; }
	}
}
