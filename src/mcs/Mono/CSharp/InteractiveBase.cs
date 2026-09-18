using System;
using System.IO;

namespace Mono.CSharp
{
	public class InteractiveBase
	{
		public static TextWriter Output = Console.Out;

		public static TextWriter Error = Console.Error;

		public static string Prompt = "csharp> ";

		public static string ContinuationPrompt = "      > ";

		public static bool QuitRequested;

		public static Evaluator Evaluator;

		public static string help => "Static methods:\n  Describe (object);       - Describes the object's type\n  LoadPackage (package);   - Loads the given Package (like -pkg:FILE)\n  LoadAssembly (assembly); - Loads the given assembly (like -r:ASSEMBLY)\n  ShowVars ();             - Shows defined local variables.\n  ShowUsing ();            - Show active using declarations.\n  Prompt                   - The prompt used by the C# shell\n  ContinuationPrompt       - The prompt for partial input\n  Time (() => { });        - Times the specified code\n  print (obj);             - Shorthand for Console.WriteLine\n  quit;                    - You'll never believe it - this quits the repl!\n  help;                    - This help text\n";

		public static object quit
		{
			get
			{
				QuitRequested = true;
				return typeof(Evaluator.QuitValue);
			}
		}

		public static void ShowVars()
		{
			Output.Write(Evaluator.GetVars());
			Output.Flush();
		}

		public static void ShowUsing()
		{
			Output.Write(Evaluator.GetUsing());
			Output.Flush();
		}

		public static TimeSpan Time(Action a)
		{
			DateTime now = DateTime.Now;
			a();
			return DateTime.Now - now;
		}

		public static void LoadPackage(string pkg)
		{
			if (pkg == null)
			{
				Error.WriteLine("Invalid package specified");
				return;
			}
			string[] array = Driver.GetPackageFlags(pkg, null).Trim(' ', '\n', '\r', '\t').Split(' ', '\t');
			foreach (string text in array)
			{
				if (text.StartsWith("-r:") || text.StartsWith("/r:") || text.StartsWith("/reference:"))
				{
					string file = text.Substring(text.IndexOf(':') + 1);
					Evaluator.LoadAssembly(file);
				}
			}
		}

		public static void LoadAssembly(string assembly)
		{
			Evaluator.LoadAssembly(assembly);
		}

		public static void print(object obj)
		{
			Output.WriteLine(obj);
		}

		public static void print(string fmt, params object[] args)
		{
			Output.WriteLine(fmt, args);
		}

		public static void Quit()
		{
			QuitRequested = true;
		}
	}
}
