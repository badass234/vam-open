using System.Reflection.Emit;
using Mono.CSharp.Nullable;

namespace Mono.CSharp
{
	public struct InstanceEmitter
	{
		private readonly Expression instance;

		private readonly bool addressRequired;

		public InstanceEmitter(Expression instance, bool addressLoad)
		{
			this.instance = instance;
			addressRequired = addressLoad;
		}

		public void Emit(EmitContext ec, bool conditionalAccess)
		{
			if (conditionalAccess && Expression.IsNeverNull(instance))
			{
				conditionalAccess = false;
			}
			Label label;
			Unwrap unwrap;
			if (conditionalAccess)
			{
				label = ec.DefineLabel();
				unwrap = instance as Unwrap;
			}
			else
			{
				label = default(Label);
				unwrap = null;
			}
			IMemoryLocation memoryLocation = null;
			bool flag = false;
			if (unwrap != null)
			{
				unwrap.Store(ec);
				unwrap.EmitCheck(ec);
				ec.Emit(OpCodes.Brtrue_S, label);
			}
			else
			{
				if (conditionalAccess && addressRequired)
				{
					memoryLocation = instance as VariableReference;
					if (memoryLocation == null)
					{
						memoryLocation = instance as LocalTemporary;
					}
					if (memoryLocation == null)
					{
						EmitLoad(ec, false);
						ec.Emit(OpCodes.Dup);
						ec.EmitLoadFromPtr(instance.Type);
						flag = true;
					}
					else
					{
						instance.Emit(ec);
					}
				}
				else
				{
					EmitLoad(ec, !conditionalAccess);
					if (conditionalAccess)
					{
						flag = !IsInexpensiveLoad();
						if (flag)
						{
							ec.Emit(OpCodes.Dup);
						}
					}
				}
				if (conditionalAccess)
				{
					if (instance.Type.Kind == MemberKind.TypeParameter)
					{
						ec.Emit(OpCodes.Box, instance.Type);
					}
					ec.Emit(OpCodes.Brtrue_S, label);
					if (flag)
					{
						ec.Emit(OpCodes.Pop);
					}
				}
			}
			if (!conditionalAccess)
			{
				return;
			}
			if (!ec.ConditionalAccess.Statement)
			{
				if (ec.ConditionalAccess.Type.IsNullableType)
				{
					LiftedNull.Create(ec.ConditionalAccess.Type, Location.Null).Emit(ec);
				}
				else
				{
					ec.EmitNull();
				}
			}
			ec.Emit(OpCodes.Br, ec.ConditionalAccess.EndLabel);
			ec.MarkLabel(label);
			if (memoryLocation != null)
			{
				memoryLocation.AddressOf(ec, AddressOp.Load);
			}
			else if (unwrap != null)
			{
				unwrap.Emit(ec);
				LocalBuilder temporaryLocal = ec.GetTemporaryLocal(unwrap.Type);
				ec.Emit(OpCodes.Stloc, temporaryLocal);
				ec.Emit(OpCodes.Ldloca, temporaryLocal);
				ec.FreeTemporaryLocal(temporaryLocal, unwrap.Type);
			}
			else if (!flag)
			{
				instance.Emit(ec);
			}
		}

		public void EmitLoad(EmitContext ec, bool boxInstance)
		{
			TypeSpec type = instance.Type;
			if (addressRequired)
			{
				IMemoryLocation memoryLocation = instance as IMemoryLocation;
				if (memoryLocation != null)
				{
					memoryLocation.AddressOf(ec, AddressOp.Load);
					return;
				}
				LocalTemporary localTemporary = new LocalTemporary(type);
				instance.Emit(ec);
				localTemporary.Store(ec);
				localTemporary.AddressOf(ec, AddressOp.Load);
			}
			else
			{
				instance.Emit(ec);
				if (boxInstance && RequiresBoxing())
				{
					ec.Emit(OpCodes.Box, type);
				}
			}
		}

		public TypeSpec GetStackType(EmitContext ec)
		{
			TypeSpec type = instance.Type;
			if (addressRequired)
			{
				return ReferenceContainer.MakeType(ec.Module, type);
			}
			if (type.IsStructOrEnum)
			{
				return ec.Module.Compiler.BuiltinTypes.Object;
			}
			return type;
		}

		private bool RequiresBoxing()
		{
			TypeSpec type = instance.Type;
			if (type.IsGenericParameter && !(instance is This) && TypeSpec.IsReferenceType(type))
			{
				return true;
			}
			if (type.IsStructOrEnum)
			{
				return true;
			}
			return false;
		}

		private bool IsInexpensiveLoad()
		{
			if (instance is Constant)
			{
				return instance.IsSideEffectFree;
			}
			if (RequiresBoxing())
			{
				return false;
			}
			VariableReference variableReference = instance as VariableReference;
			if (variableReference != null)
			{
				if (!variableReference.IsRef)
				{
					return !variableReference.IsHoisted;
				}
				return false;
			}
			if (instance is LocalTemporary)
			{
				return true;
			}
			FieldExpr fieldExpr = instance as FieldExpr;
			if (fieldExpr != null)
			{
				if (!fieldExpr.IsStatic)
				{
					return fieldExpr.InstanceExpression is This;
				}
				return true;
			}
			return false;
		}
	}
}
