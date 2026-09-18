using System;

namespace Mono.CSharp
{
	public abstract class ElementTypeSpec : TypeSpec, ITypeDefinition, IMemberDefinition
	{
		public TypeSpec Element { get; private set; }

		bool ITypeDefinition.IsComImport => false;

		bool ITypeDefinition.IsPartial => false;

		bool ITypeDefinition.IsTypeForwarder => false;

		bool ITypeDefinition.IsCyclicTypeForwarder => false;

		public override string Name
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		IAssemblyDefinition ITypeDefinition.DeclaringAssembly => Element.MemberDefinition.DeclaringAssembly;

		public string Namespace
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public int TypeParametersCount => 0;

		public TypeParameterSpec[] TypeParameters
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public bool IsImported => Element.MemberDefinition.IsImported;

		bool? IMemberDefinition.CLSAttributeValue => Element.MemberDefinition.CLSAttributeValue;

		protected ElementTypeSpec(MemberKind kind, TypeSpec element, Type info)
			: base(kind, element.DeclaringType, null, info, element.Modifiers)
		{
			Element = element;
			state &= ~(StateFlags.Obsolete_Undetected | StateFlags.Obsolete | StateFlags.CLSCompliant_Undetected | StateFlags.CLSCompliant | StateFlags.MissingDependency_Undetected | StateFlags.MissingDependency | StateFlags.HasDynamicElement);
			state |= element.state & (StateFlags.Obsolete_Undetected | StateFlags.Obsolete | StateFlags.CLSCompliant_Undetected | StateFlags.CLSCompliant | StateFlags.MissingDependency_Undetected | StateFlags.MissingDependency | StateFlags.HasDynamicElement);
			if (element.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				state |= StateFlags.HasDynamicElement;
			}
			definition = this;
			cache = MemberCache.Empty;
		}

		public override ObsoleteAttribute GetAttributeObsolete()
		{
			return Element.GetAttributeObsolete();
		}

		protected virtual string GetPostfixSignature()
		{
			return null;
		}

		public override string GetSignatureForDocumentation(bool explicitName)
		{
			return Element.GetSignatureForDocumentation(explicitName) + GetPostfixSignature();
		}

		public override string GetSignatureForError()
		{
			return Element.GetSignatureForError() + GetPostfixSignature();
		}

		public override TypeSpec Mutate(TypeParameterMutator mutator)
		{
			TypeSpec typeSpec = Element.Mutate(mutator);
			if (typeSpec == Element)
			{
				return this;
			}
			ElementTypeSpec obj = (ElementTypeSpec)MemberwiseClone();
			obj.Element = typeSpec;
			obj.info = null;
			return obj;
		}

		bool ITypeDefinition.IsInternalAsPublic(IAssemblyDefinition assembly)
		{
			return Element.MemberDefinition.IsInternalAsPublic(assembly);
		}

		public TypeSpec GetAttributeCoClass()
		{
			return Element.MemberDefinition.GetAttributeCoClass();
		}

		public string GetAttributeDefaultMember()
		{
			return Element.MemberDefinition.GetAttributeDefaultMember();
		}

		public void LoadMembers(TypeSpec declaringType, bool onlyTypes, ref MemberCache cache)
		{
			Element.MemberDefinition.LoadMembers(declaringType, onlyTypes, ref cache);
		}

		public string[] ConditionalConditions()
		{
			return Element.MemberDefinition.ConditionalConditions();
		}

		public void SetIsAssigned()
		{
			Element.MemberDefinition.SetIsAssigned();
		}

		public void SetIsUsed()
		{
			Element.MemberDefinition.SetIsUsed();
		}
	}
}
