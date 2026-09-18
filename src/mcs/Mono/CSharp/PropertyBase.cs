using System;
using System.Reflection;
using System.Reflection.Emit;
using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp
{
	public abstract class PropertyBase : PropertyBasedMember
	{
		public class GetMethod : PropertyMethod
		{
			private static readonly string[] attribute_targets = new string[2] { "method", "return" };

			public const string Prefix = "get_";

			public override TypeSpec ReturnType => method.MemberType;

			public override ParametersCompiled ParameterInfo => ParametersCompiled.EmptyReadOnlyParameters;

			public override string[] ValidAttributeTargets => attribute_targets;

			public GetMethod(PropertyBase method, Modifiers modifiers, Attributes attrs, Location loc)
				: base(method, "get_", modifiers, attrs, loc)
			{
			}

			public override void Define(TypeContainer parent)
			{
				base.Define(parent);
				base.Spec = new MethodSpec(MemberKind.Method, parent.PartialContainer.Definition, this, ReturnType, ParameterInfo, base.ModFlags);
				method_data = new MethodData(method, base.ModFlags, flags, this);
				method_data.Define(parent.PartialContainer, method.GetFullName(base.MemberName));
			}
		}

		public class SetMethod : PropertyMethod
		{
			private static readonly string[] attribute_targets = new string[3] { "method", "param", "return" };

			public const string Prefix = "set_";

			protected ParametersCompiled parameters;

			public override ParametersCompiled ParameterInfo => parameters;

			public override TypeSpec ReturnType => Parent.Compiler.BuiltinTypes.Void;

			public override string[] ValidAttributeTargets => attribute_targets;

			public SetMethod(PropertyBase method, Modifiers modifiers, ParametersCompiled parameters, Attributes attrs, Location loc)
				: base(method, "set_", modifiers, attrs, loc)
			{
				this.parameters = parameters;
			}

			protected override void ApplyToExtraTarget(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
			{
				if (a.Target == AttributeTargets.Parameter)
				{
					parameters[parameters.Count - 1].ApplyAttributeBuilder(a, ctor, cdata, pa);
				}
				else
				{
					base.ApplyToExtraTarget(a, ctor, cdata, pa);
				}
			}

			public override void Define(TypeContainer parent)
			{
				parameters.Resolve(this);
				base.Define(parent);
				base.Spec = new MethodSpec(MemberKind.Method, parent.PartialContainer.Definition, this, ReturnType, ParameterInfo, base.ModFlags);
				method_data = new MethodData(method, base.ModFlags, flags, this);
				method_data.Define(parent.PartialContainer, method.GetFullName(base.MemberName));
			}
		}

		public abstract class PropertyMethod : AbstractPropertyEventMethod
		{
			private const Modifiers AllowedModifiers = Modifiers.AccessibilityMask;

			protected readonly PropertyBase method;

			protected MethodAttributes flags;

			public override AttributeTargets AttributeTargets => AttributeTargets.Method;

			public bool HasCustomAccessModifier => (base.ModFlags & Modifiers.PROPERTY_CUSTOM) != 0;

			public PropertyBase Property => method;

			public PropertyMethod(PropertyBase method, string prefix, Modifiers modifiers, Attributes attrs, Location loc)
				: base(method, prefix, attrs, loc)
			{
				this.method = method;
				base.ModFlags = ModifiersExtensions.Check(Modifiers.AccessibilityMask, modifiers, (Modifiers)0, loc, base.Report);
				base.ModFlags |= method.ModFlags & (Modifiers.STATIC | Modifiers.UNSAFE);
			}

			public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
			{
				if (a.Type == pa.MethodImpl)
				{
					method.is_external_implementation = a.IsInternalCall();
				}
				base.ApplyAttributeBuilder(a, ctor, cdata, pa);
			}

			public override bool IsClsComplianceRequired()
			{
				return method.IsClsComplianceRequired();
			}

			public virtual void Define(TypeContainer parent)
			{
				TypeDefinition partialContainer = parent.PartialContainer;
				if ((base.ModFlags & Modifiers.AccessibilityMask) == 0)
				{
					base.ModFlags |= method.ModFlags;
					flags = method.flags;
				}
				else
				{
					CheckModifiers(base.ModFlags);
					base.ModFlags |= method.ModFlags & ~Modifiers.AccessibilityMask;
					base.ModFlags |= Modifiers.PROPERTY_CUSTOM;
					if (partialContainer.Kind == MemberKind.Interface)
					{
						base.Report.Error(275, base.Location, "`{0}': accessibility modifiers may not be used on accessors in an interface", GetSignatureForError());
					}
					else if ((base.ModFlags & Modifiers.PRIVATE) != 0)
					{
						if ((method.ModFlags & Modifiers.ABSTRACT) != 0)
						{
							base.Report.Error(442, base.Location, "`{0}': abstract properties cannot have private accessors", GetSignatureForError());
						}
						base.ModFlags &= ~Modifiers.VIRTUAL;
					}
					flags = ModifiersExtensions.MethodAttr(base.ModFlags) | MethodAttributes.SpecialName;
				}
				CheckAbstractAndExtern(block != null);
				CheckProtectedModifier();
				if (block != null)
				{
					if (block.IsIterator)
					{
						Iterator.CreateIterator(this, Parent.PartialContainer, base.ModFlags);
					}
					if (Compiler.Settings.WriteMetadataOnly)
					{
						block = null;
					}
				}
			}

			public override ObsoleteAttribute GetAttributeObsolete()
			{
				return method.GetAttributeObsolete();
			}

			public override string GetSignatureForError()
			{
				return method.GetSignatureForError() + "." + prefix.Substring(0, 3);
			}

			private void CheckModifiers(Modifiers modflags)
			{
				if (!ModifiersExtensions.IsRestrictedModifier(modflags & Modifiers.AccessibilityMask, method.ModFlags & Modifiers.AccessibilityMask))
				{
					base.Report.Error(273, base.Location, "The accessibility modifier of the `{0}' accessor must be more restrictive than the modifier of the property or indexer `{1}'", GetSignatureForError(), method.GetSignatureForError());
				}
			}
		}

		private static readonly string[] attribute_targets = new string[1] { "property" };

		private PropertyMethod get;

		private PropertyMethod set;

		private PropertyMethod first;

		private PropertyBuilder PropertyBuilder;

		public override AttributeTargets AttributeTargets => AttributeTargets.Property;

		public PropertyMethod AccessorFirst => first;

		public PropertyMethod AccessorSecond
		{
			get
			{
				if (first != get)
				{
					return get;
				}
				return set;
			}
		}

		public override Variance ExpectedMemberTypeVariance
		{
			get
			{
				if (get == null || set == null)
				{
					if (set != null)
					{
						return Variance.Contravariant;
					}
					return Variance.Covariant;
				}
				return Variance.None;
			}
		}

		public PropertyMethod Get
		{
			get
			{
				return get;
			}
			set
			{
				get = value;
				if (first == null)
				{
					first = value;
				}
				Parent.AddNameToContainer(get, get.MemberName.Basename);
			}
		}

		public PropertyMethod Set
		{
			get
			{
				return set;
			}
			set
			{
				set = value;
				if (first == null)
				{
					first = value;
				}
				Parent.AddNameToContainer(set, set.MemberName.Basename);
			}
		}

		public override string[] ValidAttributeTargets => attribute_targets;

		public override bool IsUsed
		{
			get
			{
				if (IsExplicitImpl)
				{
					return true;
				}
				return Get.IsUsed | Set.IsUsed;
			}
		}

		public override string DocCommentHeader => "P:";

		protected PropertyBase(TypeDefinition parent, FullNamedExpression type, Modifiers mod_flags, Modifiers allowed_mod, MemberName name, Attributes attrs)
			: base(parent, type, mod_flags, allowed_mod, name, attrs)
		{
		}

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.HasSecurityAttribute)
			{
				a.Error_InvalidSecurityParent();
			}
			else if (a.Type == pa.Dynamic)
			{
				a.Error_MisusedDynamicAttribute();
			}
			else
			{
				PropertyBuilder.SetCustomAttribute((ConstructorInfo)ctor.GetMetaInfo(), cdata);
			}
		}

		private void CheckMissingAccessor(MemberKind kind, ParametersCompiled parameters, bool get)
		{
			if (!IsExplicitImpl)
			{
				return;
			}
			PropertySpec propertySpec = MemberCache.FindMember(filter: (kind != MemberKind.Indexer) ? new MemberFilter(base.MemberName.Name, 0, kind, null, null) : new MemberFilter(MemberCache.IndexerNameAlias, 0, kind, parameters, null), container: InterfaceType, restrictions: BindingRestriction.DeclaredOnly) as PropertySpec;
			if (propertySpec != null)
			{
				MethodSpec methodSpec = (get ? propertySpec.Get : propertySpec.Set);
				if (methodSpec != null)
				{
					base.Report.SymbolRelatedToPreviousError(methodSpec);
					base.Report.Error(551, base.Location, "Explicit interface implementation `{0}' is missing accessor `{1}'", GetSignatureForError(), methodSpec.GetSignatureForError());
				}
			}
		}

		protected override bool CheckOverrideAgainstBase(MemberSpec base_member)
		{
			bool flag = base.CheckOverrideAgainstBase(base_member);
			PropertySpec propertySpec = (PropertySpec)base_member;
			if (Get == null)
			{
				if ((base.ModFlags & Modifiers.SEALED) != 0 && propertySpec.HasGet && !propertySpec.Get.IsAccessible(this))
				{
					base.Report.SymbolRelatedToPreviousError(propertySpec);
					base.Report.Error(545, base.Location, "`{0}': cannot override because `{1}' does not have accessible get accessor", GetSignatureForError(), propertySpec.GetSignatureForError());
					flag = false;
				}
			}
			else if (!propertySpec.HasGet)
			{
				if (flag)
				{
					base.Report.SymbolRelatedToPreviousError(propertySpec);
					base.Report.Error(545, Get.Location, "`{0}': cannot override because `{1}' does not have an overridable get accessor", Get.GetSignatureForError(), propertySpec.GetSignatureForError());
					flag = false;
				}
			}
			else if (Get.HasCustomAccessModifier || propertySpec.HasDifferentAccessibility)
			{
				if (!propertySpec.Get.IsAccessible(this))
				{
					base.Report.Error(115, Get.Location, "`{0}' is marked as an override but no accessible `get' accessor found to override", GetSignatureForError());
					flag = false;
				}
				else if (!InterfaceMemberBase.CheckAccessModifiers(Get, propertySpec.Get))
				{
					Error_CannotChangeAccessModifiers(Get, propertySpec.Get);
					flag = false;
				}
			}
			if (Set == null)
			{
				if (propertySpec.HasSet)
				{
					if ((base.ModFlags & Modifiers.SEALED) != 0 && !propertySpec.Set.IsAccessible(this))
					{
						base.Report.SymbolRelatedToPreviousError(propertySpec);
						base.Report.Error(546, base.Location, "`{0}': cannot override because `{1}' does not have accessible set accessor", GetSignatureForError(), propertySpec.GetSignatureForError());
						flag = false;
					}
					if ((base.ModFlags & Modifiers.AutoProperty) != 0)
					{
						base.Report.Error(8080, base.Location, "`{0}': Auto-implemented properties must override all accessors of the overridden property", GetSignatureForError());
						flag = false;
					}
				}
			}
			else if (!propertySpec.HasSet)
			{
				if (flag)
				{
					base.Report.SymbolRelatedToPreviousError(propertySpec);
					base.Report.Error(546, Set.Location, "`{0}': cannot override because `{1}' does not have an overridable set accessor", Set.GetSignatureForError(), propertySpec.GetSignatureForError());
					flag = false;
				}
			}
			else if (Set.HasCustomAccessModifier || propertySpec.HasDifferentAccessibility)
			{
				if (!propertySpec.Set.IsAccessible(this))
				{
					base.Report.Error(115, Set.Location, "`{0}' is marked as an override but no accessible `set' accessor found to override", GetSignatureForError());
					flag = false;
				}
				else if (!InterfaceMemberBase.CheckAccessModifiers(Set, propertySpec.Set))
				{
					Error_CannotChangeAccessModifiers(Set, propertySpec.Set);
					flag = false;
				}
			}
			if ((Set == null || !Set.HasCustomAccessModifier) && (Get == null || !Get.HasCustomAccessModifier) && !InterfaceMemberBase.CheckAccessModifiers(this, propertySpec))
			{
				Error_CannotChangeAccessModifiers(this, propertySpec);
				flag = false;
			}
			return flag;
		}

		protected override void DoMemberTypeDependentChecks()
		{
			base.DoMemberTypeDependentChecks();
			IsTypePermitted();
			if (base.MemberType.IsStatic)
			{
				Error_StaticReturnType();
			}
		}

		protected override void DoMemberTypeIndependentChecks()
		{
			base.DoMemberTypeIndependentChecks();
			if (AccessorSecond != null)
			{
				if ((Get.ModFlags & Modifiers.AccessibilityMask) != 0 && (Set.ModFlags & Modifiers.AccessibilityMask) != 0)
				{
					base.Report.Error(274, base.Location, "`{0}': Cannot specify accessibility modifiers for both accessors of the property or indexer", GetSignatureForError());
				}
			}
			else if ((base.ModFlags & Modifiers.OVERRIDE) == 0 && ((Get == null && (Set.ModFlags & Modifiers.AccessibilityMask) != 0) || (Set == null && (Get.ModFlags & Modifiers.AccessibilityMask) != 0)))
			{
				base.Report.Error(276, base.Location, "`{0}': accessibility modifiers on accessors may only be used if the property or indexer has both a get and a set accessor", GetSignatureForError());
			}
		}

		protected bool DefineAccessors()
		{
			first.Define(Parent);
			if (AccessorSecond != null)
			{
				AccessorSecond.Define(Parent);
			}
			return true;
		}

		protected void DefineBuilders(MemberKind kind, ParametersCompiled parameters)
		{
			PropertyBuilder = Parent.TypeBuilder.DefineProperty(GetFullName(base.MemberName), PropertyAttributes.None, base.MemberType.GetMetaInfo(), null, null, parameters.GetMetaInfo(), null, null);
			PropertySpec propertySpec = ((kind != MemberKind.Indexer) ? new PropertySpec(kind, Parent.Definition, this, base.MemberType, PropertyBuilder, base.ModFlags) : new IndexerSpec(Parent.Definition, this, base.MemberType, parameters, PropertyBuilder, base.ModFlags));
			if (Get != null)
			{
				propertySpec.Get = Get.Spec;
				Parent.MemberCache.AddMember(this, Get.Spec.Name, Get.Spec);
			}
			else
			{
				CheckMissingAccessor(kind, parameters, true);
			}
			if (Set != null)
			{
				propertySpec.Set = Set.Spec;
				Parent.MemberCache.AddMember(this, Set.Spec.Name, Set.Spec);
			}
			else
			{
				CheckMissingAccessor(kind, parameters, false);
			}
			Parent.MemberCache.AddMember(this, PropertyBuilder.Name, propertySpec);
		}

		public override void Emit()
		{
			CheckReservedNameConflict("get_", (get == null) ? null : get.Spec);
			CheckReservedNameConflict("set_", (set == null) ? null : set.Spec);
			if (base.OptAttributes != null)
			{
				base.OptAttributes.Emit();
			}
			if (member_type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				Module.PredefinedAttributes.Dynamic.EmitAttribute(PropertyBuilder);
			}
			else if (member_type.HasDynamicElement)
			{
				Module.PredefinedAttributes.Dynamic.EmitAttribute(PropertyBuilder, member_type, base.Location);
			}
			ConstraintChecker.Check(this, member_type, type_expr.Location);
			first.Emit(Parent);
			if (AccessorSecond != null)
			{
				AccessorSecond.Emit(Parent);
			}
			base.Emit();
		}

		public override void PrepareEmit()
		{
			AccessorFirst.PrepareEmit();
			if (AccessorSecond != null)
			{
				AccessorSecond.PrepareEmit();
			}
			if (get != null)
			{
				MethodBuilder methodBuilder = Get.Spec.GetMetaInfo() as MethodBuilder;
				if (methodBuilder != null)
				{
					PropertyBuilder.SetGetMethod(methodBuilder);
				}
			}
			if (set != null)
			{
				MethodBuilder methodBuilder2 = Set.Spec.GetMetaInfo() as MethodBuilder;
				if (methodBuilder2 != null)
				{
					PropertyBuilder.SetSetMethod(methodBuilder2);
				}
			}
		}

		protected override void SetMemberName(MemberName new_name)
		{
			base.SetMemberName(new_name);
			if (Get != null)
			{
				Get.UpdateName(this);
			}
			if (Set != null)
			{
				Set.UpdateName(this);
			}
		}

		public override void WriteDebugSymbol(MonoSymbolFile file)
		{
			if (get != null)
			{
				get.WriteDebugSymbol(file);
			}
			if (set != null)
			{
				set.WriteDebugSymbol(file);
			}
		}
	}
}
