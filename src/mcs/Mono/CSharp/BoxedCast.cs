using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class BoxedCast : TypeCast
	{
		public BoxedCast(Expression expr, TypeSpec target_type)
			: base(expr, target_type)
		{
			eclass = ExprClass.Value;
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			return this;
		}

		public override void EncodeAttributeValue(IMemberContext rc, AttributeEncoder enc, TypeSpec targetType, TypeSpec parameterType)
		{
			if (targetType.BuiltinType != BuiltinTypeSpec.Type.Object)
			{
				base.EncodeAttributeValue(rc, enc, targetType, parameterType);
				return;
			}
			enc.Encode(child.Type);
			child.EncodeAttributeValue(rc, enc, child.Type, parameterType);
		}

		public override void Emit(EmitContext ec)
		{
			base.Emit(ec);
			ec.Emit(OpCodes.Box, child.Type);
		}

		public override void EmitSideEffect(EmitContext ec)
		{
			if (child.Type.IsStruct && (type.BuiltinType == BuiltinTypeSpec.Type.Object || type.BuiltinType == BuiltinTypeSpec.Type.ValueType))
			{
				child.EmitSideEffect(ec);
			}
			else
			{
				base.EmitSideEffect(ec);
			}
		}
	}
}
