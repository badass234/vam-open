using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Mono.CSharp.Nullable;

namespace Mono.CSharp
{
	public class New : ExpressionStatement, IMemoryLocation
	{
		protected Arguments arguments;

		protected Expression RequestedType;

		protected MethodSpec method;

		public Arguments Arguments => arguments;

		public bool IsGeneratedStructConstructor
		{
			get
			{
				if (arguments == null && method == null && type.IsStruct)
				{
					return GetType() == typeof(New);
				}
				return false;
			}
		}

		public Expression TypeExpression => RequestedType;

		public New(Expression requested_type, Arguments arguments, Location l)
		{
			RequestedType = requested_type;
			this.arguments = arguments;
			loc = l;
		}

		public static Constant Constantify(TypeSpec t, Location loc)
		{
			switch (t.BuiltinType)
			{
			case BuiltinTypeSpec.Type.Int:
				return new IntConstant(t, 0, loc);
			case BuiltinTypeSpec.Type.UInt:
				return new UIntConstant(t, 0u, loc);
			case BuiltinTypeSpec.Type.Long:
				return new LongConstant(t, 0L, loc);
			case BuiltinTypeSpec.Type.ULong:
				return new ULongConstant(t, 0uL, loc);
			case BuiltinTypeSpec.Type.Float:
				return new FloatConstant(t, 0.0, loc);
			case BuiltinTypeSpec.Type.Double:
				return new DoubleConstant(t, 0.0, loc);
			case BuiltinTypeSpec.Type.Short:
				return new ShortConstant(t, 0, loc);
			case BuiltinTypeSpec.Type.UShort:
				return new UShortConstant(t, 0, loc);
			case BuiltinTypeSpec.Type.SByte:
				return new SByteConstant(t, 0, loc);
			case BuiltinTypeSpec.Type.Byte:
				return new ByteConstant(t, 0, loc);
			case BuiltinTypeSpec.Type.Char:
				return new CharConstant(t, '\0', loc);
			case BuiltinTypeSpec.Type.FirstPrimitive:
				return new BoolConstant(t, false, loc);
			case BuiltinTypeSpec.Type.Decimal:
				return new DecimalConstant(t, 0m, loc);
			default:
				if (t.IsEnum)
				{
					return new EnumConstant(Constantify(EnumSpec.GetUnderlyingType(t), loc), t);
				}
				if (t.IsNullableType)
				{
					return LiftedNull.Create(t, loc);
				}
				return null;
			}
		}

		public override bool ContainsEmitWithAwait()
		{
			if (arguments != null)
			{
				return arguments.ContainsEmitWithAwait();
			}
			return false;
		}

		public Expression CheckComImport(ResolveContext ec)
		{
			if (!type.IsInterface)
			{
				return null;
			}
			TypeSpec attributeCoClass = type.MemberDefinition.GetAttributeCoClass();
			if (attributeCoClass == null)
			{
				return null;
			}
			New expr = new New(new TypeExpression(attributeCoClass, loc), arguments, loc);
			return new Cast(new TypeExpression(type, loc), expr, loc).Resolve(ec);
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			Arguments arguments;
			if (method == null)
			{
				arguments = new Arguments(1);
				arguments.Add(new Argument(new TypeOf(type, loc)));
			}
			else
			{
				arguments = Arguments.CreateForExpressionTree(ec, this.arguments, new TypeOfMethod(method, loc));
			}
			return CreateExpressionFactoryCall(ec, "New", arguments);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			type = RequestedType.ResolveAsType(ec);
			if (type == null)
			{
				return null;
			}
			eclass = ExprClass.Value;
			if (type.IsPointer)
			{
				ec.Report.Error(1919, loc, "Unsafe type `{0}' cannot be used in an object creation expression", type.GetSignatureForError());
				return null;
			}
			if (arguments == null)
			{
				Constant constant = Constantify(type, RequestedType.Location);
				if (constant != null)
				{
					return ReducedExpression.Create(constant, this);
				}
			}
			if (type.IsDelegate)
			{
				return new NewDelegate(type, arguments, loc).Resolve(ec);
			}
			TypeParameterSpec typeParameterSpec = type as TypeParameterSpec;
			if (typeParameterSpec != null)
			{
				if ((typeParameterSpec.SpecialConstraint & (SpecialConstraint.Constructor | SpecialConstraint.Struct)) == 0 && !TypeSpec.IsValueType(typeParameterSpec))
				{
					ec.Report.Error(304, loc, "Cannot create an instance of the variable type `{0}' because it does not have the new() constraint", type.GetSignatureForError());
				}
				if (arguments != null && arguments.Count != 0)
				{
					ec.Report.Error(417, loc, "`{0}': cannot provide arguments when creating an instance of a variable type", type.GetSignatureForError());
				}
				return this;
			}
			if (type.IsStatic)
			{
				ec.Report.SymbolRelatedToPreviousError(type);
				ec.Report.Error(712, loc, "Cannot create an instance of the static class `{0}'", type.GetSignatureForError());
				return null;
			}
			if (type.IsInterface || type.IsAbstract)
			{
				if (!TypeManager.IsGenericType(type))
				{
					RequestedType = CheckComImport(ec);
					if (RequestedType != null)
					{
						return RequestedType;
					}
				}
				ec.Report.SymbolRelatedToPreviousError(type);
				ec.Report.Error(144, loc, "Cannot create an instance of the abstract class or interface `{0}'", type.GetSignatureForError());
				return null;
			}
			bool dynamic;
			if (arguments != null)
			{
				arguments.Resolve(ec, out dynamic);
			}
			else
			{
				dynamic = false;
			}
			method = Expression.ConstructorLookup(ec, type, ref arguments, loc);
			if (dynamic)
			{
				arguments.Insert(0, new Argument(new TypeOf(type, loc).Resolve(ec), Argument.AType.DynamicTypeName));
				return new DynamicConstructorBinder(type, arguments, loc).Resolve(ec);
			}
			return this;
		}

