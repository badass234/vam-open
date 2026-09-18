namespace Mono.CSharp
{
	public sealed class DynamicSiteClass : HoistedStoreyClass
	{
		public DynamicSiteClass(TypeDefinition parent, MemberBase host, TypeParameters tparams)
			: base(parent, CompilerGeneratedContainer.MakeMemberName(host, "DynamicSite", parent.DynamicSitesCounter, tparams, Location.Null), tparams, Modifiers.STATIC, MemberKind.Class)
		{
			parent.DynamicSitesCounter++;
		}

		public FieldSpec CreateCallSiteField(FullNamedExpression type, Location loc)
		{
			Field field = new HoistedField(this, type, Modifiers.PUBLIC | Modifiers.STATIC, "Site" + AnonymousMethodsCounter++.ToString("X"), null, loc);
			field.Define();
			AddField(field);
			return field.Spec;
		}
	}
}
