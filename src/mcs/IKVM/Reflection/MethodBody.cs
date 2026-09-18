using System;
using System.Collections.Generic;
using System.IO;
using IKVM.Reflection.Reader;

namespace IKVM.Reflection
{
	public sealed class MethodBody
	{
		private readonly IList<ExceptionHandlingClause> exceptionClauses;

		private readonly IList<LocalVariableInfo> locals;

		private readonly bool initLocals;

		private readonly int maxStack;

		private readonly int localVarSigTok;

		private byte[] body;

		public IList<ExceptionHandlingClause> ExceptionHandlingClauses => exceptionClauses;

		public bool InitLocals => initLocals;

		public IList<LocalVariableInfo> LocalVariables => locals;

		public int LocalSignatureMetadataToken => localVarSigTok;

		public int MaxStackSize => maxStack;

		internal MethodBody(ModuleReader module, int rva, IGenericContext context)
		{
			List<ExceptionHandlingClause> list = new List<ExceptionHandlingClause>();
			List<LocalVariableInfo> list2 = new List<LocalVariableInfo>();
			Stream stream = module.GetStream();
			module.SeekRVA(rva);
			BinaryReader binaryReader = new BinaryReader(stream);
			byte b = binaryReader.ReadByte();
			if ((b & 3) == 2)
			{
				initLocals = true;
				body = binaryReader.ReadBytes(b >> 2);
				maxStack = 8;
			}
			else
			{
				if ((b & 3) != 3)
				{
					throw new BadImageFormatException();
				}
				initLocals = (b & 0x10) != 0;
				if ((short)(b | (binaryReader.ReadByte() << 8)) >> 12 != 3)
				{
					throw new BadImageFormatException("Fat format method header size should be 3");
				}
				maxStack = binaryReader.ReadUInt16();
				int count = binaryReader.ReadInt32();
				localVarSigTok = binaryReader.ReadInt32();
				body = binaryReader.ReadBytes(count);
				if ((b & 8) != 0)
				{
					stream.Position = (stream.Position + 3) & -4;
					int num = binaryReader.ReadInt32();
					if ((num & 0x80) != 0 || (num & 1) == 0)
					{
						throw new NotImplementedException();
					}
					if ((num & 0x40) != 0)
					{
						int num2 = ComputeExceptionCount((num >> 8) & 0xFFFFFF, 24);
						for (int i = 0; i < num2; i++)
						{
							int flags = binaryReader.ReadInt32();
							int tryOffset = binaryReader.ReadInt32();
							int tryLength = binaryReader.ReadInt32();
							int handlerOffset = binaryReader.ReadInt32();
							int handlerLength = binaryReader.ReadInt32();
							int classTokenOrfilterOffset = binaryReader.ReadInt32();
							list.Add(new ExceptionHandlingClause(module, flags, tryOffset, tryLength, handlerOffset, handlerLength, classTokenOrfilterOffset, context));
						}
					}
					else
					{
						int num3 = ComputeExceptionCount((num >> 8) & 0xFF, 12);
						for (int j = 0; j < num3; j++)
						{
							int flags2 = binaryReader.ReadUInt16();
							int tryOffset2 = binaryReader.ReadUInt16();
							int tryLength2 = binaryReader.ReadByte();
							int handlerOffset2 = binaryReader.ReadUInt16();
							int handlerLength2 = binaryReader.ReadByte();
							int classTokenOrfilterOffset2 = binaryReader.ReadInt32();
							list.Add(new ExceptionHandlingClause(module, flags2, tryOffset2, tryLength2, handlerOffset2, handlerLength2, classTokenOrfilterOffset2, context));
						}
					}
				}
				if (localVarSigTok != 0)
				{
					ByteReader standAloneSig = module.GetStandAloneSig((localVarSigTok & 0xFFFFFF) - 1);
					Signature.ReadLocalVarSig(module, standAloneSig, context, list2);
				}
			}
			exceptionClauses = list.AsReadOnly();
			locals = list2.AsReadOnly();
		}

		private static int ComputeExceptionCount(int size, int itemLength)
		{
			return size / itemLength;
		}

		public byte[] GetILAsByteArray()
		{
			return body;
		}
	}
}
