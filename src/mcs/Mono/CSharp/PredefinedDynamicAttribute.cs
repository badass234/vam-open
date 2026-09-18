using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class PredefinedDynamicAttribute : PredefinedAttribute
	{
		private MethodSpec tctor;

		public PredefinedDynamicAttribute(ModuleContainer module, string ns, string name)
			: base(module, ns, name)
		{
		}

		public void EmitAttribute(FieldBuilder builder, TypeSpec type, Location loc)
		{
			if (ResolveTransformationCtor(loc))
			{
				CustomAttributeBuilder customAttribute = new CustomAttributeBuilder((ConstructorInfo)tctor.GetMetaInfo(), new object[1] { GetTransformationFlags(type) });
				builder.SetCustomAttribute(customAttribute);
			}
		}

		public void EmitAttribute(ParameterBuilder builder, TypeSpec type, Location loc)
		{
			if (ResolveTransformationCtor(loc))
			{
				CustomAttributeBuilder customAttribute = new CustomAttributeBuilder((ConstructorInfo)tctor.GetMetaInfo(), new object[1] { GetTransformationFlags(type) });
				builder.SetCustomAttribute(customAttribute);
			}
		}

		public void EmitAttribute(PropertyBuilder builder, TypeSpec type, Location loc)
		{
			if (ResolveTransformationCtor(loc))
			{
				CustomAttributeBuilder customAttribute = new CustomAttributeBuilder((ConstructorInfo)tctor.GetMetaInfo(), new object[1] { GetTransformationFlags(type) });
				builder.SetCustomAttribute(customAttribute);
			}
		}

		public void EmitAttribute(TypeBuilder builder, TypeSpec type, Location loc)
		{
			if (ResolveTransformationCtor(loc))
			{
				CustomAttributeBuilder customAttribute = new CustomAttributeBuilder((ConstructorInfo)tctor.GetMetaInfo(), new object[1] { GetTransformationFlags(type) });
				builder.SetCustomAttribute(customAttribute);
			}
		}

		private static bool[] GetTransformationFlags(TypeSpec t)
		{
			ArrayContainer arrayContainer = t as ArrayContainer;
			if (arrayContainer != null)
			{
				bool[] transformationFlags = GetTransformationFlags(arrayContainer.Element);
				if (transformationFlags == null)
				{
					return new bool[2];
				}
				bool[] array = new bool[transformationFlags.Length + 1];
				array[0] = false;
				Array.Copy(transformationFlags, 0, array, 1, transformationFlags.Length);
				return array;
			}
			if (t == null)
			{
				return null;
			}
			if (t.IsGeneric)
			{
				List<bool> list = null;
				TypeSpec[] typeArguments = t.TypeArguments;
				for (int i = 0; i < typeArguments.Length; i++)
				{
					bool[] transformationFlags = GetTransformationFlags(typeArguments[i]);
					if (transformationFlags != null)
					{
						if (list == null)
						{
							list = new List<bool>();
							for (int j = 0; j <= i; j++)
							{
								list.Add(false);
							}
						}
						list.AddRange(transformationFlags);
					}
					else
					{
						list?.Add(false);
					}
				}
				if (list != null)
				{
					return list.ToArray();
				}
			}
			if (t.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				return new bool[1] { true };
			}
			return null;
		}

		private bool ResolveTransformationCtor(Location loc)
		{
			if (tctor != null)
			{
				return true;
			}
			tctor = module.PredefinedMembers.DynamicAttributeCtor.Resolve(loc);
			return tctor != null;
		}
	}
}
