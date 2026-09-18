using System;
using System.IO;
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection.Reader
{
	internal sealed class MetadataReader : MetadataRW
	{
		private readonly Stream stream;

		private const int bufferLength = 2048;

		private readonly byte[] buffer = new byte[2048];

		private int pos = 2048;

		internal MetadataReader(ModuleReader module, Stream stream, byte heapSizes)
			: base(module, (heapSizes & 1) != 0, (heapSizes & 2) != 0, (heapSizes & 4) != 0)
		{
			this.stream = stream;
		}

		private void FillBuffer(int needed)
		{
			int i = 2048 - pos;
			if (i != 0)
			{
				Buffer.BlockCopy(buffer, pos, buffer, 0, i);
			}
			pos = 0;
			int num;
			for (; i < needed; i += num)
			{
				num = stream.Read(buffer, i, 2048 - i);
				if (num == 0)
				{
					throw new BadImageFormatException();
				}
			}
			if (i != 2048)
			{
				Buffer.BlockCopy(buffer, 0, buffer, 2048 - i, i);
				pos = 2048 - i;
			}
		}

		internal ushort ReadUInt16()
		{
			return (ushort)ReadInt16();
		}

		internal short ReadInt16()
		{
			if (pos > 2046)
			{
				FillBuffer(2);
			}
			byte num = buffer[pos++];
			byte b = buffer[pos++];
			return (short)(num | (b << 8));
		}

		internal int ReadInt32()
		{
			if (pos > 2044)
			{
				FillBuffer(4);
			}
			byte num = buffer[pos++];
			byte b = buffer[pos++];
			byte b2 = buffer[pos++];
			byte b3 = buffer[pos++];
			return num | (b << 8) | (b2 << 16) | (b3 << 24);
		}

		private int ReadIndex(bool big)
		{
			if (big)
			{
				return ReadInt32();
			}
			return ReadUInt16();
		}

		internal int ReadStringIndex()
		{
			return ReadIndex(bigStrings);
		}

		internal int ReadGuidIndex()
		{
			return ReadIndex(bigGuids);
		}

		internal int ReadBlobIndex()
		{
			return ReadIndex(bigBlobs);
		}

		internal int ReadResolutionScope()
		{
			int num = ReadIndex(bigResolutionScope);
			switch (num & 3)
			{
			case 0:
				return (0 << 24) + (num >> 2);
			case 1:
				return (26 << 24) + (num >> 2);
			case 2:
				return (35 << 24) + (num >> 2);
			case 3:
				return (1 << 24) + (num >> 2);
			default:
				throw new BadImageFormatException();
			}
		}

		internal int ReadTypeDefOrRef()
		{
			int num = ReadIndex(bigTypeDefOrRef);
			switch (num & 3)
			{
			case 0:
				return (2 << 24) + (num >> 2);
			case 1:
				return (1 << 24) + (num >> 2);
			case 2:
				return (27 << 24) + (num >> 2);
			default:
				throw new BadImageFormatException();
			}
		}

		internal int ReadMemberRefParent()
		{
			int num = ReadIndex(bigMemberRefParent);
			switch (num & 7)
			{
			case 0:
				return (2 << 24) + (num >> 3);
			case 1:
				return (1 << 24) + (num >> 3);
			case 2:
				return (26 << 24) + (num >> 3);
			case 3:
				return (6 << 24) + (num >> 3);
			case 4:
				return (27 << 24) + (num >> 3);
			default:
				throw new BadImageFormatException();
			}
		}

		internal int ReadHasCustomAttribute()
		{
			int num = ReadIndex(bigHasCustomAttribute);
			switch (num & 0x1F)
			{
			case 0:
				return (6 << 24) + (num >> 5);
			case 1:
				return (4 << 24) + (num >> 5);
			case 2:
				return (1 << 24) + (num >> 5);
			case 3:
				return (2 << 24) + (num >> 5);
			case 4:
				return (8 << 24) + (num >> 5);
			case 5:
				return (9 << 24) + (num >> 5);
			case 6:
				return (10 << 24) + (num >> 5);
			case 7:
				return (0 << 24) + (num >> 5);
			case 8:
				throw new BadImageFormatException();
			case 9:
				return (23 << 24) + (num >> 5);
			case 10:
				return (20 << 24) + (num >> 5);
			case 11:
				return (17 << 24) + (num >> 5);
			case 12:
				return (26 << 24) + (num >> 5);
			case 13:
				return (27 << 24) + (num >> 5);
			case 14:
				return (32 << 24) + (num >> 5);
			case 15:
				return (35 << 24) + (num >> 5);
			case 16:
				return (38 << 24) + (num >> 5);
			case 17:
				return (39 << 24) + (num >> 5);
			case 18:
				return (40 << 24) + (num >> 5);
			case 19:
				return (42 << 24) + (num >> 5);
			default:
				throw new BadImageFormatException();
			}
		}

		internal int ReadCustomAttributeType()
		{
			int num = ReadIndex(bigCustomAttributeType);
			switch (num & 7)
			{
			case 2:
				return (6 << 24) + (num >> 3);
			case 3:
				return (10 << 24) + (num >> 3);
			default:
				throw new BadImageFormatException();
			}
		}

		internal int ReadMethodDefOrRef()
		{
			int num = ReadIndex(bigMethodDefOrRef);
			switch (num & 1)
			{
			case 0:
				return (6 << 24) + (num >> 1);
			case 1:
				return (10 << 24) + (num >> 1);
			default:
				throw new BadImageFormatException();
			}
		}

		internal int ReadHasConstant()
		{
			int num = ReadIndex(bigHasConstant);
			switch (num & 3)
			{
			case 0:
				return (4 << 24) + (num >> 2);
			case 1:
				return (8 << 24) + (num >> 2);
			case 2:
				return (23 << 24) + (num >> 2);
			default:
				throw new BadImageFormatException();
			}
		}

		internal int ReadHasSemantics()
		{
			int num = ReadIndex(bigHasSemantics);
			switch (num & 1)
			{
			case 0:
				return (20 << 24) + (num >> 1);
			case 1:
				return (23 << 24) + (num >> 1);
			default:
				throw new BadImageFormatException();
			}
		}

		internal int ReadHasFieldMarshal()
		{
			int num = ReadIndex(bigHasFieldMarshal);
			switch (num & 1)
			{
			case 0:
				return (4 << 24) + (num >> 1);
			case 1:
				return (8 << 24) + (num >> 1);
			default:
				throw new BadImageFormatException();
			}
		}

		internal int ReadHasDeclSecurity()
		{
			int num = ReadIndex(bigHasDeclSecurity);
			switch (num & 3)
			{
			case 0:
				return (2 << 24) + (num >> 2);
			case 1:
				return (6 << 24) + (num >> 2);
			case 2:
				return (32 << 24) + (num >> 2);
			default:
				throw new BadImageFormatException();
			}
		}

		internal int ReadTypeOrMethodDef()
		{
			int num = ReadIndex(bigTypeOrMethodDef);
			switch (num & 1)
			{
			case 0:
				return (2 << 24) + (num >> 1);
			case 1:
				return (6 << 24) + (num >> 1);
			default:
				throw new BadImageFormatException();
			}
		}

		internal int ReadMemberForwarded()
		{
			int num = ReadIndex(bigMemberForwarded);
			switch (num & 1)
			{
			case 0:
				return (4 << 24) + (num >> 1);
			case 1:
				return (6 << 24) + (num >> 1);
			default:
				throw new BadImageFormatException();
			}
		}

		internal int ReadImplementation()
		{
			int num = ReadIndex(bigImplementation);
			switch (num & 3)
			{
			case 0:
				return (38 << 24) + (num >> 2);
			case 1:
				return (35 << 24) + (num >> 2);
			case 2:
				return (39 << 24) + (num >> 2);
			default:
				throw new BadImageFormatException();
			}
		}

		internal int ReadField()
		{
			return ReadIndex(bigField);
		}

		internal int ReadMethodDef()
		{
			return ReadIndex(bigMethodDef);
		}

		internal int ReadParam()
		{
			return ReadIndex(bigParam);
		}

		internal int ReadProperty()
		{
			return ReadIndex(bigProperty);
		}

		internal int ReadEvent()
		{
			return ReadIndex(bigEvent);
		}

		internal int ReadTypeDef()
		{
			return ReadIndex(bigTypeDef) | 0x2000000;
		}

		internal int ReadGenericParam()
		{
			return ReadIndex(bigGenericParam) | 0x2A000000;
		}

		internal int ReadModuleRef()
		{
			return ReadIndex(bigModuleRef) | 0x1A000000;
		}
	}
}
