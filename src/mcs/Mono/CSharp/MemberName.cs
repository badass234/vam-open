using System.Diagnostics;
using System.Text;

namespace Mono.CSharp
{
	[DebuggerDisplay("{GetSignatureForError()}")]
	public class MemberName
	{
		public static readonly MemberName Null = new MemberName("");

		public readonly string Name;

		public TypeParameters TypeParameters;

		public readonly FullNamedExpression ExplicitInterface;

		public readonly Location Location;

		public readonly MemberName Left;

		public int Arity
		{
			get
			{
				if (TypeParameters != null)
				{
					return TypeParameters.Count;
				}
				return 0;
			}
		}

		public bool IsGeneric => TypeParameters != null;

		public string Basename
		{
			get
			{
				if (TypeParameters != null)
				{
					return MakeName(Name, TypeParameters);
				}
				return Name;
			}
		}

		public MemberName(string name)
			: this(name, Location.Null)
		{
		}

		public MemberName(string name, Location loc)
			: this(null, name, loc)
		{
		}

		public MemberName(string name, TypeParameters tparams, Location loc)
		{
			Name = name;
			Location = loc;
			TypeParameters = tparams;
		}

		public MemberName(string name, TypeParameters tparams, FullNamedExpression explicitInterface, Location loc)
			: this(name, tparams, loc)
		{
			ExplicitInterface = explicitInterface;
		}

		public MemberName(MemberName left, string name, Location loc)
		{
			Name = name;
			Location = loc;
			Left = left;
		}

		public MemberName(MemberName left, string name, FullNamedExpression explicitInterface, Location loc)
			: this(left, name, loc)
		{
			ExplicitInterface = explicitInterface;
		}

		public MemberName(MemberName left, MemberName right)
		{
			Name = right.Name;
			Location = right.Location;
			TypeParameters = right.TypeParameters;
			Left = left;
		}

		public void CreateMetadataName(StringBuilder sb)
		{
			if (Left != null)
			{
				Left.CreateMetadataName(sb);
			}
			if (sb.Length != 0)
			{
				sb.Append(".");
			}
			sb.Append(Basename);
		}

		public string GetSignatureForDocumentation()
		{
			string text = Basename;
			if (ExplicitInterface != null)
			{
				text = ExplicitInterface.GetSignatureForError() + "." + text;
			}
			if (Left == null)
			{
				return text;
			}
			return Left.GetSignatureForDocumentation() + "." + text;
		}

		public string GetSignatureForError()
		{
			string text = ((TypeParameters == null) ? null : ("<" + TypeParameters.GetSignatureForError() + ">"));
			text = Name + text;
			if (ExplicitInterface != null)
			{
				text = ExplicitInterface.GetSignatureForError() + "." + text;
			}
			if (Left == null)
			{
				return text;
			}
			return Left.GetSignatureForError() + "." + text;
		}

		public override bool Equals(object other)
		{
			return Equals(other as MemberName);
		}

		public bool Equals(MemberName other)
		{
			if (this == other)
			{
				return true;
			}
			if (other == null || Name != other.Name)
			{
				return false;
			}
			if (TypeParameters != null && (other.TypeParameters == null || TypeParameters.Count != other.TypeParameters.Count))
			{
				return false;
			}
			if (TypeParameters == null && other.TypeParameters != null)
			{
				return false;
			}
			if (Left == null)
			{
				return other.Left == null;
			}
			return Left.Equals(other.Left);
		}

		public override int GetHashCode()
		{
			int num = Name.GetHashCode();
			for (MemberName left = Left; left != null; left = left.Left)
			{
				num ^= left.Name.GetHashCode();
			}
			if (TypeParameters != null)
			{
				num ^= TypeParameters.Count << 5;
			}
			return num & 0x7FFFFFFF;
		}

		public static string MakeName(string name, TypeParameters args)
		{
			if (args == null)
			{
				return name;
			}
			return name + "`" + args.Count;
		}
	}
}
