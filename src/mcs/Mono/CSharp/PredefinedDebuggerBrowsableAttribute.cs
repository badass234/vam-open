using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class PredefinedDebuggerBrowsableAttribute : PredefinedAttribute
	{
		public PredefinedDebuggerBrowsableAttribute(ModuleContainer module, string ns, string name)
			: base(module, ns, name)
		{
		}

		public void EmitAttribute(FieldBuilder builder, DebuggerBrowsableState state)
		{
			MethodSpec methodSpec = module.PredefinedMembers.DebuggerBrowsableAttributeCtor.Get();
			if (methodSpec != null)
			{
				AttributeEncoder attributeEncoder = new AttributeEncoder();
				attributeEncoder.Encode((int)state);
				attributeEncoder.EncodeEmptyNamedArguments();
				builder.SetCustomAttribute((ConstructorInfo)methodSpec.GetMetaInfo(), attributeEncoder.ToArray());
			}
		}
	}
}
