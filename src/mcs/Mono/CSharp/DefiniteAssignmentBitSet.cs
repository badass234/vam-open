using System;
using System.Collections.Generic;
using System.Text;

namespace Mono.CSharp
{
	public class DefiniteAssignmentBitSet
	{
		private const uint copy_on_write_flag = 2147483648u;

		private uint bits;

		private int[] large_bits;

		public static readonly DefiniteAssignmentBitSet Empty = new DefiniteAssignmentBitSet(0);

		private bool CopyOnWrite => (bits & 0x80000000u) != 0;

		private int Length
		{
			get
			{
				if (large_bits != null)
				{
					return large_bits.Length * 32;
				}
				return 31;
			}
		}

		public bool this[int index] => GetBit(index);

		public DefiniteAssignmentBitSet(int length)
		{
			if (length > 31)
			{
				large_bits = new int[(length + 31) / 32];
			}
		}

		public DefiniteAssignmentBitSet(DefiniteAssignmentBitSet source)
		{
			if (source.large_bits != null)
			{
				large_bits = source.large_bits;
				bits = source.bits | 0x80000000u;
			}
			else
			{
				bits = source.bits & 0x7FFFFFFF;
			}
		}

		public static DefiniteAssignmentBitSet operator &(DefiniteAssignmentBitSet a, DefiniteAssignmentBitSet b)
		{
			if (AreEqual(a, b))
			{
				return a;
			}
			DefiniteAssignmentBitSet definiteAssignmentBitSet;
			if (a.large_bits == null)
			{
				definiteAssignmentBitSet = new DefiniteAssignmentBitSet(a);
				definiteAssignmentBitSet.bits &= b.bits & 0x7FFFFFFF;
				return definiteAssignmentBitSet;
			}
			definiteAssignmentBitSet = new DefiniteAssignmentBitSet(a);
			definiteAssignmentBitSet.Clone();
			int[] array = definiteAssignmentBitSet.large_bits;
			int[] array2 = b.large_bits;
			for (int i = 0; i < array.Length; i++)
			{
				array[i] &= array2[i];
			}
			return definiteAssignmentBitSet;
		}

		public static DefiniteAssignmentBitSet operator |(DefiniteAssignmentBitSet a, DefiniteAssignmentBitSet b)
		{
			if (AreEqual(a, b))
			{
				return a;
			}
			DefiniteAssignmentBitSet definiteAssignmentBitSet;
			if (a.large_bits == null)
			{
				definiteAssignmentBitSet = new DefiniteAssignmentBitSet(a);
				definiteAssignmentBitSet.bits |= b.bits;
				definiteAssignmentBitSet.bits &= 2147483647u;
				return definiteAssignmentBitSet;
			}
			definiteAssignmentBitSet = new DefiniteAssignmentBitSet(a);
			definiteAssignmentBitSet.Clone();
			int[] array = definiteAssignmentBitSet.large_bits;
			int[] array2 = b.large_bits;
			for (int i = 0; i < array.Length; i++)
			{
				array[i] |= array2[i];
			}
			return definiteAssignmentBitSet;
		}

		public static DefiniteAssignmentBitSet And(List<DefiniteAssignmentBitSet> das)
		{
			if (das.Count == 0)
			{
				throw new ArgumentException("Empty das");
			}
			DefiniteAssignmentBitSet result = das[0];
			for (int i = 1; i < das.Count; i++)
			{
				result &= das[i];
			}
			return result;
		}

		public void Set(int index)
		{
			if (CopyOnWrite && !this[index])
			{
				Clone();
			}
			SetBit(index);
		}

		public void Set(int index, int length)
		{
			for (int i = 0; i < length; i++)
			{
				if (CopyOnWrite && !this[index + i])
				{
					Clone();
				}
				SetBit(index + i);
			}
		}

		public override string ToString()
		{
			int length = Length;
			StringBuilder stringBuilder = new StringBuilder(length);
			for (int i = 0; i < length; i++)
			{
				stringBuilder.Append(this[i] ? '1' : '0');
			}
			return stringBuilder.ToString();
		}

		private void Clone()
		{
			large_bits = (int[])large_bits.Clone();
		}

		private bool GetBit(int index)
		{
			if (large_bits != null)
			{
				return (large_bits[index >> 5] & (1 << index)) != 0;
			}
			return (bits & (1 << index)) != 0;
		}

		private void SetBit(int index)
		{
			if (large_bits == null)
			{
				bits |= (uint)(1 << index);
			}
			else
			{
				large_bits[index >> 5] |= 1 << index;
			}
		}

		public static bool AreEqual(DefiniteAssignmentBitSet a, DefiniteAssignmentBitSet b)
		{
			if (a.large_bits == null)
			{
				return (a.bits & 0x7FFFFFFF) == (b.bits & 0x7FFFFFFF);
			}
			for (int i = 0; i < a.large_bits.Length; i++)
			{
				if (a.large_bits[i] != b.large_bits[i])
				{
					return false;
				}
			}
			return true;
		}
	}
}
