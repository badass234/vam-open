using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace Mono.CSharp
{
	public class CommandLineParser
	{
		private enum ParseResult
		{
			Success = 0,
			Error = 1,
			Stop = 2,
			UnknownOption = 3
		}

		private static readonly char[] argument_value_separator = new char[2] { ';', ',' };

		private static readonly char[] numeric_value_separator = new char[3] { ';', ',', ' ' };

		private readonly TextWriter output;

		private readonly Report report;

		private bool stop_argument;

		private Dictionary<string, int> source_file_index;

		private CompilerSettings parser_settings;

		public bool HasBeenStopped => stop_argument;

		public event Func<string[], int, int> UnknownOptionHandler;

		public CommandLineParser(TextWriter errorOutput)
			: this(errorOutput, Console.Out)
		{
		}

		public CommandLineParser(TextWriter errorOutput, TextWriter messagesOutput)
		{
			StreamReportPrinter streamReportPrinter = new StreamReportPrinter(errorOutput);
			parser_settings = new CompilerSettings();
			report = new Report(new CompilerContext(parser_settings, streamReportPrinter), streamReportPrinter);
			output = messagesOutput;
		}

		private void About()
		{
			output.WriteLine("The Mono C# compiler is Copyright 2001-2011, Novell, Inc.\n\nThe compiler source code is released under the terms of the \nMIT X11 or GNU GPL licenses\n\nFor more information on Mono, visit the project Web site\n   http://www.mono-project.com\n\nThe compiler was written by Miguel de Icaza, Ravi Pratap, Martin Baulig, Marek Safar, Raja R Harinath, Atushi Enomoto");
		}

		public CompilerSettings ParseArguments(string[] args)
		{
			CompilerSettings compilerSettings = new CompilerSettings();
			if (!ParseArguments(compilerSettings, args))
			{
				return null;
			}
			return compilerSettings;
		}

		public bool ParseArguments(CompilerSettings settings, string[] args)
		{
			if (settings == null)
			{
				throw new ArgumentNullException("settings");
			}
			List<string> list = null;
			bool flag = true;
			stop_argument = false;
			source_file_index = new Dictionary<string, int>();
			for (int i = 0; i < args.Length; i++)
			{
				string text = args[i];
				if (text.Length == 0)
				{
					continue;
				}
				if (text[0] == '@')
				{
					string text2 = text.Substring(1);
					if (list == null)
					{
						list = new List<string>();
					}
					if (list.Contains(text2))
					{
						report.Error(1515, "Response file `{0}' specified multiple times", text2);
						return false;
					}
					list.Add(text2);
					string[] array = LoadArgs(text2);
					if (array == null)
					{
						report.Error(2011, "Unable to open response file: " + text2);
						return false;
					}
					args = AddArgs(args, array);
					continue;
				}
				if (flag)
				{
					if (text == "--")
					{
						flag = false;
						continue;
					}
					bool flag2 = text[0] == '-';
					bool flag3 = text[0] == '/';
					if (flag2)
					{
						switch (ParseOptionUnix(text, ref args, ref i, settings))
						{
						case ParseResult.Stop:
							stop_argument = true;
							return true;
						case ParseResult.UnknownOption:
							if (UnknownOptionHandler != null)
							{
								int num = UnknownOptionHandler(args, i);
								if (num != -1)
								{
									i = num;
									continue;
								}
							}
							break;
						case ParseResult.Success:
						case ParseResult.Error:
							continue;
						}
					}
					if (flag2 | flag3)
					{
						string option = (flag2 ? ("/" + text.Substring(1)) : text);
						switch (ParseOption(option, ref args, settings))
						{
						case ParseResult.UnknownOption:
							if (flag3 && text.Length > 3 && text.IndexOf('/', 2) > 0)
							{
								break;
							}
							if (UnknownOptionHandler != null)
							{
								int num2 = UnknownOptionHandler(args, i);
								if (num2 != -1)
								{
									i = num2;
									continue;
								}
							}
							Error_WrongOption(text);
							return false;
						case ParseResult.Stop:
							stop_argument = true;
							return true;
						case ParseResult.Success:
						case ParseResult.Error:
							continue;
						}
					}
				}
				ProcessSourceFiles(text, false, settings.SourceFiles);
			}
			return report.Errors == 0;
		}

		private void ProcessSourceFiles(string spec, bool recurse, List<SourceFile> sourceFiles)
		{
			string path;
			string pattern;
			SplitPathAndPattern(spec, out path, out pattern);
			if (pattern.IndexOf('*') == -1)
			{
				AddSourceFile(spec, sourceFiles);
				return;
			}
			string[] files;
			try
			{
				files = Directory.GetFiles(path, pattern);
			}
			catch (DirectoryNotFoundException)
			{
				report.Error(2001, "Source file `" + spec + "' could not be found");
				return;
			}
			catch (IOException)
			{
				report.Error(2001, "Source file `" + spec + "' could not be found");
				return;
			}
			string[] array = files;
			foreach (string fileName in array)
			{
				AddSourceFile(fileName, sourceFiles);
			}
			if (recurse)
			{
				string[] array2 = null;
				try
				{
					array2 = Directory.GetDirectories(path);
				}
				catch
				{
				}
				array = array2;
				foreach (string text in array)
				{
					ProcessSourceFiles(text + "/" + pattern, true, sourceFiles);
				}
			}
		}

		private static string[] AddArgs(string[] args, string[] extra_args)
		{
			string[] array = new string[extra_args.Length + args.Length];
			int num = Array.IndexOf(args, "--");
			if (num != -1)
			{
				Array.Copy(args, array, num);
				extra_args.CopyTo(array, num);
				Array.Copy(args, num, array, num + extra_args.Length, args.Length - num);
			}
			else
			{
				args.CopyTo(array, 0);
				extra_args.CopyTo(array, args.Length);
			}
			return array;
		}

		private void AddAssemblyReference(string alias, string assembly, CompilerSettings settings)
		{
			if (assembly.Length == 0)
			{
				report.Error(1680, "Invalid reference alias `{0}='. Missing filename", alias);
			}
			else if (!IsExternAliasValid(alias))
			{
				report.Error(1679, "Invalid extern alias for -reference. Alias `{0}' is not a valid identifier", alias);
			}
			else
			{
				settings.AssemblyReferencesAliases.Add(Tuple.Create(alias, assembly));
			}
		}

		private void AddResource(AssemblyResource res, CompilerSettings settings)
		{
			if (settings.Resources == null)
			{
				settings.Resources = new List<AssemblyResource>();
				settings.Resources.Add(res);
			}
			else if (settings.Resources.Contains(res))
			{
				report.Error(1508, "The resource identifier `{0}' has already been used in this assembly", res.Name);
			}
			else
			{
				settings.Resources.Add(res);
			}
		}

		private void AddSourceFile(string fileName, List<SourceFile> sourceFiles)
		{
			string fullPath = Path.GetFullPath(fileName);
			int value;
			if (source_file_index.TryGetValue(fullPath, out value))
			{
				string name = sourceFiles[value - 1].Name;
				if (fileName.Equals(name))
				{
					report.Warning(2002, 1, "Source file `{0}' specified multiple times", name);
					return;
				}
				report.Warning(2002, 1, "Source filenames `{0}' and `{1}' both refer to the same file: {2}", fileName, name, fullPath);
			}
			else
			{
				SourceFile sourceFile = new SourceFile(fileName, fullPath, sourceFiles.Count + 1);
				sourceFiles.Add(sourceFile);
				source_file_index.Add(fullPath, sourceFile.Index);
			}
		}

		public bool ProcessWarningsList(string text, Action<int> action)
		{
			bool result = true;
			string[] array = text.Split(numeric_value_separator, StringSplitOptions.RemoveEmptyEntries);
			foreach (string text2 in array)
			{
				int result2;
				if (!int.TryParse(text2, NumberStyles.AllowLeadingWhite, CultureInfo.InvariantCulture, out result2))
				{
					report.Error(1904, "`{0}' is not a valid warning number", text2);
					result = false;
				}
				else if (report.CheckWarningCode(result2, Location.Null))
				{
					action(result2);
				}
			}
			return result;
		}

		private void Error_RequiresArgument(string option)
		{
			report.Error(2006, "Missing argument for `{0}' option", option);
		}

		private void Error_RequiresFileName(string option)
		{
			report.Error(2005, "Missing file specification for `{0}' option", option);
		}

		private void Error_WrongOption(string option)
		{
			report.Error(2007, "Unrecognized command-line option: `{0}'", option);
		}

		private static bool IsExternAliasValid(string identifier)
		{
			return Tokenizer.IsValidIdentifier(identifier);
		}

		private static string[] LoadArgs(string file)
		{
			List<string> list = new List<string>();
			StreamReader streamReader;
			try
			{
				streamReader = new StreamReader(file);
			}
			catch
			{
				return null;
			}
			StringBuilder stringBuilder = new StringBuilder();
			string text;
			while ((text = streamReader.ReadLine()) != null)
			{
				int length = text.Length;
				for (int i = 0; i < length; i++)
				{
					char c = text[i];
					switch (c)
					{
					case '"':
					case '\'':
					{
						char c2 = c;
						for (i++; i < length; i++)
						{
							c = text[i];
							if (c == c2)
							{
								break;
							}
							stringBuilder.Append(c);
						}
						break;
					}
					case ' ':
						if (stringBuilder.Length > 0)
						{
							list.Add(stringBuilder.ToString());
							stringBuilder.Length = 0;
						}
						break;
					default:
						stringBuilder.Append(c);
						break;
					}
				}
				if (stringBuilder.Length > 0)
				{
					list.Add(stringBuilder.ToString());
					stringBuilder.Length = 0;
				}
			}
			return list.ToArray();
		}

		private void OtherFlags()
		{
			output.WriteLine("Other flags in the compiler\n   --fatal[=COUNT]    Makes error after COUNT fatal\n   --lint             Enhanced warnings\n   --metadata-only    Produced assembly will contain metadata only\n   --parse            Only parses the source file\n   --runtime:VERSION  Sets mscorlib.dll metadata version: v1, v2, v4\n   --stacktrace       Shows stack trace at error location\n   --timestamp        Displays time stamps of various compiler events\n   -v                 Verbose parsing (for debugging the parser)\n   --mcs-debug X      Sets MCS debugging level to X\n   --break-on-ice     Breaks compilation on public compiler error");
		}

		private ParseResult ParseOption(string option, ref string[] args, CompilerSettings settings)
		{
			int num = option.IndexOf(':');
			string text;
			string text2;
			if (num == -1)
			{
				text = option;
				text2 = "";
			}
			else
			{
				text = option.Substring(0, num);
				text2 = option.Substring(num + 1);
			}
			switch (text.ToLowerInvariant())
			{
			case "/nologo":
				return ParseResult.Success;
			case "/t":
			case "/target":
				switch (text2)
				{
				case "exe":
					settings.Target = Target.Exe;
					break;
				case "winexe":
					settings.Target = Target.WinExe;
					break;
				case "library":
					settings.Target = Target.Library;
					settings.TargetExt = ".dll";
					break;
				case "module":
					settings.Target = Target.Module;
					settings.TargetExt = ".netmodule";
					break;
				default:
					report.Error(2019, "Invalid target type for -target. Valid options are `exe', `winexe', `library' or `module'");
					return ParseResult.Error;
				}
				return ParseResult.Success;
			case "/out":
				if (text2.Length == 0)
				{
					Error_RequiresFileName(option);
					return ParseResult.Error;
				}
				settings.OutputFile = text2;
				return ParseResult.Success;
			case "/o":
			case "/o+":
			case "/optimize":
			case "/optimize+":
				settings.Optimize = true;
				return ParseResult.Success;
			case "/o-":
			case "/optimize-":
				settings.Optimize = false;
				return ParseResult.Success;
			case "/incremental":
			case "/incremental+":
			case "/incremental-":
				return ParseResult.Success;
			case "/d":
			case "/define":
			{
				if (text2.Length == 0)
				{
					Error_RequiresArgument(option);
					return ParseResult.Error;
				}
				string[] array = text2.Split(argument_value_separator);
				for (int i = 0; i < array.Length; i++)
				{
					string text5 = array[i].Trim();
					if (!Tokenizer.IsValidIdentifier(text5))
					{
						report.Warning(2029, 1, "Invalid conditional define symbol `{0}'", text5);
					}
					else
					{
						settings.AddConditionalSymbol(text5);
					}
				}
				return ParseResult.Success;
			}
			case "/bugreport":
				output.WriteLine("To file bug reports, please visit: http://www.mono-project.com/Bugs");
				return ParseResult.Success;
			case "/pkg":
			{
				if (text2.Length == 0)
				{
					Error_RequiresArgument(option);
					return ParseResult.Error;
				}
				string packageFlags = Driver.GetPackageFlags(string.Join(" ", text2.Split(';', ',', '\n', '\r')), report);
				if (packageFlags == null)
				{
					return ParseResult.Error;
				}
				string[] extra_args = packageFlags.Trim(' ', '\n', '\r', '\t').Split(' ', '\t');
				args = AddArgs(args, extra_args);
				return ParseResult.Success;
			}
			case "/linkres":
			case "/linkresource":
			case "/res":
			case "/resource":
			{
				AssemblyResource assemblyResource = null;
				string[] array2 = text2.Split(argument_value_separator, StringSplitOptions.RemoveEmptyEntries);
				switch (array2.Length)
				{
				case 1:
					if (array2[0].Length != 0)
					{
						assemblyResource = new AssemblyResource(array2[0], Path.GetFileName(array2[0]));
						break;
					}
					goto default;
				case 2:
					assemblyResource = new AssemblyResource(array2[0], array2[1]);
					break;
				case 3:
					if (array2[2] != "public" && array2[2] != "private")
					{
						report.Error(1906, "Invalid resource visibility option `{0}'. Use either `public' or `private' instead", array2[2]);
						return ParseResult.Error;
					}
					assemblyResource = new AssemblyResource(array2[0], array2[1], array2[2] == "private");
					break;
				default:
					report.Error(-2005, "Wrong number of arguments for option `{0}'", option);
					return ParseResult.Error;
				}
				if (assemblyResource != null)
				{
					assemblyResource.IsEmbeded = text[1] == 'r' || text[1] == 'R';
					AddResource(assemblyResource, settings);
				}
				return ParseResult.Success;
			}
			case "/recurse":
				if (text2.Length == 0)
				{
					Error_RequiresFileName(option);
					return ParseResult.Error;
				}
				ProcessSourceFiles(text2, true, settings.SourceFiles);
				return ParseResult.Success;
			case "/r":
			case "/reference":
			{
				if (text2.Length == 0)
				{
					Error_RequiresFileName(option);
					return ParseResult.Error;
				}
				string[] array3 = text2.Split(argument_value_separator);
				string[] array = array3;
				foreach (string text3 in array)
				{
					if (text3.Length == 0)
					{
						continue;
					}
					string text4 = text3;
					int num2 = text4.IndexOf('=');
					if (num2 > -1)
					{
						string alias = text3.Substring(0, num2);
						string assembly = text3.Substring(num2 + 1);
						AddAssemblyReference(alias, assembly, settings);
						if (array3.Length != 1)
						{
							report.Error(2034, "Cannot specify multiple aliases using single /reference option");
							return ParseResult.Error;
						}
					}
					else
					{
						settings.AssemblyReferences.Add(text4);
					}
				}
				return ParseResult.Success;
			}
			case "/addmodule":
			{
				if (text2.Length == 0)
				{
					Error_RequiresFileName(option);
					return ParseResult.Error;
				}
				string[] array = text2.Split(argument_value_separator);
				foreach (string item2 in array)
				{
					settings.Modules.Add(item2);
				}
				return ParseResult.Success;
			}
			case "/win32res":
				if (text2.Length == 0)
				{
					Error_RequiresFileName(option);
					return ParseResult.Error;
				}
				if (settings.Win32IconFile != null)
				{
					report.Error(1565, "Cannot specify the `win32res' and the `win32ico' compiler option at the same time");
				}
				settings.Win32ResourceFile = text2;
				return ParseResult.Success;
			case "/win32icon":
				if (text2.Length == 0)
				{
					Error_RequiresFileName(option);
					return ParseResult.Error;
				}
				if (settings.Win32ResourceFile != null)
				{
					report.Error(1565, "Cannot specify the `win32res' and the `win32ico' compiler option at the same time");
				}
				settings.Win32IconFile = text2;
				return ParseResult.Success;
			case "/doc":
				if (text2.Length == 0)
				{
					Error_RequiresFileName(option);
					return ParseResult.Error;
				}
				settings.DocumentationFile = text2;
				return ParseResult.Success;
			case "/lib":
			{
				if (text2.Length == 0)
				{
					return ParseResult.Error;
				}
				string[] array = text2.Split(argument_value_separator);
				foreach (string item in array)
				{
					settings.ReferencesLookupPaths.Add(item);
				}
				return ParseResult.Success;
			}
			case "/debug-":
				settings.GenerateDebugInfo = false;
				return ParseResult.Success;
			case "/debug":
				if (text2.Equals("full", StringComparison.OrdinalIgnoreCase) || text2.Equals("pdbonly", StringComparison.OrdinalIgnoreCase) || num < 0)
				{
					settings.GenerateDebugInfo = true;
					return ParseResult.Success;
				}
				if (text2.Length > 0)
				{
					report.Error(1902, "Invalid debug option `{0}'. Valid options are `full' or `pdbonly'", text2);
				}
				else
				{
					Error_RequiresArgument(option);
				}
				return ParseResult.Error;
			case "/debug+":
				settings.GenerateDebugInfo = true;
				return ParseResult.Success;
			case "/checked":
			case "/checked+":
				settings.Checked = true;
				return ParseResult.Success;
			case "/checked-":
				settings.Checked = false;
				return ParseResult.Success;
			case "/clscheck":
			case "/clscheck+":
				settings.VerifyClsCompliance = true;
				return ParseResult.Success;
			case "/clscheck-":
				settings.VerifyClsCompliance = false;
				return ParseResult.Success;
			case "/unsafe":
			case "/unsafe+":
				settings.Unsafe = true;
				return ParseResult.Success;
			case "/unsafe-":
				settings.Unsafe = false;
				return ParseResult.Success;
			case "/warnaserror":
			case "/warnaserror+":
				if (text2.Length == 0)
				{
					settings.WarningsAreErrors = true;
					parser_settings.WarningsAreErrors = true;
				}
				else if (!ProcessWarningsList(text2, settings.AddWarningAsError))
				{
					return ParseResult.Error;
				}
				return ParseResult.Success;
			case "/warnaserror-":
				if (text2.Length == 0)
				{
					settings.WarningsAreErrors = false;
				}
				else if (!ProcessWarningsList(text2, settings.AddWarningOnly))
				{
					return ParseResult.Error;
				}
				return ParseResult.Success;
			case "/warn":
			case "/w":
				if (text2.Length == 0)
				{
					Error_RequiresArgument(option);
					return ParseResult.Error;
				}
				SetWarningLevel(text2, settings);
				return ParseResult.Success;
			case "/nowarn":
				if (text2.Length == 0)
				{
					Error_RequiresArgument(option);
					return ParseResult.Error;
				}
				if (!ProcessWarningsList(text2, settings.SetIgnoreWarning))
				{
					return ParseResult.Error;
				}
				return ParseResult.Success;
			case "/noconfig":
				settings.LoadDefaultReferences = false;
				return ParseResult.Success;
			case "/platform":
				if (text2.Length == 0)
				{
					Error_RequiresArgument(option);
					return ParseResult.Error;
				}
				switch (text2.ToLowerInvariant())
				{
				case "arm":
					settings.Platform = Platform.Arm;
					break;
				case "anycpu":
					settings.Platform = Platform.AnyCPU;
					break;
				case "x86":
					settings.Platform = Platform.X86;
					break;
				case "x64":
					settings.Platform = Platform.X64;
					break;
				case "itanium":
					settings.Platform = Platform.IA64;
					break;
				case "anycpu32bitpreferred":
					settings.Platform = Platform.AnyCPU32Preferred;
					break;
				default:
					report.Error(1672, "Invalid -platform option `{0}'. Valid options are `anycpu', `anycpu32bitpreferred', `arm', `x86', `x64' or `itanium'", text2);
					return ParseResult.Error;
				}
				return ParseResult.Success;
			case "/sdk":
				if (text2.Length == 0)
				{
					Error_RequiresArgument(option);
					return ParseResult.Error;
				}
				settings.SdkVersion = text2;
				return ParseResult.Success;
			case "/errorreport":
			case "/filealign":
				if (text2.Length == 0)
				{
					Error_RequiresArgument(option);
					return ParseResult.Error;
				}
				return ParseResult.Success;
			case "/helpinternal":
				OtherFlags();
				return ParseResult.Stop;
			case "/help":
			case "/?":
				Usage();
				return ParseResult.Stop;
			case "/main":
			case "/m":
				if (text2.Length == 0)
				{
					Error_RequiresArgument(option);
					return ParseResult.Error;
				}
				settings.MainClass = text2;
				return ParseResult.Success;
			case "/nostdlib":
			case "/nostdlib+":
				settings.StdLib = false;
				return ParseResult.Success;
			case "/nostdlib-":
				settings.StdLib = true;
				return ParseResult.Success;
			case "/fullpaths":
				settings.ShowFullPaths = true;
				return ParseResult.Success;
			case "/keyfile":
				if (text2.Length == 0)
				{
					Error_RequiresFileName(option);
					return ParseResult.Error;
				}
				settings.StrongNameKeyFile = text2;
				return ParseResult.Success;
			case "/keycontainer":
				if (text2.Length == 0)
				{
					Error_RequiresArgument(option);
					return ParseResult.Error;
				}
				settings.StrongNameKeyContainer = text2;
				return ParseResult.Success;
			case "/delaysign+":
			case "/delaysign":
				settings.StrongNameDelaySign = true;
				return ParseResult.Success;
			case "/delaysign-":
				settings.StrongNameDelaySign = false;
				return ParseResult.Success;
			case "/langversion":
				if (text2.Length == 0)
				{
					Error_RequiresArgument(option);
					return ParseResult.Error;
				}
				switch (text2.ToLowerInvariant())
				{
				case "iso-1":
				case "1":
					settings.Version = LanguageVersion.ISO_1;
					return ParseResult.Success;
				case "default":
					settings.Version = LanguageVersion.V_6;
					return ParseResult.Success;
				case "2":
				case "iso-2":
					settings.Version = LanguageVersion.ISO_2;
					return ParseResult.Success;
				case "3":
					settings.Version = LanguageVersion.V_3;
					return ParseResult.Success;
				case "4":
					settings.Version = LanguageVersion.V_4;
					return ParseResult.Success;
				case "5":
					settings.Version = LanguageVersion.V_5;
					return ParseResult.Success;
				case "6":
					settings.Version = LanguageVersion.V_6;
					return ParseResult.Success;
				case "experimental":
					settings.Version = LanguageVersion.Experimental;
					return ParseResult.Success;
				case "future":
					report.Warning(8000, 1, "Language version `future' is no longer supported");
					goto case "6";
				default:
					report.Error(1617, "Invalid -langversion option `{0}'. It must be `ISO-1', `ISO-2', Default or value in range 1 to 6", text2);
					return ParseResult.Error;
				}
			case "/codepage":
				if (text2.Length == 0)
				{
					Error_RequiresArgument(option);
					return ParseResult.Error;
				}
				if (!(text2 == "utf8"))
				{
					if (!(text2 == "reset"))
					{
						try
						{
							settings.Encoding = Encoding.GetEncoding(int.Parse(text2));
						}
						catch
						{
							report.Error(2016, "Code page `{0}' is invalid or not installed", text2);
						}
						return ParseResult.Error;
					}
					settings.Encoding = Encoding.Default;
				}
				else
				{
					settings.Encoding = Encoding.UTF8;
				}
				return ParseResult.Success;
			case "runtimemetadataversion":
				if (text2.Length == 0)
				{
					Error_RequiresArgument(option);
					return ParseResult.Error;
				}
				settings.RuntimeMetadataVersion = text2;
				return ParseResult.Success;
			default:
				return ParseResult.UnknownOption;
			}
		}

		private ParseResult ParseOptionUnix(string arg, ref string[] args, ref int i, CompilerSettings settings)
		{
			switch (arg)
			{
			case "-v":
				settings.VerboseParserFlag++;
				return ParseResult.Success;
			case "--version":
				Version();
				return ParseResult.Stop;
			case "--parse":
				settings.ParseOnly = true;
				return ParseResult.Success;
			case "--main":
			case "-m":
				report.Warning(-29, 1, "Compatibility: Use -main:CLASS instead of --main CLASS or -m CLASS");
				if (i + 1 >= args.Length)
				{
					Error_RequiresArgument(arg);
					return ParseResult.Error;
				}
				settings.MainClass = args[++i];
				return ParseResult.Success;
			case "--unsafe":
				report.Warning(-29, 1, "Compatibility: Use -unsafe instead of --unsafe");
				settings.Unsafe = true;
				return ParseResult.Success;
			case "/?":
			case "/h":
			case "/help":
			case "--help":
				Usage();
				return ParseResult.Stop;
			case "--define":
				report.Warning(-29, 1, "Compatibility: Use -d:SYMBOL instead of --define SYMBOL");
				if (i + 1 >= args.Length)
				{
					Error_RequiresArgument(arg);
					return ParseResult.Error;
				}
				settings.AddConditionalSymbol(args[++i]);
				return ParseResult.Success;
			case "--tokenize":
				settings.TokenizeOnly = true;
				return ParseResult.Success;
			case "-o":
			case "--output":
				report.Warning(-29, 1, "Compatibility: Use -out:FILE instead of --output FILE or -o FILE");
				if (i + 1 >= args.Length)
				{
					Error_RequiresArgument(arg);
					return ParseResult.Error;
				}
				settings.OutputFile = args[++i];
				return ParseResult.Success;
			case "--checked":
				report.Warning(-29, 1, "Compatibility: Use -checked instead of --checked");
				settings.Checked = true;
				return ParseResult.Success;
			case "--stacktrace":
				settings.Stacktrace = true;
				return ParseResult.Success;
			case "--linkresource":
			case "--linkres":
				report.Warning(-29, 1, "Compatibility: Use -linkres:VALUE instead of --linkres VALUE");
				if (i + 1 >= args.Length)
				{
					Error_RequiresArgument(arg);
					return ParseResult.Error;
				}
				AddResource(new AssemblyResource(args[++i], args[i]), settings);
				return ParseResult.Success;
			case "--resource":
			case "--res":
				report.Warning(-29, 1, "Compatibility: Use -res:VALUE instead of --res VALUE");
				if (i + 1 >= args.Length)
				{
					Error_RequiresArgument(arg);
					return ParseResult.Error;
				}
				AddResource(new AssemblyResource(args[++i], args[i], true), settings);
				return ParseResult.Success;
			case "--target":
				report.Warning(-29, 1, "Compatibility: Use -target:KIND instead of --target KIND");
				if (i + 1 >= args.Length)
				{
					Error_RequiresArgument(arg);
					return ParseResult.Error;
				}
				switch (args[++i])
				{
				case "library":
					settings.Target = Target.Library;
					settings.TargetExt = ".dll";
					break;
				case "exe":
					settings.Target = Target.Exe;
					break;
				case "winexe":
					settings.Target = Target.WinExe;
					break;
				case "module":
					settings.Target = Target.Module;
					settings.TargetExt = ".dll";
					break;
				default:
					report.Error(2019, "Invalid target type for -target. Valid options are `exe', `winexe', `library' or `module'");
					break;
				}
				return ParseResult.Success;
			case "-r":
			{
				report.Warning(-29, 1, "Compatibility: Use -r:LIBRARY instead of -r library");
				if (i + 1 >= args.Length)
				{
					Error_RequiresArgument(arg);
					return ParseResult.Error;
				}
				string text2 = args[++i];
				int num = text2.IndexOf('=');
				if (num > -1)
				{
					string alias = text2.Substring(0, num);
					string assembly = text2.Substring(num + 1);
					AddAssemblyReference(alias, assembly, settings);
					return ParseResult.Success;
				}
				settings.AssemblyReferences.Add(text2);
				return ParseResult.Success;
			}
			case "-L":
				report.Warning(-29, 1, "Compatibility: Use -lib:ARG instead of --L arg");
				if (i + 1 >= args.Length)
				{
					Error_RequiresArgument(arg);
					return ParseResult.Error;
				}
				settings.ReferencesLookupPaths.Add(args[++i]);
				return ParseResult.Success;
			case "--lint":
				settings.EnhancedWarnings = true;
				return ParseResult.Success;
			case "--nostdlib":
				report.Warning(-29, 1, "Compatibility: Use -nostdlib instead of --nostdlib");
				settings.StdLib = false;
				return ParseResult.Success;
			case "--nowarn":
			{
				report.Warning(-29, 1, "Compatibility: Use -nowarn instead of --nowarn");
				if (i + 1 >= args.Length)
				{
					Error_RequiresArgument(arg);
					return ParseResult.Error;
				}
				int ignoreWarning = 0;
				try
				{
					ignoreWarning = int.Parse(args[++i]);
				}
				catch
				{
					Usage();
					Environment.Exit(1);
				}
				settings.SetIgnoreWarning(ignoreWarning);
				return ParseResult.Success;
			}
			case "--wlevel":
				report.Warning(-29, 1, "Compatibility: Use -warn:LEVEL instead of --wlevel LEVEL");
				if (i + 1 >= args.Length)
				{
					Error_RequiresArgument(arg);
					return ParseResult.Error;
				}
				SetWarningLevel(args[++i], settings);
				return ParseResult.Success;
			case "--mcs-debug":
				if (i + 1 >= args.Length)
				{
					Error_RequiresArgument(arg);
					return ParseResult.Error;
				}
				try
				{
					settings.DebugFlags = int.Parse(args[++i]);
				}
				catch
				{
					Error_RequiresArgument(arg);
					return ParseResult.Error;
				}
				return ParseResult.Success;
			case "--about":
				About();
				return ParseResult.Stop;
			case "--recurse":
				report.Warning(-29, 1, "Compatibility: Use -recurse:PATTERN option instead --recurse PATTERN");
				if (i + 1 >= args.Length)
				{
					Error_RequiresArgument(arg);
					return ParseResult.Error;
				}
				ProcessSourceFiles(args[++i], true, settings.SourceFiles);
				return ParseResult.Success;
			case "--timestamp":
				settings.Timestamps = true;
				return ParseResult.Success;
			case "--debug":
			case "-g":
				report.Warning(-29, 1, "Compatibility: Use -debug option instead of -g or --debug");
				settings.GenerateDebugInfo = true;
				return ParseResult.Success;
			case "--noconfig":
				report.Warning(-29, 1, "Compatibility: Use -noconfig option instead of --noconfig");
				settings.LoadDefaultReferences = false;
				return ParseResult.Success;
			case "--metadata-only":
				settings.WriteMetadataOnly = true;
				return ParseResult.Success;
			case "--break-on-ice":
				settings.BreakOnInternalError = true;
				return ParseResult.Success;
			default:
				if (arg.StartsWith("--fatal", StringComparison.Ordinal))
				{
					int result = 1;
					if (arg.StartsWith("--fatal=", StringComparison.Ordinal))
					{
						int.TryParse(arg.Substring(8), out result);
					}
					settings.FatalCounter = result;
					return ParseResult.Success;
				}
				if (arg.StartsWith("--runtime:", StringComparison.Ordinal))
				{
					switch (arg.Substring(10))
					{
					case "v1":
					case "V1":
						settings.StdLibRuntimeVersion = RuntimeVersion.v1;
						break;
					case "v2":
					case "V2":
						settings.StdLibRuntimeVersion = RuntimeVersion.v2;
						break;
					case "v4":
					case "V4":
						settings.StdLibRuntimeVersion = RuntimeVersion.v4;
						break;
					}
					return ParseResult.Success;
				}
				if (arg.StartsWith("--getresourcestrings:", StringComparison.Ordinal))
				{
					string text = arg.Substring(21).Trim();
					if (text.Length < 1)
					{
						Error_RequiresArgument(arg);
						return ParseResult.Error;
					}
					if (settings.GetResourceStrings == null)
					{
						settings.GetResourceStrings = new List<string>();
					}
					settings.GetResourceStrings.Add(text);
					return ParseResult.Success;
				}
				return ParseResult.UnknownOption;
			}
		}

		private void SetWarningLevel(string s, CompilerSettings settings)
		{
			int num = -1;
			try
			{
				num = int.Parse(s);
			}
			catch
			{
			}
			if (num < 0 || num > 4)
			{
				report.Error(1900, "Warning level must be in the range 0-4");
			}
			else
			{
				settings.WarningLevel = num;
			}
		}

		private static void SplitPathAndPattern(string spec, out string path, out string pattern)
		{
			int num = spec.LastIndexOf('/');
			switch (num)
			{
			case 0:
				path = "\\";
				pattern = spec.Substring(1);
				break;
			default:
				path = spec.Substring(0, num);
				pattern = spec.Substring(num + 1);
				break;
			case -1:
				num = spec.LastIndexOf('\\');
				if (num != -1)
				{
					path = spec.Substring(0, num);
					pattern = spec.Substring(num + 1);
				}
				else
				{
					path = ".";
					pattern = spec;
				}
				break;
			}
		}

		private void Usage()
		{
			output.WriteLine("Mono C# compiler, Copyright 2001-2011 Novell, Inc., Copyright 2011-2012 Xamarin, Inc\nmcs [options] source-files\n   --about              About the Mono C# compiler\n   -addmodule:M1[,Mn]   Adds the module to the generated assembly\n   -checked[+|-]        Sets default aritmetic overflow context\n   -clscheck[+|-]       Disables CLS Compliance verifications\n   -codepage:ID         Sets code page to the one in ID (number, utf8, reset)\n   -define:S1[;S2]      Defines one or more conditional symbols (short: -d)\n   -debug[+|-], -g      Generate debugging information\n   -delaysign[+|-]      Only insert the public key into the assembly (no signing)\n   -doc:FILE            Process documentation comments to XML file\n   -fullpaths           Any issued error or warning uses absolute file path\n   -help                Lists all compiler options (short: -?)\n   -keycontainer:NAME   The key pair container used to sign the output assembly\n   -keyfile:FILE        The key file used to strongname the ouput assembly\n   -langversion:TEXT    Specifies language version: ISO-1, ISO-2, 3, 4, 5, Default or Future\n   -lib:PATH1[,PATHn]   Specifies the location of referenced assemblies\n   -main:CLASS          Specifies the class with the Main method (short: -m)\n   -noconfig            Disables implicitly referenced assemblies\n   -nostdlib[+|-]       Does not reference mscorlib.dll library\n   -nowarn:W1[,Wn]      Suppress one or more compiler warnings\n   -optimize[+|-]       Enables advanced compiler optimizations (short: -o)\n   -out:FILE            Specifies output assembly name\n   -pkg:P1[,Pn]         References packages P1..Pn\n   -platform:ARCH       Specifies the target platform of the output assembly\n                        ARCH can be one of: anycpu, anycpu32bitpreferred, arm,\n                        x86, x64 or itanium. The default is anycpu.\n   -recurse:SPEC        Recursively compiles files according to SPEC pattern\n   -reference:A1[,An]   Imports metadata from the specified assembly (short: -r)\n   -reference:ALIAS=A   Imports metadata using specified extern alias (short: -r)\n   -sdk:VERSION         Specifies SDK version of referenced assemblies\n                        VERSION can be one of: 2, 4, 4.5 (default) or a custom value\n   -target:KIND         Specifies the format of the output assembly (short: -t)\n                        KIND can be one of: exe, winexe, library, module\n   -unsafe[+|-]         Allows to compile code which uses unsafe keyword\n   -warnaserror[+|-]    Treats all warnings as errors\n   -warnaserror[+|-]:W1[,Wn] Treats one or more compiler warnings as errors\n   -warn:0-4            Sets warning level, the default is 4 (short -w:)\n   -helppublic        Shows public and advanced compiler options\n\nResources:\n   -linkresource:FILE[,ID] Links FILE as a resource (short: -linkres)\n   -resource:FILE[,ID]     Embed FILE as a resource (short: -res)\n   -win32res:FILE          Specifies Win32 resource file (.res)\n   -win32icon:FILE         Use this icon for the output\n   @file                   Read response file for more options\n\nOptions can be of the form -option or /option");
		}

		private void Version()
		{
			string arg = MethodBase.GetCurrentMethod().DeclaringType.Assembly.GetName().Version.ToString();
			output.WriteLine("Mono C# compiler version {0}", arg);
		}
	}
}
