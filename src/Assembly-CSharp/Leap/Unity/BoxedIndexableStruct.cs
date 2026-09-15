using System;

namespace Leap.Unity
{
	public class BoxedIndexableStruct<Element, IndexableStruct> : IIndexable<Element>, IPoolable where IndexableStruct : struct, IIndexableStruct<Element, IndexableStruct>
	{
		public IndexableStruct? maybeIndexableStruct;

		public Element this[int idx]
		{
			get
			{
				if (!maybeIndexableStruct.HasValue)
				{
					throw new NullReferenceException("PooledIndexableStructWrapper failed to index missing " + typeof(IndexableStruct).Name + "; did you assign its maybeIndexableStruct field?");
				}
				return maybeIndexableStruct.Value[idx];
			}
		}

		public int Count
		{
			get
			{
				if (!maybeIndexableStruct.HasValue)
				{
					return 0;
				}
				return maybeIndexableStruct.Value.Count;
			}
		}

		public void OnSpawn()
		{
		}

		public void OnRecycle()
		{
			maybeIndexableStruct = null;
		}
	}
}
