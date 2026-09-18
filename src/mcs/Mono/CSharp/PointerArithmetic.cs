using System;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class PointerArithmetic : Expression
	{
		private Expression left;

		private Expression right;

		private readonly Binary.Operator op;

		public PointerArithmetic(Binary.Operator op, Expression l, Expression r, TypeSpec t, Location loc)
		{
			type = t;
			base.loc = loc;
			left = l;
			right = r;
			this.op = op;
		}

		public override bool ContainsEmitWithAwait()
		{
			throw new NotImplementedException();
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			Error_PointerInsideExpressionTree(ec);
			return null;
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			eclass = ExprClass.Variable;
			PointerContainer pointerContainer = left.Type as PointerContainer;
			if (pointerContainer != null && pointerContainer.Element.Kind == MemberKind.Void)
			{
				Error_VoidPointerOperation(ec);
				return null;
			}
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			TypeSpec typeSpec = left.Type;
			TypeSpec t;
			if (TypeManager.HasElementType(typeSpec))
			{
				t = TypeManager.GetElementType(typeSpec);
			}
			else
			{
				FieldExpr fieldExpr = left as FieldExpr;
				t = ((fieldExpr == null) ? typeSpec : ((FixedFieldSpec)fieldExpr.Spec).ElementType);
			}
			int size = BuiltinTypeSpec.GetSize(t);
			TypeSpec typeSpec2 = right.Type;
			if ((op & Binary.Operator.SubtractionMask) != 0 && typeSpec2.IsPointer)
			{
				left.Emit(ec);
				right.Emit(ec);
				ec.Emit(OpCodes.Sub);
				if (size != 1)
				{
					if (size == 0)
					{
						ec.Emit(OpCodes.Sizeof, t);
					}
					else
					{
						ec.EmitInt(size);
					}
					ec.Emit(OpCodes.Div);
				}
				ec.Emit(OpCodes.Conv_I8);
				return;
			}
			Constant constant = left as Constant;
			if (constant != null)
			{
				if (constant.IsDefaultValue)
				{
					left = EmptyExpression.Null;
				}
				else
				{
					constant = null;
				}
			}
			left.Emit(ec);
			Constant constant2 = right as Constant;
			if (constant2 != null)
			{
				if (constant2.IsDefaultValue)
				{
					return;
				}
				if (size != 0)
				{
					right = new IntConstant(ec.BuiltinTypes, size, right.Location);
				}
				else
				{
					right = new SizeOf(new TypeExpression(t, right.Location), right.Location);
				}
				ResolveContext rc = new ResolveContext(ec.MemberContext, ResolveContext.Options.UnsafeScope);
				right = new Binary(Binary.Operator.Multiply, right, constant2).Resolve(rc);
				if (right == null)
				{
					return;
				}
			}
			right.Emit(ec);
			switch (typeSpec2.BuiltinType)
			{
			case BuiltinTypeSpec.Type.Byte:
			case BuiltinTypeSpec.Type.SByte:
			case BuiltinTypeSpec.Type.Short:
			case BuiltinTypeSpec.Type.UShort:
				ec.Emit(OpCodes.Conv_I);
				break;
			case BuiltinTypeSpec.Type.UInt:
				ec.Emit(OpCodes.Conv_U);
				break;
			}
			if (constant2 == null && size != 1)
			{
				if (size == 0)
				{
					ec.Emit(OpCodes.Sizeof, t);
				}
				else
				{
					ec.EmitInt(size);
				}
				if (typeSpec2.BuiltinType == BuiltinTypeSpec.Type.Long || typeSpec2.BuiltinType == BuiltinTypeSpec.Type.ULong)
				{
					ec.Emit(OpCodes.Conv_I8);
				}
				Binary.EmitOperatorOpcode(ec, Binary.Operator.Multiply, typeSpec2, right);
			}
			if (constant == null)
			{
				if (typeSpec2.BuiltinType == BuiltinTypeSpec.Type.Long)
				{
					ec.Emit(OpCodes.Conv_I);
				}
				else if (typeSpec2.BuiltinType == BuiltinTypeSpec.Type.ULong)
				{
					ec.Emit(OpCodes.Conv_U);
				}
				Binary.EmitOperatorOpcode(ec, op, typeSpec, right);
			}
		}
	}
}
