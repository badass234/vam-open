using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Mono.CSharp;

namespace DynamicCSharp.Compiler
{
	internal sealed class McsCompiler : ICodeCompiler
	{
		private static string outputDirectory = string.Empty;

		private static bool generateSymbols;

		private static long assemblyCounter;

		internal static string OutputDirectory
		{
			get
			{
				return outputDirectory;
			}
			set
			{
				if (value != string.Empty && !Directory.Exists(value))
				{
					throw new IOException("The specified directory path does not exist. Make sure the specified directory path exists before setting this property");
				}
				outputDirectory = value;
			}
		}

		internal static bool GenerateSymbols
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

		public CompilerResults CompileAssemblyFromDom(CompilerParameters options, CodeCompileUnit compilationUnit)
		{
			return CompileAssemblyFromDomBatch(options, new CodeCompileUnit[1] { compilationUnit });
		}

		public CompilerResults CompileAssemblyFromDomBatch(CompilerParameters options, CodeCompileUnit[] compilationUnits)
		{
			if (options == null)
			{
				throw new ArgumentNullException("options");
			}
			try
			{
				return CompileFromDomBatch(options, compilationUnits);
			}
			finally
			{
				options.TempFiles.Delete();
			}
		}

		public CompilerResults CompileAssemblyFromFile(CompilerParameters options, string fileName)
		{
			return CompileAssemblyFromFileBatch(options, new string[1] { fileName });
		}

		public CompilerResults CompileAssemblyFromFileBatch(CompilerParameters options, string[] fileNames)
		{
			if (options == null)
			{
				throw new ArgumentNullException("options");
			}
			CompilerSettings settings = GetSettings(options);
			foreach (string text in fileNames)
			{
				string fullPath = Path.GetFullPath(text);
				SourceFile item = new SourceFile(text, fullPath, settings.SourceFiles.Count + 1);
				settings.SourceFiles.Add(item);
			}
			return CompileFromSettings(settings, options.GenerateInMemory);
		}

		public CompilerResults CompileAssemblyFromSource(CompilerParameters options, string source)
		{
			return CompileAssemblyFromSourceBatch(options, new string[1] { source });
		}

		public CompilerResults CompileAssemblyFromSourceBatch(CompilerParameters options, string[] sources)
		{
			if (options == null)
			{
				throw new ArgumentNullException("options");
			}
			CompilerSettings settings = GetSettings(options);
			foreach (string source in sources)
			{
				Func<Stream> streamIfDynamicFile = delegate
				{
					string s = ((!string.IsNullOrEmpty(source)) ? source : string.Empty);
					return new MemoryStream(Encoding.UTF8.GetBytes(s));
				};
				SourceFile item = new SourceFile(string.Empty, string.Empty, settings.SourceFiles.Count + 1, streamIfDynamicFile);
				settings.SourceFiles.Add(item);
			}
			return CompileFromSettings(settings, options.GenerateInMemory);
		}

		private CompilerResults CompileFromDomBatch(CompilerParameters options, CodeCompileUnit[] compilationUnits)
		{
			throw new NotImplementedException("Use compile from source or file!");
		}

		private CompilerResults CompileFromSettings(CompilerSettings settings, bool generateInMemory)
		{
			CompilerResults compilerResults = new CompilerResults(new TempFileCollection(Path.GetTempPath()));
			McsDriver mcsDriver = new McsDriver(new CompilerContext(settings, new McsReporter(compilerResults)));
			AssemblyBuilder assembly = null;
			try
			{
				mcsDriver.Compile(out assembly, AppDomain.CurrentDomain, generateInMemory);
			}
			catch (Exception ex)
			{
				compilerResults.Errors.Add(new CompilerError
				{
					IsWarning = false,
					ErrorText = ex.ToString()
				});
			}
			compilerResults.CompiledAssembly = assembly;
			return compilerResults;
		}

		private void SetTargetEnumField(FieldInfo field, object instance, MCSTarget target)
		{
			try
			{
				field.SetValue(instance, (int)target);
			}
			catch
			{
			}
		}

		private CompilerSettings GetSettings(CompilerParameters parameters)
		{
			CompilerSettings compilerSettings = new CompilerSettings();
			StringEnumerator enumerator = parameters.ReferencedAssemblies.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					string current = enumerator.Current;
					compilerSettings.AssemblyReferences.Add(current);
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
			compilerSettings.Encoding = Encoding.UTF8;
			compilerSettings.GenerateDebugInfo = parameters.IncludeDebugInformation;
			compilerSettings.MainClass = parameters.MainClass;
			compilerSettings.Platform = Platform.AnyCPU;
			compilerSettings.StdLibRuntimeVersion = RuntimeVersion.v4;
			FieldInfo field = typeof(CompilerSettings).GetField("Target");
			if (parameters.GenerateExecutable)
			{
				SetTargetEnumField(field, compilerSettings, MCSTarget.Exe);
				compilerSettings.TargetExt = ".exe";
			}
			else
			{
				SetTargetEnumField(field, compilerSettings, MCSTarget.Library);
				compilerSettings.TargetExt = ".dll";
			}
			if (parameters.GenerateInMemory)
			{
				SetTargetEnumField(field, compilerSettings, MCSTarget.Library);
			}
			if (parameters.OutputAssembly != null && !parameters.OutputAssembly.StartsWith("DynamicAssembly_"))
			{
				parameters.OutputAssembly = (compilerSettings.OutputFile = Path.Combine(outputDirectory, parameters.OutputAssembly + "_" + assemblyCounter + compilerSettings.TargetExt));
			}
			else
			{
				parameters.OutputAssembly = (compilerSettings.OutputFile = Path.Combine(outputDirectory, "DynamicAssembly_" + assemblyCounter + compilerSettings.TargetExt));
			}
			assemblyCounter++;
			compilerSettings.OutputFile = parameters.OutputAssembly;
			compilerSettings.GenerateDebugInfo = generateSymbols;
			compilerSettings.Version = LanguageVersion.V_6;
			compilerSettings.WarningLevel = parameters.WarningLevel;
			compilerSettings.WarningsAreErrors = parameters.TreatWarningsAsErrors;
			compilerSettings.Optimize = false;
			if (parameters.CompilerOptions != null)
			{
				string[] array = parameters.CompilerOptions.Split(' ');
				string[] array2 = array;
				foreach (string text in array2)
				{
					if (text.StartsWith("/define:"))
					{
						compilerSettings.AddConditionalSymbol(text.Remove(0, 8));
					}
				}
			}
			return compilerSettings;
		}
	}
}
