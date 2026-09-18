using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Mono.CSharp
{
	public class Report
	{
		public const int RuntimeErrorId = 10000;

		private Dictionary<int, WarningRegions> warning_regions_table;

		private ReportPrinter printer;

		private int reporting_disabled;

		private readonly CompilerSettings settings;

		private List<string> extra_information = new List<string>();

		public static readonly int[] AllWarnings = new int[137]
		{
			28, 67, 78, 105, 108, 109, 114, 162, 164, 168,
			169, 183, 184, 197, 219, 251, 252, 253, 278, 282,
			402, 414, 419, 420, 429, 436, 437, 440, 458, 464,
			465, 467, 469, 472, 473, 612, 618, 626, 628, 642,
			649, 652, 657, 658, 659, 660, 661, 665, 672, 675,
			693, 728, 809, 824, 1030, 1058, 1060, 1066, 1522, 1570,
			1571, 1572, 1573, 1574, 1580, 1581, 1584, 1587, 1589, 1590,
			1591, 1592, 1607, 1616, 1633, 1634, 1635, 1685, 1690, 1691,
			1692, 1695, 1696, 1697, 1699, 1700, 1701, 1702, 1709, 1711,
			1717, 1718, 1720, 1735, 1901, 1956, 1981, 1998, 2002, 2023,
			2029, 3000, 3001, 3002, 3003, 3005, 3006, 3007, 3008, 3009,
			3010, 3011, 3012, 3013, 3014, 3015, 3016, 3017, 3018, 3019,
			3021, 3022, 3023, 3024, 3026, 3027, 4014, 4024, 4025, 4026,
			7035, 7080, 7081, 7082, 7095, 8009, 8094
		};

		private static HashSet<int> AllWarningsHashSet;

		public int Warnings => printer.WarningsCount;

		public int Errors => printer.ErrorsCount;

		public bool IsDisabled => reporting_disabled > 0;

		public ReportPrinter Printer => printer;

		public Report(CompilerContext context, ReportPrinter printer)
		{
			if (context == null)
			{
				throw new ArgumentNullException("settings");
			}
			if (printer == null)
			{
				throw new ArgumentNullException("printer");
			}
			settings = context.Settings;
			this.printer = printer;
		}

		public void DisableReporting()
		{
			reporting_disabled++;
		}

		public void EnableReporting()
		{
			reporting_disabled--;
		}

		public void FeatureIsNotAvailable(CompilerContext compiler, Location loc, string feature)
		{
			string arg;
			switch (compiler.Settings.Version)
			{
			case LanguageVersion.ISO_1:
				arg = "1.0";
				break;
			case LanguageVersion.ISO_2:
				arg = "2.0";
				break;
			case LanguageVersion.V_3:
				arg = "3.0";
				break;
			case LanguageVersion.V_4:
				arg = "4.0";
				break;
			case LanguageVersion.V_5:
				arg = "5.0";
				break;
			case LanguageVersion.V_6:
				arg = "6.0";
				break;
			default:
				throw new InternalErrorException("Invalid feature version", compiler.Settings.Version);
			}
			Error(1644, loc, "Feature `{0}' cannot be used because it is not part of the C# {1} language specification", feature, arg);
		}

		public void FeatureIsNotSupported(Location loc, string feature)
		{
			Error(1644, loc, "Feature `{0}' is not supported in Mono mcs1 compiler. Consider using the `gmcs' compiler instead", feature);
		}

		public void RuntimeMissingSupport(Location loc, string feature)
		{
			Error(-88, loc, "Your .NET Runtime does not support `{0}'. Please use the latest Mono runtime instead.", feature);
		}

		public void SymbolRelatedToPreviousError(Location loc, string symbol)
		{
			SymbolRelatedToPreviousError(loc.ToString());
		}

		public void SymbolRelatedToPreviousError(MemberSpec ms)
		{
			if (reporting_disabled > 0 || !printer.HasRelatedSymbolSupport)
			{
				return;
			}
			MemberCore memberCore = ms.MemberDefinition as MemberCore;
			while (ms is ElementTypeSpec)
			{
				ms = ((ElementTypeSpec)ms).Element;
				memberCore = ms.MemberDefinition as MemberCore;
			}
			if (memberCore != null)
			{
				SymbolRelatedToPreviousError(memberCore);
				return;
			}
			if (ms.DeclaringType != null)
			{
				ms = ms.DeclaringType;
			}
			ImportedTypeDefinition importedTypeDefinition = ms.MemberDefinition as ImportedTypeDefinition;
			if (importedTypeDefinition != null)
			{
				ImportedAssemblyDefinition importedAssemblyDefinition = importedTypeDefinition.DeclaringAssembly as ImportedAssemblyDefinition;
				SymbolRelatedToPreviousError(importedAssemblyDefinition.Location);
			}
		}

		public void SymbolRelatedToPreviousError(MemberCore mc)
		{
			SymbolRelatedToPreviousError(mc.Location, mc.GetSignatureForError());
		}

		public void SymbolRelatedToPreviousError(string loc)
		{
			string item = $"{loc} (Location of the symbol related to previous ";
			if (!extra_information.Contains(item))
			{
				extra_information.Add(item);
			}
		}

		public bool CheckWarningCode(int code, Location loc)
		{
			if (AllWarningsHashSet == null)
			{
				AllWarningsHashSet = new HashSet<int>(AllWarnings);
			}
			if (AllWarningsHashSet.Contains(code))
			{
				return true;
			}
			Warning(1691, 1, loc, "`{0}' is not a valid warning number", code);
			return false;
		}

		public void ExtraInformation(Location loc, string msg)
		{
			extra_information.Add($"{loc} {msg}");
		}

		public WarningRegions RegisterWarningRegion(Location location)
		{
			WarningRegions value;
			if (warning_regions_table == null)
			{
				value = null;
				warning_regions_table = new Dictionary<int, WarningRegions>();
			}
			else
			{
				warning_regions_table.TryGetValue(location.File, out value);
			}
			if (value == null)
			{
				value = new WarningRegions();
				warning_regions_table.Add(location.File, value);
			}
			return value;
		}

		public void Warning(int code, int level, Location loc, string message)
		{
			WarningRegions value;
			if (reporting_disabled <= 0 && settings.IsWarningEnabled(code, level) && (warning_regions_table == null || loc.IsNull || !warning_regions_table.TryGetValue(loc.File, out value) || value.IsWarningEnabled(code, loc.Row)))
			{
				AbstractMessage msg;
				if (settings.IsWarningAsError(code))
				{
					message = "Warning as Error: " + message;
					msg = new ErrorMessage(code, loc, message, extra_information);
				}
				else
				{
					msg = new WarningMessage(code, loc, message, extra_information);
				}
				extra_information.Clear();
				printer.Print(msg, settings.ShowFullPaths);
			}
		}

		public void Warning(int code, int level, Location loc, string format, string arg)
		{
			Warning(code, level, loc, string.Format(format, arg));
		}

		public void Warning(int code, int level, Location loc, string format, string arg1, string arg2)
		{
			Warning(code, level, loc, string.Format(format, arg1, arg2));
		}

		public void Warning(int code, int level, Location loc, string format, params object[] args)
		{
			Warning(code, level, loc, string.Format(format, args));
		}

		public void Warning(int code, int level, string message)
		{
			Warning(code, level, Location.Null, message);
		}

		public void Warning(int code, int level, string format, string arg)
		{
			Warning(code, level, Location.Null, format, arg);
		}

		public void Warning(int code, int level, string format, string arg1, string arg2)
		{
			Warning(code, level, Location.Null, format, arg1, arg2);
		}

		public void Warning(int code, int level, string format, params string[] args)
		{
			Warning(code, level, Location.Null, string.Format(format, args));
		}

		public void Error(int code, Location loc, string error)
		{
			if (reporting_disabled <= 0)
			{
				ErrorMessage errorMessage = new ErrorMessage(code, loc, error, extra_information);
				extra_information.Clear();
				printer.Print(errorMessage, settings.ShowFullPaths);
				if (settings.Stacktrace)
				{
					Console.WriteLine(FriendlyStackTrace(new StackTrace(true)));
				}
				if (printer.ErrorsCount == settings.FatalCounter)
				{
					throw new FatalException(errorMessage.Text);
				}
			}
		}

		public void Error(int code, Location loc, string format, string arg)
		{
			Error(code, loc, string.Format(format, arg));
		}

		public void Error(int code, Location loc, string format, string arg1, string arg2)
		{
			Error(code, loc, string.Format(format, arg1, arg2));
		}

		public void Error(int code, Location loc, string format, params string[] args)
		{
			Error(code, loc, string.Format(format, args));
		}

		public void Error(int code, string error)
		{
			Error(code, Location.Null, error);
		}

		public void Error(int code, string format, string arg)
		{
			Error(code, Location.Null, format, arg);
		}

		public void Error(int code, string format, string arg1, string arg2)
		{
			Error(code, Location.Null, format, arg1, arg2);
		}

		public void Error(int code, string format, params string[] args)
		{
			Error(code, Location.Null, string.Format(format, args));
		}

		public ReportPrinter SetPrinter(ReportPrinter printer)
		{
			ReportPrinter result = this.printer;
			this.printer = printer;
			return result;
		}

		[Conditional("MCS_DEBUG")]
		public static void Debug(string message, params object[] args)
		{
		}

		[Conditional("MCS_DEBUG")]
		public static void Debug(int category, string message, params object[] args)
		{
			StringBuilder stringBuilder = new StringBuilder(message);
			if (args != null && args.Length != 0)
			{
				stringBuilder.Append(": ");
				bool flag = true;
				foreach (object obj in args)
				{
					if (flag)
					{
						flag = false;
					}
					else
					{
						stringBuilder.Append(", ");
					}
					if (obj == null)
					{
						stringBuilder.Append("null");
					}
					else
					{
						stringBuilder.Append(obj);
					}
				}
			}
			Console.WriteLine(stringBuilder.ToString());
		}

		private static string FriendlyStackTrace(StackTrace t)
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			for (int i = 0; i < t.FrameCount; i++)
			{
				StackFrame frame = t.GetFrame(i);
				MethodBase method = frame.GetMethod();
				if (!flag && method.ReflectedType == typeof(Report))
				{
					continue;
				}
				flag = true;
				stringBuilder.Append("\tin ");
				if (frame.GetFileLineNumber() > 0)
				{
					stringBuilder.AppendFormat("(at {0}:{1}) ", frame.GetFileName(), frame.GetFileLineNumber());
				}
				stringBuilder.AppendFormat("{0}.{1} (", method.ReflectedType.Name, method.Name);
				bool flag2 = true;
				ParameterInfo[] parameters = method.GetParameters();
				foreach (ParameterInfo parameterInfo in parameters)
				{
					if (!flag2)
					{
						stringBuilder.Append(", ");
					}
					flag2 = false;
					stringBuilder.Append(parameterInfo.ParameterType.FullName);
				}
				stringBuilder.Append(")\n");
			}
			return stringBuilder.ToString();
		}
	}
}
