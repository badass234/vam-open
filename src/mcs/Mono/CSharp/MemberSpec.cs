using System;
using System.Collections.Generic;

namespace Mono.CSharp
{
	public abstract class MemberSpec
	{
		[Flags]
		public enum StateFlags
		{
			Obsolete_Undetected = 1,
			Obsolete = 2,
			CLSCompliant_Undetected = 4,
			CLSCompliant = 8,
			MissingDependency_Undetected = 0x10,
			MissingDependency = 0x20,
			HasDynamicElement = 0x40,
			ConstraintsChecked = 0x80,
			IsAccessor = 0x200,
			IsGeneric = 0x400,
			PendingMetaInflate = 0x1000,
			PendingMakeMethod = 0x2000,
			PendingMemberCacheMembers = 0x4000,
			PendingBaseTypeInflate = 0x8000,
			InterfacesExpanded = 0x10000,
			IsNotCSharpCompatible = 0x20000,
			SpecialRuntimeType = 0x40000,
			InflatedExpressionType = 0x80000,
			InflatedNullableType = 0x100000,
			GenericIterateInterface = 0x200000,
			GenericTask = 0x400000,
			InterfacesImported = 0x800000
		}

		protected const StateFlags SharedStateFlags = StateFlags.Obsolete_Undetected | StateFlags.Obsolete | StateFlags.CLSCompliant_Undetected | StateFlags.CLSCompliant | StateFlags.MissingDependency_Undetected | StateFlags.MissingDependency | StateFlags.HasDynamicElement;

		protected Modifiers modifiers;

		public StateFlags state;

		protected IMemberDefinition definition;

		public readonly MemberKind Kind;

		protected TypeSpec declaringType;

		public virtual int Arity => 0;

		public TypeSpec DeclaringType
		{
			get
			{
				return declaringType;
			}
			set
			{
				declaringType = value;
			}
		}

		public IMemberDefinition MemberDefinition => definition;

		public Modifiers Modifiers
		{
			get
			{
				return modifiers;
			}
			set
			{
				modifiers = value;
			}
		}

		public virtual string Name => definition.Name;

		public bool IsAbstract => (modifiers & Modifiers.ABSTRACT) != 0;

		public bool IsAccessor
		{
			get
			{
				return (state & StateFlags.IsAccessor) != 0;
			}
			set
			{
				state = (value ? (state | StateFlags.IsAccessor) : (state & ~StateFlags.IsAccessor));
			}
		}

		public bool IsGeneric
		{
			get
			{
				return (state & StateFlags.IsGeneric) != 0;
			}
			set
			{
				state = (value ? (state | StateFlags.IsGeneric) : (state & ~StateFlags.IsGeneric));
			}
		}

		public bool IsNotCSharpCompatible
		{
			get
			{
				return (state & StateFlags.IsNotCSharpCompatible) != 0;
			}
			set
			{
				state = (value ? (state | StateFlags.IsNotCSharpCompatible) : (state & ~StateFlags.IsNotCSharpCompatible));
			}
		}

		public bool IsPrivate => (modifiers & Modifiers.PRIVATE) != 0;

		public bool IsPublic => (modifiers & Modifiers.PUBLIC) != 0;

		public bool IsStatic => (modifiers & Modifiers.STATIC) != 0;

		protected MemberSpec(MemberKind kind, TypeSpec declaringType, IMemberDefinition definition, Modifiers modifiers)
		{
			Kind = kind;
			this.declaringType = declaringType;
			this.definition = definition;
			this.modifiers = modifiers;
			if (kind == MemberKind.MissingType)
			{
				state = StateFlags.MissingDependency;
			}
			else
			{
				state = StateFlags.Obsolete_Undetected | StateFlags.CLSCompliant_Undetected | StateFlags.MissingDependency_Undetected;
			}
		}

		public virtual ObsoleteAttribute GetAttributeObsolete()
		{
			if ((state & (StateFlags.Obsolete_Undetected | StateFlags.Obsolete)) == 0)
			{
				return null;
			}
			state &= ~StateFlags.Obsolete_Undetected;
			ObsoleteAttribute attributeObsolete = definition.GetAttributeObsolete();
			if (attributeObsolete != null)
			{
				state |= StateFlags.Obsolete;
			}
			return attributeObsolete;
		}

		public List<MissingTypeSpecReference> GetMissingDependencies()
		{
			return GetMissingDependencies(this);
		}

		public List<MissingTypeSpecReference> GetMissingDependencies(MemberSpec caller)
		{
			if ((state & (StateFlags.MissingDependency_Undetected | StateFlags.MissingDependency)) == 0)
			{
				return null;
			}
			state &= ~StateFlags.MissingDependency_Undetected;
			List<MissingTypeSpecReference> list = ((definition is ImportedDefinition) ? ResolveMissingDependencies(caller) : ((!(this is ElementTypeSpec)) ? null : ((ElementTypeSpec)this).Element.GetMissingDependencies(caller)));
			if (list != null)
			{
				state |= StateFlags.MissingDependency;
			}
			return list;
		}

