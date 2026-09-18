using System.Reflection.Emit;

namespace Mono.CSharp
{
	internal sealed class TypeOfField : TypeOfMember<FieldSpec>
	{
		public TypeOfField(FieldSpec field, Location loc)
			: base(field, loc)
		{
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			type = ec.Module.PredefinedTypes.FieldInfo.Resolve();
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
		}

		protected override PredefinedMember<MethodSpec> GetTypeFromHandle(EmitContext ec)
		{
			return ec.Module.PredefinedMembers.FieldInfoGetFieldFromHandle;
		}

		protected override PredefinedMember<MethodSpec> GetTypeFromHandleGeneric(EmitContext ec)
		{
			return ec.Module.PredefinedMembers.FieldInfoGetFieldFromHandle2;
		}
	}
}
