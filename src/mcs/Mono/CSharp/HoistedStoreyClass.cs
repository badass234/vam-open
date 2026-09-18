namespace Mono.CSharp
{
	public class HoistedStoreyClass : CompilerGeneratedContainer
	{
		public sealed class HoistedField : Field
		{
			public HoistedField(HoistedStoreyClass parent, FullNamedExpression type, Modifiers mod, string name, Attributes attrs, Location loc)
				: base(parent, type, mod, new MemberName(name, loc), attrs)
			{
			}

			protected override bool ResolveMemberType()
			{
				if (!base.ResolveMemberType())
				{
					return false;
				}
				HoistedStoreyClass genericStorey = ((HoistedStoreyClass)Parent).GetGenericStorey();
				if (genericStorey != null && genericStorey.Mutator != null)
				{
					member_type = genericStorey.Mutator.Mutate(base.MemberType);
				}
				return true;
			}
		}

		protected TypeParameterMutator mutator;

		public TypeParameterMutator Mutator
		{
			get
			{
				return mutator;
			}
			set
			{
				mutator = value;
			}
		}

		public HoistedStoreyClass(TypeDefinition parent, MemberName name, TypeParameters tparams, Modifiers mods, MemberKind kind)
			: base(parent, name, mods | Modifiers.PRIVATE, kind)
		{
			if (tparams != null)
			{
				TypeParameters typeParameters = name.TypeParameters;
				TypeParameterSpec[] array = new TypeParameterSpec[tparams.Count];
				TypeParameterSpec[] array2 = new TypeParameterSpec[tparams.Count];
				for (int i = 0; i < tparams.Count; i++)
				{
					typeParameters[i] = tparams[i].CreateHoistedCopy(spec);
					array[i] = tparams[i].Type;
					array2[i] = typeParameters[i].Type;
				}
				TypeParameterInflator inflator = new TypeParameterInflator(this, null, array, array2);
				for (int j = 0; j < tparams.Count; j++)
				{
					array[j].InflateConstraints(inflator, array2[j]);
				}
				mutator = new TypeParameterMutator(tparams, typeParameters);
			}
		}

		public HoistedStoreyClass GetGenericStorey()
		{
			TypeContainer typeContainer = this;
			while (typeContainer != null && typeContainer.CurrentTypeParameters == null)
			{
				typeContainer = typeContainer.Parent;
			}
			return typeContainer as HoistedStoreyClass;
		}
	}
}
