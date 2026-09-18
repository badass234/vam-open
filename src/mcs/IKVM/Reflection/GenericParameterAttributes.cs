using System;

namespace IKVM.Reflection
{
	[Flags]
	public enum GenericParameterAttributes
	{
		None = 0,
		Covariant = 1,
		Contravariant = 2,
		VarianceMask = Covariant | Contravariant,
		ReferenceTypeConstraint = 4,
		NotNullableValueTypeConstraint = 8,
		DefaultConstructorConstraint = 0x10,
		SpecialConstraintMask = ReferenceTypeConstraint | NotNullableValueTypeConstraint | DefaultConstructorConstraint
	}
}
