using System.Reflection;

namespace Mono.CSharp
{
	public class Const : FieldBase
	{
		private const Modifiers AllowedModifiers = Modifiers.AccessibilityMask | Modifiers.NEW;

		public Const(TypeDefinition parent, FullNamedExpression type, Modifiers mod_flags, MemberName name, Attributes attrs)
			: base(parent, type, mod_flags, Modifiers.AccessibilityMask | Modifiers.NEW, name, attrs)
		{
			base.ModFlags |= Modifiers.STATIC;
		}

		public override bool Define()
		{
			if (!base.Define())
			{
				return false;
			}
			if (!member_type.IsConstantCompatible)
			{
				Error_InvalidConstantType(member_type, base.Location, base.Report);
			}
			FieldAttributes fieldAttributes = FieldAttributes.Static | ModifiersExtensions.FieldAttr(base.ModFlags);
			fieldAttributes = ((member_type.BuiltinType != BuiltinTypeSpec.Type.Decimal) ? (fieldAttributes | FieldAttributes.Literal) : (fieldAttributes | FieldAttributes.InitOnly));
			FieldBuilder = Parent.TypeBuilder.DefineField(base.Name, base.MemberType.GetMetaInfo(), fieldAttributes);
			spec = new ConstSpec(Parent.Definition, this, base.MemberType, FieldBuilder, base.ModFlags, initializer);
			Parent.MemberCache.AddMember(spec);
			if ((fieldAttributes & FieldAttributes.InitOnly) != FieldAttributes.PrivateScope)
			{
				Parent.PartialContainer.RegisterFieldForInitialization(this, new FieldInitializer(this, initializer, base.Location));
			}
			if (declarators != null)
			{
				TypeExpression type = new TypeExpression(base.MemberType, base.TypeExpression.Location);
				foreach (FieldDeclarator declarator in declarators)
				{
					Const obj = new Const(Parent, type, base.ModFlags & ~Modifiers.STATIC, new MemberName(declarator.Name.Value, declarator.Name.Location), base.OptAttributes);
					obj.initializer = declarator.Initializer;
					((ConstInitializer)obj.initializer).Name = declarator.Name.Value;
					obj.Define();
					Parent.PartialContainer.Members.Add(obj);
				}
			}
			return true;
		}

		public void DefineValue()
		{
			ResolveContext rc = new ResolveContext(this);
			((ConstSpec)spec).GetConstant(rc);
		}

		public override void Emit()
		{
			Constant constant = ((ConstSpec)spec).Value as Constant;
			if (constant.Type.BuiltinType == BuiltinTypeSpec.Type.Decimal)
			{
				Module.PredefinedAttributes.DecimalConstant.EmitAttribute(FieldBuilder, (decimal)constant.GetValue(), constant.Location);
			}
			else
			{
				FieldBuilder.SetConstant(constant.GetValue());
			}
			base.Emit();
		}

		public static void Error_InvalidConstantType(TypeSpec t, Location loc, Report Report)
		{
			if (t.IsGenericParameter)
			{
				Report.Error(1959, loc, "Type parameter `{0}' cannot be declared const", t.GetSignatureForError());
			}
			else
			{
				Report.Error(283, loc, "The type `{0}' cannot be declared const", t.GetSignatureForError());
			}
		}

		public override void Accept(StructuralVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
