using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public abstract class ParameterBase : Attributable
	{
		protected ParameterBuilder builder;

		public ParameterBuilder Builder => builder;

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.HasSecurityAttribute)
			{
				a.Error_InvalidSecurityParent();
			}
			else if (a.Type == pa.Dynamic)
			{
				a.Error_MisusedDynamicAttribute();
			}
			else
			{
				builder.SetCustomAttribute((ConstructorInfo)ctor.GetMetaInfo(), cdata);
			}
		}

		public override bool IsClsComplianceRequired()
		{
			return false;
		}
	}
}
