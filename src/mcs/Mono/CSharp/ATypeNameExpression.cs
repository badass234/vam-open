namespace Mono.CSharp
{
	public abstract class ATypeNameExpression : FullNamedExpression
	{
		private string name;

		protected TypeArguments targs;

		public int Arity
		{
			get
			{
				if (targs != null)
				{
					return targs.Count;
				}
				return 0;
			}
		}

		public bool HasTypeArguments
		{
			get
			{
				if (targs != null)
				{
					return !targs.IsEmpty;
				}
				return false;
			}
		}

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		public TypeArguments TypeArguments => targs;

		protected ATypeNameExpression(string name, Location l)
		{
			this.name = name;
			loc = l;
		}

		protected ATypeNameExpression(string name, TypeArguments targs, Location l)
		{
			this.name = name;
			this.targs = targs;
			loc = l;
		}

		protected ATypeNameExpression(string name, int arity, Location l)
			: this(name, new UnboundTypeArguments(arity, l), l)
		{
		}

		public override bool Equals(object obj)
		{
			ATypeNameExpression aTypeNameExpression = obj as ATypeNameExpression;
			if (aTypeNameExpression != null && aTypeNameExpression.Name == Name)
			{
				if (targs != null)
				{
					return targs.Equals(aTypeNameExpression.targs);
				}
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		public static string GetMemberType(MemberCore mc)
		{
			if (mc is Property)
			{
				return "property";
			}
			if (mc is Indexer)
			{
				return "indexer";
			}
			if (mc is FieldBase)
			{
				return "field";
			}
			if (mc is MethodCore)
			{
				return "method";
			}
			if (mc is EnumMember)
			{
				return "enum";
			}
			if (mc is Event)
			{
				return "event";
			}
			return "type";
		}

		public override string GetSignatureForError()
		{
			if (targs != null)
			{
				return Name + "<" + targs.GetSignatureForError() + ">";
			}
			return Name;
		}

		public abstract Expression LookupNameExpression(ResolveContext rc, MemberLookupRestrictions restriction);
	}
}
