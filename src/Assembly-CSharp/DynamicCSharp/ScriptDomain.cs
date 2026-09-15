using System;
using System.IO;
using System.Reflection;
using System.Security;
using DynamicCSharp.Compiler;
using DynamicCSharp.Security;
using UnityEngine;

namespace DynamicCSharp
{
	public class ScriptDomain
	{
		private static ScriptDomain domain;

		private AppDomain sandbox;

		private AssemblyChecker checker;

		private ScriptCompiler compilerService;

		internal static ScriptDomain Active => domain;

		public ScriptCompiler CompilerService => compilerService;

		private ScriptDomain(string name)
		{
			domain = this;
			sandbox = AppDomain.CurrentDomain;
			checker = new AssemblyChecker();
		}

		public ScriptAssembly LoadAssemblyFromResources(string resourcePath)
		{
			TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);
			if (textAsset == null)
			{
				throw new DllNotFoundException($"Failed to load dll from resources path '{resourcePath}'");
			}
			return LoadAssembly(textAsset.bytes);
		}

		public ScriptAssembly LoadAssembly(string fullPath)
		{
			CheckDisposed();
			if (!File.Exists(fullPath))
			{
				throw new DllNotFoundException($"Failed to load dll at '{fullPath}'");
			}
			byte[] data = File.ReadAllBytes(fullPath);
			byte[] symbols = null;
			if (DynamicCSharp.Settings.debugMode && File.Exists(fullPath = ".mdb"))
			{
				symbols = File.ReadAllBytes(fullPath + ".mdb");
			}
			return LoadAssembly(data, symbols);
		}

		public ScriptAssembly LoadAssembly(AssemblyName name)
		{
			CheckDisposed();
			Assembly rawAssembly = sandbox.Load(name);
			return new ScriptAssembly(this, rawAssembly);
		}

		public ScriptAssembly LoadAssembly(byte[] data, byte[] symbols = null)
		{
			CheckDisposed();
			if (DynamicCSharp.Settings.securityCheckCode)
			{
				SecurityCheckAssembly(data, true);
			}
			Assembly assembly = null;
			assembly = ((symbols != null && DynamicCSharp.Settings.debugMode) ? sandbox.Load(data, symbols) : sandbox.Load(data));
			if (assembly == null)
			{
				return null;
			}
			return new ScriptAssembly(this, assembly);
		}

		public bool TryLoadAssembly(string fullPath, out ScriptAssembly result)
		{
			try
			{
				result = LoadAssembly(fullPath);
				return true;
			}
			catch (Exception)
			{
				result = null;
				return false;
			}
		}

		public bool TryLoadAssembly(AssemblyName name, out ScriptAssembly result)
		{
			try
			{
				result = LoadAssembly(name);
				return true;
			}
			catch (Exception)
			{
				result = null;
				return false;
			}
		}

		public bool TryLoadAssembly(byte[] data, out ScriptAssembly result)
		{
			try
			{
				result = LoadAssembly(data);
				return true;
			}
			catch (Exception)
			{
				result = null;
				return false;
			}
		}

		public ScriptType CompileAndLoadScriptFile(string file)
		{
			CheckCompiler();
			string[] sourceFiles = new string[1] { file };
			if (!compilerService.CompileFiles(sourceFiles, DynamicCSharp.Settings.assemblyReferences))
			{
				compilerService.PrintErrors();
				return null;
			}
			if (compilerService.HasWarnings)
			{
				compilerService.PrintWarnings();
			}
			return LoadAssembly(compilerService.AssemblyData, compilerService.SymbolsData)?.MainType;
		}

		public ScriptAssembly CompileAndLoadScriptFiles(params string[] files)
		{
			CheckCompiler();
			if (!compilerService.CompileFiles(files, DynamicCSharp.Settings.assemblyReferences))
			{
				compilerService.PrintErrors();
				return null;
			}
			if (compilerService.HasWarnings)
			{
				compilerService.PrintWarnings();
			}
			return LoadAssembly(compilerService.AssemblyData, compilerService.SymbolsData);
		}

		public ScriptType CompileAndLoadScriptSource(string source)
		{
			CheckCompiler();
			string[] sourceContent = new string[1] { source };
			if (!compilerService.CompileSources(sourceContent, DynamicCSharp.Settings.assemblyReferences))
			{
				compilerService.PrintErrors();
				return null;
			}
			if (compilerService.HasWarnings)
			{
				compilerService.PrintWarnings();
			}
			return LoadAssembly(compilerService.AssemblyData, compilerService.SymbolsData)?.MainType;
		}

