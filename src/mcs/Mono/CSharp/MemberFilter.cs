using System;

namespace Mono.CSharp
{
	public struct MemberFilter : IEquatable<MemberSpec>
	{
		public readonly string Name;

		public readonly MemberKind Kind;

		public readonly AParametersCollection Parameters;

		public readonly TypeSpec MemberType;

		public readonly int Arity;

		public MemberFilter(MethodSpec m)
		{
			Name = m.Name;
			Kind = MemberKind.Method;
			Parameters = m.Parameters;
			MemberType = m.ReturnType;
			Arity = m.Arity;
		}

		public MemberFilter(string name, int arity, MemberKind kind, AParametersCollection param, TypeSpec type)
		{
			Name = name;
			Kind = kind;
			Parameters = param;
			MemberType = type;
			Arity = arity;
		}

		public static MemberFilter Constructor(AParametersCollection param)
		{
			return new MemberFilter(Mono.CSharp.Constructor.ConstructorName, 0, MemberKind.Constructor, param, null);
		}

		public static MemberFilter Property(string name, TypeSpec type)
		{
			return new MemberFilter(name, 0, MemberKind.Property, null, type);
		}

		public static MemberFilter Field(string name, TypeSpec type)
		{
			return new MemberFilter(name, 0, MemberKind.Field, null, type);
		}

		public static MemberFilter Method(string name, int arity, AParametersCollection param, TypeSpec type)
		{
			return new MemberFilter(name, arity, MemberKind.Method, param, type);
		}

		public bool Equals(MemberSpec other)
		{
			if ((other.Kind & Kind & MemberKind.MaskType) == 0)
			{
				return false;
			}
			if (Arity >= 0 && Arity != other.Arity)
			{
				return false;
			}
			if (Parameters != null)
			{
				if (!(other is IParametersMember))
				{
					return false;
				}
				AParametersCollection parameters = ((IParametersMember)other).Parameters;
				if (!TypeSpecComparer.Override.IsEqual(Parameters, parameters))
				{
					return false;
				}
			}
			if (MemberType != null)
			{
				if (!(other is IInterfaceMemberSpec))
				{
					return false;
				}
				if (!TypeSpecComparer.Override.IsEqual(((IInterfaceMemberSpec)other).MemberType, MemberType))
				{
					return false;
				}
			}
			return true;
		}
	}
}
