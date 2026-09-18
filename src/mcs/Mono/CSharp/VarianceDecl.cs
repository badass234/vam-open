namespace Mono.CSharp
{
	public class VarianceDecl
	{
		public Variance Variance { get; private set; }

		public Location Location { get; private set; }

		public VarianceDecl(Variance variance, Location loc)
		{
			Variance = variance;
			Location = loc;
		}

		public static Variance CheckTypeVariance(TypeSpec t, Variance expected, IMemberContext member)
		{
			TypeParameterSpec typeParameterSpec = t as TypeParameterSpec;
			if (typeParameterSpec != null)
			{
				Variance variance = typeParameterSpec.Variance;
				if ((expected == Variance.None && variance != expected) || (expected == Variance.Covariant && variance == Variance.Contravariant) || (expected == Variance.Contravariant && variance == Variance.Covariant))
				{
					((TypeParameter)typeParameterSpec.MemberDefinition).ErrorInvalidVariance(member, expected);
				}
				return expected;
			}
			if (t.TypeArguments.Length != 0)
			{
				TypeParameterSpec[] typeParameters = t.MemberDefinition.TypeParameters;
				TypeSpec[] typeArguments = TypeManager.GetTypeArguments(t);
				for (int i = 0; i < typeArguments.Length; i++)
				{
					Variance variance2 = typeParameters[i].Variance;
					CheckTypeVariance(typeArguments[i], (Variance)((int)variance2 * (int)expected), member);
				}
				return expected;
			}
			ArrayContainer arrayContainer = t as ArrayContainer;
			if (arrayContainer != null)
			{
				return CheckTypeVariance(arrayContainer.Element, expected, member);
			}
			return Variance.None;
		}
	}
}
