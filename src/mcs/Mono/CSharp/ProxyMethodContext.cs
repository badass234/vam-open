using System;

namespace Mono.CSharp
{
	internal struct ProxyMethodContext : IMemberContext, IModuleContext
	{
		private readonly TypeContainer container;

		public TypeSpec CurrentType
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public TypeParameters CurrentTypeParameters
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public MemberCore CurrentMemberDefinition
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public bool IsObsolete => false;

		public bool IsUnsafe
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public bool IsStatic => false;

		public ModuleContainer Module => container.Module;

		public ProxyMethodContext(TypeContainer container)
		{
			this.container = container;
		}

		public string GetSignatureForError()
		{
			throw new NotImplementedException();
		}

		public ExtensionMethodCandidates LookupExtensionMethod(string name, int arity)
		{
			throw new NotImplementedException();
		}

		public FullNamedExpression LookupNamespaceOrType(string name, int arity, LookupMode mode, Location loc)
		{
			throw new NotImplementedException();
		}

		public FullNamedExpression LookupNamespaceAlias(string name)
		{
			throw new NotImplementedException();
		}
	}
}
