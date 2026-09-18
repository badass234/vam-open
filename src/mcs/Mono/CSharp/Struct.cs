using System;
using System.Runtime.InteropServices;

namespace Mono.CSharp
{
	public sealed class Struct : ClassOrStruct
	{
		private bool is_unmanaged;

		private bool has_unmanaged_check_done;

		private bool InTransit;

		private const Modifiers AllowedModifiers = Modifiers.AccessibilityMask | Modifiers.NEW | Modifiers.UNSAFE;

		public override AttributeTargets AttributeTargets => AttributeTargets.Struct;

		public Struct(TypeContainer parent, MemberName name, Modifiers mod, Attributes attrs)
			: base(parent, name, attrs, MemberKind.Struct)
		{
			base.ModFlags = ModifiersExtensions.Check(Modifiers.AccessibilityMask | Modifiers.NEW | Modifiers.UNSAFE, mod, base.IsTopLevel ? Modifiers.INTERNAL : Modifiers.PRIVATE, base.Location, base.Report) | Modifiers.SEALED;
			spec = new TypeSpec(Kind, null, this, null, base.ModFlags);
		}

		public override void Accept(StructuralVisitor visitor)
		{
			visitor.Visit(this);
		}

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			base.ApplyAttributeBuilder(a, ctor, cdata, pa);
			if (!(a.Type == pa.StructLayout))
			{
				return;
			}
			Constant namedValue = a.GetNamedValue("CharSet");
			if (namedValue == null)
			{
				return;
			}
			for (int i = 0; i < base.Members.Count; i++)
			{
				FixedField fixedField = base.Members[i] as FixedField;
				if (fixedField != null)
				{
					fixedField.CharSetValue = (CharSet)System.Enum.Parse(typeof(CharSet), namedValue.GetValue().ToString());
				}
			}
		}

		private bool CheckStructCycles()
		{
			if (InTransit)
			{
				return false;
			}
			InTransit = true;
			foreach (MemberCore member in base.Members)
			{
				Field field = member as Field;
				if (field == null)
				{
					continue;
				}
				TypeSpec memberType = field.Spec.MemberType;
				if (!memberType.IsStruct || memberType is BuiltinTypeSpec)
				{
					continue;
				}
				TypeSpec[] typeArguments = memberType.TypeArguments;
				for (int i = 0; i < typeArguments.Length; i++)
				{
					if (!CheckFieldTypeCycle(typeArguments[i]))
					{
						base.Report.Error(523, field.Location, "Struct member `{0}' of type `{1}' causes a cycle in the struct layout", field.GetSignatureForError(), memberType.GetSignatureForError());
						break;
					}
				}
				if ((!field.IsStatic || memberType != CurrentType) && !CheckFieldTypeCycle(memberType))
				{
					base.Report.Error(523, field.Location, "Struct member `{0}' of type `{1}' causes a cycle in the struct layout", field.GetSignatureForError(), memberType.GetSignatureForError());
					break;
				}
			}
			InTransit = false;
			return true;
		}

		private static bool CheckFieldTypeCycle(TypeSpec ts)
		{
			return (ts.MemberDefinition as Struct)?.CheckStructCycles() ?? true;
		}

		protected override bool DoDefineMembers()
		{
			bool result = base.DoDefineMembers();
			if (base.PrimaryConstructorParameters != null || (initialized_fields != null && !HasUserDefaultConstructor()))
			{
				generated_primary_constructor = DefineDefaultConstructor(false);
				generated_primary_constructor.Define();
			}
			return result;
		}

		public override void Emit()
		{
			CheckStructCycles();
			base.Emit();
		}

		private bool HasUserDefaultConstructor()
		{
			foreach (MemberCore member in base.PartialContainer.Members)
			{
				Constructor constructor = member as Constructor;
				if (constructor != null && !constructor.IsStatic && constructor.ParameterInfo.IsEmpty)
				{
					return true;
				}
			}
			return false;
		}

		public override bool IsUnmanagedType()
		{
			if (has_unmanaged_check_done)
			{
				return is_unmanaged;
			}
			if (requires_delayed_unmanagedtype_check)
			{
				return true;
			}
			TypeDefinition partialContainer = Parent.PartialContainer;
			if (partialContainer != null && partialContainer.IsGenericOrParentIsGeneric)
			{
				has_unmanaged_check_done = true;
				return false;
			}
			if (first_nonstatic_field != null)
			{
				requires_delayed_unmanagedtype_check = true;
				foreach (MemberCore member in base.Members)
				{
					Field field = member as Field;
					if (field != null && !field.IsStatic)
					{
						TypeSpec memberType = field.MemberType;
						if (memberType == null)
						{
							return true;
						}
						if (!memberType.IsUnmanaged)
						{
							has_unmanaged_check_done = true;
							return false;
						}
					}
				}
				has_unmanaged_check_done = true;
			}
			is_unmanaged = true;
			return true;
		}

		protected override TypeSpec[] ResolveBaseTypes(out FullNamedExpression base_class)
		{
			TypeSpec[] result = base.ResolveBaseTypes(out base_class);
			base_type = Compiler.BuiltinTypes.ValueType;
			return result;
		}
	}
}