		private void DoEmitTypeParameter(EmitContext ec)
		{
			MethodSpec methodSpec = ec.Module.PredefinedMembers.ActivatorCreateInstance.Resolve(loc);
			if (methodSpec != null)
			{
				MethodSpec methodSpec2 = methodSpec.MakeGenericMethod(ec.MemberContext, type);
				ec.Emit(OpCodes.Call, methodSpec2);
			}
		}

		public virtual bool Emit(EmitContext ec, IMemoryLocation target)
		{
			bool isStructOrEnum = type.IsStructOrEnum;
			VariableReference variableReference = target as VariableReference;
			if (((target != null) & isStructOrEnum) && (variableReference != null || method == null))
			{
				target.AddressOf(ec, AddressOp.Store);
			}
			else if (variableReference != null && variableReference.IsRef)
			{
				variableReference.EmitLoad(ec);
			}
			if (arguments != null)
			{
				if (ec.HasSet(BuilderContext.Options.AsyncBody) && arguments.Count > ((!(this is NewInitialize)) ? 1 : 0) && arguments.ContainsEmitWithAwait())
				{
					arguments = arguments.Emit(ec, false, true);
				}
				arguments.Emit(ec);
			}
			if (isStructOrEnum)
			{
				if (method == null)
				{
					ec.Emit(OpCodes.Initobj, type);
					return false;
				}
				if (variableReference != null)
				{
					ec.MarkCallEntry(loc);
					ec.Emit(OpCodes.Call, method);
					return false;
				}
			}
			if (type is TypeParameterSpec)
			{
				DoEmitTypeParameter(ec);
				return true;
			}
			ec.MarkCallEntry(loc);
			ec.Emit(OpCodes.Newobj, method);
			return true;
		}

		public override void Emit(EmitContext ec)
		{
			LocalTemporary localTemporary = null;
			if (method == null && type.IsStructOrEnum)
			{
				localTemporary = new LocalTemporary(type);
			}
			if (!Emit(ec, localTemporary))
			{
				localTemporary.Emit(ec);
			}
		}

		public override void EmitStatement(EmitContext ec)
		{
			LocalTemporary target = null;
			if (method == null && TypeSpec.IsValueType(type))
			{
				target = new LocalTemporary(type);
			}
			if (Emit(ec, target))
			{
				ec.Emit(OpCodes.Pop);
			}
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			if (arguments != null)
			{
				arguments.FlowAnalysis(fc);
			}
		}

		public void AddressOf(EmitContext ec, AddressOp mode)
		{
			EmitAddressOf(ec, mode);
		}

		protected virtual IMemoryLocation EmitAddressOf(EmitContext ec, AddressOp mode)
		{
			LocalTemporary localTemporary = new LocalTemporary(type);
			if (type is TypeParameterSpec)
			{
				DoEmitTypeParameter(ec);
				localTemporary.Store(ec);
				localTemporary.AddressOf(ec, mode);
				return localTemporary;
			}
			localTemporary.AddressOf(ec, AddressOp.Store);
			if (method == null)
			{
				ec.Emit(OpCodes.Initobj, type);
			}
			else
			{
				if (arguments != null)
				{
					arguments.Emit(ec);
				}
				ec.Emit(OpCodes.Call, method);
			}
			localTemporary.AddressOf(ec, mode);
			return localTemporary;
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			New obj = (New)t;
			obj.RequestedType = RequestedType.Clone(clonectx);
			if (arguments != null)
			{
				obj.arguments = arguments.Clone(clonectx);
			}
		}

		public override System.Linq.Expressions.Expression MakeExpression(BuilderContext ctx)
		{
			return System.Linq.Expressions.Expression.New((ConstructorInfo)method.GetMetaInfo(), Arguments.MakeExpression(arguments, ctx));
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
