using System.Collections.Generic;

namespace Mono.CSharp
{
	public class WarningRegions
	{
		private abstract class PragmaCmd
		{
			public int Line;

			protected PragmaCmd(int line)
			{
				Line = line;
			}

			public abstract bool IsEnabled(int code, bool previous);
		}

		private class Disable : PragmaCmd
		{
			private int code;

			public Disable(int line, int code)
				: base(line)
			{
				this.code = code;
			}

			public override bool IsEnabled(int code, bool previous)
			{
				return (this.code != code) & previous;
			}
		}

		private class DisableAll : PragmaCmd
		{
			public DisableAll(int line)
				: base(line)
			{
			}

			public override bool IsEnabled(int code, bool previous)
			{
				return false;
			}
		}

		private class Enable : PragmaCmd
		{
			private int code;

			public Enable(int line, int code)
				: base(line)
			{
				this.code = code;
			}

			public override bool IsEnabled(int code, bool previous)
			{
				return (this.code == code) | previous;
			}
		}

		private class EnableAll : PragmaCmd
		{
			public EnableAll(int line)
				: base(line)
			{
			}

			public override bool IsEnabled(int code, bool previous)
			{
				return true;
			}
		}

		private List<PragmaCmd> regions = new List<PragmaCmd>();

		public void WarningDisable(int line)
		{
			regions.Add(new DisableAll(line));
		}

		public void WarningDisable(Location location, int code, Report Report)
		{
			if (Report.CheckWarningCode(code, location))
			{
				regions.Add(new Disable(location.Row, code));
			}
		}

		public void WarningEnable(int line)
		{
			regions.Add(new EnableAll(line));
		}

		public void WarningEnable(Location location, int code, CompilerContext context)
		{
			if (context.Report.CheckWarningCode(code, location))
			{
				if (context.Settings.IsWarningDisabledGlobally(code))
				{
					context.Report.Warning(1635, 1, location, "Cannot restore warning `CS{0:0000}' because it was disabled globally", code);
				}
				regions.Add(new Enable(location.Row, code));
			}
		}

		public bool IsWarningEnabled(int code, int src_line)
		{
			bool flag = true;
			foreach (PragmaCmd region in regions)
			{
				if (src_line < region.Line)
				{
					break;
				}
				flag = region.IsEnabled(code, flag);
			}
			return flag;
		}
	}
}
