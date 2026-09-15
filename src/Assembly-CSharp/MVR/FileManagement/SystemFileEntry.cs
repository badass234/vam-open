using System.IO;

namespace MVR.FileManagement
{
	public class SystemFileEntry : FileEntry
	{
		public SystemFileEntry(string path)
			: base(path)
		{
			flagBasePath = Path + ".";
			favPath = Path + ".fav";
			base.hidePath = Path + ".hide";
			Exists = File.Exists(Path);
			FullPath = System.IO.Path.GetFullPath(Path);
			FullSlashPath = FullPath.Replace('\\', '/');
			if (Exists)
			{
				FileInfo fileInfo = new FileInfo(Path);
				LastWriteTime = fileInfo.LastWriteTime;
				Size = fileInfo.Length;
			}
		}

		public override FileEntryStream OpenStream()
		{
			return new SystemFileEntryStream(this);
		}

		public override FileEntryStreamReader OpenStreamReader()
		{
			return new SystemFileEntryStreamReader(this);
		}
	}
}
