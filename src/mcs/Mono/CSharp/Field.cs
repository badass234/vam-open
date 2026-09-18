using System;

namespace Mono.CSharp
{
	public class Field : FieldBase
	{
		private const Modifiers AllowedModifiers = Modifiers.AccessibilityMask | Modifiers.NEW | Modifiers.STATIC | Modifiers.READONLY | Modifiers.VOLATILE | Modifiers.UNSAFE;

		public Field(TypeDefinition parent, FullNamedExpression type, Modifiers mod, MemberName name, Attributes attrs)
			: base(parent, type, mod, Modifiers.AccessibilityMask | Modifiers.NEW | Modifiers.STATIC | Modifiers.READONLY | Modifiers.VOLATILE | Modifiers.UNSAFE, name, attrs)
		{
		}

		private bool CanBeVolatile()
		{
			switch (base.MemberType.BuiltinType)
			{
			case BuiltinTypeSpec.Type.FirstPrimitive:
			case BuiltinTypeSpec.Type.Byte:
			case BuiltinTypeSpec.Type.SByte:
			case BuiltinTypeSpec.Type.Char:
			case BuiltinTypeSpec.Type.Short:
			case BuiltinTypeSpec.Type.UShort:
			case BuiltinTypeSpec.Type.Int:
			case BuiltinTypeSpec.Type.UInt:
			case BuiltinTypeSpec.Type.Float:
			case BuiltinTypeSpec.Type.IntPtr:
			case BuiltinTypeSpec.Type.UIntPtr:
				return true;
			default:
				if (TypeSpec.IsReferenceType(base.MemberType))
				{
					return true;
				}
				if (base.MemberType.IsPointer)
				{
					return true;
				}
				if (base.MemberType.IsEnum)
				{
					switch (EnumSpec.GetUnderlyingType(base.MemberType).BuiltinType)
					{
					case BuiltinTypeSpec.Type.Byte:
					case BuiltinTypeSpec.Type.SByte:
					case BuiltinTypeSpec.Type.Short:
					case BuiltinTypeSpec.Type.UShort:
					case BuiltinTypeSpec.Type.Int:
					case BuiltinTypeSpec.Type.UInt:
						return true;
					default:
						return false;
					}
				}
				return false;
			}
		}

		public override void Accept(StructuralVisitor visitor)
		{
			visitor.Visit(this);
		}

		public override bool Define()
		{
			if (!base.Define())
			{
				return false;
			}
			Type[] requiredCustomModifiers = null;
			if ((base.ModFlags & Modifiers.VOLATILE) != 0)
			{
				TypeSpec typeSpec = Module.PredefinedTypes.IsVolatile.Resolve();
				if (typeSpec != null)
				{
					requiredCustomModifiers = new Type[1] { typeSpec.GetMetaInfo() };
				}
			}
			FieldBuilder = Parent.TypeBuilder.DefineField(base.Name, member_type.GetMetaInfo(), requiredCustomModifiers, null, ModifiersExtensions.FieldAttr(base.ModFlags));
			spec = new FieldSpec(Parent.Definition, this, base.MemberType, FieldBuilder, base.ModFlags);
			if ((base.ModFlags & Modifiers.BACKING_FIELD) == 0 || Parent.Kind == MemberKind.Struct)
			{
				Parent.MemberCache.AddMember(spec);
			}
			if (initializer != null)
			{
				Parent.RegisterFieldForInitialization(this, new FieldInitializer(this, initializer, base.TypeExpression.Location));
			}
			if (declarators != null)
			{
				foreach (FieldDeclarator declarator in declarators)
				{
					Field field = new Field(Parent, declarator.GetFieldTypeExpression(this), base.ModFlags, new MemberName(declarator.Name.Value, declarator.Name.Location), base.OptAttributes);
					if (declarator.Initializer != null)
					{
						field.initializer = declarator.Initializer;
					}
					field.Define();
					Parent.PartialContainer.Members.Add(field);
				}
			}
			return true;
		}

		protected override void DoMemberTypeDependentChecks()
		{
			if ((base.ModFlags & Modifiers.BACKING_FIELD) != 0)
			{
				return;
			}
			base.DoMemberTypeDependentChecks();
			if ((base.ModFlags & Modifiers.VOLATILE) != 0)
			{
				if (!CanBeVolatile())
				{
					base.Report.Error(677, base.Location, "`{0}': A volatile field cannot be of the type `{1}'", GetSignatureForError(), base.MemberType.GetSignatureForError());
				}
				if ((base.ModFlags & Modifiers.READONLY) != 0)
				{
					base.Report.Error(678, base.Location, "`{0}': A field cannot be both volatile and readonly", GetSignatureForError());
				}
			}
		}

		protected override bool VerifyClsCompliance()
		{
			if (!base.VerifyClsCompliance())
			{
				return false;
			}
			if ((base.ModFlags & Modifiers.VOLATILE) != 0)
			{
				base.Report.Warning(3026, 1, base.Location, "CLS-compliant field `{0}' cannot be volatile", GetSignatureForError());
			}
			return true;
		}
	}
}
