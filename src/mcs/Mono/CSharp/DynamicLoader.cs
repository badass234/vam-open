using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Mono.CSharp
{
	public class DynamicLoader : AssemblyReferencesLoader<Assembly>
	{
		private readonly ReflectionImporter importer;

		public ReflectionImporter Importer => importer;

		public DynamicLoader(ReflectionImporter importer, CompilerContext compiler)
			: base(compiler)
		{
			paths.Add(GetSystemDir());
			this.importer = importer;
		}

		protected override string[] GetDefaultReferences()
		{
			List<string> list = new List<string>(8);
			list.Add("System");
			list.Add("System.Xml");
			list.Add("System.Net");
			list.Add("System.Windows");
			list.Add("System.Windows.Browser");
			if (compiler.Settings.Version > LanguageVersion.ISO_2)
			{
				list.Add("System.Core");
			}
			if (compiler.Settings.Version > LanguageVersion.V_3)
			{
				list.Add("Microsoft.CSharp");
			}
			return list.ToArray();
		}

		private static string GetSystemDir()
		{
			return Path.GetDirectoryName(typeof(object).Assembly.Location);
		}

		public override bool HasObjectType(Assembly assembly)
		{
			return assembly.GetType(compiler.BuiltinTypes.Object.FullName) != null;
		}

		public override Assembly LoadAssemblyFile(string assembly, bool isImplicitReference)
		{
			Assembly result = null;
			try
			{
				try
				{
					char[] anyOf = new char[2] { '/', '\\' };
					if (assembly.IndexOfAny(anyOf) != -1)
					{
						result = Assembly.LoadFrom(assembly);
					}
					else
					{
						string text = assembly;
						if (text.EndsWith(".dll") || text.EndsWith(".exe"))
						{
							text = assembly.Substring(0, assembly.Length - 4);
						}
						result = Assembly.Load(text);
					}
				}
				catch (FileNotFoundException)
				{
					bool flag = !isImplicitReference;
					foreach (string path in paths)
					{
						string text2 = Path.Combine(path, assembly);
						if (!assembly.EndsWith(".dll") && !assembly.EndsWith(".exe"))
						{
							text2 += ".dll";
						}
						try
						{
							result = Assembly.LoadFrom(text2);
							flag = false;
						}
						catch (FileNotFoundException)
						{
							continue;
						}
						break;
					}
					if (flag)
					{
						Error_FileNotFound(assembly);
						return result;
					}
				}
			}
			catch (BadImageFormatException)
			{
				Error_FileCorrupted(assembly);
			}
			return result;
		}

		private Module LoadModuleFile(AssemblyDefinitionDynamic assembly, string module)
		{
			string text = "";
			try
			{
				try
				{
					return assembly.IncludeModule(module);
				}
				catch (FileNotFoundException)
				{
					bool flag = true;
					foreach (string path in paths)
					{
						string text2 = Path.Combine(path, module);
						if (!module.EndsWith(".netmodule"))
						{
							text2 += ".netmodule";
						}
						try
						{
							return assembly.IncludeModule(text2);
						}
						catch (FileNotFoundException ex2)
						{
							text += ex2.FusionLog;
						}
					}
					if (flag)
					{
						Error_FileNotFound(module);
						return null;
					}
				}
			}
			catch (BadImageFormatException)
			{
				Error_FileCorrupted(module);
			}
			return null;
		}

		public void LoadModules(AssemblyDefinitionDynamic assembly, RootNamespace targetNamespace)
		{
			foreach (string module3 in compiler.Settings.Modules)
			{
				Module module = LoadModuleFile(assembly, module3);
				if (module != null)
				{
					ImportedModuleDefinition module2 = importer.ImportModule(module, targetNamespace);
					assembly.AddModule(module2);
				}
			}
		}

		public override void LoadReferences(ModuleContainer module)
		{
			Assembly corlib_assembly;
			List<Tuple<RootNamespace, Assembly>> loaded;
			LoadReferencesCore(module, out corlib_assembly, out loaded);
			if (corlib_assembly == null)
			{
				return;
			}
			importer.ImportAssembly(corlib_assembly, module.GlobalRootNamespace);
			foreach (Tuple<RootNamespace, Assembly> item in loaded)
			{
				importer.ImportAssembly(item.Item2, item.Item1);
			}
		}
	}
}
