namespace Leap.Unity
{
	public interface IPoolable
	{
		void OnSpawn();

		void OnRecycle();
	}
}
