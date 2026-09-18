using System.Reflection.Emit;

namespace Mono.CSharp
{
	internal sealed class TypeOfMethod : TypeOfMember<MethodSpec>
	{
		public TypeOfMethod(MethodSpec method, Location loc)
			: base(method, loc)
		{
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			if (member.IsConstructor)
			{
				type = ec.Module.PredefinedTypes.ConstructorInfo.Resolve();
			}
			else
			{
				type = ec.Module.PredefinedTypes.MethodInfo.Resolve();
			}
			if (type == null)
			{
				return null;
			}
			return base.DoResolve(ec);
		}

		public override void Emit(EmitContext ec)
		{
			ec.Emit(OpCodes.Ldtoken, member);
			base.Emit(ec);
			ec.Emit(OpCodes.Castclass, type);
		}

		protected override PredefinedMember<MethodSpec> GetTypeFromHandle(EmitContext ec)
		{
			return ec.Module.PredefinedMembers.MethodInfoGetMethodFromHandle;
		}

		protected override PredefinedMember<MethodSpec> GetTypeFromHandleGeneric(EmitContext ec)
		{
			return ec.Module.PredefinedMembers.MethodInfoGetMethodFromHandle2;
		}
	}
}
