using System;
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection.Writer
{
	internal sealed class TableHeap : Heap
	{
		internal void Freeze(MetadataWriter mw)
		{
			if (frozen)
			{
				throw new InvalidOperationException();
			}
			frozen = true;
			unalignedlength = GetLength(mw);
		}

		protected override void WriteImpl(MetadataWriter mw)
		{
			Table[] tables = mw.ModuleBuilder.GetTables();
			mw.Write(0);
			int mDStreamVersion = mw.ModuleBuilder.MDStreamVersion;
			mw.Write((byte)(mDStreamVersion >> 16));
			mw.Write((byte)mDStreamVersion);
			byte b = 0;
			if (mw.ModuleBuilder.Strings.IsBig)
			{
				b |= 1;
			}
			if (mw.ModuleBuilder.Guids.IsBig)
			{
				b |= 2;
			}
			if (mw.ModuleBuilder.Blobs.IsBig)
			{
				b |= 4;
			}
			mw.Write(b);
			mw.Write((byte)16);
			long num = 1L;
			long num2 = 0L;
			Table[] array = tables;
			foreach (Table table in array)
			{
				if (table != null && table.RowCount > 0)
				{
					num2 |= num;
				}
				num <<= 1;
			}
			mw.Write(num2);
			mw.Write(24190111578624L);
			array = tables;
			foreach (Table table2 in array)
			{
				if (table2 != null && table2.RowCount > 0)
				{
					mw.Write(table2.RowCount);
				}
			}
			array = tables;
			foreach (Table table3 in array)
			{
				if (table3 != null && table3.RowCount > 0)
				{
					int position = mw.Position;
					table3.Write(mw);
				}
			}
			mw.Write((byte)0);
		}

		private static int GetLength(MetadataWriter mw)
		{
			int num = 24;
			Table[] tables = mw.ModuleBuilder.GetTables();
			foreach (Table table in tables)
			{
				if (table != null && table.RowCount > 0)
				{
					num += 4;
					num += table.GetLength(mw);
				}
			}
			return num + 1;
		}
	}
}
