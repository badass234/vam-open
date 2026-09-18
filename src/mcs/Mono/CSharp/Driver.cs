using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection.Emit;
using System.Security.Cryptography;

namespace Mono.CSharp
{
	public class Driver
	{
		private readonly CompilerContext ctx;

		public Report Report => ctx.Report;

		public Driver(CompilerContext ctx)
		{
			this.ctx = ctx;
		}

		private void tokenize_file(SourceFile sourceFile, ModuleContainer module, ParserSession session)
		{
			Stream dataStream;
			try
			{
				dataStream = sourceFile.GetDataStream();
			}
			catch
			{
				Report.Error(2001, "Source file `" + sourceFile.Name + "' could not be found");
				return;
			}
			using (dataStream)
			{
				SeekableStreamReader input = new SeekableStreamReader(dataStream, ctx.Settings.Encoding);
				CompilationSourceFile file = new CompilationSourceFile(module, sourceFile);
				Tokenizer tokenizer = new Tokenizer(input, file, session, ctx.Report);
				int num = 0;
				int num2 = 0;
				int num3;
				while ((num3 = tokenizer.token()) != 257)
				{
					num++;
					if (num3 == 259)
					{
						num2++;
					}
				}
				Console.WriteLine("Tokenized: " + num + " found " + num2 + " errors");
			}
		}

		public void Parse(ModuleContainer module)
		{
			bool tokenizeOnly = module.Compiler.Settings.TokenizeOnly;
			List<SourceFile> sourceFiles = module.Compiler.SourceFiles;
			Location.Initialize(sourceFiles);
			ParserSession session = new ParserSession
			{
				UseJayGlobalArrays = true,
				LocatedTokens = new LocatedToken[15000]
			};
			for (int i = 0; i < sourceFiles.Count; i++)
			{
				if (tokenizeOnly)
				{
					tokenize_file(sourceFiles[i], module, session);
				}
				else
				{
					Parse(sourceFiles[i], module, session, Report);
				}
			}
		}

		public void Parse(SourceFile file, ModuleContainer module, ParserSession session, Report report)
		{
			Stream dataStream;
			try
			{
				dataStream = file.GetDataStream();
			}
			catch
			{
				report.Error(2001, "Source file `{0}' could not be found", file.Name);
				return;
			}
			if (dataStream.ReadByte() == 77 && dataStream.ReadByte() == 90)
			{
				report.Error(2015, "Source file `{0}' is a binary file and not a text file", file.Name);
				dataStream.Close();
				return;
			}
			dataStream.Position = 0L;
			SeekableStreamReader seekableStreamReader = new SeekableStreamReader(dataStream, ctx.Settings.Encoding, session.StreamReaderBuffer);
			Parse(seekableStreamReader, file, module, session, report);
			if (ctx.Settings.GenerateDebugInfo && report.Errors == 0 && !file.HasChecksum)
			{
				dataStream.Position = 0L;
				MD5 checksumAlgorithm = session.GetChecksumAlgorithm();
				file.SetChecksum(checksumAlgorithm.ComputeHash(dataStream));
			}
			seekableStreamReader.Dispose();
			dataStream.Close();
		}

		public static void Parse(SeekableStreamReader reader, SourceFile sourceFile, ModuleContainer module, ParserSession session, Report report)
		{
			CompilationSourceFile compilationSourceFile = new CompilationSourceFile(module, sourceFile);
			module.AddTypeContainer(compilationSourceFile);
			new CSharpParser(reader, compilationSourceFile, report, session).parse();
		}

		public static int Main(string[] args)
		{
			Location.InEmacs = Environment.GetEnvironmentVariable("EMACS") == "t";
			CommandLineParser commandLineParser = new CommandLineParser(Console.Out);
			CompilerSettings compilerSettings = commandLineParser.ParseArguments(args);
			if (compilerSettings == null)
			{
				return 1;
			}
			if (commandLineParser.HasBeenStopped)
			{
				return 0;
			}
			Driver driver = new Driver(new CompilerContext(compilerSettings, new ConsoleReportPrinter()));
			if (driver.Compile() && driver.Report.Errors == 0)
			{
				if (driver.Report.Warnings > 0)
				{
					Console.WriteLine("Compilation succeeded - {0} warning(s)", driver.Report.Warnings);
				}
				Environment.Exit(0);
				return 0;
			}
			Console.WriteLine("Compilation failed: {0} error(s), {1} warnings", driver.Report.Errors, driver.Report.Warnings);
			Environment.Exit(1);
			return 1;
		}

		public static string GetPackageFlags(string packages, Report report)
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.FileName = "pkg-config";
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.UseShellExecute = false;
			processStartInfo.Arguments = "--libs " + packages;
			Process process = null;
			try
			{
				process = Process.Start(processStartInfo);
			}
			catch (Exception ex)
			{
				if (report == null)
				{
					throw;
				}
				report.Error(-27, "Couldn't run pkg-config: " + ex.Message);
				return null;
			}
			if (process.StandardOutput == null)
			{
				if (report == null)
				{
					throw new ApplicationException("Specified package did not return any information");
				}
				report.Warning(-27, 1, "Specified package did not return any information");
				process.Close();
				return null;
			}
			string text = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			if (process.ExitCode != 0)
			{
				if (report == null)
				{
					throw new ApplicationException(text);
				}
				report.Error(-27, "Error running pkg-config. Check the above output.");
				process.Close();
				return null;
			}
			process.Close();
			return text;
		}

