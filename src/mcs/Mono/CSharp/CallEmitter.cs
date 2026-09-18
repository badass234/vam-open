using System;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	internal struct CallEmitter
	{
		public Expression InstanceExpression;

		public bool DuplicateArguments;

		public bool InstanceExpressionOnStack;

		public bool HasAwaitArguments;

		public bool ConditionalAccess;

		public Arguments EmittedArguments;

		public void Emit(EmitContext ec, MethodSpec method, Arguments Arguments, Location loc)
		{
			EmitPredefined(ec, method, Arguments, false, loc);
		}

		public void EmitStatement(EmitContext ec, MethodSpec method, Arguments Arguments, Location loc)
		{
			EmitPredefined(ec, method, Arguments, true, loc);
		}

		public void EmitPredefined(EmitContext ec, MethodSpec method, Arguments Arguments, bool statement = false, Location? loc = null)
		{
			Expression expression = null;
			if (!HasAwaitArguments && ec.HasSet(BuilderContext.Options.AsyncBody))
			{
				HasAwaitArguments = Arguments?.ContainsEmitWithAwait() ?? false;
				if (HasAwaitArguments && InstanceExpressionOnStack)
				{
					throw new NotSupportedException();
				}
			}
			LocalTemporary localTemporary = null;
			OpCode opCode;
			if (method.IsStatic)
			{
				opCode = OpCodes.Call;
			}
			else
			{
				opCode = (IsVirtualCallRequired(InstanceExpression, method) ? OpCodes.Callvirt : OpCodes.Call);
				if (HasAwaitArguments)
				{
					expression = InstanceExpression.EmitToField(ec);
					InstanceEmitter instanceEmitter = new InstanceEmitter(expression, IsAddressCall(expression, opCode, method.DeclaringType));
					if (Arguments == null)
					{
						instanceEmitter.EmitLoad(ec, true);
					}
				}
				else if (!InstanceExpressionOnStack)
				{
					InstanceEmitter instanceEmitter2 = new InstanceEmitter(InstanceExpression, IsAddressCall(InstanceExpression, opCode, method.DeclaringType));
					instanceEmitter2.Emit(ec, ConditionalAccess);
					if (DuplicateArguments)
					{
						ec.Emit(OpCodes.Dup);
						if (Arguments != null && Arguments.Count != 0)
						{
							localTemporary = new LocalTemporary(instanceEmitter2.GetStackType(ec));
							localTemporary.Store(ec);
							expression = localTemporary;
						}
					}
				}
			}
			if (Arguments != null && !InstanceExpressionOnStack)
			{
				EmittedArguments = Arguments.Emit(ec, DuplicateArguments, HasAwaitArguments);
				if (EmittedArguments != null)
				{
					if (expression != null)
					{
						new InstanceEmitter(expression, IsAddressCall(expression, opCode, method.DeclaringType)).Emit(ec, ConditionalAccess);
						localTemporary?.Release(ec);
					}
					EmittedArguments.Emit(ec);
				}
			}
			if (opCode == OpCodes.Callvirt && (InstanceExpression.Type.IsGenericParameter || InstanceExpression.Type.IsStructOrEnum))
			{
				ec.Emit(OpCodes.Constrained, InstanceExpression.Type);
			}
			if (loc.HasValue)
			{
				ec.MarkCallEntry(loc.Value);
			}
			InstanceExpression = expression;
			if (method.Parameters.HasArglist)
			{
				Type[] varargsTypes = GetVarargsTypes(method, Arguments);
				ec.Emit(opCode, method, varargsTypes);
			}
			else
			{
				ec.Emit(opCode, method);
			}
			if (statement && method.ReturnType.Kind != MemberKind.Void)
			{
				ec.Emit(OpCodes.Pop);
			}
		}

		private static Type[] GetVarargsTypes(MethodSpec method, Arguments arguments)
		{
			AParametersCollection parameters = method.Parameters;
			return ((Arglist)arguments[parameters.Count - 1].Expr).ArgumentTypes;
		}

		private static bool IsVirtualCallRequired(Expression instance, MethodSpec method)
		{
			TypeSpec declaringType = method.DeclaringType;
			if (declaringType.IsStruct || declaringType.IsEnum)
			{
				return false;
			}
			if (instance is BaseThis)
			{
				return false;
			}
			if (!method.IsVirtual && Expression.IsNeverNull(instance) && !instance.Type.IsGenericParameter)
			{
				return false;
			}
			return true;
		}

		private static bool IsAddressCall(Expression instance, OpCode callOpcode, TypeSpec declaringType)
		{
			TypeSpec type = instance.Type;
			if ((!type.IsStructOrEnum || (!(callOpcode == OpCodes.Callvirt) && (!(callOpcode == OpCodes.Call) || !declaringType.IsStruct))) && !type.IsGenericParameter)
			{
				return declaringType.IsNullableType;
			}
			return true;
		}
	}
}
