namespace Leap.Unity
{
	public interface IIndexable<T>
	{
		T this[int idx] { get; }

		int Count { get; }
	}
}
