using System.Collections.Generic;

namespace IKVM.Reflection.Writer
{
	internal sealed class UserStringHeap : SimpleHeap
	{
		private List<string> list = new List<string>();

		private Dictionary<string, int> strings = new Dictionary<string, int>();

		private int nextOffset;

		internal bool IsEmpty => nextOffset == 1;

		internal UserStringHeap()
		{
			nextOffset = 1;
		}

		internal int Add(string str)
		{
			int value;
			if (!strings.TryGetValue(str, out value))
			{
				int num = str.Length * 2 + 1 + MetadataWriter.GetCompressedUIntLength(str.Length * 2 + 1);
				if (nextOffset + num > 16777215)
				{
					throw new FileFormatLimitationExceededException("No logical space left to create more user strings.", -2146233960);
				}
				value = nextOffset;
				nextOffset += num;
				list.Add(str);
				strings.Add(str, value);
			}
			return value;
		}

		protected override int GetLength()
		{
			return nextOffset;
		}

		protected override void WriteImpl(MetadataWriter mw)
		{
			mw.Write((byte)0);
			foreach (string item in list)
			{
				mw.WriteCompressedUInt(item.Length * 2 + 1);
				byte b = 0;
				string text = item;
				foreach (char c in text)
				{
					mw.Write(c);
					if (b == 0 && (c < ' ' || c > '~') && (c > '~' || (c >= '\u0001' && c <= '\b') || (c >= '\u000e' && c <= '\u001f') || c == '\'' || c == '-'))
					{
						b = 1;
					}
				}
				mw.Write(b);
			}
		}
	}
}
