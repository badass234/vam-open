namespace Mono.CSharp
{
	internal struct ConstructorInstanceQualifier : OverloadResolver.IInstanceQualifier
	{
		public TypeSpec InstanceType { get; private set; }

		public ConstructorInstanceQualifier(TypeSpec type)
		{
			this = default(ConstructorInstanceQualifier);
			InstanceType = type;
		}

		public bool CheckProtectedMemberAccess(ResolveContext rc, MemberSpec member)
		{
			return MemberExpr.CheckProtectedMemberAccess(rc, member, InstanceType);
		}
	}
}
