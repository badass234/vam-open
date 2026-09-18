using System.IO;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class CliHeader
	{
		internal const uint COMIMAGE_FLAGS_ILONLY = 1u;

		internal const uint COMIMAGE_FLAGS_32BITREQUIRED = 2u;

		internal const uint COMIMAGE_FLAGS_STRONGNAMESIGNED = 8u;

		internal const uint COMIMAGE_FLAGS_NATIVE_ENTRYPOINT = 16u;

		internal const uint COMIMAGE_FLAGS_32BITPREFERRED = 131072u;

		internal uint Cb = 72u;

		internal ushort MajorRuntimeVersion;

		internal ushort MinorRuntimeVersion;

		internal RvaSize MetaData;

		internal uint Flags;

		internal uint EntryPointToken;

		internal RvaSize Resources;

		internal RvaSize StrongNameSignature;

		internal RvaSize CodeManagerTable;

		internal RvaSize VTableFixups;

		internal RvaSize ExportAddressTableJumps;

		internal RvaSize ManagedNativeHeader;

		internal void Read(BinaryReader br)
		{
			Cb = br.ReadUInt32();
			MajorRuntimeVersion = br.ReadUInt16();
			MinorRuntimeVersion = br.ReadUInt16();
			MetaData.Read(br);
			Flags = br.ReadUInt32();
			EntryPointToken = br.ReadUInt32();
			Resources.Read(br);
			StrongNameSignature.Read(br);
			CodeManagerTable.Read(br);
			VTableFixups.Read(br);
			ExportAddressTableJumps.Read(br);
			ManagedNativeHeader.Read(br);
		}

		internal void Write(MetadataWriter mw)
		{
			mw.Write(Cb);
			mw.Write(MajorRuntimeVersion);
			mw.Write(MinorRuntimeVersion);
			MetaData.Write(mw);
			mw.Write(Flags);
			mw.Write(EntryPointToken);
			Resources.Write(mw);
			StrongNameSignature.Write(mw);
			CodeManagerTable.Write(mw);
			VTableFixups.Write(mw);
			ExportAddressTableJumps.Write(mw);
			ManagedNativeHeader.Write(mw);
		}
	}
}