		public abstract List<MissingTypeSpecReference> ResolveMissingDependencies(MemberSpec caller);

		protected virtual bool IsNotCLSCompliant(out bool attrValue)
		{
			bool? cLSAttributeValue = MemberDefinition.CLSAttributeValue;
			attrValue = cLSAttributeValue ?? false;
			return cLSAttributeValue == false;
		}

		public virtual string GetSignatureForDocumentation()
		{
			return DeclaringType.GetSignatureForDocumentation() + "." + Name;
		}

		public virtual string GetSignatureForError()
		{
			Property.BackingFieldDeclaration backingFieldDeclaration = MemberDefinition as Property.BackingFieldDeclaration;
			return string.Concat(str2: (backingFieldDeclaration != null) ? backingFieldDeclaration.OriginalProperty.MemberName.Name : Name, str0: DeclaringType.GetSignatureForError(), str1: ".");
		}

		public virtual MemberSpec InflateMember(TypeParameterInflator inflator)
		{
			MemberSpec memberSpec = (MemberSpec)MemberwiseClone();
			memberSpec.declaringType = inflator.TypeInstance;
			if (DeclaringType.IsGenericOrParentIsGeneric)
			{
				memberSpec.state |= StateFlags.PendingMetaInflate;
			}
			return memberSpec;
		}

		public bool IsAccessible(IMemberContext ctx)
		{
			Modifiers modifiers = Modifiers & Modifiers.AccessibilityMask;
			if (modifiers == Modifiers.PUBLIC)
			{
				return true;
			}
			TypeSpec typeSpec = DeclaringType;
			TypeSpec currentType = ctx.CurrentType;
			if (modifiers == Modifiers.PRIVATE)
			{
				if (currentType == null || typeSpec == null)
				{
					return false;
				}
				if (typeSpec.MemberDefinition == currentType.MemberDefinition)
				{
					return true;
				}
				return TypeManager.IsNestedChildOf(currentType, typeSpec.MemberDefinition);
			}
			if ((modifiers & Modifiers.INTERNAL) != 0)
			{
				IAssemblyDefinition assemblyDefinition;
				if (currentType != null)
				{
					assemblyDefinition = currentType.MemberDefinition.DeclaringAssembly;
				}
				else
				{
					IAssemblyDefinition declaringAssembly = ctx.Module.DeclaringAssembly;
					assemblyDefinition = declaringAssembly;
				}
				IAssemblyDefinition assembly = assemblyDefinition;
				bool flag = ((typeSpec != null) ? DeclaringType.MemberDefinition.IsInternalAsPublic(assembly) : ((ITypeDefinition)MemberDefinition).IsInternalAsPublic(assembly));
				if (flag || modifiers == Modifiers.INTERNAL)
				{
					return flag;
				}
			}
			while (currentType != null)
			{
				if (TypeManager.IsFamilyAccessible(currentType, typeSpec))
				{
					return true;
				}
				currentType = currentType.DeclaringType;
			}
			return false;
		}

		public bool IsCLSCompliant()
		{
			if ((state & StateFlags.CLSCompliant_Undetected) != 0)
			{
				state &= ~StateFlags.CLSCompliant_Undetected;
				bool attrValue;
				if (IsNotCLSCompliant(out attrValue))
				{
					return false;
				}
				if (!attrValue)
				{
					attrValue = ((DeclaringType == null) ? ((ITypeDefinition)MemberDefinition).DeclaringAssembly.IsCLSCompliant : DeclaringType.IsCLSCompliant());
				}
				if (attrValue)
				{
					state |= StateFlags.CLSCompliant;
				}
			}
			return (state & StateFlags.CLSCompliant) != 0;
		}

		public bool IsConditionallyExcluded(IMemberContext ctx)
		{
			if ((Kind & (MemberKind.Method | MemberKind.Class)) == 0)
			{
				return false;
			}
			string[] array = MemberDefinition.ConditionalConditions();
			if (array == null)
			{
				return false;
			}
			MemberCore memberCore = ctx.CurrentMemberDefinition;
			CompilationSourceFile compilationSourceFile = null;
			while (memberCore != null && compilationSourceFile == null)
			{
				compilationSourceFile = memberCore as CompilationSourceFile;
				memberCore = memberCore.Parent;
			}
			if (compilationSourceFile != null)
			{
				string[] array2 = array;
				foreach (string value in array2)
				{
					if (compilationSourceFile.IsConditionalDefined(value))
					{
						return false;
					}
				}
			}
			return true;
		}

		public override string ToString()
		{
			return GetSignatureForError();
		}
	}
}
