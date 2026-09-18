namespace Mono.CSharp
{
	public interface IMemberContext : IModuleContext
	{
		TypeSpec CurrentType { get; }

		TypeParameters CurrentTypeParameters { get; }

		MemberCore CurrentMemberDefinition { get; }

		bool IsObsolete { get; }

		bool IsUnsafe { get; }

		bool IsStatic { get; }

		string GetSignatureForError();

		ExtensionMethodCandidates LookupExtensionMethod(string name, int arity);

		FullNamedExpression LookupNamespaceOrType(string name, int arity, LookupMode mode, Location loc);

		FullNamedExpression LookupNamespaceAlias(string name);
	}
}
