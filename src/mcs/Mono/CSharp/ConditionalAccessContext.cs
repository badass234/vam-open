using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class ConditionalAccessContext
	{
		public bool Statement { get; set; }

		public Label EndLabel { get; private set; }

		public TypeSpec Type { get; private set; }

		public ConditionalAccessContext(TypeSpec type, Label endLabel)
		{
			Type = type;
			EndLabel = endLabel;
		}
	}
}
