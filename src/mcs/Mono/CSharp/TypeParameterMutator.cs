using System;
using System.Collections.Generic;

namespace Mono.CSharp
{
	public class TypeParameterMutator
	{
		private readonly TypeParameters mvar;

		private readonly TypeParameters var;

		private readonly TypeParameterSpec[] src;

		private Dictionary<TypeSpec, TypeSpec> mutated_typespec;

		public TypeParameters MethodTypeParameters => mvar;

		public TypeParameterMutator(TypeParameters mvar, TypeParameters var)
		{
			if (mvar.Count != var.Count)
			{
				throw new ArgumentException();
			}
			this.mvar = mvar;
			this.var = var;
		}

		public TypeParameterMutator(TypeParameterSpec[] srcVar, TypeParameters destVar)
		{
			if (srcVar.Length != destVar.Count)
			{
				throw new ArgumentException();
			}
			src = srcVar;
			var = destVar;
		}

		public static TypeSpec GetMemberDeclaringType(TypeSpec type)
		{
			if (type is InflatedTypeSpec)
			{
				if (type.DeclaringType == null)
				{
					return type.GetDefinition();
				}
				type = MemberCache.GetMember(GetMemberDeclaringType(type.DeclaringType), type);
			}
			return type;
		}

		public TypeSpec Mutate(TypeSpec ts)
		{
			TypeSpec value;
			if (mutated_typespec != null && mutated_typespec.TryGetValue(ts, out value))
			{
				return value;
			}
			value = ts.Mutate(this);
			if (mutated_typespec == null)
			{
				mutated_typespec = new Dictionary<TypeSpec, TypeSpec>();
			}
			mutated_typespec.Add(ts, value);
			return value;
		}

		public TypeParameterSpec Mutate(TypeParameterSpec tp)
		{
			if (mvar != null)
			{
				for (int i = 0; i < mvar.Count; i++)
				{
					if (mvar[i].Type == tp)
					{
						return var[i].Type;
					}
				}
			}
			else
			{
				for (int j = 0; j < src.Length; j++)
				{
					if (src[j] == tp)
					{
						return var[j].Type;
					}
				}
			}
			return tp;
		}

		public TypeSpec[] Mutate(TypeSpec[] targs)
		{
			TypeSpec[] array = new TypeSpec[targs.Length];
			bool flag = false;
			for (int i = 0; i < targs.Length; i++)
			{
				array[i] = Mutate(targs[i]);
				flag |= targs[i] != array[i];
			}
			if (!flag)
			{
				return targs;
			}
			return array;
		}
	}
}
