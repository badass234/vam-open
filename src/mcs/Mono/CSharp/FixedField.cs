using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Mono.CSharp
{
	public class FixedField : FieldBase
	{
		public const string FixedElementName = "FixedElementField";

		private static int GlobalCounter;

		private TypeBuilder fixed_buffer_type;

		private const Modifiers AllowedModifiers = Modifiers.AccessibilityMask | Modifiers.NEW | Modifiers.UNSAFE;

		public CharSet? CharSetValue { get; set; }

		public FixedField(TypeDefinition parent, FullNamedExpression type, Modifiers mod, MemberName name, Attributes attrs)
			: base(parent, type, mod, Modifiers.AccessibilityMask | Modifiers.NEW | Modifiers.UNSAFE, name, attrs)
		{
		}

		public override Constant ConvertInitializer(ResolveContext rc, Constant expr)
		{
			return expr.ImplicitConversionRequired(rc, rc.BuiltinTypes.Int);
		}

		public override bool Define()
		{
			if (!base.Define())
			{
				return false;
			}
			if (!BuiltinTypeSpec.IsPrimitiveType(base.MemberType))
			{
				base.Report.Error(1663, base.Location, "`{0}': Fixed size buffers type must be one of the following: bool, byte, short, int, long, char, sbyte, ushort, uint, ulong, float or double", GetSignatureForError());
			}
			else if (declarators != null)
			{
				foreach (FieldDeclarator declarator in declarators)
				{
					FixedField fixedField = new FixedField(Parent, declarator.GetFieldTypeExpression(this), base.ModFlags, new MemberName(declarator.Name.Value, declarator.Name.Location), base.OptAttributes);
					fixedField.initializer = declarator.Initializer;
					((ConstInitializer)fixedField.initializer).Name = declarator.Name.Value;
					fixedField.Define();
					Parent.PartialContainer.Members.Add(fixedField);
				}
			}
			string name = $"<{TypeDefinition.FilterNestedName(base.Name)}>__FixedBuffer{GlobalCounter++}";
			fixed_buffer_type = Parent.TypeBuilder.DefineNestedType(name, TypeAttributes.NestedPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit, Compiler.BuiltinTypes.ValueType.GetMetaInfo());
			FieldBuilder info = fixed_buffer_type.DefineField("FixedElementField", base.MemberType.GetMetaInfo(), FieldAttributes.Public);
			FieldBuilder = Parent.TypeBuilder.DefineField(base.Name, fixed_buffer_type, ModifiersExtensions.FieldAttr(base.ModFlags));
			FieldSpec element = new FieldSpec(null, this, base.MemberType, info, base.ModFlags);
			spec = new FixedFieldSpec(Module, Parent.Definition, this, FieldBuilder, element, base.ModFlags);
			Parent.MemberCache.AddMember(spec);
			return true;
		}

		protected override void DoMemberTypeIndependentChecks()
		{
			base.DoMemberTypeIndependentChecks();
			if (!base.IsUnsafe)
			{
				Expression.UnsafeError(base.Report, base.Location);
			}
			if (Parent.PartialContainer.Kind != MemberKind.Struct)
			{
				base.Report.Error(1642, base.Location, "`{0}': Fixed size buffer fields may only be members of structs", GetSignatureForError());
			}
		}

		public override void Emit()
		{
			ResolveContext rc = new ResolveContext(this);
			IntConstant intConstant = initializer.Resolve(rc) as IntConstant;
			if (intConstant != null)
			{
				int value = intConstant.Value;
				if (value <= 0)
				{
					base.Report.Error(1665, base.Location, "`{0}': Fixed size buffers must have a length greater than zero", GetSignatureForError());
					return;
				}
				EmitFieldSize(value);
				Module.PredefinedAttributes.UnsafeValueType.EmitAttribute(fixed_buffer_type);
				Module.PredefinedAttributes.CompilerGenerated.EmitAttribute(fixed_buffer_type);
				fixed_buffer_type.CreateType();
				base.Emit();
			}
		}

		private void EmitFieldSize(int buffer_size)
		{
			int size = BuiltinTypeSpec.GetSize(base.MemberType);
			if (buffer_size > int.MaxValue / size)
			{
				base.Report.Error(1664, base.Location, "Fixed size buffer `{0}' of length `{1}' and type `{2}' exceeded 2^31 limit", GetSignatureForError(), buffer_size.ToString(), base.MemberType.GetSignatureForError());
				return;
			}
			CharSet v = CharSetValue ?? Module.DefaultCharSet ?? ((CharSet)0);
			MethodSpec methodSpec = Module.PredefinedMembers.StructLayoutAttributeCtor.Resolve(base.Location);
			if (methodSpec == null)
			{
				return;
			}
			FieldSpec fieldSpec = Module.PredefinedMembers.StructLayoutSize.Resolve(base.Location);
			FieldSpec fieldSpec2 = Module.PredefinedMembers.StructLayoutCharSet.Resolve(base.Location);
			if (fieldSpec == null || fieldSpec2 == null)
			{
				return;
			}
			AttributeEncoder attributeEncoder = new AttributeEncoder();
			attributeEncoder.Encode((short)0);
			attributeEncoder.EncodeNamedArguments(new FieldSpec[2] { fieldSpec, fieldSpec2 }, new Constant[2]
			{
				new IntConstant(Compiler.BuiltinTypes, buffer_size * size, base.Location),
				new IntConstant(Compiler.BuiltinTypes, (int)v, base.Location)
			});
			fixed_buffer_type.SetCustomAttribute((ConstructorInfo)methodSpec.GetMetaInfo(), attributeEncoder.ToArray());
			if ((base.ModFlags & Modifiers.PRIVATE) == 0)
			{
				methodSpec = Module.PredefinedMembers.FixedBufferAttributeCtor.Resolve(base.Location);
				if (methodSpec != null)
				{
					attributeEncoder = new AttributeEncoder();
					attributeEncoder.EncodeTypeName(base.MemberType);
					attributeEncoder.Encode(buffer_size);
					attributeEncoder.EncodeEmptyNamedArguments();
					FieldBuilder.SetCustomAttribute((ConstructorInfo)methodSpec.GetMetaInfo(), attributeEncoder.ToArray());
				}
			}
		}
	}
}
