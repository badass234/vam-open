using System.Collections.Generic;
using System.Reflection;

namespace Mono.CSharp
{
	public abstract class InterfaceMemberBase : MemberBase
	{
		protected const Modifiers AllowedModifiersClass = Modifiers.AccessibilityMask | Modifiers.AllowedExplicitImplFlags | Modifiers.NEW | Modifiers.ABSTRACT | Modifiers.SEALED | Modifiers.STATIC | Modifiers.VIRTUAL | Modifiers.OVERRIDE;

		protected const Modifiers AllowedModifiersStruct = Modifiers.AccessibilityMask | Modifiers.AllowedExplicitImplFlags | Modifiers.NEW | Modifiers.STATIC | Modifiers.OVERRIDE;

		protected const Modifiers AllowedModifiersInterface = Modifiers.NEW | Modifiers.UNSAFE;

		public bool IsInterface;

		public readonly bool IsExplicitImpl;

		protected bool is_external_implementation;

		public TypeSpec InterfaceType;

		protected MethodSpec base_method;

		private readonly Modifiers explicit_mod_flags;

		public MethodAttributes flags;

		public abstract Variance ExpectedMemberTypeVariance { get; }

		public string ShortName => base.MemberName.Name;

		public override bool IsUsed
		{
			get
			{
				if (!IsExplicitImpl)
				{
					return base.IsUsed;
				}
				return true;
			}
		}

		protected InterfaceMemberBase(TypeDefinition parent, FullNamedExpression type, Modifiers mod, Modifiers allowed_mod, MemberName name, Attributes attrs)
			: base(parent, type, mod, allowed_mod, Modifiers.PRIVATE, name, attrs)
		{
			IsInterface = parent.Kind == MemberKind.Interface;
			IsExplicitImpl = base.MemberName.ExplicitInterface != null;
			explicit_mod_flags = mod;
		}

