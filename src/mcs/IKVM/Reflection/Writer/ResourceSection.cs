using System;
using System.Collections.Generic;
using System.IO;
using IKVM.Reflection.Reader;

namespace IKVM.Reflection.Writer
{
	internal sealed class ResourceSection
	{
		private const int RT_ICON = 3;

		private const int RT_GROUP_ICON = 14;

		private const int RT_VERSION = 16;

		private const int RT_MANIFEST = 24;

		private ResourceDirectoryEntry root = new ResourceDirectoryEntry(new OrdinalOrName("root"));

		private ByteBuffer bb;

		private List<int> linkOffsets;

		internal int Length => bb.Length;

		internal void AddVersionInfo(ByteBuffer versionInfo)
		{
			root[new OrdinalOrName(16)][new OrdinalOrName(1)][new OrdinalOrName(0)].Data = versionInfo;
		}

		internal void AddIcon(byte[] iconFile)
		{
			BinaryReader binaryReader = new BinaryReader(new MemoryStream(iconFile));
			ushort num = binaryReader.ReadUInt16();
			ushort num2 = binaryReader.ReadUInt16();
			ushort num3 = binaryReader.ReadUInt16();
			if (num != 0 || num2 != 1)
			{
				throw new ArgumentException("The supplied byte array is not a valid .ico file.");
			}
			ByteBuffer byteBuffer = new ByteBuffer(6 + 14 * num3);
			byteBuffer.Write(num);
			byteBuffer.Write(num2);
			byteBuffer.Write(num3);
			for (int i = 0; i < num3; i++)
			{
				byte value = binaryReader.ReadByte();
				byte value2 = binaryReader.ReadByte();
				byte value3 = binaryReader.ReadByte();
				byte value4 = binaryReader.ReadByte();
				ushort value5 = binaryReader.ReadUInt16();
				ushort value6 = binaryReader.ReadUInt16();
				uint num4 = binaryReader.ReadUInt32();
				uint srcOffset = binaryReader.ReadUInt32();
				ushort value7 = (ushort)(2 + i);
				byteBuffer.Write(value);
				byteBuffer.Write(value2);
				byteBuffer.Write(value3);
				byteBuffer.Write(value4);
				byteBuffer.Write(value5);
				byteBuffer.Write(value6);
				byteBuffer.Write(num4);
				byteBuffer.Write(value7);
				byte[] array = new byte[num4];
				Buffer.BlockCopy(iconFile, (int)srcOffset, array, 0, array.Length);
				root[new OrdinalOrName(3)][new OrdinalOrName(value7)][new OrdinalOrName(0)].Data = ByteBuffer.Wrap(array);
			}
			root[new OrdinalOrName(14)][new OrdinalOrName(32512)][new OrdinalOrName(0)].Data = byteBuffer;
		}

		internal void AddManifest(byte[] manifest, ushort resourceID)
		{
			root[new OrdinalOrName(24)][new OrdinalOrName(resourceID)][new OrdinalOrName(0)].Data = ByteBuffer.Wrap(manifest);
		}

		internal void ExtractResources(byte[] buf)
		{
			ByteReader byteReader = new ByteReader(buf, 0, buf.Length);
			while (byteReader.Length >= 32)
			{
				byteReader.Align(4);
				RESOURCEHEADER rESOURCEHEADER = new RESOURCEHEADER(byteReader);
				if (rESOURCEHEADER.DataSize != 0)
				{
					root[rESOURCEHEADER.TYPE][rESOURCEHEADER.NAME][new OrdinalOrName(rESOURCEHEADER.LanguageId)].Data = ByteBuffer.Wrap(byteReader.ReadBytes(rESOURCEHEADER.DataSize));
				}
			}
		}

		internal void Finish()
		{
			if (bb != null)
			{
				throw new InvalidOperationException();
			}
			bb = new ByteBuffer(1024);
			linkOffsets = new List<int>();
			root.Write(bb, linkOffsets);
			root = null;
		}

		internal void Write(MetadataWriter mw, uint rva)
		{
			foreach (int linkOffset in linkOffsets)
			{
				bb.Position = linkOffset;
				bb.Write(bb.GetInt32AtCurrentPosition() + (int)rva);
			}
			mw.Write(bb);
		}
	}
}
