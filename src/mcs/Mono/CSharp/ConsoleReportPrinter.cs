using System;
using System.IO;

namespace Mono.CSharp
{
	public class ConsoleReportPrinter : StreamReportPrinter
	{
		private static readonly string prefix;

		private static readonly string postfix;

		static ConsoleReportPrinter()
		{
			string environmentVariable = Environment.GetEnvironmentVariable("TERM");
			bool flag = false;
			switch (environmentVariable)
			{
			case "xterm":
			case "rxvt":
			case "rxvt-unicode":
				if (Environment.GetEnvironmentVariable("COLORTERM") != null)
				{
					flag = true;
				}
				break;
			case "xterm-color":
			case "xterm-256color":
				flag = true;
				break;
			}
			if (!flag || !UnixUtils.isatty(1) || !UnixUtils.isatty(2))
			{
				return;
			}
			string text = Environment.GetEnvironmentVariable("MCS_COLORS");
			if (text == null)
			{
				text = "errors=red";
			}
			if (!(text == "disable") && text.StartsWith("errors="))
			{
				text = text.Substring(7);
				int num = text.IndexOf(",");
				if (num == -1)
				{
					prefix = GetForeground(text);
				}
				else
				{
					prefix = GetBackground(text.Substring(num + 1)) + GetForeground(text.Substring(0, num));
				}
				postfix = "\u001b[0m";
			}
		}

		public ConsoleReportPrinter()
			: base(Console.Error)
		{
		}

		public ConsoleReportPrinter(TextWriter writer)
			: base(writer)
		{
		}

		private static int NameToCode(string s)
		{
			switch (s)
			{
			case "black":
				return 0;
			case "red":
				return 1;
			case "green":
				return 2;
			case "yellow":
				return 3;
			case "blue":
				return 4;
			case "magenta":
				return 5;
			case "cyan":
				return 6;
			case "grey":
			case "white":
				return 7;
			default:
				return 7;
			}
		}

		private static string GetForeground(string s)
		{
			string text;
			if (s.StartsWith("bright"))
			{
				text = "1;";
				s = s.Substring(6);
			}
			else
			{
				text = "";
			}
			return "\u001b[" + text + (30 + NameToCode(s)) + "m";
		}

		private static string GetBackground(string s)
		{
			return "\u001b[" + (40 + NameToCode(s)) + "m";
		}

		protected override string FormatText(string txt)
		{
			if (prefix != null)
			{
				return prefix + txt + postfix;
			}
			return txt;
		}
	}
}
