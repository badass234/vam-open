using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;

namespace DynamicCSharp.Compiler
{
	internal sealed class McsMarshal : ICompiler
	{
		private ICodeCompiler compiler;

		private CompilerParameters parameters;

		private string outputDirectory = string.Empty;

		private bool generateSymbols = true;

		private byte[] assemblyData;

		private byte[] symbolsData;

		public string OutputDirectory
		{
			get
			{
				return outputDirectory;
			}
			set
			{
				McsCompiler.OutputDirectory = value;
				outputDirectory = value;
			}
		}

		public bool GenerateSymbols
		{
			get
			{
				return generateSymbols;
			}
			set
			{
				generateSymbols = value;
			}
		}

		public byte[] AssemblyData => assemblyData;

		public byte[] SymbolsData => symbolsData;

		public McsMarshal()
		{
			compiler = new McsCompiler();
			parameters = new CompilerParameters();
			parameters.GenerateExecutable = false;
			parameters.GenerateInMemory = false;
			parameters.IncludeDebugInformation = generateSymbols;
			parameters.TempFiles = new TempFileCollection(Environment.GetEnvironmentVariable("TEMP"), true);
			parameters.TempFiles.KeepFiles = true;
		}

		public void AddReference(string reference)
		{
			parameters.ReferencedAssemblies.Add(reference);
		}

		public void AddReferences(IEnumerable<string> references)
		{
			foreach (string reference in references)
			{
				AddReference(reference);
			}
		}

		public void AddConditionalSymbol(string symbol)
		{
			string compilerOptions = parameters.CompilerOptions;
			compilerOptions = ((compilerOptions != null) ? (compilerOptions + " /define:" + symbol) : ("/define:" + symbol));
			parameters.CompilerOptions = compilerOptions;
		}

		public void SetSuggestedAssemblyNamePrefix(string suggestedNamePrefix)
		{
			parameters.OutputAssembly = suggestedNamePrefix;
		}

		public ScriptCompilerError[] CompileFiles(string[] files)
		{
			return CompileShared(compiler.CompileAssemblyFromFileBatch, parameters, files);
		}

		public ScriptCompilerError[] CompileSource(string[] source)
		{
			return CompileShared(compiler.CompileAssemblyFromSourceBatch, parameters, source);
		}

		private ScriptCompilerError[] CompileShared(Func<CompilerParameters, string[], CompilerResults> compileMethod, CompilerParameters parameters, string[] sourceOrFiles)
		{
			McsCompiler.OutputDirectory = outputDirectory;
			McsCompiler.GenerateSymbols = generateSymbols;
			assemblyData = null;
			symbolsData = null;
			CompilerResults compilerResults = compileMethod(parameters, sourceOrFiles);
			ScriptCompilerError[] array = new ScriptCompilerError[compilerResults.Errors.Count];
			parameters.ReferencedAssemblies.Clear();
			int num = 0;
			foreach (CompilerError error in compilerResults.Errors)
			{
				array[num] = new ScriptCompilerError
				{
					errorCode = error.ErrorNumber,
					errorText = error.ErrorText,
					fileName = error.FileName,
					line = error.Line,
					column = error.Column,
					isWarning = error.IsWarning
				};
				num++;
			}
			if (compilerResults.CompiledAssembly != null)
			{
				string text = compilerResults.CompiledAssembly.GetName().Name + ".dll";
				assemblyData = File.ReadAllBytes(text);
				File.Delete(text);
				if (generateSymbols)
				{
					string path = text + ".mdb";
					if (File.Exists(path))
					{
						symbolsData = File.ReadAllBytes(path);
						File.Delete(path);
					}
				}
			}
			return array;
		}
	}
}
