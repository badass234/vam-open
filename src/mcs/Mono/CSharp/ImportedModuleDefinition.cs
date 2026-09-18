using System;
using System.Collections.Generic;
using System.Reflection;

namespace Mono.CSharp
{
	public class ImportedModuleDefinition
	{
		private readonly Module module;

		private bool cls_compliant;

		public bool IsCLSCompliant => cls_compliant;

		public string Name => module.Name;

		public ImportedModuleDefinition(Module module)
		{
			this.module = module;
		}

		public void ReadAttributes()
		{
			foreach (CustomAttributeData customAttribute in CustomAttributeData.GetCustomAttributes(module))
			{
				Type declaringType = customAttribute.Constructor.DeclaringType;
				if (declaringType.Name == "CLSCompliantAttribute" && !(declaringType.Namespace != "System"))
				{
					cls_compliant = (bool)customAttribute.ConstructorArguments[0].Value;
				}
			}
		}

		public List<Attribute> ReadAssemblyAttributes()
		{
			Type type = module.GetType(AssemblyAttributesPlaceholder.GetGeneratedName(Name));
			if (type == null)
			{
				return null;
			}
			type.GetField(AssemblyAttributesPlaceholder.AssemblyFieldName, BindingFlags.Static | BindingFlags.NonPublic);
			return null;
		}
	}
}
