using System.IO;
using System.Text;

namespace IKVM.Reflection.Reader
{
	internal sealed class StreamHeader
	{
		internal uint Offset;

		internal uint Size;

		internal string Name;

		internal void Read(BinaryReader br)
		{
			Offset = br.ReadUInt32();
			Size = br.ReadUInt32();
			byte[] array = new byte[32];
			int num = 0;
			byte b;
			while ((b = br.ReadByte()) != 0)
			{
				array[num++] = b;
			}
			Name = Encoding.UTF8.GetString(array, 0, num);
			int num2 = -1 + ((num + 4) & -4) - num;
			br.BaseStream.Seek(num2, SeekOrigin.Current);
		}
	}
}