		public bool Compile()
		{
			CompilerSettings settings = ctx.Settings;
			if (settings.FirstSourceFile == null && (settings.Target == Target.Exe || settings.Target == Target.WinExe || settings.Target == Target.Module || settings.Resources == null))
			{
				Report.Error(2008, "No files to compile were specified");
				return false;
			}
			if (settings.Platform == Platform.AnyCPU32Preferred && (settings.Target == Target.Library || settings.Target == Target.Module))
			{
				Report.Error(4023, "Platform option `anycpu32bitpreferred' is valid only for executables");
				return false;
			}
			TimeReporter timeReporter = new TimeReporter(settings.Timestamps);
			ctx.TimeReporter = timeReporter;
			timeReporter.StartTotal();
			ModuleContainer moduleContainer = (RootContext.ToplevelTypes = new ModuleContainer(ctx));
			timeReporter.Start(TimeReporter.TimerType.ParseTotal);
			Parse(moduleContainer);
			timeReporter.Stop(TimeReporter.TimerType.ParseTotal);
			if (Report.Errors > 0)
			{
				return false;
			}
			if (settings.TokenizeOnly || settings.ParseOnly)
			{
				timeReporter.StopTotal();
				timeReporter.ShowStats();
				return true;
			}
			string text = settings.OutputFile;
			string text2;
			if (text == null)
			{
				SourceFile firstSourceFile = settings.FirstSourceFile;
				if (firstSourceFile == null)
				{
					Report.Error(1562, "If no source files are specified you must specify the output file with -out:");
					return false;
				}
				text2 = firstSourceFile.Name;
				int num = text2.LastIndexOf('.');
				if (num > 0)
				{
					text2 = text2.Substring(0, num);
				}
				text2 += settings.TargetExt;
				text = text2;
			}
			else
			{
				text2 = Path.GetFileName(text);
				if (string.IsNullOrEmpty(Path.GetFileNameWithoutExtension(text2)) || text2.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
				{
					Report.Error(2021, "Output file name is not valid");
					return false;
				}
			}
			AssemblyDefinitionDynamic assemblyDefinitionDynamic = new AssemblyDefinitionDynamic(moduleContainer, text2, text);
			moduleContainer.SetDeclaringAssembly(assemblyDefinitionDynamic);
			ReflectionImporter importer = (ReflectionImporter)(assemblyDefinitionDynamic.Importer = new ReflectionImporter(moduleContainer, ctx.BuiltinTypes));
			DynamicLoader dynamicLoader = new DynamicLoader(importer, ctx);
			dynamicLoader.LoadReferences(moduleContainer);
			if (!ctx.BuiltinTypes.CheckDefinitions(moduleContainer))
			{
				return false;
			}
			if (!assemblyDefinitionDynamic.Create(AppDomain.CurrentDomain, AssemblyBuilderAccess.Save))
			{
				return false;
			}
			moduleContainer.CreateContainer();
			dynamicLoader.LoadModules(assemblyDefinitionDynamic, moduleContainer.GlobalRootNamespace);
			moduleContainer.InitializePredefinedTypes();
			if (settings.GetResourceStrings != null)
			{
				moduleContainer.LoadGetResourceStrings(settings.GetResourceStrings);
			}
			timeReporter.Start(TimeReporter.TimerType.ModuleDefinitionTotal);
			moduleContainer.Define();
			timeReporter.Stop(TimeReporter.TimerType.ModuleDefinitionTotal);
			if (Report.Errors > 0)
			{
				return false;
			}
			if (settings.DocumentationFile != null)
			{
				new DocumentationBuilder(moduleContainer).OutputDocComment(text, settings.DocumentationFile);
			}
			assemblyDefinitionDynamic.Resolve();
			if (Report.Errors > 0)
			{
				return false;
			}
			timeReporter.Start(TimeReporter.TimerType.EmitTotal);
			assemblyDefinitionDynamic.Emit();
			timeReporter.Stop(TimeReporter.TimerType.EmitTotal);
			if (Report.Errors > 0)
			{
				return false;
			}
			timeReporter.Start(TimeReporter.TimerType.CloseTypes);
			moduleContainer.CloseContainer();
			timeReporter.Stop(TimeReporter.TimerType.CloseTypes);
			timeReporter.Start(TimeReporter.TimerType.Resouces);
			if (!settings.WriteMetadataOnly)
			{
				assemblyDefinitionDynamic.EmbedResources();
			}
			timeReporter.Stop(TimeReporter.TimerType.Resouces);
			if (Report.Errors > 0)
			{
				return false;
			}
			assemblyDefinitionDynamic.Save();
			timeReporter.StopTotal();
			timeReporter.ShowStats();
			return Report.Errors == 0;
		}
	}
}
