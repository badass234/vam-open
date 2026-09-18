using System;
using System.Globalization;
using System.Linq.Expressions;
using Mono.CSharp.Nullable;

namespace Mono.CSharp
{
	public abstract class Constant : Expression
	{
		private static readonly NumberFormatInfo nfi = CultureInfo.InvariantCulture.NumberFormat;

		public abstract bool IsDefaultValue { get; }

		public abstract bool IsNegative { get; }

		public virtual bool IsLiteral => false;

		public virtual bool IsOneInteger => false;

		public override bool IsSideEffectFree => true;

		public virtual bool IsZeroInteger => false;

		protected Constant(Location loc)
		{
			base.loc = loc;
		}

		public override string ToString()
		{
			return GetType().Name + " (" + GetValueAsLiteral() + ")";
		}

		public abstract object GetValue();

		public abstract long GetValueAsLong();

		public abstract string GetValueAsLiteral();

		public virtual object GetTypedValue()
		{
			return GetValue();
		}

		public override void Error_ValueCannotBeConverted(ResolveContext ec, TypeSpec target, bool expl)
		{
			if (!expl && IsLiteral && type.BuiltinType != BuiltinTypeSpec.Type.Double && BuiltinTypeSpec.IsPrimitiveTypeOrDecimal(target) && BuiltinTypeSpec.IsPrimitiveTypeOrDecimal(type))
			{
				ec.Report.Error(31, loc, "Constant value `{0}' cannot be converted to a `{1}'", GetValueAsLiteral(), target.GetSignatureForError());
			}
			else
			{
				base.Error_ValueCannotBeConverted(ec, target, expl);
			}
		}

		public Constant ImplicitConversionRequired(ResolveContext ec, TypeSpec type)
		{
			Constant constant = ConvertImplicitly(type);
			if (constant == null)
			{
				Error_ValueCannotBeConverted(ec, type, false);
			}
			return constant;
		}

		public override bool ContainsEmitWithAwait()
		{
			return false;
		}

		public virtual Constant ConvertImplicitly(TypeSpec type)
		{
			if (base.type == type)
			{
				return this;
			}
			if (!Convert.ImplicitNumericConversionExists(base.type, type))
			{
				return null;
			}
			bool error;
			object v = ChangeType(GetValue(), type, out error);
			if (error)
			{
				throw new InternalErrorException("Missing constant conversion between `{0}' and `{1}'", base.Type.GetSignatureForError(), type.GetSignatureForError());
			}
			return CreateConstantFromValue(type, v, loc);
		}

		public static Constant CreateConstantFromValue(TypeSpec t, object v, Location loc)
		{
			switch (t.BuiltinType)
			{
			case BuiltinTypeSpec.Type.Int:
				return new IntConstant(t, (int)v, loc);
			case BuiltinTypeSpec.Type.String:
				return new StringConstant(t, (string)v, loc);
			case BuiltinTypeSpec.Type.UInt:
				return new UIntConstant(t, (uint)v, loc);
			case BuiltinTypeSpec.Type.Long:
				return new LongConstant(t, (long)v, loc);
			case BuiltinTypeSpec.Type.ULong:
				return new ULongConstant(t, (ulong)v, loc);
			case BuiltinTypeSpec.Type.Float:
				return new FloatConstant(t, (float)v, loc);
			case BuiltinTypeSpec.Type.Double:
				return new DoubleConstant(t, (double)v, loc);
			case BuiltinTypeSpec.Type.Short:
				return new ShortConstant(t, (short)v, loc);
			case BuiltinTypeSpec.Type.UShort:
				return new UShortConstant(t, (ushort)v, loc);
			case BuiltinTypeSpec.Type.SByte:
				return new SByteConstant(t, (sbyte)v, loc);
			case BuiltinTypeSpec.Type.Byte:
				return new ByteConstant(t, (byte)v, loc);
			case BuiltinTypeSpec.Type.Char:
				return new CharConstant(t, (char)v, loc);
			case BuiltinTypeSpec.Type.FirstPrimitive:
				return new BoolConstant(t, (bool)v, loc);
			case BuiltinTypeSpec.Type.Decimal:
				return new DecimalConstant(t, (decimal)v, loc);
			default:
				if (t.IsEnum)
				{
					return new EnumConstant(CreateConstantFromValue(EnumSpec.GetUnderlyingType(t), v, loc), t);
				}
				if (v == null)
				{
					if (t.IsNullableType)
					{
						return LiftedNull.Create(t, loc);
					}
					if (TypeSpec.IsReferenceType(t))
					{
						return new NullConstant(t, loc);
					}
				}
				return null;
			}
		}

