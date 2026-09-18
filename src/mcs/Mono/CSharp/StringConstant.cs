using System;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class StringConstant : Constant
	{
		public string Value { get; protected set; }

		public override bool IsDefaultValue => Value == null;

		public override bool IsNegative => false;

		public override bool IsNull => IsDefaultValue;

		public StringConstant(BuiltinTypes types, string s, Location loc)
			: this(types.String, s, loc)
		{
		}

		public StringConstant(TypeSpec type, string s, Location loc)
			: base(loc)
		{
			base.type = type;
			eclass = ExprClass.Value;
			Value = s;
		}

		protected StringConstant(Location loc)
			: base(loc)
		{
		}

		public override object GetValue()
		{
			return Value;
		}

		public override string GetValueAsLiteral()
		{
			return "\"" + Value + "\"";
		}

		public override long GetValueAsLong()
		{
			throw new NotSupportedException();
		}

		public override void Emit(EmitContext ec)
		{
			if (Value == null)
			{
				ec.EmitNull();
				return;
			}
			if (Value.Length == 0 && ec.Module.Compiler.Settings.Optimize)
			{
				BuiltinTypeSpec builtinTypeSpec = ec.BuiltinTypes.String;
				if (ec.CurrentType != builtinTypeSpec)
				{
					FieldSpec fieldSpec = ec.Module.PredefinedMembers.StringEmpty.Get();
					if (fieldSpec != null)
					{
						ec.Emit(OpCodes.Ldsfld, fieldSpec);
						return;
					}
				}
			}
			string value = Value;
			if (ec.Module.GetResourceStrings != null && !ec.Module.GetResourceStrings.TryGetValue(value, out value))
			{
				value = Value;
			}
			ec.Emit(OpCodes.Ldstr, value);
		}

		public override void EncodeAttributeValue(IMemberContext rc, AttributeEncoder enc, TypeSpec targetType, TypeSpec parameterType)
		{
			if (type != targetType)
			{
				enc.Encode(type);
			}
			enc.Encode(Value);
		}

		public override Constant ConvertExplicitly(bool in_checked_context, TypeSpec target_type)
		{
			return null;
		}

		public override Constant ConvertImplicitly(TypeSpec type)
		{
			if (IsDefaultValue && type.BuiltinType == BuiltinTypeSpec.Type.Object)
			{
				return new NullConstant(type, loc);
			}
			return base.ConvertImplicitly(type);
		}
	}
}
