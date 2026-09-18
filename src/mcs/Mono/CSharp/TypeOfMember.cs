using System.Reflection.Emit;

namespace Mono.CSharp
{
	internal abstract class TypeOfMember<T> : Expression where T : MemberSpec
	{
		protected readonly T member;

		public override bool IsSideEffectFree => true;

		protected TypeOfMember(T member, Location loc)
		{
			this.member = member;
			base.loc = loc;
		}

		public override bool ContainsEmitWithAwait()
		{
			return false;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			Arguments arguments = new Arguments(2);
			arguments.Add(new Argument(this));
			arguments.Add(new Argument(new TypeOf(type, loc)));
			return CreateExpressionFactoryCall(ec, "Constant", arguments);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			PredefinedMember<MethodSpec> predefinedMember;
			if (member.DeclaringType.IsGenericOrParentIsGeneric)
			{
				predefinedMember = GetTypeFromHandleGeneric(ec);
				ec.Emit(OpCodes.Ldtoken, member.DeclaringType);
			}
			else
			{
				predefinedMember = GetTypeFromHandle(ec);
			}
			MethodSpec methodSpec = predefinedMember.Resolve(loc);
			if (methodSpec != null)
			{
				ec.Emit(OpCodes.Call, methodSpec);
			}
		}

		protected abstract PredefinedMember<MethodSpec> GetTypeFromHandle(EmitContext ec);

		protected abstract PredefinedMember<MethodSpec> GetTypeFromHandleGeneric(EmitContext ec);
	}
}
