using System.Collections.Generic;
using System.IO;

namespace Mono.CSharp
{
	public class CompilerContext
	{
		private static readonly TimeReporter DisabledTimeReporter = new TimeReporter(false);

		private readonly Report report;

		private readonly BuiltinTypes builtin_types;

		private readonly CompilerSettings settings;

		private Dictionary<string, SourceFile> all_source_files;

		public BuiltinTypes BuiltinTypes => builtin_types;

		public bool IsRuntimeBinder { get; set; }

		public Report Report => report;

		public CompilerSettings Settings => settings;

		public List<SourceFile> SourceFiles => settings.SourceFiles;

		public TimeReporter TimeReporter { get; set; }

		public CompilerContext(CompilerSettings settings, ReportPrinter reportPrinter)
		{
			this.settings = settings;
			report = new Report(this, reportPrinter);
			builtin_types = new BuiltinTypes();
			TimeReporter = DisabledTimeReporter;
		}

		public SourceFile LookupFile(CompilationSourceFile comp_unit, string name)
		{
			if (all_source_files == null)
			{
				all_source_files = new Dictionary<string, SourceFile>();
				foreach (SourceFile sourceFile2 in SourceFiles)
				{
					all_source_files[sourceFile2.FullPathName] = sourceFile2;
				}
			}
			string text;
			if (!Path.IsPathRooted(name))
			{
				SourceFile sourceFile = comp_unit.SourceFile;
				text = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFile.FullPathName), name));
				string directoryName = Path.GetDirectoryName(sourceFile.Name);
				if (!string.IsNullOrEmpty(directoryName))
				{
					name = Path.Combine(directoryName, name);
				}
			}
			else
			{
				text = name;
			}
			SourceFile value;
			if (all_source_files.TryGetValue(text, out value))
			{
				return value;
			}
			value = new SourceFile(name, text, all_source_files.Count + 1);
			Location.AddFile(value);
			all_source_files.Add(text, value);
			return value;
		}
	}
}
