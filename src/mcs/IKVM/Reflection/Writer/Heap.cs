using System;

namespace IKVM.Reflection.Writer
{
	internal abstract class Heap
	{
		protected bool frozen;

		protected int unalignedlength;

		internal bool IsBig => Length > 65535;

		internal int Length
		{
			get
			{
				if (!frozen)
				{
					throw new InvalidOperationException();
				}
				return (unalignedlength + 3) & -4;
			}
		}

		internal void Write(MetadataWriter mw)
		{
			int position = mw.Position;
			WriteImpl(mw);
			int num = Length - unalignedlength;
			for (int i = 0; i < num; i++)
			{
				mw.Write((byte)0);
			}
		}

		protected abstract void WriteImpl(MetadataWriter mw);
	}
}
