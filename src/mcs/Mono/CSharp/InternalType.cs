using System;

namespace Mono.CSharp
{
	internal class InternalType : TypeSpec, ITypeDefinition, IMemberDefinition
	{
		public static readonly InternalType AnonymousMethod = new InternalType("anonymous method");

		public static readonly InternalType Arglist = new InternalType("__arglist");

		public static readonly InternalType MethodGroup = new InternalType("method group");

		public static readonly InternalType NullLiteral = new InternalType("null");

		public static readonly InternalType FakeInternalType = new InternalType("<fake$type>");

		public static readonly InternalType Namespace = new InternalType("<namespace>");

		public static readonly InternalType ErrorType = new InternalType("<error>");

		public static readonly InternalType VarOutType = new InternalType("var out");

		private readonly string name;

		public override int Arity => 0;

		IAssemblyDefinition ITypeDefinition.DeclaringAssembly
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		bool ITypeDefinition.IsComImport => false;

		bool IMemberDefinition.IsImported => false;

		bool ITypeDefinition.IsPartial => false;

		bool ITypeDefinition.IsTypeForwarder => false;

		bool ITypeDefinition.IsCyclicTypeForwarder => false;

		public override string Name => name;

		string ITypeDefinition.Namespace => null;

		int ITypeDefinition.TypeParametersCount => 0;

		TypeParameterSpec[] ITypeDefinition.TypeParameters => null;

		bool? IMemberDefinition.CLSAttributeValue => null;

		private InternalType(string name)
			: base(MemberKind.InternalCompilerType, null, null, null, Modifiers.PUBLIC)
		{
			this.name = name;
			definition = this;
			cache = MemberCache.Empty;
			state = (state & ~(StateFlags.Obsolete_Undetected | StateFlags.CLSCompliant_Undetected | StateFlags.MissingDependency_Undetected)) | StateFlags.CLSCompliant;
		}

		public override string GetSignatureForError()
		{
			return name;
		}

		TypeSpec ITypeDefinition.GetAttributeCoClass()
		{
			return null;
		}

		string ITypeDefinition.GetAttributeDefaultMember()
		{
			return null;
		}

		AttributeUsageAttribute ITypeDefinition.GetAttributeUsage(PredefinedAttribute pa)
		{
			return null;
		}

		bool ITypeDefinition.IsInternalAsPublic(IAssemblyDefinition assembly)
		{
			throw new NotImplementedException();
		}

		void ITypeDefinition.LoadMembers(TypeSpec declaringType, bool onlyTypes, ref MemberCache cache)
		{
			throw new NotImplementedException();
		}

		string[] IMemberDefinition.ConditionalConditions()
		{
			return null;
		}

		ObsoleteAttribute IMemberDefinition.GetAttributeObsolete()
		{
			return null;
		}

		void IMemberDefinition.SetIsAssigned()
		{
		}

		void IMemberDefinition.SetIsUsed()
		{
		}
	}
}
