using System;
using System.Reflection;

namespace Mono.CSharp
{
	public class Enum : TypeDefinition
	{
		private sealed class ImplicitInitializer : Expression
		{
			private readonly EnumMember prev;

			private readonly EnumMember current;

			public ImplicitInitializer(EnumMember current, EnumMember prev)
			{
				this.current = current;
				this.prev = prev;
			}

			public override bool ContainsEmitWithAwait()
			{
				return false;
			}

			public override Expression CreateExpressionTree(ResolveContext ec)
			{
				throw new NotSupportedException("Missing Resolve call");
			}

			protected override Expression DoResolve(ResolveContext rc)
			{
				if (prev == null)
				{
					return New.Constantify(current.Parent.Definition, base.Location);
				}
				EnumConstant enumConstant = ((ConstSpec)prev.Spec).GetConstant(rc) as EnumConstant;
				try
				{
					return enumConstant.Increment();
				}
				catch (OverflowException)
				{
					rc.Report.Error(543, current.Location, "The enumerator value `{0}' is outside the range of enumerator underlying type `{1}'", current.GetSignatureForError(), ((Enum)current.Parent).UnderlyingType.GetSignatureForError());
					return New.Constantify(current.Parent.Definition, current.Location);
				}
			}

			public override void Emit(EmitContext ec)
			{
				throw new NotSupportedException("Missing Resolve call");
			}
		}

		public static readonly string UnderlyingValueField = "value__";

		private const Modifiers AllowedModifiers = Modifiers.AccessibilityMask | Modifiers.NEW;

		private readonly FullNamedExpression underlying_type_expr;

		public override AttributeTargets AttributeTargets => AttributeTargets.Enum;

		public FullNamedExpression BaseTypeExpression => underlying_type_expr;

		protected override TypeAttributes TypeAttr => base.TypeAttr | TypeAttributes.NotPublic | TypeAttributes.Sealed;

		public TypeSpec UnderlyingType => ((EnumSpec)spec).UnderlyingType;

		public Enum(TypeContainer parent, FullNamedExpression type, Modifiers mod_flags, MemberName name, Attributes attrs)
			: base(parent, name, attrs, MemberKind.Enum)
		{
			underlying_type_expr = type;
			base.ModFlags = ModifiersExtensions.Check(Modifiers.AccessibilityMask | Modifiers.NEW, mod_flags, base.IsTopLevel ? Modifiers.INTERNAL : Modifiers.PRIVATE, base.Location, base.Report);
			spec = new EnumSpec(null, this, null, null, base.ModFlags);
		}

		public override void Accept(StructuralVisitor visitor)
		{
			visitor.Visit(this);
		}

		public void AddEnumMember(EnumMember em)
		{
			if (em.Name == UnderlyingValueField)
			{
				base.Report.Error(76, em.Location, "An item in an enumeration cannot have an identifier `{0}'", UnderlyingValueField);
			}
			else
			{
				AddMember(em);
			}
		}

		public void Error_UnderlyingType(Location loc)
		{
			base.Report.Error(1008, loc, "Type byte, sbyte, short, ushort, int, uint, long or ulong expected");
		}

		protected override void DoDefineContainer()
		{
			TypeSpec typeSpec;
			if (underlying_type_expr != null)
			{
				typeSpec = underlying_type_expr.ResolveAsType(this);
				if (!EnumSpec.IsValidUnderlyingType(typeSpec))
				{
					Error_UnderlyingType(underlying_type_expr.Location);
					typeSpec = null;
				}
			}
			else
			{
				typeSpec = null;
			}
			if (typeSpec == null)
			{
				typeSpec = Compiler.BuiltinTypes.Int;
			}
			((EnumSpec)spec).UnderlyingType = typeSpec;
			TypeBuilder.DefineField(UnderlyingValueField, UnderlyingType.GetMetaInfo(), FieldAttributes.Public | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName);
			DefineBaseTypes();
		}

		protected override bool DoDefineMembers()
		{
			for (int i = 0; i < base.Members.Count; i++)
			{
				EnumMember enumMember = (EnumMember)base.Members[i];
				if (enumMember.Initializer == null)
				{
					enumMember.Initializer = new ImplicitInitializer(enumMember, (i == 0) ? null : ((EnumMember)base.Members[i - 1]));
				}
				enumMember.Define();
			}
			return true;
		}

		public override bool IsUnmanagedType()
		{
			return true;
		}

		protected override TypeSpec[] ResolveBaseTypes(out FullNamedExpression base_class)
		{
			base_type = Compiler.BuiltinTypes.Enum;
			base_class = null;
			return null;
		}

		protected override bool VerifyClsCompliance()
		{
			if (!base.VerifyClsCompliance())
			{
				return false;
			}
			switch (UnderlyingType.BuiltinType)
			{
			case BuiltinTypeSpec.Type.UShort:
			case BuiltinTypeSpec.Type.UInt:
			case BuiltinTypeSpec.Type.ULong:
				base.Report.Warning(3009, 1, base.Location, "`{0}': base type `{1}' is not CLS-compliant", GetSignatureForError(), UnderlyingType.GetSignatureForError());
				break;
			}
			return true;
		}
	}
}
