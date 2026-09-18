namespace Mono.CSharp
{
	public interface IGenericMethodDefinition : IMethodDefinition, IMemberDefinition
	{
		TypeParameterSpec[] TypeParameters { get; }

		int TypeParametersCount { get; }
	}
}
