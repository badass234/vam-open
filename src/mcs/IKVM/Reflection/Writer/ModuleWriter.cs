using System;
using System.IO;
using System.Security.Cryptography;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection.Writer
{
	internal static class ModuleWriter
	{
		internal static void WriteModule(StrongNameKeyPair keyPair, byte[] publicKey, ModuleBuilder moduleBuilder, PEFileKinds fileKind, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine, ResourceSection resources, int entryPointToken)
		{
			WriteModule(keyPair, publicKey, moduleBuilder, fileKind, portableExecutableKind, imageFileMachine, resources, entryPointToken, null);
		}

		internal static void WriteModule(StrongNameKeyPair keyPair, byte[] publicKey, ModuleBuilder moduleBuilder, PEFileKinds fileKind, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine, ResourceSection resources, int entryPointToken, Stream stream)
		{
			if (stream == null)
			{
				string fullyQualifiedName = moduleBuilder.FullyQualifiedName;
				bool flag = System.Type.GetType("Mono.Runtime") != null;
				if (flag)
				{
					try
					{
						File.Delete(fullyQualifiedName);
					}
					catch
					{
					}
				}
				using (FileStream stream2 = new FileStream(fullyQualifiedName, FileMode.Create))
				{
					WriteModuleImpl(keyPair, publicKey, moduleBuilder, fileKind, portableExecutableKind, imageFileMachine, resources, entryPointToken, stream2);
				}
				if (flag)
				{
					File.SetAttributes(fullyQualifiedName, (FileAttributes)int.MinValue);
				}
			}
			else
			{
				WriteModuleImpl(keyPair, publicKey, moduleBuilder, fileKind, portableExecutableKind, imageFileMachine, resources, entryPointToken, stream);
			}
		}

		private static void WriteModuleImpl(StrongNameKeyPair keyPair, byte[] publicKey, ModuleBuilder moduleBuilder, PEFileKinds fileKind, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine, ResourceSection resources, int entryPointToken, Stream stream)
		{
			moduleBuilder.ApplyUnmanagedExports(imageFileMachine);
			moduleBuilder.FixupMethodBodyTokens();
			int num = moduleBuilder.Guids.Add(moduleBuilder.GetModuleVersionIdOrEmpty());
			moduleBuilder.ModuleTable.Add(0, moduleBuilder.Strings.Add(moduleBuilder.moduleName), num, 0, 0);
			if (moduleBuilder.UserStrings.IsEmpty)
			{
				moduleBuilder.UserStrings.Add(" ");
			}
			resources?.Finish();
			PEWriter pEWriter = new PEWriter(stream);
			pEWriter.Headers.OptionalHeader.FileAlignment = (uint)moduleBuilder.__FileAlignment;
			switch (imageFileMachine)
			{
			case ImageFileMachine.I386:
				pEWriter.Headers.FileHeader.Machine = 332;
				pEWriter.Headers.FileHeader.Characteristics |= 256;
				pEWriter.Headers.OptionalHeader.SizeOfStackReserve = moduleBuilder.GetStackReserve(1048576uL);
				break;
			case ImageFileMachine.ARM:
				pEWriter.Headers.FileHeader.Machine = 452;
				pEWriter.Headers.FileHeader.Characteristics |= 288;
				pEWriter.Headers.OptionalHeader.SizeOfStackReserve = moduleBuilder.GetStackReserve(1048576uL);
				pEWriter.Headers.OptionalHeader.SectionAlignment = 4096u;
				break;
			case ImageFileMachine.AMD64:
				pEWriter.Headers.FileHeader.Machine = 34404;
				pEWriter.Headers.FileHeader.Characteristics |= 32;
				pEWriter.Headers.FileHeader.SizeOfOptionalHeader = 240;
				pEWriter.Headers.OptionalHeader.Magic = 523;
				pEWriter.Headers.OptionalHeader.SizeOfStackReserve = moduleBuilder.GetStackReserve(4194304uL);
				pEWriter.Headers.OptionalHeader.SizeOfStackCommit = 16384uL;
				pEWriter.Headers.OptionalHeader.SizeOfHeapCommit = 8192uL;
				break;
			case ImageFileMachine.IA64:
				pEWriter.Headers.FileHeader.Machine = 512;
				pEWriter.Headers.FileHeader.Characteristics |= 32;
				pEWriter.Headers.FileHeader.SizeOfOptionalHeader = 240;
				pEWriter.Headers.OptionalHeader.Magic = 523;
				pEWriter.Headers.OptionalHeader.SizeOfStackReserve = moduleBuilder.GetStackReserve(4194304uL);
				pEWriter.Headers.OptionalHeader.SizeOfStackCommit = 16384uL;
				pEWriter.Headers.OptionalHeader.SizeOfHeapCommit = 8192uL;
				break;
			default:
				throw new ArgumentOutOfRangeException("imageFileMachine");
			}
			if (fileKind == PEFileKinds.Dll)
			{
				pEWriter.Headers.FileHeader.Characteristics |= 8192;
			}
			if (fileKind == PEFileKinds.WindowApplication)
			{
				pEWriter.Headers.OptionalHeader.Subsystem = 2;
			}
			else
			{
				pEWriter.Headers.OptionalHeader.Subsystem = 3;
			}
			pEWriter.Headers.OptionalHeader.DllCharacteristics = (ushort)moduleBuilder.__DllCharacteristics;
			CliHeader cliHeader = new CliHeader();
			cliHeader.Cb = 72u;
			cliHeader.MajorRuntimeVersion = 2;
			cliHeader.MinorRuntimeVersion = (ushort)((moduleBuilder.MDStreamVersion >= 131072) ? 5 : 0);
			if ((portableExecutableKind & PortableExecutableKinds.ILOnly) != PortableExecutableKinds.NotAPortableExecutableImage)
			{
				cliHeader.Flags |= 1u;
			}
			if ((portableExecutableKind & PortableExecutableKinds.Required32Bit) != PortableExecutableKinds.NotAPortableExecutableImage)
			{
				cliHeader.Flags |= 2u;
			}
			if ((portableExecutableKind & PortableExecutableKinds.Preferred32Bit) != PortableExecutableKinds.NotAPortableExecutableImage)
			{
				cliHeader.Flags |= 131074u;
			}
			if (keyPair != null)
			{
				cliHeader.Flags |= 8u;
			}
			if (ModuleBuilder.IsPseudoToken(entryPointToken))
			{
				entryPointToken = moduleBuilder.ResolvePseudoToken(entryPointToken);
			}
			cliHeader.EntryPointToken = (uint)entryPointToken;
			moduleBuilder.Strings.Freeze();
			moduleBuilder.UserStrings.Freeze();
			moduleBuilder.Guids.Freeze();
			moduleBuilder.Blobs.Freeze();
			MetadataWriter metadataWriter = new MetadataWriter(moduleBuilder, stream);
			moduleBuilder.Tables.Freeze(metadataWriter);
			TextSection textSection = new TextSection(pEWriter, cliHeader, moduleBuilder, ComputeStrongNameSignatureLength(publicKey));
			if (textSection.ExportDirectoryLength != 0)
			{
				pEWriter.Headers.OptionalHeader.DataDirectory[0].VirtualAddress = textSection.ExportDirectoryRVA;
				pEWriter.Headers.OptionalHeader.DataDirectory[0].Size = textSection.ExportDirectoryLength;
			}
			if (textSection.ImportDirectoryLength != 0)
			{
				pEWriter.Headers.OptionalHeader.DataDirectory[1].VirtualAddress = textSection.ImportDirectoryRVA;
				pEWriter.Headers.OptionalHeader.DataDirectory[1].Size = textSection.ImportDirectoryLength;
			}
			if (textSection.ImportAddressTableLength != 0)
			{
				pEWriter.Headers.OptionalHeader.DataDirectory[12].VirtualAddress = textSection.ImportAddressTableRVA;
				pEWriter.Headers.OptionalHeader.DataDirectory[12].Size = textSection.ImportAddressTableLength;
			}
			pEWriter.Headers.OptionalHeader.DataDirectory[14].VirtualAddress = textSection.ComDescriptorRVA;
			pEWriter.Headers.OptionalHeader.DataDirectory[14].Size = textSection.ComDescriptorLength;
			if (textSection.DebugDirectoryLength != 0)
			{
				pEWriter.Headers.OptionalHeader.DataDirectory[6].VirtualAddress = textSection.DebugDirectoryRVA;
				pEWriter.Headers.OptionalHeader.DataDirectory[6].Size = textSection.DebugDirectoryLength;
			}
			pEWriter.Headers.FileHeader.TimeDateStamp = moduleBuilder.GetTimeDateStamp();
			pEWriter.Headers.FileHeader.NumberOfSections = 2;
			if (moduleBuilder.initializedData.Length != 0)
			{
				pEWriter.Headers.FileHeader.NumberOfSections++;
			}
			if (resources != null)
			{
				pEWriter.Headers.FileHeader.NumberOfSections++;
			}
			SectionHeader sectionHeader = new SectionHeader();
			sectionHeader.Name = ".text";
			sectionHeader.VirtualAddress = textSection.BaseRVA;
			sectionHeader.VirtualSize = (uint)textSection.Length;
			sectionHeader.PointerToRawData = textSection.PointerToRawData;
			sectionHeader.SizeOfRawData = pEWriter.ToFileAlignment((uint)textSection.Length);
			sectionHeader.Characteristics = 1610612768u;
			SectionHeader sectionHeader2 = new SectionHeader();
			sectionHeader2.Name = ".sdata";
			sectionHeader2.VirtualAddress = sectionHeader.VirtualAddress + pEWriter.ToSectionAlignment(sectionHeader.VirtualSize);
			sectionHeader2.VirtualSize = (uint)moduleBuilder.initializedData.Length;
			sectionHeader2.PointerToRawData = sectionHeader.PointerToRawData + sectionHeader.SizeOfRawData;
			sectionHeader2.SizeOfRawData = pEWriter.ToFileAlignment((uint)moduleBuilder.initializedData.Length);
			sectionHeader2.Characteristics = 3221225536u;
			SectionHeader sectionHeader3 = new SectionHeader();
			sectionHeader3.Name = ".rsrc";
			sectionHeader3.VirtualAddress = sectionHeader2.VirtualAddress + pEWriter.ToSectionAlignment(sectionHeader2.VirtualSize);
			sectionHeader3.PointerToRawData = sectionHeader2.PointerToRawData + sectionHeader2.SizeOfRawData;
			sectionHeader3.VirtualSize = (uint)(resources?.Length ?? 0);
			sectionHeader3.SizeOfRawData = pEWriter.ToFileAlignment(sectionHeader3.VirtualSize);
			sectionHeader3.Characteristics = 1073741888u;
			if (sectionHeader3.SizeOfRawData != 0)
			{
				pEWriter.Headers.OptionalHeader.DataDirectory[2].VirtualAddress = sectionHeader3.VirtualAddress;
				pEWriter.Headers.OptionalHeader.DataDirectory[2].Size = sectionHeader3.VirtualSize;
			}
			SectionHeader sectionHeader4 = new SectionHeader();
			sectionHeader4.Name = ".reloc";
			sectionHeader4.VirtualAddress = sectionHeader3.VirtualAddress + pEWriter.ToSectionAlignment(sectionHeader3.VirtualSize);
			sectionHeader4.VirtualSize = textSection.PackRelocations();
			sectionHeader4.PointerToRawData = sectionHeader3.PointerToRawData + sectionHeader3.SizeOfRawData;
			sectionHeader4.SizeOfRawData = pEWriter.ToFileAlignment(sectionHeader4.VirtualSize);
			sectionHeader4.Characteristics = 1107296320u;
			if (sectionHeader4.SizeOfRawData != 0)
			{
				pEWriter.Headers.OptionalHeader.DataDirectory[5].VirtualAddress = sectionHeader4.VirtualAddress;
				pEWriter.Headers.OptionalHeader.DataDirectory[5].Size = sectionHeader4.VirtualSize;
			}
			pEWriter.Headers.OptionalHeader.SizeOfCode = sectionHeader.SizeOfRawData;
			pEWriter.Headers.OptionalHeader.SizeOfInitializedData = sectionHeader2.SizeOfRawData + sectionHeader3.SizeOfRawData + sectionHeader4.SizeOfRawData;
			pEWriter.Headers.OptionalHeader.SizeOfUninitializedData = 0u;
			pEWriter.Headers.OptionalHeader.SizeOfImage = sectionHeader4.VirtualAddress + pEWriter.ToSectionAlignment(sectionHeader4.VirtualSize);
			pEWriter.Headers.OptionalHeader.SizeOfHeaders = sectionHeader.PointerToRawData;
			pEWriter.Headers.OptionalHeader.BaseOfCode = textSection.BaseRVA;
			pEWriter.Headers.OptionalHeader.BaseOfData = sectionHeader2.VirtualAddress;
			pEWriter.Headers.OptionalHeader.ImageBase = (ulong)moduleBuilder.__ImageBase;
			if (imageFileMachine == ImageFileMachine.IA64)
			{
				pEWriter.Headers.OptionalHeader.AddressOfEntryPoint = textSection.StartupStubRVA + 32;
			}
			else
			{
				pEWriter.Headers.OptionalHeader.AddressOfEntryPoint = textSection.StartupStubRVA + pEWriter.Thumb;
			}
			pEWriter.WritePEHeaders();
			pEWriter.WriteSectionHeader(sectionHeader);
			if (sectionHeader2.SizeOfRawData != 0)
			{
				pEWriter.WriteSectionHeader(sectionHeader2);
			}
			if (sectionHeader3.SizeOfRawData != 0)
			{
				pEWriter.WriteSectionHeader(sectionHeader3);
			}
			if (sectionHeader4.SizeOfRawData != 0)
			{
				pEWriter.WriteSectionHeader(sectionHeader4);
			}
			stream.Seek(sectionHeader.PointerToRawData, SeekOrigin.Begin);
			int guidHeapOffset;
			textSection.Write(metadataWriter, sectionHeader2.VirtualAddress, out guidHeapOffset);
			if (sectionHeader2.SizeOfRawData != 0)
			{
				stream.Seek(sectionHeader2.PointerToRawData, SeekOrigin.Begin);
				metadataWriter.Write(moduleBuilder.initializedData);
			}
			if (sectionHeader3.SizeOfRawData != 0)
			{
				stream.Seek(sectionHeader3.PointerToRawData, SeekOrigin.Begin);
				resources.Write(metadataWriter, sectionHeader3.VirtualAddress);
			}
			if (sectionHeader4.SizeOfRawData != 0)
			{
				stream.Seek(sectionHeader4.PointerToRawData, SeekOrigin.Begin);
				textSection.WriteRelocations(metadataWriter);
			}
			stream.SetLength(sectionHeader4.PointerToRawData + sectionHeader4.SizeOfRawData);
			if (moduleBuilder.universe.Deterministic && moduleBuilder.GetModuleVersionIdOrEmpty() == Guid.Empty)
			{
				Guid guid = GenerateModuleVersionId(stream);
				stream.Position = guidHeapOffset + (num - 1) * 16;
				stream.Write(guid.ToByteArray(), 0, 16);
				moduleBuilder.__SetModuleVersionId(guid);
			}
			if (keyPair != null)
			{
				StrongName(stream, keyPair, pEWriter.HeaderSize, sectionHeader.PointerToRawData, textSection.StrongNameSignatureRVA - sectionHeader.VirtualAddress + sectionHeader.PointerToRawData, textSection.StrongNameSignatureLength);
			}
		}

		private static int ComputeStrongNameSignatureLength(byte[] publicKey)
		{
			if (publicKey == null)
			{
				return 0;
			}
			if (publicKey.Length == 16)
			{
				return 128;
			}
			return publicKey.Length - 32;
		}

		private static void StrongName(Stream stream, StrongNameKeyPair keyPair, uint headerLength, uint textSectionFileOffset, uint strongNameSignatureFileOffset, uint strongNameSignatureLength)
		{
			SHA1Managed sHA1Managed = new SHA1Managed();
			using (CryptoStream cs = new CryptoStream(Stream.Null, sHA1Managed, CryptoStreamMode.Write))
			{
				stream.Seek(0L, SeekOrigin.Begin);
				byte[] buf = new byte[8192];
				HashChunk(stream, cs, buf, (int)headerLength);
				stream.Seek(textSectionFileOffset, SeekOrigin.Begin);
				HashChunk(stream, cs, buf, (int)(strongNameSignatureFileOffset - textSectionFileOffset));
				stream.Seek(strongNameSignatureLength, SeekOrigin.Current);
				HashChunk(stream, cs, buf, (int)(stream.Length - (strongNameSignatureFileOffset + strongNameSignatureLength)));
			}
			using (RSA key = keyPair.CreateRSA())
			{
				byte[] array = new RSAPKCS1SignatureFormatter(key).CreateSignature(sHA1Managed);
				Array.Reverse(array);
				if (array.Length != strongNameSignatureLength)
				{
					throw new InvalidOperationException("Signature length mismatch");
				}
				stream.Seek(strongNameSignatureFileOffset, SeekOrigin.Begin);
				stream.Write(array, 0, array.Length);
			}
			stream.Seek(0L, SeekOrigin.Begin);
			int num = (int)stream.Length / 4;
			BinaryReader binaryReader = new BinaryReader(stream);
			long num2 = 0L;
			for (int i = 0; i < num; i++)
			{
				num2 += binaryReader.ReadUInt32();
				int num3 = (int)(num2 >> 32);
				num2 &= 0xFFFFFFFFu;
				num2 += num3;
			}
			while (num2 >> 16 != 0L)
			{
				num2 = (num2 & 0xFFFF) + (num2 >> 16);
			}
			num2 += stream.Length;
			ByteBuffer byteBuffer = new ByteBuffer(4);
			byteBuffer.Write((int)num2);
			stream.Seek(216L, SeekOrigin.Begin);
			byteBuffer.WriteTo(stream);
		}

		internal static void HashChunk(Stream stream, CryptoStream cs, byte[] buf, int length)
		{
			while (length > 0)
			{
				int num = stream.Read(buf, 0, Math.Min(buf.Length, length));
				cs.Write(buf, 0, num);
				length -= num;
			}
		}

		private static Guid GenerateModuleVersionId(Stream stream)
		{
			SHA1Managed sHA1Managed = new SHA1Managed();
			using (CryptoStream cs = new CryptoStream(Stream.Null, sHA1Managed, CryptoStreamMode.Write))
			{
				stream.Seek(0L, SeekOrigin.Begin);
				byte[] buf = new byte[8192];
				HashChunk(stream, cs, buf, (int)stream.Length);
			}
			byte[] array = new byte[16];
			Buffer.BlockCopy(sHA1Managed.Hash, 0, array, 0, array.Length);
			array[7] &= 15;
			array[7] |= 64;
			array[8] &= 63;
			array[8] |= 128;
			return new Guid(array);
		}
	}
}
