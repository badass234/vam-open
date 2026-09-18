using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class PredefinedDecimalAttribute : PredefinedAttribute
	{
		public PredefinedDecimalAttribute(ModuleContainer module, string ns, string name)
			: base(module, ns, name)
		{
		}

		public void EmitAttribute(ParameterBuilder builder, decimal value, Location loc)
		{
			MethodSpec methodSpec = module.PredefinedMembers.DecimalConstantAttributeCtor.Resolve(loc);
			if (methodSpec != null)
			{
				int[] bits = decimal.GetBits(value);
				AttributeEncoder attributeEncoder = new AttributeEncoder();
				attributeEncoder.Encode((byte)(bits[3] >> 16));
				attributeEncoder.Encode((byte)(bits[3] >> 31));
				attributeEncoder.Encode((uint)bits[2]);
				attributeEncoder.Encode((uint)bits[1]);
				attributeEncoder.Encode((uint)bits[0]);
				attributeEncoder.EncodeEmptyNamedArguments();
				builder.SetCustomAttribute((ConstructorInfo)methodSpec.GetMetaInfo(), attributeEncoder.ToArray());
			}
		}

		public void EmitAttribute(FieldBuilder builder, decimal value, Location loc)
		{
			MethodSpec methodSpec = module.PredefinedMembers.DecimalConstantAttributeCtor.Resolve(loc);
			if (methodSpec != null)
			{
				int[] bits = decimal.GetBits(value);
				AttributeEncoder attributeEncoder = new AttributeEncoder();
				attributeEncoder.Encode((byte)((bits[3] & 0xFF0000) >> 16));
				attributeEncoder.Encode((byte)(bits[3] >> 31 << 7));
				attributeEncoder.Encode((uint)bits[2]);
				attributeEncoder.Encode((uint)bits[1]);
				attributeEncoder.Encode((uint)bits[0]);
				attributeEncoder.EncodeEmptyNamedArguments();
				builder.SetCustomAttribute((ConstructorInfo)methodSpec.GetMetaInfo(), attributeEncoder.ToArray());
			}
		}
	}
}
