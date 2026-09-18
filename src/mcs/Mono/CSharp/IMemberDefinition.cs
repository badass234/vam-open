using System;

namespace Mono.CSharp
{
	public interface IMemberDefinition
	{
		bool? CLSAttributeValue { get; }

		string Name { get; }

		bool IsImported { get; }

		string[] ConditionalConditions();

		ObsoleteAttribute GetAttributeObsolete();

		void SetIsAssigned();

		void SetIsUsed();
	}
}
