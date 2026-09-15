using System;
using UnityEngine;

namespace DynamicCSharp.Compiler
{
	public sealed class ScriptCompiler
	{
		private const string compilerModule = "DynamicCSharp.Compiler.McsMarshal";

		private static readonly object compilerLock = new object();

		private ICompiler compiler;

		private string[] warnings = new string[0];

		private string[] errors = new string[0];

		private byte[] assemblyData;

		private byte[] symbolsData;

		private volatile bool isCompiling;

		public static Type CompilerType => typeof(ScriptCompiler).Assembly.GetType("DynamicCSharp.Compiler.McsMarshal");

		public string[] Warnings => warnings;

		public bool HasWarnings => warnings.Length > 0;

		public string[] Errors => errors;

		public bool HasErrors => errors.Length > 0;

		public byte[] AssemblyData => assemblyData;

		public byte[] SymbolsData => symbolsData;

		public bool IsCompiling => isCompiling;

		public ScriptCompiler()
		{
			Type compilerType = CompilerType;
			if (compilerType == null)
			{
				throw new ApplicationException("Failed to load the compiler service. Make sure you have installed the compiler package for runtime script compilation. See documentation for help");
			}
			compiler = (ICompiler)Activator.CreateInstance(compilerType);
			if (compiler != null)
			{
				compiler.OutputDirectory = DynamicCSharp.Settings.compilerWorkingDirectory;
				compiler.GenerateSymbols = DynamicCSharp.Settings.debugMode;
			}
		}

		public void PrintWarnings()
		{
			string[] array = warnings;
			foreach (string message in array)
			{
				Debug.LogWarning(message);
			}
		}

		public void PrintErrors()
		{
			string[] array = errors;
			foreach (string message in array)
			{
				Debug.LogError(message);
			}
		}

		public void AddConditionalSymbol(string symbol)
		{
			lock (compilerLock)
			{
				compiler.AddConditionalSymbol(symbol);
			}
		}

		public void SetSuggestedAssemblyNamePrefix(string assemblyNamePrefix)
		{
			lock (compilerLock)
			{
				compiler.SetSuggestedAssemblyNamePrefix(assemblyNamePrefix);
			}
		}

		public bool CompileFiles(string[] sourceFiles, params string[] extraReferences)
		{
			isCompiling = true;
			ResetCompiler();
			ScriptCompilerError[] array = null;
			lock (compilerLock)
			{
				compiler.AddReferences(extraReferences);
				array = compiler.CompileFiles(sourceFiles);
				assemblyData = compiler.AssemblyData;
				symbolsData = compiler.SymbolsData;
			}
			bool flag = true;
			ScriptCompilerError[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				ScriptCompilerError scriptCompilerError = array2[i];
				if (scriptCompilerError.isWarning)
				{
					AddWarning(scriptCompilerError.errorCode, scriptCompilerError.errorText, scriptCompilerError.fileName, scriptCompilerError.line, scriptCompilerError.column);
					continue;
				}
				flag = false;
				AddError(scriptCompilerError.errorCode, scriptCompilerError.errorText, scriptCompilerError.fileName, scriptCompilerError.line, scriptCompilerError.column);
			}
			if (!flag)
			{
				assemblyData = null;
				symbolsData = null;
			}
			isCompiling = false;
			return flag;
		}

		public bool CompileSources(string[] sourceContent, params string[] extraReferences)
		{
			isCompiling = true;
			ResetCompiler();
			ScriptCompilerError[] array = null;
			lock (compilerLock)
			{
				compiler.AddReferences(extraReferences);
				array = compiler.CompileSource(sourceContent);
				assemblyData = compiler.AssemblyData;
				symbolsData = compiler.SymbolsData;
			}
			bool flag = true;
			ScriptCompilerError[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				ScriptCompilerError scriptCompilerError = array2[i];
				if (scriptCompilerError.isWarning)
				{
					AddWarning(scriptCompilerError.errorCode, scriptCompilerError.errorText, scriptCompilerError.fileName, scriptCompilerError.line, scriptCompilerError.column);
					continue;
				}
				flag = false;
				AddError(scriptCompilerError.errorCode, scriptCompilerError.errorText, scriptCompilerError.fileName, scriptCompilerError.line, scriptCompilerError.column);
			}
			if (!flag)
			{
				assemblyData = null;
				symbolsData = null;
			}
			isCompiling = false;
			return flag;
		}

		public AsyncCompileOperation CompileFilesAsync(string[] sourceFiles, params string[] extraReferences)
		{
			return new AsyncCompileOperation(this, () => CompileFiles(sourceFiles, extraReferences));
		}

		public AsyncCompileOperation CompileSourcesAsync(string[] sourceContent, params string[] extraReferences)
		{
			return new AsyncCompileOperation(this, () => CompileSources(sourceContent, extraReferences));
		}

		private void AddWarning(string code, string message, string file, int line, int column)
		{
			string text = $"[CS{code}]: {message} in {file} at [{line}, {column}]";
			if (line == -1 || column == -1)
			{
				text = $"[CS{code}]: {message}";
			}
			Array.Resize(ref warnings, warnings.Length + 1);
			warnings[warnings.Length - 1] = text;
		}

		private void AddError(string code, string message, string file, int line, int column)
		{
			string text = $"[CS{code}]: {message} in {file} at [{line}, {column}]";
			if (line == -1 || column == -1)
			{
				text = $"[CS{code}]: {message}";
			}
			Array.Resize(ref errors, errors.Length + 1);
			errors[errors.Length - 1] = text;
		}

		private void ResetCompiler()
		{
			errors = new string[0];
			warnings = new string[0];
			assemblyData = null;
			symbolsData = null;
		}
	}
}