		protected override bool CheckBase()
		{
			if (!base.CheckBase())
			{
				return false;
			}
			if ((caching_flags & Flags.MethodOverloadsExist) != 0)
			{
				CheckForDuplications();
			}
			if (IsExplicitImpl)
			{
				return true;
			}
			if (Parent.BaseType == null)
			{
				return true;
			}
			bool overrides = false;
			MemberSpec bestCandidate;
			MemberSpec memberSpec = FindBaseMember(out bestCandidate, ref overrides);
			if ((base.ModFlags & Modifiers.OVERRIDE) != 0)
			{
				if (memberSpec == null)
				{
					if (bestCandidate == null)
					{
						if (this is Method && ((Method)this).ParameterInfo.IsEmpty && base.MemberName.Name == Destructor.MetadataName && base.MemberName.Arity == 0)
						{
							base.Report.Error(249, base.Location, "Do not override `{0}'. Use destructor syntax instead", "object.Finalize()");
						}
						else
						{
							base.Report.Error(115, base.Location, "`{0}' is marked as an override but no suitable {1} found to override", GetSignatureForError(), ATypeNameExpression.GetMemberType(this));
						}
					}
					else
					{
						base.Report.SymbolRelatedToPreviousError(bestCandidate);
						if (this is Event)
						{
							base.Report.Error(72, base.Location, "`{0}': cannot override because `{1}' is not an event", GetSignatureForError(), TypeManager.GetFullNameSignature(bestCandidate));
						}
						else if (this is PropertyBase)
						{
							base.Report.Error(544, base.Location, "`{0}': cannot override because `{1}' is not a property", GetSignatureForError(), TypeManager.GetFullNameSignature(bestCandidate));
						}
						else
						{
							base.Report.Error(505, base.Location, "`{0}': cannot override because `{1}' is not a method", GetSignatureForError(), TypeManager.GetFullNameSignature(bestCandidate));
						}
					}
					return false;
				}
				if (bestCandidate != null)
				{
					base.Report.SymbolRelatedToPreviousError(bestCandidate);
					base.Report.SymbolRelatedToPreviousError(memberSpec);
					MemberSpec member = MemberCache.GetMember(memberSpec.DeclaringType.GetDefinition(), memberSpec);
					MemberSpec member2 = MemberCache.GetMember(bestCandidate.DeclaringType.GetDefinition(), bestCandidate);
					base.Report.Error(462, base.Location, "`{0}' cannot override inherited members `{1}' and `{2}' because they have the same signature when used in type `{3}'", GetSignatureForError(), member.GetSignatureForError(), member2.GetSignatureForError(), Parent.GetSignatureForError());
				}
				if (!CheckOverrideAgainstBase(memberSpec))
				{
					return false;
				}
				if (memberSpec.GetAttributeObsolete() != null)
				{
					if (base.OptAttributes == null || !base.OptAttributes.Contains(Module.PredefinedAttributes.Obsolete))
					{
						base.Report.SymbolRelatedToPreviousError(memberSpec);
						base.Report.Warning(672, 1, base.Location, "Member `{0}' overrides obsolete member `{1}'. Add the Obsolete attribute to `{0}'", GetSignatureForError(), memberSpec.GetSignatureForError());
					}
				}
				else if (base.OptAttributes != null && base.OptAttributes.Contains(Module.PredefinedAttributes.Obsolete))
				{
					base.Report.SymbolRelatedToPreviousError(memberSpec);
					base.Report.Warning(809, 1, base.Location, "Obsolete member `{0}' overrides non-obsolete member `{1}'", GetSignatureForError(), memberSpec.GetSignatureForError());
				}
				base_method = memberSpec as MethodSpec;
				return true;
			}
			if (memberSpec == null && bestCandidate != null && (!(bestCandidate is IParametersMember) || !(this is IParametersMember)))
			{
				memberSpec = bestCandidate;
			}
			if (memberSpec == null)
			{
				if ((base.ModFlags & Modifiers.NEW) != 0 && memberSpec == null)
				{
					base.Report.Warning(109, 4, base.Location, "The member `{0}' does not hide an inherited member. The new keyword is not required", GetSignatureForError());
				}
			}
			else
			{
				if ((base.ModFlags & Modifiers.NEW) == 0)
				{
					base.ModFlags |= Modifiers.NEW;
					if (!base.IsCompilerGenerated)
					{
						base.Report.SymbolRelatedToPreviousError(memberSpec);
						if ((memberSpec.Kind & MemberKind.NestedMask) == 0 && !IsInterface && (memberSpec.Modifiers & (Modifiers.ABSTRACT | Modifiers.VIRTUAL | Modifiers.OVERRIDE)) != 0)
						{
							base.Report.Warning(114, 2, base.Location, "`{0}' hides inherited member `{1}'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword", GetSignatureForError(), memberSpec.GetSignatureForError());
						}
						else
						{
							base.Report.Warning(108, 2, base.Location, "`{0}' hides inherited member `{1}'. Use the new keyword if hiding was intended", GetSignatureForError(), memberSpec.GetSignatureForError());
						}
					}
				}
				if (!IsInterface && memberSpec.IsAbstract && !overrides && !base.IsStatic)
				{
					switch (memberSpec.Kind)
					{
					case MemberKind.Event:
					case MemberKind.Method:
					case MemberKind.Property:
					case MemberKind.Indexer:
						base.Report.SymbolRelatedToPreviousError(memberSpec);
						base.Report.Error(533, base.Location, "`{0}' hides inherited abstract member `{1}'", GetSignatureForError(), memberSpec.GetSignatureForError());
						break;
					}
				}
			}
			return true;
		}

		protected virtual bool CheckForDuplications()
		{
			return Parent.MemberCache.CheckExistingMembersOverloads(this, ParametersCompiled.EmptyReadOnlyParameters);
		}

