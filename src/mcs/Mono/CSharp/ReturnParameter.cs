using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class ReturnParameter : ParameterBase
	{
		private MemberCore method;

		public override AttributeTargets AttributeTargets => AttributeTargets.ReturnValue;

		public override string[] ValidAttributeTargets => null;

		public ReturnParameter(MemberCore method, MethodBuilder mb, Location location)
		{
			this.method = method;
			try
			{
				builder = mb.DefineParameter(0, ParameterAttributes.None, "");
			}
			catch (ArgumentOutOfRangeException)
			{
				method.Compiler.Report.RuntimeMissingSupport(location, "custom attributes on the return type");
			}
		}

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Type == pa.CLSCompliant)
			{
				method.Compiler.Report.Warning(3023, 1, a.Location, "CLSCompliant attribute has no meaning when applied to return types. Try putting it on the method instead");
			}
			if (builder != null)
			{
				base.ApplyAttributeBuilder(a, ctor, cdata, pa);
			}
		}
	}
}
