using System;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class LocalTemporary : Expression, IMemoryLocation, IAssignMethod
	{
		private LocalBuilder builder;

		public LocalBuilder Builder => builder;

		public LocalTemporary(TypeSpec t)
		{
			type = t;
			eclass = ExprClass.Value;
		}

		public LocalTemporary(LocalBuilder b, TypeSpec t)
			: this(t)
		{
			builder = b;
		}

		public void Release(EmitContext ec)
		{
			ec.FreeTemporaryLocal(builder, type);
			builder = null;
		}

		public override bool ContainsEmitWithAwait()
		{
			return false;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			Arguments arguments = new Arguments(1);
			arguments.Add(new Argument(this));
			return CreateExpressionFactoryCall(ec, "Constant", arguments);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			return this;
		}

		public override Expression DoResolveLValue(ResolveContext ec, Expression right_side)
		{
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			if (builder == null)
			{
				throw new InternalErrorException("Emit without Store, or after Release");
			}
			ec.Emit(OpCodes.Ldloc, builder);
		}

		public void Emit(EmitContext ec, bool leave_copy)
		{
			Emit(ec);
			if (leave_copy)
			{
				Emit(ec);
			}
		}

		public void EmitAssign(EmitContext ec, Expression source, bool leave_copy, bool isCompound)
		{
			if (isCompound)
			{
				throw new NotImplementedException();
			}
			source.Emit(ec);
			Store(ec);
			if (leave_copy)
			{
				Emit(ec);
			}
		}

		public void Store(EmitContext ec)
		{
			if (builder == null)
			{
				builder = ec.GetTemporaryLocal(type);
			}
			ec.Emit(OpCodes.Stloc, builder);
		}

		public void AddressOf(EmitContext ec, AddressOp mode)
		{
			if (builder == null)
			{
				builder = ec.GetTemporaryLocal(type);
			}
			if (builder.LocalType.IsByRef)
			{
				ec.Emit(OpCodes.Ldloc, builder);
			}
			else
			{
				ec.Emit(OpCodes.Ldloca, builder);
			}
		}
	}
}
