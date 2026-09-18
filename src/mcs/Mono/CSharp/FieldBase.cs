using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public abstract class FieldBase : MemberBase
	{
		[Flags]
		public enum Status : byte
		{
			HAS_OFFSET = 4
		}

		protected FieldBuilder FieldBuilder;

		protected FieldSpec spec;

		public Status status;

		protected Expression initializer;

		protected List<FieldDeclarator> declarators;

		private static readonly string[] attribute_targets = new string[1] { "field" };

		public override AttributeTargets AttributeTargets => AttributeTargets.Field;

		public List<FieldDeclarator> Declarators => declarators;

		public Expression Initializer
		{
			get
			{
				return initializer;
			}
			set
			{
				initializer = value;
			}
		}

		public string Name => base.MemberName.Name;

		public FieldSpec Spec => spec;

		public override string[] ValidAttributeTargets => attribute_targets;

		public override string DocCommentHeader => "F:";

		protected FieldBase(TypeDefinition parent, FullNamedExpression type, Modifiers mod, Modifiers allowed_mod, MemberName name, Attributes attrs)
			: base(parent, type, mod, allowed_mod | Modifiers.ABSTRACT, Modifiers.PRIVATE, name, attrs)
		{
			if ((mod & Modifiers.ABSTRACT) != 0)
			{
				base.Report.Error(681, base.Location, "The modifier 'abstract' is not valid on fields. Try using a property instead");
			}
		}

		public void AddDeclarator(FieldDeclarator declarator)
		{
			if (declarators == null)
			{
				declarators = new List<FieldDeclarator>(2);
			}
			declarators.Add(declarator);
			Parent.AddNameToContainer(this, declarator.Name.Value);
		}

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Type == pa.FieldOffset)
			{
				status |= Status.HAS_OFFSET;
				if (!Parent.PartialContainer.HasExplicitLayout)
				{
					base.Report.Error(636, base.Location, "The FieldOffset attribute can only be placed on members of types marked with the StructLayout(LayoutKind.Explicit)");
					return;
				}
				if ((base.ModFlags & Modifiers.STATIC) != 0 || this is Const)
				{
					base.Report.Error(637, base.Location, "The FieldOffset attribute is not allowed on static or const fields");
					return;
				}
			}
			if (a.Type == pa.FixedBuffer)
			{
				base.Report.Error(1716, base.Location, "Do not use 'System.Runtime.CompilerServices.FixedBuffer' attribute. Use the 'fixed' field modifier instead");
			}
			else if (a.HasSecurityAttribute)
			{
				a.Error_InvalidSecurityParent();
			}
			else if (a.Type == pa.Dynamic)
			{
				a.Error_MisusedDynamicAttribute();
			}
			else
			{
				FieldBuilder.SetCustomAttribute((ConstructorInfo)ctor.GetMetaInfo(), cdata);
			}
		}

		public void SetCustomAttribute(MethodSpec ctor, byte[] data)
		{
			FieldBuilder.SetCustomAttribute((ConstructorInfo)ctor.GetMetaInfo(), data);
		}

		protected override bool CheckBase()
		{
			if (!base.CheckBase())
			{
				return false;
			}
			bool overrides = false;
			MemberSpec bestCandidate;
			MemberSpec memberSpec = MemberCache.FindBaseMember(this, out bestCandidate, ref overrides);
			if (memberSpec == null)
			{
				memberSpec = bestCandidate;
			}
			if (memberSpec == null)
			{
				if ((base.ModFlags & Modifiers.NEW) != 0)
				{
					base.Report.Warning(109, 4, base.Location, "The member `{0}' does not hide an inherited member. The new keyword is not required", GetSignatureForError());
				}
			}
			else
			{
				if ((base.ModFlags & (Modifiers.NEW | Modifiers.OVERRIDE | Modifiers.BACKING_FIELD)) == 0)
				{
					base.Report.SymbolRelatedToPreviousError(memberSpec);
					base.Report.Warning(108, 2, base.Location, "`{0}' hides inherited member `{1}'. Use the new keyword if hiding was intended", GetSignatureForError(), memberSpec.GetSignatureForError());
				}
				if (memberSpec.IsAbstract)
				{
					base.Report.SymbolRelatedToPreviousError(memberSpec);
					base.Report.Error(533, base.Location, "`{0}' hides inherited abstract member `{1}'", GetSignatureForError(), memberSpec.GetSignatureForError());
				}
			}
			return true;
		}

		public virtual Constant ConvertInitializer(ResolveContext rc, Constant expr)
		{
			return expr.ConvertImplicitly(base.MemberType);
		}

		protected override void DoMemberTypeDependentChecks()
		{
			base.DoMemberTypeDependentChecks();
			if (!base.MemberType.IsGenericParameter)
			{
				if (base.MemberType.IsStatic)
				{
					Error_VariableOfStaticClass(base.Location, GetSignatureForError(), base.MemberType, base.Report);
				}
				if (!base.IsCompilerGenerated)
				{
					CheckBase();
				}
				IsTypePermitted();
			}
		}

		public override void Emit()
		{
			if (member_type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				Module.PredefinedAttributes.Dynamic.EmitAttribute(FieldBuilder);
			}
			else if (!Parent.IsCompilerGenerated && member_type.HasDynamicElement)
			{
				Module.PredefinedAttributes.Dynamic.EmitAttribute(FieldBuilder, member_type, base.Location);
			}
			if ((base.ModFlags & Modifiers.COMPILER_GENERATED) != 0 && !Parent.IsCompilerGenerated)
			{
				Module.PredefinedAttributes.CompilerGenerated.EmitAttribute(FieldBuilder);
			}
			if ((base.ModFlags & Modifiers.DEBUGGER_HIDDEN) != 0)
			{
				Module.PredefinedAttributes.DebuggerBrowsable.EmitAttribute(FieldBuilder, DebuggerBrowsableState.Never);
			}
			if (base.OptAttributes != null)
			{
				base.OptAttributes.Emit();
			}
			if ((status & Status.HAS_OFFSET) == 0 && (base.ModFlags & (Modifiers.STATIC | Modifiers.BACKING_FIELD)) == 0 && Parent.PartialContainer.HasExplicitLayout)
			{
				base.Report.Error(625, base.Location, "`{0}': Instance field types marked with StructLayout(LayoutKind.Explicit) must have a FieldOffset attribute", GetSignatureForError());
			}
			ConstraintChecker.Check(this, member_type, type_expr.Location);
			base.Emit();
		}

		public static void Error_VariableOfStaticClass(Location loc, string variable_name, TypeSpec static_class, Report Report)
		{
			Report.SymbolRelatedToPreviousError(static_class);
			Report.Error(723, loc, "`{0}': cannot declare variables of static types", variable_name);
		}

		protected override bool VerifyClsCompliance()
		{
			if (!base.VerifyClsCompliance())
			{
				return false;
			}
			if (!base.MemberType.IsCLSCompliant() || this is FixedField)
			{
				base.Report.Warning(3003, 1, base.Location, "Type of `{0}' is not CLS-compliant", GetSignatureForError());
			}
			return true;
		}
	}
}
