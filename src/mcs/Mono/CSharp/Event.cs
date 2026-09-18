using System;
using System.Reflection;
using System.Reflection.Emit;
using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp
{
	public abstract class Event : PropertyBasedMember
	{
		public abstract class AEventAccessor : AbstractPropertyEventMethod
		{
			protected readonly Event method;

			private readonly ParametersCompiled parameters;

			private static readonly string[] attribute_targets = new string[3] { "method", "param", "return" };

			public const string AddPrefix = "add_";

			public const string RemovePrefix = "remove_";

			public bool IsInterfaceImplementation => method_data.implementing != null;

			public override AttributeTargets AttributeTargets => AttributeTargets.Method;

			public override TypeSpec ReturnType => Parent.Compiler.BuiltinTypes.Void;

			public MethodData MethodData => method_data;

			public override string[] ValidAttributeTargets => attribute_targets;

			public override ParametersCompiled ParameterInfo => parameters;

			protected AEventAccessor(Event method, string prefix, Attributes attrs, Location loc)
				: base(method, prefix, attrs, loc)
			{
				this.method = method;
				base.ModFlags = method.ModFlags;
				parameters = ParametersCompiled.CreateImplicitParameter(method.TypeExpression, loc);
			}

			public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
			{
				if (a.Type == pa.MethodImpl)
				{
					method.is_external_implementation = a.IsInternalCall();
				}
				base.ApplyAttributeBuilder(a, ctor, cdata, pa);
			}

			protected override void ApplyToExtraTarget(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
			{
				if (a.Target == AttributeTargets.Parameter)
				{
					parameters[0].ApplyAttributeBuilder(a, ctor, cdata, pa);
				}
				else
				{
					base.ApplyToExtraTarget(a, ctor, cdata, pa);
				}
			}

			public override bool IsClsComplianceRequired()
			{
				return method.IsClsComplianceRequired();
			}

			public virtual void Define(TypeContainer parent)
			{
				((Parameter)parameters.FixedParameters[0]).Type = method.member_type;
				parameters.Types = new TypeSpec[1] { method.member_type };
				method_data = new MethodData(method, method.ModFlags, method.flags | MethodAttributes.HideBySig | MethodAttributes.SpecialName, this);
				if (method_data.Define(parent.PartialContainer, method.GetFullName(base.MemberName)))
				{
					if (Compiler.Settings.WriteMetadataOnly)
					{
						block = null;
					}
					base.Spec = new MethodSpec(MemberKind.Method, parent.PartialContainer.Definition, this, ReturnType, ParameterInfo, method.ModFlags);
					base.Spec.IsAccessor = true;
				}
			}

			public override ObsoleteAttribute GetAttributeObsolete()
			{
				return method.GetAttributeObsolete();
			}
		}

		private AEventAccessor add;

		private AEventAccessor remove;

		private EventBuilder EventBuilder;

		protected EventSpec spec;

		public override AttributeTargets AttributeTargets => AttributeTargets.Event;

		public AEventAccessor Add
		{
			get
			{
				return add;
			}
			set
			{
				add = value;
				Parent.AddNameToContainer(value, value.MemberName.Basename);
			}
		}

		public override Variance ExpectedMemberTypeVariance => Variance.Contravariant;

		public AEventAccessor Remove
		{
			get
			{
				return remove;
			}
			set
			{
				remove = value;
				Parent.AddNameToContainer(value, value.MemberName.Basename);
			}
		}

		public override string DocCommentHeader => "E:";

		protected Event(TypeDefinition parent, FullNamedExpression type, Modifiers mod_flags, MemberName name, Attributes attrs)
			: base(parent, type, mod_flags, (parent.PartialContainer.Kind == MemberKind.Interface) ? (Modifiers.NEW | Modifiers.UNSAFE) : ((parent.PartialContainer.Kind == MemberKind.Struct) ? (Modifiers.AccessibilityMask | Modifiers.AllowedExplicitImplFlags | Modifiers.NEW | Modifiers.STATIC | Modifiers.OVERRIDE) : (Modifiers.AccessibilityMask | Modifiers.AllowedExplicitImplFlags | Modifiers.NEW | Modifiers.ABSTRACT | Modifiers.SEALED | Modifiers.STATIC | Modifiers.VIRTUAL | Modifiers.OVERRIDE)), name, attrs)
		{
		}

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.HasSecurityAttribute)
			{
				a.Error_InvalidSecurityParent();
			}
			else
			{
				EventBuilder.SetCustomAttribute((ConstructorInfo)ctor.GetMetaInfo(), cdata);
			}
		}

		protected override bool CheckOverrideAgainstBase(MemberSpec base_member)
		{
			bool result = base.CheckOverrideAgainstBase(base_member);
			if (!InterfaceMemberBase.CheckAccessModifiers(this, base_member))
			{
				Error_CannotChangeAccessModifiers(this, base_member);
				result = false;
			}
			return result;
		}

		public override bool Define()
		{
			if (!base.Define())
			{
				return false;
			}
			if (!base.MemberType.IsDelegate)
			{
				base.Report.Error(66, base.Location, "`{0}': event must be of a delegate type", GetSignatureForError());
			}
			if (!CheckBase())
			{
				return false;
			}
			add.Define(Parent);
			remove.Define(Parent);
			EventBuilder = Parent.TypeBuilder.DefineEvent(GetFullName(base.MemberName), EventAttributes.None, base.MemberType.GetMetaInfo());
			spec = new EventSpec(Parent.Definition, this, base.MemberType, base.ModFlags, Add.Spec, remove.Spec);
			Parent.MemberCache.AddMember(this, GetFullName(base.MemberName), spec);
			Parent.MemberCache.AddMember(this, Add.Spec.Name, Add.Spec);
			Parent.MemberCache.AddMember(this, Remove.Spec.Name, remove.Spec);
			return true;
		}

		public override void Emit()
		{
			CheckReservedNameConflict(null, add.Spec);
			CheckReservedNameConflict(null, remove.Spec);
			if (base.OptAttributes != null)
			{
				base.OptAttributes.Emit();
			}
			ConstraintChecker.Check(this, member_type, type_expr.Location);
			Add.Emit(Parent);
			Remove.Emit(Parent);
			base.Emit();
		}

		public override void PrepareEmit()
		{
			add.PrepareEmit();
			remove.PrepareEmit();
			EventBuilder.SetAddOnMethod(add.MethodData.MethodBuilder);
			EventBuilder.SetRemoveOnMethod(remove.MethodData.MethodBuilder);
		}

		public override void WriteDebugSymbol(MonoSymbolFile file)
		{
			add.WriteDebugSymbol(file);
			remove.WriteDebugSymbol(file);
		}
	}
}
