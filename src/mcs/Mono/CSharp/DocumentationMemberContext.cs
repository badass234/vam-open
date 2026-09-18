using System;

namespace Mono.CSharp
{
	internal sealed class DocumentationMemberContext : IMemberContext, IModuleContext
	{
		private readonly MemberCore host;

		private MemberName contextName;

		public TypeSpec CurrentType => host.CurrentType;

		public TypeParameters CurrentTypeParameters => contextName.TypeParameters;

		public MemberCore CurrentMemberDefinition => host.CurrentMemberDefinition;

		public bool IsObsolete => false;

		public bool IsUnsafe => host.IsStatic;

		public bool IsStatic => host.IsStatic;

		public ModuleContainer Module => host.Module;

		public DocumentationMemberContext(MemberCore host, MemberName contextName)
		{
			this.host = host;
			this.contextName = contextName;
		}

		public string GetSignatureForError()
		{
			return host.GetSignatureForError();
		}

		public ExtensionMethodCandidates LookupExtensionMethod(string name, int arity)
		{
			return null;
		}

		public FullNamedExpression LookupNamespaceOrType(string name, int arity, LookupMode mode, Location loc)
		{
			if (arity == 0)
			{
				TypeParameters currentTypeParameters = CurrentTypeParameters;
				if (currentTypeParameters != null)
				{
					for (int i = 0; i < currentTypeParameters.Count; i++)
					{
						TypeParameter typeParameter = currentTypeParameters[i];
						if (typeParameter.Name == name)
						{
							typeParameter.Type.DeclaredPosition = i;
							return new TypeParameterExpr(typeParameter, loc);
						}
					}
				}
			}
			return host.Parent.LookupNamespaceOrType(name, arity, mode, loc);
		}

		public FullNamedExpression LookupNamespaceAlias(string name)
		{
			throw new NotImplementedException();
		}
	}
}