		protected virtual bool CheckOverrideAgainstBase(MemberSpec base_member)
		{
			bool result = true;
			if ((base_member.Modifiers & (Modifiers.ABSTRACT | Modifiers.VIRTUAL | Modifiers.OVERRIDE)) == 0)
			{
				base.Report.SymbolRelatedToPreviousError(base_member);
				base.Report.Error(506, base.Location, "`{0}': cannot override inherited member `{1}' because it is not marked virtual, abstract or override", GetSignatureForError(), TypeManager.CSharpSignature(base_member));
				result = false;
			}
			if ((base_member.Modifiers & Modifiers.SEALED) != 0)
			{
				base.Report.SymbolRelatedToPreviousError(base_member);
				base.Report.Error(239, base.Location, "`{0}': cannot override inherited member `{1}' because it is sealed", GetSignatureForError(), TypeManager.CSharpSignature(base_member));
				result = false;
			}
			TypeSpec memberType = ((IInterfaceMemberSpec)base_member).MemberType;
			if (!TypeSpecComparer.Override.IsEqual(base.MemberType, memberType))
			{
				base.Report.SymbolRelatedToPreviousError(base_member);
				if (this is PropertyBasedMember)
				{
					base.Report.Error(1715, base.Location, "`{0}': type must be `{1}' to match overridden member `{2}'", GetSignatureForError(), memberType.GetSignatureForError(), base_member.GetSignatureForError());
				}
				else
				{
					base.Report.Error(508, base.Location, "`{0}': return type must be `{1}' to match overridden member `{2}'", GetSignatureForError(), memberType.GetSignatureForError(), base_member.GetSignatureForError());
				}
				result = false;
			}
			return result;
		}

		protected static bool CheckAccessModifiers(MemberCore this_member, MemberSpec base_member)
		{
			Modifiers modifiers = this_member.ModFlags & Modifiers.AccessibilityMask;
			Modifiers modifiers2 = base_member.Modifiers & Modifiers.AccessibilityMask;
			if ((modifiers2 & (Modifiers.PROTECTED | Modifiers.INTERNAL)) == (Modifiers.PROTECTED | Modifiers.INTERNAL))
			{
				if ((modifiers & Modifiers.PROTECTED) == 0)
				{
					return false;
				}
				if ((modifiers & Modifiers.INTERNAL) != 0)
				{
					return base_member.DeclaringType.MemberDefinition.IsInternalAsPublic(this_member.Module.DeclaringAssembly);
				}
				if (base_member.DeclaringType.MemberDefinition.IsInternalAsPublic(this_member.Module.DeclaringAssembly))
				{
					return false;
				}
				return true;
			}
			return modifiers == modifiers2;
		}

		public override bool Define()
		{
			if (IsInterface)
			{
				base.ModFlags = Modifiers.PUBLIC | Modifiers.ABSTRACT | Modifiers.VIRTUAL | (base.ModFlags & (Modifiers.NEW | Modifiers.UNSAFE));
				flags = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask | MethodAttributes.Abstract;
			}
			else
			{
				Parent.PartialContainer.MethodModifiersValid(this);
				flags = ModifiersExtensions.MethodAttr(base.ModFlags);
			}
			if (IsExplicitImpl)
			{
				InterfaceType = base.MemberName.ExplicitInterface.ResolveAsType(Parent);
				if (InterfaceType == null)
				{
					return false;
				}
				if ((base.ModFlags & Modifiers.PARTIAL) != 0)
				{
					base.Report.Error(754, base.Location, "A partial method `{0}' cannot explicitly implement an interface", GetSignatureForError());
				}
				if (!InterfaceType.IsInterface)
				{
					base.Report.SymbolRelatedToPreviousError(InterfaceType);
					base.Report.Error(538, base.Location, "The type `{0}' in explicit interface declaration is not an interface", InterfaceType.GetSignatureForError());
				}
				else
				{
					Parent.PartialContainer.VerifyImplements(this);
				}
				Modifiers modifiers = Modifiers.AllowedExplicitImplFlags;
				if (this is Method)
				{
					modifiers |= Modifiers.ASYNC;
				}
				ModifiersExtensions.Check(modifiers, explicit_mod_flags, (Modifiers)0, base.Location, base.Report);
			}
			return base.Define();
		}

