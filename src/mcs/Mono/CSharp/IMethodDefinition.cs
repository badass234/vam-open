using System.Reflection;

namespace Mono.CSharp
{
	public interface IMethodDefinition : IMemberDefinition
	{
		MethodBase Metadata { get; }
	}
}
