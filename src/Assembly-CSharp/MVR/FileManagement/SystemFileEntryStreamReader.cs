using System.IO;

namespace MVR.FileManagement
{
	public class SystemFileEntryStreamReader : FileEntryStreamReader
	{
		public SystemFileEntryStreamReader(SystemFileEntry entry)
			: base(entry)
		{
			StreamReader = new StreamReader(entry.Path);
		}
	}
}
