using System.Reflection.Emit;

namespace Mono.CSharp
{
	public abstract class VariableReference : Expression, IAssignMethod, IMemoryLocation, IVariableReference, IFixedExpression
	{
		private LocalTemporary temp;

		public abstract bool IsLockedByStatement { get; set; }

		public abstract bool IsFixed { get; }

		public abstract bool IsRef { get; }

		public abstract string Name { get; }

		protected abstract ILocalVariable Variable { get; }

		public abstract VariableInfo VariableInfo { get; }

		public bool IsHoisted => GetHoistedVariable((AnonymousExpression)null) != null;

		public abstract HoistedVariable GetHoistedVariable(AnonymousExpression ae);

		public abstract void SetHasAddressTaken();

		public virtual void AddressOf(EmitContext ec, AddressOp mode)
		{
			HoistedVariable hoistedVariable = GetHoistedVariable(ec);
			if (hoistedVariable != null)
			{
				hoistedVariable.AddressOf(ec, mode);
			}
			else
			{
				Variable.EmitAddressOf(ec);
			}
		}

		public override bool ContainsEmitWithAwait()
		{
			return false;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			HoistedVariable hoistedVariable = GetHoistedVariable(ec);
			if (hoistedVariable != null)
			{
				return hoistedVariable.CreateExpressionTree();
			}
			Arguments arguments = new Arguments(1);
			arguments.Add(new Argument(this));
			return CreateExpressionFactoryCall(ec, "Constant", arguments);
		}

		public override Expression DoResolveLValue(ResolveContext rc, Expression right_side)
		{
			if (IsLockedByStatement)
			{
				rc.Report.Warning(728, 2, loc, "Possibly incorrect assignment to `{0}' which is the argument to a using or lock statement", Name);
			}
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			Emit(ec, false);
		}

		public override void EmitSideEffect(EmitContext ec)
		{
		}

		public void EmitLoad(EmitContext ec)
		{
			Variable.Emit(ec);
		}

		public void Emit(EmitContext ec, bool leave_copy)
		{
			HoistedVariable hoistedVariable = GetHoistedVariable(ec);
			if (hoistedVariable != null)
			{
				hoistedVariable.Emit(ec, leave_copy);
				return;
			}
			EmitLoad(ec);
			if (IsRef)
			{
				ec.EmitLoadFromPtr(type);
			}
			if (leave_copy)
			{
				ec.Emit(OpCodes.Dup);
				if (IsRef)
				{
					temp = new LocalTemporary(base.Type);
					temp.Store(ec);
				}
			}
		}

		public void EmitAssign(EmitContext ec, Expression source, bool leave_copy, bool prepare_for_load)
		{
			HoistedVariable hoistedVariable = GetHoistedVariable(ec);
			if (hoistedVariable != null)
			{
				hoistedVariable.EmitAssign(ec, source, leave_copy, prepare_for_load);
				return;
			}
			New obj = source as New;
			if (obj != null)
			{
				if (!obj.Emit(ec, this))
				{
					if (leave_copy)
					{
						EmitLoad(ec);
						if (IsRef)
						{
							ec.EmitLoadFromPtr(type);
						}
					}
					return;
				}
			}
			else
			{
				if (IsRef)
				{
					EmitLoad(ec);
				}
				source.Emit(ec);
			}
			if (leave_copy)
			{
				ec.Emit(OpCodes.Dup);
				if (IsRef)
				{
					temp = new LocalTemporary(base.Type);
					temp.Store(ec);
				}
			}
			if (IsRef)
			{
				ec.EmitStoreFromPtr(type);
			}
			else
			{
				Variable.EmitAssign(ec);
			}
			if (temp != null)
			{
				temp.Emit(ec);
				temp.Release(ec);
			}
		}

		public override Expression EmitToField(EmitContext ec)
		{
			HoistedVariable hoistedVariable = GetHoistedVariable(ec);
			if (hoistedVariable != null)
			{
				return hoistedVariable.EmitToField(ec);
			}
			return base.EmitToField(ec);
		}

		public HoistedVariable GetHoistedVariable(ResolveContext rc)
		{
			return GetHoistedVariable(rc.CurrentAnonymousMethod);
		}

		public HoistedVariable GetHoistedVariable(EmitContext ec)
		{
			return GetHoistedVariable(ec.CurrentAnonymousMethod);
		}

		public override string GetSignatureForError()
		{
			return Name;
		}
	}
}