		public static Constant ExtractConstantFromValue(TypeSpec t, object v, Location loc)
		{
			switch (t.BuiltinType)
			{
			case BuiltinTypeSpec.Type.Int:
				if (v is int)
				{
					return new IntConstant(t, (int)v, loc);
				}
				break;
			case BuiltinTypeSpec.Type.String:
				if (v is string)
				{
					return new StringConstant(t, (string)v, loc);
				}
				break;
			case BuiltinTypeSpec.Type.UInt:
				if (v is uint)
				{
					return new UIntConstant(t, (uint)v, loc);
				}
				break;
			case BuiltinTypeSpec.Type.Long:
				if (v is long)
				{
					return new LongConstant(t, (long)v, loc);
				}
				break;
			case BuiltinTypeSpec.Type.ULong:
				if (v is ulong)
				{
					return new ULongConstant(t, (ulong)v, loc);
				}
				break;
			case BuiltinTypeSpec.Type.Float:
				if (v is float)
				{
					return new FloatConstant(t, (float)v, loc);
				}
				break;
			case BuiltinTypeSpec.Type.Double:
				if (v is double)
				{
					return new DoubleConstant(t, (double)v, loc);
				}
				break;
			case BuiltinTypeSpec.Type.Short:
				if (v is short)
				{
					return new ShortConstant(t, (short)v, loc);
				}
				break;
			case BuiltinTypeSpec.Type.UShort:
				if (v is ushort)
				{
					return new UShortConstant(t, (ushort)v, loc);
				}
				break;
			case BuiltinTypeSpec.Type.SByte:
				if (v is sbyte)
				{
					return new SByteConstant(t, (sbyte)v, loc);
				}
				break;
			case BuiltinTypeSpec.Type.Byte:
				if (v is byte)
				{
					return new ByteConstant(t, (byte)v, loc);
				}
				break;
			case BuiltinTypeSpec.Type.Char:
				if (v is char)
				{
					return new CharConstant(t, (char)v, loc);
				}
				break;
			case BuiltinTypeSpec.Type.FirstPrimitive:
				if (v is bool)
				{
					return new BoolConstant(t, (bool)v, loc);
				}
				break;
			case BuiltinTypeSpec.Type.Decimal:
				if (v is decimal)
				{
					return new DecimalConstant(t, (decimal)v, loc);
				}
				break;
			}
			if (t.IsEnum)
			{
				return new EnumConstant(CreateConstantFromValue(EnumSpec.GetUnderlyingType(t), v, loc), t);
			}
			if (v == null)
			{
				if (t.IsNullableType)
				{
					return LiftedNull.Create(t, loc);
				}
				if (TypeSpec.IsReferenceType(t))
				{
					return new NullConstant(t, loc);
				}
			}
			return null;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			Arguments arguments = new Arguments(2);
			arguments.Add(new Argument(this));
			arguments.Add(new Argument(new TypeOf(type, loc)));
			return CreateExpressionFactoryCall(ec, "Constant", arguments);
		}

		public abstract Constant ConvertExplicitly(bool in_checked_context, TypeSpec target_type);

