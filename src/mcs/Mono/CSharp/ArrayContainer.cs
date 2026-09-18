using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Mono.CSharp
{
	public class ArrayContainer : ElementTypeSpec
	{
		public struct TypeRankPair : IEquatable<TypeRankPair>
		{
			private TypeSpec ts;

			private int rank;

			public TypeRankPair(TypeSpec ts, int rank)
			{
				this.ts = ts;
				this.rank = rank;
			}

			public override int GetHashCode()
			{
				return ts.GetHashCode() ^ rank.GetHashCode();
			}

			public bool Equals(TypeRankPair other)
			{
				if (other.ts == ts)
				{
					return other.rank == rank;
				}
				return false;
			}
		}

		private readonly int rank;

		private readonly ModuleContainer module;

		public int Rank => rank;

		private ArrayContainer(ModuleContainer module, TypeSpec element, int rank)
			: base(MemberKind.ArrayType, element, null)
		{
			this.module = module;
			this.rank = rank;
		}

		public MethodInfo GetConstructor()
		{
			ModuleBuilder builder = module.Builder;
			Type[] array = new Type[rank];
			for (int i = 0; i < rank; i++)
			{
				array[i] = module.Compiler.BuiltinTypes.Int.GetMetaInfo();
			}
			return builder.GetArrayMethod(GetMetaInfo(), Constructor.ConstructorName, CallingConventions.HasThis, null, array);
		}

		public MethodInfo GetAddressMethod()
		{
			ModuleBuilder builder = module.Builder;
			Type[] array = new Type[rank];
			for (int i = 0; i < rank; i++)
			{
				array[i] = module.Compiler.BuiltinTypes.Int.GetMetaInfo();
			}
			return builder.GetArrayMethod(GetMetaInfo(), "Address", CallingConventions.Standard | CallingConventions.HasThis, ReferenceContainer.MakeType(module, base.Element).GetMetaInfo(), array);
		}

		public MethodInfo GetGetMethod()
		{
			ModuleBuilder builder = module.Builder;
			Type[] array = new Type[rank];
			for (int i = 0; i < rank; i++)
			{
				array[i] = module.Compiler.BuiltinTypes.Int.GetMetaInfo();
			}
			return builder.GetArrayMethod(GetMetaInfo(), "Get", CallingConventions.Standard | CallingConventions.HasThis, base.Element.GetMetaInfo(), array);
		}

		public MethodInfo GetSetMethod()
		{
			ModuleBuilder builder = module.Builder;
			Type[] array = new Type[rank + 1];
			for (int i = 0; i < rank; i++)
			{
				array[i] = module.Compiler.BuiltinTypes.Int.GetMetaInfo();
			}
			array[rank] = base.Element.GetMetaInfo();
			return builder.GetArrayMethod(GetMetaInfo(), "Set", CallingConventions.Standard | CallingConventions.HasThis, module.Compiler.BuiltinTypes.Void.GetMetaInfo(), array);
		}

		public override Type GetMetaInfo()
		{
			if (info == null)
			{
				if (rank == 1)
				{
					info = base.Element.GetMetaInfo().MakeArrayType();
				}
				else
				{
					info = base.Element.GetMetaInfo().MakeArrayType(rank);
				}
			}
			return info;
		}

		protected override string GetPostfixSignature()
		{
			return GetPostfixSignature(rank);
		}

		public static string GetPostfixSignature(int rank)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[");
			for (int i = 1; i < rank; i++)
			{
				stringBuilder.Append(",");
			}
			stringBuilder.Append("]");
			return stringBuilder.ToString();
		}

		public override string GetSignatureForDocumentation(bool explicitName)
		{
			StringBuilder stringBuilder = new StringBuilder();
			GetElementSignatureForDocumentation(stringBuilder, explicitName);
			return stringBuilder.ToString();
		}

		private void GetElementSignatureForDocumentation(StringBuilder sb, bool explicitName)
		{
			ArrayContainer arrayContainer = base.Element as ArrayContainer;
			if (arrayContainer == null)
			{
				sb.Append(base.Element.GetSignatureForDocumentation(explicitName));
			}
			else
			{
				arrayContainer.GetElementSignatureForDocumentation(sb, explicitName);
			}
			if (explicitName)
			{
				sb.Append(GetPostfixSignature(rank));
				return;
			}
			sb.Append("[");
			for (int i = 1; i < rank; i++)
			{
				if (i == 1)
				{
					sb.Append("0:");
				}
				sb.Append(",0:");
			}
			sb.Append("]");
		}

		public static ArrayContainer MakeType(ModuleContainer module, TypeSpec element)
		{
			return MakeType(module, element, 1);
		}

		public static ArrayContainer MakeType(ModuleContainer module, TypeSpec element, int rank)
		{
			TypeRankPair key = new TypeRankPair(element, rank);
			ArrayContainer value;
			if (!module.ArrayTypesCache.TryGetValue(key, out value))
			{
				value = new ArrayContainer(module, element, rank);
				value.BaseType = module.Compiler.BuiltinTypes.Array;
				ArrayContainer arrayContainer = value;
				arrayContainer.Interfaces = arrayContainer.BaseType.Interfaces;
				module.ArrayTypesCache.Add(key, value);
			}
			return value;
		}

		public override List<MissingTypeSpecReference> ResolveMissingDependencies(MemberSpec caller)
		{
			return base.Element.ResolveMissingDependencies(caller);
		}
	}
}
