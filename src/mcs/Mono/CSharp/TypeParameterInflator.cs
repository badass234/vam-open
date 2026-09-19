using System;

namespace Mono.CSharp
{
	public struct TypeParameterInflator
	{
		private readonly TypeSpec type;

		private readonly TypeParameterSpec[] tparams;

		private readonly TypeSpec[] targs;

		private readonly IModuleContext context;

		public IModuleContext Context => context;

		public TypeSpec TypeInstance => type;

		public TypeParameterSpec[] TypeParameters => tparams;

		public TypeParameterInflator(TypeParameterInflator nested, TypeSpec type)
			: this(nested.context, type, nested.tparams, nested.targs)
		{
		}

		public TypeParameterInflator(IModuleContext context, TypeSpec type, TypeParameterSpec[] tparams, TypeSpec[] targs)
		{
			if (tparams.Length != targs.Length)
			{
				throw new ArgumentException("Invalid arguments");
			}
			this.context = context;
			this.tparams = tparams;
			this.targs = targs;
			this.type = type;
		}

		public TypeSpec Inflate(TypeSpec type)
		{
			TypeParameterSpec typeParameterSpec = type as TypeParameterSpec;
			if (typeParameterSpec != null)
			{
				return Inflate(typeParameterSpec);
			}
			ElementTypeSpec elementTypeSpec = type as ElementTypeSpec;
			if (elementTypeSpec != null)
			{
				TypeSpec typeSpec = Inflate(elementTypeSpec.Element);
				if (typeSpec != elementTypeSpec.Element)
				{
					ArrayContainer arrayContainer = elementTypeSpec as ArrayContainer;
					if (arrayContainer != null)
					{
						return ArrayContainer.MakeType(context.Module, typeSpec, arrayContainer.Rank);
					}
					if (elementTypeSpec is PointerContainer)
					{
						return PointerContainer.MakeType(context.Module, typeSpec);
					}
					// A by-ref return or parameter of a generic type — `ref T this[int]` on
					// ReadOnlySpan<T> is the one the BCL ships — has to be re-made around the
					// inflated element. mono's mscorlib up to 2.0 had no by-ref members, so the
					// 2.x compiler this was decompiled from never learned to inflate one: it
					// threw, and every overload set that contains such a member (StringBuilder
					// .Append, int.Parse) failed to resolve with an internal compiler error.
					if (elementTypeSpec is ReferenceContainer)
					{
						return ReferenceContainer.MakeType(context.Module, typeSpec);
					}
					throw new NotImplementedException();
				}
				return elementTypeSpec;
			}
			if (type.Kind == MemberKind.MissingType)
			{
				return type;
			}
			int i = 0;
			TypeSpec[] array;
			if (type.IsNested)
			{
				TypeSpec container = Inflate(type.DeclaringType);
				array = type.TypeArguments;
				if (array.Length == 0 && type.Arity > 0)
				{
					array = type.MemberDefinition.TypeParameters;
				}
				type = MemberCache.FindNestedType(container, type.Name, type.Arity);
				if (array.Length != 0)
				{
					TypeSpec[] array2 = new TypeSpec[array.Length];
					for (; i < array.Length; i++)
					{
						array2[i] = Inflate(array[i]);
					}
					type = type.MakeGenericType(context, array2);
				}
				return type;
			}
			if (type.Arity == 0)
			{
				return type;
			}
			array = new TypeSpec[type.Arity];
			if (type is InflatedTypeSpec)
			{
				for (; i < array.Length; i++)
				{
					array[i] = Inflate(type.TypeArguments[i]);
				}
				type = type.GetDefinition();
			}
			else
			{
				TypeParameterSpec[] typeParameters = type.MemberDefinition.TypeParameters;
				foreach (TypeParameterSpec tp in typeParameters)
				{
					array[i++] = Inflate(tp);
				}
			}
			return type.MakeGenericType(context, array);
		}

		public TypeSpec Inflate(TypeParameterSpec tp)
		{
			for (int i = 0; i < tparams.Length; i++)
			{
				if (tparams[i] == tp)
				{
					return targs[i];
				}
			}
			return tp;
		}
	}
}
