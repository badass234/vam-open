namespace Leap.Unity
{
	public interface IIndexableStruct<T, ThisIndexableType> where ThisIndexableType : struct, IIndexableStruct<T, ThisIndexableType>
	{
		T this[int idx] { get; }

		int Count { get; }
	}
}
