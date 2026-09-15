using System.IO;

namespace MVR.FileManagement
{
	public class VarFileEntryStreamReader : FileEntryStreamReader
	{
		protected VarFileEntryStream VarStream;

		public VarFileEntryStreamReader(VarFileEntry entry)
			: base(entry)
		{
			VarStream = new VarFileEntryStream(entry);
			StreamReader = new StreamReader(VarStream.Stream);
		}

		public override void Dispose()
		{
			base.Dispose();
			if (VarStream != null)
			{
				VarStream.Dispose();
				VarStream = null;
			}
		}
	}
}