		public ScriptAssembly CompileAndLoadScriptSources(params string[] sources)
		{
			CheckCompiler();
			if (!compilerService.CompileSources(sources, DynamicCSharp.Settings.assemblyReferences))
			{
				compilerService.PrintErrors();
				return null;
			}
			if (compilerService.HasWarnings)
			{
				compilerService.PrintWarnings();
			}
			return LoadAssembly(compilerService.AssemblyData, compilerService.SymbolsData);
		}

		public AsyncCompileLoadOperation CompileAndLoadScriptFilesAsync(params string[] files)
		{
			CheckCompiler();
			string[] references = DynamicCSharp.Settings.assemblyReferences;
			return new AsyncCompileLoadOperation(this, delegate
			{
				bool result = compilerService.CompileFiles(files, references);
				if (compilerService.HasErrors)
				{
					compilerService.PrintErrors();
				}
				if (compilerService.HasWarnings)
				{
					compilerService.PrintWarnings();
				}
				return result;
			});
		}

		public AsyncCompileLoadOperation CompileAndLoadScriptSourcesAsync(params string[] sources)
		{
			CheckCompiler();
			string[] references = DynamicCSharp.Settings.assemblyReferences;
			return new AsyncCompileLoadOperation(this, delegate
			{
				bool result = compilerService.CompileSources(sources, references);
				if (compilerService.HasErrors)
				{
					compilerService.PrintErrors();
				}
				if (compilerService.HasWarnings)
				{
					compilerService.PrintWarnings();
				}
				return result;
			});
		}

		public bool SecurityCheckAssembly(string fullpath, bool throwOnError = false)
		{
			try
			{
				byte[] assemblyData = new byte[0];
				using (BinaryReader binaryReader = new BinaryReader(File.Open(fullpath, FileMode.Open)))
				{
					assemblyData = binaryReader.ReadBytes((int)binaryReader.BaseStream.Length);
				}
				checker.SecurityCheckAssembly(assemblyData);
				if (checker.HasErrors)
				{
					throw new SecurityException(checker.Errors[0].ToString());
				}
			}
			catch (Exception)
			{
				if (throwOnError)
				{
					throw;
				}
				return false;
			}
			return true;
		}

		public bool SecurityCheckAssembly(AssemblyName name, bool throwOnError = false)
		{
			try
			{
				byte[] assemblyData = new byte[0];
				using (BinaryReader binaryReader = new BinaryReader(File.Open(name.CodeBase, FileMode.Open)))
				{
					assemblyData = binaryReader.ReadBytes((int)binaryReader.BaseStream.Length);
				}
				checker.SecurityCheckAssembly(assemblyData);
				if (checker.HasErrors)
				{
					throw new SecurityException(checker.Errors[0].ToString());
				}
			}
			catch (Exception)
			{
				if (throwOnError)
				{
					throw;
				}
				return false;
			}
			return true;
		}

		public bool SecurityCheckAssembly(byte[] assemblyData, bool throwOnError = false)
		{
			try
			{
				if (assemblyData == null)
				{
					throw new ArgumentNullException("assemblyData");
				}
				checker.SecurityCheckAssembly(assemblyData);
				if (checker.HasErrors)
				{
					throw new SecurityException(checker.Errors[0].ToString());
				}
			}
			catch (Exception)
			{
				if (throwOnError)
				{
					throw;
				}
				return false;
			}
			return true;
		}

		internal void CreateCompilerService()
		{
			compilerService = new ScriptCompiler();
		}

		private void CheckDisposed()
		{
			if (sandbox == null)
			{
				throw new ObjectDisposedException("The 'ScriptDomain' has already been disposed");
			}
		}

		private void CheckCompiler()
		{
			if (compilerService == null)
			{
				throw new Exception("The compiler service has not been initialized");
			}
		}

		public static ScriptDomain CreateDomain(string domainName, bool initCompiler)
		{
			ScriptDomain scriptDomain = new ScriptDomain("DynamicCSharp");
			if (initCompiler)
			{
				scriptDomain.CreateCompilerService();
			}
			return scriptDomain;
		}
	}
}
