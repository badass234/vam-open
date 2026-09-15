namespace Leap.Unity
{
	public struct IndexableStructEnumerator<Element, IndexableStruct> where IndexableStruct : struct, IIndexableStruct<Element, IndexableStruct>
	{
		private IndexableStruct? maybeIndexable;

		private int index;

		public Element Current => maybeIndexable.Value[index];

		public IndexableStructEnumerator(IndexableStruct indexable)
		{
			maybeIndexable = indexable;
			index = -1;
		}

		public IndexableStructEnumerator<Element, IndexableStruct> GetEnumerator()
		{
			return this;
		}

		public bool MoveNext()
		{
			if (!maybeIndexable.HasValue)
			{
				return false;
			}
			index++;
			return index < maybeIndexable.Value.Count;
		}

		public void Reset()
		{
			index = -1;
		}
	}
}
