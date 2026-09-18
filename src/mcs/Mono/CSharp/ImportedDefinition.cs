using System;
using System.Collections.Generic;
using System.Reflection;

namespace Mono.CSharp
{
	internal abstract class ImportedDefinition : IMemberDefinition
	{
		protected class AttributesBag
		{
			public static readonly AttributesBag Default = new AttributesBag();

			public AttributeUsageAttribute AttributeUsage;

			public ObsoleteAttribute Obsolete;

			public string[] Conditionals;

			public string DefaultIndexerName;

			public bool? CLSAttributeValue;

			public TypeSpec CoClass;

			private static bool HasMissingType(ConstructorInfo ctor)
			{
				return false;
			}

			public static AttributesBag Read(MemberInfo mi, MetadataImporter importer)
			{
				AttributesBag attributesBag = null;
				List<string> list = null;
				foreach (CustomAttributeData customAttribute in CustomAttributeData.GetCustomAttributes(mi))
				{
					Type declaringType = customAttribute.Constructor.DeclaringType;
					string name = declaringType.Name;
					switch (name)
					{
					case "ObsoleteAttribute":
						if (!(declaringType.Namespace != "System"))
						{
							if (attributesBag == null)
							{
								attributesBag = new AttributesBag();
							}
							IList<CustomAttributeTypedArgument> constructorArguments = customAttribute.ConstructorArguments;
							if (constructorArguments.Count == 1)
							{
								attributesBag.Obsolete = new ObsoleteAttribute((string)constructorArguments[0].Value);
							}
							else if (constructorArguments.Count == 2)
							{
								attributesBag.Obsolete = new ObsoleteAttribute((string)constructorArguments[0].Value, (bool)constructorArguments[1].Value);
							}
							else
							{
								attributesBag.Obsolete = new ObsoleteAttribute();
							}
						}
						continue;
					case "ConditionalAttribute":
						if (!(declaringType.Namespace != "System.Diagnostics"))
						{
							if (attributesBag == null)
							{
								attributesBag = new AttributesBag();
							}
							if (list == null)
							{
								list = new List<string>(2);
							}
							list.Add((string)customAttribute.ConstructorArguments[0].Value);
						}
						continue;
					case "CLSCompliantAttribute":
						if (!(declaringType.Namespace != "System"))
						{
							if (attributesBag == null)
							{
								attributesBag = new AttributesBag();
							}
							attributesBag.CLSAttributeValue = (bool)customAttribute.ConstructorArguments[0].Value;
						}
						continue;
					}
					if (mi.MemberType != MemberTypes.TypeInfo && mi.MemberType != MemberTypes.NestedType)
					{
						continue;
					}
					switch (name)
					{
					case "DefaultMemberAttribute":
						if (!(declaringType.Namespace != "System.Reflection"))
						{
							if (attributesBag == null)
							{
								attributesBag = new AttributesBag();
							}
							attributesBag.DefaultIndexerName = (string)customAttribute.ConstructorArguments[0].Value;
						}
						break;
					case "AttributeUsageAttribute":
						if (declaringType.Namespace != "System" || HasMissingType(customAttribute.Constructor))
						{
							break;
						}
						if (attributesBag == null)
						{
							attributesBag = new AttributesBag();
						}
						attributesBag.AttributeUsage = new AttributeUsageAttribute((AttributeTargets)customAttribute.ConstructorArguments[0].Value);
						foreach (CustomAttributeNamedArgument namedArgument in customAttribute.NamedArguments)
						{
							if (namedArgument.MemberInfo.Name == "AllowMultiple")
							{
								attributesBag.AttributeUsage.AllowMultiple = (bool)namedArgument.TypedValue.Value;
							}
							else if (namedArgument.MemberInfo.Name == "Inherited")
							{
								attributesBag.AttributeUsage.Inherited = (bool)namedArgument.TypedValue.Value;
							}
						}
						break;
					case "CoClassAttribute":
						if (!(declaringType.Namespace != "System.Runtime.InteropServices") && !HasMissingType(customAttribute.Constructor))
						{
							if (attributesBag == null)
							{
								attributesBag = new AttributesBag();
							}
							attributesBag.CoClass = importer.ImportType((Type)customAttribute.ConstructorArguments[0].Value);
						}
						break;
					}
				}
				if (attributesBag == null)
				{
					return Default;
				}
				if (list != null)
				{
					attributesBag.Conditionals = list.ToArray();
				}
				return attributesBag;
			}
		}

		protected readonly MemberInfo provider;

		protected AttributesBag cattrs;

		protected readonly MetadataImporter importer;

		public bool IsImported => true;

		public virtual string Name => provider.Name;

		public bool? CLSAttributeValue
		{
			get
			{
				if (cattrs == null)
				{
					ReadAttributes();
				}
				return cattrs.CLSAttributeValue;
			}
		}

		protected ImportedDefinition(MemberInfo provider, MetadataImporter importer)
		{
			this.provider = provider;
			this.importer = importer;
		}

		public string[] ConditionalConditions()
		{
			if (cattrs == null)
			{
				ReadAttributes();
			}
			return cattrs.Conditionals;
		}

		public ObsoleteAttribute GetAttributeObsolete()
		{
			if (cattrs == null)
			{
				ReadAttributes();
			}
			return cattrs.Obsolete;
		}

		protected void ReadAttributes()
		{
			cattrs = AttributesBag.Read(provider, importer);
		}

		public void SetIsAssigned()
		{
		}

		public void SetIsUsed()
		{
		}
	}
}
