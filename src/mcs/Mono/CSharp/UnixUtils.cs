using System.Runtime.InteropServices;

namespace Mono.CSharp
{
	public class UnixUtils
	{
		[DllImport("libc", EntryPoint = "isatty")]
		private static extern int _isatty(int fd);

		public static bool isatty(int fd)
		{
			try
			{
				return _isatty(fd) == 1;
			}
			catch
			{
				return false;
			}
		}
	}
}
