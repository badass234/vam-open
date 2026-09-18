namespace Mono.CSharp
{
	public class ParameterData : IParameterData
	{
		private readonly string name;

		private readonly Parameter.Modifier modifiers;

		private readonly Expression default_value;

		public Expression DefaultValue => default_value;

		public bool HasExtensionMethodModifier => (modifiers & Parameter.Modifier.This) != 0;

		public bool HasDefaultValue => default_value != null;

		public Parameter.Modifier ModFlags => modifiers;

		public string Name => name;

		public ParameterData(string name, Parameter.Modifier modifiers)
		{
			this.name = name;
			this.modifiers = modifiers;
		}

		public ParameterData(string name, Parameter.Modifier modifiers, Expression defaultValue)
			: this(name, modifiers)
		{
			default_value = defaultValue;
		}
	}
}
