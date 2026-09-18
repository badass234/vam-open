using System;
using System.Collections.Generic;

namespace IKVM.Reflection.Writer
{
	internal sealed class GuidHeap : SimpleHeap
	{
		private List<Guid> list = new List<Guid>();

		internal GuidHeap()
		{
		}

		internal int Add(Guid guid)
		{
			list.Add(guid);
			return list.Count;
		}

		protected override int GetLength()
		{
			return list.Count * 16;
		}

		protected override void WriteImpl(MetadataWriter mw)
		{
			foreach (Guid item in list)
			{
				mw.Write(item.ToByteArray());
			}
		}
	}
}
