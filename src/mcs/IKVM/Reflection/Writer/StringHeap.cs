using System.Collections.Generic;
using System.Text;

namespace IKVM.Reflection.Writer
{
	internal sealed class StringHeap : SimpleHeap
	{
		private List<string> list = new List<string>();

		private Dictionary<string, int> strings = new Dictionary<string, int>();

		private int nextOffset;

		internal StringHeap()
		{
			Add("");
		}

		internal int Add(string str)
		{
			int value;
			if (!strings.TryGetValue(str, out value))
			{
				value = nextOffset;
				nextOffset += Encoding.UTF8.GetByteCount(str) + 1;
				list.Add(str);
				strings.Add(str, value);
			}
			return value;
		}

		internal string Find(int index)
		{
			foreach (KeyValuePair<string, int> @string in strings)
			{
				if (@string.Value == index)
				{
					return @string.Key;
				}
			}
			return null;
		}

		protected override int GetLength()
		{
			return nextOffset;
		}

		protected override void WriteImpl(MetadataWriter mw)
		{
			foreach (string item in list)
			{
				mw.Write(Encoding.UTF8.GetBytes(item));
				mw.Write((byte)0);
			}
		}
	}
}
