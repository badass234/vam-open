using System;
using System.Reflection;

namespace Mono.CSharp
{
	public sealed class Interface : TypeDefinition
	{
		private const Modifiers AllowedModifiers = Modifiers.AccessibilityMask | Modifiers.NEW | Modifiers.UNSAFE;

		public override AttributeTargets AttributeTargets => AttributeTargets.Interface;

		protected override TypeAttributes TypeAttr => base.TypeAttr | (TypeAttributes.ClassSemanticsMask | TypeAttributes.Abstract);

		public Interface(TypeContainer parent, MemberName name, Modifiers mod, Attributes attrs)
			: base(parent, name, attrs, MemberKind.Interface)
		{
			base.ModFlags = ModifiersExtensions.Check(Modifiers.AccessibilityMask | Modifiers.NEW | Modifiers.UNSAFE, mod, base.IsTopLevel ? Modifiers.INTERNAL : Modifiers.PRIVATE, name.Location, base.Report);
			spec = new TypeSpec(Kind, null, this, null, base.ModFlags);
		}

		public override void Accept(StructuralVisitor visitor)
		{
			visitor.Visit(this);
		}

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Type == pa.ComImport && !attributes.Contains(pa.Guid))
			{
				a.Error_MissingGuidAttribute();
			}
			else
			{
				base.ApplyAttributeBuilder(a, ctor, cdata, pa);
			}
		}

		protected override bool VerifyClsCompliance()
		{
			if (!base.VerifyClsCompliance())
			{
				return false;
			}
			if (iface_exprs != null)
			{
				TypeSpec[] array = iface_exprs;
				foreach (TypeSpec typeSpec in array)
				{
					if (!typeSpec.IsCLSCompliant())
					{
						base.Report.SymbolRelatedToPreviousError(typeSpec);
						base.Report.Warning(3027, 1, base.Location, "`{0}' is not CLS-compliant because base interface `{1}' is not CLS-compliant", GetSignatureForError(), typeSpec.GetSignatureForError());
					}
				}
			}
			return true;
		}
	}
}
