using System;
using System.Reflection;

namespace Mono.CSharp
{
	public class Property : PropertyBase
	{
		public sealed class BackingFieldDeclaration : Field
		{
			private readonly Property property;

			private const Modifiers DefaultModifiers = Modifiers.PRIVATE | Modifiers.COMPILER_GENERATED | Modifiers.BACKING_FIELD | Modifiers.DEBUGGER_HIDDEN;

			public Property OriginalProperty => property;

			public BackingFieldDeclaration(Property p, bool readOnly)
				: base(p.Parent, p.type_expr, Modifiers.PRIVATE | Modifiers.COMPILER_GENERATED | Modifiers.BACKING_FIELD | Modifiers.DEBUGGER_HIDDEN | (p.ModFlags & (Modifiers.STATIC | Modifiers.UNSAFE)), new MemberName("<" + p.GetFullName(p.MemberName) + ">k__BackingField", p.Location), null)
			{
				property = p;
				if (readOnly)
				{
					base.ModFlags |= Modifiers.READONLY;
				}
			}

			public override string GetSignatureForError()
			{
				return property.GetSignatureForError();
			}
		}

		private static readonly string[] attribute_target_auto = new string[2] { "property", "field" };

		public BackingFieldDeclaration BackingField { get; private set; }

		public Expression Initializer { get; set; }

		public override string[] ValidAttributeTargets
		{
			get
			{
				if (base.Get == null || (base.Get.ModFlags & Modifiers.COMPILER_GENERATED) == 0)
				{
					return base.ValidAttributeTargets;
				}
				return attribute_target_auto;
			}
		}

		public Property(TypeDefinition parent, FullNamedExpression type, Modifiers mod, MemberName name, Attributes attrs)
			: base(parent, type, mod, (parent.PartialContainer.Kind == MemberKind.Interface) ? (Modifiers.NEW | Modifiers.UNSAFE) : ((parent.PartialContainer.Kind == MemberKind.Struct) ? (Modifiers.AccessibilityMask | Modifiers.AllowedExplicitImplFlags | Modifiers.NEW | Modifiers.STATIC | Modifiers.OVERRIDE) : (Modifiers.AccessibilityMask | Modifiers.AllowedExplicitImplFlags | Modifiers.NEW | Modifiers.ABSTRACT | Modifiers.SEALED | Modifiers.STATIC | Modifiers.VIRTUAL | Modifiers.OVERRIDE)), name, attrs)
		{
		}

		public override void Accept(StructuralVisitor visitor)
		{
			visitor.Visit(this);
		}

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Target == AttributeTargets.Field)
			{
				BackingField.ApplyAttributeBuilder(a, ctor, cdata, pa);
			}
			else
			{
				base.ApplyAttributeBuilder(a, ctor, cdata, pa);
			}
		}

		private void CreateAutomaticProperty()
		{
			BackingField = new BackingFieldDeclaration(this, Initializer == null && base.Set == null);
			if (BackingField.Define())
			{
				if (Initializer != null)
				{
					BackingField.Initializer = Initializer;
					Parent.RegisterFieldForInitialization(BackingField, new FieldInitializer(BackingField, Initializer, base.Location));
					BackingField.ModFlags |= Modifiers.READONLY;
				}
				Parent.PartialContainer.Members.Add(BackingField);
				FieldExpr fieldExpr = new FieldExpr(BackingField, base.Location);
				if ((BackingField.ModFlags & Modifiers.STATIC) == 0)
				{
					fieldExpr.InstanceExpression = new CompilerGeneratedThis(Parent.CurrentType, base.Location);
				}
				base.Get.Block = new ToplevelBlock(Compiler, ParametersCompiled.EmptyReadOnlyParameters, Location.Null);
				Return s = new Return(fieldExpr, base.Get.Location);
				base.Get.Block.AddStatement(s);
				base.Get.ModFlags |= Modifiers.COMPILER_GENERATED;
				if (base.Set != null)
				{
					base.Set.Block = new ToplevelBlock(Compiler, base.Set.ParameterInfo, Location.Null);
					Assign expr = new SimpleAssign(fieldExpr, new SimpleName("value", Location.Null), Location.Null);
					base.Set.Block.AddStatement(new StatementExpression(expr, base.Set.Location));
					base.Set.ModFlags |= Modifiers.COMPILER_GENERATED;
				}
			}
		}

		public override bool Define()
		{
			if (!base.Define())
			{
				return false;
			}
			flags |= MethodAttributes.HideBySig | MethodAttributes.SpecialName;
			bool flag = base.AccessorFirst.Block == null && (base.AccessorSecond == null || base.AccessorSecond.Block == null) && (base.ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN)) == 0;
			if (Initializer != null)
			{
				if (!flag)
				{
					base.Report.Error(8050, base.Location, "`{0}': Only auto-implemented properties can have initializers", GetSignatureForError());
				}
				if (IsInterface)
				{
					base.Report.Error(8052, base.Location, "`{0}': Properties inside interfaces cannot have initializers", GetSignatureForError());
				}
				if (Compiler.Settings.Version < LanguageVersion.V_6)
				{
					base.Report.FeatureIsNotAvailable(Compiler, base.Location, "auto-implemented property initializer");
				}
			}
			if (flag)
			{
				base.ModFlags |= Modifiers.AutoProperty;
				if (base.Get == null)
				{
					base.Report.Error(8051, base.Location, "Auto-implemented property `{0}' must have get accessor", GetSignatureForError());
					return false;
				}
				if (Compiler.Settings.Version < LanguageVersion.V_3 && Initializer == null)
				{
					base.Report.FeatureIsNotAvailable(Compiler, base.Location, "auto-implemented properties");
				}
				CreateAutomaticProperty();
			}
			if (!DefineAccessors())
			{
				return false;
			}
			if (base.AccessorSecond == null)
			{
				PropertyMethod propertyMethod = ((!(base.AccessorFirst is GetMethod)) ? ((PropertyMethod)new GetMethod(this, (Modifiers)0, null, base.Location)) : ((PropertyMethod)new SetMethod(this, (Modifiers)0, ParametersCompiled.EmptyReadOnlyParameters, null, base.Location)));
				Parent.AddNameToContainer(propertyMethod, propertyMethod.MemberName.Basename);
			}
			if (!CheckBase())
			{
				return false;
			}
			DefineBuilders(MemberKind.Property, ParametersCompiled.EmptyReadOnlyParameters);
			return true;
		}

		public override void Emit()
		{
			if ((base.AccessorFirst.ModFlags & (Modifiers.STATIC | Modifiers.COMPILER_GENERATED)) == Modifiers.COMPILER_GENERATED && Parent.PartialContainer.HasExplicitLayout)
			{
				base.Report.Error(842, base.Location, "Automatically implemented property `{0}' cannot be used inside a type with an explicit StructLayout attribute", GetSignatureForError());
			}
			base.Emit();
		}
	}
}
