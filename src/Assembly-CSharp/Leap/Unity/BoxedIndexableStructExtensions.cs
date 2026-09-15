namespace Leap.Unity
{
	public static class BoxedIndexableStructExtensions
	{
		public static void Recycle<Element, IndexableStruct>(this BoxedIndexableStruct<Element, IndexableStruct> pooledWrapper) where IndexableStruct : struct, IIndexableStruct<Element, IndexableStruct>
		{
			Pool<BoxedIndexableStruct<Element, IndexableStruct>>.Recycle(pooledWrapper);
		}
	}
}
