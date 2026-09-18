using System.Collections.Generic;

namespace IKVM.Reflection.Writer
{
	internal sealed class ResourceDirectoryEntry
	{
		internal readonly OrdinalOrName OrdinalOrName;

		internal ByteBuffer Data;

		private int namedEntries;

		private readonly List<ResourceDirectoryEntry> entries = new List<ResourceDirectoryEntry>();

		internal ResourceDirectoryEntry this[OrdinalOrName id]
		{
			get
			{
				foreach (ResourceDirectoryEntry entry in entries)
				{
					if (entry.OrdinalOrName.IsEqual(id))
					{
						return entry;
					}
				}
				ResourceDirectoryEntry resourceDirectoryEntry = new ResourceDirectoryEntry(id);
				if (id.Name == null)
				{
					for (int i = namedEntries; i < entries.Count; i++)
					{
						if (entries[i].OrdinalOrName.IsGreaterThan(id))
						{
							entries.Insert(i, resourceDirectoryEntry);
							return resourceDirectoryEntry;
						}
					}
					entries.Add(resourceDirectoryEntry);
					return resourceDirectoryEntry;
				}
				for (int j = 0; j < namedEntries; j++)
				{
					if (entries[j].OrdinalOrName.IsGreaterThan(id))
					{
						entries.Insert(j, resourceDirectoryEntry);
						namedEntries++;
						return resourceDirectoryEntry;
					}
				}
				entries.Insert(namedEntries++, resourceDirectoryEntry);
				return resourceDirectoryEntry;
			}
		}

		private int DirectoryLength
		{
			get
			{
				if (Data != null)
				{
					return 16;
				}
				int num = 16 + entries.Count * 8;
				foreach (ResourceDirectoryEntry entry in entries)
				{
					num += entry.DirectoryLength;
				}
				return num;
			}
		}

		internal ResourceDirectoryEntry(OrdinalOrName id)
		{
			OrdinalOrName = id;
		}

		internal void Write(ByteBuffer bb, List<int> linkOffsets)
		{
			if (entries.Count != 0)
			{
				int stringTableOffset = DirectoryLength;
				Dictionary<string, int> strings = new Dictionary<string, int>();
				ByteBuffer byteBuffer = new ByteBuffer(16);
				int offset = 16 + entries.Count * 8;
				for (int i = 0; i < 3; i++)
				{
					Write(bb, i, 0, ref offset, strings, ref stringTableOffset, byteBuffer);
				}
				byteBuffer.Align(4);
				offset += byteBuffer.Length;
				WriteResourceDataEntries(bb, linkOffsets, ref offset);
				bb.Write(byteBuffer);
				WriteData(bb);
			}
		}

		private void WriteResourceDataEntries(ByteBuffer bb, List<int> linkOffsets, ref int offset)
		{
			foreach (ResourceDirectoryEntry entry in entries)
			{
				if (entry.Data != null)
				{
					linkOffsets.Add(bb.Position);
					bb.Write(offset);
					bb.Write(entry.Data.Length);
					bb.Write(0);
					bb.Write(0);
					offset += (entry.Data.Length + 3) & -4;
				}
				else
				{
					entry.WriteResourceDataEntries(bb, linkOffsets, ref offset);
				}
			}
		}

		private void WriteData(ByteBuffer bb)
		{
			foreach (ResourceDirectoryEntry entry in entries)
			{
				if (entry.Data != null)
				{
					bb.Write(entry.Data);
					bb.Align(4);
				}
				else
				{
					entry.WriteData(bb);
				}
			}
		}

		private void Write(ByteBuffer bb, int writeDepth, int currentDepth, ref int offset, Dictionary<string, int> strings, ref int stringTableOffset, ByteBuffer stringTable)
		{
			if (currentDepth == writeDepth)
			{
				bb.Write(0);
				bb.Write(0);
				bb.Write(0);
				bb.Write((ushort)namedEntries);
				bb.Write((ushort)(entries.Count - namedEntries));
			}
			foreach (ResourceDirectoryEntry entry in entries)
			{
				if (currentDepth == writeDepth)
				{
					entry.WriteEntry(bb, ref offset, strings, ref stringTableOffset, stringTable);
				}
				else
				{
					entry.Write(bb, writeDepth, currentDepth + 1, ref offset, strings, ref stringTableOffset, stringTable);
				}
			}
		}

		private void WriteEntry(ByteBuffer bb, ref int offset, Dictionary<string, int> strings, ref int stringTableOffset, ByteBuffer stringTable)
		{
			WriteNameOrOrdinal(bb, OrdinalOrName, strings, ref stringTableOffset, stringTable);
			if (Data == null)
			{
				bb.Write((uint)(int.MinValue | offset));
			}
			else
			{
				bb.Write(offset);
			}
			offset += 16 + entries.Count * 8;
		}

		private static void WriteNameOrOrdinal(ByteBuffer bb, OrdinalOrName id, Dictionary<string, int> strings, ref int stringTableOffset, ByteBuffer stringTable)
		{
			if (id.Name == null)
			{
				bb.Write((int)id.Ordinal);
				return;
			}
			int value;
			if (!strings.TryGetValue(id.Name, out value))
			{
				value = stringTableOffset;
				strings.Add(id.Name, value);
				stringTableOffset += id.Name.Length * 2 + 2;
				stringTable.Write((ushort)id.Name.Length);
				string name = id.Name;
				foreach (char c in name)
				{
					stringTable.Write((short)c);
				}
			}
			bb.Write((uint)(int.MinValue | value));
		}
	}
}
