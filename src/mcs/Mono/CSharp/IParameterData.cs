namespace Mono.CSharp
{
	public interface IParameterData
	{
		Expression DefaultValue { get; }

		bool HasExtensionMethodModifier { get; }

		bool HasDefaultValue { get; }

		Parameter.Modifier ModFlags { get; }

		string Name { get; }
	}
}
