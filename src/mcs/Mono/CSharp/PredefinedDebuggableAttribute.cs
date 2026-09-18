using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class PredefinedDebuggableAttribute : PredefinedAttribute
	{
		public PredefinedDebuggableAttribute(ModuleContainer module, string ns, string name)
			: base(module, ns, name)
		{
		}

		public void EmitAttribute(AssemblyBuilder builder, DebuggableAttribute.DebuggingModes modes)
		{
			PredefinedDebuggableAttribute debuggable = module.PredefinedAttributes.Debuggable;
			if (!debuggable.Define())
			{
				return;
			}
			MethodSpec methodSpec = null;
			foreach (MethodSpec item in MemberCache.FindMembers(debuggable.TypeSpec, Mono.CSharp.Constructor.ConstructorName, true))
			{
				if (item.Parameters.Count == 1 && item.Parameters.Types[0].Kind == MemberKind.Enum)
				{
					methodSpec = item;
				}
			}
			if (methodSpec != null)
			{
				AttributeEncoder attributeEncoder = new AttributeEncoder();
				attributeEncoder.Encode((int)modes);
				attributeEncoder.EncodeEmptyNamedArguments();
				builder.SetCustomAttribute((ConstructorInfo)methodSpec.GetMetaInfo(), attributeEncoder.ToArray());
			}
		}
	}
}
