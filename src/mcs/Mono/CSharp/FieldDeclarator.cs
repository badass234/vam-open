namespace Mono.CSharp
{
	public class FieldDeclarator
	{
		public SimpleMemberName Name { get; private set; }

		public Expression Initializer { get; private set; }

		public FieldDeclarator(SimpleMemberName name, Expression initializer)
		{
			Name = name;
			Initializer = initializer;
		}

		public virtual FullNamedExpression GetFieldTypeExpression(FieldBase field)
		{
			return new TypeExpression(field.MemberType, Name.Location);
		}
	}
}
