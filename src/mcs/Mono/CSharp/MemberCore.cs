using System;
using System.Collections.Generic;
using System.Diagnostics;
using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp
{
	[DebuggerDisplay("{GetSignatureForError()}")]
	public abstract class MemberCore : Attributable, IMemberContext, IModuleContext, IMemberDefinition
	{
		[Flags]
		public enum Flags
		{
			Obsolete_Undetected = 1,
			Obsolete = 2,
			ClsCompliance_Undetected = 4,
			ClsCompliant = 8,
			CloseTypeCreated = 0x10,
			HasCompliantAttribute_Undetected = 0x20,
			HasClsCompliantAttribute = 0x40,
			ClsCompliantAttributeFalse = 0x80,
			Excluded_Undetected = 0x100,
			Excluded = 0x200,
			MethodOverloadsExist = 0x400,
			IsUsed = 0x800,
			IsAssigned = 0x1000,
			HasExplicitLayout = 0x2000,
			PartialDefinitionExists = 0x4000,
			HasStructLayout = 0x8000,
			HasInstanceConstructor = 0x10000,
			HasUserOperators = 0x20000,
			CanBeReused = 0x40000,
			InterfacesExpanded = 0x80000
		}

		private MemberName member_name;

		private Modifiers mod_flags;

		public TypeContainer Parent;

		protected string comment;

		public Flags caching_flags;

		string IMemberDefinition.Name => member_name.Name;

		public MemberName MemberName => member_name;

		public Modifiers ModFlags
		{
			get
			{
				return mod_flags;
			}
			set
			{
				mod_flags = value;
				if ((value & Modifiers.COMPILER_GENERATED) != 0)
				{
					caching_flags = Flags.IsUsed | Flags.IsAssigned;
				}
			}
		}

		public virtual ModuleContainer Module => Parent.Module;

		public Location Location => member_name.Location;

		public abstract string DocCommentHeader { get; }

		public virtual string DocComment
		{
			get
			{
				return comment;
			}
			set
			{
				comment = value;
			}
		}

		public bool IsAvailableForReuse
		{
			get
			{
				return (caching_flags & Flags.CanBeReused) != 0;
			}
			set
			{
				caching_flags = (value ? (caching_flags | Flags.CanBeReused) : (caching_flags & ~Flags.CanBeReused));
			}
		}

		public bool IsCompilerGenerated
		{
			get
			{
				if ((mod_flags & Modifiers.COMPILER_GENERATED) != 0)
				{
					return true;
				}
				if (Parent != null)
				{
					return Parent.IsCompilerGenerated;
				}
				return false;
			}
		}

		public bool IsImported => false;

		public virtual bool IsUsed => (caching_flags & Flags.IsUsed) != 0;

		protected Report Report => Compiler.Report;

		public bool? CLSAttributeValue
		{
			get
			{
				if ((caching_flags & Flags.HasCompliantAttribute_Undetected) == 0)
				{
					if ((caching_flags & Flags.HasClsCompliantAttribute) == 0)
					{
						return null;
					}
					return (caching_flags & Flags.ClsCompliantAttributeFalse) == 0;
				}
				caching_flags &= ~Flags.HasCompliantAttribute_Undetected;
				if (base.OptAttributes != null)
				{
					Attribute attribute = base.OptAttributes.Search(Module.PredefinedAttributes.CLSCompliant);
					if (attribute != null)
					{
						caching_flags |= Flags.HasClsCompliantAttribute;
						if (attribute.GetClsCompliantAttributeValue())
						{
							return true;
						}
						caching_flags |= Flags.ClsCompliantAttributeFalse;
						return false;
					}
				}
				return null;
			}
		}

		protected bool HasClsCompliantAttribute => CLSAttributeValue.HasValue;

		public virtual CompilerContext Compiler => Module.Compiler;

		public virtual TypeSpec CurrentType => Parent.CurrentType;

		public MemberCore CurrentMemberDefinition => this;

		public virtual TypeParameters CurrentTypeParameters => null;

		public bool IsObsolete
		{
			get
			{
				if (GetAttributeObsolete() != null)
				{
					return true;
				}
				if (Parent != null)
				{
					return Parent.IsObsolete;
				}
				return false;
			}
		}

		public bool IsUnsafe
		{
			get
			{
				if ((ModFlags & Modifiers.UNSAFE) != 0)
				{
					return true;
				}
				if (Parent != null)
				{
					return Parent.IsUnsafe;
				}
				return false;
			}
		}

		public bool IsStatic => (ModFlags & Modifiers.STATIC) != 0;

		protected MemberCore(TypeContainer parent, MemberName name, Attributes attrs)
		{
			Parent = parent;
			member_name = name;
			caching_flags = Flags.Obsolete_Undetected | Flags.ClsCompliance_Undetected | Flags.HasCompliantAttribute_Undetected | Flags.Excluded_Undetected;
			AddAttributes(attrs, this);
		}

		protected virtual void SetMemberName(MemberName new_name)
		{
			member_name = new_name;
		}

		public virtual void Accept(StructuralVisitor visitor)
		{
			visitor.Visit(this);
		}

		protected bool CheckAbstractAndExtern(bool has_block)
		{
			if (Parent.PartialContainer.Kind == MemberKind.Interface)
			{
				return true;
			}
			if (has_block)
			{
				if ((ModFlags & Modifiers.EXTERN) != 0)
				{
					Report.Error(179, Location, "`{0}' cannot declare a body because it is marked extern", GetSignatureForError());
					return false;
				}
				if ((ModFlags & Modifiers.ABSTRACT) != 0)
				{
					Report.Error(500, Location, "`{0}' cannot declare a body because it is marked abstract", GetSignatureForError());
					return false;
				}
			}
			else if ((ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN | Modifiers.PARTIAL)) == 0 && !(Parent is Delegate))
			{
				if (Compiler.Settings.Version >= LanguageVersion.V_3)
				{
					PropertyBase.PropertyMethod propertyMethod = this as PropertyBase.PropertyMethod;
					if (propertyMethod is Indexer.GetIndexerMethod || propertyMethod is Indexer.SetIndexerMethod)
					{
						propertyMethod = null;
					}
					if (propertyMethod != null && propertyMethod.Property.AccessorSecond == null)
					{
						Report.Error(840, Location, "`{0}' must have a body because it is not marked abstract or extern. The property can be automatically implemented when you define both accessors", GetSignatureForError());
						return false;
					}
				}
				Report.Error(501, Location, "`{0}' must have a body because it is not marked abstract, extern, or partial", GetSignatureForError());
				return false;
			}
			return true;
		}

		protected void CheckProtectedModifier()
		{
			if ((ModFlags & Modifiers.PROTECTED) != 0)
			{
				if (Parent.PartialContainer.Kind == MemberKind.Struct)
				{
					Report.Error(666, Location, "`{0}': Structs cannot contain protected members", GetSignatureForError());
				}
				else if ((Parent.ModFlags & Modifiers.STATIC) != 0)
				{
					Report.Error(1057, Location, "`{0}': Static classes cannot contain protected members", GetSignatureForError());
				}
				else if ((Parent.ModFlags & Modifiers.SEALED) != 0 && (ModFlags & Modifiers.OVERRIDE) == 0 && !(this is Destructor))
				{
					Report.Warning(628, 4, Location, "`{0}': new protected member declared in sealed class", GetSignatureForError());
				}
			}
		}

		public abstract bool Define();

		public virtual string GetSignatureForError()
		{
			string signatureForError = Parent.GetSignatureForError();
			if (signatureForError == null)
			{
				return member_name.GetSignatureForError();
			}
			return signatureForError + "." + member_name.GetSignatureForError();
		}

		public virtual void Emit()
		{
			if (Compiler.Settings.VerifyClsCompliance)
			{
				VerifyClsCompliance();
			}
		}

		public void SetIsUsed()
		{
			caching_flags |= Flags.IsUsed;
		}

		public void SetIsAssigned()
		{
			caching_flags |= Flags.IsAssigned;
		}

		public virtual void SetConstraints(List<Constraints> constraints_list)
		{
			TypeParameters typeParameters = member_name.TypeParameters;
			if (typeParameters == null)
			{
				Report.Error(80, Location, "Constraints are not allowed on non-generic declarations");
				return;
			}
			foreach (Constraints item in constraints_list)
			{
				TypeParameter typeParameter = typeParameters.Find(item.TypeParameter.Value);
				if (typeParameter == null)
				{
					Report.Error(699, item.Location, "`{0}': A constraint references nonexistent type parameter `{1}'", GetSignatureForError(), item.TypeParameter.Value);
				}
				else
				{
					typeParameter.Constraints = item;
				}
			}
		}

		public virtual ObsoleteAttribute GetAttributeObsolete()
		{
			if ((caching_flags & (Flags.Obsolete_Undetected | Flags.Obsolete)) == 0)
			{
				return null;
			}
			caching_flags &= ~Flags.Obsolete_Undetected;
			if (base.OptAttributes == null)
			{
				return null;
			}
			Attribute attribute = base.OptAttributes.Search(Module.PredefinedAttributes.Obsolete);
			if (attribute == null)
			{
				return null;
			}
			caching_flags |= Flags.Obsolete;
			ObsoleteAttribute obsoleteAttribute = attribute.GetObsoleteAttribute();
			if (obsoleteAttribute == null)
			{
				return null;
			}
			return obsoleteAttribute;
		}

		public virtual void CheckObsoleteness(Location loc)
		{
			ObsoleteAttribute attributeObsolete = GetAttributeObsolete();
			if (attributeObsolete != null)
			{
				AttributeTester.Report_ObsoleteMessage(attributeObsolete, GetSignatureForError(), loc, Report);
			}
		}

		public bool IsAccessibleAs(TypeSpec p)
		{
			if ((mod_flags & Modifiers.PRIVATE) != 0)
			{
				return true;
			}
			while (TypeManager.HasElementType(p))
			{
				p = TypeManager.GetElementType(p);
			}
			if (p.IsGenericParameter)
			{
				return true;
			}
			while (p != null)
			{
				TypeSpec declaringType = p.DeclaringType;
				if (p.IsGeneric)
				{
					TypeSpec[] typeArguments = p.TypeArguments;
					foreach (TypeSpec p2 in typeArguments)
					{
						if (!IsAccessibleAs(p2))
						{
							return false;
						}
					}
				}
				Modifiers modifiers = p.Modifiers & Modifiers.AccessibilityMask;
				if (modifiers != Modifiers.PUBLIC)
				{
					bool flag = false;
					for (MemberCore memberCore = this; !flag && memberCore != null && memberCore.Parent != null; memberCore = memberCore.Parent)
					{
						Modifiers modifiers2 = memberCore.ModFlags & Modifiers.AccessibilityMask;
						if (modifiers <= Modifiers.PRIVATE)
						{
							if (modifiers == Modifiers.PROTECTED)
							{
								goto IL_00d9;
							}
							if (modifiers == Modifiers.PRIVATE)
							{
								if (modifiers2 == Modifiers.PRIVATE)
								{
									TypeContainer parent = memberCore.Parent;
									do
									{
										flag = parent.CurrentType.MemberDefinition == declaringType.MemberDefinition;
									}
									while (!flag && !parent.PartialContainer.IsTopLevel && (parent = parent.Parent) != null);
								}
								continue;
							}
						}
						else
						{
							if (modifiers == Modifiers.INTERNAL)
							{
								if (modifiers2 == Modifiers.PRIVATE || modifiers2 == Modifiers.INTERNAL)
								{
									flag = p.MemberDefinition.IsInternalAsPublic(memberCore.Module.DeclaringAssembly);
								}
								continue;
							}
							if (modifiers == (Modifiers.PROTECTED | Modifiers.INTERNAL))
							{
								if (modifiers2 == Modifiers.INTERNAL)
								{
									flag = p.MemberDefinition.IsInternalAsPublic(memberCore.Module.DeclaringAssembly);
									continue;
								}
								if (modifiers2 == (Modifiers.PROTECTED | Modifiers.INTERNAL))
								{
									flag = memberCore.Parent.PartialContainer.IsBaseTypeDefinition(declaringType) && p.MemberDefinition.IsInternalAsPublic(memberCore.Module.DeclaringAssembly);
									continue;
								}
								if (modifiers2 != Modifiers.PROTECTED)
								{
									if (modifiers2 != Modifiers.PRIVATE)
									{
										continue;
									}
									if (p.MemberDefinition.IsInternalAsPublic(memberCore.Module.DeclaringAssembly))
									{
										flag = true;
										continue;
									}
								}
								goto IL_00d9;
							}
						}
						throw new InternalErrorException(modifiers2.ToString());
						IL_00d9:
						switch (modifiers2)
						{
						case Modifiers.PROTECTED:
							flag = memberCore.Parent.PartialContainer.IsBaseTypeDefinition(declaringType);
							continue;
						case Modifiers.PRIVATE:
							break;
						default:
							continue;
						}
						while (memberCore.Parent != null && memberCore.Parent.PartialContainer != null)
						{
							if (memberCore.Parent.PartialContainer.IsBaseTypeDefinition(declaringType))
							{
								flag = true;
								break;
							}
							memberCore = memberCore.Parent;
						}
					}
					if (!flag)
					{
						return false;
					}
				}
				p = declaringType;
			}
			return true;
		}

		public override bool IsClsComplianceRequired()
		{
			if ((caching_flags & Flags.ClsCompliance_Undetected) == 0)
			{
				return (caching_flags & Flags.ClsCompliant) != 0;
			}
			caching_flags &= ~Flags.ClsCompliance_Undetected;
			if (HasClsCompliantAttribute)
			{
				if ((caching_flags & Flags.ClsCompliantAttributeFalse) != 0)
				{
					return false;
				}
				caching_flags |= Flags.ClsCompliant;
				return true;
			}
			if (Parent.IsClsComplianceRequired())
			{
				caching_flags |= Flags.ClsCompliant;
				return true;
			}
			return false;
		}

		public virtual string[] ConditionalConditions()
		{
			return null;
		}

		public bool IsExposedFromAssembly()
		{
			if ((ModFlags & (Modifiers.PROTECTED | Modifiers.PUBLIC)) == 0)
			{
				return this is NamespaceContainer;
			}
			for (TypeDefinition partialContainer = Parent.PartialContainer; partialContainer != null; partialContainer = partialContainer.Parent.PartialContainer)
			{
				if ((partialContainer.ModFlags & (Modifiers.PROTECTED | Modifiers.PUBLIC)) == 0)
				{
					return false;
				}
			}
			return true;
		}

		public ExtensionMethodCandidates LookupExtensionMethod(string name, int arity)
		{
			TypeContainer parent = Parent;
			do
			{
				NamespaceContainer namespaceContainer = parent as NamespaceContainer;
				if (namespaceContainer != null)
				{
					return namespaceContainer.LookupExtensionMethod(this, name, arity, 0);
				}
				parent = parent.Parent;
			}
			while (parent != null);
			return null;
		}

		public virtual FullNamedExpression LookupNamespaceAlias(string name)
		{
			return Parent.LookupNamespaceAlias(name);
		}

		public virtual FullNamedExpression LookupNamespaceOrType(string name, int arity, LookupMode mode, Location loc)
		{
			return Parent.LookupNamespaceOrType(name, arity, mode, loc);
		}

		public virtual bool EnableOverloadChecks(MemberCore overload)
		{
			return false;
		}

		protected virtual bool VerifyClsCompliance()
		{
			if (HasClsCompliantAttribute)
			{
				if (!Module.DeclaringAssembly.HasCLSCompliantAttribute)
				{
					Attribute attribute = base.OptAttributes.Search(Module.PredefinedAttributes.CLSCompliant);
					if ((caching_flags & Flags.ClsCompliantAttributeFalse) != 0)
					{
						Report.Warning(3021, 2, attribute.Location, "`{0}' does not need a CLSCompliant attribute because the assembly is not marked as CLS-compliant", GetSignatureForError());
					}
					else
					{
						Report.Warning(3014, 1, attribute.Location, "`{0}' cannot be marked as CLS-compliant because the assembly is not marked as CLS-compliant", GetSignatureForError());
					}
					return false;
				}
				if (!IsExposedFromAssembly())
				{
					Attribute attribute2 = base.OptAttributes.Search(Module.PredefinedAttributes.CLSCompliant);
					Report.Warning(3019, 2, attribute2.Location, "CLS compliance checking will not be performed on `{0}' because it is not visible from outside this assembly", GetSignatureForError());
					return false;
				}
				if ((caching_flags & Flags.ClsCompliantAttributeFalse) != 0)
				{
					if (Parent is Interface && Parent.IsClsComplianceRequired())
					{
						Report.Warning(3010, 1, Location, "`{0}': CLS-compliant interfaces must have only CLS-compliant members", GetSignatureForError());
					}
					else if (Parent.Kind == MemberKind.Class && (ModFlags & Modifiers.ABSTRACT) != 0 && Parent.IsClsComplianceRequired())
					{
						Report.Warning(3011, 1, Location, "`{0}': only CLS-compliant members can be abstract", GetSignatureForError());
					}
					return false;
				}
				if (Parent.Kind != MemberKind.Namespace && Parent.Kind != 0 && !Parent.IsClsComplianceRequired())
				{
					Attribute attribute3 = base.OptAttributes.Search(Module.PredefinedAttributes.CLSCompliant);
					Report.Warning(3018, 1, attribute3.Location, "`{0}' cannot be marked as CLS-compliant because it is a member of non CLS-compliant type `{1}'", GetSignatureForError(), Parent.GetSignatureForError());
					return false;
				}
			}
			else
			{
				if (!IsExposedFromAssembly())
				{
					return false;
				}
				if (!Parent.IsClsComplianceRequired())
				{
					return false;
				}
			}
			if (member_name.Name[0] == '_')
			{
				Warning_IdentifierNotCompliant();
			}
			if (member_name.TypeParameters != null)
			{
				member_name.TypeParameters.VerifyClsCompliance();
			}
			return true;
		}

		protected void Warning_IdentifierNotCompliant()
		{
			Report.Warning(3008, 1, MemberName.Location, "Identifier `{0}' is not CLS-compliant", GetSignatureForError());
		}

		public virtual string GetCallerMemberName()
		{
			return MemberName.Name;
		}

		public abstract string GetSignatureForDocumentation();

		public virtual void GetCompletionStartingWith(string prefix, List<string> results)
		{
			Parent.GetCompletionStartingWith(prefix, results);
		}

		public virtual void GenerateDocComment(DocumentationBuilder builder)
		{
			if (DocComment == null)
			{
				if (IsExposedFromAssembly())
				{
					Constructor constructor = this as Constructor;
					if (constructor == null || !constructor.IsDefault())
					{
						Report.Warning(1591, 4, Location, "Missing XML comment for publicly visible type or member `{0}'", GetSignatureForError());
					}
				}
				return;
			}
			try
			{
				builder.GenerateDocumentationForMember(this);
			}
			catch (Exception e)
			{
				throw new InternalErrorException(this, e);
			}
		}

		public virtual void WriteDebugSymbol(MonoSymbolFile file)
		{
		}
	}
}