		protected bool DefineParameters(ParametersCompiled parameters)
		{
			if (!parameters.Resolve(this))
			{
				return false;
			}
			bool flag = false;
			for (int i = 0; i < parameters.Count; i++)
			{
				Parameter parameter = parameters[i];
				if (parameter.HasDefaultValue && (IsExplicitImpl || this is Operator || (this is Indexer && parameters.Count == 1)))
				{
					parameter.Warning_UselessOptionalParameter(base.Report);
				}
				if (!parameter.CheckAccessibility(this))
				{
					TypeSpec typeSpec = parameters.Types[i];
					base.Report.SymbolRelatedToPreviousError(typeSpec);
					if (this is Indexer)
					{
						base.Report.Error(55, base.Location, "Inconsistent accessibility: parameter type `{0}' is less accessible than indexer `{1}'", typeSpec.GetSignatureForError(), GetSignatureForError());
					}
					else if (this is Operator)
					{
						base.Report.Error(57, base.Location, "Inconsistent accessibility: parameter type `{0}' is less accessible than operator `{1}'", typeSpec.GetSignatureForError(), GetSignatureForError());
					}
					else
					{
						base.Report.Error(51, base.Location, "Inconsistent accessibility: parameter type `{0}' is less accessible than method `{1}'", typeSpec.GetSignatureForError(), GetSignatureForError());
					}
					flag = true;
				}
			}
			return !flag;
		}

		protected override void DoMemberTypeDependentChecks()
		{
			base.DoMemberTypeDependentChecks();
			VarianceDecl.CheckTypeVariance(base.MemberType, ExpectedMemberTypeVariance, this);
		}

		public override void Emit()
		{
			if ((base.ModFlags & Modifiers.EXTERN) != 0 && !is_external_implementation && (base.OptAttributes == null || !base.OptAttributes.HasResolveError()))
			{
				if (this is Constructor)
				{
					base.Report.Warning(824, 1, base.Location, "Constructor `{0}' is marked `external' but has no external implementation specified", GetSignatureForError());
				}
				else
				{
					base.Report.Warning(626, 1, base.Location, "`{0}' is marked as an external but has no DllImport attribute. Consider adding a DllImport attribute to specify the external implementation", GetSignatureForError());
				}
			}
			base.Emit();
		}

		public override bool EnableOverloadChecks(MemberCore overload)
		{
			InterfaceMemberBase interfaceMemberBase = overload as InterfaceMemberBase;
			if (interfaceMemberBase != null && interfaceMemberBase.IsExplicitImpl)
			{
				if (IsExplicitImpl)
				{
					caching_flags |= Flags.MethodOverloadsExist;
				}
				return true;
			}
			return IsExplicitImpl;
		}

		protected void Error_CannotChangeAccessModifiers(MemberCore member, MemberSpec base_member)
		{
			Modifiers modifiers = base_member.Modifiers;
			if ((modifiers & Modifiers.AccessibilityMask) == (Modifiers.PROTECTED | Modifiers.INTERNAL) && !base_member.DeclaringType.MemberDefinition.IsInternalAsPublic(member.Module.DeclaringAssembly))
			{
				modifiers = Modifiers.PROTECTED;
			}
			base.Report.SymbolRelatedToPreviousError(base_member);
			base.Report.Error(507, member.Location, "`{0}': cannot change access modifiers when overriding `{1}' inherited member `{2}'", member.GetSignatureForError(), ModifiersExtensions.AccessibilityName(modifiers), base_member.GetSignatureForError());
		}

		protected void Error_StaticReturnType()
		{
			base.Report.Error(722, base.Location, "`{0}': static types cannot be used as return types", base.MemberType.GetSignatureForError());
		}

		protected virtual MemberSpec FindBaseMember(out MemberSpec bestCandidate, ref bool overrides)
		{
			return MemberCache.FindBaseMember(this, out bestCandidate, ref overrides);
		}

		public string GetFullName(MemberName name)
		{
			return GetFullName(name.Name);
		}

		public string GetFullName(string name)
		{
			if (!IsExplicitImpl)
			{
				return name;
			}
			return InterfaceType.GetSignatureForError() + "." + name;
		}

		public override string GetSignatureForDocumentation()
		{
			if (IsExplicitImpl)
			{
				return Parent.GetSignatureForDocumentation() + "." + InterfaceType.GetSignatureForDocumentation(true) + "#" + ShortName;
			}
			return Parent.GetSignatureForDocumentation() + "." + ShortName;
		}

		public override void SetConstraints(List<Constraints> constraints_list)
		{
			if ((base.ModFlags & Modifiers.OVERRIDE) != 0 || IsExplicitImpl)
			{
				base.Report.Error(460, base.Location, "`{0}': Cannot specify constraints for overrides and explicit interface implementation methods", GetSignatureForError());
			}
			base.SetConstraints(constraints_list);
		}
	}
}
