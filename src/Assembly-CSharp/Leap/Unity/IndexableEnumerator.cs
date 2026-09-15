namespace Leap.Unity
{
	public struct IndexableEnumerator<Element>
	{
		private IIndexable<Element> indexable;

		private int index;

		public Element Current => indexable[index];

		public IndexableEnumerator(IIndexable<Element> indexable)
		{
			this.indexable = indexable;
			index = -1;
		}

		public IndexableEnumerator<Element> GetEnumerator()
		{
			return this;
		}

		public bool MoveNext()
		{
			if (indexable == null)
			{
				return false;
			}
			index++;
			return index < indexable.Count;
		}

		public void Reset()
		{
			index = -1;
		}
	}
}
