using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class PredefinedStateMachineAttribute : PredefinedAttribute
	{
		public PredefinedStateMachineAttribute(ModuleContainer module, string ns, string name)
			: base(module, ns, name)
		{
		}

		public void EmitAttribute(MethodBuilder builder, StateMachine type)
		{
			MethodSpec methodSpec = module.PredefinedMembers.AsyncStateMachineAttributeCtor.Get();
			if (methodSpec != null)
			{
				AttributeEncoder attributeEncoder = new AttributeEncoder();
				attributeEncoder.EncodeTypeName(type);
				attributeEncoder.EncodeEmptyNamedArguments();
				builder.SetCustomAttribute((ConstructorInfo)methodSpec.GetMetaInfo(), attributeEncoder.ToArray());
			}
		}
	}
}
