namespace Mono.CSharp
{
	public abstract class CompilerGeneratedContainer : ClassOrStruct
	{
		protected CompilerGeneratedContainer(TypeContainer parent, MemberName name, Modifiers mod)
			: this(parent, name, mod, MemberKind.Class)
		{
		}

		protected CompilerGeneratedContainer(TypeContainer parent, MemberName name, Modifiers mod, MemberKind kind)
			: base(parent, name, null, kind)
		{
			base.ModFlags = mod | Modifiers.COMPILER_GENERATED | Modifiers.SEALED;
			spec = new TypeSpec(Kind, null, this, null, base.ModFlags);
		}

		protected void CheckMembersDefined()
		{
			if (base.HasMembersDefined)
			{
				throw new InternalErrorException("Helper class already defined!");
			}
		}

		protected override bool DoDefineMembers()
		{
			if (Kind == MemberKind.Class && !base.IsStatic && !base.PartialContainer.HasInstanceConstructor)
			{
				DefineDefaultConstructor(false);
			}
			return base.DoDefineMembers();
		}

		protected static MemberName MakeMemberName(MemberBase host, string name, int unique_id, TypeParameters tparams, Location loc)
		{
			string name2 = MakeName((host == null) ? null : ((host is InterfaceMemberBase) ? ((InterfaceMemberBase)host).GetFullName(host.MemberName) : host.MemberName.Name), "c", name, unique_id);
			TypeParameters typeParameters = null;
			if (tparams != null)
			{
				typeParameters = new TypeParameters(tparams.Count);
				for (int i = 0; i < tparams.Count; i++)
				{
					typeParameters.Add((TypeParameter)null);
				}
			}
			return new MemberName(name2, typeParameters, loc);
		}

		public static string MakeName(string host, string typePrefix, string name, int id)
		{
			return "<" + host + ">" + typePrefix + "__" + name + id.ToString("X");
		}

		protected override TypeSpec[] ResolveBaseTypes(out FullNamedExpression base_class)
		{
			base_type = Compiler.BuiltinTypes.Object;
			base_class = null;
			return null;
		}
	}
}
