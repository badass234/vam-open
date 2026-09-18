using System;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class ConstantTable : SortedTable<ConstantTable.Record>
	{
		internal struct Record : IRecord
		{
			internal short Type;

			internal int Parent;

			internal int Value;

			int IRecord.SortKey => IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EConstantTable_002ERecord_003E_002EIRecord_002Eget_SortKey();

			int IRecord.FilterKey => IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EConstantTable_002ERecord_003E_002EIRecord_002Eget_FilterKey();

			private int IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EConstantTable_002ERecord_003E_002EIRecord_002Eget_SortKey()
			{
				return EncodeHasConstant(Parent);
			}

			private int IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EConstantTable_002ERecord_003E_002EIRecord_002Eget_FilterKey()
			{
				return Parent;
			}
		}

		internal const int Index = 11;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Type = mr.ReadInt16();
				records[i].Parent = mr.ReadHasConstant();
				records[i].Value = mr.ReadBlobIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].Type);
				mw.WriteHasConstant(records[i].Parent);
				mw.WriteBlobIndex(records[i].Value);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.AddFixed(2).WriteHasConstant().WriteBlobIndex()
				.Value;
		}

		internal void Fixup(ModuleBuilder moduleBuilder)
		{
			for (int i = 0; i < rowCount; i++)
			{
				moduleBuilder.FixupPseudoToken(ref records[i].Parent);
			}
			Sort();
		}

		internal static int EncodeHasConstant(int token)
		{
			switch (token >> 24)
			{
			case 4:
				return ((token & 0xFFFFFF) << 2) | 0;
			case 8:
				return ((token & 0xFFFFFF) << 2) | 1;
			case 23:
				return ((token & 0xFFFFFF) << 2) | 2;
			default:
				throw new InvalidOperationException();
			}
		}

		internal object GetRawConstantValue(Module module, int parent)
		{
			Enumerator enumerator = Filter(parent).GetEnumerator();
			if (enumerator.MoveNext())
			{
				int current = enumerator.Current;
				ByteReader blob = module.GetBlob(module.Constant.records[current].Value);
				switch (module.Constant.records[current].Type)
				{
				case 2:
					return blob.ReadByte() != 0;
				case 4:
					return blob.ReadSByte();
				case 6:
					return blob.ReadInt16();
				case 8:
					return blob.ReadInt32();
				case 10:
					return blob.ReadInt64();
				case 5:
					return blob.ReadByte();
				case 7:
					return blob.ReadUInt16();
				case 9:
					return blob.ReadUInt32();
				case 11:
					return blob.ReadUInt64();
				case 12:
					return blob.ReadSingle();
				case 13:
					return blob.ReadDouble();
				case 3:
					return blob.ReadChar();
				case 14:
				{
					char[] array = new char[blob.Length / 2];
					for (int i = 0; i < array.Length; i++)
					{
						array[i] = blob.ReadChar();
					}
					return new string(array);
				}
				case 18:
					if (blob.ReadInt32() != 0)
					{
						throw new BadImageFormatException();
					}
					return null;
				default:
					throw new BadImageFormatException();
				}
			}
			throw new InvalidOperationException();
		}
	}
}
