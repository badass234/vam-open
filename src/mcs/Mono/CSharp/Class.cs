using System;
using System.Collections.Generic;
using System.Linq;

namespace Mono.CSharp
{
	public sealed class Class : ClassOrStruct
	{
		private const Modifiers AllowedModifiers = Modifiers.AccessibilityMask | Modifiers.NEW | Modifiers.ABSTRACT | Modifiers.SEALED | Modifiers.STATIC | Modifiers.UNSAFE;

		public override AttributeTargets AttributeTargets => AttributeTargets.Class;

		public Class(TypeContainer parent, MemberName name, Modifiers mod, Attributes attrs)
			: base(parent, name, attrs, MemberKind.Class)
		{
			base.ModFlags = ModifiersExtensions.Check(Modifiers.AccessibilityMask | Modifiers.NEW | Modifiers.ABSTRACT | Modifiers.SEALED | Modifiers.STATIC | Modifiers.UNSAFE, mod, base.IsTopLevel ? Modifiers.INTERNAL : Modifiers.PRIVATE, base.Location, base.Report);
			spec = new TypeSpec(Kind, null, this, null, base.ModFlags);
		}

		public override void Accept(StructuralVisitor visitor)
		{
			visitor.Visit(this);
		}

		public override void SetBaseTypes(List<FullNamedExpression> baseTypes)
		{
			MemberName memberName = base.MemberName;
			if (memberName.Name == "Object" && !memberName.IsGeneric && Parent.MemberName.Name == "System" && Parent.MemberName.Left == null)
			{
				base.Report.Error(537, base.Location, "The class System.Object cannot have a base class or implement an interface.");
			}
			base.SetBaseTypes(baseTypes);
		}

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Type == pa.AttributeUsage && !base.BaseType.IsAttribute && spec.BuiltinType != BuiltinTypeSpec.Type.Attribute)
			{
				base.Report.Error(641, a.Location, "Attribute `{0}' is only valid on classes derived from System.Attribute", a.GetSignatureForError());
			}
			if (a.Type == pa.Conditional && !base.BaseType.IsAttribute)
			{
				base.Report.Error(1689, a.Location, "Attribute `System.Diagnostics.ConditionalAttribute' is only valid on methods or attribute classes");
			}
			else if (a.Type == pa.ComImport && !attributes.Contains(pa.Guid))
			{
				a.Error_MissingGuidAttribute();
			}
			else if (a.Type == pa.Extension)
			{
				a.Error_MisusedExtensionAttribute();
			}
			else if (!a.Type.IsConditionallyExcluded(this))
			{
				base.ApplyAttributeBuilder(a, ctor, cdata, pa);
			}
		}

		protected override bool DoDefineMembers()
		{
			if ((base.ModFlags & Modifiers.ABSTRACT) == Modifiers.ABSTRACT && (base.ModFlags & (Modifiers.SEALED | Modifiers.STATIC)) != 0)
			{
				base.Report.Error(418, base.Location, "`{0}': an abstract class cannot be sealed or static", GetSignatureForError());
			}
			if ((base.ModFlags & (Modifiers.SEALED | Modifiers.STATIC)) == (Modifiers.SEALED | Modifiers.STATIC))
			{
				base.Report.Error(441, base.Location, "`{0}': a class cannot be both static and sealed", GetSignatureForError());
			}
			if (base.IsStatic)
			{
				if (base.PrimaryConstructorParameters != null)
				{
					base.Report.Error(-800, base.Location, "`{0}': Static classes cannot have primary constructor", GetSignatureForError());
					base.PrimaryConstructorParameters = null;
				}
				foreach (MemberCore member in base.Members)
				{
					if (member is Operator)
					{
						base.Report.Error(715, member.Location, "`{0}': Static classes cannot contain user-defined operators", member.GetSignatureForError());
					}
					else if (member is Destructor)
					{
						base.Report.Error(711, member.Location, "`{0}': Static classes cannot contain destructor", GetSignatureForError());
					}
					else if (member is Indexer)
					{
						base.Report.Error(720, member.Location, "`{0}': cannot declare indexers in a static class", member.GetSignatureForError());
					}
					else if ((member.ModFlags & Modifiers.STATIC) == 0 && !(member is TypeContainer))
					{
						if (member is Constructor)
						{
							base.Report.Error(710, member.Location, "`{0}': Static classes cannot have instance constructors", GetSignatureForError());
						}
						else
						{
							base.Report.Error(708, member.Location, "`{0}': cannot declare instance members in a static class", member.GetSignatureForError());
						}
					}
				}
			}
			else if (!base.PartialContainer.HasInstanceConstructor || base.PrimaryConstructorParameters != null)
			{
				generated_primary_constructor = DefineDefaultConstructor(false);
			}
			return base.DoDefineMembers();
		}

		public override void Emit()
		{
			base.Emit();
			if ((base.ModFlags & Modifiers.METHOD_EXTENSION) != 0)
			{
				Module.PredefinedAttributes.Extension.EmitAttribute(TypeBuilder);
			}
			if (base_type != null && base_type.HasDynamicElement)
			{
				Module.PredefinedAttributes.Dynamic.EmitAttribute(TypeBuilder, base_type, base.Location);
			}
		}

		public override void GetCompletionStartingWith(string prefix, List<string> results)
		{
			base.GetCompletionStartingWith(prefix, results);
			for (TypeSpec baseType = base_type; baseType != null; baseType = baseType.BaseType)
			{
				results.AddRange(from l in MemberCache.GetCompletitionMembers(this, baseType, prefix)
					where l.IsStatic
					select l.Name);
			}
		}

		protected override TypeSpec[] ResolveBaseTypes(out FullNamedExpression base_class)
		{
			TypeSpec[] array = base.ResolveBaseTypes(out base_class);
			if (base_class == null)
			{
				if (spec.BuiltinType != BuiltinTypeSpec.Type.Object)
				{
					base_type = Compiler.BuiltinTypes.Object;
				}
			}
			else
			{
				if (base_type.IsGenericParameter)
				{
					base.Report.Error(689, base_class.Location, "`{0}': Cannot derive from type parameter `{1}'", GetSignatureForError(), base_type.GetSignatureForError());
				}
				else if (base_type.IsStatic)
				{
					base.Report.SymbolRelatedToPreviousError(base_type);
					base.Report.Error(709, base.Location, "`{0}': Cannot derive from static class `{1}'", GetSignatureForError(), base_type.GetSignatureForError());
				}
				else if (base_type.IsSealed)
				{
					base.Report.SymbolRelatedToPreviousError(base_type);
					base.Report.Error(509, base.Location, "`{0}': cannot derive from sealed type `{1}'", GetSignatureForError(), base_type.GetSignatureForError());
				}
				else if (base.PartialContainer.IsStatic && base_type.BuiltinType != BuiltinTypeSpec.Type.Object)
				{
					base.Report.Error(713, base.Location, "Static class `{0}' cannot derive from type `{1}'. Static classes must derive from object", GetSignatureForError(), base_type.GetSignatureForError());
				}
				switch (base_type.BuiltinType)
				{
				case BuiltinTypeSpec.Type.ValueType:
				case BuiltinTypeSpec.Type.Enum:
				case BuiltinTypeSpec.Type.Delegate:
				case BuiltinTypeSpec.Type.MulticastDelegate:
				case BuiltinTypeSpec.Type.Array:
					if (!(spec is BuiltinTypeSpec))
					{
						base.Report.Error(644, base.Location, "`{0}' cannot derive from special class `{1}'", GetSignatureForError(), base_type.GetSignatureForError());
						base_type = Compiler.BuiltinTypes.Object;
					}
					break;
				}
				if (!IsAccessibleAs(base_type))
				{
					base.Report.SymbolRelatedToPreviousError(base_type);
					base.Report.Error(60, base.Location, "Inconsistent accessibility: base class `{0}' is less accessible than class `{1}'", base_type.GetSignatureForError(), GetSignatureForError());
				}
			}
			if (base.PartialContainer.IsStatic && array != null)
			{
				TypeSpec[] array2 = array;
				foreach (TypeSpec ms in array2)
				{
					base.Report.SymbolRelatedToPreviousError(ms);
				}
				base.Report.Error(714, base.Location, "Static class `{0}' cannot implement interfaces", GetSignatureForError());
			}
			return array;
		}

		public override string[] ConditionalConditions()
		{
			if ((caching_flags & (Flags.Excluded_Undetected | Flags.Excluded)) == 0)
			{
				return null;
			}
			caching_flags &= ~Flags.Excluded_Undetected;
			if (base.OptAttributes == null)
			{
				return null;
			}
			Attribute[] array = base.OptAttributes.SearchMulti(Module.PredefinedAttributes.Conditional);
			if (array == null)
			{
				return null;
			}
			string[] array2 = new string[array.Length];
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = array[i].GetConditionalAttributeValue();
			}
			caching_flags |= Flags.Excluded;
			return array2;
		}
	}
}
