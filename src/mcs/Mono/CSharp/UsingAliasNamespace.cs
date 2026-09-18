using System;

namespace Mono.CSharp
{
	public class UsingAliasNamespace : UsingNamespace
	{
		public struct AliasContext : IMemberContext, IModuleContext
		{
			private readonly NamespaceContainer ns;

			public TypeSpec CurrentType => null;

			public TypeParameters CurrentTypeParameters => null;

			public MemberCore CurrentMemberDefinition => null;

			public bool IsObsolete => false;

			public bool IsUnsafe
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public bool IsStatic
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public ModuleContainer Module => ns.Module;

			public AliasContext(NamespaceContainer ns)
			{
				this.ns = ns;
			}

			public string GetSignatureForError()
			{
				throw new NotImplementedException();
			}

			public ExtensionMethodCandidates LookupExtensionMethod(string name, int arity)
			{
				return null;
			}

			public FullNamedExpression LookupNamespaceOrType(string name, int arity, LookupMode mode, Location loc)
			{
				FullNamedExpression fullNamedExpression = ns.NS.LookupTypeOrNamespace(ns, name, arity, mode, loc);
				if (fullNamedExpression != null)
				{
					return fullNamedExpression;
				}
				fullNamedExpression = ns.LookupExternAlias(name);
				if (fullNamedExpression != null || ns.MemberName == null)
				{
					return fullNamedExpression;
				}
				Namespace parent = ns.NS.Parent;
				MemberName left = ns.MemberName.Left;
				while (left != null)
				{
					fullNamedExpression = parent.LookupTypeOrNamespace(this, name, arity, mode, loc);
					if (fullNamedExpression != null)
					{
						return fullNamedExpression;
					}
					left = left.Left;
					parent = parent.Parent;
				}
				if (ns.Parent != null)
				{
					return ns.Parent.LookupNamespaceOrType(name, arity, mode, loc);
				}
				return null;
			}

			public FullNamedExpression LookupNamespaceAlias(string name)
			{
				return ns.LookupNamespaceAlias(name);
			}
		}

		private readonly SimpleMemberName alias;

		public override SimpleMemberName Alias => alias;

		public UsingAliasNamespace(SimpleMemberName alias, ATypeNameExpression expr, Location loc)
			: base(expr, loc)
		{
			this.alias = alias;
		}

		public override void Define(NamespaceContainer ctx)
		{
			resolved = base.NamespaceExpression.ResolveAsTypeOrNamespace(new AliasContext(ctx), false) ?? new TypeExpression(InternalType.ErrorType, base.NamespaceExpression.Location);
		}
	}
}
