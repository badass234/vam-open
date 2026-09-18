using System;
using System.IO;
using Mono.CSharp.Linq;

namespace Mono.CSharp
{
	public class CompilerCallableEntryPoint : MarshalByRefObject
	{
		public static int[] AllWarningNumbers => Report.AllWarnings;

		public static bool InvokeCompiler(string[] args, TextWriter error)
		{
			try
			{
				CompilerSettings compilerSettings = new CommandLineParser(error).ParseArguments(args);
				if (compilerSettings == null)
				{
					return false;
				}
				return new Driver(new CompilerContext(compilerSettings, new StreamReportPrinter(error))).Compile();
			}
			finally
			{
				Reset();
			}
		}

		public static void Reset()
		{
			Reset(true);
		}

		public static void PartialReset()
		{
			Reset(false);
		}

		public static void Reset(bool full_flag)
		{
			Location.Reset();
			if (full_flag)
			{
				QueryBlock.TransparentParameter.Reset();
				TypeInfo.Reset();
			}
		}
	}
}