		private static object ChangeType(object value, TypeSpec targetType, out bool error)
		{
			IConvertible convertible = value as IConvertible;
			if (convertible == null)
			{
				error = true;
				return null;
			}
			error = false;
			try
			{
				switch (targetType.BuiltinType)
				{
				case BuiltinTypeSpec.Type.FirstPrimitive:
					return convertible.ToBoolean(nfi);
				case BuiltinTypeSpec.Type.Byte:
					return convertible.ToByte(nfi);
				case BuiltinTypeSpec.Type.Char:
					return convertible.ToChar(nfi);
				case BuiltinTypeSpec.Type.Short:
					return convertible.ToInt16(nfi);
				case BuiltinTypeSpec.Type.Int:
					return convertible.ToInt32(nfi);
				case BuiltinTypeSpec.Type.Long:
					return convertible.ToInt64(nfi);
				case BuiltinTypeSpec.Type.SByte:
					return convertible.ToSByte(nfi);
				case BuiltinTypeSpec.Type.Decimal:
					if (convertible.GetType() == typeof(char))
					{
						return (decimal)convertible.ToInt32(nfi);
					}
					return convertible.ToDecimal(nfi);
				case BuiltinTypeSpec.Type.Double:
					if (convertible.GetType() == typeof(char))
					{
						return (double)convertible.ToInt32(nfi);
					}
					return convertible.ToDouble(nfi);
				case BuiltinTypeSpec.Type.Float:
					if (convertible.GetType() == typeof(char))
					{
						return (float)convertible.ToInt32(nfi);
					}
					return convertible.ToSingle(nfi);
				case BuiltinTypeSpec.Type.String:
					return convertible.ToString(nfi);
				case BuiltinTypeSpec.Type.UShort:
					return convertible.ToUInt16(nfi);
				case BuiltinTypeSpec.Type.UInt:
					return convertible.ToUInt32(nfi);
				case BuiltinTypeSpec.Type.ULong:
					return convertible.ToUInt64(nfi);
				case BuiltinTypeSpec.Type.Object:
					return value;
				case BuiltinTypeSpec.Type.IntPtr:
				case BuiltinTypeSpec.Type.UIntPtr:
				case BuiltinTypeSpec.Type.Dynamic:
					break;
				}
			}
			catch
			{
			}
			error = true;
			return null;
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			return this;
		}

		public Constant Reduce(ResolveContext ec, TypeSpec target_type)
		{
			try
			{
				return TryReduceConstant(ec, target_type);
			}
			catch (OverflowException)
			{
				if (ec.ConstantCheckState && base.Type.BuiltinType != BuiltinTypeSpec.Type.Decimal)
				{
					ec.Report.Error(221, loc, "Constant value `{0}' cannot be converted to a `{1}' (use `unchecked' syntax to override)", GetValueAsLiteral(), target_type.GetSignatureForError());
				}
				else
				{
					Error_ValueCannotBeConverted(ec, target_type, false);
				}
				return New.Constantify(target_type, loc);
			}
		}

		public Constant TryReduce(ResolveContext rc, TypeSpec targetType)
		{
			try
			{
				return TryReduceConstant(rc, targetType);
			}
			catch (OverflowException)
			{
				return null;
			}
		}

		private Constant TryReduceConstant(ResolveContext ec, TypeSpec target_type)
		{
			if (base.Type == target_type)
			{
				if (IsLiteral)
				{
					return CreateConstantFromValue(target_type, GetValue(), loc);
				}
				return this;
			}
			if (target_type.IsEnum)
			{
				Constant constant = TryReduceConstant(ec, EnumSpec.GetUnderlyingType(target_type));
				if (constant == null)
				{
					return null;
				}
				return new EnumConstant(constant, target_type);
			}
			return ConvertExplicitly(ec.ConstantCheckState, target_type);
		}

		public bool IsDefaultInitializer(TypeSpec type)
		{
			if (type == base.Type)
			{
				return IsDefaultValue;
			}
			return this is NullLiteral;
		}

		public override void EmitSideEffect(EmitContext ec)
		{
		}

		public sealed override Expression Clone(CloneContext clonectx)
		{
			return this;
		}

		protected override void CloneTo(CloneContext clonectx, Expression target)
		{
			throw new NotSupportedException("should not be reached");
		}

		public override System.Linq.Expressions.Expression MakeExpression(BuilderContext ctx)
		{
			return System.Linq.Expressions.Expression.Constant(GetTypedValue(), type.GetMetaInfo());
		}

		public new bool Resolve(ResolveContext rc)
		{
			return true;
		}
	}
}
