using System;

namespace Mono.CSharp
{
	public interface ITypeDefinition : IMemberDefinition
	{
		IAssemblyDefinition DeclaringAssembly { get; }

		string Namespace { get; }

		bool IsPartial { get; }

		bool IsComImport { get; }

		bool IsTypeForwarder { get; }

		bool IsCyclicTypeForwarder { get; }

		int TypeParametersCount { get; }

		TypeParameterSpec[] TypeParameters { get; }

		TypeSpec GetAttributeCoClass();

		string GetAttributeDefaultMember();

		AttributeUsageAttribute GetAttributeUsage(PredefinedAttribute pa);

		bool IsInternalAsPublic(IAssemblyDefinition assembly);

		void LoadMembers(TypeSpec declaringType, bool onlyTypes, ref MemberCache cache);
	}
}
