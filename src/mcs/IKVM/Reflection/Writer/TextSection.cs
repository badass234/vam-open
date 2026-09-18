using System;
using System.Collections.Generic;
using System.Diagnostics;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Impl;
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection.Writer
{
	internal sealed class TextSection
	{
		private sealed class ExportTables
		{
			private readonly TextSection text;

			internal readonly uint entries;

			internal readonly uint ordinalBase;

			internal readonly uint nameCount;

			internal readonly uint namesLength;

			internal readonly uint exportAddressTableRVA;

			internal readonly uint exportNamePointerTableRVA;

			internal readonly uint exportOrdinalTableRVA;

			internal readonly uint namesRVA;

			internal readonly uint stubsRVA;

			private readonly uint stubLength;

			internal uint Length => (uint)((int)stubsRVA + (int)stubLength * text.moduleBuilder.unmanagedExports.Count) - text.ExportTablesRVA;

			internal ExportTables(TextSection text)
			{
				this.text = text;
				ordinalBase = GetOrdinalBase(out entries);
				namesLength = GetExportNamesLength(out nameCount);
				exportAddressTableRVA = text.ExportTablesRVA;
				exportNamePointerTableRVA = exportAddressTableRVA + 4 * entries;
				exportOrdinalTableRVA = exportNamePointerTableRVA + 4 * nameCount;
				namesRVA = exportOrdinalTableRVA + 2 * nameCount;
				stubsRVA = (namesRVA + namesLength + 15) & 0xFFFFFFF0u;
				switch (text.peWriter.Headers.FileHeader.Machine)
				{
				case 332:
					stubLength = 8u;
					break;
				case 452:
				case 34404:
					stubLength = 16u;
					break;
				default:
					throw new NotSupportedException();
				}
			}

			private uint GetOrdinalBase(out uint entries)
			{
				uint num = uint.MaxValue;
				uint num2 = 0u;
				foreach (UnmanagedExport unmanagedExport in text.moduleBuilder.unmanagedExports)
				{
					uint ordinal = (uint)unmanagedExport.ordinal;
					num = Math.Min(num, ordinal);
					num2 = Math.Max(num2, ordinal);
				}
				entries = 1 + (num2 - num);
				return num;
			}

			private uint GetExportNamesLength(out uint nameCount)
			{
				nameCount = 0u;
				uint num = (uint)(text.moduleBuilder.fileName.Length + 1);
				foreach (UnmanagedExport unmanagedExport in text.moduleBuilder.unmanagedExports)
				{
					if (unmanagedExport.name != null)
					{
						nameCount++;
						num += (uint)(unmanagedExport.name.Length + 1);
					}
				}
				return num;
			}

			internal void Write(MetadataWriter mw, uint sdataRVA)
			{
				text.moduleBuilder.unmanagedExports.Sort(CompareUnmanagedExportOrdinals);
				int i = 0;
				int num = 0;
				for (; i < entries; i++)
				{
					if (text.moduleBuilder.unmanagedExports[num].ordinal == i + ordinalBase)
					{
						mw.Write(text.peWriter.Thumb + stubsRVA + (uint)(num * (int)stubLength));
						num++;
					}
					else
					{
						mw.Write(0);
					}
				}
				text.moduleBuilder.unmanagedExports.Sort(CompareUnmanagedExportNames);
				uint num2 = (uint)(text.moduleBuilder.fileName.Length + 1);
				foreach (UnmanagedExport unmanagedExport in text.moduleBuilder.unmanagedExports)
				{
					if (unmanagedExport.name != null)
					{
						mw.Write(namesRVA + num2);
						num2 += (uint)(unmanagedExport.name.Length + 1);
					}
				}
				foreach (UnmanagedExport unmanagedExport2 in text.moduleBuilder.unmanagedExports)
				{
					if (unmanagedExport2.name != null)
					{
						mw.Write((ushort)(unmanagedExport2.ordinal - ordinalBase));
					}
				}
				mw.WriteAsciiz(text.moduleBuilder.fileName);
				foreach (UnmanagedExport unmanagedExport3 in text.moduleBuilder.unmanagedExports)
				{
					if (unmanagedExport3.name != null)
					{
						mw.WriteAsciiz(unmanagedExport3.name);
					}
				}
				for (int num3 = (int)(stubsRVA - (namesRVA + namesLength)); num3 > 0; num3--)
				{
					mw.Write((byte)0);
				}
				text.moduleBuilder.unmanagedExports.Sort(CompareUnmanagedExportOrdinals);
				int j = 0;
				int num4 = 0;
				for (; j < entries; j++)
				{
					if (text.moduleBuilder.unmanagedExports[num4].ordinal == j + ordinalBase)
					{
						switch (text.peWriter.Headers.FileHeader.Machine)
						{
						case 332:
							mw.Write(byte.MaxValue);
							mw.Write((byte)37);
							mw.Write((uint)((int)text.peWriter.Headers.OptionalHeader.ImageBase + (int)text.moduleBuilder.unmanagedExports[num4].rva.initializedDataOffset) + sdataRVA);
							mw.Write((short)0);
							break;
						case 34404:
							mw.Write((byte)72);
							mw.Write((byte)161);
							mw.Write(text.peWriter.Headers.OptionalHeader.ImageBase + text.moduleBuilder.unmanagedExports[num4].rva.initializedDataOffset + sdataRVA);
							mw.Write(byte.MaxValue);
							mw.Write((byte)224);
							mw.Write(0);
							break;
						case 452:
							mw.Write((ushort)63711);
							mw.Write((ushort)49160);
							mw.Write((ushort)63708);
							mw.Write((ushort)49152);
							mw.Write((ushort)18272);
							mw.Write((ushort)57086);
							mw.Write((uint)((int)text.peWriter.Headers.OptionalHeader.ImageBase + (int)text.moduleBuilder.unmanagedExports[num4].rva.initializedDataOffset) + sdataRVA);
							break;
						default:
							throw new NotSupportedException();
						}
						num4++;
					}
				}
			}

			private static int CompareUnmanagedExportNames(UnmanagedExport x, UnmanagedExport y)
			{
				if (x.name == null)
				{
					if (y.name != null)
					{
						return 1;
					}
					return 0;
				}
				if (y.name == null)
				{
					return -1;
				}
				return string.CompareOrdinal(x.name, y.name);
			}

			private static int CompareUnmanagedExportOrdinals(UnmanagedExport x, UnmanagedExport y)
			{
				return x.ordinal.CompareTo(y.ordinal);
			}

			internal void GetRelocations(List<Relocation> list)
			{
				ushort type;
				uint num;
				switch (text.peWriter.Headers.FileHeader.Machine)
				{
				case 332:
					type = 12288;
					num = stubsRVA + 2;
					break;
				case 34404:
					type = 40960;
					num = stubsRVA + 2;
					break;
				case 452:
					type = 12288;
					num = stubsRVA + 12;
					break;
				default:
					throw new NotSupportedException();
				}
				int i = 0;
				int num2 = 0;
				for (; i < entries; i++)
				{
					if (text.moduleBuilder.unmanagedExports[num2].ordinal == i + ordinalBase)
					{
						list.Add(new Relocation(type, num + (uint)(num2 * (int)stubLength)));
						num2++;
					}
				}
			}
		}

		private struct Relocation : IComparable<Relocation>
		{
			internal readonly uint rva;

			internal readonly ushort type;

			internal Relocation(ushort type, uint rva)
			{
				this.type = type;
				this.rva = rva;
			}

			int IComparable<Relocation>.CompareTo(Relocation other)
			{
				return rva.CompareTo(other.rva);
			}
		}

		private struct RelocationBlock
		{
			internal readonly uint PageRVA;

			internal readonly ushort[] TypeOffset;

			internal RelocationBlock(uint pageRva, ushort[] typeOffset)
			{
				PageRVA = pageRva;
				TypeOffset = typeOffset;
			}
		}

		private readonly PEWriter peWriter;

		private readonly CliHeader cliHeader;

		private readonly ModuleBuilder moduleBuilder;

		private readonly uint strongNameSignatureLength;

		private readonly uint manifestResourcesLength;

		private readonly ExportTables exportTables;

		private readonly List<RelocationBlock> relocations = new List<RelocationBlock>();

		internal uint PointerToRawData => peWriter.ToFileAlignment(peWriter.HeaderSize);

		internal uint BaseRVA => peWriter.Headers.OptionalHeader.SectionAlignment;

		internal uint ImportAddressTableRVA => BaseRVA;

		internal uint ImportAddressTableLength
		{
			get
			{
				if (!peWriter.Is32Bit)
				{
					return 16u;
				}
				return 8u;
			}
		}

		internal uint ComDescriptorRVA => ImportAddressTableRVA + ImportAddressTableLength;

		internal uint ComDescriptorLength => cliHeader.Cb;

		internal uint MethodBodiesRVA => (ComDescriptorRVA + ComDescriptorLength + 7) & 0xFFFFFFF8u;

		private uint MethodBodiesLength => (uint)moduleBuilder.methodBodies.Length;

		private uint ResourcesRVA
		{
			get
			{
				ushort machine = peWriter.Headers.FileHeader.Machine;
				if (machine == 332 || machine == 452)
				{
					return (MethodBodiesRVA + MethodBodiesLength + 3) & 0xFFFFFFFCu;
				}
				return (MethodBodiesRVA + MethodBodiesLength + 15) & 0xFFFFFFF0u;
			}
		}

		private uint ResourcesLength => manifestResourcesLength;

		internal uint StrongNameSignatureRVA => (ResourcesRVA + ResourcesLength + 3) & 0xFFFFFFFCu;

		internal uint StrongNameSignatureLength => strongNameSignatureLength;

		private uint MetadataRVA => (StrongNameSignatureRVA + StrongNameSignatureLength + 3) & 0xFFFFFFFCu;

		private uint MetadataLength => (uint)moduleBuilder.MetadataLength;

		private uint VTableFixupsRVA => (MetadataRVA + MetadataLength + 7) & 0xFFFFFFF8u;

		private uint VTableFixupsLength => (uint)(moduleBuilder.vtablefixups.Count * 8);

		internal uint DebugDirectoryRVA => VTableFixupsRVA + VTableFixupsLength;

		internal uint DebugDirectoryLength
		{
			get
			{
				if (DebugDirectoryContentsLength != 0)
				{
					return 28u;
				}
				return 0u;
			}
		}

		private uint DebugDirectoryContentsLength
		{
			get
			{
				if (moduleBuilder.symbolWriter != null)
				{
					IMAGE_DEBUG_DIRECTORY idd = default(IMAGE_DEBUG_DIRECTORY);
					return (uint)SymbolSupport.GetDebugInfo(moduleBuilder.symbolWriter, ref idd).Length;
				}
				return 0u;
			}
		}

		internal uint ExportDirectoryRVA => (DebugDirectoryRVA + DebugDirectoryLength + DebugDirectoryContentsLength + 15) & 0xFFFFFFF0u;

		internal uint ExportDirectoryLength
		{
			get
			{
				if (moduleBuilder.unmanagedExports.Count != 0)
				{
					return 40u;
				}
				return 0u;
			}
		}

		private uint ExportTablesRVA => ExportDirectoryRVA + ExportDirectoryLength;

		private uint ExportTablesLength
		{
			get
			{
				if (exportTables != null)
				{
					return exportTables.Length;
				}
				return 0u;
			}
		}

		internal uint ImportDirectoryRVA => (ExportTablesRVA + ExportTablesLength + 15) & 0xFFFFFFF0u;

		internal uint ImportDirectoryLength => ImportHintNameTableRVA - ImportDirectoryRVA + 27;

		private uint ImportHintNameTableRVA
		{
			get
			{
				if (!peWriter.Is32Bit)
				{
					return (ImportDirectoryRVA + 52 + 15) & 0xFFFFFFF0u;
				}
				return (ImportDirectoryRVA + 48 + 15) & 0xFFFFFFF0u;
			}
		}

		internal uint StartupStubRVA
		{
			get
			{
				if (peWriter.Headers.FileHeader.Machine == 512)
				{
					return (ImportDirectoryRVA + ImportDirectoryLength + 15) & 0xFFFFFFF0u;
				}
				return 2 + ((ImportDirectoryRVA + ImportDirectoryLength + 3) & 0xFFFFFFFCu);
			}
		}

		internal uint StartupStubLength
		{
			get
			{
				switch (peWriter.Headers.FileHeader.Machine)
				{
				case 332:
					return 6u;
				case 452:
				case 34404:
					return 12u;
				case 512:
					return 48u;
				default:
					throw new NotSupportedException();
				}
			}
		}

		internal int Length => (int)(StartupStubRVA - BaseRVA + StartupStubLength);

		internal TextSection(PEWriter peWriter, CliHeader cliHeader, ModuleBuilder moduleBuilder, int strongNameSignatureLength)
		{
			this.peWriter = peWriter;
			this.cliHeader = cliHeader;
			this.moduleBuilder = moduleBuilder;
			this.strongNameSignatureLength = (uint)strongNameSignatureLength;
			manifestResourcesLength = (uint)moduleBuilder.GetManifestResourcesLength();
			if (moduleBuilder.unmanagedExports.Count != 0)
			{
				exportTables = new ExportTables(this);
			}
		}

		private void WriteRVA(MetadataWriter mw, uint rva)
		{
			ushort machine = peWriter.Headers.FileHeader.Machine;
			if (machine == 332 || machine == 452)
			{
				mw.Write(rva);
			}
			else
			{
				mw.Write((ulong)rva);
			}
		}

		internal void Write(MetadataWriter mw, uint sdataRVA, out int guidHeapOffset)
		{
			moduleBuilder.TypeRef.Fixup(moduleBuilder);
			moduleBuilder.MethodDef.Fixup(this);
			moduleBuilder.MethodImpl.Fixup(moduleBuilder);
			moduleBuilder.MethodSemantics.Fixup(moduleBuilder);
			moduleBuilder.InterfaceImpl.Fixup();
			moduleBuilder.ResolveInterfaceImplPseudoTokens();
			moduleBuilder.MemberRef.Fixup(moduleBuilder);
			moduleBuilder.Constant.Fixup(moduleBuilder);
			moduleBuilder.FieldMarshal.Fixup(moduleBuilder);
			moduleBuilder.DeclSecurity.Fixup(moduleBuilder);
			moduleBuilder.GenericParam.Fixup(moduleBuilder);
			moduleBuilder.CustomAttribute.Fixup(moduleBuilder);
			moduleBuilder.FieldLayout.Fixup(moduleBuilder);
			moduleBuilder.FieldRVA.Fixup(moduleBuilder, (int)sdataRVA, (int)MethodBodiesRVA);
			moduleBuilder.ImplMap.Fixup(moduleBuilder);
			moduleBuilder.ExportedType.Fixup(moduleBuilder);
			moduleBuilder.ManifestResource.Fixup(moduleBuilder);
			moduleBuilder.MethodSpec.Fixup(moduleBuilder);
			moduleBuilder.GenericParamConstraint.Fixup(moduleBuilder);
			if (ImportAddressTableLength != 0)
			{
				WriteRVA(mw, ImportHintNameTableRVA);
				WriteRVA(mw, 0u);
			}
			cliHeader.MetaData.VirtualAddress = MetadataRVA;
			cliHeader.MetaData.Size = MetadataLength;
			if (ResourcesLength != 0)
			{
				cliHeader.Resources.VirtualAddress = ResourcesRVA;
				cliHeader.Resources.Size = ResourcesLength;
			}
			if (StrongNameSignatureLength != 0)
			{
				cliHeader.StrongNameSignature.VirtualAddress = StrongNameSignatureRVA;
				cliHeader.StrongNameSignature.Size = StrongNameSignatureLength;
			}
			if (VTableFixupsLength != 0)
			{
				cliHeader.VTableFixups.VirtualAddress = VTableFixupsRVA;
				cliHeader.VTableFixups.Size = VTableFixupsLength;
			}
			cliHeader.Write(mw);
			for (int num = (int)(MethodBodiesRVA - (ComDescriptorRVA + ComDescriptorLength)); num > 0; num--)
			{
				mw.Write((byte)0);
			}
			mw.Write(moduleBuilder.methodBodies);
			for (int num2 = (int)(ResourcesRVA - (MethodBodiesRVA + MethodBodiesLength)); num2 > 0; num2--)
			{
				mw.Write((byte)0);
			}
			moduleBuilder.WriteResources(mw);
			for (int num3 = (int)(MetadataRVA - (ResourcesRVA + ResourcesLength)); num3 > 0; num3--)
			{
				mw.Write((byte)0);
			}
			moduleBuilder.WriteMetadata(mw, out guidHeapOffset);
			for (int num4 = (int)(VTableFixupsRVA - (MetadataRVA + MetadataLength)); num4 > 0; num4--)
			{
				mw.Write((byte)0);
			}
			WriteVTableFixups(mw, sdataRVA);
			WriteDebugDirectory(mw);
			for (int num5 = (int)(ExportDirectoryRVA - (DebugDirectoryRVA + DebugDirectoryLength + DebugDirectoryContentsLength)); num5 > 0; num5--)
			{
				mw.Write((byte)0);
			}
			WriteExportDirectory(mw);
			WriteExportTables(mw, sdataRVA);
			for (int num6 = (int)(ImportDirectoryRVA - (ExportTablesRVA + ExportTablesLength)); num6 > 0; num6--)
			{
				mw.Write((byte)0);
			}
			if (ImportDirectoryLength != 0)
			{
				WriteImportDirectory(mw);
			}
			for (int num7 = (int)(StartupStubRVA - (ImportDirectoryRVA + ImportDirectoryLength)); num7 > 0; num7--)
			{
				mw.Write((byte)0);
			}
			if (peWriter.Headers.FileHeader.Machine == 34404)
			{
				mw.Write((ushort)41288);
				mw.Write(peWriter.Headers.OptionalHeader.ImageBase + ImportAddressTableRVA);
				mw.Write((ushort)57599);
			}
			else if (peWriter.Headers.FileHeader.Machine == 512)
			{
				mw.Write(new byte[32]
				{
					11, 72, 0, 2, 24, 16, 160, 64, 36, 48,
					40, 0, 0, 0, 4, 0, 16, 8, 0, 18,
					24, 16, 96, 80, 4, 128, 3, 0, 96, 0,
					128, 0
				});
				mw.Write(peWriter.Headers.OptionalHeader.ImageBase + StartupStubRVA);
				mw.Write(peWriter.Headers.OptionalHeader.ImageBase + BaseRVA);
			}
			else if (peWriter.Headers.FileHeader.Machine == 332)
			{
				mw.Write((ushort)9727);
				mw.Write((uint)(int)peWriter.Headers.OptionalHeader.ImageBase + ImportAddressTableRVA);
			}
			else
			{
				if (peWriter.Headers.FileHeader.Machine != 452)
				{
					throw new NotSupportedException();
				}
				int num8 = (int)peWriter.Headers.OptionalHeader.ImageBase + (int)ImportAddressTableRVA;
				ushort num9 = (ushort)num8;
				ushort num10 = (ushort)((uint)num8 >> 16);
				mw.Write((ushort)(62016 + (num9 >> 12)));
				mw.Write((ushort)(3072 + ((num9 << 4) & 0xF000) + (num9 & 0xFF)));
				mw.Write((ushort)(62144 + (num10 >> 12)));
				mw.Write((ushort)(3072 + ((num10 << 4) & 0xF000) + (num10 & 0xFF)));
				mw.Write((ushort)63708);
				mw.Write((ushort)61440);
			}
		}

		[Conditional("DEBUG")]
		private void AssertRVA(MetadataWriter mw, uint rva)
		{
		}

		private void WriteVTableFixups(MetadataWriter mw, uint sdataRVA)
		{
			foreach (ModuleBuilder.VTableFixups vtablefixup in moduleBuilder.vtablefixups)
			{
				mw.Write(vtablefixup.initializedDataOffset + sdataRVA);
				mw.Write(vtablefixup.count);
				mw.Write(vtablefixup.type);
			}
		}

		private void WriteDebugDirectory(MetadataWriter mw)
		{
			if (DebugDirectoryLength != 0)
			{
				IMAGE_DEBUG_DIRECTORY idd = new IMAGE_DEBUG_DIRECTORY
				{
					Characteristics = 0u,
					TimeDateStamp = peWriter.Headers.FileHeader.TimeDateStamp
				};
				byte[] debugInfo = SymbolSupport.GetDebugInfo(moduleBuilder.symbolWriter, ref idd);
				idd.PointerToRawData = DebugDirectoryRVA - BaseRVA + DebugDirectoryLength + PointerToRawData;
				idd.AddressOfRawData = DebugDirectoryRVA + DebugDirectoryLength;
				mw.Write(idd.Characteristics);
				mw.Write(idd.TimeDateStamp);
				mw.Write(idd.MajorVersion);
				mw.Write(idd.MinorVersion);
				mw.Write(idd.Type);
				mw.Write(idd.SizeOfData);
				mw.Write(idd.AddressOfRawData);
				mw.Write(idd.PointerToRawData);
				mw.Write(debugInfo);
			}
		}

		private uint GetOrdinalBase(out uint entries)
		{
			uint num = uint.MaxValue;
			uint num2 = 0u;
			foreach (UnmanagedExport unmanagedExport in moduleBuilder.unmanagedExports)
			{
				uint ordinal = (uint)unmanagedExport.ordinal;
				num = Math.Min(num, ordinal);
				num2 = Math.Max(num2, ordinal);
			}
			entries = 1 + (num2 - num);
			return num;
		}

		private uint GetExportNamesLength(out uint nameCount)
		{
			nameCount = 0u;
			uint num = 0u;
			foreach (UnmanagedExport unmanagedExport in moduleBuilder.unmanagedExports)
			{
				if (unmanagedExport.name != null)
				{
					nameCount++;
					num += (uint)(unmanagedExport.name.Length + 1);
				}
			}
			return num;
		}

		private void WriteExportDirectory(MetadataWriter mw)
		{
			if (ExportDirectoryLength != 0)
			{
				mw.Write(0);
				mw.Write(peWriter.Headers.FileHeader.TimeDateStamp);
				mw.Write((short)0);
				mw.Write((short)0);
				mw.Write(exportTables.namesRVA);
				mw.Write(exportTables.ordinalBase);
				mw.Write(exportTables.entries);
				mw.Write(exportTables.nameCount);
				mw.Write(exportTables.exportAddressTableRVA);
				mw.Write(exportTables.exportNamePointerTableRVA);
				mw.Write(exportTables.exportOrdinalTableRVA);
			}
		}

		private void WriteExportTables(MetadataWriter mw, uint sdataRVA)
		{
			if (exportTables != null)
			{
				exportTables.Write(mw, sdataRVA);
			}
		}

		private void WriteImportDirectory(MetadataWriter mw)
		{
			mw.Write(ImportDirectoryRVA + 40);
			mw.Write(0);
			mw.Write(0);
			mw.Write(ImportHintNameTableRVA + 14);
			mw.Write(ImportAddressTableRVA);
			mw.Write(new byte[20]);
			mw.Write(ImportHintNameTableRVA);
			int num = 48;
			if (!peWriter.Is32Bit)
			{
				num += 4;
				mw.Write(0);
			}
			mw.Write(0);
			for (int num2 = (int)(ImportHintNameTableRVA - (ImportDirectoryRVA + num)); num2 > 0; num2--)
			{
				mw.Write((byte)0);
			}
			mw.Write((ushort)0);
			if ((peWriter.Headers.FileHeader.Characteristics & 0x2000) != 0)
			{
				mw.WriteAsciiz("_CorDllMain");
			}
			else
			{
				mw.WriteAsciiz("_CorExeMain");
			}
			mw.WriteAsciiz("mscoree.dll");
			mw.Write((byte)0);
		}

		internal void WriteRelocations(MetadataWriter mw)
		{
			foreach (RelocationBlock relocation in relocations)
			{
				mw.Write(relocation.PageRVA);
				mw.Write(8 + relocation.TypeOffset.Length * 2);
				ushort[] typeOffset = relocation.TypeOffset;
				foreach (ushort value in typeOffset)
				{
					mw.Write(value);
				}
			}
		}

		internal uint PackRelocations()
		{
			List<Relocation> list = new List<Relocation>();
			switch (peWriter.Headers.FileHeader.Machine)
			{
			case 332:
				list.Add(new Relocation(12288, StartupStubRVA + 2));
				break;
			case 34404:
				list.Add(new Relocation(40960, StartupStubRVA + 2));
				break;
			case 512:
				list.Add(new Relocation(40960, StartupStubRVA + 32));
				list.Add(new Relocation(40960, StartupStubRVA + 40));
				break;
			case 452:
				list.Add(new Relocation(28672, StartupStubRVA));
				break;
			default:
				throw new NotSupportedException();
			}
			if (exportTables != null)
			{
				exportTables.GetRelocations(list);
			}
			list.Sort();
			uint num = 0u;
			int num2 = 0;
			while (num2 < list.Count)
			{
				uint num3 = list[num2].rva & 0xFFFFF000u;
				int i;
				for (i = 1; num2 + i < list.Count && (list[num2 + i].rva & 0xFFFFF000u) == num3; i++)
				{
				}
				ushort[] array = new ushort[(i + 1) & -2];
				int num4 = 0;
				while (num4 < i)
				{
					array[num4] = (ushort)(list[num2].type + (list[num2].rva - num3));
					num4++;
					num2++;
				}
				relocations.Add(new RelocationBlock(num3, array));
				num += (uint)(8 + array.Length * 2);
			}
			return num;
		}
	}
}
