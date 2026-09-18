using System;

namespace IKVM.Reflection.Metadata
{
	internal abstract class SortedTable<T> : Table<T> where T : SortedTable<T>.IRecord
	{
		internal interface IRecord
		{
			int SortKey { get; }

			int FilterKey { get; }
		}

		internal struct Enumerable
		{
			private readonly SortedTable<T> table;

			private readonly int token;

			internal Enumerable(SortedTable<T> table, int token)
			{
				this.table = table;
				this.token = token;
			}

			public Enumerator GetEnumerator()
			{
				T[] records = table.records;
				if (!table.Sorted)
				{
					return new Enumerator(records, table.RowCount - 1, -1, token);
				}
				int num = BinarySearch(records, table.RowCount, token & 0xFFFFFF);
				if (num < 0)
				{
					return new Enumerator(null, 0, 1, -1);
				}
				int num2 = num;
				while (num2 > 0 && (records[num2 - 1].FilterKey & 0xFFFFFF) == (token & 0xFFFFFF))
				{
					num2--;
				}
				int i = num;
				for (int num3 = table.RowCount - 1; i < num3 && (records[i + 1].FilterKey & 0xFFFFFF) == (token & 0xFFFFFF); i++)
				{
				}
				return new Enumerator(records, i, num2 - 1, token);
			}

			private static int BinarySearch(T[] records, int length, int maskedToken)
			{
				int num = 0;
				int num2 = length - 1;
				while (num <= num2)
				{
					int num3 = num + (num2 - num) / 2;
					int num4 = records[num3].FilterKey & 0xFFFFFF;
					if (maskedToken == num4)
					{
						return num3;
					}
					if (maskedToken < num4)
					{
						num2 = num3 - 1;
					}
					else
					{
						num = num3 + 1;
					}
				}
				return -1;
			}
		}

		internal struct Enumerator
		{
			private readonly T[] records;

			private readonly int token;

			private readonly int max;

			private int index;

			public int Current => index;

			internal Enumerator(T[] records, int max, int index, int token)
			{
				this.records = records;
				this.token = token;
				this.max = max;
				this.index = index;
			}

			public bool MoveNext()
			{
				while (index < max)
				{
					index++;
					if (records[index].FilterKey == token)
					{
						return true;
					}
				}
				return false;
			}
		}

		internal Enumerable Filter(int token)
		{
			return new Enumerable(this, token);
		}

		protected void Sort()
		{
			ulong[] array = new ulong[rowCount];
			for (uint num = 0u; num < array.Length; num++)
			{
				array[num] = (ulong)(((long)records[num].SortKey << 32) | num);
			}
			Array.Sort(array);
			T[] array2 = new T[rowCount];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = records[(uint)array[i]];
			}
			records = array2;
		}
	}
}
