using IKVM.Reflection.Reader;

namespace IKVM.Reflection.Writer
{
	internal sealed class BlobHeap : SimpleHeap
	{
		private struct Key
		{
			internal Key[] next;

			internal int len;

			internal int hash;

			internal int offset;
		}

		private Key[] map = new Key[8179];

		private readonly ByteBuffer buf = new ByteBuffer(32);

		internal bool IsEmpty => buf.Position == 1;

		internal BlobHeap()
		{
			buf.Write((byte)0);
		}

		internal int Add(ByteBuffer bb)
		{
			int length = bb.Length;
			if (length == 0)
			{
				return 0;
			}
			int compressedUIntLength = MetadataWriter.GetCompressedUIntLength(length);
			int num = bb.Hash();
			int i = (num & 0x7FFFFFFF) % map.Length;
			Key[] next = map;
			int num2 = i;
			for (; next[i].offset != 0; i++)
			{
				if (next[i].hash == num && next[i].len == length && buf.Match(next[i].offset + compressedUIntLength, bb, 0, length))
				{
					return next[i].offset;
				}
				if (i == num2)
				{
					if (next[i].next == null)
					{
						next[i].next = new Key[4];
						next = next[i].next;
						i = 0;
						break;
					}
					next = next[i].next;
					i = -1;
					num2 = next.Length - 1;
				}
			}
			int position = buf.Position;
			buf.WriteCompressedUInt(length);
			buf.Write(bb);
			next[i].len = length;
			next[i].hash = num;
			next[i].offset = position;
			return position;
		}

		protected override int GetLength()
		{
			return buf.Position;
		}

		protected override void WriteImpl(MetadataWriter mw)
		{
			mw.Write(buf);
		}

		internal ByteReader GetBlob(int blobIndex)
		{
			return buf.GetBlob(blobIndex);
		}
	}
}
