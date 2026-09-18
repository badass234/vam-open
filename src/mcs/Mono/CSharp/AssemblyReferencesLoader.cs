using System.Collections.Generic;
using System.IO;

namespace Mono.CSharp
{
	public abstract class AssemblyReferencesLoader<T> where T : class
	{
		protected readonly CompilerContext compiler;

		protected readonly List<string> paths;

		protected AssemblyReferencesLoader(CompilerContext compiler)
		{
			this.compiler = compiler;
			paths = new List<string>();
			paths.Add(Directory.GetCurrentDirectory());
			paths.AddRange(compiler.Settings.ReferencesLookupPaths);
		}

		public abstract bool HasObjectType(T assembly);

		protected abstract string[] GetDefaultReferences();

		public abstract T LoadAssemblyFile(string fileName, bool isImplicitReference);

		public abstract void LoadReferences(ModuleContainer module);

		protected void Error_FileNotFound(string fileName)
		{
			compiler.Report.Error(6, "Metadata file `{0}' could not be found", fileName);
		}

		protected void Error_FileCorrupted(string fileName)
		{
			compiler.Report.Error(9, "Metadata file `{0}' does not contain valid metadata", fileName);
		}

		protected void Error_AssemblyIsModule(string fileName)
		{
			compiler.Report.Error(1509, "Referenced assembly file `{0}' is a module. Consider using `-addmodule' option to add the module", fileName);
		}

		protected void Error_ModuleIsAssembly(string fileName)
		{
			compiler.Report.Error(1542, "Added module file `{0}' is an assembly. Consider using `-r' option to reference the file", fileName);
		}

		protected void LoadReferencesCore(ModuleContainer module, out T corlib_assembly, out List<Tuple<RootNamespace, T>> loaded)
		{
			compiler.TimeReporter.Start(TimeReporter.TimerType.ReferencesLoading);
			loaded = new List<Tuple<RootNamespace, T>>();
			if (module.Compiler.Settings.StdLib)
			{
				corlib_assembly = LoadAssemblyFile("mscorlib.dll", true);
			}
			else
			{
				corlib_assembly = null;
			}
			foreach (string assemblyReference in module.Compiler.Settings.AssemblyReferences)
			{
				T val = LoadAssemblyFile(assemblyReference, false);
				if (val != null && !EqualityComparer<T>.Default.Equals(val, corlib_assembly))
				{
					Tuple<RootNamespace, T> item = Tuple.Create(module.GlobalRootNamespace, val);
					if (!loaded.Contains(item))
					{
						loaded.Add(item);
					}
				}
			}
			if (corlib_assembly == null)
			{
				for (int i = 0; i < loaded.Count; i++)
				{
					Tuple<RootNamespace, T> tuple = loaded[i];
					if (HasObjectType(tuple.Item2))
					{
						corlib_assembly = tuple.Item2;
						loaded.RemoveAt(i);
						break;
					}
				}
			}
			foreach (Tuple<string, string> assemblyReferencesAlias in module.Compiler.Settings.AssemblyReferencesAliases)
			{
				T val = LoadAssemblyFile(assemblyReferencesAlias.Item2, false);
				if (val != null)
				{
					Tuple<RootNamespace, T> item2 = Tuple.Create(module.CreateRootNamespace(assemblyReferencesAlias.Item1), val);
					if (!loaded.Contains(item2))
					{
						loaded.Add(item2);
					}
				}
			}
			if (compiler.Settings.LoadDefaultReferences)
			{
				string[] defaultReferences = GetDefaultReferences();
				foreach (string fileName in defaultReferences)
				{
					T val = LoadAssemblyFile(fileName, true);
					if (val != null)
					{
						Tuple<RootNamespace, T> item3 = Tuple.Create(module.GlobalRootNamespace, val);
						if (!loaded.Contains(item3))
						{
							loaded.Add(item3);
						}
					}
				}
			}
			compiler.TimeReporter.Stop(TimeReporter.TimerType.ReferencesLoading);
		}
	}
}
