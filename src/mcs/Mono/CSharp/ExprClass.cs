namespace Mono.CSharp
{
	public enum ExprClass : byte
	{
		Unresolved = 0,
		Value = 1,
		Variable = 2,
		Namespace = 3,
		Type = 4,
		TypeParameter = 5,
		MethodGroup = 6,
		PropertyAccess = 7,
		EventAccess = 8,
		IndexerAccess = 9,
		Nothing = 10
	}
}
