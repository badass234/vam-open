using System.Collections.Generic;
using System.Text;

namespace Mono.CSharp
{
	public class CompilerSettings
	{
		public Target Target;

		public Platform Platform;

		public string TargetExt;

		public bool VerifyClsCompliance;

		public bool Optimize;

		public LanguageVersion Version;

		public bool EnhancedWarnings;

		public bool LoadDefaultReferences;

		public string SdkVersion;

		public string StrongNameKeyFile;

		public string StrongNameKeyContainer;

		public bool StrongNameDelaySign;

		public int TabSize;

		public bool WarningsAreErrors;

		public int WarningLevel;

		public List<string> AssemblyReferences;

		public List<Tuple<string, string>> AssemblyReferencesAliases;

		public List<string> Modules;

		public List<string> ReferencesLookupPaths;

		public Encoding Encoding;

		public string DocumentationFile;

		public string MainClass;

		public string OutputFile;

		public bool Checked;

		public bool StatementMode;

		public bool Unsafe;

		public string Win32ResourceFile;

		public string Win32IconFile;

		public List<AssemblyResource> Resources;

		public bool GenerateDebugInfo;

		public bool ParseOnly;

		public bool TokenizeOnly;

		public bool Timestamps;

		public int DebugFlags;

		public int VerboseParserFlag;

		public int FatalCounter;

		public bool Stacktrace;

		public bool BreakOnInternalError;

		public List<string> GetResourceStrings;

		public bool ShowFullPaths;

		public bool StdLib;

		public RuntimeVersion StdLibRuntimeVersion;

		public string RuntimeMetadataVersion;

		public bool WriteMetadataOnly;

		private readonly List<string> conditional_symbols;

		private readonly List<SourceFile> source_files;

		private List<int> warnings_as_error;

		private List<int> warnings_only;

		private HashSet<int> warning_ignore_table;

		public SourceFile FirstSourceFile
		{
			get
			{
				if (source_files.Count <= 0)
				{
					return null;
				}
				return source_files[0];
			}
		}

		public bool HasKeyFileOrContainer
		{
			get
			{
				if (StrongNameKeyFile == null)
				{
					return StrongNameKeyContainer != null;
				}
				return true;
			}
		}

		public bool NeedsEntryPoint
		{
			get
			{
				if (Target != Target.Exe)
				{
					return Target == Target.WinExe;
				}
				return true;
			}
		}

		public List<SourceFile> SourceFiles => source_files;

		public CompilerSettings()
		{
			StdLib = true;
			Target = Target.Exe;
			TargetExt = ".exe";
			Platform = Platform.AnyCPU;
			Version = LanguageVersion.V_6;
			VerifyClsCompliance = true;
			Encoding = Encoding.UTF8;
			LoadDefaultReferences = true;
			StdLibRuntimeVersion = RuntimeVersion.v4;
			WarningLevel = 4;
			TabSize = 1;
			AssemblyReferences = new List<string>();
			AssemblyReferencesAliases = new List<Tuple<string, string>>();
			Modules = new List<string>();
			ReferencesLookupPaths = new List<string>();
			conditional_symbols = new List<string>();
			conditional_symbols.Add("__MonoCS__");
			source_files = new List<SourceFile>();
		}

		public void AddConditionalSymbol(string symbol)
		{
			if (!conditional_symbols.Contains(symbol))
			{
				conditional_symbols.Add(symbol);
			}
		}

		public void AddWarningAsError(int id)
		{
			if (warnings_as_error == null)
			{
				warnings_as_error = new List<int>();
			}
			warnings_as_error.Add(id);
		}

		public void AddWarningOnly(int id)
		{
			if (warnings_only == null)
			{
				warnings_only = new List<int>();
			}
			warnings_only.Add(id);
		}

		public bool IsConditionalSymbolDefined(string symbol)
		{
			return conditional_symbols.Contains(symbol);
		}

		public bool IsWarningAsError(int code)
		{
			bool flag = WarningsAreErrors;
			if (warnings_as_error != null)
			{
				flag |= warnings_as_error.Contains(code);
			}
			if (warnings_only != null && warnings_only.Contains(code))
			{
				flag = false;
			}
			return flag;
		}

		public bool IsWarningEnabled(int code, int level)
		{
			if (WarningLevel < level)
			{
				return false;
			}
			return !IsWarningDisabledGlobally(code);
		}

		public bool IsWarningDisabledGlobally(int code)
		{
			if (warning_ignore_table != null)
			{
				return warning_ignore_table.Contains(code);
			}
			return false;
		}

		public void SetIgnoreWarning(int code)
		{
			if (warning_ignore_table == null)
			{
				warning_ignore_table = new HashSet<int>();
			}
			warning_ignore_table.Add(code);
		}
	}
}
