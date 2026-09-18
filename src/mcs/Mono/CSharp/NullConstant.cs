using System;
using System.Reflection.Emit;
using Mono.CSharp.Nullable;

namespace Mono.CSharp
{
	public class NullConstant : Constant
	{
		public override string ExprClassName => GetSignatureForError();

		public override bool IsDefaultValue => true;

		public override bool IsNegative => false;

		public override bool IsNull => true;

		public override bool IsZeroInteger => true;

		public NullConstant(TypeSpec type, Location loc)
			: base(loc)
		{
			eclass = ExprClass.Value;
			base.type = type;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			if (type == InternalType.NullLiteral || type.BuiltinType == BuiltinTypeSpec.Type.Object)
			{
				Arguments arguments = new Arguments(1);
				arguments.Add(new Argument(this));
				return CreateExpressionFactoryCall(ec, "Constant", arguments);
			}
			return base.CreateExpressionTree(ec);
		}

		public override void EncodeAttributeValue(IMemberContext rc, AttributeEncoder enc, TypeSpec targetType, TypeSpec parameterType)
		{
			switch (targetType.BuiltinType)
			{
			case BuiltinTypeSpec.Type.Object:
				enc.Encode(rc.Module.Compiler.BuiltinTypes.String);
				goto case BuiltinTypeSpec.Type.String;
			case BuiltinTypeSpec.Type.String:
			case BuiltinTypeSpec.Type.Type:
				enc.Encode(byte.MaxValue);
				return;
			}
			ArrayContainer arrayContainer = targetType as ArrayContainer;
			if (arrayContainer != null && arrayContainer.Rank == 1 && !arrayContainer.Element.IsArray)
			{
				enc.Encode(uint.MaxValue);
			}
			else
			{
				base.EncodeAttributeValue(rc, enc, targetType, parameterType);
			}
		}

		public override void Emit(EmitContext ec)
		{
			ec.EmitNull();
			if (type.IsGenericParameter)
			{
				ec.Emit(OpCodes.Unbox_Any, type);
			}
		}

		public override Constant ConvertExplicitly(bool inCheckedContext, TypeSpec targetType)
		{
			if (targetType.IsPointer)
			{
				if (IsLiteral || this is NullPointer)
				{
					return new NullPointer(targetType, loc);
				}
				return null;
			}
			if (targetType.Kind == MemberKind.InternalCompilerType && targetType.BuiltinType != BuiltinTypeSpec.Type.Dynamic)
			{
				return null;
			}
			if (!IsLiteral && !Convert.ImplicitStandardConversionExists(this, targetType))
			{
				return null;
			}
			if (TypeSpec.IsReferenceType(targetType))
			{
				return new NullConstant(targetType, loc);
			}
			if (targetType.IsNullableType)
			{
				return LiftedNull.Create(targetType, loc);
			}
			return null;
		}

		public override Constant ConvertImplicitly(TypeSpec targetType)
		{
			return ConvertExplicitly(false, targetType);
		}

		public override string GetSignatureForError()
		{
			return "null";
		}

		public override object GetValue()
		{
			return null;
		}

		public override string GetValueAsLiteral()
		{
			return GetSignatureForError();
		}

		public override long GetValueAsLong()
		{
			throw new NotSupportedException();
		}
	}
}
