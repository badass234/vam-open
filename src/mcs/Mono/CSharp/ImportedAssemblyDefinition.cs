using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Mono.CSharp
{
	public class ImportedAssemblyDefinition : IAssemblyDefinition
	{
		private readonly Assembly assembly;

		private readonly AssemblyName aname;

		private bool cls_compliant;

		private List<AssemblyName> internals_visible_to;

		private Dictionary<IAssemblyDefinition, AssemblyName> internals_visible_to_cache;

		public Assembly Assembly => assembly;

		public string FullName => aname.FullName;

		public bool HasStrongName => aname.GetPublicKey().Length != 0;

		public bool IsMissing => false;

		public bool IsCLSCompliant => cls_compliant;

		public string Location => assembly.Location;

		public string Name => aname.Name;

		public ImportedAssemblyDefinition(Assembly assembly)
		{
			this.assembly = assembly;
			aname = assembly.GetName();
		}

		public byte[] GetPublicKeyToken()
		{
			return aname.GetPublicKeyToken();
		}

		public AssemblyName GetAssemblyVisibleToName(IAssemblyDefinition assembly)
		{
			return internals_visible_to_cache[assembly];
		}

		public bool IsFriendAssemblyTo(IAssemblyDefinition assembly)
		{
			if (internals_visible_to == null)
			{
				return false;
			}
			AssemblyName value = null;
			if (internals_visible_to_cache == null)
			{
				internals_visible_to_cache = new Dictionary<IAssemblyDefinition, AssemblyName>();
			}
			else if (internals_visible_to_cache.TryGetValue(assembly, out value))
			{
				return value != null;
			}
			byte[] array = assembly.GetPublicKeyToken();
			if (array != null && array.Length == 0)
			{
				array = null;
			}
			foreach (AssemblyName item in internals_visible_to)
			{
				if (!(item.Name != assembly.Name))
				{
					if (array == null && assembly is AssemblyDefinition)
					{
						value = item;
						break;
					}
					if (ArrayComparer.IsEqual(array, item.GetPublicKeyToken()))
					{
						value = item;
						break;
					}
				}
			}
			internals_visible_to_cache.Add(assembly, value);
			return value != null;
		}

		public void ReadAttributes()
		{
			foreach (CustomAttributeData customAttribute in CustomAttributeData.GetCustomAttributes(assembly))
			{
				Type declaringType = customAttribute.Constructor.DeclaringType;
				string name = declaringType.Name;
				if (name == "CLSCompliantAttribute")
				{
					if (declaringType.Namespace == "System")
					{
						cls_compliant = (bool)customAttribute.ConstructorArguments[0].Value;
					}
				}
				else
				{
					if (!(name == "InternalsVisibleToAttribute") || declaringType.Namespace != MetadataImporter.CompilerServicesNamespace)
					{
						continue;
					}
					string text = customAttribute.ConstructorArguments[0].Value as string;
					if (text != null)
					{
						AssemblyName item;
						try
						{
							item = new AssemblyName(text);
						}
						catch (FileLoadException)
						{
							continue;
						}
						if (internals_visible_to == null)
						{
							internals_visible_to = new List<AssemblyName>();
						}
						internals_visible_to.Add(item);
					}
				}
			}
		}

		public override string ToString()
		{
			return FullName;
		}
	}
}
