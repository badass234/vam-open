using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using System.Security.Cryptography;
using Mono.CSharp;

namespace DynamicCSharp.Compiler
{
	internal sealed class McsDriver
	{
		private readonly CompilerContext context;

		public Report Report => context.Report;

		public McsDriver(CompilerContext context)
		{
			this.context = context;
		}

		public void TokenizeFile(SourceFile source, ModuleContainer module, ParserSession session)
		{
			Stream stream = null;
			try
			{
				stream = source.GetDataStream();
			}
			catch
			{
				Report.Error(2001, "Failed to open file '{0}' for reading", source.Name);
				return;
			}
			using (stream)
			{
				using (SeekableStreamReader input = new SeekableStreamReader(stream, context.Settings.Encoding))
				{
					CompilationSourceFile file = new CompilationSourceFile(module, source);
					Tokenizer tokenizer = new Tokenizer(input, file, session, context.Report);
					int num = 0;
					int num2 = 0;
					int num3 = 0;
					while ((num = tokenizer.token()) != 257)
					{
						num2++;
						if (num == 259)
						{
							num3++;
						}
					}
				}
			}
		}

		public void Parse(ModuleContainer module)
		{
			bool tokenizeOnly = module.Compiler.Settings.TokenizeOnly;
			List<SourceFile> sourceFiles = module.Compiler.SourceFiles;
			Location.Initialize(sourceFiles);
			ParserSession parserSession = new ParserSession();
			parserSession.UseJayGlobalArrays = true;
			parserSession.LocatedTokens = new LocatedToken[15000];
			ParserSession session = parserSession;
			for (int i = 0; i < sourceFiles.Count; i++)
			{
				if (tokenizeOnly)
				{
					TokenizeFile(sourceFiles[i], module, session);
				}
				else
				{
					Parse(sourceFiles[i], module, session, Report);
				}
			}
		}

		public void Parse(SourceFile source, ModuleContainer module, ParserSession session, Report report)
		{
			Stream stream = null;
			try
			{
				stream = source.GetDataStream();
			}
			catch
			{
				Report.Error(2001, "Failed to open file '{0}' for reading", source.Name);
				return;
			}
			using (stream)
			{
				if (stream.ReadByte() == 77 && stream.ReadByte() == 90)
				{
					report.Error(2015, "Failed to open file '{0}' for reading because it is a binary file. A text file was expected", source.Name);
					stream.Close();
					return;
				}
				stream.Position = 0L;
				using (SeekableStreamReader reader = new SeekableStreamReader(stream, context.Settings.Encoding, session.StreamReaderBuffer))
				{
					Parse(reader, source, module, session, report);
					if (context.Settings.GenerateDebugInfo && report.Errors == 0 && !source.HasChecksum)
					{
						stream.Position = 0L;
						MD5 checksumAlgorithm = session.GetChecksumAlgorithm();
						source.SetChecksum(checksumAlgorithm.ComputeHash(stream));
					}
				}
			}
		}

		public bool Compile(out AssemblyBuilder assembly, AppDomain domain, bool generateInMemory)
		{
			CompilerSettings settings = context.Settings;
			assembly = null;
			if (settings.FirstSourceFile == null && (settings.Target == Target.Exe || settings.Target == Target.WinExe || settings.Target == Target.Module || settings.Resources == null))
			{
				Report.Error(2008, "No source files specified");
				return false;
			}
			if (settings.Platform == Platform.AnyCPU32Preferred && (settings.Target == Target.Library || settings.Target == Target.Module))
			{
				Report.Error(4023, "The preferred platform '{0}' is only valid on executable outputs", Platform.AnyCPU32Preferred.ToString());
				return false;
			}
			TimeReporter timeReporter = new TimeReporter(settings.Timestamps);
			context.TimeReporter = timeReporter;
			timeReporter.StartTotal();
			ModuleContainer moduleContainer2 = (RootContext.ToplevelTypes = new ModuleContainer(context));
			timeReporter.Start(TimeReporter.TimerType.ParseTotal);
			Parse(moduleContainer2);
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
			string outputFile = settings.OutputFile;
			string fileName = Path.GetFileName(outputFile);
			AssemblyDefinitionDynamic assemblyDefinitionDynamic = new AssemblyDefinitionDynamic(moduleContainer2, fileName, outputFile);
			moduleContainer2.SetDeclaringAssembly(assemblyDefinitionDynamic);
			ReflectionImporter importer = (ReflectionImporter)(assemblyDefinitionDynamic.Importer = new ReflectionImporter(moduleContainer2, context.BuiltinTypes));
			DynamicLoader dynamicLoader = new DynamicLoader(importer, context);
			dynamicLoader.LoadReferences(moduleContainer2);
			if (!context.BuiltinTypes.CheckDefinitions(moduleContainer2))
			{
				return false;
			}
			if (!assemblyDefinitionDynamic.Create(domain, AssemblyBuilderAccess.RunAndSave))
			{
				return false;
			}
			moduleContainer2.CreateContainer();
			dynamicLoader.LoadModules(assemblyDefinitionDynamic, moduleContainer2.GlobalRootNamespace);
			moduleContainer2.InitializePredefinedTypes();
			if (settings.GetResourceStrings != null)
			{
				moduleContainer2.LoadGetResourceStrings(settings.GetResourceStrings);
			}
			timeReporter.Start(TimeReporter.TimerType.ModuleDefinitionTotal);
			try
			{
				moduleContainer2.Define();
			}
			catch
			{
				return false;
			}
			timeReporter.Stop(TimeReporter.TimerType.ModuleDefinitionTotal);
			if (Report.Errors > 0)
			{
				return false;
			}
			if (settings.DocumentationFile != null)
			{
				DocumentationBuilder documentationBuilder = new DocumentationBuilder(moduleContainer2);
				documentationBuilder.OutputDocComment(outputFile, settings.DocumentationFile);
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
			moduleContainer2.CloseContainer();
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
			if (!generateInMemory)
			{
				assemblyDefinitionDynamic.Save();
			}
			assembly = assemblyDefinitionDynamic.Builder;
			timeReporter.StopTotal();
			timeReporter.ShowStats();
			return Report.Errors == 0;
		}

		public static void Parse(SeekableStreamReader reader, SourceFile source, ModuleContainer module, ParserSession session, Report report)
		{
			CompilationSourceFile compilationSourceFile = new CompilationSourceFile(module, source);
			module.AddTypeContainer(compilationSourceFile);
			CSharpParser cSharpParser = new CSharpParser(reader, compilationSourceFile, report, session);
			cSharpParser.parse();
		}
	}
}
