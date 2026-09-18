using System;

namespace IKVM.Reflection.Writer
{
	internal abstract class SimpleHeap : Heap
	{
		internal void Freeze()
		{
			if (frozen)
			{
				throw new InvalidOperationException();
			}
			frozen = true;
			unalignedlength = GetLength();
		}

		protected abstract int GetLength();
	}
}
